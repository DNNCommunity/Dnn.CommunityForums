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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.ActiveForums.Services.Cache;

    internal partial class SubscriptionController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo, ISubscriptionController, SubscriptionController>, ISubscriptionController
    {
        protected override Func<ISubscriptionController> GetFactory()
        {
            return () => new SubscriptionController();
        }

        public void Subscribe(int portalId, int moduleId, int userId, int forumId)
        {
            if (!this.Subscribed(portalId, moduleId, userId, forumId))
            {
                this.InsertForUser(portalId, moduleId, userId, forumId);
            }
        }

        public void Subscribe(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            if (!this.Subscribed(portalId, moduleId, userId, forumId, topicId))
            {
                this.InsertForUser(portalId, moduleId, userId, forumId, topicId);
            }
        }

        public void Unsubscribe(int portalId, int moduleId, int userId, int forumId)
        {
            if (this.Subscribed(portalId, moduleId, userId, forumId))
            {
                this.DeleteForUser(portalId, moduleId, userId, forumId);
            }
        }

        public void Unsubscribe(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            if (this.Subscribed(portalId, moduleId, userId, forumId, topicId))
            {
                this.DeleteForUser(portalId, moduleId, userId, forumId, topicId);
            }
        }

        public void DeleteForForum(int moduleId, int forumId)
        {
            
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(moduleId, string.Format(CacheKeys.ForumSubscriberCount, moduleId, forumId));
            DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.ForumSubscriberPrefix, moduleId, forumId));
            DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.TopicSubscriberPrefix, moduleId, forumId));
            DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.CacheClearPrefix(string.Format(CacheKeys.TopicSubscriberCountPrefix, moduleId, forumId));
            this._repositoryControllerBase.Delete("WHERE ForumId = @0", forumId);
        }

        public bool Subscribed(int portalId, int moduleId, int userId, int forumId)
        {
            var cachekey = string.Format(CacheKeys.ForumSubscriber, moduleId, forumId, userId);
            var cached = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<bool>;

            if (cached == null)
            {
                var subscribed = this._repositoryControllerBase.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = 0", portalId, moduleId, userId, forumId).ToList().Count() == 1;
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, new DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<bool>(subscribed));
                return subscribed;
            }

            return cached.HasValue ? cached.Value : false;
        }

        public bool Subscribed(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            var cachekey = string.Format(CacheKeys.ForumSubscriber, moduleId, forumId, userId);
            var cached = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<bool>;

            if (cached == null)
            {
                var subscribed = this._repositoryControllerBase.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = @4", portalId, moduleId, userId, forumId, topicId).ToList().Count() == 1;
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, new DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<bool>(subscribed));
                return subscribed;
            }

            return cached.HasValue ? cached.Value : false;
        }

        public void InsertForUser(int portalId, int moduleId, int userId, int forumId)
        {
            this._repositoryControllerBase.Insert(new DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo
            {
                PortalId = portalId,
                ModuleId = moduleId,
                UserId = userId,
                ForumId = forumId,
                TopicId = 0,
                Mode = 1,
            });
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(moduleId, string.Format(CacheKeys.ForumSubscriber, moduleId, forumId, userId));
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(moduleId, string.Format(CacheKeys.ForumSubscriberCount, moduleId, forumId));
        }

        public void InsertForUser(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            this._repositoryControllerBase.Insert(new DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo
            {
                PortalId = portalId,
                ModuleId = moduleId,
                UserId = userId,
                ForumId = forumId,
                TopicId = topicId,
                Mode = 1,
            });
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(moduleId, string.Format(CacheKeys.TopicSubscriber, moduleId, forumId, topicId, userId));
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(moduleId, string.Format(CacheKeys.TopicSubscriberCount, moduleId, forumId, topicId));
        }

        public void DeleteForUser(int portalId, int moduleId, int userId, int forumId)
        {
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(moduleId, string.Format(CacheKeys.ForumSubscriber, moduleId, forumId, userId));
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(moduleId, string.Format(CacheKeys.ForumSubscriberCount, moduleId, forumId));
            this._repositoryControllerBase.Delete("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = 0", portalId, moduleId, userId, forumId);
        }

        public void DeleteForUser(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(moduleId, string.Format(CacheKeys.TopicSubscriber, moduleId, forumId, topicId, userId));
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Clear(moduleId, string.Format(CacheKeys.TopicSubscriberCount, moduleId, forumId, topicId));
            this._repositoryControllerBase.Delete("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = @4", portalId, moduleId, userId, forumId, topicId);
        }

        public int Count(int portalId, int moduleId, int forumId)
        {
            var cachekey = string.Format(CacheKeys.ForumSubscriberCount, moduleId, forumId);
            var cached = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<int>;
            if (cached == null)
            {
                int count = this._repositoryControllerBase.Count("WHERE PortalId = @0 AND ModuleId = @1 AND ForumId = @2 AND TopicId = 0", portalId, moduleId, forumId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, new DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<int>(count));
                return count;
            }

            return cached.HasValue ? cached.Value : 0;
        }

        public int Count(int portalId, int moduleId, int forumId, int topicId)
        {
            var cachekey = string.Format(CacheKeys.TopicSubscriberCount, moduleId, forumId, topicId);
            var cached = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<int>;

            if (cached == null)
            {
                int count = this._repositoryControllerBase.Count("WHERE PortalId = @0 AND ModuleId = @1 AND ForumId = @2 AND TopicId = @3", portalId, moduleId, forumId, topicId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, new DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<int>(count));
                return count;
            }

            return cached.HasValue ? (int)cached.Value : 0;
        }

        public List<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo> SubscribedForums(int portalId, int moduleId, int userId)
        {
            return this._repositoryControllerBase.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId <> 0 AND TopicId = 0", portalId, moduleId, userId).ToList();
        }

        public List<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo> SubscribedTopics(int portalId, int moduleId, int userId)
        {
            return this._repositoryControllerBase.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId <> 0 AND TopicId <> 0", portalId, moduleId, userId).ToList();
        }
    }
}
