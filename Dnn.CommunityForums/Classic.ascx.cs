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
//using DotNetNuke.Framework.JavaScriptLibraries;

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

        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi;
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

            SocialGroupId = -1;
            if (Request.QueryString[Literals.GroupId] != null && SimulateIsNumeric.IsNumeric(Request.QueryString[Literals.GroupId]))
            {
                SocialGroupId = Convert.ToInt32(Request.QueryString[Literals.GroupId]);
            }

            SetupPage();

            try
            {
                if (MainSettings != null && MainSettings.InstallDate > Utilities.NullDate())
                {
                    if (Request.IsAuthenticated && UserLastAccess == Utilities.NullDate())
                    {

                        if (ForumUser != null)
                        {
                            DateTime dtLastAccess = DateTime.UtcNow;
                            if (!(ForumUser.Profile.DateLastActivity == Utilities.NullDate()))
                            {
                                dtLastAccess = ForumUser.Profile.DateLastActivity;
                            }
                            UserLastAccess = dtLastAccess;
                        }
                    }
                    if (ForumModuleId < 1)
                    {
                        ForumModuleId = ModuleId;
                    }

                    string ctl = DefaultView;
                    string opts = string.Empty;

                    if (Request.Params[ParamKeys.ViewType] != null && Request.Params[ParamKeys.ViewType] == Views.Grid && Request.Params[ParamKeys.GridType] != null && Request.Params[ParamKeys.GridType] == Views.MyPreferences)
                    {
                        ctl = Views.MyPreferences;
                    }
                    else if (Request.Params[ParamKeys.ViewType] != null && Request.Params[ParamKeys.ViewType] == Views.Grid && Request.Params[ParamKeys.GridType] != null && Request.Params[ParamKeys.GridType] == Views.MySubscriptions)
                    {
                        ctl = Views.MySubscriptions;
                    }
                    else if (Request.Params[ParamKeys.ViewType] != null)
                    {
                        ctl = Request.Params[ParamKeys.ViewType];
                    }
                    else if (Request.Params[Literals.view] != null)
                    {
                        ctl = Request.Params[Literals.view];
                    }
                    else if (Request.Params[ParamKeys.ViewType] == null & ForumId > 0 & TopicId <= 0)
                    {
                        ctl = Views.Topics;
                    }
                    else if (Request.Params[ParamKeys.ViewType] == null && Request.Params[Literals.view] == null & TopicId > 0)
                    {
                        ctl = Views.Topic;
                    }
                    else if (Settings["amafDefaultView"] != null)
                    {
                        ctl = Settings["amafDefaultView"].ToString();
                    }
                    if (Request.QueryString[ParamKeys.PageJumpId] != null)
                    {
                        opts = $"{Literals.PageId}={Request.QueryString[ParamKeys.PageJumpId]}";
                    }
                    currView = ctl;
                    GetControl(ctl, opts);

                    if (Request.IsAuthenticated)
                    {
                        if (MainSettings.UsersOnlineEnabled)
                        {
                            DataProvider.Instance().Profiles_UpdateActivity(PortalId, ForumModuleId, UserId);
                        }
                    }
                }
                else
                {

                    string ctlPath = Globals.ModulePath + "controls/_default.ascx";
                    ForumBase ctlDefault = (ForumBase)LoadControl(ctlPath);
                    ctlDefault.ID = "ctlConfig";
                    ctlDefault.ModuleConfiguration = this.ModuleConfiguration;
                    plhLoader.Controls.Clear();
                    plhLoader.Controls.Add(ctlDefault);

                }

            }
            catch (Exception ex)
            {
                //Response.Write(ex.Message)
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        #endregion
        #region Private Methods
        private void GetControl(string view, string options)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    plhLoader.Controls.Clear();
                }
                ForumBase ctl = null;
                if (view.ToUpperInvariant() == Views.MyPreferences.ToUpperInvariant() && Request.IsAuthenticated)
                {
                    ctl = (ForumBase)LoadControl(Page.ResolveUrl(Globals.ModulePath + "controls/profile_mypreferences.ascx"));
                }
                else if (view.ToUpperInvariant() == Views.MySubscriptions.ToUpperInvariant() && Request.IsAuthenticated)
                {
                    ctl = (ForumBase)LoadControl(Page.ResolveUrl(Globals.ModulePath + "controls/profile_mysubscriptions.ascx"));
                }
                else if (view.ToUpperInvariant() == "FORUMVIEW")
                {
                    ctl = (ForumBase)new DotNetNuke.Modules.ActiveForums.Controls.ForumView();
                }
                else if (view.ToUpperInvariant() == "ADVANCED")
                {
                    ctl = (ForumBase)LoadControl(Globals.ModulePath + "advanced.ascx");
                }
                else if ((view.ToUpperInvariant() == Views.Topics.ToUpperInvariant()) || (view.ToUpperInvariant() == "topics".ToUpperInvariant()))
                {
                    ctl = (ForumBase)new DotNetNuke.Modules.ActiveForums.Controls.TopicsView();
                }
                else if ((view.ToUpperInvariant() == Views.Topic.ToUpperInvariant()) || (view.ToUpperInvariant() == "topic".ToUpperInvariant()))
                {
                    ctl = (ForumBase)new DotNetNuke.Modules.ActiveForums.Controls.TopicView();
                }
                else if (view.ToUpperInvariant() == "USERSETTINGS".ToUpperInvariant() && Request.IsAuthenticated)
                {
                    string ctlPath = string.Empty;
                    ctlPath = Globals.ModulePath + "controls/af_profile.ascx";
                    if (!System.IO.File.Exists(Utilities.MapPath(ctlPath)))
                    {
                        ctl = (ForumBase)new DotNetNuke.Modules.ActiveForums.Controls.ForumView();
                    }
                    else
                    {
                        ctl = (ForumBase)LoadControl(ctlPath);
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
                        ctl = (ForumBase)LoadControl(ctlPath);
                    }

                }
                if (ForumId > 0 & ForumInfo != null)
                {
                    ctl.ForumInfo = ForumInfo;
                }
                ctl.ID = view;
                ctl.ForumId = ForumId;
                ctl.ForumModuleId = ForumModuleId;
                int tmpForumTabId = DotNetNuke.Entities.Modules.ModuleController.Instance.GetTabModulesByModule(ForumModuleId).FirstOrDefault().TabID;
                ForumTabId = tmpForumTabId;
                if (ForumTabId <=0)
                {
                    ForumTabId = TabId;
                }
                ctl.ForumTabId = ForumTabId;
                ctl.ForumGroupId = ForumGroupId;
                ctl.DefaultForumViewTemplateId = DefaultForumViewTemplateId;
                ctl.DefaultTopicsViewTemplateId = DefaultTopicsViewTemplateId;
                ctl.DefaultTopicViewTemplateId = DefaultTopicViewTemplateId;
                ctl.ParentForumId = ParentForumId;
                if (string.IsNullOrEmpty(ForumIds))
                {
                    ForumIds = UserForumsList;
                }

                if (SocialGroupId > 0)
                {
                    ForumIds = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumIdsBySocialGroup(PortalId, ForumModuleId, SocialGroupId);

                    if (string.IsNullOrEmpty(ForumIds))
                    {
                        RoleInfo role = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(portalId: PortalId, roleId: SocialGroupId);
                        //Create new foportalId: rum
                        bool isPrivate = false;
                        if (!role.IsPublic)
                        {
                            isPrivate = true;
                        }
                        Hashtable htSettings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: ModuleId, tabId: TabId, ignoreCache: false).TabModuleSettings;

                        DotNetNuke.Modules.ActiveForums.Controllers.ForumController.CreateSocialGroupForum(PortalId, ModuleId, SocialGroupId, Convert.ToInt32(htSettings["ForumGroupTemplate"].ToString()), role.RoleName + " Discussions", role.Description, isPrivate, htSettings["ForumConfig"].ToString());
                        ForumIds = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumIdsBySocialGroup(PortalId, ForumModuleId, SocialGroupId);
                    }
                }
                ctl.ForumIds = ForumIds;
                ctl.SocialGroupId = SocialGroupId;

                //ctl.PostID = PostID
                ctl.ModuleConfiguration = this.ModuleConfiguration;
                if (!(options == string.Empty))
                {
                    ctl.Params = options;
                }
                ControlsConfig cc = new ControlsConfig();
                cc.AppPath = Page.ResolveUrl(Globals.ModulePath);
                cc.ThemePath = Page.ResolveUrl(MainSettings.ThemeLocation);
                cc.TemplatePath = Page.ResolveUrl(MainSettings.TemplatePath + "/");
                cc.PortalId = PortalId;
                cc.PageId = TabId;
                cc.ModuleId = ModuleId;
                cc.User = ForumUser;
                string authorizedViewRoles = ModuleConfiguration.InheritViewPermissions ? TabPermissionController.GetTabPermissions(TabId, PortalId).ToString("VIEW") : ModuleConfiguration.ModulePermissions.ToString("VIEW");
                cc.DefaultViewRoles = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(PortalId, authorizedViewRoles.Split(';'));
                cc.AdminRoles = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(PortalId, this.ModuleConfiguration.ModulePermissions.ToString("EDIT").Split(';'));
                cc.ProfileLink = ""; //GetProfileLink()
                cc.MembersLink = ""; // GetMembersLink()
                this.ControlConfig = cc;
                ctl.ControlConfig = cc;
                LinkControls(ctl.Controls);
                if (!plhLoader.Controls.Contains(ctl))
                {
                    plhLoader.Controls.Add(ctl);
                }
                string sOut = null;
                //TODO: this should be resources instead of harcoded text?
                sOut = System.Environment.NewLine + "<!-- " + DateTime.UtcNow.Year.ToString() + " DNN Community -->" + System.Environment.NewLine;
                sOut +=  string.Concat("<!-- DNN Community Forums ", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), " -->" , System.Environment.NewLine);

                Literal lit = new Literal();
                lit.Text = sOut;
                plhLoader.Controls.Add(lit);
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
                    ((Controls.ForumRow)ctrl).UserRoles = ForumUser.UserRoles;
                }
                if (ctrl is Controls.ControlsBase)
                {
                    ((Controls.ControlsBase)ctrl).ControlConfig = this.ControlConfig;
                    ((Controls.ControlsBase)ctrl).ForumData = this.ForumData;
                    ((Controls.ControlsBase)ctrl).ModuleConfiguration = this.ModuleConfiguration;
                }
                if (ctrl.Controls.Count > 0)
                {
                    LinkControls(ctrl.Controls);
                }
            }
        }

        private void SetupPage()
        {
            //register style sheets
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
            if (System.IO.File.Exists(Utilities.MapPath(MainSettings.ThemeLocation + "theme.min.css")))
            {
                ClientResourceManager.RegisterStyleSheet(this.Page, MainSettings.ThemeLocation + "theme.min.css", priority: 12);
            }
            else
            {
                if (System.IO.File.Exists(Utilities.MapPath(MainSettings.ThemeLocation + "theme.css")))
                {
                    ClientResourceManager.RegisterStyleSheet(this.Page, MainSettings.ThemeLocation + "theme.css", priority: 12);
                }
            }
            if (System.IO.File.Exists(Utilities.MapPath(MainSettings.ThemeLocation + "custom/theme.min.css")))
            {
                ClientResourceManager.RegisterStyleSheet(this.Page, MainSettings.ThemeLocation + "custom/theme.min.css", priority: 13);
            }
            else
            {
                if (System.IO.File.Exists(Utilities.MapPath(MainSettings.ThemeLocation + "custom/theme.css")))
                {
                    ClientResourceManager.RegisterStyleSheet(this.Page, MainSettings.ThemeLocation + "custom/theme.css", priority: 13);
                }
            }

            ClientResourceManager.RegisterStyleSheet(Page, filePath: $"{Globals.ModulePath}Resources/font-awesome-4.7.0/css/font-awesome.min.css", priority: 10);

            string lang = "en-US";
            if (Request.QueryString["language"] != null)
            {
                lang = Request.QueryString["language"];
            }
            if (string.IsNullOrEmpty(lang))
            {
                lang = PortalSettings.DefaultLanguage;
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

            StringBuilder sb = new StringBuilder();
            string handlerURL = VirtualPathUtility.ToAbsolute(Globals.ModulePath + "handlers/forumhelper.ashx") + "?TabId=" + TabId.ToString() + "&PortalId=" + PortalId.ToString() + "&moduleid=" + ModuleId + "&language=" + lang;
            sb.AppendFormat("var afHandlerURL = '{0}';", handlerURL);
            sb.AppendLine("var af_imgPath = '" + VirtualPathUtility.ToAbsolute(Globals.ModuleImagesPath) + "';");
            string sLoadImg = "";
            sLoadImg = "var afSpinLg = new Image();afSpinLg.src='" + VirtualPathUtility.ToAbsolute(Globals.ModulePath + "images/spinner-lg.gif") + "';";
            sLoadImg += "var afSpin = new Image();afSpin.src='" + VirtualPathUtility.ToAbsolute(Globals.ModulePath + "images/spinner.gif") + "';";
            sb.AppendLine(sLoadImg);
            sb.AppendLine(Utilities.LocalizeControl(Utilities.GetFile(Utilities.MapPath(Globals.ModulePath + "scripts/resx.js")), false, true));
            if (HttpContext.Current.Request.IsAuthenticated && MainSettings.UsersOnlineEnabled)
            {
                sb.AppendLine("setInterval('amaf_updateuseronline(" + ModuleId.ToString() + ")',120000);");
            }

            // Wire up the required jquery plugins: Search Popup
            sb.AppendLine("$(document).ready(function () { $('.aftb-search').afSearchPopup(); });");

            Page.ClientScript.RegisterStartupScript(Page.GetType(), "afscripts", sb.ToString(), true);

            if (ForumUser.Profile.IsMod)
            {
                ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "scripts/afmod.js");
                ClientResourceManager.RegisterStyleSheet(this.Page, Globals.ModulePath + "active/am-ui.css");
            }

        }

        #endregion
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);
            string html = stringWriter.ToString();
            html = Utilities.ParseToolBar(template: html, forumTabId: ForumTabId, forumModuleId: ForumModuleId, tabId: TabId, moduleId: ModuleId, currentUserType: CurrentUserType, forumId: ForumId);
            html = Utilities.LocalizeControl(html);
            writer.Write(html);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (SocialGroupId > 0)
            {
                ShowToolbar = false;
            }

            if (Request.QueryString["dnnprintmode"] == null)
            {
                if (HttpContext.Current.Items["ShowToolbar"] != null)
                {
                    ShowToolbar = bool.Parse(HttpContext.Current.Items["ShowToolbar"].ToString());
                }

                if (ShowToolbar == true)
                {
                    LiteralControl lit = new LiteralControl();
                    if (ForumTabId <= 0)
                    {
                        ForumTabId = DotNetNuke.Entities.Modules.ModuleController.Instance.GetTabModulesByModule(ForumModuleId).FirstOrDefault().TabID;
                    }
                    lit.Text = Utilities.BuildToolbar(ForumModuleId, ForumTabId, ModuleId, TabId, CurrentUserType, HttpContext.Current?.Response?.Cookies["language"]?.Value);
                    plhToolbar.Controls.Clear();
                    plhToolbar.Controls.Add(lit);
                }

            }

            if (ForumId > 0 && UserIsMod)
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
