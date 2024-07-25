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
namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    using System;
    using System.Data;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Web.Http;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.UI.UserControls;
    using DotNetNuke.Web.Api;

    using static DotNetNuke.Modules.ActiveForums.Handlers.HandlerBase;

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
                if (UserInfo.UserID > 0)
                {
                    DataProvider.Instance().Profiles_UpdateActivity(PortalSettings.PortalId, ForumModuleId, UserInfo.UserID);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
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
            {
                // if running from Forums Viewer module, need to get the module for the Forums instance
                int moduleId = ActiveModule.ModuleID;
                if (DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: ActiveModule.ModuleID, tabId: ActiveModule.TabID, ignoreCache: false).DesktopModule.ModuleName == string.Concat(Globals.ModuleName, " Viewer"))
                {
                    moduleId = Utilities.SafeConvertInt(DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(ActiveModule.ModuleID, ActiveModule.TabID, false).ModuleSettings["AFForumModuleID"]);
                }

                UsersOnline uo = new UsersOnline();
                string sOnlineList = uo.GetUsersOnline(PortalSettings.PortalId, ForumModuleId, UserInfo);
                IDataReader dr = DataProvider.Instance().Profiles_GetStats(PortalSettings.PortalId, ForumModuleId, 2);
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
                return Request.CreateResponse(HttpStatusCode.OK, sUsersOnline + " " + sOnlineList);
            }
        }
    }
}
