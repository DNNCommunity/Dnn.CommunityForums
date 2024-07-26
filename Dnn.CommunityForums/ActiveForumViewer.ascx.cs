// Community Forums
// Copyright (c) 2013-2024
// by DNN Community
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
    using System.Linq;
    using System.Web.UI.WebControls;

    public partial class ActiveForumViewer : ForumBase
    {
        #region Event Handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (this.Settings[ForumViewerSettingsKeys.AFForumModuleId] != null)
                {
                    string viewType = Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFViewType]);
                    int tmpModuleId = Convert.ToInt32(this.Settings[ForumViewerSettingsKeys.AFForumModuleId]);
                    int tmpForumId = Convert.ToInt32(this.Settings[ForumViewerSettingsKeys.AFForumGroupId]);
                    if (viewType.ToLowerInvariant() == "topics")
                    {
                        viewType = Views.Topics;
                        this.ctlForumLoader.ForumId = tmpForumId;
                    }
                    else
                    {
                        viewType = Views.ForumView;
                        this.ctlForumLoader.ForumGroupId = tmpForumId;
                    }

                    this.ctlForumLoader.DefaultView = viewType;
                    this.ctlForumLoader.ForumModuleId = tmpModuleId;
                    this.ctlForumLoader.ForumTabId = this.ForumTabId;
                    int tmpForumTabId = DotNetNuke.Entities.Modules.ModuleController.Instance.GetTabModulesByModule(tmpModuleId).FirstOrDefault().TabID;
                    if (tmpForumTabId <= 0) { tmpForumTabId = this.TabId; }
                    this.ctlForumLoader.ForumTabId = tmpForumTabId;
                    this.ctlForumLoader.ModuleConfiguration = this.ModuleConfiguration;
                    if (!(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFTopicsTemplate]) == null))
                    {
                        this.ctlForumLoader.DefaultTopicsViewTemplateId = Convert.ToInt32(this.Settings[ForumViewerSettingsKeys.AFTopicsTemplate]);
                    }

                    if (!(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFForumViewTemplate]) == null))
                    {
                        this.ctlForumLoader.DefaultForumViewTemplateId = Convert.ToInt32(this.Settings[ForumViewerSettingsKeys.AFForumViewTemplate]);
                    }

                    if (!(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFTopicTemplate]) == null))
                    {
                        this.ctlForumLoader.DefaultTopicViewTemplateId = Convert.ToInt32(this.Settings[ForumViewerSettingsKeys.AFTopicTemplate]);
                    }

                    System.Web.UI.HtmlControls.HtmlGenericControl oLink = new System.Web.UI.HtmlControls.HtmlGenericControl("link");
                    oLink.Attributes["rel"] = "stylesheet";
                    oLink.Attributes["type"] = "text/css";
                    oLink.Attributes["href"] = this.Page.ResolveUrl(Globals.ModulePath + "module.css");
                    System.Web.UI.Control oCSS = this.Page.FindControl("CSS");
                    if (oCSS != null)
                    {
                        int iControlIndex = 0;
                        iControlIndex = oCSS.Controls.Count;
                        oCSS.Controls.AddAt(0, oLink);
                    }
                }
                else
                {
                    Label lblMessage = new Label();
                    lblMessage.Text = "Please access the Module Settings page to configure this module.";
                    lblMessage.CssClass = "NormalRed";
                    this.Controls.Add(lblMessage);
                }
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        #endregion
    }
}
