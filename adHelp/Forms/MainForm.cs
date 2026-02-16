using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using adHelp.Models;
using adHelp.Services;
using adHelp.Utils;

namespace adHelp.Forms
{
    /// <summary>
    /// 메인 사용자 인터페이스 폼 (Part 1/5)
    /// 기본 이벤트 핸들러, 폼 생명주기, 초기화 기능
    /// .NET Framework 4.7.2 + Latest C# Features 적용
    /// </summary>
    public partial class MainForm : Form
    {
        #region Private Fields

        private ADService _adService;
        private ADServiceExt _adServiceExt;
        private CredentialInfo _currentCredential;
        private UserInfo _currentUser;
        private bool _isSearching;
        private bool _isProcessing;

        #endregion
        
        #region Constructor

        /// <summary>
        /// 생성자 - Primary Constructor 패턴 시뮬레이션
        /// </summary>
        /// <param name="credential">인증된 크리덴셜 정보</param>
        public MainForm(CredentialInfo credential)
        {
            _currentCredential = credential ?? throw new ArgumentNullException(nameof(credential));
            
            InitializeComponent();
            InitializeFormAsync();
        }

        /// <summary>
        /// 비동기 초기화 - 현대적 패턴
        /// </summary>
        private async void InitializeFormAsync()
        {
            try
            {
                // 윤도우 타이틀에 버전 정보 추가 (MainFormUI.cs에 정의됨)
                SetWindowTitle();
                
                // 다음 메서드들은 MainFormInitialization.cs에 정의됨
                await InitializeServicesAsync();
                SetupEventHandlers();
                SetupUI();
                
                // F5키 이벤트를 위한 KeyPreview 활성화
                this.KeyPreview = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"MainForm 생성자에서 예외 발생:\n{ex.Message}", "MainForm 생성자 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        #endregion

        #region Form Events

        /// <summary>
        /// 폼 로드 이벤트
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                SimpleLogger.Log("MainForm: MainForm_Load 이벤트 시작");
                
                // 사용자 검색 텍스트박스에 포커스
                textBoxUserId.Focus();
                
                // 상태바 업데이트
                UpdateStatusBar("준비");
                
                // 초기 상태 설정
                ClearUserInfo();
                UpdateButtonStates();
                
                // 연결 상태 다시 한 번 업데이트 (확실히 하기 위해)
                SimpleLogger.Log("MainForm: MainForm_Load에서 UpdateConnectionStatus 호출");
                UpdateConnectionStatus();
                
                SimpleLogger.Log("MainForm: MainForm_Load 완료");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"MainForm: MainForm_Load 오류: {ex.Message}");
                ShowError("폼 초기화 실패", ex);
            }
        }

        /// <summary>
        /// 폼 닫기 이벤트
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 리소스 정리
                _adService?.Dispose();
                _adServiceExt?.Dispose();
                
                // MainForm이 닫힐 때 크리덴셜도 정리
                _currentCredential?.Dispose();
                
