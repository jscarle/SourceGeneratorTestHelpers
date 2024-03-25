using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceGeneratorTestHelpers;

/// <summary>Provides helper methods for testing <see cref="IIncrementalGenerator"/> implementations.</summary>
public static class IncrementalGenerator
{
    /// <summary>Executes a specified <see cref="IIncrementalGenerator"/> against the provided source within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="IIncrementalGenerator"/> to execute.</typeparam>
    /// <param name="source">The source to be analyzed and processed by the <see cref="IIncrementalGenerator"/>.</param>
    /// <param name="metadataReferences">The metadata references to compile with.</param>
    /// <returns>The results of the <see cref="IIncrementalGenerator"/> execution.</returns>
    public static GeneratorDriverRunResult Run<T>(string source, IEnumerable<MetadataReference>? metadataReferences = null)
        where T : IIncrementalGenerator, new()
    {
        var generator = new T();

        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            nameof(SourceGeneratorTestHelpers),
            new[] { CSharpSyntaxTree.ParseText(source) },
            metadataReferences ?? new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) }
            );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        return runResult;
    }

    /// <summary>Executes a specified <see cref="IIncrementalGenerator"/> against the provided sources within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="IIncrementalGenerator"/> to execute.</typeparam>
    /// <param name="sources">The sources to be analyzed and processed by the <see cref="IIncrementalGenerator"/>.</param>
    /// <param name="metadataReferences">The metadata references to compile with.</param>
    /// <returns>The results of the <see cref="IIncrementalGenerator"/> execution.</returns>
    public static GeneratorDriverRunResult Run<T>(IEnumerable<string> sources, IEnumerable<MetadataReference>? metadataReferences = null)
        where T : IIncrementalGenerator, new()
    {
        var generator = new T();

        var driver = CSharpGeneratorDriver.Create(generator);

        var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(source)).ToArray();

        var compilation = CSharpCompilation.Create(
            nameof(SourceGeneratorTestHelpers),
            syntaxTrees,
            metadataReferences ?? new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) }
            );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        return runResult;
    }
}
