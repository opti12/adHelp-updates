using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace adHelp.Utils
{
    /// <summary>
    /// 메모리 기반 로거 (개선된 버전)
    /// StatusStrip 클릭 시 로그 콘솔창에서 표시
    /// 에러 레벨 지원 및 ErrorHandler와 연동
    /// </summary>
    public static class SimpleLogger
    {
        private static readonly List<LogEntry> _logEntries = new List<LogEntry>();
        private static readonly object _lock = new object();
        private const int MaxLogEntries = 1000; // 최대 로그 개수 제한

        #region Log Levels
        
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            Critical = 4
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// 로그 메시지 기록 (기존 호환성 유지)
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Log(string message)
        {
            Log(LogLevel.Info, message);
        }
        
        /// <summary>
        /// 레벨별 로그 메시지 기록
        /// </summary>
        /// <param name="level">로그 레벨</param>
        /// <param name="message">로그 메시지</param>
        public static void Log(LogLevel level, string message)
        {
            try
            {
                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Message = message
                };
                
                lock (_lock)
                {
                    _logEntries.Add(logEntry);
                    
                    // 최대 개수 초과 시 오래된 로그 제거
                    if (_logEntries.Count > MaxLogEntries)
                    {
                        _logEntries.RemoveAt(0);
                    }
                }
            }
            catch
            {
                // 로깅 실패 시 무시 (메인 기능에 영향 주지 않음)
            }
        }
        
        /// <summary>
        /// 디버그 로그
        /// </summary>
        public static void LogDebug(string message) => Log(LogLevel.Debug, message);
        
        /// <summary>
        /// 정보 로그
        /// </summary>
        public static void LogInfo(string message) => Log(LogLevel.Info, message);
        
        /// <summary>
        /// 경고 로그
        /// </summary>
        public static void LogWarning(string message) => Log(LogLevel.Warning, message);
        
        /// <summary>
        /// 에러 로그
        /// </summary>
        public static void LogError(string message) => Log(LogLevel.Error, message);
        
        /// <summary>
        /// 치명적 에러 로그
        /// </summary>
        public static void LogCritical(string message) => Log(LogLevel.Critical, message);

        /// <summary>
        /// 모든 로그 메시지 반환 (기존 호환성 유지)
        /// </summary>
        /// <returns>로그 메시지 배열</returns>
        public static string[] GetAllLogs()
        {
            lock (_lock)
            {
                return _logEntries.Select(FormatLogEntry).ToArray();
            }
        }
        
        /// <summary>
        /// 모든 로그 엔트리 반환
        /// </summary>
        /// <returns>로그 엔트리 배열</returns>
        public static LogEntry[] GetAllLogEntries()
        {
            lock (_lock)
            {
                return _logEntries.ToArray();
            }
        }
        
        /// <summary>
        /// 레벨별 로그 필터링
        /// </summary>
        /// <param name="minLevel">최소 로그 레벨</param>
        /// <returns>필터링된 로그 엔트리 배열</returns>
        public static LogEntry[] GetLogsByLevel(LogLevel minLevel)
        {
            lock (_lock)
            {
                return _logEntries.Where(entry => entry.Level >= minLevel).ToArray();
            }
        }

        /// <summary>
        /// 최근 N개 로그 메시지 반환 (기존 호환성 유지)
        /// </summary>
        /// <param name="count">가져올 로그 개수</param>
        /// <returns>최근 로그 메시지 배열</returns>
        public static string[] GetRecentLogs(int count)
        {
            lock (_lock)
            {
                return _logEntries
                    .Skip(Math.Max(0, _logEntries.Count - count))
                    .Select(FormatLogEntry)
                    .ToArray();
            }
        }

        /// <summary>
        /// 로그 개수 반환
        /// </summary>
        /// <returns>현재 저장된 로그 개수</returns>
        public static int GetLogCount()
        {
            lock (_lock)
            {
                return _logEntries.Count;
            }
        }
        
        /// <summary>
        /// 레벨별 로그 개수 반환
        /// </summary>
        /// <param name="level">로그 레벨</param>
        /// <returns>해당 레벨의 로그 개수</returns>
        public static int GetLogCountByLevel(LogLevel level)
        {
            lock (_lock)
            {
                return _logEntries.Count(entry => entry.Level == level);
            }
        }

        /// <summary>
        /// 로그 초기화
        /// </summary>
        public static void ClearLog()
        {
            lock (_lock)
            {
                _logEntries.Clear();
            }
        }

        /// <summary>
        /// 세션 시작 로그
        /// </summary>
        public static void LogSessionStart()
        {
            Log(LogLevel.Info, "========================================");
            Log(LogLevel.Info, $"adHelp 세션 시작 - 버전: {Assembly.GetExecutingAssembly().GetName().Version}");
            Log(LogLevel.Info, $"실행 경로: {Assembly.GetExecutingAssembly().Location}");
            Log(LogLevel.Info, $"로그 저장: 메모리 (최대 {MaxLogEntries}개)");
            Log(LogLevel.Info, "========================================");
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 로그 엔트리를 문자열로 포맷팅
        /// </summary>
        private static string FormatLogEntry(LogEntry entry)
        {
            string levelStr = entry.Level.ToString().ToUpper().PadRight(8);
            return $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {levelStr} {entry.Message}";
        }
        
        #endregion
    }
    
    /// <summary>
    /// 로그 엔트리 클래스
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public SimpleLogger.LogLevel Level { get; set; }
        public string Message { get; set; }
    }
}
