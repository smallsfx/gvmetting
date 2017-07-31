using NAudio.Wave;
using System;

namespace GVMetting.Media
{
    public class Audio
    {
        public event EventHandler<AudioEventArgs> DataReceived;
        private WaveFormat format = new WaveFormat(8000, 16, 1);
        private IWaveIn waveIn;
        private BufferedWaveProvider provider;
        private WaveOut waveOut; 

        /// <summary>连接音频输入设备</summary>
        public void Start()
        {
            if (this.IsRunning)
            {
                this.Stop();
            }
            this.IsRunning = true;
            if (waveIn != null) return;
            waveIn = new WaveIn { WaveFormat = format };//设置码率
            waveIn.DataAvailable += waveIn_DataAvailable;
            waveIn.RecordingStopped += waveIn_RecordingStopped;
            waveIn.StartRecording();

        }
        /// <summary>断开与音频输入设备的连接</summary>
        public void Stop()
        {
            if (this.IsRunning)
            {
                if (waveIn == null) return;
                try
                {
                    waveIn.StopRecording();
                    waveIn.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" -> audio.stop.error");
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    waveIn = null;
                }
            }
            this.IsRunning = false;            
        }

        // local -> 接收音频输入
        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                if (DataReceived != null)
                {
                    DataReceived(this, new AudioEventArgs(e.Buffer));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(" -> audio.available .error");
                Console.WriteLine(ex.ToString());
            }
        }
        // local -> 录音停止：异常
        private void waveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveIn != null) // 关闭录音对象
            {
                waveIn.Dispose();
                waveIn = null;
            }
            if (e.Exception != null)
            {
                Console.WriteLine(" -> audio.stop .error");
                Console.WriteLine(e.Exception.ToString());
            }
        }

        public bool IsRunning { get; private set; }

        public void Output(byte[] buffer)
        {
            try
            {
                if (provider == null)
                {
                    waveOut = new WaveOut();
                    provider = new BufferedWaveProvider(format);
                    provider.DiscardOnBufferOverflow = true;
                    waveOut.Init(provider);
                    waveOut.Play();
                }

                provider.AddSamples(buffer, 0, buffer.Length);
            }
            catch(Exception ex)
            {
                Console.WriteLine(" -> .audio.output.error");
                Console.WriteLine(ex.Message);
            }
        }

        private Audio()
        {
            this.IsRunning = false;
        }

        private static Audio instance;
        public static Audio Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Audio();
                }
                return instance;
            }
        }
    }
}
