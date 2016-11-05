using System.Linq;
using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public class MessageReceiver : IMessageReceiver
    {
        private readonly IMessageHandler _messageHandler;

        public MessageReceiver(IMessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public void HandleReceivedMessage(Node node, Message message)
        {
            message.LastTransferNodeId = node.Id;

            var passedChannel = message.Route.First();

            var oldMessageQueue = node.MessageQueueHandlers
                    .First(m => m.ChannelId == passedChannel.Id);

            oldMessageQueue.RemoveMessage(message);

            if (message.Route.Length != 1)
            {
                message.Route = message.Route
                    .Skip(1)
                    .ToArray();

                var destinationMessageQueue = node.MessageQueueHandlers
                    .First(m => m.ChannelId == message.Route[0].Id);

                destinationMessageQueue.AppendMessage(message);
            }
            else
            {
                _messageHandler.HandleMessage(message);
            }
        }
    }
}
