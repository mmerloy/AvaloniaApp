using AvaloniaFirstApp.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaFirstApp.Infrastructure.Services.Prediction;

public interface IPredictionService
{
    Task<IEnumerable<DefectModel>> GetDefectsFromImageAsync(string imagePath, CancellationToken cToken = default);
}
