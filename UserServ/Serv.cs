using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using static Serv.CONST;
using System.Threading;

namespace Serv
{
    class Serv
    {
        static void Main(string[] args)
        {
            UserServ user = new UserServ();
        }
    }

    class UserServ
    {
        //TcpClient DBclient;
        //readonly NetworkStream DBstream;
        TcpClient Lobbyclient;
        readonly NetworkStream Lobbystream;
        bool th_flag = true;
        IPEndPoint userPoint = new IPEndPoint(IPAddress.Parse(IP_USER), PORT_USER);
        IPEndPoint dbPoint = new IPEndPoint(IPAddress.Parse(IP_DB), PORT_DB);
        IPEndPoint lobbyPoint = new IPEndPoint(IPAddress.Parse(IP_LOBBY), PORT_LOBBY);
        private Mutex users_mutex = new Mutex(false, "user mutex");
        List<User> users = new List<User>();
        public delegate void Refreshing();
        public event Refreshing RefreshEvent;

        public UserServ()
        {
            /*  DB 접속
            DBclient = new TcpClient(user);
            DBclient.Connect(db);
            DBstream = DBclient.GetStream();

            string query = "SELECT friendlist.*, users.nickname " +
                "FROM friendlist inner join users on users.id = friendlist.friend_id";
            byte[] buf = Encoding.UTF8.GetBytes(query);
            DBstream.Write(BitConverter.GetBytes(buf.Length), 0, sizeof(int));
            DBstream.Write(buf, 0, buf.Length);

            Get_db();
            */

            Lobbyclient = new TcpClient(userPoint);
            Lobbyclient.Connect(lobbyPoint);
            Lobbystream = Lobbyclient.GetStream();

            Thread lobby_th = new Thread(new ThreadStart(Lobby_th));
            lobby_th.Start();

            TcpListener listen = new TcpListener(userPoint);
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
                id = id.Split(",".ToCharArray())[0];

                User user = new User(new Info(id, nick), stream, users, users_mutex);
                RefreshEvent += new Refreshing(user.Refreshing);
                users_mutex.WaitOne();
                users.Add(user);
                users_mutex.ReleaseMutex();
                RefreshEvent();
            }
        }

        void Get_db(NetworkStream stream)
        {
            byte[] buf = new byte[sizeof(int)];
            stream.Read(buf, 0, sizeof(int));
            int num = BitConverter.ToInt32(buf, 0);
            if (num > 0)
            {
                for (int i = 0; i < num; ++i)
                {
                    buf = new byte[sizeof(int)];
                    stream.Read(buf, 0, sizeof(int));
                    num = BitConverter.ToInt32(buf, 0);
                    buf = new byte[num];
                    stream.Read(buf, 0, num);
                    Console.WriteLine(Encoding.UTF8.GetString(buf));
                }
            }
        }

        void Lobby_th()
        {
            while (th_flag)
            {
                byte[] buf = new byte[sizeof(int)];
                Lobbystream.Read(buf, 0, sizeof(int));
                if (BitConverter.ToInt32(buf, 0) > 0)
                {
                    Console.WriteLine($"들어온 신호:{BitConverter.ToInt32(buf, 0)}");
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
        IPEndPoint userPoint = new IPEndPoint(IPAddress.Parse(IP_USER), PORT_USER);
        IPEndPoint dbPoint = new IPEndPoint(IPAddress.Parse(IP_DB), PORT_DB);
        TcpClient DBclient;
        NetworkStream DBstream;

        public User()
        { }

        public User(Info info, NetworkStream stream, List<User> users, Mutex mutex)
        {
            this.info = info;
            this.stream = stream;
            this.users = users;
            this.mutex = mutex;
            DBclient = new TcpClient(userPoint);
            DBclient.Connect(dbPoint);
            DBstream = DBclient.GetStream();
        }

        public void Refreshing()
        {
            Thread thread = new Thread(User_th);
            thread.Start();
        }

        void User_th()    //유저 리스트, 친구 리스트 전송 쓰레드
        {
            List<string> friends = new List<string>();

            //유저 리스트 전송

            //친구 리스트 전송
            string query = $"{GET_FIRENDS}{info.id}'";
            byte[] buf = Encoding.UTF8.GetBytes(query);
            DBstream.Write(BitConverter.GetBytes(buf.Length), 0, sizeof(int));
            DBstream.Write(buf, 0, buf.Length);

            buf = new byte[4];
            DBstream.Read(buf, 0, sizeof(int));
            int num = BitConverter.ToInt32(buf, 0);
            if (num < 0)
            {
                Console.WriteLine("친구 리스트 DB 접근 에러");
                return;
            }
            for (int i = 0; i < num; ++i)
            {
                DBstream.Read(buf, 0, sizeof(int));
                int len = BitConverter.ToInt32(buf, 0);
                buf = new byte[len];
                DBstream.Read(buf, 0, len);
                friends.Add(Encoding.UTF8.GetString(buf));
            }

            if (friends.Count <= 0)
            {
                Console.WriteLine("친구 리스트 빔");
                stream.Write(BitConverter.GetBytes(0), 0, sizeof(int));
                return;
            }

            stream.Write(BitConverter.GetBytes(friends.Count), 0, sizeof(int));
            foreach (string temp in friends)
            {
                buf = Encoding.UTF8.GetBytes(temp);
                stream.Write(BitConverter.GetBytes(buf.Length), 0, sizeof(int));
                stream.Write(buf, 0, buf.Length);
            }
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
        //UserCp CopyUsers(User user)
        //{
        //    UserCp copy = new UserCp(ref user.id, ref user.state);
        //    return copy;
        //}
    }

    static public partial class CONST
    {
        public const string IP_USER = "10.10.20.48";
        public const int PORT_USER = 0;
        public const string IP_DB = "10.10.20.213";
        public const int PORT_DB = 10000;
        public const string IP_LOBBY = "10.10.20.47";
        public const int PORT_LOBBY = 5001;

        //public const string UPDATE_STATE = "update users set state = 1 where id = '";
        public const string GET_FIRENDS = "SELECT friendlist.friend_id, users.nickname, users.state " +
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
