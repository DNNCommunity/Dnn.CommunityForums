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
    using System.Xml;

    using Microsoft.ApplicationBlocks.Data;

    public class Topics : DataConfig
    {
        public int Reply_Save(DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri)
        {
            return Convert.ToInt32(this.Reply_Save(ri.TopicId, ri.ReplyId, ri.ReplyToId, ri.StatusId, ri.IsApproved, ri.IsDeleted, ri.Content.Subject.Trim(), ri.Content.Body.Trim(), ri.Content.DateCreated, ri.Content.DateUpdated, ri.Content.AuthorId, ri.Content.AuthorName, ri.Content.IPAddress));
        }

        public int Reply_Save(int topicId, int replyId, int replyToId, int statusId, bool isApproved, bool isDeleted, string subject, string body, DateTime dateCreated, DateTime dateUpdated, int authorId, string authorName, string iPAddress)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.connectionString, this.dbPrefix + "Reply_Save", topicId, replyId, replyToId, statusId, isApproved, isDeleted, subject, body, dateCreated, dateUpdated, authorId, authorName, iPAddress));
        }

        public int TopicIdByUrl(int portalId, int moduleId, string uRL)
        {
            if (uRL.EndsWith("/"))
            {
                uRL = uRL.Substring(0, uRL.Length - 1);
            }

            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.connectionString, this.dbPrefix + "TopicIdByURL", portalId, moduleId, uRL));
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController")]
        public int Topics_AddRating(int topicId, int userID, int rating, string comments, string iPAddress)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(userId: userID, topicId: topicId, rating: rating, ipAddress: iPAddress);
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public IDataReader TopicForDisplay(int portalId, int moduleId, int forumId, int topicId, int userId, int rowIndex, int maxRows, string sort)
        {
            throw new NotImplementedException();
        }

        public IDataReader TopicsList(int portalId, int pageIndex, int pageSize, string forumIds, int categoryId, int tagId)
        {
            return SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "TopicsList", portalId, pageIndex, pageSize, forumIds, categoryId, tagId);
        }

        public IDataReader TopicWithReplies(int portalId, int topicId, int pageIndex, int pageSize)
        {
            return SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "TopicWithReplies", portalId, topicId, pageIndex, pageSize);
        }

    }
}
