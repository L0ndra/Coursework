namespace Coursework.Data.Entities
{
    public class MessageInitializer
    {
        public uint SenderId { get; set; }
        public uint ReceiverId { get; set; }
        public MessageType MessageType { get; set; }
        public int Size { get; set; }
        public object Data { get; set; }
    }
}
