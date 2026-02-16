using System;
using System.Security;

namespace adHelp.Models
{
    /// <summary>
    /// Active Directory 연결용 크리덴셜 정보를 담는 모델 클래스
    /// </summary>
    public class CredentialInfo
    {
        /// <summary>
        /// 도메인 이름
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 사용자 ID
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 비밀번호 (보안 문자열)
        /// </summary>
        public SecureString Password { get; set; }

        /// <summary>
        /// 도메인 컨트롤러 서버 주소
        /// </summary>
        public string DomainController { get; set; }

        /// <summary>
        /// LDAP 연결 문자열
        /// </summary>
        public string LdapPath { get; set; }

        /// <summary>
        /// 연결 포트 (기본값: 389, SSL의 경우 636)
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// SSL 사용 여부
        /// </summary>
        public bool UseSSL { get; set; }

        /// <summary>
        /// 인증 성공 여부
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// 마지막 인증 시간
        /// </summary>
        public DateTime? LastAuthenticationTime { get; set; }

        /// <summary>
        /// 인증 오류 메시지
        /// </summary>
        public string LastErrorMessage { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public CredentialInfo()
        {
            Port = 389; // 기본 LDAP 포트
            UseSSL = false;
            IsAuthenticated = false;
        }

        /// <summary>
        /// 생성자 (도메인과 사용자명 지정)
        /// </summary>
        /// <param name="domain">도메인 이름</param>
        /// <param name="username">사용자 ID</param>
        public CredentialInfo(string domain, string username) : this()
        {
            Domain = domain;
            Username = username;
            GenerateLdapPath();
        }

        /// <summary>
        /// 전체 생성자
        /// </summary>
        /// <param name="domain">도메인 이름</param>
        /// <param name="username">사용자 ID</param>
        /// <param name="password">비밀번호</param>
        /// <param name="useSSL">SSL 사용 여부</param>
        public CredentialInfo(string domain, string username, SecureString password, bool useSSL = false) : this(domain, username)
        {
            Password = password;
            UseSSL = useSSL;
            Port = useSSL ? 636 : 389;
        }

        /// <summary>
        /// 도메인 이름으로부터 LDAP 경로 생성
        /// </summary>
        private void GenerateLdapPath()
        {
            if (!string.IsNullOrEmpty(Domain))
            {
                // 도메인을 DC 형식으로 변환 (예: costco.com -> DC=costco,DC=com)
                var dcComponents = Domain.Split('.');
                var ldapDC = string.Join(",", Array.ConvertAll(dcComponents, dc => $"DC={dc}"));
                LdapPath = $"LDAP://{Domain}/{ldapDC}";
            }
        }

        /// <summary>
        /// 도메인 설정 시 LDAP 경로 자동 생성
        /// </summary>
        /// <param name="domain">도메인 이름</param>
        public void SetDomain(string domain)
        {
            Domain = domain;
            GenerateLdapPath();
        }

        /// <summary>
        /// 사용자 전체 이름 반환 (도메인\사용자)
        /// </summary>
        /// <returns>도메인\사용자 형식의 문자열</returns>
        public string GetFullUsername()
        {
            if (string.IsNullOrEmpty(Domain) || string.IsNullOrEmpty(Username))
                return Username ?? string.Empty;
            
            return $"{Domain}\\{Username}";
        }

        /// <summary>
        /// UPN 형식 사용자명 반환 (사용자@도메인)
        /// </summary>
        /// <returns>사용자@도메인 형식의 문자열</returns>
        public string GetUserPrincipalName()
        {
            if (string.IsNullOrEmpty(Domain) || string.IsNullOrEmpty(Username))
                return Username ?? string.Empty;
            
            return $"{Username}@{Domain}";
        }

        /// <summary>
        /// 크리덴셜이 유효한지 확인
        /// </summary>
        /// <returns>유효성 여부</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Domain) && 
                   !string.IsNullOrEmpty(Username) && 
                   Password != null && 
                   Password.Length > 0;
        }

        /// <summary>
        /// 비밀번호를 일반 문자열로 변환 (주의: 보안에 취약)
        /// </summary>
        /// <returns>평문 비밀번호</returns>
        public string GetPlainPassword()
        {
            if (Password == null)
                return string.Empty;

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(Password);
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }

        /// <summary>
        /// 일반 문자열을 SecureString으로 변환
        /// </summary>
        /// <param name="password">평문 비밀번호</param>
        public void SetPassword(string password)
        {
            if (Password != null)
            {
                Password.Dispose();
            }

            Password = new SecureString();
            if (!string.IsNullOrEmpty(password))
            {
                foreach (char c in password)
                {
                    Password.AppendChar(c);
                }
                Password.MakeReadOnly();
            }
        }

        /// <summary>
        /// 리소스 해제
        /// </summary>
        public void Dispose()
        {
            Password?.Dispose();
        }

        /// <summary>
        /// 객체의 문자열 표현 (비밀번호 제외)
        /// </summary>
        /// <returns>크리덴셜 정보 요약</returns>
        public override string ToString()
        {
            var status = IsAuthenticated ? "인증됨" : "미인증";
            return $"{GetFullUsername()} - {status}";
        }
    }
}
