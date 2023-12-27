using AutoMapper;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using AvaloniaFirstApp.Infrastructure.Services;
using AvaloniaFirstApp.Infrastructure.Services.Notifications;
using AvaloniaFirstApp.Infrastructure.Services.Prediction;
using AvaloniaFirstApp.Models;
using Domain;
using Domain.Defects;
using Domain.MethodConfigurations;
using DynamicData;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Utils;

namespace AvaloniaFirstApp.ViewModels;


/// <summary>
/// Это наша вью-модель!
/// </summary>
public class MainWindowViewModel : ReactiveUI.ReactiveObject
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly MethodConfigurationViewModelsLocator _methodConfigurationLocator;
    private readonly IPredictionService _predictionService;
    private readonly INotifier _notifier;

    public MainWindowViewModel(
        ApplicationDbContext db,
        IMapper mapper,
        MethodConfigurationViewModelsLocator methodConfigurationLocator,
        IPredictionService predictionService,
        INotifier notifier)
    {
        _db = db;
        _mapper = mapper;
        _methodConfigurationLocator = methodConfigurationLocator;
        _predictionService = predictionService;
        _notifier = notifier;
        MethodConfigViewModel = _methodConfigurationLocator.GetLocatedMethodConfigViewModelOrDefault(MethodConfigType.Interpolation);

        PositioningConfig = new();
        PositioningConfig.SizesChanged += SetNewSizesToRectangles;
    }

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    /// <summary>Данные о сохраненных прямоугольниках в координатах действительного источника изображения, не UI.</summary>
    private readonly Dictionary<MethodConfigurationViewModel, List<RectangleInfo>> _rectsConfigsData = new();

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

    public bool _isPredictionLoading = false;

    public bool IsPredictionLoading
    {
        get => _isPredictionLoading;
        set => this.RaiseAndSetIfChanged(ref _isPredictionLoading, value);
    }

    /// <summary>Данные о файле, в котором храниться демонстрируемое в UI изображение <see cref="SourceImage"/>.</summary>
    public IStorageFile? ImageStorageFile { get; private set; }

    /// <summary>Сохраненные профили пользователя.</summary>
    public ObservableCollection<UserProfileModel> SavedUserProfiles { get; set; }
        = new();

    public ObservableCollection<DefectModel> SavedDefects { get; set; }
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
        CurrentImageDefects.Clear();
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
    public ObservableCollection<DefectModel> CurrentImageDefects { get; set; }
        = new ObservableCollection<DefectModel>();

    public async Task MakePrediction()
    {
        if (ImageStorageFile is null)
            return;

        CurrentImageDefects.Clear();

        IEnumerable<DefectModel>? defects;

        IsPredictionLoading = true;
        try
        {
            defects = await _predictionService.GetDefectsFromImageAsync(ImageStorageFile.Path.LocalPath);
        }
        finally
        {
            IsPredictionLoading = false;
        }
        if (defects is null)
        {
            await _notifier.NotifyErrorAsync("Невозможно обнаружить дефекты на данном изображении.");
            return;
        }
        if(defects.IsEmpty())
        {
            await _notifier.NotifyWarningAsync("Дефекты не найдены.");
            return;
        }

        CurrentImageDefects.AddRange(
            defects.Select(d => { d.Location = ModifyRectByConfigs(d.Location); return d; })
        );

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
            var modifiedDefectsRects = CurrentImageDefects.ModifyForEach(d => d.Location = ModifyRectByConfigs(d.Location));
            CurrentImageDefects.Clear();
            CurrentImageDefects.AddRange(modifiedDefectsRects);
        }
        else
        {
            var imgRelatedRects = CurrentImageDefects
                .ModifyForEach(d => d.Location = ModifyRectToSave(d.Location, oldFieldWidth, oldFieldHeight))
                .ToList();
            CurrentImageDefects.Clear();
            var newUIRelatedRects = imgRelatedRects
                .ModifyForEach(d => d.Location = ModifyRectByConfigs(d.Location))
                .ToList();
            CurrentImageDefects.AddRange(newUIRelatedRects);
        }
    }

    public async Task SaveDefectsFromImage()
    {
        if (ImageStorageFile is null)
            return;

        double fieldWidth = PositioningConfig.XMultiplexer;
        double fieldHeight = PositioningConfig.YMultiplexer;

        ImageEntity imageEntity = new()
        {
            FullPath = ImageStorageFile.Path.LocalPath
        };

        List<Defect> defectsToSave =
            _mapper.Map<IEnumerable<Defect>>(CurrentImageDefects.ModifyForEach(d => d.Location = ModifyRectToSave(d.Location, fieldWidth, fieldHeight)))
            .ToList();


        await _db.ImageEntities.AddAsync(imageEntity);
        await _db.Defects.AddRangeAsync(defectsToSave.ModifyForEach(de => de.PredictedFromImage = imageEntity));
        await _db.SaveChangesAsync();

        SavedDefects.AddRange(_mapper.Map<IEnumerable<DefectModel>>(defectsToSave));
    }

    public async Task SaveCurrentProfile()
    {
        if (MethodConfigViewModel is null)
            return;

        UserProfileSavingWindow userProfileSavingWindow = new();
        var getUserProfileNameResult = await userProfileSavingWindow.ShowDialog<string>(App.MainWindow!);
        if (getUserProfileNameResult is null)
            return;

        var domainConfig = _mapper.Map<MethodConfiguration>(MethodConfigViewModel);
        UserProfile newUserProfile = new()
        {
            MethodConfiguration = domainConfig,
            SearchObjectType = SearchObjectType,
            Title = getUserProfileNameResult
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

    /// <summary>Загрузка дефектов из БД.</summary>
    public void LoadSavedDefects()
    {
        var defectsModels = _mapper.Map<IEnumerable<DefectModel>>(
            _db.Defects.Include(d => d.PredictedFromImage).AsEnumerable()
        );
        SavedDefects.Clear();
        SavedDefects.AddRange(defectsModels);
    }

    [Serializable]
    internal class DictItem
    {
        public MethodConfigurationViewModel Key { get; set; }
        public List<RectangleInfo> Value { get; set; }
    }
}