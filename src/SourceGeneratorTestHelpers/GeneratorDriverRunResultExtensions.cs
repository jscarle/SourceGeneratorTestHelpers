﻿using System.Globalization;
using System.Text;
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

        var sources = result.GeneratedTrees.Select(tree =>
        {
            var filePath = tree.FilePath;
            var source = tree.GetText().ToString();

            return new GeneratedSource(filePath, source);
        });

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


    /// <summary>Gets the generated source from a <see cref="GeneratorDriverRunResult" /> with a specific file path ending.</summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult" /> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <param name="assertOnErrors"><see langword="true" /> to assert on reported errors by the source generator, <see langword="false" /> othwerwise.</param>
    /// <param name="assertAction">The action to perform when an assertion fails. This typically involves throwing an exception with a descriptive error message.</param>
    /// <returns>The source of the generated file, or null if no matching file is found.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="result" /> is null.</exception>
    internal static string? InternalGetSource(this GeneratorDriverRunResult result, string filePathEndsWith, bool assertOnErrors, Action<string> assertAction)
    {
        if (assertOnErrors)
        {
            var errors = result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();

            if (errors.Count > 0)
            {
                var errorBuilder = new StringBuilder();
                errorBuilder.Append("There were errors in the expected output generated by the source generator.\n\n");

                foreach (var error in errors)
                {
                    var message = error.GetMessage(CultureInfo.CurrentCulture);

                    errorBuilder.Append(
#if NET6_0_OR_GREATER
                        CultureInfo.InvariantCulture,
#endif
                        $"    - {message}\n");
                }

                assertAction(errorBuilder.ToString());
            }
        }

        var generatedSource = GetSource(result, filePathEndsWith);

        return generatedSource;
    }

    /// <summary>Verifies that the generated source from a <see cref="GeneratorDriverRunResult" /> with a specific file path ending matches the expected source.</summary>
    /// <param name="result">The <see cref="GeneratorDriverRunResult" /> to get the source from.</param>
    /// <param name="filePathEndsWith">The string that the generated source's file path should end with.</param>
    /// <param name="expectedSource">The expected source that the generated source should match.</param>
    /// <param name="assertOnErrors"><see langword="true" /> to assert on reported errors by the source generator, <see langword="false" /> othwerwise.</param>
    /// <param name="assertAction">The action to perform when an assertion fails. This typically involves throwing an exception with a descriptive error message.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="result" /> is null.</exception>
    internal static void InternalShouldProduce(this GeneratorDriverRunResult result, string filePathEndsWith, string expectedSource, bool assertOnErrors, Action<string> assertAction)
    {
        var generatedSource = InternalGetSource(result, filePathEndsWith, assertOnErrors, assertAction);

        var (hasDifferences, differences) = Diff.Compare(expectedSource ?? "", generatedSource ?? "");

        if (hasDifferences)
            assertAction($"There was a difference in the expected output generated by the source generator.\n\n{differences}");
    }
}
