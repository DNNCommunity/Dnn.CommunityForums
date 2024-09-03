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
    public partial class ReplyInfo : DotNetNuke.Modules.ActiveForums.Entities.IPostInfo
    {
        private DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topicInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ContentInfo contentInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author;

        [IgnoreColumn()]
        public int ForumId
        {
            get => this.Topic.ForumId;
            set => this.Topic.ForumId = value;
        }

        [IgnoreColumn()]
        public int PostId {  get => this.ReplyId; }

        [IgnoreColumn] 
        public bool IsTopic => false;

        [IgnoreColumn] 
        public bool IsReply => true;

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
            get => this.Topic.Forum;
            set => this.Topic.Forum = value;
        }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo Author
        {
            get => this.author ?? (this.GetAuthor());
            set => this.author = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo GetAuthor()
        {
            this.author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(this.PortalId, this.Content.AuthorId));
            if (this.author == null)
            {
                this.author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo();
                this.author.AuthorId = this.Content.AuthorId;
                this.author.DisplayName = this.Content.AuthorId > 0 ? Utilities.GetSharedResource("[RESX:DeletedUser]") : Utilities.GetSharedResource("[RESX:Anonymous]");
            }

            return this.author;
        }

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

        public DotNetNuke.Modules.ActiveForums.Enums.PostStatus GetPostStatusForUser(ForumUserInfo forumUser)
        {
            switch (this.GetReplyStatusForUser(forumUser))
            {
                case DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.Forbidden:
                    {
                        return DotNetNuke.Modules.ActiveForums.Enums.PostStatus.Forbidden;
                    }

                case DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.New:
                    {
                        return DotNetNuke.Modules.ActiveForums.Enums.PostStatus.New;
                    }

                case DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.Unread:
                    {
                        return DotNetNuke.Modules.ActiveForums.Enums.PostStatus.Unread;
                    }

                case DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.Read:
                    {
                        return DotNetNuke.Modules.ActiveForums.Enums.PostStatus.Read;
                    }

                default:
                    {
                        return DotNetNuke.Modules.ActiveForums.Enums.PostStatus.Unread;
                    }
            }
        }

        public string GetPostStatusCss(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser)
        {
            string css = string.Empty;
            switch (this.GetPostStatusForUser(forumUser))
            {
                case DotNetNuke.Modules.ActiveForums.Enums.PostStatus.Forbidden:
                    {
                        css = "dcf-poststatus-no-access";
                        break;
                    }

                case DotNetNuke.Modules.ActiveForums.Enums.PostStatus.New:
                    {
                        css = "dcf-poststatus-new";
                        break;
                    }

                case DotNetNuke.Modules.ActiveForums.Enums.PostStatus.Unread:
                    {
                        css = "dcf-poststatus-unread";
                        break;
                    }

                case DotNetNuke.Modules.ActiveForums.Enums.PostStatus.Read:
                    {
                        css = "dcf-poststatus-read";
                        break;
                    }

                default:
                    {
                        css = "dcf-poststatus-unread";
                        break;
                    }
            }

            if (this.Author?.AuthorId == forumUser?.UserId)
            {
                css += " dcf-poststatus-authored";
            }

            return css;
        }
    }
}
