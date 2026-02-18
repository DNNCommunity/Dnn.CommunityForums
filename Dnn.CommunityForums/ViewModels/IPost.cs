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
    using System;
    using System.Collections.Generic;

    public class IPost
    {
        public IPost()
        {
        }

        public IPost(DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post)
        {
            this.ModuleId = post.ModuleId;
            this.ForumId = post.ForumId;
            this.ReplyId = post.ReplyId;
            this.ContentId = post.ContentId;
            this.TopicId = post.TopicId;
            this.AuthorId = post.Author?.AuthorId ?? DotNetNuke.Common.Utilities.Null.NullInteger;
            this.AuthorDisplayName = post.Author?.DisplayName ?? post.Content.AuthorName ?? string.Empty;
            this.AuthorFirstName = post.Author?.FirstName ?? string.Empty;
            this.AuthorLastName = post.Author?.LastName ?? string.Empty;
            this.AuthorUsername = post.Author?.Username ?? string.Empty;
            this.AuthorEmail = post.Author?.Email ?? string.Empty;
            this.IsLocked = post.Topic.IsLocked;
            this.IsPinned = post.Topic.IsPinned;
            this.IsArchived = post.Topic.IsArchived;
            this.IsApproved = post.IsApproved;
            this.IsRejected = post.IsRejected;
            this.IsDeleted = post.IsDeleted;
            this.StatusId = post.StatusId;
            this.IsPinned = post.Topic.IsPinned;
            this.AnnounceStart = post.Topic.AnnounceStart;
            this.AnnounceEnd = post.Topic.AnnounceEnd;
            this.IsAnnounce = post.Topic.IsAnnounce;
            this.Priority = post.Topic.Priority;
            this.Tags = post.Topic.Tags;
            this.Categories = post.Topic.Categories;
            this.SelectedCategories = post.Topic.SelectedCategories;
            this.SelectedCategoriesAsString = post.Topic.SelectedCategoriesAsString;
            this.TopicProperties = post.Topic.TopicProperties;
            this.ForumProperties = post.Forum.Properties;
            this.Subject = post.Content?.Subject ?? string.Empty;
            this.Body = post.Content?.Body ?? string.Empty;
            this.Summary = post.Content?.Summary ?? string.Empty;
            this.Forum = post.Forum;
            this.TopicUrl = post.Topic.TopicUrl;
            this.DateCreated = post.Content?.DateCreated;
            this.DateUpdated = post.Content?.DateUpdated;
            this.LikeCount = post.LikeCount;
        }

        public int TopicId { get; set; }

        public int ContentId { get; set; }

        public int ReplyId { get; set; }

        public int ForumId { get; set; }

        public int ForumGroupId { get; set; }

        public int ModuleId { get; set; }

        public int PostId => this.TopicId;

        public int AuthorId { get; set; }

        public string AuthorDisplayName { get; set; }

        public string AuthorUsername { get; set; }

        public string AuthorEmail { get; set; }

        public string AuthorFirstName { get; set; }

        public string AuthorLastName { get; set; }

        public bool IsLocked { get; set; }

        public bool IsPinned { get; set; }

        public int StatusId { get; set; }

        public bool IsApproved { get; set; }

        public bool IsRejected { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsAnnounce { get; set; }

        public bool IsArchived { get; set; }

        public DateTime? AnnounceStart { get; set; }

        public DateTime? AnnounceEnd { get; set; }

        public int Priority { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string Summary { get; set; }

        public string TopicUrl { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateUpdated { get; set; }

        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum { get; set; }

        public string ForumName => this.Forum?.ForumName;

        public string ForumGroupName => this.Forum?.GroupName;

        public string ForumDescription => this.Forum?.ForumDesc;

        public string ForumURL => this.Forum.ForumURL;

        public string Url => new ControlUtils().BuildUrl(this.Forum.PortalSettings.PortalId, this.Forum.GetTabId(), this.Forum.ModuleId, this.Forum.ForumGroup.PrefixURL, this.Forum.PrefixURL, this.Forum.ForumGroupId, this.Forum.ForumID, this.TopicId, this.TopicUrl, -1, -1, string.Empty, 1, this.ContentId, this.Forum.SocialGroupId);

        public int LikeCount {get; set; }

        public string Tags { get; set; } = string.Empty;

        public IEnumerable<Entities.TopicInfo.Category> Categories { get; set; }

        public string SelectedCategoriesAsString { get; set; }

        public IEnumerable<Entities.TopicInfo.Category> SelectedCategories { get; set; }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo> TopicProperties { get; set; }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo> ForumProperties { get; set; }
    }
}
