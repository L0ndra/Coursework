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
            ReceivedMessagesCount.Text = messagesStatistic.ReceivedMessagesCount.ToString();
            GeneralMessagesReceivedCount.Text = messagesStatistic.GeneralMessagesReceivedCount.ToString();
            AvarageDeliveryTime.Text = messagesStatistic.AvarageDeliveryTime.ToString("N");
            GeneralMessagesAvarageDeliveryTime.Text = messagesStatistic.AvarageGeneralMessagesDeliveryTime.ToString("N");
            TotalReceivedMessagesSize.Text = messagesStatistic.TotalReceivedMessagesSize.ToString();
            TotalReceivedDataSize.Text = messagesStatistic.TotalReceivedDataSize.ToString();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
