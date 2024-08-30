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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;

    [Obsolete("Deprecated in Community Forums. Scheduled for removal in 09.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.TopicInfo")]
    public class TopicInfo : DotNetNuke.Modules.ActiveForums.Entities.TopicInfo { }
}

#pragma warning disable SA1403 // File may only contain a single namespace
namespace DotNetNuke.Modules.ActiveForums.Entities
#pragma warning restore SA1403 // File may only contain a single namespace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.ComponentModel.DataAnnotations;

    [TableName("activeforums_Topics")]
    [PrimaryKey("TopicId", AutoIncrement = true)]
#pragma warning disable SA1402 // File may only contain a single type
    public class TopicInfo : IPostInfo
#pragma warning restore SA1402 // File may only contain a single type
       
    {
        [IgnoreColumn()]
        public class Category
        {
            public int id;
            public string name;
            public bool selected;

            [IgnoreColumn()]
            public Category(int id, string name, bool selected)
            {
                this.id = id;
                this.name = name;
                this.selected = selected;
            }
        }

        private List<Category> categories;

        private DotNetNuke.Modules.ActiveForums.Entities.ContentInfo contentInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo lastReply;
        private DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author;
        private DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo lastReplyAuthor;
        private int forumId = -1;
        private string tags = string.Empty;
        private string selectedcategories;
        
        public int TopicId { get; set; }

        [IgnoreColumn()]
        public int PostId { get => this.TopicId; }

        [IgnoreColumn()]
        public int ForumId
        {
            get
            {
                // TODO : clean this up to use DAL2
                if (this.forumId < 1 && this.TopicId > 0)
                {
                    this.forumId = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forum_GetByTopicId(this.TopicId);
                }

                return this.forumId;
            }
            set => this.forumId = value;
        }

        [IgnoreColumn()]
        public int PortalId { get => this.Forum.PortalId; }

        [IgnoreColumn()]
        public int ModuleId { get => this.Forum.ModuleId; }

        public int ContentId { get; set; }

        public int ViewCount { get; set; }

        public int ReplyCount { get; set; }

        public bool IsLocked { get; set; }

        public bool IsPinned { get; set; }

        public string TopicIcon { get; set; }

        public int StatusId { get; set; }

        public bool IsApproved { get; set; }

        public bool IsRejected { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsAnnounce { get; set; }

        public bool IsArchived { get; set; }

        public DateTime AnnounceStart { get; set; }

        public DateTime AnnounceEnd { get; set; }

        public TopicTypes TopicType { get; set; }

        public int Priority { get; set; } = 0;

        [ColumnName("URL")]
        public string TopicUrl { get; set; } = string.Empty;

        [IgnoreColumn()]
        public string URL => !string.IsNullOrEmpty(this.TopicUrl) && !string.IsNullOrEmpty(this.ForumURL) ? this.ForumURL + this.TopicUrl : string.Empty;

        [IgnoreColumn()]
        public string ForumURL => !string.IsNullOrEmpty(this.Forum.PrefixURL) && !string.IsNullOrEmpty(this.TopicUrl) ? "/" + this.Forum.PrefixURL + "/" : string.Empty;

        public int NextTopic { get; set; }

        public int PrevTopic { get; set; }

        public string TopicData { get; set; } = string.Empty;
        
        [IgnoreColumn()]
        public int LastReplyId { get; set; }
        
        [IgnoreColumn()]
        public int Rating { get; set; }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic
        {
            get => this;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetTopic()
        {
            return this;
        }
        
        [IgnoreColumn()]
        public int SubscriberCount => new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(portalId: this.PortalId, moduleId: this.ModuleId, forumId: this.ForumId, topicId: this.TopicId);

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo Content
        {
            get => this.contentInfo ?? (this.contentInfo = this.GetContent());
            set => this.contentInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ContentInfo GetContent()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(this.ContentId);
        }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum
        {
            get => this.forumInfo ?? (this.forumInfo = this.GetForum());
            set => this.forumInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetForum()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.ForumId); /* can't get using moduleId since ModuleId comes from Forum */
        }
        
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo Author
        {
            get => this.author ?? (this.author = this.GetAuthor(this.PortalId, this.ModuleId, this.Content.AuthorId));
            set => this.author = value;
        }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo LastReply
        {
            get => this.lastReply ?? (this.lastReply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(this.LastReplyId));
            set => this.lastReply = value;
        }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo LastReplyAuthor
        {
            get => this.lastReplyAuthor ?? (this.lastReplyAuthor = this.lastReply == null ? null : this.GetAuthor(this.PortalId, this.ModuleId, this.lastReply.Content.AuthorId));
            set => this.lastReplyAuthor = value;
        }

        [IgnoreColumn()]
        internal DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo GetAuthor(int portalId, int moduleId, int authorId)
        {
            return new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, moduleId, authorId);
        }

        [IgnoreColumn()]
        public string Tags
        {
            get
            {
                if (string.IsNullOrEmpty(this.tags))
                {
                    this.tags = string.Join(",", new DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController().GetForTopic(this.TopicId).Select(t => t.Tag.TagName));
                    if (string.IsNullOrEmpty(this.tags))
                    {
                        this.tags = string.Empty;
                    }
                }

                return this.tags;
            }
        }

        [IgnoreColumn()]
        public IEnumerable<Category> Categories
        {
            // TODO: Clean this up
            get
            {
                if (this.categories == null)
                {
                    this.categories = new DotNetNuke.Modules.ActiveForums.Controllers.CategoryController().Find("WHERE ForumId = @0 OR ForumGroupid = @1", this.ForumId, this.Forum.ForumGroupId).Select(c => { return new Category(c.TagId, c.TagName, false); }).ToList();
                    var topicCategoryIds = new DotNetNuke.Modules.ActiveForums.Controllers.TopicCategoryController().GetForTopic(this.TopicId).Select(t => t.TagId);
                    topicCategoryIds.ForEach(tc => this.categories.Where(c => c.id == tc).ForEach(c => c.selected = true));
                }

                return this.categories;
            }
        }

        [IgnoreColumn()]
        public IEnumerable<Category> SelectedCategories => this.Categories.Where(c => c.selected).ToList();

        [IgnoreColumn()]
        public string SelectedCategoriesAsString
        {
            get
            {
                if (this.selectedcategories == null)
                {
                    this.selectedcategories = string.Join(";", this.SelectedCategories.Select(c => c.id.ToString()));
                }

                return this.selectedcategories;
            }

            set
            {
                this.selectedcategories = value;
            }
        }

        [IgnoreColumn()]
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo> TopicProperties
        {
            set
            {
                this.TopicData = DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Serialize(this.Forum, value);
            }

            get
            {
                if (this.TopicData == string.Empty)
                {
                    return null;
                }
                else
                {
                    return DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Deserialize(this.TopicData);
                }
            }
        }
    }
}
