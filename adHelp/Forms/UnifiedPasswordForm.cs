using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using adHelp.Utils;

namespace adHelp.Forms
{
    /// <summary>
    /// 통합 비밀번호 관리 폼 - 전문적이고 사무적인 디자인
    /// 비밀번호 직접 수정과 자동 생성 기능을 하나의 창에서 제공
    /// </summary>
    public partial class UnifiedPasswordForm : Form
    {
        #region Form Controls - 디자이너 호환성을 위해 상단에 배치

        private TextBox textBoxNewPassword;
        private TextBox textBoxConfirmPassword;
        private Button buttonCopyPassword;
        private Button buttonToggleVisibility;
        private Label labelPasswordStrength;
        private Label labelMatchStatus;
        
        private NumericUpDown numericUpDownLength;
        private Button buttonGenerateMultiple;
        private ListBox listBoxGeneratedPasswords;
        
        private CheckBox checkBoxForceChange;
        private Button buttonOK;
        private Button buttonCancel;
        
        // OU 정보 표시용 라벨 (디자이너에서 정의됨)
        // private Label ouPrefixLabel; // "조직 단위:" 고정 라벨 (디자이너에서 정의)
        // private Label ouPathLabel;   // 실제 OU 경로 라벨 (깜박임용, 디자이너에서 정의)
        
        // OU 경고 상태용 타이머
        private System.Windows.Forms.Timer ouWarningTimer;
        private bool ouWarningVisible = true;

        #endregion

        #region Private Fields
        
        private string _selectedPassword;
        private string _userId;
        private string _userDistinguishedName; // 사용자 DN 정보
        private Models.DomainPasswordPolicy _domainPolicy;
        
        #endregion

        #region Public Properties
        
        /// <summary>
        /// 선택된 비밀번호
        /// </summary>
        public string SelectedPassword => _selectedPassword;

        /// <summary>
        /// 다음 로그인 시 비밀번호 변경 강제 여부
        /// </summary>
        public bool ForceChangeAtNextLogon => checkBoxForceChange.Checked;

        #endregion

        #region Constructor

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="domainPolicy">도메인 비밀번호 정책 (선택사항)</param>
        public UnifiedPasswordForm(string userId, Models.DomainPasswordPolicy domainPolicy = null)
            : this(userId, null, domainPolicy)
        {
        }

        /// <summary>
        /// 생성자 (사용자 DN 정보 포함)
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="userDistinguishedName">사용자 Distinguished Name</param>
        /// <param name="domainPolicy">도메인 비밀번호 정책 (선택사항)</param>
        public UnifiedPasswordForm(string userId, string userDistinguishedName, Models.DomainPasswordPolicy domainPolicy = null)
        {
            _userId = userId ?? "Unknown";
            _userDistinguishedName = userDistinguishedName;
            _domainPolicy = domainPolicy;
            
            InitializeComponent();
            
            // 아이콘 설정
            try
            {
                this.Icon = Properties.Resources.ad192_icon;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UnifiedPasswordForm 아이콘 설정 오류: {ex.Message}");
            }
            
            InitializeOUWarningTimer();
            SetupEventHandlers();
            SetupDynamicContent();
            SetupPasswordPolicySettings();
            LoadInitialData();
            
            // 초기 포커스를 새 비밀번호 텍스트박스로 설정
            this.Load += (s, e) => {
                textBoxNewPassword.Focus();
                textBoxNewPassword.Select();
            };
        }

        #endregion

        #region Event Handler Setup

        /// <summary>
        /// OU 경고 타이머 초기화
        /// </summary>
        private void InitializeOUWarningTimer()
        {
            ouWarningTimer = new Timer();
            ouWarningTimer.Interval = 600; // 600ms 간격으로 깜박임
            ouWarningTimer.Tick += OUWarningTimer_Tick;
        }

        /// <summary>
        /// OU 경고 타이머 이벤트
        /// </summary>
        private void OUWarningTimer_Tick(object sender, EventArgs e)
        {
            if (ouPathLabel != null)
            {
                ouWarningVisible = !ouWarningVisible;
                ouPathLabel.Visible = ouWarningVisible;
            }
        }

        /// <summary>
        /// 동적 컨텐츠 설정
        /// </summary>
        private void SetupDynamicContent()
        {
            // 동적 텍스트 설정
            if (!string.IsNullOrEmpty(_userId))
            {
                // userLabel 텍스트 업데이트
                userLabel.Text = $"대상 계정: {_userId}";
                
                // OU 정보 설정 (디자이너에서 정의된 라벨 사용)
                SetupOUDisplay();
                
                // 폼 타이틀 업데이트
                this.Text = $"비밀번호 관리 - {_userId}";
            }
        }

