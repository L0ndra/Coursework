using System;
using System.Windows;
using System.Windows.Controls;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;
using Coursework.Gui.Dto;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for ChannelInfoWindow.xaml
    /// </summary>
    public partial class ChannelInfoWindow
    {
        public delegate void ChannelChangeEventHandler(ChannelDto newChannelParams);
        private ChannelChangeEventHandler _onChangeChannelInfoHandler;
        private Guid _lastLineId;

        public ChannelInfoWindow()
        {
            InitializeComponent();

            InitializeCapacityField();
        }

        public void BindChannelInfo(ChannelDto channelDto, ChannelChangeEventHandler onChangeChannelInfoHandler)
        {
            _onChangeChannelInfoHandler = onChangeChannelInfoHandler;
            _lastLineId = channelDto.Id;

            ShowDto(channelDto);
        }

        private void InitializeCapacityField()
        {
            foreach (var price in AllConstants.AllPrices)
            {
                Price.Items.Add(price);
            }
        }

        private void ShowDto(ChannelDto channelDto)
        {
            Price.SelectedItem = channelDto.Price;
            ErrorChance.Text = channelDto.ErrorChance.ToString("N");
            FirstNodeId.Text = channelDto.FirstNodeId.ToString();
            SecondNodeId.Text = channelDto.SecondNodeId.ToString();
            MessageOwnerId.Text = channelDto.MessageOwnerId.ToString();
            Capacity.Text = channelDto.Capacity.ToString();
            ConnectionType.SelectedItem = channelDto.ConnectionType == Data.Entities.ConnectionType.Duplex
                ? DuplexItem
                : HalfduplexItem;
            ChannelType.SelectedItem = channelDto.ChannelType == Data.Entities.ChannelType.Ground
                ? GroundItem
                : SatteliteItem;
        }

        private void SaveDto()
        {
            var newChannelDto = new ChannelDto
            {
                Id = _lastLineId,
                Price = (int)Price.SelectedItem,
                ErrorChance = double.Parse(ErrorChance.Text),
                ConnectionType = GetNewConnectionType(),
                ChannelType = GetNewChannelType(),
                FirstNodeId = uint.Parse(FirstNodeId.Text),
                SecondNodeId = uint.Parse(SecondNodeId.Text),
                Capacity = int.Parse(Capacity.Text),
            };

            _onChangeChannelInfoHandler(newChannelDto);
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

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveDto();
                Close();
            }
            catch (Exception ex) when (ex is ChannelException || ex is NodeException ||
                ex is ArgumentNullException || ex is FormatException || ex is OverflowException)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
            }
        }

        private void Price_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = Price.SelectedIndex;

            Capacity.Text = ChannelType.SelectedItem != null && ChannelType.SelectedItem.Equals(GroundItem) 
                ? AllConstants.AllCapacities[index].ToString() 
                : (AllConstants.AllCapacities[index] / 3).ToString();
        }
    }
}
