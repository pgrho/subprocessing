using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing
{
    public sealed class ConfigCreatedEventArgs : EventArgs
    {
        private readonly string _FileName;

        internal ConfigCreatedEventArgs(string fileName)
        {
            _FileName = fileName;
        }

        public string FileName
        {
            get
            {
                return _FileName;
            }
        }
    }
}
