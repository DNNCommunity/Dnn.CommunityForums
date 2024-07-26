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

namespace DotNetNuke.Modules.ActiveForums.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Web;
    using System.Xml;

    using Microsoft.ApplicationBlocks.Data;

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forum_GetByTopicId()")]
    public class ForumsDB : DataConfig
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forum_GetByTopicId()")]
        public int Forum_GetByTopicId(int topicId) => DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forum_GetByTopicId(topicId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForSocialGroup()")]
        public IDataReader Forums_GetForSocialGroup(int portalId, int socialGroupId) => SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "GetForumsForSocialGroup", socialGroupId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetById()")]
        public IDataReader Forums_Get(int portalId, int moduleId, int forumId) => SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "ForumGet", portalId, moduleId, forumId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Get()")]
        public IDataReader Forums_List(int portalId, int moduleId, int forumGroupId, int parentForumId, bool fillLastPost) => (IDataReader)SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "Forums_List", moduleId, forumGroupId, parentForumId, fillLastPost);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForums()")]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumCollection Forums_List(int portalId, int moduleId) => new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(moduleId);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumListXML()")]
        public XmlDocument ForumListXML(int portalId, int moduleId) => new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForumListXML(portalId, moduleId);
    }
}
