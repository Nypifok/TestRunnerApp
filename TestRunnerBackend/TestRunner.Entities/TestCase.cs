namespace TestRunner.Entities;

public class TestCase
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public required string FullyQualifiedName { get; set; }
    public required string ExecutorUri { get; set; }
    public required string Source { get; set; }
}