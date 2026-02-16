using System;
using System.Threading;
using System.Windows.Forms;
using adHelp.Forms;
using adHelp.Models;
using adHelp.Services;
using adHelp.Utils;

namespace adHelp
{
    /// <summary>
    /// 애플리케이션 진입점
    /// </summary>
    static class Program
    {
        /// <summary>
        /// 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 세션 시작 로그
            try
            {
                SimpleLogger.LogSessionStart();
            }
            catch
            {
                // 로깅 실패 시 무시
            }

            // 애플리케이션 예외 처리 설정 (반드시 첫 번째로 설정)
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Windows Forms 애플리케이션 초기화
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 설정 관리자 초기화
            try
            {
                ConfigManager.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"설정 초기화 중 오류가 발생했습니다:\n{ex.Message}\n\n기본 설정으로 계속 진행합니다.",
                    "설정 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            // 자동 업데이트 시스템 초기화
            try
            {
                AutoUpdateManager.Initialize();
                SimpleLogger.LogInfo("자동 업데이트 시스템 초기화 완료");
            }
            catch (Exception updateEx)
            {
                SimpleLogger.LogError($"자동 업데이트 초기화 실패 (앱 실행 계속): {updateEx.Message}");
                // 업데이트 실패는 앱 실행을 중단하지 않음
            }

            try
            {
                // 커맨드라인 인수 처리
                var args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    string arg = args[1].TrimStart('-', '/', '\\');
                    
                    // 도움말: --help, -help, /help, -?, /?, ?
                    if (arg.Equals("help", StringComparison.OrdinalIgnoreCase) || arg == "?")
                    {
                        ShowUsageHelp();
                        return;
                    }
                    
                    // 테스트 모드: --test, -test, /test
                    if (arg.Equals("test", StringComparison.OrdinalIgnoreCase))
                    {
                        SimpleLogger.LogInfo("테스트 모드로 실행");
                        var testCredential = new CredentialInfo()
                        {
                            Domain = Environment.UserDomainName,
                            Username = Environment.UserName,
                            IsAuthenticated = true
                        };
                        testCredential.SetPassword("test");
                        Application.Run(new MainForm(testCredential));
                        return;
                    }
                }

                // 로그인 프로세스 시작 및 메인 폼 생성
                var mainForm = ShowLoginAndCreateMainForm();
                
                if (mainForm != null)
                {
                    // 메인 애플리케이션 실행 - mainForm을 주 폼으로 설정
                    Application.Run(mainForm);
                }
                // else: 사용자가 로그인을 취소했거나 로그인 실패
            }
            catch (Exception ex)
            {
                ShowFatalError("애플리케이션 시작 실패", ex);
            }
        }

        /// <summary>
        /// 로그인 처리 및 메인 애플리케이션 시작
        /// </summary>
        /// <returns>메인 폼 인스턴스 (실패 시 null)</returns>
        private static MainForm ShowLoginAndCreateMainForm()
        {
            CredentialInfo credential = null;
            ADService adService = null;

            try
            {
                // 스플래시 또는 환영 메시지 (선택적)
                ShowWelcomeMessage();

                // 로그인 폼 표시
                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        credential = loginForm.AuthenticatedCredential;
                    }
                    else
                    {
                        return null; // 사용자가 로그인을 취소함
                    }
                }

