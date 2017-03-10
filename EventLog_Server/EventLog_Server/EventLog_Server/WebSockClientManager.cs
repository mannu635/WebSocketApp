/*All rights reserved, Copyrights Ashish Patil, 2010. http://ashishware.com/
 * Permission granted to modify/use this code for non commercial purpose so long as the
 * original notice is retained. This code is for educational purpose ONLY. 
 * DISCLAIMER: The code is provided as is, without warranties of any kind. The author
 * is not responsible for any damage (of any kind) this code may cause. The author is also not
 * responsible for any misuse of the code. Use and run this code AT YOUR OWN RISK.
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace EventLog_Server
{
    class WebSockClientManager
    {
        private List<WebSockClient> _clientList;
        private delegate void ClientHandler(WebSockClient c);
        private static int _port, _maxConnection;
        private static string _origin, _location;

        private WebSockClientManager()
        {

        }
        
        public WebSockClientManager(int maxConnection,string origin,string location, int port)
        {
            if (string.IsNullOrEmpty(location) || string.IsNullOrEmpty(origin))
            {
                throw new Exception("Location and Origin are required!");
            }

            _maxConnection = maxConnection;
            _origin = origin;
            _location = location;
            _port = port;
            _clientList = new List<WebSockClient>();
        }

        public void AddClient(WebSockClient c)
        {
            if (_clientList.Count >= _maxConnection)
            { // check if any connection is available
                List<int> closedClients = new List<int>();
                for (int i = 0; i < _clientList.Count; i++)
                {
                    if (_clientList[i].TcpClientInstance.Connected == false)
                    {
                        _clientList[i].TcpClientInstance.Close();
                  
                         closedClients.Add(i);
                    }
                }

                foreach (int e in closedClients)
                {
                    _clientList.RemoveAt(e);
                }
            }

            if (_clientList.Count < _maxConnection)
            {
                _clientList.Add(c);

                Thread clientThread = new Thread(delegate()
                {
                    this.HandleClient(c); ;
                });
                c.ManagingThreadId = clientThread.ManagedThreadId;
                clientThread.Start();
                Console.WriteLine("New Thread Started:" + c.ManagingThreadId.ToString());
            }
            else
            {
                //sorry
                c.TcpClientInstance.Close();
            }

            Console.WriteLine("Thread count :" + Process.GetCurrentProcess().Threads.Count.ToString());
                       
        }

        private  void HandleClient(WebSockClient c)
        {
            try
            {
                int b;
                c.WebSocketConnectionStatus = WebSockClient.WebSockClientStatus.CONNECTING;
                c.IsSubscribed = true;
                using (NetworkStream n = c.TcpClientInstance.GetStream())
                using (StreamWriter streamWriter = new StreamWriter(n))
                {
                    byte[] buff = new byte[255];
                    c.WebSocketConnectionStatus = WebSockClient.WebSockClientStatus.CONNECTING;

                    while (c.TcpClientInstance.Connected)
                    {



                        //Read and Validate client Handshake.
                        if (c.WebSocketConnectionStatus == WebSockClient.WebSockClientStatus.CONNECTING)
                            while (n.DataAvailable)
                            { n.Read(buff, 0, buff.Length); }

                        //Send Sever Handshake to client.
                        if (c.WebSocketConnectionStatus == WebSockClient.WebSockClientStatus.CONNECTING)
                        {
                            string handshake =
                            "HTTP/1.1 101 Web Socket Protocol Handshake\r\n" +
                            "Upgrade: WebSocket\r\n" +
                            "Connection: Upgrade\r\n" +
                            "WebSocket-Origin: "+_origin+"\r\n" +
                            "WebSocket-Location: "+_location+"\r\n" +
                            "\r\n";
                            streamWriter.Write(handshake);
                            streamWriter.Flush();
                        }

                        if (c.TcpClientInstance.Connected)
                        {
                            c.WebSocketConnectionStatus = WebSockClient.WebSockClientStatus.HANDSHAKEDONE;
                        }

                        // Read data from client. Do whatever is required.
                        while (n.DataAvailable)
                        {
                            if (n.ReadByte() == 'S')
                                if (n.ReadByte() == 'T')
                                {
                                    b = n.ReadByte();
                                    if (b == 'P')
                                    {
                                        c.IsSubscribed = false;
                                        Console.WriteLine("Client:" + c.ManagingThreadId.ToString() + " unsubscribed");

                                    }
                                    else if (b == 'R')
                                    {
                                        c.IsSubscribed = true;
                                        Console.WriteLine("Client:" + c.ManagingThreadId.ToString() + " subscribed");
                                    }

                                }

                                else
                                    Console.WriteLine("Client " + c.ManagingThreadId.ToString() + "says :" + n.ReadByte().ToString());
                        }


                        // If Writebuffer is full, write stuff to client
                        if (c.WriteBuffer != null && c.WriteBuffer.Length > 0 && c.IsSubscribed)
                        {
                            n.WriteByte(0x00);
                            n.Write(c.WriteBuffer, 0, c.WriteBuffer.Length);
                            n.WriteByte(0xff);
                            c.WriteBuffer = null;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                if (c.TcpClientInstance.Connected == true)
                { Console.WriteLine(e.StackTrace); }
                return;


            }
            finally
            {
                c.TcpClientInstance.Close();
                Console.WriteLine("Client:" + c.ManagingThreadId.ToString() + " closed");
                c.WebSocketConnectionStatus = WebSockClient.WebSockClientStatus.DISCONNECTED;
            }

        }

        public void WriteData(string data)
        {

            foreach (WebSockClient wc in _clientList)
            {
                try
                {
                    if (wc.TcpClientInstance.Connected && wc.WebSocketConnectionStatus == WebSockClient.WebSockClientStatus.HANDSHAKEDONE
                        && wc.IsSubscribed)
                    {
                        wc.WriteBuffer = Encoding.ASCII.GetBytes(data);
                    }

                }
                catch
                {
                    Console.WriteLine("Writing to client failed.Closing client.");
                    wc.TcpClientInstance.Close();
                }
            }

        }


    }
}
