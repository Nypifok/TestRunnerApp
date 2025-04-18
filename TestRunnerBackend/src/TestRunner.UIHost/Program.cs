using UIHost.Extensions;
using Serilog;

namespace UIHost;

internal class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "Logs", "log.txt"),
                rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();

        //Registers dependencies for grpc services 
        builder.Services.AddGrpcServices();

        var app = builder.Build();

        //Registers all required Grpc services 
        app.UseGrpcServices();

        app.Run();
    }
}