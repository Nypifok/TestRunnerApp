using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using TestRunner.Entities;
using TestRunner.Utility.VSTestWrapper.Services;
using TestRunnerUtility.Contract;
using InnerTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace TestRunner.Utility.VSTestWrapper.Managers;

internal class VSTestManager : IVSTestManager
{
    private readonly VsTestConsoleService _consoleService;
    private readonly IMapper _mapper;
    private readonly ILogger<VSTestManager> _logger;

    public VSTestManager(VsTestConsoleService wrapper, IMapper mapper, ILogger<VSTestManager> logger)
    {
        _mapper = mapper;
        _logger = logger;
        _consoleService = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
    }

    public async Task<BuildAccessStatus> DiscoverTestsAsync(IEnumerable<string> targetBuilds,
        Func<NotificationBase, Action<Exception>, Task> callback, Action<Exception> errorHandler)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var targetBuildsEnumerated = targetBuilds.ToArray();
        var validationResult = await ValidateTargetBuilds(targetBuildsEnumerated);

        if (validationResult != BuildAccessStatus.Ok)
            return validationResult;

        await _consoleService.DiscoverTestCases(targetBuildsEnumerated,
            async (testCases) => await DiscoverTestsUpdateCallback(testCases, callback, errorHandler),
            async (testCases) => await DiscoverTestsFinishedCallback(testCases, callback, errorHandler),
            errorHandler
        );

        return validationResult;
    }

    public async Task<BuildAccessStatus> RunAllTests(IEnumerable<string> targetBuilds,
        Func<NotificationBase, Action<Exception>, Task> callback,
        Action<Exception> errorHandler)
    {
        ArgumentNullException.ThrowIfNull(callback);
        var targetBuildsEnumerated = targetBuilds.ToArray();
        var validationResult = await ValidateTargetBuilds(targetBuildsEnumerated);

        if (validationResult != BuildAccessStatus.Ok)
            return validationResult;

        await _consoleService.RunAllTests(targetBuildsEnumerated,
            async (updateArgs) => await RunTestsUpdateCallback(updateArgs, callback, errorHandler),
            async (finishedArgs, updateArgs) =>
                await RunAllTestsFinishedCallback(finishedArgs, updateArgs, callback, errorHandler),
            errorHandler);

        return validationResult;
    }

    public async Task RunSelectedTests(IEnumerable<TestCase> testCases,
        Func<NotificationBase, Action<Exception>, Task> callback, Action<Exception> errorHandler)
    {
        ArgumentNullException.ThrowIfNull(callback);

        //TODO: check build accessibility
        await _consoleService.RunSelectedTests(_mapper.Map<IEnumerable<InnerTestCase>>(testCases),
            async (updateArgs) => await RunTestsUpdateCallback(updateArgs, callback, errorHandler),
            async (finishedArgs, updateArgs) =>
                await RunAllTestsFinishedCallback(finishedArgs, updateArgs, callback, errorHandler),
            errorHandler);
    }

    private async Task<BuildAccessStatus> ValidateTargetBuilds(IEnumerable<string> targetBuilds)
    {
        var targetBuildsEnumerated = targetBuilds as string[] ?? targetBuilds.ToArray();
        try
        {
            foreach (var targetBuild in targetBuildsEnumerated)
            {
                if (!targetBuild.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || !File.Exists(targetBuild))
                    return BuildAccessStatus.PathDoesNotExist;


                await using var accessCheckStream =
                    File.Open(targetBuild, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
        }
        catch (UnauthorizedAccessException)
        {
            return BuildAccessStatus.AccessDenied;
        }
        catch (IOException ex)
        {
            return ex.HResult == -2147024864 ? BuildAccessStatus.InUseByAnotherProcess : BuildAccessStatus.AccessDenied;
        }
        catch
        {
            return BuildAccessStatus.UnknownError;
        }

        return BuildAccessStatus.Ok;
    }

    #region Callbacks

    private async Task RunTestsUpdateCallback(TestRunChangedEventArgs updateArgs,
        Func<NotificationBase, Action<Exception>, Task> innerCallback,
        Action<Exception> errorHandler)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(innerCallback);
            var notification = new TestsRunUpdatedNotification
            {
                ActiveTestCases = updateArgs?.ActiveTests is not null
                    ? _mapper.Map<IEnumerable<TestCase>>(updateArgs.ActiveTests)
                    : [],
                TestResults = updateArgs?.NewTestResults is not null
                    ? _mapper.Map<IEnumerable<TestResult>>(updateArgs.NewTestResults)
                    : [],
            };

            await innerCallback(notification, errorHandler);
        }
        catch (Exception exception)
        {
            errorHandler(exception);
        }
    }

    private async Task RunAllTestsFinishedCallback(TestRunCompleteEventArgs finishedArgs,
        TestRunChangedEventArgs updateArgs, Func<NotificationBase, Action<Exception>, Task> innerCallback,
        Action<Exception> errorHandler)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(innerCallback);
            var notification = new TestsRunFinishedNotification
            {
                TotalElapsedMilliseconds = finishedArgs.ElapsedTimeInRunningTests.TotalMilliseconds,
                TestResults = updateArgs?.NewTestResults is not null
                    ? _mapper.Map<IEnumerable<TestResult>>(updateArgs.NewTestResults)
                    : []
            };


            _logger.LogInformation($"Test run completed total time: {notification.TotalElapsedMilliseconds}ms");
            await innerCallback(notification, errorHandler);
        }
        catch (Exception e)
        {
            errorHandler(e);
        }
    }

    private async Task DiscoverTestsUpdateCallback(
        IEnumerable<InnerTestCase> testCases,
        Func<NotificationBase, Action<Exception>, Task> innerCallback, Action<Exception> errorHandler)
    {
        try
        {
            var notification = new TestsDiscoveryUpdatedNotification
            {
                TestCases = _mapper.Map<IEnumerable<TestCase>>(testCases)
            };
            await innerCallback(notification, errorHandler);
        }
        catch (Exception ex)
        {
            errorHandler(ex);
        }
    }

    private async Task DiscoverTestsFinishedCallback(
        IEnumerable<InnerTestCase> testCases,
        Func<NotificationBase, Action<Exception>, Task> innerCallback, Action<Exception> errorHandler)
    {
        try
        {
            var notification = new TestsDiscoveryFinishedNotification()
            {
                TestCases = _mapper.Map<IEnumerable<TestCase>>(testCases)
            };
            await innerCallback(notification, errorHandler);
        }
        catch (Exception ex)
        {
            errorHandler(ex);
        }
    }

    #endregion
}