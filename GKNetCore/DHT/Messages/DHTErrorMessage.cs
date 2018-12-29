/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018 by Sergey V. Zhdanovskih.
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

using System.Linq;
using BencodeNET;

namespace GKNet.DHT
{
    public class DHTErrorMessage : DHTMessage
    {
        public long ErrCode { get; private set; }
        public string ErrText { get; private set; }

        public DHTErrorMessage(MessageType type, QueryType queryType, BDictionary data) : base(type, queryType, data)
        {
        }

        protected override void Parse()
        {
            base.Parse();
            var errData = fData.Get<BList>("e");
            if (errData != null && errData.Count != 0) {
                ErrCode = errData.Get<BNumber>(0);
                ErrText = errData.Get<BString>(1).ToString();
            }
        }
    }
}
