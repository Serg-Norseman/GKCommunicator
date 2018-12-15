// Original author: AKFish
// Original home: https://github.com/akfish/ProxyEmitter
// Original license: unknown
// Comment: short version for narrow purposes (for GK)

using System;

namespace GKCore.Proxy
{
    /// <summary>
    /// Specifies the namespace of Proxy methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ProxyNamespaceAttribute : Attribute
    {
        public string Namespace { get; private set; }

        public ProxyNamespaceAttribute(string @namespace)
        {
            Namespace = @namespace;
        }
    }
}
