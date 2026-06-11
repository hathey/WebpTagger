#nullable enable
namespace WebpTagger.UI.Controls;

partial class TagEditorPanel
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
    private FlowLayoutPanel flowPanelTags = null!;
    private Panel pnlInput = null!;
    private TextBox txtNewTag = null!;
    private Button btnAddTag = null!;

    #region Component Designer generated code

    private void InitializeComponent()
    {
        lblHeader = new Label();
        flowPanelTags = new FlowLayoutPanel();
        pnlInput = new Panel();
        txtNewTag = new TextBox();
        btnAddTag = new Button();
        SuspendLayout();
        //
        // lblHeader
        //
        lblHeader.Dock = DockStyle.Top;
        lblHeader.Font = new Font(Font, FontStyle.Bold);
        lblHeader.Height = 24;
        lblHeader.Padding = new Padding(4, 4, 0, 0);
        lblHeader.Text = "Tags";
        //
        // flowPanelTags
        //
        flowPanelTags.AutoScroll = true;
        flowPanelTags.Dock = DockStyle.Fill;
        flowPanelTags.FlowDirection = FlowDirection.LeftToRight;
        flowPanelTags.Padding = new Padding(4);
        flowPanelTags.WrapContents = true;
        //
        // pnlInput
        //
        pnlInput.Controls.Add(txtNewTag);
        pnlInput.Controls.Add(btnAddTag);
        pnlInput.Dock = DockStyle.Bottom;
        pnlInput.Height = 32;
        pnlInput.Padding = new Padding(4);
        //
        // txtNewTag
        //
        txtNewTag.Dock = DockStyle.Fill;
        //
        // btnAddTag
        //
        btnAddTag.Dock = DockStyle.Right;
        btnAddTag.Text = "Add Tag";
        btnAddTag.Width = 80;
        //
        // TagEditorPanel
        //
        Controls.Add(flowPanelTags);
        Controls.Add(pnlInput);
        Controls.Add(lblHeader);
        Name = "TagEditorPanel";
        Size = new Size(300, 200);
        ResumeLayout(false);
    }

    #endregion
}
