namespace TestRunner.Utility.VSTestWrapper.Entities;

internal class TestRunSetup
{
    public string [] TargetBuilds { get; set; }
    public string ExecutionCommand
    {
        get => executionCommand;
    }

    private string executionCommand;

    public TestRunSetup()
    {
        this.executionCommand = "";
        TargetBuilds = [];
    }
}