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

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.ActiveForums.Constants;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;

    [DefaultProperty("Text"), ToolboxData("<{0}:TopicView runat=server></{0}:TopicView>")]
    public class TopicView : ForumBase
    {
        private DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic;
        private int topicTemplateId;
        private DataRow drForum;
        private DataRow drSecurity;
        private DataTable dtTopic;
        private DataTable dtAttach;
        private bool bRead;
        private bool bTrust;
        private bool bEdit;
        private bool bSubscribe;
        private bool bModerate;
        private bool bSplit;
        private int rowIndex;
        private int pageSize = 20;
        private int rowCount;
        private bool isTrusted;
        private bool isSubscribedTopic;
        private string defaultSort;
        private string tags = string.Empty;
        private bool useListActions;

        #region Public Properties

        public string TopicTemplate { get; set; } = string.Empty;

        public int OptPageSize { get; set; }

        public string OptDefaultSort { get; set; }

        public string MetaTemplate { get; set; } = "[META][TITLE][FORUMTOPIC:SUBJECT] - [PORTAL:PORTALNAME] - [TAB:TITLE] - [FORUMGROUP:GROUPNAME] - [FORUM:FORUMNAME][/TITLE][DESCRIPTION][FORUMTOPIC:BODY:255][/DESCRIPTION][KEYWORDS][TAGS][VALUE][/KEYWORDS][/META]";

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
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId), false);
                        this.Context.ApplicationInstance.CompleteRequest();
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
                        var tURL = Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.ViewType + "=topic", ParamKeys.ContentJumpId + "=" + firstUnreadPost });
                        if (this.MainSettings.UseShortUrls)
                        {
                            tURL = Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.TopicId + "=" + this.TopicId, ParamKeys.ContentJumpId + "=" + firstUnreadPost });
                        }

                        this.Response.Redirect(tURL, false);
                        this.Context.ApplicationInstance.CompleteRequest();
                    }
                }

                this.AppRelativeVirtualPath = "~/";

                // Get our default sort
                // Try the OptDefaultSort Value First
                this.defaultSort = this.OptDefaultSort;
                if (string.IsNullOrWhiteSpace(this.defaultSort))
                {
                    // Next, try getting the sort from the query string
                    this.defaultSort = (this.Request.Params[ParamKeys.Sort] + string.Empty).Trim().ToUpperInvariant();
                    if (string.IsNullOrWhiteSpace(this.defaultSort) || (this.defaultSort != "ASC" && this.defaultSort != "DESC"))
                    {
                        // If we still don't have a valid sort, try and use the value from the users profile
                        this.defaultSort = (this.ForumUser.PrefDefaultSort + string.Empty).Trim().ToUpper();
                        if (string.IsNullOrWhiteSpace(this.defaultSort) || (this.defaultSort != "ASC" && this.defaultSort != "DESC"))
                        {
                            // No other option than to just use ASC
                            this.defaultSort = "ASC";
                        }
                    }
                }

                this.LoadData(this.PageId);

                this.BindTopic();
                var tempVar = this.BasePage;
                DotNetNuke.Modules.ActiveForums.Environment.UpdateMeta(ref tempVar, this.MetaTitle, this.MetaDescription, this.MetaKeywords);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                if (ex.InnerException != null)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }

                this.RenderMessage("[RESX:Error:LoadingTopic]", ex.Message, ex);
            }
        }

        #endregion

        #region Private Methods

        private void LoadData(int pageId)
        {
            // Get our page size
            this.pageSize = this.OptPageSize;
            if (this.pageSize <= 0)
            {
                this.pageSize = this.UserId > 0 ? this.UserDefaultPageSize : this.MainSettings.PageSize;
                if (this.pageSize < 5)
                {
                    this.pageSize = 10;
                }
            }

            // If we are in print mode, set the page size to maxValue
            if (!string.IsNullOrWhiteSpace(this.Request.QueryString["dnnprintmode"]))
            {
                this.pageSize = int.MaxValue;
            }

            // If our pagesize is maxvalue, we can only have one page
            if (this.pageSize == int.MaxValue)
            {
                pageId = 1;
            }

            // Get our Row Index
            this.rowIndex = (pageId - 1) * this.pageSize;
            string cacheKey = string.Format(CacheKeys.TopicViewForUser, this.ModuleId, this.TopicId, this.UserId, HttpContext.Current?.Response?.Cookies["language"]?.Value, this.rowIndex, this.pageSize);
            DataSet ds = (DataSet)DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheRetrieve(this.ForumModuleId, cacheKey);
            if (ds == null)
            {
                ds = DotNetNuke.Modules.ActiveForums.DataProvider.Instance().UI_TopicView(this.PortalId, this.ForumModuleId, this.ForumId, this.TopicId, this.UserId, this.rowIndex, this.pageSize, this.UserInfo.IsSuperUser, this.defaultSort);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.ModuleId, cacheKey, ds);
            }

            // Test for a proper dataset
            if (ds.Tables.Count < 4 || ds.Tables[0].Rows.Count == 0 || ds.Tables[1].Rows.Count == 0)
            {
                this.Response.Redirect(Utilities.NavigateURL(this.TabId), false);
                this.Context.ApplicationInstance.CompleteRequest();
            }

            // Load our values
            this.drForum = ds.Tables[0].Rows[0];
            this.drSecurity = ds.Tables[1].Rows[0];
            this.dtTopic = ds.Tables[2];
            this.dtAttach = ds.Tables[3];

            // If we don't have any rows to display, redirect
            if (this.dtTopic.Rows.Count == 0)
            {
                if (pageId > 1)
                {
                    if (this.MainSettings.UseShortUrls)
                    {
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.TopicId + "=" + this.TopicId }), false);
                        this.Context.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + this.TopicId }), false);
                        this.Context.ApplicationInstance.CompleteRequest();
                    }
                }
                else
                {
                    if (this.MainSettings.UseShortUrls)
                    {
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ForumId + "=" + this.ForumId }), false);
                        this.Context.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topics }), false);
                        this.Context.ApplicationInstance.CompleteRequest();
                    }
                }
            }

            // first make sure we have read permissions, otherwise we need to redirect
            this.bRead = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.drSecurity["CanRead"].ToString()), this.ForumUser.UserRoleIds);

            if (!this.bRead)
            {
                if (this.PortalSettings.LoginTabId > 0)
                {
                    this.Response.Redirect(Utilities.NavigateURL(this.PortalSettings.LoginTabId, string.Empty, "returnUrl=" + this.Request.RawUrl), false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    this.Response.Redirect(Utilities.NavigateURL(this.TabId, string.Empty, "ctl=login&returnUrl=" + this.Request.RawUrl), false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }
            }

            this.bEdit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.drSecurity["CanEdit"].ToString()), this.ForumUser.UserRoleIds);
            this.bSubscribe = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.drSecurity["CanSubscribe"].ToString()), this.ForumUser.UserRoleIds);
            this.bModerate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.drSecurity["CanModerate"].ToString()), this.ForumUser.UserRoleIds);
            this.bSplit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.drSecurity["CanSplit"].ToString()), this.ForumUser.UserRoleIds);
            this.bTrust = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(this.drSecurity["CanTrust"].ToString()), this.ForumUser.UserRoleIds);

            this.isTrusted = Utilities.IsTrusted((int)this.ForumInfo.FeatureSettings.DefaultTrustValue, this.ForumUser.TrustLevel, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.ForumInfo.Security.TrustRoleIds, this.ForumUser.UserRoleIds));

            // TODO: Eventually this will use DAL2 to load from stored procedure into object model, but for now populate topic object model from stored procedure results
            this.topic = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo
            {
                ModuleId = this.ForumModuleId,
                PortalId = this.PortalId,
                TopicId = this.TopicId,
                IsPinned = Utilities.SafeConvertBool(this.drForum["IsPinned"]),
                IsLocked = Utilities.SafeConvertBool(this.drForum["IsLocked"]),
                IsApproved = Utilities.SafeConvertBool(this.drForum["IsApproved"]),
                IsDeleted = Utilities.SafeConvertBool(this.drForum["IsDeleted"]),
                IsRejected = Utilities.SafeConvertBool(this.drForum["IsRejected"]),
                IsArchived = Utilities.SafeConvertBool(this.drForum["IsArchived"]),
                IsAnnounce = Utilities.SafeConvertBool(this.drForum["IsAnnounce"]),
                AnnounceStart = Utilities.SafeConvertDateTime(this.drForum["AnnounceStart"]),
                AnnounceEnd = Utilities.SafeConvertDateTime(this.drForum["AnnounceEnd"]),
                ViewCount = Utilities.SafeConvertInt(this.drForum["ViewCount"]),
                ReplyCount = Utilities.SafeConvertInt(this.drForum["ReplyCount"]),
                TopicType = Utilities.SafeConvertInt(this.drForum["TopicType"]) < 1
                ? TopicTypes.Topic
                : TopicTypes.Poll,
                StatusId = Utilities.SafeConvertInt(this.drForum["StatusId"]),
                Rating = Utilities.SafeConvertInt(this.drForum["TopicRating"]),
                TopicIcon = this.drForum["TopicIcon"].ToString(),
                Priority = Convert.ToInt32(this.drForum["Priority"]),
                TopicUrl = this.drForum["URL"].ToString(),
                TopicData = this.drForum["TopicData"].ToString(),
                NextTopic = Utilities.SafeConvertInt(this.drForum["NextTopic"]),
                PrevTopic = Utilities.SafeConvertInt(this.drForum["PrevTopic"]),
                Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                {
                    ModuleId = this.ForumModuleId,
                    Subject = System.Net.WebUtility.HtmlDecode(this.drForum["Subject"].ToString()),
                    Summary = System.Net.WebUtility.HtmlDecode(this.drForum["Summary"].ToString()),
                    Body = System.Net.WebUtility.HtmlDecode(this.drForum["Body"].ToString()),
                    AuthorId = Utilities.SafeConvertInt(this.drForum["AuthorId"]),
                    AuthorName = this.drForum["TopicAuthor"].ToString(),
                    DateCreated = Utilities.SafeConvertDateTime(this.drForum["DateCreated"]),
                    DateUpdated = Utilities.SafeConvertDateTime(this.drForum["DateUpdated"]),
                },
                LastReply = new DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo
                {
                    TopicId = this.TopicId,
                    Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                    {
                        DateCreated = Utilities.SafeConvertDateTime(this.drForum["LastPostDate"]),
                        AuthorId = Utilities.SafeConvertInt(this.drForum["LastPostAuthorId"]),
                    },
                },
            };

            this.topic.Author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(this.PortalId, this.ForumModuleId, this.topic.Content.AuthorId);
            this.topic.Author.ForumUser.UserInfo.DisplayName = this.drForum["DisplayName"].ToString();
            this.topic.Author.ForumUser.UserInfo.FirstName = this.drForum["FirstName"].ToString();
            this.topic.Author.ForumUser.UserInfo.LastName = this.drForum["LastName"].ToString();
            this.topic.Author.ForumUser.UserInfo.Username = this.drForum["UserName"].ToString();
            this.topic.Author.ForumUser.PortalId = this.PortalId;
            this.topic.Author.ForumUser.ModuleId = this.ForumModuleId;

            this.topic.Forum = this.ForumInfo;


            this.topic.LastReply.Author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(this.PortalId, this.ForumModuleId, this.topic.LastReply.Content.AuthorId);
            this.topic.LastReply.Author.ForumUser.UserInfo.DisplayName = this.drForum["LastPostDisplayName"].ToString();
            this.topic.LastReply.Author.ForumUser.UserInfo.FirstName = this.drForum["LastPostFirstName"].ToString();
            this.topic.LastReply.Author.ForumUser.UserInfo.LastName = this.drForum["LastPostLastName"].ToString();
            this.topic.LastReply.Author.ForumUser.UserInfo.Username = this.drForum["LastPostUserName"].ToString();
            this.topic.LastReply.Author.ForumUser.PortalId = this.PortalId;
            this.topic.LastReply.Author.ForumUser.ModuleId = this.ForumModuleId;

            this.topicTemplateId = Utilities.SafeConvertInt(this.drForum["TopicTemplateId"]);
            this.tags = this.drForum["Tags"].ToString();
            this.rowCount = Utilities.SafeConvertInt(this.drForum["ReplyCount"]) + 1;
            this.isSubscribedTopic = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(this.PortalId, this.ForumModuleId, this.UserId, this.ForumInfo.ForumID, this.TopicId);

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

            if (this.MainSettings.URLRewriteEnabled && !string.IsNullOrEmpty(this.topic.TopicUrl))
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
                var @params = new List<string>
                {
                    $"{ParamKeys.ForumId}={this.ForumId}",
                    $"{ParamKeys.TopicId}={this.TopicId}",
                    $"{ParamKeys.ViewType}={Views.Topic}",
                };
                if (this.MainSettings.UseShortUrls)
                {
                    @params = new List<string> { ParamKeys.TopicId + "=" + this.TopicId };
                }

                var intPages = Convert.ToInt32(Math.Ceiling(this.rowCount / (double)this.pageSize));
                if (intPages > 1)
                {
                    @params.Add($"{ParamKeys.PageJumpId}={intPages}");
                }

                if (this.SocialGroupId > 0)
                {
                    @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
                }

                sURL = Utilities.NavigateURL(this.TabId, string.Empty, @params.ToArray()) + sTarget;
            }

            if (this.Request.IsAuthenticated)
            {
                this.Response.Redirect(sURL, false);
                this.Context.ApplicationInstance.CompleteRequest();
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
                sOutput = TemplateCache.GetCachedTemplate(this.ForumModuleId, "TopicView", this.topicTemplateId);
            }

            sOutput = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemoveObsoleteTokens(new StringBuilder(sOutput)).ToString();
            sOutput = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyForumTokenSynonyms(new StringBuilder(sOutput), this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();

            // Add Topic Scripts
            var ctlTopicScripts = (af_topicscripts)this.LoadControl("~/DesktopModules/ActiveForums/controls/af_topicscripts.ascx");
            ctlTopicScripts.ModuleConfiguration = this.ModuleConfiguration;
            this.Controls.Add(ctlTopicScripts);

            #region "Build Topic Properties"

            if (sOutput.Contains("[AF:PROPERTIES"))
            {
                var sProps = string.Empty;

                if (!string.IsNullOrWhiteSpace(this.topic.TopicData))
                {
                    var sPropTemplate = TemplateUtils.GetTemplateSection(sOutput, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");

                    try
                    {
                        var pl = DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Deserialize(this.topic.TopicData);
                        foreach (var p in pl)
                        {
                            var pName = System.Net.WebUtility.HtmlDecode(p.Name);
                            var pValue = System.Net.WebUtility.HtmlDecode(p.Value);

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

            #endregion "Build Topic Properties"

            #region "Populate Metadata"

            // If the template contains a meta template, grab it then remove the token
            if (sOutput.Contains("[META]"))
            {
                this.MetaTemplate = TemplateUtils.GetTemplateSection(sOutput, "[META]", "[/META]");
                sOutput = TemplateUtils.ReplaceSubSection(sOutput, string.Empty, "[META]", "[/META]");
            }

            // Parse Meta Template
            if (!string.IsNullOrEmpty(this.MetaTemplate))
            {
                this.MetaTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemoveObsoleteTokens(new StringBuilder(this.MetaTemplate)).ToString();
                this.MetaTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyForumTokenSynonyms(new StringBuilder(this.MetaTemplate), this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
                this.MetaTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyTopicTokenSynonyms(new StringBuilder(this.MetaTemplate), this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
                this.MetaTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTopicTokens(new StringBuilder(this.MetaTemplate), this.topic, this.PortalSettings, this.MainSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.Request.Url, this.Request.RawUrl).ToString();
                this.MetaTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceForumTokens(new StringBuilder(this.MetaTemplate), this.ForumInfo, this.PortalSettings, this.MainSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.TabId, this.ForumUser.CurrentUserType, this.Request.Url, this.Request.RawUrl).ToString();
                this.MetaTemplate = this.MetaTemplate.Replace("[TAGS]", this.tags);

                this.MetaTitle = TemplateUtils.GetTemplateSection(this.MetaTemplate, "[TITLE]", "[/TITLE]").Replace("[TITLE]", string.Empty).Replace("[/TITLE]", string.Empty);
                this.MetaTitle = this.MetaTitle.TruncateAtWord(SEOConstants.MaxMetaTitleLength);
                this.MetaDescription = TemplateUtils.GetTemplateSection(this.MetaTemplate, "[DESCRIPTION]", "[/DESCRIPTION]").Replace("[DESCRIPTION]", string.Empty).Replace("[/DESCRIPTION]", string.Empty);
                this.MetaDescription = this.MetaDescription.TruncateAtWord(SEOConstants.MaxMetaDescriptionLength);
                this.MetaKeywords = TemplateUtils.GetTemplateSection(this.MetaTemplate, "[KEYWORDS]", "[/KEYWORDS]").Replace("[KEYWORDS]", string.Empty).Replace("[/KEYWORDS]", string.Empty);
            }

            #endregion "Populate Metadata"

            #region "Setup Breadcrumbs"

            var breadCrumb = TemplateUtils.GetTemplateSection(sOutput, "[BREADCRUMB]", "[/BREADCRUMB]").Replace("[BREADCRUMB]", string.Empty).Replace("[/BREADCRUMB]", string.Empty);

            if (this.MainSettings.UseSkinBreadCrumb)
            {
                var ctlUtils = new ControlUtils();

                var groupUrl = ctlUtils.BuildUrl(this.PortalId, this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, string.Empty, this.ForumInfo.ForumGroupId, -1, -1, -1, string.Empty, 1, -1, this.SocialGroupId);
                var forumUrl = ctlUtils.BuildUrl(this.PortalId, this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumInfo.ForumGroupId, this.ForumInfo.ForumID, -1, -1, string.Empty, 1, -1, this.SocialGroupId);
                var topicUrl = ctlUtils.BuildUrl(this.PortalId, this.TabId, this.ForumModuleId, this.ForumInfo.ForumGroup.PrefixURL, this.ForumInfo.PrefixURL, this.ForumInfo.ForumGroupId, this.ForumInfo.ForumID, this.TopicId, this.topic.TopicUrl, -1, -1, string.Empty, 1, -1, this.SocialGroupId);

                var sCrumb = "<a href=\"" + groupUrl + "\">" + this.ForumInfo.GroupName + "</a>|";
                sCrumb += "<a href=\"" + forumUrl + "\">" + this.ForumInfo.ForumName + "</a>";
                sCrumb += "|<a href=\"" + topicUrl + "\">" + this.topic.Subject + "</a>";

                if (DotNetNuke.Modules.ActiveForums.Environment.UpdateBreadCrumb(this.Page.Controls, sCrumb))
                {
                    breadCrumb = string.Empty;
                }
            }

            sOutput = TemplateUtils.ReplaceSubSection(sOutput, breadCrumb, "[BREADCRUMB]", "[/BREADCRUMB]");

            #endregion "Setup Breadcrumbs"

            // Parse Common Controls First
            sOutput = this.ParseControls(sOutput);

            // Note: If the containing element is not found, GetTemplateSection returns the entire template
            // This is desired behavior in this case as it's possible that the entire template is our topics container.
            var topicControl = TemplateUtils.GetTemplateSection(sOutput, "[AF:CONTROL:CALLBACK]", "[/AF:CONTROL:CALLBACK]");

            topicControl = this.ParseTopic(topicControl);

            if (!topicControl.Contains(Globals.ForumsControlsRegisterAMTag))
            {
                topicControl = Globals.ForumsControlsRegisterAMTag + topicControl;
            }

            topicControl = Utilities.LocalizeControl(topicControl);

            // If a template was passed in, we don't need to do this.
            if (bFullTopic)
            {
                sOutput = TemplateUtils.ReplaceSubSection(sOutput, "<asp:placeholder id=\"plhTopic\" runat=\"server\" />", "[AF:CONTROL:CALLBACK]", "[/AF:CONTROL:CALLBACK]");
                sOutput = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyTopicTokenSynonyms(new StringBuilder(sOutput), this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
                sOutput = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTopicTokens(new StringBuilder(sOutput), this.topic, this.PortalSettings, this.MainSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.Request.Url, this.Request.RawUrl).ToString();

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
            if (topicControl.Contains("<dnn:BANNER") && !topicControl.Contains(Globals.BannerRegisterTag))
            {
                topicControl = Globals.BannerRegisterTag + topicControl;
            }

            var ctlTopic = this.ParseControl(topicControl);
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
                    ForumInfo = this.ForumId > 0 ? this.ForumInfo : null,
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
                    ForumId = this.ForumId,
                });
            }

            // Quick Reply -- note: always create quick reply control since topic can locked and unlocked from javascript
            if (this.CanReply)
            {
                plh = this.FindControl("plhQuickReply") as PlaceHolder;
                if (plh != null)
                {
                    plh.Controls.Clear();
                    var ctlQuickReply = (af_quickreplyform)this.LoadControl("~/DesktopModules/ActiveForums/controls/af_quickreply.ascx");
                    ctlQuickReply.ModuleConfiguration = this.ModuleConfiguration;
                    ctlQuickReply.CanTrust = this.bTrust;
                    ctlQuickReply.ModApprove = this.bModerate;
                    ctlQuickReply.IsTrusted = this.isTrusted;
                    ctlQuickReply.Subject = Utilities.GetSharedResource("[RESX:SubjectPrefix]") + " " + this.topic.Subject;
                    ctlQuickReply.AllowSubscribe = this.Request.IsAuthenticated && this.bSubscribe;
                    ctlQuickReply.AllowHTML = this.topic.Forum.FeatureSettings.AllowHTML;
                    ctlQuickReply.AllowScripts = this.topic.Forum.FeatureSettings.AllowScript;
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
                ctlTopicSort.DefaultSort = this.defaultSort;
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
                ctlTopicStatus.Status = this.topic.StatusId;
                ctlTopicStatus.ForumId = this.ForumId;
                if (this.ForumId > 0)
                {
                    ctlTopicStatus.ForumInfo = this.ForumInfo;
                }

                plh.Controls.Add(ctlTopicStatus);
            }

            this.BuildPager();
        }

        private string ParseControls(string sOutput)
        {
            // Banners
            if (sOutput.Contains("[BANNER"))
            {
                sOutput = sOutput.Replace("[BANNER]", "<dnn:BANNER runat=\"server\" GroupName=\"FORUMS\" BannerCount=\"1\" EnableViewState=\"False\" />");

                const string pattern = @"(\[BANNER:(.+?)\])";
                const string sBanner = "<dnn:BANNER runat=\"server\" BannerCount=\"1\" GroupName=\"$1\" EnableViewState=\"False\" />";

                sOutput = RegexUtils.GetCachedRegex(pattern).Replace(sOutput, sBanner);
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

            var sbOutput = new StringBuilder(sOutput);

            if (this.Request.QueryString["dnnprintmode"] != null)
            {
                sbOutput = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemoveControlTokensForDnnPrintMode(sbOutput);
                sbOutput.Append("<img src=\"" + this.Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath + "spacer.gif") + "\" width=\"800\" height=\"1\" runat=\"server\" alt=\"---\" />");
            }

            sbOutput.Replace("[NOPAGING]", "<script type=\"text/javascript\">afpagesize=" + int.MaxValue + ";</script>");
            sbOutput.Replace("[NOTOOLBAR]", string.Empty);

            // Subscribe Option
            if (this.bSubscribe)
            {
                var subControl = new ToggleSubscribe(this.ForumModuleId, this.ForumId, this.TopicId, 1);
                subControl.Checked = this.isSubscribedTopic;
                subControl.Text = "[RESX:Subscribe]";
                sbOutput.Replace("[TOPICSUBSCRIBE]", subControl.Render());
            }
            else
            {
                sbOutput.Replace("[TOPICSUBSCRIBE]", string.Empty);
            }

// TODO: remove in v10.00.00
#region "TODO: Backward compatibility -- remove in v10.00.00"

            // Topic and post actions
            // for backward compatibility, this needs to map to post actions, not topic actions
            if (sbOutput.ToString().Contains("[AF:CONTROL:TOPICACTIONS]"))
            {
                DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTokenSynonym(
                    sbOutput,
                    "[AF:CONTROL:TOPICACTIONS]",
                    "[DCF:TEMPLATE-POSTACTIONS]");
            }

            if (sbOutput.ToString().Contains("[AF:CONTROL:POSTACTIONS]"))
            {
                DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTokenSynonym(
                    sbOutput,
                    "[AF:CONTROL:POSTACTIONS]",
                    "[DCF:TEMPLATE-POSTACTIONS]");
            }

            if (sbOutput.ToString().Contains("[DCF:TEMPLATE-POSTACTIONS]"))
            {
                this.useListActions = true;
                sbOutput.Replace("[DCF:TEMPLATE-POSTACTIONS]", TemplateCache.GetCachedTemplate(this.ForumModuleId, "PostActions"));
            }

            sbOutput = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyPostActionTokenSynonyms(sbOutput, this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale, this.useListActions);
#endregion "Backward compatibility -- remove in v10.00.00"

            // Quick Reply
            if (this.CanReply)
            {
                var @params = new List<string>
                {
                    $"{ParamKeys.ViewType}={Views.Post}",
                    $"{ParamKeys.TopicId}={this.TopicId}",
                    $"{ParamKeys.ForumId}={this.ForumId}",
                };
                if (this.SocialGroupId > 0)
                {
                    @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
                }

                if (this.topic.IsLocked)
                {
                    sbOutput.Replace("[ADDREPLY]", "<span class=\"dcf-topic-lock-locked-label-" + this.TopicId.ToString() + "\" class=\"afnormal\">[RESX:TopicLocked]</span><a href=\"" + Utilities.NavigateURL(this.TabId, string.Empty, @params.ToArray()) + "\" class=\"dnnPrimaryAction dcf-topic-reply-link dcf-topic-reply-locked\">[RESX:AddReply]</a>");
                    sbOutput.Replace("[QUICKREPLY]", "<div class=\"dcf-quickreply-wrapper\" style=\"display:none;\"><asp:placeholder id=\"plhQuickReply\" runat=\"server\" /></div>");
                }
                else
                {
                    sbOutput.Replace("[ADDREPLY]", "<span class=\"dcf-topic-lock-locked-label-" + this.TopicId.ToString() + "\" class=\"afnormal\"></span><a href=\"" + Utilities.NavigateURL(this.TabId, string.Empty, @params.ToArray()) + "\" class=\"dnnPrimaryAction dcf-topic-reply-link dcf-topic-reply-unlocked\">[RESX:AddReply]</a>");
                    sbOutput.Replace("[QUICKREPLY]", "<div class=\"dcf-quickreply-wrapper\" style=\"display:block;\"><asp:placeholder id=\"plhQuickReply\" runat=\"server\" /></div>");
                }
            }
            else
            {
                if (!this.Request.IsAuthenticated)
                {
                    sbOutput.Replace("[ADDREPLY]", $"{string.Format(Utilities.GetSharedResource("[RESX:NotAuthorizedReplyPleaseLogin]"), "[DCF:LOGINLINK]", "[DCF:LOGINPOPUPLINK]")}");
                }
                else
                {
                    sbOutput.Replace("[ADDREPLY]", "[RESX:NotAuthorizedReply]");
                }

                sbOutput.Replace("[QUICKREPLY]", string.Empty);
            }

            if (sOutput.Contains("[SPLITBUTTONS]") && (this.bSplit && (this.bModerate || (this.topic.Author.AuthorId == this.UserId))) && (this.topic.ReplyCount > 0))
            {
                sbOutput.Replace("[SPLITBUTTONS]", TemplateCache.GetCachedTemplate(this.ForumModuleId, "TopicSplitButtons"));
                sbOutput.Replace("[TOPICID]", this.TopicId.ToString());
            }
            else
            {
                sbOutput.Replace("[SPLITBUTTONS]", string.Empty);
            }

            // Printer Friendly Link
            var sURL = "<a rel=\"nofollow\" href=\"" + Utilities.NavigateURL(this.TabId, string.Empty, ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + this.TopicId, "mid=" + this.ModuleId, "dnnprintmode=true") + "?skinsrc=" + System.Net.WebUtility.UrlEncode("[G]" + SkinController.RootSkin + "/" + Common.Globals.glbHostSkinFolder + "/" + "No Skin") + "&amp;containersrc=" + System.Net.WebUtility.UrlEncode("[G]" + SkinController.RootContainer + "/" + Common.Globals.glbHostSkinFolder + "/" + "No Container") + "\" target=\"_blank\">";

            sURL += "<i class=\"fa fa-print fa-fw fa-blue\"></i></a>";
            sbOutput.Replace("[AF:CONTROL:PRINTER]", sURL);

            // Email Link
            if (this.Request.IsAuthenticated)
            {
                sURL = Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ViewType + "=sendto", ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId });
                sbOutput.Replace("[AF:CONTROL:EMAIL]", "<a href=\"" + sURL + "\" rel=\"nofollow\"><i class=\"fa fa-envelope-o fa-fw fa-blue\"></i></a>");
            }
            else
            {
                sbOutput.Replace("[AF:CONTROL:EMAIL]", string.Empty);
            }

            // Pagers
            if (this.pageSize == int.MaxValue)
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
                var rateControl = new Ratings(this.ModuleId, this.ForumId, this.TopicId, true, this.topic.Rating);
                sbOutput.Replace("[POSTRATINGBUTTON]", rateControl.Render());
            }

            // Jump To
            sbOutput.Replace("[JUMPTO]", "<asp:placeholder id=\"plhQuickJump\" runat=\"server\" />");

            // Topic Status
            if (((this.bRead && this.topic.Content.AuthorId == this.UserId) || (this.bModerate && this.bEdit)) & this.topic.StatusId >= 0)
            {
                sbOutput.Replace("[AF:CONTROL:STATUS]", "<asp:placeholder id=\"plhStatus\" runat=\"server\" />");

                sbOutput.Replace("[AF:CONTROL:STATUSICON]", "<div><i class=\"fa fa-status" + this.topic.StatusId.ToString() + " fa-red fa-2x\"></i></div>");
            }
            else if (this.topic.StatusId >= 0)
            {
                sbOutput.Replace("[AF:CONTROL:STATUS]", string.Empty);

                sbOutput.Replace("[AF:CONTROL:STATUSICON]", "<div><i class=\"fa fa-status" + this.topic.StatusId.ToString() + " fa-red fa-2x\"></i></div>");
            }
            else
            {
                sbOutput.Replace("[AF:CONTROL:STATUS]", string.Empty);
                sbOutput.Replace("[AF:CONTROL:STATUSICON]", string.Empty);
            }

            // Poll
            if (this.topic.TopicType == TopicTypes.Poll)
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

                foreach (Match match in RegexUtils.GetCachedRegex(pattern, RegexOptions.Compiled & RegexOptions.IgnoreCase, 2).Matches(sOutput))
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

            // Process topic and reply templates.
            var sTopicTemplate = TemplateUtils.GetTemplateSection(sOutput, "[TOPIC]", "[/TOPIC]");
            var sReplyTemplate = TemplateUtils.GetTemplateSection(sOutput, "[REPLIES]", "[/REPLIES]");

#region "Backward compatilbility -- remove in v10.00.00"
            sTopicTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyUserTokenSynonyms(new StringBuilder(sTopicTemplate), this.PortalSettings, this.MainSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
            sTopicTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyAuthorTokenSynonyms(new StringBuilder(sTopicTemplate), this.PortalSettings, this.MainSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
            sTopicTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyTopicTokenSynonyms(new StringBuilder(sTopicTemplate), this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
            sTopicTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyPostTokenSynonyms(new StringBuilder(sTopicTemplate), this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
            sReplyTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyPostTokenSynonyms(new StringBuilder(sReplyTemplate), this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();

#endregion "Backward compatilbility -- remove in v10.00.00"

            var sTemp = string.Empty;
            var i = 0;

            if (this.dtTopic.Rows.Count > 0)
            {
                bool allReplies = true;
                foreach (DataRow dr in this.dtTopic.Rows)
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
                        allReplies = false;
                        sTopicTemplate = this.ParseContent(dr, sTopicTemplate, i);
                    }
                    else
                    {
                        sTemp += this.ParseContent(dr, sReplyTemplate, i);
                    }

                    i++;
                }

                if (this.defaultSort == "ASC")
                {
                    sOutput = TemplateUtils.ReplaceSubSection(sOutput, sTemp, "[REPLIES]", "[/REPLIES]");
                    sOutput = TemplateUtils.ReplaceSubSection(sOutput, allReplies ? string.Empty : sTopicTemplate, "[TOPIC]", "[/TOPIC]");
                }
                else
                {
                    sOutput = TemplateUtils.ReplaceSubSection(sOutput, sTemp + (allReplies ? string.Empty : sTopicTemplate), "[REPLIES]", "[/REPLIES]");
                    sOutput = TemplateUtils.ReplaceSubSection(sOutput, string.Empty, "[TOPIC]", "[/TOPIC]");
                }

                if (sTopicTemplate.Contains("[BODY]"))
                {
                    sOutput = sOutput.Replace(sTopicTemplate, string.Empty);
                }
            }

            return sOutput;
        }

        private string ParseContent(DataRow dr, string template, int rowcount)
        {
            var sOutput = template;

            bool isReply = !dr.GetInt("ReplyId").Equals(0);

            // most topic values come in first result set and are set in LoadData(); however some, like IP Address and contentId, comes in this result set.
            if (!isReply)
            {
                this.topic.Content.IPAddress = dr.GetString("IPAddress");
                this.topic.ContentId = dr.GetInt("ContentId");
                this.topic.Content.ContentId = dr.GetInt("ContentId");
            }

            // Replace Tags Control
            var tags = dr.GetString("Tags");
            if (string.IsNullOrWhiteSpace(tags))
            {
                sOutput = TemplateUtils.ReplaceSubSection(sOutput, string.Empty, "[AF:CONTROL:TAGS]", "[/AF:CONTROL:TAGS]");
            }
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

                    tagList += "<a href=\"" + Utilities.NavigateURL(this.TabId, string.Empty, new[] { ParamKeys.ViewType + "=search", ParamKeys.Tags + "=" + System.Net.WebUtility.UrlEncode(tag) }) + "\">" + tag + "</a>";
                }

                sOutput = sOutput.Replace("[AF:LABEL:TAGS]", tagList);
            }

            var sbOutput = new StringBuilder(sOutput);

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

            // Poll Results
            if (sOutput.Contains("[POLLRESULTS]"))
            {
                if (this.topic.TopicType == TopicTypes.Poll)
                {
                    var polls = new Polls();
                    sbOutput.Replace("[POLLRESULTS]", polls.PollResults(this.topic.TopicId, this.ImagePath));
                }
                else
                {
                    sbOutput.Replace("[POLLRESULTS]", string.Empty);
                }
            }

            if (isReply)
            {
                var reply = new DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo
                {
                    ModuleId = this.ForumModuleId,
                    PortalId = this.PortalId,
                    ReplyId = dr.GetInt("ReplyId"),
                    TopicId = dr.GetInt("TopicId"),
                    StatusId = dr.GetInt("StatusId"),
                    IsApproved = dr.GetBoolean("IsApproved"),
                    IsDeleted = false,
                    Topic = this.topic,
                    ContentId = dr.GetInt("ContentId"),
                    Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                    {
                        ModuleId = this.ForumModuleId,
                        ContentId = dr.GetInt("ContentId"),
                        Body = System.Net.WebUtility.HtmlDecode(dr.GetString("Body")),
                        Subject = System.Net.WebUtility.HtmlDecode(dr.GetString("Subject")),
                        AuthorId = dr.GetInt("AuthorId"),
                        DateCreated = dr.GetDateTime("DateCreated"),
                        DateUpdated = dr.GetDateTime("DateUpdated"),
                        IPAddress = dr.GetString("IPAddress"),
                    },
                    Author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(this.PortalId, this.ForumModuleId,  dr.GetInt("AuthorId")),
                };
                sbOutput.Replace("[ATTACHMENTS]", this.GetAttachments(reply.ContentId, true, this.PortalId, this.ForumModuleId));
                sbOutput.Replace("[SPLITCHECKBOX]", "<div class=\"dcf-split-checkbox\" style=\"display:none;\"><input id=\"dcf-split-checkbox-" + reply.ReplyId + "\" type=\"checkbox\" onChange=\"amaf_splitCheck(this);\" value=\"" + reply.ReplyId + "\" /><label for=\"dcf-split-checkbox-" + reply.ReplyId + "\" class=\"dcf-split-checkbox-label\">[RESX:SplitCreate]</label></div>");
                sbOutput = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(sbOutput, reply, this.PortalSettings, this.MainSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.Request.Url, this.Request.RawUrl);
            }
            else
            {
                sbOutput.Replace("[ATTACHMENTS]", this.GetAttachments(this.topic.ContentId, true, this.PortalId, this.ForumModuleId));
                sbOutput.Replace("[SPLITCHECKBOX]", string.Empty);
                sbOutput = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(sbOutput, this.topic, this.PortalSettings, this.MainSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.Request.Url, this.Request.RawUrl);
            }

            sOutput = sbOutput.ToString();

            // Legacy attachment functionality, uses "attachid"
            // &#91;IMAGE:38&#93;
            if (sOutput.Contains("&#91;IMAGE:"))
            {
                var strHost = Common.Globals.AddHTTP(Common.Globals.GetDomainName(this.Request)) + "/";
                const string pattern = "(&#91;IMAGE:(.+?)&#93;)";
                sOutput = RegexUtils.GetCachedRegex(pattern).Replace(sOutput, match => "<img src=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + this.PortalId + "&moduleid=" + this.ForumModuleId + "&attachid=" + match.Groups[2].Value + "\" border=\"0\" class=\"afimg\" />");
            }

            // Legacy attachment functionality, uses "attachid"
            // &#91;THUMBNAIL:38&#93;
            if (sOutput.Contains("&#91;THUMBNAIL:"))
            {
                var strHost = Common.Globals.AddHTTP(Common.Globals.GetDomainName(this.Request)) + "/";
                const string pattern = "(&#91;THUMBNAIL:(.+?)&#93;)";
                sOutput = RegexUtils.GetCachedRegex(pattern).Replace(sOutput, match =>
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
            if (!allowAttach || this.dtAttach.Rows.Count == 0)
            {
                return string.Empty;
            }

            const string itemTemplate = "<li><a href='/DesktopModules/ActiveForums/viewer.aspx?portalid={0}&moduleid={1}&attachmentid={2}' target='_blank'><i class='af-fileicon af-fileicon-{3}'></i><span>{4}</span></a></li>";

            this.dtAttach.DefaultView.RowFilter = "ContentId = " + contentId;

            var attachmentRows = this.dtAttach.DefaultView.ToTable().Rows;

            if (attachmentRows.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(1024);

            sb.Append("<div class='af-attach-post-list'><span>");
            sb.Append(Utilities.GetSharedResource("[RESX:Attachments]"));
            sb.Append("</span><ul>");

            foreach (DataRow dr in this.dtAttach.DefaultView.ToTable().Rows)
            {
                // AttachId, ContentId, UserID, FileName, ContentType, FileSize, FileID
                var attachId = dr.GetInt("AttachId");
                var filename = dr.GetString("Filename").TextOrEmpty();

                var fileExtension = System.IO.Path.GetExtension(filename).TextOrEmpty().Replace(".", string.Empty);

                filename = System.Net.WebUtility.HtmlEncode(DotNetNuke.Common.Utilities.RegexUtils.GetCachedRegex(@"^__\d+__\d+__", RegexOptions.Compiled & RegexOptions.IgnoreCase, 2).Replace(filename, string.Empty));
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

            var intPages = Convert.ToInt32(Math.Ceiling(this.rowCount / (double)this.pageSize));

            var @params = new List<string>();
            if (!string.IsNullOrWhiteSpace(this.Request.Params[ParamKeys.Sort]))
            {
                @params.Add($"{ParamKeys.Sort}={this.Request.Params[ParamKeys.Sort]}");
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
                pager1.TopicId = this.topic.TopicId;
                pager1.PageMode = PagerNav.Mode.Links;
                pager1.BaseURL = URL.ForumLink(this.TabId, this.ForumInfo) + this.topic.TopicUrl;
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
                pager2.TopicId = this.topic.TopicId;
                pager2.PageMode = PagerNav.Mode.Links;
                pager2.BaseURL = URL.ForumLink(this.TabId, this.ForumInfo) + this.topic.TopicUrl;
                pager2.Params = @params.ToArray();
            }
        }

        #endregion
    }
}
