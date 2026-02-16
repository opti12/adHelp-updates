using System;
using System.Collections.Generic;
using System.Text;

namespace adHelp.Utils
{
    /// <summary>
    /// Active Directory UserAccountControl 속성 해석 도우미 클래스
    /// </summary>
    public static class UserAccountControlHelper
    {
        /// <summary>
        /// UserAccountControl 플래그 열거형
        /// </summary>
        [Flags]
        public enum UserAccountControlFlags
        {
            /// <summary>로그온 스크립트 실행</summary>
            SCRIPT = 0x0001,
            /// <summary>계정 비활성화</summary>
            ACCOUNTDISABLE = 0x0002,
            /// <summary>홈 디렉터리 필요</summary>
            HOMEDIR_REQUIRED = 0x0008,
            /// <summary>계정 잠금</summary>
            LOCKOUT = 0x0010,
            /// <summary>비밀번호 불필요</summary>
            PASSWD_NOTREQD = 0x0020,
            /// <summary>비밀번호 변경 불가</summary>
            PASSWD_CANT_CHANGE = 0x0040,
            /// <summary>암호화된 텍스트 비밀번호 허용</summary>
            ENCRYPTED_TEXT_PWD_ALLOWED = 0x0080,
            /// <summary>임시 중복 계정</summary>
            TEMP_DUPLICATE_ACCOUNT = 0x0100,
            /// <summary>일반 사용자 계정</summary>
            NORMAL_ACCOUNT = 0x0200,
            /// <summary>도메인 간 신뢰 계정</summary>
            INTERDOMAIN_TRUST_ACCOUNT = 0x0800,
            /// <summary>워크스테이션 신뢰 계정</summary>
            WORKSTATION_TRUST_ACCOUNT = 0x1000,
            /// <summary>서버 신뢰 계정</summary>
            SERVER_TRUST_ACCOUNT = 0x2000,
            /// <summary>비밀번호 만료 없음</summary>
            DONT_EXPIRE_PASSWORD = 0x10000,
            /// <summary>MNS 로그온 계정</summary>
            MNS_LOGON_ACCOUNT = 0x20000,
            /// <summary>스마트카드 로그인 필수</summary>
            SMARTCARD_REQUIRED = 0x40000,
            /// <summary>위임 신뢰</summary>
            TRUSTED_FOR_DELEGATION = 0x80000,
            /// <summary>위임 불가</summary>
            NOT_DELEGATED = 0x100000,
            /// <summary>DES 키만 사용</summary>
            USE_DES_KEY_ONLY = 0x200000,
            /// <summary>사전 인증 불필요</summary>
            DONT_REQ_PREAUTH = 0x400000,
            /// <summary>비밀번호 만료됨</summary>
            PASSWORD_EXPIRED = 0x800000,
            /// <summary>위임 인증 신뢰</summary>
            TRUSTED_TO_AUTH_FOR_DELEGATION = 0x1000000,
            /// <summary>부분 비밀 허용</summary>
            PARTIAL_SECRETS_ACCOUNT = 0x4000000
        }

