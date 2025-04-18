using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace TestRunner.Utility.VSTestWrapper.Managers.EventHandlers;

public class RunEventHandler : ITestRunEventsHandler
{
    private readonly AutoResetEvent _waitHandle;
    private readonly Func<TestRunCompleteEventArgs, TestRunChangedEventArgs, Task> _finishedCallback;
    private readonly Func<TestRunChangedEventArgs, Task> _statsUpdateCallback;


    public RunEventHandler(AutoResetEvent waitHandle, Func<TestRunChangedEventArgs, Task> statsUpdateCallback,
        Func<TestRunCompleteEventArgs, TestRunChangedEventArgs, Task> finishedCallback)
    {
        _statsUpdateCallback = statsUpdateCallback ?? throw new ArgumentNullException(nameof(statsUpdateCallback));
        _finishedCallback = finishedCallback ?? throw new ArgumentNullException(nameof(finishedCallback));
        _waitHandle = waitHandle;
    }

    public void HandleLogMessage(TestMessageLevel level, string message)
    {
    }

    public void HandleTestRunComplete(
        TestRunCompleteEventArgs testRunCompleteArgs,
        TestRunChangedEventArgs lastChunkArgs,
        ICollection<AttachmentSet> runContextAttachments,
        ICollection<string> executorUris)
    {
        _finishedCallback(testRunCompleteArgs, lastChunkArgs).ConfigureAwait(false).GetAwaiter().GetResult();;
        _waitHandle.Set();
    }

    public void HandleTestRunStatsChange(TestRunChangedEventArgs? testRunChangedArgs)
    {
        if (testRunChangedArgs != null)
            _statsUpdateCallback(testRunChangedArgs).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public void HandleRawMessage(string rawMessage)
    {
        // No op
    }

    public int LaunchProcessWithDebuggerAttached(TestProcessStartInfo testProcessStartInfo)
    {
        // No op
        return -1;
    }

    public bool AttachDebuggerToProcess(int pid)
    {
        // No op
        return false;
    }
}