using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceGeneratorTestHelpers.Common;

namespace SourceGeneratorTestHelpers;

/// <summary>Provides helper methods for testing <see cref="ISourceGenerator"/> implementations.</summary>
public static class SourceGenerator
{
    /// <summary>Executes a specified <see cref="ISourceGenerator"/> against the provided source within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="ISourceGenerator"/> to execute.</typeparam>
    /// <param name="source">The source to be analyzed and processed by the <see cref="ISourceGenerator"/>.</param>
    /// <param name="cSharpParseOptions">The C# source parsing options to compile with. By default, LangVersion will be set to latest.</param>
    /// <param name="metadataReferences">The metadata references to compile with.</param>
    /// <param name="cSharpCompilationOptions">
    /// The C# compilation options to compile with. By default, Output will be set to library, and Nullable will be set to
    /// enable.
    /// </param>
    /// <returns>The results of the <see cref="ISourceGenerator"/> execution.</returns>
    public static GeneratorDriverRunResult Run<T>(
        string source,
        CSharpParseOptions? cSharpParseOptions = null,
        IEnumerable<MetadataReference>? metadataReferences = null,
        CSharpCompilationOptions? cSharpCompilationOptions = null
    )
        where T : ISourceGenerator, new()
    {
        var generator = new T();

        return Helpers.InternalRunGenerator(generator, source, cSharpParseOptions, metadataReferences, cSharpCompilationOptions).Result;
    }

    /// <summary>Gather compilation diagnostics and executes a specified <see cref="ISourceGenerator"/> against the provided source within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="ISourceGenerator"/> to execute.</typeparam>
    /// <param name="source">The source to be analyzed and processed by the <see cref="ISourceGenerator"/>.</param>
    /// <param name="cSharpParseOptions">The C# source parsing options to compile with. By default, LangVersion will be set to latest.</param>
    /// <param name="metadataReferences">The metadata references to compile with.</param>
    /// <param name="cSharpCompilationOptions">
    /// The C# compilation options to compile with. By default, Output will be set to library, and Nullable will be set to
    /// enable.
    /// </param>
    /// <returns>The compilation diagnostics and the result of the <see cref="ISourceGenerator"/> execution.</returns>
    public static (ImmutableArray<Diagnostic> Diagnostics, GeneratorDriverRunResult Result) RunWithDiagnostics<T>(
        string source,
        CSharpParseOptions? cSharpParseOptions = null,
        IEnumerable<MetadataReference>? metadataReferences = null,
        CSharpCompilationOptions? cSharpCompilationOptions = null
    )
        where T : ISourceGenerator, new()
    {
        var generator = new T();

        return Helpers.InternalRunGenerator(generator, source, cSharpParseOptions, metadataReferences, cSharpCompilationOptions);
    }

    /// <summary>Executes a specified <see cref="ISourceGenerator"/> against the provided source within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="ISourceGenerator"/> to execute.</typeparam>
    /// <param name="sources">The sources to be analyzed and processed by the <see cref="ISourceGenerator"/>.</param>
    /// <param name="cSharpParseOptions">The C# source parsing options to compile with. By default, LangVersion will be set to latest.</param>
    /// <param name="metadataReferences">The metadata references to compile with.</param>
    /// <param name="cSharpCompilationOptions">
    /// The C# compilation options to compile with. By default, Output will be set to library, and Nullable will be set to
    /// enable.
    /// </param>
    /// <returns>The result of the <see cref="ISourceGenerator"/> execution.</returns>
    public static GeneratorDriverRunResult Run<T>(
        IEnumerable<string> sources,
        CSharpParseOptions? cSharpParseOptions = null,
        IEnumerable<MetadataReference>? metadataReferences = null,
        CSharpCompilationOptions? cSharpCompilationOptions = null
    )
        where T : ISourceGenerator, new()
    {
        var generator = new T();

        return Helpers.InternalRunGenerator(generator, sources, cSharpParseOptions, metadataReferences, cSharpCompilationOptions).Result;
    }

    /// <summary>Gather compilation diagnostics and executes a specified <see cref="ISourceGenerator"/> against the provided source within a testing environment.</summary>
    /// <typeparam name="T">The type of <see cref="ISourceGenerator"/> to execute.</typeparam>
    /// <param name="sources">The sources to be analyzed and processed by the <see cref="ISourceGenerator"/>.</param>
    /// <param name="cSharpParseOptions">The C# source parsing options to compile with. By default, LangVersion will be set to latest.</param>
    /// <param name="metadataReferences">The metadata references to compile with.</param>
    /// <param name="cSharpCompilationOptions">
    /// The C# compilation options to compile with. By default, Output will be set to library, and Nullable will be set to
    /// enable.
    /// </param>
    /// <returns>The compilation diagnostics and the result of the <see cref="ISourceGenerator"/> execution.</returns>
    public static (ImmutableArray<Diagnostic> Diagnostics, GeneratorDriverRunResult Result) RunWithDiagnostics<T>(
        IEnumerable<string> sources,
        CSharpParseOptions? cSharpParseOptions = null,
        IEnumerable<MetadataReference>? metadataReferences = null,
        CSharpCompilationOptions? cSharpCompilationOptions = null
    )
        where T : ISourceGenerator, new()
    {
        var generator = new T();

        return Helpers.InternalRunGenerator(generator, sources, cSharpParseOptions, metadataReferences, cSharpCompilationOptions);
    }
}
