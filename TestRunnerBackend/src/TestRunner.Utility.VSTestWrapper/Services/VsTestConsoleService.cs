using Microsoft.Extensions.Logging;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using TestRunner.Utility.VSTestWrapper.Managers.EventHandlers;
using TestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace TestRunner.Utility.VSTestWrapper.Services;

public class VsTestConsoleService : IDisposable
{
    private readonly IVsTestConsoleWrapper _defaultWrapper;

    private readonly object _currentOperationNameLock = new();
    private string _currentOperationName;
    private readonly AutoResetEvent _currentOperationFinishedEvent = new(false);
    private readonly ILogger<VsTestConsoleService> _logger;

    private const string DEFAULT_RUN_SETTINGS = "<RunSettings><RunConfiguration></RunConfiguration></RunSettings>";

    public VsTestConsoleService(IVsTestConsoleWrapper wrapper, ILogger<VsTestConsoleService> logger)
    {
        _defaultWrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task DiscoverTestCases(string[] targetBuilds,
        Func<IEnumerable<TestCase>, Task> updateCallback,
        Func<IEnumerable<TestCase>, Task> finishedCallback,
        Action<Exception> errorHandler)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(updateCallback);
            ArgumentNullException.ThrowIfNull(finishedCallback);
            CancelCurrentOperation(nameof(DiscoverTestCases));
            Task.Run(() =>
            {
                try
                {
                    using var waitHandle = new AutoResetEvent(false);

                    var discoveryEventHandler = new DiscoveryEventHandler(waitHandle,
                        async (testCases) =>
                            await DiscoverTestsUpdatedCallback(testCases, updateCallback, errorHandler),
                        async (testCases) =>
                            await DiscoverTestsFinishedCallback(testCases, finishedCallback, errorHandler));

                    _logger.LogTrace(
                        $"{nameof(DiscoverTestCases)}: execution started, target builds: {string.Join(", ", targetBuilds)}");

                    _defaultWrapper.DiscoverTests(targetBuilds, DEFAULT_RUN_SETTINGS, discoveryEventHandler);
                    waitHandle.WaitOne();

                    _logger.LogTrace(
                        $"{nameof(DiscoverTestCases)}: execution finished, target builds: {string.Join(", ", targetBuilds)}");
                }
                catch (Exception exception)
                {
                    _logger.LogTrace(exception,
                        $"{nameof(DiscoverTestCases)} an error occured, passing to error handler.");

                    errorHandler(exception);
                }
            });
        }
        catch (Exception exception)
        {
            _logger.LogTrace(exception, $"{nameof(DiscoverTestCases)} an error occured, passing to error handler.");

            errorHandler(exception);
        }

