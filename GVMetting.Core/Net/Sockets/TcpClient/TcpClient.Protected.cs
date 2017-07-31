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
    /// <summary> 
    /// 提供Tcp网络连接服务的客户端类 
    /// 
    /// 原理: 
    /// 1.使用异步Socket通讯与服务器按照一定的通讯格式通讯,请注意与服务器的通 
    /// 讯格式一定要一致,否则可能造成服务器程序崩溃,整个问题没有克服,怎么从byte[] 
    /// 判断它的编码格式 
    /// 2.支持带标记的数据报文格式的识别,以完成大数据报文的传输和适应恶劣的网 
    /// 络环境. 
    /// </summary> 
    public partial class TcpClient
    {
        /// <summary> 
        /// 数据发送完成处理函数 
        /// </summary> 
        /// <param name="iar"></param> 
        protected virtual void SendDataEnd(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int sent = remote.EndSend(iar);
        }

        /// <summary> 
        /// 建立Tcp连接后处理过程 
        /// </summary> 
        /// <param name="iar">异步Socket</param> 
        protected virtual void Connected(IAsyncResult iar)
        {
            Socket socket = (Socket)iar.AsyncState;

            socket.EndConnect(iar);

            //创建新的会话 
            _session = new Session(socket);

            _isConnected = true;

            //触发连接建立事件 
            if (ConnectedServer != null)
                ConnectedServer(this, new NetEventArgs(_session));

            ReceiveData();
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        private void ReceiveData()
        {
            _recvDataBuffer = new Byte[DefaultBufferSize];
            //继续接收数据 
            _session.ClientSocket.BeginReceive(_recvDataBuffer
                , 0
                , DefaultBufferSize
                , SocketFlags.None
                , new AsyncCallback(RecvData)
                , _session.ClientSocket);
        }

        /// <summary> 
        /// 数据接收处理函数 
        /// </summary> 
        /// <param name="iar">异步Socket</param> 
        protected virtual void RecvData(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            try
            {
                int recv = remote.EndReceive(iar);

                _session.Datagram = new Data(_recvDataBuffer);
                //正常的退出 
                if (recv == 0)
                {
                    _session.TypeOfExit = Session.ExitType.NormalExit;

                    if (DisConnectedServer != null)
                        DisConnectedServer(this, new NetEventArgs(_session));
                }
                else
                {
                    //通过事件发布收到的报文 
                    if (ReceivedDatagram != null)
                        ReceivedDatagram(this, new NetEventArgs(_session));

                    ReceiveData();
                }
            }
            catch (SocketException ex)
            {
                //客户端退出 
                if (10054 == ex.ErrorCode)
                {
                    //服务器强制的关闭连接，强制退出 
                    _session.TypeOfExit = Session.ExitType.ExceptionExit;

                    if (DisConnectedServer != null)
                        DisConnectedServer(this, new NetEventArgs(_session));
                }
            }
            catch (Exception eex)
            {
                Console.WriteLine(eex.ToString());
            }
        }
    }
}