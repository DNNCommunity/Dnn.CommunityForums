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
namespace DotNetNuke.Modules.ActiveForums.ViewModels
{
    using System;
    using System.Collections.Generic;

    public class Forum
    {
        private readonly DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum;

        public Forum()
        {
            this.forum = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
            this.forum.Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo();
        }

        public Forum(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum)
        {
            this.forum = forum;
        }

        public int ForumId { get => this.forum.ForumID; set => this.forum.ForumID = value; }

        public string ForumName { get => this.forum.ForumName; set => this.forum.ForumName = value; }

        public string ForumDescription { get => this.forum.ForumDesc; set => this.forum.ForumDesc = value; }

        public int PortalId { get => this.forum.PortalId; set => this.forum.PortalId = value; }

        public int ModuleId { get => this.forum.ModuleId; set => this.forum.ModuleId = value; }

        public int ForumGroupId { get => this.forum.ForumGroupId; set => this.forum.ForumGroupId = value; }

        public string ForumGroupName => this.forum.GroupName;

        public int ForumPermissionsId { get => this.forum.PermissionsId; set => this.forum.PermissionsId = value; }

        public int ForumLastTopicId => this.forum.LastTopicId;

        public int ForumLastReplyId => this.forum.LastReplyId;

        public int ForumLastPostID => this.forum.LastPostID;

        public int ForumTotalTopics => this.forum.TotalTopics;

        public int TotalReplies => this.forum.TotalReplies;

        public string ForumSettingsKey { get => this.forum.ForumSettingsKey; set => this.forum.ForumSettingsKey = value; }

        public bool ForumActive { get => this.forum.Active; set => this.forum.Active = value; }

        public bool ForumHidden { get => this.forum.Hidden; set => this.forum.Hidden = value; }

        public int ForumSubscriberCount => this.forum.SubscriberCount;

        public string ForumURL => this.forum.ForumURL;

        public string ForumGroupPrefixURL => this.forum.GroupPrefixURL;

        public DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo ForumSecurity => this.forum.Security;

        public string ForumSecurityCreate => this.forum.Security.Create;

        public string ForumSecurityEdit => this.forum.Security.Edit;

        public string ForumSecurityDelete => this.forum.Security.Delete;

        public string ForumSecurityView => this.forum.Security.View;

        public string ForumSecurityAnnounce => this.forum.Security.Announce;

        public string ForumSecurityModerate => this.forum.Security.Moderate;

        public string ForumSecurityMove => this.forum.Security.Move;

        public string ForumSecuritySplit => this.forum.Security.Split;

        public string ForumSecurityTrust => this.forum.Security.Trust;

        public string ForumSecurityAttach => this.forum.Security.Attach;

        public string ForumSecurityAuthRolesCreate => this.forum.Security.Ban;

        public string ForumSecurityTag => this.forum.Security.Tag;

        public string ForumSecuritySubscribe => this.forum.Security.Subscribe;

        public string ForumPrefixURL => this.forum.PrefixURL;

        public int ParentForumId => this.forum.ParentForumId;

        public string ParentForumName => this.forum.ParentForumName;

        public bool ForumHasProperties => this.forum.HasProperties;

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo> ForumProperties => this.forum.Properties;
    }
}
