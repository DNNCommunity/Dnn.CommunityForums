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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web.Caching;
using System.Xml;

using DotNetNuke.Collections;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.UI.UserControls;

namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Scheduled for removal in 09.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.TopicInfo")]
    public class TopicInfo : DotNetNuke.Modules.ActiveForums.Entities.TopicInfo { }
}

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_Topics")]
    [PrimaryKey("TopicId", AutoIncrement = true)]
    public class TopicInfo
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

        private List<Category> _categories;

        private DotNetNuke.Modules.ActiveForums.Entities.ContentInfo _contentInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo _forumInfo;
        private DotNetNuke.Modules.ActiveForums.Author _Author;
        private int _forumId = -1;
        private string _tags = string.Empty;
        private string _selectedcategories;

        public int TopicId { get; set; }

        [IgnoreColumn()]
        public int ForumId
        {
            get
            {
                // TODO : clean this up to use DAL2
                if (this._forumId < 1 && this.TopicId > 0)
                {
                    this._forumId = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forum_GetByTopicId(this.TopicId);
                }

                return this._forumId;
            }
            set => this._forumId = value;
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
        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo Content
        {
            get => this._contentInfo ?? (this._contentInfo = this.GetContent());
            set => this._contentInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ContentInfo GetContent()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(this.ContentId);
        }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum
        {
            get => this._forumInfo ?? (this._forumInfo = this.GetForum());
            set => this._forumInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetForum()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.ForumId); /* can't get using moduleId since ModuleId comes from Forum */
        }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Author Author
        {
            get => this._Author ?? (this._Author = this.GetAuthor());
            set => this._Author = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Author GetAuthor()
        {
            this._Author = new DotNetNuke.Modules.ActiveForums.Author();
            this._Author.AuthorId = this.Content.AuthorId;
            var userInfo = DotNetNuke.Entities.Users.UserController.Instance.GetUser(this.PortalId, this.Content.AuthorId);
            if (userInfo != null)
            {
                this._Author.Email = userInfo?.Email;
                this._Author.FirstName = userInfo?.FirstName;
                this._Author.LastName = userInfo?.LastName;
                this._Author.DisplayName = userInfo?.DisplayName;
                this._Author.Username = userInfo?.Username;
            }
            else
            {
                this._Author.DisplayName = this.Content.AuthorId > 0 ? Utilities.GetSharedResource("[RESX:DeletedUser]") : Utilities.GetSharedResource("[RESX:Anonymous]");
            }

            return this._Author;
        }

        [IgnoreColumn()]
        public string Tags
        {
            get
            {
                if (string.IsNullOrEmpty(this._tags))
                {
                    this._tags = string.Join(",", new DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController().GetForTopic(this.TopicId).Select(t => t.Tag.TagName));
                    if (string.IsNullOrEmpty(this._tags))
                    {
                        this._tags = string.Empty;
                    }
                }

                return this._tags;
            }
        }

        [IgnoreColumn()]
        public IEnumerable<Category> Categories
        {
            // TODO: Clean this up
            get
            {
                if (this._categories == null)
                {
                    this._categories = new DotNetNuke.Modules.ActiveForums.Controllers.CategoryController().Find("WHERE ForumId = @0 OR ForumGroupid = @1", this.ForumId, this.Forum.ForumGroupId).Select(c => { return new Category(c.TagId, c.TagName, false); }).ToList(); ;
                    var topicCategoryIds = new DotNetNuke.Modules.ActiveForums.Controllers.TopicCategoryController().GetForTopic(this.TopicId).Select(t => t.TagId);
                    topicCategoryIds.ForEach(tc => this._categories.Where(c => c.id == tc).ForEach(c => c.selected = true));
                }

                return this._categories;
            }
        }

        [IgnoreColumn()]
        public IEnumerable<Category> SelectedCategories => this.Categories.Where(c => c.selected).ToList();

        [IgnoreColumn()]
        public string SelectedCategoriesAsString
        {
            get
            {
                if (this._selectedcategories == null)
                {
                    this._selectedcategories = string.Join(";", this.SelectedCategories.Select(c => c.id.ToString()));
                }

                return this._selectedcategories;
            }

            set
            {
                this._selectedcategories = value;
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
