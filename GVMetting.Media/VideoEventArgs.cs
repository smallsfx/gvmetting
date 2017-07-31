using System;
using System.Drawing;

namespace GVMetting.Media
{
    public class VideoEventArgs : EventArgs
    {
        public VideoEventArgs(Bitmap bitmap)
        {
            this.Data = bitmap;
        }
        public Bitmap Data { get; private set; }
    }
}
