﻿using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using SourceGeneratorTestHelpers.Common;

namespace SourceGeneratorTestHelpers.NUnit;

/// <summary>Provides extension methods for the <see cref="GeneratorDriverRunResult"/> class.</summary>
public static class GeneratorDriverRunResultExtensions
{
    /// <summary>Verifies that the generated source from a <see cref="GeneratorDriverRunResult"/> with a specific file path ending matches the expected source.</summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult"/> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <param name="expectedSource">The expected source that the generated source should match.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="result"/> is null.</exception>
    public static void ShouldProduce(
        this GeneratorDriverRunResult result,
        string filePathEndsWith,
        string expectedSource
    )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
#else
        if (result is null)
            throw new ArgumentNullException(nameof(result));
#endif

        result.InternalShouldProduce(filePathEndsWith, expectedSource, false, false, _ => { });
    }

    /// <summary>Verifies that the generated source from a <see cref="GeneratorDriverRunResult"/> with a specific file path ending matches the expected source.</summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult"/> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <param name="expectedSource">The expected source that the generated source should match.</param>
    /// <param name="assertOnErrors"><see langword="true"/> to assert on reported errors, <see langword="false"/> othwerwise. Defaults to <see langword="true"/>.</param>
    /// <param name="assertOnWarnings"><see langword="true"/> to assert on reported warnings, <see langword="false"/> othwerwise. Defaults to <see langword="false"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="result"/> is null.</exception>
    public static void ShouldProduce(
        this (ImmutableArray<Diagnostic> CompilationDiagnostics, GeneratorDriverRunResult Result) result,
        string filePathEndsWith,
        string expectedSource,
        bool assertOnErrors = true,
        bool assertOnWarnings = false
    )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result.Result);
#else
        if (result.Result is null)
            throw new ArgumentNullException(nameof(result));
#endif

        result.CompilationDiagnostics.InternalAssertOnDiagnostics(
            assertOnErrors,
            assertOnWarnings,
            message => throw new AssertionException(message),
            "There were errors in the compilation."
        );
        result.Result.Diagnostics.InternalAssertOnDiagnostics(
            assertOnErrors,
            assertOnWarnings,
            message => throw new AssertionException(message),
            "There were errors in the output generated by the source generator."
        );

        result.Result.InternalShouldProduce(
            filePathEndsWith,
            expectedSource,
            assertOnErrors,
            assertOnWarnings,
            message => throw new AssertionException(message)
        );
    }

    /// <summary>
    /// Verifies that the generated source from a <see cref="GeneratorDriverRunResult"/> with a specific file path using
    /// <see cref="Verifier.Verify(string?, string, VerifySettings?, string)"/>.
    /// </summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult"/> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <param name="verifySettings">The verify settings.</param>
    /// <param name="sourceFile">The source file.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="result"/> is null.</exception>
    public static SettingsTask VerifyAsync(
        this GeneratorDriverRunResult result,
        string filePathEndsWith,
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

        var generatedSource = result.InternalGetSource(filePathEndsWith);
        var source = generatedSource.HasValue ? generatedSource.Value.Source : "";
        
        // ReSharper disable once ExplicitCallerInfoArgument
        return Verify(source, "txt", verifySettings, sourceFile);
    }

    /// <summary>
    /// Verifies that the generated source from a <see cref="GeneratorDriverRunResult"/> with a specific file path using
    /// <see cref="Verifier.Verify(string?, string, VerifySettings?, string)"/>.
    /// </summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult"/> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <param name="assertOnErrors"><see langword="true"/> to assert on reported errors, <see langword="false"/> othwerwise. Defaults to <see langword="true"/>.</param>
    /// <param name="assertOnWarnings"><see langword="true"/> to assert on reported warnings, <see langword="false"/> othwerwise. Defaults to <see langword="false"/>.</param>
    /// <param name="verifySettings">The verify settings.</param>
    /// <param name="sourceFile">The source file.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="result"/> is null.</exception>
    public static SettingsTask VerifyAsync(
        this (ImmutableArray<Diagnostic> CompilationDiagnostics, GeneratorDriverRunResult Result) result,
        string filePathEndsWith,
        bool assertOnErrors = true,
        bool assertOnWarnings = false,
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

        result.CompilationDiagnostics.InternalAssertOnDiagnostics(
            assertOnErrors,
            assertOnWarnings,
            message => throw new AssertionException(message),
            "There were errors in the compilation."
        );
        result.Result.Diagnostics.InternalAssertOnDiagnostics(
            assertOnErrors,
            assertOnWarnings,
            message => throw new AssertionException(message),
            "There were errors in the output generated by the source generator."
        );

        var generatedSource = result.Result.InternalGetSource(filePathEndsWith);
        var source = generatedSource.HasValue ? generatedSource.Value.Source : "";

        // ReSharper disable once ExplicitCallerInfoArgument
        return Verify(source, "txt", verifySettings, sourceFile);
    }
}
