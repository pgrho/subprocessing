using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing.Emit
{
    internal static class EmitHelper
    {
        public static ConstructorInfo ServiceContractConstructor
        {
            get
            {
                return typeof(ServiceContractAttribute).GetConstructor(Type.EmptyTypes);
            }
        }
        public static ConstructorInfo OperationContractConstructor
        {
            get
            {
                return typeof(OperationContractAttribute).GetConstructor(Type.EmptyTypes);
            }
        }

        public static PropertyInfo IsOneWay
        {
            get
            {
                return typeof(OperationContractAttribute).GetProperty("IsOneWay");
            }
        }

        internal static MethodInfo GetTypeFromHandle
        {
            get
            {
                return typeof(Type).GetMethod("GetTypeFromHandle");
            }
        }

        internal static bool CanBeOneWay(this MethodInfo method)
        {
            return method.ReturnType == typeof(void)
                && method.GetParameters().All(_ => !_.ParameterType.IsByRef);
        }

        internal static void EmitLdArg(this ILGenerator gen, int offset)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            switch (offset)
            {
                case 0:
                    gen.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    gen.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    gen.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    gen.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (offset <= byte.MaxValue)
                    {
                        gen.Emit(OpCodes.Ldarg_S, (byte)offset);
                    }
                    else if (offset <= ushort.MaxValue)
                    {
                        gen.Emit(OpCodes.Ldarg, (ushort)offset);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    break;
            }
        }
        internal static void EmitLdArga(this ILGenerator gen, int offset)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (offset <= byte.MaxValue)
            {
                gen.Emit(OpCodes.Ldarga_S, (byte)offset);
            }
            else if (offset <= ushort.MaxValue)
            {
                gen.Emit(OpCodes.Ldarga, (ushort)offset);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        internal static CustomAttributeBuilder GetAttributeBuilder<T>() where T : Attribute, new()
        {
            return new CustomAttributeBuilder(typeof(T).GetConstructor(Type.EmptyTypes), new object[0]);
        }
    }
}
