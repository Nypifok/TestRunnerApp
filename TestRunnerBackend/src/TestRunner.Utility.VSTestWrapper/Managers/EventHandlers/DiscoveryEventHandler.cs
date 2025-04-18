using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using TestRunner.Entities;
using TestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace TestRunner.Utility.VSTestWrapper.Managers.EventHandlers;

public class DiscoveryEventHandler : ITestDiscoveryEventsHandler
{
    private readonly AutoResetEvent _waitHandle;
    private readonly Func<IEnumerable<TestCase>, Task> _updateCallback;
    private readonly Func<IEnumerable<TestCase>, Task> _finishedCallback;

    public DiscoveryEventHandler(AutoResetEvent waitHandle, Func<IEnumerable<TestCase>, Task> updateCallback,
        Func<IEnumerable<TestCase>, Task> finishedCallback)
    {
        this._waitHandle = waitHandle;
        _updateCallback = updateCallback;
        _finishedCallback = finishedCallback;
    }

    public void HandleDiscoveredTests(IEnumerable<TestCase>? discoveredTestCases)
    {
        discoveredTestCases = discoveredTestCases ?? [];
        _updateCallback(discoveredTestCases).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public void HandleDiscoveryComplete(long totalTests, IEnumerable<TestCase>? lastChunk, bool isAborted)
    {
        lastChunk = lastChunk ?? [];
        _finishedCallback(lastChunk).ConfigureAwait(false).GetAwaiter().GetResult();
        _waitHandle.Set();
    }

    public void HandleRawMessage(string rawMessage)
    {
    }

    public void HandleLogMessage(TestMessageLevel level, string? message)
    {
    }
}