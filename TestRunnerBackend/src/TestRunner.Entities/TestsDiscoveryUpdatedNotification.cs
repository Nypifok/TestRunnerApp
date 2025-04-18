namespace TestRunner.Entities;

public class TestsDiscoveryUpdatedNotification : NotificationBase
{
    public IEnumerable<TestCase> TestCases { get; set; }
}