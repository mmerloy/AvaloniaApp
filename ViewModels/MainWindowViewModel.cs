using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using AvaloniaFirstApp.Models;
using DynamicData;
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
    public MainWindowViewModel()
    {
        PositioningConfig = new();
        PositioningConfig.SizesChanged += SetNewSizesToRectangles;
    }

    /// <summary>Конфиг, для передачи данных о размере поля, на котором прорисовываются прямоугольники.</summary>
    public PositioningConfigViewModel PositioningConfig { get; }

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

    /// <summary>View-модель о выбранном методе обработки изображения для отрисовки в UI.</summary>
    public MethodConfigurationViewModel? MethodConfigViewModel
    {
        get => _methodConfigViewModel;
        set => this.RaiseAndSetIfChanged(ref _methodConfigViewModel, value);
    }

    private Bitmap? _imageSource = null;//? нужен чтобы явно указать что в переменную можно присвоить null

    /// <summary>
    /// Источник данных для добавления изображений плат в UI.
    /// </summary>
    public Bitmap? SourceImage
    {
        get => _imageSource;
        set => this.RaiseAndSetIfChanged(ref _imageSource, value);
    }

    /// <summary>Данные о файле, в котором храниться демонстрируемое в UI изображение <see cref="SourceImage"/>.</summary>
    public IStorageFile? ImageStorageFile { get; private set; }

    /// <summary>Данные о сохраненных прямоугольниках в координатах действительного источника изображения, не UI.</summary>
    public List<RectangleInfo>? RectanglesInfoFromImageSource { get; private set; }

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
    }

    /// <summary>Прямоугольники, демонстрируемые на изображении в UI с координатами UI.</summary>
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

        CurrentImageRectangles.Clear();

        string? notFullCoverage = null;
        if (MethodConfigViewModel is RecursialMethodConfigurationViewModel model)
            notFullCoverage = model.NotAllCoverage.ToString().Replace(".", ",");

        string rectsFilePath = ImageStorageFile.Path.LocalPath + $"{notFullCoverage}.rects";

        if (!File.Exists(rectsFilePath))
            return;

        using var fileReadStream = File.OpenRead(rectsFilePath);

        RectanglesInfoFromImageSource = await JsonSerializer.DeserializeAsync<List<RectangleInfo>>(fileReadStream);

        if (RectanglesInfoFromImageSource is null || !RectanglesInfoFromImageSource.Any())
            return;

        CurrentImageRectangles.AddRange(RectanglesInfoFromImageSource.Select(sr => ModifyRectByConfigs(sr)));
    }

    public void AddRectangleToImage(double startX, double startY, double endX, double endY)
    {
        RectanglePoint startPoint = new RectanglePoint()
        {
            X = startX,
            Y = startY
        };

        double width = Math.Abs(startX - endX);
        double height = Math.Abs(startY - endY);

        var rectangleInfo = new RectangleInfo
        {
            StartPoint = startPoint,
            Width = width,
            Height = height
        };

        CurrentImageRectangles.Add(rectangleInfo);
    }

    //TODO: refactor
    private RectangleInfo ModifyRectByConfigs(RectangleInfo rectangleInfo)
    {
        RectangleInfo rectClone = rectangleInfo.Clone();

        if (rectClone.StartPoint is not null && SourceImage is not null && PositioningConfig is not null)
        {
            var imgSize = SourceImage.Size;

            double kX = PositioningConfig.XMultiplexer / imgSize.Width;
            double kY = PositioningConfig.YMultiplexer / imgSize.Height;

            rectClone.StartPoint.X *= kX;
            rectClone.StartPoint.Y *= kY;

            rectClone.Width *= kX;
            rectClone.Height *= kY;
        }

        if (MethodConfigViewModel is not null && rectClone is not null && rectClone.StartPoint is not null)
        {
            Random rg = new();
            double k = rg.NextDouble();
            k *= rg.Next() % 2 == 0 ? 1 : -1;

            double changingWidth = MethodConfigViewModel.Inaccuracy / rectClone.Width * k;
            double changingHeight = MethodConfigViewModel.Inaccuracy / rectClone.Height * k;
            rectClone.Width = rectClone.Width - changingWidth;
            rectClone.Height = rectClone.Height - changingHeight;

            rectClone.StartPoint.X -= changingWidth / 2d;
            rectClone.StartPoint.Y += changingHeight / 2d;
        }

        return rectClone;
    }


    private RectangleInfo ModifyRectToSave(RectangleInfo rectangleInfo)
    {
        RectangleInfo rectClone = rectangleInfo.Clone();

        if (rectClone.StartPoint is not null && SourceImage is not null && PositioningConfig is not null)
        {
            var imgSize = SourceImage.Size;

            double kX = imgSize.Width / PositioningConfig.XMultiplexer;
            double kY = imgSize.Height / PositioningConfig.YMultiplexer;

            rectClone.StartPoint.X *= kX;
            rectClone.StartPoint.Y *= kY;

            rectClone.Width *= kX;
            rectClone.Height *= kY;
        }

        return rectClone;
    }

    private void SetNewSizesToRectangles(double fieldWidth, double fieldHeight)
    {
        if (RectanglesInfoFromImageSource is not null)
        {
            CurrentImageRectangles.Clear();
            CurrentImageRectangles.AddRange(RectanglesInfoFromImageSource.Select(sr => ModifyRectByConfigs(sr)));
        }
        else
        {
            var imgRelatedRects = CurrentImageRectangles
                .Select(uir => ModifyRectToSave(uir))
                .ToList();
            CurrentImageRectangles.Clear();
            var newUIRelatedRects = imgRelatedRects
                .Select(ModifyRectByConfigs)
                .ToList();
            CurrentImageRectangles.AddRange(newUIRelatedRects);
        }
    }

    public async Task SaveRectanglesToFile()
    {
        string? notFullCoverage = null;
        if (MethodConfigViewModel is RecursialMethodConfigurationViewModel model)
            notFullCoverage = model.NotAllCoverage.ToString().Replace(".", ",");

        using var fileStream = File.OpenWrite(ImageStorageFile.Path.LocalPath + $"{notFullCoverage}.rects");

        var rectangleInfosTofile = CurrentImageRectangles
            .Select(uir => ModifyRectToSave(uir))
            .ToList();

        CurrentImageRectangles.Clear();

        await JsonSerializer.SerializeAsync(fileStream, rectangleInfosTofile);
    }
}

