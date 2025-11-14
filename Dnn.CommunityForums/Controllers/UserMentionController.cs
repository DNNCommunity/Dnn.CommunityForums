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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Social.Notifications;

    internal partial class UserMentionController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.UserMentionInfo>
    {
        internal static void ProcessUserMentions(DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post)
        {
            const string Pattern = @"(<a href=\"".*?\/Activity-Feed\?userId=(?<userid>\d+?)\"">)";
            try
            {
                var userIds = new List<int>();
                var matches = RegexUtils.GetCachedRegex(Pattern, RegexOptions.Compiled & RegexOptions.IgnoreCase & RegexOptions.IgnorePatternWhitespace, 5).Matches(post.Content.Body);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        if (!string.IsNullOrEmpty(match.Groups["userid"]?.Value))
                        {
                            userIds.Add(Utilities.SafeConvertInt(match.Groups["userid"].Value));
                        }
                    }

                    userIds.Distinct().Where(u => u > 0).ToList().ForEach(u => {
                        UserMentioned(
                            portalId: post.PortalId,
                            moduleId: post.ModuleId,
                            tabId: post.Forum.GetTabId(),
                            forumGroupId: post.Forum.ForumGroupId,
                            forumId: post.ForumId,
                            topicId: post.TopicId,
                            replyId: post.ReplyId,
                            contentId: post.Content.ContentId,
                            authorId: post.Content.AuthorId,
                            userId: u,
                            requestUrl: post.RequestUri?.ToString() ?? string.Empty);
                    });
                }
            }
            catch (RegexMatchTimeoutException ex)
            {
                Exceptions.LogException(ex);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw;
            }
        }

        public static void UserMentioned(int portalId, int moduleId, int tabId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId, string requestUrl)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(
                processType: ProcessType.UserMentioned,
                portalId: portalId,
                tabId: tabId,
                moduleId: moduleId,
                forumGroupId: forumGroupId,
                forumId: forumId,
                topicId: topicId,
                replyId: replyId,
                contentId: contentId,
                authorId: authorId,
                userId: userId,
                badgeId: DotNetNuke.Common.Utilities.Null.NullInteger,
                requestUrl: requestUrl);
        }

        public bool UserMentionAfterAction(int portalId, int moduleId, int tabId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId, string requestUrl)
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.Entities.UserMentionInfo UserMention = this.Find("WHERE ContentId = @0 AND UserId = @1", contentId, userId).FirstOrDefault();
                if (UserMention == null)
                {
                    UserMention = new DotNetNuke.Modules.ActiveForums.Entities.UserMentionInfo
                    {
                        ContentId = contentId,
                        UserId = userId,
                        PortalId = portalId,
                        ModuleId = moduleId,
                        DateMentioned = DateTime.UtcNow,
                    };
                    this.Insert(UserMention);
                    if (UserMention.ForumUser.UserMentionNotificationsEnabled)
                    {
                        var subject = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(moduleId, Enums.TemplateType.UserMentionNotificationSubject, SettingsBase.GetModuleSettings(moduleId).DefaultFeatureSettings.TemplateFileNameSuffix);
                        subject = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(new StringBuilder(subject), UserMention.Content.Post, UserMention.ForumUser.PortalSettings, UserMention.ForumUser.ModuleSettings, new Services.URLNavigator().NavigationManager(), UserMention.ForumUser, new Uri(requestUrl), new Uri(requestUrl).PathAndQuery).ToString();
                        subject = subject.Length > 400 ? subject.Substring(0, 400) : subject;
                        var body = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(moduleId, Enums.TemplateType.UserMentionNotificationBody, SettingsBase.GetModuleSettings(moduleId).DefaultFeatureSettings.TemplateFileNameSuffix);
                        body = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(new StringBuilder(body), UserMention.Content.Post, UserMention.ForumUser.PortalSettings, UserMention.ForumUser.ModuleSettings, new Services.URLNavigator().NavigationManager(), UserMention.ForumUser, new Uri(requestUrl), new Uri(requestUrl).PathAndQuery).ToString();

                        string notificationKey = BuildNotificationContextKey(portalId, moduleId, contentId, userId);

                        NotificationType notificationType = NotificationsController.Instance.GetNotificationType(Globals.UserMentionNotificationType);
                        Notification notification = new Notification
                        {
                            NotificationTypeID = notificationType.NotificationTypeId,
                            Subject = subject,
                            Body = body,
                            IncludeDismissAction = true,
                            SenderUserID = authorId,
                            Context = notificationKey,
                        };
                        var users = new List<DotNetNuke.Entities.Users.UserInfo> { UserMention.ForumUser.UserInfo };
                        NotificationsController.Instance.SendNotification(notification, portalId, null, users);
                    }

                    DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.UserMentionInfo, moduleId, UserMention.UserMentionId));
                }

                return true;
            }
            catch (Exception e)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(e);
                return false;
            }
        }

        internal static string BuildNotificationContextKey(int portalId, int moduleId, int ContentId, int userId)
        {
            return $"{portalId}:{moduleId}:{ContentId}:{userId}";
        }
    }
}
