using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace screenrec.Base
{
    public interface IScreenRecorder
    {
        bool CanCaptureScreen { get; }
        int ScreensCount { get; }
        IDisposable StartRecord(string fileName, int screenIndex, EncodeSettings encodeSettings);
    }
}
