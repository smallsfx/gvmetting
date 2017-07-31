using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace GVMetting.Core.Net.Sockets
{

    public class Data
    {
        public Command Command { get; set; }
        public byte[] Body { get; set; }
        public byte[] ByteBody { get; set; }
        public DateTime Time { get; set; }
        public string Sender { get; set; }
        public string SinglePath { get; set; }

        public Data() : this(null) 
        {
            Command = Command.Null;
            //Body = "";
            Time = DateTime.Now;
            Sender = "";
            SinglePath = "";
        }
        public Data(byte[] data)
        {
            if (data != null)
            {
                int index=0;
                Command = (Command)BitConverter.ToInt32(data, index);

                index += 4;
                int bodylen = BitConverter.ToInt32(data, index);
                index += 4;

                if (Command == Command.ResponseFile)
                {
                    ByteBody = new byte[bodylen];
                    for (int i = index; i < bodylen; i++)
                        ByteBody[i-index] = data[i];
                }
                else
                {
                    Body = new byte[bodylen];
                    Array.Copy(data, index, Body,0,bodylen);
                    //Body = Encoding.UTF8.GetString(data, index, bodylen);
                }
                index += bodylen;
                int timelen = BitConverter.ToInt32(data, index);

                index += 4;
                Time = DateTime.Parse(Encoding.UTF8.GetString(data, index,timelen));

                index += timelen;
                int sendlen = BitConverter.ToInt32(data, index);

                index += 4;
                Sender = Encoding.UTF8.GetString(data, index, sendlen);

                index += sendlen;
                int singlepathlen = BitConverter.ToInt32(data, index);

                index += 4;
                SinglePath = Encoding.UTF8.GetString(data, index, singlepathlen);
            }
        }
        public byte[] ToByte()
        {
            Console.WriteLine(ToString());
            byte[] bytes=null;
            try
            {
                List<byte> result = new List<byte>();
                result.AddRange(BitConverter.GetBytes((int)Command));

                if (Command == Command.ResponseFile)
                {
                    result.AddRange(BitConverter.GetBytes(ByteBody.Length));
                    result.AddRange(ByteBody);
                }
                else
                {
                    byte[] bodyByte = Body;
                    result.AddRange(BitConverter.GetBytes(bodyByte.Length));
                    result.AddRange(bodyByte);
                }

                byte[] timeByte = Encoding.UTF8.GetBytes(Time.ToString());
                result.AddRange(BitConverter.GetBytes(timeByte.Length));
                result.AddRange(timeByte);

                byte[] sendByte = Encoding.UTF8.GetBytes(Sender);
                result.AddRange(BitConverter.GetBytes(sendByte.Length));
                result.AddRange(sendByte);

                byte[] singlePathByte = Encoding.UTF8.GetBytes(SinglePath);
                result.AddRange(BitConverter.GetBytes(singlePathByte.Length));
                result.AddRange(singlePathByte);

                bytes = result.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return bytes;
        }

        public override string ToString()
        {
            string result = string.Format("Command:{0}, Body:{1}, DateTime:{2}, Sender:{3}, SinglePath:{4}"
                , Command
                , Body
                , Time.ToString("yyyy-MM-dd HH:mi:ss")
                , Sender
                , SinglePath);
            return result;
        }

    }

    public enum Command
    {
        /// <summary>
        /// 
        /// </summary>
        Null = 0,
        /// <summary>
        /// 
        /// </summary>
        Login=1,
        /// <summary>
        /// 
        /// </summary>
        Logout=2,
        /// <summary>
        /// 发送消息
        /// </summary>
        Message=3,
        /// <summary>
        /// 请求文件列表。
        /// </summary>
        List=4,
        /// <summary>
        /// 请求单个文件，以供下载。
        /// </summary>
        RequestFile=5,
        /// <summary>
        /// 返回单个文件。
        /// </summary>
        ResponseFile=6,
        /// <summary>音频</summary>
        Audio = 7,
        /// <summary>视频</summary>
        Video = 8,
        /// <summary>客户端列表</summary>
        ClientCount=9
    }
}