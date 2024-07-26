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

namespace DotNetNuke.Modules.ActiveForums.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Microsoft.ApplicationBlocks.Data;

    public class Common : DataConfig
    {
        #region Security
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 9.00.00. Not used from at least v4 forward.")]
        public void SavePermissionSet(int permissionSetId, string permissionSet) => SqlHelper.ExecuteNonQuery(this.connectionString, this.dbPrefix + "Permissions_Save", permissionSetId, permissionSet);

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 9.00.00. Not used from at least v4 forward.")]
        public IDataReader GetRoles(int portalId) => SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "Permissions_GetRoles", portalId);

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetPermSet.")]
        public string GetPermSet(int permissionsId, string requestedAccess) => new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetPermSet(-1, permissionsId, requestedAccess);

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.SavePermSet.")]
        public string SavePermSet(int permissionsId, string requestedAccess, string permSet) => new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().SavePermSet(-1, permissionsId, requestedAccess, permSet);

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CreateAdminPermissions().")]
        public int CreatePermSet(string adminRoleId) => new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().CreateAdminPermissions(adminRoleId, -1).PermissionsId;

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CheckForumIdsForView().")]
        public string CheckForumIdsForView(int moduleId, string forumIds, string userRoles) => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CheckForumIdsForViewForRSS(moduleId, forumIds, userRoles);

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.WhichRolesCanViewForum().")]
        public string WhichRolesCanViewForum(int moduleId, int forumId, string userRoles) => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.WhichRolesCanViewForum(moduleId, forumId, userRoles);

        #endregion

        #region Views

        public DataSet UI_ActiveView(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, int timeFrame, string forumIds)
        {
            return SqlHelper.ExecuteDataset(this.connectionString, this.dbPrefix + "UI_ActiveView", portalId, moduleId, userId, rowIndex, maxRows, sort, timeFrame, forumIds);
        }

        public DataSet UI_NotReadView(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, string forumIds)
        {
            return SqlHelper.ExecuteDataset(this.connectionString, this.dbPrefix + "UI_NotRead", portalId, moduleId, userId, rowIndex, maxRows, sort, forumIds);
        }

        public DataSet UI_UnansweredView(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, string forumIds)
        {
            return SqlHelper.ExecuteDataset(this.connectionString, this.dbPrefix + "UI_UnansweredView", portalId, moduleId, userId, rowIndex, maxRows, sort, forumIds);
        }

        public DataSet UI_TagsView(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, string forumIds, int tagId)
        {
            return SqlHelper.ExecuteDataset(this.connectionString, this.dbPrefix + "UI_TagsView", portalId, moduleId, userId, rowIndex, maxRows, sort, forumIds, tagId);
        }

        public DataSet UI_MyTopicsView(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, string forumIds)
        {
            return SqlHelper.ExecuteDataset(this.connectionString, this.dbPrefix + "UI_MyTopicsView", portalId, moduleId, userId, rowIndex, maxRows, sort, forumIds);
        }

        public DataSet UI_MostLiked(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, int timeFrame, string forumIds)
        {
            return SqlHelper.ExecuteDataset(this.connectionString, this.dbPrefix + "UI_MostLiked", portalId, moduleId, userId, rowIndex, maxRows, sort, timeFrame, forumIds);
        }

        public DataSet UI_MostReplies(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, int timeFrame, string forumIds)
        {
            return SqlHelper.ExecuteDataset(this.connectionString, this.dbPrefix + "UI_MostReplies", portalId, moduleId, userId, rowIndex, maxRows, sort, timeFrame, forumIds);
        }

        public DataSet UI_Announcements(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, string forumIds)
        {
            return SqlHelper.ExecuteDataset(this.connectionString, this.dbPrefix + "UI_Announcements", portalId, moduleId, userId, rowIndex, maxRows, sort, forumIds);
        }

        public DataSet UI_Unresolved(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, string forumIds)
        {
            return SqlHelper.ExecuteDataset(this.connectionString, this.dbPrefix + "UI_Unresolved", portalId, moduleId, userId, rowIndex, maxRows, sort, forumIds);
        }
        #endregion

        #region TagCloud
        public IDataReader TagCloud_Get(int portalId, int moduleId, string forumIds, int rows)
        {
            return SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "UI_TagCloud", portalId, moduleId, forumIds, rows);
        }

        #endregion
        #region Tags
        public int Tag_GetIdByName(int portalId, int moduleId, string tagName, bool isCategory)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.connectionString, this.dbPrefix + "Tags_GetByName", portalId, moduleId, tagName.Replace("-", " ").ToLowerInvariant(), isCategory));
        }

        #endregion
        #region TopMembers
        public IDataReader TopMembers_Get(int portalId, int rows)
        {
            return SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "UI_TopMembers", portalId, rows);
        }

        #endregion
        #region CustomURLS
        public Dictionary<string, string> GetPrefixes(int portalId)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            using (IDataReader dr = SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "Forums_GetPrefixes", portalId))
            {
                while (dr.Read())
                {
                    string prefix = dr["PrefixURL"].ToString();
                    string tabid = dr["TabId"].ToString();
                    string forumid = dr["ForumId"].ToString();
                    string moduleId = dr["ModuleId"].ToString();
                    string archived = dr["Archived"].ToString();
                    string forumgroupId = dr["ForumGroupId"].ToString();
                    string groupPrefix = dr["GroupPrefixURL"].ToString();
                    if (!string.IsNullOrEmpty(groupPrefix))
                    {
                        prefix = groupPrefix + "/" + prefix;
                    }

                    dict.Add(prefix, tabid + "|" + forumid + "|" + moduleId + "|" + archived + "|" + forumgroupId + "|" + groupPrefix);
                }

                dr.Close();
            }

            return dict;
        }

        public string GetUrl(int moduleId, int forumGroupId, int forumId, int topicId, int userId, int contentId)
        {
            try
            {
                return Convert.ToString(SqlHelper.ExecuteScalar(this.connectionString, this.dbPrefix + "Util_GetUrl", moduleId, forumGroupId, forumId, topicId, userId, contentId));
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public IDataReader FindByURL(int portalId, string uRL)
        {
            return SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "FindByURL", portalId, uRL);
        }

        public IDataReader URLSearch(int portalId, string uRL)
        {
            return SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "URL_Search", portalId, uRL);
        }

        public void ArchiveURL(int portalId, int forumGroupId, int forumId, int topicId, string uRL)
        {
            SqlHelper.ExecuteNonQuery(this.connectionString, this.dbPrefix + "URL_Archive", portalId, forumGroupId, forumId, topicId, uRL);
        }

        public bool CheckForumURL(int portalId, int moduleId, string vanityName, int forumId, int forumGroupId)
        {
            try
            {
                SettingsInfo _mainSettings = SettingsBase.GetModuleSettings(moduleId);
                DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo fg = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetById(forumGroupId, moduleId);
                if (!string.IsNullOrEmpty(fg.PrefixURL))
                {
                    vanityName = fg.PrefixURL + "/" + vanityName;
                }

                if (!string.IsNullOrEmpty(_mainSettings.PrefixURLBase))
                {
                    vanityName = _mainSettings.PrefixURLBase + "/" + vanityName;
                }

                int tmpForumId = -1;
                tmpForumId = Convert.ToInt32(SqlHelper.ExecuteScalar(this.connectionString, this.dbPrefix + "URL_CheckForumVanity", portalId, vanityName));
                if (tmpForumId > 0 && forumId == -1)
                {
                    return false;
                }
                else if (tmpForumId == forumId && forumId > 0)
                {
                    return true;
                }
                else if (tmpForumId <= 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        public bool CheckGroupURL(int portalId, int moduleId, string vanityName, int forumGroupId)
        {
            try
            {
                SettingsInfo _mainSettings = SettingsBase.GetModuleSettings(moduleId);
                if (!string.IsNullOrEmpty(_mainSettings.PrefixURLBase))
                {
                    vanityName = _mainSettings.PrefixURLBase + "/" + vanityName;
                }

                int tmpForumGroupId = -1;
                tmpForumGroupId = Convert.ToInt32(SqlHelper.ExecuteScalar(this.connectionString, this.dbPrefix + "URL_CheckGroupVanity", portalId, vanityName));
                if (tmpForumGroupId > 0 && forumGroupId == -1)
                {
                    return false;
                }
                else if (tmpForumGroupId == forumGroupId && forumGroupId > 0)
                {
                    return true;
                }
                else if (tmpForumGroupId <= 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        #endregion
    }
}
