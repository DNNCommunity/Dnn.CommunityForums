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
namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Security.Policy;
    using System.Text;
    using System.Web;

    using DotNetNuke.Data;
    using DotNetNuke.Modules.ActiveForums.API;
    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Social.Notifications;

    internal partial class ReplyController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo GetById(int replyId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = base.GetById(replyId);
            if (ri != null)
            {
                ri.GetTopic();
                ri.GetForum();
                ri.GetContent();
                ri.GetAuthor();
            }

            return ri;
        }

        internal static DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo GetReply(int replyId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(replyId);
        }

        public void Reply_Delete(int portalId, int forumId, int topicId, int replyId, int delBehavior)
        {
            var ri = GetById(replyId);
            DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Reply_Delete(forumId, topicId, replyId, delBehavior);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(forumId);

            DataCache.ContentCacheClear(ri.ModuleId, string.Format(CacheKeys.ForumInfo, ri.ModuleId, forumId));
            DataCache.CacheClearPrefix(ri.ModuleId, string.Format(CacheKeys.ForumViewPrefix, ri.ModuleId));
            DataCache.CacheClearPrefix(ri.ModuleId, string.Format(CacheKeys.TopicViewPrefix, ri.ModuleId));
            DataCache.CacheClearPrefix(ri.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, ri.ModuleId));

            var objectKey = string.Format("{0}:{1}:{2}", forumId, topicId, replyId);
            JournalController.Instance.DeleteJournalItemByKey(portalId, objectKey);

            Utilities.UpdateModuleLastContentModifiedOnDate(ri.ModuleId);
            if (delBehavior != 0)
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

        public int Reply_QuickCreate(int portalId, int moduleId, int forumId, int topicId, int replyToId, string subject, string body, int userId, string displayName, bool isApproved, string iPAddress)
        {
            int replyId = -1;
            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = new DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo
            {
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
                    Summary = string.Empty
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
            UserProfileController.Profiles_ClearCache(moduleId, ri.Content.AuthorId);

            DataCache.ContentCacheClear(ri.ModuleId, string.Format(CacheKeys.ForumInfo, ri.ModuleId, ri.ForumId));
            DataCache.CacheClearPrefix(ri.ModuleId, string.Format(CacheKeys.ForumViewPrefix, ri.ModuleId));
            DataCache.CacheClearPrefix(ri.ModuleId, string.Format(CacheKeys.TopicViewPrefix, ri.ModuleId));
            DataCache.CacheClearPrefix(ri.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, ri.ModuleId));
            int replyId = Convert.ToInt32(DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Reply_Save(portalId, ri.TopicId, ri.ReplyId, ri.ReplyToId, ri.StatusId, ri.IsApproved, ri.IsDeleted, ri.Content.Subject.Trim(), ri.Content.Body.Trim(), ri.Content.DateCreated, ri.Content.DateUpdated, ri.Content.AuthorId, ri.Content.AuthorName, ri.Content.IPAddress));
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(ri.ForumId);
            return replyId;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ApproveReply(int portalId, int tabId, int moduleId, int forumId, int topicId, int replyId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: forumId, moduleId: moduleId);
            var rc = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController();
            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = rc.GetById(replyId);
            if (reply == null)
            {
                return null;
            }

            reply.IsApproved = true;
            rc.Reply_Save(portalId, moduleId, reply);
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(moduleId, forumId, topicId, replyId);

            if (forum.ModApproveTemplateId > 0 & reply.Author.AuthorId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(forum.ModApproveTemplateId, portalId, moduleId, tabId, forumId, topicId, replyId, string.Empty, reply.Author);
            }

            DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QueueApprovedReplyAfterAction(portalId, tabId, moduleId, forum.ForumGroupId, forumId, topicId, replyId, reply.Content.AuthorId);

            return reply;
        }

        internal static bool QueueApprovedReplyAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int authorId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.ApprovedReplyCreated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, authorId: authorId, requestUrl: HttpContext.Current.Request.Url.ToString());
        }

        internal static bool QueueUnapprovedReplyAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int authorId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.UnapprovedReplyCreated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, authorId: authorId, requestUrl: HttpContext.Current.Request.Url.ToString());
        }

        internal static bool ProcessApprovedReplyAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int authorId, string requestUrl)
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(replyId);
                Subscriptions.SendSubscriptions(-1, portalId, moduleId, tabId, reply.Forum, topicId, replyId, authorId, new Uri(requestUrl));

                ControlUtils ctlUtils = new ControlUtils();
                string fullURL = ctlUtils.BuildUrl(tabId, moduleId, reply.Forum.ForumGroup.PrefixURL, reply.Forum.PrefixURL, reply.Forum.ForumGroupId, forumId, topicId, reply.Topic.TopicUrl, -1, -1, string.Empty, 1, replyId, reply.Forum.SocialGroupId);

                if (fullURL.Contains("~/"))
                {
                    fullURL = Utilities.NavigateURL(tabId, string.Empty, new string[] { ParamKeys.TopicId + "=" + topicId, ParamKeys.ContentJumpId + "=" + replyId });
                }

                if (fullURL.EndsWith("/"))
                {
                    fullURL += Utilities.UseFriendlyURLs(moduleId) ? String.Concat("#", replyId) : String.Concat("?", ParamKeys.ContentJumpId, "=", replyId);
                }

                Social amas = new Social();
                amas.AddReplyToJournal(portalId, moduleId, tabId, forumId, topicId, replyId, reply.Author.AuthorId, fullURL, reply.Content.Subject, string.Empty, reply.Content.Body, reply.Forum.Security.Read, reply.Forum.SocialGroupId);

                DataCache.ContentCacheClear(reply.ModuleId, string.Format(CacheKeys.ForumInfo, reply.ModuleId, reply.ForumId));
                DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.ForumViewPrefix, reply.ModuleId));
                DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.TopicViewPrefix, reply.ModuleId));
                DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, reply.ModuleId));
                var pqc = new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController();
                pqc.Add(ProcessType.UpdateForumTopicPointers, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, authorId: authorId, requestUrl: requestUrl);
                pqc.Add(ProcessType.UpdateForumLastUpdated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, authorId: authorId, requestUrl: requestUrl);

                Utilities.UpdateModuleLastContentModifiedOnDate(moduleId);
                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }

        internal static bool ProcessUnapprovedReplyAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int authorId, string requestUrl)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.SendModerationNotification(portalId, tabId, moduleId, forumGroupId, forumId, topicId, replyId, authorId, requestUrl);
        }
    }
}
