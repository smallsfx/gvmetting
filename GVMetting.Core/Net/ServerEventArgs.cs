using System;
using System.Net;

namespace GVMetting.Core.Net
{
    public class ServerEventArgs : EventArgs
    {
        public ServerEventArgs(IPAddress[] ipAddresses, int port)
        {
            if (ipAddresses == null)
                throw new ArgumentNullException("ipAddresses");

            this.Addresses = ipAddresses;
            this.Port = port;
        }
        public IPAddress[] Addresses { get; private set; }
        public int Port { get; private set; }
    }
}