        /// <summary>
        /// UserAccountControl 값을 해석하여 영문 설명 반환 (이모지 제거)
        /// </summary>
        /// <param name="userAccountControl">UserAccountControl 값</param>
        /// <returns>해석된 영문 설명 문자열</returns>
        public static string GetUserAccountControlDescriptionEnglish(int userAccountControl)
        {
            if (userAccountControl == 0)
                return "No settings";

            var flags = new List<string>();
            var uac = (UserAccountControlFlags)userAccountControl;

            // 주요 플래그들을 우선순위에 따라 확인
            if (uac.HasFlag(UserAccountControlFlags.ACCOUNTDISABLE))
                flags.Add("Account Disabled");
            else if (uac.HasFlag(UserAccountControlFlags.NORMAL_ACCOUNT))
                flags.Add("Normal User Account (Enabled)");

            if (uac.HasFlag(UserAccountControlFlags.LOCKOUT))
                flags.Add("Account Locked");

            if (uac.HasFlag(UserAccountControlFlags.PASSWORD_EXPIRED))
                flags.Add("Password Expired");

            if (uac.HasFlag(UserAccountControlFlags.PASSWD_CANT_CHANGE))
                flags.Add("Cannot Change Password");

            if (uac.HasFlag(UserAccountControlFlags.PASSWD_NOTREQD))
                flags.Add("Password Not Required");

            if (uac.HasFlag(UserAccountControlFlags.DONT_EXPIRE_PASSWORD))
                flags.Add("Password Never Expires");

            if (uac.HasFlag(UserAccountControlFlags.SMARTCARD_REQUIRED))
                flags.Add("Smart Card Required");

            if (uac.HasFlag(UserAccountControlFlags.SCRIPT))
                flags.Add("Logon Script Executed");

            if (uac.HasFlag(UserAccountControlFlags.HOMEDIR_REQUIRED))
                flags.Add("Home Directory Required");

            if (uac.HasFlag(UserAccountControlFlags.ENCRYPTED_TEXT_PWD_ALLOWED))
                flags.Add("Encrypted Text Password Allowed");

            if (uac.HasFlag(UserAccountControlFlags.TEMP_DUPLICATE_ACCOUNT))
                flags.Add("Temporary Duplicate Account");

            if (uac.HasFlag(UserAccountControlFlags.WORKSTATION_TRUST_ACCOUNT))
                flags.Add("Workstation Trust Account");

            if (uac.HasFlag(UserAccountControlFlags.SERVER_TRUST_ACCOUNT))
                flags.Add("Server Trust Account");

            if (uac.HasFlag(UserAccountControlFlags.INTERDOMAIN_TRUST_ACCOUNT))
                flags.Add("Interdomain Trust Account");

            if (uac.HasFlag(UserAccountControlFlags.TRUSTED_FOR_DELEGATION))
                flags.Add("Trusted for Delegation");

            if (uac.HasFlag(UserAccountControlFlags.NOT_DELEGATED))
                flags.Add("Not Delegated");

            if (uac.HasFlag(UserAccountControlFlags.USE_DES_KEY_ONLY))
                flags.Add("Use DES Key Only");

            if (uac.HasFlag(UserAccountControlFlags.DONT_REQ_PREAUTH))
                flags.Add("Pre-authentication Not Required");

            if (uac.HasFlag(UserAccountControlFlags.TRUSTED_TO_AUTH_FOR_DELEGATION))
                flags.Add("Trusted to Authenticate for Delegation");

            if (uac.HasFlag(UserAccountControlFlags.PARTIAL_SECRETS_ACCOUNT))
                flags.Add("Partial Secrets Account");

            if (uac.HasFlag(UserAccountControlFlags.MNS_LOGON_ACCOUNT))
                flags.Add("MNS Logon Account");

            // 플래그가 없거나 알 수 없는 조합인 경우
            if (flags.Count == 0)
            {
                if (userAccountControl == 512)
                    return "Normal User Account (Enabled)";
                else if (userAccountControl == 514)
                    return "Normal User Account (Disabled)";
                else
                    return $"Custom Settings (Value: {userAccountControl})";
            }

            return string.Join(", ", flags);
        }

