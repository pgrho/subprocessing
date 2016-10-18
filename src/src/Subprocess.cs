using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing
{
    public class Subprocess
    {
        private readonly Process _Process;

        internal Subprocess(Process process)
        {
            if (process == null)
            {
                throw new ArgumentNullException("process"); // TODO:nameof
            }
            _Process = process;
            _Process.EnableRaisingEvents = true;
            _Process.Exited += Process_Exited;
        }

        /// <summary>
        /// プロセスが終了したときに発生します。
        /// </summary>
        [Category("Behavior")]
        [MonitoringDescription("ProcessExited")]
        public event EventHandler Exited;


        /// <summary>
        /// 関連付けられたプロセスの基本優先順位を取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスの<see cref="PriorityClass" />から算出される基本優先順位。
        /// </value>
        /// <exception cref="T:System.InvalidOperationException">
        /// プロセスが終了しています。
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// プロセスは起動していないため、プロセスID がありません。
        /// </exception>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessBasePriority")]
        public int BasePriority
        {
            get
            {
                return _Process.BasePriority;
            }
        }

        /* 
         
 

        //
        // 例外:
        //   System.InvalidOperationException:
        //     プロセスが終了していません。 または プロセス System.Diagnostics.Process.Handle が無効です。 
       
         */

        /// <summary>
        /// 関連付けられたプロセスが終了したときにプロセスによって指定された値を取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスが終了したときにプロセスによって指定されたコード。
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessExitCode")]
        public int ExitCode
        {
            get
            {
                return _Process.ExitCode;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスが終了した時刻を取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスが終了した時刻を示す System.DateTime。
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessExitTime")]
        public DateTime ExitTime
        {
            get
            {
                return _Process.ExitTime;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスのネイティブ ハンドルを取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスを起動したときに、オペレーティング システムがプロセスに割り当てたハンドル。 システムはこのハンドルを使用して、プロセス属性の追跡を続けます。
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessHandle")]
        public IntPtr Handle
        {
            get
            {
                return _Process.Handle;
            }
        }

        /// <summary>
        /// プロセスが開いたハンドルの数を取得します。
        /// </summary>
        /// <value>
        /// プロセスが開いたオペレーティング システム ハンドルの数。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessHandleCount")]
        public int HandleCount
        {
            get
            {
                return _Process.HandleCount;
            }
        }

        /// <summary>
        /// 関連付けられているプロセスが終了したかどうかを示す値を取得します。
        /// </summary>
        /// <value>
        /// <see cref="Subprocess" />コンポーネントが参照するオペレーティング システム プロセスが終了している場合は<c>true</c>。それ以外の場合は<c>false</c>。
        /// </value>
        /// <exception cref="T:System.ComponentModel.Win32Exception">
        /// プロセスの終了コードを取得できませんでした。
        /// </exception>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessTerminated")]
        public bool HasExited
        {
            get
            {
                return _Process.HasExited;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスの一意な識別子を取得します。
        /// </summary>
        /// <value>
        /// この<see cref="Subprocess" />インスタンスが参照する、システムが生成したプロセスの一意な識別子。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessId")]
        public int Id
        {
            get
            {
                return _Process.Id;
            }
        }

        /*
 

        //
        // 例外: 
        //   System.ComponentModel.Win32Exception:
        //     32 ビット プロセスが、64 ビット プロセスのモジュールにアクセスを試みています。
        // 
        //   System.InvalidOperationException:
        //     プロセス System.Diagnostics.Process.Id は使用できません。または、プロセスが終了しています。

                */


        /// <summary>
        /// 関連付けられたプロセスのメイン モジュールを取得します。
        /// </summary>
        /// <value>
        /// プロセスを起動するときに使用した System.Diagnostics.ProcessModule。
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessMainModule")]
        public ProcessModule MainModule
        {
            get
            {
                return _Process.MainModule;
            }
        }



        /* 
       

        //
        // 例外:
        //   System.ArgumentException:
        //     最大ワーキング セット サイズが無効です。 サイズは、最小作業セット サイズ以上でなければなりません。
        //
        //   System.ComponentModel.Win32Exception:
        //     ワーキング セット情報が、関連付けられたプロセスのリソースから取得できません。 または プロセスが起動されていないので、プロセス識別子またはプロセス
        //     ハンドルが 0 です。
        // 
        //   System.InvalidOperationException:
        //     プロセス System.Diagnostics.Process.Id を使用できません。 または プロセスが終了しています。
        // 
                */

        /// <summary>
        /// 関連付けられたプロセスに許可されるワーキング セットの最大サイズを取得または設定します。
        /// </summary>
        /// <value>
        /// プロセスに許可されるメモリ上のワーキング セットの最大サイズ (バイト単位)。
        /// </value>

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessMaxWorkingSet")]
        public IntPtr MaxWorkingSet
        {
            get
            {
                return _Process.MaxWorkingSet;
            }
            set
            {

                _Process.MaxWorkingSet = value;
            }
        }




        /* 

        //
        // 例外:
        //   System.ArgumentException:
        //     最小ワーキング セット サイズが無効です。 サイズは、最大作業セット サイズ以下でなければなりません。
        //
        //   System.ComponentModel.Win32Exception:
        //     ワーキング セット情報が、関連付けられたプロセスのリソースから取得できません。 または プロセスが起動されていないので、プロセス識別子またはプロセス
        //     ハンドルが 0 です。
        // 
        //   System.InvalidOperationException:
        //     プロセス System.Diagnostics.Process.Id を使用できません。 または プロセスが終了しています。 
      */
        /// <summary>
        /// 関連付けられたプロセスに許可されるワーキング セットの最小サイズを取得または設定します。
        /// </summary>
        /// <value>
        /// プロセスに必要なメモリ上の最小ワーキング セット サイズ (バイト単位)。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessMinWorkingSet")]
        public IntPtr MinWorkingSet
        {
            get
            {
                return _Process.MinWorkingSet;
            }
            set
            {

                _Process.MinWorkingSet = value;
            }
        }





        /* 

        //
        // 例外: 
        //   System.InvalidOperationException:
        //     プロセス System.Diagnostics.Process.Id を使用できません。
        // 
        //   System.ComponentModel.Win32Exception:
        //     システム プロセスまたはアイドル プロセスの System.Diagnostics.Process.Modules プロパティにアクセスしようとしています。
        //     これらのプロセスにはモジュールが存在しません。

         * 
         * 
         *         */
        /// <summary>
        /// 関連付けられたプロセスに読み込まれたモジュールを取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスに読み込まれたモジュールを表す System.Diagnostics.ProcessModule 型の配列。
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessModules")]
        public ProcessModuleCollection Modules
        {
            get
            {
                return _Process.Modules;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスに割り当てられたページングされないシステム メモリの量を取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスに割り当てられた、仮想メモリ ページング ファイルに書き込むことができないシステム メモリの容量 (バイト単位)。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessNonpagedSystemMemorySize")]
        public long NonpagedSystemMemorySize
        {
            get
            {
                return _Process.NonpagedSystemMemorySize64;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスに割り当てられたページングされるシステム メモリの量を取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスの仮想メモリ ページング ファイル内で割り当てられたメモリの量 (バイト単位)。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessPagedMemorySize")]
        public long PagedMemorySize
        {
            get
            {
                return _Process.PagedMemorySize64;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスに割り当てられたページング可能なシステム メモリの量を取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスに割り当てられた、仮想メモリ ページング ファイルに書き込むことができるシステム メモリの容量 (バイト単位)。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessPagedSystemMemorySize")]
        public long PagedSystemMemorySize
        {
            get
            {
                return _Process.PagedSystemMemorySize64;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスによって使用される、仮想メモリ ページング ファイル内のメモリの最大量を取得します。
        /// </summary>
        /// <value>
        /// プロセスの開始以降、関連付けられたプロセスの仮想メモリ ページング ファイル内で割り当てられたメモリの最大量 (バイト単位)。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessPeakPagedMemorySize")]
        public long PeakPagedMemorySize
        {
            get
            {
                return _Process.PeakPagedMemorySize64;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスによって使用される仮想メモリの最大量を取得します。
        /// </summary>
        /// <value>
        /// プロセスの開始以降、関連付けられたプロセスに割り当てられた仮想メモリの最大量 (バイト単位)。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessPeakVirtualMemorySize")]
        public long PeakVirtualMemorySize
        {
            get
            {
                return _Process.PeakVirtualMemorySize64;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスによって使用される物理メモリの最大量を取得します。
        /// </summary>
        /// <value>
        /// プロセスの開始以降、関連付けられたプロセスに割り当てられた物理メモリの最大量 (バイト単位)。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessPeakWorkingSet")]
        public long PeakWorkingSet
        {
            get
            {
                return _Process.PeakWorkingSet64;
            }
        }


        /* 

        //
        // 例外:
        //   System.ComponentModel.Win32Exception:
        //     優先順位を上げる情報が、関連付けられたプロセスのリソースから取得できませんでした。
        // 
        //   System.InvalidOperationException:
        //     プロセス System.Diagnostics.Process.Id を使用できません。
             */
        /// <summary>
        /// メイン ウィンドウのフォーカス時に、オペレーティング システムによって関連付けられたプロセスの優先順位を一時的に上げるかどうかを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// 待機状態から抜けたときにプロセスの優先順位を動的に上げる場合は true。それ以外の場合は false。 既定値は、false です。
        /// </value>


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessPriorityBoostEnabled")]
        public bool PriorityBoostEnabled
        {
            get
            {
                return _Process.PriorityBoostEnabled;
            }
            set
            {

                _Process.PriorityBoostEnabled = value;
            }
        }




        /*  
        //
        // 例外:
        //   System.ComponentModel.Win32Exception:
        //     プロセス優先順位情報が設定できませんでした。または、関連付けられたプロセスのリソースから取得できませんでした。 または プロセス ID またはプロセス
        //     ハンドルがゼロです。 (プロセスはまだ開始していません。)
        // 
        //   System.InvalidOperationException:
        //     プロセス System.Diagnostics.Process.Id を使用できません。
        // 
        //   System.ComponentModel.InvalidEnumArgumentException:
        //     System.Diagnostics.ProcessPriorityClass 列挙体で定義されている有効な値を使用していないため、優先順位クラスを設定できません。

        */
        /// <summary>
        /// 関連付けられたプロセスの全体的な優先順位カテゴリを取得または設定します。
        /// </summary>
        /// <value>
        /// プロセスの System.Diagnostics.Process.BasePriority を計算するときに使用する、関連付けられたプロセスの優先順位カテゴリ。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessPriorityClass")]
        public ProcessPriorityClass PriorityClass
        {
            get
            {
                return _Process.PriorityClass;
            }
            set
            {
                _Process.PriorityClass = value;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスに割り当てられたプライベート メモリの量を取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスに割り当てられ、他のプロセスと共有できないメモリの量 (バイト単位)。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessPrivateMemorySize")]
        public long PrivateMemorySize
        {
            get
            {
                return _Process.PrivateMemorySize64;
            }
        }

        /// <summary>
        /// このプロセスの特権プロセッサ時間を取得します。
        /// </summary>
        /// <value>
        /// プロセスが、オペレーティング システム コアでコードを実行した時間を示す System.TimeSpan。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessPrivilegedProcessorTime")]
        public TimeSpan PrivilegedProcessorTime
        {
            get
            {
                return _Process.PrivilegedProcessorTime;
            }
        }

        /// <summary>
        /// このプロセスでのスレッドの実行をスケジュールできるプロセッサを取得または設定します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスのスレッドを実行できるプロセッサを示すビットマスク。 既定値は、コンピューターのプロセッサ数によって異なります。 既定値は2 n -1 で、n はプロセッサ数です
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessProcessorAffinity")]
        public IntPtr ProcessorAffinity
        {
            get
            {
                return _Process.ProcessorAffinity;
            }
            set
            {

                _Process.ProcessorAffinity = value;
            }
        }

        /// <summary>
        /// プロセスのユーザー インターフェイスが応答するかどうかを示す値を取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスのユーザー インターフェイスがシステムに応答する場合は true。それ以外の場合は false。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessResponding")]
        public bool Responding
        {
            get
            {
                return _Process.Responding;
            }
        }


        /*     

        //
        // 例外: 
        //   System.InvalidOperationException:
        //     プロセスが終了しています。
        //
        //   System.ComponentModel.Win32Exception:
        //     Windows 関数への呼び出しでエラーが発生しました。
       */

        /// <summary>
        /// 関連付けられたプロセスが起動された時刻を取得します。
        /// </summary>
        /// <value>
        /// プロセスが起動された時刻を示す System.DateTime。 起動されたプロセスでだけ有効です。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessStartTime")]
        public DateTime StartTime
        {
            get
            {
                return _Process.StartTime;
            }
        }

        /// <summary>
        /// プロセス終了イベントの結果として発行されるイベント ハンドラー呼び出しをマーシャリングするために使用するオブジェクトを取得または設定します。
        /// </summary>
        /// <value>
        /// プロセスの System.Diagnostics.Process.Exited イベントの結果として発行されるイベント ハンドラー呼び出しをマーシャリングするために使用するSystem.ComponentModel.ISynchronizeInvoke。
        /// </value>
        [Browsable(false)]
        [DefaultValue(null)]
        [MonitoringDescription("ProcessSynchronizingObject")]
        public ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                return _Process.SynchronizingObject;
            }
            set
            {

                _Process.SynchronizingObject = value;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスで実行されているスレッドのセットを取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスで現在実行中のオペレーティング システム スレッドを表す System.Diagnostics.ProcessThread 型の配列。
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessThreads")]
        public ProcessThreadCollection Threads
        {
            get
            {
                return _Process.Threads;
            }
        }

        /// <summary>
        /// このプロセスの合計プロセッサ時間を取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスが CPU を使用した時間を示す System.TimeSpan。 この値は、System.Diagnostics.Process.UserProcessorTime と System.Diagnostics.Process.PrivilegedProcessorTime の合計です
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessTotalProcessorTime")]
        public TimeSpan TotalProcessorTime
        {
            get
            {
                return _Process.TotalProcessorTime;
            }
        }

        /// <summary>
        /// このプロセスのユーザー プロセッサ時間を取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスが、プロセスのアプリケーション部分の内部 (オペレーティング システム コアの外部) でコードを実行した時間を示す System.TimeSpan。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessUserProcessorTime")]
        public TimeSpan UserProcessorTime
        {
            get
            {
                return _Process.UserProcessorTime;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスに割り当てられた仮想メモリの量を取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスに割り当てられた仮想メモリの量 (バイト単位)。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessVirtualMemorySize")]
        public long VirtualMemorySize
        {
            get
            {
                return _Process.VirtualMemorySize64;
            }
        }

        /// <summary>
        /// 関連付けられたプロセスに割り当てられた物理メモリの量を取得します。
        /// </summary>
        /// <value>
        /// 関連付けられたプロセスに割り当てられた物理メモリの量 (バイト単位)。
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("ProcessWorkingSet")]
        public long WorkingSet
        {
            get
            {
                return _Process.WorkingSet64;
            }
        }




        /* 
         
       // 概要:
        //     このコンポーネントに関連付けられているすべてのリソースを解放します。
        public void Close();
 
 
       
       /// <summary>
/// 関連付けられたプロセスを即時中断します。
/// </summary>
        //
        // 例外:
        //   System.ComponentModel.Win32Exception:
        //     関連付けられたプロセスを終了できませんでした。 または プロセスは終了中です。 または 関連付けられたプロセスは Win16 実行可能ファイルです。
        // 
        //   System.InvalidOperationException:
        //     プロセスが既に終了しています。 または System.Diagnostics.Process オブジェクトに関連付けられているプロセスがありません。
        public void Kill(); 
         */

        /// <summary>
        /// プロセス コンポーネントにキャッシュされている関連付けられたプロセスに関するすべての情報を破棄します。
        /// </summary>
        public void Refresh()
        {
            _Process.Refresh();
        }

        /* 
       /// <summary>
/// プロセス名の書式指定は文字列にします。親コンポーネント型があれば、この型と組み合わせます。
/// </summary>
       /// <value>
/// ベース コンポーネントの System.Object.ToString() の戻り値と組み合わせた System.Diagnostics.Process.ProcessName。
/// </value>
        //
        // 例外:
        //   System.PlatformNotSupportedException:
        //     System.Diagnostics.Process.ToString() は、Windows 98 ではサポートされていません。
        public override string ToString();
       /// <summary>
/// 関連付けられたプロセスが終了するまで無期限に待機するように System.Diagnostics.Process コンポーネントに指示します。
/// </summary>
        //
        // 例外:
        //   System.ComponentModel.Win32Exception:
        //     待機設定にアクセスできませんでした。
        //
        //   System.SystemException:
        //     プロセス System.Diagnostics.Process.Id が設定されておらず、System.Diagnostics.Process.Id
        //     プロパティの判断材料となる System.Diagnostics.Process.Handle が存在しません。 または System.Diagnostics.Process
        //     オブジェクトに関連付けられているプロセスがありません。 または リモート コンピューターで実行されているプロセスに対して System.Diagnostics.Process.WaitForExit()
        //     を呼び出そうとしています。 このメソッドが利用できるのは、ローカル コンピューターで実行されているプロセスだけです。
        public void WaitForExit();
       /// <summary>
/// 関連付けられたプロセスが終了するまで、最大指定したミリ秒間待機するように System.Diagnostics.Process コンポーネントに指示します。
/// </summary>
        //
        // パラメーター:
        //   milliseconds:
        //     関連付けられたプロセスが終了するまで待機する時間。単位はミリ秒です。 最大値は、32 ビット整数で表現できる最大値で、オペレーティング システムに対して無限大で表現される値です。
       /// <value>
/// 関連付けられたプロセスが終了した場合は true。それ以外の場合は false。
/// </value>
        //
        // 例外:
        //   System.ComponentModel.Win32Exception:
        //     待機設定にアクセスできませんでした。
        //
        //   System.SystemException:
        //     プロセス System.Diagnostics.Process.Id が設定されておらず、System.Diagnostics.Process.Id
        //     プロパティの判断材料となる System.Diagnostics.Process.Handle が存在しません。 または System.Diagnostics.Process
        //     オブジェクトに関連付けられているプロセスがありません。 または リモート コンピューターで実行されているプロセスに対して System.Diagnostics.Process.WaitForExit(System.Int32)
        //     を呼び出そうとしています。 このメソッドが利用できるのは、ローカル コンピューターで実行されているプロセスだけです。
        public bool WaitForExit(int milliseconds);
       /// <summary>
/// 関連付けられたプロセスがアイドル状態になるまで、System.Diagnostics.Process コンポーネントを無期限に待機させます。 このオーバーロードは、ユーザー
/// </summary>
        //     インターフェイスとメッセージ ループを持つプロセスにだけ適用されます。
       /// <value>
/// 関連付けられたプロセスがアイドル状態になった場合は true。
/// </value>
        //
        // 例外:
        //   System.InvalidOperationException:
        //     このプロセスには、グラフィカル インターフェイスはありません。 または 不明なエラーが発生しました。 プロセスがアイドル状態への移行に失敗しました。
        //     または プロセスが既に終了しています。 または この System.Diagnostics.Process オブジェクトにはプロセスが関連付けられていません。
        public bool WaitForInputIdle();
       /// <summary>
/// 関連付けられたプロセスがアイドル状態になるまで、最大指定したミリ秒間、System.Diagnostics.Process コンポーネントを待機させます。
/// </summary>
        //     このオーバーロードは、ユーザー インターフェイスとメッセージ ループを持つプロセスにだけ適用されます。
        //
        // パラメーター:
        //   milliseconds:
        //     関連付けられたプロセスがアイドル状態になるまでの待機時間をミリ秒単位で指定する、1 ～ System.Int32.MaxValue の値。 値 0
        //     の場合はすぐに制御が戻され、値 -1 の場合は無期限に待機することを示します。
       /// <value>
/// 関連付けられたプロセスがアイドル状態になった場合は true。それ以外の場合は false。
/// </value>
        //
        // 例外:
        //   System.InvalidOperationException:
        //     このプロセスには、グラフィカル インターフェイスはありません。 または 不明なエラーが発生しました。 プロセスがアイドル状態への移行に失敗しました。
        //     または プロセスが既に終了しています。 または この System.Diagnostics.Process オブジェクトにはプロセスが関連付けられていません。
        public bool WaitForInputIdle(int milliseconds);
         */

        /// <summary>
        /// サブプロセスの終了時の処理を行います。
        /// </summary>
        /// <param name="sender">イベントソース。</param>
        /// <param name="e">イベントデータ。</param>
        private void Process_Exited(object sender, EventArgs e)
        {
            var h = Exited;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

    }
}
