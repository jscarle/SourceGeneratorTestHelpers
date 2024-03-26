using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace SourceGeneratorTestHelpers.MSTest;

internal abstract class GeneratorDriverTestBase : VerifyBase
{
    /// <summary>Verifies that the generated source from a <see cref="GeneratorDriverRunResult"/> with a specific file path ending matches the expected source.</summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult"/> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <param name="expectedSource">The expected source that the generated source should match.</param>
    /// <param name="assertOnErrors">
    /// <see langword="true"/> to assert on reported errors by the source generator, <see langword="false"/> othwerwise. Defaults to
    /// <see langword="true"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">If <paramref name="result"/> is null.</exception>
    public static void ShouldProduce(GeneratorDriverRunResult result, string filePathEndsWith, string expectedSource, bool assertOnErrors = true)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
#else
        if (result is null)
            throw new ArgumentNullException(nameof(result));
#endif

        result.InternalShouldProduce(filePathEndsWith, expectedSource, assertOnErrors, message => throw new AssertFailedException(message));
    }

    /// <summary>Verifies that the generated source from a <see cref="GeneratorDriverRunResult"/> with a specific file path ending matches the expected source.</summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult"/> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <param name="expectedSource">The expected source that the generated source should match.</param>
    /// <param name="assertOnErrors">
    /// <see langword="true"/> to assert on reported errors by the source generator, <see langword="false"/> othwerwise. Defaults to
    /// <see langword="true"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">If <paramref name="result"/> is null.</exception>
    public static void ShouldProduce(
        (ImmutableArray<Diagnostic> Diagnostics, GeneratorDriverRunResult Result) result,
        string filePathEndsWith,
        string expectedSource,
        bool assertOnErrors = true
    )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result.Result);
#else
        if (result.Result is null)
            throw new ArgumentNullException(nameof(result));
#endif

        if (assertOnErrors && result.Diagnostics.Length > 0)
            throw new AssertFailedException(result.Diagnostics.GetExceptionMessage());

        result.Result.InternalShouldProduce(filePathEndsWith, expectedSource, assertOnErrors, message => throw new AssertFailedException(message));
    }

    /// <summary>
    /// Verifies that the generated source from a <see cref="GeneratorDriverRunResult"/> with a specific file path using
    /// <see cref="VerifyBase.Verify(string?, string, VerifySettings?, string)"/>.
    /// </summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult"/> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <param name="assertOnErrors">
    /// <see langword="true"/> to assert on reported errors by the source generator, <see langword="false"/> othwerwise. Defaults to
    /// <see langword="true"/>.
    /// </param>
    /// <param name="verifySettings">The verify settings.</param>
    /// <param name="sourceFile">The source file.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="result"/> is null.</exception>
    public SettingsTask VerifyAsync(
        GeneratorDriverRunResult result,
        string filePathEndsWith,
        bool assertOnErrors = true,
        VerifySettings? verifySettings = null,
        [CallerFilePath] string sourceFile = ""
    )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
#else
        if (result is null)
            throw new ArgumentNullException(nameof(result));
#endif

        var generatedSource = result.InternalGetSource(filePathEndsWith, assertOnErrors, message => throw new AssertFailedException(message));

        // ReSharper disable once ExplicitCallerInfoArgument
        return Verify(generatedSource, "txt", verifySettings, sourceFile);
    }

    /// <summary>
    /// Verifies that the generated source from a <see cref="GeneratorDriverRunResult"/> with a specific file path using
    /// <see cref="VerifyBase.Verify(string?, string, VerifySettings?, string)"/>.
    /// </summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult"/> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <param name="assertOnErrors">
    /// <see langword="true"/> to assert on reported errors by the source generator, <see langword="false"/> othwerwise. Defaults to
    /// <see langword="true"/>.
    /// </param>
    /// <param name="verifySettings">The verify settings.</param>
    /// <param name="sourceFile">The source file.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="result"/> is null.</exception>
    public SettingsTask VerifyAsync(
        (ImmutableArray<Diagnostic> Diagnostics, GeneratorDriverRunResult Result) result,
        string filePathEndsWith,
        bool assertOnErrors = true,
        VerifySettings? verifySettings = null,
        [CallerFilePath] string sourceFile = ""
    )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result.Result);
#else
        if (result.Result is null)
            throw new ArgumentNullException(nameof(result));
#endif

        if (assertOnErrors && result.Diagnostics.Length > 0)
            throw new AssertFailedException(result.Diagnostics.GetExceptionMessage());

        var generatedSource = result.Result.InternalGetSource(filePathEndsWith, assertOnErrors, message => throw new AssertFailedException(message));

        // ReSharper disable once ExplicitCallerInfoArgument
        return Verify(generatedSource, "txt", verifySettings, sourceFile);
    }
}
