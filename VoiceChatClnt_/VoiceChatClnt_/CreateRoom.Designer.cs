namespace VoiceChatClnt_
{
	partial class CreateRoom
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
            this.components = new System.ComponentModel.Container();
            this.tb_roomTitle = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.bt_createRoom = new System.Windows.Forms.Button();
            this.tb_roompass = new System.Windows.Forms.TextBox();
            this.cb_roompass = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SuspendLayout();
            // 
            // tb_roomTitle
            // 
            this.tb_roomTitle.Location = new System.Drawing.Point(122, 9);
            this.tb_roomTitle.Name = "tb_roomTitle";
            this.tb_roomTitle.Size = new System.Drawing.Size(220, 21);
            this.tb_roomTitle.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(52, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 21);
            this.label1.TabIndex = 1;
            this.label1.Text = "방 제목";
            // 
            // bt_createRoom
            // 
            this.bt_createRoom.Location = new System.Drawing.Point(226, 68);
            this.bt_createRoom.Name = "bt_createRoom";
            this.bt_createRoom.Size = new System.Drawing.Size(116, 23);
            this.bt_createRoom.TabIndex = 2;
            this.bt_createRoom.Text = "생성";
            this.bt_createRoom.UseVisualStyleBackColor = true;
            this.bt_createRoom.Click += new System.EventHandler(this.bt_createRoom_Click);
            // 
            // tb_roompass
            // 
            this.tb_roompass.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_roompass.Enabled = false;
            this.tb_roompass.Location = new System.Drawing.Point(122, 40);
            this.tb_roompass.Name = "tb_roompass";
            this.tb_roompass.PasswordChar = '●';
            this.tb_roompass.Size = new System.Drawing.Size(220, 21);
            this.tb_roompass.TabIndex = 3;
            // 
            // cb_roompass
            // 
            this.cb_roompass.AutoSize = true;
            this.cb_roompass.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cb_roompass.Location = new System.Drawing.Point(26, 38);
            this.cb_roompass.Name = "cb_roompass";
            this.cb_roompass.Size = new System.Drawing.Size(93, 25);
            this.cb_roompass.TabIndex = 5;
            this.cb_roompass.Text = "비밀번호";
            this.cb_roompass.UseVisualStyleBackColor = true;
            this.cb_roompass.CheckedChanged += new System.EventHandler(this.cb_roompass_CheckedChanged);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // CreateRoom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 102);
            this.Controls.Add(this.cb_roompass);
            this.Controls.Add(this.tb_roompass);
            this.Controls.Add(this.bt_createRoom);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tb_roomTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateRoom";
            this.Text = "CreateRoom";
            this.Load += new System.EventHandler(this.CreateRoom_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox tb_roomTitle;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button bt_createRoom;
        private System.Windows.Forms.TextBox tb_roompass;
        private System.Windows.Forms.CheckBox cb_roompass;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    }
}