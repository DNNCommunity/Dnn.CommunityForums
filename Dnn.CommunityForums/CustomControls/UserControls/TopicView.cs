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
    using System.Diagnostics.Eventing.Reader;
    using System.Drawing.Printing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.ActiveForums.Constants;
    using DotNetNuke.Modules.ActiveForums.Controllers;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.UI.Skins;

    using static DotNetNuke.Modules.ActiveForums.Controls.ActiveGrid;

    [DefaultProperty("Text"), ToolboxData("<{0}:TopicView runat=server></{0}:TopicView>")]
    public class TopicView : ForumBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Environment));

        #region Private Members

        private string _forumName;
        private string _groupName;
        private int _topicTemplateId;
        private DataRow _drForum;
        private DataRow _drSecurity;
        private DataTable _dtTopic;
        private DataTable _dtAttach;
        private bool _bRead;
        private bool _bAttach;
        private bool _bTrust;
        private bool _bDelete;
        private bool _bEdit;
        private bool _bSubscribe;
        private bool _bModApprove;
        private bool _bModSplit;
        private bool _bModDelete;
        private bool _bModEdit;
        private bool _bModMove;
        private bool _bModUser;
        private bool _bModLock = false;
        private bool _bModPin = false;
        private bool _bAllowRSS;
        private int _rowIndex;
        private int _pageSize = 20;
        private string _myTheme = "_default";
        private string _myThemePath = string.Empty;
        private bool _bLocked;
        private bool _bPinned;
        private int _topicType;
        private string _topicSubject = string.Empty;
        private string _topicDescription = string.Empty;
        private int _viewCount;
        private int _replyCount;
        private int _topicSubscriberCount;
        private int _forumSubscriberCount;
        private int _rowCount;
        private int _statusId;
        private int _topicAuthorId;
        private string _topicAuthorDisplayName = string.Empty;
        private string _topicDateCreated = string.Empty;
        private string _memberListMode = string.Empty;
        private string _profileVisibility = string.Empty;
        private bool _enablePoints;
        private bool _trustDefault;
        private bool _isTrusted;
        private int _topicRating;
        private bool _allowHTML;
        private bool _allowLikes;
        private bool _allowScript;
        private bool _allowSubscribe;
        private int _nextTopic;
        private int _prevTopic;
        private bool _isSubscribedTopic;
        private string _lastPostDate;
        private readonly Author _lastPostAuthor = new Author();
        private string _defaultSort;
        private int _editInterval;
        private string _tags = string.Empty;
        private string _topicURL = string.Empty;
        private string _topicData = string.Empty;
        private bool _useListActions;

        #endregion

        #region Public Properties

        public string TopicTemplate { get; set; } = string.Empty;

        public int OptPageSize { get; set; }

        public string OptDefaultSort { get; set; }

        public string MetaTemplate { get; set; } = "[META][TITLE][TOPICSUBJECT] - [PORTALNAME] - [PAGENAME] - [GROUPNAME] - [FORUMNAME][/TITLE][DESCRIPTION][BODY:255][/DESCRIPTION][KEYWORDS][TAGS][VALUE][/KEYWORDS][/META]";

        public string MetaTitle { get; set; } = string.Empty;

        public string MetaDescription { get; set; } = string.Empty;

        public string MetaKeywords { get; set; } = string.Empty;
        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.ForumInfo == null)
            {
                this.ForumInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(this.PortalId, this.ForumModuleId, this.ForumId, false, this.TopicId);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (!this.Page.IsPostBack)
                {
                    // Handle an old URL form if needed
                    var sURL = this.Request.RawUrl;
                    if (!string.IsNullOrWhiteSpace(sURL) && sURL.Contains("view") && sURL.Contains("postid") && sURL.Contains("forumid"))
                    {
                        sURL = sURL.Replace("view", ParamKeys.ViewType);
                        sURL = sURL.Replace("postid", ParamKeys.TopicId);
                        sURL = sURL.Replace("forumid", ParamKeys.ForumId);
                        this.Response.Status = "301 Moved Permanently";
                        this.Response.AddHeader("Location", sURL);
                    }

                    // We must have a forum id
                    if (this.ForumId < 1)
                    {
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId));
                    }
                }

                // Redirect to the first new post if the first new post param is found in the url
                // Note that this should probably be the default behavior unless a page is specified.
                var lastPostRead = Utilities.SafeConvertInt(this.Request.Params[ParamKeys.FirstNewPost]);
                if (lastPostRead > 0)
                {
                    var firstUnreadPost = DataProvider.Instance().Utility_GetFirstUnRead(this.TopicId, Convert.ToInt32(this.Request.Params[ParamKeys.FirstNewPost]));
                    if (firstUnreadPost > lastPostRead)
                    {
                        var tURL = Utilities.NavigateURL(this.TabId, "", new[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.ViewType + "=topic", ParamKeys.ContentJumpId + "=" + firstUnreadPost });
                        if (this.MainSettings.UseShortUrls)
                        {
                            tURL = Utilities.NavigateURL(this.TabId, "", new[] { ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.ContentJumpId + "=" + firstUnreadPost });
                        }

                        this.Response.Redirect(tURL);
                    }
                }

                this.AppRelativeVirtualPath = "~/";

                this._myTheme = this.MainSettings.Theme;
                this._myThemePath = this.Page.ResolveUrl("~/DesktopModules/ActiveForums/themes/" + this._myTheme);
                // _allowAvatars = !UserPrefHideAvatars;
                this._enablePoints = this.MainSettings.EnablePoints;
                this._editInterval = this.MainSettings.EditInterval;

                // Get our default sort
                // Try the OptDefaultSort Value First
                this._defaultSort = this.OptDefaultSort;
                if (string.IsNullOrWhiteSpace(this._defaultSort))
                {
                    // Next, try getting the sort from the query string
                    this._defaultSort = (this.Request.Params[ParamKeys.Sort] + string.Empty).Trim().ToUpperInvariant();
                    if (string.IsNullOrWhiteSpace(this._defaultSort) || (this._defaultSort != "ASC" && this._defaultSort != "DESC"))
                    {
                        // If we still don't have a valid sort, try and use the value from the users profile
                        this._defaultSort = (this.ForumUser.Profile.PrefDefaultSort + string.Empty).Trim().ToUpper();
                        if (string.IsNullOrWhiteSpace(this._defaultSort) || (this._defaultSort != "ASC" && this._defaultSort != "DESC"))
                        {
                            // No other option than to just use ASC
                            this._defaultSort = "ASC";
                        }
                    }
                }

                this.LoadData(this.PageId);

                this.BindTopic();

                this._allowSubscribe = this.Request.IsAuthenticated && this._bSubscribe;

                var tempVar = this.BasePage;
                DotNetNuke.Modules.ActiveForums.Environment.UpdateMeta(ref tempVar, this.MetaTitle, this.MetaDescription, this.MetaKeywords);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message, ex.InnerException);
                }

                this.RenderMessage("[RESX:Error:LoadingTopic]", ex.Message, ex);
            }
        }

        #endregion

        #region Private Methods

        private void LoadData(int pageId)
        {
            // Get our page size
            this._pageSize = this.OptPageSize;
            if (this._pageSize <= 0)
            {
                this._pageSize = this.UserId > 0 ? this.UserDefaultPageSize : this.MainSettings.PageSize;
                if (this._pageSize < 5)
                {
                    this._pageSize = 10;
                }
            }

            // If we are in print mode, set the page size to maxValue
            if (!string.IsNullOrWhiteSpace(this.Request.QueryString["dnnprintmode"]))
            {
                this._pageSize = int.MaxValue;
            }

            // If our pagesize is maxvalue, we can only have one page
            if (this._pageSize == int.MaxValue)
            {
                pageId = 1;
            }

            // Get our Row Index
            this._rowIndex = (pageId - 1) * this._pageSize;
            string cacheKey = string.Format(CacheKeys.TopicViewForUser, this.ModuleId, this.TopicId, this.UserId, HttpContext.Current?.Response?.Cookies["language"]?.Value, this._rowIndex, this._pageSize);
            DataSet ds = (DataSet)DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheRetrieve(this.ForumModuleId, cacheKey);
            if (ds == null)
            {
                ds = DotNetNuke.Modules.ActiveForums.DataProvider.Instance().UI_TopicView(this.PortalId, this.ForumModuleId, this.ForumId, this.TopicId, this.UserId, this._rowIndex, this._pageSize, this.UserInfo.IsSuperUser, this._defaultSort);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.ModuleId, cacheKey, ds);
            }

            // Test for a proper dataset
            if (ds.Tables.Count < 4 || ds.Tables[0].Rows.Count == 0 || ds.Tables[1].Rows.Count == 0)
            {
                this.Response.Redirect(Utilities.NavigateURL(this.TabId));
            }

            // Load our values
            this._drForum = ds.Tables[0].Rows[0];
            this._drSecurity = ds.Tables[1].Rows[0];
            this._dtTopic = ds.Tables[2];
            this._dtAttach = ds.Tables[3];

            // If we don't have any rows to display, redirect
            if (this._dtTopic.Rows.Count == 0)
            {
                if (pageId > 1)
                {
                    if (this.MainSettings.UseShortUrls)
                    {
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.TopicId + "=" + this.TopicId }), true);
                    }
                    else
                    {
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + this.TopicId }), true);
                    }
                }
                else
                {
                    if (this.MainSettings.UseShortUrls)
                    {
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ForumId + "=" + this.ForumId }), true);
                    }
                    else
                    {
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topics }), true);
                    }
                }
            }

            // first make sure we have read permissions, otherwise we need to redirect
            this._bRead = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanRead"].ToString(), this.ForumUser.UserRoles);

            if (!this._bRead)
            {
                DotNetNuke.Entities.Portals.PortalSettings PortalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
                if (PortalSettings.LoginTabId > 0)
                {
                    this.Response.Redirect(Utilities.NavigateURL(PortalSettings.LoginTabId, "", "returnUrl=" + this.Request.RawUrl), true);
                }
                else
                {
                    this.Response.Redirect(Utilities.NavigateURL(this.TabId, "", "ctl=login&returnUrl=" + this.Request.RawUrl), true);
                }
            }

            // bCreate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanCreate"].ToString(), ForumUser.UserRoles);
            this._bEdit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanEdit"].ToString(), this.ForumUser.UserRoles);
            this._bDelete = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanDelete"].ToString(), this.ForumUser.UserRoles);
            // bReply = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanReply"].ToString(), ForumUser.UserRoles);
            // bPoll = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(_drSecurity["CanPoll"].ToString(), ForumUser.UserRoles);
            this._bAttach = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanAttach"].ToString(), this.ForumUser.UserRoles);
            this._bSubscribe = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanSubscribe"].ToString(), this.ForumUser.UserRoles);
            // bModMove = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(_drSecurity["CanModMove"].ToString(), ForumUser.UserRoles);
            this._bModSplit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanModSplit"].ToString(), this.ForumUser.UserRoles);
            this._bModDelete = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanModDelete"].ToString(), this.ForumUser.UserRoles);
            this._bModApprove = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanModApprove"].ToString(), this.ForumUser.UserRoles);
            this._bTrust = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanTrust"].ToString(), this.ForumUser.UserRoles);
            this._bModEdit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanModEdit"].ToString(), this.ForumUser.UserRoles);
            this._bModMove = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanModMove"].ToString(), this.ForumUser.UserRoles);
            this._bModPin = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanModPin"].ToString(), this.ForumUser.UserRoles);
            this._bModLock = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanModLock"].ToString(), this.ForumUser.UserRoles);
            this._bModUser = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this._drSecurity["CanModUser"].ToString(), this.ForumUser.UserRoles);

            this._isTrusted = Utilities.IsTrusted((int)this.ForumInfo.DefaultTrustValue, this.ForumUser.TrustLevel, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Trust, this.ForumUser.UserRoles));

            this._forumName = this._drForum["ForumName"].ToString();
            this._groupName = this._drForum["GroupName"].ToString();
            this.ForumGroupId = Utilities.SafeConvertInt(this._drForum["ForumGroupId"]);
            this._topicTemplateId = Utilities.SafeConvertInt(this._drForum["TopicTemplateId"]);
            this._bAllowRSS = Utilities.SafeConvertBool(this._drForum["AllowRSS"]);
            this._bLocked = Utilities.SafeConvertBool(this._drForum["IsLocked"]);
            this._bPinned = Utilities.SafeConvertBool(this._drForum["IsPinned"]);
            this._topicType = Utilities.SafeConvertInt(this._drForum["TopicType"]);
            this._statusId = Utilities.SafeConvertInt(this._drForum["StatusId"]);
            this._topicSubject = HttpUtility.HtmlDecode(this._drForum["Subject"].ToString());
            this._topicDescription = Utilities.StripHTMLTag(HttpUtility.HtmlDecode(this._drForum["Body"].ToString()));
            this._tags = this._drForum["Tags"].ToString();
            this._viewCount = Utilities.SafeConvertInt(this._drForum["ViewCount"]);
            this._replyCount = Utilities.SafeConvertInt(this._drForum["ReplyCount"]);
            this._topicSubscriberCount = Utilities.SafeConvertInt(this._drForum["TopicSubscriberCount"]);
            this._forumSubscriberCount = Utilities.SafeConvertInt(this._drForum["ForumSubscriberCount"]);
            this._topicAuthorId = Utilities.SafeConvertInt(this._drForum["AuthorId"]);
            this._topicAuthorDisplayName = this._drForum["TopicAuthor"].ToString();
            this._topicRating = Utilities.SafeConvertInt(this._drForum["TopicRating"]);
            this._allowHTML = Utilities.SafeConvertBool(this._drForum["AllowHTML"]);
            this._allowLikes = Utilities.SafeConvertBool(this._drForum["AllowLikes"]);
            this._allowScript = Utilities.SafeConvertBool(this._drForum["AllowScript"]);
            this._rowCount = Utilities.SafeConvertInt(this._drForum["ReplyCount"]) + 1;
            this._nextTopic = Utilities.SafeConvertInt(this._drForum["NextTopic"]);
            this._prevTopic = Utilities.SafeConvertInt(this._drForum["PrevTopic"]);
            this._lastPostDate = Utilities.GetUserFormattedDateTime(Utilities.SafeConvertDateTime(this._drForum["LastPostDate"]), this.PortalId, this.UserId);
            this._lastPostAuthor.AuthorId = Utilities.SafeConvertInt(this._drForum["LastPostAuthorId"]);
            this._lastPostAuthor.DisplayName = this._drForum["LastPostDisplayName"].ToString();
            this._lastPostAuthor.FirstName = this._drForum["LastPostFirstName"].ToString();
            this._lastPostAuthor.LastName = this._drForum["LastPostLastName"].ToString();
            this._lastPostAuthor.Username = this._drForum["LastPostUserName"].ToString();
            this._topicURL = this._drForum["URL"].ToString();
            this._topicDateCreated = Utilities.GetUserFormattedDateTime(Utilities.SafeConvertDateTime(this._drForum["DateCreated"]), this.PortalId, this.UserId);
            this._topicData = this._drForum["TopicData"].ToString();
            this._isSubscribedTopic = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(this.PortalId, this.ForumModuleId, this.UserId, this.ForumInfo.ForumID, this.TopicId);

            if (this.Page.IsPostBack)
            {
                return;
            }

            // If a content jump id was passed it, we need to calulate a page and then jump to it with an ancor
            // otherwise, we don't need to do anything

            var contentJumpId = Utilities.SafeConvertInt(this.Request.Params[ParamKeys.ContentJumpId], -1);
            if (contentJumpId < 0)
            {
                return;
            }

            var sTarget = "#" + contentJumpId;

            var sURL = string.Empty;

            if (this.MainSettings.URLRewriteEnabled && !string.IsNullOrEmpty(this._topicURL))
            {
                var db = new Data.Common();
                sURL = db.GetUrl(this.ModuleId, this.ForumGroupId, this.ForumId, this.TopicId, this.UserId, contentJumpId);

                if (!sURL.StartsWith("/"))
                {
                    sURL = "/" + sURL;
                }

                if (!sURL.EndsWith("/"))
                {
                    sURL += "/";
                }

                sURL += sTarget;
            }

            if (string.IsNullOrEmpty(sURL))
            {

                var @params = new List<string> {
                    $"{ParamKeys.ForumId}={this.ForumId}",
                    $"{ParamKeys.TopicId}={this.TopicId}",
                    $"{ParamKeys.ViewType}={Views.Topic}"
                };
                if (this.MainSettings.UseShortUrls)
                {
                    @params = new List<string> { ParamKeys.TopicId + "=" + this.TopicId };
                }

                var intPages = Convert.ToInt32(Math.Ceiling(this._rowCount / (double)this._pageSize));
                if (intPages > 1)
                {
                    @params.Add($"{ParamKeys.PageJumpId}={intPages}");
                }

                if (this.SocialGroupId > 0)
                {
                    @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
                }

                sURL = Utilities.NavigateURL(this.TabId, "", @params.ToArray()) + sTarget;
            }

            if (this.Request.IsAuthenticated)
            {
                this.Response.Redirect(sURL, true);
            }

            // Not sure why we're doing this.  I assume it may have something to do with search engines - JB
            this.Response.Status = "301 Moved Permanently";
            this.Response.AddHeader("Location", sURL);
        }

        private void BindTopic()
        {
            string sOutput;

            var bFullTopic = true;

            // Load the proper template into out output variable
            if (!string.IsNullOrWhiteSpace(this.TopicTemplate))
            {
                // Note:  The template may be set in the topic review section of the Post form.
                bFullTopic = false;
                sOutput = this.TopicTemplate;
                sOutput = Utilities.ParseSpacer(sOutput);
            }
            else
            {
                sOutput = TemplateCache.GetCachedTemplate(this.ForumModuleId, "TopicView", this._topicTemplateId);
            }

            // Handle the postinfo token if present
            if (sOutput.Contains("[POSTINFO]") && this.ForumInfo.ProfileTemplateId > 0)
            {
                sOutput = sOutput.Replace("[POSTINFO]", TemplateCache.GetCachedTemplate(this.ForumModuleId, "ProfileInfo", this.ForumInfo.ProfileTemplateId));
            }

            // Run some basic rpleacements
            sOutput = sOutput.Replace("[PORTALID]", this.PortalId.ToString());
            sOutput = sOutput.Replace("[MODULEID]", this.ForumModuleId.ToString());
            sOutput = sOutput.Replace("[TABID]", this.TabId.ToString());
            sOutput = sOutput.Replace("[TOPICID]", this.TopicId.ToString());
            sOutput = sOutput.Replace("[AF:CONTROL:FORUMID]", this.ForumId.ToString());
            sOutput = sOutput.Replace("[AF:CONTROL:FORUMGROUPID]", this.ForumGroupId.ToString());
            sOutput = sOutput.Replace("[AF:CONTROL:PARENTFORUMID]", this.ParentForumId.ToString());

            // Add Topic Scripts
            var ctlTopicScripts = (af_topicscripts)this.LoadControl("~/DesktopModules/ActiveForums/controls/af_topicscripts.ascx");
            ctlTopicScripts.ModuleConfiguration = this.ModuleConfiguration;
            this.Controls.Add(ctlTopicScripts);

            // Pretty sure this is no longer used
            /*
            if (sOutput.Contains("<am:TopicsNavigator"))
            {
                var ctl = ParseControl(sOutput);

                if(ctl != null)
                    Controls.Add(ctl);

                LinkControls(Controls);

                return;
            }
            */

            #region Build Topic Properties

            if (sOutput.Contains("[AF:PROPERTIES"))
            {
                var sProps = string.Empty;

                if (!string.IsNullOrWhiteSpace(this._topicData))
                {
                    var sPropTemplate = TemplateUtils.GetTemplateSection(sOutput, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");

                    try
                    {
                        var pl = DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Deserialize(this._topicData);
                        foreach (var p in pl)
                        {
                            var pName = HttpUtility.HtmlDecode(p.Name);
                            var pValue = HttpUtility.HtmlDecode(p.Value);
                            // This builds the replacement text for the properties template
                            var tmp = sPropTemplate.Replace("[AF:PROPERTY:LABEL]", "[RESX:" + pName + "]");
                            tmp = tmp.Replace("[AF:PROPERTY:VALUE]", pValue);
                            sProps += tmp;

                            // This deals with any specific property tokens that may be present outside of the normal properties template
                            sOutput = sOutput.Replace("[AF:PROPERTY:" + pName + ":LABEL]", Utilities.GetSharedResource("[RESX:" + pName + "]"));
                            sOutput = sOutput.Replace("[AF:PROPERTY:" + pName + ":VALUE]", pValue);
                            var pValueKey = string.IsNullOrWhiteSpace(pValue) ? string.Empty : Utilities.CleanName(pValue).ToLowerInvariant();
                            sOutput = sOutput.Replace("[AF:PROPERTY:" + pName + ":VALUEKEY]", pValueKey);
                        }
                    }
                    catch (XmlException)
                    {
                        // Property XML is invalid
                        // Nothing to do in this case but ignore the issue.
                    }
                }

                sOutput = TemplateUtils.ReplaceSubSection(sOutput, sProps, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");
            }

            #endregion

            #region Populate Metadata

            // If the template contains a meta template, grab it then remove the token
            if (sOutput.Contains("[META]"))
            {
                this.MetaTemplate = TemplateUtils.GetTemplateSection(sOutput, "[META]", "[/META]");
                sOutput = TemplateUtils.ReplaceSubSection(sOutput, string.Empty, "[META]", "[/META]");
            }

            // Parse Meta Template
            if (!string.IsNullOrEmpty(this.MetaTemplate))
            {
                this.MetaTemplate = this.MetaTemplate.Replace("[FORUMNAME]", this._forumName);
                this.MetaTemplate = this.MetaTemplate.Replace("[GROUPNAME]", this._groupName);

                DotNetNuke.Entities.Portals.PortalSettings settings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
                string pageName = (settings.ActiveTab.Title.Length == 0)

                                   ? this.Server.HtmlEncode(settings.ActiveTab.TabName)
                                   : this.Server.HtmlEncode(settings.ActiveTab.Title);

                this.MetaTemplate = this.MetaTemplate.Replace("[PAGENAME]", pageName);
                this.MetaTemplate = this.MetaTemplate.Replace("[PORTALNAME]", settings.PortalName);
                this.MetaTemplate = this.MetaTemplate.Replace("[TAGS]", this._tags);

                // Subject
                if (this.MetaTemplate.Contains("[TOPICSUBJECT:"))
                {
                    const string pattern = "(\\[TOPICSUBJECT:(.+?)\\])";
                    foreach (Match m in Regex.Matches(this.MetaTemplate, pattern))
                    {
                        var maxLength = Utilities.SafeConvertInt(m.Groups[2].Value, 255);
                        if (this._topicSubject.Length > maxLength)
                        {
                            this.MetaTemplate = this.MetaTemplate.Replace(m.Value, this._topicSubject.Substring(0, maxLength) + "...");
                        }
                        else
                        {
                            this.MetaTemplate = this.MetaTemplate.Replace(m.Value, Utilities.StripHTMLTag(this._topicSubject));
                        }
                    }
                }

                this.MetaTemplate = this.MetaTemplate.Replace("[TOPICSUBJECT]", Utilities.StripHTMLTag(this._topicSubject));

                // Body
                if (this.MetaTemplate.Contains("[BODY:"))
                {
                    const string pattern = "(\\[BODY:(.+?)\\])";
                    foreach (Match m in Regex.Matches(this.MetaTemplate, pattern))
                    {
                        var maxLength = Utilities.SafeConvertInt(m.Groups[2].Value, 512);
                        if (this._topicDescription.Length > maxLength)
                        {
                            this.MetaTemplate = this.MetaTemplate.Replace(m.Value, this._topicDescription.Substring(0, maxLength) + "...");
                        }
                        else
                        {
                            this.MetaTemplate = this.MetaTemplate.Replace(m.Value, this._topicDescription);
                        }
                    }
                }

                this.MetaTemplate = this.MetaTemplate.Replace("[BODY]", this._topicDescription);

                this.MetaTitle = TemplateUtils.GetTemplateSection(this.MetaTemplate, "[TITLE]", "[/TITLE]").Replace("[TITLE]", string.Empty).Replace("[/TITLE]", string.Empty);
                this.MetaTitle = this.MetaTitle.TruncateAtWord(SEOConstants.MaxMetaTitleLength);
                this.MetaDescription = TemplateUtils.GetTemplateSection(this.MetaTemplate, "[DESCRIPTION]", "[/DESCRIPTION]").Replace("[DESCRIPTION]", string.Empty).Replace("[/DESCRIPTION]", string.Empty);
                this.MetaDescription = this.MetaDescription.TruncateAtWord(SEOConstants.MaxMetaDescriptionLength);
                this.MetaKeywords = TemplateUtils.GetTemplateSection(this.MetaTemplate, "[KEYWORDS]", "[/KEYWORDS]").Replace("[KEYWORDS]", string.Empty).Replace("[/KEYWORDS]", string.Empty);
            }

            #endregion

            #region Setup Breadcrumbs

            var breadCrumb = TemplateUtils.GetTemplateSection(sOutput, "[BREADCRUMB]", "[/BREADCRUMB]").Replace("[BREADCRUMB]", string.Empty).Replace("[/BREADCRUMB]", string.Empty);

            if (this.MainSettings.UseSkinBreadCrumb)
            {
                var ctlUtils = new ControlUtils();

                var groupUrl = ctlUtils.BuildUrl(this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, string.Empty, this.ForumInfo.ForumGroupId, -1, -1, -1, string.Empty, 1, -1, this.SocialGroupId);
                var forumUrl = ctlUtils.BuildUrl(this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumInfo.ForumGroupId, this.ForumInfo.ForumID, -1, -1, string.Empty, 1, -1, this.SocialGroupId);
                var topicUrl = ctlUtils.BuildUrl(this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumInfo.ForumGroupId, this.ForumInfo.ForumID, this.TopicId, this._topicURL, -1, -1, string.Empty, 1, -1, this.SocialGroupId);

                var sCrumb = "<a href=\"" + groupUrl + "\">" + this._groupName + "</a>|";
                sCrumb += "<a href=\"" + forumUrl + "\">" + this._forumName + "</a>";
                sCrumb += "|<a href=\"" + topicUrl + "\">" + this._topicSubject + "</a>";

                if (DotNetNuke.Modules.ActiveForums.Environment.UpdateBreadCrumb(this.Page.Controls, sCrumb))
                {
                    breadCrumb = string.Empty;
                }
            }

            sOutput = TemplateUtils.ReplaceSubSection(sOutput, breadCrumb, "[BREADCRUMB]", "[/BREADCRUMB]");

            #endregion

            // Parse Common Controls First
            sOutput = this.ParseControls(sOutput);

            // Note: If the containing element is not found, GetTemplateSection returns the entire template
            // This is desired behavior in this case as it's possible that the entire template is our topics container.
            var topic = TemplateUtils.GetTemplateSection(sOutput, "[AF:CONTROL:CALLBACK]", "[/AF:CONTROL:CALLBACK]");

            topic = this.ParseTopic(topic);

            if (!topic.Contains(Globals.ForumsControlsRegisterAMTag))
            {
                topic = Globals.ForumsControlsRegisterAMTag + topic;
            }

            topic = Utilities.LocalizeControl(topic);

            // If a template was passed in, we don't need to do this.
            if (bFullTopic)
            {
                sOutput = TemplateUtils.ReplaceSubSection(sOutput, "<asp:placeholder id=\"plhTopic\" runat=\"server\" />", "[AF:CONTROL:CALLBACK]", "[/AF:CONTROL:CALLBACK]");

                sOutput = Utilities.LocalizeControl(sOutput);
                sOutput = Utilities.StripTokens(sOutput);

                // If we added a banner, make sure we register than banner tag
                if (sOutput.Contains("<dnn:BANNER") && !sOutput.Contains(Globals.BannerRegisterTag))
                {
                    sOutput = Globals.BannerRegisterTag + sOutput;
                }

                var ctl = this.ParseControl(sOutput);
                if (ctl != null)
                {
                    this.Controls.Add(ctl);
                }
            }

            // Create a topic placeholder if we don't have one.
            var plhTopic = this.FindControl("plhTopic") as PlaceHolder;
            if (plhTopic == null)
            {
                plhTopic = new PlaceHolder { ID = "plhTopic" };
                this.Controls.Add(plhTopic);
            }

            // Parse and add out topic control

            // If we added a banner, make sure we register than banner tag
            // This has to be done separately from sOutput because they are usually different.
            if (topic.Contains("<dnn:BANNER") && !topic.Contains(Globals.BannerRegisterTag))
            {
                topic = Globals.BannerRegisterTag + topic;
            }

            var ctlTopic = this.ParseControl(topic);
            if (ctlTopic != null)
            {
                plhTopic.Controls.Add(ctlTopic);
            }

            // Add helper controls
            // Quick Jump DropDownList
            var plh = this.FindControl("plhQuickJump") as PlaceHolder;
            if (plh != null)
            {
                plh.Controls.Clear();
                var ctlForumJump = new af_quickjump
                {
                    ModuleConfiguration = this.ModuleConfiguration,
                    ForumModuleId = this.ForumModuleId,
                    ModuleId = this.ModuleId,
                    dtForums = null,
                    ForumId = this.ForumId,
                    EnableViewState = false,
                    ForumInfo = this.ForumId > 0 ? this.ForumInfo : null
                };

                if (!plh.Controls.Contains(ctlForumJump))
                {
                    plh.Controls.Add(ctlForumJump);
                }

            }

            // Poll Container
            plh = this.FindControl("plhPoll") as PlaceHolder;
            if (plh != null)
            {
                plh.Controls.Clear();
                plh.Controls.Add(new af_polls
                {
                    ModuleConfiguration = this.ModuleConfiguration,
                    TopicId = this.TopicId,
                    ForumId = this.ForumId
                });
            }

            // Quick Reply
            if (this.CanReply) //  && _bLocked == false) /*always create quick reply in case topic is locked and unlocked from javascript */
            {
                plh = this.FindControl("plhQuickReply") as PlaceHolder;
                if (plh != null)
                {
                    plh.Controls.Clear();
                    var ctlQuickReply = (af_quickreplyform)this.LoadControl("~/DesktopModules/ActiveForums/controls/af_quickreply.ascx");
                    ctlQuickReply.ModuleConfiguration = this.ModuleConfiguration;
                    ctlQuickReply.CanTrust = this._bTrust;
                    ctlQuickReply.ModApprove = this._bModApprove;
                    ctlQuickReply.IsTrusted = this._isTrusted;
                    ctlQuickReply.Subject = Utilities.GetSharedResource("[RESX:SubjectPrefix]") + " " + this._topicSubject;
                    ctlQuickReply.AllowSubscribe = this._allowSubscribe;
                    ctlQuickReply.AllowHTML = this._allowHTML;
                    ctlQuickReply.AllowScripts = this._allowScript;
                    ctlQuickReply.ForumId = this.ForumId;
                    ctlQuickReply.SocialGroupId = this.SocialGroupId;
                    ctlQuickReply.ForumModuleId = this.ForumModuleId;
                    ctlQuickReply.ForumTabId = this.ForumTabId;
                    ctlQuickReply.RequireCaptcha = true;

                    if (this.ForumId > 0)
                    {
                        ctlQuickReply.ForumInfo = this.ForumInfo;
                    }

                    plh.Controls.Add(ctlQuickReply);
                }
            }

            // Topic Sort
            plh = this.FindControl("plhTopicSort") as PlaceHolder;
            if (plh != null)
            {
                plh.Controls.Clear();
                var ctlTopicSort = (af_topicsorter)this.LoadControl("~/DesktopModules/ActiveForums/controls/af_topicsort.ascx");
                ctlTopicSort.ModuleConfiguration = this.ModuleConfiguration;
                ctlTopicSort.ForumId = this.ForumId;
                ctlTopicSort.DefaultSort = this._defaultSort;
                if (this.ForumId > 0)
                {
                    ctlTopicSort.ForumInfo = this.ForumInfo;
                }

                plh.Controls.Add(ctlTopicSort);
            }

            // Topic Status
            plh = this.FindControl("plhStatus") as PlaceHolder;
            if (plh != null)
            {
                plh.Controls.Clear();
                var ctlTopicStatus = (af_topicstatus)this.LoadControl("~/DesktopModules/ActiveForums/controls/af_topicstatus.ascx");
                ctlTopicStatus.ModuleConfiguration = this.ModuleConfiguration;
                ctlTopicStatus.Status = this._statusId;
                ctlTopicStatus.ForumId = this.ForumId;
                if (this.ForumId > 0)
                {
                    ctlTopicStatus.ForumInfo = this.ForumInfo;
                }

                plh.Controls.Add(ctlTopicStatus);
            }

            this.BuildPager();
        }

        /*
        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                if ((ctrl) is ForumBase)
                {
                    ((ForumBase)ctrl).ModuleConfiguration = this.ModuleConfiguration;
                    ((ForumBase)ctrl).TopicId = TopicId;
                }
                if (ctrl.Controls.Count > 0)
                {
                    LinkControls(ctrl.Controls);
                }
            }
        }*/

        private string ParseControls(string sOutput)
        {
            // Do a few things before we switch to a string builder

            // Add This -- obsolete so just remove
            if (sOutput.Contains("[AF:CONTROL:ADDTHIS"))
            {
                int inStart = sOutput.IndexOf("[AF:CONTROL:ADDTHIS", 0) + 1 + 19;
                int inEnd = sOutput.IndexOf("]", inStart - 1) + 1;
                sOutput.Remove(inStart, inEnd - inStart + 1);
            }

            // Banners
            if (sOutput.Contains("[BANNER"))
            {
                sOutput = sOutput.Replace("[BANNER]", "<dnn:BANNER runat=\"server\" GroupName=\"FORUMS\" BannerCount=\"1\" EnableViewState=\"False\" />");

                const string pattern = @"(\[BANNER:(.+?)\])";
                const string sBanner = "<dnn:BANNER runat=\"server\" BannerCount=\"1\" GroupName=\"$1\" EnableViewState=\"False\" />";

                sOutput = Regex.Replace(sOutput, pattern, sBanner);
            }

            // Hide Toolbar

            if (sOutput.Contains("[NOTOOLBAR]"))
            {
                if (HttpContext.Current.Items.Contains("ShowToolbar"))
                {
                    HttpContext.Current.Items["ShowToolbar"] = false;
                }
                else
                {
                    HttpContext.Current.Items.Add("ShowToolbar", false);
                }
            }

            // Now use the string builder to do all replacements
            var sbOutput = new StringBuilder(sOutput);

            if (this.Request.QueryString["dnnprintmode"] != null)
            {
                sbOutput.Replace("[ADDREPLY]", string.Empty);
                sbOutput.Replace("[QUICKREPLY]", string.Empty);
                sbOutput.Replace("[TOPICSUBSCRIBE]", string.Empty);
                sbOutput.Replace("[AF:CONTROL:PRINTER]", string.Empty);
                sbOutput.Replace("[AF:CONTROL:EMAIL]", string.Empty);
                sbOutput.Replace("[PAGER1]", string.Empty);
                sbOutput.Replace("[PAGER2]", string.Empty);
                sbOutput.Replace("[SORTDROPDOWN]", string.Empty);
                sbOutput.Replace("[POSTRATINGBUTTON]", string.Empty);
                sbOutput.Replace("[JUMPTO]", string.Empty);
                sbOutput.Replace("[NEXTTOPIC]", string.Empty);
                sbOutput.Replace("[PREVTOPIC]", string.Empty);
                sbOutput.Replace("[AF:CONTROL:STATUS]", string.Empty);
                sbOutput.Replace("[ACTIONS:DELETE]", string.Empty);
                sbOutput.Replace("[ACTIONS:EDIT]", string.Empty);
                sbOutput.Replace("[ACTIONS:QUOTE]", string.Empty);
                sbOutput.Replace("[ACTIONS:REPLY]", string.Empty);
                sbOutput.Replace("[ACTIONS:ANSWER]", string.Empty);
                sbOutput.Replace("[ACTIONS:ALERT]", string.Empty);
                sbOutput.Replace("[ACTIONS:BAN]", string.Empty);
                sbOutput.Replace("[ACTIONS:MOVE]", string.Empty);
                sbOutput.Replace("[RESX:SortPosts]:", string.Empty);
                sbOutput.Append("<img src=\"" + this.Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath + "spacer.gif") + "\" width=\"800\" height=\"1\" runat=\"server\" alt=\"---\" />");
            }

            sbOutput.Replace("[NOPAGING]", "<script type=\"text/javascript\">afpagesize=" + int.MaxValue + ";</script>");
            sbOutput.Replace("[NOTOOLBAR]", string.Empty);

            // Subscribe Option
            if (this._bSubscribe)
            {
                var subControl = new ToggleSubscribe(this.ForumModuleId, this.ForumId, this.TopicId, 1);
                subControl.Checked = this._isSubscribedTopic;
                subControl.Text = "[RESX:Subscribe]";
                sbOutput.Replace("[TOPICSUBSCRIBE]", subControl.Render());
            }
            else
            {
                sbOutput.Replace("[TOPICSUBSCRIBE]", string.Empty);
            }

            // Topic and post actions
            var topicActions = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.TokenGet(this.ForumModuleId, "topic", "[AF:CONTROL:TOPICACTIONS]");
            var postActions = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.TokenGet(this.ForumModuleId, "topic", "[AF:CONTROL:POSTACTIONS]");
            if (sOutput.Contains("[AF:CONTROL:TOPICACTIONS]"))
            {
                this._useListActions = true;
                sbOutput.Replace("[AF:CONTROL:TOPICACTIONS]", topicActions);
                sbOutput.Replace("[AF:CONTROL:POSTACTIONS]", postActions);
            }

            // Quick Reply
            if (this.CanReply)
            {
                var @params = new List<string> {
                    $"{ParamKeys.ViewType}={Views.Post}",
                    $"{ParamKeys.TopicId}={this.TopicId}",
                    $"{ParamKeys.ForumId}={this.ForumId}",
                };
                if (this.SocialGroupId > 0)
                {
                    @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
                }

                if (this._bLocked)
                {
                    sbOutput.Replace("[ADDREPLY]", "<span class=\"dcf-topic-lock-locked-label\" class=\"afnormal\">[RESX:TopicLocked]</span><a href=\"" + Utilities.NavigateURL(this.TabId, "", @params.ToArray()) + "\" class=\"dnnPrimaryAction dcf-topic-reply-link dcf-topic-reply-locked\">[RESX:AddReply]</a>");
                    sbOutput.Replace("[QUICKREPLY]", "<div class=\"dcf-quickreply-wrapper\" style=\"display:none;\"><asp:placeholder id=\"plhQuickReply\" runat=\"server\" /></div>");
                }
                else
                {
                    sbOutput.Replace("[ADDREPLY]", "<span class=\"dcf-topic-lock-locked-label\" class=\"afnormal\"></span><a href=\"" + Utilities.NavigateURL(this.TabId, "", @params.ToArray()) + "\" class=\"dnnPrimaryAction dcf-topic-reply-link dcf-topic-reply-unlocked\">[RESX:AddReply]</a>");
                    sbOutput.Replace("[QUICKREPLY]", "<div class=\"dcf-quickreply-wrapper\" style=\"display:block;\"><asp:placeholder id=\"plhQuickReply\" runat=\"server\" /></div>");
                }
            }
            else
            {
                if (!this.Request.IsAuthenticated)
                {
                    DotNetNuke.Abstractions.Portals.IPortalSettings PortalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
                    string LoginUrl = PortalSettings.LoginTabId > 0 ? Utilities.NavigateURL(PortalSettings.LoginTabId, "", "returnUrl=" + this.Request.RawUrl) : Utilities.NavigateURL(this.TabId, "", "ctl=login&returnUrl=" + this.Request.RawUrl);

                    string onclick = string.Empty;
                    if (PortalSettings.EnablePopUps && PortalSettings.LoginTabId == Null.NullInteger && !AuthenticationController.HasSocialAuthenticationEnabled(this))
                    {
                        onclick = " onclick=\"return " + UrlUtils.PopUpUrl(HttpUtility.UrlDecode(LoginUrl), this, this.PortalSettings, true, false, 300, 650) + "\"";
                    }

                    sbOutput.Replace("[ADDREPLY]", $"<span class=\"dcf-auth-false-login\">{string.Format(Utilities.GetSharedResource("[RESX:NotAuthorizedReplyPleaseLogin]"), LoginUrl, onclick)}</span>");
                }
                else
                {
                    sbOutput.Replace("[ADDREPLY]", "<span class=\"dcf-auth-false\">[RESX:NotAuthorizedReply]</span>");
                }

                sbOutput.Replace("[QUICKREPLY]", string.Empty);
            }

            if (sOutput.Contains("[SPLITBUTTONS]") && this._bModSplit && (this._replyCount > 0))
            {
                sbOutput.Replace("[SPLITBUTTONS]", TemplateCache.GetCachedTemplate(this.ForumModuleId, "TopicSplitButtons"));
                sbOutput.Replace("[TOPICID]", this.TopicId.ToString());
            }
            else
            {
                sbOutput.Replace("[SPLITBUTTONS]", string.Empty);
            }

            // no longer using this
            sbOutput.Replace("[SPLITBUTTONS2]", string.Empty);

            // Parent Forum Link
            if (sOutput.Contains("[PARENTFORUMLINK]"))
            {
                if (this.ForumInfo.ParentForumId > 0)
                {
                    if (this.MainSettings.UseShortUrls)
                    {
                        sbOutput.Replace(oldValue: "[PARENTFORUMLINK]", "<a href=\"" + Utilities.NavigateURL(this.TabId, "", new[] { ParamKeys.ForumId + "=" + this.ForumInfo.ParentForumId }) + "\">" + this.ForumInfo.ParentForumName + "</a>");
                    }
                    else
                    {
                        sbOutput.Replace("[PARENTFORUMLINK]", "<a href=\"" + Utilities.NavigateURL(this.TabId, "", new[] { ParamKeys.ViewType + "=" + Views.Topics, ParamKeys.ForumId + "=" + this.ForumInfo.ParentForumId }) + "\">" + this.ForumInfo.ParentForumName + "</a>");
                    }
                }
                else if (this.ForumInfo.ForumGroupId > 0)
                    sbOutput.Replace("[PARENTFORUMLINK]", "<a href=\"" + Utilities.NavigateURL(this.TabId) + "\">" + this.ForumInfo.GroupName + "</a>");
            }

            // Parent Forum Name
            if (string.IsNullOrEmpty(this.ForumInfo.ParentForumName))
            {
                sbOutput.Replace("[PARENTFORUMNAME]", this.ForumInfo.ParentForumName);
            }

            // ForumLinks

            var ctlUtils = new ControlUtils();
            var groupUrl = ctlUtils.BuildUrl(this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, string.Empty, this.ForumInfo.ForumGroupId, -1, -1, -1, string.Empty, 1, -1, this.SocialGroupId);
            var forumUrl = ctlUtils.BuildUrl(this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumInfo.ForumGroupId, this.ForumInfo.ForumID, -1, -1, string.Empty, 1, -1, this.SocialGroupId);
            var topicUrl = ctlUtils.BuildUrl(this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumInfo.ForumGroupId, this.ForumInfo.ForumID, this.TopicId, this._topicURL, -1, -1, string.Empty, 1, -1, this.SocialGroupId);

            sbOutput.Replace("[FORUMMAINLINK]", "<a href=\"" + this.NavigateUrl(this.TabId) + "\">[RESX:ForumMain]</a>");
            sbOutput.Replace("[FORUMGROUPLINK]", "<a href=\"" + groupUrl + "\">" + this._groupName + "</a>");
            sbOutput.Replace("[FORUMLINK]", "<a href=\"" + forumUrl + "\">" + this._forumName + "</a>");

            // Names and Ids
            sbOutput.Replace("[FORUMID]", this.ForumId.ToString());
            sbOutput.Replace("[FORUMNAME]", this._forumName);
            sbOutput.Replace("[GROUPNAME]", this._groupName);

            // Printer Friendly Link
            var sURL = "<a rel=\"nofollow\" href=\"" + Utilities.NavigateURL(this.TabId, "", ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + this.TopicId, "mid=" + this.ModuleId, "dnnprintmode=true") + "?skinsrc=" + HttpUtility.UrlEncode("[G]" + SkinController.RootSkin + "/" + Common.Globals.glbHostSkinFolder + "/" + "No Skin") + "&amp;containersrc=" + HttpUtility.UrlEncode("[G]" + SkinController.RootContainer + "/" + Common.Globals.glbHostSkinFolder + "/" + "No Container") + "\" target=\"_blank\">";
            // sURL += "<img src=\"" + _myThemePath + "/images/print16.png\" border=\"0\" alt=\"[RESX:PrinterFriendly]\" /></a>";
            sURL += "<i class=\"fa fa-print fa-fw fa-blue\"></i></a>";
            sbOutput.Replace("[AF:CONTROL:PRINTER]", sURL);

            // Email Link
            if (this.Request.IsAuthenticated)
            {
                sURL = Utilities.NavigateURL(this.TabId, "", new[] { ParamKeys.ViewType + "=sendto", ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId });
                // sbOutput.Replace("[AF:CONTROL:EMAIL]", "<a href=\"" + sURL + "\" rel=\"nofollow\"><img src=\"" + _myThemePath + "/images/email16.png\" border=\"0\" alt=\"[RESX:EmailThis]\" /></a>");
                sbOutput.Replace("[AF:CONTROL:EMAIL]", "<a href=\"" + sURL + "\" rel=\"nofollow\"><i class=\"fa fa-envelope-o fa-fw fa-blue\"></i></a>");
            }
            else
                sbOutput.Replace("[AF:CONTROL:EMAIL]", string.Empty);

            // RSS Link
            if (this._bAllowRSS)
            {
                var url = Common.Globals.AddHTTP(Common.Globals.GetDomainName(this.Request)) + "/DesktopModules/ActiveForums/feeds.aspx?portalid=" + this.PortalId + "&forumid=" + this.ForumId + "&tabid=" + this.TabId + "&moduleid=" + this.ModuleId;
                sbOutput.Replace("[RSSLINK]", "<a href=\"" + url + "\"><img src=\"~/DesktopModules/ActiveForums/themes/" + this._myTheme + "/images/rss.png\" runat=server border=\"0\" alt=\"[RESX:RSS]\" /></a>");
            }
            else
                sbOutput.Replace("[RSSLINK]", string.Empty);

            // Subject
            this._topicSubject = this._topicSubject.Replace("[", "&#91");
            this._topicSubject = this._topicSubject.Replace("]", "&#93");
            sbOutput.Replace("[SUBJECT]", Utilities.StripHTMLTag(this._topicSubject));

            // Reply Count
            sbOutput.Replace("[REPLYCOUNT]", this._replyCount.ToString());
            sbOutput.Replace("[AF:LABEL:ReplyCount]", this._replyCount.ToString());

            // Topic Subscriber Count
            sbOutput.Replace("[TOPICSUBSCRIBERCOUNT]", this._topicSubscriberCount.ToString());

            // Forum Subscriber Count
            sbOutput.Replace("[FORUMSUBSCRIBERCOUNT]", this._forumSubscriberCount.ToString());

            // View Count
            sbOutput.Replace("[VIEWCOUNT]", this._viewCount.ToString());

            DotNetNuke.Entities.Portals.PortalSettings portalSettings = Utilities.GetPortalSettings(this.PortalId);
            // Last Post
            sbOutput.Replace("[AF:LABEL:LastPostDate]", this._lastPostDate);
            sbOutput.Replace("[AF:LABEL:LastPostAuthor]", UserProfiles.GetDisplayName(portalSettings, this.ForumModuleId, true, this._bModApprove, this.ForumUser.IsAdmin || this.ForumUser.IsSuperUser, this._lastPostAuthor.AuthorId, this._lastPostAuthor.Username, this._lastPostAuthor.FirstName, this._lastPostAuthor.LastName, this._lastPostAuthor.DisplayName));

            // Topic Info
            sbOutput.Replace("[AF:LABEL:TopicAuthor]", UserProfiles.GetDisplayName(portalSettings, this.ForumModuleId, false, false, false, this._topicAuthorId, this._topicAuthorDisplayName, string.Empty, string.Empty, this._topicAuthorDisplayName));
            sbOutput.Replace("[AF:LABEL:TopicDateCreated]", this._topicDateCreated);

            // Pagers
            if (this._pageSize == int.MaxValue)
            {
                sbOutput.Replace("[PAGER1]", string.Empty);
                sbOutput.Replace("[PAGER2]", string.Empty);
            }
            else
            {
                sbOutput.Replace("[PAGER1]", "<am:pagernav id=\"Pager1\" runat=\"server\" EnableViewState=\"False\" />");
                sbOutput.Replace("[PAGER2]", "<am:pagernav id=\"Pager2\" runat=\"server\" EnableViewState=\"False\" />");
            }

            // Sort
            sbOutput.Replace("[SORTDROPDOWN]", "<asp:placeholder id=\"plhTopicSort\" runat=\"server\" />");
            if (sOutput.Contains("[POSTRATINGBUTTON]"))
            {
                var rateControl = new Ratings(this.ModuleId, this.ForumId, this.TopicId, true, this._topicRating);
                sbOutput.Replace("[POSTRATINGBUTTON]", rateControl.Render());
            }

            // Jump To
            sbOutput.Replace("[JUMPTO]", "<asp:placeholder id=\"plhQuickJump\" runat=\"server\" />");

            // Next Topic
            if (this._nextTopic == 0)
                sbOutput.Replace("[NEXTTOPIC]", string.Empty);
            else
            {
                string nextTopic;
                if (this.MainSettings.UseShortUrls)
                {
                    if (this.SocialGroupId > 0)
                    {
                        nextTopic = Utilities.NavigateURL(this.TabId, "", ParamKeys.TopicId + "=" + this._nextTopic + "&" + Literals.GroupId + "=" + this.SocialGroupId);
                    }
                    else
                    {
                        nextTopic = Utilities.NavigateURL(this.TabId, "", ParamKeys.TopicId + "=" + this._nextTopic);
                    }
                }
                else
                {
                    if (this.SocialGroupId > 0)
                    {
                        nextTopic = Utilities.NavigateURL(this.TabId, "", ParamKeys.ForumId + "=" + this.ForumId + "&" + ParamKeys.TopicId + "=" + this._nextTopic + "&" + ParamKeys.ViewType + "=" + Views.Topic + "&" + ParamKeys.GroupId + this.SocialGroupId);
                    }
                    else
                    {
                        nextTopic = Utilities.NavigateURL(this.TabId, "", ParamKeys.ForumId + "=" + this.ForumId + "&" + ParamKeys.TopicId + "=" + this._nextTopic + "&" + ParamKeys.ViewType + "=" + Views.Topic);
                    }
                }

                sbOutput.Replace("[NEXTTOPIC]", "<a href=\"" + nextTopic + "\" rel=\"nofollow\" title=\"[RESX:PrevTopic]\"><span>[RESX:NextTopic]</span><i class=\"fa fa-chevron-right\" aria-hidden=\"true\"></i></a>");
            }

            // Previous Topic
            if (this._prevTopic == 0)
                sbOutput.Replace("[PREVTOPIC]", string.Empty);
            else
            {
                string prevTopic;
                if (this.MainSettings.UseShortUrls)
                {
                    if (this.SocialGroupId > 0)
                    {
                        prevTopic = Utilities.NavigateURL(this.TabId, "", ParamKeys.TopicId + "=" + this._prevTopic + "&" + Literals.GroupId + "=" + this.SocialGroupId);
                    }
                    else
                    {
                        prevTopic = Utilities.NavigateURL(this.TabId, "", ParamKeys.TopicId + "=" + this._prevTopic);
                    }
                }
                else
                {
                    if (this.SocialGroupId > 0)
                    {
                        prevTopic = Utilities.NavigateURL(this.TabId, "", ParamKeys.ForumId + "=" + this.ForumId + "&" + ParamKeys.TopicId + "=" + this._prevTopic + "&" + ParamKeys.ViewType + "=" + Views.Topic + "&" + Literals.GroupId + "=" + this.SocialGroupId);
                    }
                    else
                    {
                        prevTopic = Utilities.NavigateURL(this.TabId, "", ParamKeys.ForumId + "=" + this.ForumId + "&" + ParamKeys.TopicId + "=" + this._prevTopic + "&" + ParamKeys.ViewType + "=" + Views.Topic);
                    }
                }

                sbOutput.Replace("[PREVTOPIC]", "<a href=\"" + prevTopic + "\" rel=\"nofollow\" title=\"[RESX:PrevTopic]\"><i class=\"fa fa-chevron-left\" aria-hidden=\"true\"></i><span>[RESX:PrevTopic]</span></a>");
            }

            // Topic Status
            if (((this._bRead && this._topicAuthorId == this.UserId) || this._bModEdit) & this._statusId >= 0)
            {
                sbOutput.Replace("[AF:CONTROL:STATUS]", "<asp:placeholder id=\"plhStatus\" runat=\"server\" />");
                // sbOutput.Replace("[AF:CONTROL:STATUSICON]", "<img alt=\"[RESX:PostStatus" + _statusId.ToString() + "]\" src=\"" + _myThemePath + "/images/status" + _statusId.ToString() + ".png\" />");
                sbOutput.Replace("[AF:CONTROL:STATUSICON]", "<div><i class=\"fa fa-status" + this._statusId.ToString() + " fa-red fa-2x\"></i></div>");
            }
            else if (this._statusId >= 0)
            {
                sbOutput.Replace("[AF:CONTROL:STATUS]", string.Empty);
                // sbOutput.Replace("[AF:CONTROL:STATUSICON]", "<img alt=\"[RESX:PostStatus" + _statusId.ToString() + "]\" src=\"" + _myThemePath + "/images/status" + _statusId.ToString() + ".png\" />");
                sbOutput.Replace("[AF:CONTROL:STATUSICON]", "<div><i class=\"fa fa-status" + this._statusId.ToString() + " fa-red fa-2x\"></i></div>");
            }
            else
            {
                sbOutput.Replace("[AF:CONTROL:STATUS]", string.Empty);
                sbOutput.Replace("[AF:CONTROL:STATUSICON]", string.Empty);
            }

            // Poll
            if (this._topicType == (int)TopicTypes.Poll)
            {
                sbOutput.Replace("[AF:CONTROL:POLL]", "<asp:placeholder id=\"plhPoll\" runat=\"server\" />");
            }
            else
            {
                sbOutput.Replace("[AF:CONTROL:POLL]", string.Empty);
            }

            return sbOutput.ToString();
        }

        private string ParseTopic(string sOutput)
        {
            // Process our separators which are injected between rows.
            // Minimum index is 1.  If zero is specified, it will be treated as the default.

            const string pattern = @"\[REPLYSEPARATOR:(\d+?)\]";
            var separators = new Dictionary<int, string>();
            if (sOutput.Contains("[REPLYSEPARATOR"))
            {
                var defaultSeparator = TemplateUtils.GetTemplateSection(sOutput, "[REPLYSEPARATOR]", "[/REPLYSEPARATOR]", false);
                if (!string.IsNullOrWhiteSpace(defaultSeparator))
                {
                    separators.Add(0, defaultSeparator.Replace("[REPLYSEPARATOR]", string.Empty).Replace("[/REPLYSEPARATOR]", string.Empty));
                    sOutput = TemplateUtils.ReplaceSubSection(sOutput, string.Empty, "[REPLYSEPARATOR]", "[/REPLYSEPARATOR]");
                }

                foreach (Match match in Regex.Matches(sOutput, pattern))
                {
                    var rowIndex = int.Parse(match.Groups[1].Value);
                    var startTag = string.Format("[REPLYSEPARATOR:{0}]", rowIndex);
                    var endTag = string.Format("[/REPLYSEPARATOR:{0}]", rowIndex);

                    var separator = TemplateUtils.GetTemplateSection(sOutput, startTag, endTag, false);
                    if (string.IsNullOrWhiteSpace(separator))
                    {
                        continue;
                    }

                    separators[rowIndex] = separator.Replace(startTag, string.Empty).Replace(endTag, string.Empty);
                    sOutput = TemplateUtils.ReplaceSubSection(sOutput, string.Empty, startTag, endTag);
                }
            }

            // Prorcess topic and reply templates.

            var sTopicTemplate = TemplateUtils.GetTemplateSection(sOutput, "[TOPIC]", "[/TOPIC]");
            var sReplyTemplate = TemplateUtils.GetTemplateSection(sOutput, "[REPLIES]", "[/REPLIES]");
            var sTemp = string.Empty;
            var i = 0;

            if (this._dtTopic.Rows.Count > 0)
            {
                foreach (DataRow dr in this._dtTopic.Rows)
                {
                    // deal with our separator first
                    if (i > 0 && separators.Count > 0) // No separator before the first row
                    {
                        if (separators.ContainsKey(i)) // Specific row
                        {
                            sTemp += separators[i];
                        }
                        else if (separators.ContainsKey(0)) // Default
                        {
                            sTemp += separators[0];
                        }
                    }

                    var replyId = Convert.ToInt32(dr["ReplyId"]);

                    if (replyId == 0)
                    {
                        sTopicTemplate = this.ParseContent(dr, sTopicTemplate, i);
                    }
                    else
                    {
                        sTemp += this.ParseContent(dr, sReplyTemplate, i);
                    }

                    i++;
                }

                if (this._defaultSort == "ASC")
                {
                    sOutput = TemplateUtils.ReplaceSubSection(sOutput, sTemp, "[REPLIES]", "[/REPLIES]");
                    sOutput = TemplateUtils.ReplaceSubSection(sOutput, sTopicTemplate, "[TOPIC]", "[/TOPIC]");
                }
                else
                {
                    sOutput = TemplateUtils.ReplaceSubSection(sOutput, sTemp + sTopicTemplate, "[REPLIES]", "[/REPLIES]");
                    sOutput = TemplateUtils.ReplaceSubSection(sOutput, string.Empty, "[TOPIC]", "[/TOPIC]");
                }

                if (sTopicTemplate.Contains("[BODY]"))
                {
                    sOutput = sOutput.Replace(sTopicTemplate, string.Empty);
                }

            }

            return sOutput;
        }

        private string ParseContent(DataRow dr, string tempate, int rowcount)
        {
            var sOutput = tempate;

            var replyId = dr.GetInt("ReplyId");
            var topicId = dr.GetInt("TopicId");
            var postId = replyId == 0 ? topicId : replyId;
            var contentId = dr.GetInt("ContentId");
            var dateCreated = dr.GetDateTime("DateCreated");
            var dateUpdated = dr.GetDateTime("DateUpdated");
            var authorId = dr.GetInt("AuthorId");
            var username = dr.GetString("Username");
            var firstName = dr.GetString("FirstName");
            var lastName = dr.GetString("LastName");
            var displayName = dr.GetString("DisplayName");
            var userTopicCount = dr.GetInt("TopicCount");
            var userReplyCount = dr.GetInt("ReplyCount");
            var postCount = userTopicCount + userReplyCount;
            var userCaption = dr.GetString("UserCaption");
            var body = HttpUtility.HtmlDecode(dr.GetString("Body"));
            var subject = HttpUtility.HtmlDecode(dr.GetString("Subject"));
            var tags = dr.GetString("Tags");
            var signature = dr.GetString("Signature");
            var ipAddress = dr.GetString("IPAddress");
            var memberSince = dr.GetDateTime("MemberSince");
            var avatarDisabled = dr.GetBoolean("AvatarDisabled");
            var userRoles = dr.GetString("UserRoles");
            var isUserOnline = dr.GetBoolean("IsUserOnline");
            var replyStatusId = replyId > 0 ? dr.GetInt("StatusId") : 0;
            var totalPoints = this._enablePoints ? dr.GetInt("UserTotalPoints") : 0;
            var answerCount = dr.GetInt("AnswerCount");
            var rewardPoints = dr.GetInt("RewardPoints");
            var dateLastActivity = dr.GetDateTime("DateLastActivity");
            var signatureDisabled = dr.GetBoolean("SignatureDisabled");

            DotNetNuke.Entities.Users.UserInfo author = DotNetNuke.Entities.Users.UserController.Instance.GetUser(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId, authorId);

            // Populate the user object with the post author info.
            var up = new User
            {
                UserId = authorId,
                UserName = username,
                FirstName = firstName.Replace("&amp;#", "&#"),
                LastName = lastName.Replace("&amp;#", "&#"),
                DisplayName = displayName.Replace("&amp;#", "&#"),
                Profile =
                {
                    UserCaption = userCaption,
                    Signature = signature,
                    DateCreated = memberSince,
                    AvatarDisabled = avatarDisabled,
                    Roles = userRoles,
                    ReplyCount = userReplyCount,
                    TopicCount = userTopicCount,
                    AnswerCount = answerCount,
                    RewardPoints = rewardPoints,
                    DateLastActivity = dateLastActivity,
                    PrefBlockAvatars = this.UserPrefHideAvatars,
                    PrefBlockSignatures = this.UserPrefHideSigs,
                    IsUserOnline = isUserOnline,
                    SignatureDisabled = signatureDisabled
                }
            };
            if (author != null)
            {
                up.Email = author.Email;
            }

            // Perform Profile Related replacements
            sOutput = TemplateUtils.ParseProfileTemplate(sOutput, up, this.PortalId, this.ForumModuleId, this.ImagePath, this.CurrentUserType, true, this.UserPrefHideAvatars, this.UserPrefHideSigs, ipAddress, this.UserId, this.TimeZoneOffset);

            // Replace Tags Control
            if (string.IsNullOrWhiteSpace(tags))
                sOutput = TemplateUtils.ReplaceSubSection(sOutput, string.Empty, "[AF:CONTROL:TAGS]", "[/AF:CONTROL:TAGS]");
            else
            {
                sOutput = sOutput.Replace("[AF:CONTROL:TAGS]", string.Empty);
                sOutput = sOutput.Replace("[/AF:CONTROL:TAGS]", string.Empty);
                var tagList = string.Empty;
                foreach (var tag in tags.Split(','))
                {
                    if (tagList != string.Empty)
                    {
                        tagList += ", ";
                    }

                    tagList += "<a href=\"" + Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ViewType + "=search", ParamKeys.Tags + "=" + HttpUtility.UrlEncode(tag) }) + "\">" + tag + "</a>";
                }

                sOutput = sOutput.Replace("[AF:LABEL:TAGS]", tagList);
            }

            // Use a string builder from here on out.

            var sbOutput = new StringBuilder(sOutput);

            if (this._bModSplit)
            {
                sbOutput = sbOutput.Replace("[SPLITCHECKBOX]", "<div class=\"dcf-split-checkbox\" style=\"display:none;\"><input id=\"dcf-split-checkbox-" + replyId + "\" type=\"checkbox\" onChange=\"amaf_splitCheck(this);\" value=\"" + replyId + "\" /><label for=\"dcf-split-checkbox-" + replyId + "\" class=\"dcf-split-checkbox-label\">[RESX:SplitCreate]</label></div>");
            }
            else
            {
                sbOutput = sbOutput.Replace("[SPLITCHECKBOX]", string.Empty);
            }

            // Row CSS Classes
            if (rowcount % 2 == 0)
            {
                sbOutput.Replace("[POSTINFOCSS]", "afpostinfo afpostinfo1");
                sbOutput.Replace("[POSTTOPICCSS]", "afposttopic afpostreply1");
                sbOutput.Replace("[POSTREPLYCSS]", "afpostreply afpostreply1");
            }
            else
            {
                sbOutput.Replace("[POSTTOPICCSS]", "afposttopic afpostreply2");
                sbOutput.Replace("[POSTINFOCSS]", "afpostinfo afpostinfo2");
                sbOutput.Replace("[POSTREPLYCSS]", "afpostreply afpostreply2");
            }

            // Description
            if (replyId == 0)
            {
                sbOutput.Replace("[POSTID]", topicId.ToString());
                this._topicDescription = Utilities.StripHTMLTag(body).Trim();
                this._topicDescription = this._topicDescription.Replace(System.Environment.NewLine, string.Empty);
                if (this._topicDescription.Length > 255)
                {
                    this._topicDescription = this._topicDescription.Substring(0, 255);
                }
            }
            else
            {
                sbOutput.Replace("[POSTID]", replyId.ToString());
            }

            sbOutput.Replace("[FORUMID]", this.ForumId.ToString());
            sbOutput.Replace("[REPLYID]", replyId.ToString());
            sbOutput.Replace("[TOPICID]", topicId.ToString());
            sbOutput.Replace("[POSTDATE]", Utilities.GetUserFormattedDateTime(dateCreated, this.PortalId, this.UserId));
            sbOutput.Replace("[DATECREATED]", Utilities.GetUserFormattedDateTime(dateCreated, this.PortalId, this.UserId));

            // Parse Roles -- This should actually be taken care of in ParseProfileTemplate
            // if (sOutput.Contains("[ROLES:"))
            //    sOutput = TemplateUtils.ParseRoles(sOutput, (up.UserId == -1) ? string.Empty : up.Profile.Roles);

            // Delete Action
            if (this._bModDelete || (this._bDelete && authorId == this.UserId && !this._bLocked && ((replyId == 0 && this._replyCount == 0) || replyId > 0)))
            {
                if (this._useListActions)
                {
                    sbOutput.Replace("[ACTIONS:DELETE]", "<li onclick=\"amaf_postDel(" + this.ForumModuleId + "," + this.ForumId + "," + topicId + "," + replyId + ");\" title=\"[RESX:Delete]\"><i class=\"fa fa-trash-o fa-fw fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Delete]</span></li>");
                }
                else
                {
                    sbOutput.Replace("[ACTIONS:DELETE]", "<a href=\"javascript:void(0);\" class=\"af-actions\" onclick=\"amaf_postDel(" + this.ForumModuleId + "," + this.ForumId + "," + topicId + "," + replyId + ");\" title=\"[RESX:Delete]\"><i class=\"fa fa-trash-o fa-fw fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Delete]</span></a>");
                }
            }
            else
            {
                sbOutput.Replace("[ACTIONS:DELETE]", string.Empty);
            }

            if ((this.ForumUser.IsAdmin || this.ForumUser.IsSuperUser || this._bModUser) && (authorId != -1) && (authorId != this.UserId) && (author != null) && (!author.IsSuperUser) && (!author.IsAdmin))
            {
                var banParams = new List<string> { $"{ParamKeys.ViewType}={Views.ModerateBan}", ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + topicId, ParamKeys.ReplyId + "=" + replyId, ParamKeys.AuthorId + "=" + authorId };
                if (this._useListActions)
                {
                    sbOutput.Replace("[ACTIONS:BAN]", "<li onclick=\"window.location.href='" + Utilities.NavigateURL(this.TabId, "", banParams.ToArray()) + "';\" title=\"[RESX:Ban]\"><i class=\"fa fa-ban fa-fw fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Ban]</span></li>");
                }
                else
                {
                    sbOutput.Replace("[ACTIONS:BAN]", "<a class=\"af-actions\" href=\"" + Utilities.NavigateURL(this.TabId, "", banParams.ToArray()) + "\" tooltip=\"Deletes all posts for this user and unauthorizes the user.\" title=\"[RESX:Ban]\"><i class=\"fa fa-ban fa-fw fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Ban]</span></a>");
                }
            }
            else
            {
                sbOutput.Replace("[ACTIONS:BAN]", string.Empty);
            }

            // Edit Action
            if (this._bModEdit || (this._bEdit && authorId == this.UserId && (this._editInterval == 0 || SimulateDateDiff.DateDiff(SimulateDateDiff.DateInterval.Minute, dateCreated, DateTime.UtcNow) < this._editInterval)))
            {
                var sAction = PostActions.ReplyEdit;
                if (replyId == 0)
                {
                    sAction = PostActions.TopicEdit;
                }

                // var editParams = new[] { ParamKeys.ViewType + "=post", "action=" + sAction, ParamKeys.ForumId + "=" + ForumId, ParamKeys.TopicId + "=" + topicId, "postid=" + postId };
                var editParams = new List<string>() { $"{ParamKeys.ViewType}={Views.Post}", $"{ParamKeys.action}={sAction}", $"{ParamKeys.ForumId}={this.ForumId}", $"{ParamKeys.TopicId}={topicId}", $"{ParamKeys.PostId}={postId}" };
                if (this.SocialGroupId > 0)
                {
                    editParams.Add(Literals.GroupId + "=" + this.SocialGroupId);
                }

                if (this._useListActions)
                {
                    sbOutput.Replace("[ACTIONS:EDIT]", "<li onclick=\"window.location.href='" + Utilities.NavigateURL(this.TabId, "", editParams.ToArray()) + "';\" title=\"[RESX:Edit]\"><i class=\"fa fa-pencil fa-fw fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Edit]</span></li>");
                }
                else
                {
                    sbOutput.Replace("[ACTIONS:EDIT]", "<a class=\"af-actions\" href=\"" + Utilities.NavigateURL(this.TabId, "", editParams.ToArray()) + "\" title=\"[RESX:Edit]\"><i class=\"fa fa-pencil fa-fw fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Edit]</span></a>");
                }
            }
            else
            {
                sbOutput.Replace("[ACTIONS:EDIT]", string.Empty);
            }

            // Reply and Quote Actions
            if (!this._bLocked && this.CanReply)
            {
                var quoteParams = new List<string> { $"{ParamKeys.ViewType}={Views.Post}", $"{ParamKeys.ForumId}={this.ForumId}", $"{ParamKeys.TopicId}={topicId}", $"{ParamKeys.QuoteId}={postId}" };
                var replyParams = new List<string> { $"{ParamKeys.ViewType}={Views.Post}", $"{ParamKeys.ForumId}={this.ForumId}", $"{ParamKeys.TopicId}={topicId}", $"{ParamKeys.ReplyId}={postId}" };
                if (this.SocialGroupId > 0)
                {
                    quoteParams.Add(Literals.GroupId + "=" + this.SocialGroupId);
                    replyParams.Add(Literals.GroupId + "=" + this.SocialGroupId);
                }

                if (this._useListActions)
                {
                    sbOutput.Replace("[ACTIONS:QUOTE]", "<li onclick=\"window.location.href='" + Utilities.NavigateURL(this.TabId, "", quoteParams.ToArray()) + "';\" title=\"[RESX:Quote]\"><i class=\"fa fa-quote-left fa-fw fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Quote]</span></li>");
                    sbOutput.Replace("[ACTIONS:REPLY]", "<li onclick=\"window.location.href='" + Utilities.NavigateURL(this.TabId, "", replyParams.ToArray()) + "';\" title=\"[RESX:Reply]\"><i class=\"fa fa-reply fa-fw fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Reply]</span></li>");
                }
                else
                {
                    sbOutput.Replace("[ACTIONS:QUOTE]", "<a class=\"af-actions\" href=\"" + Utilities.NavigateURL(this.TabId, "", quoteParams.ToArray()) + "\" title=\"[RESX:Quote]\"><i class=\"fa fa-quote-left fa-fw fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Quote]</span></a>");
                    sbOutput.Replace("[ACTIONS:REPLY]", "<a class=\"af-actions\" href=\"" + Utilities.NavigateURL(this.TabId, "", replyParams.ToArray()) + "\" title=\"[RESX:Reply]\"><i class=\"fa fa-reply fa-fw fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Reply]</span></a>");
                }
            }
            else
            {
                sbOutput.Replace("[ACTIONS:QUOTE]", string.Empty);
                sbOutput.Replace("[ACTIONS:REPLY]", string.Empty);
            }

            if (this._bModMove)
            {
                sbOutput.Replace("[ACTIONS:MOVE]", "<li onclick=\"javascript:amaf_openMove(" + this.ModuleId + "," + this.ForumId + ",[TOPICID])\"';\" title=\"[RESX:Move]\"><i class=\"fa fa-exchange fa-rotate-90 fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Move]</span></li>");
            }
            else
            {
                sbOutput = sbOutput.Replace("[ACTIONS:MOVE]", string.Empty);
            }

            if (this._bModLock)
            {
                if (this._bLocked)
                {
                    sbOutput = sbOutput.Replace("[ACTIONS:LOCK]", "<li class=\"dcf-topic-lock-outer\" onclick=\"javascript:if(confirm('[RESX:Confirm:UnLock]')){amaf_Lock(" + this.ModuleId + "," + this.ForumId + ",[TOPICID]);};\" title=\"[RESX:UnLockTopic]\"><i class=\"fa fa-unlock fa-fm fa-blue dcf-topic-lock-inner\"></i><span class=\"dcf-topic-lock-text dcf-link-text \">[RESX:UnLock]</span></li>");
                }
                else
                {
                    sbOutput = sbOutput.Replace("[ACTIONS:LOCK]", "<li class=\"dcf-topic-lock-outer\" onclick=\"javascript:if(confirm('[RESX:Confirm:Lock]')){amaf_Lock(" + this.ModuleId + "," + this.ForumId + ",[TOPICID]);};\" title=\"[RESX:LockTopic]\"><i class=\"fa fa-lock fa-fm fa-blue dcf-topic-lock-inner\"></i><span class=\"dcf-topic-lock-text dcf-link-text\">[RESX:Lock]</span></li>");
                }
            }
            else
            {
                sbOutput = sbOutput.Replace("[ACTIONS:LOCK]", string.Empty);
            }

            if (this._bModPin)
            {
                if (this._bPinned)
                {
                    sbOutput.Replace("[ACTIONS:PIN]", "<li class=\"dcf-topic-pin-outer\" onclick=\"javascript:if(confirm('[RESX:Confirm:UnPin]')){amaf_Pin(" + this.ModuleId + "," + this.ForumId + ",[TOPICID]);};\" title=\"[RESX:UnPinTopic]\"><i class=\"fa fa-thumb-tack fa-fm fa-blue dcf-topic-pin-unpin dcf-topic-pin-inner\"></i><span class=\"dcf-topic-pin-text dcf-link-text\">[RESX:UnPin]</span></li>");
                }
                else
                {
                    sbOutput.Replace("[ACTIONS:PIN]", "<li class=\"dcf-topic-pin-outer\" onclick=\"javascript:if(confirm('[RESX:Confirm:Pin]')){amaf_Pin(" + this.ModuleId + "," + this.ForumId + ",[TOPICID]);};\" title=\"[RESX:PinTopic]\"><i class=\"fa fa-thumb-tack fa-fm fa-blue dcf-topic-pin-pin dcf-topic-pin-inner\"></i><span class=\"dcf-topic-pin-text dcf-link-text\">[RESX:Pin]</span></li>");
                }
            }
            else
            {
                sbOutput = sbOutput.Replace("[ACTIONS:PIN]", string.Empty);
            }

            sbOutput = sbOutput.Replace("[TOPICID]", this.TopicId.ToString());

            // Status
            if (this._statusId <= 0 || (this._statusId == 3 && replyStatusId == 0))
            {
                sbOutput.Replace("[ACTIONS:ANSWER]", string.Empty);
            }
            else if (replyStatusId == 1)
            {
                // Answered
                if (this._useListActions)
                {
                    sbOutput.Replace("[ACTIONS:ANSWER]", "<li class=\"af-answered\" title=\"[RESX:Status:Answer]\"><em></em>[RESX:Status:Answer]</li>");
                }
                else
                {
                    sbOutput.Replace("[ACTIONS:ANSWER]", "<span class=\"af-actions af-answered\" title=\"[RESX:Status:Answer]\"><em></em>[RESX:Status:Answer]</span>");
                }
            }
            else
            {
                // Not Answered
                if (replyId > 0 && ((this.UserId == this._topicAuthorId && !this._bLocked) || this._bModEdit))
                {
                    // Can mark answer
                    if (this._useListActions)
                    {
                        sbOutput.Replace("[ACTIONS:ANSWER]", "<li class=\"af-markanswer\" onclick=\"amaf_MarkAsAnswer(" + this.ModuleId.ToString() + "," + this.ForumId.ToString() + "," + topicId.ToString() + "," + replyId.ToString() + ");\" title=\"[RESX:Status:SelectAnswer]\"><em></em>[RESX:Status:SelectAnswer]</li>");
                    }
                    else
                    {
                        sbOutput.Replace("[ACTIONS:ANSWER]", "<a class=\"af-actions af-markanswer\" href=\"#\" onclick=\"amaf_MarkAsAnswer(" + this.ModuleId.ToString() + "," + this.ForumId.ToString() + "," + topicId.ToString() + "," + replyId.ToString() + "); return false;\" title=\"[RESX:Status:SelectAnswer]\"><em></em>[RESX:Status:SelectAnswer]</a>");
                    }
                }
                else
                {
                    // Display Nothing
                    sbOutput.Replace("[ACTIONS:ANSWER]", string.Empty);
                }
            }

            // User Edit Date
            if (dateUpdated == dateCreated || dateUpdated == DateTime.MinValue || dateUpdated == Utilities.NullDate())
            {
                sbOutput.Replace("[EDITDATE]", string.Empty);
            }
            else
            {
                sbOutput.Replace("[EDITDATE]", Utilities.GetUserFormattedDateTime(dateUpdated, this.PortalId, this.UserId));
            }

            // Mod Edit Date
            if (this._bModEdit)
            {
                if (dateCreated == dateUpdated || dateUpdated == DateTime.MinValue || dateUpdated == Utilities.NullDate())
                {
                    sbOutput.Replace("[MODEDITDATE]", string.Empty);
                }
                else
                {
                    sbOutput.Replace("[MODEDITDATE]", Utilities.GetUserFormattedDateTime(dateUpdated, this.PortalId, this.UserId));
                }
            }
            else
            {
                sbOutput.Replace("[MODEDITDATE]", string.Empty);
            }

            if (this._allowLikes)
            {
                (int count, bool liked) likes = new DotNetNuke.Modules.ActiveForums.Controllers.LikeController().Get(this.UserId, contentId);
                string image = likes.liked ? "fa-thumbs-o-up" : "fa-thumbs-up";

                if (this.CanReply)
                {
                    sbOutput = sbOutput.Replace("[LIKES]", "<i id=\"af-topicview-likes1-" + contentId.ToString() + "\" class=\"fa " + image + "\" style=\"cursor:pointer\" onclick=\"amaf_likePost(" + this.ModuleId + "," + this.ForumId + "," + contentId + ")\" > " + likes.count.ToString() + "</i>");
                    sbOutput = sbOutput.Replace("[LIKESx2]", "<i id=\"af-topicview-likes2-" + contentId.ToString() + "\" class=\"fa " + image + " fa-2x\" style=\"cursor:pointer\" onclick=\"amaf_likePost(" + this.ModuleId + "," + this.ForumId + "," + contentId + ")\" > " + likes.count.ToString() + "</i>");
                    sbOutput = sbOutput.Replace("[LIKESx3]", "<i id=\"af-topicview-likes3-" + contentId.ToString() + "\" class=\"fa " + image + " fa-3x\" style=\"cursor:pointer\" onclick=\"amaf_likePost(" + this.ModuleId + "," + this.ForumId + "," + contentId + ")\" > " + likes.count.ToString() + "</i>");
                }
                else
                {
                    sbOutput = sbOutput.Replace("[LIKES]", "<i id=\"af-topicview-likes1\" class=\"fa " + image + "\" style=\"cursor:default\" > " + likes.count.ToString() + "</i>");
                    sbOutput = sbOutput.Replace("[LIKESx2]", "<i id=\"af-topicview-likes2\" class=\"fa " + image + " fa-2x\" style=\"cursor:default\" > " + likes.count.ToString() + "</i>");
                    sbOutput = sbOutput.Replace("[LIKESx3]", "<i id=\"af-topicview-likes3\" class=\"fa " + image + " fa-3x\" style=\"cursor:default\" > " + likes.count.ToString() + "</i>");
                }
            }
            else
            {
                sbOutput = sbOutput.Replace("[LIKES]", string.Empty);
                sbOutput = sbOutput.Replace("[LIKESx2]", string.Empty);
                sbOutput = sbOutput.Replace("[LIKESx3]", string.Empty);
            }

            // Poll Results
            if (sOutput.Contains("[POLLRESULTS]"))
            {
                if (this._topicType == 1)
                {
                    var polls = new Polls();
                    sbOutput.Replace("[POLLRESULTS]", polls.PollResults(topicId, this.ImagePath));
                }
                else
                {
                    sbOutput.Replace("[POLLRESULTS]", string.Empty);
                }
            }

            // Mod Alert
            // var alertParams = new[] { ParamKeys.ViewType + "=modreport", ParamKeys.ForumId + "=" + ForumId, ParamKeys.TopicId + "=" + topicId, ParamKeys.ReplyId + "=" + postId };
            var alertParams = new List<string> { $"{ParamKeys.ViewType}=modreport", $"{ParamKeys.ForumId}={this.ForumId}", $"{ParamKeys.TopicId}={topicId}", "${ParamKeys.ReplyId}={postId}" };
            if (this.SocialGroupId > 0)
            {
                alertParams.Add($"{Literals.GroupId}={this.SocialGroupId}");
            }

            if (this.Request.IsAuthenticated)
            {
                if (this._useListActions)
                {
                    sbOutput.Replace("[ACTIONS:ALERT]", "<li onclick=\"window.location.href='" + Utilities.NavigateURL(this.TabId, "", alertParams.ToArray()) + "';\" title=\"[RESX:Alert]\"><i class=\"fa fa-bell-o fa-fw fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Alert]</span></li>");
                }
                else
                {
                    sbOutput.Replace("[ACTIONS:ALERT]", "<a class=\"af-actions\" href=\"" + Utilities.NavigateURL(this.TabId, "", alertParams.ToArray()) + "\" title=\"[RESX:Alert]\"><i class=\"fa fa-bell-o fa-fw fa-blue\"></i><span class=\"dcf-link-text\">[RESX:Alert]</span></a>");
                }
            }
            else
            {
                sbOutput.Replace("[ACTIONS:ALERT]", string.Empty);
            }

            // Process Body
            if (string.IsNullOrEmpty(body))
            {
                body = " <br />";
            }

            var sBody = Utilities.ManageImagePath(body, HttpContext.Current.Request.Url);

            sBody = sBody.Replace("[", "&#91;");
            sBody = sBody.Replace("]", "&#93;");
            if (sBody.ToUpper().Contains("&#91;CODE&#93;"))
            {
                sBody = Regex.Replace(sBody, "(&#91;CODE&#93;)", "[CODE]", RegexOptions.IgnoreCase);
                sBody = Regex.Replace(sBody, "(&#91;\\/CODE&#93;)", "[/CODE]", RegexOptions.IgnoreCase);

            }

            // sBody = sBody.Replace("&lt;CODE&gt;", "<CODE>")
            if (Regex.IsMatch(sBody, "\\[CODE([^>]*)\\]", RegexOptions.IgnoreCase))
            {
                var objCode = new CodeParser();
                sBody = CodeParser.ParseCode(HttpUtility.HtmlDecode(sBody));
            }

            sBody = Utilities.StripExecCode(sBody);
            if (this.MainSettings.AutoLinkEnabled)
            {
                sBody = Utilities.AutoLinks(sBody, this.Request.Url.Host);
            }

            if (sBody.Contains("<%@"))
            {
                sBody = sBody.Replace("<%@ ", "&lt;&#37;@ ");
            }

            if (body.ToLowerInvariant().Contains("runat"))
            {
                sBody = Regex.Replace(sBody, "runat", "&#114;&#117;nat", RegexOptions.IgnoreCase);
            }

            sbOutput.Replace("[BODY]", sBody);

            // Subject
            sbOutput.Replace("[SUBJECT]", subject);

            // Attachments
            sbOutput.Replace("[ATTACHMENTS]", this.GetAttachments(contentId, true, this.PortalId, this.ForumModuleId));

            // Switch back from the string builder to a normal string before we perform the image/thumbnail replacements.

            sOutput = sbOutput.ToString();

            // Legacy attachment functionality, uses "attachid"
            // &#91;IMAGE:38&#93;
            if (sOutput.Contains("&#91;IMAGE:"))
            {
                var strHost = Common.Globals.AddHTTP(Common.Globals.GetDomainName(this.Request)) + "/";
                const string pattern = "(&#91;IMAGE:(.+?)&#93;)";
                sOutput = Regex.Replace(sOutput, pattern, match => "<img src=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + this.PortalId + "&moduleid=" + this.ForumModuleId + "&attachid=" + match.Groups[2].Value + "\" border=\"0\" class=\"afimg\" />");
            }

            // Legacy attachment functionality, uses "attachid"
            // &#91;THUMBNAIL:38&#93;
            if (sOutput.Contains("&#91;THUMBNAIL:"))
            {
                var strHost = Common.Globals.AddHTTP(Common.Globals.GetDomainName(this.Request)) + "/";
                const string pattern = "(&#91;THUMBNAIL:(.+?)&#93;)";
                sOutput = Regex.Replace(sOutput, pattern, match =>
                {
                    var thumbId = match.Groups[2].Value.Split(':')[0];
                    var parentId = match.Groups[2].Value.Split(':')[1];
                    return "<a href=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + this.PortalId + "&moduleid=" + this.ForumModuleId + "&attachid=" + parentId + "\" target=\"_blank\"><img src=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + this.PortalId + "&moduleid=" + this.ForumModuleId + "&attachid=" + thumbId + "\" border=\"0\" class=\"afimg\" /></a>";
                });
            }

            return sOutput;
        }

        // Renders the [ATTACHMENTS] block
        private string GetAttachments(int contentId, bool allowAttach, int portalId, int moduleId)
        {
            if (!allowAttach || this._dtAttach.Rows.Count == 0)
            {
                return string.Empty;
            }

            const string itemTemplate = "<li><a href='/DesktopModules/ActiveForums/viewer.aspx?portalid={0}&moduleid={1}&attachmentid={2}' target='_blank'><i class='af-fileicon af-fileicon-{3}'></i><span>{4}</span></a></li>";

            this._dtAttach.DefaultView.RowFilter = "ContentId = " + contentId;

            var attachmentRows = this._dtAttach.DefaultView.ToTable().Rows;

            if (attachmentRows.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(1024);

            sb.Append("<div class='af-attach-post-list'><span>");
            sb.Append(Utilities.GetSharedResource("[RESX:Attachments]"));
            sb.Append("</span><ul>");

            foreach (DataRow dr in this._dtAttach.DefaultView.ToTable().Rows)
            {
                // AttachId, ContentId, UserID, FileName, ContentType, FileSize, FileID

                var attachId = dr.GetInt("AttachId");
                var filename = dr.GetString("Filename").TextOrEmpty();

                var fileExtension = System.IO.Path.GetExtension(filename).TextOrEmpty().Replace(".", string.Empty);

                filename = HttpUtility.HtmlEncode(Regex.Replace(filename, @"^__\d+__\d+__", string.Empty));

                sb.AppendFormat(itemTemplate, portalId, moduleId, attachId, fileExtension, filename);
            }

            sb.Append("</ul></div>");

            return sb.ToString();
        }

        private void BuildPager()
        {
            var pager1 = this.FindControl("Pager1") as PagerNav;
            var pager2 = this.FindControl("Pager2") as PagerNav;

            if (pager1 == null && pager2 == null)
            {
                return;
            }

            var intPages = Convert.ToInt32(Math.Ceiling(this._rowCount / (double)this._pageSize));

            var @params = new List<string>();
            if (!string.IsNullOrWhiteSpace(this.Request.Params[ParamKeys.Sort]))
            {
                @params.Add(ParamKeys.Sort + "=" + this.Request.Params[ParamKeys.Sort]);
            }

            if (pager1 != null)
            {
                pager1.PageCount = intPages;
                pager1.CurrentPage = this.PageId;
                pager1.TabID = Utilities.SafeConvertInt(this.Request.Params["TabId"], -1);
                pager1.ForumID = this.ForumId;
                pager1.UseShortUrls = this.MainSettings.UseShortUrls;
                pager1.PageText = Utilities.GetSharedResource("[RESX:Page]");
                pager1.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
                pager1.View = Views.Topic;
                pager1.TopicId = this.TopicId;
                pager1.PageMode = PagerNav.Mode.Links;
                pager1.BaseURL = URL.ForumLink(this.TabId, this.ForumInfo) + this._topicURL;
                pager1.Params = @params.ToArray();
            }

            if (pager2 != null)
            {
                pager2.PageCount = intPages;
                pager2.CurrentPage = this.PageId;
                pager2.TabID = Utilities.SafeConvertInt(this.Request.Params["TabId"], -1);
                pager2.ForumID = this.ForumId;
                pager2.UseShortUrls = this.MainSettings.UseShortUrls;
                pager2.PageText = Utilities.GetSharedResource("[RESX:Page]");
                pager2.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
                pager2.View = Views.Topic;
                pager2.TopicId = this.TopicId;
                pager2.PageMode = PagerNav.Mode.Links;
                pager2.BaseURL = URL.ForumLink(this.TabId, this.ForumInfo) + this._topicURL;
                pager2.Params = @params.ToArray();
            }

        }

        #endregion
    }
}
