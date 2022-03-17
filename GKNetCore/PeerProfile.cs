/*
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

using System;
using BencodeNET;
using GKNet.DHT;

namespace GKNet
{
    public class PeerProfile
    {
        public const string INVISIBLE_PROFILE_VALUE = "-";

        protected string fPublicKey;

        public DHTId NodeId { get; set; }

        public string UserName { get; set; }
        public string Country { get; set; }
        public string Languages { get; set; }
        public string TimeZone { get; set; }
        public string Email { get; set; }

        public string PublicKey
        {
            get { return fPublicKey; }
            set { fPublicKey = value; }
        }


        public PeerProfile()
        {
        }

        public void Load(BDictionary data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            UserName = data.Get<BString>("uname").ToString();
            Country = data.Get<BString>("uctry").ToString();
            TimeZone = data.Get<BString>("utz").ToString();
            Languages = data.Get<BString>("ulangs").ToString();
            Email = data.Get<BString>("uemail").ToString();
            PublicKey = data.Get<BString>("upublkey").ToString();
        }

        public virtual void Save(BDictionary data)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return UserName;
        }
    }
}
