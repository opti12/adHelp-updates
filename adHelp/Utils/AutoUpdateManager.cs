using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using AutoUpdaterDotNET;

namespace adHelp.Utils
{
    /// <summary>
    /// AutoUpdater.NET을 사용한 자동 업데이트 관리자
    /// GitHub Releases를 통한 업데이트 기능 제공
    /// </summary>
    public static class AutoUpdateManager
    {
        #region 상수 및 설정

        /// <summary>
        /// GitHub Repository의 업데이트 XML 파일 URL
        /// 실제 GitHub Repository URL로 변경 필요
        /// </summary>
        // private const string UPDATE_XML_URL = "file:///D:/repos/adHelp/UpdateInfo.xml"; // 로컬 테스트용
        private const string UPDATE_XML_URL = "https://opti12.github.io/adHelp-updates/UpdateInfo.xml"; // 원본 URL
                
        /// <summary>
        /// 업데이트 체크 주기 (분)
        /// </summary>
        private const int UPDATE_CHECK_INTERVAL_MINUTES = 60;

        /// <summary>
        /// 강제 업데이트 여부 (기본값: false)
        /// </summary>
        private const bool MANDATORY_UPDATE = false;
        
        /// <summary>
        /// AutoUpdater.NET 1.9.2 호환성을 위한 어셈블리 참조
        /// </summary>
        private static Assembly _currentAssembly;

        #endregion

        #region 대체 업데이트 체크

