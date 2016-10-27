using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;

namespace Coursework.Data.Builder
{
    public class NodeGenerator : INodeGenerator
    {
        private static int _currentId;

        public Node[] GenerateNodes(int count)
        {
            var nodes = Enumerable
                .Range(_currentId, count)
                .Select(CreateNode)
                .ToArray();

            _currentId += count;

            return nodes;
        }

        public void ResetAccumulator()
        {
            _currentId = 0;
        }

        private Node CreateNode(int id)
        {
            return new Node
            {
                Id = (uint)id,
                MessageQueueHandlers = new List<MessageQueueHandler>(),
                LinkedNodesId = new SortedSet<uint>(),
                IsActive = false
            };
        }
    }
}
