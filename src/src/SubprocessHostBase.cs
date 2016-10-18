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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Shipwreck.Subprocessing
{
    public abstract class SubprocessHostBase
    {
        private static readonly object StaticLock = new object();
        protected readonly object InstanceLock = new object();

        internal static TraceSource TraceSource = new TraceSource(typeof(SubprocessHostBase).Namespace + ".Host");

        private static string _TempDir;
        private static long _AssemblyCount;
        private static readonly Dictionary<object, string> _EntryPoints = new Dictionary<object, string>();

        internal SubprocessHostBase()
        {
        }

        internal static string TempDir
        {
            get
            {
                InitTempDir();
                return _TempDir;
            }
        }

        private static void InitTempDir()
        {
            lock (StaticLock)
            {
                if (_TempDir != null)
                {
                    return;
                }

                var tempRoot = new DirectoryInfo(Path.Combine(Path.GetTempPath(), typeof(SubprocessHostBase).Namespace));

                if (tempRoot.Exists)
                {
                    var booted = DateTime.Now.AddMilliseconds(-NativeMethods.GetTickCount()).AddDays(-7);

                    foreach (var d in tempRoot.GetDirectories())
                    {
                        if (d.CreationTime < booted)
                        {
                            try
                            {
                                d.Delete(true);
                                TraceSource.TraceEvent(
                                        TraceEventType.Information,
                                        0,
                                        Resources.DeletedTempDirArg0,
                                        d.FullName);
                            }
                            catch (Exception ex)
                            {
                                TraceSource.TraceEvent(
                                        TraceEventType.Warning,
                                        0,
                                        Resources.FailedToDeleteExistingTempDirArg0WithArg1,
                                        d.FullName,
                                        ex);
                            }
                        }
                    }
                }

                var r = new Random();
                for (; ; )
                {
                    var d = Path.Combine(tempRoot.FullName, r.Next().ToString("x8"));

                    try
                    {
                        if (!Directory.Exists(d))
                        {
                            Directory.CreateDirectory(d);
                            _TempDir = d;
                            TemporaryDirectory.CreateSemaphore(d);
                            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                            TraceSource.TraceEvent(
                                    TraceEventType.Information,
                                    0,
                                    Resources.CreatedTempDirArg0,
                                    d);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        TraceSource.TraceEvent(
                                TraceEventType.Warning,
                                0,
                                Resources.FailedToCreateTempDirArg0WithArg1,
                                d,
                                ex);
                    }
                }
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var an = new AssemblyName(args.Name).Name;
            {
                var p = Path.Combine(_TempDir, an + ".dll");
                if (File.Exists(p))
                {
                    return Assembly.LoadFile(p);
                }
            }
            {
                var p = Path.Combine(_TempDir, an + ".exe");
                if (File.Exists(p))
                {
                    return Assembly.LoadFile(p);
                }
            }

            return null;
        }


        protected static MethodInfo ThrowIfInvalidEntryPoint(MulticastDelegate entryPoint)
        {
            if (entryPoint == null)
            {
                throw new ArgumentNullException("entryPoint"); // TODO:nameof
            }
            var invocationList = entryPoint.GetInvocationList();
            if (invocationList.Length != 1)
            {
                throw new ArgumentException("{0}は単一のstaticメソッドである必要があります。", "entryPoint"); // TODO:nameof
            }

            var method = invocationList[0].Method;
            if (!method.IsStatic || !method.IsPublic || (!method.DeclaringType.IsPublic && !method.DeclaringType.IsNestedPublic))
            {
                throw new ArgumentException("{0}は単一のstaticメソッドである必要があります。", "entryPoint"); // TODO:nameof
            }
            return method;
        }

        internal static string GetDynamicEntryPoint(object key, BuilderBase builder)
        {
            var tk = key;
            lock (StaticLock)
            {
                string assemblyFile;
                if (_EntryPoints.TryGetValue(tk, out assemblyFile))
                {
                    return assemblyFile;
                }

                assemblyFile = Path.Combine(TempDir, "E" + Interlocked.Increment(ref _AssemblyCount) + ".T" + DateTime.Now.Ticks + ".exe");
                builder.Save(assemblyFile);

                return assemblyFile;
            }
        }
        internal static string GetDynamicLibrary(object key, BuilderBase builder)
        {
            var tk = key;
            lock (StaticLock)
            {
                string assemblyFile;
                if (_EntryPoints.TryGetValue(tk, out assemblyFile))
                {
                    return assemblyFile;
                }

                assemblyFile = Path.Combine(TempDir, "L" + Interlocked.Increment(ref _AssemblyCount) + ".T" + DateTime.Now.Ticks + ".dll");
                builder.Save(assemblyFile);

                return assemblyFile;
            }
        }



        private static void AppendConfig(ConfigXmlDocument configDocument, ConfigurationUserLevel userLevel)
        {
            var c = ConfigurationManager.OpenExeConfiguration(userLevel);
            if (c.HasFile)
            {
                if (configDocument.DocumentElement == null)
                {
                    configDocument.Load(c.FilePath);
                }
                else
                {
                    var other = new ConfigXmlDocument();
                    other.Load(c.FilePath);

                    {
                        XmlElement usg = null;
                        string usgChildren = null;

                        foreach (XmlElement s in other.SelectNodes("/configuration/configSections/sectionGroup[@name=\"userSettings\"]/section"))
                        {
                            if (usg == null)
                            {
                                usg = (XmlElement)configDocument.SelectSingleNode("/configuration/configSections/sectionGroup[@name=\"userSettings\"]");
                                if (usg == null)
                                {
                                    var cs = configDocument.DocumentElement.GetOrPrepend("configSections");

                                    usg = configDocument.CreateElement("sectionGroup");

                                    foreach (XmlAttribute attr in s.ParentNode.Attributes)
                                    {
                                        usg.SetAttribute(attr.LocalName, attr.NamespaceURI, attr.Value);
                                    }
                                    usg.InnerXml = s.ParentNode.InnerXml;
                                    cs.AppendChild(usg);
                                    break;
                                }
                                usgChildren = usg.InnerXml;
                            }

                            usgChildren += s.OuterXml;
                        }
                        if (usgChildren != null)
                        {
                            usg.InnerXml = usgChildren;
                        }
                    }
                    {
                        XmlElement us = null;
                        foreach (XmlElement se in other.SelectNodes("/configuration/userSettings/*/setting"))
                        {
                            if (us == null)
                            {
                                us = configDocument.DocumentElement.GetOrAppend("userSettings");
                            }
                            var secName = se.ParentNode.LocalName;
                            var ps = us.GetOrAppend(secName);
                            var name = se.GetAttribute("name");
                            var sete = ps.GetByNameOrAppend("setting", name, "name", name, "serializeAs", se.GetAttribute("serializeAs"));
                            sete.InnerXml = se.InnerXml;
                        }
                    }
                }
            }
        }

        protected static Process Start<T>(string executableFileName, T parameter, SubprocessConfiguration configuration, Uri address)
        {
            var configPath = Path.Combine(TempDir, "C" + Interlocked.Increment(ref _AssemblyCount) + ".T" + DateTime.Now.Ticks + ".config");

            var configDocument = new ConfigXmlDocument();
            AppendConfig(configDocument, ConfigurationUserLevel.None);
            AppendConfig(configDocument, ConfigurationUserLevel.PerUserRoaming);
            AppendConfig(configDocument, ConfigurationUserLevel.PerUserRoamingAndLocal);

            if (configuration.ShouldCreateConfig)
            {
                if (configDocument.DocumentElement == null)
                {
                    configDocument.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<configuration></configuration>");
                }
                configDocument.Save(configPath);
                configuration.RaiseConfigCreated(new ConfigCreatedEventArgs(configPath));
            }
            else if (configDocument.DocumentElement != null)
            {
                configDocument.Save(configPath);
            }

            var psi = new ProcessStartInfo(executableFileName);

            var spp = new SubprocessArgument<T>();
            spp.TemporaryDirectory = TempDir;
            spp.Address = address;
            spp.Parameter = parameter;
            spp.ParentProcessId = Process.GetCurrentProcess().Id;
            spp.IsStandalone = configuration.IsStandalone;
            if (configuration.AttachDebugger)
            {
                spp.DebuggerInfo = DebuggerInfoProvider.GetCurrent();
            }

            var argFilePath = Path.Combine(TempDir, "A" + Interlocked.Increment(ref _AssemblyCount) + ".T" + DateTime.Now.Ticks + ".xml");

            using (var fs = new FileStream(argFilePath, FileMode.Create))
            {
                new DataContractSerializer(spp.GetType()).WriteObject(fs, spp);
            }

            psi.Arguments = configDocument.DocumentElement != null
                            ? (argFilePath + " \"" + configPath + "\"")
                            : argFilePath;


            var p = Process.Start(psi);
            return p;
        }
    }
}
