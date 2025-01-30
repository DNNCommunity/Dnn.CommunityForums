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

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using DotNetNuke.Data;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using System;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;
    using System.Web;
    using DotNetNuke.UI.UserControls;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Collections;

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

        public static string GetForumIdsBySocialGroup(int portalId, int moduleId, int socialGroupId)
        {
            return socialGroupId > 0 ? string.Join(";", new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Find("WHERE SocialGroupId = @0 AND ModuleId = @1", socialGroupId, moduleId).Select(f => f.ForumID.ToString()).ToArray()) : string.Empty;
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

        public static string GetForumsForUser(int portalId, int moduleId, ForumUserInfo forumUser, string permissionType = "CanView", bool strict = false)
        {
            // Setting strict to true enforces the actual permission
            // If strict is false, forums will show up in the list if they are not hidden for users
            // that don't otherwise have access
            var forumIds = string.Empty;
            DotNetNuke.Modules.ActiveForums.Entities.ForumCollection fc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(moduleId);
            foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f in fc)
            {
                var roles = new HashSet<int>();
                switch (permissionType)
                {
                    case "CanView":
                        roles = f.Security?.ViewRoleIds;
                        break;
                    case "CanRead":
                        roles = f.Security?.ReadRoleIds;
                        break;
                    case "CanApprove":
                        roles = f.Security?.ModerateRoleIds;
                        break;
                    case "CanEdit":
                        roles = f.Security?.EditRoleIds;
                        break;
                    default:
                        roles = f.Security?.ViewRoleIds;
                        break;
                }

                var hasPermissions = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(roles, forumUser.UserRoleIds);

                if ((hasPermissions || (!strict && !f.Hidden && (permissionType == "CanView" || permissionType == "CanRead"))) && f.Active)
                {
                    forumIds += string.Concat(f.ForumID, ";");
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
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.IterateForumsList(forums.ToList(), currentUser, fi =>
                {
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", "-1", fi.GroupName);
                    index += 1;
                },
                fi =>
                {
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", fi.ForumID.ToString(), "--" + fi.ForumName);
                    index += 1;
                },
                fi =>
                {
                    sb.AppendFormat("<option value=\"{0}\">----{1}</option>", fi.ForumID.ToString(), fi.ForumName);
                    index += 1;
                },
                includeHiddenForums
                );
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

            forumInfo.ForumSettingsKey = useGroupFeatures ? (forumGroupInfo != null ? forumGroupInfo.GroupSettingsKey : string.Empty) : (forumInfo.ForumID > 0 ? $"F:{forumInfo.ForumID}" : string.Empty);

            // TODO: When this method is updated to use DAL2 for update, uncomment Cacheable attribute on ForumInfo
            var forumId = Convert.ToInt32(DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Forum_Save(portalId, forumInfo.ForumID, forumInfo.ModuleId, forumInfo.ForumGroupId, forumInfo.ParentForumId, forumInfo.ForumName, forumInfo.ForumDesc, forumInfo.SortOrder, forumInfo.Active, forumInfo.Hidden, forumInfo.ForumSettingsKey, forumInfo.PermissionsId, forumInfo.PrefixURL, forumInfo.SocialGroupId, forumInfo.HasProperties));
            if (!useGroupFeatures && string.IsNullOrEmpty(forumInfo.ForumSettingsKey))
            {
                forumInfo.ForumSettingsKey = $"F:{forumId}";
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
                DataContext.Instance().Execute(System.Data.CommandType.Text, "DELETE FROM {databaseOwner}{objectQualifier}activeforums_Settings WHERE ModuleId = @0 AND GroupKey = @1", forumInfo.ModuleId, $"F:{forumInfo.ForumID}");
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

        internal static void IterateForumsList(System.Collections.Generic.List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> forums, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUserInfo,
            Action<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> groupAction,
            Action<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> forumAction,
            Action<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> subForumAction,
            bool includeHiddenForums)
        {
            string tmpGroupKey = string.Empty;
            foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi in forums.Where(f => (includeHiddenForums || !f.Hidden) && f.ForumGroup != null && (includeHiddenForums || !f.ForumGroup.Hidden) && (forumUserInfo.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(f.Security?.ViewRoleIds, forumUserInfo.UserRoleIds))))
            {
                string groupKey = $"{fi.GroupName}{fi.ForumGroupId}";
                if (tmpGroupKey != groupKey)
                {
                    groupAction(fi);
                    tmpGroupKey = groupKey;
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

                int permissionsId = pc.CreateAdminPermissions(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetAdministratorsRoleId(portalId).ToString(), moduleId).PermissionsId;

                DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo(portalId)
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

                            DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId: permissionsId, requestedAccess: requestedAccess, objectId: groupAdmin, objectType: 2);
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

                            DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, requestedAccess: requestedAccess, groupMember, 0);
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

                                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, requestedAccess: requestedAccess, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredUsersRoleId(portalId).ToString(), 0);
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

                                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, requestedAccess: requestedAccess, DotNetNuke.Common.Globals.glbRoleAllUsers, 0);
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
            return new DotNetNuke.Modules.ActiveForums.Controllers.ForumTopicController(moduleId).GetForumForTopic(topicId).ForumId;
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

            if (fi.LastReplyId > 0)
            {
                var ri = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(fi.ModuleId).GetById(fi.LastReplyId);
            }

            string sURL = new ControlUtils().TopicURL(tabId, fi.ModuleId, fi.LastPostTopicId, fi.ForumGroup.PrefixURL, fi.PrefixURL, fi.LastPostTopicUrl);
            if (sURL.Contains("~/"))
            {
                sURL = Utilities.NavigateURL(tabId, string.Empty, new[] { $"{ParamKeys.TopicId}={fi.LastPostTopicId}", fi.LastPostIsReply ? $"{ParamKeys.ContentJumpId}={fi.LastReplyId}" : string.Empty });
            }
            if (sURL.EndsWith("/") && fi.LastPostIsReply)
            {
                sURL += Utilities.UseFriendlyURLs(fi.ModuleId) ? $"#{fi.LastPostID}" : $"?{ParamKeys.ContentJumpId}={fi.LastPostID}";
            }

            return $"<a href=\"{sURL}\">{System.Net.WebUtility.HtmlEncode(subject)}</a>";
        }
    }
}
