using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using adHelp.Models;
using adHelp.Utils;

namespace adHelp.Services
{
    /// <summary>
    /// Active Directory 확장 서비스 - 계정 관리 기능
    /// 사용자 계정 잠금 해제, 비밀번호 변경 등의 관리 작업을 수행
    /// </summary>
    public class ADServiceExt : IDisposable
    {
        private PrincipalContext _managementContext;
        private CredentialInfo _credential;
        private bool _disposed;
        private bool _isConnected;



        /// <summary>
        /// 연결 상태
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// 관리 기능 사용 가능 여부
        /// AD 연결이 성공적으로 이루어진 경우 기본적으로 관리 기능 활성화
        /// (실제 권한은 개별 작업 시도 시 검증)
        /// </summary>
        public bool IsManagementEnabled => _isConnected && _credential != null;

        /// <summary>
        /// 생성자
        /// </summary>
        public ADServiceExt()
        {
            _disposed = false;
            _isConnected = false;
        }

        /// <summary>
        /// 연결 상태 테스트
        /// </summary>
        /// <returns>연결 상태</returns>
        public bool TestConnection()
        {
            if (!_isConnected || _managementContext == null)
                return false;

            try
            {
                // 간단한 쿼리로 연결 테스트
                var testUser = UserPrincipal.FindByIdentity(_managementContext, Environment.UserName);
                return true;
            }
            catch
            {
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// AD에 연결
        /// </summary>
        /// <param name="credential">인증 정보</param>
        /// <returns>연결 성공 여부</returns>
        public bool Connect(CredentialInfo credential)
        {
            try
            {
                if (credential == null || !credential.IsValid())
                {
                    throw new ArgumentException("유효하지 않은 크리덴셜 정보입니다.");
                }

                // 기존 연결 해제
                Disconnect();

                _credential = credential;

                // 관리용 PrincipalContext 생성
                _managementContext = new PrincipalContext(
                    ContextType.Domain,
                    credential.Domain,
                    credential.GetFullUsername(),
                    credential.GetPlainPassword()
                );

                // 연결 테스트
                var testUser = UserPrincipal.FindByIdentity(_managementContext, Environment.UserName);
                
                _isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                CleanupResources();
                throw new InvalidOperationException($"AD 확장 서비스 연결 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// AD 연결 해제
        /// </summary>
        public void Disconnect()
        {
            CleanupResources();
            _isConnected = false;
        }

        /// <summary>
        /// 사용자 계정 잠금 해제
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>잠금 해제 성공 여부</returns>
        public bool UnlockUserAccount(string userId)
        {
            if (!IsConnected)
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다. 먼저 로그인을 완료해주세요.");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("사용자 ID는 필수입니다.");

            try
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(_managementContext, userId))
                {
                    if (user == null)
                    {
                        throw new InvalidOperationException($"사용자 '{userId}'를 찾을 수 없습니다. 사용자 ID를 확인해주세요.");
                    }

                    // 계정 상태 확인
                    if (user.Enabled == false)
                    {
                        throw new InvalidOperationException($"사용자 '{userId}'의 계정이 비활성화되어 있습니다. 관리자에게 문의하세요.");
                    }

                    if (!user.IsAccountLockedOut())
                    {
                        return true; // 이미 잠금 해제된 상태
                    }

                    // 계정 잠금 해제 시도
                    user.UnlockAccount();
                    user.Save();

                    return true;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // MS AD 스타일의 전문적인 영어 메시지
                string rootMessage = ErrorHandler.GetRootExceptionMessage(ex);
                
                if (rootMessage.Contains("logon failure"))
                {
                    throw new UnauthorizedAccessException("Logon failure: unknown user name or bad password.", ex);
                }
                else if (ErrorHandler.IsAccessDeniedError(rootMessage))
                {
                    throw new UnauthorizedAccessException("Access is denied. You do not have sufficient privileges to unlock user accounts.", ex);
                }
                else
                {
                    throw new UnauthorizedAccessException($"Insufficient access rights to perform the operation. {rootMessage}", ex);
                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                string comMessage = GetCOMExceptionMessage(ex, "unlock account");
                throw new InvalidOperationException(comMessage, ex);
            }
            catch (Exception ex)
            {
                // 기타 예상치 못한 오류
                string rootMessage = ErrorHandler.GetRootExceptionMessage(ex);
                
                if (ErrorHandler.IsAccessDeniedError(rootMessage))
                {
                    throw new UnauthorizedAccessException("Access is denied. Administrator privileges are required.", ex);
                }
                else if (rootMessage.Contains("network") || rootMessage.Contains("RPC"))
                {
                    throw new InvalidOperationException("The server is not operational. Please check network connectivity.", ex);
                }
                else
                {
                    throw new InvalidOperationException($"An error occurred while unlocking the account. {rootMessage}", ex);
                }
            }
        }

        /// <summary>
        /// 사용자 비밀번호 변경 (자동 잠금 해제 포함)
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="newPassword">새 비밀번호</param>
        /// <param name="mustChangeAtNextLogon">다음 로그인 시 비밀번호 변경 강제 여부</param>
        /// <returns>비밀번호 변경 성공 여부</returns>
        public bool ChangeUserPassword(string userId, string newPassword, bool mustChangeAtNextLogon = false)
        {
            if (!IsConnected)
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다. 먼저 로그인을 완료해주세요.");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("사용자 ID는 필수입니다.");

            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentException("새 비밀번호는 필수입니다.");

            // 기본 권한 체크 (도메인 제한 제거)
            if (_credential == null)
                throw new InvalidOperationException("인증 정보가 없습니다. 다시 로그인해주세요.");

            bool wasAccountUnlocked = false;
            string statusMessage = "";

            try
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(_managementContext, userId))
                {
                    if (user == null)
                    {
                        throw new InvalidOperationException($"사용자 '{userId}'를 찾을 수 없습니다. 사용자 ID를 확인해주세요.");
                    }

                    // 계정 상태 확인
                    if (user.Enabled == false)
                    {
                        throw new InvalidOperationException($"사용자 '{userId}'의 계정이 비활성화되어 있습니다. 관리자에게 문의하세요.");
                    }

                    // 계정 잠금 상태 확인 및 자동 해제
                    if (user.IsAccountLockedOut())
                    {
                        try
                        {
                            // 자동으로 계정 잠금 해제 시도
                            user.UnlockAccount();
                            user.Save();
                            wasAccountUnlocked = true;
                            statusMessage = $"계정 잠금이 자동으로 해제되었습니다. ";
                        }
                        catch (Exception unlockEx)
                        {
                            // 잠금 해제 실패 시 구체적인 오류 메시지
                            string unlockError = ErrorHandler.GetRootExceptionMessage(unlockEx);
                            if (ErrorHandler.IsAccessDeniedError(unlockError))
                            {
                                throw new UnauthorizedAccessException($"계정 잠금 해제 권한이 없습니다. 관리자 권한이 필요합니다.", unlockEx);
                            }
                            else
                            {
                                throw new InvalidOperationException($"계정 잠금 해제에 실패했습니다: {unlockError}", unlockEx);
                            }
                        }
                    }

                    // 비밀번호 변경 권한 확인 (별도의 DirectoryEntry 사용)
                    DirectoryEntry tempDe = null;
                    try
                    {
                        tempDe = (DirectoryEntry)user.GetUnderlyingObject();
                        if (tempDe.Properties["userAccountControl"].Value != null)
                        {
                            int uac = Convert.ToInt32(tempDe.Properties["userAccountControl"].Value);
                            const int PASSWD_CANT_CHANGE = 0x0040;
                            
                            if ((uac & PASSWD_CANT_CHANGE) != 0)
                            {
                                throw new InvalidOperationException($"사용자 '{userId}'는 비밀번호 변경이 금지된 계정입니다. 관리자에게 문의하세요.");
                            }
                        }
                    }
                    finally
                    {
                        // DirectoryEntry는 UserPrincipal이 관리하므로 수동으로 Dispose하지 않음
                        tempDe = null;
                    }

                    // 비밀번호 변경
                    user.SetPassword(newPassword);

                    // 다음 로그인 시 비밀번호 변경 강제 설정
                    if (mustChangeAtNextLogon)
                    {
                        user.ExpirePasswordNow();
                    }

                    user.Save();
                    
                    // 성공 메시지에 잠금 해제 정보 포함
                    if (wasAccountUnlocked)
                    {
                        throw new InvalidOperationException($"{statusMessage}비밀번호가 성공적으로 변경되었습니다.");
                    }
                    
                    return true;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // MS AD 스타일의 전문적인 영어 메시지
                string rootMessage = ErrorHandler.GetRootExceptionMessage(ex);
                
                if (rootMessage.Contains("logon failure"))
                {
                    throw new UnauthorizedAccessException("Logon failure: unknown user name or bad password.", ex);
                }
                else if (ErrorHandler.IsAccessDeniedError(rootMessage))
                {
                    throw new UnauthorizedAccessException("Access is denied. You do not have permission to reset passwords.", ex);
                }
                else
                {
                    throw new UnauthorizedAccessException($"Insufficient access rights to perform the operation. {rootMessage}", ex);
                }
            }
            catch (System.DirectoryServices.AccountManagement.PasswordException ex)
            {
                // 비밀번호 정책 위반 - MS AD 스타일
                string rootMessage = ErrorHandler.GetRootExceptionMessage(ex);
                
                if (rootMessage.Contains("complexity"))
                {
                    throw new InvalidOperationException("The password does not meet the password policy requirements. Check the minimum password length, password complexity and password history requirements.", ex);
                }
                else if (rootMessage.Contains("length"))
                {
                    throw new InvalidOperationException("The password does not meet the length requirements of the password policy.", ex);
                }
                else if (rootMessage.Contains("history"))
                {
                    throw new InvalidOperationException("The specified password does not meet the password history policy requirements.", ex);
                }
                else
                {
                    throw new InvalidOperationException($"The password does not meet the password policy requirements. {rootMessage}", ex);
                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                string comMessage = GetCOMExceptionMessage(ex, "change password");
                throw new InvalidOperationException(comMessage, ex);
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                // Inner Exception을 확인하여 실제 오류 파악
                if (tie.InnerException != null)
                {
                    string rootMessage = ErrorHandler.GetRootExceptionMessage(tie);
                    
                    // 권한 부족 오류
                    if (tie.InnerException is UnauthorizedAccessException || ErrorHandler.IsAccessDeniedError(rootMessage))
                    {
                        throw new UnauthorizedAccessException("Access is denied. You do not have permission to reset passwords. Administrator privileges may be required.", tie.InnerException);
                    }
                    // COM 예외 처리
                    else if (tie.InnerException is System.Runtime.InteropServices.COMException comEx)
                    {
                        string comMessage = GetCOMExceptionMessage(comEx, "change password");
                        throw new InvalidOperationException(comMessage, comEx);
                    }
                    // 비밀번호 정책 위반
                    else if (rootMessage.Contains("password") || 
                             rootMessage.Contains("0x800708C5") || // ERROR_PASSWORD_RESTRICTION
                             rootMessage.Contains("0x80070056")) // ERROR_INVALID_PASSWORD
                    {
                        throw new InvalidOperationException("The password does not meet the password policy requirements.", tie.InnerException);
                    }
                    else
                    {
                        // 기타 Inner Exception
                        throw new InvalidOperationException($"An error occurred while changing the password. {rootMessage}", tie.InnerException);
                    }
                }
                else
                {
                    // Inner Exception이 없는 경우
                    throw new InvalidOperationException($"An error occurred while changing the password. {tie.Message}", tie);
                }
            }
            catch (Exception ex)
            {
                // InvalidOperationException에서 성공 메시지인 경우 특별 처리
                if (ex is InvalidOperationException && ex.Message.Contains("비밀번호가 성공적으로 변경되었습니다"))
                {
                    // 성공 메시지를 그대로 전달
                    throw;
                }
                
                // 기타 예상치 못한 오류
                string rootMessage = ErrorHandler.GetRootExceptionMessage(ex);
                
                // 권한 관련 오류 표현 확인
                if (ErrorHandler.IsAccessDeniedError(rootMessage))
                {
                    throw new UnauthorizedAccessException("Access is denied. You do not have permission to reset passwords. Administrator privileges may be required.", ex);
                }
                else if (rootMessage.Contains("password") || rootMessage.Contains("Password"))
                {
                    throw new InvalidOperationException("The password does not meet the password policy requirements.", ex);
                }
                else if (rootMessage.Contains("network") || rootMessage.Contains("RPC"))
                {
                    throw new InvalidOperationException("The server is not operational. Please check network connectivity.", ex);
                }
                else
                {
                    throw new InvalidOperationException($"An error occurred while changing the password. {rootMessage}", ex);
                }
            }
        }

        /// <summary>
        /// 사용자 계정 활성화/비활성화
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="enabled">활성화 여부</param>
        /// <returns>설정 성공 여부</returns>
        public bool SetUserAccountEnabled(string userId, bool enabled)
        {
            if (!IsConnected)
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다. 먼저 로그인을 완료해주세요.");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("사용자 ID는 필수입니다.");

            try
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(_managementContext, userId))
                {
                    if (user == null)
                    {
                        throw new InvalidOperationException($"사용자 '{userId}'를 찾을 수 없습니다.");
                    }

                    user.Enabled = enabled;
                    user.Save();

                    return true;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"계정 상태 변경 권한이 없습니다: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"계정 상태 변경 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 다음 로그인 시 비밀번호 변경 강제 설정
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="mustChange">다음 로그인 시 변경 강제 여부</param>
        /// <returns>설정 성공 여부</returns>
        public bool SetPasswordMustChangeAtNextLogon(string userId, bool mustChange = true)
        {
            if (!IsConnected)
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다. 먼저 로그인을 완료해주세요.");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("사용자 ID는 필수입니다.");

            try
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(_managementContext, userId))
                {
                    if (user == null)
                    {
                        throw new InvalidOperationException($"사용자 '{userId}'를 찾을 수 없습니다.");
                    }

                    if (mustChange)
                    {
                        user.ExpirePasswordNow();
                    }
                    else
                    {
                        // 비밀번호 만료 해제는 직접적인 API가 없으므로 
                        // DirectoryEntry를 사용하여 pwdLastSet 속성을 -1로 설정
                        using (DirectoryEntry de = (DirectoryEntry)user.GetUnderlyingObject())
                        {
                            de.Properties["pwdLastSet"].Value = -1;
                            de.CommitChanges();
                        }
                    }

                    user.Save();
                    return true;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"비밀번호 정책 변경 권한이 없습니다: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"비밀번호 정책 변경 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 사용자 계정 정보 확인 (관리자 관점)
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>관리 정보가 포함된 사용자 정보</returns>
        public UserInfo GetUserManagementInfo(string userId)
        {
            if (!IsConnected)
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다.");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("사용자 ID는 필수입니다.");

            try
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(_managementContext, userId))
                {
                    if (user == null)
                        return null;

                    var userInfo = new UserInfo
                    {
                        UserId = user.SamAccountName,
                        DisplayName = user.DisplayName,
                        FullName = user.Name,
                        Email = user.EmailAddress,
                        IsEnabled = user.Enabled ?? false,
                        LastLogon = user.LastLogon,
                        PasswordLastSet = user.LastPasswordSet,
                        IsLockedOut = user.IsAccountLockedOut(),
                        AccountExpiryDate = user.AccountExpirationDate,
                        BadPasswordCount = user.BadLogonCount,
                        Description = user.Description
                    };

                    // 잠금 시간 정보
                    if (userInfo.IsLockedOut)
                    {
                        userInfo.LockoutTime = user.AccountLockoutTime;
                    }

                    return userInfo;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"사용자 관리 정보 조회 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 비밀번호 변경 권한 확인
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="credential">인증 정보</param>
        /// <returns>권한 여부</returns>
        public bool HasPasswordChangePermission(string userId, CredentialInfo credential)
        {
            if (!IsConnected)
                return false;

            if (string.IsNullOrEmpty(userId) || credential == null)
                return false;

            try
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(_managementContext, userId))
                {
                    if (user == null)
                        return false;

                    // 실제 비밀번호 변경을 시도하지 않고 권한만 확인
                    // DirectoryEntry를 사용하여 접근 권한 확인
                    using (DirectoryEntry de = (DirectoryEntry)user.GetUnderlyingObject())
                    {
                        // 사용자 객체에 접근할 수 있는지 확인
                        if (de == null)
                            return false;
                            
                        // 기본적으로 객체에 접근할 수 있다면 비밀번호 변경 권한이 있다고 가정
                        // 필요하다면 더 상세한 권한 검사를 여기에 추가할 수 있음
                        
                        // 사용자 계정 제어 플래그 확인
                        var userAccountControl = de.Properties["userAccountControl"].Value;
                        if (userAccountControl != null)
                        {
                            // 계정이 비활성화되거나 비밀번호 변경이 금지된 경우 처리
                            int uac = Convert.ToInt32(userAccountControl);
                            const int PASSWD_CANT_CHANGE = 0x0040;
                            
                            if ((uac & PASSWD_CANT_CHANGE) != 0)
                            {
                                return false; // 비밀번호 변경 금지된 계정
                            }
                        }
                        
                        return true;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 접근 권한이 없는 경우
                return false;
            }
            catch (Exception ex)
            {
                // 기타 오류 발생 시 로그 기록 후 false 반환
                System.Diagnostics.Debug.WriteLine($"HasPasswordChangePermission 오류: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 계정 잠금 해제 권한 확인
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="credential">인증 정보</param>
        /// <returns>권한 여부</returns>
        public bool HasUnlockPermission(string userId, CredentialInfo credential)
        {
            if (!IsConnected)
                return false;

            if (string.IsNullOrEmpty(userId) || credential == null)
                return false;

            try
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(_managementContext, userId))
                {
                    if (user == null)
                        return false;

                    // DirectoryEntry를 사용하여 계정 잠금 해제 권한 확인
                    using (DirectoryEntry de = (DirectoryEntry)user.GetUnderlyingObject())
                    {
                        // 사용자 객체에 접근할 수 있는지 확인
                        if (de == null)
                            return false;
                            
                        // 계정이 비활성화된 경우 잠금 해제 권한 없음
                        if (user.Enabled == false)
                            return false;
                            
                        // 권한 테스트: lockoutTime 속성에 접근할 수 있는지 확인
                        // 실제 잠금 해제를 시도하지 않고 속성 접근만 테스트
                        var lockoutTime = de.Properties["lockoutTime"].Value;
                        
                        // 기본적으로 객체와 lockoutTime 속성에 접근할 수 있다면 권한이 있다고 판단
                        return true;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 접근 권한이 없는 경우
                return false;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // COM 오류가 발생한 경우 (권한 부족 등)
                return false;
            }
            catch (Exception ex)
            {
                // 기타 오류 발생 시 로그 기록 후 false 반환
                System.Diagnostics.Debug.WriteLine($"HasUnlockPermission 오류: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// COM 예외를 분석하여 적절한 오류 메시지 반환
        /// </summary>
        /// <param name="ex">COM 예외</param>
        /// <param name="operationType">작업 유형 (예: "unlock account", "change password")</param>
        /// <returns>사용자 친화적 오류 메시지</returns>
        private string GetCOMExceptionMessage(System.Runtime.InteropServices.COMException ex, string operationType)
        {
            // HRESULT 오류 코드에 따른 상세 메시지 (MS AD 공식 스타일)
            switch (ex.HResult)
            {
                // 일반적인 액세스 및 권한 오류
                case -2147024891: // 0x80070005 - E_ACCESSDENIED
                    return "Access is denied. You do not have sufficient privileges to perform this operation.";
                case -2147023570: // 0x8007052E - ERROR_LOGON_FAILURE
                    return "Logon failure: unknown user name or bad password.";
                case -2147022651: // 0x800708C5 - ERROR_PASSWORD_RESTRICTION
                    return "Unable to update the password. The value provided for the new password does not meet the length, complexity, or history requirements of the domain.";
                case -2147022694: // 0x80070786 - ERROR_ACCOUNT_RESTRICTION
                    return "The user account has been restricted and cannot be used to log on to this computer.";
                case -2147024809: // 0x80070057 - E_INVALIDARG
                    return "One or more arguments are not valid.";
                case -2147024637: // 0x80070533 - ERROR_ACCOUNT_DISABLED
                    return "The account is disabled.";
                case -2147024234: // 0x80070056 - ERROR_INVALID_PASSWORD
                    return "The password is invalid.";
                case -2147023501: // 0x80070773 - ERROR_PASSWORD_EXPIRED
                    return "The user's password has expired.";
                case -2147023499: // 0x80070775 - ERROR_ACCOUNT_LOCKED_OUT
                    return "The referenced account is currently locked out and may not be logged on to.";
                
                // 네트워크 및 RPC 오류
                case -2147023174: // 0x800706BA - RPC_S_SERVER_UNAVAILABLE
                    return "The RPC server is unavailable.";
                case -2147023170: // 0x800706BE - RPC_S_CALL_FAILED
                    return "The remote procedure call failed.";
                case -2147024593: // 0x800704CF - ERROR_NETWORK_UNREACHABLE
                    return "The network location cannot be reached.";
                case -2147024332: // 0x800705B4 - ERROR_TIMEOUT
                    return "The operation timed out.";
                
                // 도메인 및 디렉토리 오류
                case -2147023541: // 0x8007054B - ERROR_NO_SUCH_DOMAIN
                    return "The specified domain either does not exist or could not be contacted.";
                case -2147023538: // 0x8007054E - ERROR_DOMAIN_CONTROLLER_NOT_FOUND
                    return "The domain controller for this domain could not be found.";
                
                // LDAP 기본 오류
                case -2147016656: // 0x80072030 - LDAP_NO_SUCH_OBJECT
                    return "The specified user cannot be found in the directory.";
                case -2147016655: // 0x80072031 - LDAP_INVALID_CREDENTIALS
                    return "The supplied credential is invalid.";
                case -2147016654: // 0x80072032 - LDAP_INSUFFICIENT_RIGHTS
                    return "Insufficient access rights to perform the operation.";
                case -2147016652: // 0x80072034 - LDAP_UNAVAILABLE
                    return "The server is not operational.";
                case -2147016623: // 0x80072051 - LDAP_SERVER_DOWN
                    return "The LDAP server is unavailable.";
                
                // 추가 LDAP 오류
                case -2147016671: // 0x80072021 - LDAP_OPERATIONS_ERROR
                    return "An operations error occurred.";
                case -2147016670: // 0x80072022 - LDAP_PROTOCOL_ERROR
                    return "A protocol error occurred.";
                case -2147016669: // 0x80072023 - LDAP_TIMELIMIT_EXCEEDED
                    return "The time limit for this request was exceeded.";
                case -2147016668: // 0x80072024 - LDAP_SIZELIMIT_EXCEEDED
                    return "The size limit for this request was exceeded.";
                case -2147016664: // 0x80072028 - LDAP_STRONG_AUTH_REQUIRED
                    return "Strong authentication is required for this operation.";
                case -2147016661: // 0x8007202B - LDAP_REFERRAL
                    return "The server does not hold the target entry of the request.";
                case -2147016660: // 0x8007202C - LDAP_ADMINLIMIT_EXCEEDED
                    return "Some administrative limit has been exceeded.";
                case -2147016659: // 0x8007202D - LDAP_UNAVAILABLE_CRIT_EXTENSION
                    return "A critical extension is unavailable.";
                case -2147016658: // 0x8007202E - LDAP_CONFIDENTIALITY_REQUIRED
                    return "This operation requires a secure connection.";
                case -2147016657: // 0x8007202F - LDAP_SASL_BIND_IN_PROGRESS
                    return "A SASL bind is in progress.";
                case -2147016680: // 0x80072010 - LDAP_NO_SUCH_ATTRIBUTE
                    return "The specified attribute does not exist.";
                case -2147016679: // 0x80072011 - LDAP_UNDEFINED_TYPE
                    return "An undefined attribute type was specified.";
                case -2147016678: // 0x80072012 - LDAP_INAPPROPRIATE_MATCHING
                    return "An inappropriate matching rule was specified.";
                case -2147016677: // 0x80072013 - LDAP_CONSTRAINT_VIOLATION
                    return "A constraint violation occurred.";
                case -2147016676: // 0x80072014 - LDAP_TYPE_OR_VALUE_EXISTS
                    return "An attribute or value already exists.";
                case -2147016675: // 0x80072015 - LDAP_INVALID_SYNTAX
                    return "An invalid syntax was specified.";
                
                // 시스템 오류
                case -2147024786: // 0x8007000E - E_OUTOFMEMORY
                    return "Insufficient memory resources are available to complete this operation.";
                case -2147418113: // 0x8000FFFF - E_UNEXPECTED
                    return "An unexpected error occurred.";
                case -2147467259: // 0x80004005 - E_FAIL
                    return "An unspecified error occurred.";
                
                // DS (Directory Service) 특정 오류
                case -2147016646: // 0x8007200A - ERROR_DS_ATTRIBUTE_OR_VALUE_EXISTS
                    return "The attribute or value already exists.";
                
                default:
                    // 메시지 내용에 따른 처리
                    string rootMessage = ErrorHandler.GetRootExceptionMessage(ex);
                    
                    if (ErrorHandler.IsAccessDeniedError(rootMessage))
                    {
                        return "Access is denied. You do not have sufficient privileges to perform this operation.";
                    }
                    else if (rootMessage.Contains("RPC server"))
                    {
                        return "The RPC server is unavailable. Please check the network connectivity to the domain controller.";
                    }
                    else if (rootMessage.Contains("timeout"))
                    {
                        return "The operation timed out. Please check the network connectivity and try again.";
                    }
                    else if (rootMessage.Contains("no such object") || rootMessage.Contains("not found"))
                    {
                        return "The specified object cannot be found in the directory.";
                    }
                    else if (rootMessage.Contains("password"))
                    {
                        return "The password does not meet the password policy requirements.";
                    }
                    else
                    {
                        return $"An error occurred while attempting to {operationType}. (Error: 0x{ex.HResult:X8})";
                    }
            }
        }

        /// <summary>
        /// 리소스 정리
        /// </summary>
        private void CleanupResources()
        {
            try
            {
                _managementContext?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"리소스 정리 중 오류: {ex.Message}");
            }
            finally
            {
                _managementContext = null;
            }
        }

        #region IDisposable Implementation

        /// <summary>
        /// 리소스 해제
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 리소스 해제 (내부)
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                    CleanupResources();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// 소멸자
        /// </summary>
        ~ADServiceExt()
        {
            Dispose(false);
        }

        #endregion
    }
}
