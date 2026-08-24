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

    using DotNetNuke.Data;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;

    internal class TopicTrackingController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.TopicTrackingInfo, ITopicTrackingController, TopicTrackingController>, ITopicTrackingController
    {
        protected override Func<ITopicTrackingController> GetFactory()
        {
            return () => new TopicTrackingController();
        }

        public DotNetNuke.Modules.ActiveForums.Entities.TopicTrackingInfo GetByUserIdTopicId(int moduleId, int userId, int topicId)
        {
            string cachekey = string.Format(CacheKeys.TopicTrackingInfo, moduleId, topicId, userId);
            DotNetNuke.Modules.ActiveForums.Entities.TopicTrackingInfo topicTrackingInfo = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.TopicTrackingInfo;
            if (topicTrackingInfo == null)
            {
                // this accommodates duplicates which may exist since currently no uniqueness applied in database
                topicTrackingInfo = this._repositoryControllerBase.Find("WHERE UserId = @0 AND TopicId = @1", userId, topicId).OrderBy(t => t.DateAdded).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topicTrackingInfo);
            }

            return topicTrackingInfo;
        }

        public int GetTopicsReadCountForUserForum(int moduleId, int userId, int forumId)
        {
            string cachekey = string.Format(CacheKeys.TopicReadCount, moduleId, forumId, userId);
            var topicReadCount = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(moduleId, cachekey);
            if (topicReadCount == null)
            {
                topicReadCount = DataContext.Instance().ExecuteQuery<int>(
                    System.Data.CommandType.Text,
                    "SELECT COUNT(*) FROM {databaseOwner}{objectQualifier}communityforums_Topics_Tracking tt LEFT OUTER JOIN {databaseOwner}{objectQualifier}communityforums_Topics t ON t.TopicId = tt.TopicId WHERE tt.UserId = @0 AND tt.ForumId = @1 AND t.IsDeleted = 0",
                    userId,
                    forumId).FirstOrDefault();

                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(moduleId, cachekey, topicReadCount);
            }

            return (int)topicReadCount;
        }

        public int GetTopicsReadCountByUser(int moduleId, int userId)
        {
            return DataContext.Instance().ExecuteQuery<int>(
                System.Data.CommandType.Text,
                "SELECT COUNT(*) FROM {databaseOwner}{objectQualifier}communityforums_Topics_Tracking tt WHERE tt.UserId = @0",
                userId).FirstOrDefault();
        }

        public int GetTopicsReadCountByUser(int moduleId, int userId, DateTime minDateTime)
        {
            return DataContext.Instance().ExecuteQuery<int>(
                System.Data.CommandType.Text,
                "SELECT COUNT(*) FROM {databaseOwner}{objectQualifier}communityforums_Topics_Tracking tt WHERE tt.UserId = @0 AND DateAdded IS NOT NULL AND DateAdded >= @1",
                userId,
                minDateTime).FirstOrDefault();
        }
    }
}
