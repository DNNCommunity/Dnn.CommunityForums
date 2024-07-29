namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Caching;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using global::DotNetNuke.ComponentModel.DataAnnotations;

    [TableName("activeforums_Replies")]
    [PrimaryKey("ReplyId")]
    [Scope("ModuleId")]
    [Cacheable("activeforums_Replies", CacheItemPriority.Low)]
    public partial class ReplyInfo
    {
        private DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topicInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ContentInfo contentInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author;
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
            get => this.topicInfo ?? (this.topicInfo = this.GetTopic());
            set => this.topicInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetTopic()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(this.TopicId);
        }

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
            get => this.author ?? (this.GetAuthor());
            set => this.author = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo GetAuthor()
        {
            this.author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(this.PortalId, this.Content.AuthorId));
            if (this.author == null)
            {
                this.author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo();
                this.author.AuthorId = this.Content.AuthorId;
                this.author.DisplayName = this.Content.AuthorId > 0 ? Utilities.GetSharedResource("[RESX:DeletedUser]") : Utilities.GetSharedResource("[RESX:Anonymous]");
            }

            return this.author;
        }
    }
}
