using System;

namespace GVMetting.Media
{
    public class AudioEventArgs : EventArgs
    {
        public AudioEventArgs(byte[] data)
        {
            this.Data = data;
        }
        public byte[] Data { get; private set; }
    }
}
