using System.Diagnostics;
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
        var parentPid = ParseParentPid(args);
        if (parentPid != null)
        {
            StartParentWatcher(parentPid.Value);
        }
        
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
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

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            Console.WriteLine("UIHOST_READY");
        });
        
        await app.RunAsync();
    }
    private static int? ParseParentPid(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--parent" && int.TryParse(args[i + 1], out var pid))
                return pid;
        }
        return null;
    }

    private static void StartParentWatcher(int parentPid)
    {
        try
        {
            var parent = Process.GetProcessById(parentPid);
            parent.EnableRaisingEvents = true;
            parent.Exited += (s, e) =>
            {
                Environment.Exit(0);
            };
        }
        catch (Exception)
        {
            Environment.Exit(0);
        }
    }
}