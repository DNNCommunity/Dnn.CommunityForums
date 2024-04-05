//
// Community Forums
// Copyright (c) 2013-2021
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
using DotNetNuke.Data;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Modules.ActiveForums.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetNuke.Modules.ActiveForums.Controllers 
{
    internal partial class SubscriptionController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo>
    {
        public void Subscribe(int portalId, int moduleId, int userId, int forumId)
        {
            if (!Subscribed(portalId, moduleId, userId, forumId))
            {
                InsertForUser(portalId, moduleId, userId, forumId);
            }
        }
        public void Subscribe(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            if (!Subscribed(portalId, moduleId, userId, forumId, topicId))
            {
                InsertForUser(portalId, moduleId, userId, forumId, topicId);
            }
        }
        public void Unsubscribe(int portalId, int moduleId, int userId, int forumId)
        {
            if (Subscribed(portalId, moduleId, userId, forumId))
            {
                DeleteForUser(portalId, moduleId, userId, forumId);
            }
        }
        public void Unsubscribe(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            if (Subscribed(portalId, moduleId, userId, forumId, topicId))
            {
                DeleteForUser(portalId, moduleId, userId, forumId, topicId);
            }
        }
        public void DeleteForUser(int portalId, int moduleId, int userId, int forumId)
        {
            Delete("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = 0", portalId, moduleId, userId, forumId);
        }
        public void DeleteForUser(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            Delete("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = @4", portalId, moduleId, userId, forumId, topicId);
        }
        public bool Subscribed(int portalId, int moduleId, int userId, int forumId)
        {
            return Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = 0", portalId, moduleId, userId, forumId).Count() == 1;
        }
        public bool Subscribed(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            return Find("WHERE PortalId = @0 AND ModuleId = @1 AND UserId = @2 AND ForumId = @3 AND TopicId = @4", portalId, moduleId, userId, forumId, topicId).Count() == 1;
        }
        public void InsertForUser(int portalId, int moduleId, int userId, int forumId)
        {
            Insert(new DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo
            {
                PortalId = portalId,
                ModuleId = moduleId,
                UserId = userId,
                ForumId = forumId,
                TopicId = 0,
                Mode = 1
            });
        }
        public void InsertForUser(int portalId, int moduleId, int userId, int forumId, int topicId)
        {
            Insert(new DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo
            {
                PortalId = portalId,
                ModuleId = moduleId,
                UserId = userId,
                ForumId = forumId,
                TopicId = topicId,
                Mode = 1
            });
        }
        public int Count(int portalId, int moduleId, int forumId)
        {
            return Count("WHERE PortalId = @0 AND ModuleId = @1 AND ForumId = @2 AND TopicId = 0", portalId, moduleId, forumId);
        }
        public int Count(int portalId, int moduleId, int forumId, int topicId)
        {
            return Count("WHERE PortalId = @0 AND ModuleId = @1 AND ForumId = @2 AND TopicId = @3", portalId, moduleId, forumId, topicId);
        }
    }
}