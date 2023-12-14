using Domain.MethodConfigurations;
using ReactiveUI;
using System;

namespace AvaloniaFirstApp.ViewModels;

/// <summary>Метод весовых коэффициентов</summary>
public class WeightCoefficientsMethodConfigurationViewModel : MethodConfigurationViewModel, IEquatable<WeightCoefficientsMethodConfigurationViewModel>
{
    private bool _color;
    private bool _contrastRatio;
    private bool _brightness;

    public bool Color
    {
        get => _color;
        set => this.RaiseAndSetIfChanged(ref _color, value);
    }

    public bool ContrastRatio
    {
        get => _contrastRatio;
        set => this.RaiseAndSetIfChanged(ref _contrastRatio, value);
    }

    public bool Brightness
    {
        get => _brightness;
        set => this.RaiseAndSetIfChanged(ref _brightness, value);
    }

    public override MethodConfigType GetConfigType() => MethodConfigType.Weighted;

    public bool Equals(WeightCoefficientsMethodConfigurationViewModel? other)
    {
        var x = this;
        var y = other;
        return x.Color == y.Color && x.ContrastRatio == y.ContrastRatio && x.Brightness == y.Brightness && base.Equals((MethodConfigurationViewModel)other);
    }
}
