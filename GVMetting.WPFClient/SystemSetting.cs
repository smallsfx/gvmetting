using System.ComponentModel;
using System.Runtime.InteropServices;

namespace GVMetting.WPFClient
{
    public class SystemSetting : ObservableObject
    {
        private string ip = string.Empty;
        public string IP
        {
            get { return this.ip; }
            set { ip = value; this.Notify("IP"); }
        }
        
        private string port = string.Empty;
        public string Port
        {
            get { return this.port; }
            set { this.port = value; this.Notify("Port"); }
        }

        private string audioDevice = string.Empty;
        public string AudioDevice
        {
            get { return this.audioDevice; }
            set { this.audioDevice = value; this.Notify("AudioDevice"); }
        }

        private string videoDevice = string.Empty;
        public string VideoDevice
        {
            get { return this.videoDevice; }
            set { this.videoDevice = value; this.Notify("VideoDevice"); }
        }

        private SystemSetting() {
            this.port = "12345";
            this.ip = "127.0.0.1";//"182.92.1.223";//
        }

        static SystemSetting instance;
        public static SystemSetting Instance
        {
            get
            {
                if (instance == null)
                    instance = new SystemSetting();
                return instance;
            }
        }
    }

    [ComVisible(false)]
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
