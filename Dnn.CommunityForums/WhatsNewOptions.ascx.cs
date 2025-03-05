// Copyright (c) by DNN Community
//
// DNN Community licenses this file to you under the MIT license.
//
// See the LICENSE file in the project root for more information.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public partial class WhatsNewOptions : PortalModuleBase
    {
        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.lnkUpdate.Click += this.lnkUpdate_Click;
            this.lnkCancel.Click += this.lnkCancel_Click;

            this.chkRSS.CheckedChanged += this.chkRSS_Change;
            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "scripts/afutils.js", 102);

            if (!this.Page.IsPostBack)
            {
                this.LoadForm();
            }
        }

        // Private Sub tsTags_Callback(ByVal sender As Object, ByVal e As Modules.ActiveForums.Controls.CallBackEventArgs) Handles tsTags.Callback
        //    tsTags.Datasource = DataProvider.Instance.Tags_Search(PortalId, ModuleId, e.Parameter.ToString + "%")
        //    tsTags.Refresh(e.Output)
        // End Sub
        private void lnkUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Construct Forums
                var forums = string.Empty;
                if (this.GetNodeCount() != this.trForums.CheckedNodes.Count)
                {
                    if (this.trForums.CheckedNodes.Count > 1)
                    {
                        forums = string.Empty;
                        foreach (TreeNode tr in this.trForums.CheckedNodes)
                        {
                            if (tr.Value.Contains("F:"))
                            {
                                forums += tr.Value.Split(':')[1] + ":";
                            }
                        }
                    }
                    else if (this.trForums.CheckedNodes.Count > 0)
                    {
                        var sv = this.trForums.CheckedNodes[0].Value;
                        if (sv.Contains("F:"))
                        {
                            forums = sv.Split(':')[1];
                        }
                    }
                }

                var moduleController = new ModuleController();

                // Load the current settings
                var settings = WhatsNewModuleSettings.CreateFromModuleSettings(DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: this.ModuleId, tabId: this.TabId, ignoreCache: true).ModuleSettings);

                // Update Settings Values
                settings.Forums = forums;

                int rows;
                if (int.TryParse(this.txtNumItems.Text, out rows))
                {
                    settings.Rows = int.Parse(this.txtNumItems.Text);
                }

                settings.Format = this.txtTemplate.Text;
                settings.Header = this.txtHeader.Text;
                settings.Footer = this.txtFooter.Text;
                settings.RSSEnabled = this.chkRSS.Checked;
                settings.TopicsOnly = this.chkTopicsOnly.Checked;
                settings.RandomOrder = this.chkRandomOrder.Checked;
                settings.Tags = this.txtTags.Text;

                if (this.chkRSS.Checked)
                {
                    settings.RSSIgnoreSecurity = this.chkIgnoreSecurity.Checked;
                    settings.RSSIncludeBody = this.chkIncludeBody.Checked;

                    int rssCacheTimeout;
                    if (int.TryParse(this.txtCache.Text, out rssCacheTimeout))
                    {
                        settings.RSSCacheTimeout = rssCacheTimeout;
                    }
                }

                // Save Settings
                settings.Save(moduleController, this.ModuleId);

                // Redirect back to the portal home page
                this.Response.Redirect(Utilities.NavigateURL(this.TabId), false);
                this.Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void lnkCancel_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(Utilities.NavigateURL(this.TabId), false);
            this.Context.ApplicationInstance.CompleteRequest();
        }

        private void chkRSS_Change(object sender, EventArgs e)
        {
            this.pnlRSS.Visible = this.chkRSS.Checked;
            this.rqvTxtCache.Enabled = this.chkRSS.Checked;
        }

        #endregion

        #region Private Methods

        private int GetNodeCount()
        {
            var nc = this.trForums.Nodes.Count;
            foreach (TreeNode node in this.trForums.Nodes)
            {
                nc += 1;
                if (node.ChildNodes.Count <= 0)
                {
                    continue;
                }

                nc += node.ChildNodes.Count;
                foreach (TreeNode cnode in node.ChildNodes)
                {
                    nc += cnode.ChildNodes.Count;

                    if (cnode.ChildNodes.Count <= 0)
                    {
                        continue;
                    }

                    foreach (TreeNode subnode in cnode.ChildNodes)
                    {
                        nc += cnode.ChildNodes.Count;
                    }
                }
            }

            return nc;
        }

        private void LoadForm()
        {
            var moduleSettings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: this.ModuleId, tabId: this.TabId, ignoreCache: false).ModuleSettings;
            var settings = WhatsNewModuleSettings.CreateFromModuleSettings(moduleSettings);

            this.txtNumItems.Text = settings.Rows.ToString();
            this.txtTemplate.Text = settings.Format;
            this.txtHeader.Text = settings.Header;
            this.txtFooter.Text = settings.Footer;
            this.chkRSS.Checked = settings.RSSEnabled;
            this.chkTopicsOnly.Checked = settings.TopicsOnly;
            this.chkRandomOrder.Checked = settings.RandomOrder;
            this.chkIgnoreSecurity.Checked = settings.RSSIgnoreSecurity;
            this.chkIncludeBody.Checked = settings.RSSIncludeBody;
            this.txtCache.Text = settings.RSSCacheTimeout.ToString();
            this.txtTags.Text = settings.Tags;

            this.BindForumsTree();

            if (settings.Forums != string.Empty)
            {
                this.CheckNodes(settings.Forums);
            }

            this.pnlRSS.Visible = this.chkRSS.Checked;
            this.rqvTxtCache.Enabled = this.chkRSS.Checked;
        }

        private void CheckNodes(string forumList)
        {
            var forums = forumList.Split(':');

            // Clear all Nodes
            this.ManageCheck(false);

            foreach (var f in forums)
            {
                if (f.Trim() != string.Empty)
                {
                    this.ManageCheck(false, "F:" + f);
                }
            }
        }

        private void ManageCheck(bool state, string value = "")
        {
            foreach (TreeNode node in this.trForums.Nodes)
            {
                if (!node.Checked)
                {
                    node.Checked = node.Value == value || state;
                    if (node.Checked && node.Parent != null)
                    {
                        node.Parent.Expanded = true;
                    }
                }

                if (node.ChildNodes.Count <= 0)
                {
                    continue;
                }

                foreach (TreeNode cnode in node.ChildNodes)
                {
                    if (!cnode.Checked)
                    {
                        cnode.Checked = cnode.Value == value || state;
                        if (cnode.Checked & cnode.Parent != null)
                        {
                            cnode.Parent.Expanded = true;
                        }
                    }

                    if (cnode.ChildNodes.Count <= 0)
                    {
                        continue;
                    }

                    foreach (TreeNode subnode in cnode.ChildNodes)
                    {
                        if (subnode.Checked)
                        {
                            continue;
                        }

                        subnode.Checked = subnode.Value == value || state;
                        if (subnode.Checked && subnode.Parent != null)
                        {
                            subnode.Parent.Expanded = true;
                        }
                    }
                }
            }
        }

        private void BindForumsTree()
        {
            var trNodes = new TreeNodeCollection();
            TreeNode trGroupNode = null;
            TreeNode trParentNode = null;
            IDataReader reader = null;
            var dt = new DataTable();
            dt.Load(DataProvider.Instance().PortalForums(this.PortalId));

            var tmpGroup = string.Empty;
            var i = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (tmpGroup != row["ForumGroupId"].ToString())
                {
                    trGroupNode = new TreeNode
                    {
                        Text = row["GroupName"].ToString(),
                        ImageUrl = Globals.ModulePath + "images/tree/tree_group.png",
                        ShowCheckBox = true,
                        SelectAction = TreeNodeSelectAction.None,
                        Value = "G:" + row["ForumGroupId"],
                    };
                    trGroupNode.Expanded = i == 0;
                    i += 1;
                    tmpGroup = row["ForumGroupId"].ToString();
                    trNodes.Add(trGroupNode);
                }

                if (Convert.ToInt32(row["ParentForumId"]) == 0)
                {
                    var trNode = new TreeNode
                    {
                        Text = row["ForumName"].ToString(),
                        ImageUrl = Globals.ModulePath + "images/tree/tree_forum.png",
                        ShowCheckBox = true,
                        Expanded = false,
                        SelectAction = TreeNodeSelectAction.None,
                        Value = "F:" + row["ForumId"],
                    };
                    if (this.HasSubForums(Convert.ToInt32(row["ForumId"]), dt))
                    {
                        this.AddChildNodes(trNode, dt, row);
                    }

                    // If trNode.ChildNodes.Count > 0 Then
                    trGroupNode.ChildNodes.Add(trNode);

                    // End If
                }
            }

            foreach (TreeNode tr in trNodes)
            {
                if (tr.ChildNodes.Count > 0)
                {
                    this.trForums.Nodes.Add(tr);
                }
            }
        }

        private void AddChildNodes(TreeNode parentNode, DataTable dt, DataRow dr)
        {
            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToInt32(dr["ForumId"]) != Convert.ToInt32(row["ParentForumId"]))
                {
                    continue;
                }

                var tNode = new TreeNode
                {
                    Text = row["ForumName"].ToString(),
                    ImageUrl = Globals.ModulePath + "tree/tree_forum.png",
                    ShowCheckBox = true,
                    Value = "F:" + row["ForumId"],
                    Checked = false,
                    SelectAction = TreeNodeSelectAction.None,
                };
                parentNode.ChildNodes.Add(tNode);
                this.AddChildNodes(tNode, dt, row);
            }
        }

        private bool HasSubForums(int forumId, DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToInt32(row["ParentForumId"]) == forumId)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var stringWriter = new System.IO.StringWriter();
            var htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);
            string html = stringWriter.ToString();
            html = Utilities.LocalizeControl(html, "WhatsNew.ascx");
            writer.Write(html);
        }

        #endregion

    }
}
