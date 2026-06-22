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

    using DotNetNuke.Common.Utilities;

    public class SqlDataProvider : DataProvider
    {
        public override DataSet Dashboard_Get(int PortalId, int ModuleId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteDataSet("activeforums_DashBoard_Stats", PortalId, ModuleId);
        }

        public override IDataReader Badges_List(int ModuleId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_Badges_List", ModuleId);
        }

        public override IDataReader Filters_List(int PortalId, int ModuleId, int PageIndex, int PageSize, string Sort, string SortColumn)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_Filters_List", PortalId, ModuleId, PageIndex, PageSize, Sort, SortColumn);
        }

        public override int Filters_Save(int PortalId, int ModuleId, int FilterId, string Find, string Replace, string FilterType)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Filters_Save", PortalId, ModuleId, FilterId, Find, Replace, FilterType);
        }

        public override void Forums_Move(int ModuleId, int ForumId, int SortDirection)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Forums_MoveForum", ModuleId, ForumId, SortDirection);
        }

        public override int Forum_Save(int PortalId, int ForumId, int ModuleId, int ForumGroupId, int ParentForumId, string ForumName, string ForumDesc, int SortOrder, bool Active, bool Hidden, string ForumSettingsKey, int PermissionsId, string PrefixURL, int SocialGroupId, bool HasProperties)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Forum_Save", PortalId, ForumId, ModuleId, ForumGroupId, ParentForumId, ForumName, ForumDesc, SortOrder, Active, Hidden, ForumSettingsKey, PermissionsId, PrefixURL, SocialGroupId, HasProperties);
        }

        public override void Groups_Move(int ModuleId, int ForumGroupId, int SortDirection)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Groups_MoveGroup", ModuleId, ForumGroupId, SortDirection);
        }

        public override int Groups_Save(int PortalId, int ModuleId, int ForumGroupId, string GroupName, int SortOrder, bool Active, bool Hidden, int PermissionsId, string PrefixURL, string GroupSettingsKey)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Groups_Save", PortalId, ModuleId, ForumGroupId, GroupName, SortOrder, Active, Hidden, PermissionsId, PrefixURL, GroupSettingsKey);
        }

        public override DataSet Poll_Get(int TopicId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteDataSet("activeforums_Poll_Get", TopicId);
        }

        public override IDataReader Poll_GetResults(int TopicId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_Poll_GetResults", TopicId);
        }

        public override int Poll_HasVoted(int TopicId, int UserId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Poll_HasVoted", TopicId, UserId);
        }

        public override void Poll_Option_Save(int PollOptionsId, int PollId, string OptionName, int TopicId)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Poll_Options_Save", PollOptionsId, PollId, OptionName, TopicId);
        }

        public override int Poll_Save(int PollId, int TopicId, int UserId, string Question, string PollType)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Poll_Save", PollId, TopicId, UserId, Question, PollType);
        }

        public override void Poll_Vote(int PollId, int PollOptionId, string Response, string IPAddress, int UserId)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Poll_Vote", PollId, PollOptionId, Response, IPAddress, UserId);
        }

        public override DataSet Mod_Pending(int PortalId, int ModuleId, int ForumId, int UserId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteDataSet("activeforums_Mod_Pending", PortalId, ModuleId, ForumId, UserId);
        }

        public override void Mod_Reject(int PortalId, int ModuleId, int UserId, int ForumId, int TopicId, int ReplyId, int Reason, string Comment)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Mod_Reject", PortalId, ModuleId, UserId, ForumId, TopicId, ReplyId, Reason, Comment);
        }

        public override IDataReader Ranks_List(int ModuleId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_Ranks_List", ModuleId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Use Ranks_List(int ModuleId)")]
        public override IDataReader Ranks_List(int PortalId, int ModuleId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_Ranks_List", ModuleId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Controllers.RankController() and using DAL2")]
        public override void Ranks_Delete(int PortalId, int ModuleId, int RankId)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Ranks_Delete", PortalId, ModuleId, RankId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Controllers.RankController() and using DAL2")]
        public override IDataReader Ranks_Get(int PortalId, int ModuleId, int RankId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_Ranks_Get", PortalId, ModuleId, RankId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Controllers.RankController() and using DAL2")]
        public override int Ranks_Save(int PortalId, int ModuleId, int RankId, string RankName, int MinPosts, int MaxPosts, string Display)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Ranks_Save", PortalId, ModuleId, RankId, RankName, MinPosts, MaxPosts, Display);
        }

        public override IDataReader Reply_Get(int PortalId, int ModuleId, int TopicId, int ReplyId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_Reply_Get", PortalId, ModuleId, TopicId, ReplyId);
        }

        public override int Reply_Save(int PortalId, int TopicId, int ReplyId, int ReplyToId, int StatusId, bool IsApproved, bool IsDeleted, string Subject, string Body, DateTime DateCreated, DateTime DateUpdated, int AuthorId, string AuthorName, string IPAddress)
        {
            Subject = Utilities.NormalizeHtmlForStorage(Subject);
            Body = Utilities.NormalizeHtmlForStorage(Body);
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Reply_Save", PortalId, TopicId, ReplyId, ReplyToId, StatusId, IsApproved, IsDeleted, Subject, Body, DateCreated, DateUpdated, AuthorId, AuthorName, IPAddress);
        }

        public override void Reply_UpdateStatus(int PortalId, int ModuleId, int TopicId, int ReplyId, int UserId, int StatusId, bool IsMod)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Replies_UpdateStatus", PortalId, ModuleId, TopicId, ReplyId, UserId, StatusId, IsMod);
        }

        public override void Reply_Delete(int ForumId, int TopicId, int ReplyId, int DelBehavior)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Reply_Delete", ForumId, TopicId, ReplyId, DelBehavior);
        }

        public override IDataReader Search_DotNetNuke(int moduleId, DateTime beginDateUtc)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_Search_GetSearchItemsFromBegDate", moduleId, beginDateUtc);
        }

        public override IDataReader Subscriptions_GetDigest(string SubscriptionType, DateTime StartDate)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_Subscriptions_DigestGet", SubscriptionType, StartDate);
        }

        public override IDataReader Subscriptions_GetSubscribers(int PortalId, int ForumId, int TopicId, int Mode)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_Subscriptions_Subscribers", PortalId, ForumId, TopicId, Mode);
        }

        public override int Subscription_Update(int PortalId, int ModuleId, int ForumId, int TopicId, int Mode, int UserId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Subscriptions_Update", PortalId, ModuleId, ForumId, TopicId, Mode, UserId);
        }

        public override IDataReader Categories_List(int PortalId, int ModuleId, int PageIndex, int PageSize, string Sort, string SortColumn, int ForumId, int ForumGroupId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_Categories_List", PortalId, ModuleId, PageIndex, PageSize, Sort, SortColumn, ForumId, ForumGroupId);
        }

        public override IDataReader Tags_List(int PortalId, int ModuleId, int PageIndex, int PageSize, string Sort, string SortColumn)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_Tags_List", PortalId, ModuleId, PageIndex, PageSize, Sort, SortColumn);
        }

        public override int Tags_Save(int PortalId, int ModuleId, int TagId, string TagName, int Items, int TopicId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Tags_Save", PortalId, ModuleId, TagId, TagName, Items, TopicId);
        }

        public override void Topics_Delete(int ForumId, int TopicId, int DelBehavior)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Topics_Delete", ForumId, TopicId, DelBehavior, true);
        }

        public override void Topics_Delete_For_User(int ModuleId, int UserId, int DelBehavior)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Topics_Delete_For_User", ModuleId, UserId, DelBehavior);
        }

        public override void Topics_Move(int PortalId, int ModuleId, int ForumId, int TopicId)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Topics_Move", PortalId, ModuleId, ForumId, TopicId);
        }

        public override int Topics_Save(int PortalId, int ModuleId, int TopicId, int ViewCount, int ReplyCount, bool IsLocked, bool IsPinned, string TopicIcon, int StatusId, bool IsApproved, bool IsDeleted, bool IsAnnounce, bool IsArchived, DateTime AnnounceStart, DateTime AnnounceEnd, string Subject, string Body, string Summary, DateTime DateCreated, DateTime DateUpdated, int AuthorId, string AuthorName, string IPAddress, int TopicType, int priority, string URL, string TopicData)
        {
            Subject = Utilities.NormalizeHtmlForStorage(Subject);
            Body = Utilities.NormalizeHtmlForStorage(Body);
            Summary = Utilities.NormalizeHtmlForStorage(Summary);
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Topics_Save", PortalId, ModuleId, TopicId, ViewCount, ReplyCount, IsLocked, IsPinned, TopicIcon, StatusId, IsApproved, IsDeleted, IsAnnounce, IsArchived, Null.GetNull(AnnounceStart, DBNull.Value), Null.GetNull(AnnounceEnd, DBNull.Value), Subject, Body, Summary, DateCreated, DateUpdated, AuthorId, AuthorName, IPAddress, TopicType, priority, URL, TopicData);
        }

        public override void Replies_Split(int OldTopicId, int NewTopicId, string listreplies, DateTime dateUpdate, int FirstReplyId)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Replies_Split", OldTopicId, NewTopicId, listreplies, dateUpdate, FirstReplyId);
        }

        public override void Topics_UpdateStatus(int PortalId, int ModuleId, int TopicId, int ReplyId, int TopicStatusId, int ReplyStatusId, int UserId)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Topics_UpdateStatus", PortalId, ModuleId, TopicId, ReplyId, TopicStatusId, ReplyStatusId, UserId);
        }

        public override DataSet UI_TopicsView(int PortalId, int ModuleId, int ForumId, int UserId, int PageIndex, int PageSize, bool IsSuper, string SortColumn)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteDataSet("activeforums_UI_TopicsView", PortalId, ModuleId, ForumId, UserId, PageIndex, PageSize, IsSuper, SortColumn);
        }

        public override DataSet UI_TopicView(int PortalId, int ModuleId, int ForumId, int TopicId, int UserId, int PageIndex, int PageSize, bool IsSuper, string Sort)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteDataSet("activeforums_UI_TopicView", PortalId, ModuleId, ForumId, TopicId, UserId, PageIndex, PageSize, IsSuper, Sort);
        }

        public override void Profiles_UpdateActivity(int PortalId, int UserId)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_UserProfiles_UpdateActivity", PortalId, UserId);
        }

        public override IDataReader Profiles_GetStats(int PortalId, int Interval)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_UserProfiles_Stats", PortalId, Interval);
        }

        public override IDataReader Profiles_MemberList(int PortalId, int MaxRows, int RowIndex, string Filter)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_UserProfiles_List", PortalId, MaxRows, RowIndex, Filter);
        }

        public override int Content_GetID(int topicId, int? replyId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Content_GetID", topicId, replyId);
        }

        public override int Forum_Maintenance(int ForumId, int OlderThanTimeFrame, int LastActivityTimeFrame, int ByUserId, bool WithoutReplies, bool TestRun, int DelBehavior)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Forums_Maintenance", ForumId, OlderThanTimeFrame, LastActivityTimeFrame, ByUserId, WithoutReplies, TestRun, DelBehavior);
        }

        public override void Utility_MarkAllRead(int ModuleId, int UserId, int ForumId)
        {
            DotNetNuke.Data.SqlDataProvider.Instance().ExecuteNonQuery("activeforums_Util_MarkAsRead", ModuleId, UserId, ForumId);
        }

        public override int Utility_GetFirstUnRead(int TopicId, int LastReadId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteScalar<int>("activeforums_Util_GetFirstUnread", TopicId, LastReadId);
        }

        public override IDataReader PortalForums(int PortalId)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_TP_PortalForums", PortalId);
        }

        public override IDataReader GetPosts(string Forums, bool TopicsOnly, bool RandomOrder, int Rows, string Tags, int FilterByUserId = -1)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_TP_GetPosts", Forums, TopicsOnly, RandomOrder, Rows, Tags, FilterByUserId);
        }

        public override IDataReader GetPostsByUser(int PortalId, int Rows, bool IsSuperUser, int currentUserId, int FilteredUserid, bool TopicsOnly, string ForumIds)
        {
            return DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("activeforums_TP_GetByUser", PortalId, Rows, IsSuperUser, currentUserId, FilteredUserid, TopicsOnly, ForumIds);
        }
    }
}
