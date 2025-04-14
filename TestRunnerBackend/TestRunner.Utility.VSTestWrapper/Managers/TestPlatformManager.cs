using System.Diagnostics;
using TestRunnerUtility.Contract;

namespace TestRunner.Utility.VSTestWrapper.Managers;

internal class TestPlatformManager:ITestPlatformManager
{
    public TestPlatformManager()
    {
        
    }
    public async Task RunAllTests()
    {
        var vstestPath = Path.Combine(AppContext.BaseDirectory, "vstestconsole", "vstest.console.exe");
        Console.WriteLine($"Running Tests for {vstestPath}");
        // var process = new Process
        // {
        //     StartInfo = new ProcessStartInfo
        //     {
        //         FileName = vstestPath,
        //         Arguments = @"H:\pets\TestProject1\TestProject1\bin\Debug\net8.0\TestProject1.dll",
        //         RedirectStandardOutput = true,
        //         RedirectStandardError = true,
        //         UseShellExecute = false
        //     }
        // };
        //
        // process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
        // process.ErrorDataReceived += (s, e) => Console.Error.WriteLine(e.Data);
        //
        // process.Start();
        // process.BeginOutputReadLine();
        // process.BeginErrorReadLine();
        // process.WaitForExit();
        
    }
}