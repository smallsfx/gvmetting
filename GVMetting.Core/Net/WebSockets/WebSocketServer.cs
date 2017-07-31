using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace GVMetting.Core.Net.WebSockets
{
    public class WebSocketServer : IDisposable
    {
        #region Fields

        private Socket listener;
        private List<WebSocketState> clients;
        private bool disposed = false;

        #endregion

        #region Ctors
        public WebSocketServer(int listenPort) : this(IPAddress.Any, listenPort) { }
        public WebSocketServer(IPEndPoint localEP): this(localEP.Address, localEP.Port) { }

        /// <summary>
        /// 异步TCP服务器
        /// </summary>
        /// <param name="localIPAddress">监听的IP地址</param>
        /// <param name="listenPort">监听的端口</param>
        public WebSocketServer(IPAddress localIPAddress, int listenPort)
        {
            Address = localIPAddress;
            Port = listenPort;
            this.Encoding = Encoding.Default;

            IPEndPoint localEP = new IPEndPoint(localIPAddress, listenPort);
            clients = new List<WebSocketState>();
            listener = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEP);
        }

        #endregion

        #region Properties

        /// <summary>
        /// 服务器是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// 监听的IP地址
        /// </summary>
        public IPAddress Address { get; private set; }
        /// <summary>
        /// 监听的端口
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// 通信使用的编码
        /// </summary>
        public Encoding Encoding { get; set; }

        #endregion

        #region Server

        /// <summary> 启动服务器 </summary>
        /// <returns>异步TCP服务器</returns>
        public WebSocketServer Start()
        {
            return Start(100);
        }

        /// <summary> 启动服务器 </summary>
        /// <param name="backlog">
        /// 服务器所允许的挂起连接序列的最大长度
        /// </param>
        /// <returns>异步TCP服务器</returns>
        public WebSocketServer Start(int backlog)
        {
            if (!IsRunning)
            {
                IsRunning = true;
                listener.Listen(backlog);
                listener.BeginAccept(
                  new AsyncCallback(HandleAccepted), listener);
            }
            return this;
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        /// <returns>异步TCP服务器</returns>
        public WebSocketServer Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                listener.Close();

                lock (this.clients)
                {
                    for (int i = 0; i < this.clients.Count; i++)
                    {
                        this.clients[i].Client.Disconnect(false);
                    }
                    this.clients.Clear();
                }

            }
            return this;
        }

        #endregion

        #region Receive

        private void HandleAccepted(IAsyncResult ar)
        {
            if (IsRunning)
            {
                Socket socketListener = (Socket)ar.AsyncState;
                var client = socketListener.EndAccept(ar);
                
                byte[] buffer = new byte[client.ReceiveBufferSize];
                var state = new WebSocketState(client, buffer);
                
                lock (this.clients)
                {
                    this.clients.Add(state);
                    RaiseClientConnected(client);
                }

                client.BeginReceive(state.Buffer
                    , 0
                    , WebSocketState.BufferSize
                    , SocketFlags.None
                    , new AsyncCallback(HandleReceived)
                    , state);
                socketListener.BeginAccept(
                  new AsyncCallback(HandleAccepted), socketListener);
            }
        }
        private void HandleReceived(IAsyncResult ar)
        {
            if (IsRunning)
            {
                WebSocketState state = (WebSocketState)ar.AsyncState;
                var client = state.Client;
                int numberOfReadBytes = 0;
                try
                {
                    numberOfReadBytes = client.EndReceive(ar);
                }
                catch
                {
                    numberOfReadBytes = 0;
                }

                if (numberOfReadBytes == 0)
                {
                    lock (this.clients)
                    {
                        this.clients.Remove(state);
                        RaiseClientDisconnected(state.Client);
                        return;
                    }
                }

                byte[] receivedBytes = new byte[numberOfReadBytes];
                Buffer.BlockCopy(
                  state.Buffer, 0,
                  receivedBytes, 0, numberOfReadBytes);
                RaiseDatagramReceived(client, receivedBytes);
                RaisePlaintextReceived(client, receivedBytes);
                state.Client.BeginReceive(state.Buffer
                    , 0
                    , WebSocketState.BufferSize
                    , SocketFlags.None
                    , HandleReceived
                    , state);
            }
        }

        #endregion

        #region Events

        /// <summary>接收到数据报文事件</summary>
        public event EventHandler<SocketReceivedEventArgs<byte[]>> DatagramReceived;
        /// <summary>接收到数据报文明文事件</summary>
        public event EventHandler<SocketReceivedEventArgs<string>> PlaintextReceived;

        private void RaiseDatagramReceived(Socket sender, byte[] datagram)
        {
            if (DatagramReceived != null)
            {
                DatagramReceived(this, new SocketReceivedEventArgs<byte[]>(sender, datagram));
            }
        }

        private void RaisePlaintextReceived(Socket sender, byte[] datagram)
        {
            if (PlaintextReceived != null)
            {
                PlaintextReceived(this, new SocketReceivedEventArgs<string>(
                  sender, this.Encoding.GetString(datagram, 0, datagram.Length)));
            }
        }

        /// <summary>与客户端的连接已建立事件</summary>
        public event EventHandler<SocketEventArgs> ClientConnected;
        /// <summary>与客户端的连接已断开事件</summary>
        public event EventHandler<SocketEventArgs> ClientDisconnected;

        private void RaiseClientConnected(Socket client)
        {
            if (ClientConnected != null)
            {
                ClientConnected(this, new SocketEventArgs(client));
            }
        }

        private void RaiseClientDisconnected(Socket client)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(this, new SocketEventArgs(client));
            }
        }

        #endregion

        #region Send
        private static void SendCallback(IAsyncResult ar)
        {
            try
            { 
                Socket handler = (Socket)ar.AsyncState;   
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("-> server.send.callback.error");
                Console.WriteLine(e.ToString());
            }
        }
        /// <summary>发送报文至指定的客户端</summary>
        /// <param name="client">客户端</param>
        /// <param name="datagram">报文</param>
        public void Send(Socket client, byte[] datagram)
        {
            try
            {
                client.BeginSend(datagram
                    , 0
                    , datagram.Length
                    , 0
                    , new AsyncCallback(SendCallback)
                    , client);
            }
            catch { }
        }

        /// <summary>发送报文至指定的客户端</summary>
        /// <param name="client">客户端</param>
        /// <param name="datagram">报文</param>
        public void Send(Socket client, string datagram)
        {
            Send(client, this.Encoding.GetBytes(datagram));
        }

        /// <summary>发送报文至所有客户端</summary>
        /// <param name="datagram">报文</param>
        public void SendAll(byte[] datagram)
        {
            for (int i = 0; i < this.clients.Count; i++)
            {
                Send(this.clients[i].Client, datagram);
            }
        }

        /// <summary>发送报文至所有客户端</summary>
        /// <param name="datagram">报文</param>
        public void SendAll(string datagram)
        {
            SendAll(this.Encoding.GetBytes(datagram));
        }

        #endregion

        #region IDisposable Members
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
