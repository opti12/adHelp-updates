using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using adHelp.Models;
using adHelp.Services;
using adHelp.Utils;

namespace adHelp.Forms
{
    /// <summary>
    /// MainForm의 사용자 작업 메서드들 (Part 2/5)
    /// 사용자 검색, 비밀번호 관리, 계정 잠금 해제 등 핵심 기능들
    /// .NET Framework 4.7.2 호환 버전
    /// </summary>
    public partial class MainForm : Form
    {
        #region Private Methods - User Search and Refresh

        /// <summary>
        /// 사용자 검색 수행 (async/await 패턴)
        /// </summary>
        private async Task SearchUserAsync()
        {
            var userId = textBoxUserId.Text.Trim();
            if (string.IsNullOrEmpty(userId))
            {
                MessageBox.Show("사용자 ID를 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.None);
                textBoxUserId.Focus();
                return;
            }

            if (_isSearching) return;

            try
            {
                _isSearching = true;
                UpdateButtonStates();
                UpdateStatusBar($"사용자 '{userId}' 검색 중...");

                var userInfo = await Task.Run(() => _adService.GetUserInfo(userId)).ConfigureAwait(false);

                this.Invoke((Action)(() =>
                {
                    if (userInfo != null)
                    {
                        _currentUser = userInfo;
                        DisplayUserInfo(userInfo);
                        
                        // 그룹 정보 로드
                        _ = LoadUserGroupsAsync(userId);
                        
                        UpdateStatusBar($"사용자 '{userId}' 검색 완료");
                    }
                    else
                    {
                        ClearUserInfo();
                        MessageBox.Show($"사용자 '{userId}'를 찾을 수 없습니다.", "검색 결과", MessageBoxButtons.OK, MessageBoxIcon.None);
                        UpdateStatusBar($"사용자 '{userId}' 검색 실패 - 찾을 수 없음");
                    }
                }));
            }
            catch (Exception ex)
            {
                this.Invoke((Action)(() =>
                {
                    ClearUserInfo();
                    ErrorHandler.HandleException(ex, $"사용자 '{userId}' 검색 실패", this);
                    UpdateStatusBar($"사용자 '{userId}' 검색 오류");
                }));
            }
            finally
            {
                this.Invoke((Action)(() =>
                {
                    _isSearching = false;
                    UpdateButtonStates();
                    
                    // 검색 완료 후 항상 포커스를 textBoxUserId로 복원하여 연속 검색 가능하게 함
                    textBoxUserId.Focus();
                    textBoxUserId.SelectAll();
                }));
            }
        }

        /// <summary>
        /// 사용자 정보 새로고침
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        private async Task RefreshUserInfoAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId) || _isSearching) return;

