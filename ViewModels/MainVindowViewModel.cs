using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
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

        SourceImage = new Bitmap(files.First().Path.LocalPath);
    }
}
