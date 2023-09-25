using ReactiveUI;

namespace AvaloniaFirstApp.ViewModels;

public class InterpolationMethodConfigurationViewModel : MethodConfigurationViewModel
{
    private byte _interpolationCount;

    /// <summary>Количество интервалов интерполяции.</summary>
    public byte InterpolationCount
    {
        get => _interpolationCount;
        set => this.RaiseAndSetIfChanged(ref _interpolationCount, value);
    }
}
