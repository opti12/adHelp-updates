using System;
using System.DirectoryServices;
using adHelp.Models;
using adHelp.Utils;

namespace adHelp.Services
{
    /// <summary>
    /// Active Directory 도메인 정책 및 OU 관리 서비스
    /// </summary>
    public partial class ADService
    {
        /// <summary>
        /// 도메인 비밀번호 정책 조회
        /// </summary>
        /// <returns>도메인 비밀번호 정책 정보</returns>
        public DomainPasswordPolicy GetDomainPasswordPolicy()
        {
            System.Diagnostics.Debug.WriteLine("=== GetDomainPasswordPolicy 시작 ===");
            
            if (!_isConnected)
            {
                System.Diagnostics.Debug.WriteLine("❌ AD에 연결되어 있지 않음");
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다.");
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"도메인: {_credential?.Domain}");
                System.Diagnostics.Debug.WriteLine($"연결된 서버: {_connectedServerName}");
                System.Diagnostics.Debug.WriteLine($"도메인 엔트리 경로: {_domainEntry?.Path}");
                
                var policy = new DomainPasswordPolicy
                {
                    DomainName = _credential?.Domain ?? "Unknown",
                    DomainController = _connectedServerName ?? "Unknown",
                    RetrievedAt = DateTime.Now
                };

                // 도메인 루트 엔트리에 접근
                using (var domainEntry = new DirectoryEntry(
                    _domainEntry.Path,
                    _credential.GetFullUsername(),
                    _credential.GetPlainPassword()))
                {
                    System.Diagnostics.Debug.WriteLine("도메인 엔트리 생성 성공");
                    
                    // 도메인 정책 속성들 조회
                    string[] propertiesToLoad = {
                        "maxPwdAge",
                        "minPwdAge", 
                        "minPwdLength",
                        "pwdHistoryLength",
                        "pwdProperties",
                        "lockoutThreshold",
                        "lockoutDuration",
                        "lockOutObservationWindow"
                    };
                    
                    System.Diagnostics.Debug.WriteLine($"속성 로드 시도: {string.Join(", ", propertiesToLoad)}");
                    domainEntry.RefreshCache(propertiesToLoad);
                    System.Diagnostics.Debug.WriteLine("속성 로드 완료");

                    // 최대 비밀번호 사용 기간 (100ns 단위, 음수로 저장됨)
                    if (domainEntry.Properties.Contains("maxPwdAge") && domainEntry.Properties["maxPwdAge"].Count > 0)
                    {
                        var maxPwdAgeValue = domainEntry.Properties["maxPwdAge"][0];
                        if (long.TryParse(maxPwdAgeValue.ToString(), out long longValue) && longValue < 0)
                        {
                            // 100ns 단위를 일 단위로 변환 (10,000,000 * 60 * 60 * 24 = 864,000,000,000)
                            policy.MaxPasswordAge = (int)(Math.Abs(longValue) / 864000000000);
                        }
                    }

                    // 최소 비밀번호 사용 기간
                    if (domainEntry.Properties.Contains("minPwdAge") && domainEntry.Properties["minPwdAge"].Count > 0)
                    {
                        var minPwdAgeValue = domainEntry.Properties["minPwdAge"][0];
                        if (long.TryParse(minPwdAgeValue.ToString(), out long longValue))
                        {
                            policy.MinPasswordAge = (int)(Math.Abs(longValue) / 864000000000);
                        }
                    }

                    // 최소 비밀번호 길이
                    System.Diagnostics.Debug.WriteLine("비밀번호 최소 길이 속성 확인 중...");
                    if (domainEntry.Properties.Contains("minPwdLength") && domainEntry.Properties["minPwdLength"].Count > 0)
                    {
                        var minPwdLengthValue = domainEntry.Properties["minPwdLength"][0];
                        System.Diagnostics.Debug.WriteLine($"minPwdLength 원시 값: {minPwdLengthValue} (타입: {minPwdLengthValue?.GetType()})");
                        
                        if (int.TryParse(minPwdLengthValue.ToString(), out int minLength))
                        {
                            policy.MinPasswordLength = minLength;
                            System.Diagnostics.Debug.WriteLine($"✅ 최소 비밀번호 길이 설정: {minLength}자");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ minPwdLength 파싱 실패: {minPwdLengthValue}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ minPwdLength 속성이 없거나 비어있음");
                    }

                    // 비밀번호 히스토리 길이
                    if (domainEntry.Properties.Contains("pwdHistoryLength") && domainEntry.Properties["pwdHistoryLength"].Count > 0)
                    {
                        if (int.TryParse(domainEntry.Properties["pwdHistoryLength"][0].ToString(), out int historyLength))
                        {
                            policy.PasswordHistoryLength = historyLength;
                        }
                    }

                    // 비밀번호 복잡성 요구사항 (pwdProperties 비트 플래그)
                    if (domainEntry.Properties.Contains("pwdProperties") && domainEntry.Properties["pwdProperties"].Count > 0)
                    {
                        if (int.TryParse(domainEntry.Properties["pwdProperties"][0].ToString(), out int pwdProperties))
                        {
                            // DOMAIN_PASSWORD_COMPLEX = 0x00000001
                            policy.PasswordComplexityRequired = (pwdProperties & 0x00000001) != 0;
                        }
                    }

                    // 계정 잠금 임계값
                    if (domainEntry.Properties.Contains("lockoutThreshold") && domainEntry.Properties["lockoutThreshold"].Count > 0)
                    {
                        if (int.TryParse(domainEntry.Properties["lockoutThreshold"][0].ToString(), out int threshold))
                        {
                            policy.AccountLockoutThreshold = threshold;
                        }
                    }

                    // 계정 잠금 기간 (분 단위로 변환)
                    if (domainEntry.Properties.Contains("lockoutDuration") && domainEntry.Properties["lockoutDuration"].Count > 0)
                    {
                        var lockoutDurationValue = domainEntry.Properties["lockoutDuration"][0];
                        if (long.TryParse(lockoutDurationValue.ToString(), out long longValue) && longValue < 0)
                        {
                            // 100ns 단위를 분 단위로 변환 (10,000,000 * 60 = 600,000,000)
                            policy.AccountLockoutDuration = (int)(Math.Abs(longValue) / 600000000);
                        }
                    }

                    // 계정 잠금 관찰 창
                    if (domainEntry.Properties.Contains("lockOutObservationWindow") && domainEntry.Properties["lockOutObservationWindow"].Count > 0)
                    {
                        var observationWindowValue = domainEntry.Properties["lockOutObservationWindow"][0];
                        if (long.TryParse(observationWindowValue.ToString(), out long longValue) && longValue < 0)
                        {
                            policy.AccountLockoutObservationWindow = (int)(Math.Abs(longValue) / 600000000);
                        }
                    }
                }

                // 정책 조회 결과 상세 로그
                System.Diagnostics.Debug.WriteLine("=== 도메인 정책 조회 결과 ===");
                System.Diagnostics.Debug.WriteLine($"최대 비밀번호 사용기간: {policy.MaxPasswordAge}일");
                System.Diagnostics.Debug.WriteLine($"최소 비밀번호 사용기간: {policy.MinPasswordAge}일");
                System.Diagnostics.Debug.WriteLine($"최소 비밀번호 길이: {policy.MinPasswordLength}자");
                System.Diagnostics.Debug.WriteLine($"비밀번호 히스토리: {policy.PasswordHistoryLength}개");
                System.Diagnostics.Debug.WriteLine($"복잡성 요구사항: {policy.PasswordComplexityRequired}");
                System.Diagnostics.Debug.WriteLine($"잠금 임계값: {policy.AccountLockoutThreshold}회");
                System.Diagnostics.Debug.WriteLine($"정책 유효성: {policy.IsValid()}");
                System.Diagnostics.Debug.WriteLine("=== 도메인 정책 조회 완료 ===");
                
                LogMessage($"AD 정책 조회 성공: 최대 {policy.MaxPasswordAge}일, 최소 길이 {policy.MinPasswordLength}자");
                return policy;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetDomainPasswordPolicy 예외 발생: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"스택 트레이스: {ex.StackTrace}");
                LogMessage($"AD 정책 조회 실패: {ex.Message}");
                
                // 기본값 반환 (오류 시)
                var fallbackPolicy = new DomainPasswordPolicy
                {
                    DomainName = _credential?.Domain ?? "Unknown",
                    DomainController = _connectedServerName ?? "Unknown",
                    RetrievedAt = DateTime.Now
                };
                
                System.Diagnostics.Debug.WriteLine($"기본값 정책 반환 - 최소 길이: {fallbackPolicy.MinPasswordLength}자");
                return fallbackPolicy;
            }
        }

