namespace adHelp.Models
{
    /// <summary>
    /// 그룹 구성원 정보 클래스
    /// </summary>
    public class GroupMemberInfo
    {
        /// <summary>
        /// 표시명
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// SAM 계정명
        /// </summary>
        public string SAMAccountName { get; set; }

        /// <summary>
        /// 구성원 유형 (사용자, 그룹, 기타)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 이메일 주소
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 활성화 상태
        /// </summary>
        public string Enabled { get; set; }

        /// <summary>
        /// 구분된 이름 (DN)
        /// </summary>
        public string DistinguishedName { get; set; }

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public GroupMemberInfo()
        {
            DisplayName = "-";
            SAMAccountName = "-";
            Type = "-";
            Email = "-";
            Enabled = "-";
            DistinguishedName = "-";
        }

        /// <summary>
        /// 매개변수 생성자
        /// </summary>
        public GroupMemberInfo(string displayName, string samAccountName, string type, string email, string enabled, string distinguishedName = "-")
        {
            DisplayName = displayName ?? "-";
            SAMAccountName = samAccountName ?? "-";
            Type = type ?? "-";
            Email = email ?? "-";
            Enabled = enabled ?? "-";
            DistinguishedName = distinguishedName ?? "-";
        }
    }
}
