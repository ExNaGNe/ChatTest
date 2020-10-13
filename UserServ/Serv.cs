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
        private Mutex users_mutex = new Mutex(false,"user mutex");
        List<UserList> users = new List<UserList>();

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
                NetworkStream stream =  client.GetStream();
                byte[] buf = new byte[sizeof(int)];
                stream.Read(buf, 0, sizeof(int));
                int len = BitConverter.ToInt32(buf, 0);
                buf = new byte[len];
                stream.Read(buf, 0, len);

                string id = Encoding.UTF8.GetString(buf);
                UserList user = new UserList(id, STATE.ONLINE, stream, users_mutex);
                users_mutex.WaitOne();
                users.Add(user);
                users_mutex.ReleaseMutex();
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
            while(th_flag)
            {
                byte[] buf = new byte[sizeof(int)];
                Lobbystream.Read(buf, 0, sizeof(int));
                if(BitConverter.ToInt32(buf, 0) > 0)
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

    public class UserList
    {
        public string id { get; private set; }
        public STATE state { get; private set; }
        public NetworkStream stream { get; private set; }
        Mutex mutex { get; private set; }
        public delegate void 

        public UserList()
        { }

        public UserList(string id, STATE state,NetworkStream stream, Mutex mutex)
        {
            this.id = id;
            this.state = state;
            this.stream = stream;
            this.mutex = mutex;

            Thread thread = new Thread(User_th);
        }

        void User_th()
        {
            IPEndPoint userPoint = new IPEndPoint(IPAddress.Parse(IP_USER), PORT_USER);
            IPEndPoint dbPoint = new IPEndPoint(IPAddress.Parse(IP_DB), PORT_DB);
            TcpClient DBclient = new TcpClient(userPoint);
            DBclient.Connect(dbPoint);
            NetworkStream DBstream = DBclient.GetStream();

            string query = $"{GET_FIRENDS}{id}'";
            byte[] buf = Encoding.UTF8.GetBytes(query);
            DBstream.Write(BitConverter.GetBytes(buf.Length), 0, sizeof(int));
            DBstream.Write(buf, 0, buf.Length);

            buf = new byte[4];
            DBstream.Read(buf, 0, sizeof(int));
            int num = BitConverter.ToInt32(buf, 0);
            if(num < 0)
            {
                Console.WriteLine("친구 리스트 DB 접근 에러");
                return;
            }
        }

        void Friend_th()
        {

        }
    }
}