        /// <summary>
        /// 비밀번호 만료 및 변경 가능 날짜 계산 (실제 AD 속성 사용)
        /// </summary>
        /// <param name="userInfo">사용자 정보 객체</param>
        /// <param name="result">검색 결과</param>
        internal void CalculatePasswordExpiry(UserInfo userInfo, SearchResult result)
        {
            try
            {
                // 1. userAccountControl에서 DONT_EXPIRE_PASSWORD 플래그 확인
                int userAccountControl = GetPropertyValueAsInt(result, "userAccountControl");
                const int ADS_UF_DONT_EXPIRE_PASSWD = 0x10000; // 65536
                
                bool passwordNeverExpires = (userAccountControl & ADS_UF_DONT_EXPIRE_PASSWD) != 0;
                
                if (passwordNeverExpires)
                {
                    userInfo.PasswordExpiryDate = null;
                    userInfo.IsPasswordExpired = false;
                    
                    // 비밀번호가 만료되지 않는 경우, 암호 변경 가능 날짜 계산
                    if (userInfo.PasswordLastSet.HasValue)
                    {
                        var domainPolicy = DomainPasswordPolicy;
                        int minPasswordAge = domainPolicy.MinPasswordAge;
                        
                        if (minPasswordAge > 0)
                        {
                            userInfo.PasswordCanChangeDate = userInfo.PasswordLastSet.Value.AddDays(minPasswordAge);
                        }
                        else
                        {
                            userInfo.PasswordCanChangeDate = userInfo.PasswordLastSet.Value;
                        }
                    }
                    else
                    {
                        userInfo.PasswordCanChangeDate = null;
                    }
                    return;
                }
                
                // 2. pwdLastSet 특수값 처리
                string pwdLastSetStr = GetPropertyValue(result, "pwdLastSet");
                if (!string.IsNullOrEmpty(pwdLastSetStr))
                {
                    long pwdLastSetValue = Convert.ToInt64(pwdLastSetStr);
                    
                    if (pwdLastSetValue == 0)
                    {
                        // 0 = 다음 로그인 시 비밀번호 변경 필요
                        userInfo.MustChangePasswordAtNextLogon = true;
                        userInfo.PasswordExpiryDate = DateTime.Now; // 참조용
                        userInfo.IsPasswordExpired = false; // 만료과는 구분
                        userInfo.PasswordCanChangeDate = DateTime.Now; // 즉시 변경 가능
                        return;
                    }
                    
                    if (pwdLastSetValue == -1)
                    {
                        // -1 = 비밀번호 만료되지 않음
                        userInfo.PasswordExpiryDate = null;
                        userInfo.IsPasswordExpired = false;
                        userInfo.PasswordCanChangeDate = DateTime.Now; // 즉시 변경 가능
                        return;
                    }
                }
                
                // 3. msDS-UserPasswordExpiryTimeComputed 속성 사용 (계산된 비밀번호 만료 시간)
                string computedExpiryStr = GetPropertyValue(result, "msDS-UserPasswordExpiryTimeComputed");
                if (!string.IsNullOrEmpty(computedExpiryStr))
                {
                    long computedExpiry = Convert.ToInt64(computedExpiryStr);
                    
                    if (computedExpiry == 0 || computedExpiry == long.MaxValue || computedExpiry == 9223372036854775807)
                    {
                        // 만료되지 않음
                        userInfo.PasswordExpiryDate = null;
                        userInfo.IsPasswordExpired = false;
                    }
                    else
                    {
                        // 실제 계산된 만료 날짜 사용
                        userInfo.PasswordExpiryDate = DateTime.FromFileTime(computedExpiry);
                        userInfo.IsPasswordExpired = userInfo.PasswordExpiryDate.HasValue && userInfo.PasswordExpiryDate.Value <= DateTime.Now;
                    }
                    
                    // 암호 변경 가능 날짜 계산 (pwdLastSet + minPasswordAge)
                    if (userInfo.PasswordLastSet.HasValue)
                    {
                        var domainPolicy = DomainPasswordPolicy;
                        int minPasswordAge = domainPolicy.MinPasswordAge;
                        
                        if (minPasswordAge > 0)
                        {
                            userInfo.PasswordCanChangeDate = userInfo.PasswordLastSet.Value.AddDays(minPasswordAge);
                        }
                        else
                        {
                            userInfo.PasswordCanChangeDate = userInfo.PasswordLastSet.Value;
                        }
                    }
                    else
                    {
                        userInfo.PasswordCanChangeDate = null;
                    }
                    return;
                }
                
                // 4. 계산된 만료 시간이 없는 경우, pwdLastSet 기반으로 도메인 정책 사용
                if (userInfo.PasswordLastSet.HasValue)
                {
                    // 도메인 정책에서 최대 비밀번호 사용 기간 가져오기
                    var domainPolicy = DomainPasswordPolicy;
                    int maxPasswordAge = domainPolicy.MaxPasswordAge;
                    int minPasswordAge = domainPolicy.MinPasswordAge;
                    
                    if (maxPasswordAge > 0)
                    {
                        userInfo.PasswordExpiryDate = userInfo.PasswordLastSet.Value.AddDays(maxPasswordAge);
                        userInfo.IsPasswordExpired = userInfo.PasswordExpiryDate.Value <= DateTime.Now;
                    }
                    else
                    {
                        // 도메인 정책에서 비밀번호 만료이 설정되지 않은 경우
                        userInfo.PasswordExpiryDate = null;
                        userInfo.IsPasswordExpired = false;
                    }
                    
                    // 암호를 바꿀 수 있는 날짜 계산 (pwdLastSet + minPasswordAge)
                    if (minPasswordAge > 0)
                    {
                        userInfo.PasswordCanChangeDate = userInfo.PasswordLastSet.Value.AddDays(minPasswordAge);
                    }
                    else
                    {
                        // 최소 비밀번호 사용 기간이 0인 경우 즉시 변경 가능
                        userInfo.PasswordCanChangeDate = userInfo.PasswordLastSet.Value;
                    }
                }
                else
                {
                    // pwdLastSet도 없는 경우 - 상태를 알 수 없음
                    userInfo.PasswordExpiryDate = null;
                    userInfo.IsPasswordExpired = false;
                    userInfo.PasswordCanChangeDate = null;
                }
            }
            catch (Exception ex)
            {
                // 오류 발생 시 기본값 설정
                System.Diagnostics.Debug.WriteLine($"비밀번호 만료 계산 오류: {ex.Message}");
                userInfo.PasswordExpiryDate = null;
                userInfo.IsPasswordExpired = false;
                userInfo.PasswordCanChangeDate = null;
            }
        }

