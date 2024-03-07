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
//
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using DotNetNuke.Common.Controls;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Modules.ActiveForums.Entities;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using Microsoft.ApplicationBlocks.Data;
namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal class ForumController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>
    {
        private static ForumsDB _forumDB;
        internal static ForumsDB GetForumsDB()
        {
            return _forumDB ?? (_forumDB = new ForumsDB());
        }
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetById(int forumId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum = base.GetById(forumId);
            if (forum != null)
            {
                forum.ForumGroup = forum.ForumGroupId > 0 ? new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetForumGroup(forum.ModuleId, forum.ForumGroupId) : null;
                forum.ForumSettings = (Hashtable)DataCache.GetSettings(forum.ModuleId, forum.ForumSettingsKey, string.Format(CacheKeys.ForumSettingsByKey, forum.ModuleId, forum.ForumSettingsKey), true);
                forum.Security = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(forum.PermissionsId);
                if (forum.HasProperties)
                {
                    var propC = new PropertiesController();
                    forum.Properties = propC.ListProperties(forum.PortalId, 1, forumId);
                }
            }
            string cachekey = string.Format(CacheKeys.ForumInfo, forum.ModuleId, forumId);
            DataCache.SettingsCacheStore(forum.ModuleId, cachekey, forum);
            return forum;
        }
        public DotNetNuke.Modules.ActiveForums.Entities.ForumCollection GetForums(int ModuleId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumCollection forums = DataCache.SettingsCacheRetrieve(ModuleId, string.Format(CacheKeys.ForumList, ModuleId)) as DotNetNuke.Modules.ActiveForums.Entities.ForumCollection;
            if (forums == null)
            {
                forums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
                foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum in base.Get(ModuleId))
                {
                    forum.ForumGroup = forum.ForumGroupId > 0 ? new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetForumGroup(ModuleId, forum.ForumGroupId) : null;
                    forum.ForumSettings = (Hashtable)DataCache.GetSettings(ModuleId, forum.ForumSettingsKey, string.Format(CacheKeys.ForumSettingsByKey, ModuleId, forum.ForumSettingsKey), true);
                    forum.Security = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(forum.PermissionsId);
                    if (forum.HasProperties)
                    {
                        var propC = new PropertiesController();
                        forum.Properties = propC.ListProperties(forum.PortalId, 1, forum.ForumID);
                    }
                    forums.Add(forum);
                }
                DataCache.SettingsCacheStore(ModuleId, string.Format(CacheKeys.ForumList, ModuleId), forums);
            }
            return forums;
        }
        public static DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetForum(int portalId, int moduleId, int forumId) => GetForum(portalId, moduleId, forumId, false);
        public static DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetForum(int portalId, int moduleId, int forumId, bool ignoreCache)
        {
            string cachekey = string.Format(CacheKeys.ForumInfo, moduleId, forumId);
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum = ignoreCache ? null : DataCache.SettingsCacheRetrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ForumInfo;
            if (forum == null)
            {
                forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId);
                DataCache.SettingsCacheStore(forum.ModuleId, cachekey, forum);
            }
            return forum;
        }
        public static string GetForumIdsBySocialGroup(int portalId, int socialGroupId)
        {
            return socialGroupId > 0 ? string.Join(";", new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Find("WHERE SocialGroupId = @0", socialGroupId).Select(f => f.ForumID.ToString()).ToArray()) : string.Empty;
        }
        internal static DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forums_Get(int portalId, int moduleId, int forumId, bool useCache)
        {
            return GetForum(portalId: portalId, moduleId: moduleId, forumId: forumId, !useCache);
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
                forumId = GetForumsDB().Forum_GetByTopicId(topicId);
            }

            return forumId <= 0 ? null : GetForum(portalId, moduleId, forumId, !useCache);
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
                        roles = f.Security.View;
                        break;
                    case "CanRead":
                        roles = f.Security.Read;
                        break;
                    case "CanApprove":
                        roles = f.Security.ModApprove;
                        break;
                    case "CanEdit":
                        roles = f.Security.ModEdit;
                        break;
                    default:
                        roles = f.Security.View;
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
        public XmlDocument GetForumListXML(int PortalId, int ModuleId)
        {
            XmlDocument xDoc = new XmlDocument();
            object obj = DataCache.SettingsCacheRetrieve(ModuleId, string.Format(CacheKeys.ForumListXml, ModuleId));
            if (obj == null)
            {
                Data.ForumsDB db = new Data.ForumsDB();
                DotNetNuke.Modules.ActiveForums.Entities.ForumCollection fc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(ModuleId);
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
                        groups.Append("<group groupid=\"" + f.ForumGroupId.ToString() + "\" active=\"" + f.ForumGroup.Active.ToString().ToLowerInvariant() + "\" hidden=\"" + f.ForumGroup.Hidden.ToString().ToLowerInvariant() + "\">");
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
                    forums.Append(" allowrss=\"" + f.ForumSettings["ALLOWRSS"].ToString() + "\"");
                    forums.Append(" parentforumid=\"" + f.ParentForumId.ToString() + "\"");
                    forums.Append(" viewroles=\"" + f.Security.View.ToString() + "\"");
                    forums.Append(" readroles=\"" + f.Security.Read.ToString() + "\"");
                    forums.Append(" replyroles=\"" + f.Security.Reply.ToString() + "\"");
                    forums.Append(" modroles=\"" + f.Security.ModApprove.ToString() + "\"");
                    forums.Append(" modmove=\"" + f.Security.ModMove.ToString() + "\"");
                    forums.Append(">");
                    forums.Append("<name><![CDATA[" + f.ForumName + "]]></name>");
                    forums.Append("<description><![CDATA[" + f.ForumDesc + "]]></description>");
                    forums.Append("<security>");
                    forums.Append("<view>" + f.Security.View + "</view>");
                    forums.Append("<read>" + f.Security.Read + "</read>");
                    forums.Append("<create>" + f.Security.Create + "</create>");
                    forums.Append("<reply>" + f.Security.Reply + "</reply>");
                    forums.Append("<edit>" + f.Security.Edit + "</edit>");
                    forums.Append("<delete>" + f.Security.Delete + "</delete>");
                    forums.Append("<lock>" + f.Security.Lock + "</lock>");
                    forums.Append("<pin>" + f.Security.Pin + "</pin>");
                    forums.Append("<modapprove>" + f.Security.ModApprove + "</modapprove>");
                    forums.Append("<modedit>" + f.Security.ModEdit + "</modedit>");
                    forums.Append("<moddelete>" + f.Security.ModDelete + "</moddelete>");
                    forums.Append("<modlock>" + f.Security.ModLock + "</modlock>");
                    forums.Append("<modpin>" + f.Security.ModPin + "</modpin>");
                    forums.Append("<modmove>" + f.Security.ModMove + "</modmove>");
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
            //Logger.Log(xDoc.OuterXml)
            return xDoc;
        }
        public static string GetForumsHtmlOption(int portalId, int moduleId, User currentUser)
        {
            var userForums = GetForumsForUser(currentUser.UserRoles, portalId, moduleId, "CanView");
            var dt = DataProvider.Instance().UI_ForumView(portalId, moduleId, currentUser.UserId, currentUser.IsSuperUser, userForums).Tables[0];
            var i = 0;
            var n = 1;
            var tmpGroupCount = 0;
            var tmpForumCount = 0;
            var tmpGroupKey = string.Empty;
            var tmpForumKey = string.Empty;
            var sb = new StringBuilder();
            foreach (DataRow dr in dt.Rows)
            {
                var bView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(dr["CanView"].ToString(), currentUser.UserRoles);
                var groupName = Convert.ToString(dr["GroupName"]);
                var groupId = Convert.ToInt32(dr["ForumGroupId"]);
                var groupKey = groupName + groupId.ToString();
                var forumName = Convert.ToString(dr["ForumName"]);
                var forumId = Convert.ToInt32(dr["ForumId"]);
                var forumKey = forumName + forumId.ToString();
                var parentForumId = Convert.ToInt32(dr["ParentForumId"]);

                //TODO - Need to add support for Group Permissions and GroupHidden

                if (tmpGroupKey != groupKey)
                {
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", "-1", groupName);
                    n += 1;
                    tmpGroupKey = groupKey;
                }

                if (bView)
                {
                    if (parentForumId == 0)
                    {
                        sb.AppendFormat("<option value=\"{0}\">{1}</option>", dr["ForumID"], "--" + dr["ForumName"]);
                        n += 1;
                        sb.Append(GetSubForums(n, Convert.ToInt32(dr["ForumId"]), dt, ref n));
                    }

                }
            }

            return sb.ToString();
        }
        public static int Forums_Save(int portalId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi, bool isNew, bool useGroup)
        {
            var rc = new RoleController();
            var db = new Data.Common();
            var permissionsId = -1;
            if (useGroup && (string.IsNullOrEmpty(fi.ForumSettingsKey) || fi.PermissionsId == -1))
            {
                var fg = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetForumGroup(fi.ModuleId, fi.ForumGroupId);
                if (fg != null)
                {
                    fi.ForumSettingsKey = fg.GroupSettingsKey;
                    fi.PermissionsId = fg.PermissionsId;
                }
            }
            else if (fi.PermissionsId <= 0 && useGroup == false)
            {
                var ri = rc.GetRoleByName(portalId, "Administrators");
                if (ri != null)
                {
                    fi.PermissionsId = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().CreateAdminPermissions(ri.RoleID.ToString()).PermissionsId;
                    permissionsId = fi.PermissionsId;
                    isNew = true;
                }
                if (fi.ForumID > 0 & !(string.IsNullOrEmpty(fi.ForumSettingsKey)))
                {
                    if (fi.ForumSettingsKey.Contains("G:"))
                    {
                        fi.ForumSettingsKey = string.Empty;
                    }
                }
                if (string.IsNullOrEmpty(fi.ForumSettingsKey) && fi.ForumID > 0)
                {
                    fi.ForumSettingsKey = $"F:{fi.ForumID}";
                }
            }
            else if (useGroup == false && string.IsNullOrEmpty(fi.ForumSettingsKey) && fi.ForumID > 0)
            {
                fi.ForumSettingsKey = $"F:{fi.ForumID}";
            }

            var forumId = Convert.ToInt32(DataProvider.Instance().Forum_Save(portalId, fi.ForumID, fi.ModuleId, fi.ForumGroupId, fi.ParentForumId, fi.ForumName, fi.ForumDesc, fi.SortOrder, fi.Active, fi.Hidden, fi.ForumSettingsKey, fi.PermissionsId, fi.PrefixURL, fi.SocialGroupId, fi.HasProperties));
            if (String.IsNullOrEmpty(fi.ForumSettingsKey))
                fi.ForumSettingsKey = string.Concat("F:", forumId);

            if (fi.ForumSettingsKey.Contains("G:"))
                DataProvider.Instance().Forum_ConfigCleanUp(fi.ModuleId, string.Concat("F:", fi.ForumID));

            if (isNew && useGroup == false)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CreateDefaultSets(portalId, permissionsId);

                var sKey = "F:" + forumId.ToString();
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.TopicsTemplateId, "0");
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.TopicTemplateId, "0");
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.TopicFormId, "0");
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.ReplyFormId, "0");
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.AllowRSS, "false");
            }

            // Clear the caches
            DataCache.SettingsCacheClear(fi.ModuleId, string.Format(CacheKeys.ForumList, fi.ModuleId));
            DataCache.SettingsCacheClear(fi.ModuleId, string.Format(CacheKeys.ForumInfo, fi.ModuleId, forumId));

            return forumId;
        }
        public static DataTable GetForumView(int portalId, int moduleId, int currentUserId, bool isSuperUser, string forumIds)
        {
            DataSet ds;
            DataTable dt;
            var cachekey = string.Format(CacheKeys.ForumViewForUser, moduleId, currentUserId, forumIds);

            var dataSetXML = DataCache.ContentCacheRetrieve(moduleId, cachekey) as string;

            // cached datatable is held as an XML string (because data vanishes if just caching the DT in this instance)
            if (dataSetXML != null)
            {
                var sr = new StringReader(dataSetXML);
                ds = new DataSet();
                ds.ReadXml(sr);
                dt = ds.Tables[0];
            }
            else
            {
                ds = DataProvider.Instance().UI_ForumView(portalId, moduleId, currentUserId, isSuperUser, forumIds);
                dt = ds.Tables[0];

                var sw = new StringWriter();

                dt.WriteXml(sw);
                var result = sw.ToString();

                sw.Close();
                sw.Dispose();

                DataCache.ContentCacheStore(moduleId, cachekey, result);
            }

            return dt;
        }
        private static string GetSubForums(int itemCount, int parentForumId, DataTable dtForums, ref int n)
        {
            var sb = new StringBuilder();
            dtForums.DefaultView.RowFilter = string.Concat("ParentForumId = ", parentForumId);
            if (dtForums.DefaultView.Count > 0)
            {
                foreach (DataRow dr in dtForums.DefaultView.ToTable().Rows)
                {
                    sb.AppendFormat("<option value=\"{0}\">----{1}</option>", dr["ForumID"], dr["ForumName"]);
                    itemCount += 1;
                }
            }
            n = itemCount;
            return sb.ToString();
        }
        public static int CreateGroupForum(int portalId, int moduleId, int socialGroupId, int forumGroupId, string forumName, string forumDescription, bool isPrivate, string forumConfig)
        {
            var forumId = -1;

            try
            {
                var forumsDb = new Data.Common();
                DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo gi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetForumGroup(moduleId, forumGroupId);
                var socialGroup = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(portalId: portalId, roleId: socialGroupId);
                var groupAdmin = string.Concat(socialGroupId.ToString(), ":0");
                var groupMember = socialGroupId.ToString();

                var ri = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleByName(portalId: portalId, roleName: "Administrators");
                var permissionsId = forumsDb.CreatePermSet(ri.RoleID.ToString());

                moduleId = gi.ModuleId;

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

                forumId = Forums_Save(portalId, fi, true, true);
                fi = GetForum(portalId, moduleId, forumId);
                fi.PermissionsId = permissionsId;
                Forums_Save(portalId, fi, false, false);

                var xDoc = new XmlDocument();
                xDoc.LoadXml(forumConfig);

                var xRoot = xDoc.DocumentElement;
                if (xRoot != null)
                {
                    var xSecList = xRoot.SelectSingleNode("//security[@type='groupadmin']");
                    string permSet;
                    string requestedAccess;
                    if (xSecList != null)
                    {
                        foreach (XmlNode n in xSecList.ChildNodes)
                        {
                            requestedAccess = n.Name;
                            if (n.Attributes == null || n.Attributes["value"].Value != "true")
                                continue;
                            permSet = forumsDb.GetPermSet(permissionsId, requestedAccess);
                            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(groupAdmin, 2, permSet);
                            forumsDb.SavePermSet(permissionsId, requestedAccess, permSet);
                        }
                    }

                    xSecList = xRoot.SelectSingleNode("//security[@type='groupmember']");
                    if (xSecList != null)
                    {
                        foreach (XmlNode n in xSecList.ChildNodes)
                        {
                            requestedAccess = n.Name;

                            if (n.Attributes == null || n.Attributes["value"].Value != "true")
                                continue;

                            permSet = forumsDb.GetPermSet(permissionsId, requestedAccess);
                            permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(groupMember, 0, permSet);
                            forumsDb.SavePermSet(permissionsId, requestedAccess, permSet);
                        }
                    }

                    if (socialGroup.IsPublic)
                    {
                        xSecList = xRoot.SelectSingleNode("//security[@type='registereduser']");
                        ri = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleByName(portalId: portalId, roleName: "Registered Users");
                        if (xSecList != null)
                        {
                            foreach (XmlNode n in xSecList.ChildNodes)
                            {
                                requestedAccess = n.Name;

                                if (n.Attributes == null || n.Attributes["value"].Value != "true")
                                    continue;

                                permSet = forumsDb.GetPermSet(permissionsId, requestedAccess);
                                permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(ri.RoleID.ToString(), 0, permSet);
                                forumsDb.SavePermSet(permissionsId, requestedAccess, permSet);
                            }
                        }

                        xSecList = xRoot.SelectSingleNode("//security[@type='anon']");
                        if (xSecList != null)
                        {
                            foreach (XmlNode n in xSecList.ChildNodes)
                            {
                                requestedAccess = n.Name;

                                if (n.Attributes == null || n.Attributes["value"].Value != "true")
                                    continue;

                                permSet = forumsDb.GetPermSet(permissionsId, requestedAccess);
                                permSet = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet("-1", 0, permSet);
                                forumsDb.SavePermSet(permissionsId, requestedAccess, permSet);
                            }
                        }
                    }
                }
            }
            catch
            {
                // do nothing?? 
            }

            DataCache.SettingsCacheClear(moduleId, string.Format(CacheKeys.ForumListXml, moduleId));

            return forumId;
        }
    }
}