using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Collections;

namespace dbProject_server
{
    public partial class Form1 : Form
    {
        TcpListener server = null;
        TcpClient client = null;
        static int counter = 0;
        bool Connckd = false;

        TcpListener chat_server = null;
        TcpClient chat_clientSocket = null;
        static int chat_counter = 0;
        public Dictionary<TcpClient, string> chat_clientList = new Dictionary<TcpClient, string>();

        TcpListener notice_server = null;
        TcpClient notice_clientSocket = null;
        static int notice_counter = 0;
        public Dictionary<TcpClient, string> notice_clientList = new Dictionary<TcpClient, string>();

        MySqlConnection Notice_MConn;
        public string notice_Sql = "Data Source=localhost;Database=rtemd;User Id=root;Password=root";

        bool isMove = false;
        Point fPt;

        MySqlConnection chart_login_MConn;
        public string chart_login_Sql = "Data Source=localhost;Database=rtemd;User Id=root;Password=root";
        public ArrayList login_result = new ArrayList();

        MySqlConnection chart_logout_MConn;
        public string chart_logout_Sql = "Data Source=localhost;Database=rtemd;User Id=root;Password=root";
        public ArrayList logout_result = new ArrayList();

        public string list_name;
        public Form1()
        {
            InitializeComponent();
            listView1.SmallImageList = imageList1;
        }

