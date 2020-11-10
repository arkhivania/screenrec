using screenrec.VideoEncode;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Foundation.Metadata;
using Windows.Media.MediaProperties;

namespace screenrec
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var form = new Form())
            {
                using (var device = Direct3D11Helpers.CreateDevice())
                {
                    if (!ApiInformation.IsApiContractPresent(typeof(Windows.Foundation.UniversalApiContract).FullName, 8))
                        return;

                    var monitors = MonitorEnumerationHelper.GetMonitors().ToArray();
                    var monitor = monitors.First();

                    var item = CaptureHelper
                        .CreateItemForMonitor(monitor.Hmon);
                    if (item == null)
                        return;

                    var temp = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
                    temp.Video.Bitrate = 512 * 8 * 1024;
                    temp.Video.FrameRate.Numerator = 30;
                    temp.Video.FrameRate.Denominator = 1;

                    using (var stream = new FileStream("out.mp4", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (var _encoder = new Encoder(device, item, temp))
                        {
                            var task = Task
                                .Run(() => _encoder.EncodeAsync(stream.AsRandomAccessStream()));

                            form.ShowDialog();
                            _encoder.StopCapture();

                            task.Wait();
                        }
                    }
                }
            }
        }
    }
}
