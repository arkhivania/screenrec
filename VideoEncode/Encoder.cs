using SharpDX.DXGI;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage.Streams;


namespace screenrec.VideoEncode
{
    sealed class Encoder : IDisposable
    {
        TimeSpan begin;
        public TimeSpan Length { get; private set; } = TimeSpan.Zero;

        public Encoder(IDirect3DDevice device,
            GraphicsCaptureItem item,
            MediaEncodingProfile encodingProfile)
        {
            _device = device;
            _captureItem = item;
            this.encodingProfile = encodingProfile;
            _isRecording = false;

            CreateMediaObjects();

            _frameGenerator = new CaptureFrameWait(
                    _device,
                    _captureItem,
                    _captureItem.Size);
        }

        public async Task EncodeAsync(IRandomAccessStream stream)
        {
            if (!_isRecording)
            {
                _isRecording = true;
                var transcode = await _transcoder.PrepareMediaStreamSourceTranscodeAsync(_mediaStreamSource, stream, encodingProfile);
                await transcode.TranscodeAsync();
            }
        }

        public void StopCapture()
        {
            _isRecording = false;
            _frameGenerator.Stop();
        }

        public void Dispose()
        {
            if (_closed)
                return;

            _closed = true;

            if (!_isRecording)
                DisposeInternal();

            _isRecording = false;
        }

        private void DisposeInternal()
        {
            _frameGenerator.Dispose();
        }

        private void CreateMediaObjects()
        {
            // Create our encoding profile based on the size of the item
            int width = _captureItem.Size.Width;
            int height = _captureItem.Size.Height;

            // Describe our input: uncompressed BGRA8 buffers
            var videoProperties = VideoEncodingProperties.CreateUncompressed(MediaEncodingSubtypes.Bgra8, (uint)width, (uint)height);
            _videoDescriptor = new VideoStreamDescriptor(videoProperties);

            // Create our MediaStreamSource
            _mediaStreamSource = new MediaStreamSource(_videoDescriptor);
            _mediaStreamSource.BufferTime = TimeSpan.FromSeconds(0);
            _mediaStreamSource.Starting += OnMediaStreamSourceStarting;
            _mediaStreamSource.SampleRequested += OnMediaStreamSourceSampleRequested;

            // Create our transcoder
            _transcoder = new MediaTranscoder();
            _transcoder.HardwareAccelerationEnabled = true;
        }

        private void OnMediaStreamSourceSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            try
            {
                while (_isRecording && !_closed)
                    using (var frame = _frameGenerator.GetNextFrame())
                    {
                        if (frame == null)
                        {
                            args.Request.Sample = null;
                            return;
                        }

                        args.Request.Sample = MediaStreamSample.CreateFromDirect3D11Surface(frame.Surface, frame.SystemRelativeTime);
                        Length = frame.SystemRelativeTime.Subtract(begin);
                        return;
                    }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                Debug.WriteLine(e);
                args.Request.Sample = null;
            }
        }

        private void OnMediaStreamSourceStarting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
        {
            using (var frame = _frameGenerator.GetNextFrame())
                args.Request.SetActualStartPosition(begin = frame.SystemRelativeTime);
        }

        private IDirect3DDevice _device;

        private GraphicsCaptureItem _captureItem;
        private readonly MediaEncodingProfile encodingProfile;
        private readonly CaptureFrameWait _frameGenerator;

        private VideoStreamDescriptor _videoDescriptor;
        private MediaStreamSource _mediaStreamSource;
        private MediaTranscoder _transcoder;
        private bool _isRecording;
        private bool _closed = false;
    }
}
