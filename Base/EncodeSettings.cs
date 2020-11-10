using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace screenrec.Base
{
    public struct EncodeSettings
    {
        public int Bitrate { get; set; }
        public int FPS { get; set; }
        public TimeSpan? MinimumVideoLength { get; set; }
    }
}
