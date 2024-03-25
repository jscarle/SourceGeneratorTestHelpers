using System.Globalization;
using System.Text;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace SourceGeneratorTestHelpers;

/// <summary>Provides utility methods for comparing text and generating diffs.</summary>
public static class Diff
{
    /// <summary>Compares two strings and generates a diff report indicating the differences between them.</summary>
    /// <param name="string1">The first string to compare.</param>
    /// <param name="string2">The second string to compare.</param>
    /// <param name="normalizeLineEndings">Optional flag indicating whether to normalize line endings to '\n' before comparison. Defaults to true.</param>
    /// <param name="ignoreWhitespace">Optional flag indicating whether to ignore whitespace differences. Defaults to false.</param>
    /// <returns>A tuple containing a boolean indicating whether differences exist and a string representing the diff report.</returns>
    public static (bool HasDifferences, string Differences) Compare(
        string? string1,
        string? string2,
        bool normalizeLineEndings = true,
        bool ignoreWhitespace = false
    )
    {
        // Pad the left margin to align line numbers.
        const int leftMargin = 7;

        var oldText = NormalizeLineEndings(string1, normalizeLineEndings);
        var newText = NormalizeLineEndings(string2, normalizeLineEndings);

        var differ = new Differ();
        var inlineBuilder = new InlineDiffBuilder(differ);
        var diffResult = inlineBuilder.BuildDiffModel(oldText, newText, ignoreWhitespace);

        var diff = new StringBuilder();

        foreach (var line in diffResult.Lines)
        {
            if (line.Type == ChangeType.Unchanged)
                continue;

            var position = line.Position?.ToString(CultureInfo.InvariantCulture);
            var prefix = position != null ? $"L{position} " : "";
            var lineText = line.Text;

            switch (line.Type)
            {
                case ChangeType.Inserted:
                    diff.AppendLine(
#if NET6_0_OR_GREATER
                        CultureInfo.InvariantCulture,
#endif
                        $"{prefix,leftMargin}+ {lineText}"
                        );

                    break;
                case ChangeType.Deleted:
                    diff.AppendLine(
#if NET6_0_OR_GREATER
                        CultureInfo.InvariantCulture,
#endif
                        $"{prefix,leftMargin}- {lineText}"
                        );

                    break;
            }
        }

        return (diffResult.HasDifferences, diff.ToString());
    }

    private static string? NormalizeLineEndings(string? str, bool normalize)
    {
        return normalize
            ? str?.Replace(
                "\r\n",
                "\n"
#if NET6_0_OR_GREATER
            , StringComparison.InvariantCulture
#endif
                )
            : str;
    }
}
