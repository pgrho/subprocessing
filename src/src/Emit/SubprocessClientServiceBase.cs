using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing.Emit
{
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class SubprocessClientServiceBase
    {
        private int _ProcessId;

        public int ProcessId
        {
            get
            {
                if (_ProcessId == 0)
                {
                    _ProcessId = Process.GetCurrentProcess().Id;
                }
                return _ProcessId;
            }
        }
    }
}
