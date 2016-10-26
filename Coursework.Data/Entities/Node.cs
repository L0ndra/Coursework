using System.Collections.Generic;
using Coursework.Data.MessageServices;

namespace Coursework.Data.Entities
{
    public class Node
    {
        public uint Id { get; set; }
        public IList<MessageQueueHandler> MessageQueue { get; set; }
        public SortedSet<uint> LinkedNodesId { get; set; } 
        public NodeType NodeType { get; set; }
    }
}
