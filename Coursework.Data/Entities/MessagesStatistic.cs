namespace Coursework.Data.Entities
{
    public class MessagesStatistic
    {
        public int MessagesCount { get; set; }
        public int GeneralMessagesCount { get; set; }
        public int ReceivedMessagesCount { get; set; }
        public int GeneralMessagesReceivedCount { get; set; }
        public double AvarageDeliveryTime { get; set; }
        public double AvarageGeneralMessagesDeliveryTime { get; set; }
        public int TotalReceivedMessagesSize { get; set; }
        public int TotalReceivedDataSize { get; set; }
    }
}