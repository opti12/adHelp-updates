using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using adHelp.Utils;

namespace adHelp.Forms
{
    /// <summary>
    /// 로그 콘솔창 폼
    /// 메모리에 저장된 로그 메시지들을 표시
    /// </summary>
    public partial class LogConsoleForm : Form
    {
        #region Private Fields

        private System.Windows.Forms.Timer _refreshTimer;
        private int _lastLogCount = 0;

        #endregion

        #region Components

        private TextBox textBoxLog;
        private Button buttonClear;
        private Button buttonRefresh;
        private Button buttonClose;
        private Label labelStatus;
        private CheckBox checkBoxAutoRefresh;

        #endregion

        #region Constructor

        public LogConsoleForm()
        {
            InitializeComponent();
            
            // 아이콘 설정
            try
            {
                this.Icon = Properties.Resources.ad192_icon;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogConsoleForm 아이콘 설정 오류: {ex.Message}");
            }
            
            InitializeTimer();
            
            // 폼 로드 후 로그를 로드하고 스크롤
            this.Load += LogConsoleForm_Load;
        }

        #endregion

        #region Initialization

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form 설정
            this.Text = "로그 콘솔 - AD Helper";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.Font = new Font("맑은 고딕", 9F);

            // 텍스트박스 (로그 표시)
            this.textBoxLog = new TextBox();
            this.textBoxLog.Multiline = true;
            this.textBoxLog.ScrollBars = ScrollBars.Both;
            this.textBoxLog.WordWrap = false;
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.Font = new Font("Consolas", 9F);
            this.textBoxLog.BackColor = Color.Black;
            this.textBoxLog.ForeColor = Color.LightGreen;
            this.textBoxLog.Location = new Point(12, 12);
            this.textBoxLog.Size = new Size(760, 480);
            this.textBoxLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.textBoxLog.TabIndex = 0;

            // 상태 레이블
            this.labelStatus = new Label();
            this.labelStatus.Text = "로그 개수: 0";
            this.labelStatus.Location = new Point(12, 505);
            this.labelStatus.Size = new Size(200, 20);
            this.labelStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.labelStatus.TabIndex = 1;

            // 자동 새로고침 체크박스
            this.checkBoxAutoRefresh = new CheckBox();
            this.checkBoxAutoRefresh.Text = "자동 새로고침 (5초)";
            this.checkBoxAutoRefresh.Location = new Point(12, 530);
            this.checkBoxAutoRefresh.Size = new Size(150, 20);
            this.checkBoxAutoRefresh.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.checkBoxAutoRefresh.Checked = true;
            this.checkBoxAutoRefresh.CheckedChanged += CheckBoxAutoRefresh_CheckedChanged;
            this.checkBoxAutoRefresh.TabIndex = 2;

            // 지우기 버튼
            this.buttonClear = new Button();
            this.buttonClear.Text = "로그 지우기";
            this.buttonClear.Location = new Point(617, 505);
            this.buttonClear.Size = new Size(80, 25);
            this.buttonClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.buttonClear.Click += ButtonClear_Click;
            this.buttonClear.TabIndex = 3;

            // 새로고침 버튼
            this.buttonRefresh = new Button();
            this.buttonRefresh.Text = "새로고침";
            this.buttonRefresh.Location = new Point(702, 505);
            this.buttonRefresh.Size = new Size(70, 25);
            this.buttonRefresh.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.buttonRefresh.Click += ButtonRefresh_Click;
            this.buttonRefresh.TabIndex = 4;

            // 닫기 버튼
            this.buttonClose = new Button();
            this.buttonClose.Text = "닫기";
            this.buttonClose.Location = new Point(702, 535);
            this.buttonClose.Size = new Size(70, 25);
            this.buttonClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.buttonClose.Click += ButtonClose_Click;
            this.buttonClose.TabIndex = 5;

