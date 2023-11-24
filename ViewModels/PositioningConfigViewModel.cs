using ReactiveUI;
using System;

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

    public void SetCoefs(double x, double y)
    {
        double oldX =_xMultiplexer, oldY = _yMultiplexer;
        XMultiplexer = x; YMultiplexer = y;
        SizesChanged?.Invoke(oldX, oldY);
    }

    public event Action<double, double>? SizesChanged;
}
