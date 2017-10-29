using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading;

namespace GVMetting.Core.Net
{
    /// <summary>异步TCP客户端</summary>
    public class AsyncTcpClient : IDisposable
    {
        #region Fields

        private TcpClient tcpClient;
        private bool disposed = false;
        private int retries = 0;

        #endregion

        #region Ctors

        /// <summary>异步TCP客户端</summary>
        /// <param name="remoteHostName">远端服务器主机名</param>
        /// <param name="remotePort">远端服务器端口</param>
        public AsyncTcpClient(string remoteHostName, int remotePort)
            : this(Dns.GetHostAddresses(remoteHostName), remotePort)
        {
        }

        /// <summary>异步TCP客户端</summary>
        /// <param name="remoteIPAddresses">远端服务器IP地址列表</param>
        /// <param name="remotePort">远端服务器端口</param>
        public AsyncTcpClient(IPAddress[] remoteIPAddresses, int remotePort)
            : this(remoteIPAddresses, remotePort, null)
        {
        }

        /// <summary>异步TCP客户端</summary>
        /// <param name="remoteIPAddresses">远端服务器IP地址列表</param>
        /// <param name="remotePort">远端服务器端口</param>
        /// <param name="localEP">本地客户端终结点</param>
        public AsyncTcpClient(
          IPAddress[] remoteIPAddresses, int remotePort, IPEndPoint localEP)
        {
            this.Addresses = remoteIPAddresses;
            this.Port = remotePort;
            this.LocalIPEndPoint = localEP;
            this.Encoding = Encoding.Default;

            if (this.LocalIPEndPoint != null)
            {
                this.tcpClient = new TcpClient(this.LocalIPEndPoint);
            }
            else
            {
                this.tcpClient = new TcpClient();
            }
            this.tcpClient.ReceiveBufferSize = 1024 * 10;
            Retries = 3;
            RetryInterval = 5;
        }

        #endregion

        #region Events

        /// <summary>接收到数据报文事件</summary>
        public event EventHandler<ReceivedEventArgs<byte[]>> DatagramReceived;

        /// <summary>与服务器的连接已建立事件</summary>
        public event EventHandler<ServerEventArgs> ServerConnected;
        /// <summary>与服务器的连接已断开事件</summary>
        public event EventHandler<ServerEventArgs> ServerDisconnected;
        /// <summary>与服务器的连接发生异常事件</summary>
        public event EventHandler<ExceptionEventArgs> ServerException;

        private void RaiseDatagramReceived(TcpClient sender, byte[] datagram)
        {
            if (DatagramReceived != null)
            {
                DatagramReceived(this,
                  new ReceivedEventArgs<byte[]>(sender, datagram));
            }
        }

        private void RaiseServerConnected(IPAddress[] ipAddresses, int port)
        {
            if (ServerConnected != null)
            {
                ServerConnected(this, new ServerEventArgs(ipAddresses, port));
            }
        }

        private void RaiseDisconnected(IPAddress[] ipAddresses, int port)
        {
            if (ServerDisconnected != null)
            {
                ServerDisconnected(this, new ServerEventArgs(ipAddresses, port));
            }
        }

        private void RaiseServerException(IPAddress[] ipAddresses, int port, Exception innerException)
        {
            if (ServerException != null)
            {
                ServerException(this, new ExceptionEventArgs(ipAddresses, port, innerException));
            }
        }

        #endregion

        #region Properties

        /// <summary>是否已与服务器建立连接</summary>
        public bool Connected { get { return tcpClient.Client.Connected; } }
        /// <summary>
        /// 远端服务器的IP地址列表
        /// </summary>
        public IPAddress[] Addresses { get; private set; }
        /// <summary>远端服务器的端口</summary>
        public int Port { get; private set; }
        /// <summary>连接重试次数</summary>
        public int Retries { get; set; }
        /// <summary>连接重试间隔</summary>
        public int RetryInterval { get; set; }
        /// <summary>远端服务器终结点</summary>
        public IPEndPoint RemoteIPEndPoint
        {
            get { return new IPEndPoint(Addresses[0], Port); }
        }
        /// <summary>本地客户端终结点</summary>
        protected IPEndPoint LocalIPEndPoint { get; private set; }
        /// <summary>通信所使用的编码</summary>
        public Encoding Encoding { get; set; }

