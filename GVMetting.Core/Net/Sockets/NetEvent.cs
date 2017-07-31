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
    /// 网络通讯事件模型委托 
    /// </summary> 
    public delegate void NetEvent(object sender, NetEventArgs e); 

    /// <summary> 
    /// 服务器程序的事件参数,包含了激发该事件的会话对象 
    /// </summary> 
    public class NetEventArgs : EventArgs
    {
        /// <summary> 
        /// 客户端与服务器之间的会话 
        /// </summary> 
        private Session _client;

        /// <summary> 
        /// 构造函数 
        /// </summary> 
        /// <param name="client">客户端会话</param> 
        public NetEventArgs(Session client)
        {
            _client = client;
        }

        /// <summary> 
        /// 获得激发该事件的会话对象 
        /// </summary> 
        public Session Client
        {
            get
            {
                return _client;
            }
        }
    } 
}
