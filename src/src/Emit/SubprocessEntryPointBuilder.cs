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
    internal sealed class SubprocessEntryPointBuilder : EntryPointBuilder
    {
        private readonly MethodInfo _Method;
        internal SubprocessEntryPointBuilder(Type genericParameter, bool isSTAThread, bool isWinApp, MethodInfo method)
            : base(genericParameter, isSTAThread, isWinApp)
        {
            _Method = method;
        }

        protected override void EmitExecuteCore(ILGenerator gen)
        {
            if (_Method.GetParameters().Length > 0)
            {
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Call, typeof(SubprocessArgument<>).MakeGenericType(GenericParameter).GetMethod("get_Parameter"));
            }
            gen.Emit(OpCodes.Call, _Method);
            gen.Emit(OpCodes.Ret);
        }
    }
}