using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using adHelp.Models;

namespace adHelp.Forms
{
    /// <summary>
    /// 사용자 상세 정보를 표시하는 폼
    /// Get-ADUser -Properties * 결과와 같은 모든 AD 정보 표시
    /// 키보드 단축키 지원: Ctrl+C (복사), Ctrl+A (전체선택)
    /// </summary>
    public partial class UserDetailForm : Form
    {
        private UserDetailInfo _userDetailInfo;

        /// <summary>
        /// 생성자 (사용자 ID와 ADService로 상세 정보 조회)
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="adService">AD 서비스</param>
        public UserDetailForm(string userId, Services.ADService adService)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("사용자 ID는 필수입니다.", nameof(userId));
            if (adService == null)
                throw new ArgumentNullException(nameof(adService));

            try
            {
                // ADService를 통해 사용자 상세 정보 조회
                _userDetailInfo = adService.GetUserDetailInfo(userId);
                if (_userDetailInfo == null)
                {
                    throw new InvalidOperationException($"사용자 '{userId}'를 찾을 수 없습니다.");
                }

                InitializeComponent();
                InitializeLayout();
                LoadUserDetailInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"사용자 상세 정보 조회 중 오류가 발생했습니다:\n{ex.Message}", 
                               "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="userDetailInfo">사용자 상세 정보</param>
        public UserDetailForm(UserDetailInfo userDetailInfo)
        {
            _userDetailInfo = userDetailInfo ?? throw new ArgumentNullException(nameof(userDetailInfo));
            InitializeComponent();
            InitializeLayout();
            LoadUserDetailInfo();
        }

        /// <summary>
        /// 레이아웃 및 이벤트 초기화
        /// </summary>
        private void InitializeLayout()
        {
            // 폼 제목에 사용자 ID 추가
            this.Text = $"사용자 상세 정보 - {_userDetailInfo.BasicInfo?.UserId ?? "Unknown"}";
            
            // 아이콘 설정
            try
            {
                this.Icon = Properties.Resources.ad192_icon;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UserDetailForm 아이콘 설정 오류: {ex.Message}");
            }

            // 이벤트 핸들러 연결
            this.buttonClose.Click += ButtonClose_Click;
            this.buttonCopyAll.Click += ButtonCopyAll_Click;
            this.Resize += UserDetailForm_Resize;
            this.Load += UserDetailForm_Load; // Load 이벤트 추가
            
            // TabControl 이벤트 연결
            if (this.tabControl != null)
            {
                this.tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            }

            // 버튼 위치 조정
            AdjustButtonPositions();

            // 툴팁 설정
            var toolTip = new ToolTip();
            toolTip.SetToolTip(this.buttonCopyAll, "모든 정보를 클립보드에 복사합니다 (복사 완료 메시지 표시)");
        }

        /// <summary>
        /// 버튼 위치 조정
        /// </summary>
        private void AdjustButtonPositions()
        {
            if (this.buttonClose != null && this.buttonCopyAll != null && this.buttonPanel != null)
            {
                // 버튼 패널 내에서의 상대적 위치 계산
                int panelWidth = this.buttonPanel.ClientSize.Width;
                int rightMargin = 20;
                
                this.buttonClose.Location = new Point(panelWidth - this.buttonClose.Width - rightMargin, 15);
                this.buttonCopyAll.Location = new Point(panelWidth - this.buttonClose.Width - this.buttonCopyAll.Width - rightMargin - 10, 15);
            }
        }

        /// <summary>
        /// 폼 크기 변경 시 버튼 위치 조정 및 컬럼 크기 최적화
        /// </summary>
        private void UserDetailForm_Resize(object sender, EventArgs e)
        {
            AdjustButtonPositions();
            
            // 탭 컨트롤 내의 모든 ListView의 컬럼 크기 재조정
            OptimizeAllListViewColumns();
        }

        /// <summary>
        /// 폼 로드 완료 시 컬럼 크기 초기 설정
        /// </summary>
        private void UserDetailForm_Load(object sender, EventArgs e)
        {
            // 폼이 완전히 로드된 후 컬럼 크기 조정
            OptimizeAllListViewColumns();
        }

        /// <summary>
        /// 탭 선택 변경 시 컬럼 크기 조정
        /// </summary>
        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("탭 선택 변경 이벤트 발생 - 컬럼 크기 조정 시도");
            OptimizeAllListViewColumns();
        }

