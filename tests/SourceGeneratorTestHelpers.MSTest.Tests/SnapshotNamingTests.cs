using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SourceGeneratorTestHelpers.Tests.Fixtures;

[assembly: DoNotParallelize]

namespace SourceGeneratorTestHelpers.MSTest.Tests;

// Caller paths keep integration-test snapshots inside temporary directories.
// ReSharper disable ExplicitCallerInfoArgument
[TestClass]
public sealed class SnapshotNamingTests : GeneratorDriverTestBase
{
    private string _directory = "";

    [TestInitialize]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "SourceGeneratorTestHelpers.MSTest.Naming", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    [TestCleanup]
    public void TearDown()
    {
        Directory.Delete(_directory, recursive: true);
    }

    [TestMethod]
    public async Task ClassNameDiffersFromFileName_VerifyAsync_ReusesVerifySnapshot()
    {
        var result = SnapshotGenerator.Run("// output");
        var expectedPath = Path.Combine(_directory, $"SnapshotNamingTests.{nameof(ClassNameDiffersFromFileName_VerifyAsync_ReusesVerifySnapshot)}.verified.txt");
        await File.WriteAllTextAsync(expectedPath, "// output");

        await VerifyAsync(result, "TestId.g.cs", sourceFile: Path.Combine(_directory, "DifferentFile.cs"));
        var diagnostics = (ImmutableArray<Diagnostic>.Empty, result);
        await VerifyAsync(diagnostics, "TestId.g.cs", sourceFile: Path.Combine(_directory, "DifferentFile.cs"));

        CollectionAssert.AreEqual(new[] { expectedPath }, Directory.GetFiles(_directory));
    }

    [TestMethod]
    [DataRow("a/b:c?", 42, true)]
    [DataRow(null, -1, false)]
    public async Task ParameterValues_VerifyAsync_ReusesVerifySnapshot(string? text, int count, bool enabled)
    {
        var result = SnapshotGenerator.Run("// output");
        var suffix = text is null ? "_text=null_count=-1_enabled=False" : "_text=a-b-c-_count=42_enabled=True";
        var expectedPath = Path.Combine(_directory, $"SnapshotNamingTests.{nameof(ParameterValues_VerifyAsync_ReusesVerifySnapshot)}{suffix}.verified.txt");
        await File.WriteAllTextAsync(expectedPath, "// output");

        await VerifyAsync(result, "TestId.g.cs", sourceFile: Path.Combine(_directory, "DifferentFile.cs"));

        CollectionAssert.AreEqual(new[] { expectedPath }, Directory.GetFiles(_directory));
        Assert.AreEqual(text is null ? -1 : 42, count);
        Assert.AreEqual(text is not null, enabled);
    }
}

[TestClass]
public sealed class ExistingContextPropertyTests : GeneratorDriverTestBase
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "MSTEST0036:Do not shadow TestContext", Justification = "Exercise consumers with an existing context property.")]
    public new TestContext? TestContext { get; set; }

    [TestMethod]
    public async Task ConsumerContextProperty_VerifyAsync_ReusesVerifySnapshot()
    {
        var directory = Path.Combine(Path.GetTempPath(), "SourceGeneratorTestHelpers.MSTest.Naming", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var result = SnapshotGenerator.Run("// output");
            var expectedPath = Path.Combine(directory, "ExistingContextPropertyTests.ConsumerContextProperty_VerifyAsync_ReusesVerifySnapshot.verified.txt");
            await File.WriteAllTextAsync(expectedPath, "// output");

            await VerifyAsync(result, "TestId.g.cs", sourceFile: Path.Combine(directory, "DifferentFile.cs"));

            CollectionAssert.AreEqual(new[] { expectedPath }, Directory.GetFiles(directory));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
