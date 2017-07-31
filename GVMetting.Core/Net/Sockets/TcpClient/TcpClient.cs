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
        /// 客户端与服务器之间的会话类 
        /// </summary> 
        private Session _session;

        /// <summary> 
        /// 客户端是否已经连接服务器 
        /// </summary> 
        private bool _isConnected = false;

        /// <summary> 
        /// 接收数据缓冲区大小 
        /// </summary> 
        public const int DefaultBufferSize = 1024 * 1024;

        /// <summary> 
        /// 接收数据缓冲区 
        /// </summary> 
        private byte[] _recvDataBuffer;

    } 
}
