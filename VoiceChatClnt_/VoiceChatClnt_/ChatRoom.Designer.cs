namespace VoiceChatClnt_
{
    partial class ChatRoom
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
            this.lv_parti = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.bt_invite = new System.Windows.Forms.Button();
            this.bt_exitRoom = new System.Windows.Forms.Button();
            this.lv_roomFriends = new System.Windows.Forms.ListView();
            this.label2 = new System.Windows.Forms.Label();
            this.rtb_chatWindow = new System.Windows.Forms.RichTextBox();
            this.tb_inputMsg = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lv_parti
            // 
            this.lv_parti.GridLines = true;
            this.lv_parti.HideSelection = false;
            this.lv_parti.Location = new System.Drawing.Point(272, 44);
            this.lv_parti.Name = "lv_parti";
            this.lv_parti.Size = new System.Drawing.Size(219, 144);
            this.lv_parti.TabIndex = 0;
            this.lv_parti.UseCompatibleStateImageBehavior = false;
            this.lv_parti.View = System.Windows.Forms.View.Details;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(268, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 21);
            this.label1.TabIndex = 1;
            this.label1.Text = "참여자 목록";
            // 
            // bt_invite
            // 
            this.bt_invite.Location = new System.Drawing.Point(272, 396);
            this.bt_invite.Name = "bt_invite";
            this.bt_invite.Size = new System.Drawing.Size(107, 23);
            this.bt_invite.TabIndex = 2;
            this.bt_invite.Text = "친구 초대";
            this.bt_invite.UseVisualStyleBackColor = true;
            this.bt_invite.Click += new System.EventHandler(this.bt_invite_Click);
            // 
            // bt_exitRoom
            // 
            this.bt_exitRoom.Location = new System.Drawing.Point(384, 396);
            this.bt_exitRoom.Name = "bt_exitRoom";
            this.bt_exitRoom.Size = new System.Drawing.Size(107, 23);
            this.bt_exitRoom.TabIndex = 3;
            this.bt_exitRoom.Text = "나가기";
            this.bt_exitRoom.UseVisualStyleBackColor = true;
            this.bt_exitRoom.Click += new System.EventHandler(this.bt_exitRoom_Click);
            // 
            // lv_roomFriends
            // 
            this.lv_roomFriends.GridLines = true;
            this.lv_roomFriends.HideSelection = false;
            this.lv_roomFriends.Location = new System.Drawing.Point(272, 227);
            this.lv_roomFriends.Name = "lv_roomFriends";
            this.lv_roomFriends.Size = new System.Drawing.Size(219, 144);
            this.lv_roomFriends.TabIndex = 4;
            this.lv_roomFriends.UseCompatibleStateImageBehavior = false;
            this.lv_roomFriends.View = System.Windows.Forms.View.Details;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(268, 203);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 21);
            this.label2.TabIndex = 5;
            this.label2.Text = "친구 목록";
            // 
            // rtb_chatWindow
            // 
            this.rtb_chatWindow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtb_chatWindow.Location = new System.Drawing.Point(13, 44);
            this.rtb_chatWindow.Name = "rtb_chatWindow";
            this.rtb_chatWindow.Size = new System.Drawing.Size(233, 327);
            this.rtb_chatWindow.TabIndex = 6;
            this.rtb_chatWindow.Text = "";
            // 
            // tb_inputMsg
            // 
            this.tb_inputMsg.Location = new System.Drawing.Point(13, 396);
            this.tb_inputMsg.Name = "tb_inputMsg";
            this.tb_inputMsg.Size = new System.Drawing.Size(233, 21);
            this.tb_inputMsg.TabIndex = 7;
            this.tb_inputMsg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_inputMsg_KeyDown);
            // 
            // ChatRoom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 447);
            this.Controls.Add(this.tb_inputMsg);
            this.Controls.Add(this.rtb_chatWindow);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lv_roomFriends);
            this.Controls.Add(this.bt_exitRoom);
            this.Controls.Add(this.bt_invite);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lv_parti);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ChatRoom";
            this.Text = "ChatRoom";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChatRoom_FormClosed);
            this.Load += new System.EventHandler(this.ChatRoom_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lv_parti;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bt_invite;
        private System.Windows.Forms.Button bt_exitRoom;
        private System.Windows.Forms.ListView lv_roomFriends;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox rtb_chatWindow;
        private System.Windows.Forms.TextBox tb_inputMsg;
    }
}