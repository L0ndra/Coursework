using Coursework.Data.Constants;

namespace Coursework.Data.Entities
{
    public class Message
    {
        public int Size { get; set; }
        public uint SenderId { get; set; }
        public uint ReceiverId { get; set; }
        public uint LastTransferNodeId { get; set; }
        public object Data { get; set; }
        public Channel[] Route { get; set; }
        public int SendAttempts { get; set; }
        public MessageType MessageType { get; set; }
        public bool IsSuccess => Route.Length == 0 && SendAttempts < AllConstants.MaxAttempts;
    }
}
