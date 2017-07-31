using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GVMetting.Core
{
    public class Message
    {
        public string Sender { get; set; }
        public Command Command { get; set; }
        //public byte[] Audio { get; set; }
        //public Bitmap Video { get; set; }
        public byte[] Body { get; private set; }
        public Dictionary<string, byte[]> Metas { get; set; }
        public Message() : this(null) { }
        public Message(byte[] data)
        {
            this.Metas = new Dictionary<string, byte[]>();
            this.StringBody = string.Empty;
            this.Sender = string.Empty;
            if (data != null)
            {
                this.parse(data);
            }
        }

        public string this[string key]
        {
            get
            {
                if (this.Metas.ContainsKey(key))
                {
                    return Encoding.Default.GetString(this.Metas[key]);
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (this.Metas.ContainsKey(key))
                {
                    this.Metas[key] = Encoding.Default.GetBytes(value);
                }
                else
                {
                    this.Metas.Add(key, Encoding.Default.GetBytes(value));
                }
            }
        }
        public void Append(string key, string value)
        {
            Metas.Add(key, Encoding.Default.GetBytes(value));
        }


        #region Body 类型转换
        public Bitmap BitmapBody
        {
            get
            {
                if (this.Body.Length == 0)
                {
                    return null;
                }
                return ByteHelper.Byte2Bitmap(this.Body);
            }
            set
            {
                this.Body = ByteHelper.Bitmap2Byte(value);
            }
        }
        public string StringBody
        {
            get
            {

                if (this.Body.Length == null)
                {
                    return string.Empty;
                }
                return Encoding.Default.GetString(this.Body);
            }
            set
            {
                this.Body = Encoding.Default.GetBytes(value);
            }
        }
        #endregion //#region Body 类型转换

        #region 协议转换
        private void parse(byte[] data)
        {
            int index = 0;
            // Command
            this.Command = (Command)BitConverter.ToInt32(data, index);
            index += 4;
            // Sender Length
            int length = BitConverter.ToInt32(data, index);
            index += 4;
            // Sender
            this.Sender = Encoding.Default.GetString(data, index, length);
            index += length;
            // Body Length
            length = BitConverter.ToInt32(data, index);
            index += 4;
            // Body
            this.Body = new byte[length];
            Array.Copy(data, index, this.Body, 0, length);
            index += length;
            // Metas
            while (index < data.Length - 1)
            {
                // Meta Key Length
                length = BitConverter.ToInt32(data, index);
                index += 4;
                // Meta Key
                string key = Encoding.Default.GetString(data, index, length);
                index += length;
                // Meta Value Length
                length = BitConverter.ToInt32(data, index);
                index += 4;
                // Meta Value
                var temp = new byte[length];
                Array.Copy(data, index, temp, 0, length);
                index += length;

                if (this.Metas.ContainsKey(key))
                {
                    this.Metas[key] = temp;
                }
                else
                {
                    this.Metas.Add(key, temp);
                }
            }
        }

        public byte[] ToByte()
        {
            List<byte> bytes = new List<byte>();
            byte[] temp, templen;
            // Command
            temp = BitConverter.GetBytes((int)this.Command);
            bytes.AddRange(temp);
            // Sender Length / Sender
            temp = Encoding.Default.GetBytes(this.Sender);
            templen = BitConverter.GetBytes(temp.Length);
            bytes.AddRange(templen);
            bytes.AddRange(temp);

            // Body Length / Body
            templen = BitConverter.GetBytes(this.Body.Length);
            bytes.AddRange(templen);
            bytes.AddRange(this.Body);

            // Metas
            if (this.Metas.Count > 0)
            {
                foreach (var item in this.Metas)
                {
                    temp = Encoding.Default.GetBytes(item.Key);
                    templen = BitConverter.GetBytes(temp.Length);
                    bytes.AddRange(templen);
                    bytes.AddRange(temp);

                    templen = BitConverter.GetBytes(item.Value.Length);
                    bytes.AddRange(templen);
                    bytes.AddRange(item.Value);
                }
            }

            return bytes.ToArray();
        }
        #endregion //#region 协议转换


        public static Message CreateVideo(Bitmap video)
        {
            Message message = new Message();
            message.Command = Command.Video;
            message.BitmapBody = ByteHelper.SizeImage(video);
            return message;
        }
        public static Message CreateAudio(byte[] audio)
        {
            Message message = new Message();
            message.Command = Command.Audio;
            message.Body = audio;
            return message;
        }
    }

    public enum Command
    {
        Login=1,Logout=0,
        Text=101,Audio=102,Video=103,
        List=201
    }
}
