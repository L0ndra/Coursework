using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageRepository : IMessageRepository
    {
        private readonly INetworkHandler _network;

        public MessageRepository(INetworkHandler network)
        {
            _network = network;
        }

        public Message[] GetAllMessages()
        {
            var messageInNodes = _network.Nodes
                .SelectMany(n => n.MessageQueueHandlers)
                .SelectMany(m => m.Messages);

            var messageInChannels = _network.Channels
                .Select(c => c.FirstMessage)
                .Union(_network.Channels
                    .Select(c => c.SecondMessage));

            var receivedMessages = _network.Nodes
                .SelectMany(n => n.ReceivedMessages);

            return messageInNodes
                .Union(messageInChannels)
                .Union(receivedMessages)
                .ToArray();
        }

        public Message[] GetAllMessages(uint nodeId)
        {
            var messageInNodes = _network.Nodes
                .First(n => n.Id == nodeId)
                .MessageQueueHandlers
                .SelectMany(m => m.Messages);

            return messageInNodes.ToArray();
        }
    }
}
