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

    public class Topic
    {
        public Topic()
        {
        }

        public Topic(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic)
        {
            this.ModuleId = topic.ModuleId;
            this.ForumId = topic.ForumId;
            this.TopicId = topic.TopicId;
            this.AuthorId = topic.Author?.AuthorId ?? DotNetNuke.Common.Utilities.Null.NullInteger;
            this.AuthorDisplayName = topic.Author?.DisplayName ?? topic.Content.AuthorName ?? string.Empty;
            this.AuthorFirstName = topic.Author?.FirstName ?? string.Empty;
            this.AuthorLastName = topic.Author?.LastName ?? string.Empty;
            this.AuthorUsername = topic.Author?.Username ?? string.Empty;
            this.AuthorEmail = topic.Author?.Email ?? string.Empty;
            this.LastReplyAuthorId = topic.LastReplyAuthor?.AuthorId ?? DotNetNuke.Common.Utilities.Null.NullInteger;
            this.LastReplyAuthorDisplayName = topic.LastReplyAuthor?.DisplayName ?? topic.LastReply?.Content?.AuthorName ?? string.Empty;
            this.LastReplyAuthorFirstName = topic.LastReplyAuthor?.FirstName ?? string.Empty;
            this.LastReplyAuthorLastName = topic.LastReplyAuthor?.LastName ?? string.Empty;
            this.LastReplyAuthorUsername = topic.LastReplyAuthor?.Username ?? string.Empty;
            this.LastReplyAuthorEmail = topic.LastReplyAuthor?.Email ?? string.Empty;
            this.IsLocked = topic.IsLocked;
            this.IsPinned = topic.IsPinned;
            this.IsArchived = topic.IsArchived;
            this.IsApproved = topic.IsApproved;
            this.IsRejected = topic.IsRejected;
            this.IsDeleted = topic.IsDeleted;
            this.StatusId = topic.StatusId;
            this.IsPinned = topic.IsPinned;
            this.AnnounceStart = topic.AnnounceStart;
            this.AnnounceEnd = topic.AnnounceEnd;
            this.Priority = topic.Priority;
            this.Tags = topic.Tags;
            this.Categories = topic.Categories;
            this.SelectedCategories = topic.SelectedCategories;
            this.SelectedCategoriesAsString = topic.SelectedCategoriesAsString;
            this.TopicProperties = topic.TopicProperties;
            this.ForumProperties = topic.Forum.Properties;
            this.Subject = topic.Content?.Subject ?? string.Empty;
            this.Body = topic.Content?.Body ?? string.Empty;
            this.Summary = topic.Content?.Summary ?? string.Empty;
            this.Forum = topic.Forum;
        }

        public int TopicId { get; set; }

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

        public int LastReplyAuthorId { get; set; }

        public string LastReplyAuthorDisplayName { get; set; }

        public string LastReplyAuthorUsername { get; set; }

        public string LastReplyAuthorEmail { get; set; }

        public string LastReplyAuthorFirstName { get; set; }

        public string LastReplyAuthorLastName { get; set; }

        public bool IsLocked { get; set; }

        public bool IsPinned { get; set; }

        public int StatusId { get; set; }

        public bool IsApproved { get; set; }

        public bool IsRejected { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsAnnounce { get => this.AnnounceStart.HasValue; }

        public bool IsArchived { get; set; }

        public DateTime? AnnounceStart { get; set; }

        public DateTime? AnnounceEnd { get; set; }

        public int Priority { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string Summary { get; set; }

        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum { get; set; }

        public string ForumName => this.Forum?.ForumName;

        public string ForumGroupName => this.Forum?.GroupName;

        public string ForumDescription => this.Forum?.ForumDesc;

        public string ForumURL => this.Forum.ForumURL;

        public string Tags { get; set; } = string.Empty;

        public IEnumerable<Entities.TopicInfo.Category> Categories { get; set; }

        public string SelectedCategoriesAsString { get; set; }

        public IEnumerable<Entities.TopicInfo.Category> SelectedCategories { get; set; }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo> TopicProperties { get; set; }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo> ForumProperties { get; set; }
    }
}
