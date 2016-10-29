using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageReceiver : IMessageReceiver
    {
        private readonly INetworkHandler _network;
        private readonly IMessageSender _messageSender;

        public MessageReceiver(INetworkHandler network, IMessageSender messageSender)
        {
            _network = network;
            _messageSender = messageSender;
        }

        public void HandleReceivedMessage(Node node, Message message)
        {
            message.LastTransferNodeId = node.Id;
            message.Route = new Channel[0];

            if (message.MessageType == MessageType.InitializeMessage)
            {
                var currentNode = _network.GetNodeById(message.ReceiverId);

                currentNode.IsActive = true;

                InitializeLinkedNodes(currentNode);
            }
        }

        private void InitializeLinkedNodes(Node node)
        {
            foreach (var linkedNodeId in node.LinkedNodesId)
            {
                var linkedNode = _network.GetNodeById(linkedNodeId);

                if (!linkedNode.IsActive)
                {
                    var initializeMessage = CreateInitializeMessage(node.Id, linkedNodeId);

                    _messageSender.StartSendProcess(initializeMessage);
                }
            }
        }

        private MessageInitializer CreateInitializeMessage(uint senderId, uint receiverId)
        {
            return new MessageInitializer
            {
                MessageType = MessageType.InitializeMessage,
                ReceiverId = receiverId,
                SenderId = senderId,
                Data = null,
                Size = AllConstants.InitializeMessageSize
            };
        }
    }
}
