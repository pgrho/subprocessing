using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing
{
    public sealed class DuplexSubprocess<TCallback> : Subprocess
        where TCallback : class
    {
        internal DuplexSubprocess(Process process)
            : base(process)
        {
        }

        /// <summary>
        /// サブプロセスに関連付けられたコールバックを取得します。
        /// </summary>
        /// <value>
        /// サブプロセスに関連付けられたコールバックインターフェイス。
        /// 接続が行われていない場合は<c>null</c>。
        /// </value>
        public TCallback Callback { get; internal set; }
    }
}
