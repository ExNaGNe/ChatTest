using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static Serv.CONST;

namespace Serv
{
    static public partial class CONST
    {
        public const int PORT_ROOM = 10006;     //룸 서버 포트
        public const string IP_LOBBY = "10.10.20.47";       //로비 서버 IP
        public const int PORT_LOBBY = 7001;                 //로비 서버 포트

        public static string NOW() => DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
    }

    static public class NETSTREAM
    {
        static public void Write(NetworkStream stream, int num)
        {
            stream.Write(BitConverter.GetBytes(num), 0, sizeof(int));
        }

        static public void Write(NetworkStream stream, string str)
        {
            stream.Write(BitConverter.GetBytes(str.Length), 0, sizeof(int));
            stream.Write(Encoding.UTF8.GetBytes(str), 0, Encoding.UTF8.GetBytes(str).Length);
        }

        static public void Write(NetworkStream stream, List<string> str)
        {
            stream.Write(BitConverter.GetBytes(str.Count), 0, sizeof(int));

            foreach (string temp in str)
            {
                var buf = Encoding.UTF8.GetBytes(temp);
                stream.Write(BitConverter.GetBytes(buf.Length), 0, sizeof(int));
                stream.Write(buf, 0, buf.Length);
            }
        }

        static public void Write(NetworkStream stream, IEnumerable<string> str)
        {
            stream.Write(BitConverter.GetBytes(str.Count()), 0, sizeof(int));

            foreach (string temp in str)
            {
                var buf = Encoding.UTF8.GetBytes(temp);
                stream.Write(BitConverter.GetBytes(buf.Length), 0, sizeof(int));
                stream.Write(buf, 0, buf.Length);
            }
        }

        static public int ReadInt(NetworkStream stream)
        {
            var buf = BitConverter.GetBytes(-1);
            stream.Read(buf, 0, sizeof(int));

            return BitConverter.ToInt32(buf, 0);
        }

        static public string ReadStr(NetworkStream stream)
        {
            var buf = new byte[sizeof(int)];
            stream.Read(buf, 0, sizeof(int));
            int len = BitConverter.ToInt32(buf, 0);
            if (len <= 0)
                return null;
            buf = new byte[len];
            stream.Read(buf, 0, len);
            return Encoding.UTF8.GetString(buf);
        }
    }

    class RoomServ
    {
        TcpClient Lobbyclient;
        NetworkStream Lobbystream;
        TcpClient client;
        bool Serv_Flag = true;
        List<Room> Rooms = new List<Room>();
        Mutex RoomsMutex = new Mutex(false, "Rooms");

        static void Main(string[] args)
        {
            RoomServ Serv = new RoomServ();
            Serv.Run();
        }

        RoomServ()
        {
            try
            {
                Lobbyclient = new TcpClient(new IPEndPoint(IPAddress.Any, 0));
                Lobbyclient.Connect(new IPEndPoint(IPAddress.Parse(IP_LOBBY), PORT_LOBBY));                //로비서버 연결
                Lobbystream = Lobbyclient.GetStream();

                Console.WriteLine($"[{NOW()}]로비서버 연결");
                Thread lobby_th = new Thread(new ThreadStart(Lobby_th));
                lobby_th.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{NOW()}]로비서버 연결 실패 {ex.Message}");
            }
        }

        void Run()
        {
            TcpListener listen = new TcpListener(new IPEndPoint(IPAddress.Any, PORT_ROOM));
            listen.Start();

            while (Serv_Flag)
            {
                try
                {
                    client = listen.AcceptTcpClient();
                    Console.WriteLine($"[{NOW()}]클라이언트 연결됨");
                    NetworkStream stream = client.GetStream();

                    string RoomNum = NETSTREAM.ReadStr(stream);
                    string id = RoomNum.Split(",".ToCharArray())[1];
                    RoomNum = RoomNum.Split(",".ToCharArray())[0];

                    try
                    {
                        Room room = Rooms.Find(x => x.RoomNum.ToString() == RoomNum);
                        Room.User user = new Room.User(id, stream);
                        room.AddUser(user);
                    }
                    catch (NullReferenceException)
                    {

                    }
                }
                catch
                {
                    Console.WriteLine($"[{NOW()}]클라이언트 연결 실패");
                }
            }
            Console.WriteLine($"[{NOW()}]클라이언트 접속 대기 종료");
        }

        void Lobby_th()
        {
            while(Serv_Flag)
            {
                try
                {
                    string orign = NETSTREAM.ReadStr(Lobbystream);
                    string roomnum = orign.Split(",".ToCharArray())[0];
                    string title = orign.Split(",".ToCharArray())[1];
                    string max = orign.Split(",".ToCharArray())[2];

                    Room room = new Room(roomnum, title, max, RoomsMutex);
                    RoomsMutex.WaitOne();
                    Rooms.Add(room);
                    RoomsMutex.ReleaseMutex();
                    Console.WriteLine($"{NOW()}{roomnum}번방({title}) 추가");
                }
                catch
                {
                    Console.WriteLine($"[{NOW()}]방 생성 실패");
                }
            }
        }

        class Room
        {
            public int RoomNum { get; set; } = 0;
            public string title { get; set; } = string.Empty;
            public int max { get; set; } = 0;
            public int num { get; set; } = 0;
            Mutex RoomsMutex;
            List<User> Users = new List<User>();
            public Room(int RoomNum, string title, int max, Mutex RoomsMutex)
            {
                this.RoomNum = RoomNum;
                this.title = title;
                this.max = max;
                this.RoomsMutex = RoomsMutex;
            }

            public Room(string RoomNum, string title, string max, Mutex RoomsMutex)
            {
                this.RoomNum = int.Parse(RoomNum);
                this.title = title;
                this.max = int.Parse(max);
                this.RoomsMutex = RoomsMutex;
            }

            public void AddUser(User user)
            {
                Users.Add(user);
            }

            //bool ExitUser(string id)
            //{

            //}

            public class User
            {
                string id = string.Empty;
                NetworkStream stream;

                public User(string id, NetworkStream stream)
                {
                    this.id = id;
                    this.stream = stream;
                }
            }
        }
    }
}
