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

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController.GetById()")]
        public abstract IDataReader Filters_Get(int portalId, int moduleId, int filterId);

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController.GetEmoticons()")]
        public abstract IDataReader Filters_GetEmoticons(int moduleId);

        public abstract IDataReader Filters_List(int portalId, int moduleId, int pageIndex, int pageSize, string sort, string sortColumn);

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Not Used")]
        public abstract IDataReader Filters_ListByType(int portalId, int moduleId, string filterType);

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController.Delete()")]
        public abstract void Filters_Delete(int portalId, int moduleId, int filterId);

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController.Delete()")]
        public abstract void Filters_DeleteByModuleId(int portalId, int moduleId);
        #endregion

        #region Forums
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public abstract void Forums_Delete(int portalId, int moduleId, int forumId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public abstract IDataReader Forums_Get(int portalId, int moduleId, int forumID, int userId, bool withSecurity);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public abstract IDataReader Forums_List(int portalId, int moduleId, int forumGroupId, int parentForumId, bool fillLastPost);

        public abstract void Forums_Move(int moduleId, int forumId, int sortDirection);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Forum_Save(int PortalId, int ForumId, int ModuleId, int ForumGroupId, int ParentForumId, string ForumName, string ForumDesc, int SortOrder, bool Active, bool Hidden, string ForumSettingsKey, int PermissionsId, string PrefixURL, int SocialGroupId, bool HasProperties)")]
        public abstract int Forum_Save(int portalId, int forumId, int moduleId, int forumGroupId, int parentForumId, string forumName, string forumDesc, int sortOrder, bool active, bool hidden, string forumSettingsKey, string forumSecurityKey, int permissionsId, string prefixURL, int socialGroupId, bool hasProperties);

        public abstract int Forum_Save(int portalId, int forumId, int moduleId, int forumGroupId, int parentForumId, string forumName, string forumDesc, int sortOrder, bool active, bool hidden, string forumSettingsKey, int permissionsId, string prefixURL, int socialGroupId, bool hasProperties);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract void Forum_ConfigCleanUp(int moduleId, string forumSettingsKey);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Forum_ConfigCleanUp(int ModuleId, string ForumSettingsKey)")]
        public abstract void Forum_ConfigCleanUp(int moduleId, string forumSettingsKey, string forumSecurityKey);
        #endregion

        #region Groups
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public abstract void Groups_Delete(int moduleID, int forumGroupID);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract IDataReader Groups_Get(int moduleId, int forumGroupID);

        public abstract void Groups_Move(int moduleId, int forumGroupId, int sortDirection);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public abstract int Groups_Save(int portalId, int moduleId, int forumGroupId, string groupName, int sortOrder, bool active, bool hidden, int permissionsId, string prefixURL);

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
        #region "Deprecated Methods"
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public abstract void Profiles_Create(int PortalId, int ModuleId, int UserId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public abstract DataSet Profiles_Get(int PortalId, int ModuleId, int UserId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public abstract void Profiles_Save(int PortalId, int ModuleId, int UserId, int TopicCount, int ReplyCount, int ViewCount, int AnswerCount, int RewardPoints, string UserCaption, string Signature, bool SignatureDisabled, int TrustLevel, bool AdminWatch, bool AttachDisabled, string Avatar, int AvatarType, bool AvatarDisabled, string PrefDefaultSort, bool PrefDefaultShowReplies, bool PrefJumpLastPost, bool PrefTopicSubscribe, int PrefSubscriptionType, bool PrefBlockAvatars, bool PrefBlockSignatures, int PrefPageSize);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public abstract IDataReader Profiles_GetUsersOnline(int PortalId, int ModuleId, int Interval);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public abstract void Profiles_UpdateActivity(int PortalId, int ModuleId, int UserId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public abstract IDataReader Profiles_MemberList(int PortalId, int ModuleId, int MaxRows, int RowIndex, string Filter);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public abstract IDataReader Profiles_GetStats(int PortalId, int ModuleId, int Interval);

        #endregion
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
        public abstract DataSet Search(int portalId, int moduleId, int userId, int searchId, int rowIndex, int maxRows, string searchString, int matchType, int searchField, int timespan, int authorId, string author, string forums, string tags, int resultType, int sort, int maxCacheHours, bool fullText);

        public abstract int Search_ManageFullText(bool enabled);

        public abstract int Search_GetFullTextStatus();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Using Search_DotNetNuke(int moduleId, DateTime beginDateUtc)")]
        public abstract IDataReader Search_DotNetNuke(int moduleId);

        public abstract IDataReader Search_DotNetNuke(int moduleId, DateTime beginDateUtc);
        #endregion

        #region Security
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Obsoleted by activeforums_Permissions")]
        public abstract void Security_Delete(int securedId, int objectId, int secureAction, int secureType, int objectType);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Obsoleted by activeforums_Permissions")]
        public abstract IDataReader Security_Get(int securedId, int objectId, int secureType);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Obsoleted by activeforums_Permissions")]
        public abstract IDataReader Security_GetByKey(string securityKey);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Obsoleted by activeforums_Permissions")]
        public abstract IDataReader Security_GetByUser(int portalId, int forumId, int userId, bool isSuperUser);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Obsoleted by activeforums_Permissions")]
        public abstract void Security_Save(int securedId, int objectId, string secureAction, bool secureActionValue, int secureType, string objectName, int objectType, string securityKey);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Obsoleted by activeforums_Permissions")]
        public abstract IDataReader Security_SearchObjects(int portalId, string search);
        #endregion

        #region Settings
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No Longer Used.")]
        public abstract IDataReader Settings_List(int moduleId, string groupKey);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No Longer Used.")]
        public abstract IDataReader Settings_ListAll(int moduleId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No Longer Used.")]
        public abstract string Settings_Get(int moduleId, string groupKey, string settingName);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No Longer Used.")]
        public abstract void Settings_Delete(int moduleId, string groupKey, string settingName);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No Longer Used.")]
        public abstract void Settings_Save(int moduleId, string groupKey, string settingName, string settingValue);
        #endregion

        #region Subscriptions
        public abstract IDataReader Subscriptions_GetDigest(string subscriptionType, DateTime startDate);

        public abstract IDataReader Subscriptions_GetSubscribers(int portalId, int forumId, int topicId, int mode);

        public abstract int Subscription_Update(int portalId, int moduleId, int forumId, int topicId, int mode, int userId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract int Subscriptions_IsSubscribed(int portalId, int moduleId, int forumId, int topicId, int mode, int userId);
        #endregion

        #region Categories
        public abstract IDataReader Categories_List(int portalId, int moduleId, int pageIndex, int pageSize, string sort, string sortColumn, int forumId, int forumGroupId);
        #endregion

        #region Tags
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract void Tags_Delete(int portalId, int moduleId, int tagId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract void Tags_DeleteByTopicId(int portalId, int moduleId, int topicId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract IDataReader Tags_Get(int portalId, int moduleId, int tagId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract IDataReader Tags_List(int PortalId, int ModuleId, bool IsCategory, int PageIndex, int PageSize, string Sort, string SortColumn, int ForumId, int ForumGroupId);

        public abstract IDataReader Tags_List(int PortalId, int ModuleId, int PageIndex, int PageSize, string Sort, string SortColumn);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract int Tags_Save(int portalId, int moduleId, int tagId, string tagName, int clicks, int items, int priority, int topicId, int forumId, int forumGroupId);

        public abstract int Tags_Save(int portalId, int moduleId, int tagId, string tagName, int items, int topicId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract IDataReader Tags_Search(int portalId, int moduleId, string search);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract void Tags_AddTopicToCategory(int portalId, int moduleId, int tagId, int topicId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract void Tags_DeleteTopicToCategory(int portalId, int moduleId, int tagId, int topicId);
        #endregion

        #region Templates
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public abstract void Templates_Delete(int templateId, int portalId, int moduleId);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public abstract IDataReader Templates_Get(int templateId, int portalId, int moduleId);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public abstract IDataReader Templates_List(int portalId, int moduleId, int templateType);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public abstract IDataReader Templates_List(int portalId, int moduleId, int templateType, int rowIndex, int pageSize);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public abstract int Templates_Save(int templateId, int portalId, int moduleId, int templateType, bool isSystem, string title, string subject, string template);
        #endregion

        #region Topics
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController")]
        public abstract int Topics_AddRating(int topicId, int userID, int rating, string comments, string iPAddress);

        public abstract void Topics_Delete(int forumId, int topicId, int delBehavior);

        public abstract void Topics_Delete_For_User(int moduleId, int userId, int delBehavior);

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.GetById(int TopicId)")]
        public abstract IDataReader Topics_Get(int portalId, int moduleId, int topicId, int forumId, int userId, bool withSecurity);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController")]
        public abstract int Topics_GetRating(int topicId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract IDataReader Topics_List(int forumId, int portalId, int moduleId);

        public abstract void Topics_Move(int portalId, int moduleId, int forumId, int topicId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract IDataReader Topics_Replies(int topicId);
        
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract int Topics_Save(int portalId, int topicId, int viewCount, int replyCount, bool isLocked, bool isPinned, string topicIcon, int statusId, bool isApproved, bool isDeleted, bool isAnnounce, bool isArchived, DateTime announceStart, DateTime announceEnd, string subject, string body, string summary, DateTime dateCreated, DateTime dateUpdated, int authorId, string authorName, string iPAddress, int topicType, int topicPriority, string uRL, string topicData);

        public abstract int Topics_Save(int portalId, int moduleId, int topicId, int viewCount, int replyCount, bool isLocked, bool isPinned, string topicIcon, int statusId, bool isApproved, bool isDeleted, bool isAnnounce, bool isArchived, DateTime announceStart, DateTime announceEnd, string subject, string body, string summary, DateTime dateCreated, DateTime dateUpdated, int authorId, string authorName, string iPAddress, int topicType, int topicPriority, string uRL, string topicData);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract int Topics_SaveToForum(int forumId, int topicId, int lastReplyId);

        public abstract void Replies_Split(int oldTopicId, int newTopicId, string listreplies, DateTime dateUpdated, int firstReplyId);

        public abstract void Topics_UpdateStatus(int portalId, int moduleId, int topicId, int replyId, int topicStatusId, int replyStatusId, int userId);
        #endregion

        #region Content
        public abstract int Content_GetID(int topicId, int? replyId);
        #endregion

        #region MailQueue

        [Obsolete(message: "Deprecated in Community Forums. Scheduled removal in v9.0.0.0. No longer used.")]
        public abstract IDataReader Queue_List();

        [Obsolete(message: "Deprecated in Community Forums. Scheduled removal in v9.0.0.0. No longer used.")]
        public abstract void Queue_Delete(int emailId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract void Queue_Add(string emailFrom, string emailTo, string emailSubject, string emailBody, string emailBodyPlainText, string emailCC, string emailBCC);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract void Queue_Add(int portalId, string emailFrom, string emailTo, string emailSubject, string emailBody, string emailBodyPlainText, string emailCC, string emailBCC);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public abstract void Queue_Add(int portalId, string emailFrom, string emailTo, string emailSubject, string emailBody, string emailCC, string emailBCC);
        #endregion

        #region Maintenance
        public abstract int Forum_Maintenance(int forumId, int olderThanTimeFrame, int lastActivityTimeFrame, int byUserId, bool withoutReplies, bool testRun, int delBehavior);
        #endregion

        #region Dashboard
        public abstract DataSet Dashboard_Get(int portalId, int moduleId);
        #endregion

        #region UI
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No Longer Used.")]
        public abstract DataSet UI_ForumView(int portalId, int moduleId, int userId, bool isSuper, string forumIds);

        public abstract DataSet UI_TopicsView(int portalId, int moduleId, int forumId, int userId, int pageIndex, int pageSize, bool isSuper, string sortColumn);

        public abstract DataSet UI_TopicView(int portalId, int moduleId, int forumId, int topicId, int userId, int pageIndex, int pageSize, bool isSuper, string sort);

        public abstract DataSet UI_NotReadView(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, bool isSuper);

        public abstract DataSet UI_UnansweredView(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, bool isSuper);

        public abstract DataSet UI_MyTopicsView(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, bool isSuper);

        public abstract DataSet UI_ActiveView(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, bool isSuper, int timeFrame);

        public abstract DataSet UI_MostLiked(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, bool isSuper, int timeFrame);

        public abstract DataSet UI_MostReplies(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, bool isSuper, int timeFrame);

        public abstract DataSet UI_Announcements(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, bool isSuper);

        public abstract DataSet UI_Unresolved(int portalId, int moduleId, int userId, int rowIndex, int maxRows, string sort, bool isSuper);
        #endregion

        #region Utility Items
        public abstract void Utility_MarkAllRead(int moduleId, int userId, int forumId);

        public abstract int Utility_GetFirstUnRead(int topicId, int lastReadId);
        #endregion

        #region Top Posts
        public abstract IDataReader PortalForums(int portalId);

        public abstract IDataReader GetPosts(int portalId, string forums, bool topicsOnly, bool randomOrder, int rows, string tags, int filterByUserId = -1);

        public abstract IDataReader GetPostsByUser(int portalId, int rows, bool isSuperUser, int currentUserId, int filteredUserid, bool topicsOnly, string forumIds);
        #endregion
    }
}
