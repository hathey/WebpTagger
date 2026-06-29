using System.Diagnostics;
using WebpTagger.Core.Diagnostics;

namespace WebpTagger.UI;

internal sealed class LogViewerForm : Form
{
    private readonly TextBox _logTextBox;

    public LogViewerForm()
    {
        Text = "Diagnostics Log";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(700, 480);
        MinimumSize = new Size(420, 280);

        _logTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Font = new Font(FontFamily.GenericMonospace, 9f),
            Dock = DockStyle.Fill,
        };

        var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8) };
        var closeButton = new Button { Text = "Close", DialogResult = DialogResult.OK, Dock = DockStyle.Right, Width = 90 };
        var openFolderButton = new Button { Text = "Open Log Folder", Dock = DockStyle.Right, Width = 130 };
        var clearButton = new Button { Text = "Clear Log", Dock = DockStyle.Right, Width = 90 };
        var refreshButton = new Button { Text = "Refresh", Dock = DockStyle.Right, Width = 90 };

        buttonPanel.Controls.Add(closeButton);
        buttonPanel.Controls.Add(openFolderButton);
        buttonPanel.Controls.Add(clearButton);
        buttonPanel.Controls.Add(refreshButton);

        Controls.Add(_logTextBox);
        Controls.Add(buttonPanel);

        AcceptButton = closeButton;

        refreshButton.Click += (_, _) => LoadLog();
        clearButton.Click += (_, _) =>
        {
            DiagnosticsLog.Clear();
            LoadLog();
        };
        openFolderButton.Click += (_, _) => OpenLogFolder();

        LoadLog();
    }

    private void LoadLog()
    {
        var content = DiagnosticsLog.ReadAll();
        _logTextBox.Text = string.IsNullOrEmpty(content) ? "(no log entries yet)" : content;
        _logTextBox.SelectionStart = _logTextBox.Text.Length;
        _logTextBox.ScrollToCaret();
    }

    private void OpenLogFolder()
    {
        var directory = Path.GetDirectoryName(DiagnosticsLog.FilePath);
        if (directory is null || !Directory.Exists(directory))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(directory) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            MessageBox.Show(this, $"Could not open the log folder:\n\n{ex.Message}", "WebpTagger - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
