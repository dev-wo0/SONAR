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

namespace dbProject_client
{
    public partial class Form1 : Form
    {
        Form3 frm3;
        public TcpClient clientSocket = new TcpClient();
        string[] return_flag;
        public string MyName;
        bool isMove = false;
        Point fPt;
        public Form1()
        {
            InitializeComponent();
            this.Text = "SONAR";
            new Thread(delegate ()

            {
                InitSocket();
            }).Start();
        }
        private void InitSocket()
        {
            try
            {
                clientSocket.Connect("223.195.109.34", 9000);
                labelStatus.Text = "Client Program - Server Connected ...";
            }

            catch (SocketException se)
            {
                MessageBox.Show(se.Message, "Error");
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }
        
        public void DisplayText(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new MethodInvoker(delegate
                {
                    richTextBox1.AppendText(Environment.NewLine + " >> " + text);
                }));
            }
            else
                richTextBox1.AppendText(Environment.NewLine + " >> " + text);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (clientSocket != null)
                clientSocket.Close();
        }

        private void login_Box_Click(object sender, EventArgs e)    //로그인 버튼
        {
            NetworkStream stream = clientSocket.GetStream();
            byte[] sbuffer = Encoding.Unicode.GetBytes("login:" + id_textbox.Text + ":" + pw_textbox.Text + ":$");
            stream.Write(sbuffer, 0, sbuffer.Length);
            stream.Flush();

            byte[] rbuffer = new byte[1024];
            stream.Read(rbuffer, 0, rbuffer.Length);
            string msg = Encoding.Unicode.GetString(rbuffer);
            return_flag = msg.Split(':');
            ///////////////////////////////////
            ///return_flag[0] = return string
            ///return_flag[1] = result Name
            ///////////////////////////////////
            if (return_flag[0] == "Success")
            {
                MyName = return_flag[1].ToString();
                MessageBox.Show(MyName + "님 로그인 되었습니다", "확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Form2 frm2 = new Form2(this);
                frm2.Show();
                this.Hide();
            }
            else if (return_flag[0] == "Incorrect")
            {
                MessageBox.Show("비밀번호 불일치");
            }
            else if(return_flag[0]=="NoID")
            {
                MessageBox.Show("일치 계정 없음");
            }
            else
            {
                MessageBox.Show("ID / PW를 입력하세요.");
            }

            msg = "Data from Server : " + msg;

            DisplayText(msg);
        }

        private void join_Box_Click(object sender, EventArgs e)
        {
            frm3 = new Form3(this);
            frm3.ShowDialog();
        }

        private void pw_textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Enter)
            {
                login_Box_Click(sender, e);
            }
        }

        private void id_textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                pw_textbox.Focus();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            isMove = true;

            fPt = new Point(e.X, e.Y);
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMove && (e.Button & MouseButtons.Left) == MouseButtons.Left)
                Location = new Point(this.Left - (fPt.X - e.X), this.Top - (fPt.Y - e.Y));
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            isMove = false;
        }
    }
}
