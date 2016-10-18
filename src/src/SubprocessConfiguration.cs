using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing
{
    public class SubprocessConfiguration
    {
        public SubprocessConfiguration()
        {
#if DEBUG
            AttachDebugger = true;
#endif
            //    ExitOnParentExit = true;
        }

        public event EventHandler<ConfigCreatedEventArgs> ConfigCreated;

        [DefaultValue(false)]
        public bool IsWindowsApplication { get; set; }

        [DefaultValue(false)]
        public bool IsSTAThread { get; set; }

#if DEBUG
        [DefaultValue(true)]
#else
        [DefaultValue(false)]
#endif
        public bool AttachDebugger { get; set; }

        /// <summary>
        /// プロセスが親プロセスの終了後も存続するかどうかを示す値を取得または設定します。
        /// </summary>
        public bool IsStandalone { get; set; }

        internal bool ShouldCreateConfig
        {
            get
            {
                return ConfigCreated != null;
            }
        }

        internal void RaiseConfigCreated(ConfigCreatedEventArgs e)
        {
            var h = ConfigCreated;
            if (h != null)
            {
                h(this, e);
            }
        }
    }
}