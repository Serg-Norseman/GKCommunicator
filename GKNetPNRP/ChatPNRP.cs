using System;
using System.ServiceModel;
using System.ServiceModel.PeerResolvers;
using GKNetCore;

namespace GKNetPNRP
{
    public class ChatPNRP : IChatCore, IChat
    {
        private string fMemberName;
        // the channel instance where we execute our service methods against
        private IChatChannel fChannel;
        // the instance context which in this case is our window since it is the service host
        private InstanceContext fContext;
        // our binding transport for the p2p mesh
        private NetPeerTcpBinding fBinding;
        // the factory to create our chat channel
        private ChannelFactory<IChatChannel> fChannelFactory;
        // an interface provided by the channel exposing events to indicate
        // when we have connected or disconnected from the mesh
        private IOnlineStatus fStatusHandler;

        public event EventHandler Offline;
        public event EventHandler Online;

        public event OnSynchronizeMemberList OnSynchronizeMemberList;

        public ChatPNRP()
        {

        }

        public void Close()
        {
            if (fChannel != null) {
                fChannel.Leave(fMemberName);
                fChannel.Close();
            }
        }

        // this method gets called from a background thread to 
        // connect the service client to the p2p mesh specified
        // by the binding info in the app.config
        public void ConnectToMesh()
        {
            //since this window is the service behavior use it as the instance context
            fContext = new InstanceContext(this);

            //use the binding from the app.config with default settings
            //m_binding = new NetPeerTcpBinding("WPFChatBinding");
            fBinding = new NetPeerTcpBinding();
            fBinding.Port = 0;
            fBinding.Resolver.Mode = PeerResolverMode.Auto;
            fBinding.Security.Mode = SecurityMode.None;

            //create a new channel based off of our composite interface "IChatChannel" and the 
            //endpoint specified in the app.config
            //m_channelFactory = new DuplexChannelFactory<IChatChannel>(m_site, "WPFChatEndpoint");
            fChannelFactory = new DuplexChannelFactory<IChatChannel>(fContext, fBinding, "net.p2p://gedkeeper.network");
            fChannel = fChannelFactory.CreateChannel();

            //the next 3 lines setup the event handlers for handling online/offline events
            //in the MS P2P world, online/offline is defined as follows:
            //Online: the client is connected to one or more peers in the mesh
            //Offline: the client is all alone in the mesh
            fStatusHandler = fChannel.GetProperty<IOnlineStatus>();
            fStatusHandler.Online += new EventHandler(ostat_Online);
            fStatusHandler.Offline += new EventHandler(ostat_Offline);

            //this is an empty unhandled method on the service interface.
            //why? because for some reason p2p clients don't try to connect to the mesh
            //until the first service method call.  so to facilitate connecting i call this method
            //to get the ball rolling.
            fChannel.InitializeMesh();
        }

        public void Chat(string Member, string Message)
        {
            fChannel.Chat(Member, Message);
        }

        public void Join(string Member)
        {
            fChannel.Join(Member);
        }

        public void Whisper(string Member, string MemberTo, string Message)
        {
            fChannel.Whisper(Member, MemberTo, Message);
        }

        public void Leave(string Member)
        {

        }

        public void InitializeMesh()
        {
            // do nothing
        }

        public void SynchronizeMemberList(string Member)
        {
            fChannel.SynchronizeMemberList(Member);

            if (OnSynchronizeMemberList != null) {
                OnSynchronizeMemberList(this, Member);
            }
        }

        private void ostat_Offline(object sender, EventArgs e)
        {
            if (Offline != null) {
                Offline(this, e);
            }
        }

        private void ostat_Online(object sender, EventArgs e)
        {
            if (Online != null) {
                Online(this, e);
            }
        }
    }
}