        #region OU (Organizational Unit) Helper Methods

        /// <summary>
        /// 사용자의 OU 경로 추출
        /// </summary>
        /// <param name="distinguishedName">사용자의 Distinguished Name</param>
        /// <returns>OU 경로 (예: OU=Persons,OU=KR,OU=Enterprise,DC=INTL,DC=costco,DC=com)</returns>
        public static string ExtractOUPath(string distinguishedName)
        {
            if (string.IsNullOrEmpty(distinguishedName))
                return null;

            try
            {
                // DN에서 CN= 부분을 제거하고 OU= 부분만 추출
                // 예: CN=user123,OU=Persons,OU=KR,OU=Enterprise,DC=INTL,DC=costco,DC=com
                // -> OU=Persons,OU=KR,OU=Enterprise,DC=INTL,DC=costco,DC=com
                
                int firstCommaIndex = distinguishedName.IndexOf(',');
                if (firstCommaIndex == -1)
                    return null;
                
                string ouPath = distinguishedName.Substring(firstCommaIndex + 1).Trim();
                return ouPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OU 경로 추출 오류: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 사용자가 허용된 OU 경로에 속하는지 확인
        /// </summary>
        /// <param name="userOUPath">사용자의 OU 경로</param>
        /// <param name="allowedOUPath">허용된 OU 경로</param>
        /// <returns>허용된 OU 또는 하위 OU에 속하면 true</returns>
        public static bool IsUserInAllowedOU(string userOUPath, string allowedOUPath)
        {
            if (string.IsNullOrEmpty(userOUPath) || string.IsNullOrEmpty(allowedOUPath))
                return false;

            try
            {
                // 대소문자 구분 없이 비교
                string userOU = userOUPath.ToUpperInvariant();
                string allowedOU = allowedOUPath.ToUpperInvariant();
                
                // 정확히 일치하거나 허용된 OU의 하위 OU인지 확인
                // 예: 사용자 OU가 "OU=SubUnit,OU=Persons,OU=KR,OU=Enterprise,DC=INTL,DC=costco,DC=com"
                //     허용된 OU가 "OU=Persons,OU=KR,OU=Enterprise,DC=INTL,DC=costco,DC=com"
                //     -> true (하위 OU)
                
                return userOU.Equals(allowedOU) || userOU.EndsWith("," + allowedOU);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OU 검증 오류: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 코스트코 한국 조직의 허용된 OU 경로
        /// </summary>
        public static readonly string ALLOWED_COSTCO_KR_OU = "OU=Persons,OU=KR,OU=Enterprise,DC=INTL,DC=costco,DC=com";

        /// <summary>
        /// 사용자가 코스트코 한국 조직에 속하는지 확인
        /// </summary>
        /// <param name="userOUPath">사용자의 OU 경로</param>
        /// <returns>코스트코 한국 조직에 속하면 true</returns>
        public static bool IsUserInCostcoKoreaOU(string userOUPath)
        {
            return IsUserInAllowedOU(userOUPath, ALLOWED_COSTCO_KR_OU);
        }

        #endregion
    }
}
