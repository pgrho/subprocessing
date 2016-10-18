using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing
{
    internal sealed class TemporaryDirectory
    {
        private class Closure
        {
            private readonly Semaphore _Semaphore;
            private readonly string _DirectoryPath;

            internal Closure(Semaphore semaphore, string directoryPath)
            {
                _Semaphore = semaphore;
                _DirectoryPath = directoryPath;
            }

            internal void CurrentDomain_ProcessExit(object sender, EventArgs e)
            {
                try
                {
                    var r = _Semaphore.Release();
                    if (r == int.MaxValue - 1)
                    {
                        // TODO:Windows 7未満対応
                        var psi = new ProcessStartInfo("cmd.exe", "/C TIMEOUT 5 & RD /s /q " + _DirectoryPath);
                        psi.UseShellExecute = false;
                        psi.RedirectStandardError = false;
                        psi.RedirectStandardInput = false;
                        psi.RedirectStandardOutput = false;
                        psi.CreateNoWindow = true;
                        Process.Start(psi);
                    }
                }
                catch { }
            }
        }

        internal static void CreateSemaphore(string directoryPath)
        {
            var s = new Semaphore(int.MaxValue, int.MaxValue, GetSemaporeName(directoryPath));
            s.WaitOne();
            AppDomain.CurrentDomain.ProcessExit += new Closure(s, directoryPath).CurrentDomain_ProcessExit;
        }

        private static string GetSemaporeName(string directoryPath)
        {
            return (typeof(Subprocess).FullName + "/" + directoryPath).Replace(Path.DirectorySeparatorChar, '$').Replace(Path.AltDirectorySeparatorChar, '$');
        }
        internal static void OpenSemaphore(string directoryPath)
        {
            Semaphore s;
            if (Semaphore.TryOpenExisting(GetSemaporeName(directoryPath), out s))
            {
                s.WaitOne();
                AppDomain.CurrentDomain.ProcessExit += new Closure(s, directoryPath).CurrentDomain_ProcessExit;
            }
        }

    }
}
