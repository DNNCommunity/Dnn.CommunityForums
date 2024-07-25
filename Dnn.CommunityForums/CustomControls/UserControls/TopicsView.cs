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

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.ActiveForums.Constants;
    using DotNetNuke.Services.Authentication;

    [DefaultProperty("Text"), ToolboxData("<{0}:TopicsView runat=server></{0}:TopicsView>")]
    public class TopicsView : ForumBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Environment));

        #region Private Members
        private string _metaTemplate = "[META][TITLE][PORTALNAME] - [PAGENAME] - [GROUPNAME] - [FORUMNAME][/TITLE][DESCRIPTION][BODY][/DESCRIPTION][KEYWORDS][VALUE][/KEYWORDS][/META]";
        private string _metaTitle = string.Empty;
        private string _metaDescription = string.Empty;
        private string _metaKeywords = string.Empty;
        private string forumName;
        private string groupName;
        // Private ForumGroupId As Integer = 0
        // Private TopicsTemplateId As Integer
        private DataRow drForum;
        private DataRow drSecurity;
        private DataTable dtTopics;
        private DataTable dtAnnounce;
        private DataTable dtSubForums;
        private bool bView = false;
        private bool bRead = false;
        // private bool bReply = false;
        // private bool bCreate = false;
        private bool bPoll = false;
        private bool bDelete = false;
        private bool bEdit = false;
        private bool bLock = false;
        private bool bPin = false;
        private bool bSubscribe = false;
        private bool bModApprove = false;
        private bool bModMove = false;
        private bool bModSplit = false;
        private bool bModDelete = false;
        private bool bModEdit = false;
        private bool bModLock = false;
        private bool bModPin = false;
        private bool bAllowRSS = false;
        private int rowIndex = 1;
        private int pageSize = 20;
        private int topicRowCount = 0;
        private int forumSubscriberCount;
        private string myTheme = "_default";
        private string myThemePath = string.Empty;
        private string lastReplySubjectReplaceTag = string.Empty;
        private bool isSubscribedForum = false;
        private string sGroupURL = string.Empty;
        private string sForumURL = string.Empty;

        #endregion
        #region Public Properties
        public string MetaTemplate
        {
            get
            {
                return this._metaTemplate;
            }

            set
            {
                this._metaTemplate = value;
            }
        }

        public string MetaTitle
        {
            get
            {
                return this._metaTitle;
            }

            set
            {
                this._metaTitle = value;
            }
        }

        public string MetaDescription
        {
            get
            {
                return this._metaDescription;
            }

            set
            {
                this._metaDescription = value;
            }
        }

        public string MetaKeywords
        {
            get
            {
                return this._metaKeywords;
            }

            set
            {
                this._metaKeywords = value;
            }
        }

        private string _forumUrl = string.Empty;

        public string ForumUrl
        {
            get
            {
                return this._forumUrl;
            }

            set
            {
                this._forumUrl = value;
            }
        }
        #endregion

        #region Controls
        protected af_quickjump ctlForumJump = new af_quickjump();

        // Protected WithEvents cbActions As New DotNetNuke.Modules.ActiveForums.Controls.Callback
        // protected Modal ctlModal = new Modal();
        protected ForumView ctlForumSubs = new ForumView();
        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // ctlModal.Callback += ctlModal_Callback;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (this.ForumId < 1)
                {
                    this.Response.Redirect(this.NavigateUrl(this.TabId));
                }

                if (this.ForumInfo == null)
                {
                    this.Response.Redirect(this.NavigateUrl(this.TabId));
                }

                if (this.ForumInfo.Active == false)
                {
                    this.Response.Redirect(this.NavigateUrl(this.TabId));
                }

                this.AppRelativeVirtualPath = "~/";
                this.myTheme = this.MainSettings.Theme;
                this.myThemePath = this.Page.ResolveUrl(this.MainSettings.ThemeLocation);
                int defaultTemplateId = this.ForumInfo.TopicsTemplateId;
                if (this.DefaultTopicsViewTemplateId >= 0)
                {
                    defaultTemplateId = this.DefaultTopicsViewTemplateId;
                }

                string TopicsTemplate = string.Empty;
                TopicsTemplate = TemplateCache.GetCachedTemplate(this.ForumModuleId, "TopicsView", defaultTemplateId);
                if (TopicsTemplate.Contains("[NOTOOLBAR]"))
                {
                    if (HttpContext.Current.Items.Contains("ShowToolbar"))
                    {
                        HttpContext.Current.Items["ShowToolbar"] = false;
                    }
                    else
                    {
                        HttpContext.Current.Items.Add("ShowToolbar", false);
                    }

                    TopicsTemplate = TopicsTemplate.Replace("[NOTOOLBAR]", string.Empty);
                }

                TopicsTemplate = TopicsTemplate.Replace("[PORTALID]", this.PortalId.ToString());
                TopicsTemplate = TopicsTemplate.Replace("[MODULEID]", this.ModuleId.ToString());
                TopicsTemplate = TopicsTemplate.Replace("[TABID]", this.TabId.ToString());
                TopicsTemplate = TopicsTemplate.Replace("[AF:CONTROL:FORUMID]", this.ForumId.ToString());
                TopicsTemplate = TopicsTemplate.Replace("[AF:CONTROL:FORUMGROUPID]", this.ForumGroupId.ToString());
                TopicsTemplate = TopicsTemplate.Replace("[AF:CONTROL:PARENTFORUMID]", this.ParentForumId.ToString());

                this.pageSize = this.MainSettings.PageSize;
                if (this.UserId > 0)
                {
                    this.pageSize = this.UserDefaultPageSize;
                }

                if (this.pageSize < 5)
                {
                    this.pageSize = 10;
                }

                if (this.PageId == 1)
                {
                    this.rowIndex = 0;
                }
                else
                {
                    this.rowIndex = (this.PageId * this.pageSize) - this.pageSize;
                }

                string sort = SortColumns.ReplyCreated;
                if (TopicsTemplate.Contains("[AF:SORT:TOPICCREATED]"))
                {
                    sort = SortColumns.TopicCreated;
                    TopicsTemplate = TopicsTemplate.Replace("[AF:SORT:TOPICCREATED]", string.Empty);
                }

                TopicsTemplate = CheckControls(TopicsTemplate);

                TopicsTemplate = TopicsTemplate.Replace("[AF:SORT:REPLYCREATED]", string.Empty);
                if (TopicsTemplate.Contains("[TOPICS]"))
                {
                    string cacheKey = string.Format(CacheKeys.TopicsViewForUser, this.ModuleId, this.ForumId, this.UserId, HttpContext.Current?.Response?.Cookies["language"]?.Value, this.rowIndex, this.pageSize);
                    DataSet ds = (DataSet)DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheRetrieve(this.ForumModuleId, cacheKey);
                    if (ds == null)
                    {
                        ds = DotNetNuke.Modules.ActiveForums.DataProvider.Instance().UI_TopicsView(this.PortalId, this.ForumModuleId, this.ForumId, this.UserId, this.rowIndex, this.pageSize, this.UserInfo.IsSuperUser, sort);
                        DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.ModuleId, cacheKey, ds);
                    }

                    if (ds.Tables.Count > 0)
                    {
                        this.drForum = ds.Tables[0].Rows[0];
                        this.drSecurity = ds.Tables[1].Rows[0];
                        this.dtSubForums = ds.Tables[2];
                        this.dtTopics = ds.Tables[3];
                        if (this.PageId == 1)
                        {
                            this.dtAnnounce = ds.Tables[4];
                        }

                        this.bView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanView"].ToString(), this.ForumUser.UserRoles);
                        this.bRead = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanRead"].ToString(), this.ForumUser.UserRoles);
                        // bCreate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanCreate"].ToString(), ForumUser.UserRoles);
                        this.bEdit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanEdit"].ToString(), this.ForumUser.UserRoles);
                        this.bDelete = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanDelete"].ToString(), this.ForumUser.UserRoles);
                        // bReply = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanReply"].ToString(), ForumUser.UserRoles);
                        this.bPoll = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanPoll"].ToString(), this.ForumUser.UserRoles);

                        this.bSubscribe = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanSubscribe"].ToString(), this.ForumUser.UserRoles);
                        this.bModMove = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanModMove"].ToString(), this.ForumUser.UserRoles);
                        this.bModSplit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanModSplit"].ToString(), this.ForumUser.UserRoles);
                        this.bModDelete = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanModDelete"].ToString(), this.ForumUser.UserRoles);
                        this.bModApprove = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanModApprove"].ToString(), this.ForumUser.UserRoles);
                        this.bModEdit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanModEdit"].ToString(), this.ForumUser.UserRoles);
                        this.bModPin = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanModPin"].ToString(), this.ForumUser.UserRoles);
                        this.bModLock = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanModLock"].ToString(), this.ForumUser.UserRoles);
                        this.bModApprove = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanModApprove"].ToString(), this.ForumUser.UserRoles);

                        ControlUtils ctlUtils = new ControlUtils();
                        this.sGroupURL = ctlUtils.BuildUrl(this.TabId, this.ModuleId, this.ForumInfo.ForumGroup.PrefixURL, string.Empty, this.ForumInfo.ForumGroupId, -1, -1, -1, string.Empty, 1, -1, this.SocialGroupId);
                        this.sForumURL = ctlUtils.BuildUrl(this.TabId, this.ModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumInfo.ForumGroupId, this.ForumInfo.ForumID, -1, -1, string.Empty, 1, -1, this.SocialGroupId);
                        if (this.bView)
                        {
                            this.forumName = this.drForum["ForumName"].ToString();
                            this.groupName = this.drForum["GroupName"].ToString();
                            this.ForumGroupId = Convert.ToInt32(this.drForum["ForumGroupId"]);
                            // TopicsTemplateId = CInt(drForum("TopicsTemplateId"))
                            try
                            {
                                this.bAllowRSS = Convert.ToBoolean(this.drForum["AllowRSS"]);
                            }
                            catch
                            {
                                this.bAllowRSS = false;
                            }

                            if (this.bRead == false)
                            {
                                this.bAllowRSS = false;
                            }

                            this.topicRowCount = Convert.ToInt32(this.drForum["TopicRowCount"]);
                            this.forumSubscriberCount = Utilities.SafeConvertInt(this.drForum["ForumSubscriberCount"]);
                            if (this.UserId > 0)
                            {
                                this.isSubscribedForum = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(this.PortalId, this.ForumModuleId, this.UserId, this.ForumId);
                            }

                            if (this.MainSettings.UseSkinBreadCrumb)
                            {
                                DotNetNuke.Modules.ActiveForums.Environment.UpdateBreadCrumb(this.Page.Controls, "<a href=\"" + this.sGroupURL + "\">" + this.groupName + "</a>");
                                TopicsTemplate = TopicsTemplate.Replace("<div class=\"afcrumb\">[FORUMMAINLINK] > [FORUMGROUPLINK]</div>", string.Empty);
                            }

                            if (TopicsTemplate.Contains("[META]"))
                            {
                                this.MetaTemplate = TemplateUtils.GetTemplateSection(TopicsTemplate, "[META]", "[/META]");
                                TopicsTemplate = TemplateUtils.ReplaceSubSection(TopicsTemplate, string.Empty, "[META]", "[/META]");
                            }

                            // Parse Meta Template
                            if (!string.IsNullOrEmpty(this.MetaTemplate))
                            {
                                this.MetaTemplate = this.MetaTemplate.Replace("[FORUMNAME]", this.forumName);
                                this.MetaTemplate = this.MetaTemplate.Replace("[GROUPNAME]", this.groupName);
                                PortalSettings settings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
                                string pageName = settings.ActiveTab.Title.Length == 0
                                    ? this.Server.HtmlEncode(settings.ActiveTab.TabName)
                                    : this.Server.HtmlEncode(settings.ActiveTab.Title);
                                this.MetaTemplate = this.MetaTemplate.Replace("[PAGENAME]", pageName);
                                this.MetaTemplate = this.MetaTemplate.Replace("[PORTALNAME]", settings.PortalName);
                                this.MetaTemplate = this.MetaTemplate.Replace("[TAGS]", string.Empty);
                                if (this.MetaTemplate.Contains("[TOPICSUBJECT:"))
                                {
                                    string pattern = "(\\[TOPICSUBJECT:(.+?)\\])";
                                    Regex regExp = new Regex(pattern);
                                    MatchCollection matches = null;
                                    matches = regExp.Matches(this.MetaTemplate);
                                    foreach (Match m in matches)
                                    {
                                        this.MetaTemplate = this.MetaTemplate.Replace(m.Value, string.Empty);
                                    }
                                }

                                this.MetaTemplate = this.MetaTemplate.Replace("[TOPICSUBJECT]", string.Empty);
                                if (this.MetaTemplate.Contains("[BODY:"))
                                {
                                    string pattern = "(\\[BODY:(.+?)\\])";
                                    Regex regExp = new Regex(pattern);
                                    MatchCollection matches = null;
                                    matches = regExp.Matches(this.MetaTemplate);
                                    foreach (Match m in matches)
                                    {
                                        int iLen = Convert.ToInt32(m.Groups[2].Value);
                                        if (this.ForumInfo.ForumDesc.Length > iLen)
                                        {
                                            this.MetaTemplate = this.MetaTemplate.Replace(m.Value, this.ForumInfo.ForumDesc.Substring(0, iLen) + "...");
                                        }
                                        else
                                        {
                                            this.MetaTemplate = this.MetaTemplate.Replace(m.Value, this.ForumInfo.ForumDesc);
                                        }
                                    }
                                }

                                this.MetaTemplate = this.MetaTemplate.Replace("[BODY]", Utilities.StripHTMLTag(this.ForumInfo.ForumDesc));

                                this.MetaTitle = TemplateUtils.GetTemplateSection(this.MetaTemplate, "[TITLE]", "[/TITLE]").Replace("[TITLE]", string.Empty).Replace("[/TITLE]", string.Empty);
                                this.MetaTitle = this.MetaTitle.TruncateAtWord(SEOConstants.MaxMetaTitleLength);
                                this.MetaDescription = TemplateUtils.GetTemplateSection(this.MetaTemplate, "[DESCRIPTION]", "[/DESCRIPTION]").Replace("[DESCRIPTION]", string.Empty).Replace("[/DESCRIPTION]", string.Empty);
                                this.MetaDescription = this.MetaDescription.TruncateAtWord(SEOConstants.MaxMetaDescriptionLength);
                                this.MetaKeywords = TemplateUtils.GetTemplateSection(this.MetaTemplate, "[KEYWORDS]", "[/KEYWORDS]").Replace("[KEYWORDS]", string.Empty).Replace("[/KEYWORDS]", string.Empty);
                            }

                            this.BindTopics(TopicsTemplate);
                        }
                        else
                        {
                            this.Response.Redirect(this.NavigateUrl(this.TabId), true);
                        }

                        try
                        {
                            DotNetNuke.Framework.CDefault tempVar = this.BasePage;
                            DotNetNuke.Modules.ActiveForums.Environment.UpdateMeta(ref tempVar, this.MetaTitle, this.MetaDescription, this.MetaKeywords);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.Message, ex);
                            if (ex.InnerException != null)
                            {
                                Logger.Error(ex.InnerException.Message, ex.InnerException);
                            }
                        }
                    }
                    else
                    {
                        this.Response.Redirect(this.NavigateUrl(this.TabId), true);
                    }

                    if (this.Session["modal_View"] != null)
                    {
                        // LoadModal(Session["modal_View"].ToString(), Session["modal_options"].ToString());
                    }
                }
                else
                {
                    string fs = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.ForumUser.UserRoles, this.PortalId, this.ForumModuleId, "CanEdit");
                    if (!string.IsNullOrEmpty(fs))
                    {
                        this.bModEdit = true;
                    }

                    TopicsTemplate = this.ParseControls(TopicsTemplate);
                    TopicsTemplate = Utilities.LocalizeControl(TopicsTemplate);
                    this.Controls.Add(this.ParseControl(TopicsTemplate));
                    this.LinkControls(this.Controls);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc.Message, exc);
                if (exc.InnerException != null)
                {
                    Logger.Error(exc.InnerException.Message, exc.InnerException);
                }

                this.RenderMessage("[RESX:Error:LoadingTopics]", exc.Message, exc);
            }
        }

        private void BindTopics(string TopicsTemplate)
        {
            string sOutput = TopicsTemplate;
            string subTemplate = string.Empty;

            // Subforum Template

            if (sOutput.Contains("[SUBFORUMS]"))
            {
                if (this.dtSubForums.Rows.Count > 0)
                {
                    subTemplate = TemplateUtils.GetTemplateSection(sOutput, "[SUBFORUMS]", "[/SUBFORUMS]");
                }

                sOutput = TemplateUtils.ReplaceSubSection(sOutput, "<asp:placeholder id=\"plhSubForums\" runat=\"server\" />", "[SUBFORUMS]", "[/SUBFORUMS]");
            }

            // Parse Common Controls
            sOutput = this.ParseControls(sOutput);
            // Parse Topics
            sOutput = this.ParseTopics(sOutput, this.dtTopics, "TOPICS");
            // Parse Announce
            string sAnnounce = TemplateUtils.GetTemplateSection(sOutput, "[ANNOUNCEMENTS]", "[/ANNOUNCEMENTS]");
            if (this.dtAnnounce != null)
            {
                if (this.dtAnnounce.Rows.Count > 0)
                {
                    sAnnounce = this.ParseTopics(sAnnounce, this.dtAnnounce, "ANNOUNCEMENT");
                }
                else
                {
                    sAnnounce = string.Empty;
                }
            }
            else
            {
                sAnnounce = string.Empty;
            }

            sOutput = TemplateUtils.ReplaceSubSection(sOutput, sAnnounce, "[ANNOUNCEMENTS]", "[/ANNOUNCEMENTS]");

            sOutput = Utilities.LocalizeControl(sOutput);
            this.Controls.Add(this.ParseControl(sOutput));
            this.BuildPager();

            PlaceHolder plh = (PlaceHolder)this.FindControl("plhQuickJump");
            if (plh != null)
            {
                this.ctlForumJump = new af_quickjump();
                this.ctlForumJump.ForumModuleId = this.ForumModuleId;
                this.ctlForumJump.ModuleConfiguration = this.ModuleConfiguration;
                this.ctlForumJump.ForumId = this.ForumId;
                this.ctlForumJump.ModuleId = this.ModuleId;
                if (this.ForumId > 0)
                {
                    this.ctlForumJump.ForumInfo = this.ForumInfo;
                }

                plh.Controls.Add(this.ctlForumJump);
            }

            plh = (PlaceHolder)this.FindControl("plhSubForums");
            if (plh != null)
            {
                this.ctlForumSubs = (ForumView)this.LoadControl(typeof(ForumView), null);
                this.ctlForumSubs.ModuleConfiguration = this.ModuleConfiguration;
                this.ctlForumSubs.ForumId = this.ForumId;
                this.ctlForumSubs.Forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.ForumId, this.ForumModuleId).SubForums;
                this.ctlForumSubs.ForumTabId = this.ForumTabId;
                this.ctlForumSubs.ForumModuleId = this.ForumModuleId;
                this.ctlForumSubs.SubsOnly = true;
                this.ctlForumSubs.DisplayTemplate = subTemplate;
                if (this.ForumId > 0)
                {
                    this.ctlForumSubs.ForumInfo = this.ForumInfo;
                }

                plh.Controls.Add(this.ctlForumSubs);
            }

            // Me.Controls.Add(cbActions)
            // this.Controls.Add(ctlModal);
            // LoadCallBackScripts()
        }

        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                if (ctrl is ForumBase)
                {
                    ((ForumBase)ctrl).ModuleConfiguration = this.ModuleConfiguration;

                }

                if (ctrl.Controls.Count > 0)
                {
                    this.LinkControls(ctrl.Controls);
                }
            }
        }

        private string ParseControls(string Template)
        {
            string MyTheme = this.MainSettings.Theme;
            string sOutput = Template;
            sOutput = "<%@ Register TagPrefix=\"ac\" Namespace=\"DotNetNuke.Modules.ActiveForums.Controls\" Assembly=\"DotNetNuke.Modules.ActiveForums\" %>" + sOutput;

            // Forum Drop Downlist
            sOutput = sOutput.Replace("[JUMPTO]", "<asp:placeholder id=\"plhQuickJump\" runat=\"server\" />");
            // Tag Cloud
            sOutput = sOutput.Replace("[AF:CONTROLS:TAGCLOUD]", "<ac:tagcloud ModuleId=\"" + this.ModuleId + "\" PortalId=\"" + this.PortalId + "\" tabid=\"" + this.TabId + "\" runat=\"server\" />");

            sOutput = sOutput.Replace("[FORUMSUBSCRIBERCOUNT]", this.forumSubscriberCount.ToString());

            // Forum Subscription Control
            if (this.bSubscribe)
            {
                Controls.ToggleSubscribe subControl = new Controls.ToggleSubscribe(this.ForumModuleId, this.ForumId, -1, 0);
                subControl.Checked = this.isSubscribedForum;
                subControl.Text = "[RESX:Subscribe]";
                sOutput = sOutput.Replace("[FORUMSUBSCRIBE]", subControl.Render());
            }
            else
            {
                sOutput = sOutput.Replace("[FORUMSUBSCRIBE]", string.Empty);
            }

            if (this.Request.IsAuthenticated)
            {
                sOutput = sOutput.Replace("[MARKFORUMREAD]", "<am:MarkForumRead EnableViewState=\"False\" id=\"amMarkForumRead\" MID=\"" + this.ForumModuleId + "\" runat=\"server\" />");
            }
            else
            {
                sOutput = sOutput.Replace("[MARKFORUMREAD]", string.Empty);
            }

            if (this.CanCreate)
            {
                string[] Params = { };
                if (this.SocialGroupId <= 0)
                {
                    Params = new string[] { ParamKeys.ViewType + "=post", ParamKeys.ForumId + "=" + this.ForumId };
                }
                else
                {
                    Params = new string[] { ParamKeys.ViewType + "=post", ParamKeys.ForumId + "=" + this.ForumId, "GroupId=" + this.SocialGroupId, };
                }

                sOutput = sOutput.Replace("[ADDTOPIC]", "<a href=\"" + this.NavigateUrl(this.TabId, "", Params) + "\" class=\"dnnPrimaryAction\">[RESX:AddTopic]</a>");
            }
            else if (!this.Request.IsAuthenticated)
            {
                DotNetNuke.Abstractions.Portals.IPortalSettings PortalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
                string LoginUrl = PortalSettings.LoginTabId > 0 ? Utilities.NavigateURL(PortalSettings.LoginTabId, "", "returnUrl=" + this.Request.RawUrl) : Utilities.NavigateURL(this.TabId, "", "ctl=login&returnUrl=" + this.Request.RawUrl);

                string onclick = string.Empty;
                if (PortalSettings.EnablePopUps && PortalSettings.LoginTabId == Null.NullInteger && !AuthenticationController.HasSocialAuthenticationEnabled(this))
                {
                    onclick = " onclick=\"return " + UrlUtils.PopUpUrl(HttpUtility.UrlDecode(LoginUrl), this, this.PortalSettings, true, false, 300, 650) + "\"";
                }

                sOutput = sOutput.Replace("[ADDTOPIC]", $"<span class=\"dcf-auth-false-login\">{string.Format(Utilities.GetSharedResource("[RESX:NotAuthorizedTopicPleaseLogin]"), LoginUrl, onclick)}</span>");
            }

            else
            {
                sOutput = sOutput.Replace("[ADDTOPIC]", "<div class=\"amnormal\">[RESX:NotAuthorizedTopic]</div>");
            }

            sOutput = sOutput.Replace("[ADDPOLL]", string.Empty);
            string Url = null;
            if (this.bAllowRSS)
            {

                Url = DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName(this.Request)) + "/DesktopModules/ActiveForums/feeds.aspx?portalid=" + this.PortalId + "&forumid=" + this.ForumId + "&tabid=" + this.TabId + "&moduleid=" + this.ForumModuleId;
                if (this.SocialGroupId > 0)
                {
                    Url += "&GroupId=" + this.SocialGroupId;
                }

                sOutput = sOutput.Replace("[RSSLINK]", "<a href=\"" + Url + "\"><img src=\"" + this.myThemePath + "/images/rss.png\" border=\"0\" alt=\"[RESX:RSS]\" /></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[RSSLINK]", string.Empty);
            }

            if (this.Request.IsAuthenticated)
            {
                Url = this.NavigateUrl(this.TabId, "", new string[] { ParamKeys.ViewType + "=sendto", ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId });
                sOutput = sOutput.Replace("[AF:CONTROL:EMAIL]", "<a href=\"" + Url + "\" rel=\"nofollow\"><img src=\"" + this.myThemePath + "/images/email16.png\" border=\"0\" alt=\"[RESX:EmailThis]\" /></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[AF:CONTROL:EMAIL]", string.Empty);
            }

            if (sOutput.Contains("[AF:CONTROL:ADDTHIS"))
            {
                int inStart = sOutput.IndexOf("[AF:CONTROL:ADDTHIS", 0) + 1 + 19;
                int inEnd = sOutput.IndexOf("]", inStart - 1) + 1;
                sOutput.Remove(inStart, inEnd - inStart + 1);
            }

            sOutput = sOutput.Replace("[MINISEARCH]", "<am:MiniSearch  EnableViewState=\"False\" id=\"amMiniSearch\" MID=\"" + this.ModuleId + "\" TID=\"" + this.TabId + "\" FID=\"" + this.ForumId + "\" runat=\"server\" />");
            sOutput = sOutput.Replace("[PAGER1]", "<am:pagernav id=\"Pager1\"  EnableViewState=\"False\" runat=\"server\" />");
            sOutput = sOutput.Replace("[PAGER2]", "<am:pagernav id=\"Pager2\" runat=\"server\" EnableViewState=\"False\" />");
            if (sOutput.Contains("[PARENTFORUMLINK]"))
            {
                if (this.ForumInfo.ParentForumId > 0)
                {
                    sOutput = sOutput.Replace("[PARENTFORUMLINK]", "<a href=\"" + Utilities.NavigateURL(this.TabId) + "\">" + this.ForumInfo.ParentForumName + "</a>");
                }
                else if (this.ForumInfo.ForumGroupId > 0)
                {
                    sOutput = sOutput.Replace("[PARENTFORUMLINK]", "<a href=\"" + Utilities.NavigateURL(this.TabId) + "\">" + this.ForumInfo.GroupName + "</a>");
                }
            }

            // If String.IsNullOrEmpty(ForumInfo.ParentForumName) Then
            sOutput = sOutput.Replace("[PARENTFORUMNAME]", this.ForumInfo.ParentForumName);
            // End If

            sOutput = sOutput.Replace("[FORUMMAINLINK]", "<a href=\"" + this.NavigateUrl(this.TabId) + "\">[RESX:ForumMain]</a>");
            sOutput = sOutput.Replace("[FORUMGROUPLINK]", "<a href=\"" + this.sGroupURL + "\">" + this.groupName + "</a>");

            sOutput = sOutput.Replace("[FORUMNAME]", this.forumName);
            sOutput = sOutput.Replace("[FORUMID]", this.ForumId.ToString());
            sOutput = sOutput.Replace("[GROUPNAME]", this.groupName);
            if (this.bModDelete)
            {
                sOutput = sOutput.Replace("[ACTIONS:DELETE]", "<a href=\"javascript:void(0)\" onclick=\"amaf_modDel(" + this.ModuleId + "," + this.ForumId + ",[TOPICID]);\" style=\"vertical-align:middle;\" title=\"[RESX:DeleteTopic]\" /><i class=\"fa fa-trash-o fa-fw fa-blue\"></i></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[ACTIONS:DELETE]", string.Empty);
            }

            if (this.bModEdit)
            {
                string[] EditParams = { ParamKeys.ViewType + "=post", "action=te", ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=0-0" };
                sOutput = sOutput.Replace("[ACTIONS:EDIT]", "<a title=\"[RESX:EditTopic]\" href=\"" + this.NavigateUrl(this.TabId, "", EditParams) + "\"><i class=\"fa fa-pencil-square-o fa-fw fa-blue\"></i></a>");
                sOutput = sOutput.Replace("0-0", "[TOPICID]");
                sOutput = sOutput.Replace("[AF:QUICKEDITLINK]", "<a href=\"javascript:void(0)\" title=\"[RESX:TopicQuickEdit]\" onclick=\"amaf_quickEdit(" + this.ModuleId + "," + this.ForumId + ",[TOPICID]);\"><i class=\"fa fa-cog fa-fw fa-blue\"></i></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[AF:QUICKEDITLINK]", string.Empty);
                sOutput = sOutput.Replace("[ACTIONS:EDIT]", string.Empty);
            }

            if (this.bModMove)
            {
                sOutput = sOutput.Replace("[ACTIONS:MOVE]", "<a href=\"javascript:void(0)\" onclick=\"javascript:amaf_openMove(" + this.ModuleId + "," + this.ForumId + ",[TOPICID]);\" title=\"[RESX:MoveTopic]\" style=\"vertical-align:middle;\" /><i class=\"fa fa-exchange fa-rotate-90 fa-blue\"></i></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[ACTIONS:MOVE]", string.Empty);
            }

            if (this.bModLock)
            {
                sOutput = sOutput.Replace("[ACTIONS:LOCK]", "<a href=\"javascript:void(0)\" class=\"dcf-topic-lock-outer\" onclick=\"javascript:if(confirm('[RESX:Confirm:Lock]')){amaf_Lock(" + this.ModuleId + "," + this.ForumId + ",[TOPICID]);};\" title=\"[RESX:LockTopic]\" style=\"vertical-align:middle;\"><i class=\"fa fa-lock fa-fw fa-blue dcf-topic-lock-inner\"></i></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[ACTIONS:LOCK]", string.Empty);
            }

            if (this.bModPin)
            {
                sOutput = sOutput.Replace("[ACTIONS:PIN]", "<a href=\"javascript:void(0)\" class=\"dcf-topic-pin-outer\" onclick=\"javascript:if(confirm('[RESX:Confirm:Pin]')){amaf_Pin(" + this.ModuleId + "," + this.ForumId + ",[TOPICID]);};\" title=\"[RESX:PinTopic]\" style=\"vertical-align:middle;\"><i class=\"fa fa-thumb-tack fa-fw fa-blue dcf-topic-pin-pin dcf-topic-pin-inner\"></i></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[ACTIONS:PIN]", string.Empty);
            }

            sOutput = sOutput.Replace("[FORUMLINK]", "<a href=\"" + this.sForumURL + "\">" + this.forumName + "</a>");

            return sOutput;
        }

        private string ParseTopics(string Template, DataTable Topics, string Section)
        {
            string sOutput = Template;
            sOutput = TemplateUtils.GetTemplateSection(Template, "[" + Section + "]", "[/" + Section + "]");
            string sTopics = string.Empty;
            string MemberListMode = this.MainSettings.MemberListMode;
            var ProfileVisibility = this.MainSettings.ProfileVisibility;
            string UserNameDisplay = this.MainSettings.UserNameDisplay;
            string sLastReply = TemplateUtils.GetTemplateSection(sOutput, "[LASTPOST]", "[/LASTPOST]");
            int iLength = 0;
            if (sLastReply.Contains("[LASTPOSTSUBJECT:"))
            {
                int inStart = sLastReply.IndexOf("[LASTPOSTSUBJECT:", 0) + 1 + 17;
                int inEnd = sLastReply.IndexOf("]", inStart - 1) + 1;
                string sLength = sLastReply.Substring(inStart - 1, inEnd - inStart);
                iLength = Convert.ToInt32(sLength);
            }

            int rowcount = 0;
            this.lastReplySubjectReplaceTag = "[LASTPOSTSUBJECT:" + iLength.ToString() + "]";
            if (Topics == null)
            {
                sOutput = TemplateUtils.ReplaceSubSection(Template, string.Empty, "[" + Section + "]", "[/" + Section + "]");
                return sOutput;
            }

            if (Topics.Rows.Count > 0)
            {
                foreach (DataRow drTopic in Topics.Rows)
                {
                    string sTopicsTemplate = sOutput;
                    int TopicId = Convert.ToInt32(drTopic["TopicId"]);
                    string Subject = HttpUtility.HtmlDecode(Convert.ToString(drTopic["Subject"]));
                    string Summary = HttpUtility.HtmlDecode(Convert.ToString(drTopic["Summary"]));
                    string Body = HttpUtility.HtmlDecode(Convert.ToString(drTopic["Body"]));
                    // Strip comments

                    int AuthorId = Convert.ToInt32(drTopic["AuthorId"]);
                    string AuthorName = Convert.ToString(drTopic["AuthorName"]).ToString().Replace("&amp;#", "&#");
                    string AuthorFirstName = Convert.ToString(drTopic["AuthorFirstName"]).ToString().Replace("&amp;#", "&#");
                    string AuthorLastName = Convert.ToString(drTopic["AuthorLastName"]).ToString().Replace("&amp;#", "&#");
                    string AuthorUserName = Convert.ToString(drTopic["AuthorUserName"]);
                    if (AuthorUserName == string.Empty)
                    {
                        AuthorUserName = AuthorName;
                    }

                    string AuthorDisplayName = Convert.ToString(drTopic["AuthorDisplayName"]).ToString().Replace("&amp;#", "&#");
                    int ReplyCount = Convert.ToInt32(drTopic["ReplyCount"]);
                    int ViewCount = Convert.ToInt32(drTopic["ViewCount"]);
                    int TopicSubscriberCount = Utilities.SafeConvertInt(drTopic["TopicSubscriberCount"]);
                    DateTime DateCreated = Convert.ToDateTime(drTopic["DateCreated"]);
                    int StatusId = Convert.ToInt32(drTopic["StatusId"]);
                    // LastReply info
                    int LastReplyId = Convert.ToInt32(drTopic["LastReplyId"]);
                    string LastReplySubject = HttpUtility.HtmlDecode(Convert.ToString(drTopic["LastReplySubject"]));
                    if (LastReplySubject == "")
                    {
                        LastReplySubject = "RE: " + Subject;
                    }

                    string LastReplySummary = HttpUtility.HtmlDecode(Convert.ToString(drTopic["LastReplySummary"]));
                    if (LastReplySummary == "")
                    {
                        LastReplySummary = Summary;
                    }

                    int LastReplyAuthorId = Convert.ToInt32(drTopic["LastReplyAuthorId"]);
                    string LastReplyAuthorName = Convert.ToString(drTopic["LastReplyAuthorName"]).ToString().Replace("&amp;#", "&#");
                    string LastReplyFirstName = Convert.ToString(drTopic["LastReplyFirstName"]).ToString().Replace("&amp;#", "&#");
                    string LastReplyLastName = Convert.ToString(drTopic["LastReplyLastName"]).ToString().Replace("&amp;#", "&#");
                    string LastReplyUserName = Convert.ToString(drTopic["LastReplyUserName"]);
                    string LastReplyDisplayName = Convert.ToString(drTopic["LastReplyDisplayName"]).ToString().Replace("&amp;#", "&#");
                    DateTime LastReplyDate = Convert.ToDateTime(drTopic["LastReplyDate"]);
                    int UserLastTopicRead = Convert.ToInt32(drTopic["UserLastTopicRead"]);
                    int UserLastReplyRead = Convert.ToInt32(drTopic["UserLastReplyRead"]);
                    bool isLocked = Convert.ToBoolean(drTopic["IsLocked"]);
                    bool isPinned = Convert.ToBoolean(drTopic["IsPinned"]);
                    string TopicURL = drTopic["TopicURL"].ToString();
                    string topicData = drTopic["TopicData"].ToString();
                    if (isLocked)
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("fa-lock", "fa-unlock");
                        sTopicsTemplate = sTopicsTemplate.Replace("[RESX:Lock]", "[RESX:UnLock]");
                        sTopicsTemplate = sTopicsTemplate.Replace("[RESX:LockTopic]", "[RESX:UnLockTopic]");
                        sTopicsTemplate = sTopicsTemplate.Replace("[RESX:Confirm:Lock]", "[RESX:Confirm:UnLock]");
                        sTopicsTemplate = sTopicsTemplate.Replace("[ICONLOCK]", "&nbsp;&nbsp;<i id=\"af-topicsview-lock-" + TopicId.ToString() + "\" class=\"fa fa-lock fa-fw fa-red\"></i>");
                    }
                    else
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[ICONLOCK]", "&nbsp;&nbsp;<i id=\"af-topicsview-lock-" + TopicId.ToString() + "\" class=\"fa fa-fw fa-red\"></i>");
                    }

                    if (isPinned)
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("dcf-topic-pin-pin", "dcf-topic-pin-unpin");
                        sTopicsTemplate = sTopicsTemplate.Replace("[RESX:Pin]", "[RESX:UnPin]");
                        sTopicsTemplate = sTopicsTemplate.Replace("[RESX:PinTopic]", "[RESX:UnPinTopic]");
                        sTopicsTemplate = sTopicsTemplate.Replace("[RESX:Confirm:Pin]", "[RESX:Confirm:UnPin]");
                        sTopicsTemplate = sTopicsTemplate.Replace("[ICONPIN]", "&nbsp;&nbsp;<i id=\"af-topicsview-pin-" + TopicId.ToString() + "\" class=\"fa fa-thumb-tack fa-fw fa-red\"></i>");
                    }
                    else
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[ICONPIN]", "&nbsp;&nbsp;<i id=\"af-topicsview-pin-" + TopicId.ToString() + "\" class=\"fa fa-fw fa-red\"></i>");
                    }

                    if (string.IsNullOrEmpty(topicData))
                    {
                        sTopicsTemplate = TemplateUtils.ReplaceSubSection(sTopicsTemplate, string.Empty, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");
                    }
                    else
                    {
                        string sPropTemplate = TemplateUtils.GetTemplateSection(sTopicsTemplate, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");
                        string sProps = string.Empty;
                        var pl = DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Deserialize(topicData);
                        foreach (var p in pl)
                        {
                            string tmp = sPropTemplate;
                            var pName = HttpUtility.HtmlDecode(p.Name);
                            var pValue = HttpUtility.HtmlDecode(p.Value); tmp = tmp.Replace("[AF:PROPERTY:LABEL]", Utilities.GetSharedResource("[RESX:" + pName + "]"));
                            tmp = tmp.Replace("[AF:PROPERTY:VALUE]", pValue);
                            sTopicsTemplate = sTopicsTemplate.Replace("[AF:PROPERTY:" + pName + ":LABEL]", Utilities.GetSharedResource("[RESX:" + pName + "]"));
                            sTopicsTemplate = sTopicsTemplate.Replace("[AF:PROPERTY:" + pName + ":VALUE]", pValue);
                            string pValueKey = string.Empty;
                            if (!string.IsNullOrEmpty(pValue))
                            {
                                pValueKey = Utilities.CleanName(pValue).ToLowerInvariant();
                            }

                            sTopicsTemplate = sTopicsTemplate.Replace("[AF:PROPERTY:" + pName + ":VALUEKEY]", pValueKey);
                            sProps += tmp;
                        }

                        sTopicsTemplate = TemplateUtils.ReplaceSubSection(sTopicsTemplate, sProps, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");

                    }

                    sTopicsTemplate = sTopicsTemplate.Replace("[TOPICID]", TopicId.ToString());
                    sTopicsTemplate = sTopicsTemplate.Replace("[AUTHORID]", AuthorId.ToString());
                    sTopicsTemplate = sTopicsTemplate.Replace("[FORUMID]", this.ForumId.ToString());
                    sTopicsTemplate = sTopicsTemplate.Replace("[USERID]", this.UserId.ToString());

                    if (UserLastTopicRead == 0 || (UserLastTopicRead > 0 & UserLastReplyRead < this.ReplyId))
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[POSTICON]", "<div><i class=\"fa fa-file-o fa-2x fa-red\"></i></div>");
                    }
                    else
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[POSTICON]", "<div><i class=\"fa fa-file-o fa-2x fa-grey\"></i></div>");
                    }

                    if (!string.IsNullOrEmpty(Summary))
                    {
                        if (!Utilities.HasHTML(Summary))
                        {
                            Summary = Summary.Replace(System.Environment.NewLine, "<br />");
                        }
                    }

                    string sBodyTitle = string.Empty;
                    if (this.bRead)
                    {
                        sBodyTitle = this.GetTitle(Body, AuthorId);
                    }

                    sTopicsTemplate = sTopicsTemplate.Replace("[BODYTITLE]", sBodyTitle);
                    sTopicsTemplate = sTopicsTemplate.Replace("[BODY]", this.GetBody(Body, AuthorId));
                    int BodyLength = -1;
                    string BodyTrim = string.Empty;

                    if (Template.Contains("[BODY:"))
                    {
                        int inStart = Template.IndexOf("[BODY:", 0) + 1 + 5;
                        int inEnd = Template.IndexOf("]", inStart - 1) + 1 - 1;
                        string sLength = Template.Substring(inStart, inEnd - inStart);
                        BodyLength = Convert.ToInt32(sLength);
                        BodyTrim = "[BODY:" + BodyLength.ToString() + "]";
                    }

                    string BodyPlain = string.Empty;
                    if (string.IsNullOrEmpty(Summary) && sTopicsTemplate.Contains("[SUMMARY]") && string.IsNullOrEmpty(BodyTrim))
                    {
                        BodyTrim = "[BODY:250]";
                        BodyLength = 250;
                    }

                    if (!(BodyTrim == string.Empty))
                    {
                        BodyPlain = Body.Replace("<br>", System.Environment.NewLine);
                        BodyPlain = BodyPlain.Replace("<br />", System.Environment.NewLine);
                        BodyPlain = Utilities.StripHTMLTag(BodyPlain);
                        if (BodyLength > 0 & BodyPlain.Length > BodyLength)
                        {
                            BodyPlain = BodyPlain.Substring(0, BodyLength);
                        }

                        BodyPlain = BodyPlain.Replace(System.Environment.NewLine, "<br />");
                        sTopicsTemplate = sTopicsTemplate.Replace(BodyTrim, BodyPlain);
                    }

                    if (string.IsNullOrEmpty(Summary))
                    {
                        Summary = BodyPlain;
                        Summary = Summary.Replace("<br />", "  ");
                    }

                    sTopicsTemplate = sTopicsTemplate.Replace("[SUMMARY]", Summary);
                    string sPollImage = "";
                    if (Convert.ToInt32(drTopic["TopicType"]) == 1)
                    {
                        // sPollImage = "<img src=\"" + MyThemePath + "/images/poll.png\" style=\"vertical-align:middle;\" alt=\"[RESX:Poll]\" />";

                        sPollImage = "&nbsp;<i class=\"fa fa-signal fa-fw fa-red\"></i>";
                    }

                    string sTopicURL = string.Empty;
                    ControlUtils ctlUtils = new ControlUtils();
                    sTopicURL = ctlUtils.BuildUrl(this.TabId, this.ModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumGroupId, this.ForumId, TopicId, TopicURL, -1, -1, string.Empty, 1, -1, this.SocialGroupId);

                    var @params = new List<string> { ParamKeys.TopicId + "=" + TopicId, ParamKeys.ContentJumpId + "=" + LastReplyId };

                    if (this.SocialGroupId > 0)
                    {
                        @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
                    }

                    string sLastReplyURL = this.NavigateUrl(this.TabId, "", @params.ToArray());

                    if (!string.IsNullOrEmpty(sTopicURL))
                    {
                        if (sTopicURL.EndsWith("/"))
                        {
                            sLastReplyURL = sTopicURL + (Utilities.UseFriendlyURLs(this.ForumModuleId) ? String.Concat("#", LastReplyId) : String.Concat("?", ParamKeys.ContentJumpId, "=", LastReplyId));
                        }
                    }

                    string sLastReadURL = string.Empty;
                    string sUserJumpUrl = string.Empty;
                    if (UserLastReplyRead > 0)
                    {
                        @params = new List<string> { $"{ParamKeys.ForumId}={this.ForumId}", $"{ParamKeys.TopicId}={TopicId}", $"{ParamKeys.ViewType}=topic", $"{ParamKeys.FirstNewPost}={UserLastReplyRead}" };
                        if (this.SocialGroupId > 0)
                        {
                            @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
                        }

                        sLastReadURL = this.NavigateUrl(this.TabId, "", @params.ToArray());

                        if (this.MainSettings.UseShortUrls)
                        {
                            @params = new List<string> { ParamKeys.TopicId + "=" + TopicId, ParamKeys.FirstNewPost + "=" + UserLastReplyRead };
                            if (this.SocialGroupId > 0)
                            {
                                @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
                            }

                            sLastReadURL = this.NavigateUrl(this.TabId, "", @params.ToArray());

                        }

                        if (sTopicURL.EndsWith("/"))
                        {
                            sLastReadURL = sTopicURL + (Utilities.UseFriendlyURLs(this.ForumModuleId) ? String.Concat("#", UserLastReplyRead) : String.Concat("?", ParamKeys.FirstNewPost, "=", UserLastReplyRead));
                        }
                    }

                    if (this.UserPrefJumpLastPost && sLastReadURL != string.Empty)
                    {
                        sTopicURL = sLastReadURL;
                        sUserJumpUrl = sLastReadURL;
                    }

                    if (this.UserId == -1 || LastReplyId == 0)
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:ICONLINK:LASTREAD]", string.Empty);
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:URL:LASTREAD]", string.Empty);
                    }
                    else
                    {
                        if ((UserLastTopicRead >= TopicId || UserLastTopicRead == 0) & (UserLastReplyRead >= LastReplyId || UserLastReplyRead == 0))
                        {
                            sTopicsTemplate = sTopicsTemplate.Replace("[AF:ICONLINK:LASTREAD]", string.Empty);
                            sTopicsTemplate = sTopicsTemplate.Replace("[AF:URL:LASTREAD]", string.Empty);
                        }
                        else
                        {
                            sTopicsTemplate = sTopicsTemplate.Replace("[AF:ICONLINK:LASTREAD]", "<a href=\"" + sLastReadURL + "\" rel=\"nofollow\"><img src=\"" + this.myThemePath + "/images/miniarrow_down.png\" style=\"vertical-align:middle;\" alt=\"[RESX:JumpToLastRead]\" border=\"0\" class=\"afminiarrow\" /></a>");
                            sTopicsTemplate = sTopicsTemplate.Replace("[AF:URL:LASTREAD]", sLastReadURL);
                        }

                    }

                    if (LastReplyId > 0 && this.bRead)
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:ICONLINK:LASTREPLY]", "<a href=\"" + sLastReplyURL + "\" rel=\"nofollow\"><img src=\"" + this.myThemePath + "/images/miniarrow_right.png\" style=\"vertical-align:middle;\" alt=\"[RESX:JumpToLastReply]\" border=\"0\" class=\"afminiarrow\" /></a>");
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:URL:LASTREPLY]", sLastReplyURL);
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:UI:MINIPAGER]", this.GetSubPages(this.TabId, ReplyCount, this.ForumId, TopicId));
                    }
                    else
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:ICONLINK:LASTREPLY]", string.Empty);
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:URL:LASTREPLY]", string.Empty);
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:UI:MINIPAGER]", string.Empty);
                    }

                    sTopicsTemplate = sTopicsTemplate.Replace("[TOPICURL]", sTopicURL);
                    Subject = Utilities.StripHTMLTag(Subject);

                    sTopicsTemplate = sTopicsTemplate.Replace("[SUBJECT]", Subject + sPollImage);
                    sTopicsTemplate = sTopicsTemplate.Replace("[SUBJECTLINK]", this.GetTopic(this.TabId, this.ForumId, TopicId, Subject, sBodyTitle, this.UserId, AuthorId, ReplyCount, -1, sTopicURL) + sPollImage);

                    var displayName = UserProfiles.GetDisplayName(this.PortalSettings, this.ForumModuleId, true, this.bModApprove, this.ForumUser.IsAdmin || this.ForumUser.IsSuperUser, AuthorId, AuthorUserName, AuthorFirstName, AuthorLastName, AuthorDisplayName).ToString().Replace("&amp;#", "&#");
                    if (Utilities.StripHTMLTag(displayName) == Utilities.GetSharedResource("[RESX:Anonymous]"))
                    {
                        displayName = displayName.Replace(Utilities.GetSharedResource("[RESX:Anonymous]"), AuthorName);
                    }

                    sTopicsTemplate = sTopicsTemplate.Replace("[STARTEDBY]", displayName);
                    sTopicsTemplate = sTopicsTemplate.Replace("[DATECREATED]", Utilities.GetUserFormattedDateTime(DateCreated, this.PortalId, this.UserId));
                    sTopicsTemplate = sTopicsTemplate.Replace("[REPLIES]", ReplyCount.ToString());
                    sTopicsTemplate = sTopicsTemplate.Replace("[VIEWS]", ViewCount.ToString());
                    sTopicsTemplate = sTopicsTemplate.Replace("[TOPICSUBSCRIBERCOUNT]", TopicSubscriberCount.ToString());
                    sTopicsTemplate = sTopicsTemplate.Replace("[ROWCSS]", this.GetRowCSS(UserLastTopicRead, UserLastReplyRead, TopicId, LastReplyId, rowcount));

                    if (Convert.ToInt32(drTopic["TopicRating"]) == 0)
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[POSTRATINGDISPLAY]", string.Empty);
                    }
                    else
                    {
                        string sRatingImage = null;
                        // sRatingImage = "<img src=""" & MyThemePath & "/yellow_star_0" & drTopic("TopicRating").ToString & ".gif"" alt=""" & drTopic("TopicRating").ToString & """ />"
                        // sRatingImage = "<span class=\"af-rater rate" + drTopic["TopicRating"].ToString() + "\">&nbsp;</span>";

                        sRatingImage = "<span class=\"fa-rater fa-rate" + drTopic["TopicRating"].ToString() + "\"><i class=\"fa fa-star1\"></i><i class=\"fa fa-star2\"></i><i class=\"fa fa-star3\"></i><i class=\"fa fa-star4\"></i><i class=\"fa fa-star5\"></i></span>";
                        sTopicsTemplate = sTopicsTemplate.Replace("[POSTRATINGDISPLAY]", sRatingImage);
                    }

                    if (sTopicsTemplate.Contains("[STATUS]"))
                    {
                        string sImg = string.Empty;
                        if (StatusId == -1)
                        {
                            sTopicsTemplate = sTopicsTemplate.Replace("[STATUS]", string.Empty);
                        }
                        else
                        {
                            // sImg = "<img alt=\"[RESX:PostStatus" + StatusId.ToString() + "]\" src=\"" + MyThemePath + "/images/status" + StatusId.ToString() + ".png\" />";

                            sImg = "<div><i class=\"fa fa-status" + StatusId.ToString() + " fa-red fa-2x\"></i></div>";
                        }

                        sTopicsTemplate = sTopicsTemplate.Replace("[STATUS]", sImg);
                    }

                    if (sTopicsTemplate.Contains("[LASTPOST]"))
                    {
                        if (LastReplyId == 0)
                        {
                            sTopicsTemplate = TemplateUtils.ReplaceSubSection(sTopicsTemplate, Utilities.GetUserFormattedDateTime(LastReplyDate, this.PortalId, this.UserId), "[LASTPOST]", "[/LASTPOST]");
                        }
                        else
                        {
                            string sLastReplyTemp = sLastReply;
                            if (this.bRead)
                            {
                                sLastReplyTemp = sLastReplyTemp.Replace("[AF:ICONLINK:LASTREPLY]", "<a href=\"" + sLastReplyURL + "\" rel=\"nofollow\"><img src=\"" + this.myThemePath + "/images/miniarrow_right.png\" style=\"vertical-align:middle;\" alt=\"[RESX:JumpToLastReply]\" border=\"0\" class=\"afminiarrow\" /></a>");
                                sLastReplyTemp = sLastReplyTemp.Replace("[AF:URL:LASTREPLY]", sLastReplyURL);
                            }
                            else
                            {
                                sLastReplyTemp = sLastReplyTemp.Replace("[AF:ICONLINK:LASTREPLY]", string.Empty);
                                sLastReplyTemp = sLastReplyTemp.Replace("[AF:URL:LASTREPLY]", string.Empty);
                            }

                            sLastReplyTemp = sLastReplyTemp.Replace(this.lastReplySubjectReplaceTag, Utilities.GetLastPostSubject(LastReplyId, TopicId, this.ForumId, this.TabId, LastReplySubject, iLength, this.pageSize, ReplyCount, this.bRead));
                            // sLastReplyTemp = sLastReplyTemp.Replace("[RESX:BY]", Utilities.GetSharedResource("By.Text"))
                            if (LastReplyAuthorId > 0)
                            {
                                sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDISPLAYNAME]", UserProfiles.GetDisplayName(this.PortalSettings, this.ForumModuleId, true, this.bModApprove, this.ForumUser.IsAdmin || this.ForumUser.IsSuperUser, LastReplyAuthorId, LastReplyUserName, LastReplyFirstName, LastReplyLastName, LastReplyDisplayName).ToString().Replace("&amp;#", "&#"));
                            }
                            else
                            {
                                sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDISPLAYNAME]", LastReplyAuthorName);
                            }

                            sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDATE]", Utilities.GetUserFormattedDateTime(LastReplyDate, this.PortalId, this.UserId));
                            sTopicsTemplate = TemplateUtils.ReplaceSubSection(sTopicsTemplate, sLastReplyTemp, "[LASTPOST]", "[/LASTPOST]");
                        }
                    }

                    sTopics += sTopicsTemplate;
                    rowcount += 1;
                }

                sOutput = TemplateUtils.ReplaceSubSection(Template, sTopics, "[" + Section + "]", "[/" + Section + "]");
            }
            else
            {
                sOutput = TemplateUtils.ReplaceSubSection(Template, string.Empty, "[" + Section + "]", "[/" + Section + "]");
            }

            return sOutput;
        }

        private void BuildPager()
        {
            if (this.topicRowCount > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controls.PagerNav Pager1 = null;
                Pager1 = (DotNetNuke.Modules.ActiveForums.Controls.PagerNav)this.FindControl("Pager1");

                DotNetNuke.Modules.ActiveForums.Controls.PagerNav Pager2 = null;
                object obj = this.FindControl("Pager2");
                if (obj != null)
                {
                    Pager2 = (DotNetNuke.Modules.ActiveForums.Controls.PagerNav)obj;
                }

                int intPages = 0;
                if (Pager1 != null)
                {
                    intPages = Convert.ToInt32(System.Math.Ceiling(this.topicRowCount / (double)this.pageSize));
                    Pager1.PageCount = intPages;
                    Pager1.PageMode = PagerNav.Mode.Links;
                    Pager1.BaseURL = URL.ForumLink(this.TabId, this.ForumInfo);
                    Pager1.CurrentPage = this.PageId;
                    Pager1.TabID = Convert.ToInt32(this.Request.Params["TabId"]);
                    Pager1.ForumID = this.ForumId;
                    Pager1.UseShortUrls = this.MainSettings.UseShortUrls;
                    Pager1.PageText = Utilities.GetSharedResource("[RESX:Page]");
                    Pager1.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
                    Pager1.View = Views.Topics;
                    if (this.Request.Params[ParamKeys.Sort] != null)
                    {
                        string[] Params = { $"{ParamKeys.Sort}={this.Request.Params[ParamKeys.Sort]}", "afcol=" + this.Request.Params["afcol"] };
                        Pager1.Params = Params;
                        if (Pager2 != null)
                        {
                            Pager2.Params = Params;
                        }

                    }
                }

                if (Pager2 != null)
                {
                    Pager2.PageMode = Modules.ActiveForums.Controls.PagerNav.Mode.Links;
                    Pager2.BaseURL = URL.ForumLink(this.TabId, this.ForumInfo);
                    Pager2.UseShortUrls = this.MainSettings.UseShortUrls;
                    Pager2.PageCount = intPages;
                    Pager2.CurrentPage = this.PageId;
                    Pager2.TabID = Convert.ToInt32(this.Request.Params["TabId"]);
                    Pager2.ForumID = this.ForumId;
                    Pager2.PageText = Utilities.GetSharedResource("[RESX:Page]");
                    Pager2.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
                    Pager2.View = Views.Topics;
                }
            }

        }

        private string GetIcon(int LastTopicRead, int LastReplyRead, int TopicId, int ReplyId, string Icon, bool Pinned = false, bool Locked = false)
        {
            if (Icon != string.Empty)
            {
                return "<img src=\"" + this.myThemePath + "/emoticons/" + Icon + "\" alt=\"" + Icon + "\" />";
            }

            if (Pinned && Locked)
            {
                return "<img src=\"" + this.myThemePath + "/images/topic_pinlocked.png\" alt=\"[RESX:PinnedLocked]\" />";
            }

            if (Pinned)
            {
                return "<img src=\"" + this.myThemePath + "/images/topic_pin.png\" alt=\"[RESX:Pinned]\" />";
            }

            if (Locked)
            {
                return "<img src=\"" + this.myThemePath + "/images/topic_lock.png\" alt=\"[RESX:Locked]\" />";
            }

            if (!this.Request.IsAuthenticated)
            {
                return "<img src=\"" + this.myThemePath + "/images/topic.png\" alt=\"[RESX:TopicRead]\" />";
            }

            if (LastTopicRead == 0)
            {
                return "<img src=\"" + this.myThemePath + "/images/topic_new.png\" alt=\"[RESX:TopicNew]\" />";
            }

            if (LastTopicRead > 0 & LastReplyRead < ReplyId)
            {
                return "<img src=\"" + this.myThemePath + "/images/topic_new.png\" alt=\"[RESX:TopicNew]\" />";
            }

            return "<img src=\"" + this.myThemePath + "/images/topic.png\" alt=\"[RESX:TopicRead]\" />";
        }

        private string GetTitle(string Body, int AuthorId)
        {
            if (this.bRead || AuthorId == this.UserId)
            {
                Body = HttpUtility.HtmlDecode(Body);
                Body = Body.Replace("<br>", System.Environment.NewLine);
                Body = Utilities.StripHTMLTag(Body);
                Body = Body.Length > 500 ? Body.Substring(0, 500) + "..." : Body;
                Body = Body.Replace("\"", "'");
                Body = Body.Replace("?", string.Empty);
                Body = Body.Replace("+", string.Empty);
                Body = Body.Replace("%", string.Empty);
                Body = Body.Replace("#", string.Empty);
                Body = Body.Replace("@", string.Empty);
                return Body.Trim();
            }
            else
            {
                return string.Empty;
            }

        }

        private string GetBody(string Body, int AuthorId)
        {
            if (this.bRead || AuthorId == this.UserId)
            {
                return HttpUtility.HtmlDecode(Body);
            }
            else
            {
                return string.Empty;
            }

        }

        private string GetTopic(int TabID, int ForumID, int PostID, string Subject, string BodyTitle, int UserID, int PostUserID, int Replies, int ForumOwnerID, string sLink)
        {
            string sOut = null;
            Subject = HttpUtility.HtmlDecode(Subject);
            Subject = Utilities.StripHTMLTag(Subject);
            Subject = Subject.Replace("\"", string.Empty);
            // Subject = Subject.Replace("'", string.Empty);
            Subject = Subject.Replace("#", string.Empty);
            Subject = Subject.Replace("%", string.Empty);
            // Subject = Subject.Replace("?", String.Empty)
            Subject = Subject.Replace("+", string.Empty);

            if (this.bRead)
            {
                if (sLink == string.Empty)
                {
                    string[] Params = { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                    if (this.MainSettings.UseShortUrls)
                    {
                        Params = new string[] { ParamKeys.TopicId + "=" + PostID };
                    }

                    sOut = "<a title=\"" + BodyTitle + "\" href=\"" + this.NavigateUrl(TabID, "", Params) + "\">" + Subject + "</a>";
                }
                else
                {
                    sOut = "<a title=\"" + BodyTitle + "\" href=\"" + sLink + "\">" + Subject + "</a>";
                }
            }
            else
            {
                sOut = Subject;
            }

            return sOut;
        }

        private string GetSubPages(int TabID, int Replies, int ForumID, int PostID)
        {
            int i = 0;
            string sOut = "";

            if (Replies + 1 > this.pageSize)
            {
                // sOut = "<div class=""afpagermini"">" & GetSharedResource("SubPages.Text") & "&nbsp;"
                sOut = "<div class=\"afpagermini\">(<img src=\"" + this.myThemePath + "/images/icon_multipage.png\" alt=\"[RESX:MultiPageTopic]\" style=\"vertical-align:middle;\" />";
                // Jump to pages
                int intPostPages = 0;
                intPostPages = Convert.ToInt32(System.Math.Ceiling((double)(Replies + 1) / this.pageSize));
                if (intPostPages > 3)
                {
                    for (i = 1; i <= 3; i++)
                    {
                        if (this.UseAjax)
                        {
                            var @params = new List<string> { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                            if (this.MainSettings.UseShortUrls)
                            {
                                @params = new List<string> { ParamKeys.TopicId + "=" + PostID };
                            }

                            if (i > 1)
                            {
                                @params.Add(ParamKeys.PageJumpId + "=" + i);
                            }

                            sOut += "<a href=\"" + this.NavigateUrl(TabID, "", @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                        }
                        else
                        {
                            var @params = new List<string> { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                            if (this.MainSettings.UseShortUrls)
                            {
                                @params = new List<string> { ParamKeys.TopicId + "=" + PostID };
                            }

                            if (i > 1)
                            {
                                @params.Add(ParamKeys.PageId + "=" + i);
                            }

                            sOut += "<a href=\"" + this.NavigateUrl(TabID, "", @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                        }

                    }

                    if (intPostPages > 4)
                    {
                        sOut += "...&nbsp;";
                    }

                    if (this.UseAjax)
                    {
                        var @params = new List<string> { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                        if (this.MainSettings.UseShortUrls)
                        {
                            @params = new List<string> { ParamKeys.TopicId + "=" + PostID };
                        }

                        if (i > 1)
                        {
                            @params.Add(ParamKeys.PageJumpId + "=" + i);
                        }

                        sOut += "<a href=\"" + this.NavigateUrl(TabID, "", @params.ToArray()) + "\">" + i + "</a>&nbsp;";

                    }
                    else
                    {
                        var @params = new List<string> { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                        if (this.MainSettings.UseShortUrls)
                        {
                            @params = new List<string> { ParamKeys.TopicId + "=" + PostID };
                        }

                        if (i > 1)
                        {
                            @params.Add(ParamKeys.PageId + "=" + i);
                        }

                        sOut += "<a href=\"" + this.NavigateUrl(TabID, "", @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                    }

                }
                else
                {
                    for (i = 1; i <= intPostPages; i++)
                    {
                        if (this.UseAjax)
                        {
                            // sOut &= "<span class=""afpagerminiitem"" onclick=""javascript:afPageJump(" & i & ");"">" & i & "</span>&nbsp;"
                            var @params = new List<string> { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                            if (this.MainSettings.UseShortUrls)
                            {
                                @params = new List<string> { ParamKeys.TopicId + "=" + PostID };
                            }

                            if (i > 1)
                            {
                                @params.Add(ParamKeys.PageJumpId + "=" + i);
                            }

                            sOut += "<a href=\"" + this.NavigateUrl(TabID, "", @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                        }
                        else
                        {
                            var @params = new List<string> { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                            if (this.MainSettings.UseShortUrls)
                            {
                                @params = new List<string> { ParamKeys.TopicId + "=" + PostID };
                            }

                            if (i > 1)
                            {
                                @params.Add(ParamKeys.PageId + "=" + i);
                            }

                            sOut += "<a href=\"" + this.NavigateUrl(TabID, "", @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                        }

                    }
                }

                sOut += ")</div>";
            }

            return sOut;
        }

        private string GetRowCSS(int LastTopicRead, int LastReplyRead, int TopicId, int ReplyId, int RowCount)
        {
            bool isRead = false;
            if (LastTopicRead >= TopicId && LastReplyRead >= ReplyId)
            {
                isRead = true;
            }

            if (!this.Request.IsAuthenticated)
            {
                isRead = true;
            }

            if (isRead == true)
            {
                if (RowCount % 2 == 0)
                {
                    return "aftopicrow";
                }
                else
                {
                    return "aftopicrowalt";
                }
            }
            else
            {
                if (RowCount % 2 == 0)
                {
                    return "aftopicrownew";
                }
                else
                {
                    return "aftopicrownewalt";
                }
            }
        }

        /*
        private void ctlModal_Callback(object sender, CallBackEventArgs e)
        {
            switch (e.Parameters[0].ToLowerInvariant())
            {
                case "load":
                    ctlModal.ModalContent.Controls.Clear();
                    string ctlPath = string.Empty;
                    string ctrl = e.Parameters[1].ToLowerInvariant();
                    string ctlParams = e.Parameters[2].ToLowerInvariant();
                    Session["modal_View"] = ctrl;
                    Session["modal_options"] = ctlParams;
                    LoadModal(ctrl, ctlParams);
                    break;
                case "clear":
                    ctlModal.ModalContent.Controls.Clear();
                    Session["modal_View"] = null;
                    Session["modal_options"] = null;
                    break;
            }
            ctlModal.ModalContent.RenderControl(e.Output);
        }
        private void LoadModal(string ctrl, string @params = "")
        {
            ctlModal.ModalContent.Controls.Clear();
            string ctlPath = string.Empty;
            Session["modal_View"] = ctrl;
            Session["modal_options"] = @params;
            ctlPath = "~/DesktopModules/activeforums/controls/af_" + ctrl + ".ascx";
            ForumBase ctl = (ForumBase)(LoadControl(ctlPath));
            ctl.ID = ctrl;
            ctl.ModuleConfiguration = this.ModuleConfiguration;

            if (!(@params == string.Empty))
            {
                ctl.Params = @params;
            }
            if (!(ctlModal.ModalContent.Controls.Contains(ctl)))
            {
                ctlModal.ModalContent.Controls.Add(ctl);
            }
        }
         */

        private static string CheckControls(string template)
        {
            const string tagRegistration = "<%@ Register TagPrefix=\"ac\" Namespace=\"DotNetNuke.Modules.ActiveForums.Controls\" Assembly=\"DotNetNuke.Modules.ActiveForums\" %>";

            if (string.IsNullOrWhiteSpace(template))
            {
                return string.Empty;
            }

            if (!template.Contains(tagRegistration))
            {
                template = tagRegistration + template;
            }

            return template;
        }
    }
}
