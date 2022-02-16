using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace GKLocations.Core
{
    public static class JsonHelper
    {
        private static JsonSerializerSettings _serializerSettings;

        private static JsonSerializerSettings SerializerSettings
        {
            get {
                if (_serializerSettings == null) {
                    var converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = false } };
                    var resolver = new DefaultContractResolver();
                    _serializerSettings = new JsonSerializerSettings {
                        ContractResolver = resolver,
                        Converters = converters,
                        NullValueHandling = NullValueHandling.Ignore
                    };
                }
                return _serializerSettings;
            }
        }


        public static string SerializeObject(object target)
        {
            return JsonConvert.SerializeObject(target, Formatting.Indented, SerializerSettings);
        }

        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, SerializerSettings);
        }
    }
}