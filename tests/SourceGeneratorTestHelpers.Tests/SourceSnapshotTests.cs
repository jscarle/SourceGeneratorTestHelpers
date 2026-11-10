using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using SourceGeneratorTestHelpers.Tests.Fixtures;
using SourceGeneratorTestHelpers.XUnit;
using Xunit;
using Xunit.Sdk;
using Assert = Xunit.Assert;
using AssertFailedException = Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException;
using AssertionException = NUnit.Framework.AssertionException;
using MSTestSnapshots = SourceGeneratorTestHelpers.MSTest.GeneratorDriverTestBase;
using NUnitSnapshots = SourceGeneratorTestHelpers.NUnit.GeneratorDriverRunResultExtensions;
using TestContext = Xunit.TestContext;

namespace SourceGeneratorTestHelpers.Tests;

// Tests supply caller information explicitly to keep snapshots inside temporary directories.
// ReSharper disable ExplicitCallerInfoArgument
public sealed class SourceSnapshotTests : IAsyncLifetime
{
    private readonly string _snapshotDirectory = Path.Combine(Path.GetTempPath(), "SourceGeneratorTestHelpers.Tests", Guid.NewGuid().ToString("N"));

    private static string ReceivedSuffix => $".DotNet{Environment.Version.Major}_{Environment.Version.Minor}.received.txt";

    private string SourceFile => Path.Combine(_snapshotDirectory, "SourceGeneratorTests.cs");

    public async ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(_snapshotDirectory);
        await File.WriteAllTextAsync(SourceFile, "// The calling test source file.", cancellationToken: TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        Directory.Delete(_snapshotDirectory, recursive: true);
        return ValueTask.CompletedTask;
    }

