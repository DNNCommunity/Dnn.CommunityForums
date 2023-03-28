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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Journal;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Controllers.ReplyController")]
    public class ReplyController : Controllers.ReplyController
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Delete()")]
        public void Reply_Delete(int PortalId, int ForumId, int TopicId, int ReplyId, int DelBehavior) { base.Delete(PortalId, ForumId, TopicId, ReplyId, DelBehavior); }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Get()")]
        public ReplyInfo Reply_Get(int PortalId, int ModuleId, int TopicId, int ReplyId) { return (ReplyInfo)base.Get(PortalId, ModuleId, TopicId, ReplyId); }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.QuickCreate()")]
        public int Reply_QuickCreate(int PortalId, int ModuleId, int ForumId, int TopicId, int ReplyToId, string Subject, string Body, int UserId, string DisplayName, bool IsApproved, string IPAddress) { return base.QuickCreate(PortalId, ModuleId, ForumId, TopicId, ReplyToId, Subject, Body, UserId, DisplayName, IsApproved, IPAddress); }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Save()")]
        public int Reply_Save(int PortalId, DotNetNuke.Modules.ActiveForums.ReplyInfo ri) { return base.Save(PortalId,ri); }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Approve()")]
        public Entities.ReplyInfo ApproveReply(int PortalId, int TabId, int ModuleId, int ForumId, int TopicId, int ReplyId) { return (ReplyInfo)base.Approve(PortalId, TabId, ModuleId, ForumId, TopicId, ReplyId); 
        }
    }
}
namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    public class ReplyController
    {
        IDataContext ctx;
        IRepository<Entities.ReplyInfo> repo;
        public ReplyController()
        {
            ctx = DataContext.Instance();
            repo = ctx.GetRepository<Entities.ReplyInfo>();
        }

        public Entities.ReplyInfo Get(int PortalId, int ModuleId, int ReplyId)
        { 
            Entities.ReplyInfo reply = repo.GetById(ReplyId); 
            reply.Content = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().Get(reply.ContentId);
            IDataReader dr = DataProvider.Instance().Reply_Get(PortalId, ModuleId, reply.TopicId, ReplyId); 
            while (dr.Read())
            {
                //reply.ReplyId = Convert.ToInt32(dr["ReplyId"]);
                //reply.ReplyToId = Convert.ToInt32(dr["ReplyToId"]);
                //reply.Content.AuthorId = Convert.ToInt32(dr["AuthorId"]);
                //reply.Content.AuthorName = dr["AuthorName"].ToString();
                //reply.Content.Body = dr["Body"].ToString();
                //reply.Content.ContentId = Convert.ToInt32(dr["ContentId"]);
                //reply.Content.DateCreated = Convert.ToDateTime(dr["DateCreated"]);
                //reply.Content.DateUpdated = Convert.ToDateTime(dr["DateUpdated"]);
                //reply.Content.IsDeleted = Convert.ToBoolean(dr["IsDeleted"]);
                //reply.Content.Subject = dr["Subject"].ToString();
                //reply.Content.Summary = dr["Summary"].ToString();
                //reply.Content.IPAddress = dr["IPAddress"].ToString();
                reply.Author.AuthorId = reply.Content.AuthorId;
                reply.Author.DisplayName = dr["DisplayName"].ToString();
                reply.Author.Email = dr["Email"].ToString();
                reply.Author.FirstName = dr["FirstName"].ToString();
                reply.Author.LastName = dr["LastName"].ToString();
                reply.Author.Username = dr["Username"].ToString();
                //reply.ContentId = Convert.ToInt32(dr["ContentId"]);
                //reply.IsApproved = Convert.ToBoolean(dr["IsApproved"]);
                //reply.IsDeleted = Convert.ToBoolean(dr["IsDeleted"]);
                //reply.StatusId = Convert.ToInt32(dr["StatusId"]);
                //reply.TopicId = Convert.ToInt32(dr["TopicId"]);
            }
            dr.Close();
            return reply;
        }
        public Entities.ReplyInfo Get(int PortalId, int ModuleId, int TopicId, int ReplyId)
        {
            IDataReader dr = DataProvider.Instance().Reply_Get(PortalId, ModuleId, TopicId, ReplyId);
            Entities.ReplyInfo ri = null;
            while (dr.Read())
            {
                ri = new Entities.ReplyInfo();
                ri.ReplyId = Convert.ToInt32(dr["ReplyId"]);
                ri.ReplyToId = Convert.ToInt32(dr["ReplyToId"]);
                ri.Content.AuthorId = Convert.ToInt32(dr["AuthorId"]);
                ri.Content.AuthorName = dr["AuthorName"].ToString();
                ri.Content.Body = dr["Body"].ToString();
                ri.Content.ContentId = Convert.ToInt32(dr["ContentId"]);
                ri.Content.DateCreated = Convert.ToDateTime(dr["DateCreated"]);
                ri.Content.DateUpdated = Convert.ToDateTime(dr["DateUpdated"]);
                ri.Content.IsDeleted = Convert.ToBoolean(dr["IsDeleted"]);
                ri.Content.Subject = dr["Subject"].ToString();
                ri.Content.Summary = dr["Summary"].ToString();
                ri.Content.IPAddress = dr["IPAddress"].ToString();
                ri.Author.AuthorId = ri.Content.AuthorId;
                ri.Author.DisplayName = dr["DisplayName"].ToString();
                ri.Author.Email = dr["Email"].ToString();
                ri.Author.FirstName = dr["FirstName"].ToString();
                ri.Author.LastName = dr["LastName"].ToString();
                ri.Author.Username = dr["Username"].ToString();
                ri.ContentId = Convert.ToInt32(dr["ContentId"]);
                ri.IsApproved = Convert.ToBoolean(dr["IsApproved"]);
                ri.IsDeleted = Convert.ToBoolean(dr["IsDeleted"]);
                ri.StatusId = Convert.ToInt32(dr["StatusId"]);
                ri.TopicId = Convert.ToInt32(dr["TopicId"]);
            }
            dr.Close();
            return ri;
        }

        public void Delete(int PortalId, int ForumId, int TopicId, int ReplyId, int DelBehavior)
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
        public int QuickCreate(int PortalId, int ModuleId, int ForumId, int TopicId, int ReplyToId, string Subject, string Body, int UserId, string DisplayName, bool IsApproved, string IPAddress)
        {
            int replyId = -1;
            Entities.ReplyInfo ri = new Entities.ReplyInfo();
            DateTime dt = DateTime.UtcNow;
            ri.Content.DateUpdated = dt;
            ri.Content.DateCreated = dt;
            ri.Content.AuthorId = UserId;
            ri.Content.AuthorName = DisplayName;
            ri.Content.Subject = Subject;
            ri.Content.Body = Body;
            ri.Content.IPAddress = IPAddress;
            ri.Content.Summary = string.Empty;
            ri.IsApproved = IsApproved;
            ri.IsDeleted = false;
            ri.ReplyToId = ReplyToId;
            ri.StatusId = -1;
            ri.TopicId = TopicId;
            replyId = Save(PortalId, ri);
            UpdateModuleLastContentModifiedOnDate(ModuleId);
            return replyId;
        }
        public int Save(int PortalId, Entities.ReplyInfo reply)
        {
            // Clear profile Cache to make sure the LastPostDate is updated for Flood Control
            UserProfileController.Profiles_ClearCache(reply.Content.AuthorId);

            return Convert.ToInt32(DataProvider.Instance().Reply_Save(PortalId, reply.TopicId, reply.ReplyId, reply.ReplyToId, reply.StatusId, reply.IsApproved, reply.IsDeleted, reply.Content.Subject.Trim(), reply.Content.Body.Trim(), reply.Content.DateCreated, reply.Content.DateUpdated, reply.Content.AuthorId, reply.Content.AuthorName, reply.Content.IPAddress));
        }
        public Entities.ReplyInfo Approve(Entities.ReplyInfo reply)
        {
            throw new NotImplementedException();
        }
        public Entities.ReplyInfo Approve(int PortalId, int TabId, int ModuleId, int ForumId, int TopicId, int ReplyId)
        {
            SettingsInfo ms = DataCache.MainSettings(ModuleId);
            ForumController fc = new ForumController();
            Forum fi = fc.Forums_Get(ForumId, -1, false, true);

            ReplyController rc = new ReplyController();
            Entities.ReplyInfo reply = rc.Get(PortalId, ModuleId, TopicId, ReplyId);
            if (reply == null)
            {
                return null;
            }
            reply.IsApproved = true;
            rc.Save(PortalId, reply);
            TopicsController tc = new TopicsController();
            tc.Topics_SaveToForum(ForumId, TopicId, PortalId, ModuleId, ReplyId);
            TopicInfo topic = tc.Topics_Get(PortalId, ModuleId, TopicId, ForumId, -1, false);

            if (fi.ModApproveTemplateId > 0 & reply.Author.AuthorId > 0)
            {
                Email.SendEmail(fi.ModApproveTemplateId, PortalId, ModuleId, TabId, ForumId, TopicId, ReplyId, string.Empty, reply.Author);
            }

            Subscriptions.SendSubscriptions(PortalId, ModuleId, TabId, ForumId, TopicId, ReplyId, reply.Content.AuthorId);

            try
            {
                ControlUtils ctlUtils = new ControlUtils();
                string fullURL = ctlUtils.BuildUrl(TabId, ModuleId, fi.ForumGroup.PrefixURL, fi.PrefixURL, fi.ForumGroupId, ForumId, TopicId, topic.TopicUrl, -1, -1, string.Empty, 1, ReplyId, fi.SocialGroupId);

                if (fullURL.Contains("~/"))
                {
                    fullURL = Utilities.NavigateUrl(TabId, "", new string[] { ParamKeys.TopicId + "=" + TopicId, ParamKeys.ContentJumpId + "=" + ReplyId });
                }
                if (fullURL.EndsWith("/"))
                {
                    fullURL += "?" + ParamKeys.ContentJumpId + "=" + ReplyId;
                }
                Social amas = new Social();
                amas.AddReplyToJournal(PortalId, ModuleId, ForumId, TopicId, ReplyId, reply.Author.AuthorId, fullURL, reply.Content.Subject, string.Empty, reply.Content.Body, fi.ActiveSocialSecurityOption, fi.Security.Read, fi.SocialGroupId);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return reply;
        }
        public void UpdateModuleLastContentModifiedOnDate(int ModuleId)
        {
            // signal to platform that module has updated content in order to be included in incremental search crawls
            DotNetNuke.Data.DataProvider.Instance().UpdateModuleLastContentModifiedOnDate(ModuleId);
        }

    }
}

