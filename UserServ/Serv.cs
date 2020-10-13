using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using static Serv.CONST;

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

        public UserServ()
        {
            IPEndPoint user = new IPEndPoint(IPAddress.Parse(IP_USER), PORT_USER);
            IPEndPoint db = new IPEndPoint(IPAddress.Parse(IP_DB), PORT_DB);

            DBclient = new TcpClient(user);
            DBclient.Connect(db);
            DBstream = DBclient.GetStream();

            string query = "SELECT friendlist.*, users.nickname " +
                "FROM friendlist inner join users on users.id = friendlist.friend_id";
            byte[] buf = Encoding.UTF8.GetBytes(query);
            DBstream.Write(BitConverter.GetBytes(buf.Length), 0, sizeof(int));
            DBstream.Write(buf, 0, buf.Length);

            Get_db();
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
    }

    

    static public partial class CONST
    {
        public const string IP_USER = "10.10.20.48";
        public const int PORT_USER = 10000;
        public const string IP_DB = "10.10.20.213";
        public const int PORT_DB = 10000;
        public const string IP_LOBBY = "10.10.20.47";
        public const int PORT_LOBBY = 5001;
    }
}
