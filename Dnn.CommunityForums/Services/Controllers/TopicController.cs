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
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class TopicController : ControllerBase<TopicController>
    {
        public struct TopicDto
        {
            public int ForumId { get; set; }
            public int TopicId { get; set; }
        }

        /// <summary>
        /// Subscribes to a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Subscribe</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Subscribe)]
        public HttpResponseMessage Subscribe(TopicDto dto)
        {
            if (dto.TopicId > 0 && dto.ForumId > 0)
            {

                string userRoles = new DotNetNuke.Modules.ActiveForums.UserProfileController()
                    .Profiles_Get(ActiveModule.PortalID, ForumModuleId, UserInfo.UserID).Roles;
                int subscribed = new SubscriptionController().Subscription_Update(ActiveModule.PortalID,
                    ForumModuleId, dto.ForumId, dto.TopicId, 1, UserInfo.UserID, userRoles);
                return Request.CreateResponse(HttpStatusCode.OK, subscribed == 1);
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Gets Subscriber count for a Topic
        /// </summary>
        /// <param name="ForumId" type="int"></param>
        /// <param name="TopicId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/SubscriberCount?ForumId=xxx&TopicId=xxx</remarks>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage SubscriberCount(int ForumId, int TopicId)
        {
            if (ForumId > 0 && TopicId > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(
                        ActiveModule.PortalID, ForumModuleId, ForumId, TopicId));
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Gets Subscriber count string for a Topic
        /// </summary>
        /// <param name="ForumId" type="int"></param>
        /// <param name="TopicId" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Forum/SubscriberCountString?ForumId=xxx&TopicId=xxx</remarks>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage SubscriberCountString(int ForumId, int TopicId)
        {
            if (ForumId > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    $"{new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(ActiveModule.PortalID, ForumModuleId, ForumId, TopicId)} {Utilities.GetSharedResource("[RESX:TOPICSUBSCRIBERCOUNT]", false)}");
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
          
    /// <summary>
    /// Pins a Topic
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Pin</remarks>
    [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.ModPin)]
        [ForumsAuthorize(SecureActions.Pin)]
        public HttpResponseMessage Pin(TopicDto dto)
        {
            int topicId = dto.TopicId;
            if (topicId > 0)
            {
                TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (ti != null)
                {
                    ti.IsPinned = !ti.IsPinned;
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
                    return Request.CreateResponse(HttpStatusCode.OK, value: ti.IsPinned);
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Locks a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Lock</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.ModLock)]
        [ForumsAuthorize(SecureActions.Lock)]
        public HttpResponseMessage Lock(TopicDto dto)
        {
            int topicId = dto.TopicId;
            if (topicId > 0)
            {
                TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (ti != null)
                {
                    ti.IsLocked = !ti.IsLocked;
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
                    return Request.CreateResponse(HttpStatusCode.OK, ti.IsLocked);
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
        /// <summary>
        /// Moves a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Move</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.ModMove)]
        public HttpResponseMessage Move(TopicDto dto)
        {
            int topicId = dto.TopicId;
            int forumId = dto.ForumId;
            if (topicId > 0 && forumId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (ti != null)
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Move(topicId, forumId);
                    DataCache.CacheClearPrefix(ForumModuleId, string.Format(CacheKeys.ForumViewPrefix, ForumModuleId));
                    return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
        /// <summary>
        /// Loads a Topic
        /// <param name="ForumId" type="int"></param>
        /// <param name="TopicId" type="int"></param>
        /// <returns name="Topic" type="DotNetNuke.Modules.ActiveForums.Entities.TopicInfo"></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Load?ForumId=xxx&TopicId=xxx</remarks>
        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Read)]
        public HttpResponseMessage Load(int ForumId, int TopicId)
        {
            if (TopicId > 0 && ForumId > 0)
            {
                if (ServicesHelper.IsAuthorized(PortalSettings.PortalId, ForumModuleId, ForumId, SecureActions.Read, UserInfo))
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
                    if (t != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new object[] { t });
                    }
                    else 
                    {
                        Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                }
                else
                {
                    Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        } 
        /// <summary>
        /// Deletes a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Delete</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Delete)]
        [ForumsAuthorize(SecureActions.ModDelete)]
        public HttpResponseMessage Delete(TopicDto dto)
        {
            int topicId = dto.TopicId;
            int forumId = dto.ForumId;
            if (topicId > 0 && forumId > 0)
            {
                TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (ti != null)
                {
                    new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().DeleteById(topicId);
                    return Request.CreateResponse(HttpStatusCode.OK, string.Empty);                   
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Rates a topic
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="rating" type="int"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Rate</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Edit)]
        [ForumsAuthorize(SecureActions.ModEdit)]
        public HttpResponseMessage Rate(TopicDto dto, int rating)
        {
            if (dto.TopicId > 0 && (rating >= 1 && rating <= 5))
            {   
                return Request.CreateResponse(HttpStatusCode.OK, new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(UserInfo.UserID, dto.TopicId, rating, HttpContext.Current.Request.UserHostAddress ?? string.Empty));
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}