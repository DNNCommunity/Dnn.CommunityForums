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
// 
using System.Diagnostics.Contracts;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal static class UserController
    {
        internal static void BanUser(int PortalId, int ModuleId, int UserId)
        {
            if (UserId > -1)
            {
                DataProvider.Instance().Topics_Delete_For_User(ModuleId: ModuleId, UserId: UserId, DelBehavior: SettingsBase.GetModuleSettings(ModuleId).DeleteBehavior);
                DotNetNuke.Entities.Users.UserInfo user = DotNetNuke.Entities.Users.UserController.GetUserById(portalId: PortalId, userId: UserId);
                user.Membership.Approved = false;
                DotNetNuke.Entities.Users.UserController.UpdateUser(portalId: PortalId, user: user, loggedAction: true);
                DataCache.CacheClearPrefix(ModuleId, string.Format("AF-FV-{0}-{1}", PortalId, ModuleId));
            }
        }
    }
}
