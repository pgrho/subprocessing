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
    internal class ServiceContractBuilder : BuilderBase
    {
        protected class BuilderMethodInfo
        {
            private readonly MethodInfo _Method;

            private readonly List<BuilderParameterInfo> _Parameters;

            internal BuilderMethodInfo(ServiceContractBuilder builder, MethodInfo method)
            {
                _Method = method;
                _Parameters = new List<BuilderParameterInfo>();
                var implParamCount = 0;
                foreach (var p in method.GetParameters())
                {
                    if (p.ParameterType.IsPointer)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                "{0}.{1}のパラメーター{2}にポインター型を使用することは出来ません。",
                                method.ReflectedType,
                                method.Name,
                                p.Name));
                    }

                    var isId = false;
                    var isInfo = false;
                    if (p.GetCustomAttribute<SubprocessIdAttribute>() != null)
                    {
                        if (p.ParameterType != typeof(int))
                        {
                            throw new InvalidOperationException(
                                string.Format(
                                    "{0}.{1}のパラメーター{2}には{3}が指定されていますが{4}型ではありません。",
                                    method.ReflectedType,
                                    method.Name,
                                    p.Name,
                                    typeof(SubprocessIdAttribute),
                                    typeof(int)));
                        }
                        isId = true;
                    }
                    if (p.GetCustomAttribute<SubprocessAttribute>() != null)
                    {
                        if (!builder._SubprocessType.IsAssignableFrom(p.ParameterType))
                        {
                            throw new InvalidOperationException(
                                string.Format(
                                    "{0}.{1}のパラメーター{2}には{3}が指定されていますが{4}型ではありません。",
                                    method.ReflectedType,
                                    method.Name,
                                    p.Name,
                                    typeof(SubprocessIdAttribute),
                                    builder._SubprocessType));
                        }
                        isInfo = true;
                    }

                    if (isId)
                    {
                        _Parameters.Add(new BuilderParameterInfo(p.Name, p.ParameterType, p.Position, -1, true, false));
                    }
                    else if (isInfo)
                    {
                        _Parameters.Add(new BuilderParameterInfo(p.Name, p.ParameterType, p.Position, -1, false, true));
                    }
                    else
                    {
                        _Parameters.Add(new BuilderParameterInfo(p.Name, p.ParameterType, p.Position, implParamCount++, false, false));
                    }
                }
                _Parameters.Add(
                    new BuilderParameterInfo(
                        new[] { "processId" }
                            .Concat(Enumerable.Range(2, short.MaxValue).Select(_ => "processId" + _))
                            .First(n => _Parameters.All(_ => _.Name != n)),
                        typeof(int),
                        -1,
                        implParamCount++,
                        true,
                        false));
            }

            public string Name
            {
                get
                {
                    return _Method.Name;
                }
            }
            public Type ReturnType
            {
                get
                {
                    return _Method.ReturnType;
                }
            }
            public MethodInfo Method
            {
                get
                {
                    return _Method;
                }
            }

            public List<BuilderParameterInfo> Parameters
            {
                get
                {
                    return _Parameters;
                }
            }

            public bool CanUseOneWay
            {
                get
                {
                    return _Method.CanBeOneWay();
                }
            }
        }
        protected class BuilderParameterInfo
        {
            private readonly string _Name;
            private readonly Type _ParameterType;

            private readonly int _HostIndex;

            private readonly int _ImplIndex;


            private readonly bool _IsId;


            private readonly bool _IsInfo;



            internal BuilderParameterInfo(string name, Type parameterType, int hostIndex, int implIndex, bool isId, bool isInfo)
            {
                _Name = name;
                _ParameterType = parameterType;
                _HostIndex = hostIndex;
                _ImplIndex = implIndex;
                _IsId = isId;
                _IsInfo = isInfo;
            }

            public string Name
            {
                get
                {
                    return _Name;
                }
            }

            public Type ParameterType
            {
                get { return _ParameterType; }
            }

            public int HostIndex
            {
                get
                {
                    return _HostIndex;
                }
            }

            public int ImplIndex
            {
                get
                {
                    return _ImplIndex;
                }
            }
            public bool IsId
            {
                get
                {
                    return _IsId;
                }
            }
            public bool IsInfo
            {
                get
                {
                    return _IsInfo;
                }
            }
        }

        internal const string GENERATED_SERVICE_CONTRACT = "IGeneratedServiceContract";
        internal const string HOST_SERVICE_WRAPPER = "HostServiceWrapper";
        internal const string CLIENT_SERVICE_WRAPPER = "ClientServiceWrapper";
        internal const string START_INFO = "StartInfo";

        private readonly Type _SubprocessType;
        private readonly Type _UserServiceContract;
        private Type _GeneratedServiceContract;
        private Type _HostServiceWrapper;
        private Type _ClientServiceWrapper;

        internal ServiceContractBuilder(Type userServiceContract)
            : this(userServiceContract, typeof(Subprocess))
        { }

        protected ServiceContractBuilder(Type userServiceContract, Type subprocessType)
        {
            ThrowIfInvalidContractType(userServiceContract, "userServiceContract");//TODO:nameof
            _UserServiceContract = userServiceContract;
            _SubprocessType = subprocessType;
        }


        protected virtual Type CallbackContractType
        {
            get
            {
                return null;
            }
        }

        protected Type UserServiceContract
        {
            get
            {
                return _UserServiceContract;
            }
        }

        protected Type GeneratedServiceContract
        {
            get
            {
                return _GeneratedServiceContract;
            }
        }


        public override void EmitModule(ModuleBuilder moduleBuilder)
        {
            var methods = _UserServiceContract.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy).Select(_ => new BuilderMethodInfo(this, _)).ToList();

            _GeneratedServiceContract = EmitServiceContract(moduleBuilder, methods);

            _HostServiceWrapper = EmitHostServiceWrapper(moduleBuilder, methods);

            _ClientServiceWrapper = EmitClientServiceWrapper(moduleBuilder, methods);

            EmitStartInfo(moduleBuilder);
        }

        protected void DefineTypesCore(ModuleBuilder moduleBuilder)
        {
        }

        protected static void ThrowIfInvalidContractType(Type contractType, string name)
        {
            if (!contractType.IsInterface)
            {
                throw new InvalidOperationException(string.Format(Resources.Arg0MustBeAnInterfaceType, name));//TODO:nameof
            }
        }

        protected static void SetSignature(MethodInfo mi, MethodBuilder nm)
        {
            nm.SetReturnType(mi.ReturnType);
            var ps = mi.GetParameters();
            nm.SetParameters(ps.Select(_ => _.ParameterType).ToArray());

            foreach (var p in ps)
            {
                var pb = nm.DefineParameter(p.Position + 1, ParameterAttributes.None, p.Name);
                if (p.IsOut)
                {
                    pb.SetCustomAttribute(EmitHelper.GetAttributeBuilder<OutAttribute>());
                }
            }
        }
        private Type EmitServiceContract(ModuleBuilder m, IEnumerable<BuilderMethodInfo> methods)
        {
            var ct = m.DefineType(GENERATED_SERVICE_CONTRACT, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
            ct.SetCustomAttribute(
                new CustomAttributeBuilder(
                    EmitHelper.ServiceContractConstructor,
                    new object[0],
                    new[]
                    {
                        typeof(ServiceContractAttribute).GetProperty("CallbackContract")
                    },
                    new object[]
                    {
                        CallbackContractType
                    }));

            foreach (var mi in methods)
            {
                var nm = ct.DefineMethod(mi.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.HideBySig | MethodAttributes.NewSlot);
                nm.SetReturnType(mi.ReturnType);
                nm.SetParameters(mi.Parameters.Where(_ => _.ImplIndex >= 0).Select(_ => _.ParameterType).ToArray());

                foreach (var p in mi.Parameters)
                {
                    if (p.ImplIndex >= 0)
                    {
                        nm.DefineParameter(p.ImplIndex + 1, ParameterAttributes.None, p.Name);
                    }
                }

                nm.SetCustomAttribute(new CustomAttributeBuilder(EmitHelper.OperationContractConstructor, new object[0], new[] { 
                        EmitHelper.IsOneWay }, new object[] { mi.CanUseOneWay }));
            }

            return ct.CreateType();
        }

        private Type EmitHostServiceWrapper(ModuleBuilder m, IEnumerable<BuilderMethodInfo> methods)
        {
            var si = m.DefineType(HOST_SERVICE_WRAPPER, TypeAttributes.Public | TypeAttributes.Sealed);
            si.AddInterfaceImplementation(_GeneratedServiceContract);
            si.SetCustomAttribute(
                new CustomAttributeBuilder(
                    typeof(ServiceBehaviorAttribute).GetConstructor(new Type[0]),
                    new object[0],
                    new[]
                    {
                        typeof(ServiceBehaviorAttribute).GetProperty("InstanceContextMode"),
                        typeof(ServiceBehaviorAttribute).GetProperty("ConcurrencyMode")
                    },
                    new object[]
                    {
                        InstanceContextMode.Single,
                        ConcurrencyMode.Multiple
                    }));

            var hi = si.DefineField("_HostInstance", _UserServiceContract, FieldAttributes.Private);
            var func = si.DefineField("_ProcessResolver", typeof(Func<,>).MakeGenericType(typeof(int), _SubprocessType), FieldAttributes.Private);
            {
                var ctor = si.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard, new[] { hi.FieldType, func.FieldType });
                ctor.DefineParameter(1, ParameterAttributes.None, "hostInstance");
                ctor.DefineParameter(2, ParameterAttributes.None, "processResolver");
                var gen = ctor.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, hi);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_2);
                gen.Emit(OpCodes.Stfld, func);
                gen.Emit(OpCodes.Ret);
            }

            foreach (var im in methods)
            {
                var nm = si.DefineMethod(im.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig);
                nm.SetReturnType(im.ReturnType);
                nm.SetParameters(im.Parameters.Where(_ => _.ImplIndex >= 0).Select(_ => _.ParameterType).ToArray());

                foreach (var p in im.Parameters)
                {
                    if (p.ImplIndex >= 0)
                    {
                        nm.DefineParameter(p.ImplIndex + 1, ParameterAttributes.None, p.Name);
                    }
                }

                var gen = nm.GetILGenerator();
                var lb = im.Parameters.Any(_ => _.IsInfo) ? gen.DeclareLocal(_SubprocessType) : null;
                var implParamCount = im.Parameters.Count(_ => _.ImplIndex >= 0);

                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, func);
                gen.EmitLdArg(implParamCount);
                gen.Emit(OpCodes.Callvirt, func.FieldType.GetMethod("Invoke", new[] { typeof(int) }));
                if (lb == null)
                {
                    gen.Emit(OpCodes.Pop);
                }
                else
                {
                    gen.Emit(OpCodes.Stloc_0);
                }
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, hi);

                foreach (var p in im.Parameters)
                {
                    if (p.HostIndex < 0)
                    {
                        continue;
                    }
                    if (p.IsId)
                    {
                        gen.EmitLdArg(implParamCount);
                    }
                    else if (p.IsInfo)
                    {
                        gen.Emit(OpCodes.Ldloc_0);
                    }
                    else
                    {
                        gen.EmitLdArg(p.ImplIndex + 1);
                    }
                }

                gen.Emit(OpCodes.Callvirt, im.Method);
                gen.Emit(OpCodes.Ret);
            }
            return si.CreateType();
        }

        private Type EmitClientServiceWrapper(ModuleBuilder m, IEnumerable<BuilderMethodInfo> methods)
        {
            var cs = m.DefineType(CLIENT_SERVICE_WRAPPER, TypeAttributes.Public | TypeAttributes.Sealed, typeof(SubprocessClientServiceBase));
            cs.AddInterfaceImplementation(_UserServiceContract);

            var service = cs.DefineField("_Service", _GeneratedServiceContract, FieldAttributes.Private);
            {
                var ctor = cs.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard, new[] { _GeneratedServiceContract });
                ctor.DefineParameter(1, ParameterAttributes.None, "service");
                var gen = ctor.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, service);
                gen.Emit(OpCodes.Ret);
            }

            foreach (var im in methods)
            {
                var nm = cs.DefineMethod(im.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig);
                SetSignature(im.Method, nm);

                var gen = nm.GetILGenerator();
                gen.DeclareLocal(typeof(int));

                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, cs.BaseType.GetMethod("get_ProcessId"));
                gen.Emit(OpCodes.Stloc_0);

                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, service);

                foreach (var p in im.Parameters)
                {
                    if (p.ImplIndex < 0)
                    {
                        continue;
                    }
                    if (p.IsId)
                    {
                        gen.Emit(OpCodes.Ldloc_0);
                    }
                    else
                    {
                        gen.EmitLdArg(p.HostIndex + 1);
                    }
                }

                gen.Emit(OpCodes.Callvirt, service.FieldType.GetMethod(im.Name, im.Parameters.Where(_ => _.ImplIndex >= 0).Select(_ => _.ParameterType).ToArray()));
                gen.Emit(OpCodes.Ret);
            }

            return cs.CreateType();
        }
        protected virtual Type EmitStartInfo(ModuleBuilder moduleBuilder)
        {
            var si = EmitStartInfoCore(moduleBuilder);

            return si.CreateType();
        }
        protected TypeBuilder EmitStartInfoCore(ModuleBuilder moduleBuilder)
        {
            var si = moduleBuilder.DefineType(START_INFO, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);
            var tp = si.DefineGenericParameters("TParameter")[0];
            var baseType = GetStartInfoBaseType(tp);
            si.SetParent(baseType);

            var ctorArgs = new[] { typeof(SubprocessArgument<>).MakeGenericType(tp) };
            var ctor = si.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard, ctorArgs);
            {
                ctor.DefineParameter(1, ParameterAttributes.None, "startInfo");
                var gen = ctor.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Call, GetStartInfoBaseConstructor(baseType));
                gen.Emit(OpCodes.Ret);
            }
            {
                var me = si.DefineMethod(
                            "CreateServiceWrapper",
                            MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                            UserServiceContract,
                            new[] { GeneratedServiceContract });

                me.DefineParameter(1, ParameterAttributes.None, "service");
                var gen = me.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Newobj, _ClientServiceWrapper.GetConstructors()[0]);
                gen.Emit(OpCodes.Ret);
            }
            return si;
        }

        protected virtual ConstructorInfo GetStartInfoBaseConstructor(Type baseType)
        {
            return TypeBuilder.GetConstructor(baseType, typeof(ServiceSubprocessStartInfoBase<,,>).GetConstructors()[0]);
        }

        protected virtual Type GetStartInfoBaseType(GenericTypeParameterBuilder parameterType)
        {
            return typeof(ServiceSubprocessStartInfoBase<,,>)
                        .MakeGenericType(
                            UserServiceContract,
                            GeneratedServiceContract,
                            parameterType);
        }
    }
}