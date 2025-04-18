namespace TestRunner.Entities;

public enum TestOutcome
{
    None = 0,
    Passed = 1,
    Failed = 2,
    Skipped = 3,
    NotFound = 4
}

public class TestResult
{
    public required string DisplayName { get; set; }
    public required int DurationMs { get; set; }

    public required string TestCaseId { get; set; }

    public required TestOutcome Outcome { get; set; }

    public required string ErrorMessage { get; set; }

    public required string ErrorStackTrace { get; set; }
}