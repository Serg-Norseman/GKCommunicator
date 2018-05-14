using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GKNet.DHT
{
    public interface IDHTPeersHolder
    {
        IList<IDHTPeer> GetPeersList();
    }
}
