using System;

namespace Coursework.Data.Entities
{
    public class Channel
    {
        public Guid Id { get; set; }
        public int Price { get; set; }
        public double ErrorChance { get; set; }
        public uint FirstNodeId { get; set; }
        public uint SecondNodeId { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public ChannelType ChannelType { get; set; }
        public Message FirstMessage { get; set; }
        public Message SecondMessage { get; set; }
        public bool IsFree { get; set; }
    }
}
