using BencodeNET.Objects;

namespace DHTConnector
{
    public static class TransactionId
    {
        private static byte[] fCurrent = new byte[2];

        public static BString NextId()
        {
            lock (fCurrent) {
                BString result = new BString((byte[])fCurrent.Clone());
                if (fCurrent[0] == 255) {
                    fCurrent[0] = 0;
                    fCurrent[1] += 1;
                } else {
                    fCurrent[0] += 1;
                }
                return result;
            }
        }
    }
}
