using System.ServiceModel;

namespace GKNetPNRP
{
    // this channel interface provides a multiple inheritence adapter for our channel factory
    // that aggregates the two interfaces need to create the channel
    public interface IChatChannel : IChat, IClientChannel
    {
    }
}