﻿// Copyright (c) by DNN Community
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
    using Microsoft.ApplicationBlocks.Data;

    public class SqlDataProvider : DataProvider
    {
        #region Private Members

        private const string ProviderType = "data";

        private DotNetNuke.Framework.Providers.ProviderConfiguration providerConfiguration = DotNetNuke.Framework.Providers.ProviderConfiguration.GetProviderConfiguration(ProviderType);
        private string connectionString;
        private string providerPath;
        private string objectQualifier;
        private string databaseOwner;

        #endregion

        #region Constructors

        public SqlDataProvider()
        {
            // Read the configuration specific information for this provider
            DotNetNuke.Framework.Providers.Provider objProvider = (DotNetNuke.Framework.Providers.Provider)this.providerConfiguration.Providers[this.providerConfiguration.DefaultProvider];

            // Read the attributes for this provider

            // Get Connection string from web.config
            this.connectionString = Config.GetConnectionString();

            if (this.connectionString == string.Empty)
            {
                // Use connection string specified in provider
                this.connectionString = objProvider.Attributes["connectionString"];
            }

            this.providerPath = objProvider.Attributes["providerPath"];

            this.objectQualifier = objProvider.Attributes["objectQualifier"];
            if (this.objectQualifier != string.Empty && this.objectQualifier.EndsWith("_") == false)
            {
                this.objectQualifier += "_";
            }

            this.databaseOwner = objProvider.Attributes["databaseOwner"];
            if (this.databaseOwner != string.Empty && this.databaseOwner.EndsWith(".") == false)
            {
                this.databaseOwner += ".";
            }
        }
        #endregion

        #region Properties

        public string ConnectionString
        {
            get
            {
                return this.connectionString;
            }
        }

        public string ProviderPath
        {
            get
            {
                return this.providerPath;
            }
        }

        public string ObjectQualifier
        {
            get
            {
                return this.objectQualifier;
            }
        }

        public string DatabaseOwner
        {
            get
            {
                return this.databaseOwner;
            }
        }

        #endregion

        // general
        private object GetNull(object field)
        {
            return DotNetNuke.Common.Utilities.Null.GetNull(field, DBNull.Value);
        }

        #region Dashboard
        public override DataSet Dashboard_Get(int PortalId, int ModuleId)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_DashBoard_Stats", PortalId, ModuleId);
        }

        #endregion
        #region Filters
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController.Delete()")]
        public override void Filters_Delete(int PortalId, int ModuleId, int FilterId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Filters_Delete", PortalId, ModuleId, FilterId);
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController.DeleteByModuleId()")]
        public override void Filters_DeleteByModuleId(int PortalId, int ModuleId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Filters_DeleteByModuleId", PortalId, ModuleId);
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController.GetById()")]
        public override IDataReader Filters_Get(int PortalId, int ModuleId, int FilterId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Filters_Get", PortalId, ModuleId, FilterId);
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController.GetEmoticons()")]
        public override IDataReader Filters_GetEmoticons(int moduleID)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Filters_GetEmoticons", moduleID);
        }

        public override IDataReader Filters_List(int PortalId, int ModuleId, int PageIndex, int PageSize, string Sort, string SortColumn)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Filters_List", PortalId, ModuleId, PageIndex, PageSize, Sort, SortColumn);
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Not Used")]
        public override IDataReader Filters_ListByType(int PortalId, int ModuleId, string FilterType)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Filters_ListByType", PortalId, ModuleId, FilterType);
        }

        public override int Filters_Save(int PortalId, int ModuleId, int FilterId, string Find, string Replace, string FilterType)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Filters_Save", PortalId, ModuleId, FilterId, Find, Replace, FilterType));
        }

        #endregion
        #region Forums
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public override void Forums_Delete(int PortalId, int ModuleId, int ForumId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Forums_Delete", PortalId, ModuleId, ForumId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public override IDataReader Forums_Get(int PortalId, int ModuleId, int ForumID, int UserId, bool WithSecurity)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public override IDataReader Forums_List(int PortalId, int ModuleId, int ForumGroupId, int ParentForumId, bool FillLastPost)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Forums_List", ModuleId, ForumGroupId, ParentForumId, FillLastPost);
        }

        public override void Forums_Move(int ModuleId, int ForumId, int SortDirection)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Forums_MoveForum", ModuleId, ForumId, SortDirection);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Forum_Save(int PortalId, int ForumId, int ModuleId, int ForumGroupId, int ParentForumId, string ForumName, string ForumDesc, int SortOrder, bool Active, bool Hidden, string ForumSettingsKey, int PermissionsId, string PrefixURL, int SocialGroupId, bool HasProperties)")]
        public override int Forum_Save(int PortalId, int ForumId, int ModuleId, int ForumGroupId, int ParentForumId, string ForumName, string ForumDesc, int SortOrder, bool Active, bool Hidden, string ForumSettingsKey, string ForumSecurityKey, int PermissionsId, string PrefixURL, int SocialGroupId, bool HasProperties)
        {
            return this.Forum_Save(PortalId, ForumId, ModuleId, ForumGroupId, ParentForumId, ForumName, ForumDesc, SortOrder, Active, Hidden, ForumSettingsKey, string.Empty, PermissionsId, PrefixURL, SocialGroupId, HasProperties);
        }

        public override int Forum_Save(int PortalId, int ForumId, int ModuleId, int ForumGroupId, int ParentForumId, string ForumName, string ForumDesc, int SortOrder, bool Active, bool Hidden, string ForumSettingsKey, int PermissionsId, string PrefixURL, int SocialGroupId, bool HasProperties)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Forum_Save", PortalId, ForumId, ModuleId, ForumGroupId, ParentForumId, ForumName, ForumDesc, SortOrder, Active, Hidden, ForumSettingsKey, PermissionsId, PrefixURL, SocialGroupId, HasProperties));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public override void Forum_ConfigCleanUp(int ModuleId, string ForumSettingsKey)
        {
            SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Forum_ConfigCleanUp", ModuleId, ForumSettingsKey);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Forum_ConfigCleanUp(int ModuleId, string ForumSettingsKey)")]
        public override void Forum_ConfigCleanUp(int ModuleId, string ForumSettingsKey, string ForumSecurityKey)
        {
            this.Forum_ConfigCleanUp(ModuleId, ForumSettingsKey);
        }

        #endregion
        #region Groups
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public override void Groups_Delete(int ModuleID, int ForumGroupID)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Groups_Delete", ModuleID, ForumGroupID);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public override IDataReader Groups_Get(int ModuleId, int ForumGroupID)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Groups_Get", ModuleId, ForumGroupID);
        }

        public override void Groups_Move(int ModuleId, int ForumGroupId, int SortDirection)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Groups_MoveGroup", ModuleId, ForumGroupId, SortDirection);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public override int Groups_Save(int PortalId, int ModuleId, int ForumGroupId, string GroupName, int SortOrder, bool Active, bool Hidden, int PermissionsId, string PrefixURL)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Groups_Save", PortalId, ModuleId, ForumGroupId, GroupName, SortOrder, Active, Hidden, PermissionsId, PrefixURL, $"G:{ForumGroupId}"));
        }

        public override int Groups_Save(int PortalId, int ModuleId, int ForumGroupId, string GroupName, int SortOrder, bool Active, bool Hidden, int PermissionsId, string PrefixURL, string GroupSettingsKey)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Groups_Save", PortalId, ModuleId, ForumGroupId, GroupName, SortOrder, Active, Hidden, PermissionsId, PrefixURL, GroupSettingsKey));
        }

        #endregion
        #region Polls
        public override DataSet Poll_Get(int TopicId)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Poll_Get", TopicId);
        }

        public override IDataReader Poll_GetResults(int TopicId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Poll_GetResults", TopicId);
        }

        public override int Poll_HasVoted(int TopicId, int UserId)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Poll_HasVoted", TopicId, UserId));
        }

        public override void Poll_Option_Save(int PollOptionsId, int PollId, string OptionName, int TopicId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Poll_Options_Save", PollOptionsId, PollId, OptionName, TopicId);
        }

        public override int Poll_Save(int PollId, int TopicId, int UserId, string Question, string PollType)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Poll_Save", PollId, TopicId, UserId, Question, PollType));
        }

        public override void Poll_Vote(int PollId, int PollOptionId, string Response, string IPAddress, int UserId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Poll_Vote", PollId, PollOptionId, Response, IPAddress, UserId);
        }

        #endregion
        #region Moderation
        public override DataSet Mod_Pending(int PortalId, int ModuleId, int ForumId, int UserId)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Mod_Pending", PortalId, ModuleId, ForumId, UserId);
        }

        public override void Mod_Reject(int PortalId, int ModuleId, int UserId, int ForumId, int TopicId, int ReplyId, int Reason, string Comment)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Mod_Reject", PortalId, ModuleId, UserId, ForumId, TopicId, ReplyId, Reason, Comment);
        }

        #endregion
        #region Ranks
        public override void Ranks_Delete(int PortalId, int ModuleId, int RankId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Ranks_Delete", PortalId, ModuleId, RankId);
        }

        public override IDataReader Ranks_Get(int PortalId, int ModuleId, int RankId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Ranks_Get", PortalId, ModuleId, RankId);
        }

        public override IDataReader Ranks_List(int PortalId, int ModuleId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Ranks_List", PortalId, ModuleId);
        }

        public override int Ranks_Save(int PortalId, int ModuleId, int RankId, string RankName, int MinPosts, int MaxPosts, string Display)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Ranks_Save", PortalId, ModuleId, RankId, RankName, MinPosts, MaxPosts, Display));
        }

        #endregion
        #region Replies/Comments
        public override IDataReader Reply_Get(int PortalId, int ModuleId, int TopicId, int ReplyId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Reply_Get", PortalId, ModuleId, TopicId, ReplyId);
        }

        public override int Reply_Save(int PortalId, int TopicId, int ReplyId, int ReplyToId, int StatusId, bool IsApproved, bool IsDeleted, string Subject, string Body, DateTime DateCreated, DateTime DateUpdated, int AuthorId, string AuthorName, string IPAddress)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Reply_Save", PortalId, TopicId, ReplyId, ReplyToId, StatusId, IsApproved, IsDeleted, Subject, Body, DateCreated, DateUpdated, AuthorId, AuthorName, IPAddress));
        }

        public override void Reply_UpdateStatus(int PortalId, int ModuleId, int TopicId, int ReplyId, int UserId, int StatusId, bool IsMod)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Replies_UpdateStatus", PortalId, ModuleId, TopicId, ReplyId, UserId, StatusId, IsMod);
        }

        public override void Reply_Delete(int ForumId, int TopicId, int ReplyId, int DelBehavior)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Reply_Delete", ForumId, TopicId, ReplyId, DelBehavior);
        }

        #endregion

        #region Search
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Using Search_DotNetNuke(int moduleId, DateTime beginDateUtc)")]
        public override IDataReader Search_DotNetNuke(int moduleId)
        {
            return SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Search_GetSearchItems", moduleId);
        }

        public override IDataReader Search_DotNetNuke(int moduleId, DateTime beginDateUtc)
        {
            return SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Search_GetSearchItemsFromBegDate", moduleId, beginDateUtc);
        }

        public override DataSet Search(int portalId, int moduleId, int userId, int searchId, int rowIndex, int maxRows, string searchString, int matchType, int searchField, int timespan, int authorId, string author, string forums, string tags, int resultType, int sort, int maxCacheHours, bool fullText)
        {
            return SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Search", portalId, moduleId, userId, searchId, rowIndex, maxRows, searchString, matchType, searchField, timespan, authorId, author, forums, tags, resultType, sort, maxCacheHours, fullText);
        }

        public override int Search_ManageFullText(bool enabled)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Search_ManageFullText", enabled));
        }

        public override int Search_GetFullTextStatus()
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Search_GetFullTextStatus"));
        }

        #endregion

        #region Security
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Obsoleted by activeforums_Permissions")]
        public override void Security_Delete(int SecuredId, int ObjectId, int SecureAction, int SecureType, int ObjectType)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Obsoleted by activeforums_Permissions")]
        public override IDataReader Security_Get(int SecuredId, int ObjectId, int SecureType)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Obsoleted by activeforums_Permissions")]
        public override IDataReader Security_GetByUser(int PortalId, int ForumId, int UserId, bool IsSuperUser)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Obsoleted by activeforums_Permissions")]
        public override void Security_Save(int SecuredId, int ObjectId, string SecureAction, bool SecureActionValue, int SecureType, string ObjectName, int ObjectType, string SecurityKey)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Obsoleted by activeforums_Permissions")]
        public override IDataReader Security_SearchObjects(int PortalId, string Search)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Obsoleted by activeforums_Permissions")]
        public override IDataReader Security_GetByKey(string SecurityKey)
        {
            throw new NotImplementedException();
        }

        #endregion
        #region Settings

        public override void Settings_Delete(int ModuleId, string GroupKey, string SettingName)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Settings_Delete", ModuleId, GroupKey, SettingName);
        }

        public override string Settings_Get(int ModuleId, string GroupKey, string SettingName)
        {
            return Convert.ToString(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Settings_Get", ModuleId, GroupKey, SettingName));
        }

        public override System.Data.IDataReader Settings_List(int ModuleId, string GroupKey)
        {
            return SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Settings_List", ModuleId, GroupKey);
        }

        // KR - grabs all settings for caching
        public override System.Data.IDataReader Settings_ListAll(int ModuleId)
        {
            return SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Settings_ListAll", ModuleId);
        }

        public override void Settings_Save(int ModuleId, string GroupKey, string SettingName, string SettingValue)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Settings_Save", ModuleId, GroupKey, SettingName, SettingValue);
        }

        #endregion
        #region Subscriptions
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public override int Subscriptions_IsSubscribed(int PortalId, int ModuleId, int ForumId, int TopicId, int Mode, int UserId)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Subscriptions_IsSubscribed", PortalId, ModuleId, ForumId, TopicId, Mode, UserId));
        }

        public override IDataReader Subscriptions_GetDigest(string SubscriptionType, DateTime StartDate)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Subscriptions_DigestGet", SubscriptionType, StartDate);
        }

        public override IDataReader Subscriptions_GetSubscribers(int PortalId, int ForumId, int TopicId, int Mode)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Subscriptions_Subscribers", PortalId, ForumId, TopicId, Mode);
        }

        public override int Subscription_Update(int PortalId, int ModuleId, int ForumId, int TopicId, int Mode, int UserId)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Subscriptions_Update", PortalId, ModuleId, ForumId, TopicId, Mode, UserId));
        }

        #endregion
        #region Tags
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public override void Tags_Delete(int PortalId, int ModuleId, int TagId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Tags_Delete", PortalId, ModuleId, TagId, -1);
        }

        public override void Tags_DeleteByTopicId(int PortalId, int ModuleId, int TopicId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Tags_Delete", PortalId, ModuleId, -1, TopicId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public override IDataReader Tags_Get(int PortalId, int ModuleId, int TagId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Tags_Get", PortalId, ModuleId, TagId);
        }

        public override IDataReader Tags_List(int PortalId, int ModuleId, bool IsCategory, int PageIndex, int PageSize, string Sort, string SortColumn, int ForumId, int ForumGroupId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Tags_List", PortalId, ModuleId, IsCategory, PageIndex, PageSize, Sort, SortColumn, ForumId, ForumGroupId);
        }

        public override int Tags_Save(int PortalId, int ModuleId, int TagId, string TagName, int Clicks, int Items, int Priority, int TopicId, bool IsCategory, int ForumId, int ForumGroupId)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Tags_Save", PortalId, ModuleId, TagId, TagName, Clicks, Items, Priority, TopicId, IsCategory, ForumId, ForumGroupId));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public override IDataReader Tags_Search(int PortalId, int ModuleId, string Search)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Tags_Search", PortalId, ModuleId, Search);
        }

        public override void Tags_AddTopicToCategory(int PortalId, int ModuleId, int TagId, int TopicId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Tags_AddTopicToCategory", PortalId, ModuleId, TagId, TopicId);
        }

        public override void Tags_DeleteTopicToCategory(int PortalId, int ModuleId, int TagId, int TopicId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Tags_DeleteTopicToCategory", PortalId, ModuleId, TagId, TopicId);
        }

        #endregion
        #region Templates
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public override void Templates_Delete(int TemplateId, int PortalId, int ModuleId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Templates_Delete", TemplateId, PortalId, ModuleId);
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public override IDataReader Templates_Get(int TemplateId, int PortalId, int ModuleId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Templates_Get", TemplateId, PortalId, ModuleId);
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public override IDataReader Templates_List(int PortalId, int ModuleId, int TemplateType)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Templates_List", PortalId, ModuleId, TemplateType, 0, 10000);
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public override IDataReader Templates_List(int PortalId, int ModuleId, int TemplateType, int RowIndex, int PageSize)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Templates_List", PortalId, ModuleId, TemplateType, RowIndex, PageSize);
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public override int Templates_Save(int TemplateId, int PortalId, int ModuleId, int TemplateType, bool IsSystem, string Title, string Subject, string Template)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Templates_Save", TemplateId, PortalId, ModuleId, TemplateType, IsSystem, Title, Subject, Template));
        }

        #endregion
        #region Topics
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController")]
        public override int Topics_AddRating(int TopicId, int UserID, int Rating, string Comments, string IPAddress)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(userId: UserID, topicId: TopicId, rating: Rating, ipAddress: IPAddress);
        }

        public override void Topics_Delete(int ForumId, int TopicId, int DelBehavior)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Topics_Delete", ForumId, TopicId, DelBehavior, true);
        }

        public override void Topics_Delete_For_User(int ModuleId, int UserId, int DelBehavior)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Topics_Delete_For_User", ModuleId, UserId, DelBehavior);
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.GetById(int TopicId)")]
        public override IDataReader Topics_Get(int PortalId, int ModuleId, int TopicId, int ForumId, int UserId, bool WithSecurity)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Topics_Get", PortalId, ModuleId, TopicId, ForumId, UserId, WithSecurity);
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController")]
        public override int Topics_GetRating(int TopicId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Average(topicId: TopicId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public override IDataReader Topics_List(int ForumId, int PortalId, int ModuleId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Topics_List", ForumId, PortalId, ModuleId);
        }

        public override void Topics_Move(int PortalId, int ModuleId, int ForumId, int TopicId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Topics_Move", PortalId, ModuleId, ForumId, TopicId);
        }

        public override int Topics_Save(int PortalId, int TopicId, int ViewCount, int ReplyCount, bool IsLocked, bool IsPinned, string TopicIcon, int StatusId, bool IsApproved, bool IsDeleted, bool IsAnnounce, bool IsArchived, DateTime AnnounceStart, DateTime AnnounceEnd, string Subject, string Body, string Summary, DateTime DateCreated, DateTime DateUpdated, int AuthorId, string AuthorName, string IPAddress, int TopicType, int priority, string URL, string TopicData)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Topics_Save", PortalId, TopicId, ViewCount, ReplyCount, IsLocked, IsPinned, TopicIcon, StatusId, IsApproved, IsDeleted, IsAnnounce, IsArchived, this.GetNull(AnnounceStart), this.GetNull(AnnounceEnd), Subject, Body, Summary, DateCreated, DateUpdated, AuthorId, AuthorName, IPAddress, TopicType, priority, URL, TopicData));
        }

        public override int Topics_SaveToForum(int ForumId, int TopicId, int LastReplyId)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Topics_SaveToForum", ForumId, TopicId, this.GetNull(LastReplyId)));
        }

        public override void Replies_Split(int OldTopicId, int NewTopicId, string listreplies, DateTime dateUpdate, int FirstReplyId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Replies_Split", OldTopicId, NewTopicId, listreplies, dateUpdate, FirstReplyId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public override IDataReader Topics_Replies(int TopicId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Topics_Replies", TopicId);
        }

        public override void Topics_UpdateStatus(int PortalId, int ModuleId, int TopicId, int ReplyId, int TopicStatusId, int ReplyStatusId, int UserId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Topics_UpdateStatus", PortalId, ModuleId, TopicId, ReplyId, TopicStatusId, ReplyStatusId, UserId);
        }

        #endregion
        #region UI
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No Longer Used.")]
        public override DataSet UI_ForumView(int PortalId, int ModuleId, int UserId, bool IsSuper, string ForumIds)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UI_ForumView", PortalId, ModuleId, UserId, IsSuper, -1, ForumIds);
        }

        public override DataSet UI_TopicsView(int PortalId, int ModuleId, int ForumId, int UserId, int PageIndex, int PageSize, bool IsSuper, string SortColumn)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UI_TopicsView", PortalId, ModuleId, ForumId, UserId, PageIndex, PageSize, IsSuper, SortColumn);
        }

        public override DataSet UI_TopicView(int PortalId, int ModuleId, int ForumId, int TopicId, int UserId, int PageIndex, int PageSize, bool IsSuper, string Sort)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UI_TopicView", PortalId, ModuleId, ForumId, TopicId, UserId, PageIndex, PageSize, IsSuper, Sort);
        }

        public override DataSet UI_NotReadView(int PortalId, int ModuleId, int UserId, int RowIndex, int MaxRows, string Sort, bool IsSuper)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UI_NotRead", PortalId, ModuleId, UserId, RowIndex, MaxRows, Sort, IsSuper);
        }

        public override DataSet UI_UnansweredView(int PortalId, int ModuleId, int UserId, int RowIndex, int MaxRows, string Sort, bool IsSuper)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UI_UnansweredView", PortalId, ModuleId, UserId, RowIndex, MaxRows, Sort, IsSuper);
        }

        public override DataSet UI_MyTopicsView(int PortalId, int ModuleId, int UserId, int RowIndex, int MaxRows, string Sort, bool IsSuper)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UI_MyTopicsView", PortalId, ModuleId, UserId, RowIndex, MaxRows, Sort, IsSuper);
        }

        public override DataSet UI_ActiveView(int PortalId, int ModuleId, int UserId, int RowIndex, int MaxRows, string Sort, bool IsSuper, int TimeFrame)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UI_ActiveView", PortalId, ModuleId, UserId, RowIndex, MaxRows, Sort, IsSuper, TimeFrame);
        }

        public override DataSet UI_MostLiked(int PortalId, int ModuleId, int UserId, int RowIndex, int MaxRows, string Sort, bool IsSuper, int TimeFrame)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UI_MostLiked", PortalId, ModuleId, UserId, RowIndex, MaxRows, Sort, IsSuper, TimeFrame);
        }

        public override DataSet UI_MostReplies(int PortalId, int ModuleId, int UserId, int RowIndex, int MaxRows, string Sort, bool IsSuper, int TimeFrame)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UI_MostReplies", PortalId, ModuleId, UserId, RowIndex, MaxRows, Sort, IsSuper, TimeFrame);
        }

        public override DataSet UI_Announcements(int PortalId, int ModuleId, int UserId, int RowIndex, int MaxRows, string Sort, bool IsSuper)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UI_Announcements", PortalId, ModuleId, UserId, RowIndex, MaxRows, Sort, IsSuper);
        }

        public override DataSet UI_Unresolved(int PortalId, int ModuleId, int UserId, int RowIndex, int MaxRows, string Sort, bool IsSuper)
        {
            return (DataSet)SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UI_Unresolved", PortalId, ModuleId, UserId, RowIndex, MaxRows, Sort, IsSuper);
        }

        #endregion
        #region UserProfiles
        #region "Deprecated Methods"
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public override void Profiles_Create(int PortalId, int ModuleId, int UserId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UserProfiles_Create", PortalId, UserId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public override DataSet Profiles_Get(int PortalId, int ModuleId, int UserId)
        {
            return SqlHelper.ExecuteDataset(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UserProfiles_Get", PortalId, ModuleId, UserId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public override void Profiles_Save(int PortalId, int ModuleId, int UserId, int TopicCount, int ReplyCount, int ViewCount, int AnswerCount, int RewardPoints, string UserCaption, string Signature, bool SignatureDisabled, int TrustLevel, bool AdminWatch, bool AttachDisabled, string Avatar, int AvatarType, bool AvatarDisabled, string PrefDefaultSort, bool PrefDefaultShowReplies, bool PrefJumpLastPost, bool PrefTopicSubscribe, int PrefSubscriptionType, bool PrefBlockAvatars, bool PrefBlockSignatures, int PrefPageSize)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UserProfiles_Save", PortalId, -1, UserId, TopicCount, ReplyCount, ViewCount, AnswerCount, RewardPoints, UserCaption, Signature, SignatureDisabled, TrustLevel, AdminWatch, AttachDisabled, Avatar, AvatarType, AvatarDisabled, PrefDefaultSort, PrefDefaultShowReplies, PrefJumpLastPost, PrefTopicSubscribe, PrefSubscriptionType, PrefBlockAvatars, PrefBlockSignatures, PrefPageSize);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]

        public override IDataReader Profiles_GetUsersOnline(int PortalId, int ModuleId, int Interval)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UserProfiles_GetUsersOnline", PortalId, Interval);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public override void Profiles_UpdateActivity(int PortalId, int ModuleId, int UserId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UserProfiles_UpdateActivity", PortalId, UserId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public override IDataReader Profiles_GetStats(int PortalId, int ModuleId, int Interval)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UserProfiles_Stats", PortalId, Interval);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public override IDataReader Profiles_MemberList(int PortalId, int ModuleId, int MaxRows, int RowIndex, string Filter)
        {
            return (IDataReader)(SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UserProfiles_List", PortalId, MaxRows, RowIndex, Filter));
        }

        #endregion
        public override void Profiles_UpdateActivity(int PortalId, int UserId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UserProfiles_UpdateActivity", PortalId, UserId);
        }

        public override IDataReader Profiles_GetStats(int PortalId, int Interval)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UserProfiles_Stats", PortalId, Interval);
        }

        public override IDataReader Profiles_MemberList(int PortalId, int MaxRows, int RowIndex, string Filter)
        {
            return (IDataReader)(SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_UserProfiles_List", PortalId, MaxRows, RowIndex, Filter));
        }

        #endregion

        #region Content

        public override int Content_GetID(int topicId, int? replyId)
        {
            return Utilities.SafeConvertInt(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Content_GetID", topicId, replyId));
        }

        #endregion

        #region MailQueue
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. No longer used.")]
        public override IDataReader Queue_List() => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. No longer used.")]
        public override void Queue_Delete(int EmailId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public override void Queue_Add(string EmailFrom, string EmailTo, string EmailSubject, string EmailBody, string EmailBodyPlainText, string EmailCC, string EmailBCC) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public override void Queue_Add(int portalId, string EmailFrom, string EmailTo, string EmailSubject, string EmailBody, string EmailBodyPlainText, string EmailCC, string EmailBCC) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public override void Queue_Add(int portalId, string EmailFrom, string EmailTo, string EmailSubject, string EmailBody, string EmailCC, string EmailBCC) => throw new NotImplementedException();

        #endregion
        #region Maintenance
        public override int Forum_Maintenance(int ForumId, int OlderThanTimeFrame, int LastActivityTimeFrame, int ByUserId, bool WithoutReplies, bool TestRun, int DelBehavior)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Forums_Maintenance", ForumId, OlderThanTimeFrame, LastActivityTimeFrame, ByUserId, WithoutReplies, TestRun, DelBehavior));
        }
        #endregion

        #region Utility Items
        public override void Utility_MarkAllRead(int ModuleId, int UserId, int ForumId)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Util_MarkAsRead", ModuleId, UserId, ForumId);
        }

        public override int Utility_GetFirstUnRead(int TopicId, int LastReadId)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_Util_GetFirstUnread", TopicId, LastReadId));
        }
        #endregion

        #region Top Posts
        public override IDataReader PortalForums(int PortalId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_TP_PortalForums", PortalId);
        }

        public override IDataReader GetPosts(int PortalId, string Forums, bool TopicsOnly, bool RandomOrder, int Rows, string Tags, int FilterByUserId = -1)
        {
            const bool IgnoreSecurity = false; // Required by proc but not currently used
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_TP_GetPosts", PortalId, Forums, TopicsOnly, RandomOrder, Rows, IgnoreSecurity, Tags, FilterByUserId);
        }

        public override IDataReader GetPostsByUser(int PortalId, int Rows, bool IsSuperUser, int currentUserId, int FilteredUserid, bool TopicsOnly, string ForumIds)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.ConnectionString, this.DatabaseOwner + this.ObjectQualifier + "activeforums_TP_GetByUser", PortalId, Rows, IsSuperUser, currentUserId, FilteredUserid, TopicsOnly, ForumIds);
        }
        #endregion

    }
}
