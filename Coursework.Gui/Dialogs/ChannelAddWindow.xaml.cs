using System;
using System.Windows;
using Coursework.Data;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;
using Coursework.Data.NetworkData;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for ChannelAddWindow.xaml
    /// </summary>
    public partial class ChannelAddWindow : Window
    {
        public delegate void ChannelAddEventHandler(Channel channel);
        private event ChannelAddEventHandler ChannelAdd;
        private readonly INetworkHandler _network;

        public ChannelAddWindow(INetworkHandler network, ChannelAddEventHandler channelAddEventHandler)
        {
            InitializeComponent();

            _network = network;
            ChannelAdd += channelAddEventHandler;
        }

        private void AddChannel_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveDto();
                Hide();
            }
            catch (Exception ex) when (ex is ChannelException || ex is NodeException ||
                ex is ArgumentNullException || ex is FormatException || ex is OverflowException)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private ConnectionType GetNewConnectionType()
        {
            if (ConnectionType.SelectedItem.Equals(DuplexItem))
            {
                return Data.Entities.ConnectionType.Duplex;
            }

            if (ConnectionType.SelectedItem.Equals(HalfduplexItem))
            {
                return Data.Entities.ConnectionType.HalfDuplex;
            }

            throw new ArgumentException("ConnectionType");
        }

        private ChannelType GetNewChannelType()
        {
            if (ChannelType.SelectedItem.Equals(GroundItem))
            {
                return Data.Entities.ChannelType.Ground;
            }

            if (ChannelType.SelectedItem.Equals(SatteliteItem))
            {
                return Data.Entities.ChannelType.Satellite;
            }

            throw new ArgumentException("ChannelType");
        }

        private void SaveDto()
        {
            var newChannel = new Channel
            {
                Price = int.Parse(Price.Text),
                ErrorChance = double.Parse(ErrorChance.Text),
                ConnectionType = GetNewConnectionType(),
                ChannelType = GetNewChannelType(),
                FirstNodeId = uint.Parse(FirstNodeId.Text),
                SecondNodeId = uint.Parse(SecondNodeId.Text),
                IsFree = true
            };

            _network.AddChannel(newChannel);

            OnChannelAdd(newChannel);
        }

        protected virtual void OnChannelAdd(Channel channel)
        {
            ChannelAdd?.Invoke(channel);
        }
    }
}
