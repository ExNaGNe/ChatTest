using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;


namespace Server
{
    public partial class Form1 : Form
    {
        TcpClient DB = null;
        Socket sock = null;
        Socket SockForServer = null;
        Socket servSock = null;
        IPEndPoint ep = null;
        static Socket[] sockets = new Socket[100]; //client
        static Socket[] serverSockets = new Socket[100];
        private static Mutex mutex = new Mutex();
        private static Mutex mutexForServer = new Mutex();
        static int socketCnt = 0;
        List<Room> room = new List<Room>(); //룸 리스트
        static int roomCnt = 1;
        public const string DB_CONN = "Server=10.10.20.213;Port=3306;Database=VoiceChat;Uid=root;Pwd=1234;Charset=utf8";
        public const string UP_OUTDATE = "update users set state = 0,lastout = now() where id = '";
        public static string NOW() => DateTime.Now.ToString("HH:mm:ss");

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            StartTcp();
            ConnectWithServers();
        }
        public void DBconnect()
        {
            //try
            //{
            //    using (MySqlConnection conn = new MySqlConnection(DB_CONN))
            //    {
            //        try
            //        {
            //            conn.Open();
            //            if (conn.Ping() == false)
            //            {
            //                Console.WriteLine($"DB 연결 에러");
            //                return;
            //            }
            //            string query = Get_FriendQuery();
            //            MySqlCommand comm = new MySqlCommand(query, conn);

            //            using (MySqlDataReader reader = comm.ExecuteReader())
            //            {
            //                while (reader.Read())
            //                {
            //                    rows.Add(GetRow(reader, reader.FieldCount));  //보낼 행 리스트에 추가
            //                }
            //            }
            //            NETSTREAM.Write(stream, rows);
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine($"[{NOW()}]친구 리스트 전송 에러 {ex.Message}");
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.ToString());
            //}
        }
        public void ConnectWithServers() //다른 서버들 접속 용
        {
            Console.WriteLine("서버 빌드 시작");
            SockForServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ep = new IPEndPoint(IPAddress.Any, 7000);
            SockForServer.Bind(ep);
            SockForServer.Listen(10);
            Thread t2 = new Thread(AcceptServerSockets);
            t2.Start();
        }
        public void StartTcp() //서버 시작
        {
            try
            {
                Console.WriteLine("서버 빌드 시작");
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ep = new IPEndPoint(IPAddress.Any, 7001);
                sock.Bind(ep);
                sock.Listen(10);
                Thread t2 = new Thread(AcceptSockets);
                t2.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void AcceptSockets() //Accept 대기
        {
            while (true)
            {
                Socket clientSock = sock.Accept();
                mutex.WaitOne(); //뮤텍스 대기
                sockets[socketCnt++] = clientSock; //sockets[]에 clientsocket 저장
                mutex.ReleaseMutex(); //뮤텍스 릴리즈
                Console.WriteLine("연결 성공");
                Console.WriteLine("i 값 : " + socketCnt);
                Thread t1 = new Thread(() => ConnectWithClient(clientSock)); //스래드와 연결
                t1.Start(); //스래드 시작
            }
        }
        public void AcceptServerSockets() //Accept 대기
        {
            while (true)
            {
                try
                {
                    servSock = SockForServer.Accept();
                    mutexForServer.WaitOne(); //뮤텍스 대기
                    sockets[socketCnt++] = servSock; //sockets[]에 clientsocket 저장
                    mutexForServer.ReleaseMutex(); //뮤텍스 릴리즈
                    Console.WriteLine("서버 연결 성공");
                    Console.WriteLine("i 값 : " + socketCnt);
                    Thread t1 = new Thread(ThreadServer); //스래드와 연결
                    t1.Start(); //스래드 시작
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("서버 클라이언트 접속 실패");
                }

            }
        }

        void ConnectWithClient(Socket clientSock)
        {
            byte[] signData = new byte[4];
            string ID = string.Empty;
            while (true)
            {
                try
                {
                    clientSock.Receive(signData); //signData 받기
                    int sign = BitConverter.ToInt32(signData, 0);
                    Console.WriteLine("Client에서 받은 sign 값 : " + sign.ToString());
                    try
                    {

                    }
                    catch (Exception d)
                    {
                        Console.WriteLine(d.ToString());
                    }
                    switch (sign)
                    {
                        case 1: //방 만들기
                            {
                                try
                                {
                                    clientSock.Receive(signData);
                                    int size = BitConverter.ToInt32(signData, 0);
                                    Console.WriteLine("switch 1 사이즈 값 : " + size);

                                    byte[] bytes = new byte[size];
                                    clientSock.Receive(bytes);
                                    string save = Encoding.UTF8.GetString(bytes);
                                    string[] Arr_save = new string[10];
                                    Arr_save = save.Split(",".ToArray());

                                    Room r = new Room { num = roomCnt.ToString(), name = Arr_save[0], password = Arr_save[1], playercnt = 1 };
                                    mutex.WaitOne(); //뮤텍스 대기
                                    room.Add(r);
                                    roomCnt++;
                                    mutex.ReleaseMutex(); //뮤텍스 릴리즈

                                    foreach (Room n in room)
                                    {
                                        Console.WriteLine("생성된 룸 확인 :" + n);
                                    }
                                }
                                catch (Exception a)
                                {
                                    Console.WriteLine(a.ToString());
                                }


                                break;
                            }
                        case 2: //방 새로고침
                            {
                                try
                                {
                                    string[] splitServRooms = new string[10];
                                    Console.WriteLine("룸 카운트 갯수 : " + room.Count);
                                    byte[] bytes = new byte[50];
                                    bytes = BitConverter.GetBytes(room.Count);
                                    clientSock.Send(bytes, sizeof(int), SocketFlags.None);//룸 count 보내기

                                    foreach (Room List_Room in room)
                                    {
                                        int size;
                                        byte[] StrByte = Encoding.UTF8.GetBytes(List_Room.ToString());
                                        size = StrByte.Length;
                                        clientSock.Send(BitConverter.GetBytes(size), sizeof(int), SocketFlags.None); //룸 사이즈 보내기
                                        Console.WriteLine("룸 사이즈 보내기");

                                        StrByte = Encoding.UTF8.GetBytes(List_Room.ToString());
                                        clientSock.Send(StrByte, StrByte.Length, SocketFlags.None); //룸 string 보내기
                                        Console.WriteLine("룸 string 보내기");
                                    }
                                }
                                catch (Exception b)
                                {
                                    Console.WriteLine(b.ToString());
                                }

                                break;
                            }
                        case 3: //방 접속
                            {
                                try
                                {
                                    byte[] bytes = new byte[50];
                                    clientSock.Receive(bytes); //인덱스 받기
                                    int index = BitConverter.ToInt32(bytes, 0);
                                    Console.WriteLine($"받은바이트값 : {index}");

                                    foreach (Room r in room)
                                    {
                                        if (r.num == index.ToString())
                                        {
                                            if (r.playercnt != 10) //최대 인원 수
                                            {
                                                r.playercnt += 1;
                                                sign = 1;
                                                bytes = BitConverter.GetBytes(sign);
                                                clientSock.Send(bytes, bytes.Length, SocketFlags.None);// 사인 보내기

                                                int roomNum = Convert.ToInt32(r.num); //방번호 보내기
                                                bytes = BitConverter.GetBytes(roomNum);
                                                clientSock.Send(bytes, sizeof(int), SocketFlags.None);// 사인 보내기

                                            }
                                            else
                                            {
                                                sign = -1;
                                                bytes = BitConverter.GetBytes(sign);
                                                clientSock.Send(bytes, bytes.Length, SocketFlags.None);// 사인 보내기
                                            }
                                            break;
                                        }
                                    }

                                }
                                catch (Exception c)
                                {
                                    Console.WriteLine(c.ToString());
                                }

                                break;
                            }
                        case 4: //상태
                            {
                                try
                                {
                                    byte[] tempState = new byte[10];
                                    clientSock.Receive(tempState, sizeof(int), SocketFlags.None); //사이즈 받는부분
                                    int size = BitConverter.ToInt32(tempState, 0);
                                    Console.WriteLine("switch 4 사이즈 값 : " + size);

                                    tempState = new byte[size];
                                    Console.WriteLine("clinet에서 받는부분");
                                    clientSock.Receive(tempState, tempState.Length, SocketFlags.None);  //아이디 상태 받음
                                    Console.WriteLine("아이디와 상태 받는 부분:" + Encoding.UTF8.GetString(tempState));

                                    string str_tempState = (Encoding.UTF8.GetString(tempState));
                                    string[] ary_tempstate = str_tempState.Split(",".ToArray());
                                    Console.WriteLine("클라에서 받은 아이디 및 상태 :" + str_tempState);

                                    int state = Convert.ToInt32(ary_tempstate[1]);
                                    ID = ary_tempstate[0];

                                    if (UpdateSqlQuery($"update users set state={state} where id='{ary_tempstate[0]}'") == true)
                                    {
                                        size = tempState.Length;
                                        byte[] sendToServ = BitConverter.GetBytes(size);
                                        servSock.Send(sendToServ, sendToServ.Length, SocketFlags.None);
                                        servSock.Send(tempState, tempState.Length, SocketFlags.None);
                                    }
                                    else
                                    {
                                        Console.WriteLine("쿼리문 실패");
                                    }
                                }
                                catch (Exception d)
                                {
                                    Console.WriteLine(d.ToString());
                                }

                                break;
                            }
                        case 5:
                            {
                                break;
                            }

                        case 99: //접속종료
                            {
                                mutex.WaitOne(); //뮤텍스 대기
                                for (int i = 0; i < socketCnt; i++)
                                {
                                    if (clientSock == sockets[i])
                                    {
                                        while (i++ < socketCnt - 1)
                                            sockets[i] = sockets[i + 1];
                                        break;
                                    }
                                }
                                socketCnt--;
                                mutex.ReleaseMutex(); //뮤텍스 릴리즈
                                CloseClientQuery(ID); //종료할 시 쿼리문
                                clientSock.Close();
                                Console.WriteLine(clientSock.ToString() + "접속 종료");
                                return;
                            }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"clientSock.ToString() 연결 끊김 /n{ex.Message}/n{ex.StackTrace}");
                    return;
                }
            }
        }
        void ThreadServer()
        {
            while (true)
            {

            }
        }
       
        bool UpdateSqlQuery(string query)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(DB_CONN))
                {
                    try
                    {
                        conn.Open();
                        if (conn.Ping() == false)
                        {
                            Console.WriteLine($"DB 연결 에러");
                            return false;
                        }
                        MySqlCommand comm = new MySqlCommand(query, conn);
                        if(comm.ExecuteNonQuery()>0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        void CloseClientQuery(string ID)
        {
            using (MySqlConnection conn = new MySqlConnection(DB_CONN))
            {
                try
                {
                    conn.Open();
                    if (conn.Ping() == false)
                    {
                        Console.WriteLine($"[{NOW()}]유저 종료 DB 연결 에러");
                        return;
                    }
                    string query = Update_OutQuery(ID);
                    MySqlCommand comm = new MySqlCommand(query, conn);

                    int result = comm.ExecuteNonQuery();
                    if (result < 0)
                        Console.WriteLine($"[{NOW()}]유저 종료 DB 실행 에러");
                    else
                        Console.WriteLine($"[{NOW()}]유저 종료 DB 업데이트");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{NOW()}]유저 종료 DB 에러 {ex.Message}");
                }
            }
        }
        string Update_OutQuery(string ID)
        {
            return $"{UP_OUTDATE}{ID}'";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            foreach (Room r in room)
            {
                textBox1.AppendText(r + "\r\n");
            }
        }


    } // form1 class
    public class Room
{
    public string num { get; set; }
    public string name { get; set; }
    public string password { get; set; }
    public int playercnt { get; set; }
    public override string ToString()
    {
        return num + "," + name + "," + password + "," + playercnt;
    }
}
}
