using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer.Interfaces;
using TestRunner.Utility.VSTestWrapper.Managers;
using TestRunner.Utility.VSTestWrapper.Services;
using TestRunnerUtility.Contract;

namespace TestRunner.Utility.VSTestWrapper.Extensions;

public static class VSWrapperExtensions
{
    private static readonly string _vsConsolePath =
        Path.Combine(AppContext.BaseDirectory, "Tools", "TestPlatform", "vstest.console.exe");

    public static IServiceCollection AddVSTestWrapper(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IVSTestManager, VSTestManager>();
        
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        serviceCollection.AddLogging();

        //TODO: Re-think log levels if aspcore not in use
        var traceLevel = (env is not null && env == "Development") ? TraceLevel.Verbose : TraceLevel.Off;

        var wrapper = new VsTestConsoleWrapper(_vsConsolePath, new ConsoleParameters()
        {
            TraceLevel = traceLevel,
            InheritEnvironmentVariables = true,
            LogFilePath = Path.Combine(AppContext.BaseDirectory, "Logs", "vsTestConsoleLog.txt")
        });
        wrapper.StartSession();
        serviceCollection.AddTransient<IVsTestConsoleWrapper, VsTestConsoleWrapper>((x)=>wrapper);
        serviceCollection.AddSingleton<VsTestConsoleService>();
        
        serviceCollection.AddAutoMapper(typeof(DefaultMappingProfile));
        return serviceCollection;
    }
}