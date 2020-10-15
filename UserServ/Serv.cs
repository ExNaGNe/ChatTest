using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using static Serv.CONST;
using System.Threading;
using System.IO;
using System.Timers;
using MySql.Data.MySqlClient;

namespace Serv
{
    static public partial class CONST
    {
        public const string IP_USERSERV = "10.10.20.48";    //유저 서버 IP
        public const int PORT_USERSERV = 10005;             //유저 서버 포트
        //public const string IP_DB = "10.10.20.213";         //DP 서버 IP
        //public const int PORT_DB = 10000;                   //DP 서버 포트
        public const string IP_LOBBY = "10.10.20.47";       //로비 서버 IP
        public const int PORT_LOBBY = 7000;                 //로비 서버 포트
        //DP 연결 문자열
        public const string DB_CONN = "Server=\"10.10.20.213\";Port=3306;Database=VoiceChat;Uid=root;Pwd=1234";
        //친구 리스트 쿼리문자열
        public const string GET_FIRENDS = "SELECT friendlist.friend_id, users.nickname, users.state, users.location " +
            "FROM friendlist inner join users on users.id = friendlist.friend_id where friendlist.id = '";
        //친구 추가 문자열1
        public const string GET_ACCEPT1 = "insert into friendlist values('";
        //친구 추가 문자열2
        public const string GET_ACCEPT2 = "', now())";
        //동기화 인터벌 시간
        public const int REFRESH_INTERVAL = 5000;
        //public const string UPDATE_STATE = "update users set state = 1 where id = '";

        public enum STATE
        {
            OFFLINE,
            ONLINE,
            BUSY,
            HIDE
        }

        public enum SIGN
        {
            ADD_FRIEND,
            INVITE,
            ACCEPT_FRIEND
        }
    }

    class UserServ
    {
        static void Main(string[] args)
        {
            UserServ user = new UserServ();
            user.Run();
        }

        TcpClient Lobbyclient;
        readonly NetworkStream Lobbystream;
        bool th_flag = true;
        bool Re_flag = false;
        IPEndPoint userPoint = new IPEndPoint(IPAddress.Parse(IP_USERSERV), 0);
        IPEndPoint ServPoint = new IPEndPoint(IPAddress.Parse(IP_USERSERV), PORT_USERSERV);
        IPEndPoint lobbyPoint = new IPEndPoint(IPAddress.Parse(IP_LOBBY), PORT_LOBBY);
        private Mutex users_mutex = new Mutex(false, "usermutex");
        public List<User> users = new List<User>();
        public delegate void Refreshing();
        public event Refreshing RefreshEvent;
        System.Timers.Timer timer;

        public UserServ()
        {
            try
            { 
            Lobbyclient = new TcpClient(userPoint);
            Lobbyclient.Connect(lobbyPoint);                //로비서버 연결
            Lobbystream = Lobbyclient.GetStream();

            Console.WriteLine("로비서버 연결");
            Thread lobby_th = new Thread(new ThreadStart(Lobby_th));
            lobby_th.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"로비서버 연결 실패 {ex.Message}");
            }
        }

        public void Run()
        {
            timer = new System.Timers.Timer(REFRESH_INTERVAL);  //동기화 타이머(5초)
            timer.Elapsed += TimerHandler;
            timer.AutoReset = true;

            timer.Enabled = true;
            Connect_Cla();          //클라이언트 연결
        }

