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
    public class UserBadgeController : ControllerBase<UserBadgeController>
    {
        public struct UserBadgeDto
        {
            public int ForumId { get; set; } /* this is required to satisfy the "ForumsAuthorize" attribute security checking, but not otherwise used at this time */

            public int BadgeId { get; set; }

            public int UserId { get; set; }

            public bool Assigned { get; set; }
        }

        /// <summary>
        /// Assign/unassigns a user badge to a user.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/UserBadge/Assign</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Ban)]
        public HttpResponseMessage Assign(UserBadgeDto dto)
        {
            try
            {
                if (dto.BadgeId > 0 && dto.UserId > 0)
                {
                    var userBadgeController = new DotNetNuke.Modules.ActiveForums.Controllers.UserBadgeController(this.PortalSettings.PortalId, this.ForumModuleId);
                    var userBadge = userBadgeController.GetForUserAndBadge(userId: dto.UserId, badgeId: dto.BadgeId);
                    if (userBadge == null && dto.Assigned.Equals(true))
                    {
                        userBadge = new DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo
                        {
                            BadgeId = dto.BadgeId,
                            UserId = dto.UserId,
                            PortalId = this.PortalSettings.PortalId,
                            ModuleId = this.ForumModuleId,
                            DateAssigned = DateTime.UtcNow,
                        };
                        userBadgeController.Insert(userBadge);
                        return this.Request.CreateResponse(HttpStatusCode.OK, true);
                    }

                    if (userBadge != null && dto.Assigned.Equals(false))
                    {
                        userBadgeController.DeleteById(userBadge.UserBadgeId);
                        return this.Request.CreateResponse(HttpStatusCode.OK, false);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
