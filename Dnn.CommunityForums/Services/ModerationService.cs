// Copyright (c) 2013-2024 by DNN Community
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

namespace DotNetNuke.Modules.ActiveForums
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Web.Http;

    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;
    using global::DotNetNuke.Entities.Portals;

    [ValidateAntiForgeryToken]
    public class ModerationServiceController : DnnApiController
    {
        private int tabId = -1;
        private int moduleId = -1;
        private int forumId = -1;
        private int topicId = -1;
        private int replyId = -1;

        // For the Notification API, return an object with "Result" property set to "success" if the operation succeeded.
        // In there is an error, return the error message in the "Message" property.
        // In both cases, it must return an 200 "OK" response.
        [DnnAuthorize]
        [HttpPost]
        public HttpResponseMessage ApprovePost(ModerationDTO dto)
        {
            var notify = NotificationsController.Instance.GetNotification(dto.NotificationId);

            this.ParseNotificationContext(notify.Context);

            var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.forumId, this.moduleId);
            if (fi == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Forum Not Found" });
            }

            if (!this.IsMod(this.forumId))
            {
                return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "User is not a moderator for this forum" });
            }

            if (this.replyId > 0)
            {
                var reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.moduleId).ApproveReply(portalId: this.PortalSettings.PortalId, tabId: this.tabId, moduleId: this.moduleId, forumId: this.forumId, topicId: this.topicId, replyId: this.replyId, this.UserInfo.UserID);
                if (reply == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Reply Not Found" });
                }
            }
            else
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Approve(this.moduleId, this.topicId, this.UserInfo.UserID);
                if (topic == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Topic Not Found" });
                }
            }

            NotificationsController.Instance.DeleteNotification(dto.NotificationId);

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

        [DnnAuthorize]
        [HttpPost]
        public HttpResponseMessage RejectPost(ModerationDTO dto)
        {
            var notify = NotificationsController.Instance.GetNotification(dto.NotificationId);

            this.ParseNotificationContext(notify.Context);

            var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.forumId, this.moduleId);
            if (fi == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Forum Not Found" });
            }

            if (!this.IsMod(this.forumId))
            {
                return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "User is not a moderator for this forum" });
            }

            var mc = new ModController();
            mc.Mod_Reject(this.PortalSettings.PortalId, this.moduleId, this.UserInfo.UserID, this.forumId, this.topicId, this.replyId);

            int authorId;

            if (this.replyId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.moduleId).GetById(this.replyId);

                if (reply == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Reply Not Found" });
                }

                authorId = reply.Content.AuthorId;
            }
            else
            {
                var topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.moduleId).GetById(this.topicId);
                if (topic == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Topic Not Found" });
                }

                authorId = topic.Content.AuthorId;
            }

            if (fi.FeatureSettings.ModRejectTemplateId > 0 && authorId > 0)
            {
                var uc = new DotNetNuke.Entities.Users.UserController();
                var ui = uc.GetUser(this.PortalSettings.PortalId, authorId);
                if (ui != null)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo au = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(this.moduleId)
                    {
                        AuthorId = authorId,
                        DisplayName = ui.DisplayName,
                        Email = ui.Email,
                        FirstName = ui.FirstName,
                        LastName = ui.LastName,
                        Username = ui.Username,
                    };
                    DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(fi.FeatureSettings.ModRejectTemplateId, this.PortalSettings.PortalId, this.moduleId, this.tabId, this.forumId, this.topicId, this.replyId, au);
                }
            }

            NotificationsController.Instance.DeleteNotification(dto.NotificationId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

        [DnnAuthorize]
        [HttpPost]
        public HttpResponseMessage DeletePost(ModerationDTO dto)
        {
            var notify = NotificationsController.Instance.GetNotification(dto.NotificationId);
            this.ParseNotificationContext(notify.Context);

            var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.forumId, this.moduleId);
            if (fi == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Forum Not Found" });
            }

            if (!this.IsMod(this.forumId))
            {
                return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "User is not a moderator for this forum" });
            }

            var ms = SettingsBase.GetModuleSettings(this.moduleId);
            if (this.replyId > 0 & this.replyId != this.topicId)
            {
                DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.moduleId).GetById(this.replyId);

                if (reply == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Reply Not Found" });
                }

                var rc = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.moduleId);
                rc.Reply_Delete(this.PortalSettings.PortalId, this.forumId, this.topicId, this.replyId, ms.DeleteBehavior);
                if (fi.FeatureSettings.ModDeleteTemplateId > 0 && reply?.Content?.AuthorId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(fi.FeatureSettings.ModDeleteTemplateId, fi.PortalId, fi.ModuleId, fi.TabId, fi.ForumID, this.topicId, this.replyId, reply.Author);
                }
            }
            else
            {
                var ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.moduleId).GetById(this.topicId);
                if (ti == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Topic Not Found" });
                }
                new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.moduleId).DeleteById(this.topicId);
                if (fi.FeatureSettings.ModDeleteTemplateId > 0 && ti?.Content?.AuthorId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(fi.FeatureSettings.ModDeleteTemplateId, fi.PortalId, fi.ModuleId, fi.TabId, fi.ForumID, this.topicId, this.replyId, ti.Author);
                }
            }


            NotificationsController.Instance.DeleteNotification(dto.NotificationId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

        [HttpPost]
        [DnnAuthorize]
        public HttpResponseMessage BanUser(ModerationDTO dto)
        {
            var notify = NotificationsController.Instance.GetNotification(dto.NotificationId);
            this.ParseNotificationContext(notify.Context);

            var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.forumId, this.moduleId);
            if (fi == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Forum Not Found" });
            }

            if (!this.IsMod(this.forumId))
            {
                return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "User is not a moderator for this forum" });
            }

            int authorId;
            if (this.replyId > 0 & this.replyId != this.topicId)
            {
                DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.moduleId).GetById(this.replyId);

                if (reply == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Reply Not Found" });
                }

                authorId = reply.Content.AuthorId;
            }
            else
            {
                var ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.moduleId).GetById(this.topicId);
                if (ti == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Topic Not Found" });
                }

                authorId = ti.Content.AuthorId;
            }

            string moduleTitle = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(this.moduleId, this.tabId, true).ModuleTitle;
            DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.BanUser(portalId: this.ActiveModule.PortalID, moduleId: this.moduleId, moduleTitle: moduleTitle, tabId: this.tabId, forumId: this.forumId, topicId: this.topicId, replyId: this.replyId, bannedBy: this.UserInfo, authorId: authorId);

            NotificationsController.Instance.DeleteNotification(dto.NotificationId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

        [DnnAuthorize]
        [HttpPost]
        public HttpResponseMessage IgnorePost(ModerationDTO dto)
        {
            var notify = NotificationsController.Instance.GetNotification(dto.NotificationId);
            this.ParseNotificationContext(notify.Context);

            var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.forumId, this.moduleId);
            if (fi == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = "Forum Not Found" });
            }

            if (!this.IsMod(this.forumId))
            {
                return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "User is not a moderator for this forum" });
            }

            NotificationsController.Instance.DeleteNotification(dto.NotificationId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

        #region Private Methods

        private bool IsMod(int forumId)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.GetListOfModerators(this.ActiveModule.PortalID, this.moduleId, forumId).Any(i => i.UserID == this.UserInfo.UserID);
        }

        private void ParseNotificationContext(string context)
        {
            var keys = context.Split(':');
            this.tabId = int.Parse(keys[0]);
            this.moduleId = int.Parse(keys[1]);
            this.forumId = int.Parse(keys[2]);
            this.topicId = int.Parse(keys[3]);
            this.replyId = int.Parse(keys[4]);
        }

        #endregion

        public class ModerationDTO
        {
            public int NotificationId { get; set; }
        }
    }
}
