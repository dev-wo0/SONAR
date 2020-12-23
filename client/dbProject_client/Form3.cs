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
    public partial class Form3 : Form
    {
        Form1 frm1;
        string[] return_flag;
        public Boolean idCheckd = false;

        bool isMove = false;
        Point fPt;

        public Form3(Form1 a)
        {
            InitializeComponent();
            frm1 = a;
        }

        private void id_check_but_Click(object sender, EventArgs e)     //중복확인 버튼
        {
            NetworkStream stream = frm1.clientSocket.GetStream();
            byte[] sbuffer = Encoding.Unicode.GetBytes("idCheck:" + id_textbox.Text + ":$");
            stream.Write(sbuffer, 0, sbuffer.Length);
            stream.Flush();

            byte[] rbuffer = new byte[1024];
            stream.Read(rbuffer, 0, rbuffer.Length);
            string msg = Encoding.Unicode.GetString(rbuffer);
            return_flag = msg.Split(':');
            /////////////////////////
            /// ID_check버튼을 눌렀을때 return값
            /// return_flag[0] = "ID중복확인 리턴값"
            /////////////////////////
            if (return_flag[0].ToString() == "Already")
            {
                idCheckd = false;
                label4.ForeColor = Color.Red;
                label4.Text = "아이디 중복";
            }
            else if (return_flag[0].ToString() == "Ok")
            {
                idCheckd = true;
                label4.ForeColor = Color.Blue;
                label4.Text = "OK 확인";
            }
            else if (return_flag[0].ToString() == "Notover5")
            {
                idCheckd = false;
                MessageBox.Show("아이디를 5자리 이상 입력하세요", "아이디 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                id_textbox.Focus();
            }

            msg = "Data from Server : " + msg;

            frm1.DisplayText(msg);
        }

        private void join_but_Click(object sender, EventArgs e)     //회원가입 버튼
        {
            NetworkStream stream = frm1.clientSocket.GetStream();
            byte[] sbuffer = Encoding.Unicode.GetBytes("join:" + name_textbox.Text + ":" + id_textbox.Text + ":" + pass_text.Text + ":" + passcheck_text.Text + ":$");
            stream.Write(sbuffer, 0, sbuffer.Length);
            stream.Flush();

            byte[] rbuffer = new byte[1024];
            stream.Read(rbuffer, 0, rbuffer.Length);
            string msg = Encoding.Unicode.GetString(rbuffer);
            return_flag = msg.Split(':');
            /////////////////////////
            /// 가입버튼을 눌렀을때 return값
            /// return_flag[0] = "ID중복확인 리턴값"
            /////////////////////////
            if (idCheckd)    //ID중복확인 OK
            {
                if (return_flag[0].ToString() == "Ok")
                {
                    MessageBox.Show("회원가입 완료.", "회원가입 완료", MessageBoxButtons.OK);
                    Close();
                }
                else if (return_flag[0].ToString() == "NullName")
                {
                    MessageBox.Show("이름을 적어주세요.", "이름확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    name_textbox.Focus();
                }
                else if (return_flag[0].ToString() == "NullPW")
                {
                    MessageBox.Show("비밀번호를 입력하세요..", "비밀번호 미입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    pass_text.Focus();
                }
                else if (return_flag[0].ToString() == "NotEqual")
                {
                    MessageBox.Show("비밀번호가 일치하지 않습니다.", "비밀번호 불일치", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    pass_text.Focus();
                }
            }
            else
            {
                MessageBox.Show("ID중복확인버튼을 눌러주세요.", "ID중복확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            msg = "Data from Server : " + msg;

            frm1.DisplayText(msg);
        }

        private void Form3_Shown(object sender, EventArgs e)
        {
            name_textbox.Focus();
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

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}