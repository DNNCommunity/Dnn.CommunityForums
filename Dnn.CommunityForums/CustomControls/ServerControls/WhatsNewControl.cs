﻿// Copyright (c) by DNN Community
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
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [ToolboxData("<{0}:WhatsNewControl runat=server></{0}:WhatsNewControl>")]
    public class WhatsNewControl : DotNetNuke.Entities.Modules.PortalModuleBase
    {
        #region Private Member Variables

        private string forumIds = string.Empty;
        private bool topicsOnly = true;
        private int rows = 10;
        private string tags = string.Empty;
        private int filterByUserId = -1;
        private int tabId = -1;
        private string additionalParams = string.Empty;

        #endregion

        public string AdditionalParams
        {
            get
            {
                return this.additionalParams;
            }

            set
            {
                this.additionalParams = value;
            }
        }

        public int TabId
        {
            get
            {
                return this.tabId;
            }

            set
            {
                this.tabId = value;
            }
        }

        public string ForumIds
        {
            get
            {
                return this.forumIds;
            }

            set
            {
                this.forumIds = value;
            }
        }

        public bool TopicsOnly
        {
            get
            {
                return this.topicsOnly;
            }

            set
            {
                this.topicsOnly = value;
            }
        }

        public bool RandomOrder { get; set; }

        public int Rows
        {
            get
            {
                return this.rows;
            }

            set
            {
                this.rows = value;
            }
        }

        public string Tags
        {
            get
            {
                return this.tags;
            }

            set
            {
                this.tags = value;
            }
        }

        public int FilterByUserId
        {
            get
            {
                return this.filterByUserId;
            }

            set
            {
                this.filterByUserId = value;
            }
        }

        [Description("Template for display"), PersistenceMode(PersistenceMode.InnerProperty)]
        public DisplayTemplate Template { get; set; }

        [Description("Template for display"), PersistenceMode(PersistenceMode.InnerProperty)]
        public DisplayTemplate HeaderTemplate { get; set; }

        [Description("Template for display"), PersistenceMode(PersistenceMode.InnerProperty)]
        public DisplayTemplate FooterTemplate { get; set; }

        public bool EnableRSS { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var sHeaderTemplate = "<div style=\"padding:10px;padding-top:5px;\">";
            var sFooterTemplate = "</div>";

            if (this.HeaderTemplate != null)
            {
                sHeaderTemplate = this.HeaderTemplate.Text;
            }

            if (this.FooterTemplate != null)
            {
                sFooterTemplate = this.FooterTemplate.Text;
            }

            var sTemplate = "<div style=\"padding-bottom:2px;\" class=\"normal\">[SUBJECTLINK]</div><div style=\"padding-bottom:2px;border-bottom:solid 1px #AAA;\">by [AUTHORDISPLAYNAME]</div>";
            if (this.Template != null)
            {
                sTemplate = this.Template.Text;
            }

            if (this.ForumIds == string.Empty && this.FilterByUserId <= 0)
            {
                return;
            }

            if (this.ForumIds.Contains(";"))
            {
                this.ForumIds = this.ForumIds.Replace(";", ":");
            }

            var sb = new StringBuilder(1024);
            sb.Append(sHeaderTemplate);

            var bodyLength = -1;
            var bodyTrim = string.Empty;
            if (sTemplate.Contains("[BODY:"))
            {
                var inStart = sTemplate.IndexOf("[BODY:", StringComparison.Ordinal) + 1 + 5;
                var inEnd = sTemplate.IndexOf("]", inStart - 1, StringComparison.Ordinal) + 1 - 1;
                var sLength = sTemplate.Substring(inStart, inEnd - inStart);
                bodyLength = Convert.ToInt32(sLength);
                bodyTrim = "[BODY:" + bodyLength + "]";
            }

            IDataReader dr;
            if (this.ForumIds == string.Empty && this.FilterByUserId > 0)
            {
                this.ForumIds = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.PortalId, -1, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(this.PortalId, this.UserId)).Replace(";", ":");
                dr = DataProvider.Instance().GetPostsByUser(this.PortalId, this.Rows, this.UserInfo.IsSuperUser, this.UserId, this.FilterByUserId, this.TopicsOnly, this.ForumIds);
            }
            else
            {
                dr = DataProvider.Instance().GetPosts(this.PortalId, this.ForumIds, this.TopicsOnly, this.RandomOrder, this.Rows, this.Tags, this.FilterByUserId);
            }

            try
            {
                var sTempTemplate = sTemplate;
                while (dr.Read())
                {
                    var groupName = Convert.ToString(dr["GroupName"]);
                    var groupId = Convert.ToString(dr["ForumGroupId"]);
                    var topicTabId = Convert.ToString(dr["TabId"]);
                    var topicModuleId = Convert.ToString(dr["ModuleId"]);
                    var forumName = Convert.ToString(dr["ForumName"]);
                    var forumId = Convert.ToString(dr["ForumId"]);
                    var subject = Convert.ToString(dr["Subject"]);
                    var userName = Convert.ToString(dr["AuthorUserName"]);
                    var firstName = Convert.ToString(dr["AuthorFirstName"]);
                    var lastName = Convert.ToString(dr["AuthorLastName"]);
                    var authorId = Convert.ToString(dr["AuthorId"]);
                    var displayName = Convert.ToString(dr["AuthorDisplayName"]);
                    var postDate = Convert.ToString(dr["DateCreated"]);
                    var body = Utilities.StripHTMLTag(Convert.ToString(dr["Body"]));
                    var topicId = Convert.ToString(dr["TopicId"]);
                    var replyId = Convert.ToString(dr["ReplyId"]);
                    var bodyHTML = Convert.ToString(dr["Body"]);
                    var replyCount = Convert.ToString(dr["ReplyCount"]);
                    var sForumUrl = dr["PrefixURL"].ToString();
                    var sTopicURL = dr["TopicURL"].ToString();
                    var sGroupPrefixURL = dr["GroupPrefixURL"].ToString();
                    sTempTemplate = sTempTemplate.Replace("[FORUMGROUPNAME]", groupName);
                    sTempTemplate = sTempTemplate.Replace("[FORUMGROUPID]", groupId);
                    sTempTemplate = sTempTemplate.Replace("[TOPICTABID]", topicTabId);
                    sTempTemplate = sTempTemplate.Replace("[TOPICMODULEID]", topicModuleId);
                    sTempTemplate = sTempTemplate.Replace("[FORUMNAME]", forumName);
                    sTempTemplate = sTempTemplate.Replace("[FORUMID]", forumId);
                    sTempTemplate = sTempTemplate.Replace("[SUBJECT]", subject);
                    sTempTemplate = sTempTemplate.Replace("[AUTHORUSERNAME]", userName);
                    sTempTemplate = sTempTemplate.Replace("[AUTHORFIRSTNAME]", firstName);
                    sTempTemplate = sTempTemplate.Replace("[AUTHORLASTNAME]", lastName);
                    sTempTemplate = sTempTemplate.Replace("[AUTHORID]", authorId);
                    sTempTemplate = sTempTemplate.Replace("[AUTHORDISPLAYNAME]", displayName);
                    sTempTemplate = sTempTemplate.Replace("[DATE]", Utilities.GetUserFormattedDateTime(Convert.ToDateTime(postDate), this.PortalId, this.UserId));
                    sTempTemplate = sTempTemplate.Replace("[BODY]", body);
                    sTempTemplate = sTempTemplate.Replace("[BODYHTML]", bodyHTML);
                    sTempTemplate = sTempTemplate.Replace("[BODYTEXT]", Utilities.StripHTMLTag(bodyHTML));

                    if (bodyTrim != string.Empty)
                    {
                        if (bodyLength > 0 & body.Length > bodyLength)
                        {
                            sTempTemplate = sTempTemplate.Replace(bodyTrim, body.Substring(0, bodyLength) + "...");
                        }
                        else
                        {
                            sTempTemplate = sTempTemplate.Replace(bodyTrim, body);
                        }
                    }

                    sTempTemplate = sTempTemplate.Replace("[TOPICID]", topicId);
                    sTempTemplate = sTempTemplate.Replace("[REPLYID]", replyId);
                    sTempTemplate = sTempTemplate.Replace("[REPLYCOUNT]", replyCount);

                    if (this.TabId == -1)
                    {
                        this.TabId = Convert.ToInt32(topicTabId);
                    }

                    if (Utilities.UseFriendlyURLs(Utilities.SafeConvertInt(topicModuleId)) && !string.IsNullOrEmpty(sForumUrl) && !string.IsNullOrEmpty(sTopicURL))
                    {
                        var ctlUtils = new ControlUtils();
                        sTopicURL = ctlUtils.BuildUrl(this.PortalId, Convert.ToInt32(topicTabId), Convert.ToInt32(topicModuleId), sGroupPrefixURL, sForumUrl, Convert.ToInt32(groupId), Convert.ToInt32(forumId), Convert.ToInt32(topicId), sTopicURL, -1, -1, string.Empty, 1, Convert.ToInt32(replyId), -1);

                        if (Convert.ToInt32(replyId) == 0)
                        {
                            sTempTemplate = sTempTemplate.Replace("[POSTURL]", sTopicURL);
                            sTempTemplate = sTempTemplate.Replace("[SUBJECTLINK]", "<a href=\"" + sTopicURL + "\">" + subject + "</a>");
                        }
                        else
                        {
                            if (!sTopicURL.EndsWith("/"))
                            {
                                sTopicURL += "/";
                            }

                            sTopicURL += "?afc=" + replyId;
                            sTempTemplate = sTempTemplate.Replace("[POSTURL]", sTopicURL);
                            if (this.Request.IsAuthenticated)
                            {
                                sTempTemplate = sTempTemplate.Replace("[SUBJECTLINK]", "<a href=\"" + sTopicURL + "\">" + subject + "</a>");
                            }
                            else
                            {
                                sTempTemplate = sTempTemplate.Replace("[SUBJECTLINK]", "<a href=\"" + sTopicURL + "\" rel=\"nofollow\">" + subject + "</a>");
                            }
                        }

                        sTempTemplate = sTempTemplate.Replace("[TOPICSURL]", sForumUrl);
                    }
                    else
                    {
                        List<string> @params;
                        if (Convert.ToInt32(replyId) == 0)
                        {
                            @params = new List<string> { ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId };
                            if (this.AdditionalParams != string.Empty)
                            {
                                @params.Add(this.AdditionalParams);
                            }

                            sTempTemplate = sTempTemplate.Replace("[POSTURL]", Utilities.NavigateURL(this.TabId, string.Empty, @params.ToArray()));

                            @params = new List<string> { ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId };
                            if (this.AdditionalParams != string.Empty)
                            {
                                @params.Add(this.AdditionalParams);
                            }

                            sTempTemplate = sTempTemplate.Replace("[SUBJECTLINK]", "<a href=\"" + Utilities.NavigateURL(this.TabId, string.Empty, @params.ToArray()) + "\">" + subject + "</a>");
                        }
                        else
                        {
                            @params = new List<string> { ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId, ParamKeys.ContentJumpId + "=" + replyId };
                            if (this.AdditionalParams != string.Empty)
                            {
                                @params.Add(this.AdditionalParams);
                            }

                            sTempTemplate = sTempTemplate.Replace("[POSTURL]", Utilities.NavigateURL(this.TabId, string.Empty, @params.ToArray()));

                            @params = new List<string> { ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId, ParamKeys.ContentJumpId + "=" + replyId };
                            if (this.AdditionalParams != string.Empty)
                            {
                                @params.Add(this.AdditionalParams);
                            }

                            sTempTemplate = sTempTemplate.Replace("[SUBJECTLINK]", "<a href=\"" + Utilities.NavigateURL(this.TabId, string.Empty, @params.ToArray()) + "\">" + subject + "</a>");
                        }

                        @params = new List<string> { ParamKeys.ViewType + "=" + Views.Topics, ParamKeys.ForumId + "=" + forumId };
                        if (this.AdditionalParams != string.Empty)
                        {
                            @params.Add(this.AdditionalParams);
                        }

                        sTempTemplate = sTempTemplate.Replace("[TOPICSURL]", Utilities.NavigateURL(this.TabId, string.Empty, @params.ToArray()));
                    }

                    sTempTemplate = sTempTemplate.Replace("[FORUMURL]", Utilities.NavigateURL(this.TabId));
                    sb.Append(sTempTemplate);
                }

                dr.Close();

                var sRSSImage = "<img src=\"" + this.Page.ResolveUrl("<% (DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath) %>feedicon.gif") + "\" border=\"0\" />";
                var sRSSURL = this.Page.ResolveUrl("~/desktopmodules/activeforumswhatsnew/feeds.aspx") + "?portalId=" + this.PortalId + "&tabid=" + this.TabId.ToString() + "&moduleid=" + this.ModuleId.ToString();
                var sRSSIconLink = "<a href=\"" + sRSSURL + "\">" + sRSSImage + "</a>";
                sFooterTemplate = sFooterTemplate.Replace("[RSSICON]", sRSSImage);
                sFooterTemplate = sFooterTemplate.Replace("[RSSURL]", sRSSURL);
                sFooterTemplate = sFooterTemplate.Replace("[RSSICONLINK]", sRSSIconLink);

                sb.Append(sFooterTemplate);
                var lit = new Literal { Text = sb.ToString() };
                this.Controls.Add(lit);
            }
            catch (Exception ex)
            {
                if (!dr.IsClosed)
                {
                    dr.Close();
                }

                sb.Append(ex.StackTrace);
                var lit = new Literal { Text = ex.Message };
                this.Controls.Add(lit);
            }
        }
    }
}
