using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;


namespace Fix_Server
{
    class Program
    {
        TcpClient DB = null;
        Socket sock = null;
        Socket SockForServer = null;
        Socket servSock = null;
        IPEndPoint ep = null;
        static Socket[] sockets = new Socket[100]; //client
        static Socket[] serverSockets = new Socket[100];
        private static Mutex mutex = new Mutex();
        private static Mutex mutexForServer = new Mutex();
        List<Room> room = new List<Room>(); //룸 리스트
        static int roomCnt = 1;
        public const string DB_CONN = "Server=10.10.20.213;Port=3306;Database=VoiceChat;Uid=root;Pwd=1234;Charset=utf8";
        public const string UP_OUTDATE = "update users set state = 0,lastout = now() where id = '";
        public static string NOW() => DateTime.Now.ToString("[HH:mm:ss]");
        public const int MAX_Count = 10;
        
        //다른 서버들 접속 용
        public void ConnectWithServers() 
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            Console.WriteLine(NOW() + " 서버 빌드 시작");
            SockForServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ep = new IPEndPoint(IPAddress.Any, 7000);
            SockForServer.Bind(ep);
            SockForServer.Listen(10);
            Thread t2 = new Thread(AcceptServerSockets);
            t2.Start();
        }
        public void CurrentDomain_ProcessExit(object sender, EventArgs e)
        { 
            if (UpdateSqlQuery($"update users set state = 0, location=null") == true) //DB 업데이트
            {
            }
            else
            {
                Console.WriteLine(NOW() + " 방 나감 DB 업데이트 실패");
            }
            Console.WriteLine("exit");
        }
        //서버 시작
        public void ClientServerStart() 
        {
            try
            {
                Console.WriteLine(NOW() + " 클라이언트 서버 시작");
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ep = new IPEndPoint(IPAddress.Any, 7001);
                sock.Bind(ep);
                sock.Listen(10);
                Thread t2 = new Thread(AcceptSockets);
                t2.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //Accept 대기
        public void AcceptSockets() 
        {
            while (true)
            {
                Socket clientSock = sock.Accept();
                Console.WriteLine(NOW() + " 클라이언트 연결 성공");
                //스래드와 연결
                Thread t1 = new Thread(() => ConnectWithClient(clientSock));
                //스래드 시작
                t1.Start(); 
            }
        }
        //Accept 대기
        public void AcceptServerSockets() 
        {
            while (true)
            {
                try
                {
                    servSock = SockForServer.Accept();
                    Console.WriteLine(NOW() + " 유저 서버 연결 성공");
                    //스래드와 연결
                    Thread t1 = new Thread(ThreadServer);
                    //스래드 시작
                    t1.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(NOW() + e.ToString());
                    Console.WriteLine(NOW() + " 유저 서버 클라이언트 접속 실패");
                }

            }
        }

        public int ReceieveBytesToInt(Socket clientSock, byte[] bytes)
        {
            //바이트 사이즈 받는 부분
            clientSock.Receive(bytes, sizeof(int), SocketFlags.None); 
            int size = BitConverter.ToInt32(bytes, 0);
            return size;
        }
        public string ReceieveBytesToString(Socket clientSock, byte[] bytes)
        {
            //바이트 내용 string 받음
            string temp;
            clientSock.Receive(bytes, bytes.Length, SocketFlags.None); 
            temp= Encoding.UTF8.GetString(bytes);
            return temp;
        }
        public void SendToSign(Socket clientSock, int sign, string ID)
        {
            //원하는 sign 값을 클라이언트에 보낸다
            byte[] bytes = BitConverter.GetBytes(sign);
            clientSock.Send(bytes, sizeof(int), SocketFlags.None);
            Console.WriteLine(NOW() + $" {ID} 사인 보냄 : " + sign);
        }
        public void SendBytesSize(Socket clientSock, string words, string ID)
        {
            //바이트 사이즈를 보낸다
            int size;
            byte[] StrByte = Encoding.UTF8.GetBytes(words);
            size = StrByte.Length;
            clientSock.Send(BitConverter.GetBytes(size), sizeof(int), SocketFlags.None); 
            Console.WriteLine(NOW()+$" {ID} size 보냄");
        }
        public void SendBytesString(Socket clientSock, string words, string ID)
        {
            //string을 바이트로 바꾸는 매소드
            byte[] bytes;
            bytes = Encoding.UTF8.GetBytes(words);
            clientSock.Send(bytes, bytes.Length, SocketFlags.None); 
            Console.WriteLine(NOW() + $" {ID} String 보냄");
        }
        public void AddRoomAndUsers(Socket clientSock, Room r, string ID, string NickName,byte[] bytes)
        {
            //방에 있는 인원이 꽉 차지 않았을때
            if (r.RoomUsers.Count() < MAX_Count) 
            {
                //방에 비밀번호가 없는 경우
                if (string.IsNullOrEmpty(r.password) || r.RoomUsers.Count() <= 0) 
                {
                    Console.WriteLine(NOW() + $" {ID} 비밀번호 없음");
                    
                    //sign 1 보냄
                    SendToSign(clientSock, 1, ID); 

                    //ID, 닉네임, 소켓 저장
                    User user = new User();
                    user.ID = ID; 
                    user.Nick = NickName;
                    user.UserSocket = clientSock;


                    //룸 리스트 안에 넣기 전 뮤텍스 실행
                    r.RoomMutex.WaitOne(); 
                    r.RoomUsers.Add(user); 
                    foreach (User u in r.RoomUsers)
                    {
                        //사인 7 보내기
                        SendToSign(u.UserSocket, 7, ID);
                        //user 리스트에 있는 총 인원 보내기
                        SendToSign(u.UserSocket, r.RoomUsers.Count(), ID); 
                        foreach (User users in r.RoomUsers)
                        {
                            //string 사이즈 보내기
                            SendBytesSize(u.UserSocket, users.ToString(), ID);
                            //string 보내기
                            SendBytesString(u.UserSocket, users.ToString(), ID); 
                        }
                        Console.WriteLine(NOW() + $" {ID} ID 및 닉네임 전송");
                    }
                    //룸 뮤텍스 릴리즈
                    r.RoomMutex.ReleaseMutex();

                    //DB 업데이트
                    if (UpdateSqlQuery($"update users set location={r.num} where id='{ID}'") == true) 
                    {
                    }
                    else
                    {
                        Console.WriteLine(NOW() + " 방 접속 DB 업데이트 확인");
                    }

                    //User server에 상태 전송
                    string sendforserver = $"{2},{ID},{NickName},RoomIn";
                    mutexForServer.WaitOne();
                    //아이디 및 상태 바이트 사이즈 전송 및 아이디 및 상태 전송
                    SendBytesSize(servSock, sendforserver, ID);
                    SendBytesString(servSock, sendforserver, ID);
                    mutexForServer.ReleaseMutex();
                }
                //비밀번호 있음
                else
                {
                    Console.WriteLine(NOW() + " 비밀번호 있음!!!!!!!!!!!!!!!!!!");
                    //사인 5보내기
                    SendToSign(clientSock, 5, ID);
                    //비밀번호 사이즈 받기
                    int size = ReceieveBytesToInt(clientSock, bytes); 
                    Console.WriteLine($"비밀번호 사이즈 받음 : {size}");
                    byte[] passwordByte = new byte[size];
                    string Client_Password = ReceieveBytesToString(clientSock, passwordByte);
                    Console.WriteLine($"받은 비밀번호 : {Client_Password}");
                    //비밀번호 맞을 경우 접속
                    if (Client_Password == r.password) 
                    {
                        //사인 1보내기
                        SendToSign(clientSock, 1, ID);

                        User user = new User();
                        user.ID = ID; //닉네임 저장
                        user.Nick = NickName; //닉네임 저장

                        //ID, 닉네임, 소켓 저장
                        r.RoomMutex.WaitOne(); 
                        r.RoomUsers.Add(user);
                        //접속이 가능하면 사이즈 7을 보내 리스트 초기화
                        foreach (User u in r.RoomUsers) 
                        {
                            //사인 7 보내기
                            SendToSign(u.UserSocket, 7, ID);
                            //user 리스트에 있는 총 인원 보내기
                            SendToSign(u.UserSocket, r.RoomUsers.Count(), ID);
                            //리스트 개수 만큼 리스트 정보 전송
                            foreach (User users in r.RoomUsers) 
                            {
                                //string 사이즈 보내기
                                SendBytesSize(u.UserSocket, users.ToString(), ID);
                                //string 보내기
                                SendBytesString(u.UserSocket, users.ToString(), ID);
                            }
                        }
                        // 뮤텍스 릴리즈
                        r.RoomMutex.ReleaseMutex();
                        
                        //DB 업데이트
                        if (UpdateSqlQuery($"update users set location={r.num} where id='{ID}'") == true) 
                        {
                        }
                        else
                        {
                            Console.WriteLine("");
                        }

                        //User server에 상태 전송
                        string sendforserver = $"{2},{ID},{NickName},RoomIn";
                        mutexForServer.WaitOne();
                        //아이디 및 상태 바이트 사이즈 전송 및 아이디 및 상태 전송
                        SendBytesSize(servSock, sendforserver, ID);
                        SendBytesString(servSock, sendforserver, ID);
                        mutexForServer.ReleaseMutex();
                    }
                    //비밀번호 틀렸을 경우 
                    else
                    {
                        //sign -1을 보내 실패 사인 보냄
                        SendToSign(clientSock, -1, ID); 
                    }
                }
            }
        }

        //방 나갔을 경우
        public void Room_Out(Socket clientSock, Room r, string ID, string NickName, byte[] bytres) 
        {
            //뮤텍스 실행 후 방 번호 안에 있는 리스트 ID 지우기
            r.RoomMutex.WaitOne(); 
            r.RoomUsers.Remove(r.RoomUsers.Find(x => x.ID == ID));
            //방에 있는 유저 카운트가 0이라면
            if (r.RoomUsers.Count() == 0) 
            {
                //DB 업데이트
                if (UpdateSqlQuery($"update users set location='로비' where id='{ID}'") == true) 
                {
                }
                else
                {
                    Console.WriteLine(NOW() + " 방 나감 DB 업데이트 실패");
                }
                //방 지우기
                room.Remove(r);
            }
            //방에 있는 유저 카운트가 1 이상일 경우
            else
            {
                foreach (User usercCount in r.RoomUsers)
                {
                    Console.WriteLine(NOW() + $" {ID} 접속해 있는 아이디 " + usercCount.ID);
                }

                //방 안에 있는 유저 카운트 수 만큼
                foreach (User u in r.RoomUsers) 
                {
                    //sign 7을 보내고 user 리스트에 있는 총 인원 보내기
                    SendToSign(u.UserSocket, 7, ID); 
                    SendToSign(u.UserSocket, r.RoomUsers.Count(), ID);
                   
                    //리스트 개수 만큼 리스트 정보 전송
                    foreach (User users in r.RoomUsers) 
                    {
                        //string 사이즈 보내기
                        SendBytesSize(u.UserSocket, users.ToString(), ID);
                        //string 보내기
                        SendBytesString(u.UserSocket, users.ToString(), ID); 
                    }
                }

                //DB 업데이트
                if (UpdateSqlQuery($"update users set location='로비' where id='{ID}'") == true)
                {
                }
                else
                {
                    Console.WriteLine(NOW() + $" {ID} 방 나감 DB 업데이트 실패");
                }
            }
            //Mutex Release
            r.RoomMutex.ReleaseMutex();

            //방 나가고 난 후 sign 2 번 보내서 리스트 초기화
            SendToSign(clientSock, 2, ID);
            SendToSign(clientSock, room.Count(), ID);
            foreach (Room List_Room in room)
            {
                // List<Room> 에 저장된 방의 size  및 string 보내기
                SendBytesSize(clientSock, List_Room.ToString(), ID);
                SendBytesString(clientSock, List_Room.ToString(), ID);
            }
        }

        public void ErrorRoomOut(Socket clientSock, Room r, string ID, string NickName, byte[] bytres)
        {
            //뮤텍스 실행 후 방 번호 안에 있는 리스트 ID 지우기
            r.RoomMutex.WaitOne();
            r.RoomUsers.Remove(r.RoomUsers.Find(x => x.ID == ID));
            //방에 있는 유저 카운트가 0이라면
            if (r.RoomUsers.Count() == 0)
            {
                //DB 업데이트
                if (UpdateSqlQuery($"update users set location='로비' where id='{ID}'") == true)
                {
                }
                else
                {
                    Console.WriteLine(NOW() + " 방 나감 DB 업데이트 실패");
                }
                //방 지우기
                room.Remove(r);
            }
            //방에 있는 유저 카운트가 1 이상일 경우
            else
            {
                foreach (User usercCount in r.RoomUsers)
                {
                    Console.WriteLine(NOW() + $" {ID} 접속해 있는 아이디 " + usercCount.ID);
                }

                //방 안에 있는 유저 카운트 수 만큼
                foreach (User u in r.RoomUsers)
                {
                    //sign 7을 보내고 user 리스트에 있는 총 인원 보내기
                    SendToSign(u.UserSocket, 7, ID);
                    SendToSign(u.UserSocket, r.RoomUsers.Count(), ID);

                    //리스트 개수 만큼 리스트 정보 전송
                    foreach (User users in r.RoomUsers)
                    {
                        //string 사이즈 보내기
                        SendBytesSize(u.UserSocket, users.ToString(), ID);
                        //string 보내기
                        SendBytesString(u.UserSocket, users.ToString(), ID);
                    }
                }

                //DB 업데이트
                if (UpdateSqlQuery($"update users set location='로비' where id='{ID}'") == true)
                {
                }
                else
                {
                    Console.WriteLine(NOW() + $" {ID} 방 나감 DB 업데이트 실패");
                }
            }
            //Mutex Release
            r.RoomMutex.ReleaseMutex();
        }

        void ConnectWithClient(Socket clientSock)
        {
            byte[] signData = new byte[4];
            //아이디 , 닉네임, 방 번호, sign 받을 변수 선언
            string ID = string.Empty;
            string NickName = string.Empty;
            int RoomNum = new int();
            int sign; 



            // 아이디 사이즈 받은 후 아이디 받기
            byte[] IDBytes = new byte[ReceieveBytesToInt(clientSock, signData)]; 
            ID = ReceieveBytesToString(clientSock, IDBytes);
            Console.WriteLine(NOW() + " 받은 아이디 : " + ID);

            //DB 업데이트
            if (UpdateSqlQuery($"update users set location='로비' where id='{ID}'") == true) 
            {
            }
            else
            {
                Console.WriteLine(NOW() + " 첫 connect DB 업데이트 확인 실패");
            }

            while (true)
            {
                try
                {
                    //클라이언트에서 받는 sign 번호
                    sign = ReceieveBytesToInt(clientSock, signData); 
                    Console.WriteLine(NOW() + $" {ID} 에서 받은 sign 값 : " + sign.ToString());
                    switch (sign)
                    {
                        //방 만들기
                        case 1: 
                            {
                                string temp;
                                //클라이언트가 만드는 방 string의 size와 string 받기
                                byte[] bytes = new byte[ReceieveBytesToInt(clientSock, signData)];
                                temp = ReceieveBytesToString(clientSock, bytes); 
                                Console.WriteLine(NOW() + $" {ID} 에서 받은 방 " + temp);

                                string[] Arr_save = new string[10];
                                Arr_save = temp.Split(",".ToArray());
                       
                                Room r = new Room(roomCnt.ToString(), Arr_save[0], Arr_save[1], 0);
                                Console.WriteLine(NOW() + $" {ID} 에서 받은 비밀번호 확인 " + Arr_save[1]);
                                
                                //mutex 시작 후 List<Room>의 room에 저장
                                mutex.WaitOne(); 
                                room.Add(r);
                                roomCnt++;
                                //mutex release
                                mutex.ReleaseMutex();

                                //sign 6 보내기 및 방 번호 string  보내기
                                SendToSign(clientSock, 6, ID); 
                                SendToSign(clientSock, int.Parse(r.num), ID); 
                                Console.WriteLine(NOW() + $" {ID}에서 보낸 번호 : {r.num}");

                                //foreach (Room n in room)
                                //{
                                //    Console.WriteLine(NOW() + " 생성된 룸 확인 :" + n);
                                //}
                                break;
                            }
                        //방 새로고침
                        case 2:
                            {
                                //sign 2 보내기 및 방 개수 전송
                                SendToSign(clientSock, 2, ID); 
                                SendToSign(clientSock, room.Count(), ID); 
                                Console.WriteLine("룸 카운트 갯수 : " + room.Count);

                                foreach (Room List_Room in room)
                                {
                                    // List<Room> 에 저장된 방의 size  및 string 보내기
                                    SendBytesSize(clientSock, List_Room.ToString(), ID); 
                                    SendBytesString(clientSock, List_Room.ToString(), ID); 
                                }
                                break;
                            }
                        //방 접속
                        case 3: 
                            {
                                try
                                { 
                                    byte[] bytes = new byte[4];
                                    //방 번호 받기 및 저장
                                    RoomNum = ReceieveBytesToInt(clientSock, bytes); 
                                    Console.WriteLine(NOW() + $"{ID}가 받은 방 번호 : {RoomNum}");

                                    //방 번호 찾기
                                    Room r = room.Find(x => x.num == RoomNum.ToString()); 
                                    AddRoomAndUsers(clientSock, r, ID, NickName, bytes);
                                }
                                // 방접속 실패시
                                catch (Exception e) 
                                {
                                    byte[] bytes = new byte[4];
                                    //방 번호 찾기
                                    Room r = room.Find(x => x.num == RoomNum.ToString()); 
                                    
                                    //방 입장 실패시 룸 지우기
                                    ErrorRoomOut(clientSock, r, ID, NickName, bytes); 

                                    Console.WriteLine($"ID 확인 {ID}");
                                    //상태 0으로 초기화
                                    if (UpdateSqlQuery($"update users set state=0 , lastout=NOW(), location=null where id='{ID}'") == true)  
                                    {
                                        Console.WriteLine(NOW() + $" {ID} DB OFFLine 작업 성공 ");
                                    }
                                    else
                                    {
                                        Console.WriteLine(NOW() + " DB OFFLine 작업 성공 실패");
                                    }
                                    Console.WriteLine(NOW() + $"{ID} 방 입장 실패 : " + e);
                                    return;
                                }
                                break;
                            }
                        //상태
                        case 4: 
                            {

                                byte[] bytes = new byte[4];
                                //클라에서 받을 아이디 및 상태 사이즈
                                int size = ReceieveBytesToInt(clientSock, bytes);
                                Console.WriteLine($" {ID} 받은 아이디 및 상태 size  : {size}");

                                //클라에서 받은 아이디 및 상태 바이트
                                bytes = new byte[size];
                                //클라에서 받은 아이디 및 상태
                                string str_tempState = ReceieveBytesToString(clientSock, bytes);
                                string sendforserver = "1," + str_tempState;
                                string[] ary_tempstate = str_tempState.Split(",".ToArray());
                                Console.WriteLine(NOW() + $" {ID} 클라에서 받은 아이디 및 상태 :" + str_tempState);

                                ID = ary_tempstate[0];
                                NickName = ary_tempstate[1];
                                int state = Convert.ToInt32(ary_tempstate[2]);
                                Console.WriteLine(NOW() + $" {ID} 닉네임:" + NickName);

                                if (UpdateSqlQuery($"update users set state={state} where id='{ary_tempstate[0]}'") == true)
                                {
                                    //User server에 상태 전송
                                    mutexForServer.WaitOne();
                                    //아이디 및 상태 바이트 사이즈 전송 및 아이디 및 상태 전송
                                    SendBytesSize(servSock, sendforserver, ID); 
                                    SendBytesString(servSock, sendforserver, ID);
                                    mutexForServer.ReleaseMutex();
                                }
                                else
                                {
                                    Console.WriteLine(NOW() + " 상태 변환 DB UPDATE 실패");
                                }
                                break;
                            }
                        //방 나갈때
                        case 5: 
                            {
                                try
                                {
                                    byte[] bytes = new byte[4];
                                    Room r = room.Find(x => x.num == RoomNum.ToString()); //방 번호 찾기

                                    Room_Out(clientSock, r, ID, NickName, bytes); //방 나갔을 경우

                                    //User server에 상태 전송
                                    string sendforserver = $"{2},{ID},{NickName},RoomOut";
                                    mutexForServer.WaitOne();
                                    //아이디 및 상태 바이트 사이즈 전송 및 아이디 및 상태 전송
                                    SendBytesSize(servSock, sendforserver, ID);
                                    SendBytesString(servSock, sendforserver, ID);
                                    mutexForServer.ReleaseMutex();

                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(NOW() + $"{ID} 방 나감 실패 : " + e);
                                    byte[] bytes = new byte[4];
                                    Room r = room.Find(x => x.num == RoomNum.ToString());
                                    //방 나갔을 경우
                                    Room_Out(clientSock, r, ID, NickName, bytes); 
                                }

                                break;
                            }
                        default: //접속종료
                            {
                                byte[] bytes = new byte[4];
                                //방 번호 찾기
                                Room r = room.Find(x => x.num == RoomNum.ToString());
                                //방 나갔을 경우
                                Room_Out(clientSock, r, ID, NickName, bytes); 

                                CloseClientQuery(ID); //종료할 시 쿼리문
                                clientSock.Close();
                                Console.WriteLine($" {ID} 접속 종료");
                                //DB 업데이트
                                if (UpdateSqlQuery($"update users set state={0}, location=null where id='{ID}'") == true) 
                                {
                                    Console.WriteLine(NOW() + $" {ID} 접속 종료시 DB OFFLine 작업 성공 ");
                                }
                                else
                                {
                                    Console.WriteLine(NOW() + $" {ID} 오프라인시 DB location null값 업데이트 실패");
                                }
                                return;
                            }
                    }
                    sign = -1;
                }
                //오류 있을 시 방 리스트 삭제
                catch (Exception ex)
                {
                    byte[] bytes = new byte[4];
                    try
                    {
                        Room r = room.Find(x => x.num == RoomNum.ToString());
                        ErrorRoomOut(clientSock, r, ID, NickName, bytes);
                    }
                    catch
                    {
                        Console.WriteLine(NOW() + $"{ID} 아무런 방 없음");
                    }
                    if (UpdateSqlQuery($"update users set state={0} , lastout=NOW(), location=null where id='{ID}'") == true) //
                    {
                        Console.WriteLine(NOW() + $" {ID} Catch 부분 DB OFFLine 작업 성공 ");
                    }
                    else
                    {
                        Console.WriteLine(NOW() + $" {ID} DB OFFLine 작업 성공 실패");
                    }
                    Console.WriteLine(NOW() + $" {ID} 연결 끊김 /n{ex.Message}/n{ex.StackTrace}");
                    return;
                }
            }
        }
        void ThreadServer()
        {
            while (true)
            {

            }
        }

        bool UpdateSqlQuery(string query)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(DB_CONN))
                {
                    try
                    {
                        conn.Open();
                        if (conn.Ping() == false)
                        {
                            Console.WriteLine(NOW() + $" DB 연결 에러");
                            return false;
                        }
                        MySqlCommand comm = new MySqlCommand(query, conn);
                        if (comm.ExecuteNonQuery() > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        void CloseClientQuery(string ID)
        {
            using (MySqlConnection conn = new MySqlConnection(DB_CONN))
            {
                try
                {
                    conn.Open();
                    if (conn.Ping() == false)
                    {
                        Console.WriteLine($"[{NOW()}]유저 종료 DB 연결 에러");
                        return;
                    }
                    string query = Update_OutQuery(ID);
                    MySqlCommand comm = new MySqlCommand(query, conn);

                    int result = comm.ExecuteNonQuery();
                    if (result < 0)
                        Console.WriteLine($"[{NOW()}]유저 종료 DB 실행 에러");
                    else
                        Console.WriteLine($"[{NOW()}]유저 종료 DB 업데이트");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{NOW()}]유저 종료 DB 에러 {ex.Message}");
                }
            }
        }
        string Update_OutQuery(string ID)
        {
            return $"{UP_OUTDATE}{ID}'";
        }
    }
    public class Room
    {
        public string num { get; set; }
        public string name { get; set; }
        public string password { get; set; }

        public Mutex RoomMutex = new Mutex();

        public List<User> RoomUsers = new List<User>();
        public Room(string _num, string _name, string _password, int _playercnt)
        {
            num = _num;
            name = _name;
            password = _password;
        }
        public override string ToString()
        {
            return $"{num},{name},{RoomUsers.Count()}/{Program.MAX_Count},{password}";
        }
    }
    public class User
    {
        public string ID { get; set; }
        public string Nick { get; set; }
        public Socket UserSocket { get; set; }
        public override string ToString()
        {
            return ID + "," + Nick;
        }
    }
    public class Start
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            // 로그인 서버 연결
            program.ConnectWithServers();
            //클라이언트 서버 시작
            program.ClientServerStart(); 
        }
    }
}
