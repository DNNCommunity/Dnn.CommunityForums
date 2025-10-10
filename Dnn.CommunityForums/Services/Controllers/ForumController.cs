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
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.UI;

    using DotNetNuke.Web.Api;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class ForumController : ControllerBase<DotNetNuke.Modules.ActiveForums.Services.Controllers.ForumController>
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
                    int subscribed = new SubscriptionController().Subscription_Update(this.ActiveModule.PortalID, this.ForumModuleId, dto.ForumId, -1, 1, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.ActiveModule.PortalID, this.UserInfo.UserID));
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
        [ForumsAuthorize(SecureActions.View)]
        public HttpResponseMessage SubscriberCount(int forumId)
        {
            try
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
        /// Gets Subscriber count string for a Forum.
        /// </summary>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Forum/SubscriberCountString?ForumId=xxx</remarks>
        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.View)]
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
        /// Populates list of forums for an HTML control.
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

        /// <summary>
        /// Gets a forum by ID.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <returns>ForumInfo</returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Forum/Get?forumId=x</remarks>
        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.View)]
        public HttpResponseMessage Get(int forumId)
        {
            try
            {
                if (forumId > 0)
                {
                    var forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: forumId, moduleId: this.ForumModuleId);
                    if (forum != null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, new DotNetNuke.Modules.ActiveForums.ViewModels.Forum(forum));
                    }

                    return this.Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Creates a new forum.
        /// </summary>
        /// <param name="forum">ForumInfo</param>
        /// <returns>Created forum</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage Create([FromBody] DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum)
        {
            try
            {
                if (forum != null)
                {
                    var fc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController();
                    forum.ForumID = DotNetNuke.Common.Utilities.Null.NullInteger;
                    forum.ModuleId = this.ForumModuleId;
                    forum.ParentForumId = 0;
                    var forumId = fc.Forums_Save(portalId: this.ActiveModule.PortalID, forumInfo: forum, isNew: true, useGroupFeatures: true, useGroupSecurity: true);
                    var forumCreated = fc.GetById(forumId: forumId, moduleId: this.ForumModuleId);
                    return this.Request.CreateResponse(HttpStatusCode.Created, forumCreated);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Creates a new forum.
        /// </summary>
        /// <param name="forum">ForumInfo</param>
        /// <param name="useGroupFeatures"></param>
        /// <param name="useGroupSecurity"></param>
        /// <returns>Created forum</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage Create([FromBody] DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum, bool useGroupFeatures, bool useGroupSecurity)
        {
            try
            {
                if (forum != null)
                {
                    var fc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController();
                    forum.ForumID = DotNetNuke.Common.Utilities.Null.NullInteger;
                    forum.ModuleId = this.ForumModuleId;
                    forum.ParentForumId = 0;
                    var forumId = fc.Forums_Save(portalId: this.ActiveModule.PortalID, forumInfo: forum, isNew: true, useGroupFeatures: useGroupFeatures, useGroupSecurity: useGroupSecurity);
                    var forumCreated = fc.GetById(forumId: forumId, moduleId: this.ForumModuleId);
                    return this.Request.CreateResponse(HttpStatusCode.Created, forumCreated);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Updates an existing forum.
        /// </summary>
        /// <param name="forum">ForumInfo</param>
        /// <returns>updated forum</returns>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage Update([FromBody] DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum)
        {
            try
            {
                if (forum != null && forum.ForumID > 0)
                {
                    var fc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController();
                    var forumId = fc.Forums_Save(portalId: this.ActiveModule.PortalID, forumInfo: forum, isNew: false, useGroupFeatures: forum.InheritSettings, useGroupSecurity: forum.InheritSecurity);
                    var forumUpdated = fc.GetById(forumId: forum.ForumID, moduleId: this.ForumModuleId);
                    return this.Request.CreateResponse(HttpStatusCode.Created, forumUpdated);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Updates an existing forum.
        /// </summary>
        /// <param name="forum">ForumInfo</param>
        /// <param name="useGroupFeatures"></param>
        /// <param name="useGroupSecurity"></param>
        /// <returns>Success status</returns>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage Update([FromBody] DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum, bool useGroupFeatures, bool useGroupSecurity)
        {
            try
            {
                if (forum != null && forum.ForumID > 0)
                {
                    var fc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController();
                    var forumId = fc.Forums_Save(portalId: this.ActiveModule.PortalID, forumInfo: forum, isNew: false, useGroupFeatures: useGroupFeatures, useGroupSecurity: useGroupSecurity);
                    var forumUpdated = fc.GetById(forumId: forum.ForumID, moduleId: this.ForumModuleId);
                    return this.Request.CreateResponse(HttpStatusCode.Created, forumUpdated);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Deletes a forum by ID.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage Delete(int forumId)
        {
            try
            {
                if (forumId > 0)
                {
                    var fc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController();
                    var forum = fc.GetById(forumId: forumId, moduleId: this.ForumModuleId);
                    if (forum != null)
                    {
                        fc.Forums_Delete(forumId: forumId, moduleId: this.ForumModuleId);
                        return this.Request.CreateResponse(HttpStatusCode.OK, true);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Lists all forums for the current module.
        /// </summary>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage List()
        {
            try
            {
                var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.ActiveModule.PortalID, this.UserInfo.UserID);
                var forums = new List<DotNetNuke.Modules.ActiveForums.ViewModels.Forum>();
                new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(moduleId: this.ForumModuleId).ForEach(f =>
                {
                    if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(f.Security.ViewRoleIds, forumUser.UserRoleIds))
                    {
                        forums.Add(new DotNetNuke.Modules.ActiveForums.ViewModels.Forum(f));
                    }
                });

                return this.Request.CreateResponse(HttpStatusCode.OK, forums);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Lists moderators for a forum.
        /// </summary>
        [HttpGet]
        [DnnAuthorize]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage Moderators(int forumId)
        {
            try
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(new NotImplementedException("Forum moderators retrieval not implemented yet."));
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Lists all forums the current user is subscribed to.
        /// </summary>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage Subscriptions(int userId)
        {
            try
            {
                if (userId > 0)
                {
                    var subscriptions = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().SubscribedForums(portalId: this.ActiveModule.PortalID, moduleId: this.ForumModuleId, userId: userId);
                    return this.Request.CreateResponse(HttpStatusCode.OK, subscriptions);
                }

                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Gets forum settings.
        /// </summary>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage Settings(int forumId)
        {
            try
            {
                if (forumId > 0)
                {
                    var forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: forumId, moduleId: this.ForumModuleId);
                    if (forum != null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, forum.FeatureSettings);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.NotFound);
                }

                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Gets forum security.
        /// </summary>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage Security(int forumId)
        {
            try
            {
                if (forumId > 0)
                {
                    var forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: forumId, moduleId: this.ForumModuleId);
                    if (forum != null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, forum.Security);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.NotFound);
                }

                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
