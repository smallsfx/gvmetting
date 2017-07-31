using GVMetting.Core;
using GVMetting.Core.Net;
using System;
namespace GVMetting.CommonServer
{
    public class ServiceCore
    {
        const int TCP_SERVER_PORT = 12345;
        AsyncTcpServer server;
        private ServiceCore()
        {
            Console.Write("监听服务初始化");
            server = new AsyncTcpServer(TCP_SERVER_PORT);
            server.Encoding = System.Text.Encoding.UTF8;
            server.ClientConnected += server_Connected;
            server.ClientDisconnected += server_Disconnected;
            server.DatagramReceived += server_DatagramReceived;
            Console.WriteLine(" -> 完成.");
        }

        /// <summary>启动监听服务</summary>
        public bool StartListener()
        {
            try
            {
                Console.Write("启动监听服务");
                server.Start();
                Console.WriteLine(" -> 完成.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" -> listener.start.error.");
                Console.WriteLine(ex.Message);
            }
            return false;
        }
        /// <summary>停止监听服务</summary>
        public bool StopListener()
        {
            try
            {
                Console.Write("停止监听服务");
                server.Stop();
                Console.WriteLine(" -> 完成");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" -> listener.stop.error.");
                Console.WriteLine(ex.Message);
            }
            return false;
        }
        
        void server_DatagramReceived(object sender, ReceivedEventArgs<byte[]> e)
        {
            try
            {
                var value = e.TcpClient;
                var key = value.Client.RemoteEndPoint.ToString();
                var message = new Message(e.Datagram);
                message.Sender = key;
                var datas = message.ToByte();
                //var data = ByteHelper.AppendFrom(e.Datagram,key);
                Console.Write(string.Format("{1} {0} -> {2} byte"
                    , key, DateTime.Now.ToString("hh:mm:ss"), datas.Length));
                server.SendAll(datas);
                Console.WriteLine(" -> ok.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(" -> server.receive.error.");
                Console.WriteLine(ex.Message);
            }
        }
        void server_Connected(object sender, ClientEventArgs e)
        {
            var key = e.TcpClient.Client.RemoteEndPoint.ToString();
            var data = new Message();
            data.Command = Command.Login;
            data.Sender = key;
            data.Append("uc", server.ClientCount.ToString());
            server.SendAll(data.ToByte());
            Console.WriteLine("TCP 客户端 {0} 已经连接.", key);
        }
        void server_Disconnected(object sender, ClientEventArgs e)
        {
            var key = e.TcpClient.Client.RemoteEndPoint.ToString();
            var data = new Message();
            data.Command = Command.Logout;
            data.Sender = key;
            data.Append("uc", server.ClientCount.ToString());
            server.SendAll(data.ToByte());
            Console.WriteLine("TCP 客户端 {0} 已经断开连接.", key);
        }

        static ServiceCore instance;
        public static ServiceCore Instance
        {
            get
            {
                if (instance == null)
                    instance = new ServiceCore();
                return instance;
            }
        }

    }
}
