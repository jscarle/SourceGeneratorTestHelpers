using Microsoft.CodeAnalysis;

namespace SourceGeneratorTestHelpers;

/// <summary>Provides extension methods for the <see cref="GeneratorDriverRunResult" /> class.</summary>
public static class GeneratorDriverRunResultExtensions
{
    /// <summary>Gets the generated sources from a <see cref="GeneratorDriverRunResult" />.</summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult" /> to get the sources from.</param>
    /// <returns>An <see cref="IEnumerable{GeneratedSource}" /> containing the generated sources.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="result" /> is null.</exception>
    public static IEnumerable<GeneratedSource> GetSources(this GeneratorDriverRunResult result)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
#else
        if (result is null)
            throw new ArgumentNullException(nameof(result));
#endif

        var sources = result.GeneratedTrees.Select(
            tree =>
            {
                var filePath = tree.FilePath;
                var source = tree.GetText().ToString();

                return new GeneratedSource(filePath, source);
            }
            );

        return sources;
    }

    /// <summary>Gets the generated source from a <see cref="GeneratorDriverRunResult" /> with a specific file path ending.</summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult" /> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <returns>The source of the generated file, or null if no matching file is found.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="result" /> is null.</exception>
    public static string? GetSource(this GeneratorDriverRunResult result, string filePathEndsWith)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
#else
        if (result is null)
            throw new ArgumentNullException(nameof(result));
#endif

        var generatedFileSyntax = result.GeneratedTrees.SingleOrDefault(tree => tree.FilePath.EndsWith(filePathEndsWith, StringComparison.Ordinal));

        return generatedFileSyntax?.GetText().ToString();
    }
}