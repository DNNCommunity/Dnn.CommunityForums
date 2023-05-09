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
using System.Reflection;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.ActiveForums.API;
using DotNetNuke.Modules.ActiveForums.DAL2;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Modules.ActiveForums.Entities;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Journal;
using DotNetNuke.Services.Social.Notifications;

namespace DotNetNuke.Modules.ActiveForums
{
#region ReplyInfo
	public class ReplyInfo
	{
#region Private Members
		private int _ReplyId;
		private int _TopicId;
		private int _ReplyToId;
		private int _ContentId;
		private int _StatusId;
		private bool _IsApproved;
		private bool _IsDeleted;
		private Content _Content;
		private Author _Author;
#endregion
#region Public Properties
		public int ReplyId
		{
			get
			{
				return _ReplyId;
			}
			set
			{
				_ReplyId = value;
			}
		}
		public int ReplyToId
		{
			get
			{
				return _ReplyToId;
			}
			set
			{
				_ReplyToId = value;
			}
		}
		public int TopicId
		{
			get
			{
				return _TopicId;
			}
			set
			{
				_TopicId = value;
			}
		}
		public int ContentId
		{
			get
			{
				return _ContentId;
			}
			set
			{
				_ContentId = value;
			}
		}
		public int StatusId
		{
			get
			{
				return _StatusId;
			}
			set
			{
				_StatusId = value;
			}
		}
		public bool IsApproved
		{
			get
			{
				return _IsApproved;
			}
			set
			{
				_IsApproved = value;
			}
		}
		public bool IsDeleted
		{
			get
			{
				return _IsDeleted;
			}
			set
			{
				_IsDeleted = value;
			}
		}
		public Content Content
		{
			get
			{
				return _Content;
			}
			set
			{
				_Content = value;
			}
		}
		public Author Author
		{
			get
			{
				return _Author;
			}
			set
			{
				_Author = value;
			}
		}
#endregion
		public ReplyInfo()
		{
			Content = new Content();
			Author = new Author();
		}
	}
#endregion
#region Reply Controller
	public class ReplyController
	{
#region Public Methods
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

