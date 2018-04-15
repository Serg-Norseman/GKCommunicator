using System.ServiceModel;

namespace GKNetPNRP
{
    // this is our simple service contract
    // Namespace = "http://gedkeeper.net.gkchat",
    [ServiceContract(CallbackContract = typeof(IChat))]
    public interface IChat
    {
        [OperationContract(IsOneWay = true)]
        void Join(string Member);

        [OperationContract(IsOneWay = true)]
        void Chat(string Member, string Message);

        [OperationContract(IsOneWay = true)]
        void Whisper(string Member, string MemberTo, string Message);

        [OperationContract(IsOneWay = true)]
        void Leave(string Member);

        [OperationContract(IsOneWay = true)]
        void InitializeMesh();

        [OperationContract(IsOneWay = true)]
        void SynchronizeMemberList(string Member);
    }
}
