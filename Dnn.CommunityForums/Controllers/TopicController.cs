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
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Social.Notifications;

    internal class TopicController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>
    {
        private int moduleId = -1;

        internal override string cacheKeyTemplate => CacheKeys.TopicInfo;

        internal TopicController(int moduleId)
        {
            this.moduleId = moduleId;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetById(int topicId)
        {
            var cachekey = this.GetCacheKey(moduleId: this.moduleId, id: topicId);
            var ti = DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheRetrieve(this.moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.TopicInfo;
            if (ti == null)
            {
                ti = base.GetById(topicId);
            }

            if (ti != null)
            {
                ti.ModuleId = this.moduleId;
                ti.Forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ti.ForumId, this.moduleId);

                if (ti.Forum != null)
                {
                    ti.PortalId = ti.Forum.PortalId;
                }

                ti.GetContent();
                if (ti.Content != null)
                {
                    ti.Author = ti.GetAuthor(ti.PortalId, ti.ModuleId, ti.Content.AuthorId);
                }

                if (ti.LastReply != null)
                {
                    ti.LastReplyAuthor = ti.GetAuthor(ti.PortalId, ti.ModuleId, ti.LastReply.Content.AuthorId);
                }
            }

            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, ti);
            return ti;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetByContentId(int contentId)
        {
            var cachekey = string.Format(CacheKeys.TopicInfoByContentId, this.moduleId, contentId);
            var ti = DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheRetrieve(this.moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.TopicInfo;
            if (ti == null)
            {
                ti = this.Find("WHERE ContentId = @0", contentId).FirstOrDefault();
            }

            if (ti != null)
            {
                ti.ModuleId = this.moduleId;
                ti.Forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ti.ForumId, this.moduleId);
                if (ti.Forum != null)
                {
                    ti.PortalId = ti.Forum.PortalId;
                }

                ti.GetContent();
                if (ti.Content != null)
                {
                    ti.Author = ti.GetAuthor(ti.PortalId, ti.ModuleId, ti.Content.AuthorId);
                }

                if (ti.LastReply != null)
                {
                    ti.LastReplyAuthor = ti.GetAuthor(ti.PortalId, ti.ModuleId, ti.LastReply.Content.AuthorId);
                }
            }

            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, ti);
            return ti;
        }

        public static int QuickCreate(int portalId, int moduleId, int forumId, string subject, string body, int userId, string displayName, bool isApproved, string iPAddress)
        {
            var forumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId, moduleId);
            int topicId = -1;
            var ti = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo
            {
                PortalId = portalId,
                ModuleId = moduleId,
                ForumId = forumId,
                AnnounceEnd = Utilities.NullDate(),
                AnnounceStart = Utilities.NullDate(),
                IsAnnounce = false,
                IsApproved = isApproved,
                IsArchived = false,
                IsDeleted = false,
                IsLocked = false,
                IsPinned = false,
                ReplyCount = 0,
                StatusId = -1,
                TopicIcon = string.Empty,
                TopicType = TopicTypes.Topic,
                ViewCount = 0,
                TopicUrl = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildTopicUrlSegment(portalId: portalId, moduleId: moduleId, topicId: topicId, subject: subject, forumInfo: forumInfo),
                Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo()
                {
                    AuthorId = userId,
                    AuthorName = displayName,
                    Subject = subject,
                    Body = body,
                    Summary = string.Empty,
                    IPAddress = iPAddress,
                },
            };

            topicId = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
            if (topicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(moduleId, forumId, topicId, -1);
            }

            return topicId;
        }

        public static void Replies_Split(int moduleId, int oldForumId, int newForumId, int oldTopicId, int newTopicId, string listreplies, bool isNew)
        {
            if (oldTopicId > 0 && newTopicId > 0)
            {
                var pattern = @"^\d+(\|\d+)*$";
                if (RegexUtils.GetCachedRegex(pattern).IsMatch(listreplies))
                {
                    if (isNew)
                    {
                        string[] slistreplies = listreplies.Split("|".ToCharArray(), 2);
                        string str = string.Empty;
                        if (slistreplies.Length > 1)
                        {
                            str = slistreplies[1];
                        }

                        DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Replies_Split(oldTopicId, newTopicId, str, DateTime.Now, Convert.ToInt32(slistreplies[0]));
                    }
                    else
                    {
                        DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Replies_Split(oldTopicId, newTopicId, listreplies, DateTime.Now, 0);
                    }
                }

                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForForum(moduleId, oldForumId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForForum(moduleId, newForumId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForTopic(moduleId, oldTopicId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForTopic(moduleId, newTopicId);
            }
        }

        public static DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Approve(int moduleId, int topicId, int userId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(moduleId).GetById(topicId);
            if (ti == null)
            {
                return null;
            }

            ti.IsApproved = true;
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(ti.ModuleId, ti.ForumId, topicId);

            if (ti.Forum.FeatureSettings.ModApproveTemplateId > 0 & ti.Author.AuthorId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(ti.Forum.FeatureSettings.ModApproveTemplateId, ti.PortalId, ti.ModuleId, ti.Forum.TabId, ti.ForumId, topicId, 0, ti.Author);
            }

            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.QueueApprovedTopicAfterAction(portalId: ti.PortalId, tabId: ti.Forum.TabId, moduleId: ti.Forum.ModuleId, forumGroupId: ti.Forum.ForumGroupId, forumId: ti.ForumId, topicId: topicId,  replyId: -1,  contentId: ti.ContentId, userId: userId, authorId: ti.Content.AuthorId);
            return ti;
        }

        public static void Move(int moduleId, int userId, int topicId, int newForumId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(moduleId).GetById(topicId);
            int oldForumId = (int)ti.ForumId;
            SettingsInfo settings = SettingsBase.GetModuleSettings(ti.ModuleId);
            if (settings.URLRewriteEnabled)
            {
                try
                {
                    if (!string.IsNullOrEmpty(ti.Forum.PrefixURL))
                    {
                        Data.Common dbC = new Data.Common();
                        string sURL = dbC.GetUrl(ti.ModuleId, ti.Forum.ForumGroupId, oldForumId, topicId, -1, -1);
                        if (!string.IsNullOrEmpty(sURL))
                        {
                            dbC.ArchiveURL(ti.PortalId, ti.Forum.ForumGroupId, newForumId, topicId, sURL);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }

            var oldForum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(oldForumId, moduleId);
            var newForum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(newForumId, moduleId);

            DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_Move(ti.PortalId, ti.ModuleId, newForumId, topicId);
            ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(moduleId).GetById(topicId);

            if (oldForum.FeatureSettings.ModMoveTemplateId > 0 & ti?.Author?.AuthorId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(oldForum.FeatureSettings.ModMoveTemplateId, ti.PortalId, ti.ModuleId, ti.Forum.TabId, forumId: ti.Forum.ForumID, topicId: ti.TopicId, replyId: -1, author: ti.Author);
            }

            new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.UpdateForumLastUpdated, ti.PortalId, tabId: -1, moduleId: ti.ModuleId, forumGroupId: oldForum.ForumGroupId, forumId: oldForum.ForumID, topicId: topicId, replyId: -1, contentId: ti.ContentId, authorId: ti.Content.AuthorId, userId: userId, requestUrl: null);
            new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.UpdateForumLastUpdated, ti.PortalId, tabId: -1, moduleId: ti.ModuleId, forumGroupId: newForum.ForumGroupId, forumId: newForum.ForumID, topicId: topicId, replyId: -1, contentId: ti.ContentId, authorId: ti.Content.AuthorId, userId: userId, requestUrl: null);
            Utilities.UpdateModuleLastContentModifiedOnDate(ti.ModuleId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForForum(moduleId, ti.ForumId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForTopic(moduleId, topicId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForContent(ti.ModuleId, ti.ContentId);
        }

        public static void SaveToForum(int moduleId, int forumId, int topicId, int lastReplyId = -1)
        {
            DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_SaveToForum(forumId, topicId, lastReplyId);
            Utilities.UpdateModuleLastContentModifiedOnDate(moduleId);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(forumId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForForum(moduleId, forumId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForTopic(moduleId, topicId);
        }

        public static int Save(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            ti.Content.DateUpdated = DateTime.UtcNow;
            if (ti.TopicId < 1)
            {
                ti.Content.DateCreated = DateTime.UtcNow;
            }

            if (ti.IsApproved && ti.Author.AuthorId > 0)
            {
                // TODO: put this in a better place and make it consistent with reply counter
                DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.UpdateUserTopicCount(ti.Forum.PortalId, ti.Author.AuthorId);
            } 

            DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.ClearCache(ti.Forum.PortalId, ti.Content.AuthorId);
            Utilities.UpdateModuleLastContentModifiedOnDate(ti.ModuleId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForForum(ti.ModuleId, ti.ForumId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForTopic(ti.ModuleId, ti.TopicId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForContent(ti.ModuleId, ti.ContentId);

            // if existing topic, update associated journal item
            if (ti.TopicId > 0)
            {
                string sUrl = new ControlUtils().BuildUrl(ti.PortalId, ti.Forum.TabId, ti.ModuleId, ti.Forum.ForumGroup.PrefixURL, ti.Forum.PrefixURL, ti.Forum.ForumGroupId, ti.ForumId, ti.TopicId, ti.TopicUrl, -1, -1, string.Empty, 1, -1, ti.Forum.SocialGroupId);
                new Social().UpdateJournalItemForPost(ti.PortalId, ti.ModuleId, ti.Forum.TabId, ti.ForumId, ti.TopicId, 0, ti.Author.AuthorId, sUrl, ti.Content.Subject, string.Empty, ti.Content.Body);
            }

            // TODO: convert to use DAL2?
            return Convert.ToInt32(DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_Save(ti.Forum.PortalId, ti.TopicId, ti.ViewCount, ti.ReplyCount, ti.IsLocked, ti.IsPinned, ti.TopicIcon, ti.StatusId, ti.IsApproved, ti.IsDeleted, ti.IsAnnounce, ti.IsArchived, ti.AnnounceStart, ti.AnnounceEnd, ti.Content.Subject.Trim(), ti.Content.Body.Trim(), ti.Content.Summary.Trim(), ti.Content.DateCreated, ti.Content.DateUpdated, ti.Content.AuthorId, ti.Content.AuthorName, ti.Content.IPAddress, (int)ti.TopicType, ti.Priority, ti.TopicUrl, ti.TopicData));
        }

        public void DeleteById(int topicId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = base.GetById(topicId);
            if (ti != null)
            {
                new Social().DeleteJournalItemForPost(ti.PortalId, ti.ForumId, topicId, 0);

                DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_Delete(ti.ForumId, topicId, SettingsBase.GetModuleSettings(ti.ModuleId).DeleteBehavior );
                Utilities.UpdateModuleLastContentModifiedOnDate(ti.ModuleId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForForum(ti.ModuleId, ti.ForumId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForTopic(ti.ModuleId, ti.TopicId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForContent(ti.ModuleId, ti.ContentId);

                if (SettingsBase.GetModuleSettings(ti.ModuleId).DeleteBehavior != 0)
                {
                    return;
                }

                // If it's a hard delete, delete associated attachments
                var attachmentController = new Data.AttachController();
                var fileManager = FileManager.Instance;
                var folderManager = FolderManager.Instance;
                var attachmentFolder = folderManager.GetFolder(ti.PortalId, "activeforums_Attach");

                foreach (var attachment in attachmentController.ListForPost(topicId, null))
                {
                    attachmentController.Delete(attachment.AttachmentId);

                    var file = attachment.FileId.HasValue ? fileManager.GetFile(attachment.FileId.Value) : fileManager.GetFile(attachmentFolder, attachment.FileName);

                    // Only delete the file if it exists in the attachment folder
                    if (file != null && file.FolderId == attachmentFolder.FolderID)
                    {
                        fileManager.DeleteFile(file);
                    }
                }
            }
        }

        internal static bool QueueApprovedTopicAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.ApprovedTopicCreated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, requestUrl: HttpContext.Current.Request.Url.ToString());
        }

        internal static bool QueueUnapprovedTopicAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.UnapprovedTopicCreated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, requestUrl: HttpContext.Current.Request.Url.ToString());
        }

        internal static bool ProcessApprovedTopicAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId, string requestUrl)
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(moduleId).GetById(topicId);
                if (topic == null)
                {
                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    var message = string.Format(Utilities.GetSharedResource("[RESX:UnableToFindTopicToProcess]"), replyId);
                    log.AddProperty("Message", message);
                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                    return true;
                }

                Subscriptions.SendSubscriptions(-1, portalId, moduleId, tabId, topic.Forum, topicId, 0, topic.Content.AuthorId, new Uri(requestUrl));

                string sUrl = new ControlUtils().BuildUrl(portalId, tabId, moduleId, topic.Forum.ForumGroup.PrefixURL, topic.Forum.PrefixURL, topic.Forum.ForumGroupId, forumId, topicId, topic.TopicUrl, -1, -1, string.Empty, 1, -1, topic.Forum.SocialGroupId);

                Social amas = new Social();
                amas.AddTopicToJournal(portalId, moduleId, tabId, forumId, topicId, topic.Author.AuthorId, sUrl, topic.Content.Subject, string.Empty, topic.Content.Body, topic.Forum.Security.Read, topic.Forum.SocialGroupId);

                var pqc = new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController();
                pqc.Add(ProcessType.UpdateForumTopicPointers, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, requestUrl: requestUrl);
                pqc.Add(ProcessType.UpdateForumLastUpdated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, requestUrl: requestUrl);

                Utilities.UpdateModuleLastContentModifiedOnDate(moduleId);

                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }

        internal static bool ProcessUnapprovedTopicAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int userId, int authorId, string requestUrl)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.SendModerationNotification(portalId, tabId, moduleId, forumGroupId, forumId, topicId, replyId, authorId, new Uri(requestUrl), new Uri(requestUrl).PathAndQuery);
        }

        internal static bool ProcessTopicPinned(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId, string requestUrl)
        {
            try
            {
                var topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(moduleId).GetById(topicId);
                if (topic == null)
                {
                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    var message = string.Format(Utilities.GetSharedResource("[RESX:UnableToFindTopicToProcess]"), contentId, userId);
                    log.AddProperty("Message", message);
                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                    return true;
                }

                if ((bool)topic.Author?.ForumUser?.PinNotificationsEnabled)
                {
                    var subject = Utilities.GetSharedResource("[RESX:PinNotificationSubject]");
                    subject = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTopicTokens(new StringBuilder(subject), topic, topic.Forum.PortalSettings, topic.Forum.MainSettings, new Services.URLNavigator().NavigationManager(), topic.Author.ForumUser, new Uri(requestUrl), new Uri(requestUrl).PathAndQuery).ToString();
                    subject = subject.Length > 400 ? subject.Substring(0, 400) : subject;
                    var body = Utilities.GetSharedResource("[RESX:PinNotificationBody]");
                    body = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTopicTokens(new StringBuilder(body), topic, topic.Forum.PortalSettings, topic.Forum.MainSettings, new Services.URLNavigator().NavigationManager(), topic.Author.ForumUser, new Uri(requestUrl), new Uri(requestUrl).PathAndQuery).ToString();

                    string notificationKey = BuildNotificationContextKey(tabId, moduleId, topicId, userId);

                    NotificationType notificationType = NotificationsController.Instance.GetNotificationType(Globals.PinNotificationType);
                    Notification notification = new Notification
                    {
                        NotificationTypeID = notificationType.NotificationTypeId,
                        Subject = subject,
                        Body = body,
                        IncludeDismissAction = true,
                        SenderUserID = userId,
                        Context = notificationKey,
                    };
                    var users = new List<DotNetNuke.Entities.Users.UserInfo> { topic.Author.ForumUser.UserInfo };
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

        internal static string BuildNotificationContextKey(int tabId, int moduleId, int topicId, int userId)
        {
            return $"{tabId}:{moduleId}:{topicId}:{userId}";
        }
    }
}
