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
    using System.Web;

    using DotNetNuke.Collections;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Modules.ActiveForums.ViewModels;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Log.EventLog;

    internal partial class ReplyController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo>
    {
        private int moduleId = -1;

        internal override string cacheKeyTemplate => CacheKeys.ReplyInfo;

        internal ReplyController(int moduleId)
        {
            this.moduleId = moduleId;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo GetById(int replyId)
        {
            var cachekey = this.GetCacheKey(moduleId: this.moduleId, id: replyId);
            var ri = DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheRetrieve(this.moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo;
            if (ri == null)
            {
                ri = base.GetById(replyId);
            }

            if (ri != null)
            {
                ri.ModuleId = this.moduleId;
                ri.GetTopic();
                ri.Forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ri.Topic.ForumId, this.moduleId);
                if (ri.Forum != null)
                {
                    ri.PortalId = ri.Forum.PortalId;
                }

                ri.GetContent();
                if (ri.Content != null)
                {
                    ri.Author = ri.GetAuthor(ri.PortalId, ri.ModuleId, ri.Content.AuthorId);
                }
            }

            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, ri);
            return ri;
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo> GetByTopicId(int topicId)
        {
            var replies = new List<DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo>();
            var replyIds = this.Find("WHERE TopicId = @0", topicId).Select(r => r.ReplyId).ToList();

            replyIds.ForEach(r =>
            {
                replies.Add(this.GetById(r));
            });
            return replies;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo GetByContentId(int contentId)
        {
            var cachekey = string.Format(CacheKeys.ReplyInfoByContentId, this.moduleId, contentId);
            var ri = DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheRetrieve(this.moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo;
            if (ri == null)
            {
                ri = this.Find("WHERE ContentId = @0", contentId).FirstOrDefault();
            }

            if (ri != null)
            {
                ri = this.GetById(ri.ReplyId);
            }

            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, ri);
            return ri;
        }

        public void Reply_Delete(int portalId, int forumId, int topicId, int replyId, DotNetNuke.Modules.ActiveForums.Enums.DeleteBehavior delBehavior)
        {
            var reply = this.GetById(replyId);
            DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Reply_Delete(forumId, topicId, replyId, (int)delBehavior);
            new DotNetNuke.Modules.ActiveForums.Controllers.ForumTopicController(reply.ModuleId).Update(forumId: forumId, topicId: topicId);

            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(forumId);

            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForForum(reply.ModuleId, reply.ForumId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForReply(reply.ModuleId, reply.ReplyId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForTopic(reply.ModuleId, reply.TopicId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForContent(reply.ModuleId, reply.ContentId);
            DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.ForumViewPrefix, reply.ModuleId));
            DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.TopicViewPrefix, reply.ModuleId));
            DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, reply.ModuleId));

            new Social().DeleteJournalItemForPost(portalId, forumId, topicId, replyId);

            Utilities.UpdateModuleLastContentModifiedOnDate(reply.ModuleId);
            if (delBehavior.Equals(DotNetNuke.Modules.ActiveForums.Enums.DeleteBehavior.Recycle))
            {
                return;
            }

            // If it's a hard delete, delete associated attachments
            var attachmentController = new Data.AttachController();
            var fileManager = FileManager.Instance;
            var folderManager = FolderManager.Instance;
            var attachmentFolder = folderManager.GetFolder(portalId, "activeforums_Attach");

            foreach (var attachment in attachmentController.ListForPost(topicId, replyId))
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

        public void Restore(int portalId, int forumId, int topicId, int replyId)
        {
            var reply = this.GetById(replyId);

            // if restoring reply, also restore topic if necessary
            if (reply.Topic.IsDeleted)
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(reply.ModuleId).Restore(portalId, forumId, topicId);
            }

            reply.IsDeleted = false;
            this.Update(reply);
            reply.Content.IsDeleted = false;
            new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().Update(reply.Content);
            new DotNetNuke.Modules.ActiveForums.Controllers.ForumTopicController(reply.ModuleId).Update(forumId: forumId, topicId: topicId);

            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(forumId);

            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForForum(reply.ModuleId, reply.ForumId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForReply(reply.ModuleId, reply.ReplyId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForTopic(reply.ModuleId, reply.TopicId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForContent(reply.ModuleId, reply.ContentId);
            DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.ForumViewPrefix, reply.ModuleId));
            DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.TopicViewPrefix, reply.ModuleId));
            DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, reply.ModuleId));

            Utilities.UpdateModuleLastContentModifiedOnDate(reply.ModuleId);
        }

        public int Reply_QuickCreate(int portalId, int moduleId, int forumId, int topicId, int replyToId, string subject, string body, int userId, string displayName, bool isApproved, string iPAddress)
        {
            int replyId = -1;
            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = new DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo
            {
                ModuleId = moduleId,
                PortalId = portalId,
                TopicId = topicId,
                ReplyToId = replyToId,
                IsApproved = isApproved,
                IsDeleted = false,
                StatusId = -1,
                Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                {
                    AuthorId = userId,
                    AuthorName = displayName,
                    Subject = subject,
                    Body = body,
                    IPAddress = iPAddress,
                    Summary = string.Empty,
                },
            };
            replyId = this.Reply_Save(portalId, moduleId, ri);
            Utilities.UpdateModuleLastContentModifiedOnDate(moduleId);
            return replyId;
        }

        public int Reply_Save(int portalId, int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri)
        {
            ri.Content.DateUpdated = DateTime.UtcNow;
            if (ri.ReplyId < 1)
            {
                ri.Content.DateCreated = DateTime.UtcNow;
            }

            Utilities.UpdateModuleLastContentModifiedOnDate(moduleId);

            // Clear profile Cache to make sure the LastPostDate is updated for Flood Control
            DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.ClearCache(portalId, ri.Content.AuthorId);

            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForForum(ri.ModuleId, ri.ForumId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForReply(ri.ModuleId, ri.ReplyId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForTopic(ri.ModuleId, ri.TopicId);
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClearForContent(ri.ModuleId, ri.ContentId);

            // if existing topic, update associated journal item
            if (ri.ReplyId > 0)
            {
                string fullURL = new ControlUtils().BuildUrl(portalId, ri.Forum.GetTabId(), moduleId, ri.Forum.ForumGroup.PrefixURL, ri.Forum.PrefixURL, ri.Forum.ForumGroupId, ri.ForumId, ri.TopicId, ri.Topic.TopicUrl, -1, -1, string.Empty, 1, ri.ReplyId, ri.Forum.SocialGroupId);

                if (fullURL.Contains("~/"))
                {
                    fullURL = Utilities.NavigateURL(ri.Forum.GetTabId(), string.Empty, new string[] { $"{ParamKeys.TopicId}={ri.TopicId}", $"{ParamKeys.ContentJumpId}={ri.ReplyId}", });
                }

                if (fullURL.EndsWith("/"))
                {
                    fullURL += Utilities.UseFriendlyURLs(moduleId) ? $"#{ri.ReplyId}" : $"{ParamKeys.ContentJumpId}={ri.ReplyId}";
                }

                new Social().UpdateJournalItemForPost(ri.PortalId, ri.ModuleId, ri.Forum.GetTabId(), ri.ForumId, ri.TopicId, ri.ReplyId, ri.Author.AuthorId, fullURL, ri.Content.Subject, string.Empty, ri.Content.Body);
            }

            int replyId = Convert.ToInt32(DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Reply_Save(portalId, ri.TopicId, ri.ReplyId, ri.ReplyToId, ri.StatusId, ri.IsApproved, ri.IsDeleted, ri.Content.Subject.Trim(), ri.Content.Body.Trim(), ri.Content.DateCreated, ri.Content.DateUpdated, ri.Content.AuthorId, ri.Content.AuthorName, ri.Content.IPAddress));
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(moduleId, ri.ForumId, ri.TopicId);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(ri.ForumId);
            return replyId;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ApproveReply(int portalId, int tabId, int moduleId, int forumId, int topicId, int replyId, int userId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: forumId, moduleId: moduleId);
            var rc = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(moduleId);
            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = rc.GetById(replyId);
            if (reply == null)
            {
                return null;
            }

            reply.IsApproved = true;
            rc.Reply_Save(portalId, moduleId, reply);
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(moduleId, forumId, topicId);

            if (forum.FeatureSettings.ModApproveNotify && reply.Author.AuthorId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(TemplateType.ModApprove, tabId, forum, topicId, replyId, reply.Author);
            }

            DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QueueApprovedReplyAfterAction(portalId: portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forum.ForumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: reply.ContentId, authorId: reply.Content.AuthorId, userId: userId);

            return reply;
        }

        internal static bool QueueApprovedReplyAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.ApprovedReplyCreated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, requestUrl: HttpContext.Current.Request.Url.ToString());
        }

        internal static bool QueueUnapprovedReplyAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.UnapprovedReplyCreated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, requestUrl: HttpContext.Current.Request.Url.ToString());
        }

        internal static bool ProcessApprovedReplyAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId, string requestUrl)
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(moduleId).GetById(replyId);
                if (reply == null)
                {
                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    var message = string.Format(Utilities.GetSharedResource("[RESX:UnableToFindReplyToProcess]"), replyId);
                    log.AddProperty("Message", message);
                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                    return true;
                }

                Subscriptions.SendSubscriptions(-1, portalId, moduleId, tabId, reply.Forum, topicId, replyId, authorId, new Uri(requestUrl));

                ControlUtils ctlUtils = new ControlUtils();
                string fullURL = ctlUtils.BuildUrl(portalId, tabId, moduleId, reply.Forum.ForumGroup.PrefixURL, reply.Forum.PrefixURL, reply.Forum.ForumGroupId, forumId, topicId, reply.Topic.TopicUrl, -1, -1, string.Empty, 1, replyId, reply.Forum.SocialGroupId);

                if (fullURL.Contains("~/"))
                {
                    fullURL = Utilities.NavigateURL(tabId, string.Empty, new string[] { $"{ParamKeys.TopicId}={replyId}", $"{ParamKeys.ContentJumpId}={replyId}", });
                }

                if (fullURL.EndsWith("/"))
                {
                    fullURL += Utilities.UseFriendlyURLs(moduleId) ? $"#{replyId}" : $"{ParamKeys.ContentJumpId}={replyId}";
                }

                new Social().AddReplyToJournal(portalId, moduleId, tabId, forumId, topicId, replyId, reply.Author.AuthorId, fullURL, reply.Content.Subject, string.Empty, reply.Content.Body, reply.Forum.Security.Read, reply.Forum.SocialGroupId);

                DataCache.ContentCacheClear(reply.ModuleId, string.Format(CacheKeys.ForumInfo, reply.ModuleId, reply.ForumId));
                DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.ForumViewPrefix, reply.ModuleId));
                DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.TopicViewPrefix, reply.ModuleId));
                DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, reply.ModuleId));
                var pqc = new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController();
                pqc.Add(ProcessType.UpdateForumTopicPointers, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, requestUrl: requestUrl);
                pqc.Add(ProcessType.UpdateForumLastUpdated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, requestUrl: requestUrl);

                Utilities.UpdateModuleLastContentModifiedOnDate(moduleId);

                if (reply.IsApproved && reply.Content.AuthorId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.UpdateUserReplyCount(portalId, reply.Content.AuthorId);
                }

                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }

        internal static bool ProcessUnapprovedReplyAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId, string requestUrl)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.SendModerationNotification(portalId, tabId, moduleId, forumGroupId, forumId, topicId, replyId, authorId, new Uri(requestUrl), new Uri(requestUrl).PathAndQuery);
        }
    }
}