        #endregion

        #region Connect

        /// <summary>连接到服务器</summary>
        /// <returns>异步TCP客户端</returns>
        public AsyncTcpClient Connect()
        {
            if (!Connected)
            {
                tcpClient.BeginConnect(Addresses, Port, HandleConnected, tcpClient);
            }

            return this;
        }

        /// <summary>关闭与服务器的连接</summary>
        /// <returns>异步TCP客户端</returns>
        public AsyncTcpClient Close()
        {
            if (Connected)
            {
                retries = 0;
                tcpClient.Close();
                RaiseDisconnected(Addresses, Port);
            }

            return this;
        }

        #endregion

        #region Receive

        private void HandleConnected(IAsyncResult ar)
        {
            try
            {
                tcpClient.EndConnect(ar);
                RaiseServerConnected(Addresses, Port);
                retries = 0;
            }
            catch (Exception ex)
            {
                ExceptionHandler.AlwaysHandle.HandleException(ex);
                if (retries > 0)
                {
                    //Logger.Debug(string.Format(CultureInfo.InvariantCulture,
                    //  "Connect to server with retry {0} failed.", retries));
                }

                retries++;
                if (retries > Retries)
                {
                    // we have failed to connect to all the IP Addresses, 
                    // connection has failed overall.
                    RaiseServerException(Addresses, Port, ex);
                    return;
                }
                else
                {
                    //Logger.Debug(string.Format(CultureInfo.InvariantCulture,
                    //  "Waiting {0} seconds before retrying to connect to server.",
                    //  RetryInterval));
                    Thread.Sleep(TimeSpan.FromSeconds(RetryInterval));
                    Connect();
                    return;
                }
            }

            // we are connected successfully and start asyn read operation.
            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
            tcpClient.GetStream().BeginRead(
              buffer, 0, buffer.Length, HandleDatagramReceived, buffer);
        }

        private void HandleDatagramReceived(IAsyncResult ar)
        {
            if (tcpClient.Connected == false)
            {
                return;
            }
            NetworkStream stream = tcpClient.GetStream();

            int numberOfReadBytes = 0;
            try
            {
                numberOfReadBytes = stream.EndRead(ar);
            }
            catch
            {
                numberOfReadBytes = 0;
            }

            if (numberOfReadBytes == 0)
            {
                // connection has been closed
                Close();
                return;
            }

            byte[] buffer = (byte[])ar.AsyncState;
            byte[] receivedBytes = new byte[numberOfReadBytes];
            Buffer.BlockCopy(buffer, 0, receivedBytes, 0, numberOfReadBytes);
            RaiseDatagramReceived(tcpClient, receivedBytes);

            // then start reading from the network again
            stream.BeginRead(
              buffer, 0, buffer.Length, HandleDatagramReceived, buffer);
        }

        #endregion

        #region Send

        /// <summary>发送报文</summary>
        /// <param name="datagram">报文</param>
        public void Send(byte[] datagram)
        {
            if (datagram == null)
                throw new ArgumentNullException("datagram");

            if (!Connected)
            {
                RaiseDisconnected(Addresses, Port);
                throw new InvalidProgramException(
                  "This client has not connected to server.");
            }
 
            tcpClient.GetStream().BeginWrite(
              datagram, 0, datagram.Length, HandleDatagramWritten, tcpClient);
        }

        private void HandleDatagramWritten(IAsyncResult ar)
        {
            try
            {
                ((TcpClient)ar.AsyncState).GetStream().EndWrite(ar);
            }
            catch { }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed 
        /// and unmanaged resources; <c>false</c> 
        /// to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Close();

                        if (tcpClient != null)
                        {
                            tcpClient = null;
                        }
                    }
                    catch (SocketException ex)
                    {
                        ExceptionHandler.AlwaysHandle.HandleException(ex);
                    }
                }

                disposed = true;
            }
        }

        #endregion
    }
}
