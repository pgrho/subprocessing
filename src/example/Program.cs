using Shipwreck.Subprocessing.Example.Properties;
using Shipwreck.Subprocessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Shipwreck.Subprocessing.Example
{
    public interface IDuplexService
    {
        int Add(int a, int b, [Subprocess] DuplexSubprocess<IDuplexCallback> c = null);
        void Subtract([Subprocess] DuplexSubprocess<IDuplexCallback> c, int a, int b, out int r);
        void Multipy(int a, [Subprocess] DuplexSubprocess<IDuplexCallback> c, int b);
        void Divide(int a, [SubprocessId] int pid, int b, [Subprocess] DuplexSubprocess<IDuplexCallback> c);
    }

    public interface IDuplexCallback
    {
        void Report(int c);
    }

    [DataContract]
    public enum TestEnum
    {
        [EnumMember ]
        aaa,
        [EnumMember]
        bbb,
        [EnumMember]
        ccc
    }

    internal class TestHost : IDuplexService
    {
        public int Add(int a, int b, DuplexSubprocess<IDuplexCallback> c)
        {
            Console.WriteLine("Add:{0} + {1} (PID:{2})", a, b, c.Id);
        Task.Delay (1000).ContinueWith (t=>    c.Callback.Report(a + b));
            return a + b;
        }
        public void Subtract(DuplexSubprocess<IDuplexCallback> c, int a, int b, out int r)
        {
            Console.WriteLine("Subtract:{0} - {1} (PID:{2})", a, b, c.Id);
            Task.Delay(1000).ContinueWith(t => c.Callback.Report(a - b));
            r = a - b;
        }
        public void Multipy(int a, DuplexSubprocess<IDuplexCallback> c, int b)
        {
            Console.WriteLine("Multipy:{0} * {1} (PID:{2})", a, b, c.Id);
            Task.Delay(1000).ContinueWith(t => c.Callback.Report(a * b));
        }
        public void Divide(int a, int pid, int b, DuplexSubprocess<IDuplexCallback> c)
        {
            Console.WriteLine("Divide:{0} / {1} (PID:{2}) (PID:{3})", a, b, pid, c.Id);
            Task.Delay(1000).ContinueWith(t => c.Callback.Report(a / b));
        }
    }
     
    public class Program
    { 
        [STAThread]
        static void Main(string[] args)
        {
            Helper.PrintEntryPoint();
            Application.EnableVisualStyles();
            Application.Run(new ServiceForm());
        }
    }
}
