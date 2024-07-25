//
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
//
namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Data;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;

    public partial class ActiveForumViewerSettings : ModuleSettingsBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            LoadSettings();

        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            drpForumInstance.SelectedIndexChanged += drpForumInstance_SelectedIndexChanged;
        }

        private void LoadSettings()
        {
            try
            {
                if (! Page.IsPostBack)
                {
                    // Load settings from TabModuleSettings: specific to this instance
                    LoadForums();
                    // Load settings from ModuleSettings: general for all instances
                    if (! (Convert.ToString(Settings[ForumViewerSettingsKeys.AFForumModuleId]) == null))
                    {
                        drpForumInstance.SelectedIndex = drpForumInstance.Items.IndexOf(drpForumInstance.Items.FindByValue(Convert.ToString(Settings[ForumViewerSettingsKeys.AFForumModuleId])));
                        LoadForumGroups(Convert.ToInt32(Settings[ForumViewerSettingsKeys.AFForumModuleId]));
                    }

                    if (! (Convert.ToString(Settings[ForumViewerSettingsKeys.AFTopicsTemplate]) == null))
                    {
                        BindTemplates(Convert.ToInt32(Settings[ForumViewerSettingsKeys.AFForumModuleId]));
                        drpTopicsTemplate.SelectedIndex = drpTopicsTemplate.Items.IndexOf(drpTopicsTemplate.Items.FindByValue(Convert.ToString(Settings[ForumViewerSettingsKeys.AFTopicsTemplate])));
                    }

                    if (! (Convert.ToString(Settings[ForumViewerSettingsKeys.AFForumViewTemplate]) == null))
                    {
                        drpForumViewTemplate.SelectedIndex = drpForumViewTemplate.Items.IndexOf(drpForumViewTemplate.Items.FindByValue(Convert.ToString(Settings[ForumViewerSettingsKeys.AFForumViewTemplate])));
                    }

                    if (! (Convert.ToString(Settings[ForumViewerSettingsKeys.AFTopicTemplate]) == null))
                    {
                        drpTopicTemplate.SelectedIndex = drpTopicTemplate.Items.IndexOf(drpTopicTemplate.Items.FindByValue(Convert.ToString(Settings[ForumViewerSettingsKeys.AFTopicTemplate])));
                    }

                    if (! (Convert.ToString(Settings[ForumViewerSettingsKeys.AFForumGroup]) == null))
                    {
                        drpForum.SelectedIndex = drpForum.Items.IndexOf(drpForum.Items.FindByValue(Convert.ToString(Settings[ForumViewerSettingsKeys.AFForumGroup])));
                    }

                    //If Not CType(Settings["AFEnableToolbar"], String) Is Nothing Then
                    //    chkEnableToolbar.Checked = CType(Settings["AFEnableToolbar"], Boolean)
                    //End If
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
                objModules.UpdateModuleSetting(ModuleId, ForumViewerSettingsKeys.AFTopicsTemplate, drpTopicsTemplate.SelectedItem.Value);
                objModules.UpdateModuleSetting(ModuleId, ForumViewerSettingsKeys.AFTopicTemplate, drpTopicTemplate.SelectedItem.Value);
                objModules.UpdateModuleSetting(ModuleId, ForumViewerSettingsKeys.AFForumViewTemplate, drpForumViewTemplate.SelectedItem.Value);
                objModules.UpdateModuleSetting(ModuleId, ForumViewerSettingsKeys.AFForumModuleId, drpForumInstance.SelectedItem.Value);
                objModules.UpdateModuleSetting(ModuleId, ForumViewerSettingsKeys.AFForumGroup, drpForum.SelectedItem.Value);
                //objModules.UpdateModuleSetting(ModuleId, "AFEnableToolbar", CType(chkEnableToolbar.Checked, String))
                string ForumGroup;
                ForumGroup = drpForum.SelectedItem.Value;
                if ((ForumGroup.IndexOf("GROUPID:", 0) + 1) > 0)
                {
                    objModules.UpdateModuleSetting(ModuleId, ForumViewerSettingsKeys.AFViewType, ForumViewerViewType.GROUP);
                }
                else
                {
                    objModules.UpdateModuleSetting(ModuleId, ForumViewerSettingsKeys.AFViewType, ForumViewerViewType.TOPICS);
                }

                int @int = ForumGroup.IndexOf(":") + 1;
                string sID = ForumGroup.Substring(@int);
                //ForumGroupID = CType(ForumGroup.Substring(ForumGroup.IndexOf(":")), Integer)
                objModules.UpdateModuleSetting(ModuleId, ForumViewerSettingsKeys.AFForumGroupId, sID);
                // Redirect back to the portal home page
                Response.Redirect(Utilities.NavigateURL(TabId), true);
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
            foreach (DotNetNuke.Entities.Modules.ModuleInfo mi in mc.GetModules(PortalId))
            {
                if (mi.DesktopModule.ModuleName.Trim().ToLowerInvariant() == Globals.ModuleName.ToLowerInvariant() && mi.IsDeleted == false)
                {
                    ti = tc.GetTab(mi.TabID, PortalId, false);
                    if (ti != null)
                    {
                        if (ti.IsDeleted == false)
                        {
                            drpForumInstance.Items.Insert(i, new ListItem(ti.TabName + " - " + mi.DesktopModule.ModuleName, Convert.ToString(mi.ModuleID)));
                            i += 1;
                        }
                    }

                }
            }

            drpForumInstance.Items.Insert(0, new ListItem("-- Select a Forum Instance --", "-1"));
        }

        public void LoadForumGroups(int ForumModuleID)
        {
            drpForum.Items.Insert(0, new ListItem("-- Select a Group or Forum --", "-1"));
            IDataReader dr = DataProvider.Instance().Forums_List(PortalId, ForumModuleID, -1, -1, false);
            int i = 1;
            string GroupName = string.Empty;
            string ForumName = string.Empty;
            int ForumID = -1;
            while (dr.Read())
            {
                if (GroupName != Convert.ToString(dr["GroupName"]))
                {
                    drpForum.Items.Insert(i, new ListItem(Convert.ToString(dr["GroupName"]), "GROUPID:" + Convert.ToString(dr["ForumGroupID"])));
                    i += 1;
                    GroupName = Convert.ToString(dr["GroupName"]);
                }

                if (ForumID != Convert.ToInt32(dr["ForumID"]))
                {
                    drpForum.Items.Insert(i, new ListItem("|---" + Convert.ToString(dr["ForumName"]), "FORUMID:" + Convert.ToString(dr["ForumID"])));
                    i += 1;
                    ForumID = Convert.ToInt32(dr["ForumID"]);
                }

            }

            dr.Close();

        }

        private void BindTemplates(int ForumModuleID)
        {
            string sDefault = Utilities.GetSharedResource("[RESX:Default]", true);
            BindTemplateDropDown(ForumModuleID, drpTopicsTemplate, Templates.TemplateTypes.TopicsView, sDefault, "0");
            BindTemplateDropDown(ForumModuleID, drpTopicTemplate, Templates.TemplateTypes.TopicView, sDefault, "0");
            BindTemplateDropDown(ForumModuleID, drpForumViewTemplate, Templates.TemplateTypes.ForumView, sDefault, "0");
        }

        private void drpForumInstance_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindTemplates(Convert.ToInt32(drpForumInstance.SelectedItem.Value));
            LoadForumGroups(Convert.ToInt32(drpForumInstance.SelectedItem.Value));
        }

        public void BindTemplateDropDown(int ForumModuleId, DropDownList drp, Templates.TemplateTypes TemplateType, string DefaultText, string DefaultValue)
        {
            var tc = new TemplateController();
            drp.DataTextField = "Title";
            drp.DataValueField = "TemplateID";
            drp.DataSource = tc.Template_List(PortalId, ForumModuleId, TemplateType);
            drp.DataBind();
            drp.Items.Insert(0, new ListItem(DefaultText, DefaultValue));
        }

    }
}
