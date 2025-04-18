using AutoMapper;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace TestRunner.Utility.VSTestWrapper.Extensions;

internal class DefaultMappingProfile : Profile
{
    public DefaultMappingProfile()
    {
        CreateMap<TestCase, TestRunner.Entities.TestCase>()
            .ForMember(dest => dest.ExecutorUri, opt => opt.MapFrom(src => src.ExecutorUri.ToString()));
        CreateMap<TestRunner.Entities.TestCase, TestCase>()
            .ForMember(dest => dest.ExecutorUri, opt => opt.MapFrom(src => new Uri(src.ExecutorUri)));
        CreateMap<TestResult, TestRunner.Entities.TestResult>()
            .ForMember(dest => dest.DurationMs, opt => opt.MapFrom(src => src.Duration.Milliseconds))
            .ForMember(dest => dest.TestCaseId, opt => opt.MapFrom(src => src.TestCase.Id.ToString()))
            .ForMember(dest => dest.ErrorStackTrace, opt => opt.MapFrom(src => src.ErrorStackTrace ?? ""))
            .ForMember(dest => dest.ErrorMessage, opt => opt.MapFrom(src => src.ErrorMessage ?? ""))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName ?? "Name not found"));
    }
}