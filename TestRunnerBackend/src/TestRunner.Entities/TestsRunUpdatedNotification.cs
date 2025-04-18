namespace TestRunner.Entities;

public class TestsRunUpdatedNotification : NotificationBase
{
    public IEnumerable<TestResult> TestResults { get; set; }
    public IEnumerable<TestCase> ActiveTestCases { get; set; }
}