using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GKNet.DHT
{
    public interface IDHTLogger
    {
        void WriteLog(string str, bool display = true);
    }
}
