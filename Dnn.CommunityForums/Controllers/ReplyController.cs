//
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
using DotNetNuke.Modules.ActiveForums.Entities;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Journal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

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
            DataProvider.Instance().Reply_Delete(ForumId, TopicId, ReplyId, DelBehavior);
            var objectKey = string.Format("{0}:{1}:{2}", ForumId, TopicId, ReplyId);
            JournalController.Instance.DeleteJournalItemByKey(PortalId, objectKey);

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
            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri = new DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo();
            ri.TopicId = TopicId;
            ri.ReplyToId = ReplyToId;
            ri.IsApproved = IsApproved;
            ri.IsDeleted = false;
            ri.StatusId = -1;
            ri.Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo();
            ri.Content.AuthorId = UserId;
            ri.Content.AuthorName = DisplayName;
            ri.Content.Subject = Subject;
            ri.Content.Body = Body;
            ri.Content.IPAddress = IPAddress;
            ri.Content.Summary = string.Empty;
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
            // Clear profile Cache to make sure the LastPostDate is updated for Flood Control
            UserProfileController.Profiles_ClearCache(ModuleId, ri.Content.AuthorId);

            return Convert.ToInt32(DataProvider.Instance().Reply_Save(PortalId, ri.TopicId, ri.ReplyId, ri.ReplyToId, ri.StatusId, ri.IsApproved, ri.IsDeleted, ri.Content.Subject.Trim(), ri.Content.Body.Trim(), ri.Content.DateCreated, ri.Content.DateUpdated, ri.Content.AuthorId, ri.Content.AuthorName, ri.Content.IPAddress));
        }
        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo Reply_Get(int PortalId, int ModuleId, int TopicId, int ReplyId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(ReplyId);

        }
        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ApproveReply(int PortalId, int TabId, int ModuleId, int ForumId, int TopicId, int ReplyId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: ForumId, moduleId: ModuleId);

            ReplyController rc = new ReplyController();
            DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = rc.Reply_Get(PortalId, ModuleId, TopicId, ReplyId);
            if (reply == null)
            {
                return null;
            }
            reply.IsApproved = true;
            rc.Reply_Save(PortalId, ModuleId, reply);
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(ModuleId, ForumId, TopicId, ReplyId);
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);

            if (fi.ModApproveTemplateId > 0 & reply.Author.AuthorId > 0)
            {
                Email.SendEmail(fi.ModApproveTemplateId, PortalId, ModuleId, TabId, ForumId, TopicId, ReplyId, string.Empty, reply.Author);
            }
            Subscriptions.SendSubscriptions(-1, PortalId, ModuleId, TabId, fi, TopicId, ReplyId, reply.Content.AuthorId);
            
            try
            {
                ControlUtils ctlUtils = new ControlUtils();
                string fullURL = ctlUtils.BuildUrl(TabId, ModuleId, fi.ForumGroup.PrefixURL, fi.PrefixURL, fi.ForumGroupId, ForumId, TopicId, topic.TopicUrl, -1, -1, string.Empty, 1, ReplyId, fi.SocialGroupId);

                if (fullURL.Contains("~/"))
                {
                    fullURL = Utilities.NavigateURL(TabId, "", new string[] { ParamKeys.TopicId + "=" + TopicId, ParamKeys.ContentJumpId + "=" + ReplyId });
                }
                if (fullURL.EndsWith("/"))
                {
                    fullURL += Utilities.UseFriendlyURLs(ModuleId) ? String.Concat("#", ReplyId) : String.Concat("?", ParamKeys.ContentJumpId, "=", ReplyId);
                }
                Social amas = new Social();
                amas.AddReplyToJournal(PortalId, ModuleId, TabId, ForumId, TopicId, ReplyId, reply.Author.AuthorId, fullURL, reply.Content.Subject, string.Empty, reply.Content.Body, fi.Security.Read, fi.SocialGroupId);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return reply;
        }
    }
}
