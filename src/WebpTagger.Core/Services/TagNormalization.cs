namespace WebpTagger.Core.Services;

/// <summary>
/// Shared rules for cleaning up and (de)serializing tag lists. Tags are
/// trimmed, empty entries are dropped, and duplicates are removed
/// case-insensitively while preserving the first-seen casing and order.
/// </summary>
public static class TagNormalization
{
    /// <summary>The character used to separate tags inside the EXIF XPKeywords value.</summary>
    public const char Separator = ';';

    public static List<string> Normalize(IEnumerable<string> tags)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();

        foreach (var raw in tags)
        {
            var trimmed = raw?.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            if (seen.Add(trimmed))
            {
                result.Add(trimmed);
            }
        }

        return result;
    }

    /// <summary>Parses a semicolon-separated EXIF XPKeywords value into a normalized tag list.</summary>
    public static List<string> Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new List<string>();
        }

        return Normalize(raw.Split(Separator));
    }

    /// <summary>Joins a tag list into the semicolon-separated form stored in EXIF XPKeywords.</summary>
    public static string Join(IEnumerable<string> tags) => string.Join(Separator, Normalize(tags));
}
