using Coursework.Data.Entities;

namespace Coursework.Data.NetworkData
{
    public interface INetwork
    {
        Node[] Nodes { get; }
        Channel[] Channels { get; }
    }
}