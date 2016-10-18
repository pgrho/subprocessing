using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing
{
    /// <summary>
    /// <see cref="T:Shipwreck.Subprocessing.DuplexSubprocessHost`2" />のサブプロセスに渡される引数です。
    /// </summary>
    /// <typeparam name="TService">ホストプロセスが公開するサービスの型。</typeparam>
    /// <typeparam name="TCallback">サブプロセスが公開するコールバックの型。</typeparam>
    public interface IDuplexSubprocessStartInfo<out TService, in TCallback> : IServiceSubprocessStartInfo<TService>
        where TService : class
        where TCallback : class
    {
        /// <summary>
        /// ホストプロセスとの接続を行います。
        /// </summary>
        /// <param name="callback">公開するコールバックインターフェイス。</param>
        void Open(TCallback callback);
    }

    /// <summary>
    /// <see cref="T:Shipwreck.Subprocessing.DuplexSubprocessHost`2" />のサブプロセスに渡されるアプリケーション定義のパラメーターを含む引数です。
    /// </summary>
    /// <typeparam name="TService">ホストプロセスが公開するサービスの型。</typeparam>
    /// <typeparam name="TCallback">サブプロセスが公開するコールバックの型。</typeparam>
    /// <typeparam name="TParameter">サブプロセスに渡されるアプリケーション定義のパラメーターの型。</typeparam>
    public interface IDuplexSubprocessStartInfo<out TService, in TCallback, out TParameter>
        : IDuplexSubprocessStartInfo<TService, TCallback>
        , IServiceSubprocessStartInfo<TService, TParameter>
        where TService : class
        where TCallback : class
    {
    }
}
