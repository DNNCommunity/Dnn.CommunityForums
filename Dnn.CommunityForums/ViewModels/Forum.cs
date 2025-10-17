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

namespace DotNetNuke.Modules.ActiveForums.ViewModels
{
    using System.Collections.Generic;

    public class Forum
    {
        private readonly DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum;

        public Forum(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum)
        {
            this.forum = forum;
            if (this.forum.Security == null)
            {
                this.forum.Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo();
            }
        }

        public int Id { get => this.forum.ForumID; set => this.forum.ForumID = value; }

        public string Name { get => this.forum.ForumName; set => this.forum.ForumName = value; }

        public string Description { get => this.forum.ForumDesc; set => this.forum.ForumDesc = value; }

        public int PortalId { get => this.forum.PortalId; set => this.forum.PortalId = value; }

        public int ModuleId { get => this.forum.ModuleId; set => this.forum.ModuleId = value; }

        public int GroupId { get => this.forum.ForumGroupId; set => this.forum.ForumGroupId = value; }

        public string GroupName => this.forum.GroupName;

        public int PermissionsId { get => this.forum.PermissionsId; set => this.forum.PermissionsId = value; }

        public int LastTopicId => this.forum.LastTopicId;

        public int LastReplyId => this.forum.LastReplyId;

        public int LastPostID => this.forum.LastPostID;

        public int TotalTopics => this.forum.TotalTopics;

        public int TotalReplies => this.forum.TotalReplies;

        public string SettingsKey { get => this.forum.ForumSettingsKey; set => this.forum.ForumSettingsKey = value; }

        public bool Active { get => this.forum.Active; set => this.forum.Active = value; }

        public bool Hidden { get => this.forum.Hidden; set => this.forum.Hidden = value; }

        public int SubscriberCount => this.forum.SubscriberCount;

        public string URL => this.forum.ForumURL;

        public string GroupPrefixURL => this.forum.GroupPrefixURL;

        public DotNetNuke.Modules.ActiveForums.ViewModels.Permissions Security => new DotNetNuke.Modules.ActiveForums.ViewModels.Permissions(this.forum.Security);

        public string PrefixURL => this.forum.PrefixURL;

        public int ParentForumId => this.forum.ParentForumId;

        public string ParentForumName => this.forum.ParentForumName;

        public bool HasProperties => this.forum.HasProperties;

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo> Properties => this.forum.Properties;

        /// <summary>
        /// Maps the current Forum view model to a ForumInfo entity.
        /// </summary>
        /// <returns>A new instance of ForumInfo with properties set from this view model.</returns>
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo ToForumInfo()
        {
            var forumInfo = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
            forumInfo.ForumID = this.Id;
            forumInfo.ForumName = this.Name;
            forumInfo.ForumDesc = this.Description;
            forumInfo.PortalId = this.PortalId;
            forumInfo.ModuleId = this.ModuleId;
            forumInfo.ForumGroupId = this.GroupId;
            forumInfo.PermissionsId = this.PermissionsId;
            forumInfo.LastTopicId = this.LastTopicId;
            forumInfo.LastReplyId = this.LastReplyId;
            forumInfo.TotalTopics = this.TotalTopics;
            forumInfo.TotalReplies = this.TotalReplies;
            forumInfo.ForumSettingsKey = this.SettingsKey;
            forumInfo.Active = this.Active;
            forumInfo.Hidden = this.Hidden;
            forumInfo.PrefixURL = this.PrefixURL;
            forumInfo.ParentForumId = this.ParentForumId;
            forumInfo.HasProperties = this.HasProperties;
            forumInfo.Properties = this.Properties != null ? new List<DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo>(this.Properties) : null;

            return forumInfo;
        }
    }
}
