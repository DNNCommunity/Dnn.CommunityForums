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
namespace DotNetNuke.Modules.ActiveForums.Services
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Principal;
    using System.Threading;
    using System.Web.Helpers;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Web.Api;

    /// <summary>
    /// Use this attribute to set which forum permission is required to perform an API action.
    /// The forumId included in the API URL will be used to verify that the action can be performed based on the permissions for the forum.
    /// </summary>
    public sealed class ForumsAuthorizeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="permissionNeeded"></param>
        public ForumsAuthorizeAttribute(SecureActions permissionNeeded)
        {
            this.PermissionNeeded = permissionNeeded;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public SecureActions PermissionNeeded { get; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <remarks>System.InvalidOperationException is thrown if you specify this attribute on an API method that does not include forumId</remarks>
        public override bool IsAuthorized(AuthFilterContext context)
        {
            Requires.NotNull("context", context);
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            if (identity.IsAuthenticated)
            {
                int forumId = -1;
                if (forumId <= 0)
                {
                    if (context.ActionContext.Request.GetRouteData().Route.DataTokens.ContainsKey(Literals.ForumId))
                    {
                        forumId = Utilities.SafeConvertInt(context.ActionContext.Request.GetRouteData().Route
                                                                                        .DataTokens[Literals.ForumId]
                                                                                        .ToString());
                    }
                }

                if (forumId <= 0)
                {
                    try
                    {
                        forumId = Utilities.SafeConvertInt(context.ActionContext.Request.GetQueryNameValuePairs()
                                                                                        .Where(q => q.Key.ToLowerInvariant() == Literals.ForumId.ToLowerInvariant())
                                                                                        .FirstOrDefault().Value);
                    }
                    catch
                    {
                    }
                }

                if (forumId <= 0)
                {
                    try
                    {
                        var postData = context.ActionContext.Request.Content.ReadAsStringAsync().Result;
                        forumId = Utilities.SafeConvertInt(System.Web.Helpers.Json.Decode(postData).ForumId);
                    }
                    catch
                    {
                    }
                }

                if (forumId <= 0)
                {
                    throw new System.InvalidOperationException();
                }
                else
                {
                    ModuleInfo moduleInfo = context.ActionContext.Request.FindModuleInfo();
                    UserInfo userInfo = ServiceLocator<IPortalController, PortalController>.Instance.GetCurrentPortalSettings().UserInfo;
                    int moduleId = DotNetNuke.Modules.ActiveForums.Utilities.GetForumModuleId(moduleInfo.ModuleID, moduleInfo.TabID);
                    return ServicesHelper.IsAuthorized(moduleInfo.PortalID, moduleId, forumId, this.PermissionNeeded, userInfo);
                }
            }

            return false;
        }
    }
}
