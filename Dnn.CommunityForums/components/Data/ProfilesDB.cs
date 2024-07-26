// Copyright (c) 2013-2024 by DNN Community
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

namespace DotNetNuke.Modules.ActiveForums.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    using Microsoft.ApplicationBlocks.Data;

    public class Profiles : DataConfig
    {
        public void Profiles_Create(int portalId, int moduleId, int userId)
        {
            SqlHelper.ExecuteNonQuery(this.connectionString, this.dbPrefix + "UserProfiles_Create", portalId, -1, userId);
        }

        public void Profiles_UpdateActivity(int portalId, int moduleId, int userId)
        {
            SqlHelper.ExecuteNonQuery(this.connectionString, this.dbPrefix + "UserProfiles_UpdateActivity", portalId, userId);
        }

        public IDataReader Profiles_GetUsersOnline(int portalId, int moduleId, int interval)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "UserProfiles_GetUsersOnline", portalId, moduleId, interval);
        }

        public IDataReader Profiles_Get(int portalId, int moduleId, int userId)
        {
            return SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "UserProfiles_Get", portalId, -1, userId);
        }

        public void Profiles_Save(int portalId, int moduleId, int userId, int topicCount, int replyCount, int viewCount, int answerCount, int rewardPoints, string userCaption, string signature, bool signatureDisabled, int trustLevel, bool adminWatch, bool attachDisabled, string avatar, int avatarType, bool avatarDisabled, string prefDefaultSort, bool prefDefaultShowReplies, bool prefJumpLastPost, bool prefTopicSubscribe, int prefSubscriptionType, bool prefUseAjax, bool prefBlockAvatars, bool prefBlockSignatures, int prefPageSize, string yahoo, string mSN, string iCQ, string aOL, string occupation, string location, string interests, string webSite, string badges)
        {
            SqlHelper.ExecuteNonQuery(this.connectionString, this.dbPrefix + "UserProfiles_Save", portalId, -1, userId, topicCount, replyCount, viewCount, answerCount, rewardPoints, userCaption, signature, signatureDisabled, trustLevel, adminWatch, attachDisabled, avatar, avatarType, avatarDisabled, prefDefaultSort, prefDefaultShowReplies, prefJumpLastPost, prefTopicSubscribe, prefSubscriptionType, prefUseAjax, prefBlockAvatars, prefBlockSignatures, prefPageSize, yahoo, mSN, iCQ, aOL, occupation, location, interests, webSite, badges);
        }

        public IDataReader Profiles_GetStats(int portalId, int moduleId, int interval)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "UserProfiles_Stats", portalId, moduleId, interval);
        }

        public IDataReader Profiles_MemberList(int portalId, int maxRows, int rowIndex, string filter)
        {
            // Return CType(SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner & ObjectQualifier & "activeforums_UserProfiles_Members", PortalId, MaxRows, RowIndex, Filter), IDataReader)
            return (IDataReader)SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "UserProfiles_List", portalId, maxRows, rowIndex, filter);
        }

        public void Profile_UpdateTopicCount(int portalId, int userId)
        {
            string sSql = "UPDATE " + this.dbPrefix + "UserProfiles SET TopicCount = ISNULL((Select Count(t.TopicId) from ";
            sSql += this.dbPrefix + "Topics as t INNER JOIN ";
            sSql += this.dbPrefix + "Content as c ON t.ContentId = c.ContentId AND c.AuthorId = @AuthorId INNER JOIN ";
            sSql += this.dbPrefix + "ForumTopics as ft ON ft.TopicId = t.TopicId INNER JOIN ";
            sSql += this.dbPrefix + "Forums as f ON ft.ForumId = f.ForumId ";
            sSql += "WHERE c.AuthorId = @AuthorId AND t.IsApproved = 1 AND t.IsDeleted=0 AND f.PortalId=@PortalId),0) ";
            sSql += "WHERE UserId = @AuthorId AND PortalId = @PortalId";
            sSql = sSql.Replace("@AuthorId", userId.ToString());
            sSql = sSql.Replace("@PortalId", portalId.ToString());
            SqlHelper.ExecuteNonQuery(this.connectionString, CommandType.Text, sSql);
        }
    }
}
