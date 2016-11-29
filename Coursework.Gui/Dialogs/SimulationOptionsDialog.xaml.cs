using System;
using System.Windows;
using Coursework.Data.Constants;
using Coursework.Data.Services;
using Coursework.Gui.Helpers;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for SimulationOptionsDialog.xaml
    /// </summary>
    public partial class SimulationOptionsDialog
    {
        public delegate void SimulationStarter(double messageGenerateChance, int tableUpdatePeriod, bool isDatagramMode,
            bool isRouterStupid, int messagesSize);
        private event SimulationStarter SimulationStart;
        private readonly IExceptionDecorator _exceptionCatcher;

        public SimulationOptionsDialog(SimulationStarter simulationStarter)
        {
            InitializeComponent();

            SimulationStart += simulationStarter;

            MessageGenerateChance.Text = AllConstants.MessageGenerateChance.ToString("N");
            TableUpdatePeriod.Text = AllConstants.UpdateTablePeriod.ToString();
            GeneratedMessagesSizes.Text = AllConstants.DefaultMessageSize.ToString();

            _exceptionCatcher = new ExceptionCatcher();
        }

        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            Action action = () =>
            {
                var messageGenerateChance = double.Parse(MessageGenerateChance.Text);
                var tableUpdatePeriod = int.Parse(TableUpdatePeriod.Text);
                var isDatagramMode = IsDatagramMode.IsChecked != null && IsDatagramMode.IsChecked.Value;
                var isRouterStupid = IsMessageRouterStupid.IsChecked != null && IsMessageRouterStupid.IsChecked.Value;
                var messagesSizes = IsMessageSizeSpecified.IsChecked.HasValue && IsMessageSizeSpecified.IsChecked.Value
                    ? int.Parse(GeneratedMessagesSizes.Text)
                    : 0;

                OnSimulationStart(messageGenerateChance, tableUpdatePeriod, isDatagramMode, isRouterStupid, messagesSizes);

                Close();
            };

            _exceptionCatcher.Decorate(action, ExceptionMessageBox.Show);
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected virtual void OnSimulationStart(double messageGenerateChance, int tableUpdatePeriod, 
            bool isDatagramMode, bool isRouterStupid, int messagesSize)
        {
            SimulationStart?.Invoke(messageGenerateChance, tableUpdatePeriod, isDatagramMode, isRouterStupid,
                messagesSize);
        }

        private void SpecifyMessageChecked_OnChange(object sender, RoutedEventArgs e)
        {
            if (IsMessageSizeSpecified.IsChecked.HasValue && IsMessageSizeSpecified.IsChecked.Value)
            {
                GeneratedMessagesSizesContainer.Visibility = Visibility.Visible;
            }
            else
            {
                GeneratedMessagesSizesContainer.Visibility = Visibility.Hidden;
            }
        }
    }
}
