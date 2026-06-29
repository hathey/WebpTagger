using WebpTagger.Core.Services;

namespace WebpTagger.UI;

internal sealed class TagLibraryEditorForm : Form
{
    private readonly TagLibrary _tagLibrary;
    private readonly ListBox _listBox;
    private readonly EventHandler _changedHandler;

    public TagLibraryEditorForm(TagLibrary tagLibrary)
    {
        _tagLibrary = tagLibrary;

        Text = "Edit Tag List";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(320, 420);
        MinimumSize = new Size(240, 280);

        _listBox = new ListBox { Dock = DockStyle.Fill, SelectionMode = SelectionMode.MultiExtended };
        _listBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelected();
            }
        };

        var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8) };
        var closeButton = new Button { Text = "Close", DialogResult = DialogResult.OK, Dock = DockStyle.Right, Width = 90 };
        var removeButton = new Button { Text = "Remove Selected", Dock = DockStyle.Right, Width = 130 };

        buttonPanel.Controls.Add(closeButton);
        buttonPanel.Controls.Add(removeButton);

        Controls.Add(_listBox);
        Controls.Add(buttonPanel);

        AcceptButton = closeButton;

        removeButton.Click += (_, _) => RemoveSelected();

        _changedHandler = (_, _) => RefreshList();
        _tagLibrary.Changed += _changedHandler;
        FormClosed += (_, _) => _tagLibrary.Changed -= _changedHandler;

        RefreshList();
    }

    private void RefreshList()
    {
        _listBox.BeginUpdate();
        _listBox.Items.Clear();
        foreach (var tag in _tagLibrary.Tags)
        {
            _listBox.Items.Add(tag);
        }

        _listBox.EndUpdate();
    }

    private void RemoveSelected()
    {
        foreach (var tag in _listBox.SelectedItems.Cast<string>().ToList())
        {
            _tagLibrary.Remove(tag);
        }
    }
}