                // LoginForm에서 남겨둔 ADService 인스턴스 정리
                // (인증 성공 시 LoginForm에서 dispose하지 않았으므로)
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"폼 닫기 중 오류: {ex.Message}");
            }
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// 사용자 검색 버튼 클릭 - 현대적 async 패턴
        /// </summary>
        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            await SearchUserAsync();
        }

        /// <summary>
        /// 폼 키다운 이벤트 - F5키 처리
        /// </summary>
        private async void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                
                // 현재 사용자가 있으면 새로고침, 없으면 무시
                if (_currentUser != null)
                {
                    await RefreshUserInfoAsync(_currentUser.UserId);
                }
            }
        }

        /// <summary>
        /// 사용자 ID 텍스트박스 Enter 키 처리
        /// </summary>
        private async void textBoxUserId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true; // 기본 처리 방지
                e.SuppressKeyPress = true; // 비프음 방지
                await SearchUserAsync();
            }
        }

        /// <summary>
        /// 새로고침 버튼 클릭
        /// </summary>
        private async void buttonRefresh_Click(object sender, EventArgs e)
        {
            if (_currentUser != null)
            {
                await RefreshUserInfoAsync(_currentUser.UserId);
            }
        }

        /// <summary>
        /// 검증 버튼 클릭
        /// </summary>
        private async void buttonVerify_Click(object sender, EventArgs e)
        {
            await VerifyUserCredentialsAsync();
        }

        /// <summary>
        /// 계정 잠금 해제 버튼 클릭
        /// </summary>
        private async void buttonUnlock_Click(object sender, EventArgs e)
        {
            await UnlockUserAccountAsync();
        }

        /// <summary>
        /// 비밀번호 관리 버튼 클릭
        /// </summary>
        private async void buttonPasswordManager_Click(object sender, EventArgs e)
        {
            await ShowPasswordManagerAsync();
        }

        /// <summary>
        /// 자세히 보기 버튼 클릭
        /// </summary>
        private async void buttonViewDetails_Click(object sender, EventArgs e)
        {
            await ShowUserDetailsAsync();
        }

        /// <summary>
        /// 종료 메뉴 클릭
        /// </summary>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 업데이트 확인 메뉴 클릭
        /// </summary>
        private void checkUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SimpleLogger.LogInfo("수동 업데이트 체크 시작");
                AutoUpdateManager.CheckForUpdatesManually();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"수동 업데이트 체크 실패: {ex.Message}");
                MessageBox.Show(
                    $"업데이트 확인 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "업데이트 확인 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 정보 메뉴 클릭
        /// </summary>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAboutDialog();
        }

        /// <summary>
        /// 계정 상태 요약 텍스트박스 클릭 - 전체 선택
        /// </summary>
        private void textBoxAccountSummary_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxAccountSummary.Text) && textBoxAccountSummary.Text != "-")
            {
                textBoxAccountSummary.SelectAll();
            }
        }

        /// <summary>
        /// 계정 상태 요약 텍스트박스 포커스 - 전체 선택
        /// </summary>
        private void textBoxAccountSummary_Enter(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxAccountSummary.Text) && textBoxAccountSummary.Text != "-")
            {
                textBoxAccountSummary.SelectAll();
            }
        }
        
        /// <summary>
        /// StatusStrip 클릭 이벤트 - 로그 콘솔창 열기
        /// </summary>
        private void StatusStrip_Click(object sender, EventArgs e)
        {
            try
            {
                SimpleLogger.Log("MainForm: StatusStrip 클릭 - 로그 콘솔창 열기");
                
                // 로그 콘솔창 생성 및 표시
                var logConsoleForm = new LogConsoleForm();
                logConsoleForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"MainForm: 로그 콘솔창 열기 오류: {ex.Message}");
                MessageBox.Show($"로그 콘솔창을 열 수 없습니다:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 그룹 목록 더블클릭 이벤트 - 그룹 상세 정보 표시
        /// </summary>
        private void listBoxGroups_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                // 선택된 그룹 확인
                if (listBoxGroups.SelectedItem == null)
                {
                    return;
                }

                string selectedGroup = listBoxGroups.SelectedItem.ToString();
                if (string.IsNullOrEmpty(selectedGroup))
                {
                    return;
                }

                SimpleLogger.Log($"MainForm: 그룹 선택 더블클릭: {selectedGroup}");
                
                // 그룹 상세 정보 폼 열기
                var groupDetailForm = new GroupDetailForm(_adService, selectedGroup);
                groupDetailForm.ShowDialog(this);
                
                SimpleLogger.Log($"MainForm: 그룹 상세 폼 닫힘");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"MainForm: 그룹 더블클릭 오류: {ex.Message}");
                ErrorHandler.HandleException(ex, "그룹 정보 표시 오류", this);
            }
        }

        #endregion

        #region Private Methods - UI Management

        /// <summary>
        /// 버튼 상태 업데이트
        /// </summary>
        private void UpdateButtonStates()
        {
            bool hasUser = _currentUser != null;
            bool isProcessing = _isSearching || _isProcessing;
            
            // 비밀번호 변경 권한 검사 (도메인 제한 제거)
            bool canChangePassword = hasUser && !isProcessing && CheckPasswordChangePermission();
            
            // 잠금해제 권한 검사 (도메인 제한 제거 - 실제 권한은 실행 시 검증)
            bool canUnlock = hasUser && !isProcessing && CheckUnlockPermission();

            // 검색 관련 버튼
            buttonSearch.Enabled = !isProcessing;
            buttonRefresh.Enabled = hasUser && !isProcessing;
            buttonVerify.Enabled = hasUser && !isProcessing;

            // 관리 기능 버튼
            buttonUnlock.Enabled = canUnlock && _currentUser?.IsLockedOut == true;
            buttonPasswordManager.Enabled = canChangePassword; // 도메인 제한 제거
            
            // 자세히 보기 버튼 (사용자가 있으면 활성화)
            buttonViewDetails.Enabled = hasUser && !isProcessing;

            // 텍스트박스
            textBoxUserId.Enabled = !isProcessing;
        }
        
        /// <summary>
        /// 비밀번호 변경 권한 검사
        /// </summary>
        /// <returns>권한 여부</returns>
        private bool CheckPasswordChangePermission()
        {
            try
            {
                // AD 연결 상태 확인
                if (_adServiceExt == null || !_adServiceExt.IsConnected)
                {
                    return false;
                }
                
                // 현재 사용자에 대한 비밀번호 변경 권한 테스트
                // 실제로 비밀번호 변경을 시도하지 않고 권한만 확인
                if (_currentUser != null)
                {
                    return _adServiceExt.HasPasswordChangePermission(_currentUser.UserId, _currentCredential);
                }
                
                return false;
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"CheckPasswordChangePermission 오류: {ex.Message}");
                // 오류 발생 시 권한이 없다고 가정 (보수적 접근)
                return false;
            }
        }
        
        /// <summary>
        /// 계정 잠금 해제 권한 검사
        /// </summary>
        /// <returns>권한 여부</returns>
        private bool CheckUnlockPermission()
        {
            try
            {
                // AD 연결 상태 확인
                if (_adServiceExt == null || !_adServiceExt.IsConnected)
                {
                    return false;
                }
                
                // 현재 사용자가 있는 경우 실제 권한 확인
                if (_currentUser != null)
                {
                    // 실제 계정 잠금 해제 권한 확인
                    return _adServiceExt.HasUnlockPermission(_currentUser.UserId, _currentCredential);
                }
                
                return false;
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"CheckUnlockPermission 오류: {ex.Message}");
                // 오류 발생 시 권한이 없다고 가정 (보수적 접근)
                return false;
            }
        }
        
        /// <summary>
        /// 사용자 정보 초기화
        /// </summary>
        private void ClearUserInfo()
        {
            _currentUser = null;

            // 모든 값 라벨 초기화
            labelUserIdValue.Text = "-";
            labelDisplayNameValue.Text = "-";
            labelEmailValue.Text = "-";
            labelDepartmentValue.Text = "-";
            labelTitleValue.Text = "-";
            labelAccountStatusValue.Text = "-";
            labelLastLogonValue.Text = "-";
            labelPasswordLastSetValue.Text = "-";
            labelPasswordExpiryValue.Text = "-";
            labelPasswordCanChangeValue.Text = "-";
            labelPasswordStatusValue.Text = "-";
            labelBadPasswordCountValue.Text = "-";
            labelLockoutTimeValue.Text = "-";
            
            // 추가 필드들 초기화
            labelLogonCountValue.Text = "-";
            labelAccountCreatedValue.Text = "-";
            labelAccountModifiedValue.Text = "-";
            labelUserAccountControlValue.Text = "-";
            
            // 계정 제어 설명 박스 초기화
            labelUserAccountControlDescription.Text = "-";

            // 계정 상태 요약 텍스트박스 초기화
            textBoxAccountSummary.Text = "-";

            // 색상 초기화
            labelAccountStatusValue.ForeColor = Color.Black;
            labelPasswordStatusValue.ForeColor = Color.Black;
            labelLockoutTimeValue.ForeColor = Color.Black;

            // 그룹 정보 초기화
            listBoxGroups.Items.Clear();
            labelGroupsCount.Text = "그룹 수: 0";

            UpdateButtonStates();
        }

        /// <summary>
        /// 사용자 정보 표시 - 현대적 패턴 매칭 사용
        /// </summary>
        private void DisplayUserInfo(UserInfo userInfo)
        {
            if (userInfo == null)
            {
                ClearUserInfo();
                return;
            }

            try
            {
                // 기본 정보 - null 조건부 연산자 사용
                labelUserIdValue.Text = userInfo.UserId;
                labelDisplayNameValue.Text = userInfo.DisplayName ?? "-";
                labelEmailValue.Text = userInfo.Email ?? "-";
                labelDepartmentValue.Text = userInfo.Department ?? "-";
                labelTitleValue.Text = userInfo.Title ?? "-";

                // 계정 상태 - 전통적 if-else 사용
                string statusText;
                Color statusColor;
                
                if (userInfo.IsEnabled)
                {
                    statusText = "활성화";
                    statusColor = Color.Green;
                }
                else
                {
                    statusText = "비활성화";
                    statusColor = Color.Red;
                }
                    
                labelAccountStatusValue.Text = statusText;
                labelAccountStatusValue.ForeColor = statusColor;

                // 시간 정보 - 현대적 메서드 호출
                labelLastLogonValue.Text = FormatDateTime(userInfo.LastLogon);
                labelPasswordLastSetValue.Text = FormatDateTime(userInfo.PasswordLastSet);
                labelPasswordExpiryValue.Text = FormatDateTime(userInfo.PasswordExpiryDate);
                labelPasswordCanChangeValue.Text = FormatDateTime(userInfo.PasswordCanChangeDate);

                // 비밀번호 상태 - record의 메서드 사용
                labelPasswordStatusValue.Text = userInfo.GetPasswordStatus();
                labelPasswordStatusValue.ForeColor = GetPasswordStatusColor(userInfo);

                // 로그인 실패 및 잠금 정보
                labelBadPasswordCountValue.Text = userInfo.BadPasswordCount.ToString();
                labelLockoutTimeValue.Text = FormatDateTime(userInfo.LockoutTime);
                labelLockoutTimeValue.ForeColor = userInfo.IsLockedOut ? Color.Red : Color.Black;
                
                // 추가 정보 필드들
                labelLogonCountValue.Text = userInfo.LogonCount.ToString();
                labelAccountCreatedValue.Text = FormatDateTime(userInfo.WhenCreated);
                labelAccountModifiedValue.Text = FormatDateTime(userInfo.WhenChanged);
                labelUserAccountControlValue.Text = $"0x{userInfo.UserAccountControl:X} ({userInfo.UserAccountControl})";
                
                // 계정 제어 설명 박스 업데이트
                var uacDescription = UserAccountControlHelper.GetUserAccountControlDetailedDescription(userInfo.UserAccountControl);
                labelUserAccountControlDescription.Text = uacDescription;
                
                // 계정 상태 요약 업데이트
                UpdateAccountSummary(userInfo);
                
            }
            catch (Exception ex)
            {
                ShowError("사용자 정보 표시 오류", ex);
            }
        }


        /// <summary>
        /// 계정 상태 요약 업데이트
        /// </summary>
        /// <param name="userInfo">사용자 정보</param>
        private void UpdateAccountSummary(UserInfo userInfo)
        {
            if (userInfo == null)
            {
                textBoxAccountSummary.Text = "-";
                return;
            }

            try
            {
                // 도메인명 추출 (짧은 형식으로 변환)
                string domainName = ExtractShortDomainName(_currentCredential?.Domain ?? "unknown");
                
                // 계정명
                string accountName = userInfo.UserId ?? "unknown";
                
                // 계정 상태 (IT 영어 표현)
                string accountStatus;
                if (!userInfo.IsEnabled)
                {
                    accountStatus = "disabled";
                }
                else if (userInfo.IsLockedOut)
                {
                    accountStatus = "locked";
                }
                else
                {
                    accountStatus = "active";
                }
                
                // 잠긴 시간 (잠김 상태인 경우만 표시)
                string lockoutTimeInfo = "";
                if (userInfo.IsLockedOut && userInfo.LockoutTime.HasValue && userInfo.LockoutTime.Value != DateTime.MinValue)
                {
                    string lockoutTime = userInfo.LockoutTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    lockoutTimeInfo = $" | Locked at: {lockoutTime}";
                }
                
                string summary = $"{domainName}\\{accountName} | Status: {accountStatus}{lockoutTimeInfo}";
                
                textBoxAccountSummary.Text = summary;
                
                SimpleLogger.Log($"MainForm: 계정 상태 요약 업데이트: {summary}");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"MainForm: UpdateAccountSummary 오류: {ex.Message}");
                textBoxAccountSummary.Text = "Error: Failed to generate summary";
            }
        }

        #endregion

        #region Private Methods - Utility Methods

        /// <summary>
        /// 상태바 업데이트
        /// </summary>
        private void UpdateStatusBar(string message)
        {
            try
            {
                toolStripStatusLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
                statusStrip.Refresh();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"상태바 업데이트 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// DSA.msc 열기 버튼 클릭
        /// </summary>
        private void buttonOpenDSA_Click(object sender, EventArgs e)
        {
            OpenDSAWithCurrentCredentials();
        }

        /// <summary>
        /// 그룹 복사 버튼 클릭
        /// </summary>
        private void buttonCopyGroups_Click(object sender, EventArgs e)
        {
            CopyGroupsToClipboard(sender, e);
        }

        /// <summary>
        /// 오류 표시
        /// </summary>
        private void ShowError(string title, Exception ex)
        {
            ErrorHandler.HandleException(ex, title, this, true);
            UpdateStatusBar($"오류: {ex.Message}");
        }

        #endregion
    }
}
