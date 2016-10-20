using System.Windows;
using Coursework.Gui.Dto;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for ChannelInfoWindow.xaml
    /// </summary>
    public partial class ChannelInfoWindow : Window
    {
        public ChannelInfoWindow()
        {
            InitializeComponent();
        }

        public void BindChannelInfo(ChannelDto channelDto)
        {
            Price.Text = channelDto.Price.ToString();
            ErrorChance.Text = channelDto.ErrorChance.ToString("N");
            FirstNodeId.Text = channelDto.FirstNodeId.ToString();
            SecondNodeId.Text = channelDto.SecondNodeId.ToString();
            ConnectionType.Text = channelDto.ConnectionType.ToString();
        }
    }
}
