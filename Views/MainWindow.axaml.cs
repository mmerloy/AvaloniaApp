using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaFirstApp.ViewModels;
using System;


namespace AvaloniaFirstApp;

public partial class MainWindow : Window
{
    public MainVindowViewModel? DataSourceInstance => DataContext as MainVindowViewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    private Point? _startPoint = null;

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        var imgCanvas = sender as Canvas;
        if (imgCanvas is null)
            return;

        _startPoint = e.GetCurrentPoint(imgCanvas).Position;
    }

    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (_startPoint is null)
            return;

        var imgCanvas = sender as Canvas;
        if (imgCanvas is null)
            return;

        if (DataSourceInstance is null)
        {
            _startPoint = null;
            return;
        }

        var endPoint = e.GetCurrentPoint(imgCanvas).Position;

        Point startPoint = _startPoint.Value;

        (startPoint, endPoint) = ChouseStartAndEndPoint(ref startPoint, ref endPoint);

        DataSourceInstance.AddRectangleToImage(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);

        _startPoint = null;
    }

    private static (Point Start, Point End) ChouseStartAndEndPoint(ref Point lhs, ref Point rhs)
    {
        lhs.Deconstruct(out double x1, out double y1);
        rhs.Deconstruct(out double x2, out double y2);
        return (
            Start: new Point(Math.Min(x1, x2), Math.Min(y1, y2)),
            End: new Point(Math.Max(x1, x2), Math.Max(y1, y2))
        );
    }
}