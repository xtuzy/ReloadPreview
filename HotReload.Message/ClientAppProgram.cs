/*using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Xamarin.Essentials;

namespace ClientApp
{
    public delegate void AcceptFileHandler(string path);
    public class ClientAppProgram : IDisposable
    {
        // My Attributes
        private Socket m_sock;                      // Server connection
        private byte[] m_byBuff = new byte[1024];    // Recieved data buffer
        public event AcceptFileHandler AcceptedFileEvent;              // Add Message Event handler for Form

        public string ServerIPAddressText;
        public int ServerPort;
        public ClientAppProgram(string serverIp,int port)
        { 
            ServerIPAddressText = serverIp;
            ServerPort = port;
        }


        /// <summary>
        /// Connect button pressed. Attempt a connection to the server and 
        /// setup Recieved data callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Connect()
        {
            try
            {
                // Close the socket if it is still open
                if (m_sock != null && m_sock.Connected)
                {
                    m_sock.Shutdown(SocketShutdown.Both);
                    System.Threading.Thread.Sleep(10);
                    m_sock.Close();
                }

                // Create the socket object
                m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Define the Server address and port
                IPEndPoint epServer = new IPEndPoint(IPAddress.Parse(ServerIPAddressText), ServerPort);

                // Connect to the server blocking method and setup callback for recieved data
                // m_sock.Connect( epServer );
                // SetupRecieveCallback( m_sock );

                // Connect to server non-Blocking method
                m_sock.Blocking = false;
                AsyncCallback onconnect = new AsyncCallback(OnConnect);
                m_sock.BeginConnect(epServer, onconnect, m_sock);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ",Server Connect failed!");
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            // Socket was the passed in object
            Socket sock = (Socket)ar.AsyncState;

            // Check if we were sucessfull
            try
            {
                //sock.EndConnect( ar );
                if (sock.Connected)
                    SetupRecieveCallback(sock);
                else
                    Console.WriteLine("Unable to connect to remote machine," + "Connect Failed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ",Unusual error during Connect!");
            }
        }

        /// <summary>
        /// Get the new data and send it out to all other connections. 
        /// Note: If not data was recieved the connection has probably 
        /// died.
        /// </summary>
        /// <param name="ar"></param>
        private void OnRecievedData(IAsyncResult ar)
        {
            // Socket was the passed in object
            Socket sock = (Socket)ar.AsyncState;

            // Check if we got any data
            try
            {
                int nBytesRec = sock.EndReceive(ar);
                if (nBytesRec > 0)
                {
                    // Wrote the data to the List
                    string sRecieved = Encoding.ASCII.GetString(m_byBuff, 0, nBytesRec);

                    //区别每次传输,该次传输在这里取完
                    if (sRecieved.Contains("START"))
                    {
                        //string length = sRecieved.Split('|')[1];
                        sock.Send(Encoding.ASCII.GetBytes("OK"));
                        var tempFile = Path.Combine(FileSystem.CacheDirectory, "tempDll.dll");
                        using (var stream = File.Create(tempFile))
                        {
                            int bytes;
                            do
                            {
                                bytes = sock.Receive(m_byBuff, m_byBuff.Length, SocketFlags.None);//接收数据长度
                                stream.Write(m_byBuff, 0, bytes);//写入流
                                stream.Flush();//缓冲写入文件
                            }
                            while (bytes > 0);
                        }

                        AcceptedFileEvent.Invoke(tempFile);
                    }
                        
                    //下次回调
                    // If the connection is still usable restablish the callback
                    SetupRecieveCallback(sock);
                }
                else
                {
                    // If no data was recieved then the connection is probably dead
                    Console.WriteLine("Client {0}, disconnected", sock.RemoteEndPoint);
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ",Unusual error druing Recieve!");
            }
        }

        /// <summary>
        /// Setup the callback for recieved data and loss of conneciton
        /// 接受数据,建立回调
        /// </summary>
        private void SetupRecieveCallback(Socket sock)
        {
            try
            {
                AsyncCallback recieveData = new AsyncCallback(OnRecievedData);//回调
                sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData, sock);//接受数据,植入回调
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ",Setup Recieve Callback failed!");
            }
        }

        /// <summary>
        /// Close the Socket connection bofore going home
        /// </summary>
        private void Connect_Closing()
        {
            if (m_sock != null && m_sock.Connected)
            {
                m_sock.Shutdown(SocketShutdown.Both);
                m_sock.Close();
                Console.WriteLine("连接关闭");
            }
        }

        public void SendMessage(byte[] message)
        {
            // Check we are connected
            if (m_sock == null || !m_sock.Connected)
            {
                Console.WriteLine("Must be connected to Send a message");
                return;
            }

            try
            {
                m_sock.Send(message, message.Length, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " Send Message Failed!");
            }
        }

        public void Dispose()
        {
            Connect_Closing();
        }
    }
}
*/