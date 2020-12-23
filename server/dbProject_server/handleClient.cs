using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace dbProject_server
{
    class handleClient
    {
        TcpClient clientSocket;
        int clientNo;
        string[] test;
        MySqlConnection MConn;
        public string strSql = "Data Source=localhost;Database=rtemd;User Id=root;Password=root";

        public string login_time;
        /// <summary>
        /// 로그인 부울값
        /// </summary>
        /// <param login_chk="로그인 리턴값  Success = 성공 / Incorrect = PW불일치 / NoID = 계정 없음 / Enter = id/pw미입력"></param>
        /// <param return_name="DB안의 이름값"></param>
        string login_chk;
        string return_name = "False";

        /// <summary>
        /// ID중복확인 부울값
        /// </summary>
        /// <param id_chk="ID중복확인 리턴값    Already = 이미 있는 아이디 / Ok = 가능 아이디 / Notover5 = 5글자 이하입력"></param>
        string id_chk;

        /// <summary>
        /// 회원가입 부울값
        /// </summary>
        /// <param join_chk="회원가입 리턴값    Ok = 회원가입 완료 / NotEqual = 비밀번호 미일치 / NullName = 이름 입력 안함 / NullPW = 비밀번호 미입력"></param>
        string join_chk;

        public void startClient(TcpClient ClientSocket, int clientNo)
        {
            this.clientSocket = ClientSocket;
            this.clientNo = clientNo;

            Thread t_hanlder = new Thread(doChat);
            t_hanlder.IsBackground = true;
            t_hanlder.Start();
        }

        public delegate void MessageDisplayHandler(string text);
        public event MessageDisplayHandler OnReceived;

        public delegate void CalculateClientCounter();
        public event CalculateClientCounter OnCalculated;

        public delegate void ListViewOnline(string name);
        public event ListViewOnline OnLoginListViewed;

        public delegate void ListViewOffline(string name);
        public event ListViewOffline OnLogoutListViewed;

        private void doChat()
        {
            NetworkStream stream = null;
            try
            {
                byte[] buffer = new byte[1024];
                string msg = string.Empty;
                int bytes = 0;
                int MessageCount = 0;

                while (true)
                {
                    MConn = new MySqlConnection(strSql);
                    MConn.Open();
                    MessageCount++;
                    stream = clientSocket.GetStream();
                    bytes = stream.Read(buffer, 0, buffer.Length);
                    msg = Encoding.Unicode.GetString(buffer, 0, bytes);
                    msg = msg.Substring(0, msg.IndexOf("$"));

                    test = msg.Split(':');
                    
                    
                    /////////////////////////
                    /// Client - login버튼을 눌렀을때
                    /// test[0] = "login"
                    /// test[1] = "ID"
                    /// test[2] = "PW"
                    /////////////////////////
                    if(test[0].ToString()=="login")
                    {
                        string sql = "select user_id, user_password, user_name from user_info where user_ID= '" + test[1] + "'";
                        var Comm = new MySqlCommand(sql, MConn);
                        var myRead = Comm.ExecuteReader();

                        if (test[1].ToString() != "" && test[2].ToString() != "")
                        {
                            if (myRead.HasRows)
                            {
                                if (myRead.Read())
                                {
                                    if (myRead["user_Password"].ToString() == test[2])
                                    {   //성공
                                        login_chk = "Success";
                                        return_name = myRead["user_name"].ToString();
                                        myRead.Close();
                                        login_time = DateTime.Now.ToString();
                                        string sql2 = "insert into user_log(Name, login) values('" + return_name + "', '" + login_time + "')";
                                        var Comm2 = new MySqlCommand(sql2, MConn);
                                        Comm2.ExecuteNonQuery();

                                        if (OnReceived != null)
                                        {
                                            OnReceived(test[0] + ":" + return_name);
                                            OnLoginListViewed(return_name);
                                        }
                                    }
                                    else
                                    {   //비밀번호 불일치
                                        login_chk = "Incorrect";
                                    }
                                }
                            }
                            else
                            {   //일치 계정 없음
                                login_chk = "NoID";
                            }
                        }
                        else
                        {
                            login_chk = "Enter";
                        }
                        
                    }
                    /////////////////////////
                    /// Client - ID_check버튼을 눌렀을때
                    /// test[0] = "idCheck"
                    /// test[1] = "ID"
                    /////////////////////////
                    else if (test[0].ToString()=="idCheck")
                    {
                        if (test[1].ToString() != "" && test[1].Length > 4)    //나중에 영어 / 숫자만 들어갈수있게
                        {
                            string sql = "select user_Id from user_info where user_id= '" + test[1] + "'";
                            var Comm = new MySqlCommand(sql, MConn);
                            var myRead = Comm.ExecuteReader();
                            if (myRead.HasRows)
                            {
                                id_chk = "Already";    //아이디 중복
                            }
                            else
                            {
                                id_chk = "Ok";     //아이디 가능
                            }
                        }
                        else
                        {
                            id_chk = "Notover5";    //아이디 5자리 이하 입력
                        }
                    }
                    /////////////////////////
                    /// Client - 회원가입버튼을 눌렀을때
                    /// test[0] = "join"
                    /// test[1] = "Name"
                    /// test[2] = "Id"
                    /// test[3] = "Pw"
                    /// test[4] = "Pw확인"
                    /////////////////////////
                    else if (test[0].ToString() == "join")
                    {
                        if (test[1].ToString() != "" && (test[3].ToString() == test[4].ToString()) && test[3].ToString() !="")  // 이름 공백, 비밀번호 일치
                        {
                            string sql = "insert into user_info(user_id, user_password, user_name) values('" + test[2].ToString() + "', '" + test[3].ToString() + "', '" + test[1].ToString() + "')";
                            var Comm = new MySqlCommand(sql, MConn);
                            int i = Comm.ExecuteNonQuery();
                            if (i == 1)
                            {   //회원가입 완료
                                join_chk = "Ok";
                            }
                        }

                        else if (test[1].ToString() == "")
                        {   //이름 미입력
                            join_chk = "NullName";
                        }

                        else if (test[3].ToString() == "")
                        {   //비밀번호 미입력
                            join_chk = "NullPW";
                        }
                        else
                        {   //비밀번호 미일치
                            join_chk = "NotEqual";
                        }
                    }
                    /////////////////////////
                    /// Client - 로그아웃할때
                    /// test[0] = "logout"
                    /// test[1] = "Name"
                    /////////////////////////
                    else if (test[0].ToString() == "logout")
                    {
                        string sql3 = "update user_log set logout='" + DateTime.Now + "' where name='" + test[1].ToString() + "' AND login='" + login_time + "'";
                        var Comm3 = new MySqlCommand(sql3, MConn);
                        Comm3.ExecuteNonQuery();
                        if (OnReceived != null)
                        {
                            OnReceived(test[0] + ":" + test[1]);
                            OnLogoutListViewed(test[1]);
                        }
                    }

                    if (test[0] == "login")    //로그인일때
                        msg = login_chk + ":" + return_name.ToString() + ":";

                    else if (test[0] == "idCheck")    //ID Check일때
                        msg = id_chk + ":";

                    else if (test[0] == "join")
                        msg = join_chk + ":";

                    byte[] sbuffer = Encoding.Unicode.GetBytes(msg);
                    stream.Write(sbuffer, 0, sbuffer.Length);
                    stream.Flush();
                    
                    MConn.Close();
                }
            }
            catch (SocketException se)
            {
                Trace.WriteLine(string.Format("doChat - SocketException : {0}", se.Message));

                if (clientSocket != null)
                {
                    clientSocket.Close();
                    stream.Close();
                }

                if (OnCalculated != null)
                    OnCalculated();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("doChat - Exception : {0}", ex.Message));

                if (clientSocket != null)
                {
                    clientSocket.Close();
                    stream.Close();
                }

                if (OnCalculated != null)
                    OnCalculated();
            }
        }
    }
}