namespace WebpTagger.UI;

/// <summary>
/// Folder picker built entirely on System.IO/DriveInfo. Avoids the Windows shell
/// namespace (the OS FolderBrowserDialog/IFileOpenDialog) so a broken shell
/// extension on a user's machine can't crash the process while it's resolving
/// icons for "This PC", drives, or network locations.
/// </summary>
internal sealed class FolderPickerDialog : Form
{
    private readonly TreeView _treeView;
    private readonly TextBox _pathTextBox;
    private readonly Button _okButton;
    private readonly Button _cancelButton;
    private readonly string? _initialPath;

    public string? SelectedPath { get; private set; }

    public FolderPickerDialog(string? initialPath)
    {
        _initialPath = initialPath;

        Text = "Select Folder";
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(480, 520);
        MinimumSize = new Size(360, 320);

        _pathTextBox = new TextBox();
        _treeView = new TreeView { HideSelection = false };
        _okButton = new Button { Text = "OK" };
        _cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };

        Controls.Add(_pathTextBox);
        Controls.Add(_treeView);
        Controls.Add(_okButton);
        Controls.Add(_cancelButton);

        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        _treeView.BeforeExpand += TreeView_BeforeExpand;
        _treeView.AfterSelect += TreeView_AfterSelect;
        _okButton.Click += OkButton_Click;
        Load += FolderPickerDialog_Load;
        Resize += (_, _) => LayoutControls();

        LayoutControls();
    }

    private void LayoutControls()
    {
        const int margin = 12;
        var width = ClientSize.Width;
        var height = ClientSize.Height;

        _pathTextBox.SetBounds(margin, margin, width - margin * 2, _pathTextBox.PreferredHeight);

        var buttonTop = height - margin - _okButton.Height;
        var treeTop = _pathTextBox.Bottom + 8;
        _treeView.SetBounds(margin, treeTop, width - margin * 2, buttonTop - 8 - treeTop);

        _cancelButton.SetBounds(width - margin - _cancelButton.Width, buttonTop, _cancelButton.Width, _cancelButton.Height);
        _okButton.SetBounds(_cancelButton.Left - 8 - _okButton.Width, buttonTop, _okButton.Width, _okButton.Height);
    }

    private async void FolderPickerDialog_Load(object? sender, EventArgs e)
    {
        _treeView.Enabled = false;

        var rootPaths = await Task.Run(() =>
            DriveInfo.GetDrives()
                .Where(IsDriveReadySafe)
                .Select(d => d.RootDirectory.FullName)
                .ToList());

        foreach (var rootPath in rootPaths)
        {
            _treeView.Nodes.Add(CreateDirectoryNode(rootPath));
        }

        _treeView.Enabled = true;

        if (!string.IsNullOrEmpty(_initialPath) && Directory.Exists(_initialPath))
        {
            _pathTextBox.Text = _initialPath;
        }
    }

    private static bool IsDriveReadySafe(DriveInfo drive)
    {
        try
        {
            return drive.IsReady;
        }
        catch
        {
            return false;
        }
    }

    private static TreeNode CreateDirectoryNode(string fullPath)
    {
        var node = new TreeNode(GetDisplayName(fullPath)) { Tag = fullPath };
        node.Nodes.Add(new TreeNode());
        return node;
    }

    private static string GetDisplayName(string fullPath)
    {
        var name = Path.GetFileName(fullPath.TrimEnd(Path.DirectorySeparatorChar));
        return string.IsNullOrEmpty(name) ? fullPath : name;
    }

    private static IReadOnlyList<string> GetSubdirectoriesSafe(string path)
    {
        try
        {
            return Directory.EnumerateDirectories(path)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private void TreeView_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
    {
        var node = e.Node!;
        if (node.Nodes.Count != 1 || node.Nodes[0].Tag is not null)
        {
            return;
        }

        node.Nodes.Clear();
        foreach (var childPath in GetSubdirectoriesSafe((string)node.Tag!))
        {
            node.Nodes.Add(CreateDirectoryNode(childPath));
        }
    }

    private void TreeView_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is string path)
        {
            _pathTextBox.Text = path;
        }
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        var path = _pathTextBox.Text.Trim();
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            MessageBox.Show(this, "Please select or enter a valid folder path.", "Invalid Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SelectedPath = path;
        DialogResult = DialogResult.OK;
        Close();
    }

    public static string? Browse(IWin32Window owner, string? initialPath = null)
    {
        using var dialog = new FolderPickerDialog(initialPath);
        return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.SelectedPath : null;
    }
}
