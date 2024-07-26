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
namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Api;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class ReplyController : ControllerBase<ReplyController>
    {
        public struct ReplyDto
        {
            public int ForumId { get; set; }

            public int TopicId { get; set; }

            public int ReplyId { get; set; }
        }
#pragma warning disable CS1570

        /// <summary>
        /// Marks a reply as the answer to a topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Reply/MarkAsAnswer</remarks>
#pragma warning restore CS1570
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.ModEdit)]
        [ForumsAuthorize(SecureActions.Edit)]
        public HttpResponseMessage MarkAsAnswer(ReplyDto dto)
        {
            int forumId = dto.ForumId;
            int topicId = dto.TopicId;
            int replyId = dto.ReplyId;
            if (forumId > 0 && topicId > 0 && replyId > 0)
            {
                var r = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(replyId);
                if (r != null)
                {
                    if ((this.UserInfo.UserID == r.Topic.Author.AuthorId && !r.Topic.IsLocked) || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(r.Topic.Forum.Security.ModEdit, string.Join(";", DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(this.ActiveModule.PortalID, this.UserInfo.Roles))))
                    {
                        DataProvider.Instance().Reply_UpdateStatus(this.ActiveModule.PortalID, this.ForumModuleId, r.TopicId, replyId, this.UserInfo.UserID, 1, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(r.Topic.Forum.Security.ModEdit, string.Join(";", DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(this.ActiveModule.PortalID, this.UserInfo.Roles))));
                        DataCache.CacheClearPrefix(this.ForumModuleId, string.Format(CacheKeys.TopicViewPrefix, this.ForumModuleId));
                        return this.Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.BadRequest);
                }

                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }
#pragma warning disable CS1570
        /// <summary>
        /// Deletes a Reply
        /// </summary>
        /// <param name="forumId" type="int"></param>
        /// <param name="replyId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Reply/Delete?forumId=xxx&replyId=zzz</remarks>
#pragma warning restore CS1570
        [HttpDelete]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Delete)]
        [ForumsAuthorize(SecureActions.ModDelete)]
        public HttpResponseMessage Delete(int forumId, int replyId)
        {
            if (forumId > 0 && replyId > 0)
            {
                var rc = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController();
                var r = rc.GetById(replyId);
                if (r != null)
                {
                    rc.Reply_Delete(this.ActiveModule.PortalID, forumId, r.TopicId, replyId, SettingsBase.GetModuleSettings(this.ForumModuleId).DeleteBehavior);
                    return this.Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}
