/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System.Collections.Generic;

namespace GKLocations.Blockchain
{
    public interface INetwork
    {
        IList<string> Hosts { get; }

        // TODO: the host with the largest chain of blocks is pre-selected and it is synchronized.
        // response from Network -> Chain.ReceivedGlobalBlockchain()
        void RequestGlobalBlockchain();

        bool SendBlockToHost(string ip, string method, string data);

        User GetCurrentUser();
    }
}
