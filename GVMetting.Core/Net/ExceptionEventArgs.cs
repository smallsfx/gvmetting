using System;
using System.Net;

namespace GVMetting.Core.Net
{
    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(IPAddress[] address, int port, Exception error)
        {
            this.Addresses = address;
            this.Port = port;
            this.Exception = error;
        }
        public IPAddress[] Addresses { get; private set; }
        public int Port { get; private set; }
        public Exception Exception { get; private set; }

    }
}
