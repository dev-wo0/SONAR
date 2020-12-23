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

namespace dbProject_server
{
    class notice_handleClient
    {
        TcpClient notice_clientSocket = null;
        public Dictionary<TcpClient, string> notice_clientList = null;

        public void notice_startClient(TcpClient notice_clientSocket, Dictionary<TcpClient, string> notice_clientList)
        {
            this.notice_clientSocket = notice_clientSocket;
            this.notice_clientList = notice_clientList;

            Thread notice_t_hanlder = new Thread(notice_donotice);
            notice_t_hanlder.IsBackground = true;
            notice_t_hanlder.Start();
        }

        public delegate void MessageDisplayHandler(string notice_message, string notice_user_name);
        public event MessageDisplayHandler notice_OnReceived;

        public delegate void DisconnectedHandler(TcpClient notice_clientSocket);
        public event DisconnectedHandler notice_OnDisconnected;

        private void notice_donotice()
        {
            NetworkStream notice_stream = null;
            try
            {
                byte[] notice_buffer = new byte[1024];
                string notice_msg = string.Empty;
                int notice_bytes = 0;
                int notice_MessageCount = 0;

                while (true)
                {
                    notice_MessageCount++;
                    notice_stream = notice_clientSocket.GetStream();
                    notice_bytes = notice_stream.Read(notice_buffer, 0, notice_buffer.Length);
                    notice_msg = Encoding.Unicode.GetString(notice_buffer, 0, notice_bytes);
                    notice_msg = notice_msg.Substring(0, notice_msg.IndexOf("$"));

                    if (notice_OnReceived != null)
                        notice_OnReceived(notice_msg, notice_clientList[notice_clientSocket].ToString());
                }
            }
            catch (SocketException se)
            {
                Trace.WriteLine(string.Format("donotice - SocketException : {0}", se.Message));

                if (notice_clientSocket != null)
                {
                    if (notice_OnDisconnected != null)
                        notice_OnDisconnected(notice_clientSocket);

                    notice_clientSocket.Close();
                    notice_stream.Close();
                }
            }

            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("donotice - Exception : {0}", ex.Message));

                if (notice_clientSocket != null)
                {
                    if (notice_OnDisconnected != null)
                        notice_OnDisconnected(notice_clientSocket);

                    notice_clientSocket.Close();
                    notice_stream.Close();
                }
            }
        }
    }
}
