using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageExchanger : IMessageExchanger
    {
        private readonly INetworkHandler _network;
        private List<Message> _handledMessages;

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
            _handledMessages = new List<Message>();

            foreach (var node in _network.Nodes)
            {
                HandleMessagesInNodeQueues(node);
            }

            //TODO: implement this shit

            //foreach (var channel in _network.Channels)
            //{
            //    HandleMessageInChannel(channel);
            //}

            //throw new System.NotImplementedException();
        }

        private void HandleMessageInChannel(Channel channel)
        {
            throw new System.NotImplementedException();
        }

        private void HandleMessagesInNodeQueues(Node node)
        {
            foreach (var messageQueueHandler in node.MessageQueueHandlers)
            {
                var currentMessage = messageQueueHandler.Messages
                    .FirstOrDefault();

                var currentChannel = _network.Channels
                    .First(c => c.Id == messageQueueHandler.ChannelId);

                if (currentMessage != null)
                {
                    if (currentMessage.LastTransferNodeId == node.Id)
                    {
                        var result = TryMoveMessageToChannel(currentChannel, currentMessage);

                        if (result)
                        {
                            messageQueueHandler.RemoveMessage(currentMessage);
                            _handledMessages.Add(currentMessage);
                        }
                    }
                    else if (currentMessage.ReceiverId == node.Id)
                    {
                        // Message handle
                        messageQueueHandler.RemoveMessage(currentMessage);
                    }
                    else
                    {
                        currentMessage.LastTransferNodeId = node.Id;
                    }
                }
            }
        }

        private bool TryMoveMessageToChannel(Channel channel, Message message)
        {
            if (channel.ConnectionType == ConnectionType.HalfDuplex
                && channel.FirstMessage == null)
            {
                channel.FirstMessage = message;
                return true;
            }
            if (channel.ConnectionType == ConnectionType.Duplex
                && (channel.FirstMessage == null ||
                channel.SecondMessage == null))
            {
                if (channel.FirstMessage == null)
                {
                    channel.FirstMessage = message;
                }
                else
                {
                    channel.SecondMessage = message;
                }
                return true;
            }

            return false;
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
