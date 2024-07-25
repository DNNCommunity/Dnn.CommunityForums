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
namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    using DotNetNuke.Data;
    using DotNetNuke.Modules.ActiveForums.API;
    using DotNetNuke.Modules.ActiveForums.Data;
    using Microsoft.ApplicationBlocks.Data;

    internal class ForumController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetById(int forumId, int moduleId)
        {
            string cachekey = string.Format(CacheKeys.ForumInfo, moduleId, forumId);
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum =  DataCache.SettingsCacheRetrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ForumInfo;
            if (forum == null)
            {
                forum = base.GetById(forumId, moduleId);
                if (forum != null)
                {
                    forum.LoadForumGroup();
                    forum.LoadSubForums();
                    forum.LoadProperties();
                    forum.LoadSettings();
                    forum.LoadSecurity();
                }

                DataCache.SettingsCacheStore(moduleId, cachekey, forum);
            }

            return forum;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumCollection GetForums(int moduleId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumCollection forums = DataCache.SettingsCacheRetrieve(moduleId, string.Format(CacheKeys.ForumList, moduleId)) as DotNetNuke.Modules.ActiveForums.Entities.ForumCollection;
            if (forums == null)
            {
                forums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
                foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum in base.Get(moduleId).OrderBy(f => f.ForumGroup?.SortOrder).ThenBy(f => f.SortOrder))
                {
                    forum.LoadForumGroup();
                    forum.LoadSubForums();
                    forum.LoadProperties();
                    forum.LoadSettings();
                    forum.LoadSecurity();
                    forums.Add(forum);
                }

                DataCache.SettingsCacheStore(moduleId, string.Format(CacheKeys.ForumList, moduleId), forums);
            }

            return forums;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumCollection GetSubForums(int forumId, int moduleId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumCollection forums = DataCache.SettingsCacheRetrieve(moduleId, string.Format(CacheKeys.SubForumList, moduleId, forumId)) as DotNetNuke.Modules.ActiveForums.Entities.ForumCollection;
            if (forums == null)
            {
                forums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
                foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum in base.Get(moduleId).Where(f => f.ParentForumId == forumId).OrderBy(f => f.ForumGroup?.SortOrder).ThenBy(f => f.SortOrder))
                {
                    forum.LoadForumGroup();
                    forum.LoadProperties();
                    forum.LoadSettings();
                    forum.LoadSecurity();
                    forums.Add(forum);
                }

                DataCache.SettingsCacheStore(moduleId, string.Format(CacheKeys.SubForumList, moduleId, forumId), forums);
            }

            return forums;
        }

        public static string GetForumIdsBySocialGroup(int portalId, int moduleId, int socialGroupId)
        {
            return socialGroupId > 0 ? string.Join(";", new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Find("WHERE SocialGroupId = @0", socialGroupId).Select(f => f.ForumID.ToString()).ToArray()) : string.Empty;
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
                forumId = Forum_GetByTopicId(topicId);
            }

            return forumId <= 0 ? null : new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId, moduleId);
        }

        public static string GetForumsForUser(string userRoles, int portalId, int moduleId, string permissionType = "CanView", bool strict = false)
        {
            // Setting strict to true enforces the actual permission
            // If strict is false, forums will show up in the list if they are not hidden for users
            // that don't otherwise have access

            var forumIds = string.Empty;
            DotNetNuke.Modules.ActiveForums.Entities.ForumCollection fc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(moduleId);
            foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f in fc)
            {
                string roles;
                switch (permissionType)
                {
                    case "CanView":
                        roles = f.Security?.View;
                        break;
                    case "CanRead":
                        roles = f.Security?.Read;
                        break;
                    case "CanApprove":
                        roles = f.Security?.ModApprove;
                        break;
                    case "CanEdit":
                        roles = f.Security?.ModEdit;
                        break;
                    default:
                        roles = f.Security?.View;
                        break;
                }

                var hasPermissions = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(roles, userRoles);

                if ((hasPermissions || (!strict && !f.Hidden && (permissionType == "CanView" || permissionType == "CanRead"))) && f.Active)
                {
                    forumIds += string.Concat(f.ForumID, ";");
                }
            }

            return forumIds;
        }

        public static string GetForumsHtmlOption(int moduleId, User currentUser)
        {
            var sb = new StringBuilder();
            int index = 1;
            var forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(moduleId).Where(f => !f.Hidden && (f.ForumGroup!=null) && !f.ForumGroup.Hidden && (currentUser.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security?.View, currentUser.UserRoles)));
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.IterateForumsList(forums.ToList(), currentUser,
                fi =>
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
                }
                );
            return sb.ToString();
        }

        public XmlDocument GetForumListXML(int PortalId, int ModuleId)
        {
            XmlDocument xDoc = new XmlDocument();
            object obj = DataCache.SettingsCacheRetrieve(ModuleId, string.Format(CacheKeys.ForumListXml, ModuleId));
            if (obj == null)
            {
                DotNetNuke.Modules.ActiveForums.Entities.ForumCollection fc = GetForums(ModuleId);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sb.AppendLine();
                sb.Append("<root>");
                sb.AppendLine();
                int groupId = -1;
                System.Text.StringBuilder groups = new System.Text.StringBuilder();
                System.Text.StringBuilder forums = new System.Text.StringBuilder();
                foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f in fc)
                {
                    if (groupId != f.ForumGroupId)
                    {
                        groups.Append("<group groupid=\"" + f.ForumGroupId.ToString() + "\" active=\"" + f.ForumGroup?.Active.ToString().ToLowerInvariant() + "\" hidden=\"" + f.ForumGroup?.Hidden.ToString().ToLowerInvariant() + "\">");
                        groups.Append("<name><![CDATA[" + f.GroupName.ToString() + "]]></name>");
                        //If Not String.IsNullOrEmpty(f.ForumGroup.SEO) Then
                        //    groups.Append(f.ForumGroup.SEO)
                        //End If
                        groups.Append("</group>");
                        sb.AppendLine();
                        groupId = f.ForumGroupId;
                    }
                }

                sb.Append("<groups>");
                sb.AppendLine();
                sb.Append(groups.ToString());
                sb.Append("</groups>");
                sb.AppendLine();
                foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f in fc)
                {
                    forums.Append("<forum groupid=\"" + f.ForumGroupId.ToString() + "\" forumid=\"" + f.ForumID.ToString() + "\"");
                    //forums.Append(" name=""" & HttpUtility.UrlEncode(f.ForumName) & """")
                    //forums.Append(" desc=""" & HttpUtility.UrlEncode(Utilities.HTMLEncode(f.ForumDesc.ToString)) & """")
                    forums.Append(" active=\"" + f.Active.ToString().ToLowerInvariant() + "\"");
                    forums.Append(" hidden=\"" + f.Hidden.ToString().ToLowerInvariant() + "\"");
                    forums.Append(" totaltopics=\"" + f.TotalTopics.ToString() + "\"");
                    forums.Append(" totalreplies=\"" + f.TotalReplies.ToString() + "\"");
                    forums.Append(" lasttopicid=\"" + f.LastTopicId.ToString() + "\"");
                    forums.Append(" lastreplyid=\"" + f.LastReplyId.ToString() + "\"");
                    //forums.Append(" lastpostsubject=""" & f.LastPostSubject & """")
                    //forums.Append(" lastpostauthorname=""" & f.LastPostDisplayName & """")
                    forums.Append(" lastpostauthorid=\"" + f.LastPostUserID + "\"");
                    forums.Append(" lastpostdate=\"" + f.LastPostDateTime.ToString() + "\"");
                    forums.Append(" lastread=\"" + f.LastRead.ToString() + "\"");
                    forums.Append(" allowrss=\"" + f.AllowRSS.ToString() + "\"");
                    forums.Append(" parentforumid=\"" + f.ParentForumId.ToString() + "\"");
                    forums.Append(" viewroles=\"" + f.Security?.View.ToString() + "\"");
                    forums.Append(" readroles=\"" + f.Security?.Read.ToString() + "\"");
                    forums.Append(" replyroles=\"" + f.Security?.Reply.ToString() + "\"");
                    forums.Append(" modroles=\"" + f.Security?.ModApprove.ToString() + "\"");
                    forums.Append(" modmove=\"" + f.Security?.ModMove.ToString() + "\"");
                    forums.Append(">");
                    forums.Append("<name><![CDATA[" + f.ForumName + "]]></name>");
                    forums.Append("<description><![CDATA[" + f.ForumDesc + "]]></description>");
                    forums.Append("<security>");
                    forums.Append("<view>" + f.Security?.View + "</view>");
                    forums.Append("<read>" + f.Security?.Read + "</read>");
                    forums.Append("<create>" + f.Security?.Create + "</create>");
                    forums.Append("<reply>" + f.Security?.Reply + "</reply>");
                    forums.Append("<edit>" + f.Security?.Edit + "</edit>");
                    forums.Append("<delete>" + f.Security?.Delete + "</delete>");
                    forums.Append("<lock>" + f.Security?.Lock + "</lock>");
                    forums.Append("<pin>" + f.Security?.Pin + "</pin>");
                    forums.Append("<modapprove>" + f.Security?.ModApprove + "</modapprove>");
                    forums.Append("<modedit>" + f.Security?.ModEdit + "</modedit>");
                    forums.Append("<moddelete>" + f.Security?.ModDelete + "</moddelete>");
                    forums.Append("<modlock>" + f.Security?.ModLock + "</modlock>");
                    forums.Append("<modpin>" + f.Security?.ModPin + "</modpin>");
                    forums.Append("<modmove>" + f.Security?.ModMove + "</modmove>");
                    forums.Append("</security>");
                    //If Not String.IsNullOrEmpty(f.SEO) Then
                    //    forums.Append(f.SEO)
                    //End If

                    forums.Append("</forum>");
                    sb.AppendLine();
                }

                sb.Append("<forums>");
                sb.AppendLine();
                sb.Append(forums.ToString());
                sb.Append("</forums>");
                sb.AppendLine();
                sb.Append("<topics />");
                sb.AppendLine();
                sb.Append("<replies />");
                sb.AppendLine();
                sb.Append("</root>");
                sb.AppendLine();

                //Dim sXML As String = ds.GetXml()
                xDoc.LoadXml(sb.ToString());
                DataCache.SettingsCacheStore(ModuleId, string.Format(CacheKeys.ForumListXml, ModuleId), xDoc);
            }
            else
            {
                xDoc = (XmlDocument)obj;
            }

            return xDoc;
        }

        public int Forums_Save(int portalId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi, bool isNew, bool useGroupFeatures, bool useGroupSecurity)
        {
            var fg = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetById(fi.ForumGroupId, fi.ModuleId);
            if (useGroupSecurity)
            {
                if (fg != null)
                {
                    fi.PermissionsId = fg.PermissionsId;
                }
            }
            else
            {
                if (isNew || (fi?.PermissionsId == fg?.PermissionsId)) /* new forum or switching from group security to forum security */
                {
                    fi.PermissionsId = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().CreateAdminPermissions(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetAdministratorsRoleId(portalId).ToString(), fi.ModuleId).PermissionsId;
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CreateDefaultSets(portalId, fi.PermissionsId);
                }
            }

            fi.ForumSettingsKey = useGroupFeatures ? (fg != null ? fg.GroupSettingsKey : string.Empty) : (fi.ForumID > 0 ? $"F:{fi.ForumID}" : string.Empty);

            if (fi.ForumID <= 0)
            {
                isNew = true;
            }

            //TODO: When this method is updated to use DAL2 for update, uncomment Cacheable attribute on ForumInfo
            var forumId = Convert.ToInt32(DataProvider.Instance().Forum_Save(portalId, fi.ForumID, fi.ModuleId, fi.ForumGroupId, fi.ParentForumId, fi.ForumName, fi.ForumDesc, fi.SortOrder, fi.Active, fi.Hidden, fi.ForumSettingsKey, fi.PermissionsId, fi.PrefixURL, fi.SocialGroupId, fi.HasProperties));
            if (!useGroupFeatures && String.IsNullOrEmpty(fi.ForumSettingsKey))
            {
                fi.ForumSettingsKey = $"F:{forumId}";
            }

            if (fi.ForumSettingsKey.StartsWith("G:"))
            {
                DataProvider.Instance().Forum_ConfigCleanUp(fi.ModuleId, $"F:{fi.ForumID}");
            }

            if (isNew && useGroupFeatures == false)
            {
                var sKey = $"F:{fi.ForumID}";
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.TopicsTemplateId, "0");
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.TopicTemplateId, "0");
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.TopicFormId, "0");
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.ReplyFormId, "0");
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.AllowRSS, "false");
            }

            // Clear the caches
            DataCache.ClearSettingsCache(fi.ModuleId);
            return forumId;
        }

        public void Forums_Delete(int portalId, int forumId, int moduleId)
        {
            //TODO: When these methods are updated to use DAL2 for update, uncomment Cacheable attribute on forumInfo
            DataProvider.Instance().Forums_Delete(portalId, moduleId, forumId);
        }

        internal static void IterateForumsList(System.Collections.Generic.List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> forums, User currentUser,
            Action<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> groupAction,
            Action<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> forumAction,
            Action<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> subForumAction)
        {
            string tmpGroupKey = string.Empty;
            foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi in forums.Where(f => !f.Hidden && f.ForumGroup != null && !f.ForumGroup.Hidden && (currentUser.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security?.View, currentUser.UserRoles))))
            {
                string GroupKey = $"{fi.GroupName}{fi.ForumGroupId}";
                if (tmpGroupKey != GroupKey)
                {
                    groupAction(fi);
                    tmpGroupKey = GroupKey;
                }

                if (fi.ParentForumId == 0)
                {
                    forumAction(fi);
                    foreach (var subforum in forums.Where(f => f.ParentForumId == fi.ForumID && !f.Hidden && f.ForumGroup != null && !f.ForumGroup.Hidden && (currentUser.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security?.View, currentUser.UserRoles))))
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

                DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo
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
                    SocialGroupId = socialGroupId
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

                            DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, PermissionsId: permissionsId, requestedAccess: requestedAccess, objectId: groupAdmin, objectType: 2);
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

                                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(moduleId, permissionsId, requestedAccess: requestedAccess, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredRoleId(portalId).ToString(), 0);
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

            DataCache.SettingsCacheClear(moduleId, string.Format(CacheKeys.ForumListXml, moduleId));

            return forumId;
        }

        public static int Forum_GetByTopicId(int TopicId)
        {
            return new DotNetNuke.Data.SqlDataProvider().ExecuteScalar<int>("activeforums_ForumGetByTopicId", TopicId);
        }

        public static DateTime Forum_GetLastReadTopicByUser(int ForumId, int UserId)
        {
            try
            {
                return DataContext.Instance().ExecuteQuery<DateTime>(System.Data.CommandType.Text, "SELECT LastAccessDate FROM {databaseOwner}{objectQualifier}activeforums_Forums_Tracking WHERE ForumId = @0 AND UserId = @1", ForumId, UserId).FirstOrDefault();
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
                DataContext.Instance().Execute(System.Data.CommandType.StoredProcedure, "activeforums_SaveTopicNextPrev", forumId);
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
                DataContext.Instance().Execute(System.Data.CommandType.StoredProcedure, "activeforums_Forums_LastUpdates", forumId);
                return true;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }
        }
    }
}
