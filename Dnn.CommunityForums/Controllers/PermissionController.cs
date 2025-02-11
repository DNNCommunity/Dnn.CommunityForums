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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Xml;

    using DotNetNuke.Abstractions.Portals;

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
            Hashtable htSettings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: forum.ModuleId, tabId: forum.TabId, ignoreCache: false).TabModuleSettings;
            if (htSettings == null || htSettings.Count == 0)
            {
                htSettings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: forum.ModuleId, tabId: forum.TabId, ignoreCache: false).ModuleSettings;
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
                var roleIdAdmin = GetAdministratorsRoleId(forum.PortalId);
                var roleIdRegUsers = GetRegisteredRoleId(forum.PortalId);
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
                                    permissions.View = AddPermToSet(roleId.ToString(), 0, permissions.View);
                                    break;
                                case "read":
                                    permissions.Read = AddPermToSet(roleId.ToString(), 0, permissions.Read);
                                    break;
                                case "create":
                                    permissions.Create = AddPermToSet(roleId.ToString(), 0, permissions.Create);
                                    break;
                                case "reply":
                                    permissions.Reply = AddPermToSet(roleId.ToString(), 0, permissions.Reply);
                                    break;
                                case "edit":
                                    permissions.Edit = AddPermToSet(roleId.ToString(), 0, permissions.Edit);
                                    break;
                                case "delete":
                                    permissions.Delete = AddPermToSet(roleId.ToString(), 0, permissions.Delete);
                                    break;
                                case "lock":
                                    permissions.Lock = AddPermToSet(roleId.ToString(), 0, permissions.Lock);
                                    break;
                                case "pin":
                                    permissions.Pin = AddPermToSet(roleId.ToString(), 0, permissions.Pin);
                                    break;
                                case "attach":
                                    permissions.Attach = AddPermToSet(roleId.ToString(), 0, permissions.Attach);
                                    break;
                                case "subscribe":
                                    permissions.Subscribe = AddPermToSet(roleId.ToString(), 0, permissions.Subscribe);
                                    break;
                                case "moderate":
                                    permissions.Moderate = AddPermToSet(roleId.ToString(), 0, permissions.Moderate);
                                    break;
                                case "move":
                                    permissions.Move = AddPermToSet(roleId.ToString(), 0, permissions.Move);
                                    break;
                                case "ban":
                                    permissions.Ban = AddPermToSet(roleId.ToString(), 0, permissions.Ban);
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

        internal static DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo GetDefaultPermissions(int portalId, int moduleId)
        {
            string registeredUsersRoleId = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredRoleId(portalId).ToString();
            var permissionInfo = GetAdminPermissions(adminRole: GetAdministratorsRoleId(portalId: portalId).ToString(), moduleId: moduleId);
            DotNetNuke.Modules.ActiveForums.SecureActions[] requestedAccessList =
            {
                SecureActions.View,
                SecureActions.Read,
            };
            foreach (var access in requestedAccessList)
            {
                permissionInfo = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(permissionInfo, access, registeredUsersRoleId, 0);
                permissionInfo = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(permissionInfo, access, DotNetNuke.Common.Globals.glbRoleAllUsers, 0);
                permissionInfo = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(permissionInfo, access, DotNetNuke.Common.Globals.glbRoleUnauthUser, 0);
            }

            requestedAccessList = new[]
            {
                SecureActions.Create,
                SecureActions.Edit,
                SecureActions.Reply,
                SecureActions.Delete,
                SecureActions.Reply,
                SecureActions.Subscribe,
                SecureActions.Attach,
            };
            foreach (var access in requestedAccessList)
            {
                permissionInfo = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(permissionInfo, access, registeredUsersRoleId, 0);
            }

            return permissionInfo;
        }

        internal static void CreateDefaultSets(int portalId, int moduleId, int permissionsId)
        {
            string registeredUsersRoleId = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredRoleId(portalId).ToString();
            DotNetNuke.Modules.ActiveForums.SecureActions[] requestedAccessList =
            {
                SecureActions.View,
                SecureActions.Read,
            };
            foreach (var access in requestedAccessList)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, access, registeredUsersRoleId, 0);
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, access, DotNetNuke.Common.Globals.glbRoleAllUsers, 0);
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, access, DotNetNuke.Common.Globals.glbRoleUnauthUser, 0);
            }

            requestedAccessList = new[]
            {
                SecureActions.Create,
                SecureActions.Edit,
                SecureActions.Reply,
                SecureActions.Delete,
                SecureActions.Reply,
                SecureActions.Subscribe,
                SecureActions.Attach,
            };
            foreach (var access in requestedAccessList)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, access, registeredUsersRoleId, 0);
            }
        }

        internal static DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo GetAdminPermissions(string adminRole, int moduleId)
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
            return permissionInfo;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo CreateAdminPermissions(string adminRole, int moduleId)
        {
            return this.Insert(GetAdminPermissions(adminRole: adminRole, moduleId: moduleId));
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
        public static string GetRoleIds(string[] roles, int portalId) => GetPortalRoleIds(portalId, roles);

        internal static string GetUsersRoleIds(int PortalId, DotNetNuke.Entities.Users.UserInfo u)
        {
            var roles = new System.Collections.Generic.List<int>();
            if (u != null && u.Roles != null)
            {
                {
                    foreach (var r in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: PortalId))
                    {
                        foreach (string role in u?.Roles)
                        {
                            if (!string.IsNullOrEmpty(role))
                            {
                                if (r.RoleName == role)
                                {
                                    if (!roles.Contains(r.RoleID))
                                    {
                                        roles.Add(r.RoleID);
                                    }

                                    break;
                                }
                            }
                        }
                    }

                    if (u != null && u.Social.Roles != null && u.Social.Roles != null)
                    {
                        foreach (DotNetNuke.Entities.Users.UserRoleInfo r in u?.Social?.Roles)
                        {
                            if (!roles.Contains(r.RoleID))
                            {
                                roles.Add(r.RoleID);
                            }
                        }
                    }
                }
            }

            return string.Join(";", roles.ToArray());
        }

        internal static string GetPortalRoleIds(int portalId, string[] roles)
        {
            string roleIds = (string)DataCache.SettingsCacheRetrieve(-1, string.Format(CacheKeys.RoleIDs, portalId));
            if (string.IsNullOrEmpty(roleIds))
            {
                foreach (var ri in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: portalId))
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

        public static bool HasPerm(string authorizedRoles, DotNetNuke.Entities.Users.UserInfo user)
        {
            return HasPerm(authorizedRoles, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(user.PortalID, user));
        }

        internal static string GetUsersRoleIds(int PortalId, DotNetNuke.Entities.Users.UserInfo u)
        {
            var roles = new System.Collections.Generic.List<int>();
            if (u != null && u.Roles != null)
            {
                {
                    foreach (var r in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: PortalId))
                    {
                        foreach (string role in u?.Roles)
                        {
                            if (!string.IsNullOrEmpty(role))
                            {
                                if (r.RoleName == role)
                                {
                                    if (!roles.Contains(r.RoleID))
                                    {
                                        roles.Add(r.RoleID);
                                    }

                                    break;
                                }
                            }
                        }
                    }

                    if (u != null && u.Social.Roles != null && u.Social.Roles != null)
                    {
                        foreach (DotNetNuke.Entities.Users.UserRoleInfo r in u?.Social?.Roles)
                        {
                            if (!roles.Contains(r.RoleID))
                            {
                                roles.Add(r.RoleID);
                            }
                        }
                    }
                }
            }

            return string.Join(";", roles.ToArray());
        }

        public static bool HasPerm(string authorizedRoles, DotNetNuke.Entities.Users.UserInfo user)
        {
            return HasPerm(authorizedRoles, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(user.PortalID, user));
        }

        public static bool HasPerm(string authorizedRoles, int portalId, int moduleId, int userId)
        {
            return HasPerm(authorizedRoles, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId).GetByUserId(portalId, userId).UserRoles);
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public static bool HasPerm(string authorizedRoles, int userId, int portalId) => throw new NotImplementedException();

        public string GetPermSet(int moduleId, int permissionsId, string requestedAccess)
        {
            return this.GetPermSet(moduleId, permissionsId, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess));
        }

        internal string GetPermSet(int moduleId, int permissionsId, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess)
        {
            var permission = this.GetById(permissionsId, moduleId);
            return GetRolesForRequestedAccess(permission, requestedAccess);
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
                //permSet = SortPermissionSetMembers(permSet);
                SetRolesForRequestedAccess(permission, requestedAccess, permSet);
                this.Update(permission);
            }

            return this.GetPermSet(moduleId, permissionsId, requestedAccess);
        }

        public static void AddObjectToPermissions(int moduleId, int permissionsId, string requestedAccess, string objectId, int objectType)
        {
            AddObjectToPermissions(moduleId, permissionsId, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), objectId, objectType);
        }

        internal static void AddObjectToPermissions(int moduleId, int permissionsId, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string objectId, int objectType)
        {
            var pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController();
            string permSet = pc.GetPermSet(moduleId, permissionsId, requestedAccess);
            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(objectId, objectType, permSet);
            pc.SavePermSet(moduleId, permissionsId, requestedAccess, permSet);
        }

        internal static DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo AddObjectToPermSet(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string objectId, int objectType)
        {
            var permSet = GetRolesForRequestedAccess(permissionInfo, requestedAccess);
            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(objectId, objectType, permSet);
            SetRolesForRequestedAccess(permissionInfo, requestedAccess, permSet);
            return permissionInfo;
        }

        public static void RemoveObjectFromPermissions(int moduleId, int permissionsId, string requestedAccess, string objectId, int objectType)
        {
            RemoveObjectFromPermissions(moduleId, permissionsId, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), objectId, objectType);
        }

        internal static void RemoveObjectFromPermissions(int moduleId, int permissionsId, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string objectId, int objectType)
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

        public static string AddPermToSet(string objectId, int objectType, string permissionSet)
        {
            string newSet = RemovePermFromSet(objectId, objectType, permissionSet);
            string[] permSet = newSet.Split('|');
            permSet[objectType] += string.Concat(objectId, ";");
            newSet = string.Concat(permSet[0] + "|" + (permSet.Length > 1 ? permSet[1] : string.Empty) + "|" + (permSet.Length > 2 ? permSet[2] : string.Empty), "|");
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
            roleObjects = GetObjFromSecObj(portalSettings, s.Ban, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Categorize, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Create, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Delete, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Edit, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Lock, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Moderate, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Move, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Pin, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Poll, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Prioritize, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Read, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Reply, objectType, roleObjects);
            roleObjects = GetObjFromSecObj(portalSettings, s.Split, objectType, roleObjects);
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

        internal static string GetRolesForRequestedAccess(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permission, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess)
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

        internal static void SetRolesForRequestedAccess(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permission, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string permSet)
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
                        DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(Convert.ToInt32(forumId), moduleId);
                        if (forum.FeatureSettings.AllowRSS && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum.Security?.View, userRoles))
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
