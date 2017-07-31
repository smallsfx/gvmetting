using System;
using System.Net.Sockets;

namespace GVMetting.Core.Net.WebSockets
{
    internal class WebSocketState
    {
        // Size of receive buffer.     
        public const int BufferSize = 1024;
        public WebSocketState(Socket client, byte[] buffer)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            this.Client = client;
            this.Buffer = buffer;
        }
        public Socket Client { get; private set; }
        public byte[] Buffer { get; private set; }
    }

}
