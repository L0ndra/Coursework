using System.Collections.Generic;
using Coursework.Data.MessageServices;

namespace Coursework.Data.Entities
{
    public class Node
    {
        public uint Id { get; set; }
        public MessageQueueHandler MessageQueue { get; set; }
        public SortedSet<uint> LinkedNodesId { get; set; } 
    }
}
