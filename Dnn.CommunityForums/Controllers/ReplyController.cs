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
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Modules.ActiveForums.ViewModels;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    
    internal class ReplyController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo, IReplyController, ReplyController>, IReplyController
    {
        protected override Func<IReplyController> GetFactory()
        {
            return () => new ReplyController();
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo GetById(int moduleId, int replyId, TopicInfo topic = null)
        {
            var cachekey = string.Format(CacheKeys.ReplyInfo, moduleId, replyId);
            var ri = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo;
            if (ri == null)
            {
                ri = this._repositoryControllerBase.GetById(replyId);
            }

            if (ri != null)
            {
                ri.ModuleId = moduleId;
                ri.Topic = topic ?? ri.GetTopic();
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

            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, ri);
            return ri;
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo> GetByTopicId(int moduleId, int topicId)
        {
            var replies = new List<DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo>();
            var replyIds = this._repositoryControllerBase.Find("WHERE TopicId = @0", topicId).Select(r => r.ReplyId).ToList();

            replyIds.ForEach(r =>
            {
                replies.Add(this.GetById(moduleId, r));
            });
            return replies;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo GetByContentId(int moduleId, int contentId)
        {
            var cachekey = string.Format(CacheKeys.ReplyInfoByContentId, moduleId, contentId);
            var ri = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo;
            if (ri == null)
            {
                ri = this._repositoryControllerBase.Find("WHERE ContentId = @0", contentId).FirstOrDefault();
            }

            if (ri != null)
            {
                ri = this.GetById(moduleId, ri.ReplyId);
            }

            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, ri);
            return ri;
        }

        public void Reply_Delete(int portalId, int moduleId, int forumId, int topicId, int replyId, DotNetNuke.Modules.ActiveForums.Enums.DeleteBehavior delBehavior)
        {
            var reply = this.GetById(moduleId: moduleId, replyId: replyId);

            Social.DeleteJournalItemForPost(reply);

            if (reply.Forum.FeatureSettings.IndexContent)
            {
                var searchDoc = new SearchDocumentToDelete
                {
                    UniqueKey = $"{reply.ModuleId}-{reply.ContentId}",
                    ModuleId = reply.ModuleId,
                    PortalId = reply.PortalId,
                    SearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId,
                };
                DotNetNuke.Data.DataProvider.Instance().AddSearchDeletedItems(searchDoc);
            }

            // If it's a hard delete, delete associated attachments
            if (delBehavior.Equals(DotNetNuke.Modules.ActiveForums.Enums.DeleteBehavior.Remove))
            {
                reply.Content.RemoveAttachments();
            }

            DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Reply_Delete(forumId, topicId, replyId, (int)delBehavior);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumTopicController.Instance.Update(moduleId: reply.ModuleId, forumId: forumId, topicId: topicId);

            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(forumId);

            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForForum(reply.ModuleId, reply.ForumId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForReply(reply.ModuleId, reply.ReplyId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForTopic(reply.ModuleId, reply.TopicId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForContent(reply.ModuleId, reply.ContentId);

            Utilities.UpdateModuleLastContentModifiedOnDate(reply.ModuleId);
        }

        public void Restore(int portalId, int moduleId, int forumId, int topicId, int replyId)
        {
            var reply = this._repositoryControllerBase.GetById(id: replyId, scopeValue: moduleId);
            if (reply == null)
            {
                reply = this._repositoryControllerBase.GetById(id: replyId);
            }

            // if restoring reply, also restore topic if necessary
            if (reply.Topic.IsDeleted)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.Restore(portalId, reply.ModuleId, forumId, topicId);
            }

            reply.IsDeleted = false;
            this._repositoryControllerBase.Update(reply);
            reply.Content.IsDeleted = false;
            DotNetNuke.Modules.ActiveForums.Controllers.ContentController.Instance.Update(reply.Content);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumTopicController.Instance.Update(moduleId: reply.ModuleId, forumId: forumId, topicId: topicId);

            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(forumId);

            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForForum(reply.ModuleId, reply.ForumId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForReply(reply.ModuleId, reply.ReplyId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForTopic(reply.ModuleId, reply.TopicId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForContent(reply.ModuleId, reply.ContentId);

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

        public int Reply_Save(int portalId, int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply)
        {
            reply.Content.DateUpdated = DateTime.UtcNow;
            if (reply.ReplyId < 1)
            {
                reply.Content.DateCreated = DateTime.UtcNow;
            }

            Utilities.UpdateModuleLastContentModifiedOnDate(moduleId);

            // Clear profile Cache to make sure the LastPostDate is updated for Flood Control
            DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.ClearCache(portalId, reply.Content.AuthorId);

            // if existing reply being edited, update associated journal item & tags
            if (reply.ReplyId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.TagController.Instance.UpdateTopicTags(reply);
                Social.UpdateJournalItemForPost(reply);
            }

            var forum = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetById(moduleId: moduleId, forumId: reply.ForumId);
            reply.Content.Body = DotNetNuke.Modules.ActiveForums.Controllers.TagController.GetBodyWithTagsProcessed(reply, forum, new Services.URLNavigator().NavigationManager());
            var replyId = Convert.ToInt32(DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Reply_Save(portalId, reply.TopicId, reply.ReplyId, reply.ReplyToId, reply.StatusId, reply.IsApproved, reply.IsDeleted, reply.Content.Subject.Trim(), reply.Content.Body.Trim(), reply.Content.DateCreated, reply.Content.DateUpdated, reply.Content.AuthorId, reply.Content.AuthorName, reply.Content.IPAddress));
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(moduleId, reply.ForumId, reply.TopicId);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(reply.ForumId);

            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForForum(reply.ModuleId, reply.ForumId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForReply(reply.ModuleId, reply.ReplyId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForTopic(reply.ModuleId, reply.TopicId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForContent(reply.ModuleId, reply.ContentId);

            return replyId;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ApproveReply(int portalId, int tabId, int moduleId, int forumId, int topicId, int replyId, int userId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetById(moduleId: moduleId, forumId: forumId);
            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Instance.GetById(moduleId, replyId);
            if (reply == null)
            {
                return null;
            }

            reply.IsApproved = true;
            DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Instance.Reply_Save(portalId, moduleId, reply);
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
            return DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Instance.Add(ProcessType.ApprovedReplyCreated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, badgeId: DotNetNuke.Common.Utilities.Null.NullInteger, dateCreated: DateTime.UtcNow, requestUrl: HttpContext.Current.Request.Url.ToString());
        }

        internal static bool QueueUnapprovedReplyAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Instance.Add(ProcessType.UnapprovedReplyCreated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, badgeId: DotNetNuke.Common.Utilities.Null.NullInteger, dateCreated: DateTime.UtcNow, requestUrl: HttpContext.Current.Request.Url.ToString());
        }

        internal static bool ProcessApprovedReplyAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId, string requestUrl)
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Instance.GetById(moduleId, replyId);
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
                Social.AddPostToJournal(reply);

                var pqc = DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Instance;
                pqc.Add(ProcessType.UpdateForumTopicPointers, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, badgeId: DotNetNuke.Common.Utilities.Null.NullInteger, dateCreated: DateTime.UtcNow, requestUrl: requestUrl);
                pqc.Add(ProcessType.UpdateForumLastUpdated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, badgeId: DotNetNuke.Common.Utilities.Null.NullInteger, dateCreated: DateTime.UtcNow, requestUrl: requestUrl);

                Utilities.UpdateModuleLastContentModifiedOnDate(moduleId);

                if (reply.Content.AuthorId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.UpdateUserReplyCount(portalId, reply.Content.AuthorId);
                }

                reply.Content.ExtractEmbeddedImages();
                DotNetNuke.Modules.ActiveForums.Controllers.TagController.Instance.UpdateTopicTags(reply);
                DotNetNuke.Modules.ActiveForums.Controllers.UserMentionController.ProcessUserMentions(reply);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(reply.ModuleId, string.Format(CacheKeys.ForumInfo, reply.ModuleId, reply.ForumId));
                DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.ForumViewPrefix, reply.ModuleId));
                DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.TopicViewPrefix, reply.ModuleId));
                DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.TopicsViewPrefix, reply.ModuleId));

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
