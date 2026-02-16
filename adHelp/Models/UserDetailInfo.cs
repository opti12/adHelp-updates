using System;
using System.Collections.Generic;

namespace adHelp.Models
{
    /// <summary>
    /// Active Directory 사용자 상세 정보를 담는 모델 클래스
    /// Get-ADUser -Properties * 결과와 같은 모든 AD 속성 포함
    /// </summary>
    public class UserDetailInfo
    {
        /// <summary>
        /// 기본 사용자 정보
        /// </summary>
        public UserInfo BasicInfo { get; set; }

        /// <summary>
        /// 모든 AD 속성들을 담는 딕셔너리
        /// Key: 속성 이름, Value: 속성 값
        /// </summary>
        public Dictionary<string, object> AllProperties { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public UserDetailInfo()
        {
            AllProperties = new Dictionary<string, object>();
        }

        /// <summary>
        /// 속성 값을 안전하게 가져오기
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        /// <returns>속성 값 (문자열)</returns>
        public string GetPropertySafely(string propertyName)
        {
            if (AllProperties.ContainsKey(propertyName))
            {
                var value = AllProperties[propertyName];
                if (value == null)
                    return "";

                // 배열인 경우 첫 번째 값 또는 전체 배열을 문자열로 변환
                if (value is Array array)
                {
                    if (array.Length == 0)
                        return "";
                    
                    if (array.Length == 1)
                        return array.GetValue(0)?.ToString() ?? "";
                    
                    // 여러 값이 있는 경우 콤마로 구분
                    var items = new List<string>();
                    for (int i = 0; i < Math.Min(array.Length, 10); i++) // 최대 10개만 표시
                    {
                        var item = array.GetValue(i)?.ToString();
                        if (!string.IsNullOrEmpty(item))
                            items.Add(item);
                    }
                    
                    if (array.Length > 10)
                        items.Add($"... (총 {array.Length}개)");
                    
                    return string.Join(", ", items);
                }

                // DateTime 처리
                if (value is DateTime dateTime)
                {
                    if (dateTime == DateTime.MinValue)
                        return "";
                    return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }

                // Long 타입 타임스탬프 처리 (AD 타임스탬프)
                if (value is long longValue)
                {
                    // AD 타임스탬프 (100-nanosecond intervals since January 1, 1601)
                    if (longValue > 0 && longValue < 9223372036854775807) // MaxValue 체크
                    {
                        try
                        {
                            var adEpoch = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                            var convertedTime = adEpoch.AddTicks(longValue).ToLocalTime();
                            
                            // 합리적인 날짜 범위인지 확인 (1900년 ~ 2100년)
                            if (convertedTime.Year >= 1900 && convertedTime.Year <= 2100)
                                return convertedTime.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        catch
                        {
                            // 변환 실패 시 원본 값 반환
                        }
                    }
                    
                    return longValue.ToString();
                }

                // 바이트 배열 처리
                if (value is byte[] bytes)
                {
                    if (bytes.Length == 0)
                        return "";
                    
                    if (bytes.Length <= 16)
                        return BitConverter.ToString(bytes).Replace("-", "");
                    
                    return $"Binary Data ({bytes.Length} bytes)";
                }

                // 기본 문자열 변환
                return value.ToString();
            }

            return "";
        }

        /// <summary>
        /// 속성 값을 분류별로 정리하여 반환
        /// </summary>
        /// <returns>분류별 속성 딕셔너리</returns>
        public Dictionary<string, Dictionary<string, string>> GetCategorizedProperties()
        {
            var categories = new Dictionary<string, Dictionary<string, string>>();

            // 기본 정보
            var basicInfo = new Dictionary<string, string>();
            AddPropertyToCategory(basicInfo, "사용자 ID (SamAccountName)", "SamAccountName");
            AddPropertyToCategory(basicInfo, "표시 이름 (DisplayName)", "DisplayName");
            AddPropertyToCategory(basicInfo, "전체 이름 (Name)", "Name");
            AddPropertyToCategory(basicInfo, "공통 이름 (CN)", "CN");
            AddPropertyToCategory(basicInfo, "사용자 주체 이름 (UserPrincipalName)", "UserPrincipalName");
            AddPropertyToCategory(basicInfo, "구분 이름 (DistinguishedName)", "DistinguishedName");
            AddPropertyToCategory(basicInfo, "정규 이름 (CanonicalName)", "CanonicalName");
            categories["기본 정보"] = basicInfo;

            // 계정 정보
            var accountInfo = new Dictionary<string, string>();
            AddPropertyToCategory(accountInfo, "활성화 여부 (Enabled)", "Enabled");
            AddPropertyToCategory(accountInfo, "계정 상태 (UserAccountControl)", "userAccountControl");
            AddPropertyToCategory(accountInfo, "계정 만료 날짜 (AccountExpirationDate)", "AccountExpirationDate");
            AddPropertyToCategory(accountInfo, "계정 만료 (accountExpires)", "accountExpires");
            AddPropertyToCategory(accountInfo, "계정 생성일 (Created)", "Created");
            AddPropertyToCategory(accountInfo, "계정 수정일 (Modified)", "Modified");
            AddPropertyToCategory(accountInfo, "생성 타임스탬프 (createTimeStamp)", "createTimeStamp");
            AddPropertyToCategory(accountInfo, "수정 타임스탬프 (modifyTimeStamp)", "modifyTimeStamp");
            categories["계정 정보"] = accountInfo;

            return categories;
        }

        /// <summary>
        /// 속성을 카테고리에 추가하는 헬퍼 메서드
        /// </summary>
        /// <param name="category">카테고리 딕셔너리</param>
        /// <param name="displayName">표시 이름</param>
        /// <param name="propertyName">속성 이름</param>
        private void AddPropertyToCategory(Dictionary<string, string> category, string displayName, string propertyName)
        {
            var value = GetPropertySafely(propertyName);
            if (!string.IsNullOrEmpty(value))
            {
                category[displayName] = value;
            }
        }
    }
}
