using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace adHelp.Utils
{
    /// <summary>
    /// 안전하고 사용하기 쉬운 비밀번호를 생성하는 클래스
    /// </summary>
    public static class PasswordGenerator
    {
        // 기본 단어들 (코스트코 관련)
        private static readonly string[] BaseWords = {
            "Costco", "Zhtmxmzh", "Wholesale", "Member", "Value", "Quality", "Service",
            "Store", "Warehouse", "Shopping", "Business", "Company", "Market"
        };

        // 추가 단어들 (일반적인 단어)
        private static readonly string[] CommonWords = {
            "Welcome", "Password", "Secure", "Access", "Login", "Account",
            "System", "Admin", "Manager", "Office", "Computer", "Network"
        };

        // 숫자 조합
        private static readonly string[] NumberSuffixes = {
            "123", "2025", "01", "12", "99", "00", "21", "22", "23", "24", "25"
        };

        // 특수 문자 (선택적)
        private static readonly char[] SpecialChars = { '!', '@', '#', '$', '%', '&', '*' };

        private static readonly Random random = new Random();

        /// <summary>
        /// 기본 설정으로 비밀번호 생성 (12자리, 첫글자 대문자, 특수문자 제외)
        /// </summary>
        /// <returns>생성된 비밀번호</returns>
        public static string GeneratePassword()
        {
            return GeneratePassword(12, true, true, false);
        }

        /// <summary>
        /// 사용자 정의 설정으로 비밀번호 생성
        /// 기본값: 첫글자 대문자 + 소문자 + 숫자, 특수문자 제외
        /// </summary>
        /// <param name="length">비밀번호 길이</param>
        /// <param name="includeNumbers">숫자 포함 여부</param>
        /// <param name="useBaseWords">기본 단어 사용 여부</param>
        /// <param name="includeSpecialChars">특수 문자 포함 여부 (기본값: false)</param>
        /// <returns>생성된 비밀번호</returns>
        public static string GeneratePassword(int length, bool includeNumbers = true, bool useBaseWords = true, bool includeSpecialChars = false)
        {
            if (length < 6)
                throw new ArgumentException("비밀번호는 최소 6자리 이상이어야 합니다.");

            // 기본 단어 선택
            string baseWord = useBaseWords ? GetRandomBaseWord() : GetRandomCommonWord();
            
            // 첫 글자를 대문자로, 나머지는 소문자로
            string password = char.ToUpper(baseWord[0]) + baseWord.Substring(1).ToLower();

            // 필요한 길이에 맞춰 조정
            if (includeNumbers)
            {
                password = AdjustPasswordWithNumbers(password, length, includeSpecialChars);
            }
            else
            {
                password = AdjustPasswordLength(password, length, includeSpecialChars);
            }

            return password;
        }

        /// <summary>
        /// 여러 개의 비밀번호 옵션 생성 (특수문자 제외)
        /// </summary>
        /// <param name="count">생성할 비밀번호 개수</param>
        /// <param name="length">비밀번호 길이</param>
        /// <returns>비밀번호 옵션 배열</returns>
        public static string[] GenerateMultiplePasswords(int count = 5, int length = 12)
        {
            var passwords = new List<string>();
            var usedPasswords = new HashSet<string>();

            while (passwords.Count < count && passwords.Count < 50) // 무한 루프 방지
            {
                // 다양한 조합으로 생성 (특수문자는 사용하지 않음)
                bool useBaseWords = passwords.Count < count / 2; // 절반은 기본 단어 사용
                bool includeSpecialChars = false; // 특수문자 사용하지 않음
                
                string password = GeneratePassword(length, true, useBaseWords, includeSpecialChars);
                
                if (!usedPasswords.Contains(password))
                {
                    passwords.Add(password);
                    usedPasswords.Add(password);
                }
            }

            return passwords.ToArray();
        }

        /// <summary>
        /// 코스트코 특화 비밀번호 생성 (특수문자 제외)
        /// 첫글자 대문자 + 소문자 + 숫자 조합
        /// </summary>
        /// <param name="includeYear">현재 연도 포함 여부</param>
        /// <param name="length">비밀번호 길이 (기본값: 12)</param>
        /// <returns>코스트코 특화 비밀번호</returns>
        public static string GenerateCostcoPassword(bool includeYear = true, int length = 12)
        {
            var costcoWords = new[] { "Costco", "Zhtmxmzh" };
            string baseWord = costcoWords[random.Next(costcoWords.Length)];
            
            string password = char.ToUpper(baseWord[0]) + baseWord.Substring(1).ToLower();
            
            if (includeYear)
            {
                string year = DateTime.Now.Year.ToString();
                password += year;
            }
            else
            {
                password += GetRandomNumberSuffix();
            }

            // 설정된 길이에 맞춰 조정
            while (password.Length < length)
            {
                password += random.Next(0, 10).ToString();
            }

            if (password.Length > length)
            {
                password = password.Substring(0, length);
            }

            return password;
        }

        /// <summary>
        /// 비밀번호 강도 검사
        /// </summary>
        /// <param name="password">검사할 비밀번호</param>
        /// <returns>강도 점수 (0-100)</returns>
        public static int CheckPasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return 0;

            int score = 0;

            // 길이 점수 (최대 25점)
            if (password.Length >= 8) score += 10;
            if (password.Length >= 12) score += 10;
            if (password.Length >= 16) score += 5;

            // 문자 종류 점수 (각각 최대 15점)
            if (password.Any(char.IsUpper)) score += 15;
            if (password.Any(char.IsLower)) score += 15;
            if (password.Any(char.IsDigit)) score += 15;
            if (password.Any(c => SpecialChars.Contains(c))) score += 15;

            // 복잡성 점수 (최대 20점)
            if (password.Length > 0 && !password.All(c => char.IsLetter(c))) score += 10;
            if (HasNoRepeatingPatterns(password)) score += 10;

            return Math.Min(score, 100);
        }

        /// <summary>
        /// 비밀번호 강도를 문자열로 반환
        /// </summary>
        /// <param name="password">검사할 비밀번호</param>
        /// <returns>강도 설명</returns>
        public static string GetPasswordStrengthDescription(string password)
        {
            int strength = CheckPasswordStrength(password);

            if (strength >= 80) return "매우 강함";
            if (strength >= 60) return "강함";
            if (strength >= 40) return "보통";
            if (strength >= 20) return "약함";
            return "매우 약함";
        }

        #region Private Helper Methods

        private static string GetRandomBaseWord()
        {
            return BaseWords[random.Next(BaseWords.Length)];
        }

        private static string GetRandomCommonWord()
        {
            return CommonWords[random.Next(CommonWords.Length)];
        }

        private static string GetRandomNumberSuffix()
        {
            return NumberSuffixes[random.Next(NumberSuffixes.Length)];
        }

        private static string AdjustPasswordWithNumbers(string basePassword, int targetLength, bool includeSpecialChars)
        {
            StringBuilder password = new StringBuilder(basePassword);

            // 숫자 추가
            string numbers = GetRandomNumberSuffix();
            password.Append(numbers);

            // 특수 문자는 사용자 요청에 따라 제외 (기본값: false)
            // 첫글자 대문자 + 소문자 + 숫자만 사용
            if (includeSpecialChars && password.Length < targetLength)
            {
                password.Append(SpecialChars[random.Next(SpecialChars.Length)]);
            }

            // 길이 조정 - 숫자만 추가
            while (password.Length < targetLength)
            {
                password.Append(random.Next(0, 10));
            }

            if (password.Length > targetLength)
            {
                password.Length = targetLength;
            }

            return password.ToString();
        }

        private static string AdjustPasswordLength(string basePassword, int targetLength, bool includeSpecialChars)
        {
            StringBuilder password = new StringBuilder(basePassword);

            // 특수 문자는 사용자 요청에 따라 제외 (기본값: false)
            if (includeSpecialChars && password.Length < targetLength)
            {
                password.Append(SpecialChars[random.Next(SpecialChars.Length)]);
            }

            // 추가 문자로 길이 맞추기 - 소문자와 숫자 조합
            string additionalChars = "abcdefghijklmnopqrstuvwxyz0123456789";
            while (password.Length < targetLength)
            {
                password.Append(additionalChars[random.Next(additionalChars.Length)]);
            }

            if (password.Length > targetLength)
            {
                password.Length = targetLength;
            }

            return password.ToString();
        }

        private static bool HasNoRepeatingPatterns(string password)
        {
            // 3자리 이상 반복 패턴 검사
            for (int i = 0; i <= password.Length - 6; i++)
            {
                string pattern = password.Substring(i, 3);
                if (password.IndexOf(pattern, i + 3) != -1)
                {
                    return false;
                }
            }

            // 연속된 문자 검사 (3개 이상)
            for (int i = 0; i <= password.Length - 3; i++)
            {
                if (password[i] == password[i + 1] && password[i + 1] == password[i + 2])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
