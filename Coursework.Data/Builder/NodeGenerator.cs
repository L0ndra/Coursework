using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;

namespace Coursework.Data.Builder
{
    public class NodeGenerator
    {
        public static Node[] GenerateNodes(int startId, int count)
        {
            var nodes = Enumerable
                .Range(startId, count)
                .Select(CreateNode)
                .ToArray();

            return nodes;
        }

        private static Node CreateNode(int id)
        {
            return new Node
            {
                Id = (uint)id,
                MessageQueue = new MessageQueueHandler(),
                LinkedNodesId = new SortedSet<uint>()
            };
        }
    }
}
