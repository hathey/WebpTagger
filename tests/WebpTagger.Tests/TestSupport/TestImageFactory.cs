using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebpTagger.Tests.TestSupport;

/// <summary>Creates minimal .webp files on disk for use in tests.</summary>
public static class TestImageFactory
{
    public static async Task<string> CreateWebpFileAsync(
        string directory,
        string fileName,
        WebpFileFormatType fileFormat = WebpFileFormatType.Lossless)
    {
        var path = Path.Combine(directory, fileName);

        using var image = new Image<Rgba32>(4, 4, new Rgba32(255, 0, 0, 255));
        var encoder = new WebpEncoder { FileFormat = fileFormat };
        await image.SaveAsync(path, encoder);

        return path;
    }
}
