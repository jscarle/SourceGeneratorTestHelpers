using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceGeneratorTestHelpers;

/// <summary>Provides helper methods for testing <see cref="ISourceGenerator"/> implementations.</summary>
public static class SourceGenerator
{
    /// <summary>Executes a specified <see cref="ISourceGenerator"/> against the provided source within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="ISourceGenerator"/> to execute.</typeparam>
    /// <param name="source">The source to be analyzed and processed by the <see cref="ISourceGenerator"/>.</param>
    /// <returns>The results of the <see cref="ISourceGenerator"/> execution.</returns>
    public static GeneratorDriverRunResult Run<T>(string source)
        where T : ISourceGenerator, new()
    {
        var generator = new T();

        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            nameof(SourceGeneratorTestHelpers),
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) }
            );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        return runResult;
    }

    /// <summary>Executes a specified <see cref="ISourceGenerator"/> against the provided source within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="ISourceGenerator"/> to execute.</typeparam>
    /// <param name="sources">The sources to be analyzed and processed by the <see cref="ISourceGenerator"/>.</param>
    /// <returns>The results of the <see cref="ISourceGenerator"/> execution.</returns>
    public static GeneratorDriverRunResult Run<T>(IEnumerable<string> sources)
        where T : ISourceGenerator, new()
    {
        var generator = new T();

        var driver = CSharpGeneratorDriver.Create(generator);

        var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(source)).ToArray();

        var compilation = CSharpCompilation.Create(
            nameof(SourceGeneratorTestHelpers),
            syntaxTrees,
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) }
            );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        return runResult;
    }
}