            try
            {
                _isSearching = true;
                UpdateButtonStates();
                UpdateStatusBar($"사용자 '{userId}' 새로고침 중...");

                var userInfo = await Task.Run(() => _adService.GetUserInfo(userId)).ConfigureAwait(false);

                this.Invoke((Action)(() =>
                {
                    if (userInfo != null)
                    {
                        _currentUser = userInfo;
                        DisplayUserInfo(userInfo);
                        
                        // 그룹 정보도 새로고침
                        _ = LoadUserGroupsAsync(userId);
                        
                        UpdateStatusBar($"사용자 '{userId}' 새로고침 완료");
                    }
                    else
                    {
                        MessageBox.Show($"사용자 '{userId}'를 찾을 수 없습니다.", "새로고침 실패", MessageBoxButtons.OK, MessageBoxIcon.None);
                        UpdateStatusBar($"사용자 '{userId}' 새로고침 실패");
                    }
                }));
            }
            catch (Exception ex)
            {
                this.Invoke((Action)(() =>
                {
                    ErrorHandler.HandleException(ex, $"사용자 '{userId}' 새로고침 실패", this);
                    UpdateStatusBar($"사용자 '{userId}' 새로고침 오류");
                }));
            }
            finally
            {
                this.Invoke((Action)(() =>
                {
                    _isSearching = false;
                    UpdateButtonStates();
                }));
            }
        }

        #endregion

        #region Private Methods - Password Management

        /// <summary>
        /// 비밀번호 관리자 표시
        /// </summary>
        private async Task ShowPasswordManagerAsync()
        {
            if (_currentUser == null)
            {
                MessageBox.Show("사용자를 먼저 검색해주세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }

            if (_isProcessing) return;

            try
            {
                await ShowPasswordFormRecursiveAsync();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "비밀번호 관리자 오류", this);
            }
        }

        /// <summary>
        /// 비밀번호 폼을 재귀적으로 표시 (실패 시 재시도)
        /// </summary>
        private async Task ShowPasswordFormRecursiveAsync()
        {
            if (_currentUser == null) return;

            var domainPolicy = _adService != null ? _adService.DomainPasswordPolicy : null;
            
            var passwordForm = new UnifiedPasswordForm(_currentUser.UserId, _currentUser.DistinguishedName, domainPolicy);
            try
            {
                var result = passwordForm.ShowDialog(this);
            
                if (result == DialogResult.OK && !string.IsNullOrEmpty(passwordForm.SelectedPassword))
                {
                    var changeSuccess = await ApplyPasswordChangeAsync(passwordForm.SelectedPassword, passwordForm.ForceChangeAtNextLogon);
                
                    if (!changeSuccess)
                    {
                        await ShowPasswordFormRecursiveAsync();
                    }
                }
            }
            finally
            {
                if (passwordForm != null)
                    passwordForm.Dispose();
            }
        }

        /// <summary>
        /// 비밀번호 변경 실행
        /// </summary>
        /// <param name="newPassword">새 비밀번호</param>
        /// <param name="forceChangeAtNextLogon">다음 로그인 시 비밀번호 변경 강제 여부</param>
        /// <returns>비밀번호 변경 성공 여부</returns>
        private async Task<bool> ApplyPasswordChangeAsync(string newPassword, bool forceChangeAtNextLogon = true)
        {
            if (_currentUser == null || _isProcessing) return false;

            try
            {
                _isProcessing = true;
                UpdateButtonStates();
                UpdateStatusBar("비밀번호 변경 중...");

                var success = await Task.Run(() => _adServiceExt.ChangeUserPassword(_currentUser.UserId, newPassword, forceChangeAtNextLogon))
                                       .ConfigureAwait(false);

                this.Invoke((Action)(async () =>
                {
                    if (success)
                    {
                        var forceChangeMessage = (forceChangeAtNextLogon 
                            ? "\n\n※ 사용자는 다음 로그인 시 비밀번호를 변경해야 합니다." 
                            : string.Empty);
                        
                        MessageBox.Show(
                            $"사용자 '{_currentUser.UserId}'의 비밀번호가 성공적으로 변경되었습니다.{forceChangeMessage}",
                            "성공",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.None);
                        
                        await RefreshUserInfoAsync(_currentUser.UserId);
                        UpdateStatusBar($"사용자 '{_currentUser.UserId}' 비밀번호 변경 완료");
                    }
                    else
                    {
                        MessageBox.Show("비밀번호 변경에 실패했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateStatusBar("비밀번호 변경 실패");
                    }
                }));

                return success;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("비밀번호가 성공적으로 변경되었습니다"))
            {
                var forceChangeMessage = (forceChangeAtNextLogon 
                    ? "\n\n※ 사용자는 다음 로그인 시 비밀번호를 변경해야 합니다." 
                    : string.Empty);
                
                this.Invoke((Action)(async () =>
                {
                    MessageBox.Show(
                        $"사용자 '{_currentUser.UserId}': {ex.Message}{forceChangeMessage}",
                        "성공",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.None);
                    
                    await RefreshUserInfoAsync(_currentUser.UserId);
                    UpdateStatusBar($"사용자 '{_currentUser.UserId}' 비밀번호 변경 완료 (잠금 해제 포함)");
                }));
                
                return true;
            }
            catch (Exception ex)
            {
                this.Invoke((Action)(() =>
                {
                    ErrorHandler.HandleException(ex, "비밀번호 변경 실패", this);
                    UpdateStatusBar("비밀번호 변경 오류");
                }));
                
                return false;
            }
            finally
            {
                this.Invoke((Action)(() =>
                {
                    _isProcessing = false;
                    UpdateButtonStates();
                }));
            }
        }

        #endregion

        #region Private Methods - Account Management

        /// <summary>
        /// 계정 잠금 해제
        /// </summary>
        private async Task UnlockUserAccountAsync()
        {
            var validationResult = ValidateUserForUnlock();
            if (!validationResult.IsValid)
            {
                MessageBox.Show(validationResult.Message, "정보", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }

            if (_isProcessing) return;

            try
            {
                _isProcessing = true;
                UpdateButtonStates();
                UpdateStatusBar("계정 잠금 해제 중...");

                var success = await Task.Run(() => _adServiceExt.UnlockUserAccount(_currentUser.UserId))
                                       .ConfigureAwait(false);

                this.Invoke((Action)(async () =>
                {
                    if (success)
                    {
                        MessageBox.Show(
                            $"사용자 '{_currentUser.UserId}'의 계정 잠금이 성공적으로 해제되었습니다.",
                            "성공",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.None);
                        
                        await RefreshUserInfoAsync(_currentUser.UserId);
                        UpdateStatusBar($"사용자 '{_currentUser.UserId}' 계정 잠금 해제 완료");
                    }
                    else
                    {
                        MessageBox.Show("계정 잠금 해제에 실패했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateStatusBar("계정 잠금 해제 실패");
                    }
                }));
            }
            catch (Exception ex)
            {
                this.Invoke((Action)(() =>
                {
                    ErrorHandler.HandleException(ex, "계정 잠금 해제 실패", this);
                    UpdateStatusBar("계정 잠금 해제 오류");
                }));
            }
            finally
            {
                this.Invoke((Action)(() =>
                {
                    _isProcessing = false;
                    UpdateButtonStates();
                }));
            }
        }

        /// <summary>
        /// 잠금 해제 가능 여부 검증
        /// </summary>
        private (bool IsValid, string Message) ValidateUserForUnlock()
        {
            if (_currentUser == null)
                return (false, "사용자를 먼저 검색해주세요.");
                
            if (!_currentUser.IsLockedOut)
                return (false, "선택된 사용자의 계정이 잠겨있지 않습니다.");
                
            return (true, string.Empty);
        }

        /// <summary>
        /// 사용자 상세 정보 표시
        /// </summary>
        private async Task ShowUserDetailsAsync()
        {
            if (_currentUser == null)
            {
                MessageBox.Show("사용자를 먼저 검색해주세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }

            try
            {
                UpdateStatusBar("사용자 상세 정보 로드 중...");
                
                var userDetailInfo = await Task.Run(() => _adService.GetUserDetailInfo(_currentUser.UserId))
                                              .ConfigureAwait(false);
                
                this.Invoke((Action)(() =>
                {
                    if (userDetailInfo != null)
                    {
                        var detailForm = new UserDetailForm(userDetailInfo);
                        try
                        {
                            detailForm.ShowDialog(this);
                            UpdateStatusBar($"사용자 '{_currentUser.UserId}' 상세 정보 표시 완료");
                        }
                        finally
                        {
                        if (detailForm != null)
                            detailForm.Dispose();
                        }
                    }
                    else
                    {
                        MessageBox.Show("사용자 상세 정보를 가져올 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.None);
                        UpdateStatusBar("상세 정보 로드 실패");
                    }
                }));
            }
            catch (Exception ex)
            {
                this.Invoke((Action)(() =>
                {
                    ErrorHandler.HandleException(ex, "사용자 상세 정보 로드 실패", this);
                    UpdateStatusBar("상세 정보 로드 오류");
                }));
            }
        }

        /// <summary>
        /// 사용자 자격 증명 검증
        /// </summary>
        private async Task VerifyUserCredentialsAsync()
        {
            if (_currentUser == null)
            {
                MessageBox.Show("사용자를 먼저 검색해주세요.", "정보", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }

            if (_isProcessing) return;

            try
            {
                _isProcessing = true;
                UpdateButtonStates();
                UpdateStatusBar("사용자 자격 증명 검증 중...");

                var password = ShowPasswordInputDialog();
                if (string.IsNullOrEmpty(password))
                {
                    UpdateStatusBar("자격 증명 검증 취소됨");
                    return;
                }

                var isValid = await Task.Run(() => _adService.VerifyUserCredentials(_currentUser.UserId, password))
                                       .ConfigureAwait(false);

                this.Invoke((Action)(() =>
                {
                    string message, title;
                    MessageBoxIcon icon;
                    
                    if (isValid)
                    {
                        message = $"사용자 '{_currentUser.UserId}'의 자격 증명이 유효합니다.";
                        title = "검증 성공";
                        icon = MessageBoxIcon.None;
                    }
                    else
                    {
                        message = $"사용자 '{_currentUser.UserId}'의 자격 증명이 유효하지 않습니다.";
                        title = "검증 실패";
                        icon = MessageBoxIcon.Warning;
                    }

                    MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
                    
                    var statusMessage = (isValid ? "자격 증명 검증 성공" : "자격 증명 검증 실패");
                    UpdateStatusBar($"사용자 '{_currentUser.UserId}' {(statusMessage)}");
                }));
            }
            catch (Exception ex)
            {
                this.Invoke((Action)(() =>
                {
                    ErrorHandler.HandleException(ex, "자격 증명 검증 실패", this);
                    UpdateStatusBar("자격 증명 검증 오류");
                }));
            }
            finally
            {
                this.Invoke((Action)(() =>
                {
                    _isProcessing = false;
                    UpdateButtonStates();
                }));
            }
        }

        #endregion

        #region Private Methods - DSA Management

        /// <summary>
        /// 현재 자격 증명으로 DSA.msc 열기 (CreateProcessWithLogonW API 사용)
        /// 로그인 시 사용한 비밀번호로 자동 실행
        /// </summary>
        private async void OpenDSAWithCurrentCredentials()
        {
            try
            {
                if (_currentCredential == null)
                {
                    MessageBox.Show("현재 자격 증명 정보가 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (_currentCredential.Password == null)
                {
                    MessageBox.Show("저장된 비밀번호 정보가 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 임시 경로에 ClientDevices.msc 추출
                string tempDir = System.IO.Path.GetTempPath();
                string mscPath = System.IO.Path.Combine(tempDir, "ClientDevices.msc");
                
                try 
                {
                    // 항상 최신 버전으로 덮어쓰기
                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    // 리소스 이름 확인 필요 (기본 네임스페이스.폴더.파일)
                    // 프로젝트 이름이 adHelp이고, 루트에 파일을 넣었다면 adHelp.ClientDevices.msc 일 가능성 높음
                    using (var stream = assembly.GetManifestResourceStream("adHelp.ClientDevices.msc"))
                    {
                        if (stream == null)
                        {
                            throw new Exception("Embedded Resource 'adHelp.ClientDevices.msc' not found.");
                        }
                        using (var fileStream = System.IO.File.Create(mscPath))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                     MessageBox.Show($"리소스 추출 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                     return;
                }

                UpdateStatusBar("ClientDevices.msc 실행 중...");

                var domain = _currentCredential.Domain;
                var username = _currentCredential.Username;
                var password = _currentCredential.Password;
                var fullUsername = $"{domain}\\{username}";

                // CreateProcessWithLogonW API를 사용하여 실행
                bool success = await Task.Run(() => 
                    ProcessWithCredentials.StartMscWithCredentials(username, domain, password, mscPath));

                if (success)
                {
                    UpdateStatusBar($"ClientDevices.msc가 '{fullUsername}'로 실행되었습니다.");
                }
                else
                {
                    MessageBox.Show("ClientDevices.msc 실행에 실패했습니다.", "실행 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    UpdateStatusBar("ClientDevices.msc 실행 실패");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "ClientDevices.msc 실행 실패", this);
                UpdateStatusBar("ClientDevices.msc 실행 오류");
            }
        }

        /// <summary>
        /// DSA.msc 설치 여부 확인 (단순 파일 존재 확인만)
        /// </summary>
        /// <returns>설치되어 있으면 true, 아니면 false</returns>
        private async Task<bool> CheckDsaMscInstallationAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // dsa.msc 파일 직접 확인만
                    var dsaMscPath = System.IO.Path.Combine(Environment.SystemDirectory, "dsa.msc");
                    return System.IO.File.Exists(dsaMscPath);
                }
                catch
                {
                    return false;
                }
            });
        }

        #endregion

        #region Private Methods - Group Management

        /// <summary>
        /// 사용자 그룹 정보 로드
        /// </summary>
        private async Task LoadUserGroupsAsync(string userId)
        {
            try
            {
                var groups = await Task.Run(() => _adService.GetUserGroups(userId))
                                      .ConfigureAwait(false);
                
                this.Invoke((Action)(() =>
                {
                    listBoxGroups.Items.Clear();
                    
                    if (groups?.Length > 0)
                    {
                        listBoxGroups.Items.AddRange(groups);
                    }
                    
                    labelGroupsCount.Text = $"그룹 수: {(groups != null ? groups.Length : 0)}";
                }));
            }
            catch (Exception ex)
            {
                this.Invoke((Action)(() =>
                {
                    Debug.WriteLine($"그룹 정보 로드 실패: {ex.Message}");
                    labelGroupsCount.Text = "그룹 수: 로드 실패";
                }));
            }
        }

        /// <summary>
        /// 그룹 목록 복사
        /// </summary>
        private void CopyGroupsToClipboard(object sender, EventArgs e)
        {
            try
            {
                if (listBoxGroups.Items.Count == 0)
                {
                    MessageBox.Show("복사할 그룹 정보가 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.None);
                    return;
                }

                var groupText = string.Join(Environment.NewLine, 
                    listBoxGroups.Items.Cast<string>());

                Clipboard.SetText(groupText);
                UpdateStatusBar($"그룹 목록 {listBoxGroups.Items.Count}개가 클립보드에 복사되었습니다");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "그룹 목록 복사 실패", this);
            }
        }

        #endregion

        #region Event Handler Updates

        /// <summary>
        /// 이벤트 핸들러들을 async 메서드로 업데이트
        /// </summary>
        private async void SearchUser() => await SearchUserAsync();
        private async void RefreshUserInfo(string userId) => await RefreshUserInfoAsync(userId);
        private async void ShowPasswordManager() => await ShowPasswordManagerAsync();
        private async void UnlockUserAccount() => await UnlockUserAccountAsync();
        private async void ShowUserDetails() => await ShowUserDetailsAsync();
        private async void VerifyUserCredentials() => await VerifyUserCredentialsAsync();

        #endregion
    }
}
