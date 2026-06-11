using WebpTagger.Core.Models;
using WebpTagger.Core.Services;

namespace WebpTagger.UI;

public partial class MainForm : Form
{
    private const int ThumbnailSize = 128;

    private readonly ITaggingEngine _taggingEngine;
    private readonly IDirectoryScanner _directoryScanner;
    private readonly IThumbnailService _thumbnailService;
    private readonly TagLibrary _tagLibrary;

    private readonly Dictionary<string, ListViewItem> _listViewItemsByPath = new(StringComparer.OrdinalIgnoreCase);

    private string? _currentFolder;
    private CancellationTokenSource? _loadCts;

    public MainForm()
    {
        InitializeComponent();

        _taggingEngine = new TaggingEngine();
        _directoryScanner = new DirectoryScanner(_taggingEngine);
        _thumbnailService = new ThumbnailService();
        _tagLibrary = new TagLibrary();

        btnOpenFolder.Click += BtnOpenFolder_Click;
        btnSaveAll.Click += async (_, _) => await SaveAllAsync();
        listViewImages.SelectedIndexChanged += ListViewImages_SelectedIndexChanged;

        tagEditorPanel.TagAdded += TagEditorPanel_TagAdded;
        tagEditorPanel.TagRemoved += TagEditorPanel_TagRemoved;
        tagListSidebar.ApplyTagRequested += TagListSidebar_ApplyTagRequested;
        _tagLibrary.Changed += TagLibrary_Changed;

        FormClosing += MainForm_FormClosing;

        UpdateSaveButtonState();
    }

    private async void BtnOpenFolder_Click(object? sender, EventArgs e)
    {
        if (!await ConfirmDiscardChangesAsync())
        {
            return;
        }

        using var dialog = new FolderBrowserDialog
        {
            Description = "Select a folder containing .webp images",
            UseDescriptionForTitle = true,
        };

        if (!string.IsNullOrEmpty(_currentFolder))
        {
            dialog.SelectedPath = _currentFolder;
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await LoadFolderAsync(dialog.SelectedPath);
    }

    private async Task LoadFolderAsync(string folderPath)
    {
        _loadCts?.Cancel();
        var cts = new CancellationTokenSource();
        _loadCts = cts;

        _currentFolder = folderPath;
        lblFolderPath.Text = folderPath;

        listViewImages.Items.Clear();
        imageListThumbnails.Images.Clear();
        _listViewItemsByPath.Clear();
        tagEditorPanel.SetSelection(Array.Empty<ImageItem>());
        _tagLibrary.Clear();

        btnOpenFolder.Enabled = false;
        statusProgressBar.Visible = true;
        statusProgressBar.Value = 0;
        statusLabel.Text = "Scanning folder...";

        var progress = new Progress<DirectoryScanProgress>(p =>
        {
            statusProgressBar.Maximum = Math.Max(p.Total, 1);
            statusProgressBar.Value = Math.Min(p.Completed, statusProgressBar.Maximum);
            statusLabel.Text = $"Scanning {p.CurrentFileName} ({p.Completed}/{p.Total})";
        });

        IReadOnlyList<ImageItem> items;
        try
        {
            items = await _directoryScanner.ScanAsync(folderPath, progress, cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        finally
        {
            btnOpenFolder.Enabled = true;
            statusProgressBar.Visible = false;
        }

        if (cts.Token.IsCancellationRequested)
        {
            return;
        }

        listViewImages.BeginUpdate();
        foreach (var item in items)
        {
            var listViewItem = new ListViewItem(item.FileName) { Tag = item };
            _listViewItemsByPath[item.FilePath] = listViewItem;
            listViewImages.Items.Add(listViewItem);
        }

        listViewImages.EndUpdate();

        _tagLibrary.Rebuild(items);
        statusLabel.Text = $"{items.Count} image(s) loaded from {folderPath}";

        _ = LoadThumbnailsAsync(items, cts.Token);
    }

    private async Task LoadThumbnailsAsync(IReadOnlyList<ImageItem> items, CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            byte[] png;
            try
            {
                png = await _thumbnailService.GetThumbnailAsync(item.FilePath, item.LastWriteTimeUtc, ThumbnailSize, cancellationToken);
            }
            catch
            {
                // Skip files that can't be decoded (e.g. corrupt image); they
                // still show up in the grid without a thumbnail.
                continue;
            }

            if (cancellationToken.IsCancellationRequested || IsDisposed)
            {
                return;
            }

            SetThumbnail(item, png);
        }
    }

    private void SetThumbnail(ImageItem item, byte[] png)
    {
        using var memoryStream = new MemoryStream(png);
        using var loaded = Image.FromStream(memoryStream);
        var bitmap = new Bitmap(loaded);

        if (imageListThumbnails.Images.ContainsKey(item.FilePath))
        {
            imageListThumbnails.Images.RemoveByKey(item.FilePath);
        }

        imageListThumbnails.Images.Add(item.FilePath, bitmap);

        if (_listViewItemsByPath.TryGetValue(item.FilePath, out var listViewItem))
        {
            listViewItem.ImageKey = item.FilePath;
        }
    }

    private void ListViewImages_SelectedIndexChanged(object? sender, EventArgs e)
    {
        tagEditorPanel.SetSelection(GetSelectedItems());
    }

    private void TagEditorPanel_TagAdded(object? sender, string tag)
    {
        ApplyTagToSelection(tag);
        tagEditorPanel.SetSelection(GetSelectedItems());
    }

    private void TagEditorPanel_TagRemoved(object? sender, string tag)
    {
        var selected = GetSelectedItems();
        if (selected.Count == 0)
        {
            return;
        }

        foreach (var item in selected)
        {
            if (item.Tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                item.IsDirty = true;
                UpdateListViewItemText(item);
            }
        }

        tagEditorPanel.SetSelection(GetSelectedItems());
        UpdateSaveButtonState();
    }

    private void TagListSidebar_ApplyTagRequested(object? sender, string tag)
    {
        ApplyTagToSelection(tag);
        tagEditorPanel.SetSelection(GetSelectedItems());
    }

    private void TagLibrary_Changed(object? sender, EventArgs e)
    {
        tagListSidebar.SetTags(_tagLibrary.Tags);
        tagEditorPanel.SetSuggestions(_tagLibrary.Tags);
    }

    private void ApplyTagToSelection(string tag)
    {
        var selected = GetSelectedItems();
        if (selected.Count == 0)
        {
            return;
        }

        _tagLibrary.Add(tag);

        foreach (var item in selected)
        {
            if (!item.Tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)))
            {
                item.Tags.Add(tag);
                item.IsDirty = true;
                UpdateListViewItemText(item);
            }
        }

        UpdateSaveButtonState();
    }

