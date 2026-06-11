using WebpTagger.Core.Models;

namespace WebpTagger.Core.Services;

/// <inheritdoc cref="IDirectoryScanner" />
public sealed class DirectoryScanner : IDirectoryScanner
{
    private readonly ITaggingEngine _taggingEngine;

    public DirectoryScanner(ITaggingEngine taggingEngine)
    {
        _taggingEngine = taggingEngine;
    }

    public async Task<IReadOnlyList<ImageItem>> ScanAsync(
        string directoryPath,
        IProgress<DirectoryScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(directoryPath))
        {
            return Array.Empty<ImageItem>();
        }

        var files = Directory.EnumerateFiles(directoryPath, "*.webp", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var items = new List<ImageItem>(files.Count);

        for (var i = 0; i < files.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var file = files[i];
            var lastWriteTimeUtc = File.GetLastWriteTimeUtc(file);

            List<string> tags;
            try
            {
                tags = await _taggingEngine.ReadTagsAsync(file, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // A corrupt or unreadable file shouldn't abort the whole scan;
                // surface it with no tags so the user can still see/fix it.
                tags = new List<string>();
            }

            items.Add(new ImageItem(file, lastWriteTimeUtc, tags));
            progress?.Report(new DirectoryScanProgress(i + 1, files.Count, Path.GetFileName(file)));
        }

        return items;
    }
}
