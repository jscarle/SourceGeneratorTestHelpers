using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using SourceGeneratorTestHelpers.Tests.Fixtures;
using NUnitAssert = NUnit.Framework.Assert;

namespace SourceGeneratorTestHelpers.NUnit.Tests;

// Caller paths keep integration-test snapshots inside temporary directories.
// ReSharper disable ExplicitCallerInfoArgument
[TestFixture]
public sealed class SnapshotNamingTests
{
    private string _directory = "";

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "SourceGeneratorTestHelpers.NUnit.Naming", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_directory, recursive: true);
    }

    [Test]
    public async Task ClassNameDiffersFromFileName_VerifyAsync_ReusesVerifySnapshot()
    {
        var result = SnapshotGenerator.Run("// output");
        var expectedPath = Path.Combine(_directory, $"SnapshotNamingTests.{nameof(ClassNameDiffersFromFileName_VerifyAsync_ReusesVerifySnapshot)}.verified.txt");
        await File.WriteAllTextAsync(expectedPath, "// output");

        await result.VerifyAsync("TestId.g.cs", sourceFile: Path.Combine(_directory, "DifferentFile.cs"));
        var diagnostics = (ImmutableArray<Diagnostic>.Empty, result);
        await diagnostics.VerifyAsync("TestId.g.cs", sourceFile: Path.Combine(_directory, "DifferentFile.cs"));

        NUnitAssert.That(Directory.GetFiles(_directory), Is.EqualTo(new[] { expectedPath }));
    }

    [TestCase("a/b:c?", 42, true)]
    [TestCase(null, -1, false)]
    public async Task ParameterValues_VerifyAsync_ReusesVerifySnapshot(string? text, int count, bool enabled)
    {
        var result = SnapshotGenerator.Run("// output");
        var suffix = text is null ? "_text=null_count=-1_enabled=False" : "_text=a-b-c-_count=42_enabled=True";
        var expectedPath = Path.Combine(_directory, $"SnapshotNamingTests.{nameof(ParameterValues_VerifyAsync_ReusesVerifySnapshot)}{suffix}.verified.txt");
        await File.WriteAllTextAsync(expectedPath, "// output");

        await result.VerifyAsync("TestId.g.cs", sourceFile: Path.Combine(_directory, "DifferentFile.cs"));

        NUnitAssert.That(Directory.GetFiles(_directory), Is.EqualTo(new[] { expectedPath }));
        NUnitAssert.That(count, Is.EqualTo(text is null ? -1 : 42));
        NUnitAssert.That(enabled, Is.EqualTo(text is not null));
    }
}

[TestFixture("first/fixture")]
[TestFixture("second:fixture")]
public sealed class ParameterizedFixtureNamingTests(string fixtureName)
{
    private string _directory = "";

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "SourceGeneratorTestHelpers.NUnit.Naming", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_directory, recursive: true);
    }

    [TestCase(1)]
    [TestCase(2)]
    public async Task FixtureAndMethodArguments_VerifyAsync_ReusesVerifySnapshot(int count)
    {
        var result = SnapshotGenerator.Run("// output");
        var fixtureValue = fixtureName == "first/fixture" ? "first-fixture" : "second-fixture";
        var expectedPath = Path.Combine(_directory, $"ParameterizedFixtureNamingTests.{nameof(FixtureAndMethodArguments_VerifyAsync_ReusesVerifySnapshot)}_fixtureName={fixtureValue}_count={count}.verified.txt");
        await File.WriteAllTextAsync(expectedPath, "// output");

        await result.VerifyAsync("TestId.g.cs", sourceFile: Path.Combine(_directory, "DifferentFile.cs"));

        NUnitAssert.That(Directory.GetFiles(_directory), Is.EqualTo(new[] { expectedPath }));
    }
}
