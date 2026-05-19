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

namespace DotNetNuke.Modules.ActiveForums.Services.Cache
{
    using System;
    using System.Collections;

    /// <summary>
    /// Manages settings caching for module configuration and settings.
    /// </summary>
    internal sealed class SettingsCache
    {
        private static readonly int SettingsCacheMinutes = 10;

        /// <summary>
        /// Stores settings in the cache.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheObj">The object to cache.</param>
        public static void Store(int moduleId, string cacheKey, object cacheObj)
        {
            DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.StoreInternal(cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(SettingsCacheMinutes));
        }

        /// <summary>
        /// Stores settings in the cache with a custom expiration time.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheObj">The object to cache.</param>
        /// <param name="expiration">The cache expiration time.</param>
        public static void Store(int moduleId, string cacheKey, object cacheObj, DateTime expiration)
        {
            DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.StoreInternal(cacheKey, cacheObj, expiration);
        }

        /// <summary>
        /// Retrieves settings from the cache.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns>The cached settings object, or null if not found or an error occurred.</returns>
        public static object Retrieve(int moduleId, string cacheKey)
        {
            return DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.RetrieveInternal(cacheKey);
        }

        /// <summary>
        /// Removes settings from the cache.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="cacheKey">The cache key.</param>
        public static void Clear(int moduleId, string cacheKey)
        {
            DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearInternal(cacheKey);
        }

        /// <summary>
        /// Clears all settings cache for a module.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        public static void ClearAll(int moduleId)
        {
            try
            {
                object obj = Retrieve(moduleId, string.Format(CacheKeys.MainSettings, moduleId));
                if (obj != null)
                {
                    Clear(moduleId, string.Format(CacheKeys.MainSettings, moduleId));
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        /// <summary>
        /// Retrieves settings as a hashtable, using cache if available.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="settingsKey">The settings key.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="useCache">Whether to use cache.</param>
        /// <returns>A hashtable of settings.</returns>
        public static Hashtable GetSettings(int moduleId, string settingsKey, string cacheKey, bool useCache)
        {
            var ht = new Hashtable();
            if (useCache)
            {
                ht = (Hashtable)Retrieve(moduleId, cacheKey);
                if (ht == null)
                {
                    ht = DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.Instance.GetSettingsHashTableForModuleIdSettingsKey(moduleId, settingsKey);
                    Store(moduleId, cacheKey, ht);
                }
            }
            else
            {
                ht = DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.Instance.GetSettingsHashTableForModuleIdSettingsKey(moduleId, settingsKey);
            }

            return ht;
        }
    }
}
