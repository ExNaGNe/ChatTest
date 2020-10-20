using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Timers;

namespace VoiceChatClnt_
{
	public partial class Lobby : Form
	{
		VoiceChatTCP usrCommunicator = null;
		VoiceChatTCP lobbyCommunicator = null; // new VoiceChatTCP("10.10.20.47", 7000);
											   //VoiceChatUDP chatCommunicator = new VoiceChatUDP("10.10.20.213", 44444);
		static Mutex lobbyMtx = new Mutex();

		static Thread userServThread;
		static Thread lobbyServThread;
		

		ChatRoom chatRoom;

		User myUserData;
		int roomNum = -1;
		string roomName = "";

        System.Timers.Timer timer;


        public Lobby(User _user)
		{
			InitializeComponent();
			usrCommunicator = new VoiceChatTCP("10.10.20.48", 10005);	// 유저 서버 연결
			lobbyCommunicator = new VoiceChatTCP("10.10.20.47", 7001);	// 로비 서버 연결

			myUserData = _user;
			lb_user.Text = myUserData.ID;		
		}

        private void Lobby_Load(object sender, EventArgs e)
        {
            ThreadsRun();
            InitListColumns(ref lv_lobbyRooms, "번호", "제목", "인원수");
            InitListColumns(ref lv_lobbyUsers, "아이디", "닉네임", "상태");
            InitListColumns(ref lv_lobbyFriends, "아이디", "닉네임", "상태", "위치");
        }

        private void ThreadsRun()		// 각 서버와 통신 할 스레드 생성
		{			
			userServThread = new Thread(new ThreadStart(userServ));		// 유저 서버에서 신호를 받아 처리하는 스레드 생성
			lobbyServThread = new Thread(new ThreadStart(lobbyServ));	// 로비 서버에서 신호를 받아 처리하는 스레드 생성
			
			userServThread.Start();
			lobbyServThread.Start();			
		}

		private void bt_createRoom_Click(object sender, EventArgs e)
		{
			CreateRoom createRoom = new CreateRoom(lobbyCommunicator);
			createRoom.Show();
		}

		private void bt_enterRoom_Click(object sender, EventArgs e)
		{			
			lobbyCommunicator.SendInt(3);

			string strRoomNum = GetElementBySelectedRow(ref lv_lobbyRooms, 0);
			roomName = GetElementBySelectedRow(ref lv_lobbyRooms, 1);
			Console.WriteLine("들어갈 때 보낸 방 번호 : {0} ",strRoomNum);
			roomNum = int.Parse(strRoomNum);
			lobbyCommunicator.SendInt(roomNum);			
		}		

		private void userServ() //유저 서버에서 신호를 받아 처리하는 스레드
		{
			int sig = -1;

			Console.WriteLine("아이디 :  {0}, 닉네임 : {1} ", myUserData.ID, myUserData.Nickname);
			
			usrCommunicator.SendLinkedStr(myUserData.ID, myUserData.Nickname);

			while (true)
			{
				sig = usrCommunicator.RecvInt();
				
				if (sig == 0)
					ReloadFromUsrServ();
				else if (sig == 1)
					AddFriend();		
				else if (sig == 2)
					InviteAccepted();
			}
		}

		private void lobbyServ()    // 로비 서버에서 신호를 받아 처리하는 스레드
		{
			lobbyCommunicator.SendStr(myUserData.ID);

            timer = new System.Timers.Timer(3000);  //동기화 타이머(3초)
            timer.Elapsed += lobbyUpdate;
            timer.AutoReset = true;

            this.Invoke(new Action(delegate ()
			{
				cb_stateList.SelectedIndex = 0;				
			}));

			/*
			lobbyCommunicator.SendInt(4);
			lobbyCommunicator.SendLinkedStr(myUserData.ID, myUserData.Nickname, "1");
			*/
            lobbyUpdate(null, null);
            timer.Enabled = true;
            int sig = 0;
			while (true)
			{
				try { 
				sig = lobbyCommunicator.RecvInt();
					Console.WriteLine("받은 직후 신호 : {0}", sig);
                
				lobbyMtx.WaitOne();
				if (sig == -1)
					MessageBox.Show("잘못된 비밀번호");
				else if (sig == 1)
					VisitRoom();
				else if (sig == 2)
					ReloadFromLobbyServ();
				else if (sig == 5)			
					SendPassword();						
				else if (sig == 6)
					CreatedAndVisit(lobbyCommunicator.RecvInt());
				else if (sig == 7)
					UpdateChatRoomList();
				lobbyMtx.ReleaseMutex();
				}
				catch(Exception e)
				{
					Console.WriteLine("신호 : {0}", sig);
					timer.Close();
					userServThread.Abort();
					lobbyServThread.Abort();
					ExitConnect(usrCommunicator);
					ExitConnect(lobbyCommunicator);
				}
			}
		}

