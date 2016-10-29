using System.Collections.Generic;
using System.Linq;
using Coursework.Data.MessageServices;

namespace Coursework.Data.Entities
{
    public class Node
    {
        public uint Id { get; set; }
        public IList<MessageQueueHandler> MessageQueueHandlers { get; set; }
        public SortedSet<uint> LinkedNodesId { get; set; } 
        public NodeType NodeType { get; set; }
        public bool IsActive { get; set; }

        public bool GotReceivedMessages
        {
            get
            {
                var result = MessageQueueHandlers
                    .SelectMany(m => m.Messages)
                    .Any(m => m.LastTransferNodeId != Id);

                return result;
            }
        }
    }
}
