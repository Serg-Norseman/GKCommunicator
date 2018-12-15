// Original author: AKFish
// Original home: https://github.com/akfish/ProxyEmitter
// Original license: unknown
// Comment: short version for narrow purposes (for GK)

namespace GKCore.Proxy
{
    /// <summary>
    /// Base class for all Proxy class
    /// </summary>
    public abstract class ProxyBase
    {
        /// <summary>
        /// Invoke method
        /// </summary>
        /// <param name="namespace">Namespace of the method to be invoked</param>
        /// <param name="methodName">Method to be invoked</param>
        /// <param name="arguments">Argument list for the invoked method</param>
        /// <returns></returns>
        protected abstract object Invoke(string @namespace, string methodName, object[] arguments);

        /// <summary>
        /// Convert return value of <see cref="Invoke"/> method to a specific type
        /// </summary>
        /// <typeparam name="TRet">Type of the return value</typeparam>
        /// <param name="returnValue">Return value of <see cref="Invoke"/> method</param>
        /// <returns></returns>
        protected abstract TRet ConvertReturnValue<TRet>(object returnValue);
    }
}
