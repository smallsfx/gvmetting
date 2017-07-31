using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace GVMetting.Core.Net
{
    /// <summary>异步TCP服务器</summary>
    public class AsyncTcpServer : IDisposable
    {
        #region Fields

        private TcpListener listener;
        private List<ClientState> clients;

        #endregion

        #region Events

        /// <summary>接收到数据报文事件</summary>
        public event EventHandler<ReceivedEventArgs<byte[]>> DatagramReceived;
        /// <summary>与客户端的连接已建立事件</summary>
        public event EventHandler<ClientEventArgs> ClientConnected;
        /// <summary>与客户端的连接已断开事件</summary>
        public event EventHandler<ClientEventArgs> ClientDisconnected;

        private void RaiseDatagramReceived(TcpClient sender, byte[] datagram)
        {
            if (DatagramReceived != null)
            {
                DatagramReceived(this, new ReceivedEventArgs<byte[]>(sender, datagram));
            }
        }

        private void RaiseConnected(TcpClient tcpClient)
        {
            if (ClientConnected != null)
            {
                ClientConnected(this, new ClientEventArgs(tcpClient));
            }
        }

        private void RaiseDisconnected(TcpClient tcpClient)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(this, new ClientEventArgs(tcpClient));
            }
        }

        #endregion

        #region Ctors

        public AsyncTcpServer(int listenPort) : this(IPAddress.Any, listenPort) { }
        public AsyncTcpServer(IPAddress localIPAddress, int listenPort)
        {
            Address = localIPAddress;
            Port = listenPort;
            this.Encoding = Encoding.Default;

            clients = new List<ClientState>();

            listener = new TcpListener(Address, Port);
            listener.AllowNatTraversal(true);
        }

        #endregion

        #region Properties

        /// <summary>获取已连接的客户端数量。</summary>
        public int ClientCount { get { return clients.Count; } }
        /// <summary>服务器是否正在运行</summary>
        public bool IsRunning { get; private set; }
        /// <summary>监听的IP地址</summary>
        public IPAddress Address { get; private set; }
        /// <summary>监听的端口</summary>
        public int Port { get; private set; }
        /// <summary>通信使用的编码</summary>
        public Encoding Encoding { get; set; }

        #endregion

        #region Start & Stop

        public AsyncTcpServer Start()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                listener.Start();
                listener.BeginAcceptTcpClient(new AsyncCallback(HandleAccepted), listener);
            }
            return this;
        }

        public AsyncTcpServer Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                listener.Stop();

                lock (this.clients)
                {
                    for (int i = 0; i < this.clients.Count; i++)
                    {
                        this.clients[i].Client.Client.Disconnect(false);
                    }
                    this.clients.Clear();
                }

            }
            return this;
        }

        #endregion //#region Start & Stop

        #region Receive

        private void HandleAccepted(IAsyncResult ar)
        {
            if (IsRunning)
            {
                TcpListener tcpListener = (TcpListener)ar.AsyncState;

                TcpClient tcpClient = tcpListener.EndAcceptTcpClient(ar);
                byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

                var state = new ClientState(tcpClient, buffer);
                lock (this.clients)
                {
                    this.clients.Add(state);
                    RaiseConnected(tcpClient);
                }

                var stream = state.NetworkStream;
                stream.BeginRead(state.Buffer, 0, state.Buffer.Length, HandleReceived, state);
                tcpListener.BeginAcceptTcpClient(new AsyncCallback(HandleAccepted), ar.AsyncState);
            }
        }

        private void HandleReceived(IAsyncResult ar)
        {
            try
            {
                if (IsRunning == false)
                {
                    return;
                }
                var state = (ClientState)ar.AsyncState;
                var stream = state.NetworkStream;
                if (stream == null)
                {
                    return;
                }
                int size = 0;
                try
                {
                    size = stream.EndRead(ar);
                }
                catch
                {
                    size = 0;
                }

                if (size == 0)
                {
                    lock (this.clients)
                    {
                        this.clients.Remove(state);
                        RaiseDisconnected(state.Client);
                        return;
                    }
                }
                byte[] receivedBytes = new byte[size];
                Buffer.BlockCopy(state.Buffer, 0,receivedBytes, 0, size);
                RaiseDatagramReceived(state.Client, receivedBytes);

                stream.BeginRead(state.Buffer, 0, state.Buffer.Length, HandleReceived, state);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
            }

        }

        #endregion

        #region Send
        private void HandleDatagramWritten(IAsyncResult ar)
        {
            try
            {
                var tcpclient = ((TcpClient)ar.AsyncState);
                tcpclient.GetStream().EndWrite(ar);
            }
            catch { }
        }
        public void Send(TcpClient tcpClient, byte[] datagram)
        {
            try
            {
                tcpClient.GetStream().BeginWrite(
                      datagram, 0, datagram.Length, HandleDatagramWritten, tcpClient);
            }
            catch { }
        }
        public void SendAll(byte[] datagram)
        {
            for (int i = 0; i < this.clients.Count; i++)
            {
                Send(this.clients[i].Client, datagram);
            }
        }

        #endregion

        #region IDisposable
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Stop();

                        if (listener != null)
                        {
                            listener = null;
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
