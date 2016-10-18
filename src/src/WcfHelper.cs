using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing
{
    internal static class WcfHelper
    {
        internal static Binding CreateBinding()
        {
            return new NetNamedPipeBinding() { Security = new NetNamedPipeSecurity() { Mode = NetNamedPipeSecurityMode.None } };
        }
    }
}
