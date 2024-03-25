using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace SourceGeneratorTestHelpers.MSTest;

internal abstract class GeneratorDriverTestBase : VerifyBase
{
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
        var generatedSource = result.InternalGetSource(filePathEndsWith, assertOnErrors, message => throw new AssertFailedException(message));

        // ReSharper disable once ExplicitCallerInfoArgument
        return Verify(generatedSource, "txt", verifySettings, sourceFile);
    }
}
