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
        TcpClient DBclient;
        readonly NetworkStream DBstream;
        TcpClient Lobbyclient;
        readonly NetworkStream Lobbystream;
        bool th_flag = true;

        public UserServ()
        {
            IPEndPoint user = new IPEndPoint(IPAddress.Parse(IP_USER), PORT_USER);
            IPEndPoint db = new IPEndPoint(IPAddress.Parse(IP_DB), PORT_DB);
            IPEndPoint lobby = new IPEndPoint(IPAddress.Parse(IP_LOBBY), PORT_LOBBY);

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

            Lobbyclient = new TcpClient(user);
            Lobbyclient.Connect(lobby);
            Lobbystream = Lobbyclient.GetStream();

            Thread lobby_th = new Thread(new ThreadStart(Lobby_th));
            lobby_th.Start();
            lobby_th.Join();
        }

        void Get_db()
        {
            byte[] buf = new byte[sizeof(int)];
            DBstream.Read(buf, 0, sizeof(int));
            int num = BitConverter.ToInt32(buf, 0);
            if (num > 0)
            {
                for (int i = 0; i < num; ++i)
                {
                    buf = new byte[sizeof(int)];
                    DBstream.Read(buf, 0, sizeof(int));
                    num = BitConverter.ToInt32(buf, 0);
                    buf = new byte[num];
                    DBstream.Read(buf, 0, num);
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

    }
}
