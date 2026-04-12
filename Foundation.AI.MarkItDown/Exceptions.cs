namespace Foundation.AI.MarkItDown;

/// <summary>
/// Base exception for all MarkItDown conversion errors.
/// </summary>
public class MarkItDownException : Exception
{
    public MarkItDownException(string message) : base(message) { }
    public MarkItDownException(string message, Exception innerException) : base(message, innerException) { }
}


/// <summary>
/// Thrown when no registered converter can handle the input format.
/// </summary>
public class UnsupportedFormatException : MarkItDownException
{
    /// <summary>
    /// The MIME type that was attempted, if known.
    /// </summary>
    public string? MimeType { get; }

    /// <summary>
    /// The file extension that was attempted, if known.
    /// </summary>
    public string? Extension { get; }

    public UnsupportedFormatException(string message, string? mimeType = null, string? extension = null)
        : base(message)
    {
        MimeType = mimeType;
        Extension = extension;
    }
}


/// <summary>
/// Thrown when a converter accepted the input but failed during conversion.
/// </summary>
public class FileConversionException : MarkItDownException
{
    /// <summary>
    /// Name of the converter that attempted the conversion.
    /// </summary>
    public string ConverterName { get; }

    public FileConversionException(string message, string converterName, Exception innerException)
        : base(message, innerException)
    {
        ConverterName = converterName;
    }
}


/// <summary>
/// Thrown when a converter requires an optional dependency that is not available.
/// </summary>
public class MissingDependencyException : MarkItDownException
{
    /// <summary>
    /// The name of the missing dependency (NuGet package or service).
    /// </summary>
    public string DependencyName { get; }

    public MissingDependencyException(string message, string dependencyName)
        : base(message)
    {
        DependencyName = dependencyName;
    }
}
