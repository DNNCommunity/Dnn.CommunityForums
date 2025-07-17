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

    using DotNetNuke.Collections;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Social.Notifications;

    internal partial class UserBadgeController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo>
    {
        private readonly int moduleId = -1;
        private readonly int portalId = -1;

        internal override string cacheKeyTemplate => CacheKeys.UserBadgeInfo;

        internal UserBadgeController()
        {
        }

        internal UserBadgeController(int portalId, int moduleId)
        {
            this.portalId = portalId;
            this.moduleId = moduleId;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo GetById(int id)
        {
            var cachekey = this.GetCacheKey(moduleId: this.moduleId, id: id);
            DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo UserBadge = DataCache.ContentCacheRetrieve(this.moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo;
            if (UserBadge == null)
            {
                UserBadge = this.GetById(id, this.moduleId);
                if (UserBadge != null)
                {
                    UserBadge.ModuleId = this.moduleId;
                    UserBadge.PortalId = this.portalId;
                    UserBadge.GetBadge();
                    UserBadge.GetForumUser();
                }

                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, UserBadge);
            }

            return UserBadge;
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetForUser(int userId)
        {
            var cachekey = string.Format(CacheKeys.UserBadges, this.moduleId, userId);
            var UserBadges = (IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo>)DataCache.ContentCacheRetrieve(this.moduleId, cachekey);

            if (UserBadges == null)
            {
                UserBadges = this.Find("WHERE UserId = @0", userId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, UserBadges);
            }

            return UserBadges;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo GetForUserAndBadge(int userId, int badgeId)
        {
            return this.Find("WHERE UserId = @0 AND BadgeId = @1", userId, badgeId).FirstOrDefault();
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetForBadge(int badgeId)
        {
            var cachekey = string.Format(CacheKeys.BadgeUsers, this.moduleId, badgeId);
            var UserBadges = (IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo>)DataCache.ContentCacheRetrieve(this.moduleId, cachekey);

            if (UserBadges == null)
            {
                UserBadges = this.Find("WHERE BadgeId = @0", badgeId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, UserBadges);
            }

            return UserBadges;
        }

        public int BadgeCount(int badgeId)
        {
            var cachekey = string.Format(CacheKeys.BadgeUserCount, this.moduleId, badgeId);
            var count = (int?)DataCache.ContentCacheRetrieve(this.moduleId, cachekey);
            if (count == null)
            {
                count = this.Count("WHERE BadgeId = @0 ", badgeId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, count);
            }

            return (int)count;
        }

        public static void AssignUserBadge(int portalId, int moduleId, int userId, int badgeId, string requestUrl)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(
                processType: ProcessType.BadgeAssigned,
                portalId: portalId,
                tabId: DotNetNuke.Common.Utilities.Null.NullInteger,
                moduleId: moduleId,
                forumGroupId: DotNetNuke.Common.Utilities.Null.NullInteger,
                forumId: DotNetNuke.Common.Utilities.Null.NullInteger,
                topicId: DotNetNuke.Common.Utilities.Null.NullInteger,
                replyId: DotNetNuke.Common.Utilities.Null.NullInteger,
                contentId: DotNetNuke.Common.Utilities.Null.NullInteger,
                authorId: DotNetNuke.Common.Utilities.Null.NullInteger,
                userId: userId,
                badgeId: badgeId,
                requestUrl: requestUrl);
        }

        public static void UnassignUserBadge(int portalId, int moduleId, int userId, int badgeId, string requestUrl)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(
                processType: ProcessType.BadgeUnassigned,
                portalId: portalId,
                tabId: DotNetNuke.Common.Utilities.Null.NullInteger,
                moduleId: moduleId,
                forumGroupId: DotNetNuke.Common.Utilities.Null.NullInteger,
                forumId: DotNetNuke.Common.Utilities.Null.NullInteger,
                topicId: DotNetNuke.Common.Utilities.Null.NullInteger,
                replyId: DotNetNuke.Common.Utilities.Null.NullInteger,
                contentId: DotNetNuke.Common.Utilities.Null.NullInteger,
                authorId: DotNetNuke.Common.Utilities.Null.NullInteger,
                userId: userId,
                badgeId: badgeId,
                requestUrl: requestUrl);
        }

        public bool AssignUserBadgeAfterAction(int userId, int badgeId, string requestUrl)
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo UserBadge = this.Find("WHERE BadgeId = @0 AND UserId = @1", badgeId, userId).FirstOrDefault();
                if (UserBadge == null)
                {
                    UserBadge = new DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo
                    {
                        BadgeId = badgeId,
                        UserId = userId,
                        PortalId = this.portalId,
                        ModuleId = this.moduleId,
                        DateAssigned = DateTime.UtcNow,
                    };
                    this.Insert(UserBadge);
                }

                // Check if badge notifications should be sent to the user.
                // Conditions:
                // - The badge is configured to send award notifications.
                // - The forum user has enabled badge notifications.
                // - Either notifications are not suppressed for backfill, or the user account is less than 30 days old.
                if (UserBadge.ForumUser.BadgeNotificationsEnabled
                    && UserBadge.Badge.SendAwardNotification
                    && (!UserBadge.Badge.SuppresssAwardNotificationOnBackfill
                        || (DateTime.UtcNow - UserBadge.ForumUser.DateCreated.Value).TotalDays < 30))
                {
                    var subject = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(this.moduleId, Enums.TemplateType.BadgeNotificationSubject, SettingsBase.GetModuleSettings(this.moduleId).ForumFeatureSettings.TemplateFileNameSuffix);
                    subject = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceBadgeTokens(new StringBuilder(subject), UserBadge, UserBadge.ForumUser.PortalSettings, UserBadge.ForumUser.MainSettings, new Services.URLNavigator().NavigationManager(), UserBadge.ForumUser, string.IsNullOrEmpty(requestUrl) ? null : new Uri(requestUrl), string.IsNullOrEmpty(requestUrl) ? string.Empty : new Uri(requestUrl).PathAndQuery).ToString();
                    subject = subject.Length > 400 ? subject.Substring(0, 400) : subject;
                    var body = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(this.moduleId, Enums.TemplateType.BadgeNotificationBody, SettingsBase.GetModuleSettings(this.moduleId).ForumFeatureSettings.TemplateFileNameSuffix);
                    body = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceBadgeTokens(new StringBuilder(body), UserBadge, UserBadge.ForumUser.PortalSettings, UserBadge.ForumUser.MainSettings, new Services.URLNavigator().NavigationManager(), UserBadge.ForumUser, string.IsNullOrEmpty(requestUrl) ? null : new Uri(requestUrl), string.IsNullOrEmpty(requestUrl) ? string.Empty : new Uri(requestUrl).PathAndQuery).ToString();

                    string notificationKey = BuildNotificationContextKey(this.portalId, this.moduleId, badgeId, userId);

                    NotificationType notificationType = NotificationsController.Instance.GetNotificationType(Globals.BadgeNotificationType);
                    Notification notification = new Notification
                    {
                        NotificationTypeID = notificationType.NotificationTypeId,
                        Subject = subject,
                        Body = body,
                        IncludeDismissAction = true,
                        SenderUserID = userId,
                        Context = notificationKey,
                    };
                    var users = new List<DotNetNuke.Entities.Users.UserInfo> { UserBadge.ForumUser.UserInfo };
                    NotificationsController.Instance.SendNotification(notification, this.portalId, null, users);
                }

                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(this.moduleId, string.Format(CacheKeys.UserBadgeInfo, this.moduleId, UserBadge.UserBadgeId));
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(this.moduleId, string.Format(CacheKeys.UserBadges, this.moduleId, userId));
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(this.moduleId, string.Format(CacheKeys.BadgeUsers, this.moduleId, badgeId));
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(this.moduleId, string.Format(CacheKeys.BadgeUserCount, this.moduleId, badgeId));
                return true;
            }
            catch (Exception e)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(e);
                return false;
            }
        }

        public bool UnassignUserBadgeAfterAction(int userId, int badgeId)
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo UserBadge = this.Find("WHERE BadgeId = @0 AND UserId = @1", badgeId, userId).FirstOrDefault();
                if (UserBadge != null)
                {
                    this.DeleteById(UserBadge.UserBadgeId);
                }

                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(this.moduleId, string.Format(CacheKeys.UserBadgeInfo, this.moduleId, UserBadge.UserBadgeId));
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(this.moduleId, string.Format(CacheKeys.UserBadges, this.moduleId, userId));
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(this.moduleId, string.Format(CacheKeys.BadgeUsers, this.moduleId, badgeId));
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(this.moduleId, string.Format(CacheKeys.BadgeUserCount, this.moduleId, badgeId));
                return true;
            }
            catch (Exception e)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(e);
                return false;
            }
        }

        internal static string BuildNotificationContextKey(int portalId, int moduleId, int badgeId, int userId)
        {
            return $"{portalId}:{moduleId}:{badgeId}:{userId}";
        }
    }
}
