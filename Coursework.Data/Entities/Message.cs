using System;

namespace Coursework.Data.Entities
{
    public class Message
    {
        public Guid ParentId { get; set; }
        public int DataSize { get; set; }
        public int ServiceSize { get; set; }
        public uint SenderId { get; set; }
        public uint ReceiverId { get; set; }
        public uint LastTransferNodeId { get; set; }
        public object Data { get; set; }
        public Channel[] Route { get; set; }
        public int SendAttempts { get; set; }
        public MessageType MessageType { get; set; }
        public int NumberInPackage { get; set; }
        public bool IsCanceled { get; set; }
        public int Size => DataSize + ServiceSize;
        public bool IsReceived => LastTransferNodeId == ReceiverId && Route.Length == 0;
    }
}
