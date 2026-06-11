namespace WebpTagger.UI.Controls;

/// <summary>
/// Shows every tag known in the currently-open folder and lets the user
/// apply one to the images selected in the thumbnail grid.
/// </summary>
public partial class TagListSidebar : UserControl
{
    /// <summary>Raised when the user double-clicks a tag, or selects one and clicks "Apply to Selected".</summary>
    public event EventHandler<string>? ApplyTagRequested;

    public TagListSidebar()
    {
        InitializeComponent();

        listBoxTags.DoubleClick += (_, _) => RaiseApplyForSelectedTag();
        btnApply.Click += (_, _) => RaiseApplyForSelectedTag();
    }

    /// <summary>Replaces the displayed tag list, preserving the current selection if still present.</summary>
    public void SetTags(IEnumerable<string> tags)
    {
        var previouslySelected = listBoxTags.SelectedItem as string;

        listBoxTags.BeginUpdate();
        listBoxTags.Items.Clear();
        foreach (var tag in tags)
        {
            listBoxTags.Items.Add(tag);
        }

        if (previouslySelected is not null)
        {
            var index = listBoxTags.Items.IndexOf(previouslySelected);
            if (index >= 0)
            {
                listBoxTags.SelectedIndex = index;
            }
        }

        listBoxTags.EndUpdate();
    }

    private void RaiseApplyForSelectedTag()
    {
        if (listBoxTags.SelectedItem is string tag)
        {
            ApplyTagRequested?.Invoke(this, tag);
        }
    }
}
