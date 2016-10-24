using System.Collections.Generic;

namespace Coursework.Gui.Dto
{
    public class NodeDto
    {
        public uint Id { get; set; }
        public SortedSet<uint> LinkedNodesId { get; set; }
    }
}
