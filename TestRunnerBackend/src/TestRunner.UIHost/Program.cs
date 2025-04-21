using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using UIHost.Extensions;
using Serilog;

namespace UIHost;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "Logs", "log.txt"),
                rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddCommandLine(args);
        
        builder.Host.UseSerilog();

        //Registers dependencies for grpc services 
        builder.Services.AddGrpcServices();



        int port = Convert.ToUInt16(builder.Configuration["port"] ?? "5128");

        
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Loopback, port, listenOptions =>
            {
                
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });
        
        var app = builder.Build();
    
        //Registers all required Grpc services 
        app.UseGrpcServices();

        app.Run();
    }
}