        /// <summary>
        /// MissingFieldException을 방지하는 안전한 업데이트 체크
        /// </summary>
        private static void PerformSafeUpdateCheck()
        {
            try
            {
                SimpleLogger.LogInfo("안전한 업데이트 체크 시작");
                
                // 1단계: 수동 업데이트 체크로 우회 (가장 안전한 방법)
                try
                {
                    SimpleLogger.LogInfo("AutoUpdater.NET 완전 우회 - 수동 업데이트 체크 실행");
                    PerformManualUpdateCheck();
                    return;
                }
                catch (System.MissingFieldException directMfEx)
                {
                    SimpleLogger.LogError($"수동 체크에서도 MissingFieldException: {directMfEx.Message}");
                    SimpleLogger.LogInfo("모든 업데이트 체크 방법에서 MissingFieldException 발생 - 업데이트 기능 비활성화");
                    return; // 완전히 포기
                }
                catch (Exception directEx)
                {
                    SimpleLogger.LogError($"수동 업데이트 체크 실패: {directEx.Message}");
                    
                    // 2단계: AutoUpdater.NET 최소 설정으로 시도
                    try
                    {
                        SimpleLogger.LogInfo("최소 설정으로 AutoUpdater.NET 시도");
                        
                        // 모든 선택적 설정 비활성화
                        AutoUpdater.Synchronous = true;
                        AutoUpdater.ReportErrors = false;
                        
                        // 최소한의 설정만 사용하여 호출
                        AutoUpdater.Start(UPDATE_XML_URL);
                        SimpleLogger.LogInfo("최소 설정 AutoUpdater.Start() 성공");
                        return;
                    }
                    catch (System.MissingFieldException mfEx)
                    {
                        SimpleLogger.LogError($"MissingFieldException 재발생: {mfEx.Message}");
                        SimpleLogger.LogError($"MissingFieldException 위치: {mfEx.StackTrace}");
                        SimpleLogger.LogInfo("AutoUpdater.NET 완전 비활성화 - 더 이상 자동 업데이트 체크를 시도하지 않습니다.");
                        
                        // 사용자에게 수동 업데이트 안내 (수동 체크일 때만)
                        if (AutoUpdater.ReportErrors)
                        {
                            ShowUpdateError("AutoUpdater.NET 라이브러리 호환성 문제로 자동 업데이트를 사용할 수 없습니다.");
                        }
                        return;
                    }
                    catch (Exception generalEx)
                    {
                        SimpleLogger.LogError($"일반 업데이트 오류: {generalEx.Message}");
                        
                        // 3단계: 비동기 모드로 재시도
                        try
                        {
                            SimpleLogger.LogInfo("비동기 모드로 AutoUpdater.NET 재시도");
                            AutoUpdater.Synchronous = false;
                            AutoUpdater.Start(UPDATE_XML_URL);
                            SimpleLogger.LogInfo("비동기 모드로 AutoUpdater.Start() 성공");
                            return;
                        }
                        catch (System.MissingFieldException asyncMfEx)
                        {
                            SimpleLogger.LogError($"비동기 모드에서도 MissingFieldException: {asyncMfEx.Message}");
                            SimpleLogger.LogInfo("AutoUpdater.NET 완전 비활성화 - 라이브러리 호환성 문제");
                            return;
                        }
                        catch (Exception asyncEx)
                        {
                            SimpleLogger.LogError($"비동기 모드도 실패: {asyncEx.Message}");
                            
                            // 최종 단계: 모든 방법 실패 시 완전 포기
                            SimpleLogger.LogInfo("모든 AutoUpdater.NET 시도 실패 - 업데이트 기능 비활성화");
                            return;
                        }
                    }
                }
            }
            catch (System.MissingFieldException topMfEx)
            {
                SimpleLogger.LogError($"최상위 MissingFieldException: {topMfEx.Message}");
                SimpleLogger.LogError($"최상위 MissingFieldException 위치: {topMfEx.StackTrace}");
                SimpleLogger.LogInfo("AutoUpdater.NET MissingFieldException으로 인해 업데이트 기능을 비활성화합니다.");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"안전 업데이트 체크 전체 실패: {ex.Message}");
                SimpleLogger.LogError($"안전 체크 전체 실패 위치: {ex.StackTrace}");
                
                if (AutoUpdater.ReportErrors)
                {
                    ShowUpdateError($"업데이트 시스템 오류: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 업데이트 오류 메시지 표시
        /// </summary>
        private static void ShowUpdateError(string message)
        {
            try
            {
                MessageBox.Show(
                    $"{message}\n\n" +
                    $"수동으로 업데이트를 확인해주세요.\n" +
                    $"GitHub: https://github.com/opti12/adHelp-updates/releases",
                    "업데이트 시스템 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"오류 메시지 표시 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// AutoUpdater.NET 오류 시 수동 업데이트 체크 수행
        /// </summary>
        private static void PerformManualUpdateCheck()
        {
            try
            {
                SimpleLogger.LogInfo("수동 업데이트 체크 실행");
                
                // XML 파일 직접 다운로드 및 파싱
                string xmlContent;
                using (var client = new System.Net.WebClient())
                {
                    client.Headers.Add("User-Agent", "adHelp Manual Updater");
                    xmlContent = client.DownloadString(UPDATE_XML_URL);
                }
                
                // XML 파싱
                var xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(xmlContent);
                
                var versionNode = xmlDoc.SelectSingleNode("//version");
                var urlNode = xmlDoc.SelectSingleNode("//url");
                var changelogNode = xmlDoc.SelectSingleNode("//changelog");
                
                if (versionNode != null && urlNode != null)
                {
                    var availableVersion = new Version(versionNode.InnerText);
                    var currentVersion = new Version(GetCurrentVersion());
                    
                    SimpleLogger.LogInfo($"수동 체크 - 현재: {currentVersion}, 사용가능: {availableVersion}");
                    
                    if (availableVersion > currentVersion)
                    {
                        var downloadUrl = urlNode.InnerText;
                        var changelogUrl = changelogNode?.InnerText ?? "";
                        
                        // 업데이트 확인 다이얼로그 표시
                        var result = MessageBox.Show(
                            $"새로운 버전이 사용 가능합니다!\n\n" +
                            $"현재 버전: {currentVersion}\n" +
                            $"새 버전: {availableVersion}\n\n" +
                            $"지금 업데이트를 다운로드하고 설치하시겠습니까?",
                            "업데이트 가능",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);
                        
                        if (result == DialogResult.Yes)
                        {
                            // 직접 업데이트 다운로드 및 설치 실행
                            PerformDirectUpdate(downloadUrl, availableVersion.ToString());
                        }
                        else
                        {
                            SimpleLogger.LogInfo("사용자가 업데이트를 취소했습니다.");
                        }
                    }
                    else
                    {
                        // 수동 체크일 때만 메시지 표시 (자동 체크에서는 조용히 처리)
                        if (AutoUpdater.ReportErrors)
                        {
                            MessageBox.Show(
                                "현재 최신 버전을 사용하고 있습니다.",
                                "업데이트 확인",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            SimpleLogger.LogInfo("자동 체크 - 최신 버전 사용 중");
                        }
                    }
                }
                else
                {
                    throw new Exception("업데이트 정보를 파싱할 수 없습니다.");
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"수동 업데이트 체크 실패: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 직접 업데이트 다운로드 및 설치 수행
        /// </summary>
        /// <param name="downloadUrl">다운로드 URL</param>
        /// <param name="newVersion">새 버전</param>
        private static void PerformDirectUpdate(string downloadUrl, string newVersion)
        {
            try
            {
                SimpleLogger.LogInfo($"직접 업데이트 시작 - URL: {downloadUrl}");
                
                // 임시 다운로드 경로 설정
                var tempPath = Path.GetTempPath();
                var fileName = $"adHelp_v{newVersion}.exe";
                var downloadPath = Path.Combine(tempPath, fileName);
                
                SimpleLogger.LogInfo($"다운로드 경로: {downloadPath}");
                
                // 진행률 표시 폼 생성 (간단한 구현)
                var progressForm = CreateProgressForm();
                progressForm.Show();
                
                try
                {
                    // 파일 다운로드
                    using (var client = new System.Net.WebClient())
                    {
                        client.Headers.Add("User-Agent", "adHelp Direct Updater");
                        
                        // 진행률 이벤트 핸들러
                        client.DownloadProgressChanged += (sender, e) => {
                            try
                            {
                                if (progressForm != null && !progressForm.IsDisposed)
                                {
                                    progressForm.Invoke(new Action(() => {
                                        UpdateProgressForm(progressForm, e.ProgressPercentage, "다운로드 중...");
                                    }));
                                }
                            }
                            catch { }
                        };
                        
                        // 동기 다운로드 (간단한 구현)
                        client.DownloadFile(downloadUrl, downloadPath);
                    }
                    
                    progressForm?.Close();
                    
                    SimpleLogger.LogInfo("다운로드 완료");
                    
                    // 다운로드된 파일 확인
                    if (File.Exists(downloadPath))
                    {
                        var fileInfo = new FileInfo(downloadPath);
                        SimpleLogger.LogInfo($"다운로드된 파일 크기: {fileInfo.Length} bytes");
                        
                        // 업데이트 설치 확인
                        var installResult = MessageBox.Show(
                            $"업데이트 다운로드가 완료되었습니다.\n\n" +
                            $"파일 크기: {fileInfo.Length / 1024 / 1024:F1} MB\n\n" +
                            $"지금 설치하시겠습니까?\n" +
                            $"(현재 애플리케이션이 종료되고 새 버전이 설치됩니다)",
                            "업데이트 설치",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        
                        if (installResult == DialogResult.Yes)
                        {
                            // 업데이트 설치 실행
                            InstallUpdate(downloadPath);
                        }
                        else
                        {
                            SimpleLogger.LogInfo("사용자가 설치를 취소했습니다.");
                            MessageBox.Show(
                                $"업데이트 파일이 다운로드되었습니다:\n{downloadPath}\n\n나중에 수동으로 설치할 수 있습니다.",
                                "업데이트 준비 완료",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        throw new Exception("다운로드된 파일을 찾을 수 없습니다.");
                    }
                }
                catch (Exception downloadEx)
                {
                    progressForm?.Close();
                    throw new Exception($"다운로드 실패: {downloadEx.Message}", downloadEx);
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"직접 업데이트 실패: {ex.Message}");
                MessageBox.Show(
                    $"업데이트 다운로드 중 오류가 발생했습니다:\n\n{ex.Message}\n\n수동으로 업데이트를 다운로드해주세요.",
                    "업데이트 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 업데이트 설치 실행
        /// </summary>
        /// <param name="updateFilePath">업데이트 파일 경로</param>
        private static void InstallUpdate(string updateFilePath)
        {
            try
            {
                SimpleLogger.LogInfo($"업데이트 설치 시작: {updateFilePath}");
                
                // 설치 스크립트 생성 (배치 파일)
                var scriptPath = Path.Combine(Path.GetTempPath(), "adHelp_update.bat");
                var currentExePath = Assembly.GetExecutingAssembly().Location;
                var currentExeDir = Path.GetDirectoryName(currentExePath);
                var currentExeName = Path.GetFileName(currentExePath);
                var backupPath = Path.Combine(currentExeDir, $"{Path.GetFileNameWithoutExtension(currentExeName)}_backup.exe");
                
                var batchContent = $@"@echo off
echo adHelp 업데이트 설치 중...
ping 127.0.0.1 -n 3 > nul
echo 기존 파일 백업...
copy ""{currentExePath}"" ""{backupPath}""
echo 새 버전 설치...
copy ""{updateFilePath}"" ""{currentExePath}""
echo 임시 파일 정리...
del ""{updateFilePath}""
del ""{scriptPath}""
echo 업데이트 완료! 새 버전을 시작합니다.
start "" ""{currentExePath}""
";
                
                File.WriteAllText(scriptPath, batchContent, System.Text.Encoding.Default);
                
                SimpleLogger.LogInfo("설치 스크립트 생성 완료");
                
                // 배치 파일 실행
                var processStartInfo = new ProcessStartInfo()
                {
                    FileName = scriptPath,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                
                Process.Start(processStartInfo);
                
                SimpleLogger.LogInfo("업데이트 설치 스크립트 실행 - 애플리케이션 종료");
                
                // 현재 애플리케이션 종료
                Application.Exit();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"업데이트 설치 실패: {ex.Message}");
                MessageBox.Show(
                    $"업데이트 설치 중 오류가 발생했습니다:\n\n{ex.Message}\n\n수동으로 설치해주세요.",
                    "설치 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 진행률 표시 폼 생성
        /// </summary>
        /// <returns>진행률 폼</returns>
        private static Form CreateProgressForm()
        {
            var form = new Form()
            {
                Text = "업데이트 다운로드",
                Size = new System.Drawing.Size(400, 120),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                TopMost = true
            };
            
            var progressBar = new ProgressBar()
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(340, 23),
                Style = ProgressBarStyle.Continuous
            };
            
            var label = new Label()
            {
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(340, 23),
                Text = "다운로드 준비 중...",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            
            form.Controls.Add(progressBar);
            form.Controls.Add(label);
            
            // 폼에 컨트롤 참조 저장 (Tag 사용)
            form.Tag = new { ProgressBar = progressBar, Label = label };
            
            return form;
        }
        
        /// <summary>
        /// 진행률 폼 업데이트
        /// </summary>
        /// <param name="form">진행률 폼</param>
        /// <param name="percentage">진행률 (0-100)</param>
        /// <param name="message">상태 메시지</param>
        private static void UpdateProgressForm(Form form, int percentage, string message)
        {
            try
            {
                if (form?.Tag != null)
                {
                    dynamic controls = form.Tag;
                    if (controls.ProgressBar is ProgressBar progressBar)
                    {
                        progressBar.Value = Math.Min(percentage, 100);
                    }
                    if (controls.Label is Label label)
                    {
                        label.Text = $"{message} ({percentage}%)";
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"진행률 업데이트 실패: {ex.Message}");
            }
        }

        #endregion

        #region 초기화 및 시작

        /// <summary>
        /// 자동 업데이트 시스템 초기화
        /// 앱 시작 시 한 번 호출
        /// </summary>
        public static void Initialize()
        {
            try
            {
                SimpleLogger.LogInfo("자동 업데이트 시스템 초기화 시작");
                
                // 어셈블리 정보 유효성 체크
                ValidateAssemblyInfo();

                // AutoUpdater 기본 설정
                ConfigureAutoUpdater();

                // 로그인 이후로 지연 실행 - 더 긴 지연 시간 설정
                SimpleLogger.LogInfo("자동 업데이트 체크를 30초 후로 지연 예약");
                var initTimer = new System.Windows.Forms.Timer();
                initTimer.Interval = 30000; // 30초 후 업데이트 체크 (로그인 완료 후)
                initTimer.Tick += (sender, e) => {
                    initTimer.Stop();
                    initTimer.Dispose();
                    CheckForUpdatesAsync(); // 자동 체크 (조용히)
                };
                initTimer.Start();

                SimpleLogger.LogInfo("자동 업데이트 시스템 초기화 완료");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"자동 업데이트 초기화 실패: {ex.Message}");
                SimpleLogger.LogError($"초기화 오류 스택트레이스: {ex.StackTrace}");
                // 업데이트 실패는 앱 실행을 중단하지 않음
            }
        }
        
        /// <summary>
        /// 어셈블리 정보 유효성 체크
        /// </summary>
        private static void ValidateAssemblyInfo()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyName = assembly.GetName();
                
                SimpleLogger.LogInfo($"어셈블리 이름: {assemblyName.Name}");
                SimpleLogger.LogInfo($"어셈블리 버전: {assemblyName.Version}");
                SimpleLogger.LogInfo($"어셈블리 위치: {assembly.Location}");
                
                // 버전 정보 유효성 체크
                if (assemblyName.Version == null)
                {
                    SimpleLogger.LogWarning("어셈블리 버전 정보가 null입니다.");
                }
                
                // 어셈블리 속성 체크
                var attributes = assembly.GetCustomAttributes(false);
                SimpleLogger.LogInfo($"어셈블리 속성 수: {attributes.Length}");
                
                foreach (var attr in attributes)
                {
                    if (attr is AssemblyTitleAttribute titleAttr)
                    {
                        SimpleLogger.LogInfo($"어셈블리 제목: {titleAttr.Title}");
                    }
                    else if (attr is AssemblyVersionAttribute versionAttr)
                    {
                        SimpleLogger.LogInfo($"어셈블리 버전 속성: {versionAttr.Version}");
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"어셈블리 정보 체크 실패: {ex.Message}");
                SimpleLogger.LogError($"어셈블리 체크 오류 스택트레이스: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// AutoUpdater 기본 설정 구성
        /// </summary>
        private static void ConfigureAutoUpdater()
        {
            try
            {
                // 기본 설정
                AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
                AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;
                // ParseUpdateInfoEvent를 등록하면 기본 XML 파싱이 비활성화되므로 제거
                // AutoUpdater.ParseUpdateInfoEvent += AutoUpdater_ParseUpdateInfoEvent;

                // UI 설정
                AutoUpdater.AppTitle = "AD Helper";
                
                // 아이콘 설정 - 안전하게 처리
                try
                {
                    AutoUpdater.Icon = GetApplicationBitmap();
                }
                catch (Exception iconEx)
                {
                    SimpleLogger.LogWarning($"아이콘 설정 실패: {iconEx.Message}");
                }
                
                // 업데이트 동작 설정
                AutoUpdater.Mandatory = MANDATORY_UPDATE;
                AutoUpdater.UpdateMode = Mode.Normal;
                
                // 다운로드 및 설치 설정
                AutoUpdater.DownloadPath = Path.GetTempPath();
                
                // MissingFieldException 방지를 위한 안전한 ExecutablePath 설정
                SetExecutablePathSafely();
                
                // 1.9.2 새 기능: 더 나은 오류 처리
                AutoUpdater.RunUpdateAsAdmin = false; // 관리자 권한이 필요한 경우만 true
                
                // 자동 업데이트 조용히 처리 설정 - 가장 중요!
                AutoUpdater.ReportErrors = false; // 기본값을 false로 설정
                AutoUpdater.ShowRemindLaterButton = false; // "나중에 알림" 버튼 비활성화
                AutoUpdater.ShowSkipButton = false; // "건너뛰기" 버튼 비활성화
                
                SimpleLogger.LogInfo("자돐 업데이트 기본 설정: ReportErrors = false (조용히 처리)");
                
                // 1.9.2 호환성: 어셈블리 정보 명시적 설정
                try
                {
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var assemblyName = currentAssembly.GetName();
                    
                    // AutoUpdater에서 사용할 어셈블리 정보 명시적 설정
                    if (assemblyName?.Version != null)
                    {
                        // 내부적으로 사용할 수 있도록 정적 참조 보관
                        _currentAssembly = currentAssembly;
                        SimpleLogger.LogInfo($"어셈블리 참조 설정 완료: {assemblyName.Name} v{assemblyName.Version}");
                    }
                }
                catch (Exception assemblyEx)
                {
                    SimpleLogger.LogError($"어셈블리 정보 설정 실패: {assemblyEx.Message}");
                }
                
                // 언어 설정 (한국어)
                SetKoreanMessages();

                // 주기적 업데이트 체크 설정 - 조용한 모드로 기본 설정
                AutoUpdater.Mandatory = false;
                
                // 1.9.2 새 기능: 더 정확한 타이머 설정
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = UPDATE_CHECK_INTERVAL_MINUTES * 60 * 1000; // 분을 밀리초로 변환
                timer.Tick += (sender, e) => {
                    // 주기적 체크도 조용히 수행
                    AutoUpdater.ReportErrors = false;
                    CheckForUpdatesAsync();
                };
                timer.Start();

                SimpleLogger.LogInfo($"AutoUpdater 설정 완료 - 버전: 1.9.2, 체크 주기: {UPDATE_CHECK_INTERVAL_MINUTES}분, 기본 모드: 조용히");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"AutoUpdater 설정 중 오류 발생: {ex.Message}");
                SimpleLogger.LogError($"설정 오류 스택트레이스: {ex.StackTrace}");
                throw; // 설정 실패 시 상위로 전달
            }
        }

        /// <summary>
        /// 한국어 메시지 설정
        /// </summary>
        private static void SetKoreanMessages()
        {
            // AutoUpdater.NET의 기본 메시지를 한국어로 설정
            // 실제로는 리소스 파일을 통해 처리하는 것이 좋지만,
            // 간단한 구현을 위해 여기서는 기본 설정 사용
        }

        #endregion

        #region 안전한 설정 메서드

        /// <summary>
        /// MissingFieldException을 방지하는 안전한 ExecutablePath 설정
        /// </summary>
        private static void SetExecutablePathSafely()
        {
            try
            {
                // 1단계: AutoUpdater.NET 1.9.2에서 ExecutablePath 설정을 건너뛰기
                // MissingFieldException의 주요 원인이므로 설정하지 않음
                SimpleLogger.LogInfo("ExecutablePath 설정을 건너뛰어 MissingFieldException 방지");
                
                // 2단계: 대신 어셈블리 정보를 명시적으로 설정
                var currentAssembly = Assembly.GetExecutingAssembly();
                _currentAssembly = currentAssembly;
                
                var assemblyLocation = currentAssembly.Location;
                SimpleLogger.LogInfo($"어셈블리 위치 확인: {assemblyLocation}");
                
                // 3단계: 프로세스 정보 로깅 (디버깅용)
                try
                {
                    var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                    var processPath = currentProcess.MainModule?.FileName;
                    SimpleLogger.LogInfo($"현재 프로세스 경로: {processPath}");
                }
                catch (Exception processEx)
                {
                    SimpleLogger.LogWarning($"프로세스 정보 가져오기 실패: {processEx.Message}");
                }
                
                SimpleLogger.LogInfo("ExecutablePath 설정 없이 안전한 초기화 완료");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"안전한 ExecutablePath 설정 실패: {ex.Message}");
                // 설정 실패해도 계속 진행
            }
        }

        #endregion

        #region 업데이트 체크

        /// <summary>
        /// 수동 업데이트 체크 (사용자가 직접 요청)
        /// </summary>
        public static void CheckForUpdatesManually()
        {
            try
            {
                SimpleLogger.LogInfo("수동 업데이트 체크 시작");
                SimpleLogger.LogInfo($"업데이트 URL: {UPDATE_XML_URL}");
                
                // 수동 체크 시에는 결과를 항상 표시
                AutoUpdater.ReportErrors = true;
                SimpleLogger.LogInfo("수동 체크: ReportErrors = true 설정");
                
                // URL 접근 가능성 체크
                try
                {
                    using (var client = new System.Net.WebClient())
                    {
                        // WebClient 타임아웃 설정 (간접적으로)
                        client.Headers.Add("User-Agent", "adHelp AutoUpdater");
                        string testResponse = client.DownloadString(UPDATE_XML_URL);
                        SimpleLogger.LogInfo($"URL 접근 성공, 응답 크기: {testResponse.Length} 문자");
                        SimpleLogger.LogInfo($"XML 내용 미리보기: {testResponse.Substring(0, Math.Min(200, testResponse.Length))}");
                    }
                }
                catch (Exception urlEx)
                {
                    SimpleLogger.LogError($"URL 접근 실패: {urlEx.Message}");
                    MessageBox.Show(
                        $"업데이트 서버에 연결할 수 없습니다.\n\nURL: {UPDATE_XML_URL}\n오류: {urlEx.Message}\n\n네트워크 연결을 확인해주세요.",
                        "업데이트 서버 연결 실패",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                
                SimpleLogger.LogInfo("AutoUpdater.Start() 호출 시작");
                
                // MissingFieldException 해결을 위한 단계별 안전 체크
                PerformSafeUpdateCheck();
                
                SimpleLogger.LogInfo("AutoUpdater.Start() 호출 완료");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"수동 업데이트 체크 실패: {ex.Message}");
                SimpleLogger.LogError($"StackTrace: {ex.StackTrace}");
                MessageBox.Show(
                    $"업데이트 확인 중 오류가 발생했습니다.\n\n{ex.Message}\n\n자세한 오류 정보는 로그를 확인해주세요.",
                    "업데이트 확인 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 자동 업데이트 체크 (백그라운드, 조용히)
        /// </summary>
        private static void CheckForUpdatesAsync()
        {
            try
            {
                SimpleLogger.LogInfo("자동 업데이트 체크 시작");
                
                // 자동 체크는 조용히 수행 - 메시지 표시 안함
                AutoUpdater.ReportErrors = false;
                SimpleLogger.LogInfo("자동 체크: ReportErrors = false 설정 (조용히 수행)");
                
                // MissingFieldException 방지를 위한 안전한 호출
                PerformSafeUpdateCheck();
                
                SimpleLogger.LogInfo("자동 업데이트 체크 완료");
            }
            catch (System.MissingFieldException mfEx)
            {
                SimpleLogger.LogError($"MissingFieldException 발생 (자동 체크): {mfEx.Message}");
                SimpleLogger.LogInfo("AutoUpdater.NET MissingFieldException으로 인해 자동 업데이트 체크를 건너뜁니다.");
                // 자동 체크에서는 사용자에게 알리지 않음
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"자동 업데이트 체크 실패: {ex.Message}");
                // 자동 체크 실패는 사용자에게 알리지 않음
            }
        }

        #endregion

        #region 이벤트 핸들러

        /// <summary>
        /// 업데이트 정보 파싱 이벤트
        /// </summary>
        private static void AutoUpdater_ParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            try
            {
                SimpleLogger.LogInfo("업데이트 정보 파싱 시작");
                
                // 기본 XML 파싱 사용
                // 필요 시 커스텀 파싱 로직 구현 가능
                
                SimpleLogger.LogInfo($"업데이트 정보 파싱 완료");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"업데이트 정보 파싱 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 업데이트 체크 완료 이벤트
        /// </summary>
        private static void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            // 버전 정보 변수 맨 위에 선언
            string currentVersionStr = GetCurrentVersion();
            string availableVersionStr = "Unknown";
            
            try
            {
                SimpleLogger.LogInfo("AutoUpdater_CheckForUpdateEvent 시작");
                SimpleLogger.LogInfo($"args 객체 타입: {args?.GetType().FullName}");
                
                if (args == null)
                {
                    SimpleLogger.LogError("UpdateInfoEventArgs가 null입니다.");
                    return;
                }
                
                // args 객체의 모든 속성 정보 로깅
                try
                {
                    var properties = args.GetType().GetProperties();
                    SimpleLogger.LogInfo($"사용 가능한 속성 수: {properties.Length}");
                    foreach (var prop in properties)
                    {
                        try
                        {
                            var value = prop.GetValue(args);
                            SimpleLogger.LogInfo($"속성: {prop.Name} = {value ?? "null"}");
                        }
                        catch (Exception propEx)
                        {
                            SimpleLogger.LogError($"속성 {prop.Name} 접근 오류: {propEx.Message}");
                        }
                    }
                }
                catch (Exception reflectionEx)
                {
                    SimpleLogger.LogError($"리플렉션 오류: {reflectionEx.Message}");
                }

                if (args.Error != null)
                {
                    SimpleLogger.LogError($"업데이트 체크 오류: {args.Error.Message}");
                    SimpleLogger.LogError($"오류 스택트레이스: {args.Error.StackTrace}");
                    
                    // 수동 체크일 때만 오류 표시
                    if (AutoUpdater.ReportErrors)
                    {
                        MessageBox.Show(
                            $"업데이트 확인 중 오류가 발생했습니다.\n\n오류 내용: {args.Error.Message}\n\n자세한 내용은 로그를 확인해주세요.",
                            "업데이트 확인 오류",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                    return;
                }

                // IsUpdateAvailable 속성 안전하게 접근
                bool isUpdateAvailable = false;
                try
                {
                    // AutoUpdater.NET 1.9.2에서 IsUpdateAvailable 속성 확인
                    var isUpdateAvailableProp = args.GetType().GetProperty("IsUpdateAvailable");
                    if (isUpdateAvailableProp != null)
                    {
                        var value = isUpdateAvailableProp.GetValue(args);
                        if (value is bool boolValue)
                        {
                            isUpdateAvailable = boolValue;
                            SimpleLogger.LogInfo($"IsUpdateAvailable: {isUpdateAvailable}");
                        }
                        else
                        {
                            SimpleLogger.LogWarning($"IsUpdateAvailable 값이 bool 타입이 아닙니다: {value?.GetType().Name}");
                        }
                    }
                    else
                    {
                        SimpleLogger.LogWarning("IsUpdateAvailable 속성을 찾을 수 없습니다.");
                        
                        // 대체 방법: 버전 비교로 업데이트 가능 여부 판단
                        if (availableVersionStr != "Unknown" && availableVersionStr != "버전 정보 확인 불가")
                        {
                            try
                            {
                                var currentVer = new Version(currentVersionStr);
                                var availableVer = new Version(availableVersionStr);
                                isUpdateAvailable = availableVer > currentVer;
                                SimpleLogger.LogInfo($"버전 비교 결과 - 업데이트 가능: {isUpdateAvailable} (현재: {currentVer}, 사용가능: {availableVer})");
                            }
                            catch (Exception verCompareEx)
                            {
                                SimpleLogger.LogError($"버전 비교 실패: {verCompareEx.Message}");
                                isUpdateAvailable = false;
                            }
                        }
                    }
                }
                catch (Exception propEx)
                {
                    SimpleLogger.LogError($"IsUpdateAvailable 속성 접근 오류: {propEx.Message}");
                    isUpdateAvailable = false;
                }

                if (isUpdateAvailable)
                {
                    // 버전 정보 업데이트 - 이미 선언된 변수 사용
                    try
                    {
                        SimpleLogger.LogInfo($"현재 설치된 버전: {currentVersionStr}");
                        
                        // AutoUpdater.NET 1.9.2에서 사용 가능한 속성들
                        var availableVersionProp = args.GetType().GetProperty("CurrentVersion");
                        if (availableVersionProp != null)
                        {
                            var versionValue = availableVersionProp.GetValue(args);
                            if (versionValue != null)
                            {
                                availableVersionStr = versionValue.ToString();
                                SimpleLogger.LogInfo($"사용 가능한 버전: {availableVersionStr}");
                            }
                        }
                        
                        // 대체 속성 시도
                        if (availableVersionStr == "Unknown")
                        {
                            // InstalledVersion 속성 시도 (1.9.2에서 새로 추가된 속성)
                            var installedVersionProp = args.GetType().GetProperty("InstalledVersion");
                            if (installedVersionProp != null)
                            {
                                var installedValue = installedVersionProp.GetValue(args);
                                SimpleLogger.LogInfo($"설치된 버전 (InstalledVersion): {installedValue}");
                            }
                            
                            // DownloadURL에서 버전 정보 추출 시도
                            var downloadUrlProp = args.GetType().GetProperty("DownloadURL");
                            if (downloadUrlProp != null)
                            {
                                var downloadUrl = downloadUrlProp.GetValue(args)?.ToString();
                                if (!string.IsNullOrEmpty(downloadUrl))
                                {
                                    SimpleLogger.LogInfo($"다운로드 URL: {downloadUrl}");
                                    // URL에서 버전 추출 시도 (v1.3.0 같은 패턴)
                                    var versionMatch = System.Text.RegularExpressions.Regex.Match(downloadUrl, @"v?(\d+\.\d+\.\d+(?:\.\d+)?)");
                                    if (versionMatch.Success)
                                    {
                                        availableVersionStr = versionMatch.Groups[1].Value;
                                        SimpleLogger.LogInfo($"URL에서 추출한 버전: {availableVersionStr}");
                                    }
                                }
                            }
                        }
                        
                        if (availableVersionStr == "Unknown")
                        {
                            SimpleLogger.LogWarning("업데이트 버전 정보를 가져올 수 없습니다.");
                            availableVersionStr = "버전 정보 확인 불가";
                        }
                    }
                    catch (Exception versionEx)
                    {
                        SimpleLogger.LogError($"버전 정보 가져오기 실패: {versionEx.Message}");
                        SimpleLogger.LogError($"버전 오류 스택트레이스: {versionEx.StackTrace}");
                    }
                    
                    SimpleLogger.LogInfo($"업데이트 발견 - 현재: {currentVersionStr}, 사용가능: {availableVersionStr}");
                    
                    // 업데이트 다이얼로그는 AutoUpdater.NET에서 자동으로 표시
                    SimpleLogger.LogInfo("AutoUpdater.NET에서 업데이트 다이얼로그를 표시합니다.");
                }
                else
                {
                    SimpleLogger.LogInfo("최신 버전 사용 중");
                    
                    // 수동 체크일 때만 결과 표시 - 자동 체크에서는 메시지 표시 안함
                    if (AutoUpdater.ReportErrors)
                    {
                        SimpleLogger.LogInfo("수동 업데이트 체크 - '최신 버전 사용 중' 메시지 표시");
                        MessageBox.Show(
                            "현재 최신 버전을 사용하고 있습니다.",
                            "업데이트 확인",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        SimpleLogger.LogInfo("자동 업데이트 체크 - 최신 버전이므로 메시지 표시 안함");
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"업데이트 체크 이벤트 처리 실패: {ex.Message}");
                SimpleLogger.LogError($"오류 스택트레이스: {ex.StackTrace}");
                SimpleLogger.LogError($"오류 타입: {ex.GetType().FullName}");
                if (ex.InnerException != null)
                {
                    SimpleLogger.LogError($"내부 예외: {ex.InnerException.Message}");
                    SimpleLogger.LogError($"내부 예외 스택트레이스: {ex.InnerException.StackTrace}");
                }
                
                // 전체 예외 처리 - 사용자에게 알림
                if (AutoUpdater.ReportErrors)
                {
                    MessageBox.Show(
                        $"업데이트 체크 처리 중 예상치 못한 오류가 발생했습니다.\n\n" +
                        $"오류 유형: {ex.GetType().Name}\n" +
                        $"오류 메시지: {ex.Message}\n\n" +
                        $"자세한 정보는 로그 콘솔을 확인해주세요.",
                        "업데이트 오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 애플리케이션 종료 이벤트 (업데이트 설치를 위한)
        /// </summary>
        private static void AutoUpdater_ApplicationExitEvent()
        {
            try
            {
                SimpleLogger.LogInfo("업데이트를 위한 애플리케이션 종료");
                
                // 현재 애플리케이션 종료
                Application.Exit();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"애플리케이션 종료 실패: {ex.Message}");
            }
        }

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 애플리케이션 비트맵 가져오기
        /// </summary>
        /// <returns>애플리케이션 비트맵</returns>
        private static System.Drawing.Bitmap GetApplicationBitmap()
        {
            try
            {
                // 실행 파일에서 아이콘 추출 후 비트맵으로 변환
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath))
                {
                    return icon?.ToBitmap();
                }
            }
            catch
            {
                // 기본 비트맵 사용
                return null;
            }
        }

        /// <summary>
        /// 애플리케이션 아이콘 가져오기 (레거시 메서드)
        /// </summary>
        /// <returns>애플리케이션 아이콘</returns>
        private static System.Drawing.Icon GetApplicationIcon()
        {
            try
            {
                // 실행 파일에서 아이콘 추출
                return System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                // 기본 아이콘 사용
                return null;
            }
        }

        /// <summary>
        /// 현재 애플리케이션 버전 정보 가져오기
        /// </summary>
        /// <returns>버전 정보 문자열</returns>
        public static string GetCurrentVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyName = assembly.GetName();
                
                if (assemblyName?.Version != null)
                {
                    var version = assemblyName.Version;
                    return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                }
                
                // 대체 방법: AssemblyInfo에서 직접 읽기
                var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (versionAttribute != null)
                {
                    return versionAttribute.InformationalVersion;
                }
                
                // 또 다른 대체 방법
                var fileVersionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
                if (fileVersionAttribute != null)
                {
                    return fileVersionAttribute.Version;
                }
                
                SimpleLogger.LogWarning("모든 버전 정보 가져오기 방법 실패, 기본값 사용");
                return "1.2.4.0"; // AssemblyInfo.cs에 지정된 버전
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"버전 정보 가져오기 실패: {ex.Message}");
                return "1.2.4.0"; // 기본값
            }
        }

        /// <summary>
        /// 업데이트 URL 설정 (동적 변경 가능)
        /// </summary>
        /// <param name="updateUrl">새로운 업데이트 URL</param>
        public static void SetUpdateUrl(string updateUrl)
        {
            if (!string.IsNullOrWhiteSpace(updateUrl))
            {
                SimpleLogger.LogInfo($"업데이트 URL 변경: {updateUrl}");
                // 실제 구현에서는 설정 파일에 저장하거나 변수로 관리
            }
        }

        /// <summary>
        /// 업데이트 기능 활성화/비활성화 상태 확인
        /// </summary>
        /// <returns>활성화 여부</returns>
        public static bool IsUpdateEnabled()
        {
            try
            {
                // ConfigManager는 static 클래스이므로 직접 사용
                return ConfigManager.GetSetting<bool>("AutoUpdate.Enabled");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogWarning($"업데이트 설정 확인 실패, 기본값 사용: {ex.Message}");
                return true; // 기본값: 활성화
            }
        }

        #endregion

        #region 디버그 및 테스트

        /// <summary>
        /// 업데이트 시스템 상태 정보 가져오기 (디버그용)
        /// </summary>
        /// <returns>상태 정보 문자열</returns>
        public static string GetUpdateSystemInfo()
        {
            try
            {
                return $"AutoUpdate System Info:\n" +
                       $"- Current Version: {GetCurrentVersion()}\n" +
                       $"- Update URL: {UPDATE_XML_URL}\n" +
                       $"- Check Interval: {UPDATE_CHECK_INTERVAL_MINUTES} minutes\n" +
                       $"- Mandatory: {MANDATORY_UPDATE}\n" +
                       $"- Enabled: {IsUpdateEnabled()}\n" +
                       $"- Download Path: {AutoUpdater.DownloadPath}";
            }
            catch (Exception ex)
            {
                return $"상태 정보 조회 실패: {ex.Message}";
            }
        }

        #endregion
    }
}
