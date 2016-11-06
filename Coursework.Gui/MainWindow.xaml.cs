using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Coursework.Data.AutoRunners;
using Coursework.Data.Entities;
using Coursework.Data.IONetwork;
using Coursework.Data.MessageServices;
using Coursework.Data.NetworkData;
using Coursework.Data.Util;
using Coursework.Gui.Dialogs;
using Coursework.Gui.Drawers;
using Coursework.Gui.Dto;
using Coursework.Gui.MessageService;
using Microsoft.Win32;
using NodeLocationDto = Coursework.Data.IONetwork.NodeLocationDto;

namespace Coursework.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private INetworkHandler _network;
        private IComponentDrawer _networkDrawer;
        private IComponentDrawer _nodeDrawer;
        private IComponentDrawer _channelDrawer;
        private IBackgroundWorker _backgroundWorker;
        private readonly INetworkInfoRetriever _networkInfoRetriever;
        private ChannelAddWindow _channelAddWindow;
        private IMessageRouter _messageRouter;
        private IMessageCreator _messageCreator;
        private IMessageHandler _messageHandler;
        private IMessageReceiver _messageReceiver;
        private IMessageExchanger _messageExchanger;
        private IMessageGenerator _messageGenerator;
        private IMessageRepository _messageRepository;
        private IMessageViewUpdater _messageViewUpdater;
        private readonly INetworkLocationRetriever _networkLocationRetriever;
        private Canvas GeneratedCanvas => NetworkArea.Children.OfType<Canvas>().First();

        public MainWindow(INetworkHandler network, INetworkInfoRetriever networkInfoRetriever,
            INetworkLocationRetriever networkLocationRetriever)
        {
            InitializeComponent();

            InitializeFiltrationModeComboBox();

            _network = network;
            _networkInfoRetriever = networkInfoRetriever;
            _networkLocationRetriever = networkLocationRetriever;

            InitializeDrawers();
            _channelAddWindow = new ChannelAddWindow(network, channel => _channelDrawer.DrawComponents(GeneratedCanvas));

            Loaded += OnWindowLoaded;
        }

        private void InitializeFiltrationModeComboBox()
        {
            foreach (var value in Enum.GetValues(typeof(MessageFiltrationMode))
                .Cast<MessageFiltrationMode>())
            {
                var comboBoxItem = new ComboBoxItem
                {
                    Content = value.GetDescriptionOfEnum()
                };

                FiltrationModeSelect.Items.Add(comboBoxItem);
            }

            FiltrationModeSelect.SelectedItem = FiltrationModeSelect.Items[0];
        }

        private void InitializeDrawers()
        {
            _nodeDrawer = new WideAreaNetworkNodeDrawer(_network);
            _channelDrawer = new ChannelDrawer(_network);
            _networkDrawer = new NetworkDrawer(_nodeDrawer, _channelDrawer);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _networkDrawer.DrawComponents(NetworkArea);
        }

        private void AddNode_OnClick(object sender, RoutedEventArgs e)
        {
            var node = new Node
            {
                Id = _network.Nodes.Max(n => n.Id) + 1,
                LinkedNodesId = new SortedSet<uint>(),
                MessageQueueHandlers = new List<MessageQueueHandler>(),
                IsActive = false,
                NodeType = NodeType.SimpleNode,
                ReceivedMessages = new List<Message>(),
                CanceledMessages = new List<Message>()
            };

            _network.AddNode(node);

            _nodeDrawer.DrawComponents(GeneratedCanvas);
        }

        private void AddChannel_OnClick(object sender, RoutedEventArgs e)
        {
            _channelAddWindow.Show();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SaveNetwork_OnClick(object sender, RoutedEventArgs e)
        {
            var filename = GetPathFromDialog();

            if (filename != null)
            {
                _networkInfoRetriever.Write(filename, _network);

                MessageBox.Show("File saved!", "OK", MessageBoxButton.OK, MessageBoxImage.Information,
                        MessageBoxResult.OK,
                        MessageBoxOptions.None);
            }
        }

        private void ReadNetwork_OnClick(object sender, RoutedEventArgs e)
        {
            var filename = GetPathFromDialog();

            try
            {
                _network = _networkInfoRetriever.Read(filename) as Network;

                ReasignNetwork();

                MessageBox.Show("File loaded!", "OK", MessageBoxButton.OK, MessageBoxImage.Information,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);

                _backgroundWorker?.Stop();

                _backgroundWorker = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
            }
        }

        private void ReasignNetwork()
        {
            InitializeDrawers();
            NetworkArea.Children.Remove(GeneratedCanvas);

            _networkDrawer.DrawComponents(NetworkArea);

            _channelAddWindow = new ChannelAddWindow(_network, channel => _channelDrawer.DrawComponents(GeneratedCanvas));
        }

        private void SaveNetworkLocation_OnClick(object sender, RoutedEventArgs e)
        {
            var filename = GetPathFromDialog();

            if (filename != null)
            {
                var locations = GetNodesLocations();

                _networkLocationRetriever.Write(filename, locations);

                MessageBox.Show("File saved!", "OK", MessageBoxButton.OK, MessageBoxImage.Information,
                        MessageBoxResult.OK,
                        MessageBoxOptions.None);
            }
        }

        private void ReadNetworkLocation_OnClick(object sender, RoutedEventArgs e)
        {
            var filename = GetPathFromDialog();

            try
            {
                var locations = _networkLocationRetriever.Read(filename);

                _nodeDrawer = new SmartNodeDrawer(_network, locations);
                _networkDrawer = new NetworkDrawer(_nodeDrawer, _channelDrawer);

                NetworkArea.Children.Remove(GeneratedCanvas);

                _networkDrawer.DrawComponents(NetworkArea);

                MessageBox.Show("File loaded!", "OK", MessageBoxButton.OK, MessageBoxImage.Information,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
            }
        }

        private NodeLocationDto[] GetNodesLocations()
        {
            var locations = new List<NodeLocationDto>();

            foreach (var grid in GeneratedCanvas.Children.OfType<Grid>())
            {
                var x = Canvas.GetLeft(grid);
                var y = Canvas.GetTop(grid);

                var nodeDto = (NodeDto)grid.Tag;

                var location = new NodeLocationDto
                {
                    Id = nodeDto.Id,
                    X = x,
                    Y = y
                };

                locations.Add(location);
            }

            return locations.ToArray();
        }

        private string GetPathFromDialog()
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                Filter = "JSON Files (*.json)|*.json"
            };

            return dialog.ShowDialog() == true
                ? dialog.FileName
                : null;
        }

        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            if (_backgroundWorker == null)
            {
                var simulationOptionsDialog = new SimulationOptionsDialog(InitializeAllServices);

                simulationOptionsDialog.Show();
            }
        }

        private void InitializeAllServices(double messageGenerateChance, double speed, int tableUpdatePeriod)
        {
            _messageRouter = new MessageRouter(_network);
            //_messageCreator = new PackageMessageCreator(_network, _messageRouter);
            _messageCreator = new RequestMessageCreator(_network, _messageRouter);
            _messageHandler = new MessageHandler(_network);
            _messageReceiver = new MessageReceiver(_messageHandler);
            _messageExchanger = new MessageExchanger(_network, _messageReceiver);

            _messageGenerator = new MessageGenerator(_network, _messageCreator, messageGenerateChance);

            _messageRepository = new MessageRepository(_network);
            _messageViewUpdater = new MessageViewUpdater(_messageRepository, Messages);

            _backgroundWorker = new Background.BackgroundWorker(_messageExchanger, _messageGenerator,
                _networkDrawer, _messageCreator, _messageViewUpdater, tableUpdatePeriod);

            _messageCreator.UpdateTables();

            FiltrationModeSelect_OnSelectionChanged(FiltrationModeSelect, null);

            _backgroundWorker?.Run();
        }

        private void Pause_OnClick(object sender, RoutedEventArgs e)
        {
            if (_backgroundWorker != null && _backgroundWorker.IsActive)
            {
                _backgroundWorker.Pause();

                PauseResumeButton.Content = "Resume";
            }
            else
            {
                _backgroundWorker?.Resume();

                PauseResumeButton.Content = "Pause";
            }
        }

        private void FiltrationModeSelect_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;

            var values = EnumHelper.GetEnumValueByDescription<MessageFiltrationMode>(
                    ((ComboBoxItem)comboBox.SelectedItem).Content as string);

            if (_messageViewUpdater != null)
            {
                _messageViewUpdater.MessageFiltrationMode = values.First();

                _messageViewUpdater.Show();
            }
        }

        private void ShowNetworkMatrises_Click(object sender, RoutedEventArgs e)
        {
            var networkMatrisesWindow = new NodeNetworkMatrix(_network);

            networkMatrisesWindow.Show();
        }

        private void CreateNewNetwork_OnClick(object sender, RoutedEventArgs e)
        {
            NetworkCreatorDialog.NetworkUpdateHandler networkUpdateHandler = network =>
            {
                _network = network;
                ReasignNetwork();
            };

            var newNetworkwindow = new NetworkCreatorDialog(networkUpdateHandler);

            newNetworkwindow.Show();
        }
    }
}
