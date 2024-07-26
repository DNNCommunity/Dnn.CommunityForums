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
        private static int settingsCacheMinutes = 10;
        private static int contentCacheMinutes = 2;

        public static bool IsContentCachingEnabledForModule(int moduleId)
        {
            return true;

            #region "Do not delete this code"

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

        public static int ContentCachingTime(int moduleId)
        {
            return contentCacheMinutes;

            #region "Do not delete this code"
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

        public static void SettingsCacheStore(int moduleId, string cacheKey, object cacheObj)
        {
            SettingsCacheStore(moduleId, cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(settingsCacheMinutes));
        }

        public static void ContentCacheStore(int moduleId, string cacheKey, object cacheObj)
        {
            if (IsContentCachingEnabledForModule(moduleId))
            {
                Common.Utilities.DataCache.SetCache(cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(ContentCachingTime(moduleId)));
            }
        }

        public static void SettingsCacheStore(int moduleId, string cacheKey, object cacheObj, DateTime expiration)
        {
            try
            {
                Common.Utilities.DataCache.SetCache(cacheKey, cacheObj, expiration);
            }
            catch
            {
            }
        }

        public static object SettingsCacheRetrieve(int moduleId, string cacheKey)
        {
            return Common.Utilities.DataCache.GetCache(CacheKey: cacheKey);
        }

        public static object ContentCacheRetrieve(int moduleId, string cacheKey)
        {
            if (IsContentCachingEnabledForModule(moduleId))
            {
                return Common.Utilities.DataCache.GetCache(CacheKey: cacheKey);
            }
            else
            {
                return null;
            }
        }

        public static void SettingsCacheClear(int moduleId, string cacheKey)
        {
            try
            {
                Common.Utilities.DataCache.RemoveCache(CacheKey: cacheKey);
            }
            catch
            {
            }
        }

        public static void ContentCacheClear(int moduleId, string cacheKey)
        {
            if (IsContentCachingEnabledForModule(moduleId))
            {
                try
                {
                    Common.Utilities.DataCache.RemoveCache(CacheKey: cacheKey);
                }
                catch
                {
                }
            }
        }

        public static void CacheClearPrefix(int moduleId, string cacheKeyPrefix)
        {
            try
            {
                Common.Utilities.DataCache.ClearCache(cachePrefix: cacheKeyPrefix);
            }
            catch
            {
            }
        }

        public static void ClearAllCache(int moduleId)
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

        public static void ClearAllCacheForTabId(int tabId)
        {
            Common.Utilities.DataCache.ClearModuleCache(tabId);
        }

        public static void ClearSettingsCache(int moduleId)
        {
            try
            {
                object obj = SettingsCacheRetrieve(moduleId, cacheKey: string.Format(CacheKeys.MainSettings, moduleId));
                if (obj != null)
                {
                    SettingsCacheClear(moduleId, cacheKey: string.Format(CacheKeys.MainSettings, moduleId));
                }
            }
            catch
            {
            }
        }

        public static Hashtable GetSettings(int moduleId, string settingsKey, string cacheKey, bool useCache)
        {
            var ht = new Hashtable();
            if (useCache)
            {
                object obj = SettingsCacheRetrieve(moduleId, cacheKey);
                if (obj == null)
                {
                    IDataReader dr = DataProvider.Instance().Settings_List(moduleId, settingsKey);
                    while (dr.Read())
                    {
                        if (!ht.ContainsKey(dr["SettingName"].ToString()))
                        {
                            ht.Add(dr["SettingName"].ToString(), string.Empty);
                        }

                        ht[dr["SettingName"].ToString()] = dr["SettingValue"].ToString();
                    }

                    dr.Close();
                    SettingsCacheStore(moduleId, cacheKey, ht);
                }
                else
                {
                    ht = (Hashtable)obj;
                }
            }
            else
            {
                IDataReader dr = DataProvider.Instance().Settings_List(moduleId, settingsKey);
                while (dr.Read())
                {
                    if (!ht.ContainsKey(dr["SettingName"].ToString()))
                    {
                        ht.Add(dr["SettingName"].ToString(), string.Empty);
                    }

                    ht[dr["SettingName"].ToString()] = dr["SettingValue"].ToString();
                }

                dr.Close();
            }

            return ht;
        }
    }
}
