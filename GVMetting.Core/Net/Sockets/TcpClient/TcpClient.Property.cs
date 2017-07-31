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
        /// 返回客户端与服务器之间的会话对象 
        /// </summary> 
        public Session ClientSession
        {
            get
            {
                return _session;
            }
        }

        /// <summary> 
        /// 返回客户端与服务器之间的连接状态 
        /// </summary> 
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }
    }
}