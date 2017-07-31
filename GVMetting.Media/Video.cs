using AForge.Video.DirectShow;
using System;

namespace GVMetting.Media
{
    public class Video
    {
        VideoCaptureDevice vsp;
        public event EventHandler<VideoEventArgs> DataReceived;
        /// <summary>连接视频输入设备</summary>
        public void Start(string device)
        {
            if (this.IsRunning)
            {
                this.Stop();
            }
            // 默认设备  
            vsp = new VideoCaptureDevice(device);
            vsp.NewFrame += VideoSource_NewFrame;
            // 开启视频  
            vsp.Start();

            this.IsRunning = true;
        }
        /// <summary>断开与视频输入设备的连接</summary>
        public void Stop()
        {
            if (this.IsRunning && vsp.IsRunning)
            {   // 停止视频  
                vsp.SignalToStop();
                vsp.WaitForStop();
            }
            this.IsRunning = false;
        }
        // local -> 接收视频输入
        void VideoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                if (DataReceived != null)
                {
                    DataReceived(this, new VideoEventArgs(eventArgs.Frame));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(" -> video.available .error");
                Console.WriteLine(ex.ToString());
            }
        }

        public bool IsRunning { get;private set; }

        private Video()
        {
            this.IsRunning = false;
        }

        private static Video instance;
        public static Video Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Video();
                }
                return instance;
            }
        }
    }
}
