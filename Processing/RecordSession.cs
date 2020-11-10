using screenrec;
using screenrec.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.MediaProperties;

namespace screenrec.Processing
{
    class RecordSession : IDisposable
    {
        readonly FileStream fileStream;
        readonly VideoEncode.Encoder encoder;

        public RecordSession(IDirect3DDevice device,
            string fileName,
            GraphicsCaptureItem captureItem,
            EncodeSettings encodeSettings,
            ILog log)
        {
            var profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
            profile.Video.Bitrate = (uint)encodeSettings.Bitrate;
            profile.Video.FrameRate.Numerator = (uint)encodeSettings.FPS;
            profile.Video.FrameRate.Denominator = 1;

            fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            encoder = new VideoEncode.Encoder(device, captureItem, profile);

            _ = encoder.EncodeAsync(fileStream.AsRandomAccessStream())
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                        log.WriteError(t.Exception);
                    try
                    {
                        encoder.Dispose();
                        fileStream.Dispose();
                        var fi = new FileInfo(fileName);
                        if (fi.Exists)
                            if (fi.Length == 0 ||
                                encodeSettings.MinimumVideoLength != null && encoder.Length < encodeSettings.MinimumVideoLength.Value)
                            {
                                log.WriteInformation($"Recorded file length: {encoder.Length}, deleted by threshold");
                                fi.Delete();
                            }
                    }
                    catch (Exception e)
                    {
                        log.WriteError(e);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        bool closed = false;
        public void Dispose()
        {
            if (closed)
                return;

            closed = true;
            encoder.StopCapture();
        }
    }
}
