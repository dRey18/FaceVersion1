namespace FaceManagement
{
    partial class frmFaceManagementSetPhoto
    {

        ///必需的设计器变量。

        private System.ComponentModel.IContainer components = null;


        //清理所有正在使用的资源。

        //<param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码


        //设计器支持所需的方法 - 不要
        //使用代码编辑器修改此方法的内容。

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFaceManagementSetPhoto));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnLogin = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxEmployeeNo = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnDel = new System.Windows.Forms.Button();
            this.btnGet = new System.Windows.Forms.Button();
            this.btnSet = new System.Windows.Forms.Button();
            this.textBoxFilePath = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxFDID = new System.Windows.Forms.TextBox();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBoxFaceType = new System.Windows.Forms.ComboBox();
            this.pictureBoxFace = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFace)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(723, 106);
            this.panel1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Consolas", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(225, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(382, 51);
            this.label1.TabIndex = 1;
            this.label1.Text = "Carga de imagen";
            // 
            // btnLogin
            // 
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.Location = new System.Drawing.Point(29, 124);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(103, 29);
            this.btnLogin.TabIndex = 4;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.ForeColor = System.Drawing.Color.DimGray;
            this.label2.Location = new System.Drawing.Point(161, 129);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 18);
            this.label2.TabIndex = 57;
            this.label2.Text = "Employee No";
            // 
            // textBoxEmployeeNo
            // 
            this.textBoxEmployeeNo.Location = new System.Drawing.Point(307, 128);
            this.textBoxEmployeeNo.Name = "textBoxEmployeeNo";
            this.textBoxEmployeeNo.Size = new System.Drawing.Size(159, 25);
            this.textBoxEmployeeNo.TabIndex = 56;
            this.textBoxEmployeeNo.Text = "3";
            // 
            // btnBrowse
            // 
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowse.Location = new System.Drawing.Point(344, 258);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(93, 30);
            this.btnBrowse.TabIndex = 61;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnDel
            // 
            this.btnDel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDel.Location = new System.Drawing.Point(545, 372);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(103, 29);
            this.btnDel.TabIndex = 67;
            this.btnDel.Text = "Delete";
            this.btnDel.UseVisualStyleBackColor = true;
            //this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // btnGet
            // 
            this.btnGet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGet.Location = new System.Drawing.Point(363, 372);
            this.btnGet.Name = "btnGet";
            this.btnGet.Size = new System.Drawing.Size(103, 29);
            this.btnGet.TabIndex = 66;
            this.btnGet.Text = "Get";
            this.btnGet.UseVisualStyleBackColor = true;
            //this.btnGet.Click += new System.EventHandler(this.btnGet_Click);
            // 
            // btnSet
            // 
            this.btnSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSet.Location = new System.Drawing.Point(186, 372);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(103, 29);
            this.btnSet.TabIndex = 65;
            this.btnSet.Text = "Set";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.button6_Click);
            // 
            // textBoxFilePath
            // 
            this.textBoxFilePath.Location = new System.Drawing.Point(308, 158);
            this.textBoxFilePath.Name = "textBoxFilePath";
            this.textBoxFilePath.Size = new System.Drawing.Size(158, 25);
            this.textBoxFilePath.TabIndex = 71;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.ForeColor = System.Drawing.Color.DimGray;
            this.label7.Location = new System.Drawing.Point(161, 160);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 18);
            this.label7.TabIndex = 72;
            this.label7.Text = "FilePath";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.ForeColor = System.Drawing.Color.DimGray;
            this.label8.Location = new System.Drawing.Point(161, 190);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(168, 18);
            this.label8.TabIndex = 73;
            this.label8.Text = "Face DataBase No";
            // 
            // textBoxFDID
            // 
            this.textBoxFDID.Location = new System.Drawing.Point(307, 189);
            this.textBoxFDID.Name = "textBoxFDID";
            this.textBoxFDID.Size = new System.Drawing.Size(159, 25);
            this.textBoxFDID.TabIndex = 74;
            this.textBoxFDID.Text = "1";
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Items.AddRange(new object[] {
            "English",
            "Chinese"});
            this.comboBoxLanguage.Location = new System.Drawing.Point(596, 440);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(115, 26);
            this.comboBoxLanguage.TabIndex = 75;
            this.comboBoxLanguage.Text = "English";
            //this.comboBoxLanguage.SelectedIndexChanged += new System.EventHandler(this.comboBoxLanguage_SelectedIndexChanged);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(37, 374);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(87, 27);
            this.btnClear.TabIndex = 76;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.ForeColor = System.Drawing.Color.DimGray;
            this.label9.Location = new System.Drawing.Point(161, 218);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(138, 18);
            this.label9.TabIndex = 77;
            this.label9.Text = "Face Lib Type";
            // 
            // comboBoxFaceType
            // 
            this.comboBoxFaceType.FormattingEnabled = true;
            this.comboBoxFaceType.Items.AddRange(new object[] {
            "blackFD",
            "staticFD"});
            this.comboBoxFaceType.Location = new System.Drawing.Point(308, 216);
            this.comboBoxFaceType.Name = "comboBoxFaceType";
            this.comboBoxFaceType.Size = new System.Drawing.Size(158, 26);
            this.comboBoxFaceType.TabIndex = 78;
            this.comboBoxFaceType.Text = "blackFD";
            // 
            // pictureBoxFace
            // 
            this.pictureBoxFace.BackColor = System.Drawing.Color.White;
            this.pictureBoxFace.Location = new System.Drawing.Point(14, 171);
            this.pictureBoxFace.Name = "pictureBoxFace";
            this.pictureBoxFace.Size = new System.Drawing.Size(134, 185);
            this.pictureBoxFace.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxFace.TabIndex = 59;
            this.pictureBoxFace.TabStop = false;
            // 
            // FaceManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(723, 474);
            this.Controls.Add(this.comboBoxFaceType);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.comboBoxLanguage);
            this.Controls.Add(this.textBoxFDID);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxFilePath);
            this.Controls.Add(this.btnDel);
            this.Controls.Add(this.btnGet);
            this.Controls.Add(this.btnSet);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.pictureBoxFace);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxEmployeeNo);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FaceManagement";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ACSDemo";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFace)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxEmployeeNo;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnDel;
        private System.Windows.Forms.Button btnGet;
        private System.Windows.Forms.Button btnSet;
        private System.Windows.Forms.TextBox textBoxFilePath;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxFDID;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox comboBoxFaceType;
        private System.Windows.Forms.PictureBox pictureBoxFace;
    }
}

