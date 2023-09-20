using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvaloniaFirstApp.ViewModels;


/// <summary>
/// Это наша вью-модель!
/// </summary>
public class MainWindowViewModel : ReactiveUI.ReactiveObject
{
    private bool _isRecursionMethodSelected;

    public bool IsRecursionMethodSelected
    {
        get => _isRecursionMethodSelected;
        set
        {
            if (value)
                MethodConfigViewModel = new RecursialMethodConfigurationViewModel();
            else
                MethodConfigViewModel = null;
            this.RaiseAndSetIfChanged(ref _isRecursionMethodSelected, value);
        }
    }


    private bool _isWeightCoefficientsMethodSelected;

    public bool IsWeightCoefficientsMethodSelected
    {
        get => _isWeightCoefficientsMethodSelected;
        set
        {
            if (value)
                MethodConfigViewModel = new WeightCoefficientsMethodConfigurationViewModel();
            else
                MethodConfigViewModel = null;
            this.RaiseAndSetIfChanged(ref _isWeightCoefficientsMethodSelected, value);
        }
    }


    private bool _isInterpolationSelected;

    public bool IsInterpolationSelected
    {
        get => _isInterpolationSelected;
        set
        {
            if (value)
                MethodConfigViewModel = new InterpolationMethodConfigurationViewModel();
            else
                MethodConfigViewModel = null;
            this.RaiseAndSetIfChanged(ref _isInterpolationSelected, value);
        }
    }

    private MethodConfigurationViewModel? _methodConfigViewModel;

    public MethodConfigurationViewModel? MethodConfigViewModel
    {
        get => _methodConfigViewModel;
        set => this.RaiseAndSetIfChanged(ref _methodConfigViewModel, value);
    }

    private Bitmap? _imageSource = null;//? нужен чтобы явно указать что в перемнную можно присвоить null

    /// <summary>
    /// Обертка над приватной _imageSource для доступа
    /// Источник данных для добавления изображений плат
    /// </summary>
    public Bitmap? SourceImage
    {
        get => _imageSource;
        set => this.RaiseAndSetIfChanged(ref _imageSource, value);
    }

    public IStorageFile? ImageStorageFile { get; private set; }

    /// <summary>
    /// Метод очистки поля для изображения
    /// </summary>
    public void ClearImage()
    {
        ImageStorageFile = null;
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

        double? notFullCoverage = null;
        if (MethodConfigViewModel is RecursialMethodConfigurationViewModel model)
            notFullCoverage = model.NotAllCoverage;

        string rectsFilePath = ImageStorageFile.Path.LocalPath + $"{notFullCoverage}.rects";

        if (!File.Exists(rectsFilePath))
            return;

        using var fileReadStream = File.OpenRead(rectsFilePath);

        var rectsCollection = await JsonSerializer.DeserializeAsync<List<RectangleInfo>>(fileReadStream);

        if (rectsCollection is null || !rectsCollection.Any())
            return;

        rectsCollection!.ForEach(r => ModifyRectByMethodConfig(r));
        CurrentImageRectangles.Clear();
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

    //TODO: refactor
    private void ModifyRectByMethodConfig(RectangleInfo rectangleInfo)
    {
        if (MethodConfigViewModel is not null && rectangleInfo is not null && rectangleInfo.StartPoint is not null)
        {
            Random rg = new();
            double k = rg.NextDouble();
            k *= rg.Next() % 2 == 0 ? 1 : -1;

            double changingWidth = MethodConfigViewModel.Inaccuracy / rectangleInfo.Width * k;
            double changingHeight = MethodConfigViewModel.Inaccuracy / rectangleInfo.Height * k;
            rectangleInfo.Width = rectangleInfo.Width - changingWidth;
            rectangleInfo.Height = rectangleInfo.Height - changingHeight;

            rectangleInfo.StartPoint.X -= changingWidth / 2d;
            rectangleInfo.StartPoint.Y += changingHeight / 2d;
        }
    }

    public async Task SaveRectanglesToImage()
    {
        double? notFullCoverage = null;
        if(MethodConfigViewModel is RecursialMethodConfigurationViewModel model)
            notFullCoverage = model.NotAllCoverage;

        using var fileStream = File.OpenWrite(ImageStorageFile.Path.LocalPath + $"{notFullCoverage}.rects");

        await JsonSerializer.SerializeAsync(fileStream, CurrentImageRectangles);
        CurrentImageRectangles.Clear();
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