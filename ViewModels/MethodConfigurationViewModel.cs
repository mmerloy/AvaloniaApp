using ReactiveUI;

namespace AvaloniaFirstApp.ViewModels;

public abstract class MethodConfigurationViewModel : ReactiveObject
{
    double _inaccuracy = 0;

    public double Inaccuracy
    {
        get => _inaccuracy;
        set => this.RaiseAndSetIfChanged(ref _inaccuracy, value);
    }
}