using Microsoft.TestPlatform.VsTestConsole.TranslationLayer.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using NUnit.Framework;
using TestRunner.Utility.VSTestWrapper.Managers.EventHandlers;
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
        _mockWrapper = new Mock<IVsTestConsoleWrapper>(MockBehavior.Loose);
        _service = new VsTestConsoleService(_mockWrapper.Object);
    }

    [Test]
    public void Constructor_NullWrapper_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new VsTestConsoleService(null));
    }

    [Test]
    public async Task Discover_EmptyCallbacks_UsesErrorHandler()
    {
        var targetBuilds = Array.Empty<string>();
        var errorHandlerMock = new Mock<Action<Exception>>();
        
        await _service.DiscoverTestCases(targetBuilds, null, null, errorHandlerMock.Object);
        
        errorHandlerMock.Verify(error => error.Invoke(It.IsAny<Exception>()), Times.Once);
    }

    [Test]
    public async Task RunAll_EmptyCallbacks_UsesErrorHandler()
    {
        var targetBuilds = Array.Empty<string>();
        var errorHandlerMock = new Mock<Action<Exception>>();
        
        await _service.RunAllTests(targetBuilds, null, null, errorHandlerMock.Object);
        
        errorHandlerMock.Verify(error => error.Invoke(It.IsAny<Exception>()), Times.Once);
    }

    [Test]
    public async Task RunSelected_EmptyCallbacks_UsesErrorHandler()
    {
        var testCases = Array.Empty<TestCase>();
        var errorHandlerMock = new Mock<Action<Exception>>();
        
        await _service.RunSelectedTests(testCases, null, null, errorHandlerMock.Object);
        
        errorHandlerMock.Verify(error => error.Invoke(It.IsAny<Exception>()), Times.Once);
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