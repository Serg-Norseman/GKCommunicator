namespace GKNet.Core
{
    public interface ILogger
    {
        void WriteLog(string str, bool display = true);
    }
}
