using WebpTagger.Core.Models;

namespace WebpTagger.Core.Services;

/// <summary>
/// Maintains the set of distinct tags seen across every <see cref="ImageItem"/>
/// in the currently-open folder, sorted and de-duplicated case-insensitively.
/// </summary>
public sealed class TagLibrary
{
    private readonly SortedSet<string> _tags = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Raised whenever the tag set changes.</summary>
    public event EventHandler? Changed;

    /// <summary>All known tags, sorted case-insensitively.</summary>
    public IReadOnlyList<string> Tags => _tags.ToList();

    /// <summary>Replaces the tag set with the union of tags from <paramref name="items"/>.</summary>
    public void Rebuild(IEnumerable<ImageItem> items)
    {
        _tags.Clear();

        foreach (var item in items)
        {
            foreach (var tag in item.Tags)
            {
                _tags.Add(tag);
            }
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Adds a tag to the library (e.g. when the user types a brand-new tag),
    /// even if no image currently uses it. Returns true if the tag was new.
    /// </summary>
    public bool Add(string tag)
    {
        var trimmed = tag.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        if (_tags.Add(trimmed))
        {
            Changed?.Invoke(this, EventArgs.Empty);
            return true;
        }

        return false;
    }

    /// <summary>Clears all tags (e.g. when the open folder changes to one with no images).</summary>
    public void Clear()
    {
        if (_tags.Count == 0)
        {
            return;
        }

        _tags.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
