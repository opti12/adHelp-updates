using System;
using System.Collections.Generic;
using System.Linq;

namespace adHelp.Models
{
    /// <summary>
    /// Active Directory 사용자 정보를 담는 모델 클래스
    /// .NET Framework 4.7.2 호환 버전
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 기본 생성자
        /// </summary>
        public UserInfo()
        {
            IsEnabled = true;
            MemberOfGroups = new List<string>();
        }

        /// <summary>
        /// 사용자 ID (SAM Account Name)
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 사용자 표시 이름
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 사용자 전체 이름
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 이메일 주소
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 계정 활성화 상태
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 계정 잠금 상태
        /// </summary>
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// 비밀번호 만료 여부
        /// </summary>
        public bool IsPasswordExpired { get; set; }

        /// <summary>
        /// 다음 로그인 시 비밀번호 변경 필요 여부 (pwdLastSet = 0)
        /// </summary>
        public bool MustChangePasswordAtNextLogon { get; set; }

        /// <summary>
        /// 마지막 로그인 시간
        /// </summary>
        public DateTime? LastLogon { get; set; }

        /// <summary>
        /// 비밀번호 마지막 변경 시간
        /// </summary>
        public DateTime? PasswordLastSet { get; set; }

        /// <summary>
        /// 비밀번호 만료 날짜
        /// </summary>
        public DateTime? PasswordExpiryDate { get; set; }

        /// <summary>
        /// 암호를 바꿀 수 있는 날짜 (pwdLastSet + minPwdAge)
        /// </summary>
        public DateTime? PasswordCanChangeDate { get; set; }

        /// <summary>
        /// 계정 만료 날짜
        /// </summary>
        public DateTime? AccountExpiryDate { get; set; }

        /// <summary>
        /// 비밀번호 틀린 횟수
        /// </summary>
        public int BadPasswordCount { get; set; }

        /// <summary>
        /// 계정 잠금 시간
        /// </summary>
        public DateTime? LockoutTime { get; set; }

        /// <summary>
        /// 소속 부서
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// 직책
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 전화번호
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 사무실 위치
        /// </summary>
        public string Office { get; set; }

        /// <summary>
        /// 상급자 정보
        /// </summary>
        public string Manager { get; set; }

        /// <summary>
        /// 계정 생성 날짜
        /// </summary>
        public DateTime? WhenCreated { get; set; }

        /// <summary>
        /// 계정 마지막 수정 날짜
        /// </summary>
        public DateTime? WhenChanged { get; set; }

        /// <summary>
        /// 사용자 주요 그룹 (Primary Group)
        /// </summary>
        public string PrimaryGroup { get; set; }

        /// <summary>
        /// 사용자가 속한 그룹 목록
        /// </summary>
        public IList<string> MemberOfGroups { get; set; }

        /// <summary>
        /// 홈 디렉토리
        /// </summary>
        public string HomeDirectory { get; set; }

        /// <summary>
        /// 로그인 스크립트
        /// </summary>
        public string LoginScript { get; set; }

        /// <summary>
        /// 사용자 계정 제어 플래그
        /// </summary>
        public int UserAccountControl { get; set; }

        /// <summary>
        /// 로그온 횟수 (logonCount)
        /// </summary>
        public int LogonCount { get; set; }

        /// <summary>
        /// 설명
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Distinguished Name (DN)
        /// </summary>
        public string DistinguishedName { get; set; }

        /// <summary>
        /// 계정 상태를 문자열로 반환
        /// </summary>
        /// <returns>계정 상태 설명</returns>
        public string GetAccountStatus()
        {
            if (!IsEnabled)
                return "비활성화";
            if (IsLockedOut)
                return "잠금";
            if (IsPasswordExpired)
                return "비밀번호 만료";
            return "정상";
        }

        /// <summary>
        /// 비밀번호 상태를 문자열로 반환
        /// </summary>
        /// <returns>비밀번호 상태 설명</returns>
        public string GetPasswordStatus()
        {
            if (MustChangePasswordAtNextLogon)
                return "다음 로그인 시 변경 필요";
                
            if (IsPasswordExpired)
                return "만료됨";
                
            if (PasswordExpiryDate.HasValue)
            {
                var daysLeft = (PasswordExpiryDate.Value - DateTime.Now).Days;
                if (daysLeft <= 0)
                    return "만료됨";
                if (daysLeft <= 7)
                    return $"곧 만료 ({daysLeft}일 남음)";
                return $"정상 ({daysLeft}일 남음)";
            }
            
            if (PasswordLastSet.HasValue)
                return "만료되지 않음";
                
            return "설정 필요";
        }

        /// <summary>
        /// 비밀번호 상태의 위험도를 반환
        /// </summary>
        /// <returns>위험도 수준</returns>
        public PasswordRiskLevel GetPasswordRiskLevel()
        {
            var status = GetPasswordStatus();
            
            if (status.Contains("만료") || status.Contains("설정 필요"))
                return PasswordRiskLevel.Critical;
                
            if (status.Contains("곧 만료") || status.Contains("변경 필요"))
                return PasswordRiskLevel.Warning;
                
            return PasswordRiskLevel.Normal;
        }

        /// <summary>
        /// 사용자 정보의 완성도를 확인
        /// </summary>
        /// <returns>프로필 완성 여부</returns>
        public bool IsProfileComplete()
        {
            return !string.IsNullOrWhiteSpace(DisplayName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Department);
        }

        /// <summary>
        /// 계정이 활성 상태인지 확인 (종합 판단)
        /// </summary>
        /// <returns>활성 계정 여부</returns>
        public bool IsActiveAccount()
        {
            return IsEnabled && 
                   !IsLockedOut && 
                   !IsPasswordExpired &&
                   (!AccountExpiryDate.HasValue || AccountExpiryDate.Value > DateTime.Now);
        }

        /// <summary>
        /// 객체의 문자열 표현
        /// </summary>
        /// <returns>사용자 정보 요약</returns>
        public override string ToString()
        {
            var displayName = string.IsNullOrWhiteSpace(DisplayName) ? "이름 없음" : DisplayName;
            return $"{UserId} ({displayName}) - {GetAccountStatus()}";
        }
    }

    /// <summary>
    /// 비밀번호 위험도 열거형
    /// </summary>
    public enum PasswordRiskLevel
    {
        Normal,
        Warning, 
        Critical
    }
}
