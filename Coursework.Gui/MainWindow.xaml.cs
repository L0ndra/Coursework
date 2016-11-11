using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private Background.BackgroundWorker _backgroundWorker;
        private readonly INetworkInfoRetriever _networkInfoRetriever;
        private ChannelAddWindow _channelAddWindow;
        private IMessageRouter _messageRouter;
        private IMessageCreator _messageCreator;
        private IMessageCreator _updateMatrixMessageCreator;
        private IMessageCreator _positiveResponseMessageCreator;
        private IMessageCreator _negativeResponseMessageCreator;
        private IMessageHandler _messageHandler;
        private IMessageReceiver _messageReceiver;
        private IMessageExchanger _messageExchanger;
        private IMessageGenerator _messageGenerator;
        private IMessageRepository _messageRepository;
        private IMessageRegistrator _messageRegistrator;
        private IMessageViewUpdater _messageViewUpdater;
        private IMessagesStatisticCounter _messagesStatisticCounter;
        private readonly INetworkLocationMapRetriever _networkLocationMapRetriever;
        private Canvas GeneratedCanvas => NetworkArea.Children.OfType<Canvas>().First();

        public MainWindow(INetworkHandler network, INetworkInfoRetriever networkInfoRetriever,
            INetworkLocationMapRetriever networkLocationMapRetriever)
        {
            InitializeComponent();

            InitializeFiltrationModeComboBox();

            _network = network;
            _networkInfoRetriever = networkInfoRetriever;
            _networkLocationMapRetriever = networkLocationMapRetriever;

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

                _networkLocationMapRetriever.Write(filename, locations);

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
                var locations = _networkLocationMapRetriever.Read(filename);

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

        private NodeLocationMapDto[] GetNodesLocations()
        {
            var locations = new List<NodeLocationMapDto>();

            foreach (var grid in GeneratedCanvas.Children.OfType<Grid>())
            {
                var x = Canvas.GetLeft(grid);
                var y = Canvas.GetTop(grid);

                var nodeDto = (NodeDto)grid.Tag;

                var location = new NodeLocationMapDto
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
            _network.ClearMessages();
            _network.Reset();

            var simulationOptionsDialog = new SimulationOptionsDialog(InitializeAllServices);

            simulationOptionsDialog.Show();
        }

        private void InitializeAllServices(double messageGenerateChance, int tableUpdatePeriod,
            bool isPackageMode)
        {
            _messageRouter = new MessageRouter(_network);

            if (isPackageMode)
            {
                _messageCreator = new PackageMessageCreator(_network, _messageRouter);
            }
            else
            {
                _messageCreator = new RequestMessageCreator(_network, _messageRouter);
            }

            _positiveResponseMessageCreator = new PositiveResponseCreator(_network, _messageRouter);
            _negativeResponseMessageCreator = new NegativeResponseCreator(_network, _messageRouter);
            _updateMatrixMessageCreator = new UpdateMatrixMessageCreator(_network, _messageRouter);

            _messageHandler = new MessageHandler(_network, _messageCreator, 
                _updateMatrixMessageCreator, _positiveResponseMessageCreator);
            _messageReceiver = new MessageReceiver(_messageHandler, _negativeResponseMessageCreator);
            _messageExchanger = new MessageExchanger(_network, _messageReceiver);

            _messageGenerator = new MessageGenerator(_network, _messageCreator, messageGenerateChance);

            _messageRepository = new MessageRepository(_network);
            _messageViewUpdater = new MessageViewUpdater(_messageRepository, Messages);

            _messageRegistrator = new MessageRegistrator(_messageRepository);

            _messagesStatisticCounter = new MessagesStatisticCounter(_messageRegistrator, _messageRepository);

            _backgroundWorker = new Background.BackgroundWorker(_messageExchanger, _messageGenerator,
                _networkDrawer, _messageCreator, _messageRegistrator, _messageViewUpdater, tableUpdatePeriod);

            _messageCreator.UpdateTables();

            FiltrationModeSelect_OnSelectionChanged(FiltrationModeSelect, null);

            _backgroundWorker.Run();

            _backgroundWorker.Interval = IntervalSlider.Value;
        }

        private void Pause_OnClick(object sender, RoutedEventArgs e)
        {
            _backgroundWorker.Pause();
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

        private void Resume_OnClick(object sender, RoutedEventArgs e)
        {
            _backgroundWorker?.Resume();
        }

        private void Stop_OnClick(object sender, RoutedEventArgs e)
        {
            if (_backgroundWorker != null)
            {
                _backgroundWorker.Stop();

                _backgroundWorker = null;

                var statistic = _messagesStatisticCounter.Count();

                var messageStatisticWindow = new MessagesStatisticWindow(statistic);

                messageStatisticWindow.Show();
            }
        }

        private void RangeBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_backgroundWorker != null)
            {
                _backgroundWorker.Interval = IntervalSlider.Value;
            }

            if (IntervalValue != null)
            {
                IntervalValue.Text = IntervalSlider.Value.ToString("N0");
            }
        }

        private void AddMessage_Onclick(object sender, RoutedEventArgs e)
        {
            var messageAddWindow = new MessageAddWindow(MessageCreate);

            messageAddWindow.Show();
        }

        private void MessageCreate(MessageInitializer messageInitializer)
        {
            var messages = _messageCreator.CreateMessages(messageInitializer);

            if (messages != null)
            {
                var senderId = messageInitializer.SenderId;
                _messageCreator.AddInQueue(messages, senderId);
            }
            else
            {
                MessageBox.Show("Message Cannot Be Created", "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
            }
        }
    }
}
