/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2022 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace GKNet.Blockchain
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
