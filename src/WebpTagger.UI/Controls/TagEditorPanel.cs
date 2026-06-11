using WebpTagger.Core.Models;

namespace WebpTagger.UI.Controls;

/// <summary>
/// Shows the tags for the currently-selected image(s) as removable chips,
/// and lets the user type a new tag to apply to the selection.
/// </summary>
public partial class TagEditorPanel : UserControl
{
    private IReadOnlyList<ImageItem> _selection = Array.Empty<ImageItem>();

    /// <summary>Raised when the user types a new tag and presses Enter or clicks "Add Tag".</summary>
    public event EventHandler<string>? TagAdded;

    /// <summary>Raised when the user clicks the "x" on a tag chip.</summary>
    public event EventHandler<string>? TagRemoved;

    public TagEditorPanel()
    {
        InitializeComponent();

        btnAddTag.Click += (_, _) => CommitNewTag();
        txtNewTag.KeyDown += TxtNewTag_KeyDown;

        UpdateInputEnabled();
    }

    /// <summary>Updates the panel to show tags for the given selection (0, 1, or many images).</summary>
    public void SetSelection(IReadOnlyList<ImageItem> items)
    {
        _selection = items;
        RenderChips();
        UpdateInputEnabled();
    }

    /// <summary>Provides the known tag list for autocomplete suggestions in the new-tag box.</summary>
    public void SetSuggestions(IEnumerable<string> tags)
    {
        var source = new AutoCompleteStringCollection();
        source.AddRange(tags.ToArray());
        txtNewTag.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        txtNewTag.AutoCompleteSource = AutoCompleteSource.CustomSource;
        txtNewTag.AutoCompleteCustomSource = source;
    }

    private void RenderChips()
    {
        flowPanelTags.SuspendLayout();

        foreach (Control control in flowPanelTags.Controls)
        {
            control.Dispose();
        }

        flowPanelTags.Controls.Clear();

        var tags = _selection
            .SelectMany(item => item.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase);

        foreach (var tag in tags)
        {
            flowPanelTags.Controls.Add(CreateChip(tag));
        }

        flowPanelTags.ResumeLayout();
    }

    private Control CreateChip(string tag)
    {
        var chip = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(3),
            Padding = new Padding(6, 3, 2, 2),
        };

        var label = new Label
        {
            Text = tag,
            AutoSize = true,
            Margin = new Padding(0, 2, 6, 0),
        };

        var removeButton = new Button
        {
            Text = "x",
            AutoSize = false,
            Size = new Size(20, 20),
            Margin = new Padding(0),
            FlatStyle = FlatStyle.Flat,
            TabStop = false,
        };
        removeButton.FlatAppearance.BorderSize = 0;
        removeButton.Click += (_, _) => TagRemoved?.Invoke(this, tag);

        chip.Controls.Add(label);
        chip.Controls.Add(removeButton);
        return chip;
    }

    private void TxtNewTag_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        CommitNewTag();
    }

    private void CommitNewTag()
    {
        var tag = txtNewTag.Text.Trim();
        if (string.IsNullOrEmpty(tag))
        {
            return;
        }

        txtNewTag.Clear();
        TagAdded?.Invoke(this, tag);
    }

    private void UpdateInputEnabled()
    {
        var hasSelection = _selection.Count > 0;
        txtNewTag.Enabled = hasSelection;
        btnAddTag.Enabled = hasSelection;
    }
}
