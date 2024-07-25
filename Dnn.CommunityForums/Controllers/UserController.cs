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
//
namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Data;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Social.Notifications;

    internal static class UserController
    {
        private class ContentForUser
        {
            internal int ForumId { get; set; }

            internal int TopicId { get; set; }

            internal int ReplyId { get; set; }

            internal string Subject { get; set; }

            internal DateTime DateUpdated { get; set; }
        }

        internal static void BanUser(int portalId, int tabId, int moduleId, string moduleTitle, int forumId, int topicId, int replyId, DotNetNuke.Entities.Users.UserInfo bannedBy, int authorId)
        {
            if (authorId > -1)
            {
                DotNetNuke.Entities.Users.UserInfo bannedUser = DotNetNuke.Entities.Users.UserController.GetUserById(portalId: portalId, userId: authorId);
                string sBody = string.Empty;
                string authorName = string.Empty;
                string sSubject = string.Empty;
                if (replyId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(replyId);
                    sBody = reply.Content.Body;
                    sSubject = reply.Content.Subject;
                    authorName = reply.Author.DisplayName;
                }
                else
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                    sBody = topic.Content.Body;
                    sSubject = topic.Content.Subject;
                    authorName = topic.Author.DisplayName;
                }

                string notificationSubject = Utilities.GetSharedResource("[RESX:BanAlertSubject]");
                notificationSubject = notificationSubject.Replace("[Username]", bannedUser.Username);
                notificationSubject = notificationSubject.Replace("[DisplayName]", bannedUser.DisplayName);
                notificationSubject = notificationSubject.Replace("[BannedBy]", bannedBy.DisplayName);
                string body = Utilities.GetSharedResource("[RESX:BanAlertBody]");
                body = body.Replace("[Subject]", sSubject);

                StringBuilder postsRemoved = new StringBuilder();

                var contentForBannedUser = DataContext.Instance().ExecuteQuery<ContentForUser>(System.Data.CommandType.StoredProcedure, "activeforums_Content_GetJournalKeysForUser", bannedUser.UserID, moduleId).ToList();
                string objectKey;
                contentForBannedUser.ForEach(c =>
                {
                    objectKey = c.ReplyId < 1 ? $"{c.ForumId}:{c.TopicId}" : $"{c.ForumId}:{c.TopicId}:{c.ReplyId}";
                    if (JournalController.Instance.GetJournalItemByKey(portalId, objectKey) != null)
                    {
                        JournalController.Instance.DeleteJournalItemByKey(portalId, objectKey);
                    }

                    postsRemoved.AppendLine($"{Utilities.GetUserFriendlyDateTimeString(c.DateUpdated, moduleId, bannedBy)}\t{c.Subject}");
                    DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.RemoveModerationNotifications(tabId, moduleId, c.ForumId, c.TopicId, c.ReplyId);
                });
                body = body.Replace("[PostsRemoved]", postsRemoved.ToString());

                Notification notification = new Notification();
                notification.NotificationTypeID = NotificationsController.Instance.GetNotificationType(Globals.BanUserNotificationType).NotificationTypeId;
                notification.Subject = notificationSubject;
                notification.Body = body;
                notification.IncludeDismissAction = true;
                notification.SenderUserID = bannedBy.UserID;
                notification.Context = DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.BuildNotificationContextKey(tabId, moduleId, forumId, topicId, replyId);

                var modRoles = DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.GetModeratorRoles(portalId, moduleId, forumId);
                NotificationsController.Instance.SendNotification(notification, portalId, modRoles, null);

                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", moduleTitle));
                string userBannedMsg = String.Format(Utilities.GetSharedResource("[RESX:UserBanned]"), bannedUser.Username);
                log.AddProperty("Message", userBannedMsg);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);

                DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_Delete_For_User(moduleId: moduleId, userId: bannedUser.UserID, delBehavior: SettingsBase.GetModuleSettings(moduleId).DeleteBehavior);
                bannedUser.Membership.Approved = false;
                DotNetNuke.Entities.Users.UserController.UpdateUser(portalId: portalId, user: bannedUser, loggedAction: true);
                DataCache.CacheClearPrefix(moduleId, string.Format("AF-FV-{0}-{1}", portalId, moduleId));
            }
        }
    }
}
