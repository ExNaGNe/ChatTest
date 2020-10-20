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
		VoiceChatTCP communicator = null;		
		
		public CreateRoom(VoiceChatTCP _communicator)
		{
			InitializeComponent();
			communicator = _communicator;
			
		}

		private void bt_createRoom_Click(object sender, EventArgs e)
		{
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
