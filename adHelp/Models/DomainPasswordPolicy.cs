using System;

namespace adHelp.Models
{
    /// <summary>
    /// 도메인 비밀번호 정책 정보를 담는 모델 클래스
    /// </summary>
    public class DomainPasswordPolicy
    {
        /// <summary>
        /// 비밀번호 최대 사용 기간 (일 단위)
        /// </summary>
        public int MaxPasswordAge { get; set; }

        /// <summary>
        /// 비밀번호 최소 사용 기간 (일 단위)
        /// </summary>
        public int MinPasswordAge { get; set; }

        /// <summary>
        /// 최소 비밀번호 길이
        /// </summary>
        public int MinPasswordLength { get; set; }

        /// <summary>
        /// 비밀번호 히스토리 개수
        /// </summary>
        public int PasswordHistoryLength { get; set; }

        /// <summary>
        /// 비밀번호 복잡성 요구사항
        /// </summary>
        public bool PasswordComplexityRequired { get; set; }

        /// <summary>
        /// 계정 잠금 임계값 (잘못된 로그인 시도 횟수)
        /// </summary>
        public int AccountLockoutThreshold { get; set; }

        /// <summary>
        /// 계정 잠금 기간 (분 단위)
        /// </summary>
        public int AccountLockoutDuration { get; set; }

        /// <summary>
        /// 계정 잠금 관찰 창 (분 단위)
        /// </summary>
        public int AccountLockoutObservationWindow { get; set; }

        /// <summary>
        /// 도메인 컨트롤러명
        /// </summary>
        public string DomainController { get; set; }

        /// <summary>
        /// 도메인명
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// 정책 조회 시간
        /// </summary>
        public DateTime RetrievedAt { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public DomainPasswordPolicy()
        {
            // 기본값 설정 (정책 조회 실패 시 사용)
            MaxPasswordAge = 90; // 기본 90일
            MinPasswordAge = 1;  // 기본 1일
            MinPasswordLength = 8; // 기본 8자
            PasswordHistoryLength = 24; // 기본 24개
            PasswordComplexityRequired = true;
            AccountLockoutThreshold = 0; // 기본 잠금 없음
            AccountLockoutDuration = 0;
            AccountLockoutObservationWindow = 0;
            RetrievedAt = DateTime.Now;
        }

        /// <summary>
        /// 비밀번호 만료 경고 시작일 (만료 전 14일)
        /// </summary>
        public int PasswordExpiryWarningDays => 14;

        /// <summary>
        /// 비밀번호 만료 위험 시작일 (만료 전 7일)
        /// </summary>
        public int PasswordExpiryDangerDays => 7;

        /// <summary>
        /// 정책 정보 요약
        /// </summary>
        /// <returns>정책 요약 문자열</returns>
        public string GetPolicySummary()
        {
            return $"도메인: {DomainName}\n" +
                   $"최대 비밀번호 사용기간: {MaxPasswordAge}일\n" +
                   $"최소 비밀번호 길이: {MinPasswordLength}자\n" +
                   $"복잡성 요구사항: {(PasswordComplexityRequired ? "필요" : "불필요")}\n" +
                   $"잠금 임계값: {(AccountLockoutThreshold > 0 ? $"{AccountLockoutThreshold}회 시도" : "제한 없음")}\n" +
                   $"조회 시간: {RetrievedAt:yyyy-MM-dd HH:mm:ss}";
        }

        /// <summary>
        /// 유효한 정책인지 확인
        /// </summary>
        /// <returns>유효성 여부</returns>
        public bool IsValid()
        {
            return MaxPasswordAge > 0 && 
                   MinPasswordLength > 0 && 
                   !string.IsNullOrEmpty(DomainName);
        }
    }
}
