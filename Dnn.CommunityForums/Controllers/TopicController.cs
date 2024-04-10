//T
// Community Forums
// Copyright (c) 2013-2021
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
using DotNetNuke.Modules.ActiveForums.Entities;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Journal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal class TopicController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetById(int TopicId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = base.GetById(TopicId);
            if (ti != null)
            {
                ti.GetForum();
                ti.GetContent();
                ti.GetAuthor();
            }
            return ti;
        }
        public static int QuickCreate(int PortalId, int ModuleId, int ForumId, string Subject, string Body, int UserId, string DisplayName, bool IsApproved, string IPAddress)
        {
            int topicId = -1;
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo(); 
            ti.Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo();

            ti.ForumId = ForumId;
            ti.AnnounceEnd = Utilities.NullDate();
            ti.AnnounceStart = Utilities.NullDate();
            ti.Content.AuthorId = UserId;
            ti.Content.AuthorName = DisplayName;
            ti.Content.Subject = Subject;
            ti.Content.Body = Body;
            ti.Content.Summary = string.Empty;

            ti.Content.IPAddress = IPAddress;

            ti.IsAnnounce = false;
            ti.IsApproved = IsApproved;
            ti.IsArchived = false;
            ti.IsDeleted = false;
            ti.IsLocked = false;
            ti.IsPinned = false;
            ti.ReplyCount = 0;
            ti.StatusId = -1;
            ti.TopicIcon = string.Empty;
            ti.TopicType = TopicTypes.Topic;
            ti.ViewCount = 0;
            topicId = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);


            if (topicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(ModuleId, ForumId, topicId, -1);
                if (UserId > 0)
                {
                    //TODO: update this to be consistent with reply count
                    Data.Profiles uc = new Data.Profiles();
                    uc.Profile_UpdateTopicCount(PortalId, UserId);
                }
            }
            return topicId;
        }
        public static void Replies_Split(int OldTopicId, int NewTopicId, string listreplies, bool isNew)
        {
            Regex rgx = new Regex(@"^\d+(\|\d+)*$");
            if (OldTopicId > 0 && NewTopicId > 0 && rgx.IsMatch(listreplies))
            {
                if (isNew)
                {
                    string[] slistreplies = listreplies.Split("|".ToCharArray(), 2);
                    string str = "";
                    if (slistreplies.Length > 1) str = slistreplies[1];
                    DataProvider.Instance().Replies_Split(OldTopicId, NewTopicId, str, DateTime.Now, Convert.ToInt32(slistreplies[0]));
                }
                else
                {
                    DataProvider.Instance().Replies_Split(OldTopicId, NewTopicId, listreplies, DateTime.Now, 0);
                }
            }
        }
        public static DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Approve(int TopicId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
            if (ti == null)
            {
                return null;
            }
            ti.IsApproved = true;
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(ti.ModuleId, ti.ForumId, TopicId);

            if (ti.Forum.ModApproveTemplateId > 0 & ti.Author.AuthorId > 0)
            {
                Email.SendEmail(ti.Forum.ModApproveTemplateId, ti.PortalId, ti.ModuleId, ti.Forum.TabId, ti.ForumId, TopicId, 0, string.Empty, ti.Author);
            }

            Subscriptions.SendSubscriptions(ti.PortalId, ti.ModuleId, ti.Forum.TabId, ti.ForumId, TopicId, 0, ti.Content.AuthorId);

            try
            {
                ControlUtils ctlUtils = new ControlUtils();
                string sUrl = ctlUtils.BuildUrl(ti.Forum.TabId, ti.ModuleId, ti.Forum.ForumGroup.PrefixURL, ti.Forum.PrefixURL, ti.Forum.ForumGroupId, ti.Forum.ForumID, TopicId, ti.TopicUrl, -1, -1, string.Empty, 1, -1, ti.Forum.SocialGroupId);
                Social amas = new Social();
                amas.AddTopicToJournal(ti.PortalId, ti.ModuleId, ti.Forum.TabId, ti.ForumId, TopicId, ti.Author.AuthorId, sUrl, ti.Content.Subject, string.Empty, ti.Content.Body, ti.Forum.Security.Read, ti.Forum.SocialGroupId);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return ti;
        }
        public static void Move(int TopicId, int NewForumId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
            SettingsInfo settings = SettingsBase.GetModuleSettings(ti.ModuleId);
            if (settings.URLRewriteEnabled)
            {
                try
                {
                    int oldForumId = (int)ti.ForumId;
                    if (!(string.IsNullOrEmpty(ti.Forum.PrefixURL)))
                    {
                        Data.Common dbC = new Data.Common();
                        string sURL = dbC.GetUrl(ti.ModuleId, ti.Forum.ForumGroupId, oldForumId, TopicId, -1, -1);
                        if (!(string.IsNullOrEmpty(sURL)))
                        {
                            dbC.ArchiveURL(ti.PortalId, ti.Forum.ForumGroupId, NewForumId, TopicId, sURL);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
            DataProvider.Instance().Topics_Move(ti.PortalId, ti.ModuleId, NewForumId, TopicId);
            Utilities.UpdateModuleLastContentModifiedOnDate(ti.ModuleId);
        }
        public static int SaveToForum(int ModuleId, int ForumId, int TopicId, int LastReplyId = -1)
        {
            Utilities.UpdateModuleLastContentModifiedOnDate(ModuleId);
            return Convert.ToInt32(DataProvider.Instance().Topics_SaveToForum(ForumId, TopicId, LastReplyId));
        }
        public static int Save(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            ti.Content.DateUpdated = DateTime.UtcNow;
            if (ti.TopicId < 1)
            {
                ti.Content.DateCreated = DateTime.UtcNow;
            }
            UserProfileController.Profiles_ClearCache(ti.ModuleId, ti.Content.AuthorId);
            if (ti.IsApproved && ti.Author.AuthorId > 0)
            {   //TODO: put this in a better place and make it consistent with reply counter
                var uc = new Data.Profiles();
                uc.Profile_UpdateTopicCount(ti.Forum.PortalId, ti.Author.AuthorId);
            }
            //TODO: convert to use DAL2?
            return Convert.ToInt32(DataProvider.Instance().Topics_Save(ti.Forum.PortalId, ti.TopicId, ti.ViewCount, ti.ReplyCount, ti.IsLocked, ti.IsPinned, ti.TopicIcon, ti.StatusId, ti.IsApproved, ti.IsDeleted, ti.IsAnnounce, ti.IsArchived, ti.AnnounceStart, ti.AnnounceEnd, ti.Content.Subject.Trim(), ti.Content.Body.Trim(), ti.Content.Summary.Trim(), ti.Content.DateCreated, ti.Content.DateUpdated, ti.Content.AuthorId, ti.Content.AuthorName, ti.Content.IPAddress, (int)ti.TopicType, ti.Priority, ti.TopicUrl, ti.TopicData));
        }
        public void DeleteById(int TopicId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = base.GetById(TopicId);
            if (ti != null)
            {
                try
                {
                    var objectKey = string.Format("{0}:{1}", ti.ForumId, TopicId);
                    JournalController.Instance.DeleteJournalItemByKey(ti.PortalId, objectKey);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }

                DataProvider.Instance().Topics_Delete(ti.ForumId, TopicId, SettingsBase.GetModuleSettings(ti.ModuleId).DeleteBehavior );
                Utilities.UpdateModuleLastContentModifiedOnDate(ti.ModuleId);
                DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.ForumViewPrefix, ti.ModuleId));

                if (SettingsBase.GetModuleSettings(ti.ModuleId).DeleteBehavior != 0)
                    return;

                // If it's a hard delete, delete associated attachments
                var attachmentController = new Data.AttachController();
                var fileManager = FileManager.Instance;
                var folderManager = FolderManager.Instance;
                var attachmentFolder = folderManager.GetFolder(ti.PortalId, "activeforums_Attach");

                foreach (var attachment in attachmentController.ListForPost(TopicId, null))
                {
                    attachmentController.Delete(attachment.AttachmentId);

                    var file = attachment.FileId.HasValue ? fileManager.GetFile(attachment.FileId.Value) : fileManager.GetFile(attachmentFolder, attachment.FileName);

                    // Only delete the file if it exists in the attachment folder
                    if (file != null && file.FolderId == attachmentFolder.FolderID)
                        fileManager.DeleteFile(file);
                }
            }
        }

        public static string GetTopicIcon(int topicId, bool isRead, string themePath, int userLastTopicRead, int userLastReplyRead)
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
            else if (isRead)
            {
                return $"{themePath}/images/topic.png";
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
    }
}
