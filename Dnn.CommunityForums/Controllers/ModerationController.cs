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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;

    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Social.Notifications;

    internal class ModerationController
    {
        internal static void RemoveModerationNotifications(int tabId, int moduleId, int forumId, int topicId, int replyId)
        {
            NotificationType notificationType = NotificationsController.Instance.GetNotificationType(Globals.ModerationNotificationType);
            string notificationKey = BuildNotificationContextKey(tabId, moduleId, forumId, topicId, replyId);
            foreach (Notification notification in NotificationsController.Instance.GetNotificationByContext(notificationType.NotificationTypeId, notificationKey))
            {
                NotificationsController.Instance.DeleteNotification(notification.NotificationID);
            }
        }

        internal static string BuildNotificationContextKey(int tabId, int moduleId, int forumId, int topicId, int replyId)
        {
            return $"{tabId}:{moduleId}:{forumId}:{topicId}:{replyId}";
        }

        internal static bool SendModerationNotification(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int AuthorId, string requestUrl)
        {
            var portalSettings = Utilities.GetPortalSettings(portalId);
            var mainSettings = SettingsBase.GetModuleSettings(moduleId);
            try
            {
                int authorId;
                string subject;
                string body;
                if (replyId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(replyId);
                    subject = Utilities.GetSharedResource("NotificationSubjectReply");
                    subject = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(new StringBuilder(subject), reply, portalSettings, mainSettings, new Services.URLNavigator().NavigationManager(), new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId).GetUserFromHttpContext(portalId, moduleId), HttpContext.Current.Request).ToString();
                    body = Utilities.GetSharedResource("NotificationBodyReply");
                    body = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(new StringBuilder(body), reply, portalSettings, mainSettings, new Services.URLNavigator().NavigationManager(), new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId).GetUserFromHttpContext(portalId, moduleId), HttpContext.Current.Request).ToString();
                    authorId = reply.Content.AuthorId;
                }
                else
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                    subject = Utilities.GetSharedResource("NotificationSubjectTopic");
                    subject = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(new StringBuilder(subject), topic, portalSettings, mainSettings, new Services.URLNavigator().NavigationManager(), new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId).GetUserFromHttpContext(portalId, moduleId), HttpContext.Current.Request).ToString();
                    body = Utilities.GetSharedResource("NotificationBodyTopic");
                    body = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(new StringBuilder(body), topic, portalSettings, mainSettings, new Services.URLNavigator().NavigationManager(), new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId).GetUserFromHttpContext(portalId, moduleId), HttpContext.Current.Request).ToString();
                    authorId = topic.Content.AuthorId;
                }

                string notificationKey = BuildNotificationContextKey(tabId, moduleId, forumId, topicId, replyId);

                NotificationType notificationType = NotificationsController.Instance.GetNotificationType(Globals.ModerationNotificationType);
                Notification notification = new Notification
                {
                    NotificationTypeID = notificationType.NotificationTypeId,
                    Subject = subject,
                    Body = body,
                    IncludeDismissAction = false,
                    SenderUserID = authorId,
                    Context = notificationKey,
                };

                var modRoles = DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.GetModeratorRoles(portalId, moduleId, forumId);
                NotificationsController.Instance.SendNotification(notification, portalId, modRoles, null);
                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }

        internal static List<DotNetNuke.Entities.Users.UserInfo> GetListOfModerators(int portalId, int moduleId, int forumId)
        {
            var rp = RoleProvider.Instance();
            var uc = new DotNetNuke.Entities.Users.UserController();
            var mods = new List<DotNetNuke.Entities.Users.UserInfo>();
            foreach (var role in DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.GetModeratorRoles(portalId, moduleId, forumId))
            {
                foreach (DotNetNuke.Entities.Users.UserRoleInfo usr in rp.GetUserRoles(portalId, null, role.RoleName))
                {
                    var ui = uc.GetUser(portalId, usr.UserID);
                    if (!mods.Contains(ui))
                    {
                        mods.Add(ui);
                    }
                }
            }

            return mods;
        }

        internal static List<DotNetNuke.Security.Roles.RoleInfo> GetModeratorRoles(int portalId, int moduleId, int forumId)
        {
            var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: forumId, moduleId: moduleId);
            if (fi == null)
            {
                return null;
            }

            var modRoles = new List<DotNetNuke.Security.Roles.RoleInfo>();
            foreach (var r in fi.Security.Moderate.Split('|')[0].Split(';'))
            {
                if (!string.IsNullOrEmpty(r))
                {
                    var rid = Convert.ToInt32(r);
                    modRoles.Add(DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(portalId, rid));
                }
            }

            return modRoles;
        }
    }
}
