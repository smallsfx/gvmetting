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
        /// 服务器的Socket对象 
        /// </summary> 
        public Socket ServerSocket
        {
            get
            {
                return _svrSock;
            }
        }

        /// <summary> 
        /// 客户端会话数组,保存所有的客户端,不允许对该数组的内容进行修改 
        /// </summary> 
        public Hashtable SessionTable
        {
            get
            {
                return _sessionTable;
            }
        }

        /// <summary> 
        /// 服务器可以容纳客户端的最大能力 
        /// </summary> 
        public int Capacity
        {
            get
            {
                return _maxClient;
            }
        }

        /// <summary> 
        /// 服务器运行状态 
        /// </summary> 
        public bool IsRun
        {
            get
            {
                return _isRun;
            }

        }
    } 
}
