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

namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Web.Api;

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
            try
            {
                if (dto.ForumId > 0)
                {
                    string userPermSet = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.ActiveModule.PortalID, this.UserInfo.UserID).UserPermSet;
                    int subscribed = new SubscriptionController().Subscription_Update(this.ActiveModule.PortalID, this.ForumModuleId, dto.ForumId, -1, 1, this.UserInfo.UserID, userPermSet);
                    return this.Request.CreateResponse(HttpStatusCode.OK, subscribed == 1);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Gets Subscriber count for a Forum
        /// </summary>
        /// <param name="forumId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Forum/SubscriberCount?ForumId=xxx</remarks>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage SubscriberCount(int forumId)
        { try
            {
                if (forumId > 0)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(this.ActiveModule.PortalID, this.ForumModuleId, forumId));
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Gets Subscriber count string for a Forum
        /// </summary>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Forum/SubscriberCountString?ForumId=xxx</remarks>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage SubscriberCountString(int forumId)
        {
            try
            {
                if (forumId > 0)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, $"{new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(this.ActiveModule.PortalID, this.ForumModuleId, forumId)} {Utilities.GetSharedResource("[RESX:FORUMSUBSCRIBERCOUNT]", false)}");
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Populates list of forums for an HTML control
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Forum/ListForHtml</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Move)]
        public HttpResponseMessage ListForHtml(ForumDto dto)
        {
            try
            {
                var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.ActiveModule.PortalID, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsHtmlOption(this.ForumModuleId, user, includeHiddenForums: true));
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
