using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace d3mm
{
    public partial class D3ModManager : Form
    {
        private const float c_fPreviewAspectHeight = 180f;
        private const float c_fPreviewAspectWidth = 320f;
        private const int c_iButtonPanelHeight = 50;
        private const int c_iMaxRowHeight = 180;
        private const int c_iMinRowHeight = 24;
        private const string c_sApplicationTitle = "d3mm";
        private const string c_sColumnTitleInstalled = "Installed";
        private const string c_sColumnTitleName = "Name";
        private const string c_sColumnTitlePreview = "Preview";
        private const string c_sColumnTitleRating = "Rating";
        private const string c_sColumnTitleSubmittedBy = "Submitted By";
        private const string c_sColumnTitleSummary = "Summary";
        private const string c_sColumnTitleTags = "Tags";
        private const string c_sColumnTitleVersion = "Version";
        private const string c_sTextStart = "Start";
        private const string c_sTextInstall = "Install";
        private const string c_sTextUninstall = "Uninstall";
        private const string c_sTextWebpageHide = "Hide Webpage";
        private const string c_sTextWebpageShow = "Show Webpage";
        private const string c_sWebPageBlank = "about:blank";

        //

        private System.Windows.Forms.SplitContainer m_splitContainer;
        private BrightIdeasSoftware.ObjectListView m_listView;
        private BrightIdeasSoftware.OLVColumn m_installedColumn;
        private BrightIdeasSoftware.OLVColumn m_previewColumn;
        private BrightIdeasSoftware.OLVColumn m_nameColumn;
        private BrightIdeasSoftware.OLVColumn m_ratingColumn;
        private BrightIdeasSoftware.OLVColumn m_submittedByColumn;
        private BrightIdeasSoftware.OLVColumn m_summaryColumn;
        private BrightIdeasSoftware.OLVColumn m_tagsColumn;
        private BrightIdeasSoftware.OLVColumn m_versionColumn;
        private WebBrowser m_webBroser;
        private Button m_buttonInstall;
        private Button m_buttonStart;
        private Button m_buttonWeb;
        private Panel m_panel;
        private D3ModDatabase m_modDatabase;
        private bool m_bInitialized;
        private bool m_bReopen;

        //

        public bool Reopen
        {
            get { return m_bReopen; }
        }

        //

        public D3ModManager()
        {
            this.InitializeComponent();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.WindowState = ApplicationProperties.Config.Maximized
                ? System.Windows.Forms.FormWindowState.Maximized
                : System.Windows.Forms.FormWindowState.Normal;
            this.ClientSize = new System.Drawing.Size(
                ApplicationProperties.Config.Width,
                ApplicationProperties.Config.Height);
            this.Text = c_sApplicationTitle;

            m_modDatabase = new D3ModDatabase();
            m_modDatabase.onModStateChange += OnModStateChange;

            m_splitContainer = new SplitContainer();
            m_splitContainer.Dock = DockStyle.Fill;
            this.Controls.Add(m_splitContainer);

            m_listView = new BrightIdeasSoftware.ObjectListView();
            m_listView.Dock = DockStyle.Fill;
            m_listView.View = View.Details;
            m_listView.FullRowSelect = true;
            m_listView.CheckBoxes = true;
            m_listView.TriStateCheckBoxes = true;
            m_listView.ShowGroups = false;
            m_listView.TabIndex = 0;
            m_listView.UseCompatibleStateImageBehavior = false;
            m_listView.BorderStyle = BorderStyle.None;
            m_listView.AfterSorting += this.OnAfterSorting;
            m_listView.ColumnWidthChanged += this.OnColumnWidthChanged;
            m_listView.SelectionChanged += this.OnSelectionChanged;
            m_listView.CheckedAspectName = nameof(D3ModInfo.Installed);

            m_installedColumn = new BrightIdeasSoftware.OLVColumn();
            m_previewColumn = new BrightIdeasSoftware.OLVColumn();
            m_nameColumn = new BrightIdeasSoftware.OLVColumn();
            m_ratingColumn = new BrightIdeasSoftware.OLVColumn();
            m_submittedByColumn = new BrightIdeasSoftware.OLVColumn();
            m_summaryColumn = new BrightIdeasSoftware.OLVColumn();
            m_tagsColumn = new BrightIdeasSoftware.OLVColumn();
            m_versionColumn = new BrightIdeasSoftware.OLVColumn();
            m_listView.AllColumns.Add(m_installedColumn);
            m_listView.AllColumns.Add(m_previewColumn);
            m_listView.AllColumns.Add(m_nameColumn);
            m_listView.AllColumns.Add(m_summaryColumn);
            m_listView.AllColumns.Add(m_ratingColumn);
            m_listView.AllColumns.Add(m_submittedByColumn);
            m_listView.AllColumns.Add(m_tagsColumn);
            m_listView.AllColumns.Add(m_versionColumn);
            m_listView.OwnerDraw = true;
            m_listView.RebuildColumns();

            m_installedColumn.AspectName = nameof(D3ModInfo.VersionInstalled);
            m_installedColumn.Text = c_sColumnTitleInstalled;
            m_installedColumn.Width = ApplicationProperties.Config.InstalledColumnWidth;
            m_installedColumn.Renderer = new BrightIdeasSoftware.BarRenderer(0, 100);
            m_installedColumn.IsVisible = ApplicationProperties.Config.InstalledColumnVisible;
            m_installedColumn.VisibilityChanged += this.OnColumnVisibilityChanged;
            m_previewColumn.AspectName = nameof(D3ModInfo.ImageURLLocalFullPath);
            m_previewColumn.Text = c_sColumnTitlePreview;
            m_previewColumn.Width = ApplicationProperties.Config.PreviewColumnWidth;
            m_previewColumn.Renderer = new BrightIdeasSoftware.ImageRenderer();
            m_previewColumn.IsVisible = ApplicationProperties.Config.PreviewColumnVisible;
            m_previewColumn.VisibilityChanged += this.OnColumnVisibilityChanged;
            m_nameColumn.AspectName = nameof(D3ModInfo.Name);
            m_nameColumn.Text = c_sColumnTitleName;
            m_nameColumn.Width = ApplicationProperties.Config.NameColumnWidth;
            m_nameColumn.IsVisible = ApplicationProperties.Config.NameColumnVisible;
            m_nameColumn.VisibilityChanged += this.OnColumnVisibilityChanged;
            m_summaryColumn.AspectName = nameof(D3ModInfo.Summary);
            m_summaryColumn.Text = c_sColumnTitleSummary;
            m_summaryColumn.Width = ApplicationProperties.Config.SummaryColumnWidth;
            m_summaryColumn.IsVisible = ApplicationProperties.Config.SummaryColumnVisible;
            m_summaryColumn.VisibilityChanged += this.OnColumnVisibilityChanged;
            m_ratingColumn.AspectName = nameof(D3ModInfo.Rating);
            m_ratingColumn.AspectToStringFormat = "{0:#,##0.0}";
            m_ratingColumn.Text = c_sColumnTitleRating;
            m_ratingColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            m_ratingColumn.Width = ApplicationProperties.Config.RatingColumnWidth;
            m_ratingColumn.IsVisible = ApplicationProperties.Config.RatingColumnVisible;
            m_ratingColumn.VisibilityChanged += this.OnColumnVisibilityChanged;
            m_submittedByColumn.AspectName = nameof(D3ModInfo.SubmittedBy);
            m_submittedByColumn.Text = c_sColumnTitleSubmittedBy;
            m_submittedByColumn.Width = ApplicationProperties.Config.SubmittedByColumnWidth;
            m_submittedByColumn.IsVisible = ApplicationProperties.Config.SubmittedByColumnVisible;
            m_submittedByColumn.VisibilityChanged += this.OnColumnVisibilityChanged;
            m_tagsColumn.AspectName = nameof(D3ModInfo.Tags);
            m_tagsColumn.AspectToStringConverter = delegate (object x) { return string.Join(", ", (string[])x); };
            m_tagsColumn.Text = c_sColumnTitleTags;
            m_tagsColumn.Width = ApplicationProperties.Config.TagsColumnWidth;
            m_tagsColumn.IsVisible = ApplicationProperties.Config.TagsColumnVisible;
            m_tagsColumn.VisibilityChanged += this.OnColumnVisibilityChanged;
            m_versionColumn.AspectName = nameof(D3ModInfo.Version);
            m_versionColumn.Text = c_sColumnTitleVersion;
            m_versionColumn.Width = ApplicationProperties.Config.VersionColumnWidth;
            m_versionColumn.IsVisible = ApplicationProperties.Config.VersionColumnVisible;
            m_versionColumn.VisibilityChanged += this.OnColumnVisibilityChanged;

            if (m_previewColumn.IsVisible)
            {
                m_listView.RowHeight = Math.Max(
                    c_iMinRowHeight,
                    Math.Min(
                        (int)((ApplicationProperties.Config.PreviewColumnWidth / c_fPreviewAspectWidth) * c_fPreviewAspectHeight),
                        c_iMaxRowHeight));
            }
            else
            {
                m_listView.RowHeight = c_iMinRowHeight;
            }

            m_listView.PrimarySortOrder = (SortOrder)ApplicationProperties.Config.PrimarySortOrder;
            m_listView.PrimarySortColumn = ApplicationProperties.Config.PrimarySortColumn == -1
                ? null
                : m_listView.GetColumn(ApplicationProperties.Config.PrimarySortColumn);
            m_listView.SecondarySortOrder = (SortOrder)ApplicationProperties.Config.SecondarySortOrder;
            m_listView.SecondarySortColumn = ApplicationProperties.Config.SecondarySortColumn == -1
                ? null
                : m_listView.GetColumn(ApplicationProperties.Config.SecondarySortColumn);
            m_listView.SetObjects(m_modDatabase.Mods);

            m_listView.RebuildColumns();

            m_webBroser = new WebBrowser();
            m_webBroser.Dock = DockStyle.Fill;

            m_panel = new Panel();
            m_panel.Dock = DockStyle.Bottom;
            m_panel.Height = c_iButtonPanelHeight;

            m_buttonInstall = new Button();
            m_buttonInstall.Text = c_sTextInstall;
            m_buttonInstall.Dock = DockStyle.Fill;
            m_buttonInstall.Height = c_iButtonPanelHeight;
            m_buttonInstall.Enabled = false;
            m_buttonInstall.Click += OnInstallButtonClicked;

            m_buttonStart = new Button();
            m_buttonStart.Text = c_sTextStart;
            m_buttonStart.Dock = DockStyle.Right;
            m_buttonStart.Height = c_iButtonPanelHeight;
            m_buttonStart.Click += OnStartButtonClicked;
            m_buttonStart.Enabled = !string.IsNullOrEmpty(ApplicationProperties.Config.Executable);

            m_buttonWeb = new Button();
            m_buttonWeb.Text = ApplicationProperties.Config.WebBrowserCollapsed
                ? c_sTextWebpageShow
                : c_sTextWebpageHide;
            m_buttonWeb.Dock = DockStyle.Right;
            m_buttonWeb.Height = c_iButtonPanelHeight;
            m_buttonWeb.Click += OnWebButtonClicked;

            m_panel.Controls.Add(m_buttonInstall);
            m_panel.Controls.Add(m_buttonStart);
            m_panel.Controls.Add(m_buttonWeb);
            m_splitContainer.Panel1.Controls.Add(m_listView);
            m_splitContainer.Panel1.Controls.Add(m_panel);
            m_splitContainer.Panel2.Controls.Add(m_webBroser);
            m_splitContainer.SplitterDistance =
                Screen.GetWorkingArea(this).Width - ApplicationProperties.Config.SplitterDistanceRight;
            m_splitContainer.SplitterMoved += this.OnSplitterMoved;
            m_splitContainer.Panel2Collapsed = ApplicationProperties.Config.WebBrowserCollapsed;

            m_bInitialized = true;
        }

        private void OnModStateChange(D3ModInfo _modInfo)
        {
            m_listView.RefreshObject(_modInfo);
        }

        private void OnAfterSorting(object sender, BrightIdeasSoftware.AfterSortingEventArgs e)
        {
            if (!m_bInitialized)
                return;

            ApplicationProperties.Config.PrimarySortOrder = (int)e.SortOrder;
            ApplicationProperties.Config.PrimarySortColumn = e.ColumnToSort?.Index ?? -1;
            ApplicationProperties.Config.SecondarySortOrder = (int)e.SecondarySortOrder;
            ApplicationProperties.Config.SecondarySortColumn = e.SecondaryColumnToSort?.Index ?? -1;
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            if (!m_bInitialized)
                return;

            if (this.WindowState == FormWindowState.Normal)
            {
                ApplicationProperties.Config.Maximized = false;
                ApplicationProperties.Config.Width = this.Width;
                ApplicationProperties.Config.Height = this.Height;
            }
            else if (this.WindowState == FormWindowState.Maximized)
            {
                ApplicationProperties.Config.Maximized = true;
            }

            base.OnClientSizeChanged(e);
        }

        private void OnColumnWidthChanged(Object sender, ColumnWidthChangedEventArgs e)
        {
            if (!m_bInitialized)
                return;

            BrightIdeasSoftware.OLVColumn column = ((BrightIdeasSoftware.ObjectListView)sender)
                .GetColumn(e.ColumnIndex);
            int iWidth = column.Width;

            switch (e.ColumnIndex)
            {
                case 0: ApplicationProperties.Config.InstalledColumnWidth = iWidth; break;
                case 1: ApplicationProperties.Config.PreviewColumnWidth = iWidth; break;
                case 2: ApplicationProperties.Config.NameColumnWidth = iWidth; break;
                case 3: ApplicationProperties.Config.SummaryColumnWidth = iWidth; break;
                case 4: ApplicationProperties.Config.RatingColumnWidth = iWidth; break;
                case 5: ApplicationProperties.Config.SubmittedByColumnWidth = iWidth; break;
                case 6: ApplicationProperties.Config.TagsColumnWidth = iWidth; break;
                case 7: ApplicationProperties.Config.VersionColumnWidth = iWidth; break;
            }

            if (column == m_previewColumn
                && m_previewColumn.IsVisible)
            {
                m_bReopen = true;
                this.Close();
            }
        }

        private void OnColumnVisibilityChanged(Object sender, EventArgs e)
        {
            if (!m_bInitialized)
                return;

            BrightIdeasSoftware.OLVColumn column = ((BrightIdeasSoftware.OLVColumn)sender);
            bool bVisible = column.IsVisible;

            switch (column.DisplayIndex)
            {
                case 0: ApplicationProperties.Config.InstalledColumnVisible = bVisible; break;
                case 1: ApplicationProperties.Config.PreviewColumnVisible = bVisible; break;
                case 2: ApplicationProperties.Config.NameColumnVisible = bVisible; break;
                case 3: ApplicationProperties.Config.SummaryColumnVisible = bVisible; break;
                case 4: ApplicationProperties.Config.RatingColumnVisible = bVisible; break;
                case 5: ApplicationProperties.Config.SubmittedByColumnVisible = bVisible; break;
                case 6: ApplicationProperties.Config.TagsColumnVisible = bVisible; break;
                case 7: ApplicationProperties.Config.VersionColumnVisible = bVisible; break;
            }

            if (column.DisplayIndex == 1)
            {
                m_bReopen = true;
                this.Close();
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            D3ModInfo d3ModInfo = m_listView.SelectedObject as D3ModInfo;

            if (!m_splitContainer.Panel2Collapsed)
            {
                m_webBroser.Url = new System.Uri(d3ModInfo?.URL ?? c_sWebPageBlank);
            }

            if (d3ModInfo != null)
            {
                m_buttonInstall.Enabled = d3ModInfo.Installed != null;
                if (d3ModInfo.Installed == true)
                {
                    m_buttonInstall.Text = c_sTextUninstall;
                    m_buttonInstall.Click -= this.OnUninstallButtonClicked;
                    m_buttonInstall.Click -= this.OnInstallButtonClicked;
                    m_buttonInstall.Click += this.OnUninstallButtonClicked;
                }
                else
                {
                    m_buttonInstall.Text = c_sTextInstall;
                    m_buttonInstall.Click -= this.OnUninstallButtonClicked;
                    m_buttonInstall.Click -= this.OnInstallButtonClicked;
                    m_buttonInstall.Click += this.OnInstallButtonClicked;
                }
            }
            else
            {
                m_buttonInstall.Enabled = false;
            }
        }

        private void OnInstallButtonClicked(object sender, System.EventArgs e)
        {
            D3ModInfo d3ModInfo = m_listView.SelectedObject as D3ModInfo;
            if (d3ModInfo != null)
            {
                d3ModInfo.Installed = true;
            }
        }

        private void OnUninstallButtonClicked(object sender, System.EventArgs e)
        {
            D3ModInfo d3ModInfo = m_listView.SelectedObject as D3ModInfo;
            if (d3ModInfo != null)
            {
                d3ModInfo.Installed = false;
            }
        }

        private void OnStartButtonClicked(object sender, System.EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = ApplicationProperties.Config.Executable,
                WorkingDirectory = System.IO.Path.GetDirectoryName(ApplicationProperties.Config.Executable),
            };

            Process.Start(startInfo);

        }

        private void OnWebButtonClicked(object sender, System.EventArgs e)
        {
            if (m_splitContainer.Panel2Collapsed)
            {
                ApplicationProperties.Config.WebBrowserCollapsed = false;
                m_buttonWeb.Text = c_sTextWebpageHide;
                m_webBroser.Url = new System.Uri((m_listView.SelectedObject as D3ModInfo)?.URL ?? c_sWebPageBlank);
                m_splitContainer.Panel2Collapsed = false;
            }
            else
            {
                ApplicationProperties.Config.WebBrowserCollapsed = true;
                m_buttonWeb.Text = c_sTextWebpageShow;
                m_webBroser.Url = new System.Uri(c_sWebPageBlank);
                m_splitContainer.Panel2Collapsed = true;
            }
        }

        private void OnSplitterMoved(Object sender, SplitterEventArgs e)
        {
            if (!m_bInitialized)
                return;

            ApplicationProperties.Config.SplitterDistanceRight =
               Screen.GetWorkingArea(this).Width - m_splitContainer.SplitterDistance;
        }
    }
}
