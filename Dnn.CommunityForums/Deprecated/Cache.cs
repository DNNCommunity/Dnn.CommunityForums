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

using System;
using System.Collections;
using System.Data;
using System.Web;
namespace DotNetNuke.Modules.ActiveForums
{
    public partial class DataCache
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use SettingsCacheStore(int ModuleId, string cacheKey, object cacheObj) or ContentCacheStore(int ModuleId, string cacheKey, object cacheObj)")]
        public static bool CacheStore(string cacheKey, object cacheObj)
        {
            SettingsCacheStore(-1, cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(settingsCacheMinutes));
            return true;
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use SettingsCacheStore(int ModuleId, string cacheKey, object cacheObj, DateTime Expiration) or ContentCacheStore(int ModuleId, string cacheKey, object cacheObj, DateTime Expiration)")]
        public static bool CacheStore(string cacheKey, object cacheObj, DateTime Expiration)
        {
            SettingsCacheStore(-1, cacheKey, cacheObj, Expiration);
            return true;
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use SettingsCacheRetrieve(int ModuleId, string cacheKey) or ContentCacheRetrieve(int ModuleId, string cacheKey)")]
        public static object CacheRetrieve(string cacheKey)
        {
            return SettingsCacheRetrieve(ModuleId: -1, cacheKey: cacheKey);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use CacheClear(int ModuleId, string cacheKey)")]
        public static bool CacheClear(string cacheKey)
        {
            SettingsCacheClear(ModuleId: -1, cacheKey: cacheKey);
            return true;
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use CacheClearPrefix(int ModuleId, string cacheKeyPrefix)")]
        public static bool CacheClearPrefix(string cacheKeyPrefix)
        {
            CacheClearPrefix(ModuleId: -1, cacheKeyPrefix);
            return true;
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use ClearAllCache(int ModuleId).")]
        public static void ClearAllCache(int ModuleId, int TabId)
        {
            ClearAllCache(ModuleId);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00.")]
        public static void ClearForumsByGroupCache(int ModuleID, int GroupID)
        {
            object obj = SettingsCacheRetrieve(ModuleID, ModuleID + GroupID + "ForumsByGroup");
            if (obj != null)
            {
                SettingsCacheClear(ModuleID, ModuleID + GroupID + "ForumsByGroup");
            }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Only Cleared but never Set so not needed.")]
        public static void ClearForumGroupsCache(int ModuleID)
        {
            SettingsCacheClear(ModuleID, ModuleID + "ForumGroups");
            IDataReader rd;
            rd = DataProvider.Instance().Groups_List(ModuleID);
            while (rd.Read())
            {
                ClearForumsByGroupCache(ModuleID, Convert.ToInt32(rd["ForumGroupID"]));
            }
            rd.Close();
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public static void ClearForumSettingsCache(int ForumID)
        {
            SettingsCacheClear(-1, string.Format(CacheKeys.ForumSettings, -1, ForumID));
            SettingsCacheClear(-1, string.Format(CacheKeys.ForumInfo, -1, ForumID));
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Not Used.")]
        public static void ClearAllForumSettingsCache(int ModuleID)
        {
            try
            {
                IDataReader rd;
                rd = DataProvider.Instance().Forums_List(-1, ModuleID, -1, -1, false);
                while (rd.Read())
                {
                    int ForumId = Convert.ToInt32(rd["ForumID"]);
                    SettingsCacheClear(ModuleID, string.Format(CacheKeys.ForumSettings, ModuleID, ForumId));
                    SettingsCacheClear(ModuleID, string.Format(CacheKeys.ForumInfo, ModuleID, ForumId));
                }
                rd.Close();
            }
            catch
            {
            }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public static void ClearFilterCache(int ModuleID)
        {
            object obj = SettingsCacheRetrieve(ModuleID, ModuleID + "FilterList");
            if (obj != null)
            {
                //Current.Cache.Remove(ModuleID & "FilterList")
                SettingsCacheClear(ModuleID, ModuleID + "FilterList");
            }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Replace with DotNetNuke.Modules.ActiveForums.SettingsBase.GetModuleSettings")]
        public static SettingsInfo MainSettings(int ModuleId)
        {
            return SettingsBase.GetModuleSettings(ModuleId);
        }
    }
}
