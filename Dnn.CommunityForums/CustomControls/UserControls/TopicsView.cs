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
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.ActiveForums.Constants;
    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Services.Authentication;

    [DefaultProperty("Text"), ToolboxData("<{0}:TopicsView runat=server></{0}:TopicsView>")]
    public class TopicsView : ForumBase
    {
        private DataRow drForum;
        private DataRow drSecurity;
        private DataTable dtTopics;
        private DataTable dtAnnounce;
        private DataTable dtSubForums;
        private bool bView;
        private bool bSubscribe;
        private int rowIndex = 1;
        private int pageSize = 20;
        private int topicRowCount;
        private bool isSubscribedForum;
        private bool useListActions;

        public string MetaTemplate { get; set; } = "[META][TITLE][PORTAL:PORTALNAME] - [TAB:TITLE] - [FORUMGROUP:GROUPNAME] - [FORUM:FORUMNAME][/TITLE][DESCRIPTION][FORUM:FORUMDESCRIPTION][/DESCRIPTION][KEYWORDS][VALUE][/KEYWORDS][/META]";

        public string MetaTitle { get; set; } = string.Empty;

        public string MetaDescription { get; set; } = string.Empty;

        public string MetaKeywords { get; set; } = string.Empty;

        public string ForumUrl { get; set; } = string.Empty;

        #region Controls
        protected af_quickjump ctlForumJump = new af_quickjump();

        protected ForumView ctlForumSubs = new ForumView();
        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (this.ForumId < 1)
                {
                    this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }

                if (this.ForumInfo == null)
                {
                    this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }

                if (this.ForumInfo.Active == false)
                {
                    this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }

                this.AppRelativeVirtualPath = "~/";
                int defaultTemplateId = this.ForumInfo.FeatureSettings.TopicsTemplateId;
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

#region "Backward compatilbility -- remove in v10.00.00"

                if (topicsTemplate.Contains("[AF:CONTROL:TOPICACTIONS]"))
                {
                    DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTokenSynonym(
                        topicsTemplate,
                        "[AF:CONTROL:TOPICACTIONS]",
                        "[DCF:TEMPLATE-TOPICACTIONS]");
                }

                if (topicsTemplate.Contains("[DCF:TEMPLATE-TOPICACTIONS]"))
                {
                    this.useListActions = true;
                    topicsTemplate = topicsTemplate.Replace("[DCF:TEMPLATE-TOPICACTIONS]", TemplateCache.GetCachedTemplate(this.ForumModuleId, "TopicActions"));
                }

                topicsTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemoveObsoleteTokens(new StringBuilder(topicsTemplate)).ToString();
                topicsTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyUserTokenSynonyms(new StringBuilder(topicsTemplate), this.PortalSettings, this.MainSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
                topicsTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyAuthorTokenSynonyms(new StringBuilder(topicsTemplate), this.PortalSettings, this.MainSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
                topicsTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyForumTokenSynonyms(new StringBuilder(topicsTemplate), this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
                topicsTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyTopicTokenSynonyms(new StringBuilder(topicsTemplate), this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale).ToString();
                topicsTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyTopicActionTokenSynonyms(new StringBuilder(topicsTemplate), this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale, this.useListActions).ToString();

#endregion "Backward compatilbility -- remove in v10.00.00"

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
                        this.bSubscribe = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanSubscribe"].ToString(), this.ForumUser.UserRoles);

                        if (this.bView)
                        {

                            this.topicRowCount = Convert.ToInt32(this.drForum["TopicRowCount"]);
                            if (this.UserId > 0)
                            {
                                this.isSubscribedForum = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(this.PortalId, this.ForumModuleId, this.UserId, this.ForumId);
                            }

                            if (this.MainSettings.UseSkinBreadCrumb)
                            {
                                string groupURL = new ControlUtils().BuildUrl(this.PortalId, this.TabId, this.ModuleId, this.ForumInfo.ForumGroup.PrefixURL, string.Empty, this.ForumInfo.ForumGroupId, -1, -1, -1, string.Empty, 1, -1, this.SocialGroupId);
                                DotNetNuke.Modules.ActiveForums.Environment.UpdateBreadCrumb(this.Page.Controls, "<a href=\"" + groupURL + "\">" + this.ForumInfo.ForumGroup.GroupName + "</a>");
                                topicsTemplate = topicsTemplate.Replace("<div class=\"afcrumb\">[FORUM:FORUMMAINLINK] > [FORUMGROUP:FORUMGROUPLINK]</div>", string.Empty);
                            }

                            if (topicsTemplate.Contains("[META]"))
                            {
                                this.MetaTemplate = TemplateUtils.GetTemplateSection(topicsTemplate, "[META]", "[/META]");
                                topicsTemplate = TemplateUtils.ReplaceSubSection(topicsTemplate, string.Empty, "[META]", "[/META]");
                            }

                            // Parse Meta Template
                            if (!string.IsNullOrEmpty(this.MetaTemplate))
                            {
                                this.MetaTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemoveObsoleteTokens(new StringBuilder( this.MetaTemplate)).ToString();
                                this.MetaTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceForumTokens(new StringBuilder(this.MetaTemplate), this.ForumInfo, this.PortalSettings, this.MainSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.TabId, this.ForumUser.CurrentUserType, this.Request.Url, this.Request.RawUrl).ToString();
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
                            this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                            this.Context.ApplicationInstance.CompleteRequest();
                        }

                        try
                        {
                            DotNetNuke.Framework.CDefault tempVar = this.BasePage;
                            DotNetNuke.Modules.ActiveForums.Environment.UpdateMeta(ref tempVar, this.MetaTitle, this.MetaDescription, this.MetaKeywords);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                            if (ex.InnerException != null)
                            {
                                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex.InnerException);
                            }
                        }
                    }
                    else
                    {
                        this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                        this.Context.ApplicationInstance.CompleteRequest();
                    }
                }
                else
                {
                    topicsTemplate = this.ParseControls(topicsTemplate);
                    topicsTemplate = Utilities.LocalizeControl(topicsTemplate);
                    this.Controls.Add(this.ParseControl(topicsTemplate));
                    this.LinkControls(this.Controls);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                if (ex.InnerException != null)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex.InnerException);
                }

                this.RenderMessage("[RESX:Error:LoadingTopics]", ex.Message, ex);
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
            if (sOutput.Contains("[ANNOUNCEMENTS]"))
            {
                string sAnnounce = TemplateUtils.GetTemplateSection(sOutput, "[ANNOUNCEMENTS]", "[/ANNOUNCEMENTS]");
                if (this.dtAnnounce != null && this.dtAnnounce.Rows.Count > 0)
                {
                    sAnnounce = this.ParseTopics(sAnnounce, this.dtAnnounce, "ANNOUNCEMENT");
                }
                else
                {
                    sAnnounce = string.Empty;
                }

                sOutput = TemplateUtils.ReplaceSubSection(sOutput, sAnnounce, "[ANNOUNCEMENTS]", "[/ANNOUNCEMENTS]");
            }
                
            StringBuilder stringBuilder = new StringBuilder(sOutput);
            stringBuilder = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceForumTokens(stringBuilder, this.ForumInfo, this.PortalSettings, this.MainSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.TabId, this.ForumUser.CurrentUserType, this.Request.Url, this.Request.RawUrl);
            sOutput = stringBuilder.ToString();

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

            //Forum Drop Downlist
            sOutput = sOutput.Replace("[JUMPTO]", "<asp:placeholder id=\"plhQuickJump\" runat=\"server\" />");
            //Tag Cloud
            sOutput = sOutput.Replace("[AF:CONTROLS:TAGCLOUD]", "<ac:tagcloud ModuleId=\"" + this.ModuleId + "\" PortalId=\"" + this.PortalId + "\" tabid=\"" + this.TabId + "\" runat=\"server\" />");


            //Forum Subscription Control
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
                var @params = new List<string>
                {
                    $"{ParamKeys.ViewType}={Views.Post}",
                    $"{ParamKeys.ForumId}={this.ForumId}",
                };
                if (this.SocialGroupId > 0)
                {
                    @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
                }

                sOutput = sOutput.Replace("[ADDTOPIC]", $"<a href=\"{this.NavigateUrl(this.TabId, string.Empty, @params.ToArray())}\" class=\"dnnPrimaryAction\">[RESX:AddTopic]</a>");
            }
            else
            {
                if (!this.Request.IsAuthenticated)
                {
                    sOutput = sOutput.Replace("[ADDTOPIC]", $"{string.Format(Utilities.GetSharedResource("[RESX:NotAuthorizedTopicPleaseLogin]"), "[DCF:LOGINLINK]", "[DCF:LOGINPOPUPLINK]")}");
                }
                else
                {
                    sOutput = sOutput.Replace("[ADDTOPIC]", "[RESX:NotAuthorizedTopic]");
                }
            }

            sOutput = sOutput.Replace("[ADDPOLL]", string.Empty);

            if (this.Request.IsAuthenticated)
            {
                string Url = this.NavigateUrl(this.TabId, string.Empty, new string[] { ParamKeys.ViewType + "=sendto", ParamKeys.ForumId + "=" + this.ForumId, ParamKeys.TopicId + "=" + this.TopicId });
                sOutput = sOutput.Replace("[AF:CONTROL:EMAIL]", "<a href=\"" + Url + "\" rel=\"nofollow\"><img src=\"" + Utilities.ResolveUrl(this.MainSettings.ThemeLocation + "/images/email16.png") + "\" border=\"0\" alt=\"[RESX:EmailThis]\" /></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[AF:CONTROL:EMAIL]", string.Empty);
            }
            sOutput = sOutput.Replace("[MINISEARCH]", "<am:MiniSearch id=\"amMiniSearch\" runat=\"server\" EnableViewState=\"False\" MID=\"" + this.ModuleId + "\" TID=\"" + this.TabId + "\" FID=\"" + this.ForumId + "\" />");
            sOutput = sOutput.Replace("[PAGER1]", "<am:pagernav id=\"Pager1\" runat=\"server\" EnableViewState=\"False\" />");
            sOutput = sOutput.Replace("[PAGER2]", "<am:pagernav id=\"Pager2\" runat=\"server\" EnableViewState=\"False\" />");



            return sOutput;
        }

        private string ParseTopics(string Template, DataTable Topics, string Section)
        {
            if (Topics?.Rows?.Count < 1)
            {
                return TemplateUtils.ReplaceSubSection(Template, string.Empty, "[" + Section + "]", "[/" + Section + "]");
            }

            string template = TemplateUtils.GetTemplateSection(Template, "[" + Section + "]", "[/" + Section + "]");
            string sTopics = string.Empty;

            int rowcount = 0;
            foreach (DataRow drTopic in Topics.Rows)
            {
                string topicTemplate = template;

                /* pull in as much data as needed from stored procedure to populate object model; eventually want to move this part to DAL2 */
                var topicInfo = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo
                {
                    ModuleId = this.ForumModuleId,
                    PortalId = this.PortalId,
                    ForumId = Convert.ToInt32(drTopic["ForumId"]),
                    Forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(Convert.ToInt32(drTopic["ForumId"]), this.ForumModuleId),
                    TopicId = Convert.ToInt32(drTopic["TopicId"]),
                    TopicType = (TopicTypes)Enum.Parse(typeof(TopicTypes), Convert.ToInt32(drTopic["TopicType"]).ToString()),
                    Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                    {
                        ModuleId = this.ForumModuleId,
                        Subject = System.Net.WebUtility.HtmlDecode(Convert.ToString(drTopic["Subject"])),
                        Summary = System.Net.WebUtility.HtmlDecode(Convert.ToString(drTopic["Summary"])),
                        DateCreated = Convert.ToDateTime(drTopic["DateCreated"]),
                        Body = System.Net.WebUtility.HtmlDecode(Convert.ToString(drTopic["Body"])),
                        AuthorId = Convert.ToInt32(drTopic["AuthorId"]),
                        AuthorName = Convert.ToString(drTopic["AuthorName"]).ToString().Replace("&amp;#", "&#"),
                    },
                    LastReplyId = Convert.ToInt32(drTopic["LastReplyId"]),
                    ReplyCount = Convert.ToInt32(drTopic["ReplyCount"]),
                    ViewCount = Convert.ToInt32(drTopic["ViewCount"]),
                    StatusId = Convert.ToInt32(drTopic["StatusId"]),
                    IsLocked = Convert.ToBoolean(drTopic["IsLocked"]),
                    IsPinned = Convert.ToBoolean(drTopic["IsPinned"]),
                    Rating = Convert.ToInt32(drTopic["TopicRating"]),
                    TopicUrl = drTopic["TopicURL"].ToString(),
                    TopicData = drTopic["TopicData"].ToString(),
                    LastReply = new DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo
                    {
                        ModuleId = this.ForumModuleId,
                        PortalId = this.PortalId,
                        Author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(this.PortalId, this.ForumModuleId, Convert.ToInt32(drTopic["LastReplyAuthorId"])),
                        ReplyId = Convert.ToInt32(drTopic["LastReplyId"]),
                        TopicId = Convert.ToInt32(drTopic["TopicId"]),
                        Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                        {
                            ModuleId = this.ForumModuleId,
                            Subject = System.Net.WebUtility.HtmlDecode(Convert.ToString(drTopic["LastReplySubject"])),
                            Summary = System.Net.WebUtility.HtmlDecode(Convert.ToString(drTopic["LastReplySummary"])),
                            DateCreated = Convert.ToDateTime(drTopic["LastReplyDate"]),
                            AuthorId = Convert.ToInt32(drTopic["LastReplyAuthorId"]),
                            AuthorName = Convert.ToString(drTopic["LastReplyAuthorName"]).ToString().Replace("&amp;#", "&#"),
                        },
                    },
                };
                if (!(string.IsNullOrEmpty(topicInfo.Content.Summary)) && (!(Utilities.HasHTML(topicInfo.Content.Summary))))
                {
                    topicInfo.Content.Summary = topicInfo.Content.Summary.Replace(System.Environment.NewLine, "<br />");
                }

                topicInfo.Author.ForumUser.ModuleId = this.ForumModuleId;
                topicInfo.Author.ForumUser.PortalId = this.PortalId;
                topicInfo.Author.FirstName = topicInfo.Author?.FirstName?.Replace("&amp;#", "&#");
                topicInfo.Author.LastName = topicInfo.Author.LastName?.Replace("&amp;#", "&#");
                topicInfo.Author.DisplayName = topicInfo.Author.DisplayName?.Replace("&amp;#", "&#");
                topicInfo.LastReplyAuthor.ForumUser.ModuleId = this.ForumModuleId;
                topicInfo.LastReplyAuthor.ForumUser.PortalId = this.PortalId;
                topicInfo.LastReplyAuthor.FirstName = topicInfo.Author.FirstName?.Replace("&amp;#", "&#");
                topicInfo.LastReplyAuthor.LastName = topicInfo.Author.LastName?.Replace("&amp;#", "&#");
                topicInfo.LastReplyAuthor.DisplayName = topicInfo.Author.DisplayName?.Replace("&amp;#", "&#");

                int UserLastTopicRead = Convert.ToInt32(drTopic["UserLastTopicRead"]);
                int UserLastReplyRead = Convert.ToInt32(drTopic["UserLastReplyRead"]);

                if (string.IsNullOrEmpty(topicInfo.LastReply.Content.Subject))
                {
                    topicInfo.LastReply.Content.Subject = "RE: " + topicInfo.Content.Subject;
                }

                if (topicInfo.LastReply.Content.Summary == string.Empty)
                {
                    topicInfo.LastReply.Content.Summary = topicInfo.Content.Summary;
                }

                if (string.IsNullOrEmpty(topicInfo.TopicData))
                {
                    topicTemplate = TemplateUtils.ReplaceSubSection(topicTemplate, string.Empty, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");
                }
                else
                {
                    string sPropTemplate = TemplateUtils.GetTemplateSection(topicTemplate, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");
                    string sProps = string.Empty;
                    var pl = DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Deserialize(topicInfo.TopicData);
                    foreach (var p in pl)
                    {
                        string tmp = sPropTemplate;
                        var pName = System.Net.WebUtility.HtmlDecode(p.Name);
                        var pValue = System.Net.WebUtility.HtmlDecode(p.Value);
                        tmp = tmp.Replace("[AF:PROPERTY:LABEL]", Utilities.GetSharedResource("[RESX:" + pName + "]"));
                        tmp = tmp.Replace("[AF:PROPERTY:VALUE]", pValue);
                        topicTemplate = topicTemplate.Replace("[AF:PROPERTY:" + pName + ":LABEL]", Utilities.GetSharedResource("[RESX:" + pName + "]"));
                        topicTemplate = topicTemplate.Replace("[AF:PROPERTY:" + pName + ":VALUE]", pValue);
                        string pValueKey = string.Empty;
                        if (!(string.IsNullOrEmpty(pValue)))
                        {
                            pValueKey = Utilities.CleanName(pValue).ToLowerInvariant();
                        }
                        topicTemplate = topicTemplate.Replace("[AF:PROPERTY:" + pName + ":VALUEKEY]", pValueKey);
                        sProps += tmp;
                    }

                    topicTemplate = TemplateUtils.ReplaceSubSection(topicTemplate, sProps, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");
                }

                if (topicTemplate.Contains("[LASTPOST]") && topicInfo.LastReply?.ReplyId == 0)
                {
                    topicTemplate = TemplateUtils.ReplaceSubSection(topicTemplate, string.Empty, "[LASTPOST]", "[/LASTPOST]");
                }

                StringBuilder stringBuilder = new StringBuilder(topicTemplate);

                if (this.UserId == -1 || topicInfo.LastReplyId == 0)
                {
                    stringBuilder.Replace("[AF:ICONLINK:LASTREAD]", string.Empty);
                    stringBuilder.Replace("[AF:URL:LASTREAD]", string.Empty);
                }

                if ((UserLastTopicRead >= topicInfo.TopicId || UserLastTopicRead == 0) & (UserLastReplyRead >= topicInfo.LastReplyId || UserLastReplyRead == 0))
                {
                    stringBuilder.Replace("[AF:ICONLINK:LASTREAD]", string.Empty);
                    stringBuilder.Replace("[AF:URL:LASTREAD]", string.Empty);
                }

                stringBuilder = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTopicTokens(stringBuilder, topicInfo, this.PortalSettings, this.MainSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.Request.Url, this.Request.RawUrl);
                stringBuilder.Replace("[LASTPOST]", string.Empty).Replace("[/LASTPOST]", string.Empty);
                stringBuilder.Replace("[ROWCSS]", this.GetRowCSS(UserLastTopicRead, UserLastReplyRead, topicInfo.TopicId, topicInfo.LastReplyId, rowcount));

                if (topicInfo.LastReplyId < 1)
                {
                    stringBuilder.Replace("[AF:UI:MINIPAGER]", string.Empty);
                }
                else
                {
                    stringBuilder.Replace("[AF:UI:MINIPAGER]", this.GetSubPages(this.TabId, topicInfo.ReplyCount, this.ForumId, topicInfo.TopicId));
                }

                sTopics += stringBuilder.ToString();
                rowcount += 1;
            }

            return TemplateUtils.ReplaceSubSection(Template, sTopics, "[" + Section + "]", "[/" + Section + "]");
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


        private string GetSubPages(int tabID, int replies, int forumID, int postID)
        {
            int i = 0;
            string sOut = string.Empty;

            if (replies + 1 > this.pageSize)
            {
                List<string> Params = new List<string>();
                sOut = "<div class=\"afpagermini\">(<img src=\"" + this.MainSettings.ThemeLocation + "/images/icon_multipage.png\" alt=\"[RESX:MultiPageTopic]\" style=\"vertical-align:middle;\" />";
                //Jump to pages
                int intPostPages = 0;
                intPostPages = Convert.ToInt32(System.Math.Ceiling((double)(replies + 1) / this.pageSize));
                if (intPostPages > 3)
                {
                    for (i = 1; i <= 3; i++)
                    {

                        Params = new List<string> { ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + postID, ParamKeys.ViewType + "=" + Views.Topic };
                        if (this.MainSettings.UseShortUrls)
                        {
                            Params = new List<string> { ParamKeys.TopicId + "=" + postID };
                        }
                        if (i > 1)
                        {
                            Params.Add(ParamKeys.PageId + "=" + i);
                        }
                        sOut += "<a href=\"" + this.NavigateUrl(tabID, string.Empty, Params.ToArray()) + "\">" + i + "</a>&nbsp;";

                        if (intPostPages > 4)
                        {
                            sOut += "...&nbsp;";
                        }

                        Params = new List<string> { ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + postID, ParamKeys.ViewType + "=" + Views.Topic };
                        if (this.MainSettings.UseShortUrls)
                        {
                            Params = new List<string> { ParamKeys.TopicId + "=" + postID };
                        }
                        if (i > 1)
                        {
                            Params.Add(ParamKeys.PageJumpId + "=" + i);
                        }
                        sOut += "<a href=\"" + this.NavigateUrl(tabID, string.Empty, Params.ToArray()) + "\">" + i + "</a>&nbsp;";

                        sOut += "<a href=\"" + this.NavigateUrl(tabID, string.Empty, Params.ToArray()) + "\">" + i + "</a>&nbsp;";
                    }
                }
                else
                {
                    for (i = 1; i <= intPostPages; i++)
                    {
                        Params = new List<string> { ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + postID, ParamKeys.ViewType + "=" + Views.Topic };
                        if (this.MainSettings.UseShortUrls)
                        {
                            Params = new List<string> { ParamKeys.TopicId + "=" + postID };
                        }
                        if (i > 1)
                        {
                            Params.Add(ParamKeys.PageJumpId + "=" + i);
                        }
                        sOut += "<a href=\"" + this.NavigateUrl(tabID, string.Empty, Params.ToArray()) + "\">" + i + "</a>&nbsp;";
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
