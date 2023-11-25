using ReactiveUI;
using System;

namespace AvaloniaFirstApp.ViewModels;

public class InterpolationMethodConfigurationViewModel : MethodConfigurationViewModel, IEquatable<InterpolationMethodConfigurationViewModel>
{
    private byte _interpolationCount;

    /// <summary>Количество интервалов интерполяции.</summary>
    public byte InterpolationCount
    {
        get => _interpolationCount;
        set => this.RaiseAndSetIfChanged(ref _interpolationCount, value);
    }

    public bool Equals(InterpolationMethodConfigurationViewModel? other)
    {
        var x = this;
        var y = other;
        return x.InterpolationCount == y.InterpolationCount && base.Equals((MethodConfigurationViewModel)other);
    }
}
