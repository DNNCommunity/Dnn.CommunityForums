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
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Web.UI.WebControls;
    using System.Xml;

    using DotNetNuke.Collections;
    using DotNetNuke.Data;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Extensions;

    internal class ForumController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>
    {
        internal override string cacheKeyTemplate => CacheKeys.ForumInfo;

        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetById(int forumId, int moduleId)
        {
            var cachekey = this.GetCacheKey(moduleId: moduleId, id: forumId);
            var forum = LoadFromSettingsCache(moduleId, cachekey);
            if (forum == null)
            {
                if (moduleId.Equals(-1))
                {
                    forum = this.GetById(forumId);
                }
                else
                {
                    forum = base.GetById(forumId, moduleId);
                }

                if (forum != null)
                {
                    forum.LoadForumGroup();
                    forum.LoadSubForums();
                    forum.LoadProperties();
                    forum.LoadFeatureSettings();
                    forum.LoadPortalSettings();
                    forum.LoadMainSettings();
                    forum.LoadModuleInfo();
                    forum.LoadSecurity();
                    forum.LoadLastPost();
                }

                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheStore(moduleId, cachekey, forum);
            }

            return forum;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumCollection GetForums(int moduleId)
        {
            string cacheKey = string.Format(CacheKeys.ForumList, moduleId);
            DotNetNuke.Modules.ActiveForums.Entities.ForumCollection forums = DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheRetrieve(moduleId, cacheKey) as DotNetNuke.Modules.ActiveForums.Entities.ForumCollection;
            if (forums == null)
            {
                forums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
                foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum in this.Get(moduleId).OrderBy(f => f.ForumGroup?.SortOrder).ThenBy(f => f.SortOrder))
                {
                    forum.LoadForumGroup();
                    forum.LoadSubForums();
                    forum.LoadProperties();
                    forum.LoadFeatureSettings();
                    forum.LoadPortalSettings();
                    forum.LoadMainSettings();
                    forum.LoadModuleInfo();
                    forum.LoadSecurity();
                    forum.LoadLastPost();
                    forums.Add(forum);
                    forum.UpdateCache();
                }

                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheStore(moduleId, cacheKey, forums);
            }

            return forums;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumCollection GetSubForums(int forumId, int moduleId)
        {
            string cacheKey = string.Format(CacheKeys.SubForumList, moduleId, forumId);
            DotNetNuke.Modules.ActiveForums.Entities.ForumCollection forums = DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheRetrieve(moduleId, cacheKey) as DotNetNuke.Modules.ActiveForums.Entities.ForumCollection;
            if (forums == null)
            {
                forums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
                foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum in this.Find("WHERE ParentForumId = @0", forumId).OrderBy(f => f.ForumGroup?.SortOrder).ThenBy(f => f.SortOrder))
                {
                    forum.LoadForumGroup();
                    forum.LoadProperties();
                    forum.LoadFeatureSettings();
                    forum.LoadPortalSettings();
                    forum.LoadMainSettings();
                    forum.LoadModuleInfo();
                    forum.LoadSecurity();
                    forums.Add(forum);
                }

                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheStore(moduleId, cacheKey, forums);
            }

            return forums;
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use HashSet<int> GetForumIdsBySocialGroup(int moduleId, int socialGroupId)")]
        public static string GetForumIdsBySocialGroup(int portalId, int moduleId, int socialGroupId) => GetForumIdsBySocialGroup(moduleId, socialGroupId).FromHashSetToDelimitedString<int>(";");

        internal static HashSet<int> GetForumIdsBySocialGroup(int moduleId, int socialGroupId)
        {
            return socialGroupId > 0 ? new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Find("WHERE SocialGroupId = @0 AND ModuleId = @1", socialGroupId, moduleId).Select(f => f.ForumID).Distinct().ToHashSet() : new HashSet<int>();
        }

        internal static DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forums_Get(int portalId, int moduleId, int forumId, bool useCache, int topicId)
        {
            if (forumId <= 0 && topicId <= 0)
            {
                return null;
            }

            // Get the forum by topic id
            if (topicId > 0 & forumId <= 0)
            {
                forumId = Forum_GetByTopicId(moduleId, topicId);
            }

            return forumId <= 0 ? null : new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId, moduleId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use HashSet<int> GetForumsForUser(int moduleId, ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.SecureActions action)")]
        public static string GetForumsForUser(int portalId, int moduleId, ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.SecureActions action = DotNetNuke.Modules.ActiveForums.SecureActions.View, string permissionType = "CanView") => GetForumsForUser(moduleId: moduleId, forumUser: forumUser, action: action).FromHashSetToDelimitedString(";");

        internal static HashSet<int> GetForumsForUser(int moduleId, ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.SecureActions action = DotNetNuke.Modules.ActiveForums.SecureActions.View)
        {
            var forumIds = new HashSet<int>();
            DotNetNuke.Modules.ActiveForums.Entities.ForumCollection fc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(moduleId);
            foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f in fc)
            {
                if (f.Active && !f.Hidden && f.ForumGroup != null && f.ForumGroup.Active && !f.ForumGroup.Hidden)
                {
                    var roles = new HashSet<int>();
                    switch (action)
                    {
                        case DotNetNuke.Modules.ActiveForums.SecureActions.View:
                            roles = f.Security?.ViewRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Read:
                            roles = f.Security?.ReadRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Moderate:
                            roles = f.Security?.ModerateRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Edit:
                            roles = f.Security?.EditRoleIds;
                            break;
                        default:
                            roles = f.Security?.ViewRoleIds;
                            break;
                    }

                    var hasPermissions = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(roles, forumUser.UserRoleIds);

                    if (hasPermissions)
                    {
                        forumIds.Add(f.ForumID);
                    }
                }
            }

            return forumIds;
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use  GetForumsHtmlOption(int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo currentUser)")]
        public static string GetForumsHtmlOption(int moduleId, DotNetNuke.Modules.ActiveForums.User currentUser)
        {
            var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId).GetByUserId(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId, currentUser.UserId);
            return GetForumsHtmlOption(moduleId, currentUser: user, includeHiddenForums: true);
        }

        internal static string GetForumsHtmlOption(int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo currentUser, bool includeHiddenForums)
        {
            var sb = new StringBuilder();
            int index = 1;
            var forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(moduleId).Where(f => (includeHiddenForums || !f.Hidden) && (f.ForumGroup != null) && (includeHiddenForums || !f.ForumGroup.Hidden) && (currentUser.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(f.Security?.ViewRoleIds, currentUser.UserRoleIds)));
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.IterateForumsList(
                forums: forums.ToList(),
                forumUserInfo: currentUser,
                groupAction: fi =>
                {
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", "-1", fi.GroupName);
                    index += 1;
                },
                forumAction: fi =>
                {
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", fi.ForumID.ToString(), "--" + fi.ForumName);
                    index += 1;
                },
                subForumAction: fi =>
                {
                    sb.AppendFormat("<option value=\"{0}\">----{1}</option>", fi.ForumID.ToString(), fi.ForumName);
                    index += 1;
                },
                includeHiddenForums: includeHiddenForums,
                includeInactiveForums: includeHiddenForums);
            return sb.ToString();
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public XmlDocument GetForumListXML(int portalId, int moduleId) => throw new NotImplementedException();

        public int Forums_Save(int portalId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo, bool isNew, bool useGroupFeatures, bool useGroupSecurity)
        {
            var oldPermissionsId = -1;
            var copyDownGroupSettings = false;
            if (forumInfo.ForumID <= 0)
            {
                isNew = true;
            }

            var forumGroupInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetById(forumInfo.ForumGroupId, forumInfo.ModuleId);
            if (useGroupSecurity)
            {
                if (isNew)
                {
                    if (forumGroupInfo != null)
                    {
                        forumInfo.PermissionsId = forumGroupInfo.PermissionsId;
                    }
                }
                else
                {
                    if (!forumInfo.InheritSecurity)
                    {
                        oldPermissionsId = forumInfo.PermissionsId;
                        forumInfo.PermissionsId = forumGroupInfo.PermissionsId;
                    }
                }
            }
            else
            {
                if (isNew || forumInfo.InheritSecurity) /* new forum not inheriting security or existing forum switching from group security to forum security */
                {
                    forumInfo.PermissionsId = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().Insert(forumGroupInfo.Security).PermissionsId;
                }
            }

            // if not using group features and new forum or existing forum previously using inherited settings, copy down group settings as a starting point
            if (!useGroupFeatures && (isNew || forumInfo.InheritSettings))
            {
                copyDownGroupSettings = true;
            }

            forumInfo.ForumSettingsKey = useGroupFeatures ? (forumGroupInfo != null ? forumGroupInfo.GroupSettingsKey : string.Empty) : (forumInfo.ForumID > 0 ? $"F{forumInfo.ForumID}" : string.Empty);

            // TODO: When this method is updated to use DAL2 for update, uncomment Cacheable attribute on ForumInfo
            var forumId = Convert.ToInt32(DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Forum_Save(portalId, forumInfo.ForumID, forumInfo.ModuleId, forumInfo.ForumGroupId, forumInfo.ParentForumId, forumInfo.ForumName, forumInfo.ForumDesc, forumInfo.SortOrder, forumInfo.Active, forumInfo.Hidden, forumInfo.ForumSettingsKey, forumInfo.PermissionsId, forumInfo.PrefixURL, forumInfo.SocialGroupId, forumInfo.HasProperties));
            forumInfo = this.GetById(forumId, forumInfo.ModuleId);
            if (!useGroupFeatures && string.IsNullOrEmpty(forumInfo.ForumSettingsKey))
            {
                forumInfo.ForumSettingsKey = $"F{forumId}";
                this.Update(forumInfo);
            }

            // if new forum and not using group features, copy group features to forum features as starting point
            if (copyDownGroupSettings)
            {
                forumInfo.FeatureSettings = forumInfo.ForumGroup.FeatureSettings;
                FeatureSettings.Save(forumInfo.ModuleId, forumInfo.ForumSettingsKey, forumInfo.FeatureSettings);
                this.Update(forumInfo);
            }

            if (oldPermissionsId != -1)
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().RemoveIfUnused(permissionId: oldPermissionsId, moduleId: forumInfo.ModuleId);
            }

            // if now inheriting group settings, remove any previously-defined forum settings
            if (forumInfo.InheritSettings)
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.SettingsController().DeleteForModuleIdSettingsKey(forumInfo.ModuleId, $"F{forumInfo.ForumID}");
            }

            // Clear the caches
            ClearSettingsCache(forumInfo.ModuleId);
            return forumId;
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public void Forums_Delete(int portalId, int forumId, int moduleId) => Forums_Delete(forumId: forumId, moduleId: moduleId);

        internal void Forums_Delete(int forumId, int moduleId)
        {
            var parentForumId = this.GetById(forumId, moduleId).ParentForumId;
            new DotNetNuke.Modules.ActiveForums.Controllers.ForumTopicController(moduleId).DeleteForForum(forumId);
            new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().DeleteForForum(moduleId, forumId);
            this.DeleteById(forumId);
            DataContext.Instance().Execute(System.Data.CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}activeforums_Forums_RepairSort", forumId, parentForumId);
            ClearSettingsCache(moduleId);
        }

        internal static void IterateForumsList(
            System.Collections.Generic.List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> forums,
            DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUserInfo,
            Action<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> groupAction,
            Action<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> forumAction,
            Action<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> subForumAction,
            bool includeHiddenForums,
            bool includeInactiveForums)
        {
            string tmpSettingsKey = string.Empty;
            foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi in forums.Where(f => (includeHiddenForums || !f.Hidden) && f.ForumGroup != null && (includeHiddenForums || !f.ForumGroup.Hidden) && (includeInactiveForums || f.Active) && f.ForumGroup != null && (includeInactiveForums || f.ForumGroup.Active) && (forumUserInfo.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(f.Security?.ViewRoleIds, forumUserInfo.UserRoleIds))))
            {
                string settingsKey = $"{fi.GroupName}{fi.ForumGroupId}";
                if (tmpSettingsKey != settingsKey)
                {
                    groupAction(fi);
                    tmpSettingsKey = settingsKey;
                }

                if (fi.ParentForumId == 0)
                {
                    forumAction(fi);
                    foreach (var subforum in forums.Where(f => f.ParentForumId == fi.ForumID && (!f.Hidden && f.ForumGroup != null && !f.ForumGroup.Hidden && (forumUserInfo.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(f.Security?.ViewRoleIds, forumUserInfo.UserRoleIds)))))
                    {
                        subForumAction(subforum);
                    }
                }
            }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.CreateSocialGroupForum (renamed method to be more descriptive).")]
        public static int CreateGroupForum(int portalId, int moduleId, int socialGroupId, int forumGroupId, string forumName, string forumDescription, bool isPrivate, string forumConfig)
        {
            return CreateSocialGroupForum(portalId, moduleId, socialGroupId, forumGroupId, forumName, forumDescription, isPrivate, forumConfig);
        }

        public static int CreateSocialGroupForum(int portalId, int moduleId, int socialGroupId, int forumGroupId, string forumName, string forumDescription, bool isPrivate, string forumConfig)
        {
            var forumId = -1;

            try
            {
                PermissionController pc = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController();
                DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo gi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetById(forumGroupId, moduleId);
                var socialGroup = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(portalId: portalId, roleId: socialGroupId);
                var groupAdmin = string.Concat(socialGroupId.ToString(), ":0");
                var groupMember = socialGroupId.ToString();
                var portalSettings = Utilities.GetPortalSettings(portalId);
                int permissionsId = pc.CreateAdminPermissions(portalSettings, moduleId).PermissionsId;

                DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo(portalSettings)
                {
                    ForumDesc = forumDescription,
                    Active = true,
                    ForumGroupId = forumGroupId,
                    ForumID = -1,
                    ForumName = forumName,
                    Hidden = isPrivate,
                    ModuleId = gi.ModuleId,
                    ParentForumId = 0,
                    PortalId = portalId,
                    PermissionsId = gi.PermissionsId,
                    SortOrder = 0,
                    SocialGroupId = socialGroupId,
                };

                var fc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController();
                forumId = fc.Forums_Save(portalId, fi, true, true, true);
                fi = fc.GetById(forumId, gi.ModuleId);
                fi.PermissionsId = permissionsId;
                new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Forums_Save(portalId, fi, false, false, false);

                var xDoc = new XmlDocument();
                xDoc.LoadXml(forumConfig);

                var xRoot = xDoc.DocumentElement;
                if (xRoot != null)
                {
                    var xSecList = xRoot.SelectSingleNode("//security[@type='groupadmin']");
                    string requestedAccess;
                    if (xSecList != null)
                    {
                        foreach (XmlNode n in xSecList.ChildNodes)
                        {
                            requestedAccess = n.Name;
                            if (n.Attributes == null || n.Attributes["value"].Value != "true")
                            {
                                continue;
                            }

                            DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId: permissionsId, requestedAccess: (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), objectId: groupAdmin);
                        }
                    }

                    xSecList = xRoot.SelectSingleNode("//security[@type='groupmember']");
                    if (xSecList != null)
                    {
                        foreach (XmlNode n in xSecList.ChildNodes)
                        {
                            requestedAccess = n.Name;

                            if (n.Attributes == null || n.Attributes["value"].Value != "true")
                            {
                                continue;
                            }

                            DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, requestedAccess: (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), objectId: groupMember);
                        }
                    }

                    if (socialGroup.IsPublic)
                    {
                        xSecList = xRoot.SelectSingleNode("//security[@type='registereduser']");
                        if (xSecList != null)
                        {
                            foreach (XmlNode n in xSecList.ChildNodes)
                            {
                                requestedAccess = n.Name;

                                if (n.Attributes == null || n.Attributes["value"].Value != "true")
                                {
                                    continue;
                                }

                                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, requestedAccess: (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), objectId: DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredUsersRoleId(portalSettings).ToString());
                            }
                        }

                        xSecList = xRoot.SelectSingleNode("//security[@type='anon']");
                        if (xSecList != null)
                        {
                            foreach (XmlNode n in xSecList.ChildNodes)
                            {
                                requestedAccess = n.Name;

                                if (n.Attributes == null || n.Attributes["value"].Value != "true")
                                {
                                    continue;
                                }

                                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, requestedAccess: (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), requestedAccess), objectId: DotNetNuke.Common.Globals.glbRoleAllUsers);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheClear(moduleId, string.Format(CacheKeys.ForumListXml, moduleId));

            return forumId;
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static int Forum_GetByTopicId(int topicId)
        {
            return Forum_GetByTopicId(-1, topicId);
        }

        public static int Forum_GetByTopicId(int moduleId, int topicId)
        {
            try
            {
                var forumTopic = new DotNetNuke.Modules.ActiveForums.Controllers.ForumTopicController(moduleId).GetForumForTopic(topicId);
                if (forumTopic != null)
                {
                    return forumTopic.ForumId;
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            return DotNetNuke.Common.Utilities.Null.NullInteger;
        }

        public static DateTime Forum_GetLastReadTopicByUser(int forumId, int userId)
        {
            try
            {
                return DataContext.Instance().ExecuteQuery<DateTime>(System.Data.CommandType.Text, "SELECT LastAccessDate FROM {databaseOwner}{objectQualifier}activeforums_Forums_Tracking WHERE ForumId = @0 AND UserId = @1", forumId, userId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return DateTime.MinValue;
            }
        }

        internal static bool RecalculateTopicPointers(int forumId)
        {
            try
            {
                DataContext.Instance().Execute(System.Data.CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}activeforums_SaveTopicNextPrev", forumId);
                return true;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }
        }

        internal static bool UpdateForumLastUpdates(int forumId)
        {
            try
            {
                DataContext.Instance().Execute(System.Data.CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}activeforums_Forums_LastUpdates", forumId);
                return true;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }
        }

        internal static void UpdatePermissionsForSocialGroupForums(int moduleId)
        {
            try
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Get().Where(f => f.SocialGroupId != 0).ForEach(forum =>
                {
                    new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().UpdateSecurityForSocialGroupForum(forum);
                });
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        internal static string GetLastPostSubjectLinkTag(int length, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi, int tabId)
        {
            string subject = Utilities.StripHTMLTag(System.Net.WebUtility.HtmlDecode(fi.LastPostSubject)).Replace("[", "&#91").Replace("]", "&#93");
            if (subject.Length > length & length > 0)
            {
                subject = subject.Substring(0, length) + "...";
            }

            string link = fi.LastPost.GetLink();

            return $"<a href=\"{link}\">{System.Net.WebUtility.HtmlEncode(subject)}</a>";
        }
    }
}
