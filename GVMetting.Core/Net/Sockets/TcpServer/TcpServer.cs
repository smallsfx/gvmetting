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
    /// 提供TCP连接服务的服务器类 
    /// 
    /// 特点: 
    /// 1.使用hash表保存所有已连接客户端的状态，收到数据时能实现快速查找.每当 
    /// 有一个新的客户端连接就会产生一个新的会话(Session).该Session代表了客 
    /// 户端对象. 
    /// 2.使用异步的Socket事件作为基础，完成网络通讯功能. 
    /// 3.支持带标记的数据报文格式的识别,以完成大数据报文的传输和适应恶劣的网 
    /// 络环境.初步规定该类支持的最大数据报文为640K(即一个数据包的大小不能大于 
    /// 640K,否则服务器程序会自动删除报文数据,认为是非法数据),防止因为数据报文 
    /// 无限制的增长而导致服务器崩溃 
    /// 4.通讯格式默认使用Encoding.Default格式这样就可以和以前32位程序的客户端 
    /// 通讯.也可以使用U-16和U-8的的通讯方式进行.可以在该DatagramResolver类的 
    /// 继承类中重载编码和解码函数,自定义加密格式进行通讯.总之确保客户端与服务 
    /// 器端使用相同的通讯格式 
    /// 5.使用C# native code,将来出于效率的考虑可以将C++代码写成的32位dll来代替 
    /// C#核心代码, 但这样做缺乏可移植性,而且是Unsafe代码(该类的C++代码也存在) 
    /// 6.可以限制服务器的最大登陆客户端数目 
    /// 7.比使用TcpListener提供更加精细的控制和更加强大异步数据传输的功能,可作为 
    /// TcpListener的替代类 
    /// 8.使用异步通讯模式,完全不用担心通讯阻塞和线程问题,无须考虑通讯的细节 
    /// 
    /// </summary> 
    public partial class TcpServer
    {
        /// <summary> 
        /// 默认的服务器最大连接客户端端数据 
        /// </summary> 
        public const int DefaultMaxClient = 100;

        /// <summary> 
        /// 接收数据缓冲区大小64K 
        /// </summary> 
        public const int DefaultBufferSize = 1024 * 1024;

        /// <summary>
        /// 服务器程序监听的IP地址
        /// </summary>
        private IPAddress _serverIP;
        /// <summary> 
        /// 服务器程序使用的端口 
        /// </summary> 
        private ushort _port;

        /// <summary> 
        /// 服务器程序允许的最大客户端连接数 
        /// </summary> 
        private ushort _maxClient;

        /// <summary> 
        /// 服务器的运行状态 
        /// </summary> 
        private bool _isRun;

        /// <summary> 
        /// 接收数据缓冲区 
        /// </summary> 
        private byte[] _recvDataBuffer;

        /// <summary> 
        /// 服务器使用的异步Socket类, 
        /// </summary> 
        private Socket _svrSock;

        /// <summary> 
        /// 保存所有客户端会话的哈希表 
        /// </summary> 
        private Hashtable _sessionTable;

        /// <summary> 
        /// 构造函数 
        /// </summary> 
        /// <param name="port">服务器端监听的端口号</param> 
        /// <param name="maxClient">服务器能容纳客户端的最大能力</param> 
        /// <param name="serverIP"></param>
        public TcpServer(IPAddress serverIP,ushort port, ushort maxClient)
        {
            _serverIP = serverIP;
            _port = port;
            _maxClient = maxClient;
        }
        public TcpServer(IPAddress serverIP, ushort port)
            : this(serverIP, port, 5)
        {
        }
    } 
}
