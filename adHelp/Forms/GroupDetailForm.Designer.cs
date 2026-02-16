using System;
using System.Drawing;
using System.Windows.Forms;

namespace adHelp.Forms
{
    partial class GroupDetailForm
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
            this.groupBoxInfo = new System.Windows.Forms.GroupBox();
            this.labelGroupName = new System.Windows.Forms.Label();
            this.labelGroupNameValue = new System.Windows.Forms.Label();
            this.labelDescription = new System.Windows.Forms.Label();
            this.labelDescriptionValue = new System.Windows.Forms.Label();
            this.labelDistinguishedName = new System.Windows.Forms.Label();
            this.labelDistinguishedNameValue = new System.Windows.Forms.Label();
            this.labelCreated = new System.Windows.Forms.Label();
            this.labelCreatedValue = new System.Windows.Forms.Label();
            this.labelModified = new System.Windows.Forms.Label();
            this.labelModifiedValue = new System.Windows.Forms.Label();
            this.labelGroupType = new System.Windows.Forms.Label();
            this.labelGroupTypeValue = new System.Windows.Forms.Label();
            this.labelEmail = new System.Windows.Forms.Label();
            this.labelEmailValue = new System.Windows.Forms.Label();
            this.labelManagedBy = new System.Windows.Forms.Label();
            this.labelManagedByValue = new System.Windows.Forms.Label();
            this.labelGuid = new System.Windows.Forms.Label();
            this.labelGuidValue = new System.Windows.Forms.Label();
            this.groupBoxMembers = new System.Windows.Forms.GroupBox();
            this.dataGridViewMembers = new System.Windows.Forms.DataGridView();
            this.labelMembersCount = new System.Windows.Forms.Label();
            this.labelMembersCountValue = new System.Windows.Forms.Label();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.groupBoxInfo.SuspendLayout();
            this.groupBoxMembers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMembers)).BeginInit();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxInfo
            // 
            this.groupBoxInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxInfo.Controls.Add(this.labelGroupName);
            this.groupBoxInfo.Controls.Add(this.labelGroupNameValue);
            this.groupBoxInfo.Controls.Add(this.labelDescription);
            this.groupBoxInfo.Controls.Add(this.labelDescriptionValue);
            this.groupBoxInfo.Controls.Add(this.labelDistinguishedName);
            this.groupBoxInfo.Controls.Add(this.labelDistinguishedNameValue);
            this.groupBoxInfo.Controls.Add(this.labelCreated);
            this.groupBoxInfo.Controls.Add(this.labelCreatedValue);
            this.groupBoxInfo.Controls.Add(this.labelModified);
            this.groupBoxInfo.Controls.Add(this.labelModifiedValue);
            this.groupBoxInfo.Controls.Add(this.labelGroupType);
            this.groupBoxInfo.Controls.Add(this.labelGroupTypeValue);
            this.groupBoxInfo.Controls.Add(this.labelEmail);
            this.groupBoxInfo.Controls.Add(this.labelEmailValue);
            this.groupBoxInfo.Controls.Add(this.labelManagedBy);
            this.groupBoxInfo.Controls.Add(this.labelManagedByValue);
            this.groupBoxInfo.Controls.Add(this.labelGuid);
            this.groupBoxInfo.Controls.Add(this.labelGuidValue);
            this.groupBoxInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxInfo.Location = new System.Drawing.Point(12, 12);
            this.groupBoxInfo.Name = "groupBoxInfo";
            this.groupBoxInfo.Size = new System.Drawing.Size(860, 200);
            this.groupBoxInfo.TabIndex = 0;
            this.groupBoxInfo.TabStop = false;
            this.groupBoxInfo.Text = "👥 그룹 기본 정보";
            // 
            // labelGroupName
            // 
            this.labelGroupName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelGroupName.Location = new System.Drawing.Point(15, 25);
            this.labelGroupName.Name = "labelGroupName";
            this.labelGroupName.Size = new System.Drawing.Size(80, 13);
            this.labelGroupName.TabIndex = 0;
            this.labelGroupName.Text = "그룹명:";
            // 
            // labelGroupNameValue
            // 
            this.labelGroupNameValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelGroupNameValue.Location = new System.Drawing.Point(100, 25);
            this.labelGroupNameValue.Name = "labelGroupNameValue";
            this.labelGroupNameValue.Size = new System.Drawing.Size(300, 13);
            this.labelGroupNameValue.TabIndex = 1;
            this.labelGroupNameValue.Text = "-";
            // 
            // labelDescription
            // 
            this.labelDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelDescription.Location = new System.Drawing.Point(15, 50);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(80, 13);
            this.labelDescription.TabIndex = 2;
            this.labelDescription.Text = "설명:";
            // 
            // labelDescriptionValue
            // 
            this.labelDescriptionValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelDescriptionValue.Location = new System.Drawing.Point(100, 50);
            this.labelDescriptionValue.Name = "labelDescriptionValue";
            this.labelDescriptionValue.Size = new System.Drawing.Size(300, 13);
            this.labelDescriptionValue.TabIndex = 3;
            this.labelDescriptionValue.Text = "-";
            // 
            // labelDistinguishedName
            // 
            this.labelDistinguishedName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelDistinguishedName.Location = new System.Drawing.Point(15, 75);
            this.labelDistinguishedName.Name = "labelDistinguishedName";
            this.labelDistinguishedName.Size = new System.Drawing.Size(80, 13);
            this.labelDistinguishedName.TabIndex = 4;
            this.labelDistinguishedName.Text = "구분된 이름:";
            // 
            // labelDistinguishedNameValue
            // 
            this.labelDistinguishedNameValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelDistinguishedNameValue.Location = new System.Drawing.Point(100, 75);
            this.labelDistinguishedNameValue.Name = "labelDistinguishedNameValue";
            this.labelDistinguishedNameValue.Size = new System.Drawing.Size(745, 13);
            this.labelDistinguishedNameValue.TabIndex = 5;
            this.labelDistinguishedNameValue.Text = "-";
            // 
            // labelCreated
            // 
            this.labelCreated.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelCreated.Location = new System.Drawing.Point(15, 100);
            this.labelCreated.Name = "labelCreated";
            this.labelCreated.Size = new System.Drawing.Size(80, 13);
            this.labelCreated.TabIndex = 6;
            this.labelCreated.Text = "생성일:";
            // 
            // labelCreatedValue
            // 
            this.labelCreatedValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelCreatedValue.Location = new System.Drawing.Point(100, 100);
            this.labelCreatedValue.Name = "labelCreatedValue";
            this.labelCreatedValue.Size = new System.Drawing.Size(160, 13);
            this.labelCreatedValue.TabIndex = 7;
            this.labelCreatedValue.Text = "-";
            // 
            // labelModified
            // 
            this.labelModified.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelModified.Location = new System.Drawing.Point(280, 100);
            this.labelModified.Name = "labelModified";
            this.labelModified.Size = new System.Drawing.Size(80, 13);
            this.labelModified.TabIndex = 8;
            this.labelModified.Text = "수정일:";
            // 
            // labelModifiedValue
            // 
            this.labelModifiedValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelModifiedValue.Location = new System.Drawing.Point(365, 100);
            this.labelModifiedValue.Name = "labelModifiedValue";
            this.labelModifiedValue.Size = new System.Drawing.Size(160, 13);
            this.labelModifiedValue.TabIndex = 9;
            this.labelModifiedValue.Text = "-";
            // 
            // labelGroupType
            // 
            this.labelGroupType.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelGroupType.Location = new System.Drawing.Point(15, 125);
            this.labelGroupType.Name = "labelGroupType";
            this.labelGroupType.Size = new System.Drawing.Size(80, 13);
            this.labelGroupType.TabIndex = 10;
            this.labelGroupType.Text = "그룹 유형:";
            // 
            // labelGroupTypeValue
            // 
            this.labelGroupTypeValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelGroupTypeValue.Location = new System.Drawing.Point(100, 125);
            this.labelGroupTypeValue.Name = "labelGroupTypeValue";
            this.labelGroupTypeValue.Size = new System.Drawing.Size(300, 13);
            this.labelGroupTypeValue.TabIndex = 11;
            this.labelGroupTypeValue.Text = "-";
            // 
            // labelEmail
            // 
            this.labelEmail.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelEmail.Location = new System.Drawing.Point(15, 150);
            this.labelEmail.Name = "labelEmail";
            this.labelEmail.Size = new System.Drawing.Size(80, 13);
            this.labelEmail.TabIndex = 12;
            this.labelEmail.Text = "이메일:";
            // 
            // labelEmailValue
            // 
            this.labelEmailValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelEmailValue.Location = new System.Drawing.Point(100, 150);
            this.labelEmailValue.Name = "labelEmailValue";
            this.labelEmailValue.Size = new System.Drawing.Size(300, 13);
            this.labelEmailValue.TabIndex = 13;
            this.labelEmailValue.Text = "-";
            // 
            // labelManagedBy
            // 
            this.labelManagedBy.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelManagedBy.Location = new System.Drawing.Point(450, 150);
            this.labelManagedBy.Name = "labelManagedBy";
            this.labelManagedBy.Size = new System.Drawing.Size(80, 13);
            this.labelManagedBy.TabIndex = 14;
            this.labelManagedBy.Text = "관리자:";
            // 
            // labelManagedByValue
            // 
            this.labelManagedByValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelManagedByValue.Location = new System.Drawing.Point(535, 150);
            this.labelManagedByValue.Name = "labelManagedByValue";
            this.labelManagedByValue.Size = new System.Drawing.Size(300, 13);
            this.labelManagedByValue.TabIndex = 15;
            this.labelManagedByValue.Text = "-";
            // 
            // labelGuid
            // 
            this.labelGuid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelGuid.Location = new System.Drawing.Point(15, 175);
            this.labelGuid.Name = "labelGuid";
            this.labelGuid.Size = new System.Drawing.Size(80, 13);
            this.labelGuid.TabIndex = 16;
            this.labelGuid.Text = "GUID:";
            // 
            // labelGuidValue
            // 
            this.labelGuidValue.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.labelGuidValue.Location = new System.Drawing.Point(100, 175);
            this.labelGuidValue.Name = "labelGuidValue";
            this.labelGuidValue.Size = new System.Drawing.Size(300, 13);
            this.labelGuidValue.TabIndex = 17;
            this.labelGuidValue.Text = "-";
            // 
            // groupBoxMembers
            // 
            this.groupBoxMembers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMembers.Controls.Add(this.dataGridViewMembers);
            this.groupBoxMembers.Controls.Add(this.labelMembersCount);
            this.groupBoxMembers.Controls.Add(this.labelMembersCountValue);
            this.groupBoxMembers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxMembers.Location = new System.Drawing.Point(12, 222);
            this.groupBoxMembers.Name = "groupBoxMembers";
            this.groupBoxMembers.Size = new System.Drawing.Size(860, 320);
            this.groupBoxMembers.TabIndex = 1;
            this.groupBoxMembers.TabStop = false;
            this.groupBoxMembers.Text = "👤 구성원 목록";
            // 
            // dataGridViewMembers
            // 
            this.dataGridViewMembers.AllowUserToAddRows = false;
            this.dataGridViewMembers.AllowUserToDeleteRows = false;
            this.dataGridViewMembers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewMembers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewMembers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMembers.Location = new System.Drawing.Point(15, 45);
            this.dataGridViewMembers.MultiSelect = false;
            this.dataGridViewMembers.Name = "dataGridViewMembers";
            this.dataGridViewMembers.ReadOnly = true;
            this.dataGridViewMembers.RowHeadersVisible = false;
            this.dataGridViewMembers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewMembers.Size = new System.Drawing.Size(830, 260);
            this.dataGridViewMembers.TabIndex = 2;
            this.dataGridViewMembers.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewMembers_CellDoubleClick);
            // 
            // labelMembersCount
            // 
            this.labelMembersCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelMembersCount.Location = new System.Drawing.Point(15, 25);
            this.labelMembersCount.Name = "labelMembersCount";
            this.labelMembersCount.Size = new System.Drawing.Size(80, 13);
            this.labelMembersCount.TabIndex = 0;
            this.labelMembersCount.Text = "구성원 수:";
            // 
            // labelMembersCountValue
            // 
            this.labelMembersCountValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelMembersCountValue.Location = new System.Drawing.Point(100, 25);
            this.labelMembersCountValue.Name = "labelMembersCountValue";
            this.labelMembersCountValue.Size = new System.Drawing.Size(50, 13);
            this.labelMembersCountValue.TabIndex = 1;
            this.labelMembersCountValue.Text = "0";
            // 
            // panelButtons
            // 
            this.panelButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelButtons.Controls.Add(this.buttonRefresh);
            this.panelButtons.Controls.Add(this.buttonClose);
            this.panelButtons.Location = new System.Drawing.Point(12, 548);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(860, 40);
            this.panelButtons.TabIndex = 2;
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefresh.Location = new System.Drawing.Point(695, 8);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(75, 25);
            this.buttonRefresh.TabIndex = 0;
            this.buttonRefresh.Text = "새로고침";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonClose.Location = new System.Drawing.Point(780, 8);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 25);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "닫기";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.labelStatus.Location = new System.Drawing.Point(12, 595);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(860, 15);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "준비됨";
            // 
            // GroupDetailForm
            // 
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 621);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.groupBoxMembers);
            this.Controls.Add(this.groupBoxInfo);
            this.MinimumSize = new System.Drawing.Size(900, 660);
            this.Name = "GroupDetailForm";
            this.ShowIcon = true;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "그룹 정보";
            this.groupBoxInfo.ResumeLayout(false);
            this.groupBoxMembers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMembers)).EndInit();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox groupBoxInfo;
        private Label labelGroupName;
        private Label labelGroupNameValue;
        private Label labelDescription;
        private Label labelDescriptionValue;
        private Label labelDistinguishedName;
        private Label labelDistinguishedNameValue;
        private Label labelCreated;
        private Label labelCreatedValue;
        private Label labelModified;
        private Label labelModifiedValue;
        private Label labelGroupType;
        private Label labelGroupTypeValue;
        private Label labelEmail;
        private Label labelEmailValue;
        private Label labelManagedBy;
        private Label labelManagedByValue;
        private Label labelGuid;
        private Label labelGuidValue;
        
        private GroupBox groupBoxMembers;
        private DataGridView dataGridViewMembers;
        private Label labelMembersCount;
        private Label labelMembersCountValue;
        
        private Panel panelButtons;
        private Button buttonRefresh;
        private Button buttonClose;
        private Label labelStatus;
    }
}
