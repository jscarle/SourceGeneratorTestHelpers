using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceGeneratorTestHelpers;

/// <summary>Provides helper methods for testing <see cref="ISourceGenerator"/> implementations.</summary>
public static class SourceGenerator
{
    /// <summary>Executes a specified <see cref="ISourceGenerator"/> against the provided source within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="ISourceGenerator"/> to execute.</typeparam>
    /// <param name="source">The source to be analyzed and processed by the <see cref="ISourceGenerator"/>.</param>
    /// <param name="cSharpParseOptions">The C# source parsing options to compile with.</param>
    /// <param name="metadataReferences">The metadata references to compile with.</param>
    /// <param name="cSharpCompilationOptions">The C# compilation options to compile with.</param>
    /// <returns>The results of the <see cref="ISourceGenerator"/> execution.</returns>
    public static GeneratorDriverRunResult Run<T>(
        string source,
        CSharpParseOptions? cSharpParseOptions = null,
        IEnumerable<MetadataReference>? metadataReferences = null,
        CSharpCompilationOptions? cSharpCompilationOptions = null
    )
        where T : ISourceGenerator, new()
    {
        var generators = GetGenerators<T>().Select(x => (ISourceGenerator)x);
        var driver = CSharpGeneratorDriver.Create(generators, null, cSharpParseOptions);

        var compilation = CSharpCompilation.Create(
            nameof(SourceGeneratorTestHelpers),
            new[] { CSharpSyntaxTree.ParseText(source, cSharpParseOptions) },
            metadataReferences ?? new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            cSharpCompilationOptions
            );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        return runResult;
    }

    /// <summary>Executes a specified <see cref="ISourceGenerator"/> against the provided source within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="ISourceGenerator"/> to execute.</typeparam>
    /// <param name="sources">The sources to be analyzed and processed by the <see cref="ISourceGenerator"/>.</param>
    /// <param name="cSharpParseOptions">The C# source parsing options to compile with.</param>
    /// <param name="metadataReferences">The metadata references to compile with.</param>
    /// <param name="cSharpCompilationOptions">The C# compilation options to compile with.</param>
    /// <returns>The results of the <see cref="ISourceGenerator"/> execution.</returns>
    public static GeneratorDriverRunResult Run<T>(
        IEnumerable<string> sources,
        CSharpParseOptions? cSharpParseOptions = null,
        IEnumerable<MetadataReference>? metadataReferences = null,
        CSharpCompilationOptions? cSharpCompilationOptions = null
    )
        where T : ISourceGenerator, new()
    {
        var generators = GetGenerators<T>().Select(x => (ISourceGenerator)x);
        var driver = CSharpGeneratorDriver.Create(generators, null, cSharpParseOptions);

        var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(source, cSharpParseOptions)).ToArray();

        var compilation = CSharpCompilation.Create(
            nameof(SourceGeneratorTestHelpers),
            syntaxTrees,
            metadataReferences ?? new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            cSharpCompilationOptions
            );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        return runResult;
    }

    private static IEnumerable<T> GetGenerators<T>()
        where T : ISourceGenerator, new()
    {
        yield return new T();
    }
}
