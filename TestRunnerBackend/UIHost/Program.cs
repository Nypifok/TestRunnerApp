using System.Text;

using UIHost.Services;

namespace UIHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddGrpc();

        var app = builder.Build();

        app.MapGrpcService<FileManagerService>();

        app.Run();
    }
}