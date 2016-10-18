using Shipwreck.Subprocessing.Emit;
using Shipwreck.Subprocessing.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Shipwreck.Subprocessing.Emit
{
    internal abstract class EntryPointBuilder : BuilderBase
    {
        private readonly Type _GenericParameter;

        public Type GenericParameter
        {
            get { return _GenericParameter; }
        } 

        private readonly bool _IsSTAThread;
        private readonly bool _IsWindowsApplication;
        private MethodBuilder _EntryPoint;

        internal EntryPointBuilder(Type genericParameter, bool isSTAThread, bool isWinApp)
        {
            _GenericParameter = genericParameter;
            _IsSTAThread = isSTAThread;
            _IsWindowsApplication = isWinApp;
        }

        protected override MethodInfo GetEntryPoint()
        {
            return _EntryPoint;
        }

        protected abstract void EmitExecuteCore(ILGenerator gen);

        protected override PEFileKinds GetPEFileKinds()
        {
            return _IsWindowsApplication ? PEFileKinds.WindowApplication : PEFileKinds.ConsoleApplication;
        }

        public override void EmitModule(ModuleBuilder mod)
        {
            var spr = mod.DefineType("SubprocessEntryPoint",
                       TypeAttributes.Sealed,
                       typeof(SubprocessEntryPointBase<>).MakeGenericType(_GenericParameter));

            var ctor = spr.DefineDefaultConstructor(MethodAttributes.Assembly | MethodAttributes.HideBySig);

            var executeCore = spr.DefineMethod("ExecuteCore", MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig);
            {
                executeCore.SetSignature(typeof(void), null, null, new[] { _GenericParameter }, null, null);

                executeCore.DefineParameter(1, ParameterAttributes.None, "parameter");

                var gen = executeCore.GetILGenerator();
                EmitExecuteCore(gen);
            }

            spr.CreateType();

            var prg = mod.DefineType("Program", TypeAttributes.Sealed);

            var resolve = prg.DefineMethod("CurrentDomain_AssemblyResolve", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig);
            {
                EmitAssemblyResolve(resolve);
            }

            var cctor = prg.DefineMethod(".cctor", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig);
            {
                var getDir = typeof(Environment).GetMethod("get_CurrentDirectory");
                var getDom = typeof(AppDomain).GetMethod("get_CurrentDomain");
                var resCtor = typeof(ResolveEventHandler).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
                var addAr = typeof(AppDomain).GetMethod("add_AssemblyResolve");

                var gen = cctor.GetILGenerator();
                gen.Emit(OpCodes.Call, getDom);
                gen.Emit(OpCodes.Ldnull);
                gen.Emit(OpCodes.Ldftn, resolve);
                gen.Emit(OpCodes.Newobj, resCtor);
                gen.Emit(OpCodes.Callvirt, addAr);
                gen.Emit(OpCodes.Ret);
            }

            _EntryPoint = prg.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig);
            {
                _EntryPoint.SetSignature(typeof(void), null, null, new[] { typeof(string[]) }, null, null);

                if (_IsSTAThread)
                {
                    var sta = new CustomAttributeBuilder(typeof(STAThreadAttribute).GetConstructor(new Type[0]), new object[0]);
                    _EntryPoint.SetCustomAttribute(sta);
                }

                var execute = spr.BaseType.GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                _EntryPoint.DefineParameter(1, ParameterAttributes.None, "args");
                var gen = _EntryPoint.GetILGenerator();
                gen.Emit(OpCodes.Newobj, ctor);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, execute);
                gen.Emit(OpCodes.Ret);
            }
            prg.CreateType();
        }

        private static void EmitAssemblyResolve(MethodBuilder resolve)
        {
            var p = Path.GetDirectoryName(typeof(Subprocess).Assembly.Location);

            resolve.SetReturnType(typeof(Assembly));
            resolve.SetParameters(typeof(object), typeof(ResolveEventArgs));
            resolve.DefineParameter(1, ParameterAttributes.None, "sender");
            resolve.DefineParameter(2, ParameterAttributes.None, "args");

            var gen = resolve.GetILGenerator();

            gen.DeclareLocal(typeof(AssemblyName));
            gen.DeclareLocal(typeof(string));
            gen.DeclareLocal(typeof(Assembly));

            var cultureIsNull = gen.DefineLabel();
            var ret = gen.DefineLabel();

            Action<string, string, bool> emitCore = (path, ext, useCulture) =>
            {
                gen.Emit(OpCodes.Ldstr, path);
                if (useCulture)
                {
                    gen.Emit(OpCodes.Ldloc_0);
                    gen.Emit(OpCodes.Callvirt, typeof(AssemblyName).GetMethod("get_CultureName"));
                }
                gen.Emit(OpCodes.Ldloc_0);
                gen.Emit(OpCodes.Callvirt, typeof(AssemblyName).GetMethod("get_Name"));
                gen.Emit(OpCodes.Ldstr, ext);
                gen.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }));
                if (useCulture)
                {
                    gen.Emit(OpCodes.Call, typeof(Path).GetMethod("Combine", new[] { typeof(string), typeof(string), typeof(string) }));
                }
                else
                {
                    gen.Emit(OpCodes.Call, typeof(Path).GetMethod("Combine", new[] { typeof(string), typeof(string) }));
                }
                gen.Emit(OpCodes.Stloc_1);

                gen.Emit(OpCodes.Ldloc_1);
                gen.Emit(OpCodes.Call, typeof(File).GetMethod("Exists", new[] { typeof(string) }));
                var bl = gen.DefineLabel();
                gen.Emit(OpCodes.Brfalse_S, bl);

                gen.Emit(OpCodes.Ldloc_1);
                gen.Emit(OpCodes.Call, typeof(Assembly).GetMethod("LoadFile", new[] { typeof(string) }));
                gen.Emit(OpCodes.Stloc_2);
                gen.Emit(OpCodes.Br, ret);

                gen.MarkLabel(bl);
            };

            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Callvirt, typeof(ResolveEventArgs).GetMethod("get_Name"));
            gen.Emit(OpCodes.Newobj, typeof(AssemblyName).GetConstructor(new[] { typeof(string) }));
            gen.Emit(OpCodes.Stloc_0);

            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Callvirt, typeof(AssemblyName).GetMethod("get_CultureInfo"));
            gen.Emit(OpCodes.Brfalse, cultureIsNull);

            emitCore(p, ".dll", true);
            emitCore(p, ".exe", true);

            gen.MarkLabel(cultureIsNull);

            emitCore(p, ".dll", false);
            emitCore(p, ".exe", false);

            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Stloc_2);
            gen.MarkLabel(ret);
            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Ret);

            //TODO:private bin path
        }
    }
}
