#nullable enable
namespace WebpTagger.UI;

partial class MainForm
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

    private Panel topPanel = null!;
    private Button btnOpenFolder = null!;
    private Button btnSaveAll = null!;
    private Label lblFolderPath = null!;
    private SplitContainer mainSplitContainer = null!;
    private ListView listViewImages = null!;
    private ImageList imageListThumbnails = null!;
    private SplitContainer rightSplitContainer = null!;
    private Controls.TagEditorPanel tagEditorPanel = null!;
    private Controls.TagListSidebar tagListSidebar = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel statusLabel = null!;
    private ToolStripProgressBar statusProgressBar = null!;

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        topPanel = new Panel();
        btnOpenFolder = new Button();
        btnSaveAll = new Button();
        lblFolderPath = new Label();
        mainSplitContainer = new SplitContainer();
        listViewImages = new ListView();
        imageListThumbnails = new ImageList(components);
        rightSplitContainer = new SplitContainer();
        tagEditorPanel = new Controls.TagEditorPanel();
        tagListSidebar = new Controls.TagListSidebar();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        statusProgressBar = new ToolStripProgressBar();

        topPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
        mainSplitContainer.Panel1.SuspendLayout();
        mainSplitContainer.Panel2.SuspendLayout();
        mainSplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)rightSplitContainer).BeginInit();
        rightSplitContainer.Panel1.SuspendLayout();
        rightSplitContainer.Panel2.SuspendLayout();
        rightSplitContainer.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        //
        // topPanel
        //
        topPanel.Controls.Add(lblFolderPath);
        topPanel.Controls.Add(btnSaveAll);
        topPanel.Controls.Add(btnOpenFolder);
        topPanel.Dock = DockStyle.Top;
        topPanel.Height = 42;
        topPanel.Padding = new Padding(8, 7, 8, 7);
        //
        // btnOpenFolder
        //
        btnOpenFolder.Dock = DockStyle.Left;
        btnOpenFolder.Width = 110;
        btnOpenFolder.Text = "Open Folder...";
        //
        // btnSaveAll
        //
        btnSaveAll.Dock = DockStyle.Right;
        btnSaveAll.Width = 100;
        btnSaveAll.Text = "Save All";
        //
        // lblFolderPath
        //
        lblFolderPath.AutoEllipsis = true;
        lblFolderPath.Dock = DockStyle.Fill;
        lblFolderPath.Padding = new Padding(8, 0, 8, 0);
        lblFolderPath.Text = "(no folder selected)";
        lblFolderPath.TextAlign = ContentAlignment.MiddleLeft;
        //
        // mainSplitContainer
        //
        mainSplitContainer.Dock = DockStyle.Fill;
        mainSplitContainer.Location = new Point(0, 42);
        mainSplitContainer.Size = new Size(1000, 586);
        mainSplitContainer.SplitterDistance = 650;
        mainSplitContainer.Panel1.Controls.Add(listViewImages);
        mainSplitContainer.Panel2.Controls.Add(rightSplitContainer);
        //
        // listViewImages
        //
        listViewImages.Dock = DockStyle.Fill;
        listViewImages.HideSelection = false;
        listViewImages.LargeImageList = imageListThumbnails;
        listViewImages.MultiSelect = true;
        listViewImages.UseCompatibleStateImageBehavior = false;
        listViewImages.View = View.LargeIcon;
        //
        // imageListThumbnails
        //
        imageListThumbnails.ColorDepth = ColorDepth.Depth32Bit;
        imageListThumbnails.ImageSize = new Size(128, 128);
        //
        // rightSplitContainer
        //
        rightSplitContainer.Dock = DockStyle.Fill;
        rightSplitContainer.Orientation = Orientation.Horizontal;
        rightSplitContainer.Size = new Size(346, 586);
        rightSplitContainer.SplitterDistance = 320;
        rightSplitContainer.Panel1.Controls.Add(tagEditorPanel);
        rightSplitContainer.Panel2.Controls.Add(tagListSidebar);
        //
        // tagEditorPanel
        //
        tagEditorPanel.Dock = DockStyle.Fill;
        tagEditorPanel.Size = new Size(346, 320);
        //
        // tagListSidebar
        //
        tagListSidebar.Dock = DockStyle.Fill;
        tagListSidebar.Size = new Size(346, 262);
        //
        // statusStrip
        //
        statusStrip.Items.Add(statusLabel);
        statusStrip.Items.Add(statusProgressBar);
        //
        // statusLabel
        //
        statusLabel.Spring = true;
        statusLabel.Text = "Ready";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // statusProgressBar
        //
        statusProgressBar.Visible = false;
        statusProgressBar.Width = 200;
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 650);
        Controls.Add(mainSplitContainer);
        Controls.Add(statusStrip);
        Controls.Add(topPanel);
        MinimumSize = new Size(700, 450);
        Text = "WebpTagger";

        topPanel.ResumeLayout(false);
        mainSplitContainer.Panel1.ResumeLayout(false);
        mainSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
        mainSplitContainer.ResumeLayout(false);
        rightSplitContainer.Panel1.ResumeLayout(false);
        rightSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)rightSplitContainer).EndInit();
        rightSplitContainer.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
}
