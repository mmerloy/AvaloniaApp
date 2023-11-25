using Avalonia.Controls;
using AvaloniaFirstApp.ViewModels;
using Domain.MethodConfigurations;

namespace AvaloniaFirstApp.Infrastructure.Services;

public class MethodConfigurationViewModelsLocator
{
    private readonly IResourceDictionary _resources;

    public MethodConfigurationViewModelsLocator()
    {
        _resources = Avalonia.Application.Current!.Resources;
    }

    /// <summary>Получить аллоцированную в App.xaml MV</summary>
    public MethodConfigurationViewModel? GetLocatedMethodConfigViewModelOrDefault(MethodConfigType methodType)
        => methodType switch
        {
            MethodConfigType.Interpolation => _resources["InterpolationMethodConfigurationViewModel"] as MethodConfigurationViewModel,
            MethodConfigType.Recursion => _resources["RecursialMethodConfigurationViewModel"] as MethodConfigurationViewModel,
            MethodConfigType.Weighted => _resources["WeightCoefficientsMethodConfigurationViewModel"] as MethodConfigurationViewModel,
            _ => null,
        };

}
