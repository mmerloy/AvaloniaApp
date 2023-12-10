using AvaloniaFirstApp.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaFirstApp.Infrastructure.Services.Prediction;

public interface IPredictionService
{
    Task<IEnumerable<DefectModel>> GetDefectsFromImageAsync(string imagePath, CancellationToken cToken = default);

    /// <summary>
    /// Получить картинку с отображенными дефектами.
    /// </summary>
    /// <param name="inputImagePath">Исходное изображение.</param>
    /// <returns>Путь к картинке с отображенными дефектами.</returns>
    Task<string> GetImageWithDefectsAsync(string inputImagePath, CancellationToken cToken = default);
}
