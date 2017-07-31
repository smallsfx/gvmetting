using AForge.Video.DirectShow;
using System.Collections.Generic;

namespace GVMetting.Media
{
    public class DeviceTools
    {
        public static DeviceObject[] GetAudioDevices()
        {
            FilterInfoCollection devices = new FilterInfoCollection(FilterCategory.AudioInputDevice);
            List<DeviceObject> result = new List<DeviceObject>();
            foreach (FilterInfo devcie in devices)
            {
                result.Add(new DeviceObject{
                    Name = devcie.Name,
                    Moniker = devcie.MonikerString
                });
            }
            return result.ToArray();
        }
        public static DeviceObject[] GetVideoDevices()
        {
            FilterInfoCollection devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            List<DeviceObject> result = new List<DeviceObject>();
            foreach (FilterInfo devcie in devices)
            {
                result.Add(new DeviceObject
                {
                    Name = devcie.Name,
                    Moniker = devcie.MonikerString
                });
            }
            return result.ToArray();
        }
    }
}
