using Shipwreck.Subprocessing.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing.Emit
{
    internal sealed class DuplexContractBuilder : ServiceContractBuilder
    {
        internal const string HOST_CALLBACK_WRAPPER = "HostCallbackWrapper";
        internal const string CLIENT_CALLBACK_WRAPPER = "ClientCallbackWrapper";
        internal const string GENERATED_CALLBACK_CONTRACT = "IGeneratedCallbackContract";
        internal const string GET_CURRENT_CALLBACK_WRAPPER = "GetCurrentCallbackWrapper";

        private readonly Type _UserCallbackContract;

        private Type _GeneratedCallbackContract;
        private Type _ClientCallbackWrapper;

        internal DuplexContractBuilder(Type userServiceContract, Type userCallbackContract)
            : base(userServiceContract, typeof(DuplexSubprocess<>).MakeGenericType(userCallbackContract))
        {
            ThrowIfInvalidContractType(userCallbackContract, "userCallbackContract");//TODO:nameof
            _UserCallbackContract = userCallbackContract;
        }

        protected override Type CallbackContractType
        {
            get
            {
                return _GeneratedCallbackContract;
            }
        }

        public override void EmitModule(ModuleBuilder moduleBuilder)
        {
            var mps = new Dictionary<MethodInfo, MethodBuilder>();
            _GeneratedCallbackContract = EmitCallbackContract(moduleBuilder, mps);
            EmitHostCallbackWrapper(moduleBuilder, mps);
            _ClientCallbackWrapper = EmitClientCallbackWrapper(moduleBuilder, mps);
            base.EmitModule(moduleBuilder);
        }


        private Type EmitCallbackContract(ModuleBuilder m, Dictionary<MethodInfo, MethodBuilder> methodPairs)
        {
            var ct = m.DefineType(GENERATED_CALLBACK_CONTRACT, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
            ct.SetCustomAttribute(new CustomAttributeBuilder(EmitHelper.ServiceContractConstructor, new object[0]));

            foreach (var mi in _UserCallbackContract.GetMethods())
            {
                var nm = ct.DefineMethod(mi.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.HideBySig | MethodAttributes.NewSlot);
                SetSignature(mi, nm);

                nm.SetCustomAttribute(
                    new CustomAttributeBuilder(EmitHelper.OperationContractConstructor,
                    new object[0],
                    new[]
                    {
                        EmitHelper.IsOneWay
                    },
                    new object[]
                    {
                        mi.CanBeOneWay()
                    }));

                methodPairs[mi] = nm;
            }

            return ct.CreateType();
        }


        private Type EmitHostCallbackWrapper(ModuleBuilder m, Dictionary<MethodInfo, MethodBuilder> methodPairs)
        {
            var ct = m.DefineType(HOST_CALLBACK_WRAPPER, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);
            var ctor = CreateWrapper(ct, _UserCallbackContract, _GeneratedCallbackContract, methodPairs.Select(kv => Tuple.Create(kv.Key, (MethodInfo)kv.Value)));

            var cm = ct.DefineMethod(GET_CURRENT_CALLBACK_WRAPPER, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, CallingConventions.Standard, _UserCallbackContract, Type.EmptyTypes);
            var gen = cm.GetILGenerator();
            gen.Emit(OpCodes.Call, typeof(OperationContext).GetMethod("get_Current"));
            gen.Emit(OpCodes.Call, typeof(OperationContext).GetMethod("GetCallbackChannel").MakeGenericMethod(CallbackContractType));
            gen.Emit(OpCodes.Newobj, ctor);
            gen.Emit(OpCodes.Ret);

            return ct.CreateType();
        }
        private Type EmitClientCallbackWrapper(ModuleBuilder m, Dictionary<MethodInfo, MethodBuilder> methodPairs)
        {
            var ct = m.DefineType(CLIENT_CALLBACK_WRAPPER, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);
            CreateWrapper(ct, _GeneratedCallbackContract, _UserCallbackContract, methodPairs.Select(kv => Tuple.Create(kv.Key, kv.Key)));
            return ct.CreateType();
        }

        private ConstructorBuilder CreateWrapper(TypeBuilder ct, Type bt, Type wt, IEnumerable<Tuple<MethodInfo, MethodInfo>> signatureImpls)
        {
            ct.AddInterfaceImplementation(bt);
            ct.SetCustomAttribute(new CustomAttributeBuilder(EmitHelper.ServiceContractConstructor, new object[0]));

            var fld = ct.DefineField("_Impl", wt, FieldAttributes.Private);


            var ctor = ct.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard, new[] { wt });
            {
                ctor.DefineParameter(1, ParameterAttributes.None, "impl");
                var gen = ctor.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, fld);
                gen.Emit(OpCodes.Ret);
            }

            foreach (var kv in signatureImpls)
            {
                var mi = kv.Item1;
                var nm = ct.DefineMethod(mi.Name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual);
                SetSignature(mi, nm);

                var gen = nm.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, fld);
                foreach (var p in mi.GetParameters())
                {
                    gen.EmitLdArg(p.Position + 1);
                }
                gen.Emit(OpCodes.Callvirt, kv.Item2);
                gen.Emit(OpCodes.Ret);
            }

            return ctor;
        }

        protected override Type EmitStartInfo(ModuleBuilder moduleBuilder)
        {
            var si = EmitStartInfoCore(moduleBuilder);

            var me = si.DefineMethod("CreateCallbackWrapper", MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, CallbackContractType, new[] { _UserCallbackContract });

            me.DefineParameter(1, ParameterAttributes.None, "callback");
            var gen = me.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Newobj, _ClientCallbackWrapper.GetConstructors()[0]);
            gen.Emit(OpCodes.Ret);

            return si.CreateType();
        }
         
        protected override  ConstructorInfo GetStartInfoBaseConstructor(Type baseType)
        {
            return TypeBuilder.GetConstructor(baseType, typeof(DuplexSubprocessStartInfoBase<,,,,>).GetConstructors()[0]);
        }

        protected override Type GetStartInfoBaseType(GenericTypeParameterBuilder parameterType)
        {
            return typeof(DuplexSubprocessStartInfoBase<,,,,>)
                        .MakeGenericType(
                            UserServiceContract,
                            GeneratedServiceContract,
                            _UserCallbackContract,
                            CallbackContractType,
                            parameterType);
        }
    }
}