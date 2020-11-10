using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace screenrec
{
    public class ActionThroughDispose : IDisposable
    {
        readonly Action disposeAction;

        public ActionThroughDispose(Action disposeAction)
        {
            this.disposeAction = disposeAction;
        }

        #region IDisposable Members

        public void Dispose()
        {
            disposeAction();
        }

        #endregion
    }
}
