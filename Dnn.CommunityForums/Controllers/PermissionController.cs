// Copyright (c) 2013-2024 by DNN Community
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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections.Specialized;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Web.Razor.Parser.SyntaxTree;
    using System.Web.Security;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Controls;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Modules.ActiveForums.API;
    using DotNetNuke.Modules.ActiveForums.DAL2;
    using Microsoft.ApplicationBlocks.Data;

    internal class PermissionController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
    {
        private const string emptyPermissions = ";|||";
        internal new void DeleteById<TProperty>(TProperty permissionsId, int moduleId)
        {
            var cachekey = string.Format(CacheKeys.PermissionsInfo, moduleId, permissionsId);
            DataCache.SettingsCacheClear(moduleId, cachekey);
            this.DeleteById(permissionsId);
        }

        internal new void Delete(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo)
        {
            var cachekey = string.Format(CacheKeys.PermissionsInfo, permissionInfo.ModuleId, permissionInfo.PermissionsId);
            DataCache.SettingsCacheClear(permissionInfo.ModuleId, cachekey);
            base.Delete(permissionInfo);
        }

        internal new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo Insert(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo)
        {
            base.Insert(permissionInfo);
            return this.GetById(permissionInfo.PermissionsId, permissionInfo.ModuleId);
        }

        internal new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo Update(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo)
        {
            base.Update(permissionInfo);
            return this.GetById(permissionInfo.PermissionsId, permissionInfo.ModuleId);
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo GetById(int permissionId, int moduleId)
        {
            var cachekey = string.Format(CacheKeys.PermissionsInfo, moduleId, permissionId);
            DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissions = DataCache.SettingsCacheRetrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo;
            if (permissions == null)
            {
                permissions = base.GetById(permissionId, moduleId);
                DataCache.SettingsCacheStore(moduleId, cachekey, permissions);
            }

            return permissions;
        }

        internal static int GetAdministratorsRoleId(int portalId)
        {
            return Utilities.GetPortalSettings(portalId).AdministratorRoleId;
        }

        internal static string GetAdministratorsRoleName(int portalId)
        {
            return Utilities.GetPortalSettings(portalId).AdministratorRoleName;
        }

        internal static int GetRegisteredRoleId(int portalId)
        {
            return Utilities.GetPortalSettings(portalId).RegisteredRoleId;
        }

        internal static string GetRegisteredRoleName(int portalId)
        {
            return Utilities.GetPortalSettings(portalId).RegisteredRoleName;
        }

        internal static void CreateDefaultSets(int portalId, int moduleId, int permissionsId)
        {
            string[] requestedAccessList = new[] { "View", "Read" };
            string registeredUsersRoleId = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredRoleId(portalId).ToString();
            foreach (string access in requestedAccessList)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, access, registeredUsersRoleId, 0);
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, access, DotNetNuke.Common.Globals.glbRoleAllUsers, 0);
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, access, DotNetNuke.Common.Globals.glbRoleUnauthUser, 0);
            }

            requestedAccessList = new[] { "Create", "Reply" };
            foreach (string access in requestedAccessList)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, access, registeredUsersRoleId, 0);
            }
        }
        internal DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo CreateAdminPermissions(string adminRole, int moduleId)
        {
            string adminRoleId = $"{adminRole};{emptyPermissions}";
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
                Block = adminRoleId,
                Trust = adminRoleId,
                Subscribe = adminRoleId,
                Announce = adminRoleId,
                Prioritize = adminRoleId,
                Moderate = adminRoleId,
                Move = adminRoleId,
                Split = adminRoleId,
                Ban = adminRoleId,
                ModuleId = moduleId,
            };
            this.Insert(permissionInfo);
            return permissionInfo;
        }

        internal static DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo GetEmptyPermissions(int moduleId)
        {
            return new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo
            {
                View = emptyPermissions,
                Read = emptyPermissions,
                Create = emptyPermissions,
                Reply = emptyPermissions,
                Edit = emptyPermissions,
                Delete = emptyPermissions,
                Lock = emptyPermissions,
                Pin = emptyPermissions,
                Attach = emptyPermissions,
                Poll = emptyPermissions,
                Block = emptyPermissions,
                Trust = emptyPermissions,
                Subscribe = emptyPermissions,
                Announce = emptyPermissions,
                Prioritize = emptyPermissions,
                Moderate = emptyPermissions,
                Move = emptyPermissions,
                Split = emptyPermissions,
                Ban = emptyPermissions,
                ModuleId = moduleId,
            };
        }

        public static bool HasAccess(string authorizedRoles, string userRoles)
        {
            return string.IsNullOrEmpty(authorizedRoles) || string.IsNullOrEmpty(userRoles) ? false : HasRequiredPerm(authorizedRoles.Split(new[] { ';' }), userRoles.Split(new[] { ';' }));
        }

        internal static bool HasRequiredPerm(string[] authorizedRoles, string[] userRoles)
        {
            bool bolAuth = false;
            if (userRoles != null)
            {
                foreach (string role in authorizedRoles)
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        foreach (string authRole in userRoles)
                        {
                            if (!string.IsNullOrEmpty(authRole))
                            {
                                if (role == authRole)
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

        internal static System.Collections.Generic.IList<DotNetNuke.Security.Roles.RoleInfo> GetRoles(int portalId)
        {
            object obj = DataCache.SettingsCacheRetrieve(moduleId: -1, cacheKey: string.Format(CacheKeys.Roles, portalId));
            System.Collections.Generic.IList<DotNetNuke.Security.Roles.RoleInfo> roles;
            if (obj == null)
            {
                roles = DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: portalId);

                // add pseudo-roles for anon/unauth and all users
                roles.Add(new DotNetNuke.Security.Roles.RoleInfo { RoleID = int.Parse(DotNetNuke.Common.Globals.glbRoleUnauthUser), RoleName = DotNetNuke.Common.Globals.glbRoleUnauthUserName });
                roles.Add(new DotNetNuke.Security.Roles.RoleInfo { RoleID = int.Parse(DotNetNuke.Common.Globals.glbRoleAllUsers), RoleName = DotNetNuke.Common.Globals.glbRoleAllUsersName });
                DataCache.SettingsCacheStore(moduleId: -1, cacheKey: string.Format(CacheKeys.Roles, portalId), cacheObj: roles);
            }
            else
            {
                roles = (System.Collections.Generic.IList<DotNetNuke.Security.Roles.RoleInfo>)obj;
            }

            return roles;
        }

        internal static string GetNamesForRoles(int portalId, string roles)
        {
            try
            {
                string roleNames = string.Empty;
                string roleName;
                foreach (string role in roles.Split(new[] { ';' }))
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        switch (role)
                        {
                            case DotNetNuke.Common.Globals.glbRoleAllUsers:
                                roleNames = string.Concat(roleNames + DotNetNuke.Common.Globals.glbRoleAllUsersName, ";");
                                break;
                            case DotNetNuke.Common.Globals.glbRoleUnauthUser:
                                roleNames = string.Concat(roleNames + DotNetNuke.Common.Globals.glbRoleUnauthUserName, ";");
                                break;
                            default:
                                roleName = GetRoleName(portalId: portalId, role: role);
                                if (roleName != null)
                                {
                                    roleNames = string.Concat(roleNames + roleName, ";");
                                }

                                break;
                        }
                    }
                }

                return roleNames;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return string.Empty;
            }
        }

        internal static string GetRoleName(int portalId, string role)
        {
            return GetRoles(portalId).Where(r => r.RoleID == Utilities.SafeConvertInt(role)).Select(r => r.RoleName).FirstOrDefault();
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use GetRoleIds(int PortalId, string[] Roles).")]
        public static string GetRoleIds(string[] roles, int portalId) => GetRoleIds(portalId, roles);

        internal static string GetRoleIds(int portalId, string[] roles)
        {
            string roleIds = (string)DataCache.SettingsCacheRetrieve(-1, string.Format(CacheKeys.RoleIDs, portalId));
            if (string.IsNullOrEmpty(roleIds))
            {
                foreach (DotNetNuke.Security.Roles.RoleInfo ri in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: portalId))
                {
                    string roleName = ri.RoleName;
                    foreach (string role in roles)
                    {
                        if (!string.IsNullOrEmpty(role))
                        {
                            if (roleName == role)
                            {
                                roleIds += string.Concat(ri.RoleID.ToString(), ";");
                                break;
                            }
                        }
                    }
                }

                // add pseudo-roles for anon/unauth and all users
                roleIds = string.Concat(roleIds, DotNetNuke.Common.Globals.glbRoleAllUsers, ";", DotNetNuke.Common.Globals.glbRoleUnauthUser, ";");
                DataCache.SettingsCacheStore(-1, string.Format(CacheKeys.RoleIDs, portalId), roleIds);
            }

            return roleIds;
        }

        internal static NameValueCollection GetRolesNVC(int portalId, string roles)
        {
            try
            {
                var nvc = new NameValueCollection();
                string roleName;
                foreach (string role in roles.Split(new[] { ';' }))
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
                                roleName = GetRoleName(portalId, role);
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
        
        public static bool HasPerm(string authorizedRoles, int userId, int portalId)
        {
            string userRoles;
            userRoles = UserRolesDictionary.GetRoles(portalId, userId);
            if (string.IsNullOrEmpty(userRoles))
            {
                string[] roles = DotNetNuke.Entities.Users.UserController.GetUserById(portalId, userId).Roles;
                string roleIds = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(portalId, roles);
                userRoles = roleIds + "|" + userId + "|" + string.Empty + "|";
                UserRolesDictionary.AddRoles(portalId, userId, userRoles);
            }

            if (string.IsNullOrEmpty(userRoles))
            {
                return false;
            }

            return HasPerm(authorizedRoles, userRoles);
        }

        public string GetPermSet(int moduleId, int permissionsId, string requestedAccess)
        {
            var permission = this.GetById(permissionsId, moduleId);
            return this.GetRolesForRequestedAccess(permission, requestedAccess);
        }

        public string SavePermSet(int moduleId, int permissionsId, string requestedAccess, string permSet)
        {
            var permission = this.GetById(permissionsId, moduleId);
            if (permission != null)
            {
                this.SetRolesForRequestedAccess(permission, requestedAccess, permSet);
            }

            return this.GetPermSet(moduleId, permissionsId, requestedAccess);
        }

        public static void AddObjectToPermissions(int moduleId, int permissionsId, string requestedAccess, string objectId, int objectType)
        {
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController();
            string permSet = pc.GetPermSet(moduleId, permissionsId, requestedAccess);
            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(objectId, objectType, permSet);
            pc.SavePermSet(moduleId, permissionsId, requestedAccess, permSet);
        }

        public static void RemoveObjectFromPermissions(int moduleId, int permissionsId, string requestedAccess, string objectId, int objectType)
        {
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController();
            string permSet = pc.GetPermSet(moduleId, permissionsId, requestedAccess);
            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemovePermFromSet(objectId, objectType, permSet);
            pc.SavePermSet(moduleId, permissionsId, requestedAccess, permSet);
        }

        public static bool HasPerm(string authorizedRoles, string userPermSet)
        {
            if (string.IsNullOrEmpty(authorizedRoles) || string.IsNullOrEmpty(userPermSet))
            {
                return false;
            }

            string[] permSet = authorizedRoles.Split('|');
            string[] userSet = userPermSet.Split('|');

            // Authorized
            string[] authRoles = permSet[0].Split(';');
            string[] userRoles = userSet[0].Split(';');
            if (HasRequiredPerm(authRoles, userRoles))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(permSet[1]))
            {
                string[] authUsers = permSet[1].Split(';');
                if (!string.IsNullOrEmpty(userSet[1]))
                {
                    string[] userIds = userSet[1].Split(';');
                    if (HasRequiredPerm(authUsers, userIds))
                    {
                        return true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(permSet[2]))
            {
                string[] authGroups = permSet[2].Split(';');
                if (!string.IsNullOrEmpty(userSet[2]))
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

        public static string RemovePermFromSet(string objectId, int objectType, string permissionSet)
        {
            if (string.IsNullOrEmpty(permissionSet))
            {
                return string.Empty;
            }

            string newSet = permissionSet;
            string[] permSet = newSet.Split('|');
            string permSection = permSet[objectType];
            string newSection = string.Empty;
            foreach (string s in permSection.Split(';'))
            {
                if (!string.IsNullOrEmpty(s))
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

        public static string AddPermToSet(string objectId, int objectType, string permissionSet)
        {
            string newSet = RemovePermFromSet(objectId, objectType, permissionSet);
            string[] permSet = newSet.Split('|');
            permSet[objectType] += string.Concat(objectId, ";");
            newSet = string.Concat(permSet[0] + "|" + permSet[1] + "|" + permSet[2], "|");
            return newSet;
        }

        internal static bool RemoveObjectFromAll(int moduleId, string objectId, int objectType, int permissionsId)
        {
            var enumType = typeof(SecureActions);
            Array values = Enum.GetValues(enumType);
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController();
            for (int i = 0; i < values.Length; i++)
            {
                string text = Convert.ToString(Enum.Parse(enumType, values.GetValue(i).ToString()));
                string permSet = pc.GetPermSet(moduleId, permissionsId, text);
                permSet = RemovePermFromSet(objectId, objectType, permSet);
                pc.SavePermSet(moduleId, permissionsId, text, permSet);
            }

            return true;
        }

        internal static string GetSecureObjectList(IPortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo s, int objectType)
        {
            string roleObjects = string.Empty;

            roleObjects = GetObjFromSecObj(portalSettings, s.Announce, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Attach, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Categorize, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Create, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Delete, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Edit, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Lock, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Moderate, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Move, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Split, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Ban, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Pin, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Poll, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Prioritize, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Read, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Reply, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Subscribe, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Tag, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Trust, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.View, objectType, roleObjects);

            return roleObjects;
        }

        internal static string GetObjFromSecObj(IPortalSettings portalSettings, string permSet, int index, string objects)
        {
            if (string.IsNullOrEmpty(permSet))
            {
                permSet = portalSettings.AdministratorRoleId + ";|||";
            }

            string[] perms = permSet.Split('|');
            if (perms[index] != null)
            {
                if (!string.IsNullOrEmpty(perms[index]))
                {
                    foreach (string s in perms[index].Split(';'))
                    {
                        if (!string.IsNullOrEmpty(s))
                        {
                            if (Array.IndexOf(objects.Split(';'), s) == -1)
                            {
                                objects += s + ";";
                            }
                        }
                    }
                }
            }

            return objects;
        }

        internal string GetRolesForRequestedAccess(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permission, string requestedAccess)
        {
            if (permission == null)
            {
                return emptyPermissions;
            }

            string access = string.Empty;
            switch (requestedAccess.ToUpperInvariant())
            {
                case "ANNOUNCE":
                    access = permission.Announce;
                    break;
                case "ATTACH":
                    access = permission.Attach;
                    break;
                case "CATEGORIZE":
                    access = permission.Categorize;
                    break;
                case "CREATE":
                    access = permission.Create;
                    break;
                case "DELETE":
                    access = permission.Delete;
                    break;
                case "EDIT":
                    access = permission.Edit;
                    break;
                case "LOCK":
                    access = permission.Lock;
                    break;
                case "PIN":
                    access = permission.Pin;
                    break;
                case "POLL":
                    access = permission.Poll;
                    break;
                case "PRIORITIZE":
                    access = permission.Prioritize;
                    break;
                case "READ":
                    access = permission.Read;
                    break;
                case "REPLY":
                    access = permission.Reply;
                    break;
                case "SUBSCRIBE":
                    access = permission.Subscribe;
                    break;
                case "TAG":
                    access = permission.Tag;
                    break;
                case "TRUST":
                    access = permission.Trust;
                    break;
                case "VIEW":
                    access = permission.View;
                    break;
                case "MODERATE":
                    access = permission.Moderate;
                    break;
                case "MOVE":
                    access = permission.Move;
                    break;
                case "SPLIT":
                    access = permission.Split;
                    break;
                case "BAN":
                    access = permission.Ban;
                    break;
                default:
                    access = emptyPermissions;
                    break;
            }

            if (string.IsNullOrEmpty(access))
            {
                access = emptyPermissions;
            }

            return access;
        }

        internal void SetRolesForRequestedAccess(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permission, string requestedAccess, string permSet)
        {
            if (permission != null)
            {
                switch (requestedAccess.ToUpperInvariant())
                {
                    case "ANNOUNCE":
                        permission.Announce = permSet;
                        break;
                    case "ATTACH":
                        permission.Attach = permSet;
                        break;
                    case "CATEGORIZE":
                        permission.Categorize = permSet;
                        break;
                    case "CREATE":
                        permission.Create = permSet;
                        break;
                    case "DELETE":
                        permission.Delete = permSet;
                        break;
                    case "EDIT":
                        permission.Edit = permSet;
                        break;
                    case "LOCK":
                        permission.Lock = permSet;
                        break;
                    case "PIN":
                        permission.Pin = permSet;
                        break;
                    case "POLL":
                        permission.Poll = permSet;
                        break;
                    case "PRIORITIZE":
                        permission.Prioritize = permSet;
                        break;
                    case "READ":
                        permission.Read = permSet;
                        break;
                    case "REPLY":
                        permission.Reply = permSet;
                        break;
                    case "SUBSCRIBE":
                        permission.Subscribe = permSet;
                        break;
                    case "TAG":
                        permission.Tag = permSet;
                        break;
                    case "TRUST":
                        permission.Trust = permSet;
                        break;
                    case "VIEW":
                        permission.View = permSet;
                        break;
                    case "MODERATE":
                        permission.Moderate = permSet; 
                        break;
                    case "MOVE":
                        permission.Move = permSet; 
                        break;
                    case "SPLIT":
                        permission.Split = permSet; 
                        break;
                    case "BAN":
                        permission.Ban = permSet;
                        break;
                    default:
                        break;
                }

                this.Update(permission);
            }
        }

        public static string WhichRolesCanViewForum(int moduleId, int forumId, string userRoles)
        {
            string cacheKey = string.Format(CacheKeys.ViewRolesForForum, moduleId, forumId);
            string sRoles = (string)DataCache.SettingsCacheRetrieve(moduleId, cacheKey);

            if (string.IsNullOrEmpty(sRoles))
            {
                var forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId, moduleId);
                int portalId = forum.PortalId;
                int permissionId = forum.PermissionsId;

                var permission = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(permissionId, moduleId);

                string canView = permission.View;
                foreach (string role in userRoles.Split(";".ToCharArray()))
                {
                    if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(canView, string.Concat(role, ";||")))
                    {
                        sRoles += role + ":";
                    }
                }

                DataCache.SettingsCacheStore(moduleId, cacheKey, sRoles);
            }

            return sRoles;
        }

        public static string CheckForumIdsForViewForRSS(int moduleId, string forumIds, string userRoles)
        {
            string cacheKey = string.Format(CacheKeys.ViewRolesForForumList, moduleId, forumIds);
            string sForums = (string)DataCache.SettingsCacheRetrieve(moduleId, cacheKey);
            if (string.IsNullOrEmpty(sForums))
            {
                sForums = string.Empty;
                if (!string.IsNullOrEmpty(forumIds))
                {
                    foreach (string forumId in forumIds.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(Convert.ToInt32(forumId));
                        if (forum.AllowRSS && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum.Security?.View, userRoles))
                        {
                            sForums += forum.ForumID.ToString() + ":";
                        }
                    }
                }

                DataCache.SettingsCacheStore(moduleId, cacheKey, sForums);
            }

            return sForums;
        }
    }
}
