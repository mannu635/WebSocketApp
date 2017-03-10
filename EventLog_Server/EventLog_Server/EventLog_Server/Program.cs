using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Configuration;
/*All rights reserved, Copyrights Ashish Patil, 2010. http://ashishware.com/
 * Permission granted to modify/use this code for non commercial purpose so long as the
 * original notice is retained. This code is for educational purpose ONLY. 
 * DISCLAIMER: The code is provided as is, without warranties of any kind. The author
 * is not responsible for any damage (of any kind) this code may cause. The author is also not
 * responsible for any misuse of the code. Use and run this code AT YOUR OWN RISK.
 * 
 */

namespace EventLog_Server
{
    class Program
    {
        static WebSockClientManager m;
        static int _port, _maxConnection;
        static string _origin, _location;

       
        static void Main(string[] args)
        {
            try
            {
                if (EventLog.Exists(ConfigurationSettings.AppSettings["LOGTOMONITOR"]))
                {
                    EventLog e = new EventLog(ConfigurationSettings.AppSettings["LOGTOMONITOR"]);
                      e.EntryWritten += new EntryWrittenEventHandler(e_EntryWritten);
                      e.EnableRaisingEvents = true;
                }
            }
            catch
            {
                Console.WriteLine("Error opening event log! Application will close.");
                return;
            }

            if (!int.TryParse(System.Configuration.ConfigurationSettings.AppSettings["PORT"], out _port))
            {
                Console.WriteLine("Invalid port number specified in config file.");
                return;
            }

            if (!int.TryParse(System.Configuration.ConfigurationSettings.AppSettings["MAXCONNECTIONCOUNT"], out _maxConnection))
            {
                Console.WriteLine("Invalid max connection number specified in config file.");
                return;
            }

            _location = System.Configuration.ConfigurationSettings.AppSettings["WEBSOCKETLOCATION"];
            _origin = System.Configuration.ConfigurationSettings.AppSettings["WEBSOCKETORIGIN"];

            TcpListener t = new TcpListener(IPAddress.Loopback, _port);            
            m = new WebSockClientManager(_maxConnection,_origin,_location,_port);           
            t.Start();
            while (true)
            {
                TcpClient c = t.AcceptTcpClient();
                //c.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
                WebSockClient w = new WebSockClient(c);
                m.AddClient(w);
                Thread.Sleep(300);
                if (w.WebSocketConnectionStatus == WebSockClient.WebSockClientStatus.HANDSHAKEDONE)
                { 
                    w.WriteBuffer = ASCIIEncoding.ASCII.GetBytes("Hello Client "+ w.ManagingThreadId.ToString());  
                }
              
            }
                      
        }

        static void e_EntryWritten(object sender, EntryWrittenEventArgs e)
        {
            m.WriteData(e.Entry.InstanceId + "::" + e.Entry.TimeGenerated.ToString() + ": " + e.Entry.Message);
        }

       
    }
 
}
