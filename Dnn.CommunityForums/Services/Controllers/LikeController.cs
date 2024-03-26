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
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
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
    public class LikeController : ControllerBase<LikeController>
    {
        public struct LikeDto
        {
            public int ForumId { get; set; }
            public int ContentId { get; set; }
        }
        /// <summary>
        /// Increments/Decrements Likes for a ContentId for a User
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Like/Like</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Reply)] 
        public HttpResponseMessage Like(LikeDto dto)
        {
            if ((DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForum(PortalSettings.PortalId, ForumModuleId, dto.ForumId).AllowLikes) &&
                ServicesHelper.IsAuthorized(PortalSettings.PortalId, ForumModuleId, dto.ForumId, SecureActions.Reply, UserInfo))
            {
                return Request.CreateResponse(HttpStatusCode.OK, new DotNetNuke.Modules.ActiveForums.Controllers.LikeController().Like(dto.ContentId, UserInfo.UserID));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        /// <summary>
        /// Gets number of Likes for a ContentId 
        /// </summary>
        /// <param name="forumId"></param>
        /// <param name="contentId"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Like/Get/1/1</remarks>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage Get(int forumId, int contentId)
        {
            if (DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForum(PortalSettings.PortalId, ForumModuleId, forumId).AllowLikes)
            {
                return Request.CreateResponse(HttpStatusCode.OK, value: new DotNetNuke.Modules.ActiveForums.Controllers.LikeController().Get(UserInfo.UserID, contentId));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}