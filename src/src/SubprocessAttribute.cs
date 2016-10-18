using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class SubprocessAttribute : Attribute
    {
    }
}
