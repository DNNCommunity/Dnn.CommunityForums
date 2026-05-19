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
    /// Manages content caching for forum content (topics, replies, forums).
    /// </summary>
    internal sealed class ContentCache
    {
        private static readonly int ContentCacheMinutes = 2;

        #region "Do not delete this code"

        // DNN module caching uses "output caching" which doesn't work correctly with this module; in particular, CSS files are not referenced.
        // Until this is resolved, content caching for this module is always enabled.
        // When per-module caching is re-enabled, restore the full logic here and replace `return true` accordingly.
        private static bool IsCachingEnabled(int moduleId)
        {
            return true;

            /*
            if (moduleId < 0)
            {
                return true;
            }
            else
            {
                object isCachingEnabledForModule = DotNetNuke.Common.Utilities.DataCache.GetCache(string.Format(CacheKeys.CacheEnabled, moduleId));
                if (isCachingEnabledForModule == null)
                {
                    DotNetNuke.Entities.Modules.ModuleInfo objModule = new DotNetNuke.Entities.Modules.ModuleController().GetModule(moduleId);
                    isCachingEnabledForModule = (!string.IsNullOrEmpty(objModule.CacheMethod)) && (objModule.CacheTime > 0);
                    DotNetNuke.Common.Utilities.DataCache.SetCache(string.Format(CacheKeys.CacheEnabled, moduleId), isCachingEnabledForModule, DateTime.UtcNow.AddMinutes(ContentCacheMinutes));
                }
                return (bool)isCachingEnabledForModule;
            }
            */
        }

        #endregion

        /// <summary>
        /// Stores content in the cache.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheObj">The object to cache.</param>
        public static void Store(int moduleId, string cacheKey, object cacheObj)
        {
            if (IsCachingEnabled(moduleId))
            {
                DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.StoreInternal(cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(ContentCacheMinutes));
            }
        }

        /// <summary>
        /// Retrieves content from the cache.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns>The cached content, or null if not found or caching is disabled.</returns>
        public static object Retrieve(int moduleId, string cacheKey)
        {
            if (IsCachingEnabled(moduleId))
            {
                return DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.RetrieveInternal(cacheKey);
            }

            return null;
        }

        /// <summary>
        /// Removes content from the cache.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="cacheKey">The cache key.</param>
        public static void Clear(int moduleId, string cacheKey)
        {
            if (IsCachingEnabled(moduleId))
            {
                DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearInternal(cacheKey);
            }
        }

        /// <summary>
        /// Clears all content cache for a specific topic.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="topicId">The topic ID.</param>
        public static void ClearForTopic(int moduleId, int topicId)
        {
            if (IsCachingEnabled(moduleId))
            {
                try
                {
                    Clear(moduleId, string.Format(CacheKeys.TopicInfo, moduleId, topicId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.TopicTrackingInfoPrefix, moduleId, topicId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.ForumViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.TopicViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.TopicsViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.FilteredTopicsPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.ToolbarPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.WhatsNewData, moduleId));
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Clears all content cache for a specific content item.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="contentId">The content ID.</param>
        public static void ClearForContent(int moduleId, int contentId)
        {
            if (IsCachingEnabled(moduleId))
            {
                try
                {
                    Clear(moduleId, string.Format(CacheKeys.ContentInfo, moduleId, contentId));
                    Clear(moduleId, string.Format(CacheKeys.TopicInfoByContentId, moduleId, contentId));
                    Clear(moduleId, string.Format(CacheKeys.ReplyInfoByContentId, moduleId, contentId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.ForumViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.TopicViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.TopicsViewPrefix, moduleId));
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Clears all content cache for a specific reply.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="replyId">The reply ID.</param>
        public static void ClearForReply(int moduleId, int replyId)
        {
            if (IsCachingEnabled(moduleId))
            {
                try
                {
                    Clear(moduleId, string.Format(CacheKeys.ReplyInfo, moduleId, replyId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.ForumViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.TopicViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.TopicsViewPrefix, moduleId));
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Clears all content cache for a specific forum.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="forumId">The forum ID.</param>
        public static void ClearForForum(int moduleId, int forumId)
        {
            if (IsCachingEnabled(moduleId))
            {
                try
                {
                    Clear(moduleId, string.Format(CacheKeys.ForumInfo, moduleId, forumId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.ForumViewPrefix, moduleId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.TopicReadCountPrefix, moduleId, forumId));
                    DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.ForumTrackingInfoPrefix, moduleId, forumId));
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
        }
    }
}
