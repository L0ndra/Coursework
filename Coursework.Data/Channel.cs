namespace Coursework.Data
{
    public class Channel
    {
        public int Price { get; set; }
        public double ErrorChance { get; set; }
        public uint FirstNodeId { get; set; }
        public uint SecondNodeId { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public ChannelType ChannelType { get; set; }
    }
}
