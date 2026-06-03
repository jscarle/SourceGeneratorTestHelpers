# Source Generator Test Helpers（源生成器测试辅助工具）

用于简化 .NET 源生成器测试的测试辅助方法和扩展方法。

[![main](https://img.shields.io/github/actions/workflow/status/jscarle/SourceGeneratorTestHelpers/main.yml?logo=github)](https://github.com/jscarle/SourceGeneratorTestHelpers)
[![nuget](https://img.shields.io/nuget/v/SourceGeneratorTestHelpers)](https://www.nuget.org/packages/SourceGeneratorTestHelpers)
[![downloads](https://img.shields.io/nuget/dt/SourceGeneratorTestHelpers)](https://www.nuget.org/packages/SourceGeneratorTestHelpers)

## 测试一个源生成器

```csharp
var result = SourceGenerator.Run<YourSourceGenerator>("要测试的代码");
```

## 测试一个增量源生成器

```csharp
var result = IncrementalGenerator.Run<YourSourceGenerator>("要测试的代码");
```

## 添加.NET环境和依赖

```csharp
    var references = new List<MetadataReference>();

    // 添加 .NET 10 所有程序集（ReferenceAssemblies.Net.Net100）
    ImmutableArray<MetadataReference> defaultReferences = ReferenceAssemblies.Net.Net100.ResolveAsync(null, default)。Result;
    references.AddRange(defaultReferences);

    //添加依赖
    references.Add(MetadataReference.CreateFromFile(
        Path.Combine("D:","C#", "Lib", "Debug", "net10.0", "XXX.dll")));

    var result = IncrementalGenerator.Run<YourSourceGenerator>("要测试的代码", null, references);
```

## 获取生成的源代码

### 获取所有生成的源代码

```csharp
var generatedSources =  result.GetSources();
```

### 获取特定文件的源代码

```csharp
var generatedSource =  result.GetSource("TestId.g.cs");
```

### 对比生成的源代码与期望的源代码

你可以对比生成的源代码与期望的源代码之间的差异。结果包含一个bool值 hasDifferences 以及按行区分的差异列表 differences。

```csharp
var (hasDifferences, differences) = Diff.Compare(generatedSource, expectedSource);
```

## 进行验证

### 使用断言

借助下面其中一个测试框架包，你还可以断言生成的源代码与期望源的代码之间的差异。

[![XUnit](https://img.shields.io/nuget/dt/SourceGeneratorTestHelpers.XUnit?label=XUnit)](https://www.nuget.org/packages/SourceGeneratorTestHelpers.XUnit)
[![NUnit](https://img.shields.io/nuget/dt/SourceGeneratorTestHelpers.NUnit?label=NUnit)](https://www.nuget.org/packages/SourceGeneratorTestHelpers.NUnit)
[![MSTest](https://img.shields.io/nuget/dt/SourceGeneratorTestHelpers.MSTest?label=MSTest)](https://www.nuget.org/packages/SourceGeneratorTestHelpers.MSTest)

```csharp
var result = IncrementalGenerator.Run<YourSourceGenerator>("要测试的代码");

result.ShouldProduce("TestId.g.cs", "预期的代码");
```

_注意：如果你不想断言源生成器运行期间的诊断错误，可以像下面这样直接禁用。_

```csharp
var result = IncrementalGenerator.Run<YourSourceGenerator>("要测试的代码");

result.ShouldProduce("TestId.g.cs", "预期的代码", false);
```

### 使用Verify

内置了对 [Verify](https://github.com/VerifyTests/Verify) 的支持，通过 VerifyAsync 方法实现。 

#### XUnit

```cs
public class SourceGeneratorTests
{
    [Fact]
    public Task ShouldProductTestId()
    {
        var result = IncrementalGenerator.Run<YourSourceGenerator>("要测试的代码");
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
        var result = IncrementalGenerator.Run<YourSourceGenerator>("要测试的代码");
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
        var result = IncrementalGenerator.Run<YourSourceGenerator>("要测试的代码");
        return VerifyAsync("TestId.g.cs");
    }
}
```
