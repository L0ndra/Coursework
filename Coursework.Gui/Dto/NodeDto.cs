using System.Collections.Generic;
using Coursework.Data.Entities;

namespace Coursework.Gui.Dto
{
    public class NodeDto
    {
        public uint Id { get; set; }
        public SortedSet<uint> LinkedNodesId { get; set; }
        public NodeType NodeType { get; set; }
    }
}
