using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing.Emit
{
    /// <summary>
    /// <see cref="T:Shipwreck.Subprocessing.DuplexSubprocessHost`2" />のサブプロセス起動情報の基底クラスです。
    /// </summary>
    /// <typeparam name="TUserService">ユーザーが指定したホストプロセスが公開するサービスの型。</typeparam>
    /// <typeparam name="TGeneratedService">生成されたサービスコントラクトの型。</typeparam>
    /// <typeparam name="TUserCallback">ユーザーが指定したサブプロセスが公開するコールバックの型。</typeparam>
    /// <typeparam name="TGeneratedCallback">生成されたコールバックコントラクトの型。</typeparam>
    /// <typeparam name="TParameter">サブプロセスに渡されるアプリケーション定義のパラメーターの型。</typeparam>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class DuplexSubprocessStartInfoBase<TUserService, TGeneratedService, TUserCallback, TGeneratedCallback, TParameter>
        : IDuplexSubprocessStartInfo<TUserService, TUserCallback, TParameter>
        where TUserService : class
        where TGeneratedService : class
        where TUserCallback : class
        where TGeneratedCallback : class
    {
        /// <summary>
        /// ホストプロセスのサービスに接続するWCFクライアントです。
        /// </summary>
        private sealed class Client : DuplexClientBase<TGeneratedService>
        {
            /// <summary>
            /// <see cref="Client" />クラスの新しいインスタンスを初期化します。
            /// </summary>
            /// <param name="callback">コールバックコントラクトのインスタンス。</param>
            /// <param name="address">サービスのエンドポイントURI。</param>
            internal Client(TGeneratedCallback callback, Uri address)
                : base(
                    new InstanceContext(callback),
                    new ServiceEndpoint(
                            ContractDescription.GetContract(typeof(TGeneratedService)),
                            WcfHelper.CreateBinding(),
                            new EndpointAddress(address)))
            { }
        }

        /// <summary>
        /// ホストプロセスが公開するサービスのエンドポイントURIです。
        /// </summary>
        private readonly Uri _Address;

        /// <summary>
        /// <see cref="Service" />のバッキングストアです。
        /// </summary>
        private TUserService _Service;

        /// <summary>
        /// <see cref="Parameter" />のバッキングストアです。
        /// </summary>
        private readonly TParameter _Parameter;

        /// <summary>
        /// <see cref="T:Shipwreck.Subprocessing.Emit.DuplexSubprocessStartInfoBase`5" />クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="argument">サブプロセスに渡された起動引数。</param>
        public DuplexSubprocessStartInfoBase(SubprocessArgument<TParameter> argument)
        {
            _Address = argument.Address;
            _Parameter = argument.Parameter;
        }

        /// <inheritdoc />
        public TParameter Parameter
        {
            get
            {
                return _Parameter;
            }
        }

        /// <inheritdoc />
        public TUserService Service
        {
            get
            {
                return _Service;
            }
        }

        /// <inheritdoc />
        public bool IsOpened
        {
            get
            {
                return _Service != null;
            }
        }

        /// <inheritdoc />
        public void Open(TUserCallback callback)
        {
            if (_Service != null)
            {
                return;
            }

            var dc = new Client(CreateCallbackWrapper(callback), _Address);
            dc.Open();
            var hi = dc.ChannelFactory.CreateChannel();
            _Service = CreateServiceWrapper(hi);
        }

        /// <summary>
        /// ホストプロセスの内部コントラクト型をユーザーの指定したインターフェイスでラップします。
        /// </summary>
        /// <param name="service">ラップする内部コントラクト型のインスタンス。</param>
        /// <returns>ラップされたユーザーコントラクト型のインスタンス。</returns>
        protected abstract TUserService CreateServiceWrapper(TGeneratedService service);

        /// <summary>
        /// ユーザーの指定したコールバックインターフェイスをホストプロセスが生成したコントラクトインターフェイスでラップします。
        /// </summary>
        /// <param name="callback">ユーザーの指定したコールバックコントラクトのインスタンス。</param>
        /// <returns>ラップされた内部コールバックコントラクト型のインスタンス。</returns>
        protected abstract TGeneratedCallback CreateCallbackWrapper(TUserCallback callback);
    }
}
