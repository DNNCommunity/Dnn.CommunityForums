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
    using System.Reflection;

    /// <summary>
    /// Provides common static cache operations backed by DNN's DataCache.
    /// </summary>
    internal static class CacheBase
    {
        /// <summary>
        /// Stores an object in the cache.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheObj">The object to cache.</param>
        /// <param name="expiration">The cache expiration time.</param>
        internal static void StoreInternal(string cacheKey, object cacheObj, DateTime expiration)
        {
            try
            {
                DotNetNuke.Common.Utilities.DataCache.SetCache(cacheKey, cacheObj, expiration);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        /// <summary>
        /// Retrieves an object from the cache.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns>The cached object, or null if not found or an error occurred.</returns>
        internal static object RetrieveInternal(string cacheKey)
        {
            try
            {
                return DotNetNuke.Common.Utilities.DataCache.GetCache(cacheKey: cacheKey);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return null;
        }

        /// <summary>
        /// Removes an object from the cache.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        internal static void ClearInternal(string cacheKey)
        {
            try
            {
                DotNetNuke.Common.Utilities.DataCache.RemoveCache(cacheKey: cacheKey);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        /// <summary>
        /// Clears cache by prefix for a given module.
        /// </summary>
        /// <param name="cacheKeyPrefix">The cache key prefix.</param>
        /// 
        public static void CacheClearPrefix(string cacheKeyPrefix)
        {
            try
            {
                DotNetNuke.Common.Utilities.DataCache.ClearCache(cachePrefix: cacheKeyPrefix);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        /// <summary>
        /// Clears all cache for a specific module.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        public static void ClearAllCache(int moduleId)
        {
            try
            {
                DotNetNuke.Common.Utilities.DataCache.ClearCache(string.Format(CacheKeys.CacheModulePrefix, moduleId));
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        /// <summary>
        /// Clears all module cache for a specific tab.
        /// </summary>
        /// <param name="tabId">The tab ID.</param>
        public static void ClearAllCacheForTabId(int tabId)
        {
            try
            {
                DotNetNuke.Common.Utilities.DataCache.ClearModuleCache(tabId);;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }
    }
}
