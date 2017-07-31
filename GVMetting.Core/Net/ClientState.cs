using System.Collections.Generic;
using System.Net.Sockets;

namespace GVMetting.Core.Net
{
    internal class ClientState
    {
        public ClientState(TcpClient client,byte[] buffer)
        {
            this.Client = client;
            this.Buffer = buffer;
        }

        public TcpClient Client { get; private set; }
        public byte[] Buffer { get; private set; }
        public NetworkStream NetworkStream
        {
            get
            {
                return Client.GetStream();
            }
        }
    }
}
