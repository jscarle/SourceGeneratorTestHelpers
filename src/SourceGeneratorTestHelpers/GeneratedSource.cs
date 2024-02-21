namespace SourceGeneratorTestHelpers;

/// <summary>Represents a generated source file.</summary>
public sealed class GeneratedSource : IEquatable<GeneratedSource>
{
    /// <summary>Gets the path to the generated source file.</summary>
    public string FilePath { get; }

    /// <summary>Gets the source code of the generated file.</summary>
    public string Source { get; }

    /// <summary>Initializes a new instance of the <see cref="GeneratedSource" /> class.</summary>
    /// <param name="filePath">The path to the generated source file.</param>
    /// <param name="source">The source code of the generated file.</param>
    public GeneratedSource(string filePath, string source)
    {
        FilePath = filePath;
        Source = source;
    }

    /// <inheritdoc />
    public bool Equals(GeneratedSource? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return FilePath == other.FilePath;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is GeneratedSource other && Equals(other));
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return FilePath.GetHashCode(
#if NET6_0_OR_GREATER
            StringComparison.Ordinal
#endif
            );
    }

    /// <summary>Determines if two <see cref="GeneratedSource" /> instances are equal.</summary>
    /// <param name="left">The first <see cref="GeneratedSource" /> instance.</param>
    /// <param name="right">The second <see cref="GeneratedSource" /> instance.</param>
    /// <returns>True if the instances are equal, false otherwise.</returns>
    public static bool operator ==(GeneratedSource? left, GeneratedSource? right)
    {
        return Equals(left, right);
    }

    /// <summary>Determines if two <see cref="GeneratedSource" /> instances are not equal.</summary>
    /// <param name="left">The first <see cref="GeneratedSource" /> instance.</param>
    /// <param name="right">The second <see cref="GeneratedSource" /> instance.</param>
    /// <returns>True if the instances are not equal, false otherwise.</returns>
    public static bool operator !=(GeneratedSource? left, GeneratedSource? right)
    {
        return !Equals(left, right);
    }
}