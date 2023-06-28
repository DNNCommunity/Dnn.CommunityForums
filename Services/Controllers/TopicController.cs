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
    public class TopicController : ControllerBase<LikeController>
    {
        public struct TopicDto
        {
            public int ForumId { get; set; }
            public int TopicId { get; set; }
            public int ContentId { get; set; }
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
                TopicInfo t = tc.Topics_Get(PortalSettings.PortalId, ActiveModule.ModuleID, topicId);
                if (t != null)
                {
                    t.IsPinned = !t.IsPinned;
                    tc.TopicSave(PortalSettings.PortalId, t);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
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
        [ForumsAuthorize(SecureActions.ModPin)]
        [ForumsAuthorize(SecureActions.Pin)]
        public HttpResponseMessage Lock(TopicDto dto)
        {
            int topicId = dto.TopicId;
            if (topicId > 0)
            {
                TopicsController tc = new TopicsController();
                TopicInfo t = tc.Topics_Get(PortalSettings.PortalId, ActiveModule.ModuleID, topicId);
                if (t != null)
                {
                    t.IsLocked = !t.IsLocked;
                    tc.TopicSave(PortalSettings.PortalId, t);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}