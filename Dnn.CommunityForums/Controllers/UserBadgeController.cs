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

        internal UserBadgeController(int portalId, int moduleId)
        {
            this.portalId = portalId;
            this.moduleId = moduleId;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo GetById(int id)
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

        internal IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetForUser(int userId)
        {
            var cachekey = string.Format(CacheKeys.UserBadges, this.moduleId, userId);
            var UserBadges = (IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo>)DataCache.ContentCacheRetrieve(this.moduleId, cachekey);

            if (UserBadges == null)
            {
                UserBadges = this.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2", this.portalId, this.moduleId, userId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, UserBadges);
            }

            return UserBadges;
        }

        internal IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetDistinctForUser(int userId)
        {
            var cachekey = string.Format(CacheKeys.UserBadges, this.moduleId, userId);
            var distinctUserBadges = (IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo>)DataCache.ContentCacheRetrieve(this.moduleId, cachekey);

            if (distinctUserBadges == null)
            {
                distinctUserBadges = this.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2", this.portalId, this.moduleId, userId)
                    .GroupBy(b => b.BadgeId)
                    .Select(g => g.OrderByDescending(b => b.DateAssigned).First())
                    .ToList();
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, distinctUserBadges);
            }

            return distinctUserBadges;
        }

        internal IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetForUserAndBadge(int userId, int badgeId)
        {
            return this.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND BadgeId = @3", this.portalId, this.moduleId, userId, badgeId);
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo GetForUserAndBadgeAndDateAssigned(int userId, int badgeId, DateTime dateAssigned)
        {
            return this.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND BadgeId = @3 AND DateAssigned = @4", this.portalId, this.moduleId, userId, badgeId, dateAssigned).FirstOrDefault();
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo GetLatestForUserAndBadge(int userId, int badgeId)
        {
            return this.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND BadgeId = @3", this.portalId, this.moduleId, userId, badgeId).OrderByDescending(b => b.DateAssigned).FirstOrDefault();
        }

        internal IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetForBadge(int badgeId)
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

        internal int BadgeCount(int userId, int badgeId)
        {
            var cachekey = string.Format(CacheKeys.BadgeUserCount, this.moduleId, badgeId, userId);
            var count = (int?)DataCache.ContentCacheRetrieve(this.moduleId, cachekey);
            if (count == null)
            {
                count = this.Count("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND BadgeId = @3", this.portalId, this.moduleId, userId, badgeId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, count);
            }

            return (int)count;
        }

        internal static void AssignUserBadge(int portalId, int moduleId, int userId, int badgeId, string requestUrl)
        {
            try
            {
                var award = true;
                var userBadgeController = new DotNetNuke.Modules.ActiveForums.Controllers.UserBadgeController(portalId: portalId, moduleId: moduleId);
                DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo userBadge = null;
                var badge = new DotNetNuke.Modules.ActiveForums.Controllers.BadgeController().GetById(badgeId);
                if (badge.OneTimeAward)
                {
                    userBadge = userBadgeController.GetLatestForUserAndBadge(userId: userId, badgeId: badgeId);
                    if (userBadge != null)
                    {
                        // If the badge is one-time award and already assigned, do not reassign.
                        award = false;
                    }
                }

                if (award)
                {
                    userBadge = new DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo
                    {
                        BadgeId = badgeId,
                        UserId = userId,
                        PortalId = portalId,
                        ModuleId = moduleId,
                        DateAssigned = DateTime.UtcNow,
                    };
                    userBadgeController.Insert(userBadge);
                    ClearBadgeCache(userBadge);

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
                    dateCreated: userBadge.DateAssigned,
                    requestUrl: requestUrl);
                }
            }
            catch (Exception e)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(e);
            }
        }

        private static void ClearBadgeCache(DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo userBadge)
        {
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(userBadge.ModuleId, string.Format(CacheKeys.UserBadgeInfo, userBadge.ModuleId, userBadge.UserBadgeId));
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(userBadge.ModuleId, string.Format(CacheKeys.UserBadges, userBadge.ModuleId, userBadge.UserId));
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(userBadge.ModuleId, string.Format(CacheKeys.BadgeUsers, userBadge.ModuleId, userBadge.BadgeId));
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(userBadge.ModuleId, string.Format(CacheKeys.BadgeUserCount, userBadge.ModuleId, userBadge.BadgeId, userBadge.UserId));
        }

        internal void UnassignUserBadge(int portalId, int userId, int badgeId, DateTime dateAssigned)
        {
            try
            {
                var userBadge = this.GetForUserAndBadgeAndDateAssigned(userId: userId, badgeId: badgeId, dateAssigned: dateAssigned);
                if (userBadge != null)
                {
                    ClearBadgeCache(userBadge);
                    this.DeleteById(userBadge.UserBadgeId);
                }
            }
            catch (Exception e)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(e);
            }
        }

        internal bool AssignUserBadgeAfterAction(int portalId, int userId, int badgeId, DateTime dateAssigned, string requestUrl)
        {
            try
            {
                var userBadge = this.GetForUserAndBadgeAndDateAssigned(userId: userId, badgeId: badgeId, dateAssigned: dateAssigned);
                if (userBadge == null)
                {
                    // If the user badge does not exist, cannot proceed but return true to indicate no error occurred; we just won't send a notification but need to clear the process action from the queue.
                    return true;
                }

                // Check if badge notifications should be sent to the user.
                // Conditions:
                // - The badge is configured to send award notifications.
                // - The forum user has enabled badge notifications.
                // - Either notifications are not suppressed for backfill, or user was assigned badge after backfill completed date.
                if (userBadge.ForumUser.BadgeNotificationsEnabled
                    && userBadge.Badge.SendAwardNotification
                    && (!userBadge.Badge.SuppresssAwardNotificationOnBackfill
                        || (userBadge.Badge.InitialBackfillCompletedDate.HasValue && userBadge.DateAssigned > userBadge.Badge.InitialBackfillCompletedDate.Value)))
                {
                    var subject = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(this.moduleId, Enums.TemplateType.BadgeNotificationSubject, SettingsBase.GetModuleSettings(this.moduleId).DefaultFeatureSettings.TemplateFileNameSuffix);
                    subject = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceBadgeTokens(new StringBuilder(subject), userBadge, userBadge.ForumUser.PortalSettings, userBadge.ForumUser.ModuleSettings, new Services.URLNavigator().NavigationManager(), userBadge.ForumUser, string.IsNullOrEmpty(requestUrl) ? null : new Uri(requestUrl), string.IsNullOrEmpty(requestUrl) ? string.Empty : new Uri(requestUrl).PathAndQuery).ToString();
                    subject = subject.Length > 400 ? subject.Substring(0, 400) : subject;
                    var body = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(this.moduleId, Enums.TemplateType.BadgeNotificationBody, SettingsBase.GetModuleSettings(this.moduleId).DefaultFeatureSettings.TemplateFileNameSuffix);
                    body = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceBadgeTokens(new StringBuilder(body), userBadge, userBadge.ForumUser.PortalSettings, userBadge.ForumUser.ModuleSettings, new Services.URLNavigator().NavigationManager(), userBadge.ForumUser, string.IsNullOrEmpty(requestUrl) ? null : new Uri(requestUrl), string.IsNullOrEmpty(requestUrl) ? string.Empty : new Uri(requestUrl).PathAndQuery).ToString();

                    string notificationKey = BuildNotificationContextKey(this.portalId, this.moduleId, badgeId, userId, userBadge.DateAssigned);

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
                    var users = new List<DotNetNuke.Entities.Users.UserInfo> { userBadge.ForumUser.UserInfo };
                    NotificationsController.Instance.SendNotification(notification, this.portalId, null, users);
                }

                return true;
            }
            catch (Exception e)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(e);
                return false;
            }
        }

        private static string BuildNotificationContextKey(int portalId, int moduleId, int badgeId, int userId, DateTime dateAssigned)
        {
            return $"{portalId}:{moduleId}:{badgeId}:{userId}:{dateAssigned}";
        }
    }
}
