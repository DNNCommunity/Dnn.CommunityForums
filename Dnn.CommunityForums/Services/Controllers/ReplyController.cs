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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Http;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.ActiveForums.API;
using DotNetNuke.Modules.ActiveForums.DAL2;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Modules.ActiveForums.Entities;
using DotNetNuke.Security;
using DotNetNuke.Security.Roles;
using DotNetNuke.UI.UserControls;
using DotNetNuke.Web.Api;
using static DotNetNuke.Modules.ActiveForums.Handlers.HandlerBase;

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
                TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = tc.Topics_Get(ActiveModule.PortalID, ForumModuleId, topicId);
                if (t != null)
                {
                    Forum f = new DotNetNuke.Modules.ActiveForums.ForumController().Forums_Get(forumId, UserInfo.UserID, true);
                    if ((UserInfo.UserID == t.Author.AuthorId && !t.IsLocked) || Permissions.HasAccess(f.Security.ModEdit, string.Join(";",UserInfo.Roles)))
                    {
                        DataProvider.Instance().Reply_UpdateStatus(ActiveModule.PortalID, ForumModuleId, topicId, replyId, UserInfo.UserID, 1, Permissions.HasAccess(f.Security.ModEdit, string.Join(";", UserInfo.Roles)));
                        DataCache.CacheClearPrefix(ForumModuleId, string.Format(CacheKeys.TopicViewPrefix, ForumModuleId));
                        return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                    }
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}