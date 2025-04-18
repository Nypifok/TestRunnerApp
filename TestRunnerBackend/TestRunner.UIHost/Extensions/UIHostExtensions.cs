using TestRunner.Utility.VSTestWrapper.Extensions;
using UIHost.Dispatchers;
using UIHost.Services;

namespace UIHost.Extensions;

internal static class UIHostExtensions
{
    public static IEndpointRouteBuilder UseGrpcServices(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<TestSessionService>();
        endpoints.MapGrpcService<ProcessingNotificationService>();
        return endpoints;
    }

    public static IServiceCollection AddGrpcServices(this IServiceCollection serviceCollection)
    {
        //Weak dependency on TestRunner.Utility.VSTestWrapper which implements TestRunnerUtility.Contract
        serviceCollection.AddVSTestWrapper();
        serviceCollection.AddAutoMapper(typeof(DefaultMappingProfile));
        serviceCollection.AddSingleton<NotificationsDispatcher>();
        serviceCollection.AddGrpc();
        return serviceCollection;
    }
}