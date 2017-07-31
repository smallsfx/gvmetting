using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GVMetting.WPFClient.SettingViews
{
    public partial class IconView : UserControl
    {
        public IconView()
        {
            this.DataContext = this;
            InitializeComponent();
            this.Loaded += this.OnLoaded;
        }
        public static readonly DependencyProperty AllIconsProperty =
           DependencyProperty.Register("AllIcons", typeof(IEnumerable), typeof(IconView), new PropertyMetadata(default(IEnumerable)));

        public IEnumerable AllIcons
        {
            get { return (IEnumerable)GetValue(AllIconsProperty); }
            set { SetValue(AllIconsProperty, value); }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var dict = new ResourceDictionary { Source = new Uri("Resources/Icons.xaml",UriKind.RelativeOrAbsolute) };
            var foundIcons = dict
                .OfType<DictionaryEntry>()
                .Where(de => de.Value is Canvas)
                .Select(de => new MetroIcon((string)de.Key, (Canvas)de.Value))
                .OrderBy(mi => mi.Name)
                .ToList();
            this.AllIcons = foundIcons;
        }

        public sealed class MetroIcon
        {
            public MetroIcon(string name, Visual visual)
            {
                this.Name = name;
                this.Visual = visual;
            }

            public string Name { get; private set; }
            public Visual Visual { get; set; }
        }
    }
}
