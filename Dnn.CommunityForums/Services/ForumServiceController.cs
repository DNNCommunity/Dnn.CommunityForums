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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Results;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;

    [DnnAuthorize]
    [ValidateAntiForgeryToken]
    public class ForumServiceController : DnnApiController
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public HttpResponseMessage CreateThumbnail(CreateThumbnailDTO dto)
        {
            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string EncryptTicket(EncryptTicketDTO dto) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Moved to Attachment Controller.")]
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public Task<HttpResponseMessage> UploadFile()
        {
            return new Task<HttpResponseMessage>(() => this.Request.CreateResponse(HttpStatusCode.BadRequest));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Moved to Attachment Controller.")]
        [HttpGet]
        public HttpResponseMessage GetUserFileUrl(int fileId)
        {
            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [HttpGet]
        public HttpResponseMessage GetTopicList(int forumId)
        {
            var portalSettings = this.PortalSettings;
            var userInfo = portalSettings.UserInfo;

            DataSet ds = DataProvider.Instance().UI_TopicsView(portalSettings.PortalId, this.ActiveModule.ModuleID, forumId, userInfo.UserID, 0, 20, userInfo.IsSuperUser, SortColumns.ReplyCreated);
            if (ds.Tables.Count > 0)
            {
                DataTable dtTopics = ds.Tables[3];

                Dictionary<string, string> rows = new Dictionary<string, string>();
                foreach (DataRow dr in dtTopics.Rows)
                {
                    rows.Add(dr["TopicId"].ToString(), dr["Subject"].ToString());
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, rows.ToJson());
            }

            return this.Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [HttpGet]
        public HttpResponseMessage GetForumsList()
        {
            var portalSettings = this.PortalSettings;
            var userInfo = portalSettings.UserInfo;
            var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ActiveModule.ModuleID).GetByUserId(this.ActiveModule.PortalID, userInfo.UserID);
            Dictionary<string, string> rows = new Dictionary<string, string>();
            foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi in new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Get(this.ActiveModule.ModuleID).Where(f => !f.Hidden && !f.ForumGroup.Hidden && (this.UserInfo.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(f.Security.ViewRoleIds, forumUser.UserRoleIds))))
            {
                rows.Add(fi.ForumID.ToString(), fi.ForumName.ToString());
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, rows.ToJson());
        }

        public class CreateSplitDTO
        {
            public int OldTopicId { get; set; }

            public int NewTopicId { get; set; }

            public int NewForumId { get; set; }

            public string Subject { get; set; }

            public string Replies { get; set; }
        }

        [HttpPost]
        public HttpResponseMessage CreateSplit(CreateSplitDTO dto)
        {
            if (dto.NewTopicId == dto.OldTopicId)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK);
            }

            var portalSettings = this.PortalSettings;
            var userInfo = portalSettings.UserInfo;
            var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ActiveModule.ModuleID).GetByUserId(this.ActiveModule.PortalID, userInfo.UserID);

            var oldForum = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(portalSettings.PortalId, this.ActiveModule.ModuleID, 0, true, dto.OldTopicId);
            var newForum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(dto.NewForumId, this.ActiveModule.ModuleID);
            if (oldForum != null && newForum != null)
            {
                var ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ActiveModule.ModuleID).GetById(dto.OldTopicId);
                if (ti != null)
                {
                    bool hasCreatePerm;

                    if (oldForum == newForum)
                    {
                        hasCreatePerm = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(oldForum.Security.CreateRoleIds, forumUser.UserRoleIds);
                    }
                    else
                    {
                        hasCreatePerm = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(oldForum.Security.CreateRoleIds, forumUser.UserRoleIds) && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(newForum.Security.CreateRoleIds, forumUser.UserRoleIds);
                    }

                    var canSplit = (ti.Content.AuthorId == userInfo.UserID && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(oldForum.Security.SplitRoleIds, forumUser.UserRoleIds)) || userInfo.IsAdmin || userInfo.IsSuperUser ||
                                   (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(oldForum.Security.ModerateRoleIds, forumUser.UserRoleIds) && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(oldForum.Security.SplitRoleIds, forumUser.UserRoleIds));

                    if (hasCreatePerm && canSplit)
                    {
                        int topicId;

                        if (dto.NewTopicId < 1)
                        {
                            var subject = Utilities.CleanString(portalSettings.PortalId, dto.Subject, false, EditorType.TEXTBOX, false, false, this.ActiveModule.ModuleID, string.Empty, false);
                            var replies = dto.Replies.Split('|');
                            var firstReply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ActiveModule.ModuleID).GetById(Convert.ToInt32(replies[0]));
                            var firstContent = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(firstReply.ContentId, this.ActiveModule.ModuleID);
                            topicId = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.QuickCreate(portalSettings.PortalId, this.ActiveModule.ModuleID, dto.NewForumId, subject, string.Empty, firstContent.AuthorId, firstContent.AuthorName, true, this.Request.GetIPAddress());
                            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Replies_Split(this.ActiveModule.ModuleID, oldForum.ForumID, newForum.ForumID, dto.OldTopicId, topicId, dto.Replies, true);
                        }
                        else
                        {
                            topicId = dto.NewTopicId;
                            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Replies_Split(this.ActiveModule.ModuleID, oldForum.ForumID, newForum.ForumID, dto.OldTopicId, topicId, dto.Replies, false);
                        }

                        return this.Request.CreateResponse(HttpStatusCode.OK, topicId);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public class CreateThumbnailDTO
        {
            public int FileId { get; set; }

            public int Height { get; set; }

            public int Width { get; set; }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public class EncryptTicketDTO
        {
            public string Url { get; set; }
        }
    }
}
