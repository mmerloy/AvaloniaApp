using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AvaloniaFirstApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.Opened += MainWindow_Activated; ;


    }

    private void MainWindow_Activated(object? sender, System.EventArgs e)
    {
        //App.MainWindow.FindDescendantOfType<Canvas>()!.PointerPressed += (s, args) =>
        //{
        //    var point = args.GetCurrentPoint(CNV);
        //    var x = point.Position.X;
        //    var y = point.Position.Y;
        //    if (point.Properties.IsLeftButtonPressed)
        //    {
        //        // left button pressed
        //    }
        //    if (point.Properties.IsRightButtonPressed)
        //    {
        //        // right button pressed
        //    }
        //};
    }
}