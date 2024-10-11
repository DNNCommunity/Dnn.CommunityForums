﻿// Copyright (c) 2013-2024 by DNN Community
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
    using System.Data;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Web.Api;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class UserController : ControllerBase<UserController>
    {
        /// <summary>
        /// Fired by UI while user is online to update user's profile with
        /// </summary>
        /// <param>none</param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/User/UpdateUserIsOnline</remarks>
        [HttpPost]
        [DnnAuthorize]
        public HttpResponseMessage UpdateUserIsOnline()
        {
            try
            {
                if (this.UserInfo.UserID > 0)
                {
                    DataProvider.Instance().Profiles_UpdateActivity(this.PortalSettings.PortalId, this.UserInfo.UserID);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Fired by UI to get users online
        /// </summary>
        /// <param>none</param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/User/GetUsersOnline</remarks>
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetUsersOnline()
        {
            try
            {
                // if running from Forums Viewer module, need to get the module for the Forums instance
                int moduleId = this.ActiveModule.ModuleID;
                if (DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: this.ActiveModule.ModuleID, tabId: this.ActiveModule.TabID, ignoreCache: false).DesktopModule.ModuleName == string.Concat(Globals.ModuleName, " Viewer"))
                {
                    moduleId = Utilities.SafeConvertInt(DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(this.ActiveModule.ModuleID, this.ActiveModule.TabID, false).ModuleSettings["AFForumModuleID"]);
                }

                var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.ActiveModule.PortalID, this.UserInfo.UserID);
                string sOnlineList = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetUsersOnline(this.PortalSettings, SettingsBase.GetModuleSettings(this.ForumModuleId), this.ForumModuleId, user);
                IDataReader dr = DataProvider.Instance().Profiles_GetStats(this.PortalSettings.PortalId, this.ForumModuleId, 2);
                int anonCount = 0;
                int memCount = 0;
                int memTotal = 0;
                while (dr.Read())
                {
                    anonCount = Convert.ToInt32(dr["Guests"]);
                    memCount = Convert.ToInt32(dr["Members"]);
                    memTotal = Convert.ToInt32(dr["MembersTotal"]);
                }

                dr.Close();
                string sUsersOnline = null;
                sUsersOnline = Utilities.GetSharedResource("[RESX:UsersOnline]");
                sUsersOnline = sUsersOnline.Replace("[USERCOUNT]", memCount.ToString());
                sUsersOnline = sUsersOnline.Replace("[TOTALMEMBERCOUNT]", memTotal.ToString());
                return this.Request.CreateResponse(HttpStatusCode.OK, sUsersOnline + " " + sOnlineList);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }
        }
    }
}
