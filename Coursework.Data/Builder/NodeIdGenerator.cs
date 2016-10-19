using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;

namespace Coursework.Data.Builder
{
    public class NodeIdGenerator
    {
        public static Node[] GenerateNodes(int startId, int count)
        {
            var nodes = Enumerable
                .Range(startId, count)
                .Select(i => new Node { Id = (uint)i })
                .ToArray();

            return nodes;
        }
    }
}
