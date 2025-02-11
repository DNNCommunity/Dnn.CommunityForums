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

using DotNetNuke.Services.Log.EventLog;

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public partial class Classic : ForumBase
    {
        private string currView = string.Empty;

        #region Private Members

        #endregion
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.SocialGroupId = -1;
            if (this.Request.QueryString[Literals.GroupId] != null && SimulateIsNumeric.IsNumeric(this.Request.QueryString[Literals.GroupId]))
            {
                this.SocialGroupId = Convert.ToInt32(this.Request.QueryString[Literals.GroupId]);
            }
            this.SetupPage();

#if DEBUG
            //ForumsConfig.Install_Upgrade_CreateForumDefaultSettingsAndSecurity_080200();
            //new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().RemoveUnused(this.ForumModuleId);
            //DotNetNuke.Modules.ActiveForums.Helpers.UpgradeModuleSettings.AddUrlPrefixLikes_080200();
            //ForumsConfig.Install_LikeNotificationType_080200();
            //ForumsConfig.Install_PinNotificationType_080200();
            //ForumsConfig.Sort_PermissionSets_080200();
#endif

            try
            {
                if (this.MainSettings != null && this.MainSettings.InstallDate > Utilities.NullDate())
                {
                    if (this.ForumModuleId < 1)
                    {
                        this.ForumModuleId = this.ModuleId;
                    }

                    string ctl = this.DefaultView;
                    string opts = string.Empty;

                    if (this.Request.Params[ParamKeys.ViewType] != null && this.Request.Params[ParamKeys.ViewType] == Views.Grid && this.Request.Params[ParamKeys.GridType] != null && this.Request.Params[ParamKeys.GridType] == Views.MyPreferences)
                    {
                        ctl = Views.MyPreferences;
                    }
                    else if (this.Request.Params[ParamKeys.ViewType] != null && this.Request.Params[ParamKeys.ViewType] == Views.Grid && this.Request.Params[ParamKeys.GridType] != null && this.Request.Params[ParamKeys.GridType] == Views.MySubscriptions)
                    {
                        ctl = Views.MySubscriptions;
                    }
                    else if (this.Request.Params[ParamKeys.ViewType] != null && this.Request.Params[ParamKeys.ViewType] == Views.Grid && this.Request.Params[ParamKeys.GridType] != null && this.Request.Params[ParamKeys.GridType] == Views.Likes)
                    {
                        ctl = Views.Likes;
                        if (this.Request.QueryString[ParamKeys.ContentId] != null)
                        {
                            opts = $"{ParamKeys.ContentId}={this.Request.QueryString[ParamKeys.ContentId]}";
                        }
                    }
                    else if (this.Request.Params[ParamKeys.ViewType] != null)
                    {
                        ctl = this.Request.Params[ParamKeys.ViewType];
                    }
                    else if (this.Request.Params[Literals.View] != null)
                    {
                        ctl = this.Request.Params[Literals.View];
                    }
                    else if (this.Request.Params[ParamKeys.ViewType] == null & this.ForumId > 0 & this.TopicId <= 0)
                    {
                        ctl = Views.Topics;
                    }
                    else if (this.Request.Params[ParamKeys.ViewType] == null && this.Request.Params[Literals.View] == null & this.TopicId > 0)
                    {
                        ctl = Views.Topic;
                    }
                    else if (this.Settings["amafDefaultView"] != null)
                    {
                        ctl = this.Settings["amafDefaultView"].ToString();
                    }

                    if (this.Request.QueryString[ParamKeys.PageJumpId] != null)
                    {
                        opts = $"{Literals.PageId}={this.Request.QueryString[ParamKeys.PageJumpId]}";
                    }

                    this.currView = ctl;
                    this.GetControl(ctl, opts);

                    if (this.Request.IsAuthenticated)
                    {
                        if (this.MainSettings.UsersOnlineEnabled)
                        {
                            DataProvider.Instance().Profiles_UpdateActivity(this.PortalId, this.UserId);
                        }
                    }
                }
                else
                {
                    string ctlPath = Globals.ModulePath + "controls/_default.ascx";
                    ForumBase ctlDefault = (ForumBase)this.LoadControl(ctlPath);
                    ctlDefault.ID = "ctlConfig";
                    ctlDefault.ModuleConfiguration = this.ModuleConfiguration;
                    this.plhLoader.Controls.Clear();
                    this.plhLoader.Controls.Add(ctlDefault);
                }
            }
            catch (Exception ex)
            {
                // Response.Write(ex.Message)
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        #endregion
        #region Private Methods
        private void GetControl(string view, string options)
        {
            try
            {
                if (!this.Page.IsPostBack)
                {
                    this.plhLoader.Controls.Clear();
                }

                ForumBase ctl = null;
                if (view.ToUpperInvariant() == Views.MyPreferences.ToUpperInvariant() && this.Request.IsAuthenticated)
                {
                    ctl = (ForumBase)this.LoadControl(this.Page.ResolveUrl(Globals.ModulePath + "controls/profile_mypreferences.ascx"));
                }
                else if (view.ToUpperInvariant() == Views.MySubscriptions.ToUpperInvariant() && this.Request.IsAuthenticated)
                {
                    ctl = (ForumBase)this.LoadControl(this.Page.ResolveUrl(Globals.ModulePath + "controls/profile_mysubscriptions.ascx"));
                }
                else if (view.ToUpperInvariant() == "FORUMVIEW")
                {
                    ctl = (ForumBase)new DotNetNuke.Modules.ActiveForums.Controls.ForumView();
                }
                else if (view.ToUpperInvariant() == "ADVANCED")
                {
                    ctl = (ForumBase)this.LoadControl(Globals.ModulePath + "advanced.ascx");
                }
                else if ((view.ToUpperInvariant() == Views.Topics.ToUpperInvariant()) || (view.ToUpperInvariant() == "topics".ToUpperInvariant()))
                {
                    ctl = (ForumBase)new DotNetNuke.Modules.ActiveForums.Controls.TopicsView();
                }
                else if ((view.ToUpperInvariant() == Views.Topic.ToUpperInvariant()) || (view.ToUpperInvariant() == "topic".ToUpperInvariant()))
                {
                    ctl = (ForumBase)new DotNetNuke.Modules.ActiveForums.Controls.TopicView();
                }
                else if (view.ToUpperInvariant() == "USERSETTINGS".ToUpperInvariant() && this.Request.IsAuthenticated)
                {
                    string ctlPath = string.Empty;
                    ctlPath = Globals.ModulePath + "controls/af_profile.ascx";
                    if (!System.IO.File.Exists(Utilities.MapPath(ctlPath)))
                    {
                        ctl = (ForumBase)new DotNetNuke.Modules.ActiveForums.Controls.ForumView();
                    }
                    else
                    {
                        ctl = (ForumBase)this.LoadControl(ctlPath);
                    }
                }
                else
                {
                    // this is where af_post.ascx is used
                    string ctlPath = string.Empty;
                    ctlPath = Globals.ModulePath + "controls/af_" + view + ".ascx";
                    if (!System.IO.File.Exists(Utilities.MapPath(ctlPath)))
                    {
                        ctl = (ForumBase)new DotNetNuke.Modules.ActiveForums.Controls.ForumView();
                    }
                    else
                    {
                        ctl = (ForumBase)this.LoadControl(ctlPath);
                    }
                }

                if (this.ForumId > 0 & this.ForumInfo != null)
                {
                    ctl.ForumInfo = this.ForumInfo;
                }

                ctl.ID = view;
                ctl.ForumId = this.ForumId;
                ctl.ForumModuleId = this.ForumModuleId;
                int tmpForumTabId = DotNetNuke.Entities.Modules.ModuleController.Instance.GetTabModulesByModule(this.ForumModuleId).FirstOrDefault().TabID;
                this.ForumTabId = tmpForumTabId;
                if (this.ForumTabId <= 0)
                {
                    this.ForumTabId = this.TabId;
                }

                ctl.ForumTabId = this.ForumTabId;
                ctl.ForumGroupId = this.ForumGroupId;
                ctl.DefaultForumViewTemplateId = this.DefaultForumViewTemplateId;
                ctl.DefaultTopicsViewTemplateId = this.DefaultTopicsViewTemplateId;
                ctl.DefaultTopicViewTemplateId = this.DefaultTopicViewTemplateId;
                ctl.ParentForumId = this.ParentForumId;
                if (string.IsNullOrEmpty(this.ForumIds))
                {
                    this.ForumIds = this.UserForumsList;
                }

                if (this.SocialGroupId > 0)
                {
                    this.ForumIds = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumIdsBySocialGroup(this.PortalId, this.ForumModuleId, this.SocialGroupId);

                    if (string.IsNullOrEmpty(this.ForumIds))
                    {
                        RoleInfo role = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(portalId: this.PortalId, roleId: this.SocialGroupId);
                        Hashtable htSettings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: this.ModuleId, tabId: this.TabId, ignoreCache: false).TabModuleSettings;
                        if (htSettings == null || htSettings.Count == 0)
                        {
                            htSettings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: this.ModuleId, tabId: this.TabId, ignoreCache: false).ModuleSettings;
                        }

                        if (htSettings == null || htSettings.Count == 0 || !htSettings.ContainsKey("ForumGroupTemplate"))
                        {
                            var ex = new Exception($"Unable to configure forum for Social Group: {this.SocialGroupId}");
                            DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                        }
                        else
                        {
                            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.CreateSocialGroupForum(this.PortalId, this.ModuleId, this.SocialGroupId, Convert.ToInt32(htSettings["ForumGroupTemplate"].ToString()), role.RoleName + " Discussions", role.Description, !role.IsPublic, htSettings["ForumConfig"].ToString());
                            this.ForumIds = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumIdsBySocialGroup(this.PortalId, this.ForumModuleId, this.SocialGroupId);
                        }
                    }
                }

                ctl.ForumIds = this.ForumIds;
                ctl.SocialGroupId = this.SocialGroupId;

                // ctl.PostID = PostID
                ctl.ModuleConfiguration = this.ModuleConfiguration;
                if (!(options == string.Empty))
                {
                    ctl.Params = options;
                }

                ControlsConfig cc = new ControlsConfig();
                cc.AppPath = this.Page.ResolveUrl(Globals.ModulePath);
                cc.ThemePath = this.Page.ResolveUrl(this.MainSettings.ThemeLocation);
                cc.TemplatePath = this.Page.ResolveUrl(this.MainSettings.TemplatePath + "/");
                cc.PortalId = this.PortalId;
                cc.PageId = this.TabId;
                cc.ModuleId = this.ModuleId;
                cc.ForumModuleId = this.ForumModuleId;
                cc.User = this.ForumUser;
                string authorizedViewRoles = this.ModuleConfiguration.InheritViewPermissions ? TabPermissionController.GetTabPermissions(this.TabId, this.PortalId).ToString("VIEW") : this.ModuleConfiguration.ModulePermissions.ToString("VIEW");
                cc.DefaultViewRoles = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetPortalRoleIds(this.PortalId, authorizedViewRoles.Split(';'));
                cc.AdminRoles = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetPortalRoleIds(this.PortalId, this.ModuleConfiguration.ModulePermissions.ToString("EDIT").Split(';'));
                cc.ProfileLink = string.Empty; // GetProfileLink()
                cc.MembersLink = string.Empty; // GetMembersLink()
                this.ControlConfig = cc;
                ctl.ControlConfig = cc;
                this.LinkControls(ctl.Controls);
                if (!this.plhLoader.Controls.Contains(ctl))
                {
                    this.plhLoader.Controls.Add(ctl);
                }

                string sOut = null;

                // TODO: this should be resources instead of harcoded text?
                sOut = System.Environment.NewLine + "<!-- " + DateTime.UtcNow.Year.ToString() + " DNN Community -->" + System.Environment.NewLine;
                sOut += string.Concat("<!-- DNN Community Forums ", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), " -->", System.Environment.NewLine);

                Literal lit = new Literal();
                lit.Text = sOut;
                this.plhLoader.Controls.Add(lit);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                if (ctrl is Controls.ForumRow)
                {
                    ((Controls.ForumRow)ctrl).UserPermSet = this.ForumUser.UserPermSet;
                }

                if (ctrl is Controls.ControlsBase)
                {
                    ((Controls.ControlsBase)ctrl).ControlConfig = this.ControlConfig;
                    ((Controls.ControlsBase)ctrl).ModuleConfiguration = this.ModuleConfiguration;
                }

                if (ctrl.Controls.Count > 0)
                {
                    this.LinkControls(ctrl.Controls);
                }
            }
        }

        private void SetupPage()
        {
            // register style sheets
            if (System.IO.File.Exists(Utilities.MapPath(Globals.ThemesPath + "themes.min.css")))
            {
                ClientResourceManager.RegisterStyleSheet(this.Page, Globals.ThemesPath + "themes.min.css", priority: 11);
            }
            else
            {
                if (System.IO.File.Exists(Utilities.MapPath(Globals.ThemesPath + "themes.css")))
                {
                    ClientResourceManager.RegisterStyleSheet(this.Page, Globals.ThemesPath + "themes.css", priority: 11);
                }
            }

            if (System.IO.File.Exists(Utilities.MapPath(this.MainSettings.ThemeLocation + "theme.min.css")))
            {
                ClientResourceManager.RegisterStyleSheet(this.Page, this.MainSettings.ThemeLocation + "theme.min.css", priority: 12);
            }
            else
            {
                if (System.IO.File.Exists(Utilities.MapPath(this.MainSettings.ThemeLocation + "theme.css")))
                {
                    ClientResourceManager.RegisterStyleSheet(this.Page, this.MainSettings.ThemeLocation + "theme.css", priority: 12);
                }
            }

            if (System.IO.File.Exists(Utilities.MapPath(this.MainSettings.ThemeLocation + "custom/theme.min.css")))
            {
                ClientResourceManager.RegisterStyleSheet(this.Page, this.MainSettings.ThemeLocation + "custom/theme.min.css", priority: 13);
            }
            else
            {
                if (System.IO.File.Exists(Utilities.MapPath(this.MainSettings.ThemeLocation + "custom/theme.css")))
                {
                    ClientResourceManager.RegisterStyleSheet(this.Page, this.MainSettings.ThemeLocation + "custom/theme.css", priority: 13);
                }
            }

            ClientResourceManager.RegisterStyleSheet(this.Page, filePath: $"{Globals.ModulePath}Resources/font-awesome-4.7.0/css/font-awesome.min.css", priority: 10);

            string lang = "en-US";
            if (this.Request.QueryString["language"] != null)
            {
                lang = this.Request.QueryString["language"];
            }

            if (string.IsNullOrEmpty(lang))
            {
                lang = this.PortalSettings.DefaultLanguage;
            }

            if (string.IsNullOrEmpty(lang))
            {
                lang = "en-US";
            }

            ClientAPI.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);

            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "scripts/jquery-searchPopup.js");

            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "scripts/json2009.min.js");
            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "scripts/afcommon.js");
            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "scripts/afutils.js");
            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "active/amlib.js");
            ClientResourceManager.RegisterStyleSheet(this.Page, Globals.ModulePath + "active/am-ui.css");

            StringBuilder sb = new StringBuilder();
            string handlerURL = VirtualPathUtility.ToAbsolute(Globals.ModulePath + "handlers/forumhelper.ashx") + "?TabId=" + this.TabId.ToString() + "&PortalId=" + this.PortalId.ToString() + "&moduleid=" + this.ModuleId + "&language=" + lang;
            sb.AppendFormat("var afHandlerURL = '{0}';", handlerURL);
            sb.AppendLine("var af_imgPath = '" + VirtualPathUtility.ToAbsolute(Globals.ModuleImagesPath) + "';");
            string sLoadImg = string.Empty;
            sLoadImg = "var afSpinLg = new Image();afSpinLg.src='" + VirtualPathUtility.ToAbsolute(Globals.ModulePath + "images/spinner-lg.gif") + "';";
            sLoadImg += "var afSpin = new Image();afSpin.src='" + VirtualPathUtility.ToAbsolute(Globals.ModulePath + "images/spinner.gif") + "';";
            sb.AppendLine(sLoadImg);
            sb.AppendLine(Utilities.LocalizeControl(Utilities.GetFile(Utilities.MapPath(Globals.ModulePath + "scripts/resx.js")), false, true));
            if (HttpContext.Current.Request.IsAuthenticated && this.MainSettings.UsersOnlineEnabled)
            {
                sb.AppendLine("setInterval('amaf_updateuseronline(" + this.ModuleId.ToString() + ")',120000);");
            }

            // Wire up the required jquery plugins: Search Popup
            sb.AppendLine("$(document).ready(function () { $('.aftb-search').afSearchPopup(); });");

            this.Page.ClientScript.RegisterStartupScript(this.Page.GetType(), "afscripts", sb.ToString(), true);

        }

        #endregion
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);
            string html = stringWriter.ToString();
            html = Utilities.ParseToolBar(template: html, portalId: this.PortalId, forumTabId: this.ForumTabId, forumModuleId: this.ForumModuleId, tabId: this.TabId, moduleId: this.ModuleId, forumId: this.ForumId, forumUser: this.ForumUser, requestUri: this.Request.Url, rawUrl: this.Request.RawUrl);
            html = Utilities.LocalizeControl(html);
            writer.Write(html);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.SocialGroupId > 0)
            {
                this.ShowToolbar = false;
            }

            if (this.Request.QueryString["dnnprintmode"] == null)
            {
                if (HttpContext.Current.Items["ShowToolbar"] != null)
                {
                    this.ShowToolbar = bool.Parse(HttpContext.Current.Items["ShowToolbar"].ToString());
                }

                if (this.ShowToolbar == true)
                {
                    LiteralControl lit = new LiteralControl();
                    if (this.ForumTabId <= 0)
                    {
                        this.ForumTabId = DotNetNuke.Entities.Modules.ModuleController.Instance.GetTabModulesByModule(this.ForumModuleId).FirstOrDefault().TabID;
                    }

                    lit.Text = Utilities.BuildToolbar(this.PortalId, this.ForumModuleId, this.ForumTabId, this.ModuleId, this.TabId, this.ForumUser, this.Request.Url, rawUrl: this.Request.RawUrl, HttpContext.Current?.Response?.Cookies["language"]?.Value);
                    this.plhToolbar.Controls.Clear();
                    this.plhToolbar.Controls.Add(lit);
                }
            }

            if (this.ForumId > 0 && this.ForumUser.GetIsMod(this.ForumModuleId))
            {
                Controls.HtmlControlLoader ctl = new Controls.HtmlControlLoader();
                ctl.ControlId = "aftopicedit";
                ctl.Height = "500px";
                ctl.Width = "500px";
                ctl.Name = Utilities.GetSharedResource("[RESX:TopicQuickEdit]");
                ctl.FilePath = Globals.ModulePath + "controls/htmlcontrols/quickedit.ascx";
                this.Controls.Add(ctl);

                ctl = new Controls.HtmlControlLoader();
                ctl.ControlId = "aftopicmove";
                ctl.Height = "350px";
                ctl.Width = "500px";
                ctl.Name = Utilities.GetSharedResource("[RESX:MoveTopicTitle]");
                ctl.FilePath = Globals.ModulePath + "controls/htmlcontrols/movetopic.ascx";
                this.Controls.Add(ctl);
            }
        }
    }
}
