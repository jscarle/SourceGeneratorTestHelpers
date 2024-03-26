using System.Globalization;
using System.Text;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace SourceGeneratorTestHelpers.Common;

internal static class Diff
{
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
