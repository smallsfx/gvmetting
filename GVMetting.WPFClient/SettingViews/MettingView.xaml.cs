using System.Windows.Controls;

namespace GVMetting.WPFClient.SettingViews
{
    public partial class MettingView : UserControl
    {
        public MettingView()
        {
            InitializeComponent();
            this.DataContext = SystemSetting.Instance;
        }
    }
}
