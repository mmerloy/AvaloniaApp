using ReactiveUI;

namespace AvaloniaFirstApp.ViewModels;

public class RecursialMethodConfigurationViewModel : MethodConfigurationViewModel
{
    double _notAllCoverage = 1;

    public double NotAllCoverage
    {
        get => _notAllCoverage;
        set => this.RaiseAndSetIfChanged(ref _notAllCoverage, value);
    }
}
