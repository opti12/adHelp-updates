using System;
using System.Runtime.InteropServices;
using System.Security;
using System.ComponentModel;

namespace adHelp.Utils
{
    /// <summary>
    /// CreateProcessWithLogonW API를 사용한 프로세스 실행 유틸리티
    /// 비밀번호를 직접 전달하여 다른 사용자 자격 증명으로 프로세스 실행
    /// </summary>
    public static class ProcessWithCredentials
    {
        #region Win32 API 선언

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessWithLogonW(
            string userName,
            string domain,
            IntPtr password,
            LogonFlags logonFlags,
            string applicationName,
            string commandLine,
            CreationFlags creationFlags,
            IntPtr environment,
            string currentDirectory,
            ref STARTUPINFO startupInfo,
            out PROCESS_INFORMATION processInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        #endregion

        #region 구조체 및 열거형

        [Flags]
        public enum LogonFlags : uint
        {
            /// <summary>
            /// 네트워크 자격 증명만 사용 (runas /netonly와 유사)
            /// </summary>
            LOGON_NETCREDENTIALS_ONLY = 0x00000002,
            
            /// <summary>
            /// 사용자 프로필과 함께 로그온
            /// </summary>
            LOGON_WITH_PROFILE = 0x00000001
        }

        [Flags]
        public enum CreationFlags : uint
        {
            NONE = 0x00000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NO_WINDOW = 0x08000000,
            NORMAL_PRIORITY_CLASS = 0x00000020
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 지정된 자격 증명으로 프로세스 실행
        /// </summary>
        /// <param name="userName">사용자명</param>
        /// <param name="domain">도메인</param>
        /// <param name="password">비밀번호 (SecureString)</param>
        /// <param name="applicationName">실행할 애플리케이션</param>
        /// <param name="commandLine">명령줄 인수</param>
        /// <param name="useNetworkCredentialsOnly">네트워크 자격 증명만 사용할지 여부</param>
        /// <returns>프로세스 실행 성공 여부</returns>
        public static bool StartProcessWithCredentials(
            string userName,
            string domain,
            SecureString password,
            string applicationName,
            string commandLine,
            bool useNetworkCredentialsOnly = true)
        {
            IntPtr passwordPtr = IntPtr.Zero;
            PROCESS_INFORMATION processInfo = new PROCESS_INFORMATION();

            try
            {
                // SecureString을 IntPtr로 변환
                passwordPtr = Marshal.SecureStringToGlobalAllocUnicode(password);

                // STARTUPINFO 초기화
                STARTUPINFO startupInfo = new STARTUPINFO();
                startupInfo.cb = (uint)Marshal.SizeOf(startupInfo);

                // LogonFlags 설정
                LogonFlags logonFlags = useNetworkCredentialsOnly 
                    ? LogonFlags.LOGON_NETCREDENTIALS_ONLY 
                    : LogonFlags.LOGON_WITH_PROFILE;

                // CreationFlags 설정
                CreationFlags creationFlags = CreationFlags.NORMAL_PRIORITY_CLASS;

                // 프로세스 생성
                bool success = CreateProcessWithLogonW(
                    userName,
                    domain,
                    passwordPtr,
                    logonFlags,
                    applicationName,
                    commandLine,
                    creationFlags,
                    IntPtr.Zero,      // environment
                    null,             // currentDirectory
                    ref startupInfo,
                    out processInfo);

                if (!success)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"CreateProcessWithLogonW failed with error code: {errorCode}");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to start process with credentials: {ex.Message}", ex);
            }
            finally
            {
                // 메모리 정리
                if (passwordPtr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(passwordPtr);
                }

                // 프로세스 핸들 정리
                if (processInfo.hProcess != IntPtr.Zero)
                {
                    CloseHandle(processInfo.hProcess);
                }
                if (processInfo.hThread != IntPtr.Zero)
                {
                    CloseHandle(processInfo.hThread);
                }
            }
        }

        /// <summary>
        /// DSA.msc를 지정된 자격 증명으로 실행
        /// </summary>
        /// <param name="userName">사용자명</param>
        /// <param name="domain">도메인</param>
        /// <param name="password">비밀번호</param>
        /// <returns>실행 성공 여부</returns>
        public static bool StartDsaMscWithCredentials(string userName, string domain, SecureString password)
        {
            try
            {
                return StartProcessWithCredentials(
                    userName,
                    domain,
                    password,
                    "mmc.exe",
                    "mmc.exe dsa.msc",
                    useNetworkCredentialsOnly: true); // runas /netonly와 유사
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to start DSA.msc: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 지정된 .msc 파일을 지정된 자격 증명으로 실행
        /// </summary>
        /// <param name="userName">사용자명</param>
        /// <param name="domain">도메인</param>
        /// <param name="password">비밀번호</param>
        /// <param name="mscPath">.msc 파일 경로</param>
        /// <returns>실행 성공 여부</returns>
        public static bool StartMscWithCredentials(string userName, string domain, SecureString password, string mscPath)
        {
            try
            {
                return StartProcessWithCredentials(
                    userName,
                    domain,
                    password,
                    "mmc.exe",
                    $"mmc.exe \"{mscPath}\"",
                    useNetworkCredentialsOnly: true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to start MSC file: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
