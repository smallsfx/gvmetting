using MahApps.Metro;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GVMetting.WPFClient.SettingViews
{
    public partial class ThemeView : UserControl
    {
        public List<ThemeObject> AccentColors { get; set; }
        public List<ThemeObject> AppThemes { get; set; }

        public static readonly DependencyProperty SelectedThemeProperty =
           DependencyProperty.Register("SelectedTheme"
           , typeof(ThemeObject)
           , typeof(ThemeView)
           , new PropertyMetadata(default(ThemeObject), SelectedThemePropertyChangedCallback));

        static void SelectedThemePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var data = e.NewValue as ThemeObject;
            if (data == null) return;
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = ThemeManager.GetAppTheme(data.Name);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
        }
        public ThemeObject SelectedTheme
        {
            get { return (ThemeObject)GetValue(SelectedThemeProperty); }
            set
            {
                SetValue(SelectedThemeProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedColorProperty =
           DependencyProperty.Register("SelectedColor"
           , typeof(ThemeObject)
           , typeof(ThemeView)
           , new PropertyMetadata(default(ThemeObject), SelectedColorPropertyChangedCallback));

        static void SelectedColorPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var data = e.NewValue as ThemeObject;
            if (data == null) return;
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(data.Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
        }

        public ThemeObject SelectedColor
        {
            get { return (ThemeObject)GetValue(SelectedColorProperty); }
            set
            {
                SetValue(SelectedColorProperty, value);
            }
        }

        public ThemeView()
        {
            InitializeComponent();


            // create accent color menu items for the demo
            this.AccentColors =
                ThemeManager.Accents
                            .Select(a => new ThemeObject() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                            .ToList();

            // create metro theme color menu items for the demo
            this.AppThemes =
                ThemeManager.AppThemes
                            .Select(a => new ThemeObject() { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
                            .ToList();

            var theme = ThemeManager.DetectAppStyle(Application.Current);
            this.DataContext = this;
            this.SelectedColor = this.AccentColors [0];
            this.SelectedTheme = this.AppThemes[0];
        }
    }

    public class ThemeObject
    {
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }
    }

}
