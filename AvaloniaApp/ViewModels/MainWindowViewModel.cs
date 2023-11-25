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
        IsRecursionMethodSelected = true;
        SearchObjectViewModel = new() {
            Circle = true
        };
        SearchObjectViewModel.SetSearchType += SetSearchTypeToConfig;
        PositioningConfig.SizesChanged += SetNewSizesToRectangles;
    }

    private void SetSearchTypeToConfig(SearchObjectType ot)
    {
        MethodConfigViewModel.SearchObject = ot;
    }

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>Данные о сохраненных прямоугольниках в координатах действительного источника изображения, не UI.</summary>
    private Dictionary<MethodConfigurationViewModel, List<RectangleInfo>> _rectsConfigsData = new();

    /// <summary>Конфиг, для передачи данных о размере поля, на котором прорисовываются прямоугольники.</summary>
    public PositioningConfigViewModel PositioningConfig { get; }

    public SearchObjectViewModel SearchObjectViewModel { get; }

    private bool _isRecursionMethodSelected;
    private RecursialMethodConfigurationViewModel _recursialMethodConfig = new();

    public bool IsRecursionMethodSelected
    {
        get => _isRecursionMethodSelected;
        set
        {
            if (value)
                MethodConfigViewModel = _recursialMethodConfig;
            else
                MethodConfigViewModel = null;
            this.RaiseAndSetIfChanged(ref _isRecursionMethodSelected, value);
        }
    }


    private bool _isWeightCoefficientsMethodSelected;

    private WeightCoefficientsMethodConfigurationViewModel _weightMethodConfig = new();
    public bool IsWeightCoefficientsMethodSelected
    {
        get => _isWeightCoefficientsMethodSelected;
        set
        {
            if (value)
                MethodConfigViewModel = _weightMethodConfig;
            else
                MethodConfigViewModel = null;
            this.RaiseAndSetIfChanged(ref _isWeightCoefficientsMethodSelected, value);
        }
    }


    private bool _isInterpolationSelected;
    private InterpolationMethodConfigurationViewModel _interpolationMethodConfig = new();
    public bool IsInterpolationSelected
    {
        get => _isInterpolationSelected;
        set
        {
            if (value)
                MethodConfigViewModel = _interpolationMethodConfig;
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

    /// <summary>
    /// Метод очистки поля для изображения
    /// </summary>
    public void ClearImage()
    {
        ImageStorageFile = null;
        SourceImage = null;
        if (_rectsConfigsData.TryGetValue(MethodConfigViewModel, out _))
            _rectsConfigsData[MethodConfigViewModel].Clear();
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

        FileInfo file = new(ImageStorageFile.Path.LocalPath);
        DirectoryInfo dir = file.Directory!;
        string rectsDataFileName = $"{ImageStorageFile.Name}.rects";
        string rectsDataDirPath = Path.Combine(dir.FullName, datasDirName);

        if (!Directory.Exists(rectsDataDirPath))
            return;

        string rectsDataFilePath = Path.Combine(rectsDataDirPath, rectsDataFileName);

        if (!File.Exists(rectsDataFilePath))
            return;

        using var fileStream = File.OpenRead(rectsDataFilePath);

        _rectsConfigsData = _rectsConfigsData = JsonSerializer.Deserialize<List<DictItem>>(fileStream, _jsonOptions)
                .ToDictionary(i => i.Key, i => i.Value);

        CurrentImageRectangles
            .AddRange(_rectsConfigsData.Where(p => MethodConfigViewModelEquals(p.Key, MethodConfigViewModel)).SelectMany(sr => sr.Value.Select(ModifyRectByConfigs)));
    }

    public static bool MethodConfigViewModelEquals(MethodConfigurationViewModel x, MethodConfigurationViewModel y)
    {
        if (x.GetType() != y.GetType())
            return false;

        var type = x.GetType();
        if (type == typeof(WeightCoefficientsMethodConfigurationViewModel))
            return ((IEquatable<WeightCoefficientsMethodConfigurationViewModel>)x).Equals((WeightCoefficientsMethodConfigurationViewModel)y);
        else if (type == typeof(RecursialMethodConfigurationViewModel))
            return ((IEquatable<RecursialMethodConfigurationViewModel>)x).Equals((RecursialMethodConfigurationViewModel)y);
        else if (type == typeof(InterpolationMethodConfigurationViewModel))
            return ((IEquatable<InterpolationMethodConfigurationViewModel>)x).Equals((InterpolationMethodConfigurationViewModel)y);
        else
            return false;
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

            //double k = Math.Min(kX, kY);

            rectClone.StartPoint.X *= kX;
            rectClone.StartPoint.Y *= kY;

            rectClone.Width *= kX;
            rectClone.Height *= kY;
        }

        if (MethodConfigViewModel is not null && rectClone.StartPoint is not null)
        {
            Random rg = new();
            double k = rg.NextDouble();
            k *= rg.Next() % 2 == 0 ? 1 : -1;

            double H = 130;
            double changingWidth = MethodConfigViewModel.Inaccuracy / rectClone.Width * k * H;
            double changingHeight = MethodConfigViewModel.Inaccuracy / rectClone.Height * k * H;
            rectClone.Width = rectClone.Width - changingWidth;
            rectClone.Height = rectClone.Height - changingHeight;

            rectClone.StartPoint.X -= changingWidth / 2d;
            rectClone.StartPoint.Y += changingHeight / 2d;
        }

        return rectClone;
    }


    private RectangleInfo ModifyRectToSave(RectangleInfo rectangleInfo, double fieldWidth, double fieldHeight)
    {
        RectangleInfo rectClone = rectangleInfo.Clone();

        if (rectClone.StartPoint is not null && SourceImage is not null)
        {
            var imgSize = SourceImage.Size;

            double kX = fieldWidth / imgSize.Width;
            double kY = fieldHeight / imgSize.Height;

            //double k = Math.Min(kX, kY);

            rectClone.StartPoint.X /= kX;
            rectClone.StartPoint.Y /= kY;

            rectClone.Width /= kX;
            rectClone.Height /= kY;
        }

        return rectClone;
    }

    private void SetNewSizesToRectangles(double oldFieldWidth, double oldFieldHeight)
    {
        var list = _rectsConfigsData.FirstOrDefault(p => MethodConfigViewModelEquals(p.Key, MethodConfigViewModel)).Value;
        if (list is not null)
        {
            CurrentImageRectangles.Clear();
            CurrentImageRectangles.AddRange(list.Select(ModifyRectByConfigs));
        }
        else
        {
            var imgRelatedRects = CurrentImageRectangles
                .Select(uir => ModifyRectToSave(uir, oldFieldWidth, oldFieldHeight))
                .ToList();
            CurrentImageRectangles.Clear();
            var newUIRelatedRects = imgRelatedRects
                .Select(ModifyRectByConfigs)
                .ToList();
            CurrentImageRectangles.AddRange(newUIRelatedRects);
        }
    }

    private string datasDirName = "data";

    public void SaveRectanglesToFile()
    {
        if (ImageStorageFile is null)
            return;

        if (MethodConfigViewModel is null)
            return;

        FileInfo file = new(ImageStorageFile.Path.LocalPath);
        DirectoryInfo dir = file.Directory!;
        string rectsDataFileName = $"{ImageStorageFile.Name}.rects";
        string rectsDataDirPath = Path.Combine(dir.FullName, datasDirName);

        if(!Directory.Exists(rectsDataDirPath))
            Directory.CreateDirectory(rectsDataDirPath);

        string rectsDataFilePath = Path.Combine(rectsDataDirPath, rectsDataFileName);
        if (File.Exists(rectsDataFilePath))
        {
            using var fileReadStream = File.OpenRead(rectsDataFilePath);
            _rectsConfigsData = JsonSerializer.Deserialize<List<DictItem>>(fileReadStream, _jsonOptions)
                .ToDictionary(i => i.Key, i => i.Value);
        }

        using var fileStream = File.Open(rectsDataFilePath, FileMode.Create);

        double fieldWidth = PositioningConfig.XMultiplexer;
        double fieldHeight = PositioningConfig.YMultiplexer;

        
        var key = _rectsConfigsData.FirstOrDefault(p => MethodConfigViewModelEquals(p.Key, MethodConfigViewModel)).Key;
        if (key is null)
            key = MethodConfigViewModel;

        _rectsConfigsData[key] = CurrentImageRectangles
            .Select(uir => ModifyRectToSave(uir, fieldWidth, fieldHeight))
            .ToList();

        CurrentImageRectangles.Clear();

        JsonSerializer.Serialize(fileStream, _rectsConfigsData.Select(x => new DictItem { Key = x.Key, Value = x.Value }).ToList(), _jsonOptions);
        _rectsConfigsData.Clear();
    }
}

[Serializable]
internal class DictItem
{
    public MethodConfigurationViewModel Key { get; set; }
    public List<RectangleInfo> Value { get; set; }
}

