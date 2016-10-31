using Coursework.Data.Entities;

namespace Coursework.Data.NetworkData
{
    public interface INodeHandler
    {
        void AddNode(Node node);
        Node GetNodeById(uint id);
        void RemoveNode(uint nodeId);
    }
}