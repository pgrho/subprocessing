using Shipwreck.Subprocessing.Emit;
using Shipwreck.Subprocessing.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing.Emit
{
    internal sealed class ServiceSubprocessEntryPointBuilder : EntryPointBuilder
    {
        private readonly ConstructorInfo _Constructor;
        private readonly MethodInfo _Method;
        internal ServiceSubprocessEntryPointBuilder(Type genericParameter, bool isSTAThread, bool isWinApp, ConstructorInfo constructor, MethodInfo method)
            : base(genericParameter, isSTAThread, isWinApp)
        {
            _Constructor = constructor;
            _Method = method;
        }

        protected override void EmitExecuteCore(ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Newobj, _Constructor);
            gen.Emit(OpCodes.Call, _Method);
            gen.Emit(OpCodes.Ret);
        }
    }
}