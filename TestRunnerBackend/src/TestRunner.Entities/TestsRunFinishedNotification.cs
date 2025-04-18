namespace TestRunner.Entities;

public class TestsRunFinishedNotification : NotificationBase
{
    public IEnumerable<TestResult> TestResults { get; set; }
    public double TotalElapsedMilliseconds { get; set; }
}