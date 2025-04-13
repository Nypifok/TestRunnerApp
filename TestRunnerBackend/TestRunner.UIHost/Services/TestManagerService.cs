using TestRunner.Contract.Grpc.V1;
using Grpc.Core;
using TestRunnerUtility.Contract;

namespace UIHost.Services;

internal class TestManagerService : TestManager.TestManagerBase
{
    private readonly ITestPlatformManager _testPlatformManager;

    public TestManagerService(ITestPlatformManager testPlatformManager)
    {
        _testPlatformManager = testPlatformManager ?? throw new ArgumentNullException(nameof(testPlatformManager));
    }

    public override async Task<RunAllTestsResponse> RunAllTests(RunAllTestsRequest request, ServerCallContext context)
    {
        await _testPlatformManager.RunAllTests();
        return new();
    }
}