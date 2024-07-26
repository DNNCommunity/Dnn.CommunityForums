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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Http.Controllers;
    using System.Xml.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.ActiveForums.API;
    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Social.Notifications;

    internal class TopicController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetById(int topicId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = base.GetById(topicId);
            if (ti != null)
            {
                ti.GetForum();
                ti.GetContent();
                ti.GetAuthor();
            }

            return ti;
        }

        public static int QuickCreate(int portalId, int moduleId, int forumId, string subject, string body, int userId, string displayName, bool isApproved, string iPAddress)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId);
            int topicId = -1;
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo();
            ti.Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo();

            ti.ForumId = forumId;
            ti.AnnounceEnd = Utilities.NullDate();
            ti.AnnounceStart = Utilities.NullDate();
            ti.Content.AuthorId = userId;
            ti.Content.AuthorName = displayName;
            ti.Content.Subject = subject;
            ti.Content.Body = body;
            ti.Content.Summary = string.Empty;

            ti.Content.IPAddress = iPAddress;

            ti.IsAnnounce = false;
            ti.IsApproved = isApproved;
            ti.IsArchived = false;
            ti.IsDeleted = false;
            ti.IsLocked = false;
            ti.IsPinned = false;
            ti.ReplyCount = 0;
            ti.StatusId = -1;
            ti.TopicIcon = string.Empty;
            ti.TopicType = TopicTypes.Topic;
            ti.ViewCount = 0;
            ti.TopicUrl = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildTopicUrl(portalId: portalId, moduleId: moduleId, topicId: topicId, subject: subject, forumInfo: forumInfo);

            topicId = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);

            if (topicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(moduleId, forumId, topicId, -1);
            }

            return topicId;
        }

        public static void Replies_Split(int oldTopicId, int newTopicId, string listreplies, bool isNew)
        {
            Regex rgx = new Regex(@"^\d+(\|\d+)*$");
            if (oldTopicId > 0 && newTopicId > 0 && rgx.IsMatch(listreplies))
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
        }

        public static DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Approve(int topicId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
            if (ti == null)
            {
                return null;
            }

            ti.IsApproved = true;
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(ti.ModuleId, ti.ForumId, topicId);

            DataCache.ContentCacheClear(ti.ModuleId, string.Format(CacheKeys.ForumInfo, ti.ModuleId, ti.ForumId));
            DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.ForumViewPrefix, ti.ModuleId));
            DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.TopicViewPrefix, ti.ModuleId));
            DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, ti.ModuleId));

            if (ti.Forum.ModApproveTemplateId > 0 & ti.Author.AuthorId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(ti.Forum.ModApproveTemplateId, ti.PortalId, ti.ModuleId, ti.Forum.TabId, ti.ForumId, topicId, 0, string.Empty, ti.Author);
            }

            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.QueueApprovedTopicAfterAction(ti.PortalId, ti.Forum.TabId, ti.Forum.ModuleId, ti.Forum.ForumGroupId, ti.ForumId, topicId, -1, ti.Content.AuthorId);
            return ti;
        }

        public static void Move(int topicId, int newForumId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
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

            var oldForum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(oldForumId);
            var newForum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(newForumId);

            DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_Move(ti.PortalId, ti.ModuleId, newForumId, topicId);
            ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);

            if (oldForum.ModMoveTemplateId > 0 & ti?.Author?.AuthorId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(oldForum.ModMoveTemplateId, ti.PortalId, ti.ModuleId, ti.Forum.TabId, forumId: ti.Forum.ForumID, ti.TopicId, -1, string.Empty, ti.Author);
            }

            new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.UpdateForumLastUpdated, ti.PortalId, tabId: -1, moduleId: ti.ModuleId, forumGroupId: oldForum.ForumGroupId, forumId: oldForum.ForumID, topicId: topicId, replyId: -1, authorId: ti.Content.AuthorId, requestUrl: null);
            new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.UpdateForumLastUpdated, ti.PortalId, tabId: -1, moduleId: ti.ModuleId, forumGroupId: newForum.ForumGroupId, forumId: newForum.ForumID, topicId: topicId, replyId: -1, authorId: ti.Content.AuthorId, requestUrl: null);
            Utilities.UpdateModuleLastContentModifiedOnDate(ti.ModuleId);
            DataCache.ContentCacheClear(ti.ModuleId, string.Format(CacheKeys.ForumInfo, ti.ModuleId, ti.ForumId));
            DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.ForumViewPrefix, ti.ModuleId));
            DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.TopicViewPrefix, ti.ModuleId));
            DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, ti.ModuleId));
        }

        public static void SaveToForum(int moduleId, int forumId, int topicId, int lastReplyId = -1)
        {
            DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_SaveToForum(forumId, topicId, lastReplyId);
            Utilities.UpdateModuleLastContentModifiedOnDate(moduleId);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(forumId);
            DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.ForumInfo, moduleId, forumId));
            DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.ForumViewPrefix, moduleId));
            DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.TopicViewPrefix, moduleId));
            DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.TopicsViewPrefix, moduleId));
        }

        public static int Save(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            ti.Content.DateUpdated = DateTime.UtcNow;
            if (ti.TopicId < 1)
            {
                ti.Content.DateCreated = DateTime.UtcNow;
            }

            UserProfileController.Profiles_ClearCache(ti.ModuleId, ti.Content.AuthorId);

            Utilities.UpdateModuleLastContentModifiedOnDate(ti.ModuleId);
            DataCache.ContentCacheClear(ti.ModuleId, string.Format(CacheKeys.ForumInfo, ti.ModuleId, ti.ForumId));
            DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.ForumViewPrefix, ti.ModuleId));
            DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.TopicViewPrefix, ti.ModuleId));
            DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, ti.ModuleId));

            // TODO: convert to use DAL2?
            int topicId = Convert.ToInt32(DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_Save(ti.Forum.PortalId, ti.TopicId, ti.ViewCount, ti.ReplyCount, ti.IsLocked, ti.IsPinned, ti.TopicIcon, ti.StatusId, ti.IsApproved, ti.IsDeleted, ti.IsAnnounce, ti.IsArchived, ti.AnnounceStart, ti.AnnounceEnd, ti.Content.Subject.Trim(), ti.Content.Body.Trim(), ti.Content.Summary.Trim(), ti.Content.DateCreated, ti.Content.DateUpdated, ti.Content.AuthorId, ti.Content.AuthorName, ti.Content.IPAddress, (int)ti.TopicType, ti.Priority, ti.TopicUrl, ti.TopicData));
            if (ti.IsApproved && ti.Author.AuthorId > 0)
            {   // TODO: put this in a better place and make it consistent with reply counter
                var uc = new Data.Profiles();
                uc.Profile_UpdateTopicCount(ti.Forum.PortalId, ti.Author.AuthorId);
            }

            return topicId;
        }

        public void DeleteById(int topicId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = base.GetById(topicId);
            if (ti != null)
            {
                try
                {
                    var objectKey = string.Format("{0}:{1}", ti.ForumId, topicId);
                    JournalController.Instance.DeleteJournalItemByKey(ti.PortalId, objectKey);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }

                DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_Delete(ti.ForumId, topicId, SettingsBase.GetModuleSettings(ti.ModuleId).DeleteBehavior );
                Utilities.UpdateModuleLastContentModifiedOnDate(ti.ModuleId);
                DataCache.ContentCacheClear(ti.ModuleId, string.Format(CacheKeys.ForumInfo, ti.ModuleId, ti.ForumId));
                DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.ForumViewPrefix, ti.ModuleId));
                DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.TopicViewPrefix, ti.ModuleId));
                DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.TopicsViewPrefix, ti.ModuleId));

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

        internal static string GetTopicIcon(int topicId, string themePath, int userLastTopicRead, int userLastReplyRead)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
            if (!string.IsNullOrWhiteSpace(ti.TopicIcon))
            {
                return $"{themePath}/emoticons/{ti.TopicIcon}";
            }
            else if (ti.IsPinned && ti.IsLocked)
            {
                return $"{themePath}/images/topic_pinlocked.png";
            }
            else if (ti.IsPinned)
            {
                return $"{themePath}/images/topic_pin.png";
            }
            else if (ti.IsLocked)
            {
                return $"{themePath}/images/topic_lock.png";
            }
            else
            {
                // Unread has to be calculated based on a few fields
                if ((ti.ReplyCount <= 0 && topicId > userLastTopicRead) || (ti.Forum.LastReplyId > userLastReplyRead))
                {
                    return $"{themePath}/images/topic.png";
                }
                else
                {
                    return $"{themePath}/images/topic_new.png";
                }
            }
        }

        internal static bool QueueApprovedTopicAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int authorId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.ApprovedTopicCreated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, authorId: authorId, requestUrl: HttpContext.Current.Request.Url.ToString());
        }

        internal static bool QueueUnapprovedTopicAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int authorId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController().Add(ProcessType.UnapprovedTopicCreated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, authorId: authorId, requestUrl: HttpContext.Current.Request.Url.ToString());
        }

        internal static bool ProcessApprovedTopicAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int authorId, string requestUrl)
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                Subscriptions.SendSubscriptions(-1, portalId, moduleId, tabId, topic.Forum, topicId, 0, topic.Content.AuthorId, new Uri(requestUrl));

                string sUrl = new ControlUtils().BuildUrl(tabId, moduleId, topic.Forum.ForumGroup.PrefixURL, topic.Forum.PrefixURL, topic.Forum.ForumGroupId, forumId, topicId, topic.TopicUrl, -1, -1, string.Empty, 1, -1, topic.Forum.SocialGroupId);

                Social amas = new Social();
                amas.AddTopicToJournal(portalId, moduleId, tabId, forumId, topicId, topic.Author.AuthorId, sUrl, topic.Content.Subject, string.Empty, topic.Content.Body, topic.Forum.Security.Read, topic.Forum.SocialGroupId);

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

        internal static bool ProcessUnapprovedTopicAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int authorId, string requestUrl)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.SendModerationNotification(portalId, tabId, moduleId, forumGroupId, forumId, topicId, replyId, authorId, requestUrl);
        }
    }
}
