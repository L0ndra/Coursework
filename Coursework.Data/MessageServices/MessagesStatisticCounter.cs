using System.Linq;
using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public class MessagesStatisticCounter : IMessagesStatisticCounter
    {
        private readonly IMessageRegistrator _messageRegistrator;
        private readonly IMessageRepository _messageRepository;

        public MessagesStatisticCounter(IMessageRegistrator messageRegistrator, IMessageRepository messageRepository)
        {
            _messageRegistrator = messageRegistrator;
            _messageRepository = messageRepository;
        }

        public MessagesStatistic Count()
        {
            var allMessages = GetAllMessages();

            var receivedMessages = GetReceivedMessages(allMessages);

            var generalReceivedMessages = GetGeneralReceivedMessages(receivedMessages);

            var avarageDeliveryTime = CountAvarageDeliveryTime(receivedMessages);

            var avarageGeneralMessagesDeliveryTime = CountAvarageGeneralMessagesDeliveryTime(generalReceivedMessages);

            var receivedSize = CountReceivedSize(receivedMessages);

            var receivedDataSize = CountReceivedDataSize(receivedMessages);

            var messageStatistic = new MessagesStatistic
            {
                MessagesCount = allMessages.GroupBy(m => m.ParentId).Count(),
                ReceivedMessagesCount = receivedMessages.GroupBy(m => m.ParentId).Count(),
                GeneralMessagesReceivedCount = generalReceivedMessages.GroupBy(m => m.ParentId).Count(),
                AvarageDeliveryTime = avarageDeliveryTime,
                AvarageGeneralMessagesDeliveryTime = avarageGeneralMessagesDeliveryTime,
                TotalReceivedMessagesSize = receivedSize,
                TotalReceivedDataSize = receivedDataSize
            };

            return messageStatistic;
        }

        protected virtual int CountReceivedDataSize(Message[] receivedMessages)
        {
            var receivedDataSize = receivedMessages.Sum(m => m.DataSize);
            return receivedDataSize;
        }

        protected virtual int CountReceivedSize(Message[] receivedMessages)
        {
            var receivedSize = receivedMessages.Sum(m => m.Size);
            return receivedSize;
        }

        protected virtual double CountAvarageGeneralMessagesDeliveryTime(Message[] generalReceivedMessages)
        {
            var avarageGeneralMessagesDeliveryTime = generalReceivedMessages
                .GroupBy(m => m.ParentId)
                .Sum(m => _messageRegistrator.MessagesEndTimes[m.Key]
                          - _messageRegistrator.MessagesStartTimes[m.Key])
                                                     /(double) generalReceivedMessages.GroupBy(m => m.ParentId).Count();
            return avarageGeneralMessagesDeliveryTime;
        }

        protected virtual double CountAvarageDeliveryTime(Message[] receivedMessages)
        {
            var avarageDeliveryTime = receivedMessages
                .GroupBy(m => m.ParentId)
                .Sum(m => _messageRegistrator.MessagesEndTimes[m.Key]
                          - _messageRegistrator.MessagesStartTimes[m.Key])
                                      /(double) receivedMessages.GroupBy(m => m.ParentId).Count();
            return avarageDeliveryTime;
        }

        protected virtual Message[] GetGeneralReceivedMessages(Message[] receivedMessages)
        {
            var generalReceivedMessages = receivedMessages
                .Where(m => m.MessageType == MessageType.General)
                .ToArray();
            return generalReceivedMessages;
        }

        protected virtual Message[] GetReceivedMessages(Message[] allMessages)
        {
            var receivedMessages = allMessages
                .Where(m => m.IsReceived && allMessages
                    .Where(m1 => m1.ParentId == m.ParentId)
                    .All(m1 => m1.IsReceived || m1.IsCanceled))
                .ToArray();
            return receivedMessages;
        }

        protected virtual Message[] GetAllMessages()
        {
            var allMessages = _messageRepository
                .GetAllMessages()
                .ToArray();
            return allMessages;
        }
    }
}
