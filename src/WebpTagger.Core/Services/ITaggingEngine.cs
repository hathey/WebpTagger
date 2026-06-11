namespace WebpTagger.Core.Services;

/// <summary>
/// Reads and writes the tag list stored in a .webp file's EXIF metadata
/// (the Windows "XPKeywords" field).
/// </summary>
public interface ITaggingEngine
{
    /// <summary>Reads the normalized tag list from a .webp file's EXIF metadata.</summary>
    Task<List<string>> ReadTagsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the given tag list back into the .webp file's EXIF metadata,
    /// re-encoding the file. An empty <paramref name="tags"/> list removes
    /// the XPKeywords entry entirely.
    /// </summary>
    Task SaveTagsAsync(string filePath, IReadOnlyList<string> tags, CancellationToken cancellationToken = default);
}
