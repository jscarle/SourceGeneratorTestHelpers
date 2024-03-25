# Source Generator Test Helpers

Test helpers and extension methods to simplify testing of .NET source generators.

[![main](https://img.shields.io/github/actions/workflow/status/jscarle/SourceGeneratorTestHelpers/main.yml?logo=github)](https://github.com/jscarle/SourceGeneratorTestHelpers)
[![nuget](https://img.shields.io/nuget/v/SourceGeneratorTestHelpers)](https://www.nuget.org/packages/SourceGeneratorTestHelpers)
[![downloads](https://img.shields.io/nuget/dt/SourceGeneratorTestHelpers)](https://www.nuget.org/packages/SourceGeneratorTestHelpers)

## Testing a source generator

```csharp
var result = SourceGenerator.Run<YourSourceGenerator>("your source");
```

## Testing an incremental source generator

```csharp
var result = IncrementalGenerator.Run<YourSourceGenerator>("your source");
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
var result = IncrementalGenerator.Run<YourSourceGenerator>("your source");

result.ShouldProduce("TestId.g.cs", "expected source");
```

_Note: If you do not wish to assert on errors produced during diagnostics of the source generator run, you can simply disable them as such._

```csharp
var result = IncrementalGenerator.Run<YourSourceGenerator>("your source");

result.ShouldProduce("TestId.g.cs", "expected source", false);
```

### Verify the difference

Support for [Verify](https://github.com/VerifyTests/Verify) is built-in using the `VerifyAsync` method.

#### XUnit

```cs
public class SourceGeneratorTests
{
    [Fact]
    public Task ShouldProductTestId()
    {
        var result = IncrementalGenerator.Run<YourSourceGenerator>("your source");
        return result.VerifyAsync("TestId.g.cs");
    }
}
```

#### NUnit

```cs
[TestFixture]
public class SourceGeneratorTests
{
    [Test]
    public Task ShouldProductTestId()
    {
        var result = IncrementalGenerator.Run<YourSourceGenerator>("your source");
        return result.VerifyAsync("TestId.g.cs");
    }
}
```

#### MSTest

```cs
[TestClass]
public class SourceGeneratorTests :
    GeneratorDriverTestBase
{
    [TestMethod]
    public Task ShouldProductTestId()
    {
        var result = IncrementalGenerator.Run<YourSourceGenerator>("your source");
        return VerifyAsync("TestId.g.cs");
    }
}
```
