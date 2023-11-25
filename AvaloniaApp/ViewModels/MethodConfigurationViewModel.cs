using AvaloniaFirstApp.Models;
using Domain.MethodConfigurations;
using ReactiveUI;
using System;
using System.Text.Json.Serialization;

namespace AvaloniaFirstApp.ViewModels;

[Serializable]
[JsonDerivedType(typeof(MethodConfigurationViewModel), typeDiscriminator: "base")]
[JsonDerivedType(typeof(InterpolationMethodConfigurationViewModel), typeDiscriminator: "Interpolation")]
[JsonDerivedType(typeof(RecursialMethodConfigurationViewModel), typeDiscriminator: "Recurtial")]
[JsonDerivedType(typeof(WeightCoefficientsMethodConfigurationViewModel), typeDiscriminator: "Weight")]
public abstract class MethodConfigurationViewModel : ReactiveObject
{
    //private SearchObjectType _searchObject;

    //[JsonConverter(typeof(JsonStringEnumConverter))]
    //public SearchObjectType SearchObject
    //{
    //    get => _searchObject;
    //    set => this.RaiseAndSetIfChanged(ref _searchObject, value);
    //}

    public abstract MethodConfigType GetConfigType();

    private double _inaccuracy = 0;
    public double Inaccuracy
    {
        get => _inaccuracy;
        set => this.RaiseAndSetIfChanged(ref _inaccuracy, value);
    }

    //public bool Equals(MethodConfigurationViewModel? other)
    //{
    //    return Inaccuracy == other.Inaccuracy && SearchObject == other.SearchObject;
    //}
}