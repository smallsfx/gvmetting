using GVMetting.Media;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace GVMetting.WPFClient.SettingViews
{
    public partial class DeviceView : UserControl
    {
        public ObservableCollection<DeviceObject> VideoDevices { get; set; }
        public ObservableCollection<DeviceObject> AudioDevices { get; set; }

        public static readonly DependencyProperty SelectedVideoDeviceProperty =
           DependencyProperty.Register("SelectedVideoDevice"
           , typeof(DeviceObject)
           , typeof(DeviceView)
           , new PropertyMetadata(default(DeviceObject), SelectedVideoDevicePropertyChangedCallback));

        static void SelectedVideoDevicePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SystemSetting.Instance.VideoDevice = (e.NewValue as DeviceObject).Moniker;
        }
        public DeviceObject SelectedVideoDevice
        {
            get { return (DeviceObject)GetValue(SelectedVideoDeviceProperty); }
            set { SetValue(SelectedVideoDeviceProperty, value); }
        }

        public static readonly DependencyProperty SelectedAudioDeviceProperty =
           DependencyProperty.Register("SelectedAudioDevice"
           , typeof(DeviceObject)
           , typeof(DeviceView)
           , new PropertyMetadata(default(DeviceObject), SelectedAudioDevicePropertyChangedCallback));

        static void SelectedAudioDevicePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SystemSetting.Instance.AudioDevice = (e.NewValue as DeviceObject).Moniker;
        }

        public DeviceObject SelectedAudioDevice
        {
            get { return (DeviceObject)GetValue(SelectedAudioDeviceProperty); }
            set { SetValue(SelectedAudioDeviceProperty, value); }
        }

        private ICommand testAudioDevice;

        public ICommand TestAudioDevice
        {
            get
            {
                return this.testAudioDevice ?? (this.testAudioDevice = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        if (Audio.Instance.IsRunning)
                        {
                            Audio.Instance.DataReceived -= Audio_DataReceived;
                            Audio.Instance.Stop();
                        }
                        else
                        {

                            Audio.Instance.DataReceived += Audio_DataReceived;
                            Audio.Instance.Start();
                        }
                    }
                });
            }
        }
        private ICommand testVideoDevice;

        public ICommand TestVideoDevice
        {
            get
            {
                return this.testVideoDevice ?? (this.testVideoDevice = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        if (Video.Instance.IsRunning)
                        {
                            Video.Instance.DataReceived -= Video_DataReceived;
                            Video.Instance.Stop();
                            this.image.Source = null;
                        }
                        else
                        {
                            Video.Instance.DataReceived += Video_DataReceived;
                            Video.Instance.Start(this.SelectedVideoDevice.Moniker);
                        }
                    }
                });
            }
        }

        private ICommand refreshAudioDevice;

        public ICommand RefreshAudioDevice
        {
            get
            {
                return this.refreshAudioDevice ?? (this.refreshAudioDevice = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        this.RefreshAudioDevices();
                    }
                });
            }
        }
        private ICommand refreshVedioDevice;

        public ICommand RefreshVedioDevice
        {
            get
            {
                return this.refreshVedioDevice ?? (this.refreshVedioDevice = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        this.RefreshVideoDevices();
                    }
                });
            }
        }
        //public void

        public DeviceView()
        {
            this.AudioDevices = new ObservableCollection<DeviceObject>();
            this.VideoDevices = new ObservableCollection<DeviceObject>();
            InitializeComponent();

            this.DataContext = this;

            this.RefreshAudioDevices();
            this.RefreshVideoDevices();
            Audio.Instance.DataReceived += Audio_DataReceived;
        }

        void Audio_DataReceived(object sender, AudioEventArgs e)
        {
            //Console.WriteLine("Audio Input {0} Bytes", e.Data.Length);
            Audio.Instance.Output(e.Data);
        }

        void Video_DataReceived(object sender, VideoEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                image.Source = AV.ChangeBitmapToImageSource(e.Data);
            }));
        }

        private void RefreshAudioDevices()
        {
            this.AudioDevices.Clear();
            foreach (var devcie in DeviceTools.GetAudioDevices())
            {
                this.AudioDevices.Add(devcie);
            }
            this.SelectedAudioDevice = this.AudioDevices[0];
        }
        private void RefreshVideoDevices()
        {
            this.VideoDevices.Clear();
            foreach (var devcie in DeviceTools.GetVideoDevices())
            {
                this.VideoDevices.Add(devcie);
            }
            this.SelectedVideoDevice = this.VideoDevices[0];
        }
    }
}
