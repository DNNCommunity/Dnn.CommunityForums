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

    using DotNetNuke.Data;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;

    internal class ContentController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.ContentInfo, IContentController, ContentController>, IContentController
    {
        protected override Func<IContentController> GetFactory()
        {
            return () => new ContentController();
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo GetById(int moduleId, int contentId)
        {
            var cachekey = string.Format(CacheKeys.ContentInfo, moduleId, contentId);
            DotNetNuke.Modules.ActiveForums.Entities.ContentInfo content = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ContentInfo;
            if (content == null)
            {
                content = this._repositoryControllerBase.GetById(contentId);
                if (moduleId.Equals(-1) && !content.Equals(null))
                {
                    content.UpdateCache();
                }
                else
                {
                    DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, content);
                }
            }

            return content;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo Save<TProperty>(DotNetNuke.Modules.ActiveForums.Entities.ContentInfo item, TProperty id)
        {
            if (id == null || id.Equals(0) || id.Equals(-1) || this._repositoryControllerBase.GetById(id) == null)
            {
                this._repositoryControllerBase.Insert(item);
            }
            else
            {
                this._repositoryControllerBase.Update(item);
            }

            return item;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public static int GetMostLikesCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return 0;
            }

            string cachekey = string.Format(CacheKeys.MostLikesCount, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), timeFrameMinutes);
            var postCount = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as int?;
            if (postCount == null || !postCount.HasValue)
            {
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                postCount = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(
                    System.Data.CommandType.Text,
                    $@"SELECT COUNT(DISTINCT ContentId)
                        FROM (
                            SELECT DISTINCT t.TopicId, 0 AS ReplyId, c.ContentId, ISNULL(l.LikeCount, 0) AS LikeCount, t.LastReplyDate
                            FROM {{databaseOwner}}[{{objectQualifier}}communityforums_Content] c
                            INNER JOIN {{databaseOwner}}[{{objectQualifier}}communityforums_Topics] t1
                                ON t1.ContentId = c.ContentId
                            INNER JOIN {{databaseOwner}}[{{objectQualifier}}vw_communityforums_TopicsView] t
                                ON t.TopicId = t1.TopicId
                            INNER JOIN (SELECT PostId AS ContentId, COUNT(Id) AS LikeCount FROM {{databaseOwner}}[{{objectQualifier}}communityforums_Likes] WHERE Checked = 1 GROUP BY PostId) l ON l.ContentId = c.ContentId
                            WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                                AND (@1=0 OR DATEDIFF(mi, t.LastReplyDate, GETUTCDATE()) <= @1)
                            UNION
                            SELECT DISTINCT r.TopicId, r.ReplyId, r.ContentId, ISNULL(l.LikeCount, 0) AS LikeCount, t.LastReplyDate
                            FROM {{databaseOwner}}[{{objectQualifier}}communityforums_Content] c
                            INNER JOIN {{databaseOwner}}[{{objectQualifier}}communityforums_Replies] r
                                ON r.ContentId = c.ContentId
                            INNER JOIN {{databaseOwner}}[{{objectQualifier}}vw_communityforums_TopicsView] t
                                ON t.TopicId = r.TopicId
                            INNER JOIN (SELECT PostId AS ContentId, COUNT(Id) AS LikeCount FROM {{databaseOwner}}[{{objectQualifier}}communityforums_Likes] WHERE Checked = 1 GROUP BY PostId) l ON l.ContentId = c.ContentId
                            WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                                AND (@1=0 OR DATEDIFF(mi, t.LastReplyDate, GETUTCDATE()) <= @1)
                        ) AS ContentIds",
                    forumsIdsList,
                    timeFrameMinutes).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, postCount);
            }

            return postCount.Value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public static IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.IPostInfo> GetMostLikes(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize)
        {
            if (forumIds == null || forumIds.Count == 0)
            {
                return Enumerable.Empty<DotNetNuke.Modules.ActiveForums.Entities.IPostInfo>();
            }

            string cachekey = string.Format(CacheKeys.MostLikes, moduleId, forumIds.FromHashSetToDelimitedString<int>(";"), pageId, pageSize, timeFrameMinutes);
            IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.IPostInfo> posts = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.IPostInfo>;
            if (posts == null)
            {
                var skip = (pageId - 1) * pageSize;
                var forumsIdsList = forumIds.FromHashSetToDelimitedString<int>(",");
                if (timeFrameMinutes.Equals(int.MaxValue))
                {
                    timeFrameMinutes = 0;
                }

                var postInfo = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<PostIdResult>(
                    System.Data.CommandType.Text,
                    $@"SELECT ContentId, TopicId, ReplyId
                        FROM (
                            SELECT DISTINCT ContentIds.ContentId, ContentIds.TopicId, ContentIds.ReplyId, ContentIds.LikeCount
                            FROM (
                                SELECT DISTINCT t.TopicId, NULL AS ReplyId, c.ContentId, ISNULL(l.LikeCount, 0) AS LikeCount, t.LastReplyDate
                                FROM {{databaseOwner}}[{{objectQualifier}}communityforums_Content] c
                                INNER JOIN {{databaseOwner}}[{{objectQualifier}}communityforums_Topics] t1
                                    ON t1.ContentId = c.ContentId
                                INNER JOIN {{databaseOwner}}[{{objectQualifier}}vw_communityforums_TopicsView] t
                                    ON t.TopicId = t1.TopicId
                                INNER JOIN (SELECT PostId AS ContentId, COUNT(Id) AS LikeCount FROM {{databaseOwner}}[{{objectQualifier}}communityforums_Likes] WHERE Checked = 1 GROUP BY PostId) l ON l.ContentId = c.ContentId
                                WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                                    AND (@1=0 OR DATEDIFF(mi, t.LastReplyDate, GETUTCDATE()) <= @1)
                                UNION
                                SELECT DISTINCT r.TopicId, r.ReplyId, r.ContentId, ISNULL(l.LikeCount, 0) AS LikeCount, t.LastReplyDate
                                FROM {{databaseOwner}}[{{objectQualifier}}communityforums_Content] c
                                INNER JOIN {{databaseOwner}}[{{objectQualifier}}communityforums_Replies] r
                                    ON r.ContentId = c.ContentId
                                INNER JOIN {{databaseOwner}}[{{objectQualifier}}vw_communityforums_TopicsView] t
                                    ON t.TopicId = r.TopicId
                                INNER JOIN (SELECT PostId AS ContentId, COUNT(Id) AS LikeCount FROM {{databaseOwner}}[{{objectQualifier}}communityforums_Likes] WHERE Checked = 1 GROUP BY PostId) l ON l.ContentId = c.ContentId
                                WHERE t.ForumId IN (SELECT value FROM STRING_SPLIT(@0, ','))
                                    AND (@1=0 OR DATEDIFF(mi, t.LastReplyDate, GETUTCDATE()) <= @1)
                            ) AS ContentIds
                            INNER JOIN {{databaseOwner}}[{{objectQualifier}}vw_communityforums_TopicsView] t
                                ON t.TopicId = ContentIds.TopicId
                                ORDER BY ContentIds.LikeCount DESC
                                OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY
                        ) AS ContentIds",
                    forumsIdsList,
                    timeFrameMinutes);

                posts = postInfo.Where(postinfo => postinfo.ContentId.HasValue).Select(post =>
                {
                    var topic = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(moduleId, post.TopicId.Value);
                    if (!post.ReplyId.HasValue)
                    {
                        return (DotNetNuke.Modules.ActiveForums.Entities.IPostInfo)topic;
                    }

                    return (DotNetNuke.Modules.ActiveForums.Entities.IPostInfo)DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Instance.GetById(moduleId: moduleId, replyId: post.ReplyId.Value, topic: topic);
                });

                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, posts);
            }

            return posts;
        }

        private class PostIdResult
        {
            public int? ContentId { get; set; }

            public int? TopicId  { get; set; }

            public int? ReplyId  { get; set; }
        }
    }
}
