using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VoiceChatClnt_
{
	public partial class CreateRoom : Form
	{
        static string[] Not_Allow = new string[] { "," };
        VoiceChatTCP communicator = null;		
		
		public CreateRoom(VoiceChatTCP _communicator)
		{
			InitializeComponent();
			communicator = _communicator;
            tb_roomTitle.MaxLength = 16;
            tb_roompass.MaxLength = 16;
		}

		private void bt_createRoom_Click(object sender, EventArgs e)
		{
            if (string.IsNullOrEmpty(tb_roomTitle.Text.Trim()))
            {
                MessageBox.Show("방 제목은 빌 수 없습니다.");
                return;
            }
            if (Not_Allow.Any(tb_roomTitle.Text.Contains) || Not_Allow.Any(tb_roompass.Text.Contains))
            {
                MessageBox.Show("방 제목이나 비밀번호에 포함할 수 없는 특수문자가 포함되었습니다.");
                return;
            }
            if(cb_roompass.Checked && tb_roompass.Text.Length < 4)
            {
                MessageBox.Show("방의 비밀번호는 4글자 이상이여야 합니다.");
                return;
            }
            communicator.SendInt(1);
            communicator.SendLinkedStr(tb_roomTitle.Text, tb_roompass.Text, "1");
            Close();
		}

        private void cb_roompass_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_roompass.Checked)
            { 
                tb_roompass.Enabled = true;
            }
            else
            {
                tb_roompass.Text = string.Empty;
                tb_roompass.Enabled = false;
            }
        }

        private void CreateRoom_Load(object sender, EventArgs e)
        {

        }
    }
}
