using System;
using System.Net.Sockets;

namespace GVMetting.Core.Net
{
    public class ReceivedEventArgs<T> : EventArgs
    {
        public ReceivedEventArgs(TcpClient tcpClient, T datagram)
        {
            TcpClient = tcpClient;
            Datagram = datagram;
        }

        public TcpClient TcpClient { get; private set; }

        public T Datagram { get; private set; }
    }
}
