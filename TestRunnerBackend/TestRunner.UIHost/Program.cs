using TestRunner.Utility.VSTestWrapper.Extensions;
using UIHost.Extensions;

namespace UIHost;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddGrpc();
        
        //Weak dependency on TestRunner.Utility.VSTestWrapper which implements TestRunnerUtility.Contract
        builder.Services.AddVSTestWrapper();
        
        var app = builder.Build();
        
        //Registers all required Grpc services 
        app.UseGrpcServices();

        app.Run();
    }
}