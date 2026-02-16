using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using adHelp.Models;
using adHelp.Utils;

namespace adHelp.Forms
{
    /// <summary>
    /// MainForm의 UI 업데이트, 타이틀 관리 (Part 4/5)
    /// 사용자 정보 표시, 상태 업데이트, 포맷팅 등 UI 관련 기능들
    /// </summary>
    public partial class MainForm : Form
    {
        #region Private Methods - UI Updates






        
        /// <summary>
        /// 연결 상태 업데이트
        /// </summary>
        private void UpdateConnectionStatus()
        {
            try
            {
                if (_adService != null && _adService.IsConnected)
                {
                    string userDomain = _currentCredential != null ? _currentCredential.Domain : "알 수 없음";
                    string connectedServer = _adService.ConnectedServerName;
                    
                    if (string.IsNullOrEmpty(connectedServer))
                    {
                        connectedServer = "서버 정보 없음";
                    }
                    
                    string displayText = $"연결됨: {userDomain} - {connectedServer}";
                    
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() => UpdateLabelSafely(displayText)));
                    }
                    else
                    {
                        UpdateLabelSafely(displayText);
                    }
                }
                else
                {
                    string errorText = "연결되지 않음";
                    
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() => 
                        {
                            labelConnectionStatus.Text = errorText;
                            labelConnectionStatus.ForeColor = Color.Red;
                        }));
                    }
                    else
                    {
                        labelConnectionStatus.Text = errorText;
                        labelConnectionStatus.ForeColor = Color.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"MainForm: UpdateConnectionStatus 오류: {ex.Message}");
                labelConnectionStatus.Text = "연결 상태 오류";
                labelConnectionStatus.ForeColor = Color.Red;
            }
        }
        
        /// <summary>
        /// 레이블 안전하게 업데이트
        /// </summary>
        /// <param name="text">표시할 텍스트</param>
        private void UpdateLabelSafely(string text)
        {
            try
            {
                labelConnectionStatus.Visible = true;
                labelConnectionStatus.BringToFront();
                labelConnectionStatus.Text = text;
                labelConnectionStatus.ForeColor = Color.Green;
                
                labelConnectionStatus.Invalidate();
                labelConnectionStatus.Update();
                
                if (labelConnectionStatus.Parent != null)
                {
                    labelConnectionStatus.Parent.Invalidate();
                    labelConnectionStatus.Parent.Update();
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"MainForm: UpdateLabelSafely 오류: {ex.Message}");
            }
        }
        


        #endregion

        #region Private Methods - Helper Functions

        /// <summary>
        /// 전체 도메인명에서 짧은 도메인명 추출
        /// </summary>
        private string ExtractShortDomainName(string fullDomain)
        {
            if (string.IsNullOrEmpty(fullDomain))
                return "unknown";
                
            int dotIndex = fullDomain.IndexOf('.');
            return dotIndex > 0 ? fullDomain.Substring(0, dotIndex) : fullDomain;
        }

        #endregion
        
        #region Private Methods - Window Title Management

        /// <summary>
        /// 윈도우 타이틀 설정
        /// </summary>
        private void SetWindowTitle()
        {
            try
            {
                string baseTitle = VersionHelper.GetFullProductTitle(false);
                this.Text = baseTitle;
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"MainForm: SetWindowTitle 오류: {ex.Message}");
                this.Text = "AD Helper";
            }
        }
        
        /// <summary>
        /// 사용자 정보를 포함한 윈도우 타이틀 업데이트
        /// </summary>
        private void UpdateWindowTitle()
        {
            try
            {
                string currentUser = _currentCredential != null ? _currentCredential.Username : null;
                string connectedDomain = _currentCredential != null ? _currentCredential.Domain : null;
                
                string title = VersionHelper.GetWindowTitle(currentUser, connectedDomain);
                this.Text = title;
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"MainForm: UpdateWindowTitle 오류: {ex.Message}");
            }
        }

        #endregion
    }
}
