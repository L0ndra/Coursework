namespace Coursework.Data.IONetwork
{
    public interface INetworkLocationMapRetriever
    {
        NodeLocationMapDto[] Read(string filename);
        void Write(string filename, NodeLocationMapDto[] nodeLocationsMap);
    }
}