		public void UpdateChatRoomList()
		{
			List<string> strUserList = lobbyCommunicator.RecvStringList();
			chatRoom.UpdatePartiList(strUserList);			
		}

		public void CreatedAndVisit(int recvRoomNum)
		{
			Console.WriteLine("받은 방번호 : {0}", recvRoomNum);
			
			roomNum = recvRoomNum;
			timer.Enabled = true;			
			lobbyCommunicator.SendInt(3);
			lobbyCommunicator.SendInt(roomNum);			
		}

		public void SendPassword()
		{
			timer.Enabled = false;
			this.Invoke(new Action(delegate ()
			{
				InputPass inputPass = new InputPass(lobbyCommunicator);
				inputPass.Show();
			}));
			timer.Enabled = true;
		}

		private void lobbyUpdate(object sender, ElapsedEventArgs e)  // 로비 서버에 지속적으로 업데이트 신호를 보내는 스레드
        {
            lobbyMtx.WaitOne();
            lobbyCommunicator.SendInt(2);
            lobbyMtx.ReleaseMutex();
        }

        public void InitListsUsr()	// Form에 있는 리스트 뷰 초기화
		{
			this.Invoke(new Action(delegate ()
			{
				lv_lobbyUsers.Items.Clear();
				lv_lobbyFriends.Items.Clear();				

			}));
		}

        public void InitListsLobby()
        {
            this.Invoke(new Action(delegate ()
            {
                lv_lobbyRooms.Items.Clear();
            }));
        }

		public void VisitRoom()
		{
			this.Invoke(new Action(delegate ()
			{
				Console.WriteLine("들어가려는 방 번호 : {0}",roomNum);
				chatRoom = new ChatRoom(myUserData, usrCommunicator, lobbyCommunicator, roomNum, roomName);
				chatRoom.FormClosed += (object sender, FormClosedEventArgs e) => { timer.Enabled = true; chatRoom = null; };
				chatRoom.Show();
				
			}));		
		}

		public void ReloadFromUsrServ()	// 유저 서버에서 업데이트 할 데이터를 받고 리스트 뷰에 적용
		{
            InitListsUsr();

			List<string> strUsers = usrCommunicator.RecvStringList();
			List<string> strFriends = usrCommunicator.RecvStringList();			

			this.Invoke(new Action(delegate () {

				SetRowsAsListStr(ref lv_lobbyUsers, strUsers, 1);
				SetRowsAsListStr(ref lv_lobbyFriends, strFriends, 1);

				InitListSize(ref lv_lobbyUsers);
				InitListSize(ref lv_lobbyFriends);

				if (chatRoom != null)
					chatRoom.UpdateFriendList(strFriends);
				
			}));
		}		

		public void ReloadFromLobbyServ()	// 로비 서버에서 업데이트 할 데이터를 받고 리스트 뷰에 적용
		{
            InitListsLobby();

            List<string> strRooms = lobbyCommunicator.RecvStringList();            

            if (strRooms.Count <= 0)
                return;

			this.Invoke(new Action(delegate () {

				SetRowsAsListStr(ref lv_lobbyRooms, strRooms);

			}));
		}

		public void AddFriend()	// 친구 추가 신호를 받고 처리
		{
			string strUserData = usrCommunicator.RecvString();
			string[] aryStrUserData = strUserData.Split(",".ToCharArray());

			DialogResult dr = MessageBox.Show($"{aryStrUserData[0]}님께서 친구신청을 하셨습니다.", "친구신청", MessageBoxButtons.YesNo);
			if (dr == DialogResult.Yes)
			{
				usrCommunicator.SendInt(3);
				usrCommunicator.SendStr(aryStrUserData[0]);
			}
		}

