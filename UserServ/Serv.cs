using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using static Serv.CONST;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Serv
{
    class UserServ
    {
        static void Main(string[] args)
        {
            UserServ user = new UserServ();
            user.Run();
        }

        //TcpClient DBclient;
        //readonly NetworkStream DBstream;
        TcpClient Lobbyclient;
        readonly NetworkStream Lobbystream;
        bool th_flag = true;
        IPEndPoint userPoint = new IPEndPoint(IPAddress.Parse(IP_USERSERV), 0);
        IPEndPoint ServPoint = new IPEndPoint(IPAddress.Parse(IP_USERSERV), PORT_USERSERV);
        //IPEndPoint dbPoint = new IPEndPoint(IPAddress.Parse(IP_DB), PORT_DB);
        IPEndPoint lobbyPoint = new IPEndPoint(IPAddress.Parse(IP_LOBBY), PORT_LOBBY);
        private Mutex users_mutex = new Mutex(false, "user mutex");
        List<User> users = new List<User>();
        //List<Info> infos = new List<Info>();
        public delegate void Refreshing();
        public event Refreshing RefreshEvent;

        public UserServ()
        {
            Lobbyclient = new TcpClient(userPoint);
            Lobbyclient.Connect(lobbyPoint);
            Lobbystream = Lobbyclient.GetStream();

            Thread lobby_th = new Thread(new ThreadStart(Lobby_th));
            lobby_th.Start();
        }

        public void Run()
        {
            TcpListener listen = new TcpListener(ServPoint);
            listen.Start();

            while (th_flag)
            {
                TcpClient client = listen.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                byte[] buf = new byte[sizeof(int)];
                stream.Read(buf, 0, sizeof(int));
                int len = BitConverter.ToInt32(buf, 0);
                buf = new byte[len];
                stream.Read(buf, 0, len);

                string id = Encoding.UTF8.GetString(buf);
                string nick = id.Split(",".ToCharArray())[1];
                string port = id.Split(",".ToCharArray())[2];
                id = id.Split(",".ToCharArray())[0];
                string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

                User user = new User(new Info(id, nick), ip, port, stream, users, users_mutex);
                RefreshEvent += new Refreshing(user.Refresh);
                users_mutex.WaitOne();
                users.Add(user);
                users_mutex.ReleaseMutex();
                RefreshEvent();
            }
        }

        void Lobby_th()
        {
            while (th_flag)
            {
                byte[] buf = new byte[sizeof(int)];
                Lobbystream.Read(buf, 0, sizeof(int));
                int len = BitConverter.ToInt32(buf, 0);
                if ( len > 0)
                {
                    users_mutex.WaitOne();
                    List<User> copy = new List<User>(users);
                    users_mutex.ReleaseMutex();
                    Console.WriteLine($"들어온 신호:{len}");
                    buf = new byte[len];
                    Lobbystream.Read(buf, 0, len);
                    string id = Encoding.UTF8.GetString(buf).Split(",".ToCharArray())[0];
                    STATE state = (STATE) int.Parse(Encoding.UTF8.GetString(buf).Split(",".ToCharArray())[1]);
                    foreach(User temp in copy)
                    {
                        if(temp.info.id == id)
                        {
                            users_mutex.WaitOne();
                            temp.info.state = state;
                            users = copy;
                            users_mutex.ReleaseMutex();
                            break;
                        }
                    }
                    RefreshEvent();
                }
                else
                {
                    Console.WriteLine($"들어온 신호:{BitConverter.ToInt32(buf, 0)}");
                    th_flag = false;
                }
            }
            Lobbystream.Close();
        }
    }

    public class User
    {
        public Info info { get; private set; }
        public NetworkStream stream { get; private set; }
        List<User> users;
        Mutex mutex { get; set; }
        //string ip;
        //string port;
        IPEndPoint ClaPoint;
        IPEndPoint ServPoint = new IPEndPoint(IPAddress.Parse(IP_USERSERV), 0);
        //TcpClient DBclient;
        //NetworkStream DBstream;

        public User()
        { }

        public User(Info info, string ip, string port, NetworkStream stream, List<User> users, Mutex mutex)
        {
            this.info = info;
            this.stream = stream;
            this.users = users;
            this.mutex = mutex;
            ClaPoint = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
            //this.ip = ip;
            //this.port = port;
            //DBclient = new TcpClient(ServPoint);
            //DBclient.Connect(dbPoint);
            //DBstream = DBclient.GetStream();
            Thread thread = new Thread(User_th);
            thread.Start();
        }

        public void Refresh()
        {
            Console.WriteLine($"{info.id} 동기화 진입");
            Thread thread = new Thread(list_th);
            thread.Start();
        }

        void User_th()
        {
            int sign = 0;
            TcpClient client = new TcpClient(ServPoint);
            client.Connect(ClaPoint);
            NetworkStream ClaStream = client.GetStream();
            while(sign != -1)
            {
                byte[] buf = new byte[sizeof(int)];
                stream.Read()
            }
        }

        void list_th()    //유저 리스트, 친구 리스트 전송 쓰레드
        {
            mutex.WaitOne();
            List<User> copy = users.ToList();
            mutex.ReleaseMutex();
            //유저 리스트 전송
            var buf = BitConverter.GetBytes(copy.Count());
            stream.Write(buf, 0, sizeof(int));
            foreach(User temp in copy)
            {
                buf = Encoding.UTF8.GetBytes(temp.info.GetString());
                var len = BitConverter.GetBytes(buf.Length);
                stream.Write(len, 0, sizeof(int));
                stream.Write(buf, 0, buf.Length);
            }
            //친구 리스트 전송
            List<string> rows = new List<string>();
            using (MySqlConnection conn = new MySqlConnection(DB_CONN))
            {
                string query = Get_FriendQuery(info.id);
                try
                {
                    if (conn.Ping() == false)
                    {
                        Console.WriteLine($"DB 연결 에러");
                        return;
                    }
                    conn.Open();
                    MySqlCommand comm = new MySqlCommand(query, conn);

                    using (MySqlDataReader reader = comm.ExecuteReader())
                    {
                        //int field = reader.FieldCount;
                        while (reader.Read())
                        {
                            string row = string.Empty;
                            foreach(string temp in reader)
                            {
                                row += $"{temp},";
                            }
                            row.Remove(row.Length - 1);
                            rows.Add(row);
                        }
                    }

                    stream.Write(BitConverter.GetBytes(rows.Count), 0, sizeof(int));
                    foreach(string temp in rows)
                    {
                        stream.Write(BitConverter.GetBytes(temp.Length), 0, sizeof(int));
                        buf = Encoding.UTF8.GetBytes(temp);
                        stream.Write(buf, 0, buf.Length);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"친구 리스트 전송 에러 {ex.Message}");
                }
            }
        }

        string Get_FriendQuery(string id)
        {
            return $"{GET_FIRENDS}{info.id}'";
        }
    }

    public class Info
    {
        public string id { get; set; }
        public string nick_name { get; set; }
        public STATE state { get; set; }
        public string location {get; set;}

        public Info(string id, string nick, STATE state = STATE.ONLINE, string location = "로비")
        {
            this.id = id;
            nick_name = nick;
            this.state = state;
            this.location = location;
        }
        public string GetString()
        {
            return $"{id},{nick_name},{state},{location}";
        }
        //UserCp CopyUsers(User user)
        //{
        //    UserCp copy = new UserCp(ref user.id, ref user.state);
        //    return copy;
        //}
    }

    static public partial class CONST
    {
        public const string IP_USERSERV = "10.10.20.48";
        public const int PORT_USERSERV = 10005;
        public const int PORT_USERINPUT = 10006;
        public const string IP_DB = "10.10.20.213";
        public const int PORT_DB = 10000;
        public const string IP_LOBBY = "10.10.20.47";
        public const int PORT_LOBBY = 5001;
        public const string DB_CONN = "Server=\"10.10.20.213\";Port=3306;Database=VoiceChat;Uid=root;Pwd=1234";

        //public const string UPDATE_STATE = "update users set state = 1 where id = '";
        public const string GET_FIRENDS = "SELECT friendlist.friend_id, users.nickname, users.state, users.location " +
            "FROM friendlist inner join users on users.id = friendlist.friend_id where friendlist.id = '";

        public enum STATE
        {
            OFFLINE,
            ONLINE,
            BUSY,
            HIDE
        }
    }
}
