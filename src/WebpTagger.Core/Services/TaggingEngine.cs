using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace WebpTagger.Core.Services;

/// <summary>
/// Reads and writes tags stored in the EXIF "XPKeywords" field of a .webp
/// file. See docs/ARCHITECTURE.md for the storage format.
/// </summary>
public sealed class TaggingEngine : ITaggingEngine
{
    public async Task<List<string>> ReadTagsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var info = await Image.IdentifyAsync(filePath, cancellationToken).ConfigureAwait(false);

        var exif = info.Metadata.ExifProfile;
        if (exif is null)
        {
            return new List<string>();
        }

        if (exif.TryGetValue(ExifTag.XPKeywords, out var value) && value.Value is string raw)
        {
            return TagNormalization.Parse(raw);
        }

        return new List<string>();
    }

    public async Task SaveTagsAsync(string filePath, IReadOnlyList<string> tags, CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(filePath, cancellationToken).ConfigureAwait(false);

        var exif = image.Metadata.ExifProfile ?? new ExifProfile();
        image.Metadata.ExifProfile = exif;

        var normalized = TagNormalization.Normalize(tags);
        if (normalized.Count == 0)
        {
            exif.RemoveValue(ExifTag.XPKeywords);
        }
        else
        {
            exif.SetValue(ExifTag.XPKeywords, TagNormalization.Join(normalized));
        }

        // ImageSharp doesn't expose the original lossy "quality" used to encode
        // the file, only whether it was lossy or lossless. Preserve that choice;
        // a re-saved lossy file uses the encoder's default quality (75).
        var webpMetadata = image.Metadata.GetWebpMetadata();
        var encoder = new WebpEncoder
        {
            FileFormat = webpMetadata.FileFormat,
        };

        await image.SaveAsync(filePath, encoder, cancellationToken).ConfigureAwait(false);
    }
}