    private List<ImageItem> GetSelectedItems() =>
        listViewImages.SelectedItems
            .Cast<ListViewItem>()
            .Select(listViewItem => (ImageItem)listViewItem.Tag!)
            .ToList();

    private void UpdateListViewItemText(ImageItem item)
    {
        if (_listViewItemsByPath.TryGetValue(item.FilePath, out var listViewItem))
        {
            listViewItem.Text = item.IsDirty ? $"{item.FileName} *" : item.FileName;
        }
    }

    private void UpdateSaveButtonState()
    {
        btnSaveAll.Enabled = _listViewItemsByPath.Values.Any(lvi => ((ImageItem)lvi.Tag!).IsDirty);
    }

    private async Task SaveAllAsync()
    {
        var dirtyItems = _listViewItemsByPath.Values
            .Select(lvi => (ImageItem)lvi.Tag!)
            .Where(item => item.IsDirty)
            .ToList();

        if (dirtyItems.Count == 0)
        {
            return;
        }

        btnOpenFolder.Enabled = false;
        btnSaveAll.Enabled = false;
        statusProgressBar.Visible = true;
        statusProgressBar.Maximum = dirtyItems.Count;
        statusProgressBar.Value = 0;

        var failures = new List<string>();

        foreach (var item in dirtyItems)
        {
            statusLabel.Text = $"Saving {item.FileName}...";

            try
            {
                await _taggingEngine.SaveTagsAsync(item.FilePath, item.Tags);
                item.IsDirty = false;
                item.LastWriteTimeUtc = File.GetLastWriteTimeUtc(item.FilePath);
                UpdateListViewItemText(item);

                var png = await _thumbnailService.GetThumbnailAsync(item.FilePath, item.LastWriteTimeUtc, ThumbnailSize);
                SetThumbnail(item, png);
            }
            catch (Exception ex)
            {
                failures.Add($"{item.FileName}: {ex.Message}");
            }

            statusProgressBar.Value++;
        }

        statusProgressBar.Visible = false;
        btnOpenFolder.Enabled = true;
        UpdateSaveButtonState();

        if (failures.Count > 0)
        {
            statusLabel.Text = $"Saved with {failures.Count} error(s).";
            MessageBox.Show(
                this,
                string.Join(Environment.NewLine, failures),
                "Some files failed to save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        else
        {
            statusLabel.Text = $"Saved {dirtyItems.Count} file(s).";
        }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!HasUnsavedChanges())
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            "You have unsaved tag changes. Save before closing?",
            "Unsaved changes",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Warning);

        switch (result)
        {
            case DialogResult.Yes:
                e.Cancel = true;
                _ = SaveThenCloseAsync();
                break;
            case DialogResult.Cancel:
                e.Cancel = true;
                break;
        }
    }

    private async Task SaveThenCloseAsync()
    {
        await SaveAllAsync();
        FormClosing -= MainForm_FormClosing;
        Close();
    }

    private async Task<bool> ConfirmDiscardChangesAsync()
    {
        if (!HasUnsavedChanges())
        {
            return true;
        }

        var result = MessageBox.Show(
            this,
            "You have unsaved tag changes in the current folder. Save before opening a new folder?",
            "Unsaved changes",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Warning);

        switch (result)
        {
            case DialogResult.Yes:
                await SaveAllAsync();
                return true;
            case DialogResult.No:
                return true;
            default:
                return false;
        }
    }

    private bool HasUnsavedChanges() =>
        _listViewItemsByPath.Values.Any(lvi => ((ImageItem)lvi.Tag!).IsDirty);
}
