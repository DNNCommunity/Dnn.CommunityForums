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

using DotNetNuke.Collections;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Web.Security;
    using System.Xml;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Modules.ActiveForums.Extensions;

    internal class PermissionController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
    {
        private const string emptyPermissions = "";

        internal override string cacheKeyTemplate => CacheKeys.PermissionsInfo;

        internal new void DeleteById<TProperty>(TProperty permissionsId, int moduleId)
        {
            var cachekey = this.GetCacheKey(moduleId: moduleId, id: permissionsId);
            DataCache.SettingsCacheClear(moduleId, cachekey);
            this.DeleteById(permissionsId);
        }

        internal new void Delete(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo)
        {
            var cachekey = this.GetCacheKey(moduleId: permissionInfo.ModuleId, id: permissionInfo.PermissionsId);
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
            var cachekey = this.GetCacheKey(moduleId: permissionInfo.ModuleId, id: permissionInfo.PermissionsId);
            DataCache.SettingsCacheClear(permissionInfo.ModuleId, cachekey);
            base.Update(permissionInfo);
            return this.GetById(permissionInfo.PermissionsId, permissionInfo.ModuleId);
        }

        internal void UpdateSecurityForSocialGroupForum(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum)
        {
            var permissions = this.GetById(forum.PermissionsId, forum.ModuleId);
            Hashtable htSettings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: forum.ModuleId, tabId: forum.GetTabId(), ignoreCache: false).TabModuleSettings;
            if (htSettings == null || htSettings.Count == 0)
            {
                htSettings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: forum.ModuleId, tabId: forum.GetTabId(), ignoreCache: false).ModuleSettings;
            }

            if (htSettings == null || htSettings.Count == 0 || !htSettings.ContainsKey("ForumConfig"))
            {
                var ex = new Exception($"Unable to configure forum security for Social Group: {forum.SocialGroupId}");
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            else
            {
                var xDoc = new XmlDocument();
                xDoc.LoadXml(htSettings["ForumConfig"].ToString());
                XmlNode xRoot = xDoc.DocumentElement;
                var roleIdAnon = Convert.ToInt32(Common.Globals.glbRoleUnauthUser);
                var roleIdAdmin = GetAdministratorsRoleId(forum.PortalSettings);
                var roleIdRegUsers = GetRegisteredUsersRoleId(forum.PortalSettings);
                var roleIdGroupMember = forum.SocialGroupId;
                foreach (var sectype in new string[] { "groupadmin", "groupmember", "registereduser", "anon" })
                {
                    var xNode = xRoot.SelectSingleNode("//defaultforums/forum/security[@type='" + sectype + "']");
                    foreach (XmlNode sNode in xNode.ChildNodes)
                    {
                        var sPerm = sNode.Name;
                        var roleId = -1;
                        switch (sectype)
                        {
                            case "groupadmin":
                                roleId = roleIdAdmin;
                                break;
                            case "groupmember":
                                roleId = roleIdGroupMember;
                                break;
                            case "registereduser":
                                roleId = roleIdRegUsers;
                                break;
                            case "anon":
                                roleId = roleIdAnon;
                                break;
                        }

                        if (Convert.ToBoolean(sNode.Attributes["value"].Value))
                        {
                            switch (sPerm)
                            {
                                case "view":
                                    permissions.View = AddPermToSet(roleId.ToString(), permissions.View);
                                    break;
                                case "read":
                                    permissions.Read = AddPermToSet(roleId.ToString(), permissions.Read);
                                    break;
                                case "create":
                                    permissions.Create = AddPermToSet(roleId.ToString(), permissions.Create);
                                    break;
                                case "reply":
                                    permissions.Reply = AddPermToSet(roleId.ToString(), permissions.Reply);
                                    break;
                                case "edit":
                                    permissions.Edit = AddPermToSet(roleId.ToString(), permissions.Edit);
                                    break;
                                case "delete":
                                    permissions.Delete = AddPermToSet(roleId.ToString(), permissions.Delete);
                                    break;
                                case "lock":
                                    permissions.Lock = AddPermToSet(roleId.ToString(), permissions.Lock);
                                    break;
                                case "pin":
                                    permissions.Pin = AddPermToSet(roleId.ToString(), permissions.Pin);
                                    break;
                                case "attach":
                                    permissions.Attach = AddPermToSet(roleId.ToString(), permissions.Attach);
                                    break;
                                case "subscribe":
                                    permissions.Subscribe = AddPermToSet(roleId.ToString(), permissions.Subscribe);
                                    break;
                                case "moderate":
                                    permissions.Moderate = AddPermToSet(roleId.ToString(), permissions.Moderate);
                                    break;
                                case "move":
                                    permissions.Move = AddPermToSet(roleId.ToString(), permissions.Move);
                                    break;
                                case "manageusers":
                                    permissions.ManageUsers = AddPermToSet(roleId.ToString(), permissions.ManageUsers);
                                    break;
                            }
                        }
                    }
                }

                this.Update(permissions);
            }
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo GetById(int permissionId, int moduleId)
        {
            var cachekey = this.GetCacheKey(moduleId: moduleId, id: permissionId);
            DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissions = DataCache.SettingsCacheRetrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo;
            if (permissions == null)
            {
                permissions = base.GetById(permissionId, moduleId);
                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheStore(moduleId, cachekey, permissions);
            }

            return permissions;
        }

        internal void RemoveUnused(int moduleId)
        {
            foreach (var permissionInfo in this.Get(moduleId))
            {
                var isUsed = permissionInfo.PermissionsId == SettingsBase.GetModuleSettings(moduleId).DefaultPermissionId;
                if (!isUsed)
                {
                    isUsed = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().Get(moduleId).Any(g => g.PermissionsId == permissionInfo.PermissionsId);
                    if (!isUsed)
                    {
                        isUsed = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Get(moduleId).Any(f => f.PermissionsId == permissionInfo.PermissionsId);
                        if (!isUsed)
                        {
                            var cachekey = this.GetCacheKey(moduleId: moduleId, id: permissionInfo.PermissionsId);
                            this.DeleteById(permissionInfo.PermissionsId, moduleId);
                        }
                    }
                }
            }
        }

        internal void RemoveIfUnused(int permissionId, int moduleId)
        {
            var isUsed = permissionId == SettingsBase.GetModuleSettings(moduleId).DefaultPermissionId;
            if (!isUsed)
            {
                isUsed = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().Get(moduleId).Any(g => g.PermissionsId == permissionId);
                if (!isUsed)
                {
                    isUsed = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Get(moduleId).Any(f => f.PermissionsId == permissionId);
                    if (!isUsed)
                    {
                        var cachekey = this.GetCacheKey(moduleId: moduleId, id: permissionId);
                        this.DeleteById(permissionId, moduleId);
                    }
                }
            }
        }

        internal static int GetAdministratorsRoleId(DotNetNuke.Entities.Portals.PortalSettings portalSettings)
        {
            return portalSettings.AdministratorRoleId;
        }

        internal static string GetAdministratorsRoleName(DotNetNuke.Entities.Portals.PortalSettings portalSettings)
        {
            return portalSettings.AdministratorRoleName;
        }

        internal static int GetRegisteredUsersRoleId(DotNetNuke.Entities.Portals.PortalSettings portalSettings)
        {
            return portalSettings.RegisteredRoleId;
        }

        internal static string GetRegisteredUsersRoleName(DotNetNuke.Entities.Portals.PortalSettings portalSettings)
        {
            return portalSettings.RegisteredRoleName;
        }

        internal static DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo GetDefaultPermissions(DotNetNuke.Entities.Portals.PortalSettings portalSettings, int moduleId)
        {
            string registeredUsersRoleId = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredUsersRoleId(portalSettings: portalSettings).ToString();
            var permissionInfo = GetAdminPermissions(portalSettings: portalSettings, moduleId: moduleId);
            DotNetNuke.Modules.ActiveForums.SecureActions[] requestedAccessList =
            {
                SecureActions.View,
                SecureActions.Read,
            };
            foreach (var access in requestedAccessList)
            {
                permissionInfo = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(permissionInfo, access, registeredUsersRoleId);
                permissionInfo = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(permissionInfo, access, DotNetNuke.Common.Globals.glbRoleAllUsers);
                permissionInfo = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(permissionInfo, access, DotNetNuke.Common.Globals.glbRoleUnauthUser);
            }

            requestedAccessList = new[]
            {
                SecureActions.Create,
                SecureActions.Edit,
                SecureActions.Delete,
                SecureActions.Reply,
                SecureActions.Subscribe,
                SecureActions.Attach,
                SecureActions.Mention,
                SecureActions.Tag,
            };
            foreach (var access in requestedAccessList)
            {
                permissionInfo = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(permissionInfo, access, registeredUsersRoleId);
            }

            permissionInfo.ModuleId = moduleId;
            return permissionInfo;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo CreateAdminPermissions(DotNetNuke.Entities.Portals.PortalSettings portalSettings, int moduleId)
        {
            return this.Insert(GetAdminPermissions(portalSettings: portalSettings, moduleId: moduleId));
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo CreateDefaultPermissions(DotNetNuke.Entities.Portals.PortalSettings portalSettings, int moduleId)
        {
            return this.Insert(GetDefaultPermissions(portalSettings: portalSettings, moduleId: moduleId));
        }

        internal static DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo GetAdminPermissions(DotNetNuke.Entities.Portals.PortalSettings portalSettings, int moduleId)
        {
            var adminRoleId = $"{GetAdministratorsRoleId(portalSettings: portalSettings).ToString()};{emptyPermissions}";
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
                Trust = adminRoleId,
                Subscribe = adminRoleId,
                Announce = adminRoleId,
                Prioritize = adminRoleId,
                Moderate = adminRoleId,
                Move = adminRoleId,
                Split = adminRoleId,
                ManageUsers = adminRoleId,
                ModuleId = moduleId,
            };
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
                Trust = emptyPermissions,
                Subscribe = emptyPermissions,
                Announce = emptyPermissions,
                Prioritize = emptyPermissions,
                Moderate = emptyPermissions,
                Move = emptyPermissions,
                Split = emptyPermissions,
                ManageUsers = emptyPermissions,
                ModuleId = moduleId,
            };
        }

        internal static HashSet<int> GetRoleIdsFromPermSet(string permSet)
        {
            var roleIds = new HashSet<int>();
            if (!string.IsNullOrEmpty(permSet))
            {
                var perms = permSet.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (perms.Length > 0 && !string.IsNullOrEmpty(perms[0]))
                {
                    roleIds = GetRoleIdsFromRoleString(perms[0]);
                }
            }

            return roleIds;
        }

        internal static string GetRoleIds(HashSet<int> roleIds)
        {
            return roleIds.FromHashSetToDelimitedString<int>(";");
        }

        internal static HashSet<int> GetRoleIdsFromRoleString(string roleString)
        {
            return roleString.ToHashSetFromDelimitedString<int>(";");
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        internal static HashSet<int> GetRoleIdsFromRoleIdArray(string[] roles)
        {
            return roles.Select(r => Convert.ToInt32(r)).ToHashSet();
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        internal static HashSet<int> GetRoleIdsFromRoleNameArray(int portalId, string[] roles)
        {
            return GetRoleIdsFromRoleString(GetPortalRoleIds(portalId: portalId, roles: roles));
        }

        public static bool HasAccess(string authorizedRoles, string userRoles)
        {
            return !string.IsNullOrEmpty(authorizedRoles) && !string.IsNullOrEmpty(userRoles) && HasRequiredPerm(authorizedRoleIds: GetRoleIdsFromRoleString(authorizedRoles), userRoleIds: GetRoleIdsFromRoleString(userRoles));
        }

        internal static bool HasRequiredPerm(HashSet<int> authorizedRoleIds, HashSet<int> userRoleIds)
        {
            return userRoleIds.Intersect(authorizedRoleIds).Any();
        }

        internal static System.Collections.Generic.IList<DotNetNuke.Security.Roles.RoleInfo> GetRoles(DotNetNuke.Entities.Portals.PortalSettings portalSettings)
        {
            object obj = DataCache.SettingsCacheRetrieve(moduleId: -1, cacheKey: string.Format(CacheKeys.Roles, portalSettings.PortalId));
            System.Collections.Generic.IList<DotNetNuke.Security.Roles.RoleInfo> roles;
            if (obj == null)
            {
                roles = DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: portalSettings.PortalId);

                // add pseudo-roles for anon/unauth and all users
                if (!roles.Any(r => r.RoleID == int.Parse(DotNetNuke.Common.Globals.glbRoleUnauthUser)))
                {
                    roles.Add(new DotNetNuke.Security.Roles.RoleInfo { RoleID = int.Parse(DotNetNuke.Common.Globals.glbRoleUnauthUser), RoleName = DotNetNuke.Common.Globals.glbRoleUnauthUserName });
                }

                if (!roles.Any(r => r.RoleID == int.Parse(DotNetNuke.Common.Globals.glbRoleAllUsers)))
                {
                    roles.Add(new DotNetNuke.Security.Roles.RoleInfo { RoleID = int.Parse(DotNetNuke.Common.Globals.glbRoleAllUsers), RoleName = DotNetNuke.Common.Globals.glbRoleAllUsersName });
                }
                DataCache.SettingsCacheStore(moduleId: -1, cacheKey: string.Format(CacheKeys.Roles, portalSettings.PortalId), cacheObj: roles);
            }
            else
            {
                roles = (System.Collections.Generic.IList<DotNetNuke.Security.Roles.RoleInfo>)obj;
            }

            return roles;
        }

        internal static string GetNamesForRoles(DotNetNuke.Entities.Portals.PortalSettings portalSettings, string roles)
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
                                roleName = GetRoleName(portalSettings: portalSettings, roleId: Utilities.SafeConvertInt(role));
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

        internal static string GetRoleName(DotNetNuke.Entities.Portals.PortalSettings portalSettings, int roleId)
        {
            return GetRoles(portalSettings).Where(r => r.RoleID == Utilities.SafeConvertInt(roleId)).Select(r => r.RoleName).FirstOrDefault();
        }

        internal static int GetRoleId(DotNetNuke.Entities.Portals.PortalSettings portalSettings, string roleName)
        {
            return GetRoles(portalSettings).Where(r => r.RoleName.Equals(roleName)).Select(r => r.RoleID).FirstOrDefault();
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use GetPortalRoleIds(int PortalId, delimitedString[] Roles).")]
        public static string GetRoleIds(string[] roles, int portalId) => GetPortalRoleIds(portalId, roles);


        internal static string GetPortalRoleIds(int portalId, string[] roles)
        {
            string roleIds = (string)DataCache.SettingsCacheRetrieve(-1, string.Format(CacheKeys.RoleIDs, portalId));
            if (string.IsNullOrEmpty(roleIds))
            {
                var rolesIdHashSet = new HashSet<int>
                {
                    Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
                    Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser),
                };
                foreach (var ri in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: portalId))
                {
                    foreach (var role in roles)
                    {
                        if (!string.IsNullOrEmpty(role) && role.Equals(ri.RoleName))
                        {
                            rolesIdHashSet.Add(ri.RoleID);
                        }
                    }
                }

                roleIds = GetRoleIds(rolesIdHashSet);
                DataCache.SettingsCacheStore(-1, string.Format(CacheKeys.RoleIDs, portalId), roleIds);
            }

            return roleIds;
        }

        internal static NameValueCollection GetRolesNVC(DotNetNuke.Entities.Portals.PortalSettings portalSettings, string roles)
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
                                roleName = GetRoleName(portalSettings, Utilities.SafeConvertInt(role));
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

        internal static HashSet<int> GetUsersRoleIds(DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Entities.Users.UserInfo u)
        {
            var roleIds = new HashSet<int>
            {
                Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
            };

            if (u != null)
            {
                if (u.UserID < 0)
                {
                    roleIds.Add(Convert.ToInt32(Common.Globals.glbRoleUnauthUser));
                }

                if (u.IsSuperUser)
                {
                    roleIds.Add(Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser));
                    roleIds.Add(portalSettings.AdministratorRoleId);
                }

                if (u.Roles != null)
                {
                    {
                        u.Roles.ForEach(roleName => roleIds.Add(GetRoleId(portalSettings: portalSettings, roleName: roleName)));
                        u.Social?.Roles?.ForEach(r => roleIds.Add(r.RoleID));
                    }
                }
            }

            return roleIds.Distinct().OrderBy(r => r).ToHashSet();
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public static bool HasPerm(string authorizedPermSet, int userId, int portalId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public static bool HasPerm(string authorizedRoles, DotNetNuke.Entities.Users.UserInfo user) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public static bool HasPerm(string authorizedPermSet, string userPermSet) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public static bool HasPerm(DotNetNuke.Entities.Portals.PortalSettings portalSettings, string authorizedPermSet, DotNetNuke.Entities.Users.UserInfo user) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static bool HasPerm(string authorizedPermSet, int portalId, int moduleId, int userId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetPermSet(int moduleId, int permissionsId, string requestedAccess) => throw new NotImplementedException();

        internal static string GetPermSetForRequestedAccess(int moduleId, int permissionsId, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess)
        {
            var permission = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(permissionsId, moduleId);
            return GetPermSetForRequestedAccess(permission, requestedAccess);
        }

        internal static string GetPermSetForRequestedAccess(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permission, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess)
        {
            if (permission == null)
            {
                return emptyPermissions;
            }

            switch (requestedAccess)
            {
                case SecureActions.Announce:
                    return permission.Announce;
                case SecureActions.Attach:
                    return permission.Attach;
                case SecureActions.Categorize:
                    return permission.Categorize;
                case SecureActions.Create:
                    return permission.Create;
                case SecureActions.Delete:
                    return permission.Delete;
                case SecureActions.Edit:
                    return permission.Edit;
                case SecureActions.Lock:
                    return permission.Lock;
                case SecureActions.Pin:
                    return permission.Pin;
                case SecureActions.Poll:
                    return permission.Poll;
                case SecureActions.Prioritize:
                    return permission.Prioritize;
                case SecureActions.Read:
                    return permission.Read;
                case SecureActions.Reply:
                    return permission.Reply;
                case SecureActions.Subscribe:
                    return permission.Subscribe;
                case SecureActions.Tag:
                    return permission.Tag;
                case SecureActions.Mention:
                    return permission.Mention;
                case SecureActions.Trust:
                    return permission.Trust;
                case SecureActions.View:
                    return permission.View;
                case SecureActions.Moderate:
                    return permission.Moderate;
                case SecureActions.Move:
                    return permission.Move;
                case SecureActions.Split:
                    return permission.Split;
                case SecureActions.ManageUsers:
                    return permission.ManageUsers;
                default:
                    return emptyPermissions;
            }
        }

        internal static HashSet<int> GetRoleIdsForRequestedAccess(int moduleId, int permissionsId, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess)
        {
            var permission = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(permissionsId, moduleId);
            return GetRoleIdsFromPermSet(GetPermSetForRequestedAccess(permission, requestedAccess));
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public string SavePermSet(int moduleId, int permissionsId, string requestedAccess, string permSet)
        {
            return this.SavePermSet(moduleId, permissionsId, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), permSet);
        }

        public string SavePermSet(int moduleId, int permissionsId, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string permSet)
        {
            var permission = this.GetById(permissionsId, moduleId);
            if (permission != null)
            {
                SetPermSetForRequestedAccess(permission, requestedAccess, permSet);
                this.Update(permission);
            }

            return GetPermSetForRequestedAccess(permission, requestedAccess);
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public static void AddObjectToPermissions(int moduleId, int permissionsId, string requestedAccess, string objectId, int objectType)
        {
            AddObjectToPermissions(moduleId, permissionsId, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), objectId);
        }

        internal static void AddObjectToPermissions(int moduleId, int permissionsId, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string objectId)
        {
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController();
            string permSet = GetPermSetForRequestedAccess(moduleId, permissionsId, requestedAccess);
            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(objectId, permSet);
            pc.SavePermSet(moduleId, permissionsId, requestedAccess, permSet);
        }

        internal static DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo AddObjectToPermSet(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string objectId)
        {
            var permSet = GetPermSetForRequestedAccess(permissionInfo, requestedAccess);
            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(objectId, permSet);
            SetPermSetForRequestedAccess(permissionInfo, requestedAccess, permSet);
            return permissionInfo;
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        internal static DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo AddObjectToPermSet(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string objectId, int objectType)
        {
            return AddObjectToPermSet(permissionInfo, requestedAccess, objectId);
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public static void RemoveObjectFromPermissions(int moduleId, int permissionsId, string requestedAccess, string objectId, int objectType)
        {
            RemoveObjectFromPermissions(moduleId, permissionsId, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), objectId);
        }

        internal static void RemoveObjectFromPermissions(int moduleId, int permissionsId, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string objectId)
        {
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController();
            string permSet = GetPermSetForRequestedAccess(moduleId, permissionsId, requestedAccess);
            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemovePermFromSet(objectId, permSet);
            pc.SavePermSet(moduleId, permissionsId, requestedAccess, permSet);
        }

        internal static string RemovePermFromSet(string objectId, string permissionSet)
        {
            return GetRoleIds(RemoveRoleIdFromRoleIds(Convert.ToInt32(objectId), GetRoleIdsFromPermSet(permissionSet)));
        }

        internal static HashSet<int> RemoveRoleIdFromRoleIds(int roleId, HashSet<int> roleIds)
        {
            roleIds.Remove(roleId);
            return roleIds;
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public static string RemovePermFromSet(string objectId, int objectType, string permissionSet)
        {
            return RemovePermFromSet(objectId, permissionSet);
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        internal static string SortPermissionSetMembers(string permissionSet)
        {
            if (string.IsNullOrEmpty(permissionSet))
            {
                return string.Empty;
            }

            var permSet = permissionSet;
            var members = permSet.ToHashSetFromDelimitedString<int>(";").OrderBy(r => r);
            permSet = string.Join(";", members);
            if (!string.IsNullOrEmpty(permSet))
            {
                permSet += ";";
            }

            return permSet;
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public static string AddPermToSet(string objectId, int objectType, string permissionSet)
        {
            return AddPermToSet(objectId, permissionSet);
        }

        internal static string AddPermToSet(string objectId, string permissionSet)
        {
            return GetRoleIds(AddRoleIdToRoleIds(Convert.ToInt32(objectId), GetRoleIdsFromPermSet(permissionSet)));
        }

        internal static HashSet<int> AddRoleIdToRoleIds(int roleId, HashSet<int> roleIds)
        {
            roleIds.Add(roleId);
            return roleIds;
        }

        internal static bool RemoveObjectFromAll(int moduleId, string objectId, int permissionsId)
        {
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController();
            var permissionInfo = pc.GetById(permissionsId);
            if (permissionInfo != null)
            {
                permissionInfo = RemoveObjectFromAll(permissionInfo, objectId);
                pc.Update(permissionInfo);
            }

            return true;
        }

        internal static DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo RemoveObjectFromAll(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo, string objectId)
        {
            var enumType = typeof(SecureActions);
            Array values = Enum.GetValues(enumType);
            for (int i = 0; i < values.Length; i++)
            {
                string requestedAccess = Convert.ToString(Enum.Parse(enumType, values.GetValue(i).ToString()));
                string permSet = GetPermSetForRequestedAccess(permissionInfo, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess));
                permSet = RemovePermFromSet(objectId, permSet);
                SetPermSetForRequestedAccess(permissionInfo, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), permSet);
            }

            return permissionInfo;
        }

        internal static HashSet<int> GetRoleIdsUsedByPermissionInfo(IPortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo)
        {
            var roleObjects = new HashSet<int>();

            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.AnnounceRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.AttachRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.CategorizeRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.CreateRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.DeleteRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.EditRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.LockRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.ManageUsersRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.ModerateRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.MentionRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.MoveRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.PinRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.PollRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.PrioritizeRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.ReadRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.ReplyRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.SplitRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.SubscribeRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.TagRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.TrustRoleIds);
            roleObjects = MergeRoleIds(portalSettings: portalSettings, existingRoleIds: roleObjects, newRoleIds: permissionInfo.ViewRoleIds);

            return roleObjects;
        }

        internal static string GetSecureObjectList(IPortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo)
        {
            return GetRoleIdsUsedByPermissionInfo(portalSettings: portalSettings, permissionInfo: permissionInfo).FromHashSetToDelimitedString<int>(";");
        }

        internal static string GetObjFromSecObj(IPortalSettings portalSettings, string permSet, string objects)
        {
            return MergeRoleIds(portalSettings, existingRoleIds: GetRoleIdsFromPermSet(objects), newRoleIds: GetRoleIdsFromPermSet(permSet)).FromHashSetToDelimitedString<int>(";");
        }

        internal static HashSet<int> MergeRoleIds(IPortalSettings portalSettings, HashSet<int> existingRoleIds, HashSet<int> newRoleIds)
        {
            if (existingRoleIds == null)
            {
                existingRoleIds = new HashSet<int>();
            }

            if (!existingRoleIds.Any())
            {
                newRoleIds.Add(portalSettings.AdministratorRoleId);
            }

            existingRoleIds.UnionWith(newRoleIds);
            return existingRoleIds;
        }

        internal static void SetPermSetForRequestedAccess(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permission, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string permSet)
        {
            if (permission != null)
            {
                switch (requestedAccess)
                {
                    case SecureActions.Announce:
                        permission.Announce = permSet;
                        break;
                    case SecureActions.Attach:
                        permission.Attach = permSet;
                        break;
                    case SecureActions.Categorize:
                        permission.Categorize = permSet;
                        break;
                    case SecureActions.Create:
                        permission.Create = permSet;
                        break;
                    case SecureActions.Delete:
                        permission.Delete = permSet;
                        break;
                    case SecureActions.Edit:
                        permission.Edit = permSet;
                        break;
                    case SecureActions.Lock:
                        permission.Lock = permSet;
                        break;
                    case SecureActions.Pin:
                        permission.Pin = permSet;
                        break;
                    case SecureActions.Poll:
                        permission.Poll = permSet;
                        break;
                    case SecureActions.Prioritize:
                        permission.Prioritize = permSet;
                        break;
                    case SecureActions.Read:
                        permission.Read = permSet;
                        break;
                    case SecureActions.Reply:
                        permission.Reply = permSet;
                        break;
                    case SecureActions.Subscribe:
                        permission.Subscribe = permSet;
                        break;
                    case SecureActions.Tag:
                        permission.Tag = permSet;
                        break;
                    case SecureActions.Trust:
                        permission.Trust = permSet;
                        break;
                    case SecureActions.View:
                        permission.View = permSet;
                        break;
                    case SecureActions.Mention:
                        permission.Mention = permSet;
                        break;
                    case SecureActions.Moderate:
                        permission.Moderate = permSet;
                        break;
                    case SecureActions.Move:
                        permission.Move = permSet;
                        break;
                    case SecureActions.Split:
                        permission.Split = permSet;
                        break;
                    case SecureActions.ManageUsers:
                        permission.ManageUsers = permSet;
                        break;
                    default:
                        break;
                }
            }
        }

        public static string WhichRolesCanViewForum(int moduleId, int forumId, string userRoles)
        {
            string cacheKey = string.Format(CacheKeys.ViewRolesForForum, moduleId, forumId);
            string sRoles = (string)DataCache.SettingsCacheRetrieve(moduleId, cacheKey);

            if (string.IsNullOrEmpty(sRoles))
            {
                var forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId, moduleId);
                var permission = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId, moduleId).PermissionsId, moduleId);

                foreach (string role in userRoles.Split(";".ToCharArray()))
                {
                    if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(permission.ViewRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromRoleString(userRoles)))
                    {
                        sRoles += role + ":";
                    }
                }

                DataCache.SettingsCacheStore(moduleId, cacheKey, sRoles);
            }

            return sRoles;
        }

        public static string CheckForumIdsForViewForRSS(int moduleId, string forumIds, HashSet<int> userRoleIds)
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
                        DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(Convert.ToInt32(forumId), moduleId);
                        if (forum.FeatureSettings.AllowRSS && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(forum.Security?.ViewRoleIds, userRoleIds))
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
