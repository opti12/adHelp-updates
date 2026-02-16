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
    /// Active Directory 기본 연결 및 조회 서비스
    /// </summary>
    public partial class ADService : IDisposable
    {
        private DirectoryEntry _domainEntry;
        private DirectorySearcher _searcher;
        private PrincipalContext _principalContext;
        private CredentialInfo _credential;
        private bool _isConnected;
        private bool _disposed;
        private string _connectedServerName; // 실제 연결된 서버명

        /// <summary>
        /// 연결 상태
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// 현재 사용중인 크리덴셜 정보
        /// </summary>
        public CredentialInfo CurrentCredential => _credential;

        /// <summary>
        /// 도메인 비밀번호 정책 (캐시됨)
        /// </summary>
        private DomainPasswordPolicy _domainPasswordPolicy;

        /// <summary>
        /// 도메인 비밀번호 정책 조회 (캐시 사용)
        /// </summary>
        public DomainPasswordPolicy DomainPasswordPolicy
        {
            get
            {
                if (_domainPasswordPolicy == null)
                {
                    _domainPasswordPolicy = GetDomainPasswordPolicy();
                }
                return _domainPasswordPolicy;
            }
        }

        /// <summary>
        /// 실제 연결된 도메인 컨트롤러 서버명
        /// </summary>
        public string ConnectedServerName 
        { 
            get 
            {
                LogMessage($"ConnectedServerName 속성 접근 - 반환값: '{_connectedServerName}'");
                return _connectedServerName;
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public ADService()
        {
            _isConnected = false;
            _disposed = false;
            _connectedServerName = null;
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

                // LDAP 경로 생성
                string ldapPath = $"LDAP://{credential.Domain}";
                
                // DirectoryEntry 생성
                _domainEntry = new DirectoryEntry(
                    ldapPath,
                    credential.GetFullUsername(),
                    credential.GetPlainPassword()
                );

                // 연결 테스트 (실제로 AD에 접근)
                object nativeObject = _domainEntry.NativeObject;

                // 실제 연결된 도메인 컨트롤러 서버명 추출
                _connectedServerName = GetConnectedServerName();

                // DirectorySearcher 초기화
                _searcher = new DirectorySearcher(_domainEntry);
                _searcher.ClientTimeout = TimeSpan.FromSeconds(ConfigManager.ConnectionTimeout);

                // PrincipalContext 초기화 (계정 관리용)
                _principalContext = new PrincipalContext(
                    ContextType.Domain,
                    credential.Domain,
                    credential.GetFullUsername(),
                    credential.GetPlainPassword()
                );

                // 연결 성공 표시
                _isConnected = true;
                _credential.IsAuthenticated = true;
                _credential.LastAuthenticationTime = DateTime.Now;
                _credential.LastErrorMessage = null;

                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                if (_credential != null)
                {
                    _credential.IsAuthenticated = false;
                    _credential.LastErrorMessage = ex.Message;
                }

                // 리소스 정리
                CleanupResources();
                
                throw new InvalidOperationException($"AD 연결 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// AD 연결 해제
        /// </summary>
        public void Disconnect()
        {
            CleanupResources();
            _isConnected = false;
            _connectedServerName = null;
            
            if (_credential != null)
            {
                _credential.IsAuthenticated = false;
            }
        }

        /// <summary>
        /// 사용자 정보 조회
        /// </summary>
        /// <param name="userId">사용자 ID (SAM Account Name)</param>
        /// <returns>사용자 정보 객체</returns>
        public UserInfo GetUserInfo(string userId)
        {
            if (!_isConnected)
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다.");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("사용자 ID는 필수입니다.");

            try
            {
                // LDAP 필터 설정
                _searcher.Filter = $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={userId}))";
                
                // 가져올 속성 지정
                _searcher.PropertiesToLoad.Clear();
                AddPropertiesToLoad();

                // 검색 실행
                SearchResult result = _searcher.FindOne();
                
                if (result == null)
                {
                    return null; // 사용자 없음
                }

                // UserInfo 객체 생성 및 데이터 매핑
                return MapSearchResultToUserInfo(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"사용자 정보 조회 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 연결 상태 테스트
        /// </summary>
        /// <returns>연결 상태</returns>
        public bool TestConnection()
        {
            if (!_isConnected || _domainEntry == null)
                return false;

            try
            {
                // 간단한 LDAP 쿼리로 연결 테스트
                object nativeObject = _domainEntry.NativeObject;
                return true;
            }
            catch
            {
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// 사용자 존재 여부 확인
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>존재 여부</returns>
        public bool UserExists(string userId)
        {
            try
            {
                return GetUserInfo(userId) != null;
            }
            catch
            {
                return false;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// 로깅 메시지 출력 (디버그 및 텍스트 파일)
        /// </summary>
        /// <param name="message">로그 메시지</param>
        private void LogMessage(string message)
        {
            // VS 디버그 출력
            System.Diagnostics.Debug.WriteLine(message);
            
            // 텍스트 파일 로깅 (실행파일 위치에 생성)
            SimpleLogger.Log($"ADService: {message}");
        }

        /// <summary>
        /// 실제 연결된 도메인 컨트롤러 서버명 추출
        /// </summary>
        /// <returns>연결된 서버명 (FQDN)</returns>
        private string GetConnectedServerName()
        {
            try
            {
                LogMessage("서버명 추출 시작...");
                
                // 방법 1: RootDSE를 통해 실제 연결된 서버 정보 가져오기
                try
                {
                    using (DirectoryEntry rootDSE = new DirectoryEntry("LDAP://RootDSE", 
                        _credential.GetFullUsername(), 
                        _credential.GetPlainPassword()))
                    {
                        object nativeObject = rootDSE.NativeObject; // 연결 강제
                        LogMessage("RootDSE 연결 성공");
                        
                        // dnsHostName 속성 조회
                        if (rootDSE.Properties.Contains("dnsHostName"))
                        {
                            var dnsHostNameProp = rootDSE.Properties["dnsHostName"];
                            if (dnsHostNameProp.Count > 0 && dnsHostNameProp.Value != null)
                            {
                                string serverName = dnsHostNameProp.Value.ToString();
                                LogMessage($"RootDSE dnsHostName 발견: {serverName}");
                                if (!string.IsNullOrEmpty(serverName))
                                {
                                    LogMessage($"RootDSE 방법 성공 - 서버명 반환: {serverName}");
                                    return serverName;
                                }
                            }
                        }
                        
                        // serverName 속성 조회
                        if (rootDSE.Properties.Contains("serverName"))
                        {
                            var serverNameProp = rootDSE.Properties["serverName"];
                            if (serverNameProp.Count > 0 && serverNameProp.Value != null)
                            {
                                string serverDN = serverNameProp.Value.ToString();
                                LogMessage($"RootDSE serverName 발견: {serverDN}");
                                
                                // CN=ADSDCI00858P81,CN=Servers,... 에서 서버명만 추출
                                if (serverDN.StartsWith("CN="))
                                {
                                    int commaIndex = serverDN.IndexOf(',');
                                    if (commaIndex > 3)
                                    {
                                        string serverShortName = serverDN.Substring(3, commaIndex - 3);
                                        string fullServerName = $"{serverShortName}.{_credential.Domain}";
                                        LogMessage($"RootDSE serverName 방법 성공 - 서버 FQDN: {fullServerName}");
                                        return fullServerName;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"RootDSE 조회 실패: {ex.Message}");
                }
                
                // 방법 2: DirectoryEntry Path에서 서버명 추출
                try
                {
                    if (_domainEntry != null)
                    {
                        string path = _domainEntry.Path;
                        LogMessage($"DirectoryEntry Path: {path}");
                        
                        if (!string.IsNullOrEmpty(path) && path.StartsWith("LDAP://"))
                        {
                            // LDAP://서버명/DC=... 형태에서 서버명 추출
                            string serverPart = path.Substring(7); // "LDAP://" 제거
                            int slashIndex = serverPart.IndexOf('/');
                            if (slashIndex > 0)
                            {
                                string serverName = serverPart.Substring(0, slashIndex);
                                if (!serverName.Equals(_credential.Domain, StringComparison.OrdinalIgnoreCase))
                                {
                                    LogMessage($"Path 방법 성공 - 서버명 반환: {serverName}");
                                    return serverName;
                                }
                            }
                            else if (!serverPart.Contains("/"))
                            {
                                // LDAP://서버명 형태
                                if (!serverPart.Equals(_credential.Domain, StringComparison.OrdinalIgnoreCase))
                                {
                                    LogMessage($"Path 방법 성공 - 서버명 반환: {serverPart}");
                                    return serverPart;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"DirectoryEntry Path 추출 실패: {ex.Message}");
                }
                
                // 방법 3: LOGONSERVER 환경변수 사용
                try
                {
                    string logonServer = Environment.GetEnvironmentVariable("LOGONSERVER");
                    System.Diagnostics.Debug.WriteLine($"LOGONSERVER 환경변수: {logonServer}");
                    if (!string.IsNullOrEmpty(logonServer))
                    {
                        string serverName = logonServer.TrimStart('\\');
                        if (!string.IsNullOrEmpty(serverName))
                        {
                            // LOGONSERVER는 로컬 로그온 서버이므로 AD 서버와 다를 수 있음
                            // IP 도메인인 경우 LOGONSERVER 사용 안함
                            if (!_credential.Domain.Contains(".") || _credential.Domain.Contains("192.168"))
                            {
                                System.Diagnostics.Debug.WriteLine($"IP 도메인이므로 LOGONSERVER 사용 안함");
                            }
                            else
                            {
                                string fullName = $"{serverName}.{_credential.Domain}";
                                System.Diagnostics.Debug.WriteLine($"LOGONSERVER에서 가져온 서버: {fullName}");
                                return fullName;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"LOGONSERVER 환경변수 조회 실패: {ex.Message}");
                }
                
                // 방법 4: DirectoryEntry의 옵션을 통해 서버 정보 확인
                try
                {
                    // 다시 DirectoryEntry의 Properties에서 서버 정보 찾기
                    if (_domainEntry != null && _domainEntry.Properties != null)
                    {
                        System.Diagnostics.Debug.WriteLine("도메인 엔트리 속성 조회");
                        foreach (string propName in _domainEntry.Properties.PropertyNames)
                        {
                            System.Diagnostics.Debug.WriteLine($"도메인 엔트리 속성: {propName}");
                        }
                        
                        // 도메인 엔트리에서 서버 정보 찾기
                        if (_domainEntry.Properties.Contains("serverName"))
                        {
                            var serverProp = _domainEntry.Properties["serverName"];
                            if (serverProp.Count > 0 && serverProp.Value != null)
                            {
                                string serverInfo = serverProp.Value.ToString();
                                System.Diagnostics.Debug.WriteLine($"도메인 엔트리 serverName: {serverInfo}");
                                return serverInfo;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"도메인 엔트리 조회 실패: {ex.Message}");
                }
                
                // 모든 방법 실패 - 도메인명만 반환
                LogMessage("모든 서버명 추출 방법 실패 - 도메인명 반환");
                return _credential.Domain;
            }
            catch (Exception ex)  
            {
                LogMessage($"서버명 추출 전체 오류: {ex.Message}");
                return _credential?.Domain ?? "알 수 없음";
            }
        }

        /// <summary>
        /// 검색할 속성들을 추가
        /// </summary>
        private void AddPropertiesToLoad()
        {
            string[] properties = {
                "sAMAccountName",        // 사용자 ID
                "displayName",           // 표시 이름
                "cn",                    // Common Name
                "mail",                  // 이메일
                "userAccountControl",    // 계정 제어 플래그
                "lastLogon",            // 마지막 로그인
                "lastLogonTimestamp",    // 마지막 로그인 타임스탬프
                "pwdLastSet",           // 비밀번호 마지막 설정
                "msDS-UserPasswordExpiryTimeComputed", // 계산된 비밀번호 만료 시간
                "userAccountControl",    // 계정 제어 플래그 (비밀번호 만료되지 않음 체크용)
                "accountExpires",       // 계정 만료
                "lockoutTime",          // 잠금 시간
                "badPwdCount",          // 잘못된 비밀번호 횟수
                "logonCount",          // 로그온 횟수
                "department",           // 부서
                "title",                // 직책
                "telephoneNumber",      // 전화번호
                "physicalDeliveryOfficeName", // 사무실
                "manager",              // 상급자
                "whenCreated",          // 생성일
                "whenChanged",          // 수정일
                "memberOf",             // 그룹 멤버십
                "homeDirectory",        // 홈 디렉토리
                "scriptPath",           // 로그인 스크립트
                "distinguishedName",    // DN
                "userPrincipalName"     // UPN
            };

            foreach (string property in properties)
            {
                _searcher.PropertiesToLoad.Add(property);
            }
        }

        /// <summary>
        /// SearchResult를 UserInfo 객체로 매핑
        /// </summary>
        /// <param name="result">검색 결과</param>
        /// <returns>UserInfo 객체</returns>
        private UserInfo MapSearchResultToUserInfo(SearchResult result)
        {
            try
            {
                var userInfo = new UserInfo();

                // 기본 정보
                userInfo.UserId = GetPropertyValue(result, "sAMAccountName");
                userInfo.DisplayName = GetPropertyValue(result, "displayName");
                userInfo.FullName = GetPropertyValue(result, "cn");
                userInfo.Email = GetPropertyValue(result, "mail");
                userInfo.DistinguishedName = GetPropertyValue(result, "distinguishedName");

                // 계정 상태 정보
                int userAccountControl = GetPropertyValueAsInt(result, "userAccountControl");
                userInfo.UserAccountControl = userAccountControl;
                userInfo.IsEnabled = (userAccountControl & 0x0002) == 0; // ADS_UF_ACCOUNTDISABLE
                
                // 시간 정보
                userInfo.LastLogon = ConvertLdapTimeToDateTime(GetPropertyValue(result, "lastLogon"));
                userInfo.PasswordLastSet = ConvertLdapTimeToDateTime(GetPropertyValue(result, "pwdLastSet"));
                userInfo.WhenCreated = GetPropertyValueAsDateTime(result, "whenCreated");
                userInfo.WhenChanged = GetPropertyValueAsDateTime(result, "whenChanged");

                // 잠금 및 실패 정보
                userInfo.AccountExpiryDate = ConvertAccountExpiresToDateTime(GetPropertyValue(result, "accountExpires"));
                userInfo.LockoutTime = ConvertLdapTimeToDateTime(GetPropertyValue(result, "lockoutTime"));
                userInfo.IsLockedOut = userInfo.LockoutTime.HasValue && userInfo.LockoutTime.Value > DateTime.MinValue;
                userInfo.BadPasswordCount = GetPropertyValueAsInt(result, "badPwdCount");
                userInfo.LogonCount = GetPropertyValueAsInt(result, "logonCount");

                // 조직 정보
                userInfo.Department = GetPropertyValue(result, "department");
                userInfo.Title = GetPropertyValue(result, "title");
                userInfo.PhoneNumber = GetPropertyValue(result, "telephoneNumber");
                userInfo.Office = GetPropertyValue(result, "physicalDeliveryOfficeName");
                userInfo.Manager = GetPropertyValue(result, "manager");

                // 기타 정보
                userInfo.HomeDirectory = GetPropertyValue(result, "homeDirectory");
                userInfo.LoginScript = GetPropertyValue(result, "scriptPath");

                // 그룹 멤버십
                userInfo.MemberOfGroups = GetPropertyValues(result, "memberOf");

                // 비밀번호 만료 계산 (실제 AD 속성 사용)
                CalculatePasswordExpiry(userInfo, result);

                return userInfo;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"사용자 정보 매핑 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 속성값 가져오기 (문자열)
        /// </summary>
        internal string GetPropertyValue(SearchResult result, string propertyName)
        {
            if (result.Properties.Contains(propertyName) && result.Properties[propertyName].Count > 0)
            {
                return result.Properties[propertyName][0]?.ToString();
            }
            return null;
        }

        /// <summary>
        /// 속성값 가져오기 (정수)
        /// </summary>
        internal int GetPropertyValueAsInt(SearchResult result, string propertyName)
        {
            string value = GetPropertyValue(result, propertyName);
            if (int.TryParse(value, out int intValue))
            {
                return intValue;
            }
            return 0;
        }

        /// <summary>
        /// 속성값 가져오기 (DateTime)
        /// </summary>
        private DateTime? GetPropertyValueAsDateTime(SearchResult result, string propertyName)
        {
            if (result.Properties.Contains(propertyName) && result.Properties[propertyName].Count > 0)
            {
                object value = result.Properties[propertyName][0];
                if (value is DateTime dateTime)
                {
                    return dateTime;
                }
            }
            return null;
        }

        /// <summary>
        /// 속성값들 가져오기 (배열)
        /// </summary>
        private string[] GetPropertyValues(SearchResult result, string propertyName)
        {
            if (result.Properties.Contains(propertyName))
            {
                var values = new List<string>();
                foreach (object value in result.Properties[propertyName])
                {
                    values.Add(value.ToString());
                }
                return values.ToArray();
            }
            return new string[0];
        }

        /// <summary>
        /// LDAP 시간을 DateTime으로 변환
        /// </summary>
        private DateTime? ConvertLdapTimeToDateTime(string ldapTime)
        {
            if (string.IsNullOrEmpty(ldapTime) || ldapTime == "0")
                return null;

            try
            {
                long fileTime = Convert.ToInt64(ldapTime);
                if (fileTime == 0)
                    return null;

                return DateTime.FromFileTime(fileTime);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// AccountExpires 값을 DateTime으로 변환
        /// </summary>
        private DateTime? ConvertAccountExpiresToDateTime(string accountExpires)
        {
            if (string.IsNullOrEmpty(accountExpires))
                return null;

            try
            {
                long expires = Convert.ToInt64(accountExpires);
                if (expires == 0 || expires == long.MaxValue || expires == 9223372036854775807)
                    return null; // 만료되지 않음

                return DateTime.FromFileTime(expires);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 리소스 정리
        /// </summary>
        private void CleanupResources()
        {
            try
            {
                _searcher?.Dispose();
                _domainEntry?.Dispose();
                _principalContext?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"리소스 정리 중 오류: {ex.Message}");
            }
            finally
            {
                _searcher = null;
                _domainEntry = null;
                _principalContext = null;
            }
        }

        #endregion

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
        ~ADService()
        {
            Dispose(false);
        }

        #endregion

        #region Credential Verification

        /// <summary>
        /// 사용자 자격 증명 검증
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="password">비밀번호</param>
        /// <returns>인증 성공 여부</returns>
        public bool VerifyUserCredentials(string userId, string password)
        {
            if (!_isConnected)
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다.");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
                throw new ArgumentException("사용자 ID와 비밀번호는 필수입니다.");

            try
            {
                // 사용자 검색
                using (UserPrincipal user = UserPrincipal.FindByIdentity(_principalContext, userId))
                {
                    if (user == null)
                    {
                        return false; // 사용자 없음
                    }

                    // 계정 상태 확인
                    if (user.Enabled == false)
                    {
                        return false; // 비활성화된 계정
                    }

                    // 계정 잠금 확인
                    if (user.IsAccountLockedOut())
                    {
                        return false; // 잠긴 계정
                    }

                    // 새로운 PrincipalContext로 자격 증명 검증
                    using (var testContext = new PrincipalContext(
                        ContextType.Domain,
                        _credential.Domain,
                        userId,
                        password))
                    {
                        // 테스트 연결 시도
                        using (var testUser = UserPrincipal.FindByIdentity(testContext, userId))
                        {
                            return testUser != null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 인증 실패 또는 네트워크 오류
                System.Diagnostics.Debug.WriteLine($"자격 증명 검증 실패: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
