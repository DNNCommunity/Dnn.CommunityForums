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
        
        [IgnoreColumn()]
        internal DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetTopic()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(this.TopicId);
        }

        [IgnoreColumn()]
        public string Subject => Content.Subject;

        [IgnoreColumn()]
        public string Summary => Content.Summary;

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo Content
        {
            get => this.contentInfo ?? (this.contentInfo = this.GetContent());
            set => this.contentInfo = value;
        }
        
        [IgnoreColumn()]
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
            get => this.author ?? (this.author = this.GetAuthor(this.PortalId, this.Content.AuthorId));
            set => this.author = value;
        }
        
        [IgnoreColumn()]
        internal DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo GetAuthor(int portalId, int authorId)
        {
            return new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, authorId);
        }
        
        [IgnoreColumn()]
        internal DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus GetReplyStatusForUser(ForumUserInfo forumUser)
        {
            var canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.Forum.Security.View, forumUser?.UserRoles);

            if (!canView)
            {
                return DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.Forbidden;
            }

            try
            {
                if (this.ReplyId > forumUser?.GetLastTopicReplyRead(this.Topic))
                {
                    return DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.New;
                }

                if (forumUser != null && forumUser.GetIsReplyRead(this))
                {
                    return DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.Read;
                }

                return DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.Unread;
            }
            catch (Exception e)
            {
                /* this is to handle some limited unit testing without retrieving data */
                return DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.Unread;
            }
        }
    }
}
