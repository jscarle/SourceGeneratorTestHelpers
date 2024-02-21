# Source Generator Test Helpers

Test helpers and extension methods to simplify testing of .NET source generators.

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
