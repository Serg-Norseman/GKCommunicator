using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#if NETSTANDARD
using System;
using System.Reflection;
#endif

namespace BencodeNET
{
    internal static class UtilityExtensions
    {
        public static bool IsDigit(this char c)
        {
            return c >= '0' && c <= '9';
        }

        public static MemoryStream AsStream(this string str, Encoding encoding)
        {
            return new MemoryStream(encoding.GetBytes(str));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }

#if NETSTANDARD
        public static bool IsAssignableFrom(this Type type, Type otherType)
        {
            return type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
        }
#endif
    }
}