            // 컨트롤 추가
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.checkBoxAutoRefresh);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonClose);

            this.ResumeLayout(false);
        }

        private void InitializeTimer()
        {
            _refreshTimer = new Timer();
            _refreshTimer.Interval = 5000; // 5초
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 폼 로드 이벤트 - 최초 로그 로드 및 스크롤
        /// </summary>
        private void LogConsoleForm_Load(object sender, EventArgs e)
        {
            LoadLogs();
            
            // 폼이 완전히 로드된 후 스크롤 수행
            this.BeginInvoke(new Action(() =>
            {
                ScrollToBottom();
            }));
        }

        private void CheckBoxAutoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            _refreshTimer.Enabled = checkBoxAutoRefresh.Checked;
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            // 로그 개수가 변경된 경우에만 새로고침
            int currentLogCount = SimpleLogger.GetLogCount();
            if (currentLogCount != _lastLogCount)
            {
                LoadLogs();
                
                // 자동 새로고침 시에도 맨 아래로 스크롤
                this.BeginInvoke(new Action(() =>
                {
                    ScrollToBottom();
                }));
            }
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "모든 로그를 지우시겠습니까?",
                "로그 지우기 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SimpleLogger.ClearLog();
                LoadLogs();
            }
        }

        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            LoadLogs();
            
            // 수동 새로고침 시에도 맨 아래로 스크롤
            this.BeginInvoke(new Action(() =>
            {
                ScrollToBottom();
            }));
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            base.OnFormClosing(e);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 로그 로드 및 표시 (개선된 버전)
        /// </summary>
        private void LoadLogs()
        {
            try
            {
                string[] logs = SimpleLogger.GetAllLogs();
                _lastLogCount = logs.Length;

                // 로그를 텍스트박스에 표시
                textBoxLog.Text = string.Join(Environment.NewLine, logs);

                // 상태 업데이트
                labelStatus.Text = $"로그 개수: {_lastLogCount}";

                // 제목에도 로그 개수 표시
                this.Text = $"로그 콘솔 - AD Helper ({_lastLogCount}개)";
                
                // 에러 레벨별 로그 개수 표시
                UpdateLogLevelStats();
            }
            catch (Exception ex)
            {
                labelStatus.Text = $"로그 로드 오류: {ex.Message}";
            }
        }
        
        /// <summary>
        /// 맨 아래로 스크롤 (개선된 버전)
        /// </summary>
        private void ScrollToBottom()
        {
            try
            {
                if (textBoxLog.Text.Length > 0)
                {
                    // 여러 방법을 사용하여 확실하게 스크롤
                    textBoxLog.SelectionStart = textBoxLog.Text.Length;
                    textBoxLog.SelectionLength = 0;
                    textBoxLog.ScrollToCaret();
                    
                    // 추가 방법: SendMessage 사용
                    NativeMethods.SendMessage(textBoxLog.Handle, NativeMethods.WM_VSCROLL, new IntPtr(NativeMethods.SB_BOTTOM), IntPtr.Zero);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ScrollToBottom 오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 에러 레벨별 로그 통계 업데이트
        /// </summary>
        private void UpdateLogLevelStats()
        {
            try
            {
                int errorCount = SimpleLogger.GetLogCountByLevel(SimpleLogger.LogLevel.Error);
                int warningCount = SimpleLogger.GetLogCountByLevel(SimpleLogger.LogLevel.Warning);
                int criticalCount = SimpleLogger.GetLogCountByLevel(SimpleLogger.LogLevel.Critical);
                
                string stats = $"로그 개수: {_lastLogCount}";
                
                if (criticalCount > 0)
                {
                    stats += $" | 치명적: {criticalCount}";
                }
                
                if (errorCount > 0)
                {
                    stats += $" | 에러: {errorCount}";
                }
                
                if (warningCount > 0)
                {
                    stats += $" | 경고: {warningCount}";
                }
                
                labelStatus.Text = stats;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateLogLevelStats 오류: {ex.Message}");
            }
        }

        #endregion
    }
    
    /// <summary>
    /// 네이티브 Windows API 메서드
    /// </summary>
    internal static class NativeMethods
    {
        public const int WM_VSCROLL = 0x115;
        public const int SB_BOTTOM = 7;
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
