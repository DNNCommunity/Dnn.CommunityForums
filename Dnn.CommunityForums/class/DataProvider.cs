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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Data;

    public abstract class DataProvider
    {
        private static DataProvider objProvider;

        // constructor
        static DataProvider()
        {
            CreateProvider();
        }

        // dynamically create provider
        private static void CreateProvider()
        {
            objProvider = (DataProvider)Framework.Reflection.CreateObject("data", "DotNetNuke.Modules.ActiveForums", string.Empty);
        }

        // return the provider
        public static new DataProvider Instance()
        {
            return objProvider;
        }

        #region Badges
        public abstract IDataReader Badges_List(int moduleId);
        #endregion

        #region Filters
        public abstract int Filters_Save(int portalId, int moduleId, int filterId, string find, string replace, string filterType);

        public abstract IDataReader Filters_List(int portalId, int moduleId, int pageIndex, int pageSize, string sort, string sortColumn);

        #endregion

        #region Forums
        public abstract void Forums_Move(int moduleId, int forumId, int sortDirection);

        public abstract int Forum_Save(int portalId, int forumId, int moduleId, int forumGroupId, int parentForumId, string forumName, string forumDesc, int sortOrder, bool active, bool hidden, string forumSettingsKey, int permissionsId, string prefixURL, int socialGroupId, bool hasProperties);

        #endregion

        #region Groups
        public abstract void Groups_Move(int moduleId, int forumGroupId, int sortDirection);

        public abstract int Groups_Save(int portalId, int moduleId, int forumGroupId, string groupName, int sortOrder, bool active, bool hidden, int permissionsId, string prefixURL, string groupSettingsKey);
        #endregion

        #region Polls
        public abstract DataSet Poll_Get(int topicId);

        public abstract IDataReader Poll_GetResults(int topicId);

        public abstract void Poll_Option_Save(int pollOptionsId, int pollId, string optionName, int topicId);

        public abstract int Poll_Save(int pollId, int topicId, int userId, string question, string pollType);

        public abstract void Poll_Vote(int pollId, int pollOptionId, string response, string iPAddress, int userId);

        public abstract int Poll_HasVoted(int topicId, int userId);
        #endregion

        #region Profiles

        public abstract void Profiles_UpdateActivity(int PortalId, int UserId);

        public abstract IDataReader Profiles_MemberList(int PortalId, int MaxRows, int RowIndex, string Filter);

        public abstract IDataReader Profiles_GetStats(int PortalId, int Interval);
        #endregion

        #region Moderation
        public abstract DataSet Mod_Pending(int portalId, int moduleId, int forumId, int userId);

        public abstract void Mod_Reject(int portalId, int moduleId, int userId, int forumId, int topicId, int replyId, int reason, string comment);
        #endregion

        #region Ranks
        public abstract int Ranks_Save(int portalId, int moduleId, int rankId, string rankName, int minPosts, int maxPosts, string display);

        public abstract IDataReader Ranks_Get(int portalId, int moduleId, int rankId);

        public abstract IDataReader Ranks_List(int portalId, int moduleId);

        public abstract void Ranks_Delete(int portalId, int moduleId, int rankId);
        #endregion

        #region Replies/Comments
        public abstract int Reply_Save(int portalId, int topicId, int replyId, int replyToId, int statusId, bool isApproved, bool isDeleted, string subject, string body, DateTime dateCreated, DateTime dateUpdated, int authorId, string authorName, string iPAddress);

        public abstract IDataReader Reply_Get(int portalId, int moduleId, int topicId, int replyId);

        public abstract void Reply_UpdateStatus(int portalId, int moduleId, int topicId, int replyId, int userId, int statusId, bool isMod);

        public abstract void Reply_Delete(int forumId, int topicId, int replyId, int delBehavior);
        #endregion

        #region Search
        public abstract IDataReader Search_DotNetNuke(int moduleId, DateTime beginDateUtc);
        #endregion

        #region Subscriptions
        public abstract IDataReader Subscriptions_GetDigest(string subscriptionType, DateTime startDate);

        public abstract IDataReader Subscriptions_GetSubscribers(int portalId, int forumId, int topicId, int mode);

        public abstract int Subscription_Update(int portalId, int moduleId, int forumId, int topicId, int mode, int userId);
        #endregion

        #region Categories
        public abstract IDataReader Categories_List(int portalId, int moduleId, int pageIndex, int pageSize, string sort, string sortColumn, int forumId, int forumGroupId);
        #endregion

        #region Tags
        public abstract IDataReader Tags_List(int PortalId, int ModuleId, int PageIndex, int PageSize, string Sort, string SortColumn);

        public abstract int Tags_Save(int portalId, int moduleId, int tagId, string tagName, int items, int topicId);
        #endregion

        #region Topics
        public abstract void Topics_Delete(int forumId, int topicId, int delBehavior);

        public abstract void Topics_Delete_For_User(int moduleId, int userId, int delBehavior);

        public abstract void Topics_Move(int portalId, int moduleId, int forumId, int topicId);

        public abstract int Topics_Save(int portalId, int moduleId, int topicId, int viewCount, int replyCount, bool isLocked, bool isPinned, string topicIcon, int statusId, bool isApproved, bool isDeleted, bool isAnnounce, bool isArchived, DateTime announceStart, DateTime announceEnd, string subject, string body, string summary, DateTime dateCreated, DateTime dateUpdated, int authorId, string authorName, string iPAddress, int topicType, int topicPriority, string uRL, string topicData);

        public abstract void Replies_Split(int oldTopicId, int newTopicId, string listreplies, DateTime dateUpdated, int firstReplyId);

        public abstract void Topics_UpdateStatus(int portalId, int moduleId, int topicId, int replyId, int topicStatusId, int replyStatusId, int userId);
        #endregion

        #region Content
        public abstract int Content_GetID(int topicId, int? replyId);
        #endregion

        #region Maintenance
        public abstract int Forum_Maintenance(int forumId, int olderThanTimeFrame, int lastActivityTimeFrame, int byUserId, bool withoutReplies, bool testRun, int delBehavior);
        #endregion

        #region Dashboard
        public abstract DataSet Dashboard_Get(int portalId, int moduleId);
        #endregion

        #region UI
        public abstract DataSet UI_TopicsView(int portalId, int moduleId, int forumId, int userId, int pageIndex, int pageSize, bool isSuper, string sortColumn);

        public abstract DataSet UI_TopicView(int portalId, int moduleId, int forumId, int topicId, int userId, int pageIndex, int pageSize, bool isSuper, string sort);
        #endregion

        #region Utility Items
        public abstract void Utility_MarkAllRead(int moduleId, int userId, int forumId);

        public abstract int Utility_GetFirstUnRead(int topicId, int lastReadId);
        #endregion

        #region Top Posts
        public abstract IDataReader PortalForums(int portalId);

        public abstract IDataReader GetPosts(string forums, bool topicsOnly, bool randomOrder, int rows, string tags, int filterByUserId = -1);

        public abstract IDataReader GetPostsByUser(int portalId, int rows, bool isSuperUser, int currentUserId, int filteredUserid, bool topicsOnly, string forumIds);
        #endregion
    }
}
