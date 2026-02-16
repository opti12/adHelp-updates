using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using adHelp.Models;
using adHelp.Services;
using adHelp.Utils;

namespace adHelp.Forms
{
    /// <summary>
    /// MainForm의 초기화 기능 (Part 5/5)
    /// 서비스 초기화, 이벤트 핸들러 설정, UI 초기 설정 등
    /// </summary>
    public partial class MainForm : Form
    {
        #region Private Methods - Initialization

        /// <summary>
        /// 서비스 비동기 초기화 - 현대적 패턴
        /// </summary>
        private async Task InitializeServicesAsync()
        {
            try
            {
                SimpleLogger.Log("MainForm: InitializeServicesAsync 시작");
                
                _adService = new ADService();
                _adServiceExt = new ADServiceExt();

                // AD 연결을 비동기로 처리
                var connectTasks = new Task<bool>[]
                {
                    Task.Run(() => _adService.Connect(_currentCredential)),
                    Task.Run(() => _adServiceExt.Connect(_currentCredential))
                };

                var results = await Task.WhenAll(connectTasks).ConfigureAwait(false);

                if (!results.All(r => r))
                {
                    throw new InvalidOperationException("Active Directory 연결 실패");
                }
                
                // UI 업데이트는 메인 스레드에서
                this.Invoke((Action)(() =>
                {
                    SimpleLogger.Log("MainForm: AD 서비스 연결 완료 - UpdateConnectionStatus 호출");
                    
                    // 연결 완료 후 상태 업데이트
                    UpdateConnectionStatus();
                    
                    // 연결 완료 후 윈도우 타이틀 업데이트
                    UpdateWindowTitle();
                }));
                
                SimpleLogger.Log("MainForm: InitializeServicesAsync 완료");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"MainForm: InitializeServicesAsync 오류: {ex.Message}");
                throw new InvalidOperationException($"서비스 초기화 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 이벤트 핸들러 설정
        /// </summary>
        private void SetupEventHandlers()
        {
            // 폼 이벤트
            this.KeyDown += MainForm_KeyDown;
            
            // 메뉴 이벤트 (직접 최상위 메뉴)
            this.exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            this.checkUpdateToolStripMenuItem.Click += checkUpdateToolStripMenuItem_Click;
            this.aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;

            // 버튼 이벤트
            this.buttonSearch.Click += buttonSearch_Click;
            this.buttonRefresh.Click += buttonRefresh_Click;
            this.buttonVerify.Click += buttonVerify_Click;
            this.buttonOpenDSA.Click += buttonOpenDSA_Click;
            this.buttonUnlock.Click += buttonUnlock_Click;
            this.buttonPasswordManager.Click += buttonPasswordManager_Click;
            this.buttonViewDetails.Click += buttonViewDetails_Click;
            this.buttonCopyGroups.Click += buttonCopyGroups_Click;

            // 텍스트박스 이벤트
            this.textBoxUserId.KeyDown += textBoxUserId_KeyDown;
            this.textBoxAccountSummary.Click += textBoxAccountSummary_Click;
            this.textBoxAccountSummary.Enter += textBoxAccountSummary_Enter;
            
            // StatusStrip 클릭 이벤트 (로그 콘솔창 열기)
            this.statusStrip.Click += StatusStrip_Click;
            this.toolStripStatusLabel.Click += StatusStrip_Click;
            
            // 그룹 목록 더블클릭 이벤트
            this.listBoxGroups.DoubleClick += listBoxGroups_DoubleClick;
        }

        /// <summary>
        /// UI 초기 설정
        /// </summary>
        private void SetupUI()
        {
            // 아이콘 설정
            try
            {
                this.Icon = Properties.Resources.ad192_icon;
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"MainForm: 아이콘 설정 오류: {ex.Message}");
            }
            
            // IME를 영문으로 고정
            IMEHelper.SetEnglishOnly(textBoxUserId);
            
            // 메뉴 설정 - Designer와 중복 제거
            // InitializeMenu(); // 제거 - Designer에서 이미 처리됨
            
            // 상태바 설정 - Designer에서 이미 처리됨
            // 검색 그룹박스 설정 - Designer에서 이미 처리됨  
            // 사용자 정보 그룹박스 설정 - Designer에서 이미 처리됨
            // 그룹 정보 그룹박스 설정 - Designer에서 이미 처리됨
        }

        #endregion
    }
}
