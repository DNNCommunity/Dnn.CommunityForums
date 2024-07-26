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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Web;

    public partial class DataCache
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use SettingsCacheStore(int ModuleId, string cacheKey, object cacheObj) or ContentCacheStore(int ModuleId, string cacheKey, object cacheObj)")]
        public static bool CacheStore(string cacheKey, object cacheObj)
        {
            SettingsCacheStore(-1, cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(settingsCacheMinutes));
            return true;
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use SettingsCacheStore(int ModuleId, string cacheKey, object cacheObj, DateTime Expiration) or ContentCacheStore(int ModuleId, string cacheKey, object cacheObj, DateTime Expiration)")]
        public static bool CacheStore(string cacheKey, object cacheObj, DateTime expiration)
        {
            SettingsCacheStore(-1, cacheKey, cacheObj, expiration);
            return true;
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use SettingsCacheRetrieve(int ModuleId, string cacheKey) or ContentCacheRetrieve(int ModuleId, string cacheKey)")]
        public static object CacheRetrieve(string cacheKey)
        {
            return SettingsCacheRetrieve(moduleId: -1, cacheKey: cacheKey);
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use CacheClear(int ModuleId, string cacheKey)")]
        public static bool CacheClear(string cacheKey)
        {
            SettingsCacheClear(moduleId: -1, cacheKey: cacheKey);
            return true;
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use CacheClearPrefix(int ModuleId, string cacheKeyPrefix)")]
        public static bool CacheClearPrefix(string cacheKeyPrefix)
        {
            CacheClearPrefix(moduleId: -1, cacheKeyPrefix);
            return true;
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use ClearAllCache(int ModuleId).")]
        public static void ClearAllCache(int moduleId, int tabId)
        {
            ClearAllCache(moduleId);
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00.")]
        public static void ClearForumsByGroupCache(int moduleID, int groupID)
        {
            object obj = SettingsCacheRetrieve(moduleID, moduleID + groupID + "ForumsByGroup");
            if (obj != null)
            {
                SettingsCacheClear(moduleID, moduleID + groupID + "ForumsByGroup");
            }
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Only Cleared but never Set so not needed.")]
        public static void ClearForumGroupsCache(int moduleID)
        {
            SettingsCacheClear(moduleID, moduleID + "ForumGroups");
            IDataReader rd;
            rd = DataProvider.Instance().Groups_List(moduleID);
            while (rd.Read())
            {
                ClearForumsByGroupCache(moduleID, Convert.ToInt32(rd["ForumGroupID"]));
            }

            rd.Close();
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public static void ClearForumSettingsCache(int forumID)
        {
            SettingsCacheClear(-1, string.Format(CacheKeys.ForumSettings, -1, forumID));
            SettingsCacheClear(-1, string.Format(CacheKeys.ForumInfo, -1, forumID));
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Not Used.")]
        public static void ClearAllForumSettingsCache(int moduleID)
        {
            try
            {
                IDataReader rd;
                rd = DataProvider.Instance().Forums_List(-1, moduleID, -1, -1, false);
                while (rd.Read())
                {
                    int forumId = Convert.ToInt32(rd["ForumID"]);
                    SettingsCacheClear(moduleID, string.Format(CacheKeys.ForumSettings, moduleID, forumId));
                    SettingsCacheClear(moduleID, string.Format(CacheKeys.ForumInfo, moduleID, forumId));
                }

                rd.Close();
            }
            catch
            {
            }
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public static void ClearFilterCache(int moduleID)
        {
            object obj = SettingsCacheRetrieve(moduleID, moduleID + "FilterList");
            if (obj != null)
            {
                // Current.Cache.Remove(ModuleID & "FilterList")
                SettingsCacheClear(moduleID, moduleID + "FilterList");
            }
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Replace with DotNetNuke.Modules.ActiveForums.SettingsBase.GetModuleSettings")]
        public static SettingsInfo MainSettings(int moduleId)
        {
            return SettingsBase.GetModuleSettings(moduleId);
        }
    }
}
