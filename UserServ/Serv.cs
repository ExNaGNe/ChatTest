﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using static Server.CONST;
using System.Threading;
using System.IO;
using System.Timers;
using MySql.Data.MySqlClient;
using System.Runtime.CompilerServices;
using System.Data.Common;

namespace Server
{
    static public partial class CONST
    {
        public const int PORT_USERSERV = 10005;             //유저 서버 포트
        public const string IP_LOBBY = "10.10.20.47";       //로비 서버 IP
        public const int PORT_LOBBY = 7000;                 //로비 서버 포트
        //DP 연결 문자열
        public const string DB_CONN = "Server=10.10.20.213;Port=3306;Database=VoiceChat;Uid=root;Pwd=1234;Charset=utf8";
        //친구 리스트 쿼리문자열
        public const string GET_FIRENDS = "SELECT friendlist.friend_id, users.nickname, users.state, users.location, date_format(users.lastout, '%Y-%m-%d %H:%i') " +
            "FROM friendlist inner join users on users.id = friendlist.friend_id where friendlist.id = '";
        public const string GET_FIRENDS1 = " and users.state != 3";
        public const string GET_FIRENDS2 = "' and friendlist.friend_id = '";
        //친구 추가 문자열
        public const string GET_ACCEPT1 = "insert into friendlist select '";
        public const string GET_ACCEPT2 = "', now() from dual where not exists(select * from friendlist where id ='";
        public const string GET_ACCEPT3 = "' and friend_id ='";
        //친구 삭제 문자열
        public const string DEL_FRIEND1 = "delete from friendlist where id ='";
        public const string DEL_FRIEND2 = "' and friend_id = '";
        //접속 종료 시간 문자열
        public const string UP_OUTDATE = "update users set state = 0,lastout = now() where id = '";
        //동기화 인터벌 시간
        public const int REFRESH_INTERVAL = 5000;
        
        //현재 시간 반환
        public static string NOW() => DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        //IP 구하기
        public static string GetLocalIPAddress()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }

        public enum STATE
        {
            OFFLINE,
            ONLINE,
            BUSY,
            HIDE
        }

