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

    using DotNetNuke.Data;

    internal interface ITopicController : IRepository<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>
    {
        void Restore(int portalId, int moduleId, int forumId, int topicId);

        void DeleteById(int moduleId, int topicId, DotNetNuke.Modules.ActiveForums.Enums.DeleteBehavior deleteBehavior);

        DotNetNuke.Modules.ActiveForums.Entities.TopicInfo FindByURL(int moduleId, int forumId, string topicUrl);

        DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetById(int moduleId, int topicId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum = null);

        DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetByContentId(int moduleId, int contentId);

        int GetAnnouncementsCount(int moduleId, HashSet<int> forumIds);

        bool ShowAnnouncementsLink(int moduleId, HashSet<int> forumIds);

        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetAnnouncements(int moduleId, HashSet<int> forumIds, int pageId, int pageSize);

        int GetMostRepliesCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes);

        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetMostReplies(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize);

        int GetUnresolvedCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes);

        bool ShowUnresolvedLink(int moduleId, HashSet<int> forumIds);

        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetUnresolved(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize);

        int GetUnansweredCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes);

        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetUnanswered(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize);

        int GetActiveTopicsCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes);

        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetActiveTopics(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize);

        int GetMyTopicsCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int authorId);

        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetMyTopics(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize, int authorId);

        int GetMyUnreadTopicsCount(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int authorId);

        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetMyUnreadTopics(int moduleId, HashSet<int> forumIds, int timeFrameMinutes, int pageId, int pageSize, int authorId);

        int GetTaggedTopicsCount(int moduleId, HashSet<int> forumIds, int tagId, int timeFrameMinutes);

        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> GetTaggedTopics(int moduleId, HashSet<int> forumIds, int tagId, int timeFrameMinutes, int pageId, int pageSize);
    }
}
