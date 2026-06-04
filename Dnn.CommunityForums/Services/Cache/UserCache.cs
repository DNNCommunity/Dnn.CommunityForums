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

    /// <summary>
    /// Manages user-related caching for forum user data.
    /// </summary>
    internal sealed class UserCache
    {
        private static readonly int UserCacheMinutes = 2;

        /// <summary>
        /// Stores user data in the cache.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheObj">The object to cache.</param>
        public static void Store(string cacheKey, object cacheObj)
        {
            DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.StoreInternal(cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(UserCacheMinutes));
        }

        /// <summary>
        /// Retrieves user data from the cache.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns>The cached user data, or null if not found or an error occurred.</returns>
        public static object Retrieve(string cacheKey)
        {
            return DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.RetrieveInternal(cacheKey);
        }

        /// <summary>
        /// Removes user data from the cache.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        public static void Clear(string cacheKey)
        {
            DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearInternal(cacheKey);
        }
    }
}
