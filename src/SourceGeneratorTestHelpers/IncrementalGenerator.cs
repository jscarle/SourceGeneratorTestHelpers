using System.Collections.Immutable;
using System.Text;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceGeneratorTestHelpers;

/// <summary>Provides helper methods for testing <see cref="IIncrementalGenerator"/> implementations.</summary>
public static class IncrementalGenerator
{
    /// <summary>Executes a specified <see cref="IIncrementalGenerator"/> against the provided source within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="IIncrementalGenerator"/> to execute.</typeparam>
    /// <param name="source">The source to be analyzed and processed by the <see cref="IIncrementalGenerator"/>.</param>
    /// <param name="cSharpParseOptions">The C# source parsing options to compile with.</param>
    /// <param name="metadataReferences">The metadata references to compile with.</param>
    /// <param name="cSharpCompilationOptions">The C# compilation options to compile with.</param>
    /// <returns>The results of the <see cref="IIncrementalGenerator"/> execution.</returns>
    public static GeneratorDriverRunResult Run<T>(
        string source,
        CSharpParseOptions? cSharpParseOptions = null,
        IEnumerable<MetadataReference>? metadataReferences = null,
        CSharpCompilationOptions? cSharpCompilationOptions = null
    )
        where T : IIncrementalGenerator, new()
    {
        var generators = GetGenerators<T>().Select(x => x.AsSourceGenerator());
        var driver = CSharpGeneratorDriver.Create(generators, null, cSharpParseOptions);

        var compilation = CSharpCompilation.Create(
            nameof(SourceGeneratorTestHelpers),
            new[] { CSharpSyntaxTree.ParseText(source, cSharpParseOptions) },
            metadataReferences ?? NetStandard20.References.All,
            cSharpCompilationOptions
            );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        return runResult;
    }

    /// <summary>Executes a specified <see cref="IIncrementalGenerator"/> against the provided sources within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="IIncrementalGenerator"/> to execute.</typeparam>
    /// <param name="sources">The sources to be analyzed and processed by the <see cref="IIncrementalGenerator"/>.</param>
    /// <param name="cSharpParseOptions">The C# source parsing options to compile with.</param>
    /// <param name="metadataReferences">The metadata references to compile with.</param>
    /// <param name="cSharpCompilationOptions">The C# compilation options to compile with.</param>
    /// <returns>The result of the <see cref="IIncrementalGenerator"/> execution.</returns>
    public static GeneratorDriverRunResult Run<T>(
        IEnumerable<string> sources,
        CSharpParseOptions? cSharpParseOptions = null,
        IEnumerable<MetadataReference>? metadataReferences = null,
        CSharpCompilationOptions? cSharpCompilationOptions = null
    )
        where T : IIncrementalGenerator, new()
    {
        var generators = GetGenerators<T>().Select(x => x.AsSourceGenerator());
        var driver = CSharpGeneratorDriver.Create(generators, null, cSharpParseOptions);

        var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(source, cSharpParseOptions)).ToArray();

        var compilation = CSharpCompilation.Create(
            nameof(SourceGeneratorTestHelpers),
            syntaxTrees,
            metadataReferences ?? NetStandard20.References.All,
            cSharpCompilationOptions
            );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        return runResult;
    }

    /// <summary>Gather compilation diagnostics and executes a specified <see cref="IIncrementalGenerator"/> against the provided sources within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="IIncrementalGenerator"/> to execute.</typeparam>
    /// <param name="sources">The sources to be analyzed and processed by the <see cref="IIncrementalGenerator"/>.</param>
    /// <param name="cSharpParseOptions">The C# source parsing options to compile with.</param>
    /// <param name="metadataReferences">The metadata references to compile with.</param>
    /// <param name="cSharpCompilationOptions">The C# compilation options to compile with.</param>
    /// <returns>The compilation diagnostics and the result of the <see cref="IIncrementalGenerator"/> execution.</returns>
    public static (ImmutableArray<Diagnostic> Diagnostics, GeneratorDriverRunResult Result) RunWithDiagnostics<T>(
        IEnumerable<string> sources,
        CSharpParseOptions? cSharpParseOptions = null,
        IEnumerable<MetadataReference>? metadataReferences = null,
        CSharpCompilationOptions? cSharpCompilationOptions = null
    )
        where T : IIncrementalGenerator, new()
    {
        var generators = GetGenerators<T>().Select(x => x.AsSourceGenerator());
        var driver = CSharpGeneratorDriver.Create(generators, null, cSharpParseOptions);

        var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(source, cSharpParseOptions, encoding: Encoding.UTF8)).ToArray();

        var compilation = CSharpCompilation.Create(
            nameof(SourceGeneratorTestHelpers),
            syntaxTrees,
            metadataReferences ?? NetStandard20.References.All,
            cSharpCompilationOptions
            );

        var diagnostics = compilation.GetDiagnostics();

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        return (diagnostics, runResult);
    }

    private static IEnumerable<T> GetGenerators<T>()
        where T : IIncrementalGenerator, new()
    {
        yield return new T();
    }
}
