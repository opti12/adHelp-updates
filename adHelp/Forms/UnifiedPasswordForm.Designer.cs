namespace adHelp.Forms
{
    partial class UnifiedPasswordForm
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
            this.textBoxNewPassword = new System.Windows.Forms.TextBox();
            this.textBoxConfirmPassword = new System.Windows.Forms.TextBox();
            this.buttonCopyPassword = new System.Windows.Forms.Button();
            this.buttonToggleVisibility = new System.Windows.Forms.Button();
            this.labelPasswordStrength = new System.Windows.Forms.Label();
            this.labelMatchStatus = new System.Windows.Forms.Label();
            this.numericUpDownLength = new System.Windows.Forms.NumericUpDown();
            this.buttonGenerateMultiple = new System.Windows.Forms.Button();
            this.listBoxGeneratedPasswords = new System.Windows.Forms.ListBox();
            this.checkBoxForceChange = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.userLabel = new System.Windows.Forms.Label();
            this.ouPrefixLabel = new System.Windows.Forms.Label();
            this.ouPathLabel = new System.Windows.Forms.Label();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.groupBoxMain = new System.Windows.Forms.GroupBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblConfirm = new System.Windows.Forms.Label();
            this.lblStrength = new System.Windows.Forms.Label();
            this.generatorPanel = new System.Windows.Forms.Panel();
            this.groupBoxGenerator = new System.Windows.Forms.GroupBox();
            this.lblLength = new System.Windows.Forms.Label();
            this.lblGenerated = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLength)).BeginInit();
            this.headerPanel.SuspendLayout();
            this.mainPanel.SuspendLayout();
            this.groupBoxMain.SuspendLayout();
            this.generatorPanel.SuspendLayout();
            this.groupBoxGenerator.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxNewPassword
            // 
            this.textBoxNewPassword.Font = new System.Drawing.Font("Courier New", 10F);
            this.textBoxNewPassword.Location = new System.Drawing.Point(130, 28);
            this.textBoxNewPassword.Name = "textBoxNewPassword";
            this.textBoxNewPassword.Size = new System.Drawing.Size(200, 23);
            this.textBoxNewPassword.TabIndex = 1;
            this.textBoxNewPassword.UseSystemPasswordChar = true;
            // 
            // textBoxConfirmPassword
            // 
            this.textBoxConfirmPassword.Font = new System.Drawing.Font("Courier New", 10F);
            this.textBoxConfirmPassword.Location = new System.Drawing.Point(130, 63);
            this.textBoxConfirmPassword.Name = "textBoxConfirmPassword";
            this.textBoxConfirmPassword.Size = new System.Drawing.Size(200, 23);
            this.textBoxConfirmPassword.TabIndex = 5;
            this.textBoxConfirmPassword.UseSystemPasswordChar = true;
            // 
            // buttonCopyPassword
            // 
            this.buttonCopyPassword.Enabled = false;
            this.buttonCopyPassword.Location = new System.Drawing.Point(340, 28);
            this.buttonCopyPassword.Name = "buttonCopyPassword";
            this.buttonCopyPassword.Size = new System.Drawing.Size(30, 23);
            this.buttonCopyPassword.TabIndex = 2;
            this.buttonCopyPassword.Text = "📋";
            this.buttonCopyPassword.UseVisualStyleBackColor = true;
            // 
            // buttonToggleVisibility
            // 
            this.buttonToggleVisibility.Location = new System.Drawing.Point(380, 28);
            this.buttonToggleVisibility.Name = "buttonToggleVisibility";
            this.buttonToggleVisibility.Size = new System.Drawing.Size(30, 23);
            this.buttonToggleVisibility.TabIndex = 3;
            this.buttonToggleVisibility.Text = "👁";
            this.buttonToggleVisibility.UseVisualStyleBackColor = true;
            // 
            // labelPasswordStrength
            // 
            this.labelPasswordStrength.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelPasswordStrength.ForeColor = System.Drawing.Color.Gray;
            this.labelPasswordStrength.Location = new System.Drawing.Point(130, 100);
            this.labelPasswordStrength.Name = "labelPasswordStrength";
            this.labelPasswordStrength.Size = new System.Drawing.Size(200, 20);
            this.labelPasswordStrength.TabIndex = 7;
            this.labelPasswordStrength.Text = "입력 대기 중";
            // 
            // labelMatchStatus
            // 
            this.labelMatchStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelMatchStatus.Location = new System.Drawing.Point(130, 125);
            this.labelMatchStatus.Name = "labelMatchStatus";
            this.labelMatchStatus.Size = new System.Drawing.Size(300, 20);
            this.labelMatchStatus.TabIndex = 8;
            // 
            // numericUpDownLength
            // 
            this.numericUpDownLength.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.numericUpDownLength.Location = new System.Drawing.Point(65, 28);
            this.numericUpDownLength.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownLength.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDownLength.Name = "numericUpDownLength";
            this.numericUpDownLength.Size = new System.Drawing.Size(50, 23);
            this.numericUpDownLength.TabIndex = 1;
            this.numericUpDownLength.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            // 
            // buttonGenerateMultiple
            // 
            this.buttonGenerateMultiple.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.buttonGenerateMultiple.ForeColor = System.Drawing.Color.Green;
            this.buttonGenerateMultiple.Location = new System.Drawing.Point(130, 27);
            this.buttonGenerateMultiple.Name = "buttonGenerateMultiple";
            this.buttonGenerateMultiple.Size = new System.Drawing.Size(80, 25);
            this.buttonGenerateMultiple.TabIndex = 2;
            this.buttonGenerateMultiple.Text = "생성";
            this.buttonGenerateMultiple.UseVisualStyleBackColor = true;
            // 
            // listBoxGeneratedPasswords
            // 
            this.listBoxGeneratedPasswords.BackColor = System.Drawing.Color.White;
            this.listBoxGeneratedPasswords.Font = new System.Drawing.Font("Consolas", 9F);
            this.listBoxGeneratedPasswords.ItemHeight = 14;
            this.listBoxGeneratedPasswords.Location = new System.Drawing.Point(20, 85);
            this.listBoxGeneratedPasswords.Name = "listBoxGeneratedPasswords";
            this.listBoxGeneratedPasswords.Size = new System.Drawing.Size(410, 88);
            this.listBoxGeneratedPasswords.TabIndex = 4;
            // 
            // checkBoxForceChange
            // 
            this.checkBoxForceChange.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.checkBoxForceChange.Location = new System.Drawing.Point(130, 145);
            this.checkBoxForceChange.Name = "checkBoxForceChange";
            this.checkBoxForceChange.Size = new System.Drawing.Size(280, 20);
            this.checkBoxForceChange.TabIndex = 9;
            this.checkBoxForceChange.Text = "다음 로그인 시 비밀번호 변경 강제";
            // 
            // buttonOK
            // 
            this.buttonOK.Enabled = false;
            this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.buttonOK.ForeColor = System.Drawing.Color.Blue;
            this.buttonOK.Location = new System.Drawing.Point(280, 15);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(100, 30);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "비밀번호 변경";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.buttonCancel.Location = new System.Drawing.Point(390, 15);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(80, 30);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "취소";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.SystemColors.Control;
            this.headerPanel.Controls.Add(this.titleLabel);
            this.headerPanel.Controls.Add(this.userLabel);
            this.headerPanel.Controls.Add(this.ouPrefixLabel);
            this.headerPanel.Controls.Add(this.ouPathLabel);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(484, 85);
            this.headerPanel.TabIndex = 3;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.titleLabel.Location = new System.Drawing.Point(20, 15);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(131, 25);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "비밀번호 변경";
            // 
            // userLabel
            // 
            this.userLabel.AutoSize = true;
            this.userLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.userLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.userLabel.Location = new System.Drawing.Point(20, 42);
            this.userLabel.Name = "userLabel";
            this.userLabel.Size = new System.Drawing.Size(64, 15);
            this.userLabel.TabIndex = 1;
            this.userLabel.Text = "대상 계정: ";
            // 
            // ouPrefixLabel
            // 
            this.ouPrefixLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ouPrefixLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.ouPrefixLabel.Location = new System.Drawing.Point(20, 58);
            this.ouPrefixLabel.Name = "ouPrefixLabel";
            this.ouPrefixLabel.Size = new System.Drawing.Size(65, 20);
            this.ouPrefixLabel.TabIndex = 2;
            this.ouPrefixLabel.Text = "조직 단위:";
            // 
            // ouPathLabel
            // 
            this.ouPathLabel.AutoEllipsis = true;
            this.ouPathLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ouPathLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.ouPathLabel.Location = new System.Drawing.Point(85, 58);
            this.ouPathLabel.Name = "ouPathLabel";
            this.ouPathLabel.Size = new System.Drawing.Size(385, 20);
            this.ouPathLabel.TabIndex = 3;
            this.ouPathLabel.Text = "확인 중...";
            // 
            // mainPanel
            // 
            this.mainPanel.BackColor = System.Drawing.Color.White;
            this.mainPanel.Controls.Add(this.groupBoxMain);
            this.mainPanel.Location = new System.Drawing.Point(0, 85);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(500, 200);
            this.mainPanel.TabIndex = 1;
            // 
            // groupBoxMain
            // 
            this.groupBoxMain.Controls.Add(this.lblPassword);
            this.groupBoxMain.Controls.Add(this.textBoxNewPassword);
            this.groupBoxMain.Controls.Add(this.buttonCopyPassword);
            this.groupBoxMain.Controls.Add(this.buttonToggleVisibility);
            this.groupBoxMain.Controls.Add(this.lblConfirm);
            this.groupBoxMain.Controls.Add(this.textBoxConfirmPassword);
            this.groupBoxMain.Controls.Add(this.lblStrength);
            this.groupBoxMain.Controls.Add(this.labelPasswordStrength);
            this.groupBoxMain.Controls.Add(this.labelMatchStatus);
            this.groupBoxMain.Controls.Add(this.checkBoxForceChange);
            this.groupBoxMain.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxMain.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.groupBoxMain.Location = new System.Drawing.Point(20, 15);
            this.groupBoxMain.Name = "groupBoxMain";
            this.groupBoxMain.Size = new System.Drawing.Size(450, 170);
            this.groupBoxMain.TabIndex = 0;
            this.groupBoxMain.TabStop = false;
            this.groupBoxMain.Text = "새 비밀번호 설정";
            // 
            // lblPassword
            // 
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPassword.Location = new System.Drawing.Point(20, 30);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(100, 20);
            this.lblPassword.TabIndex = 0;
            this.lblPassword.Text = "새 비밀번호:";
            // 
            // lblConfirm
            // 
            this.lblConfirm.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblConfirm.Location = new System.Drawing.Point(20, 65);
            this.lblConfirm.Name = "lblConfirm";
            this.lblConfirm.Size = new System.Drawing.Size(100, 20);
            this.lblConfirm.TabIndex = 4;
            this.lblConfirm.Text = "비밀번호 확인:";
            // 
            // lblStrength
            // 
            this.lblStrength.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStrength.Location = new System.Drawing.Point(20, 100);
            this.lblStrength.Name = "lblStrength";
            this.lblStrength.Size = new System.Drawing.Size(100, 20);
            this.lblStrength.TabIndex = 6;
            this.lblStrength.Text = "비밀번호 강도:";
            // 
            // generatorPanel
            // 
            this.generatorPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.generatorPanel.Controls.Add(this.groupBoxGenerator);
            this.generatorPanel.Location = new System.Drawing.Point(0, 285);
            this.generatorPanel.Name = "generatorPanel";
            this.generatorPanel.Size = new System.Drawing.Size(500, 220);
            this.generatorPanel.TabIndex = 0;
            // 
            // groupBoxGenerator
            // 
            this.groupBoxGenerator.Controls.Add(this.lblLength);
            this.groupBoxGenerator.Controls.Add(this.numericUpDownLength);
            this.groupBoxGenerator.Controls.Add(this.buttonGenerateMultiple);
            this.groupBoxGenerator.Controls.Add(this.lblGenerated);
            this.groupBoxGenerator.Controls.Add(this.listBoxGeneratedPasswords);
            this.groupBoxGenerator.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxGenerator.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.groupBoxGenerator.Location = new System.Drawing.Point(20, 10);
            this.groupBoxGenerator.Name = "groupBoxGenerator";
            this.groupBoxGenerator.Size = new System.Drawing.Size(450, 200);
            this.groupBoxGenerator.TabIndex = 0;
            this.groupBoxGenerator.TabStop = false;
            this.groupBoxGenerator.Text = "자동 비밀번호 생성 도구";
            // 
            // lblLength
            // 
            this.lblLength.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLength.Location = new System.Drawing.Point(20, 30);
            this.lblLength.Name = "lblLength";
            this.lblLength.Size = new System.Drawing.Size(40, 20);
            this.lblLength.TabIndex = 0;
            this.lblLength.Text = "길이:";
            // 
            // lblGenerated
            // 
            this.lblGenerated.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGenerated.Location = new System.Drawing.Point(20, 60);
            this.lblGenerated.Name = "lblGenerated";
            this.lblGenerated.Size = new System.Drawing.Size(250, 20);
            this.lblGenerated.TabIndex = 3;
            this.lblGenerated.Text = "생성된 비밀번호 (더블클릭으로 선택):";
            // 
            // bottomPanel
            // 
            this.bottomPanel.BackColor = System.Drawing.SystemColors.Control;
            this.bottomPanel.Controls.Add(this.buttonOK);
            this.bottomPanel.Controls.Add(this.buttonCancel);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 516);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(484, 60);
            this.bottomPanel.TabIndex = 2;
            // 
            // UnifiedPasswordForm
            // 
            this.AcceptButton = this.buttonOK;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(484, 576);
            this.Controls.Add(this.generatorPanel);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.headerPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(500, 615);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 615);
            this.Name = "UnifiedPasswordForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "비밀번호 관리";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLength)).EndInit();
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.mainPanel.ResumeLayout(false);
            this.groupBoxMain.ResumeLayout(false);
            this.groupBoxMain.PerformLayout();
            this.generatorPanel.ResumeLayout(false);
            this.groupBoxGenerator.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label userLabel;
        private System.Windows.Forms.Label ouPrefixLabel;
        private System.Windows.Forms.Label ouPathLabel;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.GroupBox groupBoxMain;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblConfirm;
        private System.Windows.Forms.Label lblStrength;
        private System.Windows.Forms.Panel generatorPanel;
        private System.Windows.Forms.GroupBox groupBoxGenerator;
        private System.Windows.Forms.Label lblLength;
        private System.Windows.Forms.Label lblGenerated;
        private System.Windows.Forms.Panel bottomPanel;
    }
}
