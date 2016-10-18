using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing.Emit
{
    [DataContract]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SubprocessArgument
    {
        /// <summary>
        /// ホストプロセスが公開するサービスのエンドポイントURIを取得または設定します。
        /// </summary>
        [DataMember]
        [DefaultValue(null)]
        public Uri Address { get; set; }

        [DataMember]
        [DefaultValue(0)]
        public int ParentProcessId { get; set; }

        [DataMember]
        [DefaultValue(null)]
        public string TemporaryDirectory { get; set; }

        /// <summary>
        /// プロセスが親プロセスの終了後も存続するかどうかを示す値を取得または設定します。
        /// </summary>
        [DataMember]
        [DefaultValue(false)]
        public bool IsStandalone { get; set; }

        [DataMember]
        [DefaultValue(null)]
        public DebuggerInfo DebuggerInfo { get; set; }
    }

    [DataContract]
    public class SubprocessArgument<TParameter> : SubprocessArgument
    {
        /// <summary>
        /// サブプロセスに渡されるアプリケーション定義のパラメーターを取得または設定します。
        /// </summary>
        [DataMember]
        [DefaultValue(null)]
        public TParameter Parameter { get; set; }
    }
}
