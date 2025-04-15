using System.Diagnostics;
using TestRunnerUtility.Contract;

namespace TestRunner.Utility.VSTestWrapper.Managers;

internal class TestPlatformManager : ITestPlatformManager
{
    private readonly string testPlatformPath = Path.Combine(AppContext.BaseDirectory, "Tools", "TestPlatform", "vstest.console.exe");
    public TestPlatformManager()
    {
    }


    public Task ChooseTargetBuilds(params string[] paths)
    {
        throw new NotImplementedException();
    }

    public async Task RunAllTests()
    {
        Console.WriteLine($"Running Tests for {testPlatformPath}");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = testPlatformPath,
                Arguments = @"H:\pets\TestProject1\TestProject1\bin\Debug\net8.0\TestProject1.dll",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
        process.ErrorDataReceived += (s, e) => Console.Error.WriteLine(e.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();
    }
}