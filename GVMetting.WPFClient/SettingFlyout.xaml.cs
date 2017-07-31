using MahApps.Metro;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GVMetting.WPFClient.SettingViews;

namespace GVMetting.WPFClient
{
    public partial class SettingFlyout : Flyout, INotifyPropertyChanged
    {
        public OptionItem DFT_OPTION;
        public static readonly DependencyProperty OptionsProperty =
           DependencyProperty.Register("Options"
           , typeof(List<OptionItem>)
           , typeof(SettingFlyout)
           , new PropertyMetadata(default(List<OptionItem>)));

        public List<OptionItem> Options
        {
            get { return (List<OptionItem>)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
           DependencyProperty.Register("SelectedItem"
           , typeof(OptionItem)
           , typeof(SettingFlyout)
           , new PropertyMetadata(default(OptionItem)));

        public OptionItem SelectedItem
        {
            get { return (OptionItem)GetValue(SelectedItemProperty); }
            set
            {
                if (value == null)
                {
                    SetValue(SelectedItemProperty, DFT_OPTION);
                }
                else
                {
                    SetValue(SelectedItemProperty, value);
                }
            }
        }

        public SettingFlyout()
        {
            InitializeComponent();

            this.SelectedItem = DFT_OPTION = new OptionItem
            {
                Header = "设置",
                View = new NavigationView()
            };
            
            this.Options = new List<OptionItem>();

            this.Options.Add(new OptionItem
            {
                View = new ThemeView(),
                Header = "主题",
                Describe = "设置系统的皮肤及色系."
            });
            this.Options.Add(new OptionItem
            {
                View = new ConnectView(),
                Header = "连接",
                Describe = "服务端的IP及端口号设置."
            });
            this.Options.Add(new OptionItem
            {
                View = new DeviceView(),
                Header = "设备",
                Describe = "设置使用的视频输入，音频输入设备."
            });
            this.Options.Add(new OptionItem
            {
                View = new IconView(),
                Header = "图标",
                Describe = "查看支持的全部图标."
            });
            this.Options.Add(new OptionItem
            {
                View = new MettingView(),
                Header = "会议",
                Describe = ""
            });
            this.DataContext = this;
        }

        private ICommand backCmd;

        public ICommand BackCmd
        {
            get
            {
                return this.backCmd ?? (this.backCmd = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x =>
                    {
                        if (this.SelectedItem == DFT_OPTION)
                        {
                            this.IsOpen = false;
                        }
                        else
                        {
                            this.SelectedItem = DFT_OPTION;
                        }
                    }
                });
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion //#region INotifyPropertyChanged
    }

    public class SimpleCommand : ICommand
    {
        public Predicate<object> CanExecuteDelegate { get; set; }
        public Action<object> ExecuteDelegate { get; set; }

        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate != null)
                return CanExecuteDelegate(parameter);
            return true; // if there is no can execute default to true
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
                ExecuteDelegate(parameter);
        }
    }

    public class OptionItem
    {
        public string Header { get; set; }
        public string Describe { get; set; }
        public UserControl View { get; set; }
    }
}
