using System;
using System.ServiceModel;
using System.ServiceModel.PeerResolvers;
using GKNet.Core;

namespace GKNetPNRP
{
    public class ChatPNRP : IChatCore, IChat
    {
        private IChatForm fForm;

        private string fMemberName;
        private IChatChannel fChannel;

        public event EventHandler Offline;
        public event EventHandler Online;
        public event OnSynchronizeMemberList OnSynchronizeMemberList;

        public string MemberName
        {
            get { return fMemberName; }
            set { fMemberName = value; }
        }

        public ChatPNRP(IChatForm form)
        {
            fForm = form;
        }

        public void Connect()
        {
            // this method gets called from a background thread to 
            // connect the service client to the p2p mesh

            // since this window is the service behavior use it as the instance context
            var context = new InstanceContext(this);

            var binding = new NetPeerTcpBinding();
            binding.Port = 0;
            binding.Resolver.Mode = PeerResolverMode.Auto;
            binding.Security.Mode = SecurityMode.None;

            // create a new channel based off of our composite interface "IChatChannel"
            var channelFactory = new DuplexChannelFactory<IChatChannel>(context, binding, "net.p2p://gedkeeper.network");
            fChannel = channelFactory.CreateChannel();

            // the next 3 lines setup the event handlers for handling online/offline events
            // in the MS P2P world, online/offline is defined as follows:
            // Online: the client is connected to one or more peers in the mesh
            // Offline: the client is all alone in the mesh
            var statusHandler = fChannel.GetProperty<IOnlineStatus>();
            statusHandler.Online += new EventHandler(ostat_Online);
            statusHandler.Offline += new EventHandler(ostat_Offline);

            // this is an empty unhandled method on the service interface.
            // because for some reason p2p clients don't try to connect to the mesh
            // until the first service method call.  so to facilitate connecting i call this method
            // to get the ball rolling.
            fChannel.InitializeMesh();
        }

        public void Disconnect()
        {
            if (fChannel != null) {
                SendLeave(fMemberName);
                fChannel.Close();
            }
        }

        public void SendChat(string member, string message)
        {
            fChannel.Chat(member, message);
        }

        public void SendWhisper(string Member, string MemberTo, string Message)
        {
            fChannel.Whisper(Member, MemberTo, Message);
        }

        public void SendJoin(string Member)
        {
            // broadcasting a join method call to the mesh members
            fChannel.Join(Member);
        }

        public void SendLeave(string Member)
        {
            fChannel.Leave(Member);
        }

        public void SendSynchronizeMemberList(string Member)
        {
            fChannel.SynchronizeMemberList(Member);
        }

        #region IChat implementation

        void IChat.Chat(string Member, string Message)
        {
            fForm.OnChat(Member, Message);
        }

        void IChat.Whisper(string Member, string MemberTo, string Message)
        {
            fForm.OnWhisper(Member, MemberTo, Message);
        }

        void IChat.Join(string Member)
        {
            fForm.OnJoin(Member);
        }

        void IChat.Leave(string Member)
        {
            fForm.OnLeave(Member);
        }

        void IChat.InitializeMesh()
        {
        }

        void IChat.SynchronizeMemberList(string Member)
        {
            fForm.OnSynchronizeMemberList(Member);
        }

        #endregion

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
