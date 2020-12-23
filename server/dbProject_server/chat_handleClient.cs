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
    class chat_handleClient
    {
        TcpClient chat_clientSocket = null;
        public Dictionary<TcpClient, string> chat_clientList = null;

        public void chat_startClient(TcpClient chat_clientSocket, Dictionary<TcpClient, string> chat_clientList)
        {
            this.chat_clientSocket = chat_clientSocket;
            this.chat_clientList = chat_clientList;

            Thread chat_t_hanlder = new Thread(chat_doChat);
            chat_t_hanlder.IsBackground = true;
            chat_t_hanlder.Start();
        }

        public delegate void MessageDisplayHandler(string chat_message, string chat_user_name);
        public event MessageDisplayHandler chat_OnReceived;

        public delegate void DisconnectedHandler(TcpClient chat_clientSocket);
        public event DisconnectedHandler chat_OnDisconnected;

        private void chat_doChat()
        {
            NetworkStream chat_stream = null;
            try
            {
                byte[] chat_buffer = new byte[1024];
                string chat_msg = string.Empty;
                int chat_bytes = 0;
                int chat_MessageCount = 0;

                while (true)
                {
                    chat_MessageCount++;
                    chat_stream = chat_clientSocket.GetStream();
                    chat_bytes = chat_stream.Read(chat_buffer, 0, chat_buffer.Length);
                    chat_msg = Encoding.Unicode.GetString(chat_buffer, 0, chat_bytes);
                    chat_msg = chat_msg.Substring(0, chat_msg.IndexOf("$"));

                    if (chat_OnReceived != null)
                        chat_OnReceived(chat_msg, chat_clientList[chat_clientSocket].ToString());
                }
            }
            catch (SocketException se)
            {
                Trace.WriteLine(string.Format("doChat - SocketException : {0}", se.Message));

                if (chat_clientSocket != null)
                {
                    if (chat_OnDisconnected != null)
                        chat_OnDisconnected(chat_clientSocket);

                    chat_clientSocket.Close();
                    chat_stream.Close();
                }
            }

            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("doChat - Exception : {0}", ex.Message));

                if (chat_clientSocket != null)
                {
                    if (chat_OnDisconnected != null)
                        chat_OnDisconnected(chat_clientSocket);

                    chat_clientSocket.Close();
                    chat_stream.Close();
                }
            }
        }
    }
}