		public void InviteAccepted()
		{
			string origin = usrCommunicator.RecvString();
			string nick = origin.Split(",".ToCharArray())[0];
			string num = origin.Split(",".ToCharArray())[1];
			if (MessageBox.Show(this, "친구 초대", $"{nick}님이 {num}번 방으로 초대하셨습니다.", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				lobbyCommunicator.SendInt(3);
				lobbyCommunicator.SendInt(int.Parse(num));
			}
		}

		public static void InitListColumns(ref ListView lv, params string[] args) // 리스트 뷰 컬럼 추가
		{
			foreach (string str in args)
				lv.Columns.Add(str);			
		}

		void InitListSize(ref ListView lv)	// 리스트 크기 조정
		{
			foreach (ColumnHeader col in lv.Columns)
				col.Width = -2;
		}

		public static void SetRowsAsListStr(ref ListView lv, List<string> args, int isState = 0)    // 리스트에 내용 추가
		{
			int stateNum = 0;
			
			foreach (string row in args)
			{
				string[] row_arr = row.Split(",".ToCharArray());
				if (isState == 1 && int.TryParse(row_arr[2], out stateNum))
					row_arr[2] = GetStateValue(stateNum);
				ListViewItem item = new ListViewItem(row_arr);
				lv.Items.Add(item);
			}
			
		}

		static string GetStateValue(int state)	// 상태 정보를 문자열로 변경
		{
			if (state == 1)
				return "온라인";
			else if (state == 2)
				return "바쁨";
			else if (state == 3)
				return "숨기";
			else
				return "???";
		}

		private void bt_applyFriend_Click(object sender, EventArgs e)	// 친구 신청 버튼 이벤트
		{			
			string selectId = GetElementBySelectedRow(ref lv_lobbyUsers, 0);

			if (CheckExistId(ref lv_lobbyFriends, selectId))
				return;

			usrCommunicator.SendInt(1);
			usrCommunicator.SendStr(selectId);			
		}

		public string GetElementBySelectedRow(ref ListView lv, int index)	// 인자로 전달 된 리스트 뷰의 현재 선택된 행에서 id항목을 받환
		{
			ListView.SelectedListViewItemCollection items = lv.SelectedItems;

			if (items.Count <= 0)
				return "";

			ListViewItem lvItem = items[0];
			Console.WriteLine(lvItem.SubItems[index].Text);

			return lvItem.SubItems[index].Text;
		}

		private bool CheckExistId(ref ListView lv, string id)	// 인자로 전달된 리스트 뷰에 인자로 전달한 id와 같은 id가 있는지 검사
		{
			foreach (ListViewItem item in lv.Items)
			{
				if (item.SubItems[0].Equals(id))
					return true;
			}
			
			return false;
		}

		private void Lobby_FormClosing(object sender, FormClosingEventArgs e)  //	폼이 종료될 때 서버와 연결 종료
		{
            timer.Close();
			userServThread.Abort();
			lobbyServThread.Abort();
			ExitConnect(usrCommunicator);
			ExitConnect(lobbyCommunicator);
		}

		private void ExitConnect(VoiceChatTCP comm)	// 인자로 전달된 서버 연결 객체의 연결 종료
		{			
			comm.SendInt(-1);
			comm.Close();			
			comm= null;
		}

		private void bt_delFriend_Click(object sender, EventArgs e)		// 친구 삭제 이벤트
		{
			string selectId = GetElementBySelectedRow(ref lv_lobbyFriends, 0);
			if (selectId.Equals(""))
				return;
			usrCommunicator.SendInt(4);
			usrCommunicator.SendStr(selectId);
		}		

		private void lv_lobbyUsers_CursorChanged(object sender, EventArgs e)
		{
			string selectId = GetElementBySelectedRow(ref lv_lobbyUsers, 0);
			if (CheckExistId(ref lv_lobbyFriends, selectId))
				bt_applyFriend.Enabled = false;
			else
				bt_applyFriend.Enabled = true;
		}

		private void cb_stateList_SelectedIndexChanged(object sender, EventArgs e)		// 콤보 박스의 내용이 변경 될 경우 로비 서버에 신호 전달
		{
			lobbyCommunicator.SendInt(4);
			lobbyCommunicator.SendLinkedStr(myUserData.ID, myUserData.Nickname, (cb_stateList.SelectedIndex + 1).ToString());
		}

		private void lv_lobbyRooms_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			lobbyCommunicator.SendInt(3);

			string strRoomNum = GetElementBySelectedRow(ref lv_lobbyRooms, 0);
			Console.WriteLine(strRoomNum);
			roomNum = int.Parse(strRoomNum);
			lobbyCommunicator.SendInt(roomNum);
		}

		private void lv_lobbyRooms_SelectedIndexChanged(object sender, EventArgs e)
		{

		}
	}
}
