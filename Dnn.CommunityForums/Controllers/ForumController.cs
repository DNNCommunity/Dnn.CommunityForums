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
    using DotNetNuke.Framework;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Modules.ActiveForums.Helpers;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;
    using DotNetNuke.Modules.ActiveForums.Services.Controllers;

    internal class ForumController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo, IForumController, ForumController>, IForumController
    {
        protected override Func<IForumController> GetFactory()
        {
            return () => new ForumController();
        }

        public virtual DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetById(int moduleId, int forumId)
        {
            var cachekey = string.Format(CacheKeys.ForumInfo, moduleId, forumId);
            var forum = DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ForumInfo;
            if (forum == null)
            {
                if (moduleId.Equals(DotNetNuke.Common.Utilities.Null.NullInteger))
                {
                    forum = this._repositoryControllerBase.GetById(forumId);
                }
                else
                {
                    forum = this._repositoryControllerBase.GetById(id: forumId, scopeValue: moduleId);
                    if (forum == null)
                    {
                        forum = this._repositoryControllerBase.GetById(id: forumId);
                    }
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

                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Store(moduleId, cachekey, forum);
            }

            return forum;
        }

        public virtual DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetByUrlPrefix(int moduleId, string forumPrefix)
        {
            string cachekey = string.Format(CacheKeys.ForumByUrlPrefix, moduleId, forumPrefix);
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ForumInfo;
            if (forumInfo == null)
            {
                // this accommodates duplicates which may exist since currently no uniqueness applied in database
                var forumId = this._repositoryControllerBase.Find("WHERE ModuleId = @0 AND PrefixURL = @1", moduleId, forumPrefix.Trim()).OrderBy(t => t.ForumID).FirstOrDefault()?.ForumID;
                if (forumId.HasValue)
                {
                    forumInfo = this.GetById(moduleId, forumId.Value);
                }

                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Store(moduleId, cachekey, forumInfo);
            }

            return forumInfo;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumCollection GetForums(int moduleId)
        {
            string cacheKey = string.Format(CacheKeys.ForumList, moduleId);
            DotNetNuke.Modules.ActiveForums.Entities.ForumCollection forums = DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Retrieve(moduleId, cacheKey) as DotNetNuke.Modules.ActiveForums.Entities.ForumCollection;
            if (forums == null)
            {
                forums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
                foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum in this._repositoryControllerBase.Get(moduleId).OrderBy(f => f.ForumGroup?.SortOrder).ThenBy(f => f.SortOrder))
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

                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Store(moduleId, cacheKey, forums);
            }

            return forums;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumCollection GetSubForums(int moduleId, int forumId)
        {
            string cacheKey = string.Format(CacheKeys.SubForumList, moduleId, forumId);
            DotNetNuke.Modules.ActiveForums.Entities.ForumCollection forums = DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Retrieve(moduleId, cacheKey) as DotNetNuke.Modules.ActiveForums.Entities.ForumCollection;
            if (forums == null)
            {
                forums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
                foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum in this._repositoryControllerBase.Find("WHERE ParentForumId = @0", forumId).OrderBy(f => f.ForumGroup?.SortOrder).ThenBy(f => f.SortOrder))
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

                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Store(moduleId, cacheKey, forums);
            }

            return forums;
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

            return forumId <= 0 ? null : DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetById(moduleId, forumId);
        }

        internal static HashSet<int> GetForumsForUser(int moduleId, ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.SecureActions action = DotNetNuke.Modules.ActiveForums.SecureActions.View)
        {
            var forumIds = new HashSet<int>();
            DotNetNuke.Modules.ActiveForums.Entities.ForumCollection fc = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetForums(moduleId);
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
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Create:
                            roles = f.Security?.CreateRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Reply:
                            roles = f.Security?.ReplyRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Edit:
                            roles = f.Security?.EditRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Delete:
                            roles = f.Security?.DeleteRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Move:
                            roles = f.Security?.MoveRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Lock:
                            roles = f.Security?.LockRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Pin:
                            roles = f.Security?.PinRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Split:
                            roles = f.Security?.SplitRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Attach:
                            roles = f.Security?.AttachRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Poll:
                            roles = f.Security?.PollRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Trust:
                            roles = f.Security?.TrustRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Subscribe:
                            roles = f.Security?.SubscribeRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Announce:
                            roles = f.Security?.AnnounceRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Tag:
                            roles = f.Security?.TagRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Mention:
                            roles = f.Security?.MentionRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Categorize:
                            roles = f.Security?.CategorizeRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Prioritize:
                            roles = f.Security?.PrioritizeRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.Moderate:
                            roles = f.Security?.ModerateRoleIds;
                            break;
                        case DotNetNuke.Modules.ActiveForums.SecureActions.ManageUsers:
                            roles = f.Security?.ManageUsersRoleIds;
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

        internal static string GetForumsHtmlOption(int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo currentUser, bool includeHiddenForums)
        {
            var sb = new StringBuilder();
            int index = 1;
            var forums = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetForums(moduleId).Where(f => (includeHiddenForums || !f.Hidden) && (f.ForumGroup != null) && (includeHiddenForums || !f.ForumGroup.Hidden) && (currentUser.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(f.Security?.ViewRoleIds, currentUser.UserRoleIds)));
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

        public int Forums_Save(int portalId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo, bool isNew, bool useGroupFeatures, bool useGroupSecurity)
        {
            var oldPermissionsId = -1;
            var copyDownGroupSettings = false;
            if (forumInfo.ForumID <= 0)
            {
                isNew = true;
            }

            var forumGroupInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController.Instance.GetById(forumInfo.ForumGroupId, forumInfo.ModuleId);
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
                    forumInfo.PermissionsId = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.Instance.Insert(forumGroupInfo.Security).PermissionsId;
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
            forumInfo = this.GetById(forumInfo.ModuleId, forumId);
            if (!useGroupFeatures && string.IsNullOrEmpty(forumInfo.ForumSettingsKey))
            {
                forumInfo.ForumSettingsKey = $"F{forumId}";
                this._repositoryControllerBase.Update(forumInfo);
            }

            // if new forum and not using group features, copy group features to forum features as starting point
            if (copyDownGroupSettings)
            {
                forumInfo.FeatureSettings = forumInfo.ForumGroup.FeatureSettings;
                FeatureSettings.Save(forumInfo.ModuleId, forumInfo.ForumSettingsKey, forumInfo.FeatureSettings);
                this._repositoryControllerBase.Update(forumInfo);
            }

            if (oldPermissionsId != -1)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.Instance.RemoveIfUnused(permissionsId: oldPermissionsId, moduleId: forumInfo.ModuleId);
            }

            // if now inheriting group settings, remove any previously-defined forum settings
            if (forumInfo.InheritSettings)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.Instance.DeleteForModuleIdSettingsKey(forumInfo.ModuleId, $"F{forumInfo.ForumID}");
            }

            // Clear the caches
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.ClearAll(forumInfo.ModuleId);
            return forumId;
        }

        public void Forums_Delete(int forumId, int moduleId)
        {
            var parentForumId = this.GetById(moduleId, forumId).ParentForumId;
            DotNetNuke.Modules.ActiveForums.Controllers.ForumTopicController.Instance.DeleteForForum(moduleId, forumId);
            new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().DeleteForForum(moduleId, forumId);
            this._repositoryControllerBase.DeleteById(forumId);
            DataContext.Instance().Execute(System.Data.CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}communityforums_Forums_RepairSort", forumId, parentForumId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.ClearAll(moduleId);
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

        public static int CreateSocialGroupForum(int portalId, int moduleId, int socialGroupId, int forumGroupId, string forumName, string forumDescription, bool isPrivate, string forumConfig)
        {
            var forumId = -1;

            try
            {
                DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo gi = DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController.Instance.GetById(forumGroupId, moduleId);
                var socialGroup = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(portalId: portalId, roleId: socialGroupId);
                var groupAdmin = string.Concat(socialGroupId.ToString(), ":0");
                var groupMember = socialGroupId.ToString();
                var portalSettings = new DotNetNuke.Modules.ActiveForums.Helpers.PortalSettingsHelper().GetPortalSettings(portalId);
                int permissionsId = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.Instance.CreateAdminPermissions(portalSettings, moduleId).PermissionsId;

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

                forumId = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.Forums_Save(portalId, fi, true, true, true);
                fi = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetById(gi.ModuleId, forumId);
                fi.PermissionsId = permissionsId;
                DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.Forums_Save(portalId, fi, false, false, false);

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

            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(moduleId, string.Format(CacheKeys.ForumListXml, moduleId));

            return forumId;
        }

        public static int Forum_GetByTopicId(int moduleId, int topicId)
        {
            try
            {
                var forumTopic = DotNetNuke.Modules.ActiveForums.Controllers.ForumTopicController.Instance.GetByTopicId(moduleId: moduleId, topicId: topicId);
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
                return DataContext.Instance().ExecuteQuery<DateTime>(System.Data.CommandType.Text, "SELECT LastAccessDate FROM {databaseOwner}{objectQualifier}communityforums_Forums_Tracking WHERE ForumId = @0 AND UserId = @1", forumId, userId).FirstOrDefault();
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
                DataContext.Instance().Execute(System.Data.CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}communityforums_SaveTopicNextPrev", forumId);
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
                DataContext.Instance().Execute(System.Data.CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}communityforums_Forums_LastUpdates", forumId);
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
                DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.Get().Where(f => f.SocialGroupId != 0).ForEach(forum =>
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.Instance.UpdateSecurityForSocialGroupForum(forum);
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
            string link = fi.LastPost.GetLink();
            return $"<a href=\"{link}\">{System.Net.WebUtility.HtmlEncode(subject.TruncateWithEllipsis(length))}</a>";
        }

        HashSet<int> IForumController.GetForumsForUser(int moduleId, ForumUserInfo forumUser, SecureActions action)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(moduleId, forumUser, action);
        }

        HashSet<int> IForumController.GetForumIdsBySocialGroup(int moduleId, int socialGroupId)
        {
            return socialGroupId > 0 ? this._repositoryControllerBase.Find("WHERE SocialGroupId = @0 AND ModuleId = @1", socialGroupId, moduleId).Select(f => f.ForumID).Distinct().ToHashSet() : new HashSet<int>();
        }

        string IForumController.GetForumsHtmlOption(int moduleId, ForumUserInfo currentUser, bool includeHiddenForums)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsHtmlOption(moduleId, currentUser, includeHiddenForums);
        }
    }
}