    [Theory]
    [InlineData("XUnit", false)]
    [InlineData("XUnit", true)]
    [InlineData("NUnit", false)]
    [InlineData("NUnit", true)]
    [InlineData("MSTest", false)]
    [InlineData("MSTest", true)]
    public async Task MatchingApprovedSnapshot_VerifyAsync_CompletesAndDeletesStaleReceivedFile(string framework, bool includeDiagnostics)
    {
        var result = SnapshotGenerator.Run("// generated source\n");
        var resultWithDiagnostics = (ImmutableArray<Diagnostic>.Empty, result);
        const string snapshotName = "ExistingSnapshot";
        var receivedSuffix = framework == "NUnit" ? ".received.txt" : ReceivedSuffix;
        var snapshotPath = Path.Combine(_snapshotDirectory, snapshotName);
        await File.WriteAllTextAsync(snapshotPath + ".verified.txt", "// generated source\n", cancellationToken: TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(snapshotPath + receivedSuffix, "stale output", cancellationToken: TestContext.Current.CancellationToken);
        var mstest = new SnapshotTestClass();

        var verification = (framework, includeDiagnostics) switch
        {
            ("XUnit", false) => result.VerifyAsync("TestId.g.cs", "ExistingSnapshot", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken),
            ("XUnit", true) => resultWithDiagnostics.VerifyAsync("TestId.g.cs", "ExistingSnapshot", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken),
            ("NUnit", false) => NUnitSnapshots.VerifyAsync(result, "TestId.g.cs", "ExistingSnapshot", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken),
            ("NUnit", true) => NUnitSnapshots.VerifyAsync(resultWithDiagnostics, "TestId.g.cs", "ExistingSnapshot", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken),
            ("MSTest", false) => mstest.VerifyAsync(result, "TestId.g.cs", "ExistingSnapshot", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken),
            ("MSTest", true) => mstest.VerifyAsync(resultWithDiagnostics, "TestId.g.cs", "ExistingSnapshot", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(framework))
        };
        await verification;

        Assert.False(File.Exists(snapshotPath + receivedSuffix));
        Assert.Equal("// generated source\n", await File.ReadAllTextAsync(snapshotPath + ".verified.txt", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("XUnit", typeof(XunitException))]
    [InlineData("NUnit", typeof(AssertionException))]
    [InlineData("MSTest", typeof(AssertFailedException))]
    public async Task MissingApprovedSnapshot_VerifyAsync_WritesReceivedAndThrowsFrameworkAssertion(string framework, Type exceptionType)
    {
        var result = SnapshotGenerator.Run("// generated source\n");
        var snapshotPath = Path.Combine(_snapshotDirectory, "MissingSnapshot");
        var receivedSuffix = framework == "NUnit" ? ".received.txt" : ReceivedSuffix;
        var mstest = new SnapshotTestClass();

        var failure = await Record.ExceptionAsync(() => framework switch
        {
            "XUnit" => result.VerifyAsync("TestId.g.cs", "MissingSnapshot", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken),
            "NUnit" => NUnitSnapshots.VerifyAsync(result, "TestId.g.cs", "MissingSnapshot", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken),
            "MSTest" => mstest.VerifyAsync(result, "TestId.g.cs", "MissingSnapshot", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(framework))
        });

        Assert.IsType(exceptionType, failure);
        Assert.Contains("does not exist", failure.Message, StringComparison.Ordinal);
        Assert.Contains(snapshotPath + ".verified.txt", failure.Message, StringComparison.Ordinal);
        Assert.Contains(snapshotPath + receivedSuffix, failure.Message, StringComparison.Ordinal);
        Assert.Equal("// generated source\n", await File.ReadAllTextAsync(snapshotPath + receivedSuffix, cancellationToken: TestContext.Current.CancellationToken));
        Assert.False(File.Exists(snapshotPath + ".verified.txt"));
    }

    [Fact]
    public async Task ChangedApprovedSnapshot_VerifyAsync_WritesNormalizedUTF8AndPreservesApprovedFile()
    {
        var result = SnapshotGenerator.Run("// actual\r\n// café\r");
        var snapshotPath = Path.Combine(_snapshotDirectory, "ChangedSnapshot");
        var expectedBytes = Encoding.UTF8.GetBytes("// expected\r\n");
        await File.WriteAllBytesAsync(snapshotPath + ".verified.txt", expectedBytes, cancellationToken: TestContext.Current.CancellationToken);

        var failure = await Assert.ThrowsAsync<XunitException>(() => result.VerifyAsync("TestId.g.cs", "ChangedSnapshot", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("- // expected", failure.Message, StringComparison.Ordinal);
        Assert.Contains("+ // actual", failure.Message, StringComparison.Ordinal);
        Assert.Equal(Encoding.UTF8.GetBytes("// actual\n// café\n"), await File.ReadAllBytesAsync(snapshotPath + ReceivedSuffix, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(expectedBytes, await File.ReadAllBytesAsync(snapshotPath + ".verified.txt", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    [InlineData("\u0085")]
    [InlineData("\u2028")]
    [InlineData("\u2029")]
    public async Task OnlyLineEndingsDiffer_VerifyAsync_MatchesApprovedSnapshot(string lineEnding)
    {
        var result = SnapshotGenerator.Run($"// first{lineEnding}// second{lineEnding}");
        var snapshotPath = Path.Combine(_snapshotDirectory, "LineEndings");
        await File.WriteAllTextAsync(snapshotPath + ".verified.txt", "// first\r\n// second\r\n", cancellationToken: TestContext.Current.CancellationToken);

        await result.VerifyAsync("TestId.g.cs", "LineEndings", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(File.Exists(snapshotPath + ReceivedSuffix));
    }

    [Theory]
    [InlineData("// source\n", "// source")]
    [InlineData("// source", "// source\n")]
    [InlineData("// source\n", "// source \n")]
    [InlineData("// source\n", "// SOURCE\n")]
    public async Task WhitespaceOrCaseDiffers_VerifyAsync_FailsWithoutTrimmingText(string expectedSource, string actualSource)
    {
        var result = SnapshotGenerator.Run(actualSource);
        var snapshotPath = Path.Combine(_snapshotDirectory, "ExactText");
        await File.WriteAllTextAsync(snapshotPath + ".verified.txt", expectedSource, cancellationToken: TestContext.Current.CancellationToken);

        var failure = await Assert.ThrowsAsync<XunitException>(() => result.VerifyAsync("TestId.g.cs", "ExactText", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("does not match", failure.Message, StringComparison.Ordinal);
        Assert.Equal(actualSource, await File.ReadAllTextAsync(snapshotPath + ReceivedSuffix, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(expectedSource, await File.ReadAllTextAsync(snapshotPath + ".verified.txt", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EmptyGeneratedFileWithoutApprovedSnapshot_VerifyAsync_WritesEmptyReceivedFileAndFails()
    {
        var result = SnapshotGenerator.Run("");
        var snapshotPath = Path.Combine(_snapshotDirectory, "EmptySnapshot");

        await Assert.ThrowsAsync<XunitException>(() => result.VerifyAsync("TestId.g.cs", "EmptySnapshot", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Empty(await File.ReadAllBytesAsync(snapshotPath + ReceivedSuffix, cancellationToken: TestContext.Current.CancellationToken));
        Assert.False(File.Exists(snapshotPath + ".verified.txt"));

        await File.WriteAllTextAsync(snapshotPath + ".verified.txt", "", cancellationToken: TestContext.Current.CancellationToken);
        await result.VerifyAsync("TestId.g.cs", "EmptySnapshot", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(File.Exists(snapshotPath + ReceivedSuffix));
    }

    [Fact]
    public async Task MissingGeneratedFileWithEmptyApprovedSnapshot_VerifyAsync_FailsWithoutChangingFiles()
    {
        var result = SnapshotGenerator.Run(null);
        var snapshotPath = Path.Combine(_snapshotDirectory, "MissingOutput");
        await File.WriteAllTextAsync(snapshotPath + ".verified.txt", "", cancellationToken: TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(snapshotPath + ReceivedSuffix, "previous output", cancellationToken: TestContext.Current.CancellationToken);

        var failure = await Assert.ThrowsAsync<XunitException>(() => result.VerifyAsync("TestId.g.cs", "MissingOutput", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("No generated source file", failure.Message, StringComparison.Ordinal);
        Assert.Contains("TestId.g.cs", failure.Message, StringComparison.Ordinal);
        Assert.Equal("previous output", await File.ReadAllTextAsync(snapshotPath + ReceivedSuffix, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Empty(await File.ReadAllBytesAsync(snapshotPath + ".verified.txt", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("../escape")]
    [InlineData("..\\escape")]
    [InlineData("invalid:name")]
    [InlineData("invalid*name")]
    [InlineData("invalid?name")]
    [InlineData("invalid|name")]
    [InlineData("invalid<name")]
    [InlineData("invalid>name")]
    [InlineData("invalid\"name")]
    [InlineData("invalid\nname")]
    public async Task InvalidExplicitSnapshotName_VerifyAsync_RejectsNameWithoutWritingFiles(string? snapshotName)
    {
        var result = SnapshotGenerator.Run("// output");

        await Assert.ThrowsAnyAsync<ArgumentException>(() => result.VerifyAsync("TestId.g.cs", snapshotName!, sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal([SourceFile], Directory.GetFiles(_snapshotDirectory));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task InvalidGeneratedFileSuffix_VerifyAsync_RejectsSuffixWithoutWritingFiles(string? filePathEndsWith)
    {
        var result = SnapshotGenerator.Run("// output");

        await Assert.ThrowsAnyAsync<ArgumentException>(() => result.VerifyAsync(filePathEndsWith!, "InvalidSuffix", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal([SourceFile], Directory.GetFiles(_snapshotDirectory));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("relative.cs")]
    public async Task InvalidCallerPath_VerifyAsync_RejectsPathWithoutWritingFiles(string sourceFile)
    {
        var result = SnapshotGenerator.Run("// output");

        await Assert.ThrowsAnyAsync<ArgumentException>(() => result.VerifyAsync("TestId.g.cs", "InvalidCaller", sourceFile: sourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal([SourceFile], Directory.GetFiles(_snapshotDirectory));
    }

    [Fact]
    public async Task MissingFrameworkContext_VerifyAsync_RequiresExplicitSnapshotName()
    {
        var result = SnapshotGenerator.Run("// output");
        var mstest = new SnapshotTestClass();

        await Assert.ThrowsAsync<InvalidOperationException>(() => NUnitSnapshots.VerifyAsync(result, "TestId.g.cs", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<InvalidOperationException>(() => mstest.VerifyAsync(result, "TestId.g.cs", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<AssertionException>(() => NUnitSnapshots.VerifyAsync(result, "TestId.g.cs", "ExplicitName", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.True(File.Exists(Path.Combine(_snapshotDirectory, "ExplicitName.received.txt")));
    }

    [Fact]
    public async Task NullGeneratorResult_VerifyAsync_ThrowsBeforeWritingFiles()
    {
        GeneratorDriverRunResult result = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => result.VerifyAsync("TestId.g.cs", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal([SourceFile], Directory.GetFiles(_snapshotDirectory));
    }

    [Fact]
    public async Task CancelledOperation_VerifyAsync_PropagatesCancellationWithoutChangingFiles()
    {
        var result = SnapshotGenerator.Run("// output");
        var snapshotPath = Path.Combine(_snapshotDirectory, "Cancelled");
        await File.WriteAllTextAsync(snapshotPath + ".verified.txt", "// approved", cancellationToken: TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(snapshotPath + ReceivedSuffix, "previous output", cancellationToken: TestContext.Current.CancellationToken);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => result.VerifyAsync("TestId.g.cs", "Cancelled", sourceFile: SourceFile, cancellationToken: cancellation.Token));

        Assert.Equal("// approved", await File.ReadAllTextAsync(snapshotPath + ".verified.txt", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("previous output", await File.ReadAllTextAsync(snapshotPath + ReceivedSuffix, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(DiagnosticSeverity.Error, true, false, false)]
    [InlineData(DiagnosticSeverity.Warning, false, true, false)]
    [InlineData(DiagnosticSeverity.Error, true, false, true)]
    [InlineData(DiagnosticSeverity.Warning, false, true, true)]
    public async Task EnabledDiagnosticFailure_VerifyAsync_FailsBeforeSnapshotFileAccess(DiagnosticSeverity severity, bool assertOnErrors, bool assertOnWarnings, bool generatorDiagnostic)
    {
        var result = SnapshotGenerator.Run("// output", generatorDiagnostic ? severity : null);
        var descriptor = new DiagnosticDescriptor("SNAP002", "Compilation test", "Snapshot compilation diagnostic", "Tests", severity, true);
        var diagnostic = Diagnostic.Create(descriptor, Location.None);
        var diagnostics = generatorDiagnostic ? ImmutableArray<Diagnostic>.Empty : ImmutableArray.Create(diagnostic);
        var resultWithDiagnostics = (diagnostics, result);
        var sourceFile = Path.Combine(_snapshotDirectory, "DirectoryDoesNotExist", "Tests.cs");

        var failure = await Assert.ThrowsAsync<XunitException>(() => resultWithDiagnostics.VerifyAsync("TestId.g.cs", "Diagnostics", assertOnErrors, assertOnWarnings, sourceFile: sourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains(generatorDiagnostic ? "source generator" : "compilation", failure.Message, StringComparison.Ordinal);
        Assert.Contains(generatorDiagnostic ? "SNAP001" : "SNAP002", failure.Message, StringComparison.Ordinal);
        Assert.Equal([SourceFile], Directory.GetFiles(_snapshotDirectory));
        Assert.False(Directory.Exists(Path.GetDirectoryName(sourceFile)));
    }

    [Theory]
    [InlineData(DiagnosticSeverity.Error)]
    [InlineData(DiagnosticSeverity.Warning)]
    public async Task DisabledDiagnosticAssertions_VerifyAsync_AllowsMatchingSnapshot(DiagnosticSeverity severity)
    {
        var result = SnapshotGenerator.Run("// output", severity);
        var resultWithDiagnostics = (ImmutableArray<Diagnostic>.Empty, result);
        var snapshotPath = Path.Combine(_snapshotDirectory, "DisabledDiagnostics");
        await File.WriteAllTextAsync(snapshotPath + ".verified.txt", "// output", cancellationToken: TestContext.Current.CancellationToken);

        await resultWithDiagnostics.VerifyAsync("TestId.g.cs", "DisabledDiagnostics", assertOnErrors: false, assertOnWarnings: false, sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(File.Exists(snapshotPath + ReceivedSuffix));
    }

    [Fact]
    public async Task InvalidArgumentAndCompilationError_VerifyAsync_ValidatesArgumentsFirst()
    {
        var result = SnapshotGenerator.Run("// output");
        var descriptor = new DiagnosticDescriptor("SNAP002", "Compilation test", "Compilation error", "Tests", DiagnosticSeverity.Error, true);
        var diagnostics = ImmutableArray.Create(Diagnostic.Create(descriptor, Location.None));
        var resultWithDiagnostics = (diagnostics, result);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => resultWithDiagnostics.VerifyAsync("TestId.g.cs", "../invalid", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal([SourceFile], Directory.GetFiles(_snapshotDirectory));
    }

    [Theory]
    [InlineData("ApprovedRead")]
    [InlineData("ReceivedWrite")]
    [InlineData("ReceivedCleanup")]
    public async Task SnapshotPathIsDirectory_VerifyAsync_PropagatesFileAccessFailure(string operation)
    {
        var result = SnapshotGenerator.Run("// output");
        var snapshotPath = Path.Combine(_snapshotDirectory, "FileAccess");
        if (operation == "ApprovedRead")
        {
            Directory.CreateDirectory(snapshotPath + ".verified.txt");
        }
        else
        {
            Directory.CreateDirectory(snapshotPath + ReceivedSuffix);
            if (operation == "ReceivedCleanup")
                await File.WriteAllTextAsync(snapshotPath + ".verified.txt", "// output", cancellationToken: TestContext.Current.CancellationToken);
        }

        var failure = await Record.ExceptionAsync(() => result.VerifyAsync("TestId.g.cs", "FileAccess", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.NotNull(failure);
        Assert.True(failure is IOException or UnauthorizedAccessException, failure.ToString());
        if (operation == "ApprovedRead")
            Assert.False(File.Exists(snapshotPath + ReceivedSuffix));
    }

    [Fact]
    public async Task ApprovedFileContainsInvalidUTF8_VerifyAsync_PropagatesDecodingFailure()
    {
        var result = SnapshotGenerator.Run("// output");
        var snapshotPath = Path.Combine(_snapshotDirectory, "InvalidEncoding");
        await File.WriteAllBytesAsync(snapshotPath + ".verified.txt", [0xFF], cancellationToken: TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<DecoderFallbackException>(() => result.VerifyAsync("TestId.g.cs", "InvalidEncoding", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken));

        Assert.False(File.Exists(snapshotPath + ReceivedSuffix));
    }

    [Fact]
    public async Task IndependentSnapshotDirectories_VerifyAsyncInParallel_PreservesBothReceivedFiles()
    {
        var firstDirectory = Path.Combine(_snapshotDirectory, "First");
        var secondDirectory = Path.Combine(_snapshotDirectory, "Second");
        Directory.CreateDirectory(firstDirectory);
        Directory.CreateDirectory(secondDirectory);
        var firstSourceFile = Path.Combine(firstDirectory, "Tests.cs");
        var secondSourceFile = Path.Combine(secondDirectory, "Tests.cs");
        var firstResult = SnapshotGenerator.Run("// first");
        var secondResult = SnapshotGenerator.Run("// second");

        var firstVerification = Record.ExceptionAsync(() => firstResult.VerifyAsync("TestId.g.cs", "SharedName", sourceFile: firstSourceFile, cancellationToken: TestContext.Current.CancellationToken));
        var secondVerification = Record.ExceptionAsync(() => secondResult.VerifyAsync("TestId.g.cs", "SharedName", sourceFile: secondSourceFile, cancellationToken: TestContext.Current.CancellationToken));
        var failures = await Task.WhenAll(firstVerification.AsTask(), secondVerification.AsTask());

        Assert.All(failures, failure => Assert.IsType<XunitException>(failure));
        Assert.Equal("// first", await File.ReadAllTextAsync(Path.Combine(firstDirectory, "SharedName" + ReceivedSuffix), cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("// second", await File.ReadAllTextAsync(Path.Combine(secondDirectory, "SharedName" + ReceivedSuffix), cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AutomaticNaming_VerifyAsync_UsesRunningTestClassAndMethod()
    {
        var result = SnapshotGenerator.Run("// output");
        var snapshotName = $"SourceSnapshotTests.{nameof(AutomaticNaming_VerifyAsync_UsesRunningTestClassAndMethod)}";
        var verifiedPath = Path.Combine(_snapshotDirectory, snapshotName + ".verified.txt");
        await File.WriteAllTextAsync(verifiedPath, "// output", cancellationToken: TestContext.Current.CancellationToken);

        await result.VerifyAsync("TestId.g.cs", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(File.Exists(Path.Combine(_snapshotDirectory, snapshotName + ReceivedSuffix)));
    }

    [Fact]
    public async Task PositionalSnapshotNames_VerifyAsync_BindsForEveryFramework()
    {
        GeneratorDriverRunResult result = null!;
        var mstest = new SnapshotTestClass();

        await Assert.ThrowsAsync<ArgumentNullException>(() => result.VerifyAsync("TestId.g.cs", "ExplicitSnapshot", cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(() => NUnitSnapshots.VerifyAsync(result, "TestId.g.cs", "ExplicitSnapshot", cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(() => mstest.VerifyAsync(result, "TestId.g.cs", "ExplicitSnapshot", cancellationToken: TestContext.Current.CancellationToken));
    }

    private sealed class SnapshotTestClass : MSTestSnapshots;
}
