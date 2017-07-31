using GVMetting.Core;
using GVMetting.Core.Net;
using GVMetting.Media;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
namespace GVMetting.WPFClient
{
    public partial class MainWindow : MetroWindow
    {
        Dictionary<string, DisplayView> dispalyViewBuffer;
        AsyncTcpClient client;

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
            this.InitializeView();
            dispalyViewBuffer = new Dictionary<string, DisplayView>();
            Audio.Instance.DataReceived += AudioE_DataReceived;
            Video.Instance.DataReceived += Video_DataReceived;
        }
        // 接受视频输入
        void Video_DataReceived(object sender, VideoEventArgs e)
        {
            if (client != null && client.Connected)
            {
                client.Send(Message.CreateVideo(e.Data).ToByte());
            }
        }
        // 接收音频输入
        void AudioE_DataReceived(object sender, AudioEventArgs e)
        {
            if (client != null && client.Connected)
            {
                client.Send(Message.CreateAudio(e.Data).ToByte());
            }
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            Audio.Instance.Stop();
            Video.Instance.Stop();
            if (client != null && client.Connected)
            {
                this.DisConnect();
            }
        }

        void client_ServerException(object sender, ExceptionEventArgs e)
        {
            Console.WriteLine("-> client.server.exception");
            Console.WriteLine(e.Exception.ToString());
        }
        // 与服务端断开连接 -> 关闭摄像头及麦克风
        void client_Disconnected(object sender, ServerEventArgs e)
        {
            Console.WriteLine("{0} -> client.server.disconnect"
                , DateTime.Now.ToString("hh:mm:ss"));
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.InitializeView();
            }));
        }
        // 与服务端取得连接 -> 打开摄像头及麦克风
        void client_Connected(object sender, ServerEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.barserverState.Content = "已连接";
                Console.WriteLine("-> client.server.connect");
                Audio.Instance.Start();
                Video.Instance.Start(SystemSetting.Instance.VideoDevice);
            }));
        }
        // 解析服务器发送的内容
        void client_DatagramReceived(object sender, ReceivedEventArgs<byte[]> e)
        {
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.DoWork += (_sender, _e) =>
                {
                    _e.Result = _e.Argument;
                };
                bw.RunWorkerCompleted += (_sender, _e) =>
                {
                    if (client == null || !client.Connected)
                    {
                        return;
                    }
                    try
                    {
                        DisplayView ctrl = null;
                        var message = new Message(_e.Result as byte[]);
                        switch (message.Command)
                        {
                            case Command.Logout:
                                dispalyViewBuffer.Remove(message.Sender);
                                container.Children.Remove(ctrl);
                                this.RefreshLayout();

                                barPersonCount.Content = message["uc"];
                                break;
                            case Command.Login:
                                if (dispalyViewBuffer.ContainsKey(message.Sender))
                                {
                                    ctrl = dispalyViewBuffer[message.Sender];
                                }
                                else
                                {
                                    ctrl = new DisplayView();
                                    ctrl.MouseDoubleClick += view_MouseDoubleClick;
                                    container.Children.Add(ctrl);
                                    dispalyViewBuffer.Add(message.Sender, ctrl);
                                    this.RefreshLayout();
                                }
                                barPersonCount.Content = message["uc"];
                                break;
                            case Command.Audio:
                                if (dispalyViewBuffer.ContainsKey(message.Sender))
                                {
                                    ctrl = dispalyViewBuffer[message.Sender];
                                    ctrl.AudioOutput(message.Body);
                                }
                                break;
                            case Command.Video:
                                if (dispalyViewBuffer.ContainsKey(message.Sender))
                                {
                                    ctrl = dispalyViewBuffer[message.Sender];
                                    ctrl.VideoOutput(message.BitmapBody);
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(" -> .received .error");
                        Console.WriteLine(ex.ToString());
                    }
                };
                bw.RunWorkerAsync(e.Datagram);
            }
        }

        #region TCP | Connect & DisConnect
        /// <summary>获取当前用户是否加入会议</summary>
        public bool IsJoined { get { return client != null; } }
        /// <summary>使用当前用户加入会议</summary>
        public void Connect()
        {
            try
            {
                Console.WriteLine("-> client.connect ->");
                client = new AsyncTcpClient(SystemSetting.Instance.IP
                    , Int32.Parse(SystemSetting.Instance.Port));
                client.DatagramReceived += client_DatagramReceived;
                client.ServerConnected += client_Connected;
                client.ServerDisconnected += client_Disconnected;
                client.ServerException += client_ServerException;
                client.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show("服务端IP地址或端口输入错误，请核实后重新连接！");
            }
        }
        /// <summary>退出当前加入的会议</summary>
        public void DisConnect()
        {
            if (client == null) { return; }
            try
            {
                Console.WriteLine("-> client.disconnect ->");
                client.Close();
                client.Dispose();
            }
            finally
            {
                client = null;

                Console.WriteLine("{0} -> client.server.disconnect = AudioIsRunning"
                    , DateTime.Now.ToString("hh:mm:ss"));
                Audio.Instance.Stop();
                Console.WriteLine("{0} -> client.server.disconnect = VideoIsRunning"
                    , DateTime.Now.ToString("hh:mm:ss"));
                Video.Instance.Stop();
            }
        }
        #endregion //#region TCP | Connect & DisConnect

        #region UI
        private void InitializeView()
        {
            this.barlocalIP.Content = "";
            this.barPersonCount.Content = "0";
            this.barserverState.Content = "未连接";
        }
        private void OnJoinMetting(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (this.IsJoined)
            {
                button.Content = "加入会议";
                this.DisConnect();
            }
            else
            {
                button.Content = "退出会议";
                this.Connect();
            }
        }

        private void OnSetting(object sender, RoutedEventArgs e)
        {
            settingsFlyout.IsOpen = !settingsFlyout.IsOpen;
        }

        private void TancfsToAll()
        {
            Storyboard story = new Storyboard();
            int count = container.Children.Count;
            double span = 5;
            double sqrt = Math.Sqrt(count);
            if (sqrt.ToString().IndexOf('.') >= 0) sqrt = (int)sqrt + 1;
            double columns = sqrt;
            double rows = sqrt;
            while (rows * columns >= count + columns)
            {
                rows--;
            }
            double container_width = container.ActualWidth;
            double container_height = container.ActualHeight;
            double block_width = (container_width - span * columns + span) / columns;
            double block_height = (container_height - span * rows + span) / rows;

            int i = 0, j = 0;
            var time_span = TimeSpan.FromMilliseconds(300);
            var keytime = KeyTime.FromTimeSpan(time_span);
            foreach (UIElement element in container.Children)
            {
                var frames_top = new DoubleAnimationUsingKeyFrames();

                Storyboard.SetTarget(frames_top, element);
                Storyboard.SetTargetProperty(frames_top, new PropertyPath(Canvas.TopProperty));
                frames_top.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = keytime,
                    Value = j * span + block_height * j
                });
                story.Children.Add(frames_top);

                var frames_left = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(frames_left, element);
                Storyboard.SetTargetProperty(frames_left, new PropertyPath(Canvas.LeftProperty));
                frames_left.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = keytime,
                    Value = i * span + block_width * i
                });
                story.Children.Add(frames_left);

                var frames_width = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(frames_width, element);
                Storyboard.SetTargetProperty(frames_width, new PropertyPath(Control.WidthProperty));
                frames_width.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = keytime,
                    Value = block_width
                });
                story.Children.Add(frames_width);

                var frames_height = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(frames_height, element);
                Storyboard.SetTargetProperty(frames_height, new PropertyPath(Control.HeightProperty));
                frames_height.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = keytime,
                    Value = block_height
                });
                story.Children.Add(frames_height);

                i++;
                if (i == sqrt)
                {
                    i = 0;
                    j++;
                }
            }

            try
            {
                story.Begin();
            }
            catch (Exception ex)
            {
                Console.WriteLine("->TancfsToAll.error");
                Console.WriteLine(ex.Message);
            }
        }
        private void TancfsToSingle(UIElement current)
        {
            Storyboard story = new Storyboard();
            int count = container.Children.Count;
            double span = 5;
            double container_width = container.ActualWidth;
            double container_height = container.ActualHeight;
            double width = 150;
            double height = (container_height - span * (count - 2)) / (count - 1);
            double left = container_width - width;
            int i = 0;
            var time_span = TimeSpan.FromMilliseconds(300);
            var keytime = KeyTime.FromTimeSpan(time_span);
            foreach (UIElement element in container.Children)
            {
                var isCurrent = (element == current);
                var frames_top = new DoubleAnimationUsingKeyFrames();

                Storyboard.SetTarget(frames_top, element);
                Storyboard.SetTargetProperty(frames_top, new PropertyPath(Canvas.TopProperty));
                frames_top.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = keytime,
                    Value = isCurrent ? 0 : height * i + span * i
                });
                story.Children.Add(frames_top);

                var frames_left = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(frames_left, element);
                Storyboard.SetTargetProperty(frames_left, new PropertyPath(Canvas.LeftProperty));
                frames_left.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = keytime,
                    Value = isCurrent ? 0 : left
                });
                story.Children.Add(frames_left);

                var frames_width = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(frames_width, element);
                Storyboard.SetTargetProperty(frames_width, new PropertyPath(Control.WidthProperty));
                frames_width.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = keytime,
                    Value = isCurrent ? left - span : width
                });
                story.Children.Add(frames_width);

                var frames_height = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(frames_height, element);
                Storyboard.SetTargetProperty(frames_height, new PropertyPath(Control.HeightProperty));
                frames_height.KeyFrames.Add(new EasingDoubleKeyFrame()
                {
                    KeyTime = keytime,
                    Value = isCurrent ? container_height : height
                });
                story.Children.Add(frames_height);

                if (isCurrent == false) i++;
            }

            try
            {
                story.Begin();
            }
            catch (Exception ex)
            {
                Console.WriteLine("->TancfsToSingle.error");
                Console.WriteLine(ex.Message);
            }
        }

        DisplayView selected;
        private void RefreshLayout()
        {
            if (container.Children.Count == 0) return;
            if (selected == null)
            {
                this.TancfsToAll();
            }
            else
            {
                TancfsToSingle(selected as UIElement);
            }
        }

        private void testLayout()
        {
            for (int i = 0; i < 30; i++)
            {
                DisplayView view = new DisplayView();
                view.MouseDoubleClick += view_MouseDoubleClick;
                container.Children.Add(view);
            }
            this.RefreshLayout();
        }

        void view_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.selected == sender)
            {
                this.selected = null;
                this.TancfsToAll();
            }
            else if (this.selected == null)
            {
                this.selected = sender as DisplayView;
                this.TancfsToSingle(selected);
            }
            else
            {
                this.TancfsToSingle(sender as DisplayView);
                this.selected = sender as DisplayView;
            }
        }
        #endregion
    }
}