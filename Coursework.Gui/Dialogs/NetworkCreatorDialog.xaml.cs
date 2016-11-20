using System;
using System.Windows;
using Coursework.Data.Builder;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;
using Coursework.Data.NetworkData;
using Coursework.Data.Services;
using Coursework.Gui.Helpers;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for NetworkCreatorDialog.xaml
    /// </summary>
    public partial class NetworkCreatorDialog
    {
        public delegate void NetworkUpdateHandler(INetworkHandler newNetwork);
        private event NetworkUpdateHandler NetworkUpdateEvent;
        private INetworkBuilder _networkBuilder;
        private INetworkBuilder _simpleNetworkBuilder;
        private INodeGenerator _nodeGenerator;
        private readonly IExceptionDecorator _exceptionCatcher;

        public NetworkCreatorDialog(NetworkUpdateHandler networkUpdateHandler)
        {
            InitializeComponent();

            NetworkUpdateEvent += networkUpdateHandler;

            MetropolitanNodesNumber.Text = AllConstants.NodeCountInMetropolitanNetwork.ToString();
            MetropolitanNetworksCount.Text = AllConstants.MetropolitanNetworksCount.ToString();
            NetworkPower.Text = AllConstants.NetworkPower.ToString("N");

            _exceptionCatcher = new ExceptionCatcher();
        }

        private void Create_OnClick(object sender, RoutedEventArgs e)
        {
            Action action = () =>
            {
                var network = CreateNetwork();

                OnNetworkUpdateEvent(network);

                Close();
            };

            _exceptionCatcher.Decorate(action, ExceptionMessageBox.Show);
        }

        private INetworkHandler CreateNetwork()
        {
            var metropolitanNodesCount = int.Parse(MetropolitanNodesNumber.Text);
            var metropolitanNetworksCount = int.Parse(MetropolitanNetworksCount.Text);
            var networkPower = double.Parse(NetworkPower.Text);

            _nodeGenerator = new NodeGenerator();

            _nodeGenerator.ResetAccumulator();

            _simpleNetworkBuilder = new NetworkBuilder(_nodeGenerator, metropolitanNodesCount, networkPower);
            _networkBuilder = new WideAreaNetworkBuilder(_simpleNetworkBuilder, metropolitanNetworksCount);

            var network = _networkBuilder.Build();

            return network;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected virtual void OnNetworkUpdateEvent(INetworkHandler newNetwork)
        {
            NetworkUpdateEvent?.Invoke(newNetwork);
        }
    }
}
