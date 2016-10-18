using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing.Emit
{
    internal abstract class BuilderBase
    {
        internal void Save(string path)
        {
            var fileName = Path.GetFileName(path);
            var name = Path.GetFileNameWithoutExtension(fileName);

            var an = new AssemblyName(name);
            an.Version = new Version(1, 0, 0, new Random().Next(0, ushort.MaxValue));

            var asb = path == fileName
                    ? AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Save)
                    : AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Save, Path.GetDirectoryName(path));

            var mod = asb.DefineDynamicModule(name, fileName);

            EmitModule(mod);

            var entryPoint = GetEntryPoint();

            if (entryPoint != null)
            {
                asb.SetEntryPoint(entryPoint, GetPEFileKinds());
            }

            asb.Save(fileName);
        }

        protected virtual MethodInfo GetEntryPoint()
        {
            return null;
        }

        protected virtual PEFileKinds GetPEFileKinds()
        {
            return PEFileKinds.Dll;
        }

        public abstract void EmitModule(ModuleBuilder moduleBuilder);
    }
}
