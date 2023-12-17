using Avalonia.Controls;
using System;

namespace AvaloniaFirstApp.Views.ModalWindows
{
    public partial class ExceptionHandlingWindow : Window
    {
        public ExceptionHandlingWindow(Exception ex)
        {
            InitializeComponent();
            errMsgBox.Text = ex.Message;
            stakTraseBox.Text = ex.StackTrace;
        }
    }
}
