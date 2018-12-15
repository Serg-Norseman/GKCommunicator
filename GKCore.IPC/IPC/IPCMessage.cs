using System;
using System.Text;

namespace GKCore.IPC
{
    public enum StatusMessage
    {
        None = 0,
        KeepAlive,
        CloseConnection,
        Ping
    }


    [Serializable]
    public class IPCMessage
    {
        public string Service { get; set; }
        public string Method { get; set; }
        public object[] Parameters { get; set; }
        public object Return { get; set; }

        public string Error { get; set; }
        public StatusMessage StatusMsg { get; set; }


        public IPCMessage()
        {
        }

        public IPCMessage(StatusMessage status)
        {
            StatusMsg = status;
        }

        public IPCMessage(string service, string method, params object[] parameters)
        {
            Service = service;
            Method = method;
            Parameters = parameters;
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            str.Append(Service);
            str.Append(":");
            str.Append(Method);
            str.Append("(");
            if (Parameters != null && Parameters.Length > 0) {
                for (int i = 0; i < Parameters.Length; i++) {
                    if (i != 0) {
                        str.Append(",");
                    }
                    string s = Parameters[i].ToString();
                    str.Append(s);
                }
            }
            str.Append(")");
            return str.ToString();
        }

        /*public void ReadObject(byte[] data)
        {
            var parser = new BencodeParser();
            var dict = parser.Parse<BDictionary>(data);

            Service = dict.Get<BString>("Service").ToString();
            Method = dict.Get<BString>("Method").ToString();

            var paramList = dict.Get<BList>("Parameters");
            Parameters = new object[paramList.Count];
            //foreach (var param in paramList) paramList.Add(param.ToString());

            Return = dict.Get<IBObject>("Return").ToString();
        }

        public byte[] WriteObject()
        {
            var result = new BDictionary();
            result.Add("Service", Service);
            result.Add("Method", Method);

            var paramList = new BList();
            foreach (object param in Parameters) paramList.Add(param.ToString());
            result.Add("Parameters", paramList);

            if (Return != null) {
                if (Return is string) {
                    result.Add("Return", (string)Return);
                }
                if (Return is int) {
                    result.Add("Return", (int)Return);
                }
            }

            return result.EncodeAsBytes();
        }*/
    }
}
