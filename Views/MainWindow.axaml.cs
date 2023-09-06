using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using System;
using System.Globalization;
using System.Linq;

namespace AvaloniaFirstApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    //private void ClearImage(object sender, RoutedEventArgs e)
    //{
    //    ImageFieldControll.Source = null;
    //}


    //private async void SetImageAsync(object sender, RoutedEventArgs e)
    //{
    //    var topLevel = TopLevel.GetTopLevel(this);

    //    // Start async operation to open the dialog.
    //    var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
    //    {
    //        Title = "Open Image File",
    //        AllowMultiple = false
    //    });

    //    ImageFieldControll.Source = new Bitmap(files.First().Path.LocalPath);
    //}
}