        /// <summary>
        /// 이벤트 핸들러 설정
        /// </summary>
        private void SetupEventHandlers()
        {
            this.textBoxNewPassword.TextChanged += TextBoxNewPassword_TextChanged;
            this.textBoxNewPassword.KeyDown += TextBoxNewPassword_KeyDown;
            this.textBoxConfirmPassword.TextChanged += TextBoxConfirmPassword_TextChanged;
            this.textBoxConfirmPassword.KeyDown += TextBoxConfirmPassword_KeyDown;
            this.buttonCopyPassword.Click += ButtonCopyPassword_Click;
            this.buttonToggleVisibility.Click += ButtonToggleVisibility_Click;
            this.buttonGenerateMultiple.Click += ButtonGenerateMultiple_Click;
            this.listBoxGeneratedPasswords.DoubleClick += ListBoxGeneratedPasswords_DoubleClick;
            this.listBoxGeneratedPasswords.SelectedIndexChanged += ListBoxGeneratedPasswords_SelectedIndexChanged;
            this.buttonOK.Click += ButtonOK_Click;
            this.buttonCancel.Click += ButtonCancel_Click;
            
            // IME를 영문으로 고정
            IMEHelper.SetEnglishOnly(textBoxNewPassword, textBoxConfirmPassword);
        }

        #endregion

        #region Event Handlers

        private void TextBoxNewPassword_TextChanged(object sender, EventArgs e)
        {
            UpdatePasswordStrength();
            UpdateMatchStatus();
            UpdateUIState();
        }

