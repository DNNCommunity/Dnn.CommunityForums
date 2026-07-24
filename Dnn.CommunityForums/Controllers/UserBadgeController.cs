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
    using DotNetNuke.Modules.ActiveForums.Services.Cache;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Social.Notifications;
    
    /// <summary>
    /// Controller for managing badges in the DNN Community Forums module.
    /// </summary>
    internal class UserBadgeController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo, IUserBadgeController, UserBadgeController>, IUserBadgeController
    {
        protected override Func<IUserBadgeController> GetFactory()
        {
            return () => new UserBadgeController();
        }

        public DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo GetById(int portalId, int moduleId, int badgeId)
        {
            var cachekey = string.Format(CacheKeys.UserBadgeInfo, moduleId, badgeId);
            DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo userBadge = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo;
            if (userBadge == null)
            {
                userBadge = this._repositoryControllerBase.GetById(id: badgeId, scopeValue: moduleId);
                if (userBadge == null)
                {
                    userBadge = this._repositoryControllerBase.GetById(id: badgeId);
                }

                if (userBadge != null)
                {
                    userBadge.ModuleId = moduleId;
                    userBadge.PortalId = portalId;
                    userBadge.GetBadge();
                    userBadge.GetForumUser();
                }

                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, userBadge);
            }

            return userBadge;
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetForUser(int portalId, int moduleId, int userId)
        {
            var cachekey = string.Format(CacheKeys.UserBadges, moduleId, userId);
            var cached = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo>>;

            if (cached == null)
            {
                var userBadges = this._repositoryControllerBase.Find("WHERE PortalId = @0 AND (ModuleId = @1 OR @1 = -1) AND UserId = @2", portalId, moduleId, userId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, new DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo>>(userBadges, userBadges != null));
                return userBadges;
            }

            return cached.HasValue ? cached.Value : null;
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetDistinctForUser(int portalId, int moduleId, int userId)
        {
            var cachekey = string.Format(CacheKeys.UserBadgesDistinct, moduleId, userId);
            var cached = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo>>;

            if (cached == null)
            {
                var distinctUserBadges = this._repositoryControllerBase.Find("WHERE PortalId = @0 AND (ModuleId = @1 OR @1 = -1) AND UserId = @2", portalId, moduleId, userId)
                    .GroupBy(b => b.BadgeId)
                    .Select(g => g.OrderByDescending(b => b.DateAssigned).First())
                    .ToList();
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, new DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo>>(distinctUserBadges, distinctUserBadges != null));
                return distinctUserBadges;
            }

            return cached.HasValue ? cached.Value : null;
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetForUserAndBadge(int portalId, int moduleId, int userId, int badgeId)
        {
            return this._repositoryControllerBase.Find("WHERE PortalId = @0 AND (ModuleId = @1 OR @1 = -1) AND UserId = @2 AND BadgeId = @3", portalId, moduleId, userId, badgeId);
        }

        public DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo GetForUserAndBadgeAndDateAssigned(int portalId, int moduleId, int userId, int badgeId, DateTime dateAssigned)
        {
            return this._repositoryControllerBase.Find("WHERE PortalId = @0 AND (ModuleId = @1 OR @1 = -1) AND UserId = @2 AND BadgeId = @3 AND DateAssigned = @4", portalId, moduleId, userId, badgeId, dateAssigned).FirstOrDefault();
        }

        public DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo GetLatestForUserAndBadge(int portalId, int moduleId, int userId, int badgeId)
        {
            return this._repositoryControllerBase.Find("WHERE PortalId = @0 AND (ModuleId = @1 OR @1 = -1) AND UserId = @2 AND BadgeId = @3", portalId, moduleId, userId, badgeId).OrderByDescending(b => b.DateAssigned).FirstOrDefault();
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetForBadge(int moduleId, int badgeId)
        {
            var cachekey = string.Format(CacheKeys.BadgeUsers, moduleId, badgeId);
            var UserBadges = (IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo>)DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey);

            if (UserBadges == null)
            {
                UserBadges = this._repositoryControllerBase.Find("WHERE BadgeId = @0", badgeId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, UserBadges);
            }

            return UserBadges;
        }

        public int BadgeCount(int portalId, int moduleId, int userId, int badgeId)
        {
            var cachekey = string.Format(CacheKeys.BadgeUserCount, moduleId, badgeId, userId);
            var count = (int?)DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey);
            if (count == null)
            {
                count = this._repositoryControllerBase.Count("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND BadgeId = @3", portalId, moduleId, userId, badgeId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, count);
            }

            return (int)count;
        }

        public void AssignUserBadge(int portalId, int moduleId, int userId, int badgeId, string requestUrl)
        {
            try
            {
                var award = true;
                DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo userBadge = null;
                var badge = DotNetNuke.Modules.ActiveForums.Controllers.BadgeController.Instance.GetById(moduleId, badgeId);
                if (badge.OneTimeAward)
                {
                    userBadge = this.GetLatestForUserAndBadge(portalId: portalId, moduleId: moduleId, userId: userId, badgeId: badgeId);
                    if (userBadge != null)
                    {
                        // If the badge is one-time award and already assigned, do not reassign.
                        award = false;
                    }
                }

                if (award)
                {
                    var user = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance.GetByUserId(portalId, moduleId, userId); // this will create user if not exists
                    if (user == null)
                    {
                        DotNetNuke.Modules.ActiveForums.Exceptions.LogException(new ArgumentException($"User badge assigned for User: {userId} but could not be processed because user doesn't exist; skipping user badge award."));
                    }
                    else
                    {
                        userBadge = new DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo
                        {
                            BadgeId = badgeId,
                            UserId = userId,
                            PortalId = portalId,
                            ModuleId = moduleId,
                            DateAssigned = DateTime.UtcNow,
                        };
                        this._repositoryControllerBase.Insert(userBadge);
                        DotNetNuke.Modules.ActiveForums.Controllers.UserBadgeController.ClearBadgeCache(userBadge);

                        DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Instance.Add(
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
            }
            catch (Exception e)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(e);
            }
        }

        private static void ClearBadgeCache(DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo userBadge)
        {
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(userBadge.ModuleId, string.Format(CacheKeys.UserBadgeInfo, userBadge.ModuleId, userBadge.UserBadgeId));
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(userBadge.ModuleId, string.Format(CacheKeys.UserBadges, userBadge.ModuleId, userBadge.UserId));
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(userBadge.ModuleId, string.Format(CacheKeys.BadgeUsers, userBadge.ModuleId, userBadge.BadgeId));
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(userBadge.ModuleId, string.Format(CacheKeys.BadgeUserCount, userBadge.ModuleId, userBadge.BadgeId, userBadge.UserId));
        }

        public void UnassignUserBadge(int portalId, int moduleId, int userId, int badgeId, DateTime dateAssigned)
        {
            try
            {
                var userBadge = this.GetForUserAndBadgeAndDateAssigned(portalId: portalId, moduleId: moduleId, userId: userId, badgeId: badgeId, dateAssigned: dateAssigned);
                if (userBadge != null)
                {
                    this._repositoryControllerBase.DeleteById(userBadge.UserBadgeId);
                    ClearBadgeCache(userBadge);
                }
            }
            catch (Exception e)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(e);
            }
        }

        bool IUserBadgeController.AssignUserBadgeAfterAction(int portalId, int moduleId, int userId, int badgeId, DateTime dateAssigned, string requestUrl)
        {
            try
            {
                var userBadge = this.GetForUserAndBadgeAndDateAssigned(portalId: portalId, moduleId: moduleId, userId: userId, badgeId: badgeId, dateAssigned: dateAssigned);
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
                    var subject = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(moduleId, Enums.TemplateType.BadgeNotificationSubject, SettingsBase.GetModuleSettings(moduleId).DefaultFeatureSettings.TemplateFileNameSuffix, userBadge.ForumUser);
                    subject = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceBadgeTokens(new StringBuilder(subject), userBadge, userBadge.ForumUser.PortalSettings, userBadge.ForumUser.ModuleSettings, new Services.URLNavigator().NavigationManager(), userBadge.ForumUser, string.IsNullOrEmpty(requestUrl) ? null : new Uri(requestUrl), string.IsNullOrEmpty(requestUrl) ? string.Empty : new Uri(requestUrl).PathAndQuery).ToString();
                    subject = subject.Length > 400 ? subject.Substring(0, 400) : subject;
                    var body = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(moduleId, Enums.TemplateType.BadgeNotificationBody, SettingsBase.GetModuleSettings(moduleId).DefaultFeatureSettings.TemplateFileNameSuffix, userBadge.ForumUser);
                    body = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceBadgeTokens(new StringBuilder(body), userBadge, userBadge.ForumUser.PortalSettings, userBadge.ForumUser.ModuleSettings, new Services.URLNavigator().NavigationManager(), userBadge.ForumUser, string.IsNullOrEmpty(requestUrl) ? null : new Uri(requestUrl), string.IsNullOrEmpty(requestUrl) ? string.Empty : new Uri(requestUrl).PathAndQuery).ToString();

                    string notificationKey = BuildNotificationContextKey(portalId, moduleId, badgeId, userId, userBadge.DateAssigned);

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
                    NotificationsController.Instance.SendNotification(notification, portalId, null, users);
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
