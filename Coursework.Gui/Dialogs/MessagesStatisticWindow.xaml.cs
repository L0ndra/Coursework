using System.Windows;
using Coursework.Data.Entities;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for MessagesStatisticWindow.xaml
    /// </summary>
    public partial class MessagesStatisticWindow
    {
        public MessagesStatisticWindow(MessagesStatistic messagesStatistic)
        {
            InitializeComponent();

            ShowOnGui(messagesStatistic);
        }

        private void ShowOnGui(MessagesStatistic messagesStatistic)
        {
            MessagesCount.Text = messagesStatistic.MessagesCount.ToString();
            GeneralMessagesCount.Text = messagesStatistic.GeneralMessagesCount.ToString();
            ReceivedMessagesCount.Text = messagesStatistic.ReceivedMessagesCount.ToString();
            GeneralReceivedMessagesCount.Text = messagesStatistic.GeneralMessagesReceivedCount.ToString();
            AverageDeliveryTime.Text = messagesStatistic.AvarageDeliveryTime.ToString("N");
            GeneralMessagesAverageDeliveryTime.Text = messagesStatistic.AvarageGeneralMessagesDeliveryTime.ToString("N");
            TotalReceivedMessagesSize.Text = messagesStatistic.TotalReceivedMessagesSize.ToString();
            TotalReceivedDataSize.Text = messagesStatistic.TotalReceivedDataSize.ToString();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
