using System.Windows;

namespace GVMetting.WPFClient
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //var data = new ByteData()
            //{
            //    from = "192.168.1.1",
            //    Video = (Bitmap)(Bitmap.FromFile("d:\\a.jpg"))
            //};
            //data.Video.Save("d:\\c.jpg");
            //data.Metas.Add("test", new byte[] { 0x01, 0x02 });
            //data.Metas.Add("test2", new byte[] { 0x01, 0x02 });
            //data.Metas.Add("张三", new byte[] { 0x01, 0x02 });
            //var bytes = ByteHelper.Parse(data);
            //var data2 = ByteHelper.Parse(bytes);
            //data2.Video.Save("d:\\b.jpg");

            base.OnStartup(e);
        }
    }
}
