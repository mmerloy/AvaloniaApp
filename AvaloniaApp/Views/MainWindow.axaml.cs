using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaFirstApp.ViewModels;
using System;


namespace AvaloniaFirstApp;

public partial class MainWindow : Window
{
    public MainWindowViewModel? DataSourceInstance => DataContext as MainWindowViewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnImgCanvasSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        SetBounds();
    }

    private void SetBounds(object? sender = null, RoutedEventArgs? args = null)
    {
        DataSourceInstance!.PositioningConfig.SetCoefs(
            x: ImgCanvas.Bounds.Width,
            y: ImgCanvas.Bounds.Height
        );
    }
}