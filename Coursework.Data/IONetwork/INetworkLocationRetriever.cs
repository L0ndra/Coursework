namespace Coursework.Data.IONetwork
{
    public interface INetworkLocationRetriever
    {
        NodeLocationDto[] Read(string filename);
        void Write(string filename, NodeLocationDto[] nodeLocations);
    }
}
