using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using NUnit.Framework;
using TestRunner.Entities;
using TestRunner.Utility.VSTestWrapper.Managers;
using TestRunner.Utility.VSTestWrapper.Services;
using TestRunnerUtility.Contract;
using TestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace TestRunner.Utility.VSTestWrapper.Tests.Managers;

[TestFixture]
public class VSTestManagerTests
{
    private Mock<VsTestConsoleService> _mockConsoleService;
    private Mock<IMapper> _mockMapper;
    private Mock<ILogger<VSTestManager>> _mockLogger;
    private VSTestManager _manager;

    [SetUp]
    public void Setup()
    {
        _mockConsoleService = new Mock<VsTestConsoleService>(Mock.Of<IVsTestConsoleWrapper>(),
            new Mock<ILogger<VsTestConsoleService>>().Object);
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<VSTestManager>>();
        _manager = new VSTestManager(_mockConsoleService.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Test]
    public void Constructor_WhenConsoleServiceIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new VSTestManager(null, _mockMapper.Object, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WhenMapperIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new VSTestManager(_mockConsoleService.Object, null, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new VSTestManager(_mockConsoleService.Object, _mockMapper.Object, null));
    }

    [Test]
    public async Task DiscoverTestsAsync_WhenTargetBuildsIsEmpty_ReturnsUnknownError()
    {
        var targetBuilds = Array.Empty<string>();
        var callback = new Func<NotificationBase, Action<Exception>, Task>((_, _) => Task.CompletedTask);
        var errorHandler = new Action<Exception>(_ => { });

        var result = await _manager.DiscoverTestsAsync(targetBuilds, callback, errorHandler);

        Assert.That(result, Is.EqualTo(BuildAccessStatus.UnknownError));
    }


    [Test]
    public async Task RunAllTests_WhenTargetBuildsIsEmpty_DoesNotReturnsOk()
    {
        var targetBuilds = Array.Empty<string>();
        var callback = new Func<NotificationBase, Action<Exception>, Task>((_, _) => Task.CompletedTask);
        var errorHandler = new Action<Exception>(_ => { });

        var result = await _manager.RunAllTests(targetBuilds, callback, errorHandler);

        Assert.That(result, Is.Not.EqualTo(BuildAccessStatus.Ok));
    }

    [Test]
    public async Task RunSelectedTests_WhenTestCasesIsEmpty_DoesNotThrow()
    {
        var testCases = Array.Empty<Entities.TestCase>();
        var callback = new Func<NotificationBase, Action<Exception>, Task>((_, _) => Task.CompletedTask);
        var errorHandler = new Action<Exception>(_ => { });

        await _manager.RunSelectedTests(testCases, callback, errorHandler);
        Assert.Pass();
    }


    [Test]
    public async Task ValidateTargetBuilds_WhenFileDoesNotExist_ReturnsPathDoesNotExist()
    {
        var targetBuilds = new[] { "nonexistent.dll" };

        var result = await _manager.RunAllTests(targetBuilds,
            (_, _) => Task.CompletedTask,
            _ => { });

        Assert.That(result, Is.EqualTo(BuildAccessStatus.PathDoesNotExist));
    }

    [Test]
    public async Task RunAllTests_WhenTargetBuildsContainsInvalidFile_ReturnsPathDoesNotExist()
    {
        var targetBuilds = new[] { "invalid.dll" };
        var callback = new Func<NotificationBase, Action<Exception>, Task>((_, _) => Task.CompletedTask);
        var errorHandler = new Action<Exception>(_ => { });

        var result = await _manager.RunAllTests(targetBuilds, callback, errorHandler);

        Assert.That(result, Is.EqualTo(BuildAccessStatus.PathDoesNotExist));
    }

    [TearDown]
    public void TearDown()
    {
        _mockConsoleService.Object?.Dispose();
    }
}