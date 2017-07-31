using System;
using System.Net.Sockets;

namespace GVMetting.Core.Net
{
    public class ClientEventArgs : EventArgs
    {
        public ClientEventArgs(TcpClient tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");

            this.TcpClient = tcpClient;
        }
        public TcpClient TcpClient { get; private set; }
    }
}
