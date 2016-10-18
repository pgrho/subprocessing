using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Shipwreck.Subprocessing
{
    internal sealed class SubprocessTraceSource
    {
        private static readonly object lo = new object();

        private static TraceSource _TS;
        private static List<Tuple<TraceEventType, int, string, object[]>> _Pendings;

        internal static void Initialize()
        {
            List<Tuple<TraceEventType, int, string, object[]>> pd;
            lock (lo)
            {
                if (_TS == null)
                {
                    _TS = new TraceSource(typeof(Subprocess).FullName);
                    pd = _Pendings;
                    _Pendings = null;
                }
                else
                {
                    return;
                }
            }
            if (pd == null)
            {
                return;
            }
            foreach (var e in pd)
            {
                if (e.Item4 == null)
                {
                    _TS.TraceEvent(e.Item1, e.Item2, e.Item3);
                }
                else
                {
                    _TS.TraceEvent(e.Item1, e.Item2, e.Item3, e.Item4);
                }
            }
        }

        [Conditional("TRACE")]
        internal static void TraceEvent(TraceEventType eventType, int id, string message)
        {
            TraceSource ts;
            lock (lo)
            {
                ts = _TS;
                if (ts == null)
                {
                    if (_Pendings == null)
                    {
                        _Pendings = new List<Tuple<TraceEventType, int, string, object[]>>();
                    }
                    _Pendings.Add(Tuple.Create(eventType, id, message, (object[])null));
                    return;
                }
            }
            ts.TraceEvent(eventType, id, message);
        }

        [Conditional("TRACE")]
        internal static void TraceEvent(TraceEventType eventType, int id, string format, params object[] args)
        {
            TraceSource ts;
            lock (lo)
            {
                ts = _TS;
                if (ts == null)
                {
                    if (_Pendings == null)
                    {
                        _Pendings = new List<Tuple<TraceEventType, int, string, object[]>>();
                    }
                    _Pendings.Add(Tuple.Create(eventType, id, format, args));
                    return;
                }
            }
            ts.TraceEvent(eventType, id, format, args);
        }
    }
}
