using System;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using adHelp.Models;
using adHelp.Services;
using adHelp.Utils;

namespace adHelp.Forms
{
    /// <summary>
    /// AD 로그인 폼
    /// </summary>
    public partial class LoginForm : Form
    {
        private CredentialInfo _credential;
        private ADService _adService;

        /// <summary>
        /// 인증된 크리덴셜 정보
        /// </summary>
        public CredentialInfo AuthenticatedCredential => _adService?.CurrentCredential ?? _credential;

        /// <summary>
        /// 생성자
        /// </summary>
        public LoginForm()
        {
            InitializeComponent();
            InitializeForm();
        }

        /// <summary>
        /// 폼 초기화
        /// </summary>
        private void InitializeForm()
        {
            // 아이콘 설정
            try
            {
                this.Icon = Properties.Resources.ad192_icon;
                pictureBoxLogo.Image = Properties.Resources.ad192_image;
                pictureBoxLogo.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"아이콘 설정 오류: {ex.Message}");
            }
            
            // 현재 사용자의 도메인을 기본값으로 설정
            string currentDomain = GetCurrentUserDomain();
            textBoxDomain.Text = currentDomain;
            
            // 기본 사용자명 설정
            textBoxUsername.Text = ConfigManager.GetDefaultAdminUsername();
            
            // 도메인 변경 이벤트 연결
            textBoxDomain.TextChanged += textBoxDomain_TextChanged;
            textBoxUsername.TextChanged += textBoxUsername_TextChanged;
            
            // SSL 기본값 설정 (Designer에서 이미 Checked = true로 설정됨)
            // 초기 툴팁 설정
            toolTip.SetToolTip(checkBoxSSL, "포트: 636 (SSL) - 보안 연결 권장");
            
            // 기본 버튼 설정
            AcceptButton = buttonLogin;
            CancelButton = buttonCancel;
        }

