using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageExchanger : IMessageExchanger
    {
        private readonly INetworkHandler _network;

        public MessageExchanger(INetworkHandler network)
        {
            _network = network;
        }

        public void Initialize()
        {
            var centralMachine = _network.Nodes
                .FirstOrDefault(n => n.NodeType == NodeType.CentralMachine);

            if (centralMachine != null)
            {
                centralMachine.IsActive = true;

                foreach (var receiverId in centralMachine.LinkedNodesId)
                {
                    CreateInitializeMessage(centralMachine, receiverId);
                }
            }
        }

        public void HandleMessagesOnce()
        {
            throw new System.NotImplementedException();
        }

        private void CreateInitializeMessage(Node centralMachine, uint receiverId)
        {
            var receiver = _network.GetNodeById(receiverId);

            if (receiver.NodeType != NodeType.MainMetropolitanMachine)
            {
                return;
            }

            var channel = _network.GetChannel(receiverId, centralMachine.Id);

            var messageQueue = centralMachine.MessageQueueHandlers
                .First(m => m.ChannelId == channel.Id);

            var message = new Message
            {
                ReceiverId = receiverId,
                MessageType = MessageType.InitializeMessage,
                SenderId = centralMachine.Id,
                LastTransferNodeId = centralMachine.Id,
                Size = AllConstants.InitializeMessageSize,
                Data = null
            };

            messageQueue.AddMessage(message);
        }
    }
}
