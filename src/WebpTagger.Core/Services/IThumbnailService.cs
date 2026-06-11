namespace WebpTagger.Core.Services;

/// <summary>Generates and caches PNG thumbnails for .webp files.</summary>
public interface IThumbnailService
{
    /// <summary>
    /// Returns a PNG-encoded thumbnail (bounded to <paramref name="maxSize"/>
    /// x <paramref name="maxSize"/>, aspect ratio preserved) for the given
    /// file. <paramref name="lastWriteTimeUtc"/> is used to validate cache
    /// entries - pass the value from <c>ImageItem.LastWriteTimeUtc</c>.
    /// </summary>
    Task<byte[]> GetThumbnailAsync(
        string filePath,
        DateTime lastWriteTimeUtc,
        int maxSize,
        CancellationToken cancellationToken = default);
}
