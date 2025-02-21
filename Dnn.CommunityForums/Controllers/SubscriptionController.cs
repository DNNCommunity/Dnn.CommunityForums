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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI.WebControls;

    internal partial class SubscriptionController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo>
    {
        internal void Subscribe(int portalId, int moduleId, int userId, int forumId)
        {
            if (!this.Subscribed(portalId, moduleId, userId, forumId))
            {
                this.InsertForUser(portalId, moduleId, userId, forumId);
            }
        }

        internal void Subscribe(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            if (!this.Subscribed(portalId, moduleId, userId, forumId, topicId))
            {
                this.InsertForUser(portalId, moduleId, userId, forumId, topicId);
            }
        }

        internal void Unsubscribe(int portalId, int moduleId, int userId, int forumId)
        {
            if (this.Subscribed(portalId, moduleId, userId, forumId))
            {
                this.DeleteForUser(portalId, moduleId, userId, forumId);
            }
        }

        internal void Unsubscribe(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            if (this.Subscribed(portalId, moduleId, userId, forumId, topicId))
            {
                this.DeleteForUser(portalId, moduleId, userId, forumId, topicId);
            }
        }

        internal void DeleteForForum(int moduleId, int forumId)
        {
            DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.ForumSubscriberPrefix, moduleId, forumId));
            DotNetNuke.Modules.ActiveForums.DataCache.CacheClearPrefix(moduleId, string.Format(CacheKeys.TopicSubscriberPrefix, moduleId, forumId));
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.ForumSubscriberCount, moduleId, forumId));
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.TopicSubscriberCountPrefix, moduleId, forumId));
            this.Delete("WHERE ForumId = @0", forumId);
        }

        internal bool Subscribed(int portalId, int moduleId, int userId, int forumId)
        {
            var cachekey = string.Format(CacheKeys.ForumSubscriber, moduleId, forumId, userId);
            bool? subscribed = (bool?)DataCache.ContentCacheRetrieve(moduleId, cachekey);
            if (subscribed == null)
            {
                subscribed = this.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = 0", portalId, moduleId, userId, forumId).Count() == 1;
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(moduleId, cachekey, subscribed);
            }

            return (bool)subscribed;
        }

        internal bool Subscribed(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            var cachekey = string.Format(CacheKeys.TopicSubscriber, moduleId, forumId, userId, topicId);
            bool? subscribed = (bool?)DataCache.ContentCacheRetrieve(moduleId, cachekey);
            if (subscribed == null)
            {
                subscribed = this.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = @4", portalId, moduleId, userId, forumId, topicId).Count() == 1;
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(moduleId, cachekey, subscribed);
            }

            return (bool)subscribed;
        }

        internal void InsertForUser(int portalId, int moduleId, int userId, int forumId)
        {
            this.Insert(new DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo
            {
                PortalId = portalId,
                ModuleId = moduleId,
                UserId = userId,
                ForumId = forumId,
                TopicId = 0,
                Mode = 1,
            });
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.ForumSubscriber, moduleId, forumId, userId));
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.ForumSubscriberCount, moduleId, forumId));
        }

        internal void InsertForUser(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            this.Insert(new DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo
            {
                PortalId = portalId,
                ModuleId = moduleId,
                UserId = userId,
                ForumId = forumId,
                TopicId = topicId,
                Mode = 1,
            });
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.TopicSubscriber, moduleId, forumId, topicId, userId));
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.TopicSubscriberCount, moduleId, forumId, topicId));
        }

        internal void DeleteForUser(int portalId, int moduleId, int userId, int forumId)
        {
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.ForumSubscriber, moduleId, forumId, userId));
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.ForumSubscriberCount, moduleId, forumId));
            this.Delete("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = 0", portalId, moduleId, userId, forumId);
        }

        internal void DeleteForUser(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.TopicSubscriber, moduleId, forumId, topicId, userId));
            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheClear(moduleId, string.Format(CacheKeys.TopicSubscriberCount, moduleId, forumId, topicId));
            this.Delete("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = @4", portalId, moduleId, userId, forumId, topicId);
        }

        internal int Count(int portalId, int moduleId, int forumId)
        {
            var cachekey = string.Format(CacheKeys.ForumSubscriberCount, moduleId, forumId);
            var count = (int?)DataCache.ContentCacheRetrieve(moduleId, cachekey);
            if (count == null)
            {
                count = this.Count("WHERE PortalId = @0 AND ModuleId = @1 AND ForumId = @2 AND TopicId = 0", portalId, moduleId, forumId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(moduleId, cachekey, count);
            }

            return (int)count;
        }

        internal int Count(int portalId, int moduleId, int forumId, int topicId)
        {
            var cachekey = string.Format(CacheKeys.TopicSubscriberCount, moduleId, forumId, topicId);
            var count = (int?)DataCache.ContentCacheRetrieve(moduleId, cachekey);
            if (count == null)
            {
                count = this.Count("WHERE PortalId = @0 AND ModuleId = @1 AND ForumId = @2 AND TopicId = @3", portalId, moduleId, forumId, topicId);
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(moduleId, cachekey, count);
            }

            return (int)count;
        }

        internal List<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo> SubscribedForums(int portalId, int moduleId, int userId)
        {
            return this.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId <> 0 AND TopicId = 0", portalId, moduleId, userId).ToList();
        }

        internal List<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo> SubscribedTopics(int portalId, int moduleId, int userId)
        {
            return this.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId <> 0 AND TopicId <> 0", portalId, moduleId, userId).ToList();
        }
    }
}
