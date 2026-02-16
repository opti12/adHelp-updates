using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using adHelp.Models;
using adHelp.Services;
using adHelp.Utils;

namespace adHelp.Forms
{
    /// <summary>
    /// 그룹 상세 정보 표시 폼
    /// </summary>
    public partial class GroupDetailForm : Form
    {
        private readonly ADService _adService;
        private readonly string _groupName;
        private Dictionary<string, object> _groupInfo;
        private List<GroupMemberInfo> _groupMembers;

        public GroupDetailForm(ADService adService, string groupName)
        {
            _adService = adService ?? throw new ArgumentNullException(nameof(adService));
            _groupName = groupName ?? throw new ArgumentNullException(nameof(groupName));
            
            InitializeComponent();
            InitializeForm();
        }

        /// <summary>
        /// 폼 초기화
        /// </summary>
        private void InitializeForm()
        {
            try
            {
                // 아이콘 설정
                this.Icon = Properties.Resources.ad192_icon;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GroupDetailForm 아이콘 설정 오류: {ex.Message}");
            }

            // 폼 제목 설정
            this.Text = $"그룹 정보 - {_groupName}";
            
            // 데이터 그리드 설정
            SetupDataGridView();
            
            // 폼이 완전히 표시된 후에 데이터 로드 (핸들 생성 보장)
            this.Shown += async (sender, e) => 
            {
                // 폼이 완전히 표시된 후 약간의 지연을 두고 데이터 로드 시작
                await Task.Delay(100);
                await LoadGroupDataAsync();
            };
        }

        /// <summary>
        /// DataGridView 초기 설정 (컬럼 폭 자동 조정 지원)
        /// </summary>
        private void SetupDataGridView()
        {
            // 구성원 목록 DataGridView 설정
            dataGridViewMembers.AllowUserToAddRows = false;
            dataGridViewMembers.AllowUserToDeleteRows = false;
            dataGridViewMembers.ReadOnly = true;
            dataGridViewMembers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewMembers.MultiSelect = false;
            dataGridViewMembers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridViewMembers.RowHeadersVisible = false;
            dataGridViewMembers.AllowUserToResizeColumns = true; // 사용자 수동 조정 허용
            
            // 컬럼 설정 (자동 크기 조정 지원)
            dataGridViewMembers.Columns.Clear();
            
            // 표시명 컬럼
            var displayNameColumn = new DataGridViewTextBoxColumn
            {
                Name = "DisplayName",
                HeaderText = "표시명",
                DataPropertyName = "DisplayName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                MinimumWidth = 100,
                Resizable = DataGridViewTriState.True
            };
            dataGridViewMembers.Columns.Add(displayNameColumn);
            
            // 계정명 컬럼
            var accountColumn = new DataGridViewTextBoxColumn
            {
                Name = "SAMAccountName", 
                HeaderText = "계정명",
                DataPropertyName = "SAMAccountName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                MinimumWidth = 80,
                Resizable = DataGridViewTriState.True
            };
            dataGridViewMembers.Columns.Add(accountColumn);
            
            // 유형 컬럼
            var typeColumn = new DataGridViewTextBoxColumn
            {
                Name = "Type",
                HeaderText = "유형", 
                DataPropertyName = "Type",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                MinimumWidth = 50,
                Resizable = DataGridViewTriState.True
            };
            dataGridViewMembers.Columns.Add(typeColumn);
            
            // 이메일 컬럼
            var emailColumn = new DataGridViewTextBoxColumn
            {
                Name = "Email",
                HeaderText = "이메일",
                DataPropertyName = "Email", 
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                MinimumWidth = 120,
                Resizable = DataGridViewTriState.True
            };
            dataGridViewMembers.Columns.Add(emailColumn);
            
            // 활성화 상태 컬럼
            var enabledColumn = new DataGridViewTextBoxColumn
            {
                Name = "Enabled",
                HeaderText = "활성화",
                DataPropertyName = "Enabled",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                MinimumWidth = 50,
                Resizable = DataGridViewTriState.True
            };
            dataGridViewMembers.Columns.Add(enabledColumn);
            
            // DistinguishedName 컬럼 (다른 컬럼 조정 후 남은 공간으로 채움)
            var dnColumn = new DataGridViewTextBoxColumn
            {
                Name = "DistinguishedName",
                HeaderText = "DistinguishedName",
                DataPropertyName = "DistinguishedName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, // 남은 공간 채움
                MinimumWidth = 150,
                Resizable = DataGridViewTriState.True,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Consolas", 8, FontStyle.Regular),
                    ForeColor = Color.Gray
                }
            };
            dataGridViewMembers.Columns.Add(dnColumn);
        }

