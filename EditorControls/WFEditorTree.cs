using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TextAdventures.Quest.EditorControls
{
    public partial class WFEditorTree : UserControl
    {
        public WFEditorTree()
        {
            InitializeComponent();

            Resize += EditorTree_Resize;
            cmdClose.Click += cmdClose_Click;
            cmdSearch.Click += cmdSearch_Click;
            ctlTreeView.AfterSelect += ctlTreeView_AfterSelect;
            ctlTreeView.DoubleClick += ctlTreeView_DoubleClick;
            ctlTreeView.KeyUp += ctlTreeView_KeyUp;
            ctlTreeView.DragDrop += ctlTreeView_DragDrop;
            ctlTreeView.DragOver += ctlTreeView_DragOver;
            ctlTreeView.GotFocus += ctlTreeView_GotFocus;
            ctlTreeView.ItemDrag += ctlTreeView_ItemDrag;
            ctlTreeView.LostFocus += ctlTreeView_LostFocus;
            ctlTreeView.MouseUp += ctlTreeView_MouseUp;
            lstSearchResults.DoubleClick += lstSearchResults_DoubleClick;
            lstSearchResults.ItemSelectionChanged += lstSearchResults_ItemSelectionChanged;
            lstSearchResults.KeyDown += lstSearchResults_KeyDown;
            txtSearch.KeyDown += txtSearch_KeyDown;

            ShowSearchResults = false;
            ResizeColumn();
            AddHandlers();

            AddMenuClickHandler("expandall", ExpandAll);
            AddMenuClickHandler("collapseall", CollapseAll);

            ctlTreeView.HandleCreated += (s, e) => SetWindowTheme(ctlTreeView.Handle, "Explorer", null);
            ctlToolStrip.RenderMode = ToolStripRenderMode.System;

            txtSearch.Dock = DockStyle.None;
            foreach (var btn in new[] { cmdSearch, cmdClose })
            {
                btn.BackColor = Color.Transparent;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(210, 228, 244);
                var bmp = new Bitmap(btn.Image);
                bmp.MakeTransparent(Color.White);
                btn.Image = bmp;
            }
            pnlSearchContainer.Resize += (s, e) => LayoutSearchBox();
            pnlSearchContainer.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(180, 180, 180)))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlSearchContainer.Width - 1, pnlSearchContainer.Height - 1);
            };
            LayoutSearchBox();
        }

        private void LayoutSearchBox()
        {
            int rightWidth = cmdSearch.Width + (cmdClose.Visible ? cmdClose.Width : 0);
            int h = txtSearch.PreferredHeight;
            int y = (pnlSearchContainer.Height - h) / 2;
            txtSearch.SetBounds(4, y, pnlSearchContainer.Width - rightWidth - 4, h);
        }

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

    private Dictionary<string, TreeNode> m_nodes = new Dictionary<string, TreeNode>();
        private FilterOptions m_filterSettings;
        private string m_previousSelection;
        private List<string> m_openNodes;
        private List<string> m_nodesWithChildren;
        private string m_selection;
        private bool m_updatingSelection = false;
        private bool m_showSearchResults = false;
        private Func<string, string, bool> m_canDragDelegate;
        private Action<string, string> m_doDragDelegate;
        public delegate void MenuClickHandler();
        private Dictionary<string, ToolStripMenuItem> m_menus = new Dictionary<string, ToolStripMenuItem>();
        private Dictionary<string, ToolStripSeparator> m_separators = new Dictionary<string, ToolStripSeparator>();
        private Dictionary<string, MenuClickHandler> m_handlers = new Dictionary<string, MenuClickHandler>();
        public event FiltersUpdatedEventHandler FiltersUpdated;
        public delegate void FiltersUpdatedEventHandler();
        public event SelectionChangedEventHandler SelectionChanged;
        public delegate void SelectionChangedEventHandler(string key);
        public event CommitSelectionEventHandler CommitSelection;
        public delegate void CommitSelectionEventHandler();
        public event TreeGotFocusEventHandler TreeGotFocus;
        public delegate void TreeGotFocusEventHandler();
        public event TreeLostFocusEventHandler TreeLostFocus;
        public delegate void TreeLostFocusEventHandler();

        // Code for setting "Search" hint on textbox
        private const uint ECM_FIRST = 0x1500;

        private const uint EM_SETCUEBANNER = ECM_FIRST + 1;
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, string lParam);

        protected override void OnHandleCreated(System.EventArgs e)
        {
            SetTextboxHint();
            base.OnHandleCreated(e);
            RebuildImageList();
            ScaleImageList();
            ScaleButtonImages();
            ApplyContextMenuIcons();
            UpdateItemHeight();
            ScaleFilterToolStrip();
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            ScaleImageList();
            ScaleButtonImages();
            ApplyContextMenuIcons();
            UpdateItemHeight();
            ScaleFilterToolStrip();
        }

        private void ScaleFilterToolStrip()
        {
            float scale = DeviceDpi / 96f;
            int size = Math.Max(16, (int)(16 * scale));
            ctlToolStrip.ImageScalingSize = new Size(size, size);
        }

        private void UpdateItemHeight()
        {
            int padding = (int)Math.Round(10 * DeviceDpi / 96f);
            ctlTreeView.ItemHeight = ctlTreeView.Font.Height + padding;
        }

        private void RebuildImageList()
        {
            ctlImageList.Images.Clear();
            AddToImageList("s_room", Properties.Resources.s_room);
            AddToImageList("s_object", Properties.Resources.s_object);
            AddToImageList("s_exit", Properties.Resources.s_exit);
            AddToImageList("s_verb", Properties.Resources.s_verb);
            AddToImageList("s_command", Properties.Resources.s_command);
            AddToImageList("s_script", Properties.Resources.s_command);
            AddToImageList("s_function", Properties.Resources.s_function);
            AddToImageList("s_timer", Properties.Resources.s_timer);
            AddToImageList("s_turn", Properties.Resources.s_turn);
            AddToImageList("s_walk", Properties.Resources.s_walk);
            AddToImageList("s_add_page", Properties.Resources.s_add_page);
            AddToImageList("s_library", Properties.Resources.s_library);
            AddToImageList("s_template", Properties.Resources.s_template);
            AddToImageList("s_dynamictemplate", Properties.Resources.s_dynamictemplate);
            AddToImageList("s_objecttype", Properties.Resources.s_objecttype);
            AddToImageList("s_javascript", Properties.Resources.s_javascript);
            AddToImageList("s_folder", Properties.Resources.s_folder);
            AddToImageList("s_game", Properties.Resources.s_game);
        }

        private void AddToImageList(string key, Image image)
        {
            if (image != null)
                ctlImageList.Images.Add(key, image);
        }

        private void ScaleImageList()
        {
            float scale = DeviceDpi / 96f;
            int newSize = Math.Max(16, (int)(16 * scale));

            if (ctlTreeView.ImageList != null && ctlTreeView.ImageList != ctlImageList && ctlTreeView.ImageList.ImageSize.Width == newSize)
                return;

            var sourceList = ctlImageList;
            var newList = new ImageList();
            newList.ColorDepth = ColorDepth.Depth32Bit;
            newList.ImageSize = new Size(newSize, newSize);
            newList.TransparentColor = sourceList.TransparentColor;

            for (int i = 0; i < sourceList.Images.Count; i++)
            {
                string key = sourceList.Images.Keys[i];
                Bitmap bmp = null;
                string xamlName;
                if (_treeXamlNames.TryGetValue(key, out xamlName))
                    bmp = XamlIconRenderer.RenderXaml(xamlName, newSize);
                if (bmp == null)
                    bmp = newSize == 16
                        ? new Bitmap(sourceList.Images[i])
                        : ScaleImageHighQuality(sourceList.Images[i], newSize);
                newList.Images.Add(bmp);
                newList.Images.SetKeyName(i, key);
            }

            var old = ctlTreeView.ImageList;
            ctlTreeView.ImageList = newList;
            if (old != null && old != ctlImageList)
                old.Dispose();
        }

        private Bitmap _originalSearchImage;
        private Bitmap _originalCloseImage;

        private void ScaleButtonImages()
        {
            if (_originalSearchImage == null) _originalSearchImage = (Bitmap)cmdSearch.Image;
            if (_originalCloseImage == null) _originalCloseImage = (Bitmap)cmdClose.Image;

            float scale = DeviceDpi / 96f;
            ScaleButtonImage(cmdSearch, _originalSearchImage, scale);
            ScaleButtonImage(cmdClose, _originalCloseImage, scale);

            if (scale > 1f)
                ctlToolStrip.Renderer = new DpiScaledToolStripRenderer(scale);
        }

        private class DpiScaledToolStripRenderer : ToolStripProfessionalRenderer
        {
            private readonly float _scale;

            public DpiScaledToolStripRenderer(float scale)
            {
                _scale = scale;
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                var r = e.ArrowRectangle;
                var middle = new System.Drawing.Point(r.Left + r.Width / 2 + (r.Width % 2), r.Top + r.Height / 2);
                int half = Math.Max(2, (int)(2 * _scale));

                System.Drawing.Point[] arrow;
                switch (e.Direction)
                {
                    case ArrowDirection.Up:
                        arrow = new System.Drawing.Point[]
                        {
                            new System.Drawing.Point(middle.X - half, middle.Y + half / 2 + 1),
                            new System.Drawing.Point(middle.X + half + 1, middle.Y + half / 2 + 1),
                            new System.Drawing.Point(middle.X, middle.Y - half / 2)
                        };
                        break;
                    case ArrowDirection.Left:
                        arrow = new System.Drawing.Point[]
                        {
                            new System.Drawing.Point(middle.X + half / 2 + 1, middle.Y - half),
                            new System.Drawing.Point(middle.X + half / 2 + 1, middle.Y + half + 1),
                            new System.Drawing.Point(middle.X - half / 2, middle.Y)
                        };
                        break;
                    case ArrowDirection.Right:
                        arrow = new System.Drawing.Point[]
                        {
                            new System.Drawing.Point(middle.X - half / 2, middle.Y - half),
                            new System.Drawing.Point(middle.X - half / 2, middle.Y + half + 1),
                            new System.Drawing.Point(middle.X + half / 2 + 1, middle.Y)
                        };
                        break;
                    default: // Down
                        arrow = new System.Drawing.Point[]
                        {
                            new System.Drawing.Point(middle.X - half, middle.Y - half / 2),
                            new System.Drawing.Point(middle.X + half + 1, middle.Y - half / 2),
                            new System.Drawing.Point(middle.X, middle.Y + half / 2 + 1)
                        };
                        break;
                }

                using (var brush = new System.Drawing.SolidBrush(e.ArrowColor))
                    e.Graphics.FillPolygon(brush, arrow);
            }
        }

        private static void ScaleButtonImage(Button button, Bitmap original, float scale)
        {
            int newW = (int)(original.Width * scale);
            int newH = (int)(original.Height * scale);
            if (button.Image is Bitmap current && current != original && current.Width == newW) return;
            var old = button.Image;
            button.Image = new Bitmap(original, newW, newH);
            if (old != null && old != original) old.Dispose();
        }

        private static readonly Dictionary<string, string> _treeXamlNames = new Dictionary<string, string>
        {
            { "s_room",            "FolderClosed" },
            { "s_object",         "Item" },
            { "s_exit",           "Exit" },
            { "s_verb",           "Comment" },
            { "s_command",        "Keyboard" },
            { "s_script",         "Script" },
            { "s_function",       "Method" },
            { "s_timer",          "Timer" },
            { "s_turn",           "TriggerScript" },
            { "s_walk",           "TaskRunner" },
            { "s_add_page",       "AddDocument" },
            { "s_library",        "Library" },
            { "s_template",       "Template" },
            { "s_dynamictemplate","DynamicTemplate" },
            { "s_objecttype",     "TypeDefinition" },
            { "s_javascript",     "JSScript" },
            { "s_folder",         "FolderClosed" },
            { "s_game",           "Home" },
        };

        private static readonly Dictionary<string, string> _contextMenuXamlNames = new Dictionary<string, string>
        {
            { "cutToolStripMenuItem",            "Cut" },
            { "copyToolStripMenuItem",           "Copy" },
            { "pasteToolStripMenuItem",          "Paste" },
            { "deleteToolStripMenuItem",         "Delete" },
            { "AddVerbToolStripMenuItem",        "Comment" },
            { "AddCommandToolStripMenuItem",     "Keyboard" },
            { "addPageToolStripMenuItem",        "AddDocument" },
            { "AddRoomToolStripMenuItem",        "AddFolder" },
            { "AddObjectToolStripMenuItem",      "AddItem" },
            { "AddExitToolStripMenuItem",        "Exit" },
            { "AddFunctionToolStripMenuItem",    "Method" },
            { "addTimerToolStripMenuItem",       "Timer" },
            { "AddWalkthroughToolStripMenuItem", "TaskRunner" },
        };

        private void ApplyContextMenuIcons()
        {
            float scale = DeviceDpi / 96f;
            int size = Math.Max(16, (int)(16 * scale));
            foreach (ToolStripItem item in ctlContextMenu.Items)
            {
                string xamlName;
                if (_contextMenuXamlNames.TryGetValue(item.Name, out xamlName))
                {
                    var bmp = XamlIconRenderer.RenderXaml(xamlName, size);
                    if (bmp != null)
                    {
                        var old = item.Image;
                        item.Image = bmp;
                        if (old != null) old.Dispose();
                    }
                }
            }
        }

        private static Bitmap ScaleImageHighQuality(Image source, int size)
        {
            var result = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.DrawImage(source, 0, 0, size, size);
            }
            return result;
        }

        private void SetTextboxHint()
        {
            string hintText = "Search";
            SendMessage(new HandleRef(this, txtSearch.Handle), EM_SETCUEBANNER, IntPtr.Zero, hintText);
        }

        public void AddNode(string key, string text, string parentKey, System.Drawing.Color? foreColor, System.Drawing.Color? backColor, int? position = null, string imageKey = null)
        {
            if (m_nodes.ContainsKey(key)) return;
            TreeNode newNode = default(TreeNode);
            TreeNodeCollection parent = default(TreeNodeCollection);

            if (string.IsNullOrEmpty(parentKey))
            {
                parent = ctlTreeView.Nodes;
            }
            else
            {
                parent = m_nodes[parentKey].Nodes;
            }

            if (position == null)
            {
                newNode = parent.Add(key, text);
            }
            else
            {
                newNode = parent.Insert(position.Value, key, text);
            }

            if (foreColor.HasValue)
            {
                newNode.ForeColor = foreColor.Value;
            }

            if (!string.IsNullOrEmpty(imageKey))
            {
                newNode.Tag = imageKey;
                newNode.ImageKey = imageKey;
                newNode.SelectedImageKey = imageKey;
            }

            m_nodes.Add(key, newNode);
        }

        public void RemoveNode(string key)
        {
            if (!m_nodes.ContainsKey(key)) return;
            List<string> keysToRemove = new List<string>();
            RemoveChildNodesFromCache(m_nodes[key], keysToRemove);
            foreach (string nodeKey in keysToRemove)
            {
                m_nodes.Remove(nodeKey);
            }
            ctlTreeView.Nodes.Remove(m_nodes[key]);
            m_nodes.Remove(key);
        }

        private void RemoveChildNodesFromCache(TreeNode node, List<string> keysToRemove)
        {
            foreach (TreeNode childNode in m_nodes.Values)
            {
                if (childNode.Parent == node)
                {
                    keysToRemove.Add(childNode.Name);
                    RemoveChildNodesFromCache(childNode, keysToRemove);
                }
            }
        }

        public void RenameNode(string oldKey, string newKey)
        {
            TreeNode node = m_nodes[oldKey];
            m_nodes.Remove(oldKey);
            m_nodes.Add(newKey, node);
            node.Name = newKey;
            node.Text = newKey;

            if (oldKey == m_selection) m_selection = newKey;
        }

        public void RetitleNode(string key, string title)
        {
            if (!m_nodes.ContainsKey(key)) return;
            TreeNode node = m_nodes[key];
            node.Text = title;
        }

        public void SetAvailableFilters(AvailableFilters filters)
        {
            mnuFilter.DropDownItems.Clear();
            m_filterSettings = new FilterOptions();

            foreach (string key in filters.AllFilters)
            {
                System.Windows.Forms.ToolStripMenuItem newMenu = new System.Windows.Forms.ToolStripMenuItem();
                newMenu.Text = filters.Get(key);
                newMenu.Tag = key;
                newMenu.Click += FilterClicked;
                mnuFilter.DropDownItems.Add(newMenu);
            }
        }

        private void FilterClicked(object sender, System.EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            string key = (string)menuItem.Tag;
            m_filterSettings.Set(key, !m_filterSettings.IsSet(key));
            menuItem.Checked = m_filterSettings.IsSet(key);
            if (FiltersUpdated != null)
            {
                FiltersUpdated();
            }
        }

        public FilterOptions FilterSettings
        {
            get { return m_filterSettings; }
        }

        public void Clear()
        {
            ctlTreeView.Nodes.Clear();
            m_nodes.Clear();
            ShowSearchResults = false;
            txtSearch.Text = "";
        }

        public void BeginUpdate()
        {
            if ((ctlTreeView.SelectedNode == null))
            {
                m_previousSelection = "";
            }
            else
            {
                m_previousSelection = ctlTreeView.SelectedNode.Name;
            }

            m_openNodes = new List<string>();
            m_nodesWithChildren = new List<string>();

            foreach (TreeNode node in m_nodes.Values)
            {
                if (node.IsExpanded)
                {
                    m_openNodes.Add(node.Name);
                }

                if (node.GetNodeCount(false) > 0)
                {
                    m_nodesWithChildren.Add(node.Name);
                }
            }
        }

        public void EndUpdate()
        {
            if (!string.IsNullOrEmpty(m_previousSelection))
            {
                if (m_nodes.ContainsKey(m_previousSelection))
                {
                    ctlTreeView.SelectedNode = m_nodes[m_previousSelection];
                }
            }

            foreach (TreeNode node in m_nodes.Values)
            {
                if ((m_openNodes.Contains(node.Name)))
                {
                    node.Expand();
                }

                if (!m_nodesWithChildren.Contains(node.Name) && node.GetNodeCount(false) > 0)
                {
                    // Expand any nodes that have new children
                    node.Expand();
                }
            }
        }

        public void CollapseAdvancedNode()
        {
            if (m_nodes.ContainsKey("_advanced"))
            {
                m_nodes["_advanced"].Collapse();
            }
        }

        private void ctlTreeView_AfterSelect(System.Object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag is string imgKey)
                e.Node.SelectedImageKey = imgKey;
            if (m_updatingSelection)
                return;
            SelectCurrentTreeViewItem();
        }

        private void ctlTreeView_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // When right-clicking, select the item first before displaying the context menu
                ctlTreeView.SelectedNode = ctlTreeView.GetNodeAt(e.X, e.Y);
                if (ctlTreeView.SelectedNode != null && ctlTreeView.ContextMenuStrip != null)
                {
                    ctlTreeView.ContextMenuStrip.Show(ctlTreeView, e.Location);
                }
            }
        }

        private void SelectCurrentTreeViewItem()
        {
            string key = null;

            if (ctlTreeView.SelectedNode == null)
            {
                key = null;
            }
            else
            {
                key = ctlTreeView.SelectedNode.Name;
            }

            ChangeSelection(key);
        }

        private void ChangeSelection(string key)
        {
            if (key == null) return;

            if (key != m_selection)
            {
                m_selection = key;
                SetSelectedItemNoEvent(key);
                if (SelectionChanged != null)
                {
                    SelectionChanged(key);
                }
            }
        }

        public void SetSelectedItemNoEvent(string key)
        {
            m_updatingSelection = true;
            ctlTreeView.SelectedNode = m_nodes[key];
            ctlTreeView.SelectedNode.EnsureVisible();
            m_updatingSelection = false;
        }

        public void SetSelectedItem(string key)
        {
            ctlTreeView.SelectedNode = m_nodes[key];
            ctlTreeView.SelectedNode.EnsureVisible();
        }

        public void TrySetSelectedItem(string key)
        {
            if (m_nodes.ContainsKey(key))
            {
                SetSelectedItem(key);
            }
        }

        public bool TrySetSelectedItemNoEvent(string key)
        {
            if (!m_nodes.ContainsKey(key)) return false;
            SetSelectedItemNoEvent(key);
            return true;
        }

        public bool ShowFilterBar
        {
            get { return ctlToolStrip.Visible; }
            set { ctlToolStrip.Visible = value; }
        }
        public void ExpandFirst()
        {
            ctlTreeView.Nodes[0].Expand();
            if (ctlTreeView.SelectedNode != null) ctlTreeView.SelectedNode.EnsureVisible();
        }
        public void ExpandAll()
        {
            ctlTreeView.ExpandAll();
            if (ctlTreeView.SelectedNode != null) ctlTreeView.SelectedNode.EnsureVisible();
        }

        public void CollapseAll()
        {
            ctlTreeView.CollapseAll();
            if (ctlTreeView.SelectedNode != null) ctlTreeView.SelectedNode.EnsureVisible();
        }

        public void SelectFirstNode()
        {
            ctlTreeView.SelectedNode = ctlTreeView.Nodes[0];
            ctlTreeView.SelectedNode.EnsureVisible();
        }

        private void ctlTreeView_DoubleClick(object sender, System.EventArgs e)
        {
            if (CommitSelection != null)
            {
                CommitSelection();
            }
        }

        void ctlTreeView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && CommitSelection != null)
            {
                CommitSelection();
            }
        }

        private void cmdSearch_Click(System.Object sender, System.EventArgs e)
        {
            SearchButtonClicked();
        }

        private void SearchButtonClicked()
        {
            if (txtSearch.Text.Length == 0) return;
            ShowSearchResults = true;
            PopulateSearchResults(txtSearch.Text);
            lstSearchResults.Focus();
        }

        private void cmdClose_Click(object sender, System.EventArgs e)
        {
            CloseButtonClicked();
        }

        private void CloseButtonClicked()
        {
            ShowSearchResults = false;
            txtSearch.Text = "";
        }

        private bool ShowSearchResults
        {
            get { return m_showSearchResults; }
            set
            {
                if (value == m_showSearchResults) return;
                m_showSearchResults = value;

                SuspendLayout();
                lstSearchResults.Visible = value;
                cmdClose.Visible = value;
                ctlTreeView.Visible = !value;
                if (value)
                {
                    txtSearch.Width = cmdClose.Left;
                }
                else
                {
                    txtSearch.Width = cmdSearch.Left;
                }
                ResumeLayout();

                if (!value) SelectCurrentTreeViewItem();
            }
        }

        private void PopulateSearchResults(string search)
        {
            ResizeColumn();
            lstSearchResults.Items.Clear();
            foreach (TreeNode item in ctlTreeView.Nodes)
            {
                AddSearchResultsForNode(item, search, 0);
            }
        }

        private void AddSearchResultsForNode(TreeNode node, string search, int level)
        {
            if (level > 0 || IncludeRootLevelInSearchResults)
            {
                if (node.Text.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) > -1)
                {
                    lstSearchResults.Items.Add(new ListViewItem
                    {
                        Name = node.Name,
                        Text = node.Text
                    });
                }
            }

            foreach (TreeNode item in node.Nodes)
            {
                AddSearchResultsForNode(item, search, level + 1);
            }
        }

        private void lstSearchResults_DoubleClick(object sender, System.EventArgs e)
        {
            CommitSearchSelection();
        }

        private void lstSearchResults_ItemSelectionChanged(object sender, System.Windows.Forms.ListViewItemSelectionChangedEventArgs e)
        {
            if (ShowSearchResults)
            {
                if (lstSearchResults.SelectedItems.Count == 0) return;
                ChangeSelection(lstSearchResults.SelectedItems[0].Name);
            }
        }

        private void txtSearch_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchButtonClicked();
                if (lstSearchResults.Items.Count > 0)
                {
                    lstSearchResults.SelectedItems.Clear();
                    lstSearchResults.Items[0].Selected = true;
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            if (e.KeyCode == Keys.Escape)
            {
                CloseButtonClicked();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void lstSearchResults_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                CommitSearchSelection();
                CloseButtonClicked();
            }
            if (e.KeyCode == Keys.Escape)
            {
                txtSearch.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void CommitSearchSelection()
        {
            if (CommitSelection != null)
            {
                CommitSelection();
            }
        }

        private void EditorTree_Resize(object sender, System.EventArgs e)
        {
            ResizeColumn();
        }

        private void ResizeColumn()
        {
            lstSearchResults.Columns[0].Width = lstSearchResults.Width - SystemInformation.VerticalScrollBarWidth;
        }


        private bool m_includeRootLevelInSearchResults = true;
        public bool IncludeRootLevelInSearchResults
        {
            get { return m_includeRootLevelInSearchResults; }
            set { m_includeRootLevelInSearchResults = value; }
        }

        public void ScrollToTop()
        {
            if (ctlTreeView.Nodes.Count == 0)
                return;
            ctlTreeView.Nodes[0].EnsureVisible();
        }

        private void ctlTreeView_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            HighlightNode(null);
            TreeNode nodeOver = ctlTreeView.GetNodeAt(ctlTreeView.PointToClient(Cursor.Position));
            string element = (string)e.Data.GetData(DataFormats.Text);
            string newParent = nodeOver.Name;
            if (nodeOver != null && CanDrag(element, newParent))
            {
                m_doDragDelegate.Invoke(element, newParent);
            }
        }

        private void ctlTreeView_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            TreeNode nodeOver = ctlTreeView.GetNodeAt(ctlTreeView.PointToClient(Cursor.Position));
            if (nodeOver != null && CanDrag((string)e.Data.GetData(DataFormats.Text), nodeOver.Name))
            {
                e.Effect = DragDropEffects.Move;
                HighlightNode(nodeOver);
            }
            else
            {
                e.Effect = DragDropEffects.None;
                HighlightNode(null);
            }
        }

        private TreeNode m_currentHighlight;
        private Color m_currentHighlightOldForeColor;

        private Color m_currentHighlightOldBackColor;
        private void HighlightNode(TreeNode node)
        {
            if (node == m_currentHighlight) return;

            if (m_currentHighlight != null)
            {
                m_currentHighlight.ForeColor = m_currentHighlightOldForeColor;
                m_currentHighlight.BackColor = m_currentHighlightOldBackColor;
            }

            m_currentHighlight = node;

            if (m_currentHighlight == null) return;

            m_currentHighlightOldForeColor = m_currentHighlight.ForeColor;
            m_currentHighlightOldBackColor = m_currentHighlight.BackColor;
            m_currentHighlight.ForeColor = SystemColors.HighlightText;
            m_currentHighlight.BackColor = SystemColors.Highlight;
        }

        private void ctlTreeView_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
        {
            ctlTreeView.DoDragDrop(((TreeNode)e.Item).Name, DragDropEffects.Move);
        }

        private bool CanDrag(string dragItem, string dragTo)
        {
            if (m_canDragDelegate == null) return false;
            return m_canDragDelegate.Invoke(dragItem, dragTo);
        }

        public void SetCanDragDelegate(Func<string, string, bool> del)
        {
            m_canDragDelegate = del;
        }

        public void SetDoDragDelegate(Action<string, string> del)
        {
            m_doDragDelegate = del;
        }

        public void UnhookDelegates()
        {
            m_canDragDelegate = null;
            m_doDragDelegate = null;
        }

        public string SelectedItem
        {
            get { return m_selection; }
        }

        public void RemoveContextMenu()
        {
            ctlTreeView.ContextMenuStrip = null;
        }

        private void AddHandlers()
        {
            foreach (var item in ctlContextMenu.Items)
            {
                ToolStripMenuItem menuItem = item as ToolStripMenuItem;
                if (menuItem != null)
                {
                    AddHandlers((ToolStripMenuItem)item);
                    string tag = menuItem.Tag as string;
                    if (!string.IsNullOrEmpty(tag))
                    {
                        m_menus.Add(tag, menuItem);
                    }
                }

                ToolStripSeparator menuSep = item as ToolStripSeparator;
                if (menuSep != null)
                {
                    string tag = menuSep.Tag as string;
                    if (!string.IsNullOrEmpty(tag))
                    {
                        m_separators.Add(tag, menuSep);
                    }
                }
            }
        }

        private void AddHandlers(ToolStripMenuItem menu)
        {
            menu.Click += Menu_Click;
        }

        private void Menu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            if (menu.Tag == null) return;

            if (m_handlers.ContainsKey((string)menu.Tag))
            {
                m_handlers[(string)menu.Tag].Invoke();
            }
        }

        public void AddMenuClickHandler(string key, MenuClickHandler handler)
        {
            if (m_handlers.ContainsKey(key))
            {
                m_handlers.Remove(key);
            }

            m_handlers.Add(key, handler);
        }

        private void ctlTreeView_GotFocus(object sender, System.EventArgs e)
        {
            if (TreeGotFocus != null)
            {
                TreeGotFocus();
            }
        }

        private void ctlTreeView_LostFocus(object sender, System.EventArgs e)
        {
            if (TreeLostFocus != null)
            {
                TreeLostFocus();
            }
        }

        public void FocusOnTree()
        {
            ctlTreeView.Focus();
        }

        public void SetMenuVisible(string key, bool visible)
        {
            m_menus[key].Visible = visible;
        }

        public void SetMenuEnabled(string key, bool enabled)
        {
            m_menus[key].Enabled = enabled;
        }

        public void SetMenuSeparatorVisible(string key, bool visible)
        {
            m_separators[key].Visible = visible;
        }

        private void ctlContextMenu_Opening(object sender, CancelEventArgs e)
        {
            ApplyContextMenuIcons();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }
    }
}