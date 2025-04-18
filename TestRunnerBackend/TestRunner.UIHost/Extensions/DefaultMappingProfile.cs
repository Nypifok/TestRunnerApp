using AutoMapper;
using TestRunner.Contract.Grpc.V1;

namespace UIHost.Extensions;

internal class DefaultMappingProfile : Profile
{
    public DefaultMappingProfile()
    {
        CreateMap<TestRunner.Entities.TestCase, TestCase>().ReverseMap();
        CreateMap<TestRunner.Entities.TestsDiscoveryFinishedNotification, TestsDiscoveryFinishedNotification>()
            .ForMember(dest => dest.TestCases, opt => opt.Ignore())
            .AfterMap((src, dest, ctx) =>
            {
                dest.TestCases.AddRange(src.TestCases.Select(r => ctx.Mapper.Map<TestCase>(r)));
            });
        CreateMap<TestRunner.Entities.TestsDiscoveryUpdatedNotification, TestsDiscoveryUpdatedNotification>()
            .ForMember(dest => dest.TestCases, opt => opt.Ignore())
            .AfterMap((src, dest, ctx) =>
            {
                dest.TestCases.AddRange(src.TestCases.Select(r => ctx.Mapper.Map<TestCase>(r)));
            });
        
        CreateMap<TestRunner.Entities.TestResult, TestResult>();
        CreateMap<TestRunner.Entities.TestsRunFinishedNotification, TestsRunFinishedNotification>()
            .ForMember(dest => dest.TestResults, opt => opt.Ignore())
            .AfterMap((src, dest, ctx) =>
            {
                dest.TestResults.AddRange(src.TestResults.Select(r => ctx.Mapper.Map<TestResult>(r)));
            });
        CreateMap<TestRunner.Entities.TestsRunUpdatedNotification, TestsRunUpdatedNotification>()
            .ForMember(dest => dest.TestResults, opt => opt.Ignore())
            .AfterMap((src, dest, ctx) =>
            {
                dest.TestResults.AddRange(src.TestResults.Select(r => ctx.Mapper.Map<TestResult>(r)));
                dest.ActiveTestCases.AddRange(src.ActiveTestCases.Select(r => ctx.Mapper.Map<TestCase>(r)));
            });
    }
}