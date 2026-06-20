using Microsoft.CodeAnalysis;
using SourceGeneratorTestHelpers.Common;

namespace SourceGeneratorTestHelpers;

/// <summary>Provides extension methods for the <see cref="GeneratorDriverRunResult"/> class.</summary>
public static class Extensions
{
    /// <summary>Gets the generated source ending with a specific file path from a <see cref="GeneratorDriverRunResult"/>.</summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult"/> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <returns>The source of the generated file, or null if no matching file is found.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="result"/> is null.</exception>
    public static GeneratedSource? GetSource(this GeneratorDriverRunResult result, string filePathEndsWith)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.InternalGetSource(filePathEndsWith);
    }

    /// <summary>Gets the generated sources from a <see cref="GeneratorDriverRunResult"/>.</summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult"/> to get the sources from.</param>
    /// <returns>An <see cref="IEnumerable{GeneratedSource}"/> containing the generated sources.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="result"/> is null.</exception>
    public static IReadOnlyCollection<GeneratedSource> GetSources(this GeneratorDriverRunResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.InternalGetSources();
    }
}
