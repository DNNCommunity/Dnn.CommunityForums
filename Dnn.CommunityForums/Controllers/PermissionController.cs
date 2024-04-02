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
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.UI.WebControls;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.ActiveForums.DAL2;
using Microsoft.ApplicationBlocks.Data;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal class PermissionController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
    {
        private const string emptyPermissions = "||||";
        /// <summary>
        ///
        /// GetById needs to intercept and route to GetById without scope since PermissionsInfo does not yet have ModuleId
        ///
        /// </summary>
        /// <param name="permissionId"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        internal DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo GetById(int permissionId, int moduleId)
        {
            return base.GetById(permissionId);
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
        internal static void CreateDefaultSets(int PortalId, int PermissionsId)
        {
            string[] requestedAccessList = new[] { "View", "Read" };
            string RegisteredUsersRoleId = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredRoleId(PortalId).ToString();
            foreach (string access in requestedAccessList)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(PermissionsId, PermissionsId, access, RegisteredUsersRoleId, 0);
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(PermissionsId, PermissionsId, access, DotNetNuke.Common.Globals.glbRoleAllUsers, 0);
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(PermissionsId, PermissionsId, access, DotNetNuke.Common.Globals.glbRoleUnauthUser, 0);
            }
            requestedAccessList = new[] { "Create", "Reply" };
            foreach (string access in requestedAccessList)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(PermissionsId, PermissionsId, access, RegisteredUsersRoleId, 0);
            }
        }
        internal DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo CreateAdminPermissions(string adminRole)
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
                Tag = adminRoleId,
                Prioritize = adminRoleId,
                Categorize = adminRoleId,
                ModApprove = adminRoleId,
                ModMove = adminRoleId,
                ModSplit = adminRoleId,
                ModDelete = adminRoleId,
                ModUser = adminRoleId,
                ModEdit = adminRoleId,
                ModLock = adminRoleId,
                ModPin = adminRoleId
            };
            Insert(permissionInfo);
            return permissionInfo;
        }
        public static bool HasAccess(string AuthorizedRoles, string UserRoles)
        {
            return HasRequiredPerm(AuthorizedRoles.Split(new[] { ';' }), UserRoles.Split(new[] { ';' }));
        }
        internal static bool HasRequiredPerm(string[] AuthorizedRoles, string[] UserRoles)
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
        internal static string GetRoleName(int PortalId, string role)
        {
            return GetRoles(PortalId).Where(r => r.RoleID == Utilities.SafeConvertInt(role)).Select(r => r.RoleName).FirstOrDefault();
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use GetRoleIds(int PortalId, string[] Roles).")]
        public static string GetRoleIds(string[] Roles, int PortalId) => GetRoleIds(PortalId, Roles);
        internal static string GetRoleIds(int PortalId, string[] Roles)
        {
            string RoleIds = (string)DataCache.SettingsCacheRetrieve(-1, string.Format(CacheKeys.RoleIDs, PortalId));
            if (string.IsNullOrEmpty(RoleIds))
            {
                foreach (DotNetNuke.Security.Roles.RoleInfo ri in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: PortalId))
                {
                    string roleName = ri.RoleName;
                    foreach (string role in Roles)
                    {
                        if (!string.IsNullOrEmpty(role))
                        {
                            if (roleName == role)
                            {
                                RoleIds += string.Concat(ri.RoleID.ToString(), ";");
                                break;
                            }
                        }
                    }
                }
                DataCache.SettingsCacheStore(-1, string.Format(CacheKeys.RoleIDs, PortalId), RoleIds);
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

        public string GetPermSet(int PermissionsId, string requestedAccess)
        {
            var permission = base.GetById(PermissionsId);
            return GetRolesForRequestedAccess(permission, requestedAccess);
        }
        public string SavePermSet(int PermissionsId, string requestedAccess, string PermSet)
        {
            var permission = base.GetById(PermissionsId);
            if (permission != null)
            {
                SetRolesForRequestedAccess(permission, requestedAccess, PermSet);
            }
            return GetPermSet(PermissionsId, requestedAccess);
        }
        public static void AddObjectToPermissions(int ModuleId, int PermissionsId, string requestedAccess, string objectId, int objectType)
        {
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController(ModuleId);
            string permSet = pc.GetPermSet(PermissionsId, requestedAccess);
            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(objectId, objectType, permSet);
            pc.SavePermSet(PermissionsId, requestedAccess, permSet);
        }
        public static void RemoveObjectFromPermissions(int ModuleId, int PermissionsId, string requestedAccess, string objectId, int objectType)
        {
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController(ModuleId);
            string permSet = pc.GetPermSet(PermissionsId, requestedAccess);
            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemovePermFromSet(objectId, objectType, permSet);
            pc.SavePermSet(PermissionsId, requestedAccess, permSet);
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
            string newSet = RemovePermFromSet( objectId, objectType, PermissionSet);
            string[] permSet = newSet.Split('|');
            permSet[objectType] += string.Concat(objectId, ";");
            newSet = string.Concat(permSet[0] + "|" + permSet[1] + "|" + permSet[2], "|");
            return newSet;
        }
        internal static bool RemoveObjectFromAll(int ModuleId, string objectId, int objectType, int PermissionsId)
        {
            var enumType = typeof(SecureActions);
            Array values = Enum.GetValues(enumType);
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController(ModuleId);
            for (int i = 0; i < values.Length; i++)
            {
                string text = Convert.ToString(Enum.Parse(enumType, values.GetValue(i).ToString()));
                string permSet = pc.GetPermSet(PermissionsId, text);
                permSet = RemovePermFromSet(objectId, objectType, permSet);
                pc.SavePermSet(PermissionsId, text, permSet);
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
            roleObjects = GetObjFromSecObj(portalSettings, s.ModApprove, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.ModDelete, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.ModEdit, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.ModLock, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.ModMove, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.ModPin, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.ModSplit, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.ModUser, objectType, roleObjects);
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
                permSet = portalSettings.AdministratorRoleId + ";||||";
            }
            string[] perms = permSet.Split('|');
            if (perms[index] != null)
            {
                if (!(string.IsNullOrEmpty(perms[index])))
                {
                    foreach (string s in perms[index].Split(';'))
                    {
                        if (!(string.IsNullOrEmpty(s)))
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
            };

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
                case "MODAPPROVE":
                    access = permission.ModApprove;
                    break;
                case "MODDELETE":
                    access = permission.ModDelete;
                    break;
                case "MODEDIT":
                    access = permission.ModEdit;
                    break;
                case "MODLOCK":
                    access = permission.ModLock;
                    break;
                case "MODMOVE":
                    access = permission.ModMove;
                    break;
                case "MODPIN":
                    access = permission.ModPin;
                    break;
                case "MODSPLIT":
                    access = permission.ModSplit;
                    break;
                case "MODUSER":
                    access = permission.ModUser;
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
        internal void SetRolesForRequestedAccess(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permission, string requestedAccess, string PermSet)
        {
            if (permission != null)
            {
                switch (requestedAccess.ToUpperInvariant())
                {
                    case "ANNOUNCE":
                        permission.Announce = PermSet; 
                        break;
                    case "ATTACH":
                        permission.Attach = PermSet; 
                        break;
                    case "CATEGORIZE":
                        permission.Categorize = PermSet; 
                        break;
                    case "CREATE":
                        permission.Create = PermSet; 
                        break;
                    case "DELETE":
                        permission.Delete = PermSet; 
                        break;
                    case "EDIT":
                        permission.Edit = PermSet; 
                        break;
                    case "LOCK":
                        permission.Lock = PermSet; 
                        break;
                    case "PIN":
                        permission.Pin = PermSet; 
                        break;
                    case "POLL":
                        permission.Poll = PermSet; 
                        break;
                    case "PRIORITIZE":
                        permission.Prioritize = PermSet; 
                        break;
                    case "READ":
                        permission.Read = PermSet; 
                        break;
                    case "REPLY":
                        permission.Reply = PermSet; 
                        break;
                    case "SUBSCRIBE":
                        permission.Subscribe = PermSet; 
                        break;
                    case "TAG":
                        permission.Tag = PermSet;
                        break;
                    case "TRUST":
                        permission.Trust = PermSet; 
                        break;
                    case "VIEW":
                        permission.View = PermSet;
                        break;
                    case "MODAPPROVE":
                        permission.ModApprove = PermSet; 
                        break;
                    case "MODDELETE":
                        permission.ModDelete = PermSet; 
                        break;
                    case "MODEDIT":
                        permission.ModEdit = PermSet; 
                        break;
                    case "MODLOCK":
                        permission.ModLock = PermSet; 
                        break;
                    case "MODMOVE":
                        permission.ModMove = PermSet; 
                        break;
                    case "MODPIN":
                        permission.ModPin = PermSet; 
                        break;
                    case "MODSPLIT":
                        permission.ModSplit = PermSet; 
                        break;
                    case "MODUSER":
                        permission.ModUser = PermSet;
                        break;
                    default:
                        break;
                } 
                Update(permission);
            };
        }
    }
}
