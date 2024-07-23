//
// Community Forums
// Copyright (c) 2013-2024
// by DNN Community
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
//


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using Microsoft.ApplicationBlocks.Data;
using System.Xml;
namespace DotNetNuke.Modules.ActiveForums.Data
{
	public class Topics : DataConfig
	{
		public int Reply_Save(DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri)
		{
			return Convert.ToInt32(Reply_Save(ri.TopicId, ri.ReplyId, ri.ReplyToId, ri.StatusId, ri.IsApproved, ri.IsDeleted, ri.Content.Subject.Trim(), ri.Content.Body.Trim(), ri.Content.DateCreated, ri.Content.DateUpdated, ri.Content.AuthorId, ri.Content.AuthorName, ri.Content.IPAddress));
		}
		public int Reply_Save(int TopicId, int ReplyId, int ReplyToId, int StatusId, bool IsApproved, bool IsDeleted, string Subject, string Body, DateTime DateCreated, DateTime DateUpdated, int AuthorId, string AuthorName, string IPAddress)
		{
			return Convert.ToInt32(SqlHelper.ExecuteScalar(_connectionString, dbPrefix + "Reply_Save", TopicId, ReplyId, ReplyToId, StatusId, IsApproved, IsDeleted, Subject, Body, DateCreated, DateUpdated, AuthorId, AuthorName, IPAddress));
		}
		public int TopicIdByUrl(int PortalId, int ModuleId, string URL)
		{
			if (URL.EndsWith("/"))
			{
				URL = URL.Substring(0, URL.Length - 1);
			}
			return Convert.ToInt32(SqlHelper.ExecuteScalar(_connectionString, dbPrefix + "TopicIdByURL", PortalId, ModuleId, URL));
		}
		[Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController")]
		public int Topics_AddRating(int TopicId, int UserID, int Rating, string Comments, string IPAddress)
		{
			return new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(userId: UserID, topicId: TopicId, rating: Rating, IpAddress: IPAddress);
		}
		public IDataReader TopicForDisplay(int PortalId, int ModuleId, int ForumId, int TopicId, int UserId, int RowIndex, int MaxRows, string Sort)
		{
			IDataReader dr = SqlHelper.ExecuteReader(_connectionString, dbPrefix + "UI_TopicDisplay", PortalId, ModuleId, ForumId, TopicId, UserId, RowIndex, MaxRows, false, Sort);
			return dr;
		}
		public IDataReader TopicsList(int PortalId, int PageIndex, int PageSize, string ForumIds, int CategoryId, int TagId)
		{
			return SqlHelper.ExecuteReader(_connectionString, dbPrefix + "TopicsList", PortalId, PageIndex, PageSize, ForumIds, CategoryId, TagId);
		}
		public IDataReader TopicWithReplies(int PortalId, int TopicId, int PageIndex, int PageSize)
		{
			return SqlHelper.ExecuteReader(_connectionString, dbPrefix + "TopicWithReplies", PortalId, TopicId, PageIndex, PageSize);
		}
	}
}