        return Task.CompletedTask;
    }

    public Task RunAllTests(string[] targetBuilds, Func<TestRunChangedEventArgs, Task> updateCallback,
        Func<TestRunCompleteEventArgs, TestRunChangedEventArgs, Task> finishedCallback,
        Action<Exception> errorHandler)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(updateCallback);
            ArgumentNullException.ThrowIfNull(finishedCallback);
            CancelCurrentOperation(nameof(RunAllTests));
            Task.Run(() =>
            {
                try
                {
                    using var waitHandle = new AutoResetEvent(false);

                    var handler = new RunEventHandler(waitHandle,
                        (updateEventArgs) =>
                            RunTestsUpdatedCallback(updateEventArgs, updateCallback, errorHandler),
                        (completeArgs, updateArgs) =>
                            RunTestsFinishedCallback(completeArgs, updateArgs, finishedCallback, errorHandler));
                    
                    _logger.LogTrace(
                        $"{nameof(RunAllTests)}: execution started, target builds: {string.Join(", ", targetBuilds)}");
                    
                    _defaultWrapper.RunTests(
                        targetBuilds,
                        DEFAULT_RUN_SETTINGS, handler);
                    
                    _logger.LogTrace(
                        $"{nameof(RunAllTests)}: execution finished, target builds: {string.Join(", ", targetBuilds)}");

                    waitHandle.WaitOne();
                }
                catch (Exception exception)
                {
                    _logger.LogTrace(exception,
                        $"{nameof(RunAllTests)} an error occured, passing to error handler.");
                    
                    errorHandler(exception);
                }
            });
        }
        catch (Exception ex)
        {
            errorHandler(ex);
        }

        return Task.CompletedTask;
    }

    public Task RunSelectedTests(IEnumerable<TestCase> testCases, Func<TestRunChangedEventArgs, Task> updateCallback,
        Func<TestRunCompleteEventArgs, TestRunChangedEventArgs, Task> finishedCallback,
        Action<Exception> errorHandler)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(updateCallback);
            ArgumentNullException.ThrowIfNull(finishedCallback);
            CancelCurrentOperation(nameof(RunSelectedTests));
            Task.Run(() =>
            {
                try
                {
                    using var waitHandle = new AutoResetEvent(false);

                    var handler = new RunEventHandler(waitHandle,
                        (updateEventArgs) =>
                            RunTestsUpdatedCallback(updateEventArgs, updateCallback, errorHandler),
                        (completeArgs, updateArgs) =>
                            RunTestsFinishedCallback(completeArgs, updateArgs, finishedCallback, errorHandler));

                    var testCasesList = string.Join(", ", testCases.Select(t => t.Source));
                    
                    _logger.LogTrace(
                        $"{nameof(RunSelectedTests)}: execution started, target tests: {testCasesList}");
                    
                    _defaultWrapper.RunTests(
                        testCases,
                        DEFAULT_RUN_SETTINGS, handler);
                    
                    _logger.LogTrace(
                        $"{nameof(RunSelectedTests)}: execution finished, target builds: {testCasesList}");

                    waitHandle.WaitOne();
                }
                catch (Exception exception)
                {
                    _logger.LogTrace(exception, $"{nameof(RunSelectedTests)} an error occured, passing to error handler.");
                    
                    errorHandler(exception);
                }
            });
        }
        catch (Exception exception)
        {
            _logger.LogTrace(exception, $"{nameof(RunSelectedTests)} an error occured, passing to error handler.");
            
            errorHandler(exception);
        }

        return Task.CompletedTask;
    }

    private void CancelCurrentOperation(string newOperationName = null)
    {
        lock (_currentOperationNameLock)
        {
            switch (_currentOperationName)
            {
                case nameof(RunAllTests):
                case nameof(RunSelectedTests):
                    InterruptCurrentOperation(_defaultWrapper.CancelTestRun, newOperationName);
                    break;
                case nameof(DiscoverTestCases):
                    InterruptCurrentOperation(_defaultWrapper.CancelDiscovery, newOperationName);
                    break;
                default:
                    _currentOperationName = newOperationName;

                    string newOperationMessage =
                        newOperationName is null ? "" : $", new operation name: {newOperationName}";
                    
                    _logger.LogTrace(
                        $"{nameof(CancelCurrentOperation)}: cancellation completed{newOperationMessage}");
                    break;
            }
        }
    }

    private void InterruptCurrentOperation(Action interruptor, string newOperationName = null)
    {
        ArgumentNullException.ThrowIfNull(interruptor);
        _logger.LogTrace($"{nameof(CancelCurrentOperation)}: execution started");
        lock (_currentOperationNameLock)
        {
            _logger.LogTrace(
                $"{nameof(CancelCurrentOperation)}: current operation :{_currentOperationName}, trying to cancel.");

            interruptor();

            _logger.LogTrace(
                $"{nameof(CancelCurrentOperation)}: {_currentOperationName} cancel request sent, waiting for cancellation to complete.");

            if (!_currentOperationFinishedEvent.WaitOne(TimeSpan.FromSeconds(15)))
            {
                _logger.LogTrace(
                    $"{nameof(CancelCurrentOperation)}: no interruption happened, manually hard reset.");
                _defaultWrapper.EndSession();
                _defaultWrapper.StartSession();
                _currentOperationFinishedEvent.Set();
                _logger.LogTrace(
                    $"{nameof(CancelCurrentOperation)}: reset completed.");
            }
            
            _currentOperationName = newOperationName;

            string newOperationMessage = newOperationName is null ? "" : $", new operation name: {newOperationName}";
            _logger.LogTrace(
                $"{nameof(CancelCurrentOperation)}: {_currentOperationName} cancellation completed{newOperationMessage}");
        }
    }

    #region Callbacks

    private async Task RunTestsUpdatedCallback(TestRunChangedEventArgs updateArgs,
        Func<TestRunChangedEventArgs, Task> innerCallback, Action<Exception> errorHandler)
    {
        try
        {
            await innerCallback(updateArgs);
        }
        catch (Exception ex)
        {
            errorHandler(ex);
        }
    }

    private async Task RunTestsFinishedCallback(TestRunCompleteEventArgs finishedArgs,
        TestRunChangedEventArgs updateArgs,
        Func<TestRunCompleteEventArgs, TestRunChangedEventArgs, Task> innerCallback, Action<Exception> errorHandler)
    {
        try
        {
            await innerCallback(finishedArgs, updateArgs);
        }
        catch (Exception ex)
        {
            errorHandler(ex);
        }
        finally
        {
            _currentOperationFinishedEvent.Set();
        }
    }

    private async Task DiscoverTestsUpdatedCallback(IEnumerable<TestCase> testCases,
        Func<IEnumerable<TestCase>, Task> innerCallback, Action<Exception> errorHandler)
    {
        try
        {
            _logger.LogTrace($"{nameof(DiscoverTestsUpdatedCallback)}: callback execution started.");
            await innerCallback(testCases);
        }
        catch (Exception exception)
        {
            _logger.LogTrace(exception,
                $"{nameof(DiscoverTestsUpdatedCallback)} an error occured, passing to error handler.");
            errorHandler(exception);
        }
    }

    private async Task DiscoverTestsFinishedCallback(IEnumerable<TestCase> testCases,
        Func<IEnumerable<TestCase>, Task> innerCallback, Action<Exception> errorHandler)
    {
        try
        {
            _logger.LogTrace($"{nameof(DiscoverTestsFinishedCallback)}: callback execution started.");
            await innerCallback(testCases);
        }
        catch (Exception exception)
        {
            _logger.LogTrace(exception,
                $"{nameof(DiscoverTestsFinishedCallback)} an error occured, passing to error handler.");
            errorHandler(exception);
        }
        finally
        {
            _currentOperationFinishedEvent.Set();
        }
    }

    #endregion

    public void Dispose()
    {
        _defaultWrapper.EndSession();
        _currentOperationFinishedEvent.Dispose();
    }
}