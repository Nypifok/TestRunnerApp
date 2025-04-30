using AutoMapper;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using TestRunner.Entities;
using TestRunner.Utility.VSTestWrapper.Extensions;
using TestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using TestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace TestRunner.Utility.VSTestWrapper.Tests.Extensions;

[TestFixture]
public class DefaultMappingProfileTests
{
    private IMapper _mapper;

    [SetUp]
    public void Setup()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<DefaultMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Test]
    public void Configuration_IsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<DefaultMappingProfile>());
        config.AssertConfigurationIsValid();
    }


    [Test]
    public void Map_TestResultToTestRunnerTestResult_MapsCorrectly()
    {
        var testCase = new TestCase
        {
            Id = new Guid("12345678-1234-5678-1234-567812345678"),
            DisplayName = "Test Case"
        };

        var source = new TestResult(testCase)
        {
            Duration = TimeSpan.FromMilliseconds(123),
            ErrorMessage = "Test Error",
            ErrorStackTrace = "Test Stack Trace",
            DisplayName = "Test Result"
        };

        var result = _mapper.Map<TestRunner.Entities.TestResult>(source);

        Assert.Multiple(() =>
        {
            Assert.That(result.DurationMs, Is.EqualTo(source.Duration.Milliseconds));
            Assert.That(result.TestCaseId, Is.EqualTo(source.TestCase.Id.ToString()));
            Assert.That(result.ErrorMessage, Is.EqualTo(source.ErrorMessage));
            Assert.That(result.ErrorStackTrace, Is.EqualTo(source.ErrorStackTrace));
            Assert.That(result.DisplayName, Is.EqualTo(source.DisplayName));
        });
    }
}