		    foreach(var attachment in attachmentController.ListForPost(TopicId, ReplyId))
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
			ReplyInfo ri = new ReplyInfo();
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
			replyId = Reply_Save(PortalId, ri);
            UpdateModuleLastContentModifiedOnDate(ModuleId);
            return replyId;
		}
		public int Reply_Save(int PortalId, ReplyInfo ri)
		{
			// Clear profile Cache to make sure the LastPostDate is updated for Flood Control
			UserProfileController.Profiles_ClearCache(ri.Content.AuthorId);

            return Convert.ToInt32(DataProvider.Instance().Reply_Save(PortalId, ri.TopicId, ri.ReplyId, ri.ReplyToId, ri.StatusId, ri.IsApproved, ri.IsDeleted, ri.Content.Subject.Trim(), ri.Content.Body.Trim(), ri.Content.DateCreated, ri.Content.DateUpdated, ri.Content.AuthorId, ri.Content.AuthorName, ri.Content.IPAddress));
		}
		public ReplyInfo Reply_Get(int PortalId, int ModuleId, int TopicId, int ReplyId)
		{
			IDataReader dr = DataProvider.Instance().Reply_Get(PortalId, ModuleId, TopicId, ReplyId);
			ReplyInfo reply = null;
			while (dr.Read())
			{
				reply = new ReplyInfo();
				reply.ReplyId = Convert.ToInt32(dr["ReplyId"]);
				reply.ReplyToId = Convert.ToInt32(dr["ReplyToId"]);
				reply.Content.AuthorId = Convert.ToInt32(dr["AuthorId"]);
				reply.Content.AuthorName = dr["AuthorName"].ToString();
				reply.Content.Body = dr["Body"].ToString();
				reply.Content.ContentId = Convert.ToInt32(dr["ContentId"]);
				reply.Content.DateCreated = Convert.ToDateTime(dr["DateCreated"]);
				reply.Content.DateUpdated = Convert.ToDateTime(dr["DateUpdated"]);
				reply.Content.IsDeleted = Convert.ToBoolean(dr["IsDeleted"]);
				reply.Content.Subject = dr["Subject"].ToString();
				reply.Content.Summary = dr["Summary"].ToString();
				reply.Content.IPAddress = dr["IPAddress"].ToString();
				reply.Author.AuthorId = reply.Content.AuthorId;
				reply.Author.DisplayName = dr["DisplayName"].ToString();
				reply.Author.Email = dr["Email"].ToString();
				reply.Author.FirstName = dr["FirstName"].ToString();
				reply.Author.LastName = dr["LastName"].ToString();
				reply.Author.Username = dr["Username"].ToString();
				reply.ContentId = Convert.ToInt32(dr["ContentId"]);
				reply.IsApproved = Convert.ToBoolean(dr["IsApproved"]);
				reply.IsDeleted = Convert.ToBoolean(dr["IsDeleted"]);
				reply.StatusId = Convert.ToInt32(dr["StatusId"]);
				reply.TopicId = Convert.ToInt32(dr["TopicId"]);
			}
			dr.Close();
			return reply;
		}
		public ReplyInfo ApproveReply(int PortalId, int TabId, int ModuleId, int ForumId, int TopicId, int ReplyId)
        {
            ForumController fc = new ForumController();
            Forum forum = fc.Forums_Get(ForumId, -1, false, true);

            ReplyController rc = new ReplyController();
            ReplyInfo reply = rc.Reply_Get(PortalId, ModuleId, TopicId, ReplyId);
            if (reply == null)
            {
                return null;
            }
            reply.IsApproved = true;
            rc.Reply_Save(PortalId, reply);
            TopicsController tc = new TopicsController();
            tc.Topics_SaveToForum(ForumId, TopicId, PortalId, ModuleId, ReplyId);
            TopicInfo topic = tc.Topics_Get(PortalId, ModuleId, TopicId, ForumId, -1, false);

            if (forum.ModApproveTemplateId > 0 & reply.Author.AuthorId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(forum.ModApproveTemplateId, PortalId, ModuleId, TabId, ForumId, TopicId, ReplyId, string.Empty, reply.Author);
            }
            QueueApprovedReplyAfterAction(PortalId, TabId, ModuleId,forum.ForumGroupId, ForumId, TopicId, ReplyId, reply.Content.AuthorId);
            return reply;
        }
        internal static void QueueApprovedReplyAfterAction(int PortalId, int TabId, int ModuleId, int ForumGroupId, int ForumId, int TopicId, int ReplyId, int AuthorId)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Add(ProcessType.ApprovedReplyCreated, PortalId, ModuleId, ForumGroupId, ForumId, TopicId, ReplyId, AuthorId);
        }
        internal static void QueueUnapprovedReplyAfterAction(int PortalId, int TabId, int ModuleId, int ForumGroupId, int ForumId, int TopicId, int ReplyId, int AuthorId)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Add(ProcessType.UnapprovedReplyCreated, PortalId, ModuleId, ForumGroupId, ForumId, TopicId, ReplyId, AuthorId);
        }
        internal static void ProcessApprovedReplyAfterAction(int PortalId, int TabId, int ModuleId, int ForumId, int TopicId, int ReplyId)
        {
            try
            {
				DotNetNuke.Modules.ActiveForums.ForumController fc = new DotNetNuke.Modules.ActiveForums.ForumController();
				DotNetNuke.Modules.ActiveForums.TopicsController tc = new DotNetNuke.Modules.ActiveForums.TopicsController();
				DotNetNuke.Modules.ActiveForums.ReplyController rc = new DotNetNuke.Modules.ActiveForums.ReplyController();
				Forum forum = fc.GetForum(PortalId, ModuleId, ForumId);
				TopicInfo topic = tc.Topics_Get(PortalId, ModuleId, ForumId);
				ReplyInfo reply = rc.Reply_Get(PortalId, ModuleId, TopicId, ReplyId);
				Subscriptions.SendSubscriptions(PortalId, ModuleId, TabId, ForumId, TopicId, ReplyId, reply.Content.AuthorId);

                ControlUtils ctlUtils = new ControlUtils();
                string fullURL = ctlUtils.BuildUrl(TabId, ModuleId, forum.ForumGroup.PrefixURL, forum.PrefixURL, forum.ForumGroupId, ForumId, TopicId, topic.TopicUrl, -1, -1, string.Empty, 1, ReplyId, forum.SocialGroupId);

                if (fullURL.Contains("~/"))
                {
                    fullURL = Utilities.NavigateUrl(TabId, "", new string[] { ParamKeys.TopicId + "=" + TopicId, ParamKeys.ContentJumpId + "=" + ReplyId });
                }
                if (fullURL.EndsWith("/"))
                {
                    fullURL += "?" + ParamKeys.ContentJumpId + "=" + ReplyId;
                }
                Social amas = new Social();
                amas.AddReplyToJournal(PortalId, ModuleId, ForumId, TopicId, ReplyId, reply.Author.AuthorId, fullURL, reply.Content.Subject, string.Empty, reply.Content.Body, forum.Security.Read, forum.SocialGroupId);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }
        internal static void ProcessUnapprovedReplyAfterAction(int PortalId, int TabId, int ModuleId, int ForumId, int TopicId, int ReplyId)
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.ForumController fc = new DotNetNuke.Modules.ActiveForums.ForumController();
				DotNetNuke.Modules.ActiveForums.TopicsController tc = new DotNetNuke.Modules.ActiveForums.TopicsController(); 
				DotNetNuke.Modules.ActiveForums.ReplyController rc = new DotNetNuke.Modules.ActiveForums.ReplyController();
				Forum forum = fc.GetForum(PortalId, ModuleId, ForumId);
				TopicInfo topic = tc.Topics_Get(PortalId, ModuleId, ForumId);
				ReplyInfo reply = rc.Reply_Get(PortalId, ModuleId, TopicId, ReplyId);

                List<DotNetNuke.Entities.Users.UserInfo> mods = Utilities.GetListOfModerators(PortalId, ForumId);
                NotificationType notificationType = NotificationsController.Instance.GetNotificationType("AF-ForumModeration");
                string subject = Utilities.GetSharedResource("NotificationSubjectReply");
				subject = subject.Replace("[DisplayName]", reply.Content.AuthorName);
                subject = subject.Replace("[TopicSubject]", topic.Content.Subject);
                string body = Utilities.GetSharedResource("NotificationBodyReply");
                body = body.Replace("[Post]", reply.Content.Body);
                string notificationKey = string.Format("{0}:{1}:{2}:{3}:{4}", TabId, ModuleId, ForumId, TopicId, ReplyId);

                Notification notification = new Notification();
                notification.NotificationTypeID = notificationType.NotificationTypeId;
                notification.Subject = subject;
                notification.Body = body;
                notification.IncludeDismissAction = false;
				notification.SenderUserID = reply.Content.AuthorId;
                notification.Context = notificationKey;

                NotificationsController.Instance.SendNotification(notification, PortalId, null, mods);

            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        public void UpdateModuleLastContentModifiedOnDate(int ModuleId)
        {
            // signal to platform that module has updated content in order to be included in incremental search crawls
            DotNetNuke.Data.DataProvider.Instance().UpdateModuleLastContentModifiedOnDate(ModuleId);
        }

        #endregion
    }
    #endregion

}

