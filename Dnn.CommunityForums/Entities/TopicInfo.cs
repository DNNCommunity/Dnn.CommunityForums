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
using System.Xml;
using DotNetNuke.ComponentModel.DataAnnotations;
using System.Web.Caching;
using DotNetNuke.UI.UserControls;
using System.Runtime.Remoting.Messaging;
using System.Linq;
using DotNetNuke.Collections;
using System.Text;

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
                //TODO : clean this up to use DAL2
                if (_forumId < 1 && TopicId > 0)
                {
                    _forumId = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forum_GetByTopicId(TopicId);
                }
                return _forumId;
            }
            set => _forumId = value;
        }
        [IgnoreColumn()]
        public int PortalId { get => Forum.PortalId; }
        [IgnoreColumn()]
        public int ModuleId { get => Forum.ModuleId; }
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
        public string URL => !(string.IsNullOrEmpty(TopicUrl)) && !(string.IsNullOrEmpty(ForumURL)) ? ForumURL + TopicUrl : string.Empty;
        [IgnoreColumn()]
        public string ForumURL => !(string.IsNullOrEmpty(Forum.PrefixURL)) && !(string.IsNullOrEmpty(TopicUrl)) ? "/" + Forum.PrefixURL + "/" : string.Empty;
        public int NextTopic { get; set; }
        public int PrevTopic { get; set; }
        public string TopicData { get; set; } = string.Empty;

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo Content
        {
            get => _contentInfo ?? (_contentInfo = GetContent());
            set => _contentInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ContentInfo GetContent()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(ContentId);
        }
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum
        {
            get => _forumInfo ?? (_forumInfo = GetForum());
            set => _forumInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetForum()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ForumId); /* can't get using moduleId since ModuleId comes from Forum */
        }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Author Author
        {
            get => _Author ?? (_Author = GetAuthor());
            set => _Author = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Author GetAuthor()
        {
            _Author = new DotNetNuke.Modules.ActiveForums.Author();
            _Author.AuthorId = Content.AuthorId;
            var userInfo = DotNetNuke.Entities.Users.UserController.Instance.GetUser(PortalId, Content.AuthorId);
            if (userInfo != null)
            {
                _Author.Email = userInfo?.Email;
                _Author.FirstName = userInfo?.FirstName;
                _Author.LastName = userInfo?.LastName;
                _Author.DisplayName = userInfo?.DisplayName;
                _Author.Username = userInfo?.Username;
            }
            else
            {
                _Author.DisplayName = Content.AuthorId > 0 ? "Deleted User" : "Anonymous";
            }
            return _Author;
        }
        [IgnoreColumn()]
        public string Tags
        {
            get
            {
                if (string.IsNullOrEmpty(_tags))
                {
                    _tags = string.Join(",",new DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController().GetForTopic(TopicId).Select(t => t.Tag.TagName));
                    if (string.IsNullOrEmpty(_tags))
                    {
                        _tags = string.Empty;
                    }
                }
                return _tags;
            }
        }
       [IgnoreColumn()]
        public IEnumerable<Category> Categories
        {
            //TODO: Clean this up
            get
            {
                if (_categories == null)
                {
                    _categories = new DotNetNuke.Modules.ActiveForums.Controllers.CategoryController().Find("WHERE ForumId = @0 OR ForumGroupid = @1", ForumId, Forum.ForumGroupId).Select(c => { return new Category(c.TagId, c.TagName, false); }).ToList(); ;
                    var topicCategoryIds = new DotNetNuke.Modules.ActiveForums.Controllers.TopicCategoryController().GetForTopic(TopicId).Select(t => t.TagId);
                    topicCategoryIds.ForEach(tc => _categories.Where(c => c.id == tc).ForEach(c => c.selected = true));
                }
                return _categories;
            }
        }
        [IgnoreColumn()]
        public IEnumerable<Category> SelectedCategories => Categories.Where(c => c.selected).ToList();
        [IgnoreColumn()]
        public string SelectedCategoriesAsString
        {
            get
            {
                if (_selectedcategories == null)
                {
                    _selectedcategories = string.Join(";", SelectedCategories.Select(c => c.id.ToString()));
                }
                return _selectedcategories;
            }
            set
            {
                _selectedcategories = value;
            }
        }

        [IgnoreColumn()]
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo> TopicProperties
        {
            set
            {
                TopicData = DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Serialize(Forum, value);
            }
            get
            {
                    if (TopicData == string.Empty)
                {
                    return null;
                }
                else
                {
                    return DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Deserialize(TopicData);
                }
            }
        }
    }

}
