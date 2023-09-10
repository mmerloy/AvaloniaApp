using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System;
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

    /// <summary>
    /// Метод очистки поля для изображения
    /// </summary>
    public void ClearImage()
    {
        SourceImage = null;
        CurrentImageRectangles.Clear();
    }

    /// <summary>
    /// Заполнение поля изображением
    /// </summary>
    public async void SetImageAsync()
    {
        ClearImage();
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
        var rect = new RectangleInfo
        {
            Height = 10,
            Width = 23,
            StartPoint = new RectangPoint()
            {
                X = 100,
                Y = 50
            }
        };
        CurrentImageRectangles.Add(rect);


        if (name == "image3.webp")
        {
            rect.Width = 5;
            rect.Height = 12;
            rect.StartPoint.X = 10;
            rect.StartPoint.Y = 12;
        }
        else if (name == "image2.webp")
        {
            rect.Width = 20;
            rect.Height = 5;
            rect.StartPoint.X = 20;
            rect.StartPoint.Y = 32;
        }

        CurrentImageRectangles.Add(rect);
    }

    public void AddRectangleToImage(double startX, double startY, double endX, double endY)
    {
        RectangPoint startPoint = new RectangPoint()
        {
            X = startX,
            Y = startY
        };

        double width = Math.Abs(startX - endX);
        double height = Math.Abs(startY - endY);

        CurrentImageRectangles.Add(new RectangleInfo
        {
            StartPoint = startPoint,
            Width = width,
            Height = height
        });
    }
}


public class RectangleInfo
{
    public RectangPoint? StartPoint { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }
}

public class RectangPoint
{
    public double X { get; set; }
    public double Y { get; set; }
}