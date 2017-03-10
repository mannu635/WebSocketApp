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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EventLog_Server
{
    class WebSockClient
    {
        public int ManagingThreadId { get; set; }
        public string WebSocketOrigin { get; set; }
        public string WebSocketLocationURL { get; set; }
        public bool IsSubscribed { get; set; }
        private byte[] _buffer = new byte[255];
        private byte[] _writeBuffer;
        private TcpClient _tcpClient;
        public WebSockClientStatus WebSocketConnectionStatus { get; set; }

        public byte[] WriteBuffer
        {
            set { _writeBuffer = value; }
            get { return _writeBuffer; }
        }

        public TcpClient TcpClientInstance
        { get { return _tcpClient; } }

        public WebSockClient(TcpClient t)
        {
            _tcpClient = t;
        }

        public enum WebSockClientStatus
        {
            CONNECTING = 0,
            HANDSHAKEDONE = 3,
            DISCONNECTED = 6,
            CLIENTSUBSCRIBED = 7,
            CLIENTUNSUBSCRIBED = 8
        }
        public WebSockClient(string webSockOrigin, string webSockLocationURL)
        {
            this.WebSocketLocationURL = webSockLocationURL;
            this.WebSocketOrigin = webSockOrigin;
        }

    }
}
