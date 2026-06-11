using WebpTagger.Core.Models;
using WebpTagger.Core.Services;

namespace WebpTagger.Tests;

public class TagLibraryTests
{
    [Fact]
    public void Rebuild_ProducesSortedDeduplicatedCaseInsensitiveList()
    {
        var library = new TagLibrary();
        var items = new[]
        {
            new ImageItem("a.webp", DateTime.UtcNow, new[] { "Beach", "sunset" }),
            new ImageItem("b.webp", DateTime.UtcNow, new[] { "beach", "Travel" }),
        };

        library.Rebuild(items);

        Assert.Equal(new[] { "Beach", "sunset", "Travel" }, library.Tags);
    }

    [Fact]
    public void Rebuild_RaisesChangedEvent()
    {
        var library = new TagLibrary();
        var raised = false;
        library.Changed += (_, _) => raised = true;

        library.Rebuild(new[] { new ImageItem("a.webp", DateTime.UtcNow, new[] { "tag" }) });

        Assert.True(raised);
    }

    [Fact]
    public void Add_NewTag_ReturnsTrueRaisesChangedAndAppearsInTags()
    {
        var library = new TagLibrary();
        var raised = false;
        library.Changed += (_, _) => raised = true;

        var added = library.Add("Holiday");

        Assert.True(added);
        Assert.True(raised);
        Assert.Contains("Holiday", library.Tags);
    }

    [Fact]
    public void Add_ExistingTagDifferentCase_ReturnsFalseAndDoesNotRaiseChanged()
    {
        var library = new TagLibrary();
        library.Add("Holiday");

        var raised = false;
        library.Changed += (_, _) => raised = true;
        var added = library.Add("holiday");

        Assert.False(added);
        Assert.False(raised);
        Assert.Equal(new[] { "Holiday" }, library.Tags);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Add_BlankTag_ReturnsFalseAndDoesNotAdd(string tag)
    {
        var library = new TagLibrary();

        var added = library.Add(tag);

        Assert.False(added);
        Assert.Empty(library.Tags);
    }

    [Fact]
    public void Clear_RemovesAllTagsAndRaisesChanged()
    {
        var library = new TagLibrary();
        library.Add("tag1");

        var raised = false;
        library.Changed += (_, _) => raised = true;
        library.Clear();

        Assert.True(raised);
        Assert.Empty(library.Tags);
    }

    [Fact]
    public void Clear_WhenAlreadyEmpty_DoesNotRaiseChanged()
    {
        var library = new TagLibrary();

        var raised = false;
        library.Changed += (_, _) => raised = true;
        library.Clear();

        Assert.False(raised);
    }
}
