using System;
using System.Reflection;

namespace adHelp.Utils
{
    /// <summary>
    /// 애플리케이션 버전 정보를 관리하는 유틸리티 클래스
    /// </summary>
    public static class VersionHelper
    {
        /// <summary>
        /// 현재 어셈블리의 버전 정보를 가져옵니다
        /// </summary>
        public static Version Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        /// <summary>
        /// 버전 문자열을 가져옵니다 (예: "1.2.0.0")
        /// </summary>
        public static string VersionString
        {
            get
            {
                return Version.ToString();
            }
        }

        /// <summary>
        /// 간단한 버전 문자열을 가져옵니다 (예: "1.2.1.1")
        /// </summary>
        public static string ShortVersionString
        {
            get
            {
                var version = Version;
                if (version.Revision > 0)
                {
                    return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                }
                else
                {
                    return $"{version.Major}.{version.Minor}.{version.Build}";
                }
            }
        }

        /// <summary>
        /// AssemblyInformationalVersion을 가져옵니다
        /// </summary>
        public static string InformationalVersion
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                return attribute?.InformationalVersion ?? VersionString;
            }
        }

        /// <summary>
        /// 빌드 정보가 포함된 동적 버전 문자열을 가져옵니다
        /// </summary>
        public static string BuildVersion
        {
            get
            {
                var baseVersion = ShortVersionString;
                var buildDate = BuildDate;
                var buildNumber = GetBuildNumber(buildDate);
                
                // 예: "1.2.2.240720" (년도 뒤 2자리 + 월일)
                var lastDotIndex = baseVersion.LastIndexOf('.');
                if (lastDotIndex > 0)
                {
                    return $"{baseVersion.Substring(0, lastDotIndex)}.{buildNumber}";
                }
                else
                {
                    return $"{baseVersion}.{buildNumber}";
                }
            }
        }

        /// <summary>
        /// 빌드 정보를 포함한 상세 버전 문자열을 가져옵니다
        /// </summary>
        public static string DetailedVersionString
        {
            get
            {
                var baseVersion = ShortVersionString;
                var buildDate = BuildDate;
                
                return $"{baseVersion} (빌드 {BuildDateString})";
            }
        }

        /// <summary>
        /// 제품 이름을 가져옵니다
        /// </summary>
        public static string ProductName
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var attribute = assembly.GetCustomAttribute<AssemblyProductAttribute>();
                return attribute?.Product ?? "AD Helper";
            }
        }

        /// <summary>
        /// 제품 제목을 가져옵니다
        /// </summary>
        public static string ProductTitle
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var attribute = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
                return attribute?.Title ?? "adHelp";
            }
        }

        /// <summary>
        /// 설명을 가져옵니다
        /// </summary>
        public static string Description
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var attribute = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
                return attribute?.Description ?? "";
            }
        }

        /// <summary>
        /// 저작권 정보를 가져옵니다
        /// </summary>
        public static string Copyright
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var attribute = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
                return attribute?.Copyright ?? "";
            }
        }

        /// <summary>
        /// 전체 제품 타이틀을 생성합니다 (제품명 + 버전)
        /// </summary>
        /// <param name="includeBuildInfo">빌드 정보 포함 여부</param>
        /// <returns>전체 제품 타이틀</returns>
        public static string GetFullProductTitle(bool includeBuildInfo = true)
        {
            if (includeBuildInfo)
            {
                return $"{ProductName} v{DetailedVersionString}";
            }
            else
            {
                return $"{ProductName} v{ShortVersionString}";
            }
        }

        /// <summary>
        /// About 다이얼로그용 버전 문자열을 생성합니다
        /// </summary>
        /// <returns>About용 버전 문자열</returns>
        public static string GetAboutVersionString()
        {
            return $"{ProductName} v{DetailedVersionString}\n" +
                   $"어셈블리 버전: {VersionString}\n" +
                   $"파일 버전: {Version}\n" +
                   $"런타임: {RuntimeInfo}";
        }

        /// <summary>
        /// 윈도우 타이틀용 문자열을 생성합니다
        /// </summary>
        /// <param name="currentUser">현재 사용자 정보</param>
        /// <param name="connectedDomain">연결된 도메인 정보</param>
        /// <returns>윈도우 타이틀</returns>
        public static string GetWindowTitle(string currentUser = null, string connectedDomain = null)
        {
            var baseTitle = GetFullProductTitle(false);

            if (!string.IsNullOrEmpty(currentUser) && !string.IsNullOrEmpty(connectedDomain))
            {
                return $"{baseTitle} - {currentUser}@{connectedDomain}";
            }
            else if (!string.IsNullOrEmpty(currentUser))
            {
                return $"{baseTitle} - {currentUser}";
            }
            else
            {
                return baseTitle;
            }
        }

        /// <summary>
        /// 빌드 날짜를 가져옵니다 (어셈블리 생성 시간 기준)
        /// </summary>
        public static DateTime BuildDate
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fileInfo = new System.IO.FileInfo(assembly.Location);
                return fileInfo.CreationTime;
            }
        }

        /// <summary>
        /// 빌드 날짜 문자열을 가져옵니다
        /// </summary>
        public static string BuildDateString
        {
            get
            {
                return BuildDate.ToString("yyyy-MM-dd HH:mm");
            }
        }

        /// <summary>
        /// 빌드 번호를 생성합니다 (YYMMDD 형식)
        /// </summary>
        /// <param name="buildDate">빌드 날짜</param>
        /// <returns>빌드 번호</returns>
        private static string GetBuildNumber(DateTime buildDate)
        {
            return buildDate.ToString("yyMMdd");
        }

        /// <summary>
        /// 런타임 정보를 가져옵니다
        /// </summary>
        public static string RuntimeInfo
        {
            get
            {
                return $".NET Framework {Environment.Version}";
            }
        }

        /// <summary>
        /// 상세한 시스템 정보를 가져옵니다
        /// </summary>
        public static string SystemInfo
        {
            get
            {
                return $"OS: {Environment.OSVersion}, 머신: {Environment.MachineName}";
            }
        }
    }
}
