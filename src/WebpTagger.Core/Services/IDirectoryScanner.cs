using WebpTagger.Core.Models;

namespace WebpTagger.Core.Services;

/// <summary>Progress reported while scanning a directory for .webp files.</summary>
public readonly record struct DirectoryScanProgress(int Completed, int Total, string CurrentFileName);

/// <summary>Discovers .webp files in a directory and reads their tags.</summary>
public interface IDirectoryScanner
{
    /// <summary>
    /// Scans the top-level <paramref name="directoryPath"/> for .webp files
    /// and returns one <see cref="ImageItem"/> per file, with tags already
    /// loaded from EXIF metadata. Returns an empty list if the directory
    /// does not exist.
    /// </summary>
    Task<IReadOnlyList<ImageItem>> ScanAsync(
        string directoryPath,
        IProgress<DirectoryScanProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
