using AutoMapper;
using AvaloniaFirstApp.Models;
using AvaloniaFirstApp.ViewModels;
using Domain;
using Domain.MethodConfigurations;
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
                .Include<RecursialMethodConfigurationViewModel, RecursionMethodConfiguration>()
                .ReverseMap();
            
            CreateMap<InterpolationMethodConfigurationViewModel, InterpolationMethodConfiguration>().ReverseMap();
            CreateMap<WeightCoefficientsMethodConfigurationViewModel, WeightCoefficientsMethodConfiguration>().ReverseMap();
            CreateMap<RecursialMethodConfigurationViewModel, RecursionMethodConfiguration>().ReverseMap();

            CreateMap<UserProfile, UserProfileModel>()
                .ForMember(dest => dest.MethodConfigType, opt => opt.MapFrom(
                    source => source.MethodConfiguration.ConfigType));
        }
    }
}
