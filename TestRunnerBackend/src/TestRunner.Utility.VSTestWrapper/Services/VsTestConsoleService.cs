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

    private const string DEFAULT_RUN_SETTINGS = "<RunSettings><RunConfiguration></RunConfiguration></RunSettings>";

    public VsTestConsoleService(IVsTestConsoleWrapper wrapper)
    {
        _defaultWrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
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

                    _defaultWrapper.DiscoverTests(targetBuilds, DEFAULT_RUN_SETTINGS, discoveryEventHandler);

                    waitHandle.WaitOne();
                }
                catch (Exception exception)
                {
                    errorHandler(exception);
                }
            });
        }
        catch (Exception exception)
        {
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

                    _defaultWrapper.RunTests(
                        targetBuilds,
                        DEFAULT_RUN_SETTINGS, handler);

                    waitHandle.WaitOne();
                }
                catch (Exception ex)
                {
                    errorHandler(ex);
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

                    _defaultWrapper.RunTests(
                        testCases,
                        DEFAULT_RUN_SETTINGS, handler);

                    waitHandle.WaitOne();
                }
                catch (Exception ex)
                {
                    errorHandler(ex);
                }
            });
        }
        catch (Exception ex)
        {
            errorHandler(ex);
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
                    _currentOperationName = newOperationName;
                    _defaultWrapper.CancelTestRun();
                    _currentOperationFinishedEvent.WaitOne();
                    break;
                case nameof(RunSelectedTests):
                    _currentOperationName = newOperationName;
                    _defaultWrapper.CancelTestRun();
                    _currentOperationFinishedEvent.WaitOne();
                    break;
                case nameof(DiscoverTestCases):
                    _currentOperationName = newOperationName;
                    _defaultWrapper.CancelDiscovery();
                    _currentOperationFinishedEvent.WaitOne();
                    break;
                default:
                    _currentOperationName = newOperationName;
                    break;
            }
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
            await innerCallback(testCases);
        }
        catch (Exception ex)
        {
            errorHandler(ex);
        }
    }

    private async Task DiscoverTestsFinishedCallback(IEnumerable<TestCase> testCases,
        Func<IEnumerable<TestCase>, Task> innerCallback, Action<Exception> errorHandler)
    {
        try
        {
            await innerCallback(testCases);
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

    #endregion

    public void Dispose()
    {
        _defaultWrapper.EndSession();
        _currentOperationFinishedEvent.Dispose();
    }
}