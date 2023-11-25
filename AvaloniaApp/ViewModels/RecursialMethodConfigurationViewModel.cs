using Domain.MethodConfigurations;
using ReactiveUI;
using System;

namespace AvaloniaFirstApp.ViewModels;

public class RecursialMethodConfigurationViewModel : MethodConfigurationViewModel, IEquatable<RecursialMethodConfigurationViewModel>
{
    private double _notAllCoverage = 1;

    public double NotAllCoverage
    {
        get => _notAllCoverage;
        set => this.RaiseAndSetIfChanged(ref _notAllCoverage, value);
    }

    public override MethodConfigType GetConfigType() => MethodConfigType.Recursion;

    public bool Equals(RecursialMethodConfigurationViewModel? other)
    {
        var x = this;
        var y = other;
        return x.NotAllCoverage == y.NotAllCoverage && base.Equals((MethodConfigurationViewModel)other); ;
    }
}