        private void TextBoxNewPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                // 엔터 키 또는 탭 키를 누르면 비밀번호 확인으로 포커스 이동
                textBoxConfirmPassword.Focus();
                textBoxConfirmPassword.SelectAll();
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true; // 엔터 키일 때만 비프 소리 방지
                }
            }
        }

        private void TextBoxConfirmPassword_TextChanged(object sender, EventArgs e)
        {
            UpdateMatchStatus();
            UpdateUIState();
        }

        private void TextBoxConfirmPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // 엔터 키를 누르면 비밀번호 변경 실행
                if (buttonOK.Enabled)
                {
                    ButtonOK_Click(sender, e);
                }
                e.SuppressKeyPress = true; // 비프 소리 방지
            }
        }

        private void ButtonCopyPassword_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxNewPassword.Text))
            {
                try
                {
                    Clipboard.SetText(textBoxNewPassword.Text);
                    // 조용히 복사 (메시지 없음)
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex, "클립보드 복사 오류", this, true);
                }
            }
        }

        private void ButtonToggleVisibility_Click(object sender, EventArgs e)
        {
            bool showPassword = textBoxNewPassword.UseSystemPasswordChar;
            textBoxNewPassword.UseSystemPasswordChar = !showPassword;
            textBoxConfirmPassword.UseSystemPasswordChar = !showPassword;
            buttonToggleVisibility.Text = showPassword ? "🙈" : "👁";
        }

        private void ButtonGenerateMultiple_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== ButtonGenerateMultiple_Click 시작 ===");
                
                // 자동으로 목록 지우기
                listBoxGeneratedPasswords.Items.Clear();
                
                int length = (int)numericUpDownLength.Value;
                System.Diagnostics.Debug.WriteLine($"선택된 비밀번호 길이: {length}자");
                System.Diagnostics.Debug.WriteLine($"numericUpDownLength - Min: {numericUpDownLength.Minimum}, Value: {numericUpDownLength.Value}, Max: {numericUpDownLength.Maximum}");
                
                // 설정된 길이로 다양한 옵션의 비밀번호 5개 생성
                System.Diagnostics.Debug.WriteLine($"비밀번호 생성 시작 - 길이: {length}자");
                
                var passwords = new[]
                {
                    PasswordGenerator.GenerateCostcoPassword(true, length),   // 코스트코 + 연도
                    PasswordGenerator.GenerateCostcoPassword(false, length),  // 코스트코 기본
                    PasswordGenerator.GeneratePassword(length, true, true, false),   // 기본 단어 + 숫자
                    PasswordGenerator.GeneratePassword(length, true, false, false),  // 일반 단어 + 숫자
                    PasswordGenerator.GeneratePassword(length, true, true, true),    // 기본 단어 + 숫자 + 특문
                };
                
                System.Diagnostics.Debug.WriteLine($"생성된 비밀번호 개수: {passwords.Length}");
                for (int i = 0; i < passwords.Length; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"비밀번호 {i + 1}: '{passwords[i]}' (길이: {passwords[i]?.Length ?? 0})");
                }

                foreach (string password in passwords.Where(p => !string.IsNullOrEmpty(p)))
                {
                    int strength = PasswordGenerator.CheckPasswordStrength(password);
                    string strengthDesc = PasswordGenerator.GetPasswordStrengthDescription(password);
                    string displayText = $"{password} (강도: {strengthDesc} - {strength}%)";
                    
                    listBoxGeneratedPasswords.Items.Add(displayText);
                }

                System.Diagnostics.Debug.WriteLine($"리스트에 추가된 비밀번호 개수: {listBoxGeneratedPasswords.Items.Count}");
                
                if (listBoxGeneratedPasswords.Items.Count > 0)
                {
                    listBoxGeneratedPasswords.SelectedIndex = 0;
                    System.Diagnostics.Debug.WriteLine($"기본 선택: 0번째 아이템 (첫 번째)");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ 생성된 비밀번호가 없음");
                }
                
                System.Diagnostics.Debug.WriteLine("=== ButtonGenerateMultiple_Click 완료 ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ButtonGenerateMultiple_Click 오류: {ex.Message}");
                ErrorHandler.HandleException(ex, "비밀번호 생성 오류", this, true);
            }
        }

        private void ListBoxGeneratedPasswords_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 선택 변경 시 특별한 작업 없음
        }

        private void ListBoxGeneratedPasswords_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxGeneratedPasswords.SelectedItem != null)
            {
                string selectedItem = listBoxGeneratedPasswords.SelectedItem.ToString();
                // "password (강도: xxx)" 형태에서 password 부분만 추출
                int spaceIndex = selectedItem.IndexOf(' ');
                string password = spaceIndex > 0 ? selectedItem.Substring(0, spaceIndex) : selectedItem;
                
                textBoxNewPassword.Text = password;
                textBoxConfirmPassword.Text = password;
                textBoxNewPassword.Focus();
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (ValidatePassword())
            {
                _selectedPassword = textBoxNewPassword.Text;
                
                var result = MessageBox.Show(
                    $"사용자 '{_userId}'의 비밀번호를 변경하시겠습니까?" +
                    (checkBoxForceChange.Checked ? "\n\n※ 다음 로그인 시 비밀번호 변경이 강제됩니다." : ""),
                    "비밀번호 변경 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    // OK 다이얼로그 결과를 설정하지만 성공 시에만 창을 닫음
                    // 비밀번호 변경 실패 시에는 호출자(MainForm)에서 오류 메시지 표시 후 창을 닫지 않음
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                // No를 선택한 경우 창을 닫지 않고 계속 진행
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// 폼 닫기 전 리소스 정리
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // OU 경고 타이머 중지 및 정리
            if (ouWarningTimer != null)
            {
                ouWarningTimer.Stop();
                ouWarningTimer.Dispose();
                ouWarningTimer = null;
            }
            
            base.OnFormClosed(e);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// OU 정보 표시 설정
        /// </summary>
        private void SetupOUDisplay()
        {
            // 디자이너에서 정의된 라벨들이 존재하는지 확인
            if (ouPrefixLabel == null || ouPathLabel == null)
            {
                System.Diagnostics.Debug.WriteLine("OU 라벨들이 디자이너에서 정의되지 않았습니다.");
                return;
            }

            // 기본적으로 타이머 중지 및 가시성 복원
            ouWarningTimer?.Stop();
            ouPathLabel.Visible = true;
            ouWarningVisible = true;

            try
            {
                if (string.IsNullOrEmpty(_userDistinguishedName))
                {
                    ouPathLabel.Text = "정보 없음";
                    ouPathLabel.ForeColor = System.Drawing.SystemColors.GrayText;
                    ouPathLabel.Font = new System.Drawing.Font(ouPathLabel.Font, System.Drawing.FontStyle.Regular);
                    return;
                }

                // DN에서 OU 경로 추출
                string ouPath = Services.ADService.ExtractOUPath(_userDistinguishedName);
                
                if (string.IsNullOrEmpty(ouPath))
                {
                    ouPathLabel.Text = "경로 없음";
                    ouPathLabel.ForeColor = System.Drawing.Color.Orange; // 경고색
                    ouPathLabel.Font = new System.Drawing.Font(ouPathLabel.Font, System.Drawing.FontStyle.Bold); // 굵은 폰트
                    ouWarningTimer.Start(); // 깜박임 시작
                    return;
                }

                // 코스트코 한국 OU에 속하는지 확인
                bool isInAllowedOU = Services.ADService.IsUserInCostcoKoreaOU(ouPath);
                
                // OU 경로 표시 (너무 길면 줄임)
                string displayPath = ouPath;
                if (ouPath.Length > 50) // 폭이 조정되었으므로 길이 제한 조정
                {
                    displayPath = "..." + ouPath.Substring(ouPath.Length - 47);
                }
                
                ouPathLabel.Text = displayPath;
                
                // 색상 및 폰트 설정
                if (isInAllowedOU)
                {
                    ouPathLabel.ForeColor = System.Drawing.Color.Green; // 허용된 OU - 녹색
                    ouPathLabel.Font = new System.Drawing.Font(ouPathLabel.Font, System.Drawing.FontStyle.Regular); // 일반 폰트
                    // 깜빡임 없음
                }
                else
                {
                    ouPathLabel.ForeColor = System.Drawing.Color.Orange; // 허용되지 않은 OU - 경고색
                    ouPathLabel.Font = new System.Drawing.Font(ouPathLabel.Font, System.Drawing.FontStyle.Bold); // 굵은 폰트
                    ouWarningTimer.Start(); // 깜빡임 시작
                }
                
                // 디버그 로그
                System.Diagnostics.Debug.WriteLine($"OU 정보: {_userId}, DN: {_userDistinguishedName}");
                System.Diagnostics.Debug.WriteLine($"OU 경로: {ouPath}");
                System.Diagnostics.Debug.WriteLine($"허용된 OU: {isInAllowedOU}");
            }
            catch (Exception ex)
            {
                ouPathLabel.Text = "오류 발생";
                ouPathLabel.ForeColor = System.Drawing.Color.Red;
                ouPathLabel.Font = new System.Drawing.Font(ouPathLabel.Font, System.Drawing.FontStyle.Bold); // 굵은 폰트
                ouWarningTimer.Start(); // 깜빡임 시작
                System.Diagnostics.Debug.WriteLine($"OU 표시 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 도메인 비밀번호 정책 기반 설정
        /// </summary>
        private void SetupPasswordPolicySettings()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== SetupPasswordPolicySettings 시작 ===");
                System.Diagnostics.Debug.WriteLine($"사용자: {_userId}");
                System.Diagnostics.Debug.WriteLine($"도메인 정책 객체: {(_domainPolicy != null ? "존재" : "null")}");
                
                if (_domainPolicy != null)
                {
                    System.Diagnostics.Debug.WriteLine($"도메인명: {_domainPolicy.DomainName}");
                    System.Diagnostics.Debug.WriteLine($"도메인 컨트롤러: {_domainPolicy.DomainController}");
                    System.Diagnostics.Debug.WriteLine($"정책 조회 시간: {_domainPolicy.RetrievedAt}");
                    System.Diagnostics.Debug.WriteLine($"정책 유효성: {_domainPolicy.IsValid()}");
                    System.Diagnostics.Debug.WriteLine($"최소 비밀번호 길이: {_domainPolicy.MinPasswordLength}");
                    System.Diagnostics.Debug.WriteLine($"최대 비밀번호 사용기간: {_domainPolicy.MaxPasswordAge}일");
                    System.Diagnostics.Debug.WriteLine($"복잡성 요구사항: {_domainPolicy.PasswordComplexityRequired}");
                    
                    if (_domainPolicy.IsValid())
                    {
                        // AD 정책에서 최소 비밀번호 길이 가져오기
                        int minLength = _domainPolicy.MinPasswordLength;
                        System.Diagnostics.Debug.WriteLine($"AD 정책 최소 길이: {minLength}");
                        
                        // AD 정책값을 그대로 사용 (안전을 위해 최소 4자만 보장)
                        int effectiveMinLength = Math.Max(minLength, 4);  // 최소 4자만 보장
                        int defaultLength = effectiveMinLength; // AD 정책의 최소 길이를 기본값으로 사용
                        
                        System.Diagnostics.Debug.WriteLine($"적용된 최소 길이: {effectiveMinLength} (AD 정책 우선 적용)");
                        System.Diagnostics.Debug.WriteLine($"기본 비밀번호 길이: {defaultLength} (AD 정책과 동일)");
                        
                        // UI 컨트롤 설정 전 현재 값 확인
                        System.Diagnostics.Debug.WriteLine($"설정 전 - Minimum: {numericUpDownLength.Minimum}, Value: {numericUpDownLength.Value}, Maximum: {numericUpDownLength.Maximum}");
                        
                        numericUpDownLength.Minimum = effectiveMinLength;
                        numericUpDownLength.Maximum = 20;
                        numericUpDownLength.Value = defaultLength;
                        
                        // UI 컨트롤 설정 후 값 확인
                        System.Diagnostics.Debug.WriteLine($"설정 후 - Minimum: {numericUpDownLength.Minimum}, Value: {numericUpDownLength.Value}, Maximum: {numericUpDownLength.Maximum}");
                        
                        System.Diagnostics.Debug.WriteLine($"✅ AD 정책 적용 성공 - 최소: {effectiveMinLength}자 (AD 정책: {minLength}자), 기본: {defaultLength}자");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ 도메인 정책이 유효하지 않음 - 기본값 사용");
                        SetDefaultPasswordSettings();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ 도메인 정책 객체가 null - 기본값 사용");
                    SetDefaultPasswordSettings();
                }
                
                System.Diagnostics.Debug.WriteLine("=== SetupPasswordPolicySettings 완료 ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SetupPasswordPolicySettings 오류: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"스택 트레이스: {ex.StackTrace}");
                
                // 오류 시 기본값 설정
                SetDefaultPasswordSettings();
            }
        }
        
        /// <summary>
        /// 기본 비밀번호 설정 적용
        /// </summary>
        private void SetDefaultPasswordSettings()
        {
            System.Diagnostics.Debug.WriteLine("기본값 설정 시작");
            
            numericUpDownLength.Minimum = 6;
            numericUpDownLength.Maximum = 20;
            numericUpDownLength.Value = 8;
            
            System.Diagnostics.Debug.WriteLine($"기본값 설정 완료 - 최소: 6, 기본: 8, 최대: 20");
        }

        /// <summary>
        /// 초기 데이터 로드
        /// </summary>
        private void LoadInitialData()
        {
            // 초기 다중 비밀번호 생성
            ButtonGenerateMultiple_Click(null, null);
        }

        /// <summary>
        /// 비밀번호 강도 업데이트
        /// </summary>
        private void UpdatePasswordStrength()
        {
            if (string.IsNullOrEmpty(textBoxNewPassword.Text))
            {
                labelPasswordStrength.Text = "입력 대기 중";
                labelPasswordStrength.ForeColor = SystemColors.GrayText;
                return;
            }

            string password = textBoxNewPassword.Text;
            int strength = PasswordGenerator.CheckPasswordStrength(password);
            string description = PasswordGenerator.GetPasswordStrengthDescription(password);

            labelPasswordStrength.Text = $"{description} ({strength}%)";
            
            if (strength >= 80)
                labelPasswordStrength.ForeColor = Color.Green;
            else if (strength >= 60)
                labelPasswordStrength.ForeColor = Color.Orange;
            else
                labelPasswordStrength.ForeColor = Color.Red;
        }

        /// <summary>
        /// 비밀번호 일치 상태 업데이트
        /// </summary>
        private void UpdateMatchStatus()
        {
            if (string.IsNullOrEmpty(textBoxNewPassword.Text) || string.IsNullOrEmpty(textBoxConfirmPassword.Text))
            {
                labelMatchStatus.Text = "";
                return;
            }

            if (textBoxNewPassword.Text == textBoxConfirmPassword.Text)
            {
                labelMatchStatus.Text = "✓ 비밀번호가 일치합니다";
                labelMatchStatus.ForeColor = Color.Green;
            }
            else
            {
                labelMatchStatus.Text = "✗ 비밀번호가 일치하지 않습니다";
                labelMatchStatus.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// UI 상태 업데이트
        /// </summary>
        private void UpdateUIState()
        {
            bool hasPassword = !string.IsNullOrEmpty(textBoxNewPassword.Text);
            
            buttonCopyPassword.Enabled = hasPassword;
            buttonOK.Enabled = ValidatePassword();
        }

        /// <summary>
        /// 비밀번호 유효성 검사
        /// </summary>
        private bool ValidatePassword()
        {
            return !string.IsNullOrEmpty(textBoxNewPassword.Text) &&
                   !string.IsNullOrEmpty(textBoxConfirmPassword.Text) &&
                   textBoxNewPassword.Text == textBoxConfirmPassword.Text &&
                   textBoxNewPassword.Text.Length >= 6;
        }

        #endregion
    }
}
