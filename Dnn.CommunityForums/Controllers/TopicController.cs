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
    using System.Text;
    using System.Web;

    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Modules.ActiveForums.ViewModels;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Services.Social.Notifications;

    internal class TopicController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo, ITopicController, TopicController>, ITopicController
    {
        protected override Func<ITopicController> GetFactory()
        {
            return () => new TopicController();
        }

        public virtual DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetById(int moduleId, int topicId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum = null)
        {
            var cachekey = string.Format(CacheKeys.TopicInfo, moduleId, topicId);
            var ti = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.TopicInfo;
            if (ti == null)
            {
                ti = this._repositoryControllerBase.GetById(topicId);
            }

            if (ti != null)
            {
                ti.ModuleId = moduleId;
                if (forum != null)
                {
                    ti.Forum = forum;
                    ti.PortalId = forum.PortalId;
                }
                else
                {
                    ti.Forum = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetById(moduleId, ti.ForumId);
                    if (ti.Forum != null)
                    {
                        ti.PortalId = ti.Forum.PortalId;
                    }
                }

                ti.GetContent();
                if (ti.Content != null)
                {
                    ti.Author = ti.GetAuthor(ti.PortalId, ti.ModuleId, ti.Content.AuthorId);
                }

                if (ti.LastReply != null)
                {
                    ti.LastReplyAuthor = ti.GetAuthor(ti.PortalId, ti.ModuleId, ti.LastReply.Content.AuthorId);
                }
            }

            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, ti);
            return ti;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetByContentId(int moduleId, int contentId)
        {
            var cachekey = string.Format(CacheKeys.TopicInfoByContentId, moduleId, contentId);
            var ti = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.TopicInfo;
            if (ti == null)
            {
                ti = this._repositoryControllerBase.Find("WHERE ContentId = @0", contentId).FirstOrDefault();
            }

            if (ti != null)
            {
                ti = this.GetById(moduleId, ti.TopicId);
            }

            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, ti);
            return ti;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public int GetAnnouncementsCount(int moduleId, HashSet<int> forumIds)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return 0;
            }

            string cachekey = string.Format(CacheKeys.TopicAnnouncementsCount, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"));
            var topicCount = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as int?;
            if (topicCount == null || !topicCount.HasValue)
            {
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                topicCount = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT COUNT(t.TopicId)
                       FROM {{databaseOwner}}[{{objectQualifier}}activeforums_Topics] t
                       INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_ForumTopics] ft
                           ON ft.TopicId = t.TopicId
                       WHERE ft.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                         AND t.IsAnnounce = 1
                         AND t.AnnounceStart <= GETUTCDATE()
                         AND t.AnnounceEnd >= GETUTCDATE()",
                    forumsIdsList).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topicCount);
            }

            return topicCount.Value;
        }

        public bool ShowAnnouncementsLink(int moduleId, HashSet<int> forumIds)
        {
            return this.GetAnnouncementsCount(moduleId: moduleId, forumIds: forumIds) > 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetAnnouncements(int moduleId, HashSet<int> forumIds, int pageId, int pageSize)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return Enumerable.Empty<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            }

            string cachekey = string.Format(CacheKeys.TopicAnnouncements, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), pageId, pageSize);
            IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> topics = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>;
            if (topics == null)
            {
                {
                    var skip = (pageId - 1) * pageSize;
                    var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                    var topicIds = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                        System.Data.CommandType.Text,
                        $@"SELECT t.TopicId
                           FROM {{databaseOwner}}[{{objectQualifier}}activeforums_Topics] t
                           INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_ForumTopics] ft
                               ON ft.TopicId = t.TopicId
                           WHERE ft.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                             AND t.IsAnnounce = 1
                             AND t.AnnounceStart <= GETUTCDATE()
                             AND t.AnnounceEnd >= GETUTCDATE()
                           ORDER BY t.AnnounceStart DESC
                           OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                        forumsIdsList);
                    topics = topicIds.Where(topicId => topicId.HasValue).Select(topicId => this.GetById(moduleId: moduleId, topicId: topicId.Value));
                }

                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topics);
            }

            return topics;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public int GetMostRepliesCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return 0;
            }

            string cachekey = string.Format(CacheKeys.TopicMostRepliesCount, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), timeFrameMinutes);
            var topicCount = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as int?;
            if (topicCount == null || !topicCount.HasValue)
            {
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                topicCount = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT COUNT(t.TopicId)
                       FROM {{databaseOwner}}[{{objectQualifier}}vw_activeforums_TopicsView] t
                       WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                         AND t.ReplyCount > 0
                         AND (@1=0 OR DATEDIFF(mi, t.LastReplyDate, GETUTCDATE()) <= @1)",
                    forumsIdsList,
                    timeFrameMinutes).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topicCount);
            }

            return topicCount.Value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetMostReplies(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return Enumerable.Empty<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            }

            string cachekey = string.Format(CacheKeys.TopicMostReplies, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), pageId, pageSize, timeFrameMinutes);
            IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> topics = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>;
            if (topics == null)
            {
                var skip = (pageId - 1) * pageSize;
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                var topicIds = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT t.TopicId
                        FROM {{databaseOwner}}[{{objectQualifier}}vw_activeforums_TopicsView] t
                        WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                            AND t.ReplyCount > 0
                            AND (@1=0 OR DATEDIFF(mi, t.LastReplyDate, GETUTCDATE()) <= @1)
                        ORDER BY t.ReplyCount DESC
                        OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                    forumsIdsList,
                    timeFrameMinutes);
                topics = topicIds.Where(topicId => topicId.HasValue).Select(topicId => this.GetById(moduleId: moduleId, topicId: topicId.Value));
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topics);
            }

            return topics;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public int GetUnresolvedCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return 0;
            }

            string cachekey = string.Format(CacheKeys.TopicUnresolvedCount, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), timeFrameMinutes);
            var topicCount = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as int?;
            if (topicCount == null || !topicCount.HasValue)
            {
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                topicCount = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT COUNT(t.TopicId)
                       FROM {{databaseOwner}}[{{objectQualifier}}vw_activeforums_TopicsView] t
                       WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                         AND t.IsLocked = 0
                         AND t.StatusId = 1
                         AND (@1=0 OR DATEDIFF(mi, t.DateCreated, GETUTCDATE()) <= @1)",
                    forumsIdsList,
                    timeFrameMinutes).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topicCount);
            }

            return topicCount.Value;
        }

        public bool ShowUnresolvedLink(int moduleId, HashSet<int> forumIds)
        {
            return this.GetUnresolvedCount(moduleId, forumIds: forumIds, timeFrameMinutes: 0) > 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetUnresolved(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return Enumerable.Empty<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            }

            string cachekey = string.Format(CacheKeys.TopicUnresolved, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), pageId, pageSize, timeFrameMinutes);
            IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> topics = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>;
            if (topics == null)
            {
                var skip = (pageId - 1) * pageSize;
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                var topicIds = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT t.TopicId
                        FROM {{databaseOwner}}[{{objectQualifier}}vw_activeforums_TopicsView] t
                        WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                            AND t.IsLocked = 0
                            AND t.StatusId = 1
                            AND (@1=0 OR DATEDIFF(mi, t.DateCreated, GETUTCDATE()) <= @1)
                        ORDER BY t.DateCreated DESC
                        OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                    forumsIdsList,
                    timeFrameMinutes);
                topics = topicIds.Where(topicId => topicId.HasValue).Select(topicId => this.GetById(moduleId: moduleId, topicId: topicId.Value));
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topics);
            }

            return topics;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public int GetUnansweredCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return 0;
            }

            string cachekey = string.Format(CacheKeys.TopicUnansweredCount, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), timeFrameMinutes);
            var topicCount = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as int?;
            if (topicCount == null || !topicCount.HasValue)
            {
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                topicCount = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT COUNT(t.TopicId)
                       FROM {{databaseOwner}}[{{objectQualifier}}vw_activeforums_TopicsView] t
                       WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                         AND t.LastReplyId = 0
                         AND t.IsLocked = 0
                         AND t.StatusId NOT IN (0, 3)
                         AND (@1=0 OR DATEDIFF(mi, t.DateCreated, GETUTCDATE()) <= @1)",
                    forumsIdsList,
                    timeFrameMinutes).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topicCount);
            }

            return topicCount.Value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetUnanswered(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return Enumerable.Empty<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            }

            string cachekey = string.Format(CacheKeys.TopicUnanswered, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), pageId, pageSize, timeFrameMinutes);
            IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> topics = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>;
            if (topics == null)
            {
                var skip = (pageId - 1) * pageSize;
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                var topicIds = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT t.TopicId
                        FROM {{databaseOwner}}[{{objectQualifier}}vw_activeforums_TopicsView] t
                        WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                            AND t.LastReplyId = 0
                            AND t.IsLocked = 0
                            AND t.StatusId NOT IN (0, 3)
                            AND (@1=0 OR DATEDIFF(mi, t.DateCreated, GETUTCDATE()) <= @1)
                        ORDER BY t.DateCreated DESC
                        OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                    forumsIdsList,
                    timeFrameMinutes);
                topics = topicIds.Where(topicId => topicId.HasValue).Select(topicId => this.GetById(moduleId: moduleId, topicId: topicId.Value));
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topics);
            }

            return topics;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public int GetActiveTopicsCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return 0;
            }

            string cachekey = string.Format(CacheKeys.ActiveTopicsCount, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), timeFrameMinutes);
            var topicCount = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as int?;
            if (topicCount == null || !topicCount.HasValue)
            {
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                topicCount = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT COUNT(t.TopicId)
                       FROM {{databaseOwner}}[{{objectQualifier}}vw_activeforums_TopicsView] t
                       WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                         AND (@1=0 OR DATEDIFF(mi, t.LastReplyDate, GETUTCDATE()) <= @1)",
                    forumsIdsList,
                    timeFrameMinutes).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topicCount);
            }

            return topicCount.Value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetActiveTopics(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return Enumerable.Empty<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            }

            string cachekey = string.Format(CacheKeys.ActiveTopics, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), pageId, pageSize, timeFrameMinutes);
            IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> topics = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>;
            if (topics == null)
            {
                {
                    var skip = (pageId - 1) * pageSize;
                    var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                    if (timeFrameMinutes.Equals(int.MaxValue))
                    {
                        timeFrameMinutes = 0;
                    }

                    var topicIds = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                        System.Data.CommandType.Text,
                        $@"SELECT t.TopicId
                           FROM {{databaseOwner}}[{{objectQualifier}}vw_activeforums_TopicsView] t
                           WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                             AND (@1=0 OR DATEDIFF(mi, t.LastReplyDate, GETUTCDATE()) <= @1)
                           ORDER BY t.LastReplyDate DESC
                           OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                        forumsIdsList,
                        timeFrameMinutes);
                    topics = topicIds.Where(topicId => topicId.HasValue).Select(topicId => this.GetById(moduleId: moduleId, topicId: topicId.Value));
                }

                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topics);
            }

            return topics;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public int GetMyTopicsCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int authorId)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return 0;
            }

            string cachekey = string.Format(CacheKeys.MyTopicsCount, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), timeFrameMinutes, authorId);
            var topicCount = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as int?;
            if (topicCount == null || !topicCount.HasValue)
            {
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                topicCount = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT COUNT(DISTINCT TopicId)
                        FROM (
                            SELECT DISTINCT t.TopicId
                            FROM {{databaseOwner}}[{{objectQualifier}}activeforums_Content] c
                            INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Topics] t
                                ON t.ContentId = c.ContentId
                            INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_ForumTopics] ft
                                ON ft.TopicId = t.TopicId
                            WHERE c.AuthorId = @2
                                AND c.IsDeleted = 0
                                AND t.IsApproved = 1
                                AND ft.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                                AND (@1=0 OR DATEDIFF(mi, c.DateCreated, GETUTCDATE()) <= @1)
                            UNION
                            SELECT DISTINCT r.TopicId
                            FROM {{databaseOwner}}[{{objectQualifier}}activeforums_Content] c
                            INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Replies] r
                                ON r.ContentId = c.ContentId
                            INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Topics] t
                                ON t.TopicId = r.TopicId
                            INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Content] tc
                                ON tc.ContentId = t.ContentId
                            INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_ForumTopics] ft
                                ON ft.TopicId = r.TopicId
                            WHERE c.AuthorId = @2
                                AND c.IsDeleted = 0
                                AND t.IsApproved = 1
                                AND ft.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                                AND (@1=0 OR DATEDIFF(mi, tc.DateCreated, GETUTCDATE()) <= @1)
                        ) AS Topics",
                    forumsIdsList,
                    timeFrameMinutes,
                    authorId).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topicCount);
            }

            return topicCount.Value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetMyTopics(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize, int authorId)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return Enumerable.Empty<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            }

            string cachekey = string.Format(CacheKeys.MyTopics, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), pageId, pageSize, timeFrameMinutes, authorId);
            IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> topics = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>;
            if (topics == null)
            {
                {
                    var skip = (pageId - 1) * pageSize;
                    var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                    if (timeFrameMinutes.Equals(int.MaxValue))
                    {
                        timeFrameMinutes = 0;
                    }

                    var topicIds = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                        System.Data.CommandType.Text,
                        $@"SELECT DISTINCT TopicId
                            FROM (
                                SELECT DISTINCT TopicIds.TopicId, c.DateCreated
                                FROM (
                                    SELECT DISTINCT t.TopicId
                                    FROM {{databaseOwner}}[{{objectQualifier}}activeforums_Content] c
                                    INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Topics] t
                                        ON t.ContentId = c.ContentId
                                    INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_ForumTopics] ft
                                        ON ft.TopicId = t.TopicId
                                    WHERE c.AuthorId = @2
                                        AND c.IsDeleted = 0
                                        AND t.IsApproved = 1
                                        AND ft.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                                        AND (@1=0 OR DATEDIFF(mi, c.DateCreated, GETUTCDATE()) <= @1)
                                    UNION
                                    SELECT DISTINCT r.TopicId
                                    FROM {{databaseOwner}}[{{objectQualifier}}activeforums_Content] c
                                    INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Replies] r
                                        ON r.ContentId = c.ContentId
                                    INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Topics] t
                                        ON t.TopicId = r.TopicId
                                    INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Content] tc
                                        ON tc.ContentId = t.ContentId
                                    INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_ForumTopics] ft
                                        ON ft.TopicId = r.TopicId
                                    WHERE c.AuthorId = @2
                                        AND c.IsDeleted = 0
                                        AND t.IsApproved = 1
                                        AND ft.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                                        AND (@1=0 OR DATEDIFF(mi, tc.DateCreated, GETUTCDATE()) <= @1)
                                ) AS TopicIds
                                INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Topics] t
                                    ON t.TopicId = TopicIds.TopicId
                                INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Content] c
                                    ON c.ContentId = t.ContentId
                                    ORDER BY c.DateCreated DESC
                                    OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY
                            ) AS TopicIds",
                        forumsIdsList,
                        timeFrameMinutes,
                        authorId);
                    topics = topicIds.Where(topicId => topicId.HasValue).Select(topicId => this.GetById(moduleId: moduleId, topicId: topicId.Value));
                }

                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topics);
            }

            return topics;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public int GetMyUnreadTopicsCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int authorId)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return 0;
            }

            string cachekey = string.Format(CacheKeys.TopicUnreadCount, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), timeFrameMinutes, authorId);
            var topicCount = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as int?;
            if (topicCount == null || !topicCount.HasValue)
            {
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                topicCount = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT COUNT(DISTINCT TopicId)
                            FROM (
                                SELECT TopicIds.TopicId, TopicIds.LastReplyDate
                                FROM (
                                    SELECT DISTINCT t.TopicId, t.LastReplyDate
                                    FROM {{databaseOwner}}[{{objectQualifier}}vw_activeforums_TopicsView] t
                                    LEFT OUTER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Forums_Tracking] AS ftr
                                    ON ftr.ForumId = t.ForumId AND ftr.UserId = @2
                                    WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                                        AND (@1=0 OR DATEDIFF(mi, t.DateCreated, GETUTCDATE()) <= @1)
                                        AND (
		                                        (
			                                        t.TopicId NOT IN (SELECT TopicId FROM {{databaseOwner}}[{{objectQualifier}}activeforums_Topics_Tracking] WHERE UserId = @2)
			                                        AND t.TopicId > IsNull(ftr.MaxTopicRead,0)
		                                        )
		                                        OR
		                                        (
			                                        ISNULL(t.LastReplyId, 0) > (SELECT IsNull(MAX(LastReplyId),0) FROM {{databaseOwner}}[{{objectQualifier}}activeforums_Topics_Tracking] WHERE UserId = @2 AND TopicId = t.TopicId)
			                                        AND ISNULL(t.LastReplyId, 0) > IsNull(ftr.MaxReplyRead,0)
		                                        )
	                                        )                                    
                                ) AS TopicIds
                            ) AS TopicIds",
                    forumsIdsList,
                    timeFrameMinutes,
                    authorId).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topicCount);
            }

            return topicCount.Value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetMyUnreadTopics(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize, int authorId)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return Enumerable.Empty<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            }

            string cachekey = string.Format(CacheKeys.TopicUnread, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), pageId, pageSize, timeFrameMinutes, authorId);
            IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> topics = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>;
            if (topics == null)
            {
                {
                    var skip = (pageId - 1) * pageSize;
                    var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                    if (timeFrameMinutes.Equals(int.MaxValue))
                    {
                        timeFrameMinutes = 0;
                    }

                    var topicIds = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                        System.Data.CommandType.Text,
                        $@"SELECT DISTINCT TopicId
                            FROM (
                                SELECT TopicIds.TopicId, TopicIds.LastReplyDate
                                FROM (
                                    SELECT DISTINCT t.TopicId, t.LastReplyDate
                                    FROM {{databaseOwner}}[{{objectQualifier}}vw_activeforums_TopicsView] t
                                    LEFT OUTER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Forums_Tracking] AS ftr
                                    ON ftr.ForumId = t.ForumId AND ftr.UserId = @2
                                    WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                                        AND (@1=0 OR DATEDIFF(mi, t.DateCreated, GETUTCDATE()) <= @1)
                                        AND (
		                                        (
			                                        t.TopicId NOT IN (SELECT TopicId FROM {{databaseOwner}}[{{objectQualifier}}activeforums_Topics_Tracking] WHERE UserId = @2)
			                                        AND t.TopicId > IsNull(ftr.MaxTopicRead,0)
		                                        )
		                                        OR
		                                        (
			                                        ISNULL(t.LastReplyId, 0) > (SELECT IsNull(MAX(LastReplyId),0) FROM {{databaseOwner}}[{{objectQualifier}}activeforums_Topics_Tracking] WHERE UserId = @2 AND TopicId = t.TopicId)
			                                        AND ISNULL(t.LastReplyId, 0) > IsNull(ftr.MaxReplyRead,0)
		                                        )
	                                        )                                    
                                ) AS TopicIds
                                ORDER BY TopicIds.LastReplyDate DESC
                                OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY
                            ) AS TopicIds",
                        forumsIdsList,
                        timeFrameMinutes,
                        authorId);
                    topics = topicIds.Where(topicId => topicId.HasValue).Select(topicId => this.GetById(moduleId: moduleId, topicId: topicId.Value));
                }

                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topics);
            }

            return topics;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public int GetTaggedTopicsCount(int moduleId, HashSet<int> forumIds, int tagId, int timeFrameMinutes)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return 0;
            }

            string cachekey = string.Format(CacheKeys.TaggedTopicsCount, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), tagId, timeFrameMinutes);
            var topicCount = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as int?;
            if (topicCount == null || !topicCount.HasValue)
            {
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                topicCount = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT COUNT(DISTINCT t.TopicId)
                       FROM {{databaseOwner}}[{{objectQualifier}}vw_activeforums_TopicsView] t
                       INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Topics_Tags] tt ON tt.TopicId = t.TopicId
                       WHERE tt.TagId = @2
                         AND t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                         AND (@1=0 OR DATEDIFF(mi, t.DateCreated, GETUTCDATE()) <= @1)",
                    forumsIdsList,
                    timeFrameMinutes,
                    tagId).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topicCount);
            }

            return topicCount.Value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetTaggedTopics(int moduleId, HashSet<int> forumIds, int tagId, int timeFrameMinutes, int pageId, int pageSize)
        {
            if (forumIds == null || forumIds.Count == 0 || tagId <= 0)
            {
                return Enumerable.Empty<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            }

            string cachekey = string.Format(CacheKeys.TaggedTopics, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), tagId, pageId, pageSize, timeFrameMinutes);
            IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> topics = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>;
            if (topics == null)
            {
                var skip = (pageId - 1) * pageSize;
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                var topicIds = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT DISTINCT TopicId
                            FROM (
                                SELECT TopicIds.TopicId, TopicIds.DateCreated
                                FROM (SELECT DISTINCT t.TopicId, t.DateCreated
                                    FROM {{databaseOwner}}[{{objectQualifier}}vw_activeforums_TopicsView] t
                                    INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_Topics_Tags] tt ON tt.TopicId = t.TopicId
                                    WHERE tt.TagId = @2
                                        AND t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                                        AND (@1=0 OR DATEDIFF(mi, t.DateCreated, GETUTCDATE()) <= @1)                                  
                                    ) AS TopicIds
                                    ORDER BY TopicIds.DateCreated DESC
                                    OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY
                                ) AS TopicIds",
                    forumsIdsList,
                    timeFrameMinutes,
                    tagId);
                topics = topicIds.Where(topicId => topicId.HasValue).Select(topicId => this.GetById(moduleId: moduleId, topicId: topicId.Value));
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topics);
            }

            return topics;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo FindByURL(int moduleId, int forumId, string topicUrl)
        {
            string cachekey = string.Format(CacheKeys.TopicByUrl, moduleId, forumId, topicUrl);
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topicInfo = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.TopicInfo;
            if (topicInfo == null)
            {
                if (!string.IsNullOrWhiteSpace(topicUrl))
                {
                    var topicId = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                        System.Data.CommandType.Text,
                        $@"SELECT t.TopicId
                           FROM {{databaseOwner}}[{{objectQualifier}}activeforums_Topics] t
                           INNER JOIN {{databaseOwner}}[{{objectQualifier}}activeforums_ForumTopics] ft
                               ON ft.TopicId = t.TopicId
                           WHERE ft.ForumId = @0
                             AND t.URL_Hash = HASHBYTES('MD5', CONVERT(varbinary(8000), @1))
                             AND t.URL = @1",
                        forumId,
                        topicUrl.Trim()).FirstOrDefault();
                    if (topicId.HasValue)
                    {
                        topicInfo = this.GetById(moduleId: moduleId, topicId: topicId.Value);
                    }
                }

                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topicInfo);
            }

            return topicInfo;
        }

        public static int QuickCreate(int portalId, int moduleId, int forumId, string subject, string body, int userId, string displayName, bool isApproved, string iPAddress)
        {
            var forumInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetById(moduleId, forumId);
            int topicId = -1;
            var ti = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo
            {
                PortalId = portalId,
                ModuleId = moduleId,
                ForumId = forumId,
                AnnounceEnd = Utilities.NullDate(),
                AnnounceStart = Utilities.NullDate(),
                IsAnnounce = false,
                IsApproved = isApproved,
                IsArchived = false,
                IsDeleted = false,
                IsLocked = false,
                IsPinned = false,
                ReplyCount = 0,
                StatusId = -1,
                TopicIcon = string.Empty,
                TopicType = TopicTypes.Topic,
                ViewCount = 0,
                TopicUrl = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildTopicUrlSegment(portalId: portalId, moduleId: moduleId, topicId: topicId, subject: subject, forumInfo: forumInfo),
                Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo()
                {
                    AuthorId = userId,
                    AuthorName = displayName,
                    Subject = subject,
                    Body = body,
                    Summary = string.Empty,
                    IPAddress = iPAddress,
                },
            };

            topicId = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
            if (topicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(moduleId, forumId, topicId);
            }

            return topicId;
        }

        public static void Replies_Split(int moduleId, int oldForumId, int newForumId, int oldTopicId, int newTopicId, string listreplies, bool isNew)
        {
            if (oldTopicId > 0 && newTopicId > 0)
            {
                var pattern = @"^\d+(\|\d+)*$";
                if (RegexUtils.GetCachedRegex(pattern).IsMatch(listreplies))
                {
                    if (isNew)
                    {
                        string[] slistreplies = listreplies.Split("|".ToCharArray(), 2);
                        string str = string.Empty;
                        if (slistreplies.Length > 1)
                        {
                            str = slistreplies[1];
                        }

                        DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Replies_Split(oldTopicId, newTopicId, str, DateTime.Now, Convert.ToInt32(slistreplies[0]));
                    }
                    else
                    {
                        DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Replies_Split(oldTopicId, newTopicId, listreplies, DateTime.Now, 0);
                    }
                }

                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForForum(moduleId, oldForumId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForForum(moduleId, newForumId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForTopic(moduleId, oldTopicId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForTopic(moduleId, newTopicId);
            }
        }

        public static DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Approve(int moduleId, int topicId, int userId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(moduleId, topicId);
            if (ti == null)
            {
                return null;
            }

            ti.IsApproved = true;
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(ti.ModuleId, ti.ForumId, topicId);

            if (ti.Forum.FeatureSettings.ModApproveNotify && ti.Author.AuthorId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(TemplateType.ModApprove, ti.Forum.GetTabId(), ti.Forum, topicId, 0, ti.Author);
            }

            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.QueueApprovedTopicAfterAction(portalId: ti.PortalId, tabId: ti.Forum.GetTabId(), moduleId: ti.Forum.ModuleId, forumGroupId: ti.Forum.ForumGroupId, forumId: ti.ForumId, topicId: topicId, replyId: -1, contentId: ti.ContentId, userId: userId, authorId: ti.Content.AuthorId);
            return ti;
        }

        public static void Move(int moduleId, int userId, int topicId, int newForumId)
        {
            var topic = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(moduleId, topicId);
            var oldForumId = topic.ForumId;
            var moduleSettings = SettingsBase.GetModuleSettings(topic.ModuleId);
            if (moduleSettings.URLRewriteEnabled)
            {
                try
                {
                    if (!string.IsNullOrEmpty(topic.Forum.PrefixURL))
                    {
                        var sURL = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.GetUrl(topic.ModuleId, topic.Forum.ForumGroupId, oldForumId, topicId, -1, -1);
                        if (!string.IsNullOrEmpty(sURL))
                        {
                            DotNetNuke.Modules.ActiveForums.Controllers.UrlController.ArchiveURL(topic.PortalId, topic.Forum.ForumGroupId, newForumId, topicId, sURL);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }

            var oldForum = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetById(moduleId, oldForumId);
            var newForum = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetById(moduleId, newForumId);

            DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_Move(topic.PortalId, topic.ModuleId, newForumId, topicId);
            topic = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(moduleId, topicId);

            if (oldForum.FeatureSettings.ModMoveNotify && topic?.Author?.AuthorId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmail(TemplateType.ModMove, topic.Forum.GetTabId(), oldForum, topicId: topic.TopicId, replyId: -1, author: topic.Author);
            }

            DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Instance.Add(ProcessType.UpdateForumLastUpdated, topic.PortalId, tabId: -1, moduleId: topic.ModuleId, forumGroupId: oldForum.ForumGroupId, forumId: oldForum.ForumID, topicId: topicId, replyId: -1, contentId: topic.ContentId, authorId: topic.Content.AuthorId, userId: userId, badgeId: DotNetNuke.Common.Utilities.Null.NullInteger, dateCreated: DateTime.UtcNow, requestUrl: null);
            DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Instance.Add(ProcessType.UpdateForumLastUpdated, topic.PortalId, tabId: -1, moduleId: topic.ModuleId, forumGroupId: newForum.ForumGroupId, forumId: newForum.ForumID, topicId: topicId, replyId: -1, contentId: topic.ContentId, authorId: topic.Content.AuthorId, userId: userId, badgeId: DotNetNuke.Common.Utilities.Null.NullInteger, dateCreated: DateTime.UtcNow, requestUrl: null);
            Utilities.UpdateModuleLastContentModifiedOnDate(topic.ModuleId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForForum(moduleId, topic.ForumId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForTopic(moduleId, topicId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForContent(topic.ModuleId, topic.ContentId);
        }

        public static void SaveToForum(int moduleId, int forumId, int topicId)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.ForumTopicController.Instance.Update(moduleId: moduleId, forumId: forumId, topicId: topicId);
            Utilities.UpdateModuleLastContentModifiedOnDate(moduleId);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(forumId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForForum(moduleId, forumId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForTopic(moduleId, topicId);
        }

        public static int Save(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic)
        {
            topic.Content.DateUpdated = DateTime.UtcNow;
            if (topic.TopicId < 1)
            {
                topic.Content.DateCreated = DateTime.UtcNow;
            }

            var forum = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetById(moduleId: topic.ModuleId, forumId: topic.ForumId);
            topic.Content.Body = DotNetNuke.Modules.ActiveForums.Controllers.TagController.GetBodyWithTagsProcessed(topic, forum, new Services.URLNavigator().NavigationManager());

            // if editing existing topic, update associated journal item & tags
            if (topic.TopicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.TagController.Instance.UpdateTopicTags(topic);
                Social.UpdateJournalItemForPost(topic);

                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForTopic(topic.ModuleId, topic.TopicId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForContent(topic.ModuleId, topic.ContentId);
            }

            Utilities.UpdateModuleLastContentModifiedOnDate(topic.ModuleId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForForum(topic.ModuleId, topic.ForumId);

            // TODO: convert to use DAL2?
            return Convert.ToInt32(DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_Save(topic.Forum.PortalId, topic.ModuleId, topic.TopicId, topic.ViewCount, topic.ReplyCount, topic.IsLocked, topic.IsPinned, topic.TopicIcon, topic.StatusId, topic.IsApproved, topic.IsDeleted, topic.IsAnnounce, topic.IsArchived, topic.AnnounceStart ?? Utilities.NullDate(), topic.AnnounceEnd ?? Utilities.NullDate(), topic.Content.Subject.Trim(), topic.Content.Body.Trim(), topic.Content.Summary.Trim(), topic.Content.DateCreated, topic.Content.DateUpdated, topic.Content.AuthorId, topic.Content.AuthorName, topic.Content.IPAddress, (int)topic.TopicType, topic.Priority, topic.TopicUrl, topic.TopicData));
        }

        public void Restore(int portalId, int moduleId, int forumId, int topicId)
        {
            var topic = this.GetById(moduleId, topicId);
            topic.IsDeleted = false;
            this._repositoryControllerBase.Update(topic);
            topic.Content.IsDeleted = false;
            DotNetNuke.Modules.ActiveForums.Controllers.ContentController.Instance.Update(topic.Content);
            SaveToForum(topic.ModuleId, forumId, topicId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForForum(topic.ModuleId, topic.ForumId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForTopic(topic.ModuleId, topic.TopicId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForContent(topic.ModuleId, topic.ContentId);
        }

        public void DeleteById(int moduleId, int topicId, DotNetNuke.Modules.ActiveForums.Enums.DeleteBehavior deleteBehavior)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = this.GetById(moduleId, topicId);
            if (ti != null)
            {
                Social.DeleteJournalItemForPost(ti);

                if (ti.Forum.FeatureSettings.IndexContent)
                {
                    var searchDoc = new SearchDocumentToDelete
                    {
                        UniqueKey = $"{ti.ModuleId}-{ti.ContentId}",
                        ModuleId = ti.ModuleId,
                        PortalId = ti.PortalId,
                        SearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId,
                    };
                    DotNetNuke.Data.DataProvider.Instance().AddSearchDeletedItems(searchDoc);
                }

                DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Instance.GetByTopicId(ti.ModuleId, topicId).ForEach(reply =>
                {
                    DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForReply(reply.ModuleId, reply.ReplyId);
                });

                // If it's a hard delete, delete associated attachments
                if (deleteBehavior.Equals(DotNetNuke.Modules.ActiveForums.Enums.DeleteBehavior.Remove))
                {
                    ti.Content.RemoveAttachments();
                }

                DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController.Instance.DeleteForTopic(topicId);
                DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_Delete(ti.ForumId, topicId, (int)deleteBehavior);
                DotNetNuke.Modules.ActiveForums.Controllers.ForumController.UpdateForumLastUpdates(ti.ForumId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForForum(ti.ModuleId, ti.ForumId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForTopic(ti.ModuleId, ti.TopicId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForContent(ti.ModuleId, ti.ContentId);

                Utilities.UpdateModuleLastContentModifiedOnDate(ti.ModuleId);
            }
        }

        internal static bool QueueApprovedTopicAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Instance.Add(ProcessType.ApprovedTopicCreated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, badgeId: DotNetNuke.Common.Utilities.Null.NullInteger, dateCreated: DateTime.UtcNow, requestUrl: HttpContext.Current.Request.Url.ToString());
        }

        internal static bool QueueUnapprovedTopicAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Instance.Add(ProcessType.UnapprovedTopicCreated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, badgeId: DotNetNuke.Common.Utilities.Null.NullInteger, dateCreated: DateTime.UtcNow, requestUrl: HttpContext.Current.Request.Url.ToString());
        }

        internal static bool ProcessApprovedTopicAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId, string requestUrl)
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(moduleId, topicId);
                if (topic == null)
                {
                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    var message = string.Format(Utilities.GetSharedResource("[RESX:UnableToFindTopicToProcess]"), replyId);
                    log.AddProperty("Message", message);
                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                    return true;
                }

                Subscriptions.SendSubscriptions(-1, portalId, moduleId, tabId, topic.Forum, topicId, 0, topic.Content.AuthorId, new Uri(requestUrl));
                Social.AddPostToJournal(topic);

                var pqc = DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Instance;
                pqc.Add(ProcessType.UpdateForumTopicPointers, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, badgeId: DotNetNuke.Common.Utilities.Null.NullInteger, dateCreated: DateTime.UtcNow, requestUrl: requestUrl);
                pqc.Add(ProcessType.UpdateForumLastUpdated, portalId, tabId: tabId, moduleId: moduleId, forumGroupId: forumGroupId, forumId: forumId, topicId: topicId, replyId: replyId, contentId: contentId, authorId: authorId, userId: userId, badgeId: DotNetNuke.Common.Utilities.Null.NullInteger, dateCreated: DateTime.UtcNow, requestUrl: requestUrl);

                Utilities.UpdateModuleLastContentModifiedOnDate(moduleId);

                if (topic.Content.AuthorId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.UpdateUserTopicCount(portalId, topic.Content.AuthorId);
                }

                topic.Content.ExtractEmbeddedImages();
                DotNetNuke.Modules.ActiveForums.Controllers.TagController.Instance.UpdateTopicTags(topic);
                DotNetNuke.Modules.ActiveForums.Controllers.UserMentionController.ProcessUserMentions(topic);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForForum(topic.ModuleId, topic.ForumId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForTopic(topic.ModuleId, topic.TopicId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.ClearForContent(topic.ModuleId, topic.ContentId);

                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }

        internal static bool ProcessUnapprovedTopicAfterAction(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int userId, int authorId, string requestUrl)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.SendModerationNotification(portalId, tabId, moduleId, forumGroupId, forumId, topicId, replyId, authorId, new Uri(requestUrl), new Uri(requestUrl).PathAndQuery);
        }

        internal static bool ProcessTopicPinned(int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId, string requestUrl)
        {
            try
            {
                var topic = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(moduleId, topicId);
                if (topic == null)
                {
                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    var message = string.Format(Utilities.GetSharedResource("[RESX:UnableToFindTopicToProcess]"), contentId, userId);
                    log.AddProperty("Message", message);
                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                    return true;
                }

                if ((bool)topic.Author?.ForumUser?.PinNotificationsEnabled)
                {
                    var subject = Utilities.GetSharedResource("[RESX:PinNotificationSubject]");
                    subject = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTopicTokens(new StringBuilder(subject), topic, topic.Forum.PortalSettings, topic.Forum.MainSettings, new Services.URLNavigator().NavigationManager(), topic.Author.ForumUser, new Uri(requestUrl), new Uri(requestUrl).PathAndQuery).ToString();
                    subject = subject.Length > 400 ? subject.Substring(0, 400) : subject;
                    var body = Utilities.GetSharedResource("[RESX:PinNotificationBody]");
                    body = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTopicTokens(new StringBuilder(body), topic, topic.Forum.PortalSettings, topic.Forum.MainSettings, new Services.URLNavigator().NavigationManager(), topic.Author.ForumUser, new Uri(requestUrl), new Uri(requestUrl).PathAndQuery).ToString();

                    string notificationKey = BuildNotificationContextKey(tabId, moduleId, topicId, userId);

                    NotificationType notificationType = NotificationsController.Instance.GetNotificationType(Globals.PinNotificationType);
                    Notification notification = new Notification
                    {
                        NotificationTypeID = notificationType.NotificationTypeId,
                        Subject = subject,
                        Body = body,
                        IncludeDismissAction = true,
                        SenderUserID = userId,
                        Context = notificationKey,
                    };
                    var users = new List<DotNetNuke.Entities.Users.UserInfo> { topic.Author.ForumUser.UserInfo };
                    NotificationsController.Instance.SendNotification(notification, portalId, null, users);
                }

                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }

        internal static string BuildNotificationContextKey(int tabId, int moduleId, int topicId, int userId)
        {
            return $"{tabId}:{moduleId}:{topicId}:{userId}";
        }
    }
}
