namespace VoiceChatClnt_
{
	partial class InputPass
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
			this.tb_pass = new System.Windows.Forms.TextBox();
			this.bt_sendPass = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.label1.Location = new System.Drawing.Point(35, 44);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 21);
			this.label1.TabIndex = 0;
			this.label1.Text = "비밀번호";
			// 
			// tb_pass
			// 
			this.tb_pass.Location = new System.Drawing.Point(116, 43);
			this.tb_pass.Name = "tb_pass";
			this.tb_pass.Size = new System.Drawing.Size(203, 21);
			this.tb_pass.TabIndex = 1;
			// 
			// bt_sendPass
			// 
			this.bt_sendPass.Location = new System.Drawing.Point(184, 85);
			this.bt_sendPass.Name = "bt_sendPass";
			this.bt_sendPass.Size = new System.Drawing.Size(135, 23);
			this.bt_sendPass.TabIndex = 2;
			this.bt_sendPass.Text = "입력";
			this.bt_sendPass.UseVisualStyleBackColor = true;
			this.bt_sendPass.Click += new System.EventHandler(this.bt_sendPass_Click);
			// 
			// InputPass
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(362, 146);
			this.Controls.Add(this.bt_sendPass);
			this.Controls.Add(this.tb_pass);
			this.Controls.Add(this.label1);
			this.Name = "InputPass";
			this.Text = "InputPass";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tb_pass;
		private System.Windows.Forms.Button bt_sendPass;
	}
}