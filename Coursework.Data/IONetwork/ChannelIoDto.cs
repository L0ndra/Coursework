using System;
using Coursework.Data.Entities;

namespace Coursework.Data.IONetwork
{
    public class ChannelIoDto
    {
        public Guid Id { get; set; }
        public int Price { get; set; }
        public double ErrorChance { get; set; }
        public uint FirstNodeId { get; set; }
        public uint SecondNodeId { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public ChannelType ChannelType { get; set; }
    }
}
