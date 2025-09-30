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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Data;

    public partial class DataCache
    {
        private static int settingsCacheMinutes = 10;
        private static int contentCacheMinutes = 2;

        private static bool IsContentCachingEnabledForModule(int moduleId)
        {
            #region "Do not delete this code"

            return true;

            /* DNN module caching uses "output caching" which doesn't work correctly with this module; in particular, CSS files are not referenced */
            /* Until this is resolved, content caching for this module is always enabled, for 2 minutes */

            /* Track whether caching is being used at all in this module; this setting itself is cached to avoid repeated module lookups;
               so it is stored/retrieved directly using DNN API rather than local APIs since if caching is disabled would never return the correct value for this setting
            */
            /*
            if (ModuleId < 0)
            {
                return true;
            }
            else
            {
                object IsCachingEnabledForModule = DotNetNuke.Common.Utilities.DataCache.GetCache(string.Format(CacheKeys.CacheEnabled, ModuleId));
                if (IsCachingEnabledForModule == null)
                {
                    DotNetNuke.Entities.Modules.ModuleInfo objModule = new DotNetNuke.Entities.Modules.ModuleController().GetModule(ModuleId);
                    IsCachingEnabledForModule = ((!String.IsNullOrEmpty(objModule.CacheMethod)) && (objModule.CacheTime > 0));
                    DotNetNuke.Common.Utilities.DataCache.SetCache(string.Format(CacheKeys.CacheEnabled, ModuleId), IsCachingEnabledForModule, DateTime.UtcNow.AddMinutes(settingsCacheTime));
                }
                return (bool)IsCachingEnabledForModule;
            }
            */
            #endregion
        }

        private static int ContentCachingTime(int moduleId)
        {
            #region "Do not delete this code"

            return contentCacheMinutes;
            /* DNN module caching uses "output caching" which doesn't work correctly with this module; in particular, CSS files are not referenced */
            /* Until this is resolved, content caching for this module is always enabled, for 2 minutes */

            /* DNN module caching uses "output caching" which doesn't work correctly with this module; in particular, CSS files are not referenced */
            /* Track caching being used for this module; this setting itself is cached to avoid repeated module lookups;
               so it is stored/retrieved directly using DNN API rather than local APIs since if caching is disabled would never return the correct value for this setting
            */

            /*
            if (ModuleId < 0)
            {
                return 0;
            }
            else
            {
                if (!IsContentCachingEnabledForModule(ModuleId))
                {
                    return 0;
                }
                else
                {
                    object CachingTime = DotNetNuke.Common.Utilities.DataCache.GetCache(string.Format(CacheKeys.CachingTime, ModuleId));
                    if (CachingTime == null)
                    {
                        CachingTime = new DotNetNuke.Entities.Modules.ModuleController().GetModule(ModuleId).CacheTime;
                        DotNetNuke.Common.Utilities.DataCache.SetCache(string.Format(CacheKeys.CachingTime, ModuleId), CachingTime, DateTime.UtcNow.AddMinutes(settingsCacheMinutes));
                    }
                    return (int)CachingTime;
                }
            }
            */
            #endregion
        }

        internal static void SettingsCacheStore(int moduleId, string cacheKey, object cacheObj)
        {
            SettingsCacheStore(moduleId, cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(settingsCacheMinutes));
        }

        internal static void ContentCacheStore(int moduleId, string cacheKey, object cacheObj)
        {
            try
            {
                if (IsContentCachingEnabledForModule(moduleId))
                {
                    Common.Utilities.DataCache.SetCache(cacheKey,
                        cacheObj,
                        DateTime.UtcNow.AddMinutes(ContentCachingTime(moduleId)));
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static void SettingsCacheStore(int moduleId, string cacheKey, object cacheObj, DateTime expiration)
        {
            try
            {
                Common.Utilities.DataCache.SetCache(cacheKey, cacheObj, expiration);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static object SettingsCacheRetrieve(int moduleId, string cacheKey)
        {
            try
            {
                return Common.Utilities.DataCache.GetCache(CacheKey: cacheKey);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return null;
        }

        internal static object ContentCacheRetrieve(int moduleId, string cacheKey)
        {
            if (IsContentCachingEnabledForModule(moduleId))
            {
                try
                {
                    return Common.Utilities.DataCache.GetCache(CacheKey: cacheKey);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }

            return null;
        }

        internal static void UserCacheStore(string cacheKey, object cacheObj)
        {
            try
            {
                Common.Utilities.DataCache.SetCache(cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(contentCacheMinutes));
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static object UserCacheRetrieve(string cacheKey)
        {
            try
            {
                return Common.Utilities.DataCache.GetCache(CacheKey: cacheKey);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return null;
        }

        internal static void UserCacheClear(string cacheKey)
        {
            try
            {
                Common.Utilities.DataCache.RemoveCache(CacheKey: cacheKey);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static void SettingsCacheClear(int moduleId, string cacheKey)
        {
            try
            {
                Common.Utilities.DataCache.RemoveCache(CacheKey: cacheKey);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static void ContentCacheClear(int moduleId, string cacheKey)
        {
            if (IsContentCachingEnabledForModule(moduleId))
            {
                try
                {
                    Common.Utilities.DataCache.RemoveCache(CacheKey: cacheKey);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
        }

        internal static void ContentCacheClearForTopic(int moduleId, int topicId)
        {
            if (IsContentCachingEnabledForModule(moduleId))
            {
                try
                {
                    DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.TopicInfo, moduleId, topicId));
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.TopicTrackingInfoPrefix, moduleId, topicId));
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.ForumViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.TopicViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.TopicsViewPrefix, moduleId));
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
        }

        internal static void ContentCacheClearForContent(int moduleId, int contentId)
        {
            if (IsContentCachingEnabledForModule(moduleId))
            {
                try
                {
                    DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.ContentInfo, moduleId, contentId));
                    DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.TopicInfoByContentId, moduleId, contentId));
                    DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.ReplyInfoByContentId, moduleId, contentId));
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.ForumViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.TopicViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.TopicsViewPrefix, moduleId));
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
        }

        internal static void ContentCacheClearForReply(int moduleId, int replyId)
        {
            if (IsContentCachingEnabledForModule(moduleId))
            {
                try
                {
                    DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.ReplyInfo, moduleId, replyId));
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.ForumViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.TopicViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.TopicsViewPrefix, moduleId));
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
        }

        internal static void ContentCacheClearForForum(int moduleId, int forumId)
        {
            if (IsContentCachingEnabledForModule(moduleId))
            {
                try
                {
                    DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.ForumInfo, moduleId, forumId));
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.TopicReadCountPrefix, moduleId, forumId));
                    DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.ForumTrackingInfoPrefix, moduleId, forumId));
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
        }

        internal static void CacheClearPrefix(int moduleId, string cacheKeyPrefix)
        {
            try
            {
                Common.Utilities.DataCache.ClearCache(cachePrefix: cacheKeyPrefix);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static void ClearAllCache()
        {
            try
            {
                Common.Utilities.DataCache.ClearCache(cachePrefix: CacheKeys.CachePrefix);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static void ClearAllCache(int moduleId)
        {
            try
            {
                CacheClearPrefix(moduleId, string.Format(CacheKeys.CacheModulePrefix, moduleId));
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static void ClearAllCacheForTabId(int tabId)
        {
            Common.Utilities.DataCache.ClearModuleCache(tabId);
        }

        internal static void ClearSettingsCache(int moduleId)
        {
            try
            {
                object obj = SettingsCacheRetrieve(moduleId, cacheKey: string.Format(CacheKeys.MainSettings, moduleId));
                if (obj != null)
                {
                    SettingsCacheClear(moduleId, cacheKey: string.Format(CacheKeys.MainSettings, moduleId));
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static Hashtable GetSettings(int moduleId, string settingsKey, string cacheKey, bool useCache)
        {
            var ht = new Hashtable();
            if (useCache)
            {
                ht = (Hashtable)SettingsCacheRetrieve(moduleId, cacheKey);
                if (ht == null)
                {
                    ht = new DotNetNuke.Modules.ActiveForums.Controllers.SettingsController().GetSettingsHashTableForModuleIdSettingsKey(moduleId, settingsKey);
                    SettingsCacheStore(moduleId, cacheKey, ht);
                }
            }
            else
            {
                ht = new DotNetNuke.Modules.ActiveForums.Controllers.SettingsController().GetSettingsHashTableForModuleIdSettingsKey(moduleId, settingsKey);
            }

            return ht;
        }
    }
}
