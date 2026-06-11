#nullable enable
namespace WebpTagger.UI.Controls;

partial class TagListSidebar
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private Label lblHeader = null!;
    private ListBox listBoxTags = null!;
    private Button btnApply = null!;

    #region Component Designer generated code

    private void InitializeComponent()
    {
        lblHeader = new Label();
        listBoxTags = new ListBox();
        btnApply = new Button();
        SuspendLayout();
        //
        // lblHeader
        //
        lblHeader.Dock = DockStyle.Top;
        lblHeader.Font = new Font(Font, FontStyle.Bold);
        lblHeader.Height = 24;
        lblHeader.Padding = new Padding(4, 4, 0, 0);
        lblHeader.Text = "All Tags";
        //
        // listBoxTags
        //
        listBoxTags.Dock = DockStyle.Fill;
        listBoxTags.IntegralHeight = false;
        //
        // btnApply
        //
        btnApply.Dock = DockStyle.Bottom;
        btnApply.Height = 28;
        btnApply.Text = "Apply to Selected";
        //
        // TagListSidebar
        //
        Controls.Add(listBoxTags);
        Controls.Add(btnApply);
        Controls.Add(lblHeader);
        Name = "TagListSidebar";
        Size = new Size(300, 200);
        ResumeLayout(false);
    }

    #endregion
}
