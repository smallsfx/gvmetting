using System.Windows.Controls;

namespace GVMetting.WPFClient.SettingViews
{
    /// <summary>
    /// ConnectView.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectView : UserControl
    {
        public ConnectView()
        {
            InitializeComponent();
            this.DataContext = SystemSetting.Instance;
        }
    }
}
