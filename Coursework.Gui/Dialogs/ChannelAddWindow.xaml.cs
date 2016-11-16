using System;
using System.Windows;
using System.Windows.Controls;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for ChannelAddWindow.xaml
    /// </summary>
    public partial class ChannelAddWindow
    {
        public delegate void ChannelAddEventHandler(Channel channel);
        private event ChannelAddEventHandler ChannelAdd;

        public ChannelAddWindow(ChannelAddEventHandler channelAddEventHandler)
        {
            InitializeComponent();

            ChannelAdd += channelAddEventHandler;

            InitializeCapacityField();
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

        private void InitializeCapacityField()
        {
            foreach (var price in AllConstants.AllPrices)
            {
                Price.Items.Add(price);
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
                Id = Guid.NewGuid(),
                Price = (int)Price.SelectedItem,
                ErrorChance = double.Parse(ErrorChance.Text),
                ConnectionType = GetNewConnectionType(),
                ChannelType = GetNewChannelType(),
                FirstNodeId = uint.Parse(FirstNodeId.Text),
                SecondNodeId = uint.Parse(SecondNodeId.Text),
                IsBusy = false,
                Capacity = int.Parse(Capacity.Text),
            };

            OnChannelAdd(newChannel);
        }

        private void Price_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = Price.SelectedIndex;

            if (index > -1)
            {
                Capacity.Text = ChannelType.SelectedItem != null && ChannelType.SelectedItem.Equals(GroundItem)
                ? AllConstants.AllCapacities[index].ToString()
                : (AllConstants.AllCapacities[index] / 3).ToString();
            }
        }

        protected virtual void OnChannelAdd(Channel channel)
        {
            ChannelAdd?.Invoke(channel);
        }
    }
}
