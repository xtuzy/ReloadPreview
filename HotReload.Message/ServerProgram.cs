﻿using System;
using System.Threading;								// Sleeping
using System.Net;									// Used to local machine info
using System.Net.Sockets;							// Socket namespace
using System.Collections;                           // Access to the Array list
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace HotReload.Message
{
    /// <summary>
    /// 为了热重载简化过程
    /// </summary>
    public class ServerProgram
    {
        // Attributes
        public List<Socket> CurrentClients = new List<Socket>();// 代理端的Socket

        //静态处理实例,因为一个ip貌似只能一个服务?
        private static ServerProgram App;
        private Socket sockListener;//服务端的Socket

        public event EventHandler AcceptedMessageEvent;
        public IPAddress MyIp = null;
        public int Port;

        public Action<object, object> AcceptedFileEvent { get; internal set; }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="args"></param>
        public static ServerProgram CreatServerProgram(int port)
        {
            if (App != null)
                return App;

            App = new ServerProgram();
            // Welcome and Start listening
            Console.WriteLine("*** Chat Server Started {0} *** ", DateTime.Now.ToString("G"));


            //const int nPortListen = 399;
            int nPortListen = port;
            App.Port = port;

            // Determine the IPAddress of this machine
            IPAddress[] aryLocalAddr = null;
            String strHostName = "";
            try
            {
                // NOTE: DNS lookups are nice and all but quite time consuming.
                strHostName = Dns.GetHostName();
                //IPHostEntry ipEntry = Dns.GetHostByName( strHostName );//过时方法
                IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);

                aryLocalAddr = ipEntry.AddressList;//得到ip地址列表
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error trying to get local address {0} ", ex.Message);
            }

            // Verify we got an IP address. Tell the user if we did
            if (aryLocalAddr == null || aryLocalAddr.Length < 1)
            {
                Console.WriteLine("Unable to get local address");
                return null;
            }

            // Create the sockListener socket in this machines IP address

            App.sockListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //sockListener.Bind( new IPEndPoint( aryLocalAddr[0], nPortListen ) );//这里报错,说地址和 AddressFamily.InterNetwork不符合
            //sockListener.Bind( new IPEndPoint( IPAddress.Loopback, nPortListen ) );	// For use with localhost 127.0.0.1

            //这里查找出符合的ip地址,参考https://stackoverflow.com/questions/2370388/socketexception-address-incompatible-with-requested-protocol

            foreach (var ip in aryLocalAddr)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    App.MyIp = ip;
                    App.sockListener.Bind(new IPEndPoint(ip, nPortListen));
                    Console.WriteLine("Listening on : [{0}] {1}:{2}", strHostName, ip, nPortListen);
                    break;
                }
            }

            App.sockListener.Listen(10);

            // Setup a callback to be notified of connection requests
            App.sockListener.BeginAccept(new AsyncCallback(App.OnConnectRequest), App.sockListener);

            return App;
        }

        /// <summary>
        /// 回调监听链接请求
        /// </summary>
        /// <param name="ar"></param>
        private void OnConnectRequest(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            try
            {
                NewConnection(listener.EndAccept(ar));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
            listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);
        }

        /// <summary>
        /// 对新连接处理
        /// </summary>
        /// <param name="sockClient"></param>
        private void NewConnection(Socket sockClient)
        {
            if (sockClient == null)
                return;
            CurrentClients.Add(sockClient);
            Console.WriteLine("Accept from client {0}, joined", sockClient.RemoteEndPoint);
        }

        public void SendFile(string filePath)
        {
            byte[] m_byBuff = new byte[1024 * 1024];
            byte[] bytes;

            using (var stream = File.OpenRead(filePath))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }
            }

            var removeClient = new List<Socket>();
            foreach (var sock in CurrentClients)
            {
                try
                {
                    sock.Send(Encoding.ASCII.GetBytes(bytes.Length.ToString()));
                    var okBytesLength = sock.Receive(m_byBuff, m_byBuff.Length, SocketFlags.None);//接收数据长度
                    if (Encoding.ASCII.GetString(m_byBuff, 0, okBytesLength) == "OK")
                    {
                        Console.WriteLine("***Start send dll {0} to {1} ***", bytes.Length, sock.RemoteEndPoint);
                        sock.Send(bytes);
                        Console.WriteLine("***Finish send dll to {0} ***", sock.RemoteEndPoint);
                    }
                }
                catch
                {
                    // If the send fails the close the connection
                    Console.WriteLine("Send to client {0} failed, will remove it", sock.RemoteEndPoint);
                    sock.Close();
                    removeClient.Add(sock);
                }
            }

            foreach (var sock in removeClient)
            {
                CurrentClients.Remove(sock);
            }
        }
    }
}
