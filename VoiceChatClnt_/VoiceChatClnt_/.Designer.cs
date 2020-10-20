namespace VoiceChatClnt_
{
	partial class Lobby
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
			this.lv_lobbyFriends = new System.Windows.Forms.ListView();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.bt_createRoom = new System.Windows.Forms.Button();
			this.bt_enterRoom = new System.Windows.Forms.Button();
			this.lv_lobbyRooms = new System.Windows.Forms.ListView();
			this.label3 = new System.Windows.Forms.Label();
			this.lv_lobbyUsers = new System.Windows.Forms.ListView();
			this.bt_applyFriend = new System.Windows.Forms.Button();
			this.bt_delFriend = new System.Windows.Forms.Button();
			this.lb_user = new System.Windows.Forms.Label();
			this.cb_stateList = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// lv_lobbyFriends
			// 
			this.lv_lobbyFriends.GridLines = true;
			this.lv_lobbyFriends.HideSelection = false;
			this.lv_lobbyFriends.Location = new System.Drawing.Point(443, 235);
			this.lv_lobbyFriends.Name = "lv_lobbyFriends";
			this.lv_lobbyFriends.Size = new System.Drawing.Size(295, 115);
			this.lv_lobbyFriends.TabIndex = 0;
			this.lv_lobbyFriends.UseCompatibleStateImageBehavior = false;
			this.lv_lobbyFriends.View = System.Windows.Forms.View.Details;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.label1.Location = new System.Drawing.Point(19, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 21);
			this.label1.TabIndex = 2;
			this.label1.Text = "대화방 목록";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.label2.Location = new System.Drawing.Point(439, 211);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 21);
			this.label2.TabIndex = 3;
			this.label2.Text = "친구 목록";
			// 
			// bt_createRoom
			// 
			this.bt_createRoom.Location = new System.Drawing.Point(45, 381);
			this.bt_createRoom.Name = "bt_createRoom";
			this.bt_createRoom.Size = new System.Drawing.Size(145, 23);
			this.bt_createRoom.TabIndex = 4;
			this.bt_createRoom.Text = "대화방 생성";
			this.bt_createRoom.UseVisualStyleBackColor = true;
			this.bt_createRoom.Click += new System.EventHandler(this.bt_createRoom_Click);
			// 
			// bt_enterRoom
			// 
			this.bt_enterRoom.Location = new System.Drawing.Point(222, 381);
			this.bt_enterRoom.Name = "bt_enterRoom";
			this.bt_enterRoom.Size = new System.Drawing.Size(145, 23);
			this.bt_enterRoom.TabIndex = 5;
			this.bt_enterRoom.Text = "대화방 입장";
			this.bt_enterRoom.UseVisualStyleBackColor = true;
			this.bt_enterRoom.Click += new System.EventHandler(this.bt_enterRoom_Click);
			// 
			// lv_lobbyRooms
			// 
			this.lv_lobbyRooms.GridLines = true;
			this.lv_lobbyRooms.HideSelection = false;
			this.lv_lobbyRooms.Location = new System.Drawing.Point(23, 51);
			this.lv_lobbyRooms.Name = "lv_lobbyRooms";
			this.lv_lobbyRooms.Size = new System.Drawing.Size(391, 299);
			this.lv_lobbyRooms.TabIndex = 7;
			this.lv_lobbyRooms.UseCompatibleStateImageBehavior = false;
			this.lv_lobbyRooms.View = System.Windows.Forms.View.Details;
			this.lv_lobbyRooms.SelectedIndexChanged += new System.EventHandler(this.lv_lobbyRooms_SelectedIndexChanged);
			this.lv_lobbyRooms.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lv_lobbyRooms_MouseDoubleClick);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.label3.Location = new System.Drawing.Point(439, 27);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 21);
			this.label3.TabIndex = 9;
			this.label3.Text = "유저 목록";
			// 
			// lv_lobbyUsers
			// 
			this.lv_lobbyUsers.GridLines = true;
			this.lv_lobbyUsers.HideSelection = false;
			this.lv_lobbyUsers.Location = new System.Drawing.Point(443, 51);
			this.lv_lobbyUsers.Name = "lv_lobbyUsers";
			this.lv_lobbyUsers.Size = new System.Drawing.Size(295, 157);
			this.lv_lobbyUsers.TabIndex = 8;
			this.lv_lobbyUsers.UseCompatibleStateImageBehavior = false;
			this.lv_lobbyUsers.View = System.Windows.Forms.View.Details;
			this.lv_lobbyUsers.CursorChanged += new System.EventHandler(this.lv_lobbyUsers_CursorChanged);
			// 
			// bt_applyFriend
			// 
			this.bt_applyFriend.Location = new System.Drawing.Point(396, 381);
			this.bt_applyFriend.Name = "bt_applyFriend";
			this.bt_applyFriend.Size = new System.Drawing.Size(145, 23);
			this.bt_applyFriend.TabIndex = 10;
			this.bt_applyFriend.Text = "친구 신청";
			this.bt_applyFriend.UseVisualStyleBackColor = true;
			this.bt_applyFriend.Click += new System.EventHandler(this.bt_applyFriend_Click);
			// 
			// bt_delFriend
			// 
			this.bt_delFriend.Location = new System.Drawing.Point(570, 381);
			this.bt_delFriend.Name = "bt_delFriend";
			this.bt_delFriend.Size = new System.Drawing.Size(145, 23);
			this.bt_delFriend.TabIndex = 11;
			this.bt_delFriend.Text = "친구 삭제";
			this.bt_delFriend.UseVisualStyleBackColor = true;
			this.bt_delFriend.Click += new System.EventHandler(this.bt_delFriend_Click);
			// 
			// lb_user
			// 
			this.lb_user.AutoSize = true;
			this.lb_user.Location = new System.Drawing.Point(122, 35);
			this.lb_user.Name = "lb_user";
			this.lb_user.Size = new System.Drawing.Size(38, 12);
			this.lb_user.TabIndex = 12;
			this.lb_user.Text = "label4";
			// 
			// cb_stateList
			// 
			this.cb_stateList.FormattingEnabled = true;
			this.cb_stateList.Items.AddRange(new object[] {
            "온라인",
            "빠쁨",
            "숨기"});
			this.cb_stateList.Location = new System.Drawing.Point(617, 24);
			this.cb_stateList.Name = "cb_stateList";
			this.cb_stateList.Size = new System.Drawing.Size(121, 20);
			this.cb_stateList.TabIndex = 13;
			this.cb_stateList.SelectedIndexChanged += new System.EventHandler(this.cb_stateList_SelectedIndexChanged);
			// 
			// Lobby
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(767, 450);
			this.Controls.Add(this.cb_stateList);
			this.Controls.Add(this.lb_user);
			this.Controls.Add(this.bt_delFriend);
			this.Controls.Add(this.bt_applyFriend);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.lv_lobbyUsers);
			this.Controls.Add(this.lv_lobbyRooms);
			this.Controls.Add(this.bt_enterRoom);
			this.Controls.Add(this.bt_createRoom);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lv_lobbyFriends);
			this.Name = "Lobby";
			this.Text = "Lobby";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Lobby_FormClosing);
			this.Load += new System.EventHandler(this.Lobby_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView lv_lobbyFriends;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button bt_createRoom;
		private System.Windows.Forms.Button bt_enterRoom;
		private System.Windows.Forms.ListView lv_lobbyRooms;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView lv_lobbyUsers;
		private System.Windows.Forms.Button bt_applyFriend;
		private System.Windows.Forms.Button bt_delFriend;
		private System.Windows.Forms.Label lb_user;
		private System.Windows.Forms.ComboBox cb_stateList;
	}
}