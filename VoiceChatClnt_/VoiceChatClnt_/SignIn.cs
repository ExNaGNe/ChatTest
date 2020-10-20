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
	public partial class SignIn : Form
	{
		VoiceChatTCP communicator = null;

		public SignIn()
		{
			InitializeComponent();
		}

		public SignIn(VoiceChatTCP _communicator)
		{
			communicator = _communicator;
			InitializeComponent();
		}

		private void bt_signIn_Click_1(object sender, EventArgs e)
		{
			communicator = new VoiceChatTCP("10.10.20.48", 10001);
			string id = tb_signId.Text;
			string name = tb_signInName.Text;
			string pw = tb_signPw.Text;
			string pw2 = tb_signPw2.Text;

			if (!pw.Equals(pw2))
			{
				MessageBox.Show("pw가 다름");
				return;
			}
			pw = Login.Encode(pw);
			communicator.SendInt(2);
			communicator.SendLinkedStr(id, pw, name);

			int sig = communicator.RecvInt();
			if (sig == 2)
				MessageBox.Show("가입 완료");
			else if (sig == 0)
				MessageBox.Show("이미 존재하는 ID 또는 닉네임");
			else
				MessageBox.Show("???");

			communicator.Close();
			communicator = null;

			Close();

			/*					   
			string Query = "select id from users where id = ";
			Query += "'" + id + "'";
			communicator.SendStr(Query);

			int sig = communicator.RecvInt();
			if (sig > 0)
			{
				communicator.RecvString();
				MessageBox.Show("이미 존재하는 ID");
				return;
			}
			else if(!pw.Equals(pw2))
			{
				MessageBox.Show("pw가 다름");
				return;
			}

			Query = "insert into users (id, pass, nickname, signin) values(" + "'" + id + "'" + "," + "'" + Login.Encode(pw) + "'" + "," + "'" + name + "'" + "," + "now()" + ")" ;
			communicator.SendStr(Query);

			int rowLen = communicator.RecvInt();
			Console.WriteLine(rowLen);

			if (rowLen == 0)
				MessageBox.Show("가입 완료");
			else
				MessageBox.Show("???");

			Close();
			*/
		}

        private void SignIn_Load(object sender, EventArgs e)
        {

        }
    }
}
