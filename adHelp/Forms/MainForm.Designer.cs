using System;
using System.Drawing;
using System.Windows.Forms;

namespace adHelp.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBoxSearch = new System.Windows.Forms.GroupBox();
            this.labelConnectionStatus = new System.Windows.Forms.Label();
            this.labelUserId = new System.Windows.Forms.Label();
            this.textBoxUserId = new System.Windows.Forms.TextBox();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.buttonVerify = new System.Windows.Forms.Button();
            this.buttonOpenDSA = new System.Windows.Forms.Button();
            this.labelAccountSummary = new System.Windows.Forms.Label();
            this.textBoxAccountSummary = new System.Windows.Forms.TextBox();
            this.groupBoxUserInfo = new System.Windows.Forms.GroupBox();
            this.labelUserIdInfo = new System.Windows.Forms.Label();
            this.labelUserIdValue = new System.Windows.Forms.Label();
            this.labelDisplayName = new System.Windows.Forms.Label();
            this.labelDisplayNameValue = new System.Windows.Forms.Label();
            this.labelEmail = new System.Windows.Forms.Label();
            this.labelEmailValue = new System.Windows.Forms.Label();
            this.labelDepartment = new System.Windows.Forms.Label();
            this.labelDepartmentValue = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelTitleValue = new System.Windows.Forms.Label();
            this.labelAccountStatus = new System.Windows.Forms.Label();
            this.labelAccountStatusValue = new System.Windows.Forms.Label();
            this.labelLastLogon = new System.Windows.Forms.Label();
            this.labelLastLogonValue = new System.Windows.Forms.Label();
            this.labelPasswordLastSet = new System.Windows.Forms.Label();
            this.labelPasswordLastSetValue = new System.Windows.Forms.Label();
            this.labelPasswordExpiry = new System.Windows.Forms.Label();
            this.labelPasswordExpiryValue = new System.Windows.Forms.Label();
            this.labelPasswordCanChange = new System.Windows.Forms.Label();
            this.labelPasswordCanChangeValue = new System.Windows.Forms.Label();
            this.labelPasswordStatus = new System.Windows.Forms.Label();
            this.labelPasswordStatusValue = new System.Windows.Forms.Label();
            this.labelBadPasswordCount = new System.Windows.Forms.Label();
            this.labelBadPasswordCountValue = new System.Windows.Forms.Label();
            this.labelLockoutTime = new System.Windows.Forms.Label();
            this.labelLockoutTimeValue = new System.Windows.Forms.Label();
            this.labelLogonCount = new System.Windows.Forms.Label();
            this.labelLogonCountValue = new System.Windows.Forms.Label();
            this.labelAccountCreated = new System.Windows.Forms.Label();
            this.labelAccountCreatedValue = new System.Windows.Forms.Label();
            this.labelAccountModified = new System.Windows.Forms.Label();
            this.labelAccountModifiedValue = new System.Windows.Forms.Label();
            this.labelUserAccountControl = new System.Windows.Forms.Label();
            this.labelUserAccountControlValue = new System.Windows.Forms.Label();
            this.groupBoxActions = new System.Windows.Forms.GroupBox();
            this.buttonUnlock = new System.Windows.Forms.Button();
            this.buttonPasswordManager = new System.Windows.Forms.Button();
            this.buttonViewDetails = new System.Windows.Forms.Button();
            this.groupBoxGroups = new System.Windows.Forms.GroupBox();
            this.listBoxGroups = new System.Windows.Forms.ListBox();
            this.labelGroupsCount = new System.Windows.Forms.Label();
            this.buttonCopyGroups = new System.Windows.Forms.Button();
            this.groupBoxUserAccountControl = new System.Windows.Forms.GroupBox();
            this.labelUserAccountControlDescription = new System.Windows.Forms.TextBox();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.groupBoxSearch.SuspendLayout();
            this.groupBoxUserInfo.SuspendLayout();
            this.groupBoxActions.SuspendLayout();
            this.groupBoxGroups.SuspendLayout();
            this.groupBoxUserAccountControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem,
            this.checkUpdateToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(784, 24);
            this.menuStrip.TabIndex = 0;
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.exitToolStripMenuItem.Text = "E&xit";
            // 
            // checkUpdateToolStripMenuItem
            // 
            this.checkUpdateToolStripMenuItem.Name = "checkUpdateToolStripMenuItem";
            this.checkUpdateToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.checkUpdateToolStripMenuItem.Text = "U&pdate";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "&About";
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 719);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(784, 22);
            this.statusStrip.TabIndex = 1;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(43, 17);
            this.toolStripStatusLabel.Text = "준비됨";
            // 
            // groupBoxSearch
            // 
            this.groupBoxSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSearch.Controls.Add(this.labelConnectionStatus);
            this.groupBoxSearch.Controls.Add(this.labelUserId);
            this.groupBoxSearch.Controls.Add(this.textBoxUserId);
            this.groupBoxSearch.Controls.Add(this.buttonSearch);
            this.groupBoxSearch.Controls.Add(this.buttonRefresh);
            this.groupBoxSearch.Controls.Add(this.buttonVerify);
            this.groupBoxSearch.Controls.Add(this.buttonOpenDSA);
            this.groupBoxSearch.Controls.Add(this.labelAccountSummary);
            this.groupBoxSearch.Controls.Add(this.textBoxAccountSummary);
            this.groupBoxSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxSearch.Location = new System.Drawing.Point(12, 35);
            this.groupBoxSearch.Name = "groupBoxSearch";
            this.groupBoxSearch.Size = new System.Drawing.Size(760, 110);
            this.groupBoxSearch.TabIndex = 2;
            this.groupBoxSearch.TabStop = false;
            this.groupBoxSearch.Text = "🔍 사용자 검색";
            // 
            // labelConnectionStatus
            // 
            this.labelConnectionStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelConnectionStatus.ForeColor = System.Drawing.Color.Orange;
            this.labelConnectionStatus.Location = new System.Drawing.Point(15, 25);
            this.labelConnectionStatus.Name = "labelConnectionStatus";
            this.labelConnectionStatus.Size = new System.Drawing.Size(600, 15);
            this.labelConnectionStatus.TabIndex = 0;
            this.labelConnectionStatus.Text = "연결 확인 중...";
            // 
            // labelUserId
            // 
            this.labelUserId.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelUserId.Location = new System.Drawing.Point(15, 52);
            this.labelUserId.Name = "labelUserId";
            this.labelUserId.Size = new System.Drawing.Size(60, 13);
            this.labelUserId.TabIndex = 1;
            this.labelUserId.Text = "사용자 ID:";
            // 
            // textBoxUserId
            // 
            this.textBoxUserId.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.textBoxUserId.Location = new System.Drawing.Point(83, 49);
            this.textBoxUserId.Name = "textBoxUserId";
            this.textBoxUserId.Size = new System.Drawing.Size(200, 21);
            this.textBoxUserId.TabIndex = 2;
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(295, 47);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(75, 25);
            this.buttonSearch.TabIndex = 3;
            this.buttonSearch.Text = "검색";
            this.buttonSearch.UseVisualStyleBackColor = true;
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(380, 47);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(95, 25);
            this.buttonRefresh.TabIndex = 4;
            this.buttonRefresh.Text = "새로고침 (F5)";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            // 
            // buttonVerify
            // 
            this.buttonVerify.Location = new System.Drawing.Point(485, 47);
            this.buttonVerify.Name = "buttonVerify";
            this.buttonVerify.Size = new System.Drawing.Size(75, 25);
            this.buttonVerify.TabIndex = 5;
            this.buttonVerify.Text = "🔐 검증";
            this.buttonVerify.UseVisualStyleBackColor = true;
            // 
            // buttonOpenDSA
            // 
            this.buttonOpenDSA.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.buttonOpenDSA.Location = new System.Drawing.Point(570, 47);
            this.buttonOpenDSA.Name = "buttonOpenDSA";
            this.buttonOpenDSA.Size = new System.Drawing.Size(185, 25);
            this.buttonOpenDSA.TabIndex = 6;
            this.buttonOpenDSA.Text = "PC 관리 - ClientDevices";
            this.buttonOpenDSA.UseVisualStyleBackColor = true;
            // 
            // labelAccountSummary
            // 
            this.labelAccountSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelAccountSummary.Location = new System.Drawing.Point(15, 77);
            this.labelAccountSummary.Name = "labelAccountSummary";
            this.labelAccountSummary.Size = new System.Drawing.Size(60, 13);
            this.labelAccountSummary.TabIndex = 6;
            this.labelAccountSummary.Text = "상태 요약:";
            // 
            // textBoxAccountSummary
            // 
            this.textBoxAccountSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.textBoxAccountSummary.Location = new System.Drawing.Point(83, 74);
            this.textBoxAccountSummary.Name = "textBoxAccountSummary";
            this.textBoxAccountSummary.ReadOnly = true;
            this.textBoxAccountSummary.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxAccountSummary.Size = new System.Drawing.Size(675, 21);
            this.textBoxAccountSummary.TabIndex = 7;
            this.textBoxAccountSummary.TabStop = false;
            this.textBoxAccountSummary.Text = "-";
            // 
            // groupBoxUserInfo
            // 
            this.groupBoxUserInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxUserInfo.Controls.Add(this.labelUserIdInfo);
            this.groupBoxUserInfo.Controls.Add(this.labelUserIdValue);
            this.groupBoxUserInfo.Controls.Add(this.labelDisplayName);
            this.groupBoxUserInfo.Controls.Add(this.labelDisplayNameValue);
            this.groupBoxUserInfo.Controls.Add(this.labelEmail);
            this.groupBoxUserInfo.Controls.Add(this.labelEmailValue);
            this.groupBoxUserInfo.Controls.Add(this.labelDepartment);
            this.groupBoxUserInfo.Controls.Add(this.labelDepartmentValue);
            this.groupBoxUserInfo.Controls.Add(this.labelTitle);
            this.groupBoxUserInfo.Controls.Add(this.labelTitleValue);
            this.groupBoxUserInfo.Controls.Add(this.labelAccountStatus);
            this.groupBoxUserInfo.Controls.Add(this.labelAccountStatusValue);
            this.groupBoxUserInfo.Controls.Add(this.labelLastLogon);
            this.groupBoxUserInfo.Controls.Add(this.labelLastLogonValue);
            this.groupBoxUserInfo.Controls.Add(this.labelPasswordLastSet);
            this.groupBoxUserInfo.Controls.Add(this.labelPasswordLastSetValue);
            this.groupBoxUserInfo.Controls.Add(this.labelPasswordExpiry);
            this.groupBoxUserInfo.Controls.Add(this.labelPasswordExpiryValue);
            this.groupBoxUserInfo.Controls.Add(this.labelPasswordCanChange);
            this.groupBoxUserInfo.Controls.Add(this.labelPasswordCanChangeValue);
            this.groupBoxUserInfo.Controls.Add(this.labelPasswordStatus);
            this.groupBoxUserInfo.Controls.Add(this.labelPasswordStatusValue);
            this.groupBoxUserInfo.Controls.Add(this.labelBadPasswordCount);
            this.groupBoxUserInfo.Controls.Add(this.labelBadPasswordCountValue);
            this.groupBoxUserInfo.Controls.Add(this.labelLockoutTime);
            this.groupBoxUserInfo.Controls.Add(this.labelLockoutTimeValue);
            this.groupBoxUserInfo.Controls.Add(this.labelLogonCount);
            this.groupBoxUserInfo.Controls.Add(this.labelLogonCountValue);
            this.groupBoxUserInfo.Controls.Add(this.labelAccountCreated);
            this.groupBoxUserInfo.Controls.Add(this.labelAccountCreatedValue);
            this.groupBoxUserInfo.Controls.Add(this.labelAccountModified);
            this.groupBoxUserInfo.Controls.Add(this.labelAccountModifiedValue);
            this.groupBoxUserInfo.Controls.Add(this.labelUserAccountControl);
            this.groupBoxUserInfo.Controls.Add(this.labelUserAccountControlValue);
            this.groupBoxUserInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxUserInfo.Location = new System.Drawing.Point(12, 155);
            this.groupBoxUserInfo.Name = "groupBoxUserInfo";
            this.groupBoxUserInfo.Size = new System.Drawing.Size(473, 250);
            this.groupBoxUserInfo.TabIndex = 8;
            this.groupBoxUserInfo.TabStop = false;
            this.groupBoxUserInfo.Text = "👤 사용자 정보";
            // 
            // labelUserIdInfo
            // 
            this.labelUserIdInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelUserIdInfo.Location = new System.Drawing.Point(15, 25);
            this.labelUserIdInfo.Name = "labelUserIdInfo";
            this.labelUserIdInfo.Size = new System.Drawing.Size(80, 13);
            this.labelUserIdInfo.TabIndex = 0;
            this.labelUserIdInfo.Text = "사용자 ID:";
            // 
            // labelUserIdValue
            // 
            this.labelUserIdValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelUserIdValue.Location = new System.Drawing.Point(100, 25);
            this.labelUserIdValue.Name = "labelUserIdValue";
            this.labelUserIdValue.Size = new System.Drawing.Size(125, 13);
            this.labelUserIdValue.TabIndex = 1;
            this.labelUserIdValue.Text = "-";
            // 
            // labelDisplayName
            // 
            this.labelDisplayName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelDisplayName.Location = new System.Drawing.Point(15, 50);
            this.labelDisplayName.Name = "labelDisplayName";
            this.labelDisplayName.Size = new System.Drawing.Size(80, 13);
            this.labelDisplayName.TabIndex = 2;
            this.labelDisplayName.Text = "이름:";
            // 
            // labelDisplayNameValue
            // 
            this.labelDisplayNameValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelDisplayNameValue.Location = new System.Drawing.Point(100, 50);
            this.labelDisplayNameValue.Name = "labelDisplayNameValue";
            this.labelDisplayNameValue.Size = new System.Drawing.Size(125, 13);
            this.labelDisplayNameValue.TabIndex = 3;
            this.labelDisplayNameValue.Text = "-";
            // 
            // labelEmail
            // 
            this.labelEmail.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelEmail.Location = new System.Drawing.Point(15, 75);
            this.labelEmail.Name = "labelEmail";
            this.labelEmail.Size = new System.Drawing.Size(80, 13);
            this.labelEmail.TabIndex = 4;
            this.labelEmail.Text = "이메일:";
            // 
            // labelEmailValue
            // 
            this.labelEmailValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelEmailValue.Location = new System.Drawing.Point(100, 75);
            this.labelEmailValue.Name = "labelEmailValue";
            this.labelEmailValue.Size = new System.Drawing.Size(125, 13);
            this.labelEmailValue.TabIndex = 5;
            this.labelEmailValue.Text = "-";
            // 
            // labelDepartment
            // 
            this.labelDepartment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelDepartment.Location = new System.Drawing.Point(15, 100);
            this.labelDepartment.Name = "labelDepartment";
            this.labelDepartment.Size = new System.Drawing.Size(80, 13);
            this.labelDepartment.TabIndex = 6;
            this.labelDepartment.Text = "부서:";
            // 
            // labelDepartmentValue
            // 
            this.labelDepartmentValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelDepartmentValue.Location = new System.Drawing.Point(100, 100);
            this.labelDepartmentValue.Name = "labelDepartmentValue";
            this.labelDepartmentValue.Size = new System.Drawing.Size(125, 13);
            this.labelDepartmentValue.TabIndex = 7;
            this.labelDepartmentValue.Text = "-";
            // 
            // labelTitle
            // 
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelTitle.Location = new System.Drawing.Point(15, 125);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(80, 13);
            this.labelTitle.TabIndex = 8;
            this.labelTitle.Text = "직책:";
            // 
            // labelTitleValue
            // 
            this.labelTitleValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelTitleValue.Location = new System.Drawing.Point(100, 125);
            this.labelTitleValue.Name = "labelTitleValue";
            this.labelTitleValue.Size = new System.Drawing.Size(125, 13);
            this.labelTitleValue.TabIndex = 9;
            this.labelTitleValue.Text = "-";
            // 
            // labelAccountStatus
            // 
            this.labelAccountStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelAccountStatus.Location = new System.Drawing.Point(15, 150);
            this.labelAccountStatus.Name = "labelAccountStatus";
            this.labelAccountStatus.Size = new System.Drawing.Size(80, 13);
            this.labelAccountStatus.TabIndex = 10;
            this.labelAccountStatus.Text = "계정 상태:";
            // 
            // labelAccountStatusValue
            // 
            this.labelAccountStatusValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelAccountStatusValue.Location = new System.Drawing.Point(100, 150);
            this.labelAccountStatusValue.Name = "labelAccountStatusValue";
            this.labelAccountStatusValue.Size = new System.Drawing.Size(125, 13);
            this.labelAccountStatusValue.TabIndex = 11;
            this.labelAccountStatusValue.Text = "-";
            // 
            // labelLastLogon
            // 
            this.labelLastLogon.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelLastLogon.Location = new System.Drawing.Point(237, 25);
            this.labelLastLogon.Name = "labelLastLogon";
            this.labelLastLogon.Size = new System.Drawing.Size(100, 13);
            this.labelLastLogon.TabIndex = 12;
            this.labelLastLogon.Text = "마지막 로그인:";
            // 
            // labelLastLogonValue
            // 
            this.labelLastLogonValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelLastLogonValue.Location = new System.Drawing.Point(335, 25);
            this.labelLastLogonValue.Name = "labelLastLogonValue";
            this.labelLastLogonValue.Size = new System.Drawing.Size(125, 13);
            this.labelLastLogonValue.TabIndex = 13;
            this.labelLastLogonValue.Text = "-";
            // 
            // labelPasswordLastSet
            // 
            this.labelPasswordLastSet.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelPasswordLastSet.Location = new System.Drawing.Point(237, 50);
            this.labelPasswordLastSet.Name = "labelPasswordLastSet";
            this.labelPasswordLastSet.Size = new System.Drawing.Size(100, 13);
            this.labelPasswordLastSet.TabIndex = 14;
            this.labelPasswordLastSet.Text = "비밀번호 설정일:";
            // 
            // labelPasswordLastSetValue
            // 
            this.labelPasswordLastSetValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelPasswordLastSetValue.Location = new System.Drawing.Point(335, 50);
            this.labelPasswordLastSetValue.Name = "labelPasswordLastSetValue";
            this.labelPasswordLastSetValue.Size = new System.Drawing.Size(125, 13);
            this.labelPasswordLastSetValue.TabIndex = 15;
            this.labelPasswordLastSetValue.Text = "-";
            // 
            // labelPasswordExpiry
            // 
            this.labelPasswordExpiry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelPasswordExpiry.Location = new System.Drawing.Point(237, 75);
            this.labelPasswordExpiry.Name = "labelPasswordExpiry";
            this.labelPasswordExpiry.Size = new System.Drawing.Size(90, 13);
            this.labelPasswordExpiry.TabIndex = 16;
            this.labelPasswordExpiry.Text = "비밀번호 만료일:";
            // 
            // labelPasswordExpiryValue
            // 
            this.labelPasswordExpiryValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelPasswordExpiryValue.Location = new System.Drawing.Point(335, 75);
            this.labelPasswordExpiryValue.Name = "labelPasswordExpiryValue";
            this.labelPasswordExpiryValue.Size = new System.Drawing.Size(125, 13);
            this.labelPasswordExpiryValue.TabIndex = 17;
            this.labelPasswordExpiryValue.Text = "-";
            // 
            // labelPasswordCanChange
            // 
            this.labelPasswordCanChange.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelPasswordCanChange.Location = new System.Drawing.Point(237, 100);
            this.labelPasswordCanChange.Name = "labelPasswordCanChange";
            this.labelPasswordCanChange.Size = new System.Drawing.Size(100, 13);
            this.labelPasswordCanChange.TabIndex = 16;
            this.labelPasswordCanChange.Text = "암호 변경 가능일:";
            // 
            // labelPasswordCanChangeValue
            // 
            this.labelPasswordCanChangeValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelPasswordCanChangeValue.Location = new System.Drawing.Point(335, 100);
            this.labelPasswordCanChangeValue.Name = "labelPasswordCanChangeValue";
            this.labelPasswordCanChangeValue.Size = new System.Drawing.Size(125, 13);
            this.labelPasswordCanChangeValue.TabIndex = 17;
            this.labelPasswordCanChangeValue.Text = "-";
            // 
            // labelPasswordStatus
            // 
            this.labelPasswordStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelPasswordStatus.Location = new System.Drawing.Point(237, 125);
            this.labelPasswordStatus.Name = "labelPasswordStatus";
            this.labelPasswordStatus.Size = new System.Drawing.Size(100, 13);
            this.labelPasswordStatus.TabIndex = 18;
            this.labelPasswordStatus.Text = "비밀번호 상태:";
            // 
            // labelPasswordStatusValue
            // 
            this.labelPasswordStatusValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelPasswordStatusValue.Location = new System.Drawing.Point(335, 125);
            this.labelPasswordStatusValue.Name = "labelPasswordStatusValue";
            this.labelPasswordStatusValue.Size = new System.Drawing.Size(125, 13);
            this.labelPasswordStatusValue.TabIndex = 19;
            this.labelPasswordStatusValue.Text = "-";
            // 
            // labelBadPasswordCount
            // 
            this.labelBadPasswordCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelBadPasswordCount.Location = new System.Drawing.Point(237, 150);
            this.labelBadPasswordCount.Name = "labelBadPasswordCount";
            this.labelBadPasswordCount.Size = new System.Drawing.Size(100, 13);
            this.labelBadPasswordCount.TabIndex = 20;
            this.labelBadPasswordCount.Text = "로그인 실패:";
            // 
            // labelBadPasswordCountValue
            // 
            this.labelBadPasswordCountValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelBadPasswordCountValue.Location = new System.Drawing.Point(335, 150);
            this.labelBadPasswordCountValue.Name = "labelBadPasswordCountValue";
            this.labelBadPasswordCountValue.Size = new System.Drawing.Size(125, 13);
            this.labelBadPasswordCountValue.TabIndex = 21;
            this.labelBadPasswordCountValue.Text = "-";
            // 
            // labelLockoutTime
            // 
            this.labelLockoutTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelLockoutTime.Location = new System.Drawing.Point(237, 175);
            this.labelLockoutTime.Name = "labelLockoutTime";
            this.labelLockoutTime.Size = new System.Drawing.Size(100, 13);
            this.labelLockoutTime.TabIndex = 22;
            this.labelLockoutTime.Text = "잠금 시간:";
            // 
            // labelLockoutTimeValue
            // 
            this.labelLockoutTimeValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelLockoutTimeValue.Location = new System.Drawing.Point(335, 175);
            this.labelLockoutTimeValue.Name = "labelLockoutTimeValue";
            this.labelLockoutTimeValue.Size = new System.Drawing.Size(125, 13);
            this.labelLockoutTimeValue.TabIndex = 23;
            this.labelLockoutTimeValue.Text = "-";
            // 
            // labelLogonCount
            // 
            this.labelLogonCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelLogonCount.Location = new System.Drawing.Point(15, 175);
            this.labelLogonCount.Name = "labelLogonCount";
            this.labelLogonCount.Size = new System.Drawing.Size(80, 13);
            this.labelLogonCount.TabIndex = 24;
            this.labelLogonCount.Text = "로그온 횟수:";
            // 
            // labelLogonCountValue
            // 
            this.labelLogonCountValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelLogonCountValue.Location = new System.Drawing.Point(100, 175);
            this.labelLogonCountValue.Name = "labelLogonCountValue";
            this.labelLogonCountValue.Size = new System.Drawing.Size(125, 13);
            this.labelLogonCountValue.TabIndex = 25;
            this.labelLogonCountValue.Text = "-";
            // 
            // labelAccountCreated
            // 
            this.labelAccountCreated.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelAccountCreated.Location = new System.Drawing.Point(237, 200);
            this.labelAccountCreated.Name = "labelAccountCreated";
            this.labelAccountCreated.Size = new System.Drawing.Size(100, 13);
            this.labelAccountCreated.TabIndex = 26;
            this.labelAccountCreated.Text = "계정 생성일:";
            // 
            // labelAccountCreatedValue
            // 
            this.labelAccountCreatedValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelAccountCreatedValue.Location = new System.Drawing.Point(335, 200);
            this.labelAccountCreatedValue.Name = "labelAccountCreatedValue";
            this.labelAccountCreatedValue.Size = new System.Drawing.Size(125, 13);
            this.labelAccountCreatedValue.TabIndex = 27;
            this.labelAccountCreatedValue.Text = "-";
            // 
            // labelAccountModified
            // 
            this.labelAccountModified.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelAccountModified.Location = new System.Drawing.Point(15, 200);
            this.labelAccountModified.Name = "labelAccountModified";
            this.labelAccountModified.Size = new System.Drawing.Size(80, 13);
            this.labelAccountModified.TabIndex = 28;
            this.labelAccountModified.Text = "계정 수정일:";
            // 
            // labelAccountModifiedValue
            // 
            this.labelAccountModifiedValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelAccountModifiedValue.Location = new System.Drawing.Point(100, 200);
            this.labelAccountModifiedValue.Name = "labelAccountModifiedValue";
            this.labelAccountModifiedValue.Size = new System.Drawing.Size(125, 13);
            this.labelAccountModifiedValue.TabIndex = 29;
            this.labelAccountModifiedValue.Text = "-";
            // 
            // labelUserAccountControl
            // 
            this.labelUserAccountControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelUserAccountControl.Location = new System.Drawing.Point(237, 225);
            this.labelUserAccountControl.Name = "labelUserAccountControl";
            this.labelUserAccountControl.Size = new System.Drawing.Size(100, 13);
            this.labelUserAccountControl.TabIndex = 30;
            this.labelUserAccountControl.Text = "계정 제어:";
            // 
            // labelUserAccountControlValue
            // 
            this.labelUserAccountControlValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelUserAccountControlValue.Location = new System.Drawing.Point(335, 225);
            this.labelUserAccountControlValue.Name = "labelUserAccountControlValue";
            this.labelUserAccountControlValue.Size = new System.Drawing.Size(125, 13);
            this.labelUserAccountControlValue.TabIndex = 31;
            this.labelUserAccountControlValue.Text = "-";
            // 
            // groupBoxActions
            // 
            this.groupBoxActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxActions.Controls.Add(this.buttonUnlock);
            this.groupBoxActions.Controls.Add(this.buttonPasswordManager);
            this.groupBoxActions.Controls.Add(this.buttonViewDetails);
            this.groupBoxActions.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxActions.Location = new System.Drawing.Point(491, 155);
            this.groupBoxActions.Name = "groupBoxActions";
            this.groupBoxActions.Size = new System.Drawing.Size(280, 122);
            this.groupBoxActions.TabIndex = 9;
            this.groupBoxActions.TabStop = false;
            this.groupBoxActions.Text = "⚙️ 관리 기능";
            // 
            // buttonUnlock
            // 
            this.buttonUnlock.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.buttonUnlock.Location = new System.Drawing.Point(15, 25);
            this.buttonUnlock.Name = "buttonUnlock";
            this.buttonUnlock.Size = new System.Drawing.Size(250, 25);
            this.buttonUnlock.TabIndex = 0;
            this.buttonUnlock.Text = "🔓 &Unlock Account";
            this.buttonUnlock.UseVisualStyleBackColor = true;
            // 
            // buttonPasswordManager
            // 
            this.buttonPasswordManager.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.buttonPasswordManager.ForeColor = System.Drawing.Color.DarkGreen;
            this.buttonPasswordManager.Location = new System.Drawing.Point(15, 55);
            this.buttonPasswordManager.Name = "buttonPasswordManager";
            this.buttonPasswordManager.Size = new System.Drawing.Size(250, 25);
            this.buttonPasswordManager.TabIndex = 1;
            this.buttonPasswordManager.Text = "🔑 &Change Password";
            this.buttonPasswordManager.UseVisualStyleBackColor = true;
            // 
            // buttonViewDetails
            // 
            this.buttonViewDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.buttonViewDetails.ForeColor = System.Drawing.Color.DarkBlue;
            this.buttonViewDetails.Location = new System.Drawing.Point(15, 85);
            this.buttonViewDetails.Name = "buttonViewDetails";
            this.buttonViewDetails.Size = new System.Drawing.Size(250, 25);
            this.buttonViewDetails.TabIndex = 2;
            this.buttonViewDetails.Text = "🔍 View &Details";
            this.buttonViewDetails.UseVisualStyleBackColor = true;
            // 
            // groupBoxGroups
            // 
            this.groupBoxGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxGroups.Controls.Add(this.listBoxGroups);
            this.groupBoxGroups.Controls.Add(this.labelGroupsCount);
            this.groupBoxGroups.Controls.Add(this.buttonCopyGroups);
            this.groupBoxGroups.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxGroups.Location = new System.Drawing.Point(12, 416);
            this.groupBoxGroups.Name = "groupBoxGroups";
            this.groupBoxGroups.Size = new System.Drawing.Size(760, 264);
            this.groupBoxGroups.TabIndex = 10;
            this.groupBoxGroups.TabStop = false;
            this.groupBoxGroups.Text = "👥 소속 그룹";
            // 
            // listBoxGroups
            // 
            this.listBoxGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxGroups.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.listBoxGroups.HorizontalScrollbar = true;
            this.listBoxGroups.Location = new System.Drawing.Point(15, 40);
            this.listBoxGroups.Name = "listBoxGroups";
            this.listBoxGroups.Size = new System.Drawing.Size(740, 212);
            this.listBoxGroups.TabIndex = 1;
            // 
            // labelGroupsCount
            // 
            this.labelGroupsCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelGroupsCount.Location = new System.Drawing.Point(15, 25);
            this.labelGroupsCount.Name = "labelGroupsCount";
            this.labelGroupsCount.Size = new System.Drawing.Size(100, 13);
            this.labelGroupsCount.TabIndex = 0;
            this.labelGroupsCount.Text = "그룹 수: 0";
            // 
            // buttonCopyGroups
            // 
            this.buttonCopyGroups.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.buttonCopyGroups.Location = new System.Drawing.Point(650, 20);
            this.buttonCopyGroups.Name = "buttonCopyGroups";
            this.buttonCopyGroups.Size = new System.Drawing.Size(105, 23);
            this.buttonCopyGroups.TabIndex = 2;
            this.buttonCopyGroups.Text = "📋 그룹 복사";
            this.buttonCopyGroups.UseVisualStyleBackColor = true;
            // 
            // groupBoxUserAccountControl
            // 
            this.groupBoxUserAccountControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxUserAccountControl.Controls.Add(this.labelUserAccountControlDescription);
            this.groupBoxUserAccountControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxUserAccountControl.Location = new System.Drawing.Point(491, 283);
            this.groupBoxUserAccountControl.Name = "groupBoxUserAccountControl";
            this.groupBoxUserAccountControl.Size = new System.Drawing.Size(280, 122);
            this.groupBoxUserAccountControl.TabIndex = 11;
            this.groupBoxUserAccountControl.TabStop = false;
            this.groupBoxUserAccountControl.Text = "⚙️ Account Control Details";
            // 
            // labelUserAccountControlDescription
            // 
            this.labelUserAccountControlDescription.BackColor = System.Drawing.Color.LightYellow;
            this.labelUserAccountControlDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelUserAccountControlDescription.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUserAccountControlDescription.Location = new System.Drawing.Point(15, 25);
            this.labelUserAccountControlDescription.Multiline = true;
            this.labelUserAccountControlDescription.Name = "labelUserAccountControlDescription";
            this.labelUserAccountControlDescription.ReadOnly = true;
            this.labelUserAccountControlDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.labelUserAccountControlDescription.Size = new System.Drawing.Size(250, 85);
            this.labelUserAccountControlDescription.TabIndex = 0;
            this.labelUserAccountControlDescription.TabStop = false;
            this.labelUserAccountControlDescription.Text = "-";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 741);
            this.Controls.Add(this.groupBoxGroups);
            this.Controls.Add(this.groupBoxUserAccountControl);
            this.Controls.Add(this.groupBoxActions);
            this.Controls.Add(this.groupBoxUserInfo);
            this.Controls.Add(this.groupBoxSearch);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(800, 780);
            this.Name = "MainForm";
            this.Text = "AD Helper - Active Directory 사용자 관리";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.groupBoxSearch.ResumeLayout(false);
            this.groupBoxSearch.PerformLayout();
            this.groupBoxUserInfo.ResumeLayout(false);
            this.groupBoxActions.ResumeLayout(false);
            this.groupBoxGroups.ResumeLayout(false);
            this.groupBoxUserAccountControl.ResumeLayout(false);
            this.groupBoxUserAccountControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MenuStrip menuStrip;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem checkUpdateToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel;
        
        private GroupBox groupBoxSearch;
        private Label labelConnectionStatus;
        private Label labelUserId;
        private TextBox textBoxUserId;
        private Button buttonSearch;
        private Button buttonRefresh;
        private Button buttonVerify;
        private Button buttonOpenDSA;
        private Label labelAccountSummary;
        
        private GroupBox groupBoxUserInfo;
        private Label labelUserIdInfo;
        private Label labelUserIdValue;
        private Label labelDisplayName;
        private Label labelDisplayNameValue;
        private Label labelEmail;
        private Label labelEmailValue;
        private Label labelDepartment;
        private Label labelDepartmentValue;
        private Label labelTitle;
        private Label labelTitleValue;
        private Label labelAccountStatus;
        private Label labelAccountStatusValue;
        private Label labelLastLogon;
        private Label labelLastLogonValue;
        private Label labelPasswordLastSet;
        private Label labelPasswordLastSetValue;
        private Label labelPasswordExpiry;
        private Label labelPasswordExpiryValue;
        private Label labelPasswordCanChange;
        private Label labelPasswordCanChangeValue;
        private Label labelPasswordStatus;
        private Label labelPasswordStatusValue;
        private Label labelBadPasswordCount;
        private Label labelBadPasswordCountValue;
        private Label labelLockoutTime;
        private Label labelLockoutTimeValue;
        
        // 추가 필드들
        private Label labelLogonCount;
        private Label labelLogonCountValue;
        private Label labelAccountCreated;
        private Label labelAccountCreatedValue;
        private Label labelAccountModified;
        private Label labelAccountModifiedValue;
        private Label labelUserAccountControl;
        private Label labelUserAccountControlValue;
        
        private GroupBox groupBoxActions;
        private Button buttonUnlock;
        private Button buttonPasswordManager;
        private Button buttonViewDetails;
        
        
        private GroupBox groupBoxGroups;
        private ListBox listBoxGroups;
        private Label labelGroupsCount;
        private Button buttonCopyGroups;
        
        private GroupBox groupBoxUserAccountControl;
        private TextBox labelUserAccountControlDescription;
        private TextBox textBoxAccountSummary;
    }
}
