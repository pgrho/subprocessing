using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Subprocessing
{
    /// <summary>
    /// サービスを公開するホストのサブプロセスに渡される引数です。
    /// </summary>
    /// <typeparam name="TService">ホストプロセスが公開するサービスの型。</typeparam>
    public interface IServiceSubprocessStartInfo<out TService>
        where TService : class
    {
        /// <summary>
        /// ホストプロセスとの接続が行われているかどうかを示す値を取得します。
        /// </summary>
        bool IsOpened { get; }

        /// <summary>
        /// ホストプロセスが公開するサービスを取得します。
        /// </summary>
        TService Service { get; }
    }

    /// <summary>
    /// サービスを公開するホストのサブプロセスに渡されるアプリケーション定義のパラメーターを含む引数です。
    /// </summary>
    /// <typeparam name="TService">ホストプロセスが公開するサービスの型。</typeparam>
    /// <typeparam name="TParameter">サブプロセスに渡されるアプリケーション定義のパラメーターの型。</typeparam>
    public interface IServiceSubprocessStartInfo<out TService, out TParameter> : IServiceSubprocessStartInfo<TService>
        where TService : class
    {
        /// <summary>
        /// サブプロセスに渡されたアプリケーション定義のパラメーターを取得します。
        /// </summary>
        TParameter Parameter { get; }
    }
}
