namespace TestRunner.Entities;

public class TestsDiscoveryFinishedNotification :  NotificationBase
{
    public IEnumerable<TestCase> TestCases { get; set; }
}