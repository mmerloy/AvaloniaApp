using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
namespace AvaloniaFirstApp;


/// <summary>
/// Это наша вью-модель!
/// </summary>
public class MainVindowViewModel
{
    public double MyDoubleValue { get; set; }


    //public string SomeText { get; set; }

    /// <summary>
    /// Источник данных для добавления изображений плат
    /// </summary>
    public Bitmap SourceImage { get; set; } = new Bitmap(@"D:\Downloads\Новая папка\image1.webp");
}
