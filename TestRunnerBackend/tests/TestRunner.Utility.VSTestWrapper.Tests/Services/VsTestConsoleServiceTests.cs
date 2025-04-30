using Microsoft.Extensions.Logging;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using NUnit.Framework;
using TestRunner.Utility.VSTestWrapper.Services;

namespace TestRunner.Utility.VSTestWrapper.Tests.Services;

[TestFixture]
public class VsTestConsoleServiceTests
{
    private Mock<IVsTestConsoleWrapper> _mockWrapper;
    private VsTestConsoleService _service;

    [SetUp]
    public void Setup()
    {
        _mockWrapper = new Mock<IVsTestConsoleWrapper>();
        _service = new VsTestConsoleService(_mockWrapper.Object, new Mock<ILogger<VsTestConsoleService>>().Object);
    }

    [Test]
    public void Constructor_WhenWrapperIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new VsTestConsoleService(null, null));
    }

    [Test]
    public async Task DiscoverTestCases_WhenTargetBuildsIsEmpty_DoesNotThrow()
    {
        var targetBuilds = Array.Empty<string>();
        var updateCallback = new Func<IEnumerable<TestCase>, Task>(_ => Task.CompletedTask);
        var finishedCallback = new Func<IEnumerable<TestCase>, Task>(_ => Task.CompletedTask);
        var errorHandler = new Action<Exception>(_ => { });

        await _service.DiscoverTestCases(targetBuilds, updateCallback, finishedCallback, errorHandler);
        Assert.Pass();
    }


    [Test]
    public async Task RunAllTests_WhenTargetBuildsIsEmpty_DoesNotThrow()
    {
        var targetBuilds = Array.Empty<string>();
        var updateCallback = new Func<TestRunChangedEventArgs, Task>(_ => Task.CompletedTask);
        var finishedCallback =
            new Func<TestRunCompleteEventArgs, TestRunChangedEventArgs, Task>((_, _) => Task.CompletedTask);
        var errorHandler = new Action<Exception>(_ => { });

        await _service.RunAllTests(targetBuilds, updateCallback, finishedCallback, errorHandler);
        Assert.Pass();
    }

    [Test]
    public async Task RunSelectedTests_WhenTestCasesIsEmpty_DoesNotThrow()
    {
        var testCases = Array.Empty<TestCase>();
        var updateCallback = new Func<TestRunChangedEventArgs, Task>(_ => Task.CompletedTask);
        var finishedCallback =
            new Func<TestRunCompleteEventArgs, TestRunChangedEventArgs, Task>((_, _) => Task.CompletedTask);
        var errorHandler = new Action<Exception>(_ => { });

        await _service.RunSelectedTests(testCases, updateCallback, finishedCallback, errorHandler);
        Assert.Pass();
    }


    [Test]
    public void Dispose_CallsEndSession()
    {
        _service.Dispose();
        _mockWrapper.Verify(w => w.EndSession(), Times.Once);
    }

    [TearDown]
    public void TearDown()
    {
        _service?.Dispose();
    }
}