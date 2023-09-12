using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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

    public IStorageFile? ImageStorageFile { get; private set; }

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
    public async Task SetImageAsync()
    {
        ClearImage();
        var topLevel = TopLevel.GetTopLevel(App.MainWindow);

        // Start async operation to open the dialog.
        var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });

        ImageStorageFile = files.First();

        SourceImage = new Bitmap(ImageStorageFile.Path.LocalPath);
        //await CreateRectangles();
    }

    public ObservableCollection<RectangleInfo> CurrentImageRectangles { get; set; }
        = new ObservableCollection<RectangleInfo>();

    /// <summary>
    /// Для создания прямоугольников на изображении 
    /// (ошибки на плате)
    /// </summary>
    public async Task CreateRectangles()
    {
        if (ImageStorageFile is null)
            return;

        string rectsFilePath = ImageStorageFile.Path.LocalPath + ".rects";

        if (!File.Exists(rectsFilePath))
            return;


        using var fileReadStream = File.OpenRead(rectsFilePath);
        
        var rectsCollection = await JsonSerializer.DeserializeAsync<List<RectangleInfo>>(fileReadStream);

        if (rectsCollection is null || !rectsCollection.Any())
            return;

        foreach (var rect in rectsCollection)
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

    public async Task SaveRectanglesToImage()
    {
        using var fileStream = File.OpenWrite(ImageStorageFile.Path.LocalPath + ".rects");

        await JsonSerializer.SerializeAsync(fileStream, CurrentImageRectangles);
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