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
        set
        {
            this.RaiseAndSetIfChanged(ref _xMultiplexer, value);
            SizesChanged?.Invoke(_xMultiplexer, _yMultiplexer);
        }
    }

    public double YMultiplexer
    {
        get => _yMultiplexer;
        set
        {
            this.RaiseAndSetIfChanged(ref _yMultiplexer, value);
            SizesChanged?.Invoke(_xMultiplexer, _yMultiplexer);
        }
    }

    public event Action<double, double>? SizesChanged;
}
