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
	public partial class InputPass : Form
	{
		VoiceChatTCP communicator = null;
		public InputPass(VoiceChatTCP comm)
		{
			InitializeComponent();
			communicator = comm;
		}

		private void bt_sendPass_Click(object sender, EventArgs e)
		{		
			communicator.SendStr(tb_pass.Text);
		}
	}
}
