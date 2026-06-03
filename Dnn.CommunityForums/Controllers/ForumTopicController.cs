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
    using System.Linq;

    internal class ForumTopicController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.ForumTopicInfo, IForumTopicController, ForumTopicController>, IForumTopicController
    {
        protected override Func<IForumTopicController> GetFactory()
        {
            return () => new ForumTopicController();
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumTopicInfo GetByTopicId(int moduleId, int topicId)
        {
            var cachekey = string.Format(CacheKeys.ForumTopicInfo, moduleId, topicId);
            var forumTopic = DataCache.ContentCacheRetrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ForumTopicInfo;
            if (forumTopic == null)
            {
                forumTopic = this._repositoryControllerBase.Find("WHERE TopicId = @0", topicId).FirstOrDefault();

                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(moduleId, cachekey, forumTopic);
            }

            return forumTopic;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumTopicInfo GetForForumIdTopicId(int forumId, int topicId)
        {
            return this._repositoryControllerBase.Find("WHERE ForumId = @0 AND TopicId = @1", forumId, topicId).FirstOrDefault();
        }

        public void Update(int moduleId, int forumId, int topicId)
        {
            var forumTopic = this.GetForForumIdTopicId(forumId, topicId);
            if (forumTopic == null)
            {
                forumTopic = new DotNetNuke.Modules.ActiveForums.Entities.ForumTopicInfo
                {
                    ForumId = forumId,
                    TopicId = topicId,
                    LastReplyId = null,
                };
                this._repositoryControllerBase.Insert(forumTopic);
            }

            var replies = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Instance.GetByTopicId(moduleId, topicId);
            if (replies.Any())
            {
                forumTopic.LastReplyId = replies.Max(r => r.ReplyId);
            }
            else
            {
                forumTopic.LastReplyId = null;
            }

            this._repositoryControllerBase.Update(forumTopic);
        }

        public void DeleteForForum(int moduleId, int forumId)
        {
            this._repositoryControllerBase.Delete("WHERE ForumId = @0", forumId);
            DataCache.CacheClearPrefix(moduleId, CacheKeys.ForumTopicInfoPrefix);
        }
    }
}
