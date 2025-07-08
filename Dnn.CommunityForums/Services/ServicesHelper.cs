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

namespace DotNetNuke.Modules.ActiveForums.Services
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    internal static class ServicesHelper
    {
        internal static bool IsAuthorized(PortalSettings portalSettings, int moduleId, int forumId, SecureActions permissionRequired, UserInfo userInfo)
        {
            try
            {
                var roles = new HashSet<int>();
                if (permissionRequired is SecureActions.ManageUsers)
                {
                    var moduleDefaultSecurity = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(permissionId: SettingsBase.GetModuleSettings(moduleId).DefaultPermissionId, moduleId: moduleId);
                    roles = moduleDefaultSecurity.ManageUsersRoleIds;
                }
                else
                {
                    var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId, moduleId);
                    switch (permissionRequired)
                    {
                        case SecureActions.View:
                            roles = fi.Security.ViewRoleIds;
                            break;
                        case SecureActions.Read:
                            roles = fi.Security.ReadRoleIds;
                            break;
                        case SecureActions.Create:
                            roles = fi.Security.CreateRoleIds;
                            break;
                        case SecureActions.Reply:
                            roles = fi.Security.ReplyRoleIds;
                            break;
                        case SecureActions.Edit:
                            roles = fi.Security.EditRoleIds;
                            break;
                        case SecureActions.Delete:
                            roles = fi.Security.DeleteRoleIds;
                            break;
                        case SecureActions.Lock:
                            roles = fi.Security.LockRoleIds;
                            break;
                        case SecureActions.Pin:
                            roles = fi.Security.PinRoleIds;
                            break;
                        case SecureActions.Attach:
                            roles = fi.Security.AttachRoleIds;
                            break;
                        case SecureActions.Poll:
                            roles = fi.Security.PollRoleIds;
                            break;
                        case SecureActions.Trust:
                            roles = fi.Security.TrustRoleIds;
                            break;
                        case SecureActions.Subscribe:
                            roles = fi.Security.SubscribeRoleIds;
                            break;
                        case SecureActions.Announce:
                            roles = fi.Security.AnnounceRoleIds;
                            break;
                        case SecureActions.Tag:
                            roles = fi.Security.TagRoleIds;
                            break;
                        case SecureActions.Categorize:
                            roles = fi.Security.CategorizeRoleIds;
                            break;
                        case SecureActions.Prioritize:
                            roles = fi.Security.PrioritizeRoleIds;
                            break;
                        case SecureActions.Moderate:
                            roles = fi.Security.ModerateRoleIds;
                            break;
                        case SecureActions.Move:
                            roles = fi.Security.MoveRoleIds;
                            break;
                        case SecureActions.Split:
                            roles = fi.Security.SplitRoleIds;
                            break;
                        default:
                            return false;
                    }
                }

                var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId).GetByUserId(portalSettings.PortalId, userInfo.UserID);
                return DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(roles, forumUser.UserRoleIds);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return false;
        }

        internal static string CleanAndChopString(string MatchString, int maxLength)
        {
            string matchString = string.Empty;
            if (!string.IsNullOrEmpty(MatchString))
            {
                matchString = MatchString.Trim();
                matchString = DotNetNuke.Modules.ActiveForums.Utilities.XSSFilter(matchString);
                matchString = DotNetNuke.Modules.ActiveForums.Utilities.Text.RemoveHTML(matchString);
                matchString = DotNetNuke.Modules.ActiveForums.Utilities.Text.CheckSqlString(matchString);
                if (!string.IsNullOrEmpty(matchString))
                {
                    if (matchString.Length > maxLength)
                    {
                        matchString = matchString.Substring(0, 20);
                    }
                }
            }

            return matchString;
        }
    }
}
