using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SourceGeneratorTestHelpers.MSTest;

/// <summary>Provides extension methods for the <see cref="GeneratorDriverRunResult" /> class.</summary>
public static class GeneratorDriverRunResultExtensions
{
    /// <summary>Verifies that the generated source from a <see cref="GeneratorDriverRunResult" /> with a specific file path ending matches the expected source.</summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult" /> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <param name="expectedSource">The expected source that the generated source should match.</param>
    /// <param name="assertOnErrors"><see langword="true" /> to assert on reported errors by the source generator, <see langword="false" /> othwerwise. Defaults to <see langword="true" />.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="result" /> is null.</exception>
    public static void ShouldProduce(this GeneratorDriverRunResult result, string filePathEndsWith, string expectedSource, bool assertOnErrors = true)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
#else
        if (result is null)
            throw new ArgumentNullException(nameof(result));
#endif
        result.InternalShouldProduce(filePathEndsWith, expectedSource, assertOnErrors, message => throw new AssertFailedException(message));
    }
}
