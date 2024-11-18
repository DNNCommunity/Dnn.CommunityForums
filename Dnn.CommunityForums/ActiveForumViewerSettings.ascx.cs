// Copyright (c) 2013-2024 by DNN Community
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

using System.Linq;

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Linq;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;

    public partial class ActiveForumViewerSettings : ModuleSettingsBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            this.LoadSettings();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.drpForumInstance.SelectedIndexChanged += this.drpForumInstance_SelectedIndexChanged;
        }

        private void LoadSettings()
        {
            try
            {
                if (!this.Page.IsPostBack)
                {
                    // Load settings from TabModuleSettings: specific to this instance
                    this.LoadForums();

                    // Load settings from ModuleSettings: general for all instances
                    if (!(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFForumModuleId]) == null))
                    {
                        this.drpForumInstance.SelectedIndex = this.drpForumInstance.Items.IndexOf(this.drpForumInstance.Items.FindByValue(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFForumModuleId])));
                        this.LoadForumGroups(Convert.ToInt32(this.Settings[ForumViewerSettingsKeys.AFForumModuleId]));
                    }

                    if (!(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFTopicsTemplate]) == null))
                    {
                        this.BindTemplates(Convert.ToInt32(this.Settings[ForumViewerSettingsKeys.AFForumModuleId]));
                        this.drpTopicsTemplate.SelectedIndex = this.drpTopicsTemplate.Items.IndexOf(this.drpTopicsTemplate.Items.FindByValue(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFTopicsTemplate])));
                    }

                    if (!(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFForumViewTemplate]) == null))
                    {
                        this.drpForumViewTemplate.SelectedIndex = this.drpForumViewTemplate.Items.IndexOf(this.drpForumViewTemplate.Items.FindByValue(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFForumViewTemplate])));
                    }

                    if (!(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFTopicTemplate]) == null))
                    {
                        this.drpTopicTemplate.SelectedIndex = this.drpTopicTemplate.Items.IndexOf(this.drpTopicTemplate.Items.FindByValue(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFTopicTemplate])));
                    }

                    if (!(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFForumGroup]) == null))
                    {
                        this.drpForum.SelectedIndex = this.drpForum.Items.IndexOf(this.drpForum.Items.FindByValue(Convert.ToString(this.Settings[ForumViewerSettingsKeys.AFForumGroup])));
                    }

                    // If Not CType(Settings["AFEnableToolbar"], String) Is Nothing Then
                    //    chkEnableToolbar.Checked = CType(Settings["AFEnableToolbar"], Boolean)
                    // End If
                }
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        public override void UpdateSettings()
        {
            try
            {
                var objModules = new DotNetNuke.Entities.Modules.ModuleController();

                // Update ModuleSettings
                objModules.UpdateModuleSetting(this.ModuleId, ForumViewerSettingsKeys.AFTopicsTemplate, this.drpTopicsTemplate.SelectedItem.Value);
                objModules.UpdateModuleSetting(this.ModuleId, ForumViewerSettingsKeys.AFTopicTemplate, this.drpTopicTemplate.SelectedItem.Value);
                objModules.UpdateModuleSetting(this.ModuleId, ForumViewerSettingsKeys.AFForumViewTemplate, this.drpForumViewTemplate.SelectedItem.Value);
                objModules.UpdateModuleSetting(this.ModuleId, ForumViewerSettingsKeys.AFForumModuleId, this.drpForumInstance.SelectedItem.Value);
                objModules.UpdateModuleSetting(this.ModuleId, ForumViewerSettingsKeys.AFForumGroup, this.drpForum.SelectedItem.Value);

                // objModules.UpdateModuleSetting(ModuleId, "AFEnableToolbar", CType(chkEnableToolbar.Checked, String))
                string forumGroup;
                forumGroup = this.drpForum.SelectedItem.Value;
                if ((forumGroup.IndexOf("GROUPID:", 0) + 1) > 0)
                {
                    objModules.UpdateModuleSetting(this.ModuleId, ForumViewerSettingsKeys.AFViewType, ForumViewerViewType.GROUP);
                }
                else
                {
                    objModules.UpdateModuleSetting(this.ModuleId, ForumViewerSettingsKeys.AFViewType, ForumViewerViewType.TOPICS);
                }

                int @int = forumGroup.IndexOf(":") + 1;
                string sID = forumGroup.Substring(@int);

                // ForumGroupID = CType(ForumGroup.Substring(ForumGroup.IndexOf(":")), Integer)
                objModules.UpdateModuleSetting(this.ModuleId, ForumViewerSettingsKeys.AFForumGroupId, sID);

                // Redirect back to the portal home page
                this.Response.Redirect(Utilities.NavigateURL(this.TabId), false);
                this.Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        public void LoadForums()
        {
            int i = 0;
            var mc = new DotNetNuke.Entities.Modules.ModuleController();
            var tc = new DotNetNuke.Entities.Tabs.TabController();
            DotNetNuke.Entities.Tabs.TabInfo ti;
            foreach (DotNetNuke.Entities.Modules.ModuleInfo mi in mc.GetModules(this.PortalId))
            {
                if (mi.DesktopModule.ModuleName.Trim().ToLowerInvariant() == Globals.ModuleName.ToLowerInvariant() && mi.IsDeleted == false)
                {
                    ti = tc.GetTab(mi.TabID, this.PortalId, false);
                    if (ti != null)
                    {
                        if (ti.IsDeleted == false)
                        {
                            this.drpForumInstance.Items.Insert(i, new ListItem(ti.TabName + " - " + mi.DesktopModule.ModuleName, Convert.ToString(mi.ModuleID)));
                            i += 1;
                        }
                    }
                }
            }

            this.drpForumInstance.Items.Insert(0, new ListItem("-- Select a Forum Instance --", "-1"));
        }

        public void LoadForumGroups(int forumModuleID)
        {
            this.drpForum.Items.Insert(0, new ListItem("-- Select a Group or Forum --", "-1"));
            var forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(this.ModuleId).OrderBy(f => f.ForumGroup.SortOrder).ThenBy(f => f.SortOrder).ToList();

            int i = 1;
            string groupName = string.Empty;
            int forumID = -1;
            foreach (var forum in forums)
            {
                if (groupName != forum.GroupName)
                {
                    this.drpForum.Items.Insert(i, new ListItem(forum.GroupName, $"GROUPID:{forum.ForumGroupId}"));
                    i += 1;
                    groupName = forum.GroupName;
                }

                if (forumID != forum.ForumID)
                {
                    this.drpForum.Items.Insert(i, new ListItem($"|---{forum.ForumName}", $"FORUMID:{forum.ForumID}"));
                    i += 1;
                    forumID = forum.ForumID;
                }
            }
        }

        private void BindTemplates(int forumModuleID)
        {
            string sDefault = Utilities.GetSharedResource("[RESX:Default]", true);
            this.BindTemplateDropDown(forumModuleID, this.drpTopicsTemplate, Templates.TemplateTypes.TopicsView, sDefault, "0");
            this.BindTemplateDropDown(forumModuleID, this.drpTopicTemplate, Templates.TemplateTypes.TopicView, sDefault, "0");
            this.BindTemplateDropDown(forumModuleID, this.drpForumViewTemplate, Templates.TemplateTypes.ForumView, sDefault, "0");
        }

        private void drpForumInstance_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.BindTemplates(Convert.ToInt32(this.drpForumInstance.SelectedItem.Value));
            this.LoadForumGroups(Convert.ToInt32(this.drpForumInstance.SelectedItem.Value));
        }

        public void BindTemplateDropDown(int forumModuleId, DropDownList drp, Templates.TemplateTypes templateType, string defaultText, string defaultValue)
        {
            var tc = new TemplateController();
            drp.DataTextField = "Title";
            drp.DataValueField = "TemplateID";
            drp.DataSource = tc.Template_List(this.PortalId, forumModuleId, templateType);
            drp.DataBind();
            drp.Items.Insert(0, new ListItem(defaultText, defaultValue));
        }
    }
}
