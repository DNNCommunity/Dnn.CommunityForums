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
    using System.Linq;

    internal class ForumTopicController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ForumTopicInfo>
    {
        private int moduleId = -1;

        internal override string cacheKeyTemplate => CacheKeys.ForumTopicInfo;

        internal ForumTopicController()
        {
        }

        internal ForumTopicController(int moduleId)
        {
            this.moduleId = moduleId;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumTopicInfo GetForumForTopic(int topicId)
        {
            var cachekey = this.GetCacheKey(moduleId: this.moduleId, id: topicId);
            var forumTopic = DataCache.ContentCacheRetrieve(this.moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ForumTopicInfo;
            if (forumTopic == null)
            {
                forumTopic = this.Find("WHERE TopicId = @0", topicId).FirstOrDefault();

                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.moduleId, cachekey, forumTopic);
            }

            return forumTopic;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumTopicInfo GetForForumIdTopicId(int forumId, int topicId)
        {
            return this.Find("WHERE ForumId = @0 AND TopicId = @1", forumId, topicId).FirstOrDefault();
        }

        internal void Update(int forumId, int topicId)
        {
            var forumTopic = this.GetForForumIdTopicId(forumId, topicId);
            if (forumTopic == null)
            {
                forumTopic = new DotNetNuke.Modules.ActiveForums.Entities.ForumTopicInfo {
                    ForumId = forumId,
                    TopicId = topicId,
                    LastReplyId = null,
                };
                this.Insert(forumTopic);
            }

            var replies = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.moduleId).GetByTopicId(topicId);
            if (replies.Any())
            {
                forumTopic.LastReplyId = replies.Max(r => r.ReplyId);
            }
            else
            {
                forumTopic.LastReplyId = null;
            }

            this.Update(forumTopic);
        }

        internal void DeleteForForum(int forumId)
        {
            this.Delete("WHERE ForumId = @0", forumId);
            DataCache.CacheClearPrefix(this.moduleId, CacheKeys.ForumTopicInfoPrefix);
        }
    }
}
