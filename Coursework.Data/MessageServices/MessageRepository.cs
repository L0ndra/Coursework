using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;
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

        public Message[] GetAllMessages(uint? nodeId = null, 
            MessageFiltrationMode messageFiltrationMode = MessageFiltrationMode.AllMessages)
        {
            IEnumerable<Message> result = new List<Message>();

            if (nodeId != null && _network.GetNodeById(nodeId.Value) == null)
            {
                throw new NodeException("Node isn't exist");
            }

            if ((messageFiltrationMode & MessageFiltrationMode.CanceledMessagesOnly) != 0)
            {
                result = result.Union(GetCanceledMessages(nodeId));
            }

            if (nodeId == null && (messageFiltrationMode & MessageFiltrationMode.ChannelMessagesOnly) != 0)
            {
                result = result.Union(GetMessagesInChannels());
            }

            if ((messageFiltrationMode & MessageFiltrationMode.QueueMessagesOnly) != 0)
            {
                result = result.Union(GetQueueMessages(nodeId));
            }

            if ((messageFiltrationMode & MessageFiltrationMode.ReceivedMessagesOnly) != 0)
            {
                result = result.Union(GetReceivedMessages(nodeId));
            }

            return result.ToArray();
        }

        private IEnumerable<Message> GetReceivedMessages(uint? nodeId = null)
        {
            var receivedMessages = _network.Nodes
                .Where(n => nodeId == null || n.Id == nodeId)
                .SelectMany(n => n.ReceivedMessages);

            return receivedMessages;
        }

        private IEnumerable<Message> GetQueueMessages(uint? nodeId = null)
        {
            var messageInNodes = _network.Nodes
                .Where(n => nodeId == null || n.Id == nodeId)
                .SelectMany(n => n.MessageQueueHandlers)
                .SelectMany(m => m.Messages);

            return messageInNodes;
        }

        private IEnumerable<Message> GetMessagesInChannels()
        {
            var messageInChannels = _network.Channels
                .Select(c => c.FirstMessage)
                .Where(m => m != null)
                .Union(_network.Channels
                    .Select(c => c.SecondMessage)
                    .Where(m => m != null));

            return messageInChannels;
        }

        private IEnumerable<Message> GetCanceledMessages(uint? nodeId = null)
        {
            var canceledMessages = _network.Nodes
                .Where(n => nodeId == null || n.Id == nodeId)
                .SelectMany(n => n.CanceledMessages);

            return canceledMessages;
        }
    }
}
