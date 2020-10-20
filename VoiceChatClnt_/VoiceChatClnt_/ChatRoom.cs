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
using System.Net.Sockets;
using NAudio.Wave;
using System.Net;
using VoiceChatClnt_;

namespace VoiceChatClnt_
{
	public partial class ChatRoom : Form
	{
		VoiceChatTCP usrCommunicator = null;
		VoiceChatTCP lobbyCommunicator = null;
		User myUserData = null;		

		VoiceChater vChater;
		List<PartiData> partiDatas;

		TypeChater tChater;

		int voiceRoomPort;
		int chatRoomPort;
		int connected = 0;
		int start = 0;		

		Thread RecvAndReleaseThread;
		Thread RecvAndReleaseChatThread;

		public ChatRoom(User myUsr, VoiceChatTCP usrComm, VoiceChatTCP lobbyComm, int roomNum)
		{
			InitializeComponent();

			usrCommunicator = usrComm;
			lobbyCommunicator = lobbyComm;
			myUserData = myUsr;
			voiceRoomPort = roomNum + 19000;
			chatRoomPort = roomNum + 18000;	
		}

        private void ChatRoom_Load(object sender, EventArgs e)
        {
            Lobby.InitListColumns(ref lv_parti, "아이디", "닉네임", "상태", "위치");

            vChater = new VoiceChater(voiceRoomPort);
            vChater.InitSendSock();
            vChater.InitRecvSock();
            vChater.PlayInput();

            tChater = new TypeChater(myUserData.ID, chatRoomPort);

            connected = 1;
            RecvAndReleaseThread = new Thread(new ThreadStart(RecvAndRelease));
            RecvAndReleaseChatThread = new Thread(new ThreadStart(RecvAndReleaseChat));
            RecvAndReleaseThread.Start();
            RecvAndReleaseChatThread.Start();
        }

        private void RecvAndReleaseChat()
		{
			while (start == 0)
			{
				Console.WriteLine(start);
				Thread.Sleep(1);
			}

			tChater.SendMessage("입장했습니다.");

			while (connected == 1)
			{
				string recvMsg = tChater.RecvMessage();

                if (connected != 1)
                    break;

				this.Invoke(new Action(() => {
					rtb_chatWindow.AppendText(recvMsg + Environment.NewLine);					
				}));
			}
		}

		private void RecvAndRelease()
		{			
			Console.WriteLine("진입");
			byte[] data = new byte[65535];

			while (start == 0)
			{
				Console.WriteLine(start);
				Thread.Sleep(1);
			}

			while (connected == 1)
			{
				//Console.WriteLine("반복문");
				byte[] recvNumByte = new byte[4];
				EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
				int received = vChater.myRecvSock.ReceiveFrom(data, ref remoteIp);

				//Console.WriteLine("test : " + data);

				Array.Copy(data, recvNumByte, recvNumByte.Length);
				Array.Clear(data, 0, recvNumByte.Length);

				int recvNum = BitConverter.ToInt32(recvNumByte, 0);

				vChater.AddSound(recvNum, data, received);
                Array.Clear(data, 0, data.Length);
			}
		}

		public void UpdatePartiList(List<string> partiList)
		{			
			Console.WriteLine("aaa");
			SetPartiDatas(partiList);			

			this.Invoke(new Action(delegate ()
			{
				lv_parti.Items.Clear();
				Lobby.SetRowsAsListStr(ref lv_parti, partiList);
			}));
			int mySeqNum = GetMyNumFromListPartiData();
            vChater.Init(mySeqNum, partiDatas);
            vChater.PlayOutput();

            start = 1;
		}

		public int GetMyNumFromListPartiData()
		{
			foreach(PartiData data in partiDatas)
			{
				if (myUserData.ID.Equals(data.ID))
					return data.num;
			}

			return -1;
		}

		public void SetPartiDatas(List<string> partiList)
		{
			int i = 1;
			partiDatas = new List<PartiData>();

			foreach(string row in partiList)
			{ 
				string[] row_arr = row.Split(",".ToCharArray());
				partiDatas.Add(new PartiData(i++, row_arr[0]));
			}
		}

		private void bt_inputMsg_Click(object sender, EventArgs e)
		{
			string msg = tb_inputMsg.Text;
			tChater.SendMessage(msg);
		}

		private void bt_invite_Click(object sender, EventArgs e)
		{

		}

		private void bt_exitRoom_Click(object sender, EventArgs e)
		{
            Close();
		}

        private void ChatRoom_FormClosed(object sender, FormClosedEventArgs e)
        {
            connected = 0;
            tChater.SendMessage("퇴장했습니다.");

            vChater.StopInput();
            RecvAndReleaseThread.Join();

            lobbyCommunicator.SendInt(5);

            usrCommunicator = null;
            lobbyCommunicator = null;
            myUserData = null;

            vChater.CloseSock();
            tChater.CloseSock();

            partiDatas = null;
            RecvAndReleaseThread.Abort();
            RecvAndReleaseChatThread.Abort();
        }
    }
}

public class PartiData
{
	public int num { get; set; }
	public string ID { get; set; }	

	public PartiData(int _num, string _ID)
	{
		num = _num;
		ID = _ID;
	}
}

public class VoiceChater
{
	
	public Socket mySendSock { set; get; }
	public Socket myRecvSock { set; get; }
	private int myNum;
	private int roomPort;
	IPAddress multicastIP = IPAddress.Parse("229.255.255.255");
	private WaveIn inputWave;
	private List<WaveOutputer> outputWaves = null;

