using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SourceGeneratorTestHelpers.Tests.Fixtures;
using SourceGeneratorTestHelpers.XUnit;
using Xunit;
using Assert = Xunit.Assert;
using TestContext = Xunit.TestContext;

namespace SourceGeneratorTestHelpers.Tests;

// The caller path deliberately differs from the test class name.
// ReSharper disable ExplicitCallerInfoArgument
public sealed class XunitSnapshotNamingTests : IAsyncLifetime
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "SourceGeneratorTestHelpers.Naming", Guid.NewGuid().ToString("N"));
    private string SourceFile => Path.Combine(_directory, "DifferentFileName.cs");

    public ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(_directory);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Directory.Delete(_directory, recursive: true);
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task ClassNameDiffersFromFileName_VerifyAsync_ReusesVerifySnapshot()
    {
        var result = SnapshotGenerator.Run("// output");
        var expectedPath = Path.Combine(_directory, $"XunitSnapshotNamingTests.{nameof(ClassNameDiffersFromFileName_VerifyAsync_ReusesVerifySnapshot)}.verified.txt");
        await File.WriteAllTextAsync(expectedPath, "// output", TestContext.Current.CancellationToken);

        await result.VerifyAsync("TestId.g.cs", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken);
        var diagnostics = (ImmutableArray<Diagnostic>.Empty, result);
        await diagnostics.VerifyAsync("TestId.g.cs", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([expectedPath], Directory.GetFiles(_directory));
    }

    [Theory]
    [InlineData("a/b:c?", 42, true)]
    [InlineData(null, -1, false)]
    public async Task ParameterValues_VerifyAsync_ReusesVerifySnapshot(string? text, int count, bool enabled)
    {
        var result = SnapshotGenerator.Run("// output");
        var suffix = text is null ? "_text=null_count=-1_enabled=False" : "_text=a-b-c-_count=42_enabled=True";
        var expectedPath = Path.Combine(_directory, $"XunitSnapshotNamingTests.{nameof(ParameterValues_VerifyAsync_ReusesVerifySnapshot)}{suffix}.verified.txt");
        await File.WriteAllTextAsync(expectedPath, "// output", TestContext.Current.CancellationToken);

        await result.VerifyAsync("TestId.g.cs", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([expectedPath], Directory.GetFiles(_directory));
        Assert.Equal(text is null ? -1 : 42, count);
        Assert.Equal(text is not null, enabled);
    }

    [Fact]
    public async Task AsyncHelper_VerifyAsync_UsesRunningTestInsteadOfHelperName()
    {
        var result = SnapshotGenerator.Run("// output");
        var expectedPath = Path.Combine(_directory, $"XunitSnapshotNamingTests.{nameof(AsyncHelper_VerifyAsync_UsesRunningTestInsteadOfHelperName)}.verified.txt");
        await File.WriteAllTextAsync(expectedPath, "// output", TestContext.Current.CancellationToken);

        await VerifyThroughHelperAsync(result);

        Assert.Equal([expectedPath], Directory.GetFiles(_directory));
    }

    [Theory]
    [InlineData("trailing.")]
    [InlineData("trailing ")]
    public async Task ParameterEndsWithPeriodOrSpace_VerifyAsync_PreservesVerifyFileStem(string text)
    {
        var result = SnapshotGenerator.Run("// output");
        var expectedPath = Path.Combine(_directory, $"XunitSnapshotNamingTests.{nameof(ParameterEndsWithPeriodOrSpace_VerifyAsync_PreservesVerifyFileStem)}_text={text}.verified.txt");
        await File.WriteAllTextAsync(expectedPath, "// output", TestContext.Current.CancellationToken);

        await result.VerifyAsync("TestId.g.cs", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([expectedPath], Directory.GetFiles(_directory));
    }

    private async Task VerifyThroughHelperAsync(GeneratorDriverRunResult result)
    {
        await Task.Yield();
        await result.VerifyAsync("TestId.g.cs", sourceFile: SourceFile, cancellationToken: TestContext.Current.CancellationToken);
    }
}

public sealed class SnapshotNamingContainer
{
    public sealed class NestedTests : IAsyncLifetime
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "SourceGeneratorTestHelpers.Naming", Guid.NewGuid().ToString("N"));

        public ValueTask InitializeAsync()
        {
            Directory.CreateDirectory(_directory);
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            Directory.Delete(_directory, recursive: true);
            return ValueTask.CompletedTask;
        }

        [Fact]
        public async Task NestedClass_VerifyAsync_IncludesEnclosingClass()
        {
            var result = SnapshotGenerator.Run("// output");
            var expectedPath = Path.Combine(_directory, "SnapshotNamingContainer.NestedTests.NestedClass_VerifyAsync_IncludesEnclosingClass.verified.txt");
            await File.WriteAllTextAsync(expectedPath, "// output", TestContext.Current.CancellationToken);

            await result.VerifyAsync("TestId.g.cs", sourceFile: Path.Combine(_directory, "OtherFile.cs"), cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal([expectedPath], Directory.GetFiles(_directory));
        }
    }
}
