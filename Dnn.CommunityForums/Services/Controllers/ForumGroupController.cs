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

    using DotNetNuke.Modules.ActiveForums.ViewModels;
    using DotNetNuke.Web.Api;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class ForumGroupController : ControllerBase<DotNetNuke.Modules.ActiveForums.Services.Controllers.ForumGroupController>
    {
        public struct ForumGroupDto
        {
            public int ForumGroupId { get; set; }
        }

        /// <summary>
        /// Gets a ForumGroup by ID.
        /// </summary>
        /// <param name="ForumGroupId">ForumGroup ID</param>
        /// <returns>ForumGroupInfo</returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/ForumGroup/Get?ForumGroupId=x</remarks>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage Get(int ForumGroupId)
        {
            try
            {
                if (ForumGroupId > 0)
                {
                    var forumGroup = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetById(ForumGroupId, this.ForumModuleId);
                    if (forumGroup != null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, new DotNetNuke.Modules.ActiveForums.ViewModels.ForumGroup(forumGroup));
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
        /// Creates a new ForumGroup.
        /// </summary>
        /// <param name="forumGroup">ForumGroupInfo</param>
        /// <returns>Created ForumGroup</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage Create([FromBody] DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroup)
        {
            try
            {
                if (forumGroup != null)
                {
                    var fgc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController();
                    forumGroup.ForumGroupId = DotNetNuke.Common.Utilities.Null.NullInteger;
                    forumGroup.ModuleId = this.ForumModuleId;
                    var forumGroupId = fgc.Groups_Save(portalId: this.ActiveModule.PortalID, forumGroupInfo: forumGroup, isNew: true, useDefaultFeatures: true, useDefaultSecurity: true);
                    var forumGroupCreated = fgc.GetById(forumGroupId, this.ForumModuleId);
                    return this.Request.CreateResponse(HttpStatusCode.Created, forumGroupCreated);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Creates a new ForumGroup.
        /// </summary>
        /// <param name="forumGroup">ForumGroupInfo</param>
        /// <param name="useDefaultFeatures"></param>
        /// <param name="useDefaultSecurity"></param>
        /// <returns>Created ForumGroup</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage Create([FromBody] DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroup, bool useDefaultFeatures, bool useDefaultSecurity)
        {
            try
            {
                if (forumGroup != null)
                {
                    var fgc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController();
                    forumGroup.ForumGroupId = DotNetNuke.Common.Utilities.Null.NullInteger;
                    forumGroup.ModuleId = this.ForumModuleId;
                    var forumGroupId = fgc.Groups_Save(portalId: this.ActiveModule.PortalID, forumGroupInfo: forumGroup, isNew: true, useDefaultFeatures: useDefaultFeatures, useDefaultSecurity: useDefaultSecurity);
                    var forumGroupCreated = fgc.GetById(forumGroupId, this.ForumModuleId);
                    return this.Request.CreateResponse(HttpStatusCode.Created, forumGroupCreated);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Updates an existing ForumGroup.
        /// </summary>
        /// <param name="forumGroup">ForumGroupInfo</param>
        /// <returns>updated ForumGroup</returns>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage Update([FromBody] DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroup)
        {
            try
            {
                if (forumGroup != null && forumGroup.ForumGroupId > 0)
                {
                    var fgc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController();
                    var forumGroupId = fgc.Groups_Save(portalId: this.ActiveModule.PortalID, forumGroupInfo: forumGroup, isNew: false, useDefaultFeatures: forumGroup.InheritSettings, useDefaultSecurity: forumGroup.InheritSecurity);
                    var forumGroupUpdated = fgc.GetById(forumGroup.ForumGroupId, this.ForumModuleId);
                    return this.Request.CreateResponse(HttpStatusCode.Created, forumGroupUpdated);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Updates an existing ForumGroup.
        /// </summary>
        /// <param name="forumGroup">ForumGroupInfo</param>
        /// <param name="useDefaultFeatures"></param>
        /// <param name="useDefaultSecurity"></param>
        /// <returns>Success status</returns>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage Update([FromBody] DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroup, bool useDefaultFeatures, bool useDefaultSecurity)
        {
            try
            {
                if (forumGroup != null && forumGroup.ForumGroupId > 0)
                {
                    var fgc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController();
                    var forumGroupId = fgc.Groups_Save(portalId: this.ActiveModule.PortalID, forumGroupInfo: forumGroup, isNew: false, useDefaultFeatures: useDefaultFeatures, useDefaultSecurity: useDefaultSecurity);
                    var forumGroupUpdated = fgc.GetById(forumGroupId: forumGroup.ForumGroupId, moduleId: this.ForumModuleId);
                    return this.Request.CreateResponse(HttpStatusCode.Created, forumGroupUpdated);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Deletes a ForumGroup by ID.
        /// </summary>
        /// <param name="forumGroupId">ForumGroup ID</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage Delete(int forumGroupId)
        {
            try
            {
                if (forumGroupId > 0)
                {
                    var fgc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController();
                    var forumGroup = fgc.GetById(forumGroupId: forumGroupId, moduleId: this.ForumModuleId);
                    if (forumGroup != null)
                    {
                        fgc.Groups_Delete(moduleId: this.ForumModuleId, forumGroupId: forumGroupId);
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
        /// Lists all ForumGroups for the current module.
        /// </summary>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage List()
        {
            try
            {
                var forumGroups = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().Get(scopeValue: this.ForumModuleId);
                return this.Request.CreateResponse(HttpStatusCode.OK, forumGroups);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Gets ForumGroup settings.
        /// </summary>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage Settings(int ForumGroupId)
        {
            try
            {
                if (ForumGroupId > 0)
                {
                    var forumGroup = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetById(forumGroupId: ForumGroupId, moduleId: this.ForumModuleId);
                    if (forumGroup != null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, forumGroup.FeatureSettings);
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
        /// Gets ForumGroup security.
        /// </summary>
        [HttpGet]
        [DnnAuthorize]
        public HttpResponseMessage Security(int ForumGroupId)
        {
            try
            {
                if (ForumGroupId > 0)
                {
                    var forumGroup = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetById(forumGroupId: ForumGroupId, moduleId: this.ForumModuleId);
                    if (forumGroup != null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, forumGroup.Security);
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
