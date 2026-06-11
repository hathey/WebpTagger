using System.IO;

namespace WebpTagger.Core.Models;

/// <summary>
/// Represents a single .webp file in the currently-open folder, including
/// its in-memory tag state.
/// </summary>
public sealed class ImageItem
{
    public ImageItem(string filePath, DateTime lastWriteTimeUtc, IEnumerable<string>? tags = null)
    {
        FilePath = filePath;
        LastWriteTimeUtc = lastWriteTimeUtc;
        Tags = tags is null ? new List<string>() : new List<string>(tags);
    }

    /// <summary>Full path to the .webp file on disk.</summary>
    public string FilePath { get; }

    /// <summary>File name including extension, derived from <see cref="FilePath"/>.</summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>The last-write timestamp recorded when this item was loaded or last saved.</summary>
    public DateTime LastWriteTimeUtc { get; set; }

    /// <summary>Current in-memory set of tags for this image (order preserved).</summary>
    public List<string> Tags { get; }

    /// <summary>True if <see cref="Tags"/> has been modified since the last save.</summary>
    public bool IsDirty { get; set; }
}
