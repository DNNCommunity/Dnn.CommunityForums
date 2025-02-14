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
    using System.Web.UI.WebControls;

    using DotNetNuke.Collections;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Social.Notifications;

    internal partial class LikeController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.LikeInfo>
    {
        private readonly int moduleId = -1;
        private readonly int portalId = -1;

        internal override string cacheKeyTemplate => CacheKeys.LikeInfo;

        internal LikeController()
        {
        }

        internal LikeController(int portalId, int moduleId)
        {
            this.portalId = portalId;
            this.moduleId = moduleId;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.LikeInfo GetById(int id)
        {
            var cachekey = this.GetCacheKey(moduleId: this.moduleId, id: id);
            DotNetNuke.Modules.ActiveForums.Entities.LikeInfo like = DataCache.ContentCacheRetrieve(this.moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.LikeInfo;
            if (like == null)
            {
                like = this.GetById(id, this.moduleId);
                if (like != null)
                {
                    like.ModuleId = this.moduleId;
                    like.PortalId = this.portalId;
                    like.GetContent();
                    like.Content?.GetPost();
                }

                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, like);
            }

            return like;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.LikeInfo GetForUser(int userId, int postId)
        {
            var cachekey = string.Format(CacheKeys.LikedByUser, this.moduleId, postId, userId);
            var like = (DotNetNuke.Modules.ActiveForums.Entities.LikeInfo)DataCache.ContentCacheRetrieve(this.moduleId, cachekey);

            if (like == null)
            {
                like = this.Find("WHERE PostId = @0 AND UserId = @1 AND Checked = 1", postId, userId).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, like);
            }

            return like;
        }

        public bool GetLikedByUser(int userId, int postId)
        {
            var like = this.GetForUser(userId: userId, postId: postId);
            if (like == null)
            {
                return false;
            }
            else { return like.Checked; }
        }

        public (int count, bool liked) Get(int userId, int postId)
        {
            return (this.Count(postId), this.GetLikedByUser(userId, postId));
        }

        public List<DotNetNuke.Modules.ActiveForums.Entities.LikeInfo> GetForPost(int postId)
        {
            return this.Find("WHERE PostId = @0 AND Checked = 1", postId).ForEach(l =>
            {
                l.PortalId = this.portalId;
                l.ModuleId = this.moduleId;
                l.GetContent();
                l.Content?.GetPost();
            }).ToList();
        }

        public int Count(int postId)
        {
            var cachekey = string.Format(CacheKeys.LikeCount, this.moduleId, postId);
            var count = (int?)DataCache.ContentCacheRetrieve(this.moduleId, cachekey);
            if (count == null)
            {
                count = this.Count("WHERE PostId = @0 AND Checked = 1", postId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, count);
            }

            return (int)count;
        }

        public int Like(int contentId, int userId, int authorId, int tabId, int forumGroupId, int forumId, int replyId, int topicId, string requestUrl)
        {
            DotNetNuke.Modules.ActiveForums.Entities.LikeInfo like = this.Find("WHERE PostId = @0 AND UserId = @1", contentId, userId).FirstOrDefault();
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

                this.Update(like);
            }
            else
            {
                like = new DotNetNuke.Modules.ActiveForums.Entities.LikeInfo
                {
                    PostId = contentId,
                    UserId = userId,
                    Checked = true,
                };
                this.Insert(like);
                new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.PostLiked,
                                                                                             portalId: this.portalId,
                                                                                             tabId: tabId,
                                                                                             moduleId: this.moduleId,
                                                                                             forumGroupId: forumGroupId,
                                                                                             forumId: forumId,
                                                                                             topicId: topicId,
                                                                                             replyId: replyId,
                                                                                             contentId: contentId,
                                                                                             authorId: authorId,
                                                                                             userId: userId,
                                                                                             requestUrl: requestUrl);
            }

            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(this.moduleId, string.Format(CacheKeys.LikeInfo, this.moduleId, like.Id));
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(this.moduleId, string.Format(CacheKeys.LikeCount, this.moduleId, contentId));
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(this.moduleId, string.Format(CacheKeys.LikedByUser, this.moduleId, contentId, userId));
            return this.Count(contentId);
        }

        internal static bool ProcessPostLiked(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId, string requestUrl)
        {
            try
            {
                var like = new DotNetNuke.Modules.ActiveForums.Controllers.LikeController(portalId, moduleId).GetForUser(userId: userId, postId: contentId);
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

#pragma warning disable SA1403 // File may only contain a single namespace
namespace DotNetNuke.Modules.ActiveForums
#pragma warning restore SA1403 // File may only contain a single namespace
{
    using System;
    using System.Collections.Generic;

    [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Replace with DotNetNuke.Modules.ActiveForums.Controllers.LikeController")]
#pragma warning disable SA1400 // Access modifier should be declared
    class LikesController : DotNetNuke.Modules.ActiveForums.Controllers.LikeController
#pragma warning restore SA1400 // Access modifier should be declared
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Replace with DotNetNuke.Modules.ActiveForums.Controllers.LikeController.GetForPost()")]
#pragma warning disable SA1600 // Elements should be documented
        public new List<DotNetNuke.Modules.ActiveForums.Likes> GetForPost(int postId) => throw new NotImplementedException();

#pragma warning restore SA1600 // Elements should be documented
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Replace with DotNetNuke.Modules.ActiveForums.Controllers.LikeController.Like()")]
#pragma warning disable SA1600 // Elements should be documented
        public new void Like(int contentId, int userId) => throw new NotImplementedException();
#pragma warning restore SA1600 // Elements should be documented
    }
}
