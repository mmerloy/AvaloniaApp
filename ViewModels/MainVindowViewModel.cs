using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace AvaloniaFirstApp.ViewModels;


/// <summary>
/// Это наша вью-модель!
/// </summary>
public class MainVindowViewModel : INotifyPropertyChanged
{
    public double MyDoubleValue { get; set; }

    //точка доступа к таблицам биндинга
    public event PropertyChangedEventHandler? PropertyChanged;

    private Bitmap? _imageSource = null;//? нужен чтобы явно указать что в перемнную можно присвоить null

    /// <summary>
    /// Обертка над приватной _imageSource для доступа
    /// Источник данных для добавления изображений плат
    /// </summary>
    public Bitmap? SourceImage
    {
        get => _imageSource;
        set
        {
            _imageSource = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceImage)));
        }
    }

    public MainVindowViewModel()
    {
        CurrentImageRectangles.Add(new RectangleInfo
        {
            Height = 10,
            Width = 23, XX = 123,
            StartPoint = new Point()
            {
                X = 500,
                Y = 250
            }
        });
    }

    /// <summary>
    /// Метод очистки поля для изображения
    /// </summary>
    public void ClearImage()
    {
        SourceImage = null;

    }

    /// <summary>
    /// Заполнение поля изображением
    /// </summary>
    public async void SetImageAsync()
    {
        var topLevel = TopLevel.GetTopLevel(App.MainWindow);

        // Start async operation to open the dialog.
        var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });

        var firstFile = files.First();

        SourceImage = new Bitmap(firstFile.Path.LocalPath);
        CreateRectangles(firstFile.Name.ToString());
    }

    public ObservableCollection<RectangleInfo> CurrentImageRectangles { get; set; }
        = new ObservableCollection<RectangleInfo>();

    /// <summary>
    /// Для создания прямоугольников на изображении 
    /// (ошибки на плате)
    /// </summary>
    private void CreateRectangles(string name)
    {
        if (name == "image3.webp")
        {
            CurrentImageRectangles.Add(new RectangleInfo
            {
                Height = 10,
                Width = 23,
                StartPoint = new Point()
                {
                    X = 500,
                    Y = 250
                }
            });
        }
    }



    public void CreateKeyDown(PointerEventArgs args)
    {
        //var piotnt = () .GetPosition(CNV);
        //System.Diagnostics.Debug.WriteLine
    }
}


public class RectangleInfo
{
    public Point StartPoint { get; set; }

    public double Width { get; set; }
    public double XX { get; set; }

    public double Height { get; set; }
}

public class Point
{
    public double X { get; set; }
    public double Y { get; set; }
}