// Copyright (c) 2013-2024 by DNN Community
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

    using DotNetNuke.Data;
    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Modules.ActiveForums.Entities;

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

        internal void DeleteForForum(int forumId)
        {
            this.Delete("WHERE ForumId = @0", forumId);
        }

        internal void DeleteForUser(int portalId, int moduleId, int userId, int forumId)
        {
            this.Delete("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = 0", portalId, moduleId, userId, forumId);
        }

        internal void DeleteForUser(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            this.Delete("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = @4", portalId, moduleId, userId, forumId, topicId);
        }

        internal bool Subscribed(int portalId, int moduleId, int userId, int forumId)
        {
            return this.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = 0", portalId, moduleId, userId, forumId).Count() == 1;
        }

        internal bool Subscribed(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            return this.Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = @4", portalId, moduleId, userId, forumId, topicId).Count() == 1;
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
        }

        internal int Count(int portalId, int moduleId, int forumId)
        {
            return this.Count("WHERE PortalId = @0 AND ModuleId = @1 AND ForumId = @2 AND TopicId = 0", portalId, moduleId, forumId);
        }

        internal int Count(int portalId, int moduleId, int forumId, int topicId)
        {
            return this.Count("WHERE PortalId = @0 AND ModuleId = @1 AND ForumId = @2 AND TopicId = @3", portalId, moduleId, forumId, topicId);
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
