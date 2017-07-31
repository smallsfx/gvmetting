using System;
using System.Net.Sockets;

namespace GVMetting.Core.Net.WebSockets
{
    public class SocketEventArgs : EventArgs
    {
        public SocketEventArgs(Socket client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            this.Client = client;
        }
        public Socket Client { get; private set; }
    }

    public class SocketReceivedEventArgs<T> : EventArgs
    {
        public SocketReceivedEventArgs(Socket client, T datagram)
        {
            Client = client;
            Datagram = datagram;
        }

        public Socket Client { get; private set; }
        public T Datagram { get; private set; }
    }
}
