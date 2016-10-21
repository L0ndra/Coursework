using System.Collections.Generic;

namespace Coursework.Gui.Dto
{
    public class NodeDto
    {
        public uint Id { get; set; }
        public IDictionary<uint, int> LinkedNodesIdWithPrices { get; set; }
    }
}