        /// <summary>
        /// 로그인 버튼 클릭 이벤트
        /// </summary>
        private void buttonLogin_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                PerformLogin();
            }
        }

        /// <summary>
        /// 취소 버튼 클릭 이벤트
        /// </summary>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// 입력값 검증
        /// </summary>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(textBoxDomain.Text))
            {
                MessageBox.Show("도메인을 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.None);
                textBoxDomain.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxUsername.Text))
            {
                MessageBox.Show("사용자 ID를 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.None);
                textBoxUsername.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(textBoxPassword.Text))
            {
                MessageBox.Show("비밀번호를 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.None);
                textBoxPassword.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 로그인 수행
        /// </summary>
        private void PerformLogin()
        {
            // UI 비활성화
            SetControlsEnabled(false);
            labelStatus.Text = "인증 중...";
            labelStatus.ForeColor = Color.Blue;

            try
            {
                // 크리덴셜 생성
                _credential = new CredentialInfo(
                    textBoxDomain.Text.Trim(),
                    textBoxUsername.Text.Trim()
                );
                _credential.SetPassword(textBoxPassword.Text);

                // SSL 설정 적용
                _credential.UseSSL = checkBoxSSL.Checked;
                _credential.Port = checkBoxSSL.Checked ? 636 : 389;

                // AD 서비스 생성 및 연결 테스트
                _adService = new ADService();
                
                if (_adService.Connect(_credential))
                {
                    // 연결 성곰 시 크리덴셜 상태 명시적으로 설정
                    _credential.IsAuthenticated = true;
                    _credential.LastAuthenticationTime = DateTime.Now;
                    _credential.LastErrorMessage = null;
                    
                    labelStatus.Text = "인증 성공!";
                    labelStatus.ForeColor = Color.Green;
                    
                    // 잠시 대기 후 폼 닫기
                    System.Threading.Thread.Sleep(500);
                    
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    // 연결 실패 시 상태 설정
                    _credential.IsAuthenticated = false;
                    _credential.LastErrorMessage = "AD 서비스 연결 실패";
                    ShowError("인증에 실패했습니다.");
                }
            }
            catch (Exception ex)
            {
                // 예외 발생 시 상태 설정
                if (_credential != null)
                {
                    _credential.IsAuthenticated = false;
                    _credential.LastErrorMessage = ex.Message;
                }
                ShowError($"로그인 실패: {ex.Message}");
            }
            finally
            {
                // UI 재활성화
                SetControlsEnabled(true);
                if (DialogResult != DialogResult.OK)
                {
                    labelStatus.Text = "로그인 정보를 입력하세요.";
                    labelStatus.ForeColor = Color.Black;
                }
            }
        }

        /// <summary>
        /// 오류 메시지 표시
        /// </summary>
        private void ShowError(string message)
        {
            labelStatus.Text = "로그인 실패";
            labelStatus.ForeColor = Color.Red;
            
            MessageBox.Show(message, "로그인 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            // 비밀번호 필드에 포커스만 이동 (비밀번호는 지우지 않음)
            textBoxPassword.Focus();
            textBoxPassword.SelectAll();
        }

        /// <summary>
        /// 컨트롤 활성화/비활성화
        /// </summary>
        private void SetControlsEnabled(bool enabled)
        {
            textBoxDomain.Enabled = enabled;
            textBoxUsername.Enabled = enabled;
            textBoxPassword.Enabled = enabled;
            buttonLogin.Enabled = enabled;
            checkBoxSSL.Enabled = enabled;
        }

        /// <summary>
        /// SSL 체크박스 변경 이벤트
        /// </summary>
        private void checkBoxSSL_CheckedChanged(object sender, EventArgs e)
        {
            // SSL 설정에 따른 포트 정보 표시
            if (checkBoxSSL.Checked)
            {
                toolTip.SetToolTip(checkBoxSSL, "포트: 636 (SSL) - 보안 연결 사용");
            }
            else
            {
                toolTip.SetToolTip(checkBoxSSL, "포트: 389 (일반) - 보안연결 비활성화");
            }
        }

        /// <summary>
        /// 키 입력 이벤트 (엔터키 처리)
        /// </summary>
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; // 기본 처리 방지
                
                if (sender == textBoxDomain)
                    textBoxUsername.Focus();
                else if (sender == textBoxUsername)
                    textBoxPassword.Focus();
                else if (sender == textBoxPassword)
                    buttonLogin_Click(sender, e);
            }
        }

        /// <summary>
        /// 폼 로드 이벤트
        /// </summary>
        private void LoginForm_Load(object sender, EventArgs e)
        {
            labelStatus.Text = "로그인 정보를 입력하세요.";
            labelStatus.ForeColor = Color.Black;
            
            // 비밀번호 필드에 포커스 설정 (두 가지 방법 모두 사용)
            this.ActiveControl = textBoxPassword;
            textBoxPassword.Focus();
            textBoxPassword.Select();
        }

        /// <summary>
        /// 현재 사용자의 도메인 정보 가져오기
        /// </summary>
        private string GetCurrentUserDomain()
        {
            try
            {
                string userDomain = Environment.UserDomainName;
                
                // 도메인 정보가 있는지 확인
                if (!string.IsNullOrWhiteSpace(userDomain) && 
                    !userDomain.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase))
                {
                    // 도메인에 가입되어 있는 경우
                    return userDomain;
                }
                else
                {
                    // 도메인에 가입되어 있지 않은 경우 기본값 사용
                    return "intl.costco.com";
                }
            }
            catch
            {
                // 오류 발생 시 기본값 반환
                return "intl.costco.com";
            }
        }

        /// <summary>
        /// 도메인 변경 이벤트 처리
        /// </summary>
        private void textBoxDomain_TextChanged(object sender, EventArgs e)
        {
            UpdateUsernameBasedOnDomain();
        }

        /// <summary>
        /// 사용자명 변경 이벤트 처리
        /// </summary>
        private void textBoxUsername_TextChanged(object sender, EventArgs e)
        {
            // 사용자가 직접 변경한 경우이므로 도메인 기반 업데이트는 하지 않음
            // 하지만 -a 추가/제거 로직은 적용
            if (!_isUpdatingUsername)
            {
                UpdateUsernameBasedOnDomain();
            }
        }

        private bool _isUpdatingUsername = false;

        /// <summary>
        /// 도메인에 따라 사용자명 업데이트
        /// </summary>
        private void UpdateUsernameBasedOnDomain()
        {
            if (_isUpdatingUsername) return;
            
            _isUpdatingUsername = true;
            try
            {
                string domain = textBoxDomain.Text.Trim();
                string currentUsername = textBoxUsername.Text.Trim();
                
                if (domain.Equals("intl.costco.com", StringComparison.OrdinalIgnoreCase))
                {
                    // intl.costco.com 도메인인 경우 -a 추가
                    if (!string.IsNullOrEmpty(currentUsername) && !currentUsername.EndsWith("-a"))
                    {
                        textBoxUsername.Text = currentUsername + "-a";
                    }
                }
                else
                {
                    // 다른 도메인인 경우 -a 제거
                    if (!string.IsNullOrEmpty(currentUsername) && currentUsername.EndsWith("-a"))
                    {
                        textBoxUsername.Text = currentUsername.Substring(0, currentUsername.Length - 2);
                    }
                }
            }
            finally
            {
                _isUpdatingUsername = false;
            }
        }

        /// <summary>
        /// 폼 닫기 이벤트 - 리소스 정리
        /// </summary>
        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 인증 성공 시에는 ADService를 dispose하지 않음
                // (Disconnect가 호출되면 credential.IsAuthenticated가 false로 설정됨)
                if (DialogResult != DialogResult.OK)
                {
                    _adService?.Dispose();
                }
                
                // 인증에 실패한 경우에만 크리덴셜 dispose
                // 성공한 경우에는 Program.cs에서 사용할 수 있도록 유지
                if (DialogResult != DialogResult.OK && _credential != null)
                {
                    _credential.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"리소스 정리 중 오류: {ex.Message}");
            }
        }
    }
}
