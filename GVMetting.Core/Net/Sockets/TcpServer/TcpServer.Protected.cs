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
        /// 关闭一个客户端Socket,首先需要关闭Session 
        /// </summary> 
        /// <param name="client">目标Socket对象</param> 
        /// <param name="exitType">客户端退出的类型</param> 
        protected virtual void CloseClient(Socket client, Session.ExitType exitType)
        {
            //查找该客户端是否存在,如果不存在,抛出异常 
            Session closeClient = FindSession(client);

            closeClient.TypeOfExit = exitType;

            if (closeClient != null)
                CloseSession(closeClient);
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        private void ReceiveData(Socket client)
        {
            _recvDataBuffer = new Byte[DefaultBufferSize];
            //继续接收数据 
            client.BeginReceive(_recvDataBuffer
                , 0
                , DefaultBufferSize
                , SocketFlags.None
                , new AsyncCallback(ReceiveData)
                , client);
        }

        /// <summary> 
        /// 客户端连接处理函数 
        /// </summary> 
        /// <param name="iar">欲建立服务器连接的Socket对象</param> 
        protected virtual void AcceptConn(IAsyncResult iar)
        {
            //继续接受客户端 
            _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);

            //如果服务器停止了服务,就不能再接收新的客户端 
            if (!_isRun) return;

            //接受一个客户端的连接请求 
            Socket client = _svrSock.EndAccept(iar);

            Session newSession = new Session(client);
            _sessionTable.Add(newSession.ID, newSession);

            //开始接受来自该客户端的数据 
            ReceiveData(client);

            //新的客户段连接,发出通知 
            if (ClientConn != null)
                ClientConn(this, new NetEventArgs(newSession));
        }

        /// <summary> 
        /// 通过Socket对象查找Session对象 
        /// </summary> 
        /// <param name="client"></param> 
        /// <returns>找到的Session对象,如果为null,说明并不存在该回话</returns> 
        private Session FindSession(Socket client)
        {
            SessionId id = new SessionId((int)client.Handle);

            return (Session)_sessionTable[id];
        }
        
        /// <summary> 
        /// 接受数据完成处理函数，异步的特性就体现在这个函数中， 
        /// 收到数据后，会自动解析为字符串报文 
        /// </summary> 
        /// <param name="iar">目标客户端Socket</param> 
        protected virtual void ReceiveData(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            try
            {
                int recv = client.EndReceive(iar);

                if (recv == 0)
                {
                    //正常的关闭 
                    CloseClient(client, Session.ExitType.NormalExit);
                }
                else
                {
                    Session session = FindSession(client);
                    session.Datagram = new Data(_recvDataBuffer);
                    //发布收到数据的事件 
                    if (RecvData != null)
                        RecvData(this, new NetEventArgs(session));

                    // 继续接收数据
                    ReceiveData(client);
                }
            }
            catch (SocketException ex)
            {
                //客户端退出 
                if (10054 == ex.ErrorCode)
                {
                    //客户端强制关闭 
                    CloseClient(client, Session.ExitType.ExceptionExit);
                }
            }
        }

        /// <summary> 
        /// 发送数据完成处理函数 
        /// </summary> 
        /// <param name="iar">目标客户端Socket</param> 
        protected virtual void SendDataEnd(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;

            int sent = client.EndSend(iar);
        }
    } 
}
