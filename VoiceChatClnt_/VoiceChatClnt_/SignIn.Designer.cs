namespace VoiceChatClnt_
{
	partial class SignIn
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
            this.label1 = new System.Windows.Forms.Label();
            this.tb_signId = new System.Windows.Forms.TextBox();
            this.tb_signPw = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_signPw2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_signInName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.bt_signIn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(31, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "아이디";
            // 
            // tb_signId
            // 
            this.tb_signId.Location = new System.Drawing.Point(149, 37);
            this.tb_signId.Name = "tb_signId";
            this.tb_signId.Size = new System.Drawing.Size(171, 21);
            this.tb_signId.TabIndex = 1;
            // 
            // tb_signPw
            // 
            this.tb_signPw.Location = new System.Drawing.Point(149, 85);
            this.tb_signPw.Name = "tb_signPw";
            this.tb_signPw.Size = new System.Drawing.Size(171, 21);
            this.tb_signPw.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(31, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "패스워드";
            // 
            // tb_signPw2
            // 
            this.tb_signPw2.Location = new System.Drawing.Point(149, 134);
            this.tb_signPw2.Name = "tb_signPw2";
            this.tb_signPw2.Size = new System.Drawing.Size(171, 21);
            this.tb_signPw2.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label3.Location = new System.Drawing.Point(31, 134);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 21);
            this.label3.TabIndex = 4;
            this.label3.Text = "패스워드 확인";
            // 
            // tb_signInName
            // 
            this.tb_signInName.Location = new System.Drawing.Point(149, 182);
            this.tb_signInName.Name = "tb_signInName";
            this.tb_signInName.Size = new System.Drawing.Size(171, 21);
            this.tb_signInName.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label4.Location = new System.Drawing.Point(31, 182);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 21);
            this.label4.TabIndex = 6;
            this.label4.Text = "닉네임";
            // 
            // bt_signIn
            // 
            this.bt_signIn.Location = new System.Drawing.Point(245, 244);
            this.bt_signIn.Name = "bt_signIn";
            this.bt_signIn.Size = new System.Drawing.Size(75, 23);
            this.bt_signIn.TabIndex = 8;
            this.bt_signIn.Text = "가입";
            this.bt_signIn.UseVisualStyleBackColor = true;
            this.bt_signIn.Click += new System.EventHandler(this.bt_signIn_Click_1);
            // 
            // SignIn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 439);
            this.Controls.Add(this.bt_signIn);
            this.Controls.Add(this.tb_signInName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_signPw2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tb_signPw);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_signId);
            this.Controls.Add(this.label1);
            this.Name = "SignIn";
            this.Text = "SignIn";
            this.Load += new System.EventHandler(this.SignIn_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tb_signId;
		private System.Windows.Forms.TextBox tb_signPw;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tb_signPw2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tb_signInName;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button bt_signIn;
	}
}