using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SourceGeneratorTestHelpers.Common;

internal static class SourceSnapshot
{
    private static readonly UTF8Encoding SnapshotEncoding = new(false, true);

    internal static (string VerifiedPath, string ReceivedPath) GetSnapshotPaths(string filePathEndsWith, string sourceFile, string snapshotName, Assembly? testAssembly)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePathEndsWith);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);

        if (!Path.IsPathFullyQualified(sourceFile))
            throw new ArgumentException("The calling test source file path must be fully qualified.", nameof(sourceFile));

        var sourceDirectory = Path.GetDirectoryName(sourceFile);
        var sourceFileName = Path.GetFileNameWithoutExtension(sourceFile);
        if (string.IsNullOrWhiteSpace(sourceDirectory) || string.IsNullOrWhiteSpace(sourceFileName))
            throw new ArgumentException("The calling test source file path must include a directory and file name.", nameof(sourceFile));

        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotName);

        // Use portable file names so a snapshot has the same identity on Windows and Linux.
        foreach (var character in snapshotName)
        {
            if (character < ' ' || "<>:\"/\\|?*".Contains(character, StringComparison.Ordinal))
                throw new ArgumentException("The snapshot name must be a file name without directory separators or invalid characters.", nameof(snapshotName));
        }

        var snapshotPath = Path.Combine(sourceDirectory, snapshotName);
        var receivedPath = snapshotPath;
        var frameworkMetadata = testAssembly?.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => string.Equals(attribute.Key, "SourceGeneratorTestHelpers.TargetFrameworks", StringComparison.Ordinal));
        if (frameworkMetadata?.Value?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length > 1)
        {
            // Verify scopes received files by its selected package target; approved snapshots stay shared.
            var frameworkAttribute = typeof(SourceSnapshot).Assembly.GetCustomAttribute<TargetFrameworkAttribute>()!;
            var framework = new FrameworkName(frameworkAttribute.FrameworkName);
            receivedPath += $".DotNet{framework.Version.Major}_{framework.Version.Minor}";
        }

        return (snapshotPath + ".verified.txt", receivedPath + ".received.txt");
    }

    internal static async Task VerifyAsync(
        GeneratorDriverRunResult result,
        string filePathEndsWith,
        (string VerifiedPath, string ReceivedPath) snapshotPaths,
        Action<string> assertAction,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var generatedSource = result.InternalGetSource(filePathEndsWith);
        if (!generatedSource.HasValue)
        {
            assertAction($"No generated source file ended with '{filePathEndsWith}'.");
            return;
        }

        var source = generatedSource.Value.Source.ReplaceLineEndings("\n");
        var (verifiedPath, receivedPath) = snapshotPaths;

        string? expectedSource = null;
        try
        {
            var verifiedSource = await File.ReadAllTextAsync(verifiedPath, SnapshotEncoding, cancellationToken).ConfigureAwait(false);
            expectedSource = verifiedSource.ReplaceLineEndings("\n");
        }
        catch (FileNotFoundException)
        {
            // Only a missing approved file is a snapshot failure; other file-access failures propagate.
        }

        if (string.Equals(expectedSource, source, StringComparison.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Delete(receivedPath);
            return;
        }

        var (_, differences) = Diff.Compare(expectedSource ?? "", source, normalizeLineEndings: false);
        if (expectedSource is not null && string.IsNullOrEmpty(differences))
            differences = $"The text differs in whitespace or trailing newlines (expected {expectedSource.Length} characters, received {source.Length} characters).";

        await File.WriteAllTextAsync(receivedPath, source, SnapshotEncoding, cancellationToken).ConfigureAwait(false);

        var reason = expectedSource is null
            ? "The approved snapshot does not exist."
            : "The generated source does not match the approved snapshot.";

        assertAction($"{reason}\n\nApproved: {verifiedPath}\nReceived: {receivedPath}\n\n{differences}\nReview the received file and replace the approved file to accept this snapshot.");
    }
}
