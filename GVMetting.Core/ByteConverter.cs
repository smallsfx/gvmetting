using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
namespace GVMetting.Core
{
    public class ByteHelper
    {
        const string KEY_AUDIO = "a";
        const string KEY_TITLE = "t";
        const string KEY_VIDEO = "v";
        // 图像大小决定传输效率
        const int IMAGE_WIDTH = 320;
        const int IMAGE_HEIGHT = 240;

        //public static Message Parse(byte[] bytes)
        //{
        //    Message data = new Message();

        //    int index = 0;
        //    while (index<bytes.Length-1)
        //    {
        //        int length = BitConverter.ToInt32(bytes, index);
        //        index += 4;
        //        string key = Encoding.Default.GetString(bytes, index, length);
        //        index += length;

        //        length = BitConverter.ToInt32(bytes, index);
        //        index += 4;
        //        var temp = new byte[length];
        //        Array.Copy(bytes, index, temp, 0, temp.Length);
        //        index += length;
        //        switch (key)
        //        {
        //            case KEY_TITLE:
        //                data.Sender = Encoding.Default.GetString(temp);
        //                break;
        //            case KEY_AUDIO:
        //                data.Audio = temp;
        //                break;
        //            case KEY_VIDEO:
        //                data.Video = Byte2Bitmap(temp);
        //                break;
        //            default:
        //                if (data.Metas.ContainsKey(key))
        //                {
        //                    data.Metas[key] = temp;
        //                }
        //                else
        //                {
        //                    data.Metas.Add(key, temp);
        //                }
        //                break;
        //        }
        //    }

        //    return data;
        //}
        //public static byte[] BuildAudio(byte[] audio)
        //{
        //    Message data = new Message();
        //    data.Audio = audio;

        //    return Parse(data);
        //}

        //public static byte[] BuildVideo(Bitmap video)
        //{
        //    Message data = new Message();
        //    data.Video = SizeImage(video, IMAGE_WIDTH, IMAGE_HEIGHT);
        //    return Parse(data);
        //}

        //public static byte[] Parse(Message data)
        //{
        //    List<byte> bytes = new List<byte>();
        //    byte[] temp, templen;
        //    if (string.IsNullOrEmpty(data.Sender) == false)
        //    {
        //        temp = Encoding.Default.GetBytes(KEY_TITLE);
        //        templen = BitConverter.GetBytes(temp.Length);
        //        bytes.AddRange(templen);
        //        bytes.AddRange(temp);
        //        temp = Encoding.Default.GetBytes(data.Sender);
        //        templen = BitConverter.GetBytes(temp.Length);
        //        bytes.AddRange(templen);
        //        bytes.AddRange(temp);
        //    }

        //    if (data.Audio != null && data.Audio.Length > 0)
        //    {
        //        temp = Encoding.Default.GetBytes(KEY_AUDIO);
        //        templen = BitConverter.GetBytes(temp.Length);
        //        bytes.AddRange(templen);
        //        bytes.AddRange(temp);
        //        templen = BitConverter.GetBytes(data.Audio.Length);
        //        bytes.AddRange(templen);
        //        bytes.AddRange(data.Audio);
        //    }

        //    if (data.Video != null)
        //    {
        //        temp = Encoding.Default.GetBytes(KEY_VIDEO);
        //        templen = BitConverter.GetBytes(temp.Length);
        //        bytes.AddRange(templen);
        //        bytes.AddRange(temp);

        //        temp = Bitmap2Byte(data.Video);
        //        templen = BitConverter.GetBytes(temp.Length);
        //        bytes.AddRange(templen);
        //        bytes.AddRange(temp);
        //    }

        //    if( data.Metas != null && data.Metas.Count >0)
        //    {
        //        foreach (var item in data.Metas)
        //        {
        //            temp = Encoding.Default.GetBytes(item.Key);
        //            templen = BitConverter.GetBytes(temp.Length);
        //            bytes.AddRange(templen);
        //            bytes.AddRange(temp);

        //            templen = BitConverter.GetBytes(item.Value.Length);
        //            bytes.AddRange(templen);
        //            bytes.AddRange(item.Value);
        //        }
        //    }
        //    return bytes.ToArray();
        //}

        //public static byte[] Append(byte[] bytes,string key, string value)
        //{
        //    var temp = Encoding.Default.GetBytes(key);
        //    var templen = BitConverter.GetBytes(temp.Length);
        //    var list = bytes.ToList();
        //    list.AddRange(templen);
        //    list.AddRange(temp);
        //    temp = Encoding.Default.GetBytes(value);
        //    templen = BitConverter.GetBytes(temp.Length);
        //    list.AddRange(templen);
        //    list.AddRange(temp);
        //    return list.ToArray();
        //}

        //public static byte[] AppendFrom(byte[] bytes, string from)
        //{
        //    return Append(bytes,KEY_TITLE,from);
        //}
        
        public static byte[] Bitmap2Byte(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Jpeg);
                byte[] data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
                return data;
            }
        }
        public static Bitmap Byte2Bitmap(byte[] bytes)
        {
            byte[] bytelist = bytes;
            using (MemoryStream stream = new MemoryStream(bytelist))
            {
                return (Bitmap)Image.FromStream(stream);
            }
        }
        public static Bitmap SizeImage(Image srcImage
            , int iWidth = IMAGE_WIDTH
            , int iHeight = IMAGE_HEIGHT)
        {
            try
            {
                // 要保存到的图片
                Bitmap b = new Bitmap(iWidth, iHeight);
                Graphics g = Graphics.FromImage(b);
                // 插值算法的质量
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(srcImage, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(0, 0, srcImage.Width, srcImage.Height), GraphicsUnit.Pixel);
                g.Dispose();
                return b;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static byte[] Compress(byte[] arrbts)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    using (GZipStream zipStream =
                        new GZipStream(ms, CompressionMode.Compress))
                    {
                        zipStream.Write(arrbts, 0, arrbts.Length);
                    }
                }
                catch { }
                return ms.ToArray();
            }
        }
        public static byte[] DeCompress(byte[] arrbts)
        {
            using (MemoryStream ms = new MemoryStream(arrbts))
            {
                using (GZipStream zipStream =
                    new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (MemoryStream UnzipStream = new MemoryStream())
                    {
                        try
                        {

                            byte[] sDecompressed = new byte[128];
                            while (true)
                            {
                                int bytesRead = zipStream.Read(sDecompressed, 0, 128);
                                if (bytesRead == 0)
                                {
                                    break;
                                }
                                UnzipStream.Write(sDecompressed, 0, bytesRead);
                            }
                        }
                        catch { }
                        return UnzipStream.ToArray();
                    }
                }
            }
        }
    }
}
