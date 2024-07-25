namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Caching;

    using DotNetNuke.ComponentModel.DataAnnotations;

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

        [IgnoreColumn()]
        public int ForumId
        {
            get
            {
                // TODO : clean this up to use DAL2
                if (this.forumId < 1)
                {
                    this.forumId = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forum_GetByTopicId(this.TopicId);
                }

                return this.forumId;
            }
            set => this.forumId = value;
        }

        public int ReplyId { get; set; }

        public int TopicId { get; set; }

        public int ReplyToId { get; set; }

        public int ContentId { get; set; }

        public bool IsApproved { get; set; }

        public bool IsRejected { get; set; }

        public int StatusId { get; set; }

        public bool IsDeleted { get; set; }

        [IgnoreColumn()]
        public int PortalId { get => this.Forum.PortalId; }

        [IgnoreColumn()]
        public int ModuleId { get => this.Forum.ModuleId; }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic
        {
            get => this._topicInfo ?? (this._topicInfo = this.GetTopic());
            set => this._topicInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetTopic()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(this.TopicId);
        }

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
            get => this._Author ?? (this.GetAuthor());
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
                this._Author.DisplayName = this.Content.AuthorId > 0 ? Utilities.GetSharedResource("[RESX:DeletedUser]") : Utilities.GetSharedResource("[RESX:Anonymous]"); ;
            }

            return this._Author;
        }
    }
}
