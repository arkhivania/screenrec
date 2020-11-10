using screenrec;
using screenrec.Base;
using screenrec.VideoEncode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.UI.Composition;

namespace screenrec.Processing
{
    class Recorder : IScreenRecorder, IDisposable
    {
        readonly IDirect3DDevice device;
        readonly MonitorInfo[] monitors;

        public Recorder(ILog log)
        {
            this.log = log;
            try
            {
                if (!ApiInformation.IsApiContractPresent(typeof(Windows.Foundation.UniversalApiContract).FullName, 8))
                    return;

                monitors = MonitorEnumerationHelper.GetMonitors().ToArray();
                device = VideoEncode.Direct3D11Helpers.CreateDevice();
            }
            catch
            {
                device?.Dispose();
                device = null;
            }
        }

        public bool CanCaptureScreen => device != null;

        public int ScreensCount => monitors.Length;

        bool disposed = false;
        public void Dispose()
        {
            if (disposed)
                return;

            foreach (var s in openedSessions.ToArray())
                s.Dispose();

            device?.Dispose();
            disposed = true;
        }

        readonly List<RecordSession> openedSessions = new List<RecordSession>();
        private readonly ILog log;

        public IDisposable StartRecord(string fileName, int screenIndex, EncodeSettings encodeSettings)
        {
            var item = CaptureHelper
                        .CreateItemForMonitor(monitors[screenIndex].Hmon);

            var session = new RecordSession(device, fileName, item, encodeSettings, log);
            openedSessions.Add(session);
            return new ActionThroughDispose(() =>
            {
                session.Dispose();
                openedSessions.Remove(session);
            });
        }
    }
}
