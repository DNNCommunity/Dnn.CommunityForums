//
// Community Forums
// Copyright (c) 2013-2021
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
using System.Web;

namespace DotNetNuke.Modules.ActiveForums.Data
{
	public class ForumsDB : DataConfig
	{
		public int Forum_GetByTopicId(int TopicId)
		{
			return Convert.ToInt32(SqlHelper.ExecuteScalar(_connectionString, dbPrefix + "ForumGetByTopicId", TopicId));
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForSocialGroup()")]
        public IDataReader Forums_GetForSocialGroup(int PortalId, int SocialGroupId)
		{
			return SqlHelper.ExecuteReader(_connectionString, dbPrefix + "GetForumsForSocialGroup", SocialGroupId);
		}
		[Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetById()")]
		public IDataReader Forums_Get(int PortalId, int ModuleId, int ForumId)
		{
			return SqlHelper.ExecuteReader(_connectionString, dbPrefix + "ForumGet", PortalId, ModuleId, ForumId);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Get()")]
        public IDataReader Forums_List(int PortalId, int ModuleId, int ForumGroupId, int ParentForumId, bool FillLastPost)
		{
			return (IDataReader)(SqlHelper.ExecuteReader(_connectionString, dbPrefix + "Forums_List", ModuleId, ForumGroupId, ParentForumId, FillLastPost));
		}
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForums()")]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumCollection Forums_List(int PortalId, int ModuleId)
		{
			return new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Get(ModuleId);
		}
		[Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumListXML()")]
		public XmlDocument ForumListXML(int PortalId, int ModuleId)
		{
			return new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForumListXML(PortalId, ModuleId); 
		}
	}
}
