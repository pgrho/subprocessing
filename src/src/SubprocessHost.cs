using Shipwreck.Subprocessing.Emit;
using Shipwreck.Subprocessing.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing
{
    /// <summary>
    /// サブプロセスの生成と起動を行います。
    /// </summary>
    public sealed class SubprocessHost : SubprocessHostBase
    {
        /// <summary>
        /// 構築ジェネリック型に対するロックオブジェクトです。
        /// </summary>
        private static readonly object TypeLock = new object();

        /// <summary>
        /// 起動中のサブプロセスのIDとインスタンスのディクショナリです。
        /// </summary>
        private Dictionary<int, Subprocess> _Clients;

        #region StartNew メソッド

        /// <summary>
        /// 指定したエントリポイントで新しいサブプロセスを起動します。
        /// </summary>
        /// <param name="entryPoint">サブプロセスの起動後に呼び出されるパブリックな静的メソッド。</param>
        /// <param name="configuration">サブプロセスの起動設定。</param>
        /// <returns>サブプロセスの起動タスク。</returns>
        public Task<Subprocess> StartNew(Action entryPoint, SubprocessConfiguration configuration = null)
        {
            ThrowIfInvalidEntryPoint(entryPoint);
            return StartNewCore<object>(entryPoint.GetInvocationList()[0].Method, null, configuration);
        }

        /// <summary>
        /// アプリケーション定義のパラメーターとエントリポイントを指定して新しいサブプロセスを起動します。
        /// </summary>
        /// <typeparam name="TParameter">サブプロセスに渡されるアプリケーション定義のパラメーターの型。</typeparam>
        /// <param name="entryPoint">サブプロセスの起動後に呼び出されるパブリックな静的メソッド。</param>
        /// <param name="parameter">サブプロセスに渡されるアプリケーション定義のパラメーター。</param>
        /// <param name="configuration">サブプロセスの起動設定。</param>
        /// <returns>サブプロセスの起動タスク。</returns>
        public Task<Subprocess> StartNew<TParameter>(Action<TParameter> entryPoint, TParameter parameter, SubprocessConfiguration configuration = null)
        {
            ThrowIfInvalidEntryPoint(entryPoint);
            return StartNewCore(entryPoint.GetInvocationList()[0].Method, parameter, configuration);
        }

        /// <summary>
        /// アプリケーション定義のパラメーターとエントリポイントを指定して新しいサブプロセスを起動します。
        /// </summary>
        /// <typeparam name="TParameter">サブプロセスに渡されるアプリケーション定義のパラメーターの型。</typeparam>
        /// <param name="method">サブプロセスの起動後に呼び出されるパブリックな静的メソッド。</param>
        /// <param name="parameter">サブプロセスに渡されるアプリケーション定義のパラメーター。</param>
        /// <param name="configuration">サブプロセスの起動設定。</param>
        /// <returns>サブプロセスの起動タスク。</returns>
        private Task<Subprocess> StartNewCore<TParameter>(MethodInfo method, TParameter parameter, SubprocessConfiguration configuration)
        {
            var conf = configuration ?? new SubprocessConfiguration();

            return Task.Run(() =>
            {
                var tk = Tuple.Create(typeof(SubprocessHost), typeof(TParameter), conf.IsSTAThread, conf.IsWindowsApplication);
                var asf = GetDynamicEntryPoint(
                            tk,
                            new SubprocessEntryPointBuilder(
                                    typeof(SubprocessArgument<TParameter>),
                                    conf.IsSTAThread,
                                    conf.IsWindowsApplication, 
                                    method));

                var p = Start(asf, parameter, conf, null);

                lock (InstanceLock)
                {
                    if (_Clients == null)
                    {
                        _Clients = new Dictionary<int, Subprocess>();
                    }
                    var dsi = new Subprocess(p);
                    dsi.Exited += Process_Exited;
                    return _Clients[p.Id] = dsi;
                }
            });
        }

        #endregion StartNew メソッド

        /// <summary>
        /// 指定したプロセスIDのサブプロセス情報を返します。
        /// </summary>
        /// <param name="processId">サブプロセスのプロセスID。</param>
        /// <returns>サブプロセス情報。該当する情報が存在しない場合は<c>null</c>。</returns>
        private Subprocess GetSubprocessById(int processId)
        {
            lock (InstanceLock)
            {
                if (_Clients == null)
                {
                    return null;
                }
                Subprocess c;
                _Clients.TryGetValue(processId, out c);
                return c;
            }
        }

        #region イベントハンドラー

        /// <summary>
        /// サブプロセスの終了時の処理を行います。
        /// </summary>
        /// <param name="sender">イベントソース。</param>
        /// <param name="e">イベントデータ。</param>
        private void Process_Exited(object sender, EventArgs e)
        {
            lock (InstanceLock)
            {
                if (_Clients != null)
                {
                    _Clients.Remove(((Subprocess)sender).Id);
                }
            }
        }

        #endregion イベントハンドラー
    }
}
