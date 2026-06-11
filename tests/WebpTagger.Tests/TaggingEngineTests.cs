using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using WebpTagger.Core.Services;
using WebpTagger.Tests.TestSupport;

namespace WebpTagger.Tests;

public class TaggingEngineTests
{
    private readonly TaggingEngine _engine = new();

    [Fact]
    public async Task ReadTagsAsync_FileWithNoTags_ReturnsEmptyList()
    {
        using var dir = new TempDirectory();
        var path = await TestImageFactory.CreateWebpFileAsync(dir.Path, "no-tags.webp");

        var tags = await _engine.ReadTagsAsync(path);

        Assert.Empty(tags);
    }

    [Fact]
    public async Task SaveTagsAsync_ThenRead_RoundTripsSingleTag()
    {
        using var dir = new TempDirectory();
        var path = await TestImageFactory.CreateWebpFileAsync(dir.Path, "single-tag.webp");

        await _engine.SaveTagsAsync(path, new[] { "vacation" });
        var tags = await _engine.ReadTagsAsync(path);

        Assert.Equal(new[] { "vacation" }, tags);
    }

    [Fact]
    public async Task SaveTagsAsync_ThenRead_RoundTripsMultipleTags()
    {
        using var dir = new TempDirectory();
        var path = await TestImageFactory.CreateWebpFileAsync(dir.Path, "multi-tag.webp");

        await _engine.SaveTagsAsync(path, new[] { "beach", "sunset", "family" });
        var tags = await _engine.ReadTagsAsync(path);

        Assert.Equal(new[] { "beach", "sunset", "family" }, tags);
    }

    [Fact]
    public async Task SaveTagsAsync_NormalizesWhitespaceAndDuplicates()
    {
        using var dir = new TempDirectory();
        var path = await TestImageFactory.CreateWebpFileAsync(dir.Path, "messy-tags.webp");

        await _engine.SaveTagsAsync(path, new[] { "  Travel ", "travel", "Food", "   ", "Food" });
        var tags = await _engine.ReadTagsAsync(path);

        Assert.Equal(new[] { "Travel", "Food" }, tags);
    }

    [Fact]
    public async Task SaveTagsAsync_EmptyList_RemovesExistingTags()
    {
        using var dir = new TempDirectory();
        var path = await TestImageFactory.CreateWebpFileAsync(dir.Path, "cleared-tags.webp");

        await _engine.SaveTagsAsync(path, new[] { "temporary" });
        await _engine.SaveTagsAsync(path, Array.Empty<string>());

        var tags = await _engine.ReadTagsAsync(path);

        Assert.Empty(tags);
    }

    [Fact]
    public async Task SaveTagsAsync_PreservesLossyFileFormat()
    {
        using var dir = new TempDirectory();
        var path = await TestImageFactory.CreateWebpFileAsync(dir.Path, "lossy.webp", WebpFileFormatType.Lossy);

        await _engine.SaveTagsAsync(path, new[] { "edited" });

        var info = await Image.IdentifyAsync(path);
        Assert.Equal(WebpFileFormatType.Lossy, info.Metadata.GetWebpMetadata().FileFormat);
    }

    [Fact]
    public async Task SaveTagsAsync_PreservesLosslessFileFormat()
    {
        using var dir = new TempDirectory();
        var path = await TestImageFactory.CreateWebpFileAsync(dir.Path, "lossless.webp", WebpFileFormatType.Lossless);

        await _engine.SaveTagsAsync(path, new[] { "edited" });

        var info = await Image.IdentifyAsync(path);
        Assert.Equal(WebpFileFormatType.Lossless, info.Metadata.GetWebpMetadata().FileFormat);
    }
}
