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
    using System.Web.UI.Design;

    public class Topic
    {
        private readonly DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic;

        public Topic()
        {
            this.topic = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo
            {
                Author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(this.ModuleId),
                LastReplyAuthor = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(this.ModuleId),
                Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo(),
            };
        }

        public Topic(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic)
        {
            this.topic = topic;
            this.Tags = topic.Tags;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo TopicInfo() => this.topic;

        public int TopicId { get => this.topic.TopicId; set => this.topic.TopicId = value; }

        public int ForumId { get => this.topic.ForumId; set => this.topic.ForumId = value; }

        public int PortalId => this.topic.Forum.PortalId;

        public int ModuleId => this.topic.Forum.ModuleId;

        public int PostId => this.TopicId;

        public int AuthorId { get => this.topic.Author != null ? this.topic.Author.AuthorId : 0; set => this.topic.Author.AuthorId = value; }

        public string AuthorDisplayName { get => this.topic.Author != null ? this.topic.Author.DisplayName : string.Empty; set => this.topic.Author.DisplayName = value; }

        public string AuthorUsername { get => this.topic.Author != null ? this.topic.Author.Username : string.Empty; set => this.topic.Author.Username = value; }

        public string AuthorEmail { get => this.topic.Author != null ? this.topic.Author.Email : string.Empty; set => this.topic.Author.Email = value; }

        public string AuthorFirstName { get => this.topic.Author != null ? this.topic.Author.FirstName : string.Empty; set => this.topic.Author.FirstName = value; }

        public string AuthorLastName { get => this.topic.Author != null ? this.topic.Author.LastName : string.Empty; set => this.topic.Author.LastName = value; }

        public int LastReplyAuthorId { get => this.topic.LastReplyAuthor != null ? this.topic.LastReplyAuthor.AuthorId : 0; set => this.topic.LastReplyAuthor.AuthorId = value; }

        public string LastReplyAuthorDisplayName { get => this.topic.LastReplyAuthor != null ? this.topic.LastReplyAuthor.DisplayName : string.Empty; set => this.topic.LastReplyAuthor.DisplayName = value; }

        public string LastReplyAuthorUsername { get => this.topic.LastReplyAuthor != null ? this.topic.LastReplyAuthor.Username : string.Empty; set => this.topic.LastReplyAuthor.Username = value; }

        public string LastReplyAuthorEmail { get => this.topic.LastReplyAuthor != null ? this.topic.LastReplyAuthor.Email : string.Empty; set => this.topic.LastReplyAuthor.Email = value; }

        public string LastReplyAuthorFirstName { get => this.topic.LastReplyAuthor != null ? this.topic.LastReplyAuthor.FirstName : string.Empty; set => this.topic.LastReplyAuthor.FirstName = value; }

        public string LastReplyAuthorLastName { get => this.topic.LastReplyAuthor != null ? this.topic.LastReplyAuthor.LastName : string.Empty; set => this.topic.LastReplyAuthor.LastName = value; }

        public int ContentId { get => this.topic.ContentId; set => this.topic.ContentId = value; }

        public int ViewCount { get => this.topic.ViewCount; set => this.topic.ViewCount = value; }

        public int ReplyCount { get => this.topic.ReplyCount; set => this.topic.ReplyCount = value; }

        public bool IsLocked { get => this.topic.IsLocked; set => this.topic.IsLocked = value; }

        public bool IsPinned { get => this.topic.IsPinned; set => this.topic.IsPinned = value; }

        public string TopicIcon { get => this.topic.TopicIcon; set => this.topic.TopicIcon = value; }

        public int StatusId { get => this.topic.StatusId; set => this.topic.StatusId = value; }

        public bool IsApproved { get => this.topic.IsApproved; set => this.topic.IsApproved = value; }

        public bool IsRejected { get => this.topic.IsRejected; set => this.topic.IsRejected = value; }

        public bool IsDeleted { get => this.topic.IsDeleted; set => this.topic.IsDeleted = value; }

        public bool IsAnnounce { get => this.topic.IsAnnounce; set => this.topic.IsAnnounce = value; }

        public bool IsArchived { get => this.topic.IsArchived; set => this.topic.IsArchived = value; }

        public DateTime AnnounceStart { get => this.topic.AnnounceStart; set => this.topic.AnnounceStart = value; }

        public DateTime AnnounceEnd { get => this.topic.AnnounceEnd; set => this.topic.AnnounceEnd = value; }

        public TopicTypes TopicType { get => this.topic.TopicType; set => this.topic.TopicType = value; }

        public int Priority { get => this.topic.Priority; set => this.topic.Priority = value; }

        public string TopicUrl { get => this.topic.TopicUrl; set => this.topic.TopicUrl = value; }

        public int LastReplyId => this.topic.LastReplyId;

        public int Rating { get => this.topic.Rating; set => this.topic.Rating = value; }

        public string URL => this.topic.URL;

        public string Subject { get => this.topic?.Content?.Subject != null ? this.topic.Content.Subject : string.Empty; set => this.topic.Content.Subject = value; }

        public string Body { get => this.topic?.Content?.Body != null ? this.topic.Content.Body : string.Empty; set => this.topic.Content.Body = value; }

        public string Summary { get => this.topic?.Content?.Summary != null ? this.topic.Content.Summary : string.Empty; set => this.topic.Content.Summary = value; }

        public int SubscriberCount => this.topic.SubscriberCount;

        public string ForumName => this.topic.Forum.ForumName;

        public string ForumGroupName => this.topic.Forum.GroupName;

        public string ForumDescription => this.topic.Forum.ForumDesc;

        public string ForumURL => this.topic.ForumURL;

        public int NextTopic => this.topic.NextTopic;

        public int PrevTopic => this.topic.PrevTopic;

        public string TopicData { get => this.topic.TopicData; set => this.topic.TopicData = value; }

        public string Tags { get; set; } = string.Empty;

        public IEnumerable<Entities.TopicInfo.Category> Categories => this.topic.Categories;

        public string SelectedCategoriesAsString { get => this.topic.SelectedCategoriesAsString; set => this.topic.SelectedCategoriesAsString = value; }

        public IEnumerable<Entities.TopicInfo.Category> SelectedCategories => this.topic.SelectedCategories;

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo> TopicProperties { get => this.topic.TopicProperties; set => this.topic.TopicProperties = value; }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo> ForumProperties => this.topic.Forum.Properties;

        public int ForumGroupId => this.topic.Forum.ForumGroupId;

        public int ForumPermissionsId => this.topic.Forum.PermissionsId;

        public int ForumLastTopicId => this.topic.Forum.LastTopicId;

        public int ForumLastReplyId => this.topic.Forum.LastReplyId;

        public int ForumLastPostID => this.topic.Forum.LastPostID;

        public int ForumTotalTopics => this.topic.Forum.TotalTopics;

        public int ForumTotalReplies => this.topic.Forum.TotalReplies;

        public string ForumSettingsKey => this.topic.Forum.ForumSettingsKey;

        public bool ForumActive => this.topic.Forum.Active;

        public bool ForumHidden => this.topic.Forum.Hidden;

        public int ForumSubscriberCount => this.topic.Forum.SubscriberCount;

        public string ForumGroupPrefixURL => this.topic.Forum.ForumGroup.PrefixURL;

        public DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo ForumSecurity => this.topic.Forum.Security;

        public string ForumSecurityCreate => this.topic.Forum.Security.Create;

        public string ForumSecurityEdit => this.topic.Forum.Security.Edit;

        public string ForumSecurityDelete => this.topic.Forum.Security.Delete;

        public string ForumSecurityView => this.topic.Forum.Security.View;

        public string ForumSecurityAnnounce => this.topic.Forum.Security.Announce;

        public string ForumSecurityModerate => this.topic.Forum.Security.Moderate;

        public string ForumSecurityMove => this.topic.Forum.Security.Move;

        public string ForumSecuritySplit => this.topic.Forum.Security.Split;

        public string ForumSecurityTrust => this.topic.Forum.Security.Trust;

        public string ForumSecurityAttach => this.topic.Forum.Security.Attach;

        public string ForumSecurityAuthRolesCreate => this.topic.Forum.Security.Ban;

        public string ForumSecurityTag => this.topic.Forum.Security.Tag;

        public string ForumSecuritySubscribe => this.topic.Forum.Security.Subscribe;

        public string ForumPrefixURL => this.topic.Forum.PrefixURL;

        public int ParentForumId => this.topic.Forum.ParentForumId;

        public string ParentForumName => this.topic.Forum.ParentForumName;

        public bool ForumHasProperties => this.topic.Forum.HasProperties;
    }
}
