using System.ServiceModel;

namespace GKNetPNRP
{
    [ServiceContract(CallbackContract = typeof(IChat))]
    public interface IChat
    {
        [OperationContract(IsOneWay = true)]
        void Join(string member);

        [OperationContract(IsOneWay = true)]
        void Chat(string member, string message);

        [OperationContract(IsOneWay = true)]
        void Whisper(string member, string memberTo, string message);

        [OperationContract(IsOneWay = true)]
        void Leave(string member);

        [OperationContract(IsOneWay = true)]
        void InitializeMesh();

        [OperationContract(IsOneWay = true)]
        void SynchronizeMemberList(string member);
    }
}
