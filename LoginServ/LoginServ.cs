using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using static LoginServ.CONST;
using System.Threading;

namespace LoginServ
{
    class LoginServ
    {
        IPEndPoint userPoint = new IPEndPoint(IPAddress.Parse(IP_USERSERV), PORT_USERSERV);
        IPEndPoint dbPoint = new IPEndPoint(IPAddress.Parse(IP_DB), PORT_DB);
        bool ServOn = true;
        static void Main(string[] args)
        {
            LoginServ login = new LoginServ();
        }

        LoginServ()
        { }
        

        public void Run()
        {
            TcpListener listen = new TcpListener(userPoint);
            listen.Start();

            while(ServOn)
            {
                TcpClient client = listen.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                Thread thread = new Thread(() => Login_Thread(stream));
            }
        }

        void Login_Thread(NetworkStream stream)
        {
            var buf = new byte[sizeof(int)];
            stream.Read(buf, 0, sizeof(int));
            int len = BitConverter.ToInt32(buf, 0);
            buf = new byte[len];
            stream.Read(buf, 0, len);
            string query = Encoding.UTF8.GetString(buf);

            TcpClient DBclient = new TcpClient(userPoint);
            DBclient.Connect(dbPoint);
            NetworkStream DBstream = DBclient.GetStream();

            DBstream.Write(BitConverter.GetBytes(query.Length), 0, sizeof(int));
            DBstream.Write(Encoding.UTF8.GetBytes(query), 0, query.Length);
            buf = new byte[sizeof(int)];
            DBstream.Read(buf, 0, sizeof(int));
            int num = BitConverter.ToInt32(buf,0);
            if (num < 0)
            {
                stream.Write(BitConverter.GetBytes(-1), 0, sizeof(int));
                return;
            }
            stream.Write(BitConverter.GetBytes(num), 0, sizeof(int));
            for(int i=0; i<num; ++i)
            {
                DBstream.Read(buf, 0, sizeof(int));
                len = BitConverter.ToInt32(buf, 0);
                buf = new byte[len];
                DBstream.Read(buf, 0, len);
            }
        }

        //string Get_Query(string id, string pass)
        //{
        //    string query = $"{LOGIN1}{id}{LOGIN2}{pass}'";
        //    return query;
        //}
    }

    static class CONST
    {
        public const string IP_USERSERV = "10.10.20.48";
        public const int PORT_USERSERV = 0;
        public const string IP_DB = "10.10.20.213";
        public const int PORT_DB = 10000;

        //public const string LOGIN1 = "SELECT id, nickname from users where id='";
        //public const string LOGIN2 = "' pass='";
    }
}