                // 로그인 성공 시 메인 폼 생성
                if (credential != null)
                {
                    // 크리덴셜이 인증되었고 유효한지 확인
                    if (credential.IsAuthenticated && credential.IsValid())
                    {
                        // 메인 폼 생성 및 반환 (이미 인증된 크리덴셜 사용)
                        try
                        {
                            var mainForm = new MainForm(credential);
                            return mainForm;
                        }
                        catch (Exception mainFormEx)
                        {
                            MessageBox.Show($"MainForm 생성 중 오류:\n{mainFormEx.Message}", "MainForm 생성 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                    }
                    else if (!credential.IsAuthenticated)
                    {
                        MessageBox.Show(
                            $"크리덴셜이 인증되지 않았습니다.\n" +
                            $"오류 메시지: {credential.LastErrorMessage ?? "알 수 없는 오류"}",
                            "인증 실패",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    else if (!credential.IsValid())
                    {
                        MessageBox.Show(
                            "크리덴셜 정보가 유효하지 않습니다.\n" +
                            "도메인, 사용자명, 비밀번호를 확인해주세요.",
                            "유효성 검사 실패",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("크리덴셜 객체가 생성되지 않았습니다.", "크리덴셜 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ShowLoginAndCreateMainForm에서 예외 발생:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}", "로그인 처리 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // 리소스 정리
                adService?.Dispose();
                // 인증 실패 시에만 credential dispose
                if (credential != null && !credential.IsAuthenticated)
                {
                    credential.Dispose();
                }
            }

            return null;
        }

        /// <summary>
        /// 커맨드라인 사용법 도움말 표시
        /// </summary>
        private static void ShowUsageHelp()
        {
            string version = VersionHelper.GetFullProductTitle(false);
            string help = $"{version}\n" +
                          $"\n" +
                          $"Active Directory 사용자 관리 도구\n" +
                          $"사용자 검색, 계정 잠금 해제, 비밀번호 초기화 등의\n" +
                          $"AD 관리 작업을 GUI로 수행할 수 있습니다.\n" +
                          $"\n" +
                          $"사용법:\n" +
                          $"  adHelp.exe              일반 실행 (로그인 화면 표시)\n" +
                          $"  adHelp.exe --test       테스트 모드 (로그인 건너뛰기)\n" +
                          $"  adHelp.exe --help       이 도움말 표시\n" +
                          $"\n" +
                          $"옵션:\n" +
                          $"  --test, -test, /test    현재 Windows 계정으로 로그인을\n" +
                          $"                          건너뛰고 바로 메인 화면을 실행합니다.\n" +
                          $"                          (AD 연결은 실패할 수 있습니다)\n" +
                          $"\n" +
                          $"  --help, -help, /help    이 도움말을 표시합니다.\n" +
                          $"  -?, /?                  이 도움말을 표시합니다.";

            MessageBox.Show(help, $"{version} - 도움말", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 환영 메시지 표시 (선택적)
        /// </summary>
        private static void ShowWelcomeMessage()
        {
            // 간단한 환영 메시지나 스플래시 화면을 여기서 구현할 수 있음
            // 현재는 생략
        }

        /// <summary>
        /// UI 스레드 예외 처리
        /// </summary>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            try
            {
                ShowError("애플리케이션 오류", e.Exception);
            }
            catch
            {
                // 예외 처리 중에도 오류가 발생하면 애플리케이션 종료
                MessageBox.Show(
                    "심각한 오류가 발생했습니다. 애플리케이션을 종료합니다.",
                    "치명적 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
                
                Application.Exit();
            }
        }

        /// <summary>
        /// 비UI 스레드 예외 처리
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception ex)
                {
                    ShowFatalError("치명적 오류", ex);
                }
                else
                {
                    MessageBox.Show(
                        "알 수 없는 치명적 오류가 발생했습니다.",
                        "치명적 오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Stop);
                }
            }
            catch
            {
                // 마지막 시도
                try
                {
                    MessageBox.Show(
                        "복구할 수 없는 오류가 발생했습니다.",
                        "시스템 오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Stop);
                }
                catch
                {
                    // 더 이상 할 수 있는 것이 없음
                }
            }
            finally
            {
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 일반 오류 표시
        /// </summary>
        /// <param name="title">제목</param>
        /// <param name="exception">예외 객체</param>
        private static void ShowError(string title, Exception exception)
        {
            string message = $"오류가 발생했습니다:\n\n{exception.Message}";
            
            if (exception.InnerException != null)
            {
                message += $"\n\n내부 오류: {exception.InnerException.Message}";
            }

            // 개발/디버그 모드에서는 상세 정보 추가
            #if DEBUG
            message += $"\n\n상세 정보:\n{exception.StackTrace}";
            #endif

            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        /// <summary>
        /// 치명적 오류 표시
        /// </summary>
        /// <param name="title">제목</param>
        /// <param name="exception">예외 객체</param>
        private static void ShowFatalError(string title, Exception exception)
        {
            string message = $"치명적인 오류가 발생했습니다. 애플리케이션을 종료합니다.\n\n{exception.Message}";
            
            if (exception.InnerException != null)
            {
                message += $"\n\n내부 오류: {exception.InnerException.Message}";
            }

            // 로그 파일에 오류 정보 저장 시도
            try
            {
                LogErrorToFile(exception);
                message += $"\n\n오류 정보가 로그 파일에 저장되었습니다:\n{ConfigManager.GetLogFilePath()}";
            }
            catch
            {
                // 로그 저장 실패는 무시
            }

            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Stop);
        }

        /// <summary>
        /// 오류 정보를 로그 파일에 저장
        /// </summary>
        /// <param name="exception">예외 객체</param>
        private static void LogErrorToFile(Exception exception)
        {
            try
            {
                string logPath = ConfigManager.GetLogFilePath();
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FATAL ERROR\n" +
                                $"Message: {exception.Message}\n" +
                                $"Type: {exception.GetType().FullName}\n" +
                                $"StackTrace:\n{exception.StackTrace}\n";
                
                if (exception.InnerException != null)
                {
                    logEntry += $"InnerException: {exception.InnerException.Message}\n" +
                              $"InnerStackTrace:\n{exception.InnerException.StackTrace}\n";
                }
                
                logEntry += new string('-', 80) + "\n";

                // 파일에 추가 모드로 저장
                System.IO.File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // 로그 저장 실패 시 무시 (이미 오류 상황이므로)
            }
        }

        /// <summary>
        /// 애플리케이션 정보 표시 (디버그 목적)
        /// </summary>
        private static void ShowApplicationInfo()
        {
            #if DEBUG
            string info = "AD Helper Debug Information:\n\n" +
                         $"Version: {Application.ProductVersion}\n" +
                         $"Runtime: {Environment.Version}\n" +
                         $"OS: {Environment.OSVersion}\n" +
                         $"User: {Environment.UserName}\n" +
                         $"Machine: {Environment.MachineName}\n" +
                         $"Working Directory: {Environment.CurrentDirectory}\n\n" +
                         ConfigManager.GetSettingsInfo();

            MessageBox.Show(info, "Application Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            #endif
        }
    }
}
