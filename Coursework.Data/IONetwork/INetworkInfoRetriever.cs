namespace Coursework.Data.IONetwork
{
    public interface INetworkInfoRetriever
    {
        INetwork Read(string filename);
        void Write(string filename, INetwork network);
    }
}