        private void InitSocket()
        {
            server = new TcpListener(IPAddress.Any, 9000);
            client = default(TcpClient);
            server.Start();
            DisplayText(">> Server Started");

            while (true)
            {
                try
                {
                    counter++;
                    client = server.AcceptTcpClient();

                    handleClient h_client = new handleClient();
                    h_client.OnReceived += new handleClient.MessageDisplayHandler(DisplayText);
                    h_client.OnCalculated += new handleClient.CalculateClientCounter(CalculateCounter);
                    h_client.OnLoginListViewed += new handleClient.ListViewOnline(Login_Display);
                    h_client.OnLogoutListViewed += new handleClient.ListViewOffline(Logout_Display);
                    h_client.startClient(client, counter);
                }
                catch (SocketException se)
                {
                    Trace.WriteLine(string.Format("InitSocket - SocketException : {0}", se.Message));
                }

                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("InitSocket - Exception : {0}", ex.Message));
                }
            }
        }
        private void CalculateCounter()
        {
            counter--;
        }

        private void DisplayText(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new MethodInvoker(delegate
                {
                    richTextBox1.AppendText(text + Environment.NewLine);
                }));
            }

            else
                richTextBox1.AppendText(text + Environment.NewLine);
        }

        private void Login_Display(string name)
        {
            if(listView1.InvokeRequired)
            {
                if (listView1.FindItemWithText(name) == null)
                {
                    listView1.BeginInvoke(new MethodInvoker(delegate
                    {
                        ListViewItem itm = new ListViewItem(name);
                        itm.SubItems.Add("On");
                        itm.ImageIndex = 0;

                        listView1.Items.Add(itm);
                    }));
                }
                else
                {
                    listView1.BeginInvoke(new MethodInvoker(delegate
                    {
                        ListViewItem item = listView1.FindItemWithText(name);
                        ListViewItem.ListViewSubItem item2 = new ListViewItem.ListViewSubItem(item, "On");
                        item.SubItems.RemoveAt(1);
                        item.SubItems.Insert(1, item2);
                        item.ImageIndex = 0;
                    }));
                }
            }
            else
            {
                if (listView1.FindItemWithText(name) == null)
                {
                    listView1.BeginInvoke(new MethodInvoker(delegate
                    {
                        ListViewItem itm = new ListViewItem(name);
                        itm.SubItems.Add("On");
                        itm.ImageIndex = 0;

                        listView1.Items.Add(itm);
                    }));
                }
                else
                {
                    listView1.BeginInvoke(new MethodInvoker(delegate
                    {
                        ListViewItem item = listView1.FindItemWithText(name);
                        ListViewItem.ListViewSubItem item2 = new ListViewItem.ListViewSubItem(item, "On");
                        item.SubItems.RemoveAt(1);
                        item.SubItems.Insert(1, item2);
                        item.ImageIndex = 0;
                    }));
                }
            }
        }

        private void Logout_Display(string name)
        {
            if (listView1.InvokeRequired)
            {
                listView1.BeginInvoke(new MethodInvoker(delegate
                {
                    ListViewItem item = listView1.FindItemWithText(name);
                    ListViewItem.ListViewSubItem item2 = new ListViewItem.ListViewSubItem(item, "Off");
                    item.SubItems.RemoveAt(1);
                    item.ImageIndex = 1;
                    item.SubItems.Insert(1, item2);
                }));
            }
            else
            {
                listView1.BeginInvoke(new MethodInvoker(delegate
                {
                    ListViewItem item = listView1.FindItemWithText(name);
                    ListViewItem.ListViewSubItem item2 = new ListViewItem.ListViewSubItem(item, "Off");
                    item.SubItems.RemoveAt(1);
                    item.ImageIndex = 1;
                    item.SubItems.Insert(1, item2);
                }));
            }
        }

        private void serverSTART_btn_Click(object sender, EventArgs e)
        {
            if (Connckd == false)
            {
                Connckd = true;
                Thread t = new Thread(InitSocket);
                t.IsBackground = true;
                t.Start();

                Thread chat_t = new Thread(chat_InitSocket);
                chat_t.IsBackground = true;
                chat_t.Start();

                Thread notice_t = new Thread(notice_InitSocket);
                notice_t.IsBackground = true;
                notice_t.Start();

                label5.ForeColor = Color.Blue;
                label5.Text = "SERVER ON";
                
                Notice_MConn = new MySqlConnection(notice_Sql);
                Notice_MConn.Open();

                string temp;
                string[] split_temp = new string[] { "\\" };
                string[] result;
                string sql = "select max(notice_content) from notice";
                var Notice_Comm = new MySqlCommand(sql, Notice_MConn);
                var Notice_myRead = Notice_Comm.ExecuteReader();
                if(Notice_myRead.HasRows)
                {
                    if(Notice_myRead.Read())
                    {
                        temp = Notice_myRead["max(notice_content)"].ToString();
                        result = temp.Split(split_temp, StringSplitOptions.None);
                        foreach (string s in result)
                            richTextBox3.AppendText(s+"\n");
                    }
                }
                Notice_myRead.Close();
                Notice_MConn.Close();
            }
            else
            {
                MessageBox.Show("이미 서버가 켜져있습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// 채팅 ////
        private void chat_InitSocket()
        {
            chat_server = new TcpListener(IPAddress.Any, 8000);
            chat_clientSocket = default(TcpClient);
            chat_server.Start();
            chat_DisplayText(">> ChatServer Started");

            while (true)
            {
                try
                {
                    chat_counter++;
                    chat_clientSocket = chat_server.AcceptTcpClient();
                    chat_DisplayText(">> Accept connection from client");

                    NetworkStream chat_stream = chat_clientSocket.GetStream();
                    byte[] chat_buffer = new byte[1024];
                    int chat_bytes = chat_stream.Read(chat_buffer, 0, chat_buffer.Length);
                    string chat_user_name = Encoding.Unicode.GetString(chat_buffer, 0, chat_bytes);
                    chat_user_name = chat_user_name.Substring(0, chat_user_name.IndexOf("$"));
                    
                    chat_clientList.Add(chat_clientSocket, chat_user_name);


                    // send message all user

                    chat_SendMessageAll(chat_user_name + " Joined ", "", false);

                    chat_handleClient chat_h_client = new chat_handleClient();
                    chat_h_client.chat_OnReceived += new chat_handleClient.MessageDisplayHandler(chat_OnReceived);
                    chat_h_client.chat_OnDisconnected += new chat_handleClient.DisconnectedHandler(chat_h_client_OnDisconnected);
                    chat_h_client.chat_startClient(chat_clientSocket, chat_clientList);

                }

                catch (SocketException se)
                {
                    Trace.WriteLine(string.Format("InitSocket - SocketException : {0}", se.Message));
                    break;
                }

                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("InitSocket - Exception : {0}", ex.Message));
                    break;
                }
            }
        }

        private void notice_InitSocket()
        {
            notice_server = new TcpListener(IPAddress.Any, 7000);
            notice_clientSocket = default(TcpClient);
            notice_server.Start();

            while (true)
            {
                try
                {
                    notice_counter++;
                    notice_clientSocket = notice_server.AcceptTcpClient();

                    NetworkStream notice_stream = notice_clientSocket.GetStream();

                    byte[] notice_buffer = new byte[1024];
                    int notice_bytes = notice_stream.Read(notice_buffer, 0, notice_buffer.Length);
                    string notice_user_name = Encoding.Unicode.GetString(notice_buffer, 0, notice_bytes);
                    notice_user_name = notice_user_name.Substring(0, notice_user_name.IndexOf("$"));

                    notice_clientList.Add(notice_clientSocket, notice_user_name);


                    // send message all user

                    notice_SendMessageAll(richTextBox3.Text, "", false);

                    notice_handleClient notice_h_client = new notice_handleClient();
                    notice_h_client.notice_OnReceived += new notice_handleClient.MessageDisplayHandler(notice_OnReceived);
                    notice_h_client.notice_OnDisconnected += new notice_handleClient.DisconnectedHandler(notice_h_client_OnDisconnected);
                    notice_h_client.notice_startClient(notice_clientSocket, notice_clientList);

                }

                catch (SocketException se)
                {
                    Trace.WriteLine(string.Format("InitSocket - SocketException : {0}", se.Message));
                    break;
                }

                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("InitSocket - Exception : {0}", ex.Message));
                    break;
                }
            }
        }

        void chat_h_client_OnDisconnected(TcpClient clientSocket)
        {
            if (chat_clientList.ContainsKey(clientSocket))
                chat_clientList.Remove(clientSocket);
        }

        void notice_h_client_OnDisconnected(TcpClient clientSocket)
        {
            if (notice_clientList.ContainsKey(clientSocket))
                notice_clientList.Remove(clientSocket);
        }

        private void chat_OnReceived(string chat_message, string chat_user_name)
        {
            string chat_displayMessage = "From client : " + chat_user_name + " : " + chat_message;
            chat_DisplayText(chat_displayMessage);
            chat_SendMessageAll(chat_message, chat_user_name, true);
        }

        private void notice_OnReceived(string notice_message, string notice_user_name)
        {
            string notice_displayMessage = "From client : " + notice_user_name + " : " + notice_message;
            notice_DisplayText(notice_displayMessage);
            notice_SendMessageAll(notice_message, notice_user_name, true);
        }

        public void chat_SendMessageAll(string chat_message, string chat_user_name, bool chat_flag)
        {
            foreach (var chat_pair in chat_clientList)
            {
                Trace.WriteLine(string.Format("tcpclient : {0} user_name : {1}", chat_pair.Key, chat_pair.Value));

                TcpClient chat_client = chat_pair.Key as TcpClient;
                NetworkStream chat_stream = chat_client.GetStream();
                byte[] chat_buffer = null;

                if (chat_flag)
                {
                    chat_buffer = Encoding.Unicode.GetBytes(chat_user_name + " says : " + chat_message);
                }
                else
                {
                    chat_buffer = Encoding.Unicode.GetBytes(chat_message);
                }

                chat_stream.Write(chat_buffer, 0, chat_buffer.Length);
                chat_stream.Flush();
            }
        }

        public void notice_SendMessageAll(string notice_message, string notice_user_name, bool notice_flag)
        {
            foreach (var notice_pair in notice_clientList)
            {
                Trace.WriteLine(string.Format("tcpclient : {0} user_name : {1}", notice_pair.Key, notice_pair.Value));

                TcpClient notice_client = notice_pair.Key as TcpClient;
                NetworkStream notice_stream = notice_client.GetStream();
                byte[] notice_buffer = null;

                if (notice_flag)
                {
                    notice_buffer = Encoding.Unicode.GetBytes(notice_user_name + " says : " + notice_message);
                }
                else
                {
                    notice_buffer = Encoding.Unicode.GetBytes(notice_message);
                }

                notice_stream.Write(notice_buffer, 0, notice_buffer.Length);
                notice_stream.Flush();
            }
        }

        private void chat_DisplayText(string text)
        {
            if (richTextBox2.InvokeRequired)
            {
                richTextBox2.BeginInvoke(new MethodInvoker(delegate
                {
                    richTextBox2.AppendText(text + Environment.NewLine);
                }));
            }
            else
                richTextBox2.AppendText(text + Environment.NewLine);
        }

        private void notice_DisplayText(string text)
        {
            if (richTextBox3.InvokeRequired)
            {
                richTextBox3.BeginInvoke(new MethodInvoker(delegate
                {
                    richTextBox3.AppendText(text + Environment.NewLine);
                }));
            }
            else
                richTextBox3.AppendText(text + Environment.NewLine);
        }

        private void serverSTOP_btn_Click(object sender, EventArgs e)
        {
            if (Connckd == true)
            {
                Connckd = false;
                if (client != null)
                {
                    client.Close();
                    client = null;
                }

                if (server != null)
                {
                    server.Stop();
                    DisplayText(">> Server Stop");
                    server = null;
                }

                if (chat_clientSocket != null)
                {
                    chat_clientSocket.Close();
                    chat_clientSocket = null;
                }

                if (chat_server != null)
                {
                    chat_server.Stop();
                    chat_DisplayText(">> Server Stop");
                    chat_server = null;
                }

                if (notice_clientSocket != null)
                {
                    notice_clientSocket.Close();
                    notice_clientSocket = null;
                }

                if (notice_server != null)
                {
                    notice_server.Stop();
                    notice_server = null;
                    label5.ForeColor = Color.Red;
                    label5.Text = "SERVER OFF";
                }
                if (Notice_MConn != null)
                {
                    Notice_MConn.Close();
                }
            }
            listView1.Items.Clear();
            richTextBox3.Clear();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }

            if (server != null)
            {
                server.Stop();
                server = null;
            }
            if (chat_clientSocket != null)
            {
                chat_clientSocket.Close();
                chat_clientSocket = null;
            }

            if (chat_server != null)
            {
                chat_server.Stop();
                chat_server = null;
            }

            if (notice_clientSocket != null)
            {
                notice_clientSocket.Close();
                notice_clientSocket = null;
            }

            if (notice_server != null)
            {
                notice_server.Stop();
                notice_server = null;
            }
            if (Notice_MConn != null)
            {
                Notice_MConn.Close();
            }
        }

        private void notice_btn_Click(object sender, EventArgs e)
        {
            notice_SendMessageAll(richTextBox3.Text, "", false);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            isMove = true;

            fPt = new Point(e.X, e.Y);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMove && (e.Button & MouseButtons.Left) == MouseButtons.Left)
                Location = new Point(this.Left - (fPt.X - e.X), this.Top - (fPt.Y - e.Y));
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            isMove = false;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            
            if (listView1.SelectedItems.Count==1)
            {
                login_result.Clear();
                logout_result.Clear();
                list_name = listView1.FocusedItem.SubItems[0].Text;
                chart_login_MConn = new MySqlConnection(chart_login_Sql);
                chart_login_MConn.Open();
                string login_sql = "select login from user_log where Name= '" + list_name + "'";
                var chart_login_Comm = new MySqlCommand(login_sql, chart_login_MConn);
                var chart_login_myRead = chart_login_Comm.ExecuteReader();
                while(chart_login_myRead.Read())
                {
                    login_result.Add(chart_login_myRead["login"].ToString());
                }
                chart_login_myRead.Close();
                chart_login_MConn.Close();



                chart_logout_MConn = new MySqlConnection(chart_logout_Sql);
                chart_logout_MConn.Open();
                string logout_sql = "select logout from user_log where Name= '" + list_name + "'";
                var chart_logout_Comm = new MySqlCommand(logout_sql, chart_logout_MConn);
                var chart_logout_myRead = chart_logout_Comm.ExecuteReader();
                while (chart_logout_myRead.Read())
                {
                    logout_result.Add(chart_logout_myRead["logout"].ToString());
                }
                chart_logout_myRead.Close();
                chart_logout_MConn.Close();

                Statistic tistic = new Statistic(this);
                tistic.Show();
            }
        }
    }
}
