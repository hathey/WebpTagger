using System.Text.Json;

namespace WebpTagger.UI;

internal static class AppSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WebpTagger",
        "settings.json");

    private static readonly SettingsData Data = Load();

    public static string? LastFolder => Data.LastFolder;

    public static int? ZoomSize => Data.ZoomSize;

    public static IReadOnlyList<string> KnownTags => Data.KnownTags ?? (IReadOnlyList<string>)Array.Empty<string>();

    public static void SaveLastFolder(string folderPath)
    {
        Data.LastFolder = folderPath;
        Save();
    }

    public static void SaveZoomSize(int zoomSize)
    {
        Data.ZoomSize = zoomSize;
        Save();
    }

    public static void SaveKnownTags(IEnumerable<string> tags)
    {
        Data.KnownTags = tags.ToList();
        Save();
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(Data));
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static SettingsData Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new SettingsData();
            }

            return JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(FilePath)) ?? new SettingsData();
        }
        catch (IOException)
        {
            return new SettingsData();
        }
        catch (UnauthorizedAccessException)
        {
            return new SettingsData();
        }
        catch (JsonException)
        {
            return new SettingsData();
        }
    }

    private sealed class SettingsData
    {
        public string? LastFolder { get; set; }
        public int? ZoomSize { get; set; }
        public List<string>? KnownTags { get; set; }
    }
}
