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
    /// サービスを公開するサブプロセスのホストです。
    /// </summary>
    /// <typeparam name="TService">ホストプロセスが公開するサービスの型。</typeparam>
    public sealed class ServiceSubprocessHost<TService> : SubprocessHostBase
        where TService : class
    {
        /// <summary>
        /// 構築ジェネリック型に対するロックオブジェクトです。
        /// </summary>
        private static readonly object TypeLock = new object();

        /// <summary>
        /// ユーザーが指定したサービスのシングルトンインスタンスです。
        /// </summary>
        private readonly TService _ServiceInstance;

        /// <summary>
        /// 生成されたサービスコントラクトの型です。
        /// </summary>
        private static Type _GeneratedServiceContractType;

        /// <summary>
        /// 生成されたサービスコントラクトのホスト側ラッパーの型です。
        /// </summary>
        private static Type _HostServiceWrapperType;

        /// <summary>
        /// 生成されたサブプロセス起動情報の型です。
        /// </summary>
        private static Type _StartInfoType;

        /// <summary>
        /// <see cref="OpenTask" />のバッキングストアです。
        /// </summary>
        private Task _OpenTask;

        /// <summary>
        /// 公開するサービスのエンドポイントURIです。
        /// </summary>
        private Uri _Address;

        /// <summary>
        /// サブプロセスに公開するサービスのホストです。
        /// </summary>
        private ServiceHost _ServiceHost;

        /// <summary>
        /// 起動中のサブプロセスのIDとインスタンスのディクショナリです。
        /// </summary>
        private Dictionary<int, Subprocess> _Clients;

        /// <summary>
        /// <see cref="T:Shipwreck.Subprocessing.ServiceSubprocessHost`1" />クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="serviceInstance">サービスのシングルトンインスタンス。</param>
        public ServiceSubprocessHost(TService serviceInstance)
        {
            if (serviceInstance == null)
            {
                throw new ArgumentNullException("serviceInstance"); // TODO:nameof
            }
            _ServiceInstance = serviceInstance;
        }

        /// <summary>
        /// サブプロセスに公開するサービスを開始するタスクを取得します。
        /// </summary>
        private Task OpenTask
        {
            get
            {
                lock (InstanceLock)
                {
                    if (_OpenTask == null)
                    {
                        LoadContractTypes();
                        _Address = new Uri(
                                        string.Format("net.pipe://localhost/{0}/{1}/{2}/",
                                        typeof(ServiceSubprocessHost<>).FullName,
                                        GetHashCode(),
                                        DateTime.Now.Ticks).Replace('`', '_'));

                        var ctor = _HostServiceWrapperType.GetConstructor(new[] { typeof(TService), typeof(Func<int, Subprocess>) });
                        var instance = ctor.Invoke(new object[] { _ServiceInstance, (Func<int, Subprocess>)GetSubprocessById });

                        _ServiceHost = new ServiceHost(instance, _Address);
                        _ServiceHost.AddServiceEndpoint(_GeneratedServiceContractType, WcfHelper.CreateBinding(), _Address);

                        TraceSource.TraceInformation(Resources.OpeningDuplexServiceAtArg0, _Address);

                        _OpenTask = Task.Run(() => _ServiceHost.Open());
                    }
                }
                return _OpenTask;
            }
        }

        #region StartNew メソッド

        /// <summary>
        /// 指定したエントリポイントで新しいサブプロセスを起動します。
        /// </summary>
        /// <param name="entryPoint">サブプロセスの起動後に呼び出されるパブリックな静的メソッド。</param>
        /// <param name="configuration">サブプロセスの起動設定。</param>
        /// <returns>サブプロセスの起動タスク。</returns>
        public Task<Subprocess> StartNew(Action<IServiceSubprocessStartInfo<TService>> entryPoint, SubprocessConfiguration configuration = null)
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
        public Task<Subprocess> StartNew<TParameter>(Action<IServiceSubprocessStartInfo<TService, TParameter>> entryPoint, TParameter parameter, SubprocessConfiguration configuration = null)
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

            return OpenTask.ContinueWith(open =>
            {
                if (!open.IsCompleted)
                {
                    throw new InvalidOperationException(Resources.FailedToInitializeWcfServiceHost, open.Exception);
                }

                var tk = Tuple.Create(typeof(ServiceSubprocessHost<TService>), typeof(TParameter), conf.IsSTAThread, conf.IsWindowsApplication);
                var asf = GetDynamicEntryPoint(
                            tk,
                            new ServiceSubprocessEntryPointBuilder(
                                    typeof(SubprocessArgument<TParameter>),
                                    conf.IsSTAThread,
                                    conf.IsWindowsApplication,
                                    _StartInfoType.MakeGenericType(typeof(TParameter)).GetConstructors()[0],
                                    method));

                var p = Start(asf, parameter, conf, _Address);

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
        /// サービスコントラクトに関連する型を出力して読み込みます。
        /// </summary>
        private static void LoadContractTypes()
        {
            lock (TypeLock)
            {
                if (_HostServiceWrapperType != null)
                {
                    return;
                }
                var b = new ServiceContractBuilder(typeof(TService));
                var af = GetDynamicLibrary(typeof(ServiceSubprocessHost<TService>), b);
                var asm = Assembly.LoadFile(af);
                _HostServiceWrapperType = asm.GetType(ServiceContractBuilder.HOST_SERVICE_WRAPPER);
                _GeneratedServiceContractType = asm.GetType(ServiceContractBuilder.GENERATED_SERVICE_CONTRACT);
                _StartInfoType = asm.GetType(ServiceContractBuilder.START_INFO);
            }
        }

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
