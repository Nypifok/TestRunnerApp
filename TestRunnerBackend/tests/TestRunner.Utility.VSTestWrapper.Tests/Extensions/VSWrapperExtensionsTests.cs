using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer.Interfaces;
using NUnit.Framework;
using TestRunner.Utility.VSTestWrapper.Extensions;
using TestRunner.Utility.VSTestWrapper.Managers;
using TestRunner.Utility.VSTestWrapper.Services;
using TestRunnerUtility.Contract;

namespace TestRunner.Utility.VSTestWrapper.Tests.Extensions;

[TestFixture]
public class VSWrapperExtensionsTests
{
    private ServiceCollection _services;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();
    }

    [Test]
    public void AddVSTestWrapper_RegistersRequiredServices()
    {
        _services.AddVSTestWrapper();
        var serviceProvider = _services.BuildServiceProvider();
        
        Assert.Multiple(() =>
        {
            var manager = serviceProvider.GetService<IVSTestManager>();
            Assert.That(manager, Is.Not.Null);
            Assert.That(manager, Is.TypeOf<VSTestManager>());

            var consoleService = serviceProvider.GetService<VsTestConsoleService>();
            Assert.That(consoleService, Is.Not.Null);

            var wrapper = serviceProvider.GetService<IVsTestConsoleWrapper>();
            Assert.That(wrapper, Is.Not.Null);
        });
    }

    [Test]
    public void AddVSTestWrapper_RegistersAutoMapper()
    {
        _services.AddVSTestWrapper();
        var serviceProvider = _services.BuildServiceProvider();
        var mapper = serviceProvider.GetService<IMapper>();
        Assert.That(mapper, Is.Not.Null);
    }

    [Test]
    public void AddVSTestWrapper_WhenCalledMultipleTimes_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            _services.AddVSTestWrapper();
            _services.AddVSTestWrapper();
        });
    }

    [Test]
    public void AddVSTestWrapper_WhenServicesIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => VSWrapperExtensions.AddVSTestWrapper(null));
    }

    [Test]
    public void AddVSTestWrapper_RegistersServicesWithCorrectLifetime()
    {
        _services.AddVSTestWrapper();
        var serviceProvider = _services.BuildServiceProvider();
        
        Assert.Multiple(() =>
        {
            var manager1 = serviceProvider.GetService<IVSTestManager>();
            var manager2 = serviceProvider.GetService<IVSTestManager>();
            Assert.That(manager1, Is.SameAs(manager2));

            var consoleService1 = serviceProvider.GetService<VsTestConsoleService>();
            var consoleService2 = serviceProvider.GetService<VsTestConsoleService>();
            Assert.That(consoleService1, Is.SameAs(consoleService2));

            var wrapper1 = serviceProvider.GetService<IVsTestConsoleWrapper>();
            var wrapper2 = serviceProvider.GetService<IVsTestConsoleWrapper>();
            Assert.That(wrapper1, Is.SameAs(wrapper2));
        });
    }

    [Test]
    public void AddVSTestWrapper_RegistersAutoMapperWithCorrectLifetime()
    {
        _services.AddVSTestWrapper();
        var serviceProvider = _services.BuildServiceProvider();
        
        var mapper1 = serviceProvider.GetService<IMapper>();
        var mapper2 = serviceProvider.GetService<IMapper>();
        Assert.That(mapper1, Is.Not.SameAs(mapper2));
    }
} 