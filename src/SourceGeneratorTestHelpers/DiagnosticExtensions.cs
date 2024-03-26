using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SourceGeneratorTestHelpers;

/// <summary>Extension methods for working with <see cref="Diagnostic"/>.</summary>
internal static class DiagnosticExtensions
{
    /// <summary>Gets a formatted exception message from a collection of <see cref="Diagnostic"/> instances.</summary>
    /// <param name="diagnostics">The collection of diagnostics.</param>
    /// <returns>A formatted exception message.</returns>
    public static string GetExceptionMessage(this ImmutableArray<Diagnostic> diagnostics)
    {
        var separator = $"{Environment.NewLine} - ";

        var message = $"Compilation failed:{separator}{string.Join(separator, diagnostics.Select(x => x.ToString()))}{Environment.NewLine}";

        return message;
    }
}
