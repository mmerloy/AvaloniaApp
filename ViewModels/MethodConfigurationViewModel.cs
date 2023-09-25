using AvaloniaFirstApp.Models;
using ReactiveUI;
using System;
using System.Text.Json.Serialization;

namespace AvaloniaFirstApp.ViewModels;

[Serializable]
[JsonDerivedType(typeof(MethodConfigurationViewModel), typeDiscriminator: "base")]
[JsonDerivedType(typeof(InterpolationMethodConfigurationViewModel), typeDiscriminator: "Interpolation")]
[JsonDerivedType(typeof(RecursialMethodConfigurationViewModel), typeDiscriminator: "Recurtial")]
[JsonDerivedType(typeof(WeightCoefficientsMethodConfigurationViewModel), typeDiscriminator: "Weight")]

public class MethodConfigurationViewModel : ReactiveObject, IEquatable<MethodConfigurationViewModel>
{
    //public virtual string MethodName => "BaseMethod";

    private SearchObjectType _searchObject;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SearchObjectType SearchObject
    {
        get => _searchObject;
        set => this.RaiseAndSetIfChanged(ref _searchObject, value);
    }


    private double _inaccuracy = 0;
    public double Inaccuracy
    {
        get => _inaccuracy;
        set => this.RaiseAndSetIfChanged(ref _inaccuracy, value);
    }

    public bool Equals(MethodConfigurationViewModel? other)
    {
        return Inaccuracy == other.Inaccuracy && SearchObject == other.SearchObject;
    }
}