        /// <summary>
        /// 사용자 상세 정보 로드
        /// </summary>
        private void LoadUserDetailInfo()
        {
            try
            {
                var allPropsTab = CreateAllPropertiesTab();
                tabControl.TabPages.Add(allPropsTab);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"사용자 정보 로드 중 오류가 발생했습니다:\\n{ex.Message}", 
                               "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 모든 속성 탭 생성
        /// </summary>
        private TabPage CreateAllPropertiesTab()
        {
            var tab = new TabPage("📊 모든 속성");

            var listView = new ListView();
            listView.Dock = DockStyle.Fill;
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.MultiSelect = true;
            listView.Sorting = SortOrder.Ascending;

            // 키보드 이벤트 핸들러
            listView.KeyDown += ListView_KeyDown;
            
            // 컨텍스트 메뉴 추가
            var contextMenu = new ContextMenuStrip();
            var copySelectedItem = new ToolStripMenuItem("선택 항목 복사 (Ctrl+C)");
            var selectAllItem = new ToolStripMenuItem("전체 선택 (Ctrl+A)");
            var copyAllItem = new ToolStripMenuItem("전체 복사");
            
            copySelectedItem.Click += (s, e) => CopyListViewItems(listView, false, false);
            selectAllItem.Click += (s, e) => SelectAllItems(listView);
            copyAllItem.Click += (s, e) => CopyListViewItems(listView, true, false);
            
            contextMenu.Items.Add(copySelectedItem);
            contextMenu.Items.Add(selectAllItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(copyAllItem);
            listView.ContextMenuStrip = contextMenu;

            // 컬럼 추가 (초기 크기는 임시)
            listView.Columns.Add("속성 이름", 100);
            listView.Columns.Add("값", 100);

            // 모든 속성 추가
            foreach (var prop in _userDetailInfo.AllProperties)
            {
                var item = new ListViewItem(prop.Key);
                item.SubItems.Add(_userDetailInfo.GetPropertySafely(prop.Key));
                listView.Items.Add(item);
            }

            // 탭에 ListView 추가 (폼 로드 시 컬럼 크기 자동 조정됨)
            tab.Controls.Add(listView);
            
            // 탭이 선택될 때 컬럼 크기 조정을 위해 이벤트 연결
            tab.Enter += (s, e) => 
            {
                System.Diagnostics.Debug.WriteLine("탭 선택 이벤트 발생 - 컬럼 크기 조정 시도");
                OptimizeColumnWidths(listView);
            };
            return tab;
        }

        /// <summary>
        /// ListView 키보드 이벤트 처리
        /// </summary>
        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null) return;

            if (e.Control && e.KeyCode == Keys.C)
            {
                CopyListViewItems(listView, false, false);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                SelectAllItems(listView);
                e.Handled = true;
            }
        }

        /// <summary>
        /// ListView의 모든 항목 선택
        /// </summary>
        private void SelectAllItems(ListView listView)
        {
            try
            {
                listView.BeginUpdate();
                foreach (ListViewItem item in listView.Items)
                {
                    item.Selected = true;
                }
                listView.EndUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"전체 선택 중 오류가 발생했습니다:\\n{ex.Message}", 
                               "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 닫기 버튼 클릭
        /// </summary>
        private void ButtonClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 전체 복사 버튼 클릭
        /// </summary>
        private void ButtonCopyAll_Click(object sender, EventArgs e)
        {
            try
            {
                var content = GenerateCopyContent();
                Clipboard.SetText(content);
                MessageBox.Show("모든 정보가 클립보드에 복사되었습니다.", "복사 완료", 
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"복사 중 오류가 발생했습니다:\\n{ex.Message}", 
                               "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// ListView 항목들을 클립보드에 복사
        /// </summary>
        private void CopyListViewItems(ListView listView, bool copyAll, bool showMessage = true)
        {
            try
            {
                var content = new System.Text.StringBuilder();
                var items = copyAll ? listView.Items.Cast<ListViewItem>() : listView.SelectedItems.Cast<ListViewItem>();
                
                foreach (ListViewItem item in items)
                {
                    var line = new System.Text.StringBuilder();
                    line.Append(item.Text);
                    
                    foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                    {
                        if (subItem != item.SubItems[0])
                        {
                            line.Append("\t");
                            line.Append(subItem.Text);
                        }
                    }
                    content.AppendLine(line.ToString());
                }
                
                if (content.Length > 0)
                {
                    Clipboard.SetText(content.ToString());
                    
                    if (showMessage)
                    {
                        var message = copyAll ? "전체 항목이" : $"{items.Count()}개 항목이";
                        MessageBox.Show($"{message} 클립보드에 복사되었습니다.", "복사 완료", 
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("복사할 항목이 없습니다.", "정보", 
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"복사 중 오류가 발생했습니다:\\n{ex.Message}", 
                               "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 복사용 콘텐츠 생성
        /// </summary>
        private string GenerateCopyContent()
        {
            var content = new System.Text.StringBuilder();
            
            content.AppendLine("=====================================================");
            content.AppendLine($"AD Helper - 사용자 상세 정보");
            content.AppendLine($"복사 시간: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            content.AppendLine("=====================================================");
            content.AppendLine();

            // 기본 정보
            var user = _userDetailInfo.BasicInfo;
            if (user != null)
            {
                content.AppendLine("■ 기본 정보");
                content.AppendLine($"  사용자 ID: {user.UserId}");
                content.AppendLine($"  표시 이름: {user.DisplayName}");
                content.AppendLine($"  전체 이름: {user.FullName}");
                content.AppendLine($"  이메일: {user.Email}");
                content.AppendLine($"  부서: {user.Department}");
                content.AppendLine($"  직책: {user.Title}");
                content.AppendLine();
            }

            // 모든 속성 복사
            content.AppendLine("■ 모든 AD 속성");
            foreach (var prop in _userDetailInfo.AllProperties)
            {
                var value = _userDetailInfo.GetPropertySafely(prop.Key);
                if (!string.IsNullOrEmpty(value))
                {
                    content.AppendLine($"  {prop.Key}: {value}");
                }
            }

            return content.ToString();
        }

        /// <summary>
        /// 모든 ListView의 컬럼 크기 최적화
        /// 탭 컨트롤 내의 모든 ListView를 찾아서 컬럼 크기를 재조정
        /// </summary>
        private void OptimizeAllListViewColumns()
        {
            try
            {
                if (tabControl?.TabPages != null)
                {
                    foreach (TabPage tabPage in tabControl.TabPages)
                    {
                        foreach (Control control in tabPage.Controls)
                        {
                            if (control is ListView listView)
                            {
                                OptimizeColumnWidths(listView);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OptimizeAllListViewColumns 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// ListView 컬럼 크기 최적화
        /// 현재 가로 폭에 맞춰 2:8 비율로 고정 설정 (속성명 20%, 값 80%)
        /// </summary>
        /// <param name="listView">최적화할 ListView</param>
        private void OptimizeColumnWidths(ListView listView)
        {
            if (listView.Items.Count == 0 || listView.Columns.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"OptimizeColumnWidths 스킵: 데이터 없음 - Items: {listView.Items.Count}, Columns: {listView.Columns.Count}");
                return;
            }

            try
            {
                // ListView의 사용 가능한 너비 계산
                int clientWidth = listView.ClientSize.Width;
                int scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
                int borderAndMargin = 10; // 여백
                int availableWidth = clientWidth - scrollBarWidth - borderAndMargin;
                
                System.Diagnostics.Debug.WriteLine($"ListView 크기 정보 - ClientWidth: {clientWidth}px, ScrollBarWidth: {scrollBarWidth}px, AvailableWidth: {availableWidth}px");
                
                if (availableWidth <= 100) // 최소한의 공간 확보
                {
                    System.Diagnostics.Debug.WriteLine($"OptimizeColumnWidths 스킵: 사용 가능 너비 부족 - {availableWidth}px");
                    return;
                }

                // 2:8 비율로 고정 설정
                int propertyNameWidth = (int)(availableWidth * 0.2); // 20%
                int valueWidth = (int)(availableWidth * 0.8);        // 80%
                
                System.Diagnostics.Debug.WriteLine($"계산된 컬럼 크기 - 속성명: {propertyNameWidth}px (20%), 값: {valueWidth}px (80%)");
                
                // 최소 크기 보장
                int originalPropertyNameWidth = propertyNameWidth;
                int originalValueWidth = valueWidth;
                propertyNameWidth = Math.Max(propertyNameWidth, 100);
                valueWidth = Math.Max(valueWidth, 200);
                
                if (originalPropertyNameWidth != propertyNameWidth || originalValueWidth != valueWidth)
                {
                    System.Diagnostics.Debug.WriteLine($"최소 크기 적용 - 속성명: {originalPropertyNameWidth} -> {propertyNameWidth}px, 값: {originalValueWidth} -> {valueWidth}px");
                }
                
                // 전체 크기가 초과하지 않도록 조정
                int totalWidth = propertyNameWidth + valueWidth;
                if (totalWidth > availableWidth)
                {
                    System.Diagnostics.Debug.WriteLine($"전체 크기 초과 - 총 크기: {totalWidth}px, 사용가능: {availableWidth}px");
                    
                    // 비율 유지하면서 축소
                    float scale = (float)availableWidth / totalWidth;
                    int newPropertyNameWidth = (int)(propertyNameWidth * scale);
                    int newValueWidth = availableWidth - newPropertyNameWidth;
                    
                    System.Diagnostics.Debug.WriteLine($"비율 축소 적용 - Scale: {scale:F2}, 속성명: {propertyNameWidth} -> {newPropertyNameWidth}px, 값: {valueWidth} -> {newValueWidth}px");
                    
                    propertyNameWidth = newPropertyNameWidth;
                    valueWidth = newValueWidth;
                }
                
                // 컬럼 크기 적용
                System.Diagnostics.Debug.WriteLine($"최종 컬럼 크기 적용 - 속성명: {propertyNameWidth}px, 값: {valueWidth}px");
                
                listView.BeginUpdate();
                listView.Columns[0].Width = propertyNameWidth;
                listView.Columns[1].Width = valueWidth;
                listView.EndUpdate();
                
                // 전체 성공 로그
                System.Diagnostics.Debug.WriteLine($"UserDetailForm 컬럼 크기 2:8 비율 성공 - 사용가능: {availableWidth}px, 속성: {propertyNameWidth}px, 값: {valueWidth}px");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OptimizeColumnWidths 오류: {ex.Message}");
                
                // 오류 발생 시 안전한 기본값 적용 (2:8 비율)
                try
                {
                    int safeWidth = Math.Max(400, listView.ClientSize.Width - 50);
                    int safePropertyWidth = (int)(safeWidth * 0.2);
                    int safeValueWidth = (int)(safeWidth * 0.8);
                    
                    System.Diagnostics.Debug.WriteLine($"안전 모드 적용 - SafeWidth: {safeWidth}px, 속성: {safePropertyWidth}px, 값: {safeValueWidth}px");
                    
                    listView.BeginUpdate();
                    listView.Columns[0].Width = safePropertyWidth; // 20%
                    listView.Columns[1].Width = safeValueWidth; // 80%
                    listView.EndUpdate();
                }
                catch (Exception safeEx)
                {
                    System.Diagnostics.Debug.WriteLine($"안전 모드 오류: {safeEx.Message}");
                    // 최후의 안전장치
                }
            }
        }
    }
}
