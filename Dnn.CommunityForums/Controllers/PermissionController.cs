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
using System;
using System.Collections.Specialized;
using System.Data;
using System.Linq;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    public class PermissionController : DotNetNuke.Modules.ActiveForums.Controllers.ControllerBase<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
    {
        internal DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo CreateAdminPermissions(string adminRole)
        {
            string adminRoleId = string.Concat(adminRole, ";||||");
            DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo
            {
                View = adminRoleId,
                Read = adminRoleId,
                Create = adminRoleId,
                Reply = adminRoleId,
                Edit = adminRoleId,
                Delete = adminRoleId,
                Lock = adminRoleId,
                Pin = adminRoleId,
                Attach = adminRoleId,
                Poll = adminRoleId,
                Block = string.Empty, // not sure why this is blank...
                Trust = adminRoleId,
                Subscribe = adminRoleId,
                Announce = adminRoleId,
                ModApprove = adminRoleId,
                ModMove = adminRoleId,
                ModSplit = adminRoleId,
                ModDelete = adminRoleId,
                ModUser = adminRoleId,
                ModEdit = adminRoleId,
                ModLock = adminRoleId,
                ModPin = adminRoleId
            };
            Repo.Insert(permissionInfo);
            return permissionInfo;
        }
        public static bool HasAccess(string AuthorizedRoles, string UserRoles)
        {
            return HasRequiredPerm(AuthorizedRoles.Split(new[] { ';' }), UserRoles.Split(new[] { ';' }));
        }
        public static bool HasRequiredPerm(string[] AuthorizedRoles, string[] UserRoles)
        {
            bool bolAuth = false;
            if (UserRoles != null)
            {
                foreach (string role in AuthorizedRoles)
                {
                    if (!(string.IsNullOrEmpty(role)))
                    {
                        foreach (string AuthRole in UserRoles)
                        {
                            if (!(string.IsNullOrEmpty(AuthRole)))
                            {
                                if (role == AuthRole)
                                {
                                    bolAuth = true;
                                    break;
                                }
                            }
                        }
                        if (bolAuth)
                        {
                            break;
                        }
                    }
                }
                return bolAuth;
            }
            return false;
        }
        internal static System.Collections.Generic.IList<DotNetNuke.Security.Roles.RoleInfo> GetRoles(int PortalId)
        {
            object obj = DataCache.SettingsCacheRetrieve(ModuleId: -1, cacheKey: string.Format(CacheKeys.Roles, PortalId));
            System.Collections.Generic.IList<DotNetNuke.Security.Roles.RoleInfo> roles;
            if (obj == null)
            {
                roles = DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: PortalId);
                DataCache.SettingsCacheStore(ModuleId: -1, cacheKey: string.Format(CacheKeys.Roles, PortalId), cacheObj: roles);
            }
            else
            {
                roles = (System.Collections.Generic.IList<DotNetNuke.Security.Roles.RoleInfo>)obj;
            }
            return roles;
        }
        internal static string GetNamesForRoles(int PortalId, string Roles)
        {
            try
            {
                string RoleNames = string.Empty;
                int i = 0;
                string roleName;
                foreach (string role in Roles.Split(new[] { ';' }))
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        switch (role)
                        {
                            case DotNetNuke.Common.Globals.glbRoleAllUsers:
                                RoleNames = string.Concat(RoleNames + DotNetNuke.Common.Globals.glbRoleAllUsersName, ";");
                                break;
                            case DotNetNuke.Common.Globals.glbRoleUnauthUser:
                                RoleNames = string.Concat(RoleNames + DotNetNuke.Common.Globals.glbRoleUnauthUserName, ";");
                                break;
                            default:
                                roleName = GetRoleName(PortalId: PortalId, role: role);
                                if (roleName != null)
                                {
                                    RoleNames = string.Concat(RoleNames + roleName, ";");
                                }
                                break;
                        }
                    }
                }
                return RoleNames;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return string.Empty;
            }
        }
        private static string GetRoleName(int PortalId, string role)
        {
            return GetRoles(PortalId).ToArray().Where(r => r.RoleID == Utilities.SafeConvertInt(role)).Select(r => r.RoleName).FirstOrDefault();
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use GetRoleIds(int PortalId, string[] Roles).")]
        public static string GetRoleIds(string[] Roles, int PortalId)
        {
            return GetRoleIds(PortalId, Roles);
        }
        public static string GetRoleIds(int PortalId, string[] Roles)
        {
            string RoleIds = string.Empty;
            foreach (string role in Roles)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    RoleIds += string.Concat(GetRoleName(PortalId, role), ";");
                }
            }
            return RoleIds;
        }
        internal static NameValueCollection GetRolesNVC(int PortalId, string Roles)
        {
            try
            {
                var nvc = new NameValueCollection();
                string roleName;
                foreach (string role in Roles.Split(new[] { ';' }))
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        switch (role)
                        {
                            case DotNetNuke.Common.Globals.glbRoleAllUsers:
                                nvc.Add(DotNetNuke.Common.Globals.glbRoleAllUsers, DotNetNuke.Common.Globals.glbRoleAllUsersName);
                                break;
                            case Common.Globals.glbRoleUnauthUser:
                                nvc.Add(Common.Globals.glbRoleUnauthUser, DotNetNuke.Common.Globals.glbRoleUnauthUserName);
                                break;
                            default:
                                roleName = GetRoleName(PortalId, role);
                                if (roleName != null)
                                {
                                    nvc.Add(role, roleName);
                                }
                                break;
                        }
                    }
                }
                return nvc;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return null;
            }
        }
        public static bool HasPerm(string AuthorizedRoles, int UserId, int PortalId)
        {
            string userRoles;
            userRoles = UserRolesDictionary.GetRoles(PortalId, UserId);
            if (string.IsNullOrEmpty(userRoles))
            {
                string[] roles = DotNetNuke.Entities.Users.UserController.GetUserById(PortalId, UserId).Roles;
                string roleIds = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(PortalId, roles);
                userRoles = roleIds + "|" + UserId + "|" + string.Empty + "|";
                UserRolesDictionary.AddRoles(PortalId, UserId, userRoles);
            }
            if (string.IsNullOrEmpty(userRoles))
            {
                return false;
            }
            return HasPerm(AuthorizedRoles, userRoles);
        }


        public static bool HasPerm(string AuthorizedRoles, string UserPermSet)
        {
            if (string.IsNullOrEmpty(AuthorizedRoles) || string.IsNullOrEmpty(UserPermSet))
            {
                return false;
            }
            string[] permSet = AuthorizedRoles.Split('|');
            string[] userSet = UserPermSet.Split('|');
            //Authorized
            string[] authRoles = permSet[0].Split(';');
            string[] userRoles = userSet[0].Split(';');
            if (HasRequiredPerm(authRoles, userRoles))
            {
                return true;
            }
            if (!(string.IsNullOrEmpty(permSet[1])))
            {
                string[] authUsers = permSet[1].Split(';');
                if (!(string.IsNullOrEmpty(userSet[1])))
                {
                    string[] userIds = userSet[1].Split(';');
                    if (HasRequiredPerm(authUsers, userIds))
                    {
                        return true;
                    }
                }
            }
            if (!(string.IsNullOrEmpty(permSet[2])))
            {
                string[] authGroups = permSet[2].Split(';');
                if (!(string.IsNullOrEmpty(userSet[2])))
                {
                    string[] userGroups = userSet[2].Split(';');
                    if (HasRequiredPerm(authGroups, userGroups))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static string RemovePermFromSet(string objectId, int objectType, string PermissionSet)
        {
            if (string.IsNullOrEmpty(PermissionSet))
            {
                return string.Empty;
            }
            string newSet = PermissionSet;
            string[] permSet = newSet.Split('|');
            string permSection = permSet[objectType];
            string newSection = string.Empty;
            foreach (string s in permSection.Split(';'))
            {
                if (!(string.IsNullOrEmpty(s)))
                {
                    if (s != objectId)
                    {
                        newSection += string.Concat(s, ";");
                    }
                }
            }
            permSet[objectType] = newSection;
            if (permSet[0] != null)
            {
                newSet = string.Concat(permSet[0], "|");
            }
            else
            {
                newSet = "|";
            }
            if (permSet[1] != null)
            {
                newSet += string.Concat(permSet[1], "|");
            }
            else
            {
                newSet += "|";
            }
            if (permSet[2] != null)
            {
                newSet += string.Concat(permSet[2], "|");
            }
            else
            {
                newSet += "|";
            }
            return newSet;
        }
        public static string AddPermToSet(string objectId, int objectType, string PermissionSet)
        {
            string newSet = RemovePermFromSet(objectId, objectType, PermissionSet);
            string[] permSet = newSet.Split('|');
            permSet[objectType] += string.Concat(objectId, ";");
            newSet = string.Concat(permSet[0] + "|" + permSet[1] + "|" + permSet[2], "|");
            return newSet;
        }
        internal static bool RemoveObjectFromAll(string objectId, int objectType, int PermissionsId)
        {
            var enumType = typeof(SecureActions);
            Array values = Enum.GetValues(enumType);
            var db = new Data.Common();
            for (int i = 0; i < values.Length; i++)
            {
                string text = Convert.ToString(Enum.Parse(enumType, values.GetValue(i).ToString()));
                //string permSet = GetPermSet(PermissionsId, i);
                string permSet = db.GetPermSet(PermissionsId, text);
                permSet = RemovePermFromSet(objectId, objectType, permSet);
                db.SavePermSet(PermissionsId, text, permSet);
                //SavePermSet(PermissionsId, i, permSet);
            }
            return true;
        }
        //internal string GetPermSet(int permissionsId, SecureActions action)
        //{
        //    var permissionInfo = Repo.GetById(permissionsId);
        //    switch (action)
        //    {
        //        case SecureActions.View:
        //            return permissionInfo.View;
        //        case SecureActions.Read:
        //            return permissionInfo.Read;
        //        case SecureActions.Create:
        //            return permissionInfo.Create;
        //        case SecureActions.Reply:
        //            return permissionInfo.Reply;
        //        case SecureActions.Edit:
        //            return permissionInfo.Edit;
        //        case SecureActions.Delete:
        //            return permissionInfo.Delete;
        //        case SecureActions.Lock:
        //            return permissionInfo.Lock;
        //        case SecureActions.Pin:
        //            return permissionInfo.Pin;
        //        case SecureActions.Attach:
        //            return permissionInfo.Attach;
        //        case SecureActions.Poll:
        //            return permissionInfo.Poll;
        //        case SecureActions.Block:
        //            return permissionInfo.Block;
        //        case SecureActions.Trust:
        //            return permissionInfo.Trust;
        //        case SecureActions.Subscribe:
        //            return permissionInfo.Subscribe;
        //        case SecureActions.Announce:
        //            return permissionInfo.Announce;
        //        case SecureActions.Tag:
        //            return permissionInfo.Tag;
        //        case SecureActions.Categorize:
        //            return permissionInfo.Categorize;
        //        case SecureActions.Prioritize:
        //            return permissionInfo.Prioritize;
        //        case SecureActions.ModApprove:
        //            return permissionInfo.ModApprove;
        //        case SecureActions.ModMove:
        //            return permissionInfo.ModMove; 
        //        case SecureActions.ModSplit:
        //            return permissionInfo.ModSplit; 
        //        case SecureActions.ModDelete:
        //            return permissionInfo.ModDelete; 
        //        case SecureActions.ModUser:
        //            return permissionInfo.ModUser; 
        //        case SecureActions.ModEdit:
        //            return permissionInfo.ModEdit; 
        //        case SecureActions.ModLock:
        //            return permissionInfo.ModLock; 
        //        case SecureActions.ModPin:
        //            return permissionInfo.ModPin; 
        //        default:
        //            return "|||";
        //    }

        //}
        //internal static void SavePermSet(int permissionsId, SecureActions action, string permSet)
        //{


        //}
        internal static void CreateDefaultSets(int PortalId, int PermissionsId)
        {
            var db = new Data.Common();
            string RoleId = GetRoles(PortalId).Where(r => r.RoleName == "Registered Users").FirstOrDefault().RoleID.ToString();
            string permSet;
            if (!string.IsNullOrEmpty(RoleId))
            {
                permSet = db.GetPermSet(PermissionsId, "View");
                permSet = AddPermToSet(RoleId, 0, permSet);
                db.SavePermSet(PermissionsId, "View", permSet);
                permSet = db.GetPermSet(PermissionsId, "Read");
                permSet = AddPermToSet(RoleId, 0, permSet);
                db.SavePermSet(PermissionsId, "Read", permSet);
                permSet = db.GetPermSet(PermissionsId, "Create");
                permSet = AddPermToSet(RoleId, 0, permSet);
                db.SavePermSet(PermissionsId, "Create", permSet);
                permSet = db.GetPermSet(PermissionsId, "Reply");
                permSet = AddPermToSet(RoleId, 0, permSet);
                db.SavePermSet(PermissionsId, "Reply", permSet);
            }
            RoleId = Common.Globals.glbRoleAllUsers;
            permSet = db.GetPermSet(PermissionsId, "View");
            permSet = AddPermToSet(RoleId, 0, permSet);
            db.SavePermSet(PermissionsId, "View", permSet);
            permSet = db.GetPermSet(PermissionsId, "Read");
            permSet = AddPermToSet(RoleId, 0, permSet);
            db.SavePermSet(PermissionsId, "Read", permSet);

            RoleId = Common.Globals.glbRoleUnauthUser;
            permSet = db.GetPermSet(PermissionsId, "View");
            permSet = AddPermToSet(RoleId, 0, permSet);
            db.SavePermSet(PermissionsId, "View", permSet);
            permSet = db.GetPermSet(PermissionsId, "Read");
            permSet = AddPermToSet(RoleId, 0, permSet);
            db.SavePermSet(PermissionsId, "Read", permSet);
        }
    }
}
