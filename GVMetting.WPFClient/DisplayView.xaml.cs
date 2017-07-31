using GVMetting.Media;
using NAudio.Wave;
using System;
using System.Drawing;
using System.Windows.Controls;

namespace GVMetting.WPFClient
{
    public partial class DisplayView : UserControl
    {
        public DisplayView()
        {
            InitializeComponent();
        }

        #region 视频输出 VideoOutput(Bitmap bitmap)
        public void VideoOutput(Bitmap bitmap)
        {
            try
            {
                image.Source = AV.ChangeBitmapToImageSource(bitmap).Clone();
            }
            catch (Exception ex)
            {
                Console.WriteLine(" -> .video.output.error");
                Console.WriteLine(ex.Message);
            }
        }
        #endregion //#region 视频输出 VideoOutput(Bitmap bitmap)

        #region 音频输出 AudioOutput(byte[] buffer)
        public void AudioOutput(byte[] buffer)
        {
            try
            {
                Audio.Instance.Output(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(" -> .audio.output.error");
                Console.WriteLine(ex.Message);
            }
        }
        #endregion //#region 音频输出 AudioOutput(byte[] buffer)
    }
}
