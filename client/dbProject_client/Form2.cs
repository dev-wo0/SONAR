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
    public partial class Form2 : Form
    {
        Form1 frm1 = new Form1();
        TcpClient chat_clientSocket = new TcpClient();
        NetworkStream chat_stream = default(NetworkStream);
        string chat_message = string.Empty;

        TcpClient notice_clientSocket = new TcpClient();
        NetworkStream notice_stream = default(NetworkStream);
        string notice_message = string.Empty;

        bool isMove = false;
        Point fPt;

        public Form2(Form1 frm1)
        {
            InitializeComponent();
            this.frm1 = frm1;

            richTextBox1.ReadOnly = true;
            chat_clientSocket.Connect("223.195.109.34", 8000);
            chat_stream = chat_clientSocket.GetStream();

            chat_message = "Connected to Chat Server";
            chat_DisplayText(chat_message);

            byte[] chat_buffer = Encoding.Unicode.GetBytes(frm1.MyName + "$");
            chat_stream.Write(chat_buffer, 0, chat_buffer.Length);
            chat_stream.Flush();

            Thread chat_t_handler = new Thread(chat_GetMessage);
            chat_t_handler.IsBackground = true;
            chat_t_handler.Start();

            notice_clientSocket.Connect("223.195.109.34", 7000);
            notice_stream = notice_clientSocket.GetStream();

            byte[] notice_buffer = Encoding.Unicode.GetBytes(frm1.MyName + "$");
            notice_stream.Write(notice_buffer, 0, notice_buffer.Length);
            notice_stream.Flush();

            Thread notice_t_handler = new Thread(notice_GetMessage);
            notice_t_handler.IsBackground = true;
            notice_t_handler.Start();

        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            NetworkStream stream = frm1.clientSocket.GetStream();
            byte[] sbuffer = Encoding.Unicode.GetBytes("logout:" + frm1.MyName + ":$");
            stream.Write(sbuffer, 0, sbuffer.Length);
            stream.Flush();

            byte[] rbuffer = new byte[1024];
            stream.Read(rbuffer, 0, rbuffer.Length);
            string msg = Encoding.Unicode.GetString(rbuffer);

            if (chat_clientSocket != null)
            {
                chat_clientSocket.Close();
                chat_clientSocket = null;
            }
            frm1.Close();
        }

        private void btnSendText_Click(object sender, EventArgs e)
        {
            byte[] chat_buffer = Encoding.Unicode.GetBytes(this.chat_textBoxMessage.Text + "$");
            chat_stream.Write(chat_buffer, 0, chat_buffer.Length);
            chat_stream.Flush();
            chat_textBoxMessage.Clear();
            chat_textBoxMessage.Focus();
        }

        private void chat_GetMessage()
        {
            while (true)
            {
                chat_stream = chat_clientSocket.GetStream();
                int chat_BUFFERSIZE = chat_clientSocket.ReceiveBufferSize;
                byte[] chat_buffer = new byte[chat_BUFFERSIZE];
                int chat_bytes = chat_stream.Read(chat_buffer, 0, chat_buffer.Length);

                string chat_message = Encoding.Unicode.GetString(chat_buffer, 0, chat_bytes);
                chat_DisplayText(chat_message);
            }
        }

        private void notice_GetMessage()
        {
            while (true)
            {
                notice_stream = notice_clientSocket.GetStream();
                int notice_BUFFERSIZE = notice_clientSocket.ReceiveBufferSize;
                byte[] notice_buffer = new byte[notice_BUFFERSIZE];
                int notice_bytes = notice_stream.Read(notice_buffer, 0, notice_buffer.Length);

                string notice_message = Encoding.Unicode.GetString(notice_buffer, 0, notice_bytes);
                notice_DisplayText(notice_message);
            }
        }

        private void chat_DisplayText(string chat_text)
        {
            if (richTextBox2.InvokeRequired)
            {
                richTextBox2.BeginInvoke(new MethodInvoker(delegate
                {
                    richTextBox2.AppendText(chat_text + Environment.NewLine);
                }));
            }
            else
                richTextBox2.AppendText(chat_text + Environment.NewLine);
        }

        private void notice_DisplayText(string notice_text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new MethodInvoker(delegate
                {
                    richTextBox1.Text = notice_text + Environment.NewLine;
                }));
            }
            else
                richTextBox1.Text = notice_text + Environment.NewLine;
        }

        private void chat_textBoxMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Enter)
            {
                btnSendText_Click(sender, e);
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
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
    }
}
