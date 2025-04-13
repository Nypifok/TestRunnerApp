using UIHost.Services;

namespace UIHost.Extensions;

public static class UIHostExtensions
{
    public static IEndpointRouteBuilder UseGrpcServices(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<FileManagerService>();
        endpoints.MapGrpcService<TestManagerService>();
        return endpoints;
    }
}