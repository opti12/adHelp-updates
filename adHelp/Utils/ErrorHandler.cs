using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using adHelp.Utils;

namespace adHelp.Utils
{
    /// <summary>
    /// 표준화된 에러 처리 클래스
    /// 모든 예외를 일관된 방식으로 처리하고 로깅
    /// </summary>
    public static class ErrorHandler
    {
        #region Error Levels
        
        public enum ErrorLevel
        {
            Info = 0,
            Warning = 1,
            Error = 2,
            Critical = 3
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 예외 처리 및 사용자에게 표시
        /// </summary>
        /// <param name="ex">예외 객체</param>
        /// <param name="title">에러 제목</param>
        /// <param name="parentForm">부모 폼 (선택사항)</param>
        /// <param name="showToUser">사용자에게 메시지박스 표시 여부</param>
        /// <returns>처리된 에러 정보</returns>
        public static ErrorInfo HandleException(Exception ex, string title, Form parentForm = null, bool showToUser = true)
        {
            if (ex == null)
                return null;
                
            try
            {
                // 에러 정보 생성
                var errorInfo = CreateErrorInfo(ex, title);
                
                // 로깅
                LogError(errorInfo);
                
                // 사용자에게 표시
                if (showToUser)
                {
                    ShowErrorToUser(errorInfo, parentForm);
                }
                
                return errorInfo;
            }
            catch (Exception logEx)
            {
                // 에러 처리 중 오류 발생 시 최소한의 처리
                SimpleLogger.Log($"ErrorHandler 내부 오류: {logEx.Message}");
                
                if (showToUser)
                {
                    MessageBox.Show($"예기치 않은 오류가 발생했습니다:\n{ex.Message}", 
                                  title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
                return new ErrorInfo { Title = title, UserMessage = ex.Message, Level = ErrorLevel.Critical };
            }
        }
        
        /// <summary>
        /// 정보성 메시지 표시
        /// </summary>
        public static void ShowInfo(string message, string title = "정보", Form parentForm = null)
        {
            SimpleLogger.Log($"INFO: {title} - {message}");
            MessageBox.Show(parentForm, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        /// <summary>
        /// 경고 메시지 표시
        /// </summary>
        public static void ShowWarning(string message, string title = "경고", Form parentForm = null)
        {
            SimpleLogger.Log($"WARNING: {title} - {message}");
            MessageBox.Show(parentForm, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        /// <summary>
        /// 확인 대화상자 표시
        /// </summary>
        public static DialogResult ShowConfirmation(string message, string title = "확인", Form parentForm = null)
        {
            SimpleLogger.Log($"CONFIRMATION: {title} - {message}");
            return MessageBox.Show(parentForm, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 예외에서 에러 정보 생성
        /// </summary>
        private static ErrorInfo CreateErrorInfo(Exception ex, string title)
        {
            var errorInfo = new ErrorInfo
            {
                Title = title,
                Exception = ex,
                Timestamp = DateTime.Now
            };
            
            // 예외 타입별 처리
            if (ex is UnauthorizedAccessException)
            {
                errorInfo.Level = ErrorLevel.Warning;
                errorInfo.UserMessage = "권한이 부족합니다. 관리자 권한으로 다시 시도해주세요.";
                errorInfo.TechnicalMessage = GetRootExceptionMessage(ex);
            }
            else if (ex is COMException comEx)
            {
                errorInfo.Level = ErrorLevel.Error;
                var comInfo = AnalyzeCOMException(comEx);
                errorInfo.UserMessage = comInfo.UserMessage;
                errorInfo.TechnicalMessage = comInfo.TechnicalMessage;
                errorInfo.ErrorCode = comEx.HResult;
            }
            else if (ex is System.DirectoryServices.AccountManagement.PasswordException)
            {
                errorInfo.Level = ErrorLevel.Warning;
                errorInfo.UserMessage = "비밀번호가 도메인 정책을 만족하지 않습니다. 복잡성 요구사항을 확인해주세요.";
                errorInfo.TechnicalMessage = GetRootExceptionMessage(ex);
            }
            else if (ex is InvalidOperationException && ex.Message.Contains("비밀번호가 성공적으로 변경되었습니다"))
            {
                // 성공 메시지인 경우
                errorInfo.Level = ErrorLevel.Info;
                errorInfo.UserMessage = ex.Message;
                errorInfo.TechnicalMessage = ex.Message;
            }
            else if (ex is ArgumentException || ex is ArgumentNullException)
            {
                errorInfo.Level = ErrorLevel.Warning;
                errorInfo.UserMessage = "입력된 정보가 올바르지 않습니다. 다시 확인해주세요.";
                errorInfo.TechnicalMessage = ex.Message;
            }
            else
            {
                // 기타 예외
                errorInfo.Level = ErrorLevel.Error;
                errorInfo.UserMessage = "예기치 않은 오류가 발생했습니다.";
                errorInfo.TechnicalMessage = GetRootExceptionMessage(ex);
            }
            
            return errorInfo;
        }
        
        /// <summary>
        /// COM 예외 분석
        /// </summary>
        private static (string UserMessage, string TechnicalMessage) AnalyzeCOMException(COMException ex)
        {
            string userMessage;
            string technicalMessage = $"HRESULT: 0x{ex.HResult:X8} ({GetErrorCodeName(ex.HResult)})";
            
            switch (ex.HResult)
            {
                case -2147024891: // E_ACCESSDENIED
                    userMessage = "접근이 거부되었습니다. 충분한 권한이 없습니다.";
                    break;
                case -2147023570: // ERROR_LOGON_FAILURE
                    userMessage = "로그인 실패: 사용자명 또는 비밀번호가 올바르지 않습니다.";
                    break;
                case -2147022651: // ERROR_PASSWORD_RESTRICTION
                    userMessage = "비밀번호가 도메인 정책을 만족하지 않습니다.";
                    break;
                case -2147023174: // RPC_S_SERVER_UNAVAILABLE
                    userMessage = "도메인 컨트롤러에 연결할 수 없습니다. 네트워크 연결을 확인해주세요.";
                    break;
                case -2147016656: // LDAP_NO_SUCH_OBJECT
                    userMessage = "지정된 사용자를 찾을 수 없습니다.";
                    break;
                case -2147016654: // LDAP_INSUFFICIENT_RIGHTS
                    userMessage = "작업을 수행할 권한이 부족합니다.";
                    break;
                default:
                    userMessage = "Active Directory 작업 중 오류가 발생했습니다.";
                    break;
            }
            
            return (userMessage, technicalMessage);
        }
        
        /// <summary>
        /// Inner Exception을 재귀적으로 분석하여 실제 오류 메시지 추출
        /// </summary>
        public static string GetRootExceptionMessage(Exception ex)
        {
            if (ex == null) return string.Empty;
            
            Exception innerMost = ex;
            while (innerMost.InnerException != null)
            {
                innerMost = innerMost.InnerException;
            }
            
            return innerMost.Message;
        }

        /// <summary>
        /// 오류 메시지에서 권한 관련 키워드 검사
        /// </summary>
        public static bool IsAccessDeniedError(string message)
        {
            if (string.IsNullOrEmpty(message)) return false;
            
            string[] accessDeniedKeywords = 
            {
                "access is denied", "access denied", "insufficient rights",
                "insufficient privileges", "privilege", "permission denied",
                "unauthorized", "not authorized", "elevation required",
                "0x80070005", "0x8007052E", "E_ACCESSDENIED"
            };
            
            string lowerMessage = message.ToLower();
            return accessDeniedKeywords.Any(keyword => lowerMessage.Contains(keyword));
        }
        
        /// <summary>
        /// HRESULT 코드에서 오류 이름 추출
        /// </summary>
        private static string GetErrorCodeName(int hresult)
        {
            switch (hresult)
            {
                case -2147024891: return "E_ACCESSDENIED";
                case -2147023570: return "ERROR_LOGON_FAILURE";
                case -2147022651: return "ERROR_PASSWORD_RESTRICTION";
                case -2147022694: return "ERROR_ACCOUNT_RESTRICTION";
                case -2147024809: return "E_INVALIDARG";
                case -2147024637: return "ERROR_ACCOUNT_DISABLED";
                case -2147024234: return "ERROR_INVALID_PASSWORD";
                case -2147023501: return "ERROR_PASSWORD_EXPIRED";
                case -2147023499: return "ERROR_ACCOUNT_LOCKED_OUT";
                case -2147023174: return "RPC_S_SERVER_UNAVAILABLE";
                case -2147023170: return "RPC_S_CALL_FAILED";
                case -2147024593: return "ERROR_NETWORK_UNREACHABLE";
                case -2147024332: return "ERROR_TIMEOUT";
                case -2147023541: return "ERROR_NO_SUCH_DOMAIN";
                case -2147023538: return "ERROR_DOMAIN_CONTROLLER_NOT_FOUND";
                case -2147016656: return "LDAP_NO_SUCH_OBJECT";
                case -2147016655: return "LDAP_INVALID_CREDENTIALS";
                case -2147016654: return "LDAP_INSUFFICIENT_RIGHTS";
                case -2147016652: return "LDAP_UNAVAILABLE";
                case -2147016623: return "LDAP_SERVER_DOWN";
                default: return "UNKNOWN_ERROR";
            }
        }
        
        /// <summary>
        /// 에러 로깅
        /// </summary>
        private static void LogError(ErrorInfo errorInfo)
        {
            string levelStr = errorInfo.Level.ToString().ToUpper();
            string logMessage = $"{levelStr}: {errorInfo.Title} - {errorInfo.UserMessage}";
            
            if (!string.IsNullOrEmpty(errorInfo.TechnicalMessage) && 
                errorInfo.TechnicalMessage != errorInfo.UserMessage)
            {
                logMessage += $" | Technical: {errorInfo.TechnicalMessage}";
            }
            
            if (errorInfo.ErrorCode.HasValue)
            {
                logMessage += $" | Code: 0x{errorInfo.ErrorCode.Value:X8}";
            }
            
            SimpleLogger.Log(logMessage);
        }
        
        /// <summary>
        /// 사용자에게 에러 표시
        /// </summary>
        private static void ShowErrorToUser(ErrorInfo errorInfo, Form parentForm)
        {
            MessageBoxIcon icon;
            switch (errorInfo.Level)
            {
                case ErrorLevel.Info:
                    icon = MessageBoxIcon.Information;
                    break;
                case ErrorLevel.Warning:
                    icon = MessageBoxIcon.Warning;
                    break;
                case ErrorLevel.Error:
                case ErrorLevel.Critical:
                default:
                    icon = MessageBoxIcon.Error;
                    break;
            }
            
            string message = errorInfo.UserMessage;
            
            // 기술적 정보와 오류 코드를 일반 사용자에게도 표시 (하단에)
            bool hasTechnicalInfo = !string.IsNullOrEmpty(errorInfo.TechnicalMessage) && 
                                   errorInfo.TechnicalMessage != errorInfo.UserMessage;
            bool hasErrorCode = errorInfo.ErrorCode.HasValue;
            
            if (hasTechnicalInfo || hasErrorCode)
            {
                message += "\n\n────────────────────────────";
                
                if (hasTechnicalInfo)
                {
                    message += $"\n기술적 정보: {errorInfo.TechnicalMessage}";
                }
                
                if (hasErrorCode)
                {
                    string errorCodeName = GetErrorCodeName(errorInfo.ErrorCode.Value);
                    message += $"\n오류 코드: 0x{errorInfo.ErrorCode.Value:X8} ({errorCodeName})";
                }
            }
            
            MessageBox.Show(parentForm, message, errorInfo.Title, MessageBoxButtons.OK, icon);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 에러 정보 클래스
    /// </summary>
    public class ErrorInfo
    {
        public string Title { get; set; }
        public string UserMessage { get; set; }
        public string TechnicalMessage { get; set; }
        public ErrorHandler.ErrorLevel Level { get; set; }
        public Exception Exception { get; set; }
        public DateTime Timestamp { get; set; }
        public int? ErrorCode { get; set; }
    }
}
