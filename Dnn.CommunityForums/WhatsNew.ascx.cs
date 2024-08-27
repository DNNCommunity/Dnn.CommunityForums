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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;

    public partial class WhatsNew : PortalModuleBase, IActionable
    {
        #region Private Variables

        private DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo currentUser;
        private string authorizedForums;
        private WhatsNewModuleSettings settings;

        #endregion

        #region Properties

        private WhatsNewModuleSettings Settings
        {
            get
            {
                if (this.settings == null)
                {
                    var settingsCacheKey = string.Format(CacheKeys.WhatsNew, this.ModuleId);

                    var moduleSettings = DataCache.SettingsCacheRetrieve(this.ModuleId, settingsCacheKey) as Hashtable;
                    if (moduleSettings == null)
                    {
                        moduleSettings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: this.ModuleId, tabId: this.TabId, ignoreCache: false).ModuleSettings;
                        DataCache.SettingsCacheStore(this.ModuleId, settingsCacheKey, moduleSettings);
                    }

                    this.settings = WhatsNewModuleSettings.CreateFromModuleSettings(moduleSettings);
                }

                return this.settings;
            }
        }

        private DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo CurrentUser
        {
            get { return this.currentUser ?? (this.currentUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetUserFromHttpContext(this.PortalId, this.ModuleId)); }
        }

        private string AuthorizedForums
        {
            get
            {
                return this.authorizedForums ??
                       (this.authorizedForums =
                       DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CheckForumIdsForViewForRSS(-1, this.Settings.Forums, this.CurrentUser.UserRoles));
            }
        }

        #endregion

        #region DNN Actions

        public DotNetNuke.Entities.Modules.Actions.ModuleActionCollection ModuleActions
        {
            get
            {
                return new DotNetNuke.Entities.Modules.Actions.ModuleActionCollection
                                  {
                                      {
                                          this.GetNextActionID(), "Edit", /* Utilities.GetSharedResource("Configure") */
                                          DotNetNuke.Entities.Modules.Actions.ModuleActionType.EditContent, string.Empty, string.Empty,
                                          this.EditUrl(), false, Security.SecurityAccessLevel.Edit, true, false
                                      },
                                  };
            }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var dr = DataProvider.Instance().GetPosts(this.PortalId, this.AuthorizedForums, this.Settings.TopicsOnly, this.Settings.RandomOrder, this.Settings.Rows, this.Settings.Tags);

            var sb = new StringBuilder(this.Settings.Header);

            var sHost = Utilities.GetHost();

            try
            {
                while (dr.Read())
                {
                    var groupName = Convert.ToString(dr["GroupName"]);
                    var groupId = Convert.ToInt32(dr["ForumGroupId"]);
                    var topicTabId = Convert.ToInt32(dr["TabId"]);
                    var topicModuleId = Convert.ToInt32(dr["ModuleId"]);
                    var forumName = Convert.ToString(dr["ForumName"]);
                    var forumId = Convert.ToInt32(dr["ForumId"]);
                    var subject = Convert.ToString(dr["Subject"]);
                    var userName = Convert.ToString(dr["AuthorUserName"]);
                    var firstName = Convert.ToString(dr["AuthorFirstName"]);
                    var lastName = Convert.ToString(dr["AuthorLastName"]);
                    var authorId = Convert.ToInt32(dr["AuthorId"]);
                    var displayName = Convert.ToString(dr["AuthorDisplayName"]);
                    var postDate = Convert.ToDateTime(dr["DateCreated"]);
                    var body = Utilities.StripHTMLTag(Convert.ToString(dr["Body"]));
                    var topicId = Convert.ToInt32(dr["TopicId"]);
                    var replyId = Convert.ToInt32(dr["ReplyId"]);
                    var bodyHtml = Convert.ToString(dr["Body"]);
                    var replyCount = Convert.ToInt32(dr["ReplyCount"]);

                    var sForumUrl = dr["PrefixURL"].ToString();
                    var sTopicUrl = dr["TopicURL"].ToString();
                    var sGroupPrefixUrl = dr["GroupPrefixURL"].ToString();

                    var ts = SettingsBase.GetModuleSettings(topicModuleId);

                    var sbTemplate = new StringBuilder(this.Settings.Format ?? string.Empty);

                    sbTemplate = sbTemplate.Replace("[FORUMGROUPNAME]", groupName);
                    sbTemplate = sbTemplate.Replace("[FORUMGROUPID]", groupId.ToString());
                    sbTemplate = sbTemplate.Replace("[TOPICTABID]", topicTabId.ToString());
                    sbTemplate = sbTemplate.Replace("[TOPICMODULEID]", topicModuleId.ToString());
                    sbTemplate = sbTemplate.Replace("[FORUMNAME]", forumName);
                    sbTemplate = sbTemplate.Replace("[FORUMID]", forumId.ToString());
                    sbTemplate = sbTemplate.Replace("[SUBJECT]", subject);
                    sbTemplate = sbTemplate.Replace("[AUTHORUSERNAME]", userName);
                    sbTemplate = sbTemplate.Replace("[AUTHORFIRSTNAME]", firstName);
                    sbTemplate = sbTemplate.Replace("[AUTHORLASTNAME]", lastName);
                    sbTemplate = sbTemplate.Replace("[AUTHORID]", authorId.ToString());
                    sbTemplate = sbTemplate.Replace("[AUTHORDISPLAYNAME]", displayName);
                    sbTemplate = sbTemplate.Replace("[DATE]", Utilities.GetUserFormattedDateTime(postDate, this.PortalId, this.UserId));
                    sbTemplate = sbTemplate.Replace("[TOPICID]", topicId.ToString());
                    sbTemplate = sbTemplate.Replace("[REPLYID]", replyId.ToString());
                    sbTemplate = sbTemplate.Replace("[REPLYCOUNT]", replyCount.ToString());

                    if (Utilities.UseFriendlyURLs(topicModuleId) && !(string.IsNullOrEmpty(sForumUrl) && string.IsNullOrEmpty(sTopicUrl)))
                    {
                        var ctlUtils = new ControlUtils();

                        sTopicUrl = ctlUtils.BuildUrl(topicTabId, topicModuleId, sGroupPrefixUrl, sForumUrl, groupId, forumId, topicId, sTopicUrl, -1, -1, string.Empty, 1, replyId, -1);
                        sForumUrl = ctlUtils.BuildUrl(topicTabId, topicModuleId, sGroupPrefixUrl, sForumUrl, groupId, forumId, -1, string.Empty, -1, -1, string.Empty, 1, replyId, -1);

                        if (sHost.EndsWith("/") && sForumUrl.StartsWith("/"))
                        {
                            sForumUrl = sForumUrl.Substring(1);
                        }

                        if (!sForumUrl.StartsWith(sHost))
                        {
                            sForumUrl = sHost + sForumUrl;
                        }

                        if (sHost.EndsWith("/") && sTopicUrl.StartsWith("/"))
                        {
                            sTopicUrl = sTopicUrl.Substring(1);
                        }

                        if (!sTopicUrl.StartsWith(sHost))
                        {
                            sTopicUrl = sHost + sTopicUrl;
                        }

                        if (Convert.ToInt32(replyId) == 0)
                        {
                            sbTemplate = sbTemplate.Replace("[POSTURL]", sTopicUrl);
                            sbTemplate = sbTemplate.Replace("[SUBJECTLINK]", "<a href=\"" + sTopicUrl + "\">" + subject + "</a>");
                        }
                        else
                        {
                            if (!sTopicUrl.EndsWith("/") && !sTopicUrl.EndsWith("aspx"))
                            {
                                sTopicUrl += "/";
                            }

                            sTopicUrl += "?afc=" + replyId;
                            sbTemplate = sbTemplate.Replace("[POSTURL]", sTopicUrl);
                            sbTemplate = sbTemplate.Replace("[SUBJECTLINK]", "<a href=\"" + sTopicUrl + "\">" + subject + "</a>");
                        }

                        sbTemplate = sbTemplate.Replace("[TOPICSURL]", sForumUrl);
                    }
                    else
                    {
                        if (replyId == 0)
                        {
                            sbTemplate = sbTemplate.Replace("[POSTURL]", Utilities.NavigateURL(topicTabId, string.Empty, new[] { ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId }));
                            sbTemplate = sbTemplate.Replace("[SUBJECTLINK]", "<a href=\"" + Utilities.NavigateURL(topicTabId, string.Empty, new[] { ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId }) + "\">" + subject + "</a>");
                        }
                        else
                        {
                            sbTemplate = sbTemplate.Replace("[POSTURL]", Utilities.NavigateURL(topicTabId, string.Empty, new[] { ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId, ParamKeys.ContentJumpId + "=" + replyId }));
                            sbTemplate = sbTemplate.Replace("[SUBJECTLINK]", "<a href=\"" + Utilities.NavigateURL(topicTabId, string.Empty, new[] { ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId, ParamKeys.ContentJumpId + "=" + replyId }) + "\">" + subject + "</a>");
                        }

                        sbTemplate = sbTemplate.Replace("[TOPICSURL]", Utilities.NavigateURL(topicTabId, string.Empty, new[] { ParamKeys.ViewType + "=" + Views.Topics, ParamKeys.ForumId + "=" + forumId }));
                    }

                    sbTemplate = sbTemplate.Replace("[FORUMURL]", Utilities.NavigateURL(topicTabId));

                    // Do the body replacements last as they are the most likely to contain conflicts.
                    sbTemplate = sbTemplate.Replace("[BODY]", body);
                    sbTemplate = sbTemplate.Replace("[BODYHTML]", bodyHtml);
                    sbTemplate = sbTemplate.Replace("[BODYTEXT]", Utilities.StripHTMLTag(bodyHtml));

                    var template = sbTemplate.ToString();

                    // Do this regex replace first before moving on to the simpler ones.
                    template = Regex.Replace(template, @"\[BODY\:\s*(\d+)\s*\]", m => SafeSubstring(body, int.Parse(m.Groups[1].Value)), RegexOptions.IgnoreCase);

                    sb.Append(template);
                }

                dr.Close();
            }
            catch (Exception ex)
            {
                if (!dr.IsClosed)
                {
                    dr.Close();
                }

                sb.Append(ex.StackTrace);
            }

            var sRSSImage = string.Empty;
            var sRSSUrl = string.Empty;
            var sRSSIconLink = string.Empty;
            if (this.Settings.RSSEnabled)
            {
                sRSSImage = "<img src=\"" + this.Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath + "feedicon.gif") + "\" border=\"0\" />";
                sRSSUrl = this.Page.ResolveUrl("~/desktopmodules/activeforumswhatsnew/feeds.aspx") + "?portalId=" + this.PortalId + "&tabid=" + this.TabId.ToString() + "&moduleid=" + this.ModuleId.ToString();
                sRSSIconLink = "<a href=\"" + sRSSUrl + "\">" + sRSSImage + "</a>";
            }

            var footer = this.Settings.Footer;

            footer = footer.Replace("[RSSICON]", sRSSImage);
            footer = footer.Replace("[RSSURL]", sRSSUrl);
            footer = footer.Replace("[RSSICONLINK]", sRSSIconLink);

            sb.Append(footer);

            this.Controls.Add(new LiteralControl(sb.ToString()));
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var stringWriter = new StringWriter();
            var htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);
            var html = stringWriter.ToString();
            html = Utilities.LocalizeControl(html, "WhatsNew");
            writer.Write(html);
        }

        #region Helper Methods

        private static string SafeSubstring(string source, int length)
        {
            return (source.Length <= length) ? source : source.Substring(0, length);
        }

        #endregion
    }
}
