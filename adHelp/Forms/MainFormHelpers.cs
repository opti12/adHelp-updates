using System;
using System.Drawing;
using System.Windows.Forms;
using adHelp.Models;
using adHelp.Utils;

namespace adHelp.Forms
{
    /// <summary>
    /// MainForm의 Helper 메서드들 (Part 3/5)
    /// 유틸리티 및 도우미 기능들
    /// </summary>
    public partial class MainForm : Form
    {
        #region Private Methods - Helper Functions

        /// <summary>
        /// DateTime 포맷팅
        /// </summary>
        private string FormatDateTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue || dateTime.Value == DateTime.MinValue)
                return "-";
            
            return dateTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 비밀번호 상태에 따른 색상 반환 (개선된 로직)
        /// </summary>
        private Color GetPasswordStatusColor(UserInfo userInfo)
        {
            // 1. 다음 로그인 시 비밀번호 변경 필요 - 노란색
            if (userInfo.MustChangePasswordAtNextLogon)
                return Color.Orange;
                
            // 2. 비밀번호 만료 - 빨간색
            if (userInfo.IsPasswordExpired)
                return Color.Red;
            
            // 3. 비밀번호 만료 날짜가 있는 경우
            if (userInfo.PasswordExpiryDate.HasValue)
            {
                var daysUntilExpiry = (userInfo.PasswordExpiryDate.Value - DateTime.Now).Days;
                if (daysUntilExpiry <= 7)
                    return Color.Orange; // 곧 만료
                else
                    return Color.Green; // 정상
            }
            
            // 4. 기본 색상
            return Color.Black;
        }

        // ShowPasswordInputDialog 메서드는 MainForm.cs에 정의됨 (중복 제거)

        /// <summary>
        /// 비밀번호 입력 다이얼로그 표시
        /// </summary>
        private string ShowPasswordInputDialog()
        {
            // 간단한 비밀번호 입력 다이얼로그 구현
            // 실제 구현에서는 보안을 위해 SecureString을 사용해야 함
            var form = new Form()
            {
                Text = "비밀번호 입력",
                Size = new Size(300, 120),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label()
            {
                Text = "비밀번호:",
                Location = new Point(10, 20),
                Size = new Size(70, 20)
            };

            var textBox = new TextBox()
            {
                Location = new Point(90, 18),
                Size = new Size(180, 20),
                UseSystemPasswordChar = true
            };

            var buttonOk = new Button()
            {
                Text = "확인",
                Location = new Point(115, 50),
                Size = new Size(75, 23),
                DialogResult = DialogResult.OK
            };

            var buttonCancel = new Button()
            {
                Text = "취소",
                Location = new Point(195, 50),
                Size = new Size(75, 23),
                DialogResult = DialogResult.Cancel
            };

            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            textBox.Focus();

            try
            {
                return form.ShowDialog(this) == DialogResult.OK ? textBox.Text : null;
            }
            finally
            {
                form?.Dispose();
            }
        }

        /// <summary>
        /// 정보 대화상자 표시 (도메인 정책 정보 포함)
        /// </summary>
        private void ShowAboutDialog()
        {
            try
            {
                string domainPolicyInfo = "";
                
                // 도메인 비밀번호 정책 정보 추가
                if (_adService != null && _adService.IsConnected)
                {
                    try
                    {
                        var domainPolicy = _adService.DomainPasswordPolicy;
                        if (domainPolicy != null && domainPolicy.IsValid())
                        {
                            domainPolicyInfo = "\n\n도메인 비밀번호 정책:\n" +
                                             $"• 최대 비밀번호 사용기간: {domainPolicy.MaxPasswordAge}일\n" +
                                             $"• 최소 비밀번호 길이: {domainPolicy.MinPasswordLength}자\n" +
                                             $"• 복잡성 요구사항: {(domainPolicy.PasswordComplexityRequired ? "필요" : "불필요")}\n" +
                                             $"• 비밀번호 히스토리: {domainPolicy.PasswordHistoryLength}개\n" +
                                             $"• 잠금 임계값: {(domainPolicy.AccountLockoutThreshold > 0 ? $"{domainPolicy.AccountLockoutThreshold}회 시도" : "제한 없음")}\n" +
                                             $"• 경고 기간: {domainPolicy.PasswordExpiryWarningDays}일 전, 위험 기간: {domainPolicy.PasswordExpiryDangerDays}일 전";
                        }
                        else
                        {
                            domainPolicyInfo = "\n\n도메인 비밀번호 정책: 조회 불가";
                        }
                    }
                    catch (Exception policyEx)
                    {
                        SimpleLogger.Log($"도메인 정책 조회 오류: {policyEx.Message}");
                        domainPolicyInfo = "\n\n도메인 비밀번호 정책: 조회 오류";
                    }
                }
                
                string aboutMessage = $"{VersionHelper.GetAboutVersionString()}\n\n" +
                                     $"연결된 도메인: {(_currentCredential != null ? _currentCredential.Domain : "알 수 없음")}\n" +
                                     $"도메인 컨트롤러: {(_adService != null ? _adService.ConnectedServerName : "알 수 없음")}\n" +
                                     $"인증된 사용자: {(_currentCredential != null ? _currentCredential.Username : "알 수 없음")}\n\n" +
                                     $"주요 기능:\n" +
                                     $"• 사용자 정보 조회 및 비밀번호 만료 예측\n" +
                                     $"• 계정 잠금 해제\n" +
                                     $"• 비밀번호 변경 및 자동 생성\n" +
                                     $"• 도메인 정책 기반 비밀번호 관리\n" +
                                     $"• 상세 정보 조회 및 내보내기\n" +
                                     domainPolicyInfo +
                                     $"\n\n{VersionHelper.Copyright}";
                
                MessageBox.Show(
                    aboutMessage,
                    $"About {VersionHelper.ProductName}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.None);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"ShowAboutDialog 오류: {ex.Message}");
                
                // 대체 메시지
                string fallbackMessage = "AD Helper\n" +
                                        "Active Directory Management Tool\n\n" +
                                        $"연결된 도메인: {_currentCredential?.Domain ?? "알 수 없음"}\n" +
                                        $"인증된 사용자: {_currentCredential?.Username ?? "알 수 없음"}";
                
                MessageBox.Show(
                    fallbackMessage,
                    "About AD Helper",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.None);
            }
        }

        #endregion
    }
}
