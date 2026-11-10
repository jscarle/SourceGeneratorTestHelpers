# Source Generator Test Helpers

Test helpers and extension methods to simplify testing of .NET source generators.

[![develop](https://img.shields.io/github/actions/workflow/status/jscarle/SourceGeneratorTestHelpers/develop.yml?logo=github)](https://github.com/jscarle/SourceGeneratorTestHelpers)
[![nuget](https://img.shields.io/nuget/v/SourceGeneratorTestHelpers)](https://www.nuget.org/packages/SourceGeneratorTestHelpers)
[![downloads](https://img.shields.io/nuget/dt/SourceGeneratorTestHelpers)](https://www.nuget.org/packages/SourceGeneratorTestHelpers)

## Testing a source generator

```csharp
var (generator, result) = SourceGenerator.Run<YourSourceGenerator>("your source");
```

You can also run a generator against multiple source files by passing any `IEnumerable<string>`.

```csharp
var sources = new[]
{
    "namespace Tests; public partial class First;",
    "namespace Tests; public partial class Second;"
};

var (generator, result) = SourceGenerator.Run<YourSourceGenerator>(sources);
```

## Testing an incremental source generator

```csharp
var (generator, result) = IncrementalGenerator.Run<YourSourceGenerator>("your source");
```

The same multiple-source overload is available for incremental generators.

```csharp
var sources = new[]
{
    "namespace Tests; public partial class First;",
    "namespace Tests; public partial class Second;"
};

var (generator, result) = IncrementalGenerator.Run<YourSourceGenerator>(sources);
```

## Obtaining the generated source

### Getting all generated sources

```csharp
var generatedSources =  result.GetSources();
```

### A single source that ends with a specific file path

```csharp
var generatedSource =  result.GetSource("TestId.g.cs");
```

### Compare the generated source with the expected source

You can produce a diff between the generated source and the expected source. The result will contain a boolean `hasDifferences` and a line by line diff
in `differences`.

```csharp
var (hasDifferences, differences) = Diff.Compare(generatedSource, expectedSource);
```

## Assert the difference

Using one of the testing framework packages below, you can also assert the difference between the generated source and the expected source.

[![XUnit](https://img.shields.io/nuget/dt/SourceGeneratorTestHelpers.XUnit?label=XUnit)](https://www.nuget.org/packages/SourceGeneratorTestHelpers.XUnit)
[![NUnit](https://img.shields.io/nuget/dt/SourceGeneratorTestHelpers.NUnit?label=NUnit)](https://www.nuget.org/packages/SourceGeneratorTestHelpers.NUnit)
[![MSTest](https://img.shields.io/nuget/dt/SourceGeneratorTestHelpers.MSTest?label=MSTest)](https://www.nuget.org/packages/SourceGeneratorTestHelpers.MSTest)

```csharp
var (_, result) = IncrementalGenerator.Run<YourSourceGenerator>("your source");

result.ShouldProduce("TestId.g.cs", "expected source");
```

_Note: If you do not wish to assert on errors produced during diagnostics of the source generator run, you can simply disable them as such._

```csharp
var (_, result) = IncrementalGenerator.Run<YourSourceGenerator>("your source");

result.ShouldProduce("TestId.g.cs", "expected source", false);
```

### Compare with an approved snapshot

`VerifyAsync` provides built-in text snapshot testing without a Verify package dependency.
It returns a standard `Task` and uses the assertion exception for your test framework.

#### XUnit

```csharp
using SourceGeneratorTestHelpers;
using SourceGeneratorTestHelpers.XUnit;
using Xunit;

public class SourceGeneratorTests
{
    [Fact]
    public Task ShouldProduceTestId()
    {
        var (_, result) = IncrementalGenerator.Run<YourSourceGenerator>("your source");
        return result.VerifyAsync("TestId.g.cs");
    }
}
```

#### NUnit

```csharp
using NUnit.Framework;
using SourceGeneratorTestHelpers;
using SourceGeneratorTestHelpers.NUnit;

[TestFixture]
public class SourceGeneratorTests
{
    [Test]
    public Task ShouldProduceTestId()
    {
        var (_, result) = IncrementalGenerator.Run<YourSourceGenerator>("your source");
        return result.VerifyAsync("TestId.g.cs");
    }
}
```

#### MSTest

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceGeneratorTestHelpers;
using SourceGeneratorTestHelpers.MSTest;

[TestClass]
public class SourceGeneratorTests : GeneratorDriverTestBase
{
    [TestMethod]
    public Task ShouldProduceTestId()
    {
        var (_, result) = IncrementalGenerator.Run<YourSourceGenerator>("your source");
        return VerifyAsync(result, "TestId.g.cs");
    }
}
```

#### Snapshot files and approval

Snapshots are stored beside the calling test source file. Default names match Verify 31.20.0:
`<TestClass>.<TestMethod>[_parameter=value...].verified.txt`. The test class and method come from the
running test framework, so renaming the source file does not rename snapshots. Nested test classes include
their enclosing class name. The test above uses `SourceGeneratorTests.ShouldProduceTestId.verified.txt`.
The compiler supplies the caller file path only to locate the snapshot directory. If a build maps source paths to a location
that is unavailable when tests run, pass a fully qualified `sourceFile` path in an existing directory.

When the approved file is missing or the output changes, the assertion writes a `.received.txt` file and fails.
In projects targeting multiple frameworks, received names include the selected runtime target, for example
`SourceGeneratorTests.ShouldProduceTestId.DotNet11_0.received.txt`, matching Verify's convention.
Approved snapshots remain shared across target frameworks.
Review the received file, then rename or copy it over the corresponding `.verified.txt` file to approve the output.
Commit approved files to source control and ignore `*.received.txt`. A passing comparison deletes any stale received file.
Tests never create or overwrite approved files automatically and do not launch external diff tools.

Both texts are normalized to LF line endings before ordinal comparison. Other whitespace, including trailing
newlines, remains significant. Received files use UTF-8 without a byte order mark. Missing generated output fails
explicitly; an existing generated file with empty content can match an empty approved snapshot.

Parameterized tests automatically append parameter names and values, for example
`SourceGeneratorTests.ShouldProduceTestId_input=empty_enabled=True.verified.txt`. Null values use `null`,
numeric and date values use invariant formatting, and characters invalid in parameter values are replaced
with `-`. NUnit fixture constructor arguments are included before method arguments.

For custom names or multiple snapshots in one test, supply a distinct, stable name for each output:

```csharp
await result.VerifyAsync("TestId.g.cs", snapshotName: "GeneratesTestId.EmptyInput");
```

This uses `GeneratesTestId.EmptyInput.verified.txt` beside the test source file. Names exclude the
`.verified.txt` suffix and must be valid file names without directory separators. Names are not sanitized automatically.
Automatic naming requires a running test in the matching framework. Explicit names can also be used outside
that framework's runner. Use distinct names for multiple comparisons in one test; calls targeting the same
received file must not overlap.

The diagnostics-tuple overloads retain `assertOnErrors: true` and `assertOnWarnings: false` by default.
Argument validation and enabled diagnostic assertions happen before snapshot file access. Cancellation and
file-access failures propagate to the caller; pass `cancellationToken` to cancel asynchronous file operations.
The raw `GeneratorDriverRunResult` overloads compare generated text without asserting diagnostics.

#### Migrating from Verify

- Ordinary `await result.VerifyAsync("TestId.g.cs")` calls keep the same shape, but now return `Task` instead of `SettingsTask`.
- Remove `VerifySettings` arguments, fluent Verify configuration, and Verify-specific initialization.
  This feature compares generated text; it does not provide Verify's object serialization or scrubbers.
- Existing text snapshots using Verify's default class, method, and parameter naming can be reused directly.
  For custom Verify naming, pass the complete existing file stem as `snapshotName`. Move snapshots from custom
  Verify directories beside the calling test source file.
- `GeneratorDriverTestBase` remains available for MSTest but no longer inherits `VerifyBase`.
  Tests that used other inherited Verify methods must replace those calls or reference Verify independently.
- MSTest supplies the inherited `TestContext` property for automatic naming. Existing consumer `TestContext`
  properties are respected as well.
- The helper packages no longer supply Verify's implicit imports. Import your test framework and helper namespace explicitly.