        /// <summary>
        /// 그룹 데이터 비동기 로드 (개선된 오류 처리 및 디버깅)
        /// </summary>
        private async Task LoadGroupDataAsync()
        {
            try
            {
                SimpleLogger.Log($"GroupDetailForm: LoadGroupDataAsync 시작 - 그룹명: '{_groupName}'");
                
                // UI 업데이트 - 로딩 시작 (핸들 체크 추가)
                await SafeInvokeAsync(() =>
                {
                    labelStatus.Text = "그룹 정보를 로드하는 중...";
                    labelStatus.ForeColor = Color.Blue;
                    buttonRefresh.Enabled = false;
                });

                // 1단계: 그룹 기본 정보 조회
                SimpleLogger.Log($"GroupDetailForm: 그룹 기본 정보 조회 시작");
                _groupInfo = await Task.Run(() => _adService.GetGroupInfo(_groupName));
                
                if (_groupInfo == null)
                {
                    SimpleLogger.Log($"GroupDetailForm: 그룹 '{_groupName}'을 찾을 수 없음");
                    await SafeInvokeAsync(() =>
                    {
                        labelStatus.Text = $"그룹 '{_groupName}'을 찾을 수 없습니다.";
                        labelStatus.ForeColor = Color.Red;
                        buttonRefresh.Enabled = true;
                    });
                    return;
                }

                SimpleLogger.Log($"GroupDetailForm: 그룹 기본 정보 조회 성공");
                
                // 2단계: 그룹 구성원 조회
                await SafeInvokeAsync(() =>
                {
                    labelStatus.Text = "그룹 구성원을 조회하는 중...";
                    labelStatus.ForeColor = Color.Blue;
                });
                
                SimpleLogger.Log($"GroupDetailForm: 그룹 구성원 조회 시작");
                _groupMembers = await Task.Run(() => _adService.GetGroupMembers(_groupName));
                
                SimpleLogger.Log($"GroupDetailForm: 그룹 구성원 조회 완료 - {_groupMembers?.Count ?? 0}명");

                // 3단계: UI 업데이트
                await SafeInvokeAsync(() =>
                {
                    try
                    {
                        DisplayGroupInfo();
                        DisplayGroupMembers();
                        
                        int memberCount = _groupMembers?.Count ?? 0;
                        if (memberCount == 0)
                        {
                            labelStatus.Text = "그룹 정보 로드 완료 - 구성원이 없습니다.";
                            labelStatus.ForeColor = Color.Orange;
                        }
                        else
                        {
                            labelStatus.Text = $"로드 완료 - 구성원 {memberCount}명";
                            labelStatus.ForeColor = Color.Green;
                        }
                        
                        buttonRefresh.Enabled = true;
                        SimpleLogger.Log($"GroupDetailForm: UI 업데이트 완료");
                    }
                    catch (Exception uiEx)
                    {
                        SimpleLogger.Log($"GroupDetailForm: UI 업데이트 오류: {uiEx.Message}");
                        labelStatus.Text = $"UI 업데이트 실패: {uiEx.Message}";
                        labelStatus.ForeColor = Color.Red;
                        buttonRefresh.Enabled = true;
                    }
                });
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"GroupDetailForm: LoadGroupDataAsync 예외 발생: {ex.Message}");
                SimpleLogger.Log($"GroupDetailForm: Stack Trace: {ex.StackTrace}");
                
                await SafeInvokeAsync(() =>
                {
                    string errorMessage = $"그룹 정보 로드 실패: {ex.Message}";
                    labelStatus.Text = errorMessage;
                    labelStatus.ForeColor = Color.Red;
                    buttonRefresh.Enabled = true;
                    
                    // 사용자에게 더 자세한 오류 정보 제공
                    string detailedError = $"그룹 '{_groupName}'의 정보를 로드하는 중 오류가 발생했습니다.\n\n" +
                                          $"오류 내용: {ex.Message}\n\n" +
                                          $"가능한 원인:\n" +
                                          $"• 그룹명이 정확하지 않음\n" +
                                          $"• 그룹 정보 조회 권한 부족\n" +
                                          $"• 네트워크 연결 문제\n" +
                                          $"• Active Directory 서버 문제";
                    
                    ErrorHandler.HandleException(ex, "그룹 정보 로드 오류", this);
                });
            }
        }

        /// <summary>
        /// 그룹 기본 정보 표시
        /// </summary>
        private void DisplayGroupInfo()
        {
            if (_groupInfo == null) return;

            try
            {
                // 그룹명
                labelGroupNameValue.Text = GetGroupProperty("name") ?? GetGroupProperty("displayName") ?? _groupName;
                
                // 설명
                labelDescriptionValue.Text = GetGroupProperty("description") ?? "-";
                
                // 구분된 이름
                labelDistinguishedNameValue.Text = GetGroupProperty("distinguishedName") ?? "-";
                
                // 생성일
                string createdDate = GetGroupProperty("whenCreated");
                labelCreatedValue.Text = FormatDateTime(createdDate);
                
                // 수정일
                string modifiedDate = GetGroupProperty("whenChanged"); 
                labelModifiedValue.Text = FormatDateTime(modifiedDate);
                
                // 그룹 유형
                string groupType = GetGroupProperty("groupType");
                labelGroupTypeValue.Text = FormatGroupType(groupType);
                
                // 이메일
                labelEmailValue.Text = GetGroupProperty("mail") ?? "-";
                
                // 관리자
                labelManagedByValue.Text = GetGroupProperty("managedBy") ?? "-";
                
                // GUID
                string guid = GetGroupProperty("objectGUID");
                labelGuidValue.Text = FormatGuid(guid);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"GroupDetailForm: DisplayGroupInfo 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 그룹 구성원 표시 (컬럼 폭 자동 조정)
        /// </summary>
        private void DisplayGroupMembers()
        {
            try
            {
                if (_groupMembers == null)
                {
                    SimpleLogger.Log($"GroupDetailForm: _groupMembers가 null입니다.");
                    labelMembersCountValue.Text = "데이터 없음";
                    dataGridViewMembers.DataSource = null;
                    return;
                }

                SimpleLogger.Log($"GroupDetailForm: DisplayGroupMembers - {_groupMembers.Count}명의 구성원 표시 시작");
                
                // DataGridView에 바인딩
                dataGridViewMembers.DataSource = null; // 기존 바인딩 제거
                dataGridViewMembers.DataSource = _groupMembers;
                
                // 구성원 수 표시
                labelMembersCountValue.Text = _groupMembers.Count.ToString();
                
                // 컬럼 폭 자동 조정 (데이터 바인딩 후 실행)
                if (_groupMembers.Count > 0)
                {
                    AutoResizeColumns();
                }
                
                SimpleLogger.Log($"GroupDetailForm: DisplayGroupMembers 완료 - {_groupMembers.Count}명 표시됨");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"GroupDetailForm: DisplayGroupMembers 오류: {ex.Message}");
                SimpleLogger.Log($"GroupDetailForm: DisplayGroupMembers Stack Trace: {ex.StackTrace}");
                labelMembersCountValue.Text = "표시 실패";
                
                // 오류 발생 시 빈 데이터 표시
                dataGridViewMembers.DataSource = new List<GroupMemberInfo>();
            }
        }

        /// <summary>
        /// 컬럼 폭 자동 조정 (내용에 따른 최적화)
        /// </summary>
        private void AutoResizeColumns()
        {
            try
            {
                SimpleLogger.Log("GroupDetailForm: AutoResizeColumns 시작");
                
                // 먼저 DistinguishedName 컬럼을 제외한 다른 모든 컬럼들을 내용에 맞게 자동 조정
                foreach (DataGridViewColumn column in dataGridViewMembers.Columns)
                {
                    if (column.Name != "DistinguishedName")
                    {
                        // 내용에 맞는 최소 폭으로 조정
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    }
                }
                
                // 자동 크기 조정 수행
                dataGridViewMembers.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                
                // 자동 조정 후 DistinguishedName 컬럼은 Fill 모드로 남은 공간 채움
                var dnColumn = dataGridViewMembers.Columns["DistinguishedName"];
                if (dnColumn != null)
                {
                    dnColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                
                // 자동 조정이 완료된 후에는 사용자가 수동으로 조정할 수 있도록 모드 변경
                foreach (DataGridViewColumn column in dataGridViewMembers.Columns)
                {
                    if (column.Name != "DistinguishedName")
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    }
                }
                
                SimpleLogger.Log("GroupDetailForm: AutoResizeColumns 완료");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"GroupDetailForm: AutoResizeColumns 오류: {ex.Message}");
            }
        }

        #region Helper Methods

        /// <summary>
        /// 그룹 속성 값 가져오기
        /// </summary>
        private string GetGroupProperty(string propertyName)
        {
            if (_groupInfo == null || !_groupInfo.ContainsKey(propertyName))
                return null;

            var value = _groupInfo[propertyName];
            if (value == null)
                return null;
                
            if (value is object[] array && array.Length > 0)
                return array[0]?.ToString();
                
            return value.ToString();
        }

        /// <summary>
        /// DateTime 포맷팅
        /// </summary>
        private string FormatDateTime(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
                return "-";

            try
            {
                if (DateTime.TryParse(dateTimeString, out DateTime dateTime))
                {
                    return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch
            {
                // 파싱 실패 시 원본 문자열 반환
            }
            
            return dateTimeString;
        }

        /// <summary>
        /// 그룹 유형 포맷팅
        /// </summary>
        private string FormatGroupType(string groupTypeString)
        {
            if (string.IsNullOrEmpty(groupTypeString))
                return "-";

            try
            {
                if (int.TryParse(groupTypeString, out int groupType))
                {
                    // AD GroupType 플래그 해석
                    var types = new List<string>();
                    
                    if ((groupType & 0x00000001) != 0) types.Add("Built-in");
                    if ((groupType & 0x00000002) != 0) types.Add("Global");
                    if ((groupType & 0x00000004) != 0) types.Add("Domain Local");
                    if ((groupType & 0x00000008) != 0) types.Add("Universal");
                    if ((groupType & 0x80000000) != 0) types.Add("Security");
                    else types.Add("Distribution");
                    
                    return string.Join(", ", types);
                }
            }
            catch
            {
                // 파싱 실패 시 원본 문자열 반환
            }
            
            return groupTypeString;
        }

        /// <summary>
        /// GUID 포맷팅
        /// </summary>
        private string FormatGuid(string guidString)
        {
            if (string.IsNullOrEmpty(guidString))
                return "-";

            try
            {
                if (Guid.TryParse(guidString, out Guid guid))
                {
                    return guid.ToString("D").ToUpper();
                }
            }
            catch
            {
                // 파싱 실패 시 원본 문자열 반환
            }
            
            return guidString;
        }

        #endregion

        #region Safe UI Threading

        /// <summary>
        /// 안전한 UI 스레드 호출 (개선된 핸들 검사 및 재시도 로직)
        /// </summary>
        /// <param name="action">실행할 액션</param>
        private async Task SafeInvokeAsync(Action action)
        {
            if (this.IsDisposed || this.Disposing)
            {
                SimpleLogger.Log("GroupDetailForm: 폼이 이미 해제되어 Invoke를 건너뜁니다.");
                return;
            }

            try
            {
                if (this.InvokeRequired)
                {
                    // 핸들이 생성되었는지 확인
                    if (this.IsHandleCreated)
                    {
                        this.Invoke(action);
                        return;
                    }
                    
                    // 핸들이 생성될 때까지 최대 3초간 대기 (AD 서버 지연 대응)
                    int maxRetries = 30; // 30 * 100ms = 3초
                    int retryCount = 0;
                    
                    SimpleLogger.Log("GroupDetailForm: 핸들이 아직 생성되지 않아 대기를 시작합니다.");
                    
                    while (retryCount < maxRetries && !this.IsDisposed && !this.Disposing)
                    {
                        await Task.Delay(100); // 100ms 대기
                        retryCount++;
                        
                        if (this.IsHandleCreated)
                        {
                            SimpleLogger.Log($"GroupDetailForm: 핸들 생성 완료 ({retryCount * 100}ms 후)");
                            this.Invoke(action);
                            return;
                        }
                        
                        // 1초마다 진행 상황 로그 출력
                        if (retryCount % 10 == 0)
                        {
                            SimpleLogger.Log($"GroupDetailForm: 핸들 생성 대기 중... ({retryCount * 100}ms)");
                        }
                    }
                    
                    // 최대 대기 시간 초과
                    if (this.IsDisposed || this.Disposing)
                    {
                        SimpleLogger.Log("GroupDetailForm: 대기 중 폼이 해제되었습니다.");
                    }
                    else
                    {
                        SimpleLogger.Log($"GroupDetailForm: 핸들 생성 대기 시간 초과 ({maxRetries * 100}ms)");
                    }
                }
                else
                {
                    // 이미 UI 스레드에서 실행 중
                    action();
                }
            }
            catch (ObjectDisposedException)
            {
                SimpleLogger.Log("GroupDetailForm: 폼이 해제된 상태에서 Invoke 시도");
            }
            catch (InvalidOperationException ex)
            {
                SimpleLogger.Log($"GroupDetailForm: Invoke 오류: {ex.Message}");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"GroupDetailForm: SafeInvokeAsync 예상치 못한 오류: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// 컬럼 폭 재조정 (새로고침 때 사용)
        /// </summary>
        private void RefreshColumnWidths()
        {
            try
            {
                if (dataGridViewMembers.DataSource != null && dataGridViewMembers.Rows.Count > 0)
                {
                    AutoResizeColumns();
                    SimpleLogger.Log("GroupDetailForm: 컬럼 폭 재조정 완료");
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"GroupDetailForm: RefreshColumnWidths 오류: {ex.Message}");
            }
        }

        #region Event Handlers

        /// <summary>
        /// 닫기 버튼 클릭
        /// </summary>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 새로고침 버튼 클릭
        /// </summary>
        private async void buttonRefresh_Click(object sender, EventArgs e)
        {
            await LoadGroupDataAsync();
            
            // 데이터 로드 후 컬럼 폭 재조정
            if (dataGridViewMembers.Rows.Count > 0)
            {
                RefreshColumnWidths();
            }
        }

        /// <summary>
        /// 구성원 더블클릭 - 사용자인 경우 상세 정보 표시
        /// </summary>
        private void dataGridViewMembers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridViewMembers.Rows.Count)
                return;

            try
            {
                var row = dataGridViewMembers.Rows[e.RowIndex];
                var memberInfo = row.DataBoundItem as GroupMemberInfo;
                
                if (memberInfo == null)
                    return;
                
                if (memberInfo.Type == "사용자" && !string.IsNullOrEmpty(memberInfo.SAMAccountName))
                {
                    // 사용자 상세 정보 폼 표시
                    var userDetailForm = new UserDetailForm(memberInfo.SAMAccountName, _adService);
                    userDetailForm.ShowDialog(this);
                }
                else if (memberInfo.Type == "그룹" && !string.IsNullOrEmpty(memberInfo.SAMAccountName))
                {
                    // 중첩 그룹인 경우 새로운 그룹 상세 폼 표시
                    string groupName = memberInfo.DisplayName ?? memberInfo.SAMAccountName;
                    var groupDetailForm = new GroupDetailForm(_adService, groupName);
                    groupDetailForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, "구성원 정보 표시 오류", this);
            }
        }

        #endregion
    }
}
