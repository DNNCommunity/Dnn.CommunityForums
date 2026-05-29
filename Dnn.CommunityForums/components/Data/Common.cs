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

namespace DotNetNuke.Modules.ActiveForums.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Microsoft.ApplicationBlocks.Data;

    public class Common : DataConfig
    {

        #region TagCloud
        public IDataReader TagCloud_Get(int portalId, int moduleId, string forumIds, int rows)
        {
            return SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "UI_TagCloud", portalId, moduleId, forumIds, rows);
        }

        #endregion
        #region TopMembers
        public IDataReader TopMembers_Get(int portalId, int rows)
        {
            return SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "UI_TopMembers", portalId, rows);
        }

        #endregion
        #region CustomURLS

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

        public void ArchiveURL(int portalId, int forumGroupId, int forumId, int topicId, string uRL)
        {
            SqlHelper.ExecuteNonQuery(this.connectionString, this.dbPrefix + "URL_Archive", portalId, forumGroupId, forumId, topicId, uRL);
        }

        public bool CheckForumURL(int portalId, int moduleId, string vanityName, int forumId, int forumGroupId)
        {
            try
            {
                ModuleSettings _mainSettings = SettingsBase.GetModuleSettings(moduleId);
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
                ModuleSettings _mainSettings = SettingsBase.GetModuleSettings(moduleId);
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
