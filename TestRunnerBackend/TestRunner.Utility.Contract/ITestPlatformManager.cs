namespace TestRunnerUtility.Contract;

public interface ITestPlatformManager
{
    public Task ChooseTargetBuilds(params string[] paths);
    
    public Task RunAllTests();
}