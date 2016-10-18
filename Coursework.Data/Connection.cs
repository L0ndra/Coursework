namespace Coursework.Data
{
    public class Connection
    {
        public int Price { get; set; }
        public double ErrorChance { get; set; }
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
        public ConnectionType NodeLinkType { get; set; }
        public ChannelType ChannelType { get; set; }
    }
}