	public VoiceChater(int _roomPort)
	{		
		roomPort = _roomPort;		
		InitWaveIn();		
	}

	public void Init(int num, List<PartiData> _partiDatas)
	{
		myNum = num;				
		setOutList(_partiDatas);
	}

	public void InitWaveIn()
	{
		inputWave = new WaveIn();
		inputWave.WaveFormat = new WaveFormat(8000, 16, 1);
		inputWave.DataAvailable += Voice_Input;
	}

	public void InitSendSock()
	{
		mySendSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);		
	}		

	public void InitRecvSock()
	{
		myRecvSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		IPEndPoint localIP = new IPEndPoint(IPAddress.Any, roomPort);
        myRecvSock.Bind(localIP);
        myRecvSock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastIP, IPAddress.Any));
	}

	public void CloseSock()
	{		
		myRecvSock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(multicastIP, IPAddress.Any));
		mySendSock.Close();
		mySendSock.Dispose();
		myRecvSock.Close();
		myRecvSock.Dispose();
	}

	public void setOutList(List<PartiData> _partiDatas)
	{
		outputWaves = new List<WaveOutputer>();

		foreach(PartiData data in _partiDatas)
		{
			outputWaves.Add(new WaveOutputer(data.num));
		}		
	}

	public void PlayInput()
	{
		inputWave.StartRecording();
	}

	public void StopInput()
	{
		inputWave.StopRecording();
	}
	
	public void PlayOutput()
	{
		foreach(WaveOutputer output in outputWaves)
		{
			output.Output.Play();
		}
	}

	public void AddSound(int num, byte[] data, int received)
	{
        if (num == myNum)
        {
            return;
        }

        try
        {
            WaveOutputer outputer = outputWaves.Find(x => CheckIsNumOfOuter(num, x));
            outputer.AddSoundData(data, received);
        }
        catch
        {
            Console.WriteLine("AddSound 에러");
        }
        //Console.WriteLine(outputWaves.Count);
        //foreach (WaveOutputer outputer in outputWaves)
        //{
        //    if (CheckIsNumOfOuter(num, outputer) && !CheckIsNumOfOuter(myNum, outputer))
        //    {
        //        outputer.AddSoundData(data, received);
        //    }
        //}
    }

	public bool CheckIsNumOfOuter(int num, WaveOutputer waveOuter)
	{
		if (waveOuter.Num == num)
			return true;
		else
			return false;
	}

	public void Voice_Input(object sender, WaveInEventArgs e)
	{		
		
		byte[] Num = new byte[4];		
		Num = BitConverter.GetBytes(myNum);
		byte[] sendData = new byte[Num.Length + e.Buffer.Length];

		Array.Copy(Num, 0, sendData, 0, Num.Length);
		Array.Copy(e.Buffer, 0, sendData, Num.Length, e.Buffer.Length);		

		try
		{			
			IPEndPoint remote_point = new IPEndPoint(multicastIP, roomPort);
			mySendSock.SendTo(sendData, remote_point);		
			
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
		
	}
}


public class WaveOutputer
{
	public WaveOut Output { get; set; }
	public BufferedWaveProvider BufferStream { get; set; }
	public int Num { get; set; }

	public WaveOutputer(int _num)
	{		
		Num = _num;
		InitWaveOut();
	}

	public void InitWaveOut()
	{
		Output = new WaveOut();
		BufferStream = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
		BufferStream.BufferLength = 107374182;
		Output.Init(BufferStream);
	}

	public void AddSoundData(byte[] data, int len)
	{
		BufferStream.AddSamples(data, 0, len);		
	}	   	 
}


public class TypeChater
{
	public UdpClient mySendSock { set; get; }
	public UdpClient myRecvSock { set; get; }
	private IPEndPoint groupEndPoint;
	private IPEndPoint localEp;
	private int roomPort;
	private string myName;
	
	private IPAddress multicastIP = IPAddress.Parse("229.255.255.254");

	public TypeChater(string _name, int _roomPort)
	{
		roomPort = _roomPort;
		myName = _name;
		groupEndPoint = new IPEndPoint(multicastIP, roomPort);
		localEp = new IPEndPoint(IPAddress.Any, roomPort);

		SignInSendSock();
		SignInRecvSock();
	}

	public void SignInSendSock()
	{		
		mySendSock = new UdpClient();
		mySendSock.JoinMulticastGroup(multicastIP);		
	}

	public void SignInRecvSock()
	{
		myRecvSock = new UdpClient();
		myRecvSock.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		myRecvSock.ExclusiveAddressUse = false;
		myRecvSock.Client.Bind(localEp);
		myRecvSock.JoinMulticastGroup(multicastIP);
	}

	public void SendMessage(string message)
	{
		string sendStr = String.Format("{0} : {1}", myName, message);
		byte[] sendData = Encoding.UTF8.GetBytes(sendStr);
		mySendSock.Send(sendData, sendData.Length, groupEndPoint);

	}

	public string RecvMessage()
	{
		IPEndPoint recvPoint = new IPEndPoint(IPAddress.Any, 0);
		Byte[] data = myRecvSock.Receive(ref recvPoint);
		string message = Encoding.UTF8.GetString(data);

		return message;
	}

	public void CloseSock()
	{		
		mySendSock.DropMulticastGroup(multicastIP);
		mySendSock.Close();
		myRecvSock.DropMulticastGroup(multicastIP);
		myRecvSock.Close();
	}
}