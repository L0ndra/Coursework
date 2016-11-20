using System;
using System.Windows;

namespace Coursework.Gui.Helpers
{
    public class ExceptionMessageBox
    {
        public static void Show(Exception exception)
        {
            MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
        }
    }
}
