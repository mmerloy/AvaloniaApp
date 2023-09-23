using ReactiveUI;

namespace AvaloniaFirstApp.ViewModels;

public class PositioningConfigViewModel : ReactiveUI.ReactiveObject
{
    private double _xMultiplexer;
    private double _yMultiplexer;

    public double XMultiplexer
    {
        get => _xMultiplexer;
        set => this.RaiseAndSetIfChanged(ref _xMultiplexer, value);
    }

    public double YMultiplexer
    {
        get => _yMultiplexer;
        set => this.RaiseAndSetIfChanged(ref _yMultiplexer, value);
    }
}
