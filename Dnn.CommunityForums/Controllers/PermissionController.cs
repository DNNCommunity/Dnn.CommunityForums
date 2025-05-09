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
    using System.Xml;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;

    internal class PermissionController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
    {
        private const string emptyPermissions = ";|||";

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
                                    permissions.View = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.View);
                                    break;
                                case "read":
                                    permissions.Read = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.Read);
                                    break;
                                case "create":
                                    permissions.Create = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.Create);
                                    break;
                                case "reply":
                                    permissions.Reply = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.Reply);
                                    break;
                                case "edit":
                                    permissions.Edit = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.Edit);
                                    break;
                                case "delete":
                                    permissions.Delete = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.Delete);
                                    break;
                                case "lock":
                                    permissions.Lock = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.Lock);
                                    break;
                                case "pin":
                                    permissions.Pin = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.Pin);
                                    break;
                                case "attach":
                                    permissions.Attach = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.Attach);
                                    break;
                                case "subscribe":
                                    permissions.Subscribe = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.Subscribe);
                                    break;
                                case "moderate":
                                    permissions.Moderate = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.Moderate);
                                    break;
                                case "move":
                                    permissions.Move = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.Move);
                                    break;
                                case "ban":
                                    permissions.Ban = AddPermToSet(roleId.ToString(), DotNetNuke.Modules.ActiveForums.ObjectType.RoleId, permissions.Ban);
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
                permissionInfo = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(permissionInfo, access, registeredUsersRoleId, DotNetNuke.Modules.ActiveForums.ObjectType.RoleId);
                permissionInfo = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(permissionInfo, access, DotNetNuke.Common.Globals.glbRoleAllUsers, DotNetNuke.Modules.ActiveForums.ObjectType.RoleId);
                permissionInfo = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(permissionInfo, access, DotNetNuke.Common.Globals.glbRoleUnauthUser, DotNetNuke.Modules.ActiveForums.ObjectType.RoleId);
            }

            requestedAccessList = new[]
            {
                SecureActions.Create,
                SecureActions.Edit,
                SecureActions.Delete,
                SecureActions.Reply,
                SecureActions.Subscribe,
                SecureActions.Attach,
            };
            foreach (var access in requestedAccessList)
            {
                permissionInfo = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(permissionInfo, access, registeredUsersRoleId, DotNetNuke.Modules.ActiveForums.ObjectType.RoleId);
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

        internal static HashSet<int> GetRoleIdsFromRoleString(string roleString)
        {
            var roleIds = new HashSet<int>();
            if (!string.IsNullOrEmpty(roleString))
            {
                roleIds.UnionWith(roleString.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(r => Convert.ToInt32(r)));
            }

            return roleIds;
        }

        internal static HashSet<int> GetRoleIdsFromRoleIdArray(string[] roles)
        {
            return roles.Select(r => Convert.ToInt32(r)).ToHashSet();
        }

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
                                roleName = GetRoleName(portalSettings: portalSettings, role: role);
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

        internal static string GetRoleName(DotNetNuke.Entities.Portals.PortalSettings portalSettings, string role)
        {
            return GetRoles(portalSettings).Where(r => r.RoleID == Utilities.SafeConvertInt(role)).Select(r => r.RoleName).FirstOrDefault();
        }

        internal static int GetRoleId(DotNetNuke.Entities.Portals.PortalSettings portalSettings, string roleName)
        {
            return GetRoles(portalSettings).Where(r => r.RoleName.Equals(roleName)).Select(r => r.RoleID).FirstOrDefault();
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use GetPortalRoleIds(int PortalId, string[] Roles).")]
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

                roleIds = string.Join(";", rolesIdHashSet.Distinct().OrderBy(r => r));
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
                                roleName = GetRoleName(portalSettings, role);
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
                case SecureActions.Ban:
                    return permission.Ban;
                default:
                    return emptyPermissions;
            }
        }

        internal static HashSet<int> GetRoleIdsForRequestedAccess(int moduleId, int permissionsId, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess)
        {
            var permission = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(permissionsId, moduleId);
            return GetRoleIdsFromPermSet(GetPermSetForRequestedAccess(permission, requestedAccess));
        }

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
            AddObjectToPermissions(moduleId, permissionsId, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), objectId, (DotNetNuke.Modules.ActiveForums.ObjectType)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.ObjectType), objectType.ToString()));
        }

        internal static void AddObjectToPermissions(int moduleId, int permissionsId, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string objectId, DotNetNuke.Modules.ActiveForums.ObjectType objectType)
        {
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController();
            string permSet = GetPermSetForRequestedAccess(moduleId, permissionsId, requestedAccess);
            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(objectId, objectType, permSet);
            pc.SavePermSet(moduleId, permissionsId, requestedAccess, permSet);
        }

        internal static DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo AddObjectToPermSet(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string objectId, DotNetNuke.Modules.ActiveForums.ObjectType objectType)
        {
            var permSet = GetPermSetForRequestedAccess(permissionInfo, requestedAccess);
            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(objectId, (DotNetNuke.Modules.ActiveForums.ObjectType)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.ObjectType), objectType.ToString()), permSet);
            SetPermSetForRequestedAccess(permissionInfo, requestedAccess, permSet);
            return permissionInfo;
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        internal static DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo AddObjectToPermSet(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string objectId, int objectType)
        {
            return AddObjectToPermSet(permissionInfo, requestedAccess, objectId, (DotNetNuke.Modules.ActiveForums.ObjectType)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.ObjectType), objectType.ToString()));
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public static void RemoveObjectFromPermissions(int moduleId, int permissionsId, string requestedAccess, string objectId, int objectType)
        {
            RemoveObjectFromPermissions(moduleId, permissionsId, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), objectId, (DotNetNuke.Modules.ActiveForums.ObjectType)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.ObjectType), objectType.ToString()));
        }

        internal static void RemoveObjectFromPermissions(int moduleId, int permissionsId, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string objectId,  DotNetNuke.Modules.ActiveForums.ObjectType objectType)
        {
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController();
            string permSet = GetPermSetForRequestedAccess(moduleId, permissionsId, requestedAccess);
            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemovePermFromSet(objectId, objectType, permSet);
            pc.SavePermSet(moduleId, permissionsId, requestedAccess, permSet);
        }

        internal static string RemovePermFromSet(string objectId, DotNetNuke.Modules.ActiveForums.ObjectType objectType, string permissionSet)
        {
            if (string.IsNullOrEmpty(permissionSet))
            {
                return string.Empty;
            }

            var permSet = permissionSet.Split('|');
            var index = (int)objectType;

            if (index < 0 || index >= permSet.Length)
            {
                return permissionSet;
            }

            string[] permSection = permSet[index].Split(';');
            permSet[index] = string.Join(";", permSection.Where(s => s != objectId));

            return string.Join("|", permSet);
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public static string RemovePermFromSet(string objectId, int objectType, string permissionSet)
        {
            return RemovePermFromSet(objectId, (DotNetNuke.Modules.ActiveForums.ObjectType)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.ObjectType), objectType.ToString()), permissionSet);
        }

        public static string SortPermissionSetMembers(string permissionSet)
        {
            if (string.IsNullOrEmpty(permissionSet))
            {
                return string.Empty;
            }

            string newSet = string.Empty;

            string[] permSet = permissionSet.Split('|');
            for (int section = 0; section < 2; section++)
            {
                var members = permSet[section].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                Array.Sort(members);
                permSet[section] = string.Join(";", members);
                if (!string.IsNullOrEmpty(permSet[section]))
                {
                    permSet[section] += ";";
                }

                newSet += string.Concat(permSet[section], "|");
            }

            return newSet;
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public static string AddPermToSet(string objectId, int objectType, string permissionSet)
        {
            return AddPermToSet(objectId, (DotNetNuke.Modules.ActiveForums.ObjectType)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.ObjectType), objectType.ToString()), permissionSet);
        }

        internal static string AddPermToSet(string objectId, DotNetNuke.Modules.ActiveForums.ObjectType objectType, string permissionSet)
        {
            string newSet = RemovePermFromSet(objectId, objectType, permissionSet);
            string[] permSet = newSet.Split('|');
            permSet[(int)objectType] += string.Concat(objectId, ";");
            newSet = string.Concat(permSet[0] + "|" + (permSet.Length > 1 ? permSet[1] : string.Empty) + "|" + (permSet.Length > 2 ? permSet[2] : string.Empty), "|");
            return newSet;
        }

        internal static bool RemoveObjectFromAll(int moduleId, string objectId, DotNetNuke.Modules.ActiveForums.ObjectType objectType, int permissionsId)
        {
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController();
            var permissionInfo = pc.GetById(permissionsId);
            if (permissionInfo != null)
            {
                permissionInfo = RemoveObjectFromAll(permissionInfo, objectId, objectType);
                pc.Update(permissionInfo);
            }

            return true;
        }

        internal static DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo RemoveObjectFromAll(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo, string objectId, DotNetNuke.Modules.ActiveForums.ObjectType objectType)
        {
            var enumType = typeof(SecureActions);
            Array values = Enum.GetValues(enumType);
            for (int i = 0; i < values.Length; i++)
            {
                string requestedAccess = Convert.ToString(Enum.Parse(enumType, values.GetValue(i).ToString()));
                string permSet = GetPermSetForRequestedAccess(permissionInfo, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess));
                permSet = RemovePermFromSet(objectId, objectType, permSet);
                SetPermSetForRequestedAccess(permissionInfo, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), permSet);
            }

            return permissionInfo;
        }

        internal static string GetSecureObjectList(IPortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo, DotNetNuke.Modules.ActiveForums.ObjectType objectType)
        {
            string roleObjects = string.Empty;

            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Announce, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Attach, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Ban, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Categorize, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Create, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Delete, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Edit, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Lock, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Moderate, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Move, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Pin, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Poll, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Prioritize, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Read, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Reply, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Split, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Subscribe, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Tag, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.Trust, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, permissionInfo.View, objectType, roleObjects);

            return roleObjects;
        }

        private static string GetObjFromSecObj(IPortalSettings portalSettings, string permSet, DotNetNuke.Modules.ActiveForums.ObjectType objectType, string objects)
        {
            if (string.IsNullOrEmpty(permSet))
            {
                permSet = portalSettings.AdministratorRoleId + ";|||";
            }

            var index = (int)objectType;
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
                    case SecureActions.Moderate:
                        permission.Moderate = permSet;
                        break;
                    case SecureActions.Move:
                        permission.Move = permSet;
                        break;
                    case SecureActions.Split:
                        permission.Split = permSet;
                        break;
                    case SecureActions.Ban:
                        permission.Ban = permSet;
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

        public static string CheckForumIdsForViewForRSS(int moduleId, string forumIds, string userPermSet)
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
                        if (forum.FeatureSettings.AllowRSS && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(forum.Security?.ViewRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(userPermSet)))
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
