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
        private string metaTemplate = "[META][TITLE][PORTALNAME] - [PAGENAME] - [GROUPNAME] - [FORUMNAME][/TITLE][DESCRIPTION][BODY][/DESCRIPTION][KEYWORDS][VALUE][/KEYWORDS][/META]";
        private string metaTitle = string.Empty;
        private string metaDescription = string.Empty;
        private string metaKeywords = string.Empty;
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
                return this.metaTemplate;
            }

            set
            {
                this.metaTemplate = value;
            }
        }

        public string MetaTitle
        {
            get
            {
                return this.metaTitle;
            }

            set
            {
                this.metaTitle = value;
            }
        }

        public string MetaDescription
        {
            get
            {
                return this.metaDescription;
            }

            set
            {
                this.metaDescription = value;
            }
        }

        public string MetaKeywords
        {
            get
            {
                return this.metaKeywords;
            }

            set
            {
                this.metaKeywords = value;
            }
        }

        private string forumUrl = string.Empty;

        public string ForumUrl
        {
            get
            {
                return this.forumUrl;
            }

            set
            {
                this.forumUrl = value;
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

                string topicsTemplate = string.Empty;
                topicsTemplate = TemplateCache.GetCachedTemplate(this.ForumModuleId, "TopicsView", defaultTemplateId);
                if (topicsTemplate.Contains("[NOTOOLBAR]"))
                {
                    if (HttpContext.Current.Items.Contains("ShowToolbar"))
                    {
                        HttpContext.Current.Items["ShowToolbar"] = false;
                    }
                    else
                    {
                        HttpContext.Current.Items.Add("ShowToolbar", false);
                    }

                    topicsTemplate = topicsTemplate.Replace("[NOTOOLBAR]", string.Empty);
                }

                topicsTemplate = topicsTemplate.Replace("[PORTALID]", this.PortalId.ToString());
                topicsTemplate = topicsTemplate.Replace("[MODULEID]", this.ModuleId.ToString());
                topicsTemplate = topicsTemplate.Replace("[TABID]", this.TabId.ToString());
                topicsTemplate = topicsTemplate.Replace("[AF:CONTROL:FORUMID]", this.ForumId.ToString());
                topicsTemplate = topicsTemplate.Replace("[AF:CONTROL:FORUMGROUPID]", this.ForumGroupId.ToString());
                topicsTemplate = topicsTemplate.Replace("[AF:CONTROL:PARENTFORUMID]", this.ParentForumId.ToString());

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
                if (topicsTemplate.Contains("[AF:SORT:TOPICCREATED]"))
                {
                    sort = SortColumns.TopicCreated;
                    topicsTemplate = topicsTemplate.Replace("[AF:SORT:TOPICCREATED]", string.Empty);
                }

                topicsTemplate = CheckControls(topicsTemplate);

                topicsTemplate = topicsTemplate.Replace("[AF:SORT:REPLYCREATED]", string.Empty);
                if (topicsTemplate.Contains("[TOPICS]"))
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
                                topicsTemplate = topicsTemplate.Replace("<div class=\"afcrumb\">[FORUMMAINLINK] > [FORUMGROUPLINK]</div>", string.Empty);
                            }

                            if (topicsTemplate.Contains("[META]"))
                            {
                                this.MetaTemplate = TemplateUtils.GetTemplateSection(topicsTemplate, "[META]", "[/META]");
                                topicsTemplate = TemplateUtils.ReplaceSubSection(topicsTemplate, string.Empty, "[META]", "[/META]");
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

                            this.BindTopics(topicsTemplate);
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

                    topicsTemplate = this.ParseControls(topicsTemplate);
                    topicsTemplate = Utilities.LocalizeControl(topicsTemplate);
                    this.Controls.Add(this.ParseControl(topicsTemplate));
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

        private void BindTopics(string topicsTemplate)
        {
            string sOutput = topicsTemplate;
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

        private string ParseControls(string template)
        {
            string myTheme = this.MainSettings.Theme;
            string sOutput = template;
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
                string[] @params = { };
                if (this.SocialGroupId <= 0)
                {
                    @params = new string[] { ParamKeys.ViewType + "=post", ParamKeys.ForumId + "=" + this.ForumId };
                }
                else
                {
                    @params = new string[] { ParamKeys.ViewType + "=post", ParamKeys.ForumId + "=" + this.ForumId, "GroupId=" + this.SocialGroupId, };
                }

                sOutput = sOutput.Replace("[ADDTOPIC]", "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, @params) + "\" class=\"dnnPrimaryAction\">[RESX:AddTopic]</a>");
            }
            else if (!this.Request.IsAuthenticated)
            {
                DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
                string loginUrl = portalSettings.LoginTabId > 0 ? Utilities.NavigateURL(portalSettings.LoginTabId, string.Empty, "returnUrl=" + this.Request.RawUrl) : Utilities.NavigateURL(this.TabId, string.Empty, "ctl=login&returnUrl=" + this.Request.RawUrl);

                string onclick = string.Empty;
                if (portalSettings.EnablePopUps && portalSettings.LoginTabId == Null.NullInteger && !AuthenticationController.HasSocialAuthenticationEnabled(this))
                {
                    onclick = " onclick=\"return " + UrlUtils.PopUpUrl(HttpUtility.UrlDecode(loginUrl), this, this.PortalSettings, true, false, 300, 650) + "\"";
                }

                sOutput = sOutput.Replace("[ADDTOPIC]", $"<span class=\"dcf-auth-false-login\">{string.Format(Utilities.GetSharedResource("[RESX:NotAuthorizedTopicPleaseLogin]"), loginUrl, onclick)}</span>");
            }

            else
            {
                sOutput = sOutput.Replace("[ADDTOPIC]", "<div class=\"amnormal\">[RESX:NotAuthorizedTopic]</div>");
            }

            sOutput = sOutput.Replace("[ADDPOLL]", string.Empty);
            string url = null;
            if (this.bAllowRSS)
            {
                url = DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName(this.Request)) + "/DesktopModules/ActiveForums/feeds.aspx?portalid=" + this.PortalId + "&forumid=" + this.ForumId + "&tabid=" + this.TabId + "&moduleid=" + this.ForumModuleId;
                if (this.SocialGroupId > 0)
                {
                    url += "&GroupId=" + this.SocialGroupId;
                }

                sOutput = sOutput.Replace("[RSSLINK]", "<a href=\"" + url + "\"><img src=\"" + this.myThemePath + "/images/rss.png\" border=\"0\" alt=\"[RESX:RSS]\" /></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[RSSLINK]", string.Empty);
            }

            if (this.Request.IsAuthenticated)
            {
                url = this.NavigateUrl(this.TabId, string.Empty, new string[] { ParamKeys.ViewType + "=sendto", ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId });
                sOutput = sOutput.Replace("[AF:CONTROL:EMAIL]", "<a href=\"" + url + "\" rel=\"nofollow\"><img src=\"" + this.myThemePath + "/images/email16.png\" border=\"0\" alt=\"[RESX:EmailThis]\" /></a>");
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
                string[] editParams = { ParamKeys.ViewType + "=post", "action=te", ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=0-0" };
                sOutput = sOutput.Replace("[ACTIONS:EDIT]", "<a title=\"[RESX:EditTopic]\" href=\"" + this.NavigateUrl(this.TabId, string.Empty, editParams) + "\"><i class=\"fa fa-pencil-square-o fa-fw fa-blue\"></i></a>");
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

        private string ParseTopics(string template, DataTable topics, string Section)
        {
            string sOutput = template;
            sOutput = TemplateUtils.GetTemplateSection(template, "[" + Section + "]", "[/" + Section + "]");
            string sTopics = string.Empty;
            string memberListMode = this.MainSettings.MemberListMode;
            var profileVisibility = this.MainSettings.ProfileVisibility;
            string userNameDisplay = this.MainSettings.UserNameDisplay;
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
            if (topics == null)
            {
                sOutput = TemplateUtils.ReplaceSubSection(template, string.Empty, "[" + Section + "]", "[/" + Section + "]");
                return sOutput;
            }

            if (topics.Rows.Count > 0)
            {
                foreach (DataRow drTopic in topics.Rows)
                {
                    string sTopicsTemplate = sOutput;
                    int topicId = Convert.ToInt32(drTopic["TopicId"]);
                    string subject = HttpUtility.HtmlDecode(Convert.ToString(drTopic["Subject"]));
                    string summary = HttpUtility.HtmlDecode(Convert.ToString(drTopic["Summary"]));
                    string body = HttpUtility.HtmlDecode(Convert.ToString(drTopic["Body"]));

                    // Strip comments
                    int authorId = Convert.ToInt32(drTopic["AuthorId"]);
                    string authorName = Convert.ToString(drTopic["AuthorName"]).ToString().Replace("&amp;#", "&#");
                    string authorFirstName = Convert.ToString(drTopic["AuthorFirstName"]).ToString().Replace("&amp;#", "&#");
                    string authorLastName = Convert.ToString(drTopic["AuthorLastName"]).ToString().Replace("&amp;#", "&#");
                    string authorUserName = Convert.ToString(drTopic["AuthorUserName"]);
                    if (authorUserName == string.Empty)
                    {
                        authorUserName = authorName;
                    }

                    string authorDisplayName = Convert.ToString(drTopic["AuthorDisplayName"]).ToString().Replace("&amp;#", "&#");
                    int replyCount = Convert.ToInt32(drTopic["ReplyCount"]);
                    int viewCount = Convert.ToInt32(drTopic["ViewCount"]);
                    int topicSubscriberCount = Utilities.SafeConvertInt(drTopic["TopicSubscriberCount"]);
                    DateTime dateCreated = Convert.ToDateTime(drTopic["DateCreated"]);
                    int StatusId = Convert.ToInt32(drTopic["StatusId"]);

                    // LastReply info
                    int lastReplyId = Convert.ToInt32(drTopic["LastReplyId"]);
                    string lastReplySubject = HttpUtility.HtmlDecode(Convert.ToString(drTopic["LastReplySubject"]));
                    if (lastReplySubject == string.Empty)
                    {
                        lastReplySubject = "RE: " + subject;
                    }

                    string lastReplySummary = HttpUtility.HtmlDecode(Convert.ToString(drTopic["LastReplySummary"]));
                    if (lastReplySummary == string.Empty)
                    {
                        lastReplySummary = summary;
                    }

                    int lastReplyAuthorId = Convert.ToInt32(drTopic["LastReplyAuthorId"]);
                    string lastReplyAuthorName = Convert.ToString(drTopic["LastReplyAuthorName"]).ToString().Replace("&amp;#", "&#");
                    string lastReplyFirstName = Convert.ToString(drTopic["LastReplyFirstName"]).ToString().Replace("&amp;#", "&#");
                    string lastReplyLastName = Convert.ToString(drTopic["LastReplyLastName"]).ToString().Replace("&amp;#", "&#");
                    string lastReplyUserName = Convert.ToString(drTopic["LastReplyUserName"]);
                    string lastReplyDisplayName = Convert.ToString(drTopic["LastReplyDisplayName"]).ToString().Replace("&amp;#", "&#");
                    DateTime lastReplyDate = Convert.ToDateTime(drTopic["LastReplyDate"]);
                    int userLastTopicRead = Convert.ToInt32(drTopic["UserLastTopicRead"]);
                    int userLastReplyRead = Convert.ToInt32(drTopic["UserLastReplyRead"]);
                    bool isLocked = Convert.ToBoolean(drTopic["IsLocked"]);
                    bool isPinned = Convert.ToBoolean(drTopic["IsPinned"]);
                    string topicURL = drTopic["TopicURL"].ToString();
                    string topicData = drTopic["TopicData"].ToString();
                    if (isLocked)
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("fa-lock", "fa-unlock");
                        sTopicsTemplate = sTopicsTemplate.Replace("[RESX:Lock]", "[RESX:UnLock]");
                        sTopicsTemplate = sTopicsTemplate.Replace("[RESX:LockTopic]", "[RESX:UnLockTopic]");
                        sTopicsTemplate = sTopicsTemplate.Replace("[RESX:Confirm:Lock]", "[RESX:Confirm:UnLock]");
                        sTopicsTemplate = sTopicsTemplate.Replace("[ICONLOCK]", "&nbsp;&nbsp;<i id=\"af-topicsview-lock-" + topicId.ToString() + "\" class=\"fa fa-lock fa-fw fa-red\"></i>");
                    }
                    else
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[ICONLOCK]", "&nbsp;&nbsp;<i id=\"af-topicsview-lock-" + topicId.ToString() + "\" class=\"fa fa-fw fa-red\"></i>");
                    }

                    if (isPinned)
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("dcf-topic-pin-pin", "dcf-topic-pin-unpin");
                        sTopicsTemplate = sTopicsTemplate.Replace("[RESX:Pin]", "[RESX:UnPin]");
                        sTopicsTemplate = sTopicsTemplate.Replace("[RESX:PinTopic]", "[RESX:UnPinTopic]");
                        sTopicsTemplate = sTopicsTemplate.Replace("[RESX:Confirm:Pin]", "[RESX:Confirm:UnPin]");
                        sTopicsTemplate = sTopicsTemplate.Replace("[ICONPIN]", "&nbsp;&nbsp;<i id=\"af-topicsview-pin-" + topicId.ToString() + "\" class=\"fa fa-thumb-tack fa-fw fa-red\"></i>");
                    }
                    else
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[ICONPIN]", "&nbsp;&nbsp;<i id=\"af-topicsview-pin-" + topicId.ToString() + "\" class=\"fa fa-fw fa-red\"></i>");
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

                    sTopicsTemplate = sTopicsTemplate.Replace("[TOPICID]", topicId.ToString());
                    sTopicsTemplate = sTopicsTemplate.Replace("[AUTHORID]", authorId.ToString());
                    sTopicsTemplate = sTopicsTemplate.Replace("[FORUMID]", this.ForumId.ToString());
                    sTopicsTemplate = sTopicsTemplate.Replace("[USERID]", this.UserId.ToString());

                    if (userLastTopicRead == 0 || (userLastTopicRead > 0 & userLastReplyRead < this.ReplyId))
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[POSTICON]", "<div><i class=\"fa fa-file-o fa-2x fa-red\"></i></div>");
                    }
                    else
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[POSTICON]", "<div><i class=\"fa fa-file-o fa-2x fa-grey\"></i></div>");
                    }

                    if (!string.IsNullOrEmpty(summary))
                    {
                        if (!Utilities.HasHTML(summary))
                        {
                            summary = summary.Replace(System.Environment.NewLine, "<br />");
                        }
                    }

                    string sBodyTitle = string.Empty;
                    if (this.bRead)
                    {
                        sBodyTitle = this.GetTitle(body, authorId);
                    }

                    sTopicsTemplate = sTopicsTemplate.Replace("[BODYTITLE]", sBodyTitle);
                    sTopicsTemplate = sTopicsTemplate.Replace("[BODY]", this.GetBody(body, authorId));
                    int bodyLength = -1;
                    string bodyTrim = string.Empty;

                    if (template.Contains("[BODY:"))
                    {
                        int inStart = template.IndexOf("[BODY:", 0) + 1 + 5;
                        int inEnd = template.IndexOf("]", inStart - 1) + 1 - 1;
                        string sLength = template.Substring(inStart, inEnd - inStart);
                        bodyLength = Convert.ToInt32(sLength);
                        bodyTrim = "[BODY:" + bodyLength.ToString() + "]";
                    }

                    string bodyPlain = string.Empty;
                    if (string.IsNullOrEmpty(summary) && sTopicsTemplate.Contains("[SUMMARY]") && string.IsNullOrEmpty(bodyTrim))
                    {
                        bodyTrim = "[BODY:250]";
                        bodyLength = 250;
                    }

                    if (!(bodyTrim == string.Empty))
                    {
                        bodyPlain = body.Replace("<br>", System.Environment.NewLine);
                        bodyPlain = bodyPlain.Replace("<br />", System.Environment.NewLine);
                        bodyPlain = Utilities.StripHTMLTag(bodyPlain);
                        if (bodyLength > 0 & bodyPlain.Length > bodyLength)
                        {
                            bodyPlain = bodyPlain.Substring(0, bodyLength);
                        }

                        bodyPlain = bodyPlain.Replace(System.Environment.NewLine, "<br />");
                        sTopicsTemplate = sTopicsTemplate.Replace(bodyTrim, bodyPlain);
                    }

                    if (string.IsNullOrEmpty(summary))
                    {
                        summary = bodyPlain;
                        summary = summary.Replace("<br />", "  ");
                    }

                    sTopicsTemplate = sTopicsTemplate.Replace("[SUMMARY]", summary);
                    string sPollImage = string.Empty;
                    if (Convert.ToInt32(drTopic["TopicType"]) == 1)
                    {
                        // sPollImage = "<img src=\"" + MyThemePath + "/images/poll.png\" style=\"vertical-align:middle;\" alt=\"[RESX:Poll]\" />";
                        sPollImage = "&nbsp;<i class=\"fa fa-signal fa-fw fa-red\"></i>";
                    }

                    string sTopicURL = string.Empty;
                    ControlUtils ctlUtils = new ControlUtils();
                    sTopicURL = ctlUtils.BuildUrl(this.TabId, this.ModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumGroupId, this.ForumId, topicId, topicURL, -1, -1, string.Empty, 1, -1, this.SocialGroupId);

                    var @params = new List<string> { ParamKeys.TopicId + "=" + topicId, ParamKeys.ContentJumpId + "=" + lastReplyId };

                    if (this.SocialGroupId > 0)
                    {
                        @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
                    }

                    string sLastReplyURL = this.NavigateUrl(this.TabId, string.Empty, @params.ToArray());

                    if (!string.IsNullOrEmpty(sTopicURL))
                    {
                        if (sTopicURL.EndsWith("/"))
                        {
                            sLastReplyURL = sTopicURL + (Utilities.UseFriendlyURLs(this.ForumModuleId) ? String.Concat("#", lastReplyId) : String.Concat("?", ParamKeys.ContentJumpId, "=", lastReplyId));
                        }
                    }

                    string sLastReadURL = string.Empty;
                    string sUserJumpUrl = string.Empty;
                    if (userLastReplyRead > 0)
                    {
                        @params = new List<string> { $"{ParamKeys.ForumId}={this.ForumId}", $"{ParamKeys.TopicId}={topicId}", $"{ParamKeys.ViewType}=topic", $"{ParamKeys.FirstNewPost}={userLastReplyRead}" };
                        if (this.SocialGroupId > 0)
                        {
                            @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
                        }

                        sLastReadURL = this.NavigateUrl(this.TabId, string.Empty, @params.ToArray());

                        if (this.MainSettings.UseShortUrls)
                        {
                            @params = new List<string> { ParamKeys.TopicId + "=" + topicId, ParamKeys.FirstNewPost + "=" + userLastReplyRead };
                            if (this.SocialGroupId > 0)
                            {
                                @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
                            }

                            sLastReadURL = this.NavigateUrl(this.TabId, string.Empty, @params.ToArray());
                        }

                        if (sTopicURL.EndsWith("/"))
                        {
                            sLastReadURL = sTopicURL + (Utilities.UseFriendlyURLs(this.ForumModuleId) ? String.Concat("#", userLastReplyRead) : String.Concat("?", ParamKeys.FirstNewPost, "=", userLastReplyRead));
                        }
                    }

                    if (this.UserPrefJumpLastPost && sLastReadURL != string.Empty)
                    {
                        sTopicURL = sLastReadURL;
                        sUserJumpUrl = sLastReadURL;
                    }

                    if (this.UserId == -1 || lastReplyId == 0)
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:ICONLINK:LASTREAD]", string.Empty);
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:URL:LASTREAD]", string.Empty);
                    }
                    else
                    {
                        if ((userLastTopicRead >= topicId || userLastTopicRead == 0) & (userLastReplyRead >= lastReplyId || userLastReplyRead == 0))
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

                    if (lastReplyId > 0 && this.bRead)
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:ICONLINK:LASTREPLY]", "<a href=\"" + sLastReplyURL + "\" rel=\"nofollow\"><img src=\"" + this.myThemePath + "/images/miniarrow_right.png\" style=\"vertical-align:middle;\" alt=\"[RESX:JumpToLastReply]\" border=\"0\" class=\"afminiarrow\" /></a>");
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:URL:LASTREPLY]", sLastReplyURL);
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:UI:MINIPAGER]", this.GetSubPages(this.TabId, replyCount, this.ForumId, topicId));
                    }
                    else
                    {
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:ICONLINK:LASTREPLY]", string.Empty);
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:URL:LASTREPLY]", string.Empty);
                        sTopicsTemplate = sTopicsTemplate.Replace("[AF:UI:MINIPAGER]", string.Empty);
                    }

                    sTopicsTemplate = sTopicsTemplate.Replace("[TOPICURL]", sTopicURL);
                    subject = Utilities.StripHTMLTag(subject);

                    sTopicsTemplate = sTopicsTemplate.Replace("[SUBJECT]", subject + sPollImage);
                    sTopicsTemplate = sTopicsTemplate.Replace("[SUBJECTLINK]", this.GetTopic(this.TabId, this.ForumId, topicId, subject, sBodyTitle, this.UserId, authorId, replyCount, -1, sTopicURL) + sPollImage);

                    var displayName = UserProfiles.GetDisplayName(this.PortalSettings, this.ForumModuleId, true, this.bModApprove, this.ForumUser.IsAdmin || this.ForumUser.IsSuperUser, authorId, authorUserName, authorFirstName, authorLastName, authorDisplayName).ToString().Replace("&amp;#", "&#");
                    if (Utilities.StripHTMLTag(displayName) == Utilities.GetSharedResource("[RESX:Anonymous]"))
                    {
                        displayName = displayName.Replace(Utilities.GetSharedResource("[RESX:Anonymous]"), authorName);
                    }

                    sTopicsTemplate = sTopicsTemplate.Replace("[STARTEDBY]", displayName);
                    sTopicsTemplate = sTopicsTemplate.Replace("[DATECREATED]", Utilities.GetUserFormattedDateTime(dateCreated, this.PortalId, this.UserId));
                    sTopicsTemplate = sTopicsTemplate.Replace("[REPLIES]", replyCount.ToString());
                    sTopicsTemplate = sTopicsTemplate.Replace("[VIEWS]", viewCount.ToString());
                    sTopicsTemplate = sTopicsTemplate.Replace("[TOPICSUBSCRIBERCOUNT]", topicSubscriberCount.ToString());
                    sTopicsTemplate = sTopicsTemplate.Replace("[ROWCSS]", this.GetRowCSS(userLastTopicRead, userLastReplyRead, topicId, lastReplyId, rowcount));

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
                        if (lastReplyId == 0)
                        {
                            sTopicsTemplate = TemplateUtils.ReplaceSubSection(sTopicsTemplate, Utilities.GetUserFormattedDateTime(lastReplyDate, this.PortalId, this.UserId), "[LASTPOST]", "[/LASTPOST]");
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

                            sLastReplyTemp = sLastReplyTemp.Replace(this.lastReplySubjectReplaceTag, Utilities.GetLastPostSubject(lastReplyId, topicId, this.ForumId, this.TabId, lastReplySubject, iLength, this.pageSize, replyCount, this.bRead));

                            // sLastReplyTemp = sLastReplyTemp.Replace("[RESX:BY]", Utilities.GetSharedResource("By.Text"))
                            if (lastReplyAuthorId > 0)
                            {
                                sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDISPLAYNAME]", UserProfiles.GetDisplayName(this.PortalSettings, this.ForumModuleId, true, this.bModApprove, this.ForumUser.IsAdmin || this.ForumUser.IsSuperUser, lastReplyAuthorId, lastReplyUserName, lastReplyFirstName, lastReplyLastName, lastReplyDisplayName).ToString().Replace("&amp;#", "&#"));
                            }
                            else
                            {
                                sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDISPLAYNAME]", lastReplyAuthorName);
                            }

                            sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDATE]", Utilities.GetUserFormattedDateTime(lastReplyDate, this.PortalId, this.UserId));
                            sTopicsTemplate = TemplateUtils.ReplaceSubSection(sTopicsTemplate, sLastReplyTemp, "[LASTPOST]", "[/LASTPOST]");
                        }
                    }

                    sTopics += sTopicsTemplate;
                    rowcount += 1;
                }

                sOutput = TemplateUtils.ReplaceSubSection(template, sTopics, "[" + Section + "]", "[/" + Section + "]");
            }
            else
            {
                sOutput = TemplateUtils.ReplaceSubSection(template, string.Empty, "[" + Section + "]", "[/" + Section + "]");
            }

            return sOutput;
        }

        private void BuildPager()
        {
            if (this.topicRowCount > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controls.PagerNav pager1 = null;
                pager1 = (DotNetNuke.Modules.ActiveForums.Controls.PagerNav)this.FindControl("Pager1");

                DotNetNuke.Modules.ActiveForums.Controls.PagerNav pager2 = null;
                object obj = this.FindControl("Pager2");
                if (obj != null)
                {
                    pager2 = (DotNetNuke.Modules.ActiveForums.Controls.PagerNav)obj;
                }

                int intPages = 0;
                if (pager1 != null)
                {
                    intPages = Convert.ToInt32(System.Math.Ceiling(this.topicRowCount / (double)this.pageSize));
                    pager1.PageCount = intPages;
                    pager1.PageMode = PagerNav.Mode.Links;
                    pager1.BaseURL = URL.ForumLink(this.TabId, this.ForumInfo);
                    pager1.CurrentPage = this.PageId;
                    pager1.TabID = Convert.ToInt32(this.Request.Params["TabId"]);
                    pager1.ForumID = this.ForumId;
                    pager1.UseShortUrls = this.MainSettings.UseShortUrls;
                    pager1.PageText = Utilities.GetSharedResource("[RESX:Page]");
                    pager1.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
                    pager1.View = Views.Topics;
                    if (this.Request.Params[ParamKeys.Sort] != null)
                    {
                        string[] @params = { $"{ParamKeys.Sort}={this.Request.Params[ParamKeys.Sort]}", "afcol=" + this.Request.Params["afcol"] };
                        pager1.Params = @params;
                        if (pager2 != null)
                        {
                            pager2.Params = @params;
                        }
                    }
                }

                if (pager2 != null)
                {
                    pager2.PageMode = Modules.ActiveForums.Controls.PagerNav.Mode.Links;
                    pager2.BaseURL = URL.ForumLink(this.TabId, this.ForumInfo);
                    pager2.UseShortUrls = this.MainSettings.UseShortUrls;
                    pager2.PageCount = intPages;
                    pager2.CurrentPage = this.PageId;
                    pager2.TabID = Convert.ToInt32(this.Request.Params["TabId"]);
                    pager2.ForumID = this.ForumId;
                    pager2.PageText = Utilities.GetSharedResource("[RESX:Page]");
                    pager2.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
                    pager2.View = Views.Topics;
                }
            }
        }

        private string GetTitle(string body, int authorId)
        {
            if (this.bRead || authorId == this.UserId)
            {
                body = HttpUtility.HtmlDecode(body);
                body = body.Replace("<br>", System.Environment.NewLine);
                body = Utilities.StripHTMLTag(body);
                body = body.Length > 500 ? body.Substring(0, 500) + "..." : body;
                body = body.Replace("\"", "'");
                body = body.Replace("?", string.Empty);
                body = body.Replace("+", string.Empty);
                body = body.Replace("%", string.Empty);
                body = body.Replace("#", string.Empty);
                body = body.Replace("@", string.Empty);
                return body.Trim();
            }
            else
            {
                return string.Empty;
            }
        }

        private string GetBody(string body, int authorId)
        {
            if (this.bRead || authorId == this.UserId)
            {
                return HttpUtility.HtmlDecode(body);
            }
            else
            {
                return string.Empty;
            }
        }

        private string GetTopic(int tabID, int forumID, int postID, string subject, string bodyTitle, int userID, int postUserID, int replies, int forumOwnerID, string sLink)
        {
            string sOut = null;
            subject = HttpUtility.HtmlDecode(subject);
            subject = Utilities.StripHTMLTag(subject);
            subject = subject.Replace("\"", string.Empty);

            // Subject = Subject.Replace("'", string.Empty);
            subject = subject.Replace("#", string.Empty);
            subject = subject.Replace("%", string.Empty);

            // Subject = Subject.Replace("?", String.Empty)
            subject = subject.Replace("+", string.Empty);

            if (this.bRead)
            {
                if (sLink == string.Empty)
                {
                    string[] @params = { ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + postID, ParamKeys.ViewType + "=" + Views.Topic };
                    if (this.MainSettings.UseShortUrls)
                    {
                        @params = new string[] { ParamKeys.TopicId + "=" + postID };
                    }

                    sOut = "<a title=\"" + bodyTitle + "\" href=\"" + this.NavigateUrl(tabID, string.Empty, @params) + "\">" + subject + "</a>";
                }
                else
                {
                    sOut = "<a title=\"" + bodyTitle + "\" href=\"" + sLink + "\">" + subject + "</a>";
                }
            }
            else
            {
                sOut = subject;
            }

            return sOut;
        }

        private string GetSubPages(int tabID, int replies, int forumID, int postID)
        {
            int i = 0;
            string sOut = string.Empty;

            if (replies + 1 > this.pageSize)
            {
                // sOut = "<div class=""afpagermini"">" & GetSharedResource("SubPages.Text") & "&nbsp;"
                sOut = "<div class=\"afpagermini\">(<img src=\"" + this.myThemePath + "/images/icon_multipage.png\" alt=\"[RESX:MultiPageTopic]\" style=\"vertical-align:middle;\" />";

                // Jump to pages
                int intPostPages = 0;
                intPostPages = Convert.ToInt32(System.Math.Ceiling((double)(replies + 1) / this.pageSize));
                if (intPostPages > 3)
                {
                    for (i = 1; i <= 3; i++)
                    {
                        if (this.UseAjax)
                        {
                            var @params = new List<string> { ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + postID, ParamKeys.ViewType + "=" + Views.Topic };
                            if (this.MainSettings.UseShortUrls)
                            {
                                @params = new List<string> { ParamKeys.TopicId + "=" + postID };
                            }

                            if (i > 1)
                            {
                                @params.Add(ParamKeys.PageJumpId + "=" + i);
                            }

                            sOut += "<a href=\"" + this.NavigateUrl(tabID, string.Empty, @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                        }
                        else
                        {
                            var @params = new List<string> { ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + postID, ParamKeys.ViewType + "=" + Views.Topic };
                            if (this.MainSettings.UseShortUrls)
                            {
                                @params = new List<string> { ParamKeys.TopicId + "=" + postID };
                            }

                            if (i > 1)
                            {
                                @params.Add(ParamKeys.PageId + "=" + i);
                            }

                            sOut += "<a href=\"" + this.NavigateUrl(tabID, string.Empty, @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                        }
                    }

                    if (intPostPages > 4)
                    {
                        sOut += "...&nbsp;";
                    }

                    if (this.UseAjax)
                    {
                        var @params = new List<string> { ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + postID, ParamKeys.ViewType + "=" + Views.Topic };
                        if (this.MainSettings.UseShortUrls)
                        {
                            @params = new List<string> { ParamKeys.TopicId + "=" + postID };
                        }

                        if (i > 1)
                        {
                            @params.Add(ParamKeys.PageJumpId + "=" + i);
                        }

                        sOut += "<a href=\"" + this.NavigateUrl(tabID, string.Empty, @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                    }
                    else
                    {
                        var @params = new List<string> { ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + postID, ParamKeys.ViewType + "=" + Views.Topic };
                        if (this.MainSettings.UseShortUrls)
                        {
                            @params = new List<string> { ParamKeys.TopicId + "=" + postID };
                        }

                        if (i > 1)
                        {
                            @params.Add(ParamKeys.PageId + "=" + i);
                        }

                        sOut += "<a href=\"" + this.NavigateUrl(tabID, string.Empty, @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                    }
                }
                else
                {
                    for (i = 1; i <= intPostPages; i++)
                    {
                        if (this.UseAjax)
                        {
                            // sOut &= "<span class=""afpagerminiitem"" onclick=""javascript:afPageJump(" & i & ");"">" & i & "</span>&nbsp;"
                            var @params = new List<string> { ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + postID, ParamKeys.ViewType + "=" + Views.Topic };
                            if (this.MainSettings.UseShortUrls)
                            {
                                @params = new List<string> { ParamKeys.TopicId + "=" + postID };
                            }

                            if (i > 1)
                            {
                                @params.Add(ParamKeys.PageJumpId + "=" + i);
                            }

                            sOut += "<a href=\"" + this.NavigateUrl(tabID, string.Empty, @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                        }
                        else
                        {
                            var @params = new List<string> { ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + postID, ParamKeys.ViewType + "=" + Views.Topic };
                            if (this.MainSettings.UseShortUrls)
                            {
                                @params = new List<string> { ParamKeys.TopicId + "=" + postID };
                            }

                            if (i > 1)
                            {
                                @params.Add(ParamKeys.PageId + "=" + i);
                            }

                            sOut += "<a href=\"" + this.NavigateUrl(tabID, string.Empty, @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                        }
                    }
                }

                sOut += ")</div>";
            }

            return sOut;
        }

        private string GetRowCSS(int lastTopicRead, int lastReplyRead, int topicId, int replyId, int rowCount)
        {
            bool isRead = false;
            if (lastTopicRead >= topicId && lastReplyRead >= replyId)
            {
                isRead = true;
            }

            if (!this.Request.IsAuthenticated)
            {
                isRead = true;
            }

            if (isRead == true)
            {
                if (rowCount % 2 == 0)
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
                if (rowCount % 2 == 0)
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