        /// <summary>
        /// UserAccountControl 값을 각 플래그별로 분해하여 상세 설명 반환
        /// </summary>
        /// <param name="userAccountControl">UserAccountControl 값</param>
        /// <returns>플래그별 분해된 상세 설명</returns>
        public static string GetUserAccountControlDetailedDescription(int userAccountControl)
        {
            if (userAccountControl == 0)
                return "0: No settings";

            var details = new List<string>();
            var uac = (UserAccountControlFlags)userAccountControl;

            // 각 플래그를 개별적으로 확인하고 해당 값도 표시
            if (uac.HasFlag(UserAccountControlFlags.SCRIPT))
                details.Add($"0x0001 (1): Logon Script Executed");
                
            if (uac.HasFlag(UserAccountControlFlags.ACCOUNTDISABLE))
                details.Add($"0x0002 (2): Account Disabled");
                
            if (uac.HasFlag(UserAccountControlFlags.HOMEDIR_REQUIRED))
                details.Add($"0x0008 (8): Home Directory Required");
                
            if (uac.HasFlag(UserAccountControlFlags.LOCKOUT))
                details.Add($"0x0010 (16): Account Locked");
                
            if (uac.HasFlag(UserAccountControlFlags.PASSWD_NOTREQD))
                details.Add($"0x0020 (32): Password Not Required");
                
            if (uac.HasFlag(UserAccountControlFlags.PASSWD_CANT_CHANGE))
                details.Add($"0x0040 (64): Cannot Change Password");
                
            if (uac.HasFlag(UserAccountControlFlags.ENCRYPTED_TEXT_PWD_ALLOWED))
                details.Add($"0x0080 (128): Encrypted Text Password Allowed");
                
            if (uac.HasFlag(UserAccountControlFlags.TEMP_DUPLICATE_ACCOUNT))
                details.Add($"0x0100 (256): Temporary Duplicate Account");
                
            if (uac.HasFlag(UserAccountControlFlags.NORMAL_ACCOUNT))
                details.Add($"0x0200 (512): Normal User Account");
                
            if (uac.HasFlag(UserAccountControlFlags.INTERDOMAIN_TRUST_ACCOUNT))
                details.Add($"0x0800 (2048): Interdomain Trust Account");
                
            if (uac.HasFlag(UserAccountControlFlags.WORKSTATION_TRUST_ACCOUNT))
                details.Add($"0x1000 (4096): Workstation Trust Account");
                
            if (uac.HasFlag(UserAccountControlFlags.SERVER_TRUST_ACCOUNT))
                details.Add($"0x2000 (8192): Server Trust Account");
                
            if (uac.HasFlag(UserAccountControlFlags.DONT_EXPIRE_PASSWORD))
                details.Add($"0x10000 (65536): Password Never Expires");
                
            if (uac.HasFlag(UserAccountControlFlags.MNS_LOGON_ACCOUNT))
                details.Add($"0x20000 (131072): MNS Logon Account");
                
            if (uac.HasFlag(UserAccountControlFlags.SMARTCARD_REQUIRED))
                details.Add($"0x40000 (262144): Smart Card Required");
                
            if (uac.HasFlag(UserAccountControlFlags.TRUSTED_FOR_DELEGATION))
                details.Add($"0x80000 (524288): Trusted for Delegation");
                
            if (uac.HasFlag(UserAccountControlFlags.NOT_DELEGATED))
                details.Add($"0x100000 (1048576): Not Delegated");
                
            if (uac.HasFlag(UserAccountControlFlags.USE_DES_KEY_ONLY))
                details.Add($"0x200000 (2097152): Use DES Key Only");
                
            if (uac.HasFlag(UserAccountControlFlags.DONT_REQ_PREAUTH))
                details.Add($"0x400000 (4194304): Pre-authentication Not Required");
                
            if (uac.HasFlag(UserAccountControlFlags.PASSWORD_EXPIRED))
                details.Add($"0x800000 (8388608): Password Expired");
                
            if (uac.HasFlag(UserAccountControlFlags.TRUSTED_TO_AUTH_FOR_DELEGATION))
                details.Add($"0x1000000 (16777216): Trusted to Authenticate for Delegation");
                
            if (uac.HasFlag(UserAccountControlFlags.PARTIAL_SECRETS_ACCOUNT))
                details.Add($"0x4000000 (67108864): Partial Secrets Account");

            if (details.Count == 0)
            {
                return $"{userAccountControl}: Unknown or Custom Settings";
            }

            // 총합 표시와 개별 플래그들 - 16진수 (10진수) 순서로 통일
            string result = $"Total: 0x{userAccountControl:X} ({userAccountControl}){Environment.NewLine}{Environment.NewLine}";
            result += $"Individual Flags:{Environment.NewLine}";
            result += string.Join(Environment.NewLine, details);
            
            return result;
        }

        /// <summary>
        /// UserAccountControl 값의 간단한 상태 반환
        /// </summary>
        /// <param name="userAccountControl">UserAccountControl 값</param>
        /// <returns>간단한 상태 문자열</returns>
        public static string GetSimpleStatus(int userAccountControl)
        {
            var uac = (UserAccountControlFlags)userAccountControl;

            if (uac.HasFlag(UserAccountControlFlags.ACCOUNTDISABLE))
                return "비활성화됨";
            
            if (uac.HasFlag(UserAccountControlFlags.LOCKOUT))
                return "잠금됨";
                
            if (uac.HasFlag(UserAccountControlFlags.PASSWORD_EXPIRED))
                return "비밀번호 만료";
                
            if (uac.HasFlag(UserAccountControlFlags.NORMAL_ACCOUNT))
                return "활성화됨";

            return "상태 확인 필요";
        }

        /// <summary>
        /// 주요 보안 관련 플래그 확인
        /// </summary>
        /// <param name="userAccountControl">UserAccountControl 값</param>
        /// <returns>보안 관련 정보</returns>
        public static List<string> GetSecurityFlags(int userAccountControl)
        {
            var securityInfo = new List<string>();
            var uac = (UserAccountControlFlags)userAccountControl;

            if (uac.HasFlag(UserAccountControlFlags.PASSWD_CANT_CHANGE))
                securityInfo.Add("비밀번호 변경 제한");

            if (uac.HasFlag(UserAccountControlFlags.DONT_EXPIRE_PASSWORD))
                securityInfo.Add("비밀번호 만료 없음");

            if (uac.HasFlag(UserAccountControlFlags.SMARTCARD_REQUIRED))
                securityInfo.Add("스마트카드 필수");

            if (uac.HasFlag(UserAccountControlFlags.PASSWD_NOTREQD))
                securityInfo.Add("비밀번호 불필요");

            return securityInfo;
        }
    }
}
