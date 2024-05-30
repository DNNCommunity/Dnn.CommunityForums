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
using DotNetNuke.Data;
using DotNetNuke.Modules.ActiveForums.API;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Journal;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Social.Notifications;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Web;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal partial class ReplyController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo GetById(int ReplyId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = base.GetById(ReplyId);
            if (ri != null)
            {
                ri.GetTopic();
                ri.GetForum();
                ri.GetContent();
                ri.GetAuthor();
            }
            return ri;
        }
        internal static DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo GetReply(int ReplyId) 
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(ReplyId);
        }
        public void Reply_Delete(int PortalId, int ForumId, int TopicId, int ReplyId, int DelBehavior)
        {
            var ri = GetById(ReplyId);
            DataProvider.Instance().Reply_Delete(ForumId, TopicId, ReplyId, DelBehavior);
            DataCache.ContentCacheClear(ri.ModuleId, string.Format(CacheKeys.ForumInfo, ri.ModuleId, ForumId));
            DataCache.CacheClearPrefix(ri.ModuleId, string.Format(CacheKeys.ForumViewPrefix, ri.ModuleId));
            DataCache.CacheClearPrefix(ri.ModuleId, string.Format(CacheKeys.TopicViewPrefix, ri.ModuleId));
            DataCache.CacheClearPrefix(ri.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, ri.ModuleId));

            var objectKey = string.Format("{0}:{1}:{2}", ForumId, TopicId, ReplyId);
            JournalController.Instance.DeleteJournalItemByKey(PortalId, objectKey);

            Utilities.UpdateModuleLastContentModifiedOnDate(ri.ModuleId);
            if (DelBehavior != 0)
                return;

            // If it's a hard delete, delete associated attachments
            var attachmentController = new Data.AttachController();
            var fileManager = FileManager.Instance;
            var folderManager = FolderManager.Instance;
            var attachmentFolder = folderManager.GetFolder(PortalId, "activeforums_Attach");

            foreach (var attachment in attachmentController.ListForPost(TopicId, ReplyId))
            {
                attachmentController.Delete(attachment.AttachmentId);

                var file = attachment.FileId.HasValue ? fileManager.GetFile(attachment.FileId.Value) : fileManager.GetFile(attachmentFolder, attachment.FileName);

                // Only delete the file if it exists in the attachment folder
                if (file != null && file.FolderId == attachmentFolder.FolderID)
                    fileManager.DeleteFile(file);
            }
        }
        public int Reply_QuickCreate(int PortalId, int ModuleId, int ForumId, int TopicId, int ReplyToId, string Subject, string Body, int UserId, string DisplayName, bool IsApproved, string IPAddress)
        {
            int replyId = -1;
            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = new DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo
            {
                TopicId = TopicId,
                ReplyToId = ReplyToId,
                IsApproved = IsApproved,
                IsDeleted = false,
                StatusId = -1,
                Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                {
                    AuthorId = UserId,
                    AuthorName = DisplayName,
                    Subject = Subject,
                    Body = Body,
                    IPAddress = IPAddress,
                    Summary = string.Empty
                }
            };
            replyId = Reply_Save(PortalId, ModuleId, ri);
            Utilities.UpdateModuleLastContentModifiedOnDate(ModuleId);
            return replyId;
        }
        public int Reply_Save(int PortalId, int ModuleId, DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri)
        {
            ri.Content.DateUpdated = DateTime.UtcNow;
            if (ri.ReplyId < 1)
            { 
                ri.Content.DateCreated = DateTime.UtcNow;
            }
            Utilities.UpdateModuleLastContentModifiedOnDate(ModuleId);
            // Clear profile Cache to make sure the LastPostDate is updated for Flood Control
            UserProfileController.Profiles_ClearCache(ModuleId, ri.Content.AuthorId);

            DataCache.ContentCacheClear(ri.ModuleId, string.Format(CacheKeys.ForumInfo, ri.ModuleId, ri.ForumId));
            DataCache.CacheClearPrefix(ri.ModuleId, string.Format(CacheKeys.ForumViewPrefix, ri.ModuleId));
            DataCache.CacheClearPrefix(ri.ModuleId, string.Format(CacheKeys.TopicViewPrefix, ri.ModuleId));
            DataCache.CacheClearPrefix(ri.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, ri.ModuleId));
            DataCache.ContentCacheClear(ri.ModuleId, string.Format(CacheKeys.TopicViewForUser, ri.ModuleId, ri.TopicId, ri.Content.AuthorId));
            return Convert.ToInt32(DataProvider.Instance().Reply_Save(PortalId, ri.TopicId, ri.ReplyId, ri.ReplyToId, ri.StatusId, ri.IsApproved, ri.IsDeleted, ri.Content.Subject.Trim(), ri.Content.Body.Trim(), ri.Content.DateCreated, ri.Content.DateUpdated, ri.Content.AuthorId, ri.Content.AuthorName, ri.Content.IPAddress));
        }
        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ApproveReply(int PortalId, int TabId, int ModuleId, int ForumId, int TopicId, int ReplyId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: ForumId, moduleId: ModuleId);
            var rc = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController();
            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = rc.GetById(ReplyId);
            if (reply == null)
            {
                return null;
            }
            reply.IsApproved = true;
            rc.Reply_Save(PortalId, ModuleId, reply);
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(ModuleId, ForumId, TopicId, ReplyId);

            if (forum.ModApproveTemplateId > 0 & reply.Author.AuthorId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(forum.ModApproveTemplateId, PortalId, ModuleId, TabId, ForumId, TopicId, ReplyId, string.Empty, reply.Author);
            }
            DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QueueApprovedReplyAfterAction(PortalId, TabId, ModuleId, forum.ForumGroupId, ForumId, TopicId, ReplyId, reply.Content.AuthorId);

            return reply;
        }
        internal static bool QueueApprovedReplyAfterAction(int PortalId, int TabId, int ModuleId, int ForumGroupId, int ForumId, int TopicId, int ReplyId, int AuthorId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.ApprovedReplyCreated, PortalId, tabId: TabId, moduleId: ModuleId, forumGroupId: ForumGroupId, forumId: ForumId, topicId: TopicId, replyId: ReplyId, authorId: AuthorId, requestUrl: HttpContext.Current.Request.Url.ToString());
        }
        internal static bool QueueUnapprovedReplyAfterAction(int PortalId, int TabId, int ModuleId, int ForumGroupId, int ForumId, int TopicId, int ReplyId, int AuthorId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.UnapprovedReplyCreated, PortalId, tabId: TabId, moduleId: ModuleId, forumGroupId: ForumGroupId, forumId: ForumId, topicId: TopicId, replyId: ReplyId, authorId: AuthorId, requestUrl: HttpContext.Current.Request.Url.ToString());
        }
        internal static bool ProcessApprovedReplyAfterAction(int PortalId, int TabId, int ModuleId, int ForumGroupId, int ForumId, int TopicId, int ReplyId, int AuthorId, string RequestUrl)
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(ReplyId);
                Subscriptions.SendSubscriptions(-1, PortalId, ModuleId, TabId, reply.Forum, TopicId, ReplyId, AuthorId, new Uri(RequestUrl));

                ControlUtils ctlUtils = new ControlUtils();
                string fullURL = ctlUtils.BuildUrl(TabId, ModuleId, reply.Forum.ForumGroup.PrefixURL, reply.Forum.PrefixURL, reply.Forum.ForumGroupId, ForumId, TopicId, reply.Topic.TopicUrl, -1, -1, string.Empty, 1, ReplyId, reply.Forum.SocialGroupId);

                if (fullURL.Contains("~/"))
                {
                    fullURL = Utilities.NavigateURL(TabId, "", new string[] { ParamKeys.TopicId + "=" + TopicId, ParamKeys.ContentJumpId + "=" + ReplyId });
                }
                if (fullURL.EndsWith("/"))
                {
                    fullURL += Utilities.UseFriendlyURLs(ModuleId) ? String.Concat("#", ReplyId) : String.Concat("?", ParamKeys.ContentJumpId, "=", ReplyId);
                }
                Social amas = new Social();
                amas.AddReplyToJournal(PortalId, ModuleId, TabId, ForumId, TopicId, ReplyId, reply.Author.AuthorId, fullURL, reply.Content.Subject, string.Empty, reply.Content.Body, reply.Forum.Security.Read, reply.Forum.SocialGroupId);

                DataCache.ContentCacheClear(reply.ModuleId, string.Format(CacheKeys.ForumInfo, reply.ModuleId, reply.ForumId));
                DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.ForumViewPrefix, reply.ModuleId));
                DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.TopicViewPrefix, reply.ModuleId));
                DataCache.CacheClearPrefix(reply.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, reply.ModuleId));
                var pqc = new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController();
                pqc.Add(ProcessType.UpdateForumTopicPointers, PortalId, tabId: TabId, moduleId: ModuleId, forumGroupId: ForumGroupId, forumId: ForumId, topicId: TopicId, replyId: ReplyId, authorId: AuthorId, requestUrl: RequestUrl);
                pqc.Add(ProcessType.UpdateForumLastUpdated, PortalId, tabId: TabId, moduleId: ModuleId, forumGroupId: ForumGroupId, forumId: ForumId, topicId: TopicId, replyId: ReplyId, authorId: AuthorId, requestUrl: RequestUrl);

                Utilities.UpdateModuleLastContentModifiedOnDate(ModuleId);
                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }
        internal static bool ProcessUnapprovedReplyAfterAction(int PortalId, int TabId, int ModuleId, int ForumGroupId, int ForumId, int TopicId, int ReplyId, int AuthorId, string RequestUrl)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.SendModerationNotification(PortalId, TabId, ModuleId, ForumGroupId, ForumId, TopicId, ReplyId, AuthorId, RequestUrl);
        }
    }
}
