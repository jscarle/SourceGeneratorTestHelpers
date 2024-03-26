namespace SourceGeneratorTestHelpers;

/// <summary>Represents a generated source file.</summary>
public readonly struct GeneratedSource : IEquatable<GeneratedSource>
{
    internal GeneratedSource(string filePath, string source)
    {
        FilePath = filePath;
        Source = source;
    }

    /// <summary>Gets the path to the generated source file.</summary>
    public string FilePath { get; }

    /// <summary>Gets the source of the generated file.</summary>
    public string Source { get; }

    /// <inheritdoc/>
    public bool Equals(GeneratedSource other)
    {
        return FilePath == other.FilePath && Source == other.Source;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is GeneratedSource other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return FilePath.GetHashCode(
            #if NET6_0_OR_GREATER
            StringComparison.Ordinal
#endif
        );
    }

    /// <summary>Determines if two <see cref="GeneratedSource"/> instances are equal.</summary>
    /// <param name="left">The first <see cref="GeneratedSource"/> instance.</param>
    /// <param name="right">The second <see cref="GeneratedSource"/> instance.</param>
    /// <returns>True if the instances are equal, false otherwise.</returns>
    public static bool operator ==(GeneratedSource? left, GeneratedSource? right)
    {
        return Equals(left, right);
    }

    /// <summary>Determines if two <see cref="GeneratedSource"/> instances are not equal.</summary>
    /// <param name="left">The first <see cref="GeneratedSource"/> instance.</param>
    /// <param name="right">The second <see cref="GeneratedSource"/> instance.</param>
    /// <returns>True if the instances are not equal, false otherwise.</returns>
    public static bool operator !=(GeneratedSource? left, GeneratedSource? right)
    {
        return !Equals(left, right);
    }
}
