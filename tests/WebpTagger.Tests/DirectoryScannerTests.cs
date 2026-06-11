using WebpTagger.Core.Services;
using WebpTagger.Tests.TestSupport;

namespace WebpTagger.Tests;

public class DirectoryScannerTests
{
    private readonly TaggingEngine _taggingEngine = new();

    [Fact]
    public async Task ScanAsync_NonExistentDirectory_ReturnsEmpty()
    {
        var scanner = new DirectoryScanner(_taggingEngine);
        var missingPath = Path.Combine(Path.GetTempPath(), "WebpTaggerTests_missing_" + Guid.NewGuid().ToString("N"));

        var items = await scanner.ScanAsync(missingPath);

        Assert.Empty(items);
    }

    [Fact]
    public async Task ScanAsync_EmptyDirectory_ReturnsEmpty()
    {
        using var dir = new TempDirectory();
        var scanner = new DirectoryScanner(_taggingEngine);

        var items = await scanner.ScanAsync(dir.Path);

        Assert.Empty(items);
    }

    [Fact]
    public async Task ScanAsync_OnlyReturnsWebpFilesSortedByName()
    {
        using var dir = new TempDirectory();
        await TestImageFactory.CreateWebpFileAsync(dir.Path, "b.webp");
        await TestImageFactory.CreateWebpFileAsync(dir.Path, "a.webp");
        await File.WriteAllTextAsync(Path.Combine(dir.Path, "notes.txt"), "not an image");

        var scanner = new DirectoryScanner(_taggingEngine);

        var items = await scanner.ScanAsync(dir.Path);

        Assert.Equal(new[] { "a.webp", "b.webp" }, items.Select(i => i.FileName));
    }

    [Fact]
    public async Task ScanAsync_ReadsTagsFromFiles()
    {
        using var dir = new TempDirectory();
        var path = await TestImageFactory.CreateWebpFileAsync(dir.Path, "tagged.webp");
        await _taggingEngine.SaveTagsAsync(path, new[] { "alpha", "beta" });

        var scanner = new DirectoryScanner(_taggingEngine);
        var items = await scanner.ScanAsync(dir.Path);

        var item = Assert.Single(items);
        Assert.Equal(new[] { "alpha", "beta" }, item.Tags);
    }

    [Fact]
    public async Task ScanAsync_ReportsProgressForEachFile()
    {
        using var dir = new TempDirectory();
        await TestImageFactory.CreateWebpFileAsync(dir.Path, "a.webp");
        await TestImageFactory.CreateWebpFileAsync(dir.Path, "b.webp");

        var scanner = new DirectoryScanner(_taggingEngine);
        var reports = new List<DirectoryScanProgress>();
        var progress = new SynchronousProgress<DirectoryScanProgress>(reports.Add);

        await scanner.ScanAsync(dir.Path, progress);

        Assert.Equal(2, reports.Count);
        Assert.Equal(2, reports[^1].Total);
        Assert.Equal(2, reports[^1].Completed);
    }

    /// <summary>
    /// An <see cref="IProgress{T}"/> that invokes its callback inline, unlike
    /// <see cref="Progress{T}"/> which marshals through a captured
    /// <see cref="SynchronizationContext"/> and may run on another thread.
    /// </summary>
    private sealed class SynchronousProgress<T>(Action<T> callback) : IProgress<T>
    {
        public void Report(T value) => callback(value);
    }
}
