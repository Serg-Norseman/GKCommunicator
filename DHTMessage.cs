using BencodeNET.Objects;

namespace DHTConnector
{
    public class DHTMessage
    {
        public readonly MsgType Type;
        public readonly QueryType QueryType;
        public readonly BDictionary Data;

        public DHTMessage(MsgType type, QueryType queryType, BDictionary data)
        {
            Type = type;
            QueryType = queryType;
            Data = data;
        }
    }
}
