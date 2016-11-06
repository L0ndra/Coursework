using System;
using System.Windows;
using Coursework.Data.Constants;
using Coursework.Data.Exceptions;

namespace Coursework.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for SimulationOptionsDialog.xaml
    /// </summary>
    public partial class SimulationOptionsDialog
    {
        public delegate void SimulationStarter(double messageGenerateChance, double speed, int tableUpdatePeriod);
        private event SimulationStarter SimulationStart;

        public SimulationOptionsDialog(SimulationStarter simulationStarter)
        {
            InitializeComponent();

            SimulationStart += simulationStarter;

            MessageGenerateChance.Text = AllConstants.MessageGenerateChance.ToString("N");
            TableUpdatePeriod.Text = AllConstants.UpdateTablePeriod.ToString();
            Speed.Text = AllConstants.TimerInterval.ToString("N");
        }

        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var messageGenerateChance = double.Parse(MessageGenerateChance.Text);
                var speed = double.Parse(Speed.Text);
                var tableUpdatePeriod = int.Parse(TableUpdatePeriod.Text);

                OnSimulationStart(messageGenerateChance, speed, tableUpdatePeriod);

                Close();
            }
            catch (Exception ex) when (ex is ChannelException || ex is NodeException ||
                ex is ArgumentNullException || ex is FormatException || ex is OverflowException ||
                ex is ArgumentException)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected virtual void OnSimulationStart(double messageGenerateChance, double speed, int tableUpdatePeriod)
        {
            SimulationStart?.Invoke(messageGenerateChance, speed, tableUpdatePeriod);
        }
    }
}
