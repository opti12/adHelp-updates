using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using adHelp.Models;

namespace adHelp.Utils
{
    /// <summary>
    /// 애플리케이션 설정 및 구성 정보를 관리하는 클래스
    /// </summary>
    public static class ConfigManager
    {
        // 기본 설정값들
        private static readonly Dictionary<string, object> defaultSettings = new Dictionary<string, object>
        {
            { "DefaultDomain", "intl.costco.com" },
            { "DefaultAdminSuffix", "-a" },
            { "LdapPort", 389 },
            { "LdapSSLPort", 636 },
            { "UseSSL", false },
            { "ConnectionTimeout", 30 },
            { "PasswordLength", 12 },
            { "PasswordIncludeNumbers", true },
            { "PasswordIncludeSpecialChars", false },
            { "AutoLockScreen", false },
            { "LogLevel", "Info" },
            { "MaxRetryCount", 3 },
            { "SessionTimeout", 36000 }, // 10시간 (초 단위)
            { "AutoUpdate.Enabled", true } // 자동 업데이트 활성화
        };

        // 런타임 설정 캐시
        private static readonly Dictionary<string, object> runtimeSettings = new Dictionary<string, object>();

        // 초기화 상태
        private static bool isInitialized = false;

        /// <summary>
        /// 설정 관리자 초기화
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized)
                return;

            LoadDefaultSettings();
            LoadAppConfigSettings();
            
