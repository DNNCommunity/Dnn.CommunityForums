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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
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

        /// <summary>
        /// Marks a reply as the answer to a topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Reply/MarkAsAnswer</remarks>
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
                TopicsController tc = new DotNetNuke.Modules.ActiveForums.TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = tc.Topics_Get(ActiveModule.PortalID, ForumModuleId, topicId);
                if (t != null)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId);
                    if ((UserInfo.UserID == t.Author.AuthorId && !t.IsLocked) || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(f.Security.ModEdit, string.Join(";",UserInfo.Roles)))
                    {
                        DataProvider.Instance().Reply_UpdateStatus(ActiveModule.PortalID, ForumModuleId, topicId, replyId, UserInfo.UserID, 1, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(f.Security.ModEdit, string.Join(";", UserInfo.Roles)));
                        DataCache.CacheClearPrefix(ForumModuleId, string.Format(CacheKeys.TopicViewPrefix, ForumModuleId));
                        return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                    }
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
        /// <summary>
        /// Deletes a Reply
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Reply/Delete</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Delete)]
        [ForumsAuthorize(SecureActions.ModDelete)]
        public HttpResponseMessage Delete(ReplyDto dto)
        {
            int forumId = dto.ForumId;
            int topicId = dto.TopicId;
            int replyId = dto.ReplyId;
            if (forumId > 0 && topicId > 0 && replyId > 0)
            {
                var rc = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController();
                var r = rc.GetById(replyId);
                if (r != null)
                {
                    rc.Reply_Delete(ActiveModule.PortalID, forumId, topicId,replyId, SettingsBase.GetModuleSettings(ForumModuleId).DeleteBehavior);
                    return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}