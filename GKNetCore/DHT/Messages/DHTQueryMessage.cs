﻿/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2021 by Sergey V. Zhdanovskih.
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

using BencodeNET;

namespace GKNet.DHT
{
    public class DHTQueryMessage : DHTMessage
    {
        public DHTQueryMessage(MessageType type, QueryType queryType, BDictionary data) : base(type, queryType, data)
        {
        }

        protected override void Parse()
        {
            base.Parse();

            string strQueryType = fData.Get<BString>("q").ToString();
            fQueryType = GetQueryType(strQueryType);
        }
    }
}