            isInitialized = true;
        }

        /// <summary>
        /// 기본 도메인 가져오기
        /// </summary>
        public static string DefaultDomain => GetSetting<string>("DefaultDomain");

        /// <summary>
        /// 기본 관리자 계정 접미사 가져오기
        /// </summary>
        public static string DefaultAdminSuffix => GetSetting<string>("DefaultAdminSuffix");

        /// <summary>
        /// 현재 사용자명 기반 관리자 계정 ID 생성
        /// </summary>
        /// <returns>관리자 계정 ID</returns>
        public static string GetDefaultAdminUsername()
        {
            string currentUser = Environment.UserName;
            string suffix = DefaultAdminSuffix;
            
            // 이미 접미사가 있는지 확인
            if (currentUser.EndsWith(suffix))
                return currentUser;
            
            return currentUser + suffix;
        }

        /// <summary>
        /// LDAP 포트 번호 가져오기
        /// </summary>
        /// <param name="useSSL">SSL 사용 여부</param>
        /// <returns>포트 번호</returns>
        public static int GetLdapPort(bool useSSL = false)
        {
            return useSSL ? GetSetting<int>("LdapSSLPort") : GetSetting<int>("LdapPort");
        }

        /// <summary>
        /// SSL 사용 여부 가져오기
        /// </summary>
        public static bool UseSSL => GetSetting<bool>("UseSSL");

        /// <summary>
        /// 연결 타임아웃 가져오기 (초)
        /// </summary>
        public static int ConnectionTimeout => GetSetting<int>("ConnectionTimeout");

        /// <summary>
        /// 기본 비밀번호 길이 가져오기
        /// </summary>
        public static int PasswordLength => GetSetting<int>("PasswordLength");

        /// <summary>
        /// 비밀번호에 숫자 포함 여부
        /// </summary>
        public static bool PasswordIncludeNumbers => GetSetting<bool>("PasswordIncludeNumbers");

        /// <summary>
        /// 비밀번호에 특수문자 포함 여부
        /// </summary>
        public static bool PasswordIncludeSpecialChars => GetSetting<bool>("PasswordIncludeSpecialChars");

        /// <summary>
        /// 최대 재시도 횟수
        /// </summary>
        public static int MaxRetryCount => GetSetting<int>("MaxRetryCount");

        /// <summary>
        /// 세션 타임아웃 (초)
        /// </summary>
        public static int SessionTimeout => GetSetting<int>("SessionTimeout");

        /// <summary>
        /// 설정값 가져오기 (제네릭)
        /// </summary>
        /// <typeparam name="T">반환할 타입</typeparam>
        /// <param name="key">설정 키</param>
        /// <returns>설정값</returns>
        public static T GetSetting<T>(string key)
        {
            Initialize();

            // 런타임 설정 확인
            if (runtimeSettings.ContainsKey(key))
            {
                return ConvertValue<T>(runtimeSettings[key]);
            }

            // 기본 설정 확인
            if (defaultSettings.ContainsKey(key))
            {
                return ConvertValue<T>(defaultSettings[key]);
            }

            // 기본값 반환
            return default(T);
        }

        /// <summary>
        /// 설정값 저장 (런타임)
        /// </summary>
        /// <param name="key">설정 키</param>
        /// <param name="value">설정값</param>
        public static void SetSetting(string key, object value)
        {
            Initialize();
            runtimeSettings[key] = value;
        }

        /// <summary>
        /// 기본 크리덴셜 정보 생성
        /// </summary>
        /// <param name="username">사용자명 (null이면 기본 관리자 계정 사용)</param>
        /// <returns>크리덴셜 정보 객체</returns>
        public static CredentialInfo CreateDefaultCredential(string username = null)
        {
            string user = username ?? GetDefaultAdminUsername();
            string domain = DefaultDomain;
            bool useSSL = UseSSL;

            return new CredentialInfo(domain, user, null, useSSL);
        }

        /// <summary>
        /// LDAP 연결 문자열 생성
        /// </summary>
        /// <param name="domain">도메인</param>
        /// <param name="useSSL">SSL 사용 여부</param>
        /// <returns>LDAP 연결 문자열</returns>
        public static string BuildLdapConnectionString(string domain = null, bool? useSSL = null)
        {
            string targetDomain = domain ?? DefaultDomain;
            bool ssl = useSSL ?? UseSSL;
            
            string protocol = ssl ? "LDAPS" : "LDAP";
            int port = GetLdapPort(ssl);
            
            return $"{protocol}://{targetDomain}:{port}";
        }

        /// <summary>
        /// 도메인을 Distinguished Name 형식으로 변환
        /// </summary>
        /// <param name="domain">도메인명</param>
        /// <returns>DN 형식 문자열</returns>
        public static string DomainToDN(string domain = null)
        {
            string targetDomain = domain ?? DefaultDomain;
            
            if (string.IsNullOrEmpty(targetDomain))
                return string.Empty;

            string[] parts = targetDomain.Split('.');
            return string.Join(",", Array.ConvertAll(parts, part => $"DC={part}"));
        }

        /// <summary>
        /// 애플리케이션 데이터 디렉토리 경로 가져오기
        /// </summary>
        /// <returns>데이터 디렉토리 경로</returns>
        public static string GetAppDataDirectory()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDir = Path.Combine(appData, "adHelp");
            
            if (!Directory.Exists(appDir))
            {
                Directory.CreateDirectory(appDir);
            }
            
            return appDir;
        }

        /// <summary>
        /// 로그 파일 경로 가져오기
        /// </summary>
        /// <returns>로그 파일 경로</returns>
        public static string GetLogFilePath()
        {
            string dataDir = GetAppDataDirectory();
            string logFileName = $"adHelp_{DateTime.Now:yyyyMMdd}.log";
            return Path.Combine(dataDir, logFileName);
        }

        /// <summary>
        /// 설정 검증
        /// </summary>
        /// <returns>검증 결과</returns>
        public static bool ValidateSettings()
        {
            try
            {
                // 필수 설정 확인
                if (string.IsNullOrEmpty(DefaultDomain))
                    return false;

                if (string.IsNullOrEmpty(DefaultAdminSuffix))
                    return false;

                // 포트 범위 확인
                int ldapPort = GetLdapPort(false);
                int sslPort = GetLdapPort(true);
                
                if (ldapPort < 1 || ldapPort > 65535)
                    return false;
                
                if (sslPort < 1 || sslPort > 65535)
                    return false;

                // 비밀번호 길이 확인
                if (PasswordLength < 6 || PasswordLength > 128)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 설정 정보를 문자열로 출력 (디버깅용)
        /// </summary>
        /// <returns>설정 정보 문자열</returns>
        public static string GetSettingsInfo()
        {
            Initialize();
            
            var info = new List<string>
            {
                $"기본 도메인: {DefaultDomain}",
                $"관리자 접미사: {DefaultAdminSuffix}",
                $"기본 관리자 계정: {GetDefaultAdminUsername()}",
                $"LDAP 포트: {GetLdapPort(false)}",
                $"LDAP SSL 포트: {GetLdapPort(true)}",
                $"SSL 사용: {UseSSL}",
                $"연결 타임아웃: {ConnectionTimeout}초",
                $"비밀번호 길이: {PasswordLength}",
                $"비밀번호 숫자 포함: {PasswordIncludeNumbers}",
                $"비밀번호 특수문자 포함: {PasswordIncludeSpecialChars}",
                $"최대 재시도: {MaxRetryCount}",
                $"세션 타임아웃: {SessionTimeout}초"
            };

            return string.Join("\n", info);
        }

        #region Private Methods

        private static void LoadDefaultSettings()
        {
            // 기본 설정은 이미 defaultSettings에 정의되어 있음
        }

        private static void LoadAppConfigSettings()
        {
            try
            {
                // App.config에서 설정 읽기
                foreach (string key in defaultSettings.Keys)
                {
                    string configValue = System.Configuration.ConfigurationManager.AppSettings[key];
                    if (!string.IsNullOrEmpty(configValue))
                    {
                        // 타입에 맞게 변환하여 기본값 업데이트
                        object convertedValue = ConvertStringToType(configValue, defaultSettings[key].GetType());
                        if (convertedValue != null)
                        {
                            defaultSettings[key] = convertedValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 설정 로드 실패는 무시하고 기본값 사용
                System.Diagnostics.Debug.WriteLine($"설정 로드 실패: {ex.Message}");
            }
        }

        private static T ConvertValue<T>(object value)
        {
            if (value == null)
                return default(T);

            if (value is T)
                return (T)value;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        private static object ConvertStringToType(string value, Type targetType)
        {
            try
            {
                if (targetType == typeof(bool))
                {
                    return bool.Parse(value);
                }
                else if (targetType == typeof(int))
                {
                    return int.Parse(value);
                }
                else if (targetType == typeof(string))
                {
                    return value;
                }
                
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