        public enum SIGN
        {
            REFRESH,
            ADD_FRIEND,
            INVITE,
            ACCEPT_FRIEND,
            DEL_FRIEND
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
            stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetBytes(str).Length), 0, sizeof(int));
            stream.Write(Encoding.UTF8.GetBytes(str), 0, Encoding.UTF8.GetBytes(str).Length);
        }

        static public void Write(NetworkStream stream, List<string> str)
        {
            stream.Write(BitConverter.GetBytes(str.Count), 0, sizeof(int));

            foreach(string temp in str)
            {
                var buf = Encoding.UTF8.GetBytes(temp);
                stream.Write(BitConverter.GetBytes(buf.Length), 0, sizeof(int));
                stream.Write(buf, 0, buf.Length);
            }
        }

        static public void Write(NetworkStream stream, IEnumerable <string> str)
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

    class UserServ
    {
        static void Main(string[] args)
        {
            UserServ user = new UserServ();
            user.Run();
        }

        TcpClient Lobbyclient;
        TcpListener listen;
        NetworkStream Lobbystream;
        bool th_flag = true;
        bool Re_flag = false;
        public Mutex users_mutex = new Mutex(false, "usermutex");
        public List<User> users = new List<User>();
        public delegate void Refreshing();
        public event Refreshing RefreshEvent;
        System.Timers.Timer timer;

        public UserServ()   
        {}

        public void Run()   //로비 서버와 연결, 타이머 세팅 후 클라이언트와 연결
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

            timer = new System.Timers.Timer(REFRESH_INTERVAL);  //동기화 타이머(5초)
            timer.Elapsed += TimerHandler;
            timer.AutoReset = true;

            timer.Enabled = true;
            Connect_Cla();          //클라이언트 연결
        }

        void Lobby_th()             //로비 서버 쓰레드
        {
            User temp;
            STATE state;
            string id;
            string sign;
            while (th_flag)
            {
                try
                {
                    string read = NETSTREAM.ReadStr(Lobbystream);
                    if (!string.IsNullOrEmpty(read))
                    {
                        Console.WriteLine($"[{NOW()}]로비로부터 받은 문자열:{read}");
                        id = read.Split(",".ToCharArray())[1];
                        sign = read.Split(",".ToCharArray())[0];
                        //Consol.WriteLine($"[{NOW()}]로비로부터 받은 값:{id},{state}");               
                        try
                        {
                            switch (int.Parse(sign))
                            {
                                case 1:
                                    state = (STATE)int.Parse(read.Split(",".ToCharArray())[3]);
                                    temp = users.Find(x => x.info.id == id);
                                    users_mutex.WaitOne();
                                    temp.info.state = state;
                                    users_mutex.ReleaseMutex();
                                    temp.Refresh();
                                    Re_flag = true;
                                    break;
                                case 2:
                                    temp = users.Find(x => x.info.id == id);
                                    temp.Refresh();
                                    Re_flag = true;
                                    break;
                            }
                        }
                        catch (NullReferenceException ex)
                        {
                            Console.WriteLine($"[{NOW()}]새 클라이언트 접속");
                            continue;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{NOW()}]로비 쓰레드 종료");
                        th_flag = false;
                        if (listen != null)
                            listen.Stop();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{NOW()}]로비 서버 접속 끊김 {ex.Message}");
                    th_flag = false;
                    if (listen != null)
                        listen.Stop();
                    return;
                }
            }
            Lobbystream.Close();
        }

        void Connect_Cla()          //클라이언트 연결
        {
            listen = new TcpListener(new IPEndPoint(IPAddress.Any, PORT_USERSERV));
            listen.Start();

            while (th_flag)
            {
                try
                {
                    TcpClient client = listen.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();

                    string id = NETSTREAM.ReadStr(stream);
                    string nick = id.Split(",".ToCharArray())[1];
                    id = id.Split(",".ToCharArray())[0];
                    Console.WriteLine($"[{NOW()}]클라이언트 연결됨({id},{nick})");

                    Info info = new Info(id, nick);

                    User user = new User(info, stream, this);
                    Re_flag = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{NOW()}]클라이언트 연결 실패{ex.Message} {ex.StackTrace}");
                }
            }
            Console.WriteLine($"[{NOW()}]클라이언트 접속 대기 종료");
        }

        //동기화 타이머 동작
        private void TimerHandler(object sender, ElapsedEventArgs e)
        {
            var copy = RefreshEvent;
            if (Re_flag && copy != null)
            {
                Console.WriteLine($"[{NOW()}]타이머 이벤트 발생");
                copy();
            }
            Re_flag = false;
        }

        //유저 클래스
        public class User
        {
            public Info info { get; private set; }
            public NetworkStream stream { get; private set; }
            UserServ serv;
            public Mutex MyMutex { get; private set; }

            public User()
            { }

            public User(Info info, NetworkStream stream, UserServ serv)
            {
                this.info = info;
                this.stream = stream;
                this.serv = serv;
                MyMutex = new Mutex(false, $"{info.id}Mutex");

                Thread thread = new Thread(User_th);                //클라이언트 통신 쓰레드 생성
                thread.Start();

                this.serv.users_mutex.WaitOne();
                this.serv.users.Add(this);
                this.serv.users_mutex.ReleaseMutex();

                Refresh();                                          //입장 후 동기화
                this.serv.RefreshEvent += Refresh;       //동기화 이벤트에 추가
            }

            public void Refresh()   //동기화 이벤트 발생시 동기화 쓰레드 생성
            {
                Console.WriteLine($"[{NOW()}]{info.id} 동기화 진입");
                Thread thread = new Thread(list_th);
                thread.Start();
            }

            void User_th()  //유저 통신 쓰레드
            {
                int sign = 0;
                while (sign != -1 || serv.th_flag == true)
                {
                    try
                    {
                        sign = NETSTREAM.ReadInt(stream);
                        switch (sign)
                        {
                            case (int)SIGN.ADD_FRIEND:     //친구 신청 보냄
                                Console.WriteLine($"[{NOW()}]{info.id} 친구 신청");
                                Push(sign);
                                break;
                            case (int)SIGN.INVITE:     //방 초대 보냄
                                Console.WriteLine($"[{NOW()}]{info.id} 초대");
                                Push(sign);
                                break;
                            case (int)SIGN.ACCEPT_FRIEND:     //친구 신청 수락
                                Console.WriteLine($"[{NOW()}]{info.id} 친구 신청 수락");
                                Accept();
                                Refresh();
                                break;
                            case (int)SIGN.DEL_FRIEND:     //친구 삭제
                                Console.WriteLine($"[{NOW()}]{info.id} 친구 삭제");
                                DelFriend();
                                Refresh();
                                break;
                            case -1:
                                Console.WriteLine($"[{NOW()}]{info.id} 클라이언트 종료");
                                Disconnect();
                                return;
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"[{NOW()}]{info.id} 유저 연결 끊김{ex.StackTrace}");
                        Disconnect();
                        return;
                    }
                }
            }

            void Disconnect()   //유저 접속 종료 함수
            {
                serv.RefreshEvent -= Refresh;
                serv.users_mutex.WaitOne();
                serv.users.Remove(this);
                serv.users_mutex.ReleaseMutex();
                serv.Re_flag = true;
                stream.Close();
            }

            void Push(int sign)     //유저 신호 처리 함수
            {
                string id = string.Empty;
                string num = string.Empty;
                //string title = string.Empty;
                string origin = NETSTREAM.ReadStr(stream);
                if (sign == (int)SIGN.INVITE)
                { 
                    id = origin.Split(",".ToCharArray())[0];
                    num = origin.Split(",".ToCharArray())[1];
                    //title = origin.Split(",".ToCharArray())[2];

                    using (MySqlConnection conn = new MySqlConnection(DB_CONN))
                    {
                        try
                        {
                            conn.Open();
                            if (conn.Ping() == false)
                            {
                                Console.WriteLine($"[{NOW()}]친구 초대 DB 연결 에러");
                                return;
                            }
                            string query = Get_SelFriendQuery(id);
                            MySqlCommand comm = new MySqlCommand(query, conn);

                            using (MySqlDataReader reader = comm.ExecuteReader())
                            {
                                reader.Read();
                                Console.WriteLine($"[{NOW()}]{reader[0]} 위치:{reader[3]}");
                                if ((string) reader[3] != "로비")
                                {
                                    return;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{NOW()}]친구 초대 DB 에러 {ex.Message} {ex.StackTrace}");
                        }
                    }
                }
                else if(sign== (int) SIGN.ADD_FRIEND)
                {
                    id = origin;
                    using (MySqlConnection conn = new MySqlConnection(DB_CONN))
                    {
                        try
                        {
                            conn.Open();
                            if (conn.Ping() == false)
                            {
                                Console.WriteLine($"[{NOW()}]친구 신청 DB 연결 에러");
                                return;
                            }
                            string query = Get_SelFriendQuery(id);
                            MySqlCommand comm = new MySqlCommand(query, conn);

                            using (MySqlDataReader reader = comm.ExecuteReader())
                            {
                                if(reader.HasRows)
                                {
                                    return;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{NOW()}]친구 신청 DB 에러 {ex.Message}");
                        }
                    }
                }
                serv.users_mutex.WaitOne();
                List<User> copy = new List<User>(serv.users);
                serv.users_mutex.ReleaseMutex();

                try
                {
                    User temp = copy.Find(x => x.info.id == id);
                    temp.MyMutex.WaitOne();
                    NETSTREAM.Write(temp.stream, sign);              //sign 전송
                    if(sign == (int) SIGN.INVITE)
                    { 
                        NETSTREAM.Write(temp.stream, $"{info.nick_name}({info.id}),{num}");
                        Console.WriteLine($"[{NOW()}]{id} 유저 초대 {info.id}가 {num}번방으로 초대");
                    }
                    else
                        NETSTREAM.Write(temp.stream, info.GetString());
                    temp.MyMutex.ReleaseMutex();
                }
                catch (ArgumentNullException ex)
                {
                    Console.WriteLine($"[{NOW()}]초대 or 신청 실패:{ex.Message}");
                }
            }

            void Accept()       //유저 친구 수락 함수
            {
                //string id = string.Empty;
                string id = NETSTREAM.ReadStr(stream);
                //id = origin.Split(",".ToCharArray())[0];

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
                        string query = Get_AcceptQuery(id, info.id);
                        Console.WriteLine(query);
                        MySqlCommand comm = new MySqlCommand(query, conn);
                        int result1 = comm.ExecuteNonQuery();

                        query = Get_AcceptQuery(info.id, id);
                        Console.WriteLine(query);
                        comm = new MySqlCommand(query, conn);
                        int result2 = comm.ExecuteNonQuery();

                        if(result1 != 1 || result2 != 1)
                        {
                            Console.WriteLine($"[{NOW()}]친구 신청 수락 에러");
                        }
                        serv.Re_flag = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{NOW()}]친구 신청 수락 에러 {ex.Message}");
                    }
                }
            }

            void DelFriend()    //유저 친구 삭제 함수
            {
                string id = NETSTREAM.ReadStr(stream);

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
                        string query = Del_FirendQuery(id, info.id);
                        MySqlCommand comm = new MySqlCommand(query, conn);
                        int result1 = comm.ExecuteNonQuery();

                        query = Del_FirendQuery(info.id, id);
                        comm = new MySqlCommand(query, conn);
                        int result2 = comm.ExecuteNonQuery();

                        if (result1 != 1 || result2 != 1)
                        {
                            Console.WriteLine($"[{NOW()}]친구 삭제 에러");
                        }
                        serv.Re_flag = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{NOW()}]친구 삭제 에러 {ex.Message}");
                    }
                }
            }

            void list_th()    //유저 리스트, 친구 리스트 동기화 쓰레드
            {
                Console.WriteLine($"[{NOW()}]{info.id} 동기화");
                try
                {
                    serv.users_mutex.WaitOne();
                    var copy = from user in serv.users
                               where user.info.state != STATE.HIDE
                               select user.info.GetString();
                    serv.users_mutex.ReleaseMutex();
                    //유저 리스트 전송
                    MyMutex.WaitOne();
                    NETSTREAM.Write(stream, (int)SIGN.REFRESH);
                    //Console.WriteLine($"동기화 유저 리스트:{copy.Count()}");
                    NETSTREAM.Write(stream, copy);
                    //친구 리스트 전송
                    List<string> rows = new List<string>();
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
                            string query = Get_FriendQuery();
                            MySqlCommand comm = new MySqlCommand(query, conn);

                            using (MySqlDataReader reader = comm.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    rows.Add(GetRow(reader));  //보낼 행 리스트에 추가
                                }
                            }
                            NETSTREAM.Write(stream, rows);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{NOW()}]친구 리스트 전송 에러 {ex.Message}");
                        }
                    }
                    MyMutex.ReleaseMutex();
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"[{NOW()}]동기화 에러 {ex.Message} {ex.StackTrace}");
                    MyMutex.Close();
                }
            }

            string GetRow(MySqlDataReader reader)
            {
                string row = string.Empty;

                row += $"{reader[0]},{reader[1]}";

                switch(int.Parse(reader[2].ToString()))
                {
                    case 0:
                        DateTime last = Convert.ToDateTime(reader[4]);
                        if (DateTime.Now.Subtract(last).TotalDays >= 1)
                        {
                            row += $",{((int)DateTime.Now.Subtract(last).TotalDays).ToString()}일 전 접속";
                        }
                        else if (DateTime.Now.Subtract(last).TotalHours >= 1)
                        {
                            row += $",{((int)DateTime.Now.Subtract(last).TotalHours).ToString()}시간 전 접속";
                        }
                        else if (DateTime.Now.Subtract(last).TotalMinutes >= 1)
                        {
                            row += $",{((int)DateTime.Now.Subtract(last).TotalMinutes).ToString()}분 전 접속";
                        }
                        else
                        {
                            row += $",{((int)DateTime.Now.Subtract(last).TotalSeconds).ToString()}초 전 접속";
                        }
                        break;
                    default:
                        row += $",{reader[2]}";
                        break;
                }

                row += $",{reader[3]}";
                return row;
            }

            string Get_FriendQuery()
            {
                return $"{GET_FIRENDS}{info.id}'{GET_FIRENDS1}";
            }

            string Get_SelFriendQuery(string id)
            {
                return $"{GET_FIRENDS}{info.id}{GET_FIRENDS2}{id}'";
            }

            string Get_AcceptQuery(string id, string friendid)
            {
                return $"{GET_ACCEPT1}{id}','{friendid}{GET_ACCEPT2}{id}{GET_ACCEPT3}{friendid}')";
            }

            string Del_FirendQuery(string id, string friendid)
            {
                return $"{DEL_FRIEND1}{id}{DEL_FRIEND2}{friendid}'";
            }

            string Update_OutQuery()
            {
                return $"{UP_OUTDATE}{info.id}'";
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
            return $"{id},{nick_name},{(int)state},{location}";
        }
    }
}
