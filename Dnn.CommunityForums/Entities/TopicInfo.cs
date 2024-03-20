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
    [Cacheable("activeforums_Topics", CacheItemPriority.Low)]
    public class TopicInfo
    {
        private DotNetNuke.Modules.ActiveForums.Entities.ContentInfo _contentInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo _forumInfo;
        private DotNetNuke.Modules.ActiveForums.Author _Author;
        private int forumId;
        private string _tags = string.Empty;
        public class Category
        {
            public int id;
            public string name;
            public bool selected;

            public Category(int id, string name, bool selected)
            {
                this.id = id;
                this.name = name;
                this.selected = selected;
            }
        }
        private IEnumerable<Category> _categories;

        private int _forumId;
        public int TopicId { get; set; }
        [IgnoreColumn()]
        public int ForumId 
        { 
            get
            {
                //TODO : clean this up to use DAL2
                if (_forumId < 1)
                {
                    _forumId = new Data.ForumsDB().Forum_GetByTopicId(TopicId);
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
            get
            {
                if (_contentInfo == null)
                {
                    if (ContentId > 0)
                    {
                        _contentInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(ContentId);
                    }
                    if (_contentInfo == null)
                    {
                        _contentInfo = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo();
                    }
                }
                return _contentInfo; 
            }
                
                set => _contentInfo = value;
            }
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum
        {
            get
            {
                if (_forumInfo == null)
                {
                    if (ForumId > 0)
                    {
                        _forumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ForumId);
                    }
                }
                if (_forumInfo == null)
                {
                    _forumInfo = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
                }
                return _forumInfo;
            }

            set => _forumInfo = value;
        }
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Author Author
        {
            get
            {
                if (_Author == null)
                {
                    _Author = new DotNetNuke.Modules.ActiveForums.Author();
                }
                if (Content?.AuthorId != null)
                {
                    _Author.AuthorId = Content.AuthorId;
                    var userInfo = DotNetNuke.Entities.Users.UserController.Instance.GetUser(Forum.PortalId, Content.AuthorId);
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
                }
                else
                {
                    _Author.DisplayName = "Unknown";
                }
                return _Author;
            }
            set => _Author = value;
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
            get
            {
                if (_categories == null)
                {
                    _categories = new DotNetNuke.Modules.ActiveForums.Controllers.CategoryController().Find("WHERE ForumId = @0 OR ForumGroupid = @1", ForumId, Forum.ForumGroupId).Select(c => { return new Category(c.TagId, c.TagName, false); });
                    var topicCategories = new DotNetNuke.Modules.ActiveForums.Controllers.TopicCategoryController().GetForTopic(TopicId).Select(t => t.TagId);
                    topicCategories.ForEach(tc => _categories.Where(c => c.id == tc).ForEach(c => c.selected = true));

                }
                return _categories;
            }
        }
        [IgnoreColumn()]
        public string CategoriesAsString
        {
            get
            {
                return string.Join("|",Categories.Select(c => c.name));
            }
        }
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicPropertiesInfo TopicProperties
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
