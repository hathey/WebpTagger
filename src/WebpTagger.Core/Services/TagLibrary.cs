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

    /// <summary>
    /// Adds the union of tags from <paramref name="items"/> into the library. Tags already
    /// known (e.g. persisted from a prior session, or seen in a previously-opened folder)
    /// are kept, so switching folders only ever grows the known tag set.
    /// </summary>
    public void Rebuild(IEnumerable<ImageItem> items) => AddRange(items.SelectMany(item => item.Tags));

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

    /// <summary>Adds multiple tags at once, raising <see cref="Changed"/> at most once if anything was added.</summary>
    public void AddRange(IEnumerable<string> tags)
    {
        var changed = false;

        foreach (var tag in tags)
        {
            var trimmed = tag.Trim();
            if (!string.IsNullOrEmpty(trimmed) && _tags.Add(trimmed))
            {
                changed = true;
            }
        }

        if (changed)
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Removes a tag from the library only (e.g. via the tag-list editor). Does not
    /// affect any image's own tags. Returns true if the tag was known.
    /// </summary>
    public bool Remove(string tag)
    {
        if (_tags.Remove(tag))
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
