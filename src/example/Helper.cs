using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing.Example
{
    internal static class Helper
    {
        internal static void PrintEntryPoint()
        {
            Console.WriteLine("Process ID:{0}", Process.GetCurrentProcess().Id);
            Console.WriteLine("ApartmentState:{0}", Thread.CurrentThread.GetApartmentState());
            Console.WriteLine("AppSettingItem:{0}", Properties.Settings.Default.AppSettingItem);
            Console.WriteLine("UserSettingItem:{0}", Properties.Settings.Default.UserSettingItem);
            var st = new StackTrace();
            var ep = Assembly.GetEntryAssembly().EntryPoint;

            Console.WriteLine("Current StackTrace:");
            for (var i = 1; i < st.FrameCount; i++)
            {
                var m = st.GetFrame(i).GetMethod();
                Console.WriteLine(m);
                if (m == ep)
                {
                    break;
                }
            }

        }
    }
}
