using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using static LoginServ.CONST;
using System.Threading;
using MySql.Data.MySqlClient;

namespace LoginServ
{
    static class CONST
    {
        //public const string IP_USERSERV = "10.10.20.48";
        public const int PORT_LOGINSERV = 10001;
        //DP 연결 문자열
        public const string DB_CONN = "Server=10.10.20.213;Port=3306;Database=VoiceChat;Uid=root;Pwd=1234;Charset=utf8";
        //로그인 쿼리 문자열
        public const string LOGIN_QUERY1 = "select id, nickname, state, location from users where id = '";
        public const string LOGIN_QUERY2 = "' and pass = '";
        //회원가입 쿼리 문자열
        public const string SIGNIN_QUERY1 = "insert into users(id, pass, nickname, signin) values('";
        public const string SIGNIN_QUERY2 = "',now())";
        //현재 시간 반환
        public static string NOW() => DateTime.Now.ToString("HH:mm:ss");

        public enum SIGN
        {
            LOGIN = 1,
            SIGNIN
        }
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

    class LoginServ
    {
        bool ServOn = true;

        static void Main(string[] args)
        {
            LoginServ login = new LoginServ();
            login.Run();
        }
        
        public void Run()
        {
            TcpListener listen = new TcpListener(new IPEndPoint(IPAddress.Any, PORT_LOGINSERV));
            listen.Start();

            while(ServOn)
            {
                TcpClient client = listen.AcceptTcpClient();
                Console.WriteLine($"[{NOW()}]클라이언트 접속");
                NetworkStream stream = client.GetStream();
                Thread thread = new Thread(() => Login_Thread(stream));
                thread.Start();
            }
        }

        void Login_Thread(NetworkStream stream)
        {
            string id = string.Empty;
            string pass = string.Empty;
            string nick = string.Empty;
            try
            {
                int sign = NETSTREAM.ReadInt(stream);

                id = NETSTREAM.ReadStr(stream);
                switch (sign)
                {
                    case (int)SIGN.LOGIN:
                        pass = id.Split(",".ToCharArray())[1];
                        id = id.Split(",".ToCharArray())[0];
                        Console.WriteLine($"[{NOW()}]로그인: {id}");
                        Login(stream, id, pass);
                        break;
                    case (int)SIGN.SIGNIN:
                        nick = id.Split(",".ToCharArray())[2];
                        pass = id.Split(",".ToCharArray())[1];
                        id = id.Split(",".ToCharArray())[0];
                        Console.WriteLine($"[{NOW()}]회원가입: {id}");
                        SignIn(stream, id, pass, nick);
                        break;
                    case -1:
                        return;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[{NOW()}] 클라이언트 접속 에러 {ex.Message}");
            }
            stream.Close();
        }

        void Login(NetworkStream stream, string id, string pass)
        {
            using (MySqlConnection conn = new MySqlConnection(DB_CONN))
            {
                try
                {
                    conn.Open();
                    if (conn.Ping() == false)
                    {
                        Console.WriteLine($"[{NOW()}]DB 연결 에러");
                        return;
                    }
                    string query = Get_LoginQuery(id, pass);
                    MySqlCommand comm = new MySqlCommand(query, conn);

                    using (MySqlDataReader reader = comm.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            NETSTREAM.Write(stream, 1);
                            NETSTREAM.Write(stream, GetLoignRow(reader));
                        }
                        else
                            NETSTREAM.Write(stream, 0);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{NOW()}]로그인 에러: {ex.Message}");
                }
            }
        }

        void SignIn(NetworkStream stream, string id, string pass, string nick)
        {
            using (MySqlConnection conn = new MySqlConnection(DB_CONN))
            {
                try
                {
                    conn.Open();
                    if (conn.Ping() == false)
                    {
                        Console.WriteLine($"[{NOW()}]DB 연결 에러");
                        return;
                    }
                    string query = Get_LoginQuery(id, pass);
                    MySqlCommand comm = new MySqlCommand(query, conn);

                    using (MySqlDataReader reader = comm.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            //reader.Read();
                            query = Get_SigninQuery(id, pass, nick);
                            comm = new MySqlCommand(query, conn);
                            int result = comm.ExecuteNonQuery();
                            if(result > 0)
                            {
                                NETSTREAM.Write(stream, 2);
                            }
                        }
                        else
                            NETSTREAM.Write(stream, 0);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{NOW()}]회원가입 에러: {ex.Message}");
                }
            }
        }

        string Get_LoginQuery(string id, string pass)
        {
            return $"{LOGIN_QUERY1}{id}{LOGIN_QUERY2}{pass}'";
        }

        string Get_SigninQuery(string id, string pass, string nick)
        {
            return $"{SIGNIN_QUERY1}{id}','{pass}','{nick}{SIGNIN_QUERY2}";
        }

        string GetLoignRow(MySqlDataReader reader)
        {
            return $"{reader[0]},{reader[1]}";
        }
    }
}
