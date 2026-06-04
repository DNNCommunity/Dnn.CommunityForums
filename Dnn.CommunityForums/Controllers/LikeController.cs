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
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Social.Notifications;

    internal class LikeController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.LikeInfo, ILikeController, LikeController>, ILikeController
    {
        protected override Func<ILikeController> GetFactory()
        {
            return () => new LikeController();
        }

        public DotNetNuke.Modules.ActiveForums.Entities.LikeInfo GetById(int portalId, int moduleId, int id)
        {
            var cachekey = string.Format(CacheKeys.LikeInfo, moduleId, id);
            DotNetNuke.Modules.ActiveForums.Entities.LikeInfo like = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.LikeInfo;
            if (like == null)
            {
                like = this._repositoryControllerBase.GetById(id, moduleId);
                if (like != null)
                {
                    like.ModuleId = moduleId;
                    like.PortalId = portalId;
                    like.GetContent();
                    like.Content?.GetPost();
                }

                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, like);
            }

            return like;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.LikeInfo GetForUser(int portalId, int moduleId, int userId, int postId)
        {
            var cacheKey = string.Format(CacheKeys.LikedByUser, moduleId, postId, userId);
            var cached = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cacheKey) as DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<LikeInfo>;

            if (cached == null)
            {
                var like = this._repositoryControllerBase.Find("WHERE PostId = @0 AND UserId = @1 AND Checked = 1", postId, userId).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cacheKey, new DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<LikeInfo>(like, like != null));

                return like;
            }

            return cached.HasValue ? cached.Value : null;
        }

        public bool GetLikedByUser(int portalId, int moduleId, int userId, int postId)
        {
            var like = this.GetForUser(portalId: portalId, moduleId: moduleId, userId: userId, postId: postId);
            if (like == null)
            {
                return false;
            }
            else
            {
                return like.Checked;
            }
        }

        public (int Count, bool Liked) Get(int portalId, int moduleId, int userId, int postId)
        {
            return (this.Count(moduleId, postId), this.GetLikedByUser(portalId, moduleId, userId, postId));
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.LikeInfo> GetForPost(int portalId, int moduleId, int postId)
        {
            return this._repositoryControllerBase.Find("WHERE PostId = @0 AND Checked = 1", postId).ForEach(l =>
            {
                l.PortalId = portalId;
                l.ModuleId = moduleId;
                l.GetContent();
                l.Content?.GetPost();
            }).ToList();
        }

        public int Count(int moduleId, int postId)
        {
            var cachekey = string.Format(CacheKeys.LikeCount, moduleId, postId);
            var count = (int?)DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey);
            if (count == null)
            {
                count = this._repositoryControllerBase.Count("WHERE PostId = @0 AND Checked = 1", postId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, count);
            }

            return (int)count;
        }

        public int Like(int portalId, int moduleId, int contentId, int userId, int authorId, int tabId, int forumGroupId, int forumId, int replyId, int topicId, string requestUrl)
        {
            DotNetNuke.Modules.ActiveForums.Entities.LikeInfo like = this._repositoryControllerBase.Find("WHERE PostId = @0 AND UserId = @1", contentId, userId).FirstOrDefault();
            if (like != null)
            {
                if (like.Checked)
                {
                    like.Checked = false;
                }
                else
                {
                    like.Checked = true;
                }

                this._repositoryControllerBase.Update(like);
            }
            else
            {
                like = new DotNetNuke.Modules.ActiveForums.Entities.LikeInfo
                {
                    PostId = contentId,
                    UserId = userId,
                    Checked = true,
                    DateCreated = DateTime.UtcNow,
                };
                this._repositoryControllerBase.Insert(like);
                DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Instance.Add(
                    ProcessType.PostLiked,
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
                    requestUrl: requestUrl,
                    dateCreated: like.DateCreated);
            }

            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(moduleId, string.Format(CacheKeys.LikeInfo, moduleId, like.Id));
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(moduleId, string.Format(CacheKeys.LikeCount, moduleId, contentId));
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(moduleId, string.Format(CacheKeys.LikedByUser, moduleId, contentId, userId));
            return this.Count(moduleId, contentId);
        }

        internal static bool ProcessPostLiked(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId, string requestUrl)
        {
            try
            {
                var like = LikeController.Instance.GetForUser(portalId: portalId, moduleId: moduleId, userId: userId, postId: contentId);
                if (like == null)
                {
                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    var message = string.Format(Utilities.GetSharedResource("[RESX:UnableToFindLikeToProcess]"), contentId, userId);
                    log.AddProperty("Message", message);
                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                    return true;
                }

                if ((bool)like.Author?.ForumUser?.LikeNotificationsEnabled)
                {
                    var subject = Utilities.GetSharedResource("[RESX:LikeNotificationSubject]");
                    subject = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceLikeTokens(new StringBuilder(subject), like, like.Forum.PortalSettings, like.Forum.MainSettings, new Services.URLNavigator().NavigationManager(), like.Author.ForumUser, new Uri(requestUrl), new Uri(requestUrl).PathAndQuery).ToString();
                    subject = subject.Length > 400 ? subject.Substring(0, 400) : subject;
                    var body = Utilities.GetSharedResource("[RESX:LikeNotificationBody]");
                    body = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceLikeTokens(new StringBuilder(body), like, like.Forum.PortalSettings, like.Forum.MainSettings, new Services.URLNavigator().NavigationManager(), like.Author.ForumUser, new Uri(requestUrl), new Uri(requestUrl).PathAndQuery).ToString();

                    string notificationKey = BuildNotificationContextKey(tabId, moduleId, contentId, userId);

                    NotificationType notificationType = NotificationsController.Instance.GetNotificationType(Globals.LikeNotificationType);
                    Notification notification = new Notification
                    {
                        NotificationTypeID = notificationType.NotificationTypeId,
                        Subject = subject,
                        Body = body,
                        IncludeDismissAction = true,
                        SenderUserID = userId,
                        Context = notificationKey,
                    };
                    var users = new List<DotNetNuke.Entities.Users.UserInfo> { like.Author.ForumUser.UserInfo };
                    NotificationsController.Instance.SendNotification(notification, portalId, null, users);
                }

                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }

        internal static string BuildNotificationContextKey(int tabId, int moduleId, int contentId, int userId)
        {
            return $"{tabId}:{moduleId}:{contentId}:{userId}";
        }
    }
}
