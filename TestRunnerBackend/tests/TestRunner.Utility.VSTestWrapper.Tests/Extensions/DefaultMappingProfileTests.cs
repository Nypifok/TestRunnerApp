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
    public void Config_IsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<DefaultMappingProfile>());
        config.AssertConfigurationIsValid();
    }

    [Test]
    public void Map_TestCase_To_TestRunnerCase()
    {
        var source = new TestCase
        {
            Id = new Guid("12345678-1234-5678-1234-567812345678"),
            DisplayName = "Test Display Name",
            ExecutorUri = new Uri("executor://testexecutor"),
            Source = "TestSource.dll",
            CodeFilePath = "TestFile.cs",
            LineNumber = 42
        };

        var result = _mapper.Map<TestRunner.Entities.TestCase>(source);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(source.Id.ToString()) );
            Assert.That(result.DisplayName, Is.EqualTo(source.DisplayName) );
            Assert.That(result.ExecutorUri, Is.EqualTo(source.ExecutorUri.OriginalString) );
            Assert.That(result.Source, Is.EqualTo(source.Source) );
        });
    }

    [Test]
    public void Map_TestRunnerCase_To_TestCase()
    {
        var source = new TestRunner.Entities.TestCase
        {
            Id = "12345678-1234-5678-1234-567812345678",
            DisplayName = "Test Display Name",
            ExecutorUri = "executor://testexecutor/",
            Source = "TestSource.dll",
            FullyQualifiedName = "Testname",
        };

        var result = _mapper.Map<TestCase>(source);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id.ToString(), Is.EqualTo(source.Id) );
            Assert.That(result.DisplayName, Is.EqualTo(source.DisplayName) );
            Assert.That(result.ExecutorUri.ToString(), Is.EqualTo(source.ExecutorUri) );
            Assert.That(result.Source, Is.EqualTo(source.Source) );
        });
    }

    [Test]
    public void Map_TestResult_To_TestRunnerResult()
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
            Assert.That(result.DurationMs, Is.EqualTo(source.Duration.Milliseconds) );
            Assert.That(result.TestCaseId, Is.EqualTo(source.TestCase.Id.ToString()) );
            Assert.That(result.ErrorMessage, Is.EqualTo(source.ErrorMessage) );
            Assert.That(result.ErrorStackTrace, Is.EqualTo(source.ErrorStackTrace) );
            Assert.That(result.DisplayName, Is.EqualTo(source.DisplayName) );
        });
    }

    [Test]
    public void Map_TestResultWithNullValues_MapsCorrectly()
    {
        var testCase = new TestCase
        {
            Id = new Guid("12345678-1234-5678-1234-567812345678"),
            DisplayName = "Test Case"
        };

        var source = new TestResult(testCase)
        {
            Duration = TimeSpan.FromMilliseconds(0),
            ErrorMessage = null,
            ErrorStackTrace = null,
            DisplayName = null
        };

        var result = _mapper.Map<TestRunner.Entities.TestResult>(source);

        Assert.Multiple(() =>
        {
            Assert.That(result.DurationMs, Is.EqualTo(0));
            Assert.That(result.TestCaseId, Is.EqualTo(source.TestCase.Id.ToString()));
            Assert.That(result.ErrorMessage, Is.EqualTo(string.Empty));
            Assert.That(result.ErrorStackTrace, Is.EqualTo(string.Empty));
            Assert.That(result.DisplayName, Is.EqualTo("Name not found"));
        });
    }
} 