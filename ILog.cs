using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace screenrec
{
    public interface ILog
    {
        void WriteVerbose(object message);
        void WriteInformation(object information);
        void WriteError(object error);
        void WriteCritical(object criticalError);
    }
}
