///////////////////////////////////////////////////////
//NSTCPFramework
//版本：1.0.0.1
//////////////////////////////////////////////////////
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Reflection;

namespace GVMetting.Core.Net.Sockets
{
    
    public partial class TcpServer
    {
        /// <summary> 
        /// 启动服务器程序,开始监听客户端请求 
        /// </summary> 
        public virtual void Start()
        {
            if (!_isRun)
            {

                _sessionTable = new Hashtable(53);

                _recvDataBuffer = new byte[DefaultBufferSize];

                //初始化socket 
                _svrSock = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                //绑定端口 
                IPEndPoint iep = new IPEndPoint(_serverIP, _port);
                _svrSock.Bind(iep);

                //开始监听 
                _svrSock.Listen(5);

                //设置异步方法接受客户端连接 
                _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);

                _isRun = true;
            }
        }

        /// <summary> 
        /// 停止服务器程序,所有与客户端的连接将关闭 
        /// </summary> 
        public virtual void Stop()
        {
            if (_isRun)
            {

                //这个条件语句，一定要在关闭所有客户端以前调用 
                //否则在EndConn会出现错误 
                _isRun = false;

                //关闭数据连接,负责客户端会认为是强制关闭连接 
                if (_svrSock.Connected)
                {
                    _svrSock.Shutdown(SocketShutdown.Both);
                }

                CloseAllClient();

                //清理资源 
                _svrSock.Close();

                _sessionTable = null;
            }
        }


        /// <summary> 
        /// 关闭所有的客户端会话,与所有的客户端连接会断开 
        /// </summary> 
        public virtual void CloseAllClient()
        {
            foreach (Session client in _sessionTable.Values)
            {
                client.Close();
            }

            _sessionTable.Clear();
        }


        /// <summary> 
        /// 关闭一个与客户端之间的会话 
        /// </summary> 
        /// <param name="closeClient">需要关闭的客户端会话对象</param> 
        public virtual void CloseSession(Session closeClient)
        {
            Debug.Assert(closeClient != null);

            if (closeClient != null)
            {
                closeClient.Datagram = null;

                _sessionTable.Remove(closeClient.ID);

                //客户端强制关闭链接 
                if (ClientClose != null)
                    ClientClose(this, new NetEventArgs(closeClient));

                closeClient.Close();
            }
        }


        /// <summary> 
        /// 发送数据 
        /// </summary> 
        /// <param name="recvDataClient">接收数据的客户端会话</param> 
        /// <param name="datagram">数据报文</param> 
        public virtual void SendText(Session recvDataClient, Data datagram)
        {
            //获得数据编码 
            byte[] data = datagram.ToByte();

            recvDataClient.ClientSocket.BeginSend(data
                , 0
                , data.Length
                , SocketFlags.None
                , new AsyncCallback(SendDataEnd)
                , recvDataClient.ClientSocket);
        }
    } 
}