        void Lobby_th()
        {
            while (th_flag)
            {
                try
                {
                    byte[] buf = BitConverter.GetBytes(0);
                    Lobbystream.Read(buf, 0, sizeof(int));
                    int len = BitConverter.ToInt32(buf, 0);
                    if (len > 0)
                    {
                        Console.WriteLine($"로비로부터 받은 길이:{len}");
                        buf = new byte[len];
                        Lobbystream.Read(buf, 0, len);
                        string id = Encoding.UTF8.GetString(buf).Split(",".ToCharArray())[0];
                        STATE state = (STATE)int.Parse(Encoding.UTF8.GetString(buf).Split(",".ToCharArray())[1]);
                        Console.WriteLine($"로비로부터 받은 값:{id},{state}");
                        User temp = users.Find(x => x.info.id == id);
                        users_mutex.WaitOne();
                        temp.info.state = state;
                        users_mutex.ReleaseMutex();

                        Re_flag = true;
                    }
                    else
                    {
                        Console.WriteLine($"들어온 신호:{BitConverter.ToInt32(buf, 0)}");
                        th_flag = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("로그인 서버 접속 끊김");
                    th_flag = false;
                }
            }
            Lobbystream.Close();
        }

        void Connect_Cla()  //클라이언트 연결
        {
            TcpListener listen = new TcpListener(ServPoint);
            listen.Start();

            while (th_flag)
            {
                try
                {
                    TcpClient client = listen.AcceptTcpClient();
                    Console.WriteLine("클라이언트 연결됨");
                    NetworkStream stream = client.GetStream();
                    byte[] buf = new byte[sizeof(int)];
                    stream.Read(buf, 0, sizeof(int));
                    int len = BitConverter.ToInt32(buf, 0);
                    buf = new byte[len];
                    stream.Read(buf, 0, len);

                    string id = Encoding.UTF8.GetString(buf);
                    string nick = id.Split(",".ToCharArray())[1];
                    id = id.Split(",".ToCharArray())[0];

                    User user = new User(new Info(id, nick), stream, this);
                    users_mutex.WaitOne();
                    users.Add(user);
                    users_mutex.ReleaseMutex();
                    Re_flag = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("클라이언트 연결 실패");
                }
            }
        }

        private void TimerHandler(object sender, ElapsedEventArgs e)
        {
            var copy = RefreshEvent;
            if (Re_flag && copy != null)
            {
                copy();
            }
        }

        public class User
        {
            public Info info { get; private set; }
            public NetworkStream stream { get; private set; }
            UserServ Serv;
            public Mutex MyMutex { get; private set; }

            public User()
            { }

            public User(Info info, NetworkStream stream, UserServ serv)
            {
                this.info = info;
                this.stream = stream;
                Serv = serv;
                MyMutex = new Mutex(false, $"{info.id}Mutex");
                Serv.RefreshEvent += new Refreshing(Refresh);
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
                while (sign != -1)
                {
                    try
                    {
                        byte[] buf = new byte[sizeof(int)];
                        buf = BitConverter.GetBytes(-1);
                        stream.Read(buf, 0, sizeof(int));
                        sign = BitConverter.ToInt32(buf, 0);

                        switch (sign)
                        {
                            case (int)SIGN.ADD_FRIEND:     //친구 신청 보냄
                                Console.WriteLine("친구 신청");
                                Push(sign);
                                break;
                            case (int)SIGN.INVITE:     //방 초대 보냄
                                Console.WriteLine("초대");
                                Push(sign);
                                break;
                            case (int)SIGN.ACCEPT_FRIEND:     //친구 신청 수락
                                Console.WriteLine("친구 신청 수락");
                                Accept();
                                break;
                            case -1:
                                Console.WriteLine("클라이언트 종료");
                                Disconnect();
                                return;
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"{info.id} 연결 끊김 {ex.Message}");
                        Disconnect();
                        return;
                    }
                }
            }

            void Disconnect()
            {
                Serv.RefreshEvent -= new Refreshing(Refresh);
                Serv.users_mutex.WaitOne();
                Serv.users.Remove(this);
                Serv.users_mutex.ReleaseMutex();
            }

            void Push(int sign)
            {
                string id = string.Empty;
                string num = string.Empty;
                string title = string.Empty;
                var buf = new byte[sizeof(int)];
                stream.Read(buf, 0, sizeof(int));       //문자열 길이 읽음
                int len = BitConverter.ToInt32(buf, 0);
                buf = new byte[len];
                stream.Read(buf, 0, len);               //문자열 읽음
                string origin = Encoding.UTF8.GetString(buf);
                if (sign == (int)SIGN.INVITE)
                { 
                    id = origin.Split(",".ToCharArray())[0];
                    num = origin.Split(",".ToCharArray())[1];
                    title = origin.Split(",".ToCharArray())[2];
                }
                else
                    id = origin;
                Serv.users_mutex.WaitOne();
                List<User> copy = new List<User>(Serv.users);
                Serv.users_mutex.ReleaseMutex();

                try
                {
                    User temp = copy.Find(x => x.info.id == id);
                    MyMutex.WaitOne();
                    buf = BitConverter.GetBytes(sign);
                    temp.stream.Write(buf, 0, sizeof(int));         //사인 전송
                    if(sign == (int) SIGN.INVITE)
                        buf = Encoding.UTF8.GetBytes(info.GetString()+$",{num},{title}");
                    else
                        buf = Encoding.UTF8.GetBytes(info.GetString());
                    temp.stream.Write(BitConverter.GetBytes(buf.Length), 0, sizeof(int));  //문자열 길이 전송
                    temp.stream.Write(buf, 0, buf.Length);                 //문자열 전송
                    MyMutex.ReleaseMutex();
                }
                catch (ArgumentNullException ex)
                {
                    Console.WriteLine($"초대 or 신청 실패:{ex.Message}");
                }
            }

            void Accept()
            {
                string id = string.Empty;
                var buf = new byte[sizeof(int)];
                stream.Read(buf, 0, sizeof(int));       //문자열 길이 읽음
                int len = BitConverter.ToInt32(buf, 0);
                buf = new byte[len];
                stream.Read(buf, 0, len);               //문자열 읽음
                string origin = Encoding.UTF8.GetString(buf);
                id = origin.Split(",".ToCharArray())[0];

                using (MySqlConnection conn = new MySqlConnection(DB_CONN))
                {
                    try
                    {
                        conn.Open();
                        if (conn.Ping() == false)
                        {
                            Console.WriteLine($"DB 연결 에러");
                            return;
                        }
                        string query = Get_AcceptQuery(id, info.id);
                        MySqlCommand comm = new MySqlCommand(query, conn);
                        int result1 = comm.ExecuteNonQuery();

                        query = Get_AcceptQuery(info.id, id);
                        comm = new MySqlCommand(query, conn);
                        int result2 = comm.ExecuteNonQuery();

                        if(result1 != 1 || result2 != 1)
                        {
                            Console.WriteLine("친구 신청 수락 에러");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"친구 리스트 전송 에러 {ex.Message}");
                    }
                }
            }

            void list_th()    //유저 리스트, 친구 리스트 전송 쓰레드
            {
                Serv.users_mutex.WaitOne();
                List<User> copy = Serv.users.ToList();
                Serv.users_mutex.ReleaseMutex();
                //유저 리스트 전송
                MyMutex.WaitOne();
                var buf = BitConverter.GetBytes(0);         //사인
                stream.Write(buf, 0, sizeof(int));
                buf = BitConverter.GetBytes(copy.Count(x => x.info.state != STATE.HIDE));  //보낼 개수
                stream.Write(buf, 0, sizeof(int));
                foreach (User temp in copy)
                {
                    if (temp.info.state == STATE.HIDE)
                        continue;
                    buf = Encoding.UTF8.GetBytes(temp.info.GetString());
                    var len = BitConverter.GetBytes(buf.Length);   
                    stream.Write(len, 0, sizeof(int));      //보낼 문자열 길이
                    stream.Write(buf, 0, buf.Length);       //유저 정보 문자열
                }
                //친구 리스트 전송
                List<string> rows = new List<string>();
                using (MySqlConnection conn = new MySqlConnection(DB_CONN))
                {
                    try
                    {
                        if (conn.Ping() == false)
                        {
                            Console.WriteLine($"DB 연결 에러");
                            return;
                        }
                        conn.Open();
                        string query = Get_FriendQuery(info.id);
                        MySqlCommand comm = new MySqlCommand(query, conn);

                        using (MySqlDataReader reader = comm.ExecuteReader())
                        {
                            //int field = reader.FieldCount;
                            while (reader.Read())
                            {
                                string row = string.Empty;
                                foreach (string temp in reader)
                                {
                                    if (temp == STATE.HIDE.ToString())
                                        row += $"{STATE.OFFLINE},";
                                    else
                                        row += $"{temp},";
                                }
                                row.Remove(row.Length - 1);
                                rows.Add(row);  //보낼 행 리스트에 추가
                            }
                        }

                        stream.Write(BitConverter.GetBytes(rows.Count), 0, sizeof(int));    //보낼 행 개수 전송
                        foreach (string temp in rows)
                        {
                            stream.Write(BitConverter.GetBytes(temp.Length), 0, sizeof(int));   //보낼 행 길이 전송
                            buf = Encoding.UTF8.GetBytes(temp);
                            stream.Write(buf, 0, buf.Length);           //행 전송
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"친구 리스트 전송 에러 {ex.Message}");
                    }
                }
                MyMutex.ReleaseMutex();
            }

            string Get_FriendQuery(string id)
            {
                return $"{GET_FIRENDS}{info.id}'";
            }

            string Get_AcceptQuery(string id, string friendid)
            {
                return $"{GET_ACCEPT1}{info.id}','{friendid}{GET_ACCEPT2}";
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
        public string GetString()
        {
            return $"{id},{nick_name},{state},{location}";
        }
    }
}
