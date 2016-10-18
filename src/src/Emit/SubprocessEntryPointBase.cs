using Shipwreck.Subprocessing.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing.Emit
{
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class SubprocessEntryPointBase<T>
        where T : SubprocessArgument
    {
        private Process _Parent;
        public void Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                throw new Exception();
            }

            if (args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]))
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", args[1]);
            }

            T p;
            using (var fs = new FileStream(args[0], FileMode.Open))
            {
                p = (T)new DataContractSerializer(typeof(T)).ReadObject(fs);
            }

            OnAppConfigLoaded(p);

            try
            {
                if (p.DebuggerInfo != null)
                {
                    if (DebuggerInfoProvider.AttachTo(p.DebuggerInfo))
                    {
                        SubprocessTraceSource.TraceEvent(TraceEventType.Information, 0, Resources.SucceededInAttachingDebuggerArg0, p.DebuggerInfo);
                    }
                    else
                    {
                        SubprocessTraceSource.TraceEvent(TraceEventType.Warning, 0, Resources.FailedToAttachDebuggerArg0, p.DebuggerInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                SubprocessTraceSource.TraceEvent(TraceEventType.Warning, 0, Resources.FailedToAttachDebuggerArg0WithArg1, p.DebuggerInfo, ex);
            }
            if (p.ParentProcessId > 0 && !p.IsStandalone)
            {
                try
                {
                    _Parent = Process.GetProcessById(p.ParentProcessId);
                    if (_Parent != null)
                    {
                        if (_Parent.HasExited)
                        {
                            return;
                        }
                        _Parent.EnableRaisingEvents = true;
                        _Parent.Exited += Parent_Exited;
                    }
                }
                catch
                {
                    // TODO:ex
                }
            }

            ExecuteCore(p);
        }

        protected virtual void OnAppConfigLoaded(T startInfo)
        {
            SubprocessTraceSource.Initialize();
            if (startInfo.TemporaryDirectory != null)
            {
                TemporaryDirectory.OpenSemaphore(startInfo.TemporaryDirectory);
            }
        }

        private void Parent_Exited(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        protected abstract void ExecuteCore(T startInfo);
    }
}
