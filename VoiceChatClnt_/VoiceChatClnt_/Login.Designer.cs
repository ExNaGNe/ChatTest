﻿namespace VoiceChatClnt_
{
	partial class Login
	{
		/// <summary>
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form 디자이너에서 생성한 코드

		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
		/// </summary>
		private void InitializeComponent()
		{
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_id = new System.Windows.Forms.TextBox();
            this.tb_pw = new System.Windows.Forms.TextBox();
            this.bt_login = new System.Windows.Forms.Button();
            this.bt_signIn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("나눔고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(27, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "아이디";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("나눔고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(15, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "비밀번호";
            // 
            // tb_id
            // 
            this.tb_id.Location = new System.Drawing.Point(83, 20);
            this.tb_id.Name = "tb_id";
            this.tb_id.Size = new System.Drawing.Size(181, 21);
            this.tb_id.TabIndex = 2;
            // 
            // tb_pw
            // 
            this.tb_pw.Location = new System.Drawing.Point(83, 47);
            this.tb_pw.Name = "tb_pw";
            this.tb_pw.PasswordChar = '●';
            this.tb_pw.Size = new System.Drawing.Size(181, 21);
            this.tb_pw.TabIndex = 3;
            this.tb_pw.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_pw_KeyDown);
            // 
            // bt_login
            // 
            this.bt_login.Font = new System.Drawing.Font("나눔고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.bt_login.Location = new System.Drawing.Point(270, 20);
            this.bt_login.Name = "bt_login";
            this.bt_login.Size = new System.Drawing.Size(80, 77);
            this.bt_login.TabIndex = 4;
            this.bt_login.Text = "로그인";
            this.bt_login.UseVisualStyleBackColor = true;
            this.bt_login.Click += new System.EventHandler(this.bt_login_Click);
            // 
            // bt_signIn
            // 
            this.bt_signIn.Cursor = System.Windows.Forms.Cursors.Default;
            this.bt_signIn.Location = new System.Drawing.Point(164, 74);
            this.bt_signIn.Name = "bt_signIn";
            this.bt_signIn.Size = new System.Drawing.Size(100, 23);
            this.bt_signIn.TabIndex = 5;
            this.bt_signIn.Text = "회원가입";
            this.bt_signIn.UseVisualStyleBackColor = true;
            this.bt_signIn.Click += new System.EventHandler(this.bt_signIn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.bt_signIn);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.bt_login);
            this.groupBox1.Controls.Add(this.tb_id);
            this.groupBox1.Controls.Add(this.tb_pw);
            this.groupBox1.Location = new System.Drawing.Point(12, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(364, 116);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 136);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "로그인";
            this.Load += new System.EventHandler(this.Login_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tb_id;
		private System.Windows.Forms.TextBox tb_pw;
		private System.Windows.Forms.Button bt_login;
		private System.Windows.Forms.Button bt_signIn;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

