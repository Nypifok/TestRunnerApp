using Microsoft.Extensions.DependencyInjection;
using TestRunner.Utility.VSTestWrapper.Managers;
using TestRunnerUtility.Contract;

namespace TestRunner.Utility.VSTestWrapper.Extensions;

public static class VSWrapperExtensions
{
    public static IServiceCollection AddVSTestWrapper(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ITestPlatformManager, TestPlatformManager>();
        return serviceCollection;
    }
}