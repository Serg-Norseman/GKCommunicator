using System;

namespace GKNet.Core
{
    public delegate void OnSynchronizeMemberList(object sender, string Member);

    public interface IChatCore
    {
        event EventHandler Offline;
        event EventHandler Online;
        event OnSynchronizeMemberList OnSynchronizeMemberList;

        void Join(string Member);
        void Chat(string Member, string Message);
        void Whisper(string Member, string MemberTo, string Message);
        void Leave(string Member);
        void InitializeMesh();
        void SynchronizeMemberList(string Member);

        void ConnectToMesh();
        void Close();
    }
}
