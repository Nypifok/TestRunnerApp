using TestRunner.Entities;

namespace TestRunnerUtility.Contract;

public enum BuildAccessStatus
{
    Ok,
    PathDoesNotExist,
    AccessDenied,
    InUseByAnotherProcess,
    UnknownError
}

public interface IVSTestManager
{
    /// <summary>
    /// Selects target builds and validates them
    /// </summary>
    /// <param name="targetBuilds">Collection of paths to targets</param>
    /// <param name="callback">Notification callback which will be executed with each processing update</param>
    /// <param name="errorHandler">Handles an error</param>
    /// <returns>Status of operation result <see cref="BuildAccessStatus"/></returns>
    public Task<BuildAccessStatus> DiscoverTestsAsync(IEnumerable<string> targetBuilds,
        Func<NotificationBase, Action<Exception>, Task> callback, Action<Exception> errorHandler);

    /// <summary>
    /// Run all discovered tests
    /// </summary>
    /// <param name="targetBuilds">Collection of paths to targets</param>
    /// <param name="callback">Notification callback which will be executed with each processing update</param>
    /// <param name="errorHandler">Handles an error</param>
    /// <returns>Status of operation result <see cref="BuildAccessStatus"/></returns>
    public Task<BuildAccessStatus> RunAllTests(IEnumerable<string> targetBuilds, Func<NotificationBase, Action<Exception>, Task> callback, Action<Exception> errorHandler);
    
    /// <summary>
    /// Run selected tests
    /// </summary>
    /// <param name="testCases">Collection of targeted test cases</param>
    /// <param name="callback">Notification callback which will be executed with each processing update</param>
    /// <param name="errorHandler">Handles an error</param>
    public Task RunSelectedTests(IEnumerable<TestCase> testCases, Func<NotificationBase, Action<Exception>, Task> callback, Action<Exception> errorHandler);
}