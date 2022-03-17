﻿/*
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

namespace GKNet.Blockchain
{
    public class ProfileTransactionSolver : ITransactionSolver
    {
        public const string ProfileTransactionType = "profile";

        public string Sign
        {
            get {
                return ProfileTransactionType;
            }
        }

        public void Solve(IBlockchainNode node, Transaction transaction)
        {
            var profile = transaction.DeserializeContent<PeerProfile>();
            node.CommunicatorCore.AddProfile(profile);
        }

        public bool Verify(Transaction transaction)
        {
            return true;
        }
    }
}
