/*
 *  "BSLib.TeamsNet", the serverless peer-to-peer network library.
 *  Copyright (C) 2018-2025 by Sergey V. Zhdanovskih.
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
using System.Collections.Generic;
using System.Threading;
using BencodeNET;
using BSLib.TeamsNet.Logging;

namespace BSLib.TeamsNet.DHT
{
    public class DHTTransactions
    {
        private static long fCurrentTransactionId;

        private readonly ILogger fLogger;
        private readonly Dictionary<int, DHTMessage> fTransactions;

        public DHTTransactions()
        {
            fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "DHTTransactions");
            fTransactions = new Dictionary<int, DHTMessage>();
        }

        public void Clear()
        {
            fTransactions.Clear();
        }

        /// <summary>
        /// https://www.bittorrent.org/beps/bep_0005.html
        /// 
        /// The transaction ID should be encoded as a short string of binary numbers,
        /// typically 2 characters are enough as they cover 2^16 outstanding queries.
        /// </summary>
        public static BString GetNextId()
        {
            long value = Interlocked.Add(ref fCurrentTransactionId, 1);
            byte[] data = BitConverter.GetBytes((short)value);
            return new BString(data);
        }

        public void SetQuery(BString transactionId, DHTMessage message)
        {
            try {
                if (transactionId != null && transactionId.Length == 2) {
                    int tid = BitConverter.ToUInt16(transactionId.Value, 0);
                    fTransactions[tid] = message;
                }
            } catch (Exception ex) {
                fLogger.WriteError("SetQuery()", ex);
            }
        }

        public QueryType CheckQuery(BString transactionId)
        {
            QueryType result = QueryType.None;

            if (transactionId != null && transactionId.Length == 2) {
                int tid = BitConverter.ToUInt16(transactionId.Value, 0);

                DHTMessage message;
                if (fTransactions.TryGetValue(tid, out message)) {
                    result = message.QueryType;
                    fTransactions.Remove(tid);
                }
            }

            return result;
        }
    }
}
