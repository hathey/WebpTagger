namespace WebpTagger.Core.Diagnostics;

/// <summary>
/// Best-effort diagnostic log for failures that are otherwise swallowed (e.g. per-file
/// tag/thumbnail read failures) so they can be inspected from the running app instead of
/// requiring a debugger or remote access to the affected machine.
/// </summary>
public static class DiagnosticsLog
{
    public static string FilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WebpTagger",
        "diagnostics.log");

    private static readonly object SyncRoot = new();

    public static void Write(string message)
    {
        try
        {
            lock (SyncRoot)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.AppendAllText(FilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}");
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    public static string ReadAll()
    {
        try
        {
            lock (SyncRoot)
            {
                return File.Exists(FilePath) ? File.ReadAllText(FilePath) : string.Empty;
            }
        }
        catch (IOException)
        {
            return string.Empty;
        }
        catch (UnauthorizedAccessException)
        {
            return string.Empty;
        }
    }

    public static void Clear()
    {
        try
        {
            lock (SyncRoot)
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
