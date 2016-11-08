using System;
using Coursework.Data.Entities;

namespace Coursework.Gui.Dto
{
    public class ChannelDto
    {
        public Guid Id { get; set; }
        public int Price { get; set; }
        public double ErrorChance { get; set; }
        public uint FirstNodeId { get; set; }
        public uint SecondNodeId { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public ChannelType ChannelType { get; set; }
        public Guid MessageOwnerId { get; set; }
        public int Capacity { get; set; }
    }
}
