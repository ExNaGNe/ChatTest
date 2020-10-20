using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;

namespace VoiceChatClnt_
{
	public partial class Login : Form
	{
		VoiceChatTCP communicator;
		string ID;
		string PW;

		public Login()
		{
			InitializeComponent();
			StartPosition = FormStartPosition.CenterScreen;
		}

		private void bt_login_Click(object sender, EventArgs e)
		{
			communicator = new VoiceChatTCP("10.10.20.48", 10001);

			ID = tb_id.Text;
			PW = tb_pw.Text;			

			PW = Encode(PW);			

			communicator.SendInt(1);
			communicator.SendLinkedStr(ID, PW);

			int sig = communicator.RecvInt();
			if (sig == 1)
				EnterServ();			
			else
				MessageBox.Show("로그인 실패");

			communicator.Close();
			communicator = null;
		}

		private void bt_signIn_Click(object sender, EventArgs e)
		{
			SignIn signin = new SignIn();
			signin.Show();
		}

		private void EnterServ()
		{
			string recvUserData = communicator.RecvString();		

			string[] row_arr = recvUserData.Split(",".ToCharArray());

			User user = new User(row_arr[0], row_arr[1], "", "");

			OpenLobby(user);

			communicator.Close();

			Console.WriteLine("접속 성공");
			tb_pw.Text = "";
		}
		private void MainClosed(object sender, FormClosedEventArgs e)
		{
			Show();			
		}

		private void OpenLobby(User user)
		{
			Lobby lobby = new Lobby(user);
			lobby.FormClosed += new FormClosedEventHandler(MainClosed);
			lobby.Show();
			Hide();
		}

		public static string Encode(string pass)
		{
			byte[] data = Encoding.UTF8.GetBytes(pass);
			byte[] result;
			SHA512 sHA = new SHA512Managed();
			result = sHA.ComputeHash(data);
			return BitConverter.ToString(result).Replace("-", "");
		}

        private void Login_Load(object sender, EventArgs e)
        {

        }
    }

    public class VoiceChatTCP
	{		
		string servIP = "10.10.20.213";
		int servPort = 11111;

		TcpClient client = null;
		NetworkStream stream = null;

		public VoiceChatTCP()
		{
			Connect();
		}

		public VoiceChatTCP(string _servIP, int _servPort)
		{
			servIP = _servIP;
			servPort = _servPort;
			Connect();
		}

		public void Connect()
		{
			client = new TcpClient(new IPEndPoint(IPAddress.Any, 0));
			client.Connect(new IPEndPoint(IPAddress.Parse(servIP), servPort));
			stream = client.GetStream();
		}

		public void SendBytes(byte[] data)
		{
			stream.Write(data, 0, data.Length);
		}

		public byte[] RecvBytes()
		{
			byte[] data = new byte[256];
			stream.Read(data, 0, data.Length);

			return data;
		}

		public void SendLinkedStr(params string[] args)
		{
			string result = "";

			foreach (string str in args)
				result += str + ",";				
			
			result = result.Remove(result.Count() - 1, 1);
			SendStr(result);
		}

		public void SendInt(int num)
		{
			stream.Write(BitConverter.GetBytes(num), 0, sizeof(int));
		}

		public void SendStr(string query)
		{
			byte[] data = Encoding.UTF8.GetBytes(query);			
			stream.Write(BitConverter.GetBytes(data.Length), 0, sizeof(int));
			stream.Write(data, 0, data.Length);
			Console.WriteLine(query);
			Console.WriteLine(Encoding.UTF8.GetString(data));
			Console.WriteLine("전송끝");
		}

		public int RecvInt()
		{
			byte[] data = new byte[4];
			stream.Read(data, 0, sizeof(int));

			return BitConverter.ToInt32(data, 0);
		}

		public string RecvStringAsLen(int size)
		{
			byte[] data = new byte[size];
			stream.Read(data, 0, size);

			return Encoding.UTF8.GetString(data);
		}

		public List<string> RecvStringList()
		{
			List<string> list = new List<string>();

			int num = RecvInt();
			Console.WriteLine("행 개수:" + num);

			for (int i = 0; i < num; ++i)
			{
				string row = RecvString();
				list.Add(row);
			}

			return list;
		}

		public string RecvString()
		{
			int len = RecvInt();
			Console.WriteLine("행 길이:" + len);

			string row = RecvStringAsLen(len);

			//row = row.Trim().Remove(row.Count() - 1, 1);
			row = row.Trim();
			Console.WriteLine(row);

			return row;
		}

		public void Close()
		{
			stream.Close();
			client.Close();
			stream.Dispose();
			client.Dispose();
		}
	}



	public class User
	{
		public string ID { get; set; }
		public string Nickname { get; set; }
		public string State { get; set; }		
		public string Location { get; set; }

		public User(string _id, string _nickname, string _state, string _location)
		{
			ID = _id;
			Nickname = _nickname;
			State = _state;			
			Location = _location;
		}
	}
}

/*
Query = "select id, nickname, state, location from users where id = ";
Query += ("'" + ID + "'") + " and pass = '" + PW + "'";

communicator.SendStr(Query);

int recvRowCount = -1;
recvRowCount = communicator.RecvInt();   //Console.WriteLine("요소 개수:"+len.ToString());

if (recvRowCount <= 0)
{
MessageBox.Show(this, "로그인 정보 에러", "에러");
return;
}
else
{
Enter();
}
*/
/*
int recvRowCount = -1;
recvRowCount = communicator.RecvInt();   //Console.WriteLine("요소 개수:"+len.ToString());

if (recvRowCount <= 0)
{
	MessageBox.Show(this, "로그인 정보 에러", "에러");
	return;
}
else
{
	Enter();
}
*/

/*
private void Enter()
{
	int recvLen = -1;
	recvLen = communicator.RecvInt();   //Console.WriteLine("문자열 길이:" + len.ToString());

	if (recvLen > 0)
	{				
		string staff_name = communicator.RecvStringAsLen(recvLen);
		staff_name = staff_name.Trim().Remove(staff_name.Count() - 1, 1);

		string[] row_arr = staff_name.Split(",".ToCharArray());				

		User user = new User(row_arr[0], row_arr[1], row_arr[2], row_arr[3]);				

		Lobby lobby = new Lobby(user);
		lobby.FormClosed += new FormClosedEventHandler(MainClosed);
		lobby.Show();
		Hide();

		communicator.Close();

		Console.WriteLine("접속 성공");
	}
	else
	{
		MessageBox.Show(this, "로그인 정보 에러", "에러");
	}

	tb_pw.Text = "";
}
*/
