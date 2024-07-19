//
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
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
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
    public class ForumController : ControllerBase<ForumController>
    {
        public struct ForumDto
        {
            public int ForumId { get; set; }
        }
        /// <summary>
        /// Subscribes to a Forum
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Forum/Subscribe</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Subscribe)]
        public HttpResponseMessage Subscribe(ForumDto dto)
        {
            if (dto.ForumId > 0)
            {

                string userRoles = new DotNetNuke.Modules.ActiveForums.UserProfileController().Profiles_Get(ActiveModule.PortalID, ForumModuleId, UserInfo.UserID).Roles;
                int subscribed = new SubscriptionController().Subscription_Update(ActiveModule.PortalID, ForumModuleId, dto.ForumId, -1, 1, UserInfo.UserID, userRoles);
                return Request.CreateResponse(HttpStatusCode.OK, subscribed == 1);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
        /// <summary>
        /// Gets Subscriber count for a Forum
        /// </summary>
        /// <param name="ForumId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Forum/SubscriberCount?ForumId=xxx</remarks>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage SubscriberCount(int ForumId)
        {
            if (ForumId > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(ActiveModule.PortalID, ForumModuleId, ForumId));
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
        /// <summary>
         /// Gets Subscriber count string for a Forum
         /// </summary>
         /// <returns></returns>
         /// <remarks>https://dnndev.me/API/ActiveForums/Forum/SubscriberCountString?ForumId=xxx</remarks>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage SubscriberCountString(int ForumId)
        {
            if (ForumId > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, $"{new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(ActiveModule.PortalID, ForumModuleId, ForumId)} {Utilities.GetSharedResource("[RESX:FORUMSUBSCRIBERCOUNT]", false)}");
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
        /// <summary>
        /// Populates list of forums for an HTML control
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Forum/ListForHtml</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.ModMove)]
        public HttpResponseMessage ListForHtml(ForumDto dto)
        {
            var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetById(UserInfo.UserID);
            return Request.CreateResponse(HttpStatusCode.OK, DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsHtmlOption(ForumModuleId, user));
        }
    }
}