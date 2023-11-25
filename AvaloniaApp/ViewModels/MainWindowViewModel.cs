using AutoMapper;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using AvaloniaFirstApp.Infrastructure.Services;
using AvaloniaFirstApp.Models;
using Domain;
using Domain.MethodConfigurations;
using DynamicData;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
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
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly MethodConfigurationViewModelsLocator _methodConfigurationLocator;

    public MainWindowViewModel(ApplicationDbContext db, IMapper mapper, MethodConfigurationViewModelsLocator methodConfigurationLocator)
    {
        _db = db;
        _mapper = mapper;
        _methodConfigurationLocator = methodConfigurationLocator;
        MethodConfigViewModel = _methodConfigurationLocator.GetLocatedMethodConfigViewModelOrDefault(MethodConfigType.Interpolation);

        PositioningConfig = new();
        PositioningConfig.SizesChanged += SetNewSizesToRectangles;
    }

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    /// <summary>Данные о сохраненных прямоугольниках в координатах действительного источника изображения, не UI.</summary>
    private Dictionary<MethodConfigurationViewModel, List<RectangleInfo>> _rectsConfigsData = new();

    /// <summary>Конфиг, для передачи данных о размере поля, на котором прорисовываются прямоугольники.</summary>
    public PositioningConfigViewModel PositioningConfig { get; }

    private SearchObjectType _searchObjectType = SearchObjectType.Circle;
    public SearchObjectType SearchObjectType
    {
        get => _searchObjectType;
        set => this.RaiseAndSetIfChanged(ref _searchObjectType, value);
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

    /// <summary>Сохраненные профили пользователя.</summary>
    public ObservableCollection<UserProfileModel> SavedUserProfiles { get; set; }
        = new();

    private UserProfileModel? _currentProfile = null;
    public UserProfileModel? SelectedUserProfile
    {
        get => _currentProfile;
        set => this.RaiseAndSetIfChanged(ref _currentProfile, value);
    }

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

    private readonly string datasDirName = "data";

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

        if (!Directory.Exists(rectsDataDirPath))
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

    public async Task SaveCurrentProfile()
    {
        if (MethodConfigViewModel is null)
            return;

        var domainConfig = _mapper.Map<MethodConfiguration>(MethodConfigViewModel);
        UserProfile newUserProfile = new()
        {
            MethodConfiguration = domainConfig,
            SearchObjectType = SearchObjectType
        };
        await _db.UsersProfiles.AddAsync(newUserProfile);
        await _db.SaveChangesAsync();

        var profileModel = _mapper.Map<UserProfileModel>(newUserProfile);
        profileModel.MethodConfigType = MethodConfigViewModel.GetConfigType();

        SavedUserProfiles.Add(profileModel);
    }

    /// <summary>Установить профиль, выбранный во вкладке "Профили".</summary>
    public async Task SetSelectedSavedUserProfile()
    {
        if (SelectedUserProfile is null)
            return;

        UserProfile? userProfile = await _db.UsersProfiles
            .Include(up => up.MethodConfiguration)
            .FirstOrDefaultAsync(up => up.Id == SelectedUserProfile.Id);
        if (userProfile is null)
            throw new InvalidOperationException("Нет такого профиля в БД");

        var locatedConfig = _methodConfigurationLocator.GetLocatedMethodConfigViewModelOrDefault(userProfile.MethodConfiguration.ConfigType)!;
        _mapper.Map(userProfile.MethodConfiguration, locatedConfig);//Копирование данных в аллоцированный конфиг.
        SearchObjectType = userProfile.SearchObjectType;

        MethodConfigViewModel = locatedConfig;
    }

    /// <summary>Загрузка профилей из БД.</summary>
    public void LoadSavedUserProfiles()
    {
        SelectedUserProfile = null;
        var profileModels = _mapper.Map<IEnumerable<UserProfileModel>>(
            _db.UsersProfiles.Include(p => p.MethodConfiguration).AsEnumerable()
        );
        SavedUserProfiles.Clear();
        SavedUserProfiles.AddRange(profileModels);
    }

    [Serializable]
    internal class DictItem
    {
        public MethodConfigurationViewModel Key { get; set; }
        public List<RectangleInfo> Value { get; set; }
    }
}