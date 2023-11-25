using AutoMapper;
using AvaloniaFirstApp.Models;
using AvaloniaFirstApp.ViewModels;
using Domain.MethodConfigurations.Implementation;

namespace AvaloniaFirstApp.AutoMapper
{
    public class ModelsProfile : Profile
    {
        public ModelsProfile()
        {
            CreateMap<ViewModels.MethodConfigurationViewModel, Domain.MethodConfigurations.MethodConfiguration>()
                .Include<InterpolationMethodConfigurationViewModel, InterpolationMethodConfiguration>()
                .Include<WeightCoefficientsMethodConfigurationViewModel, WeightCoefficientsMethodConfiguration>()
                .Include<RecursialMethodConfigurationViewModel, RecursionMethodConfiguration>();
            
            CreateMap<InterpolationMethodConfigurationViewModel, InterpolationMethodConfiguration>();
            CreateMap<WeightCoefficientsMethodConfigurationViewModel, WeightCoefficientsMethodConfiguration>();
            CreateMap<RecursialMethodConfigurationViewModel, RecursionMethodConfiguration>();

            CreateMap<ViewModels.SearchObjectViewModel, SearchObjectType>()
                .ConvertUsing((source, d) =>
                {
                    if (source.Circle)
                        return SearchObjectType.Circle;
                    if (source.Line)
                        return SearchObjectType.Line;
                    if (source.NonDirectLine)
                        return SearchObjectType.NotDirectLine;
                    return SearchObjectType.None;
                });
        }
    }
}
