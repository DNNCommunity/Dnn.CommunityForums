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
namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.UI.UserControls;

    public class UserController
    {
        internal static int GetUserIdByUserName(int PortalId, string UserName)
        {
            try
            {
                DotNetNuke.Entities.Users.UserInfo user = DotNetNuke.Entities.Users.UserController.GetUserByName(PortalId, UserName);
                return user != null ? user.UserID : -1;
            }
            catch (Exception ex)
            {
                return -1;

            }
        }

        public User GetUser(int PortalId, int ModuleId)
        {
            User u = null;
            if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                if (HttpContext.Current == null)
                {
                    u = DNNGetCurrentUser(PortalId, ModuleId);
                }
                else if ((HttpContext.Current.Items["AFUserInfo"]) != null)
                {
                    u = (User)(HttpContext.Current.Items["AFUserInfo"]);
                    u = FillProfile(PortalId, ModuleId, u);
                }
                else
                {
                    u = DNNGetCurrentUser(PortalId, ModuleId);
                }
                if (u != null)
                {
                    if (HttpContext.Current.Items["AFUserInfo"] == null)
                    {
                        HttpContext.Current.Items.Add("AFUserInfo", u);
                    }

                }
                return u;
            }
            else
            {
                return new User();
            }
        }

        public User DNNGetCurrentUser(int PortalId, int ModuleId)
        {
            DotNetNuke.Entities.Users.UserInfo cu = DotNetNuke.Entities.Users.UserController.Instance.GetCurrentUserInfo();
            User u = LoadUser(cu);

            u = FillProfile(PortalId, ModuleId, u);
            string fs = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(u.UserRoles, PortalId, ModuleId, "CanApprove");
            if (!(string.IsNullOrEmpty(fs)))
            {
                u.Profile.IsMod = true;
            }
            else
            {
                u.Profile.IsMod = false;
            }

            return u;
        }

        private User GetDNNUser(int portalId, int userId) => LoadUser(new DotNetNuke.Entities.Users.UserController().GetUser(portalId, userId));

        private User GetDNNUser(int portalId, string userName) => LoadUser(DotNetNuke.Entities.Users.UserController.GetUserByName(portalId, userName));

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use GetDNNUser(int portalId, string userName).")]
        public User GetDNNUser(string userName)
        {
            DotNetNuke.Entities.Users.UserInfo dnnUser = DotNetNuke.Entities.Users.UserController.GetUserByName(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId, userName);
            return LoadUser(dnnUser);
        }

        public User GetUser(int PortalId, int ModuleId, int userId)
        {
            User u = GetDNNUser(PortalId, userId);
            if (u != null)
            {
                u = FillProfile(PortalId, ModuleId, u);
                string fs = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(u.UserRoles, PortalId, ModuleId, "CanApprove");
                if (!(string.IsNullOrEmpty(fs)))
                {
                    u.Profile.IsMod = true;
                }
                else
                {
                    u.Profile.IsMod = false;
                }
            }
            return u;
        }

        public User GetUser(int PortalId, int ModuleId, string userName)
        {
            User u = GetDNNUser(PortalId, userName);
            if (u != null)
            {
                u = FillProfile(PortalId, ModuleId, u);
                string fs = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(u.UserRoles, PortalId, ModuleId, "CanApprove");
                if (!(string.IsNullOrEmpty(fs)) || u.IsSuperUser || u.IsAdmin)
                {
                    u.Profile.IsMod = true;
                }
                else
                {
                    u.Profile.IsMod = false;
                }
            }
            return u;
        }

        public User FillProfile(int PortalId, int ModuleId, User u)
        {
            if (u != null && u.UserId > 0)
            {
                u.Profile = new UserProfileController().Profiles_Get(PortalId, ModuleId, u.UserId);
            }
            return u;
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use UserProfileController.Profiles_Get().")]
        public UserProfileInfo Profiles_Get(int PortalId, int ModuleId, int UserId)
        {
            return new UserProfileController().Profiles_Get(PortalId, ModuleId, UserId);
        }

        internal User LoadUser(DotNetNuke.Entities.Users.UserInfo dnnUser)
        {
            PortalSettings _portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings(dnnUser.PortalID);
            User u = new User
            {
                UserId = dnnUser.UserID,
                UserName = dnnUser.Username,
                IsSuperUser = dnnUser.IsSuperUser,
                IsAdmin = dnnUser.IsInRole(_portalSettings.AdministratorRoleName),
                DateCreated = dnnUser.Membership.CreatedDate,
                DateUpdated = dnnUser.Membership.LastActivityDate,
                FirstName = dnnUser.FirstName,
                LastName = dnnUser.LastName,
                DisplayName = dnnUser.DisplayName,
                Email = dnnUser.Email,
                UserRoles = GetRoleIds(dnnUser, _portalSettings.PortalId)
            };

            if (dnnUser.IsSuperUser)
            {
                u.UserRoles += Globals.DefaultAnonRoles + _portalSettings.AdministratorRoleId + ";";
            }
            u.UserRoles += "|" + dnnUser.UserID + "|" + string.Empty + "|";

            if (!dnnUser.IsSuperUser)
            {
                u.Properties = GetUserProperties(dnnUser);
            }

            return u;
        }

        private string GetRoleIds(UserInfo u, int PortalId)
        {
            string RoleIds = string.Empty;
            foreach (DotNetNuke.Security.Roles.RoleInfo r in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: PortalId))
            {
                string roleName = r.RoleName;
                foreach (string role in u.Roles)
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        if (roleName == role)
                        {
                            RoleIds += r.RoleID.ToString() + ";";
                            break;
                        }
                    }
                }
            }

            foreach (DotNetNuke.Security.Roles.RoleInfo r in u.Social.Roles)
            {
                RoleIds += r.RoleID.ToString() + ";";
            }

            return RoleIds;
        }

        public Hashtable GetUserProperties(DotNetNuke.Entities.Users.UserInfo dnnUser)
        {
            Hashtable ht = new Hashtable();
            foreach (DotNetNuke.Entities.Profile.ProfilePropertyDefinition up in dnnUser.Profile.ProfileProperties)
            {
                ht.Add(up.PropertyName, up.PropertyValue);
            }
            return ht;
        }
    }
}
