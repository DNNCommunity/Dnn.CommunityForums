using DotNetNuke.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_Replies")]
    [PrimaryKey("ReplyId")]
    [Scope("ModuleId")]
    [Cacheable("activeforums_Replies", CacheItemPriority.Low)]
    public partial class ReplyInfo
    {
        private DotNetNuke.Modules.ActiveForums.Entities.TopicInfo _topicInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ContentInfo _contentInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo _forumInfo;
        private DotNetNuke.Modules.ActiveForums.Author _Author;
        private int forumId; 
        public int ReplyId { get; set; }
        public int TopicId { get; set; }
        public int ReplyToId { get; set; }
        public int ContentId { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
        public int StatusId { get; set; }
        public bool IsDeleted { get; set; }
        [IgnoreColumn()]
        public int PortalId { get => Forum.PortalId; }
        [IgnoreColumn()]
        public int ModuleId { get => Forum.ModuleId; }
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic
        {
            get => _topicInfo ?? (_topicInfo = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId));
            set => _topicInfo = value;
        }
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo Content
        {
            get
            {
                if (_contentInfo == null && ContentId > 0)
                {
                    _contentInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(ContentId);
                }
                return _contentInfo;
            }

            set => _contentInfo = value;
        }
        [IgnoreColumn()]
        public int ForumId => Topic.ForumId;
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum => Topic.Forum;
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Author Author
        {
            get
            {
                if (_Author == null)
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
                }
                return _Author;
            }
            set
            { _Author = value; }
        }
    }
}
