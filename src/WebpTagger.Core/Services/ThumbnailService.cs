using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using WebpTagger.Core.Diagnostics;

namespace WebpTagger.Core.Services;

/// <inheritdoc cref="IThumbnailService" />
public sealed class ThumbnailService : IThumbnailService
{
    private readonly ConcurrentDictionary<string, (DateTime LastWriteTimeUtc, byte[] Png)> _memoryCache = new();
    private readonly string _diskCacheDirectory;

    public ThumbnailService(string? diskCacheDirectory = null)
    {
        _diskCacheDirectory = diskCacheDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WebpTagger",
            "ThumbnailCache");
    }

    public async Task<byte[]> GetThumbnailAsync(
        string filePath,
        DateTime lastWriteTimeUtc,
        int maxSize,
        CancellationToken cancellationToken = default)
    {
        var key = $"{filePath.ToLowerInvariant()}|{maxSize}";

        if (_memoryCache.TryGetValue(key, out var memoryEntry) && memoryEntry.LastWriteTimeUtc == lastWriteTimeUtc)
        {
            return memoryEntry.Png;
        }

        var diskPath = GetDiskCachePath(filePath, maxSize);
        if (File.Exists(diskPath) && File.GetLastWriteTimeUtc(diskPath) >= lastWriteTimeUtc)
        {
            var cached = await File.ReadAllBytesAsync(diskPath, cancellationToken).ConfigureAwait(false);
            _memoryCache[key] = (lastWriteTimeUtc, cached);
            return cached;
        }

        var png = await GenerateThumbnailAsync(filePath, maxSize, cancellationToken).ConfigureAwait(false);
        _memoryCache[key] = (lastWriteTimeUtc, png);

        try
        {
            Directory.CreateDirectory(_diskCacheDirectory);
            await File.WriteAllBytesAsync(diskPath, png, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The disk cache is a best-effort optimization; the in-memory
            // cache entry above is sufficient for the current session.
            DiagnosticsLog.Write($"Thumbnail disk cache write failed for \"{filePath}\": {ex.GetType().Name}: {ex.Message}");
        }

        return png;
    }

    private static async Task<byte[]> GenerateThumbnailAsync(string filePath, int maxSize, CancellationToken cancellationToken)
    {
        using var image = await Image.LoadAsync(filePath, cancellationToken).ConfigureAwait(false);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(maxSize, maxSize),
        }));

        using var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        return memoryStream.ToArray();
    }

    private string GetDiskCachePath(string filePath, int maxSize)
    {
        var hashInput = $"{filePath.ToLowerInvariant()}|{maxSize}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hashInput)));
        return Path.Combine(_diskCacheDirectory, $"{hash}.png");
    }
}
