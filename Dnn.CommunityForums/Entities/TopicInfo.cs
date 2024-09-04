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
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.Entities.Portals;

    [TableName("activeforums_Topics")]
    [PrimaryKey("TopicId", AutoIncrement = true)]
#pragma warning disable SA1402 // File may only contain a single type
    public class TopicInfo : IPostInfo
#pragma warning restore SA1402 // File may only contain a single type

    {
        [IgnoreColumn]
        public class Category
        {
            public int id;
            public string name;
            public bool selected;

            [IgnoreColumn]
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

        [IgnoreColumn]
        public bool IsTopic => true;

        [IgnoreColumn]
        public bool IsReply => false;

        [IgnoreColumn]
        public int ReplyId => 0;

        [IgnoreColumn]
        public int PostId { get => this.TopicId; }

        [IgnoreColumn]
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

        [IgnoreColumn]
        public int PortalId { get => this.Forum.PortalId; }

        [IgnoreColumn]
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

        [IgnoreColumn]
        public string Subject => this.Content.Subject;

        [IgnoreColumn]
        public string Summary => this.Content.Summary;

        [IgnoreColumn]
        public string URL => !string.IsNullOrEmpty(this.TopicUrl) && !string.IsNullOrEmpty(this.ForumURL) ? this.ForumURL + this.TopicUrl : string.Empty;

        [IgnoreColumn]
        public string ForumURL => !string.IsNullOrEmpty(this.Forum.PrefixURL) && !string.IsNullOrEmpty(this.TopicUrl) ? "/" + this.Forum.PrefixURL + "/" : string.Empty;

        public int NextTopic { get; set; }

        public int PrevTopic { get; set; }

        public string TopicData { get; set; } = string.Empty;

        [IgnoreColumn]
        public int LastReplyId { get; set; }

        [IgnoreColumn]
        public int Rating { get; set; }

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic
        {
            get => this;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetTopic()
        {
            return this;
        }

        [IgnoreColumn]
        public int SubscriberCount => new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(portalId: this.PortalId, moduleId: this.ModuleId, forumId: this.ForumId, topicId: this.TopicId);

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo Content
        {
            get => this.contentInfo ?? (this.contentInfo = this.GetContent());
            set => this.contentInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ContentInfo GetContent()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(this.ContentId);
        }

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum
        {
            get => this.forumInfo ?? (this.forumInfo = this.GetForum());
            set => this.forumInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetForum()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.ForumId); /* can't get using moduleId since ModuleId comes from Forum */
        }

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo Author
        {
            get => this.author ?? (this.author = this.GetAuthor(this.PortalId, this.ModuleId, this.Content.AuthorId));
            set => this.author = value;
        }

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo LastReply
        {
            get => this.lastReply ?? (this.lastReply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(this.LastReplyId));
            set => this.lastReply = value;
        }

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo LastReplyAuthor
        {
            get => this.lastReplyAuthor ?? (this.lastReplyAuthor = this.lastReply == null ? null : this.GetAuthor(this.PortalId, this.ModuleId, this.lastReply.Content.AuthorId));
            set => this.lastReplyAuthor = value;
        }

        [IgnoreColumn]
        internal DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo GetAuthor(int portalId, int moduleId, int authorId)
        {
            return new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, moduleId, authorId);
        }

        [IgnoreColumn]
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

        [IgnoreColumn]
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

        [IgnoreColumn]
        public IEnumerable<Category> SelectedCategories => this.Categories.Where(c => c.selected).ToList();

        [IgnoreColumn]
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

        [IgnoreColumn]
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo> TopicProperties
        {
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

            set
            {
                this.TopicData = DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Serialize(this.Forum, value);
            }
        }

        [IgnoreColumn]
        internal DotNetNuke.Modules.ActiveForums.Enums.TopicStatus GetTopicStatusForUser(ForumUserInfo forumUser)
        {
            var canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.Forum.Security.View, forumUser?.UserRoles);

            if (!canView)
            {
                return DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Forbidden;
            }

            try
            {
                if (this.TopicId > forumUser?.GetLastTopicRead(this))
                {
                    return DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.New;
                }

                if (forumUser != null && forumUser.GetIsTopicRead(this))
                {
                    return DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Read;
                }

                return DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Unread;
            }

            catch
            {
                /* this is to handle some limited unit testing without retrieving data */
                return DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Unread;
            }
        }

        public DotNetNuke.Modules.ActiveForums.Enums.PostStatus GetPostStatusForUser(ForumUserInfo forumUser)
        {
            switch (this.GetTopicStatusForUser(forumUser))
            {
                case DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Forbidden:
                    {
                        return DotNetNuke.Modules.ActiveForums.Enums.PostStatus.Forbidden;
                    }

                case DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.New:
                    {
                        return DotNetNuke.Modules.ActiveForums.Enums.PostStatus.New;
                    }

                case DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Unread:
                    {
                        return DotNetNuke.Modules.ActiveForums.Enums.PostStatus.Unread;
                    }

                case DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Read:
                    {
                        return DotNetNuke.Modules.ActiveForums.Enums.PostStatus.Read;
                    }

                default:
                    {
                        return DotNetNuke.Modules.ActiveForums.Enums.PostStatus.Unread;
                    }
            }
        }

        internal string GetTopicStatusCss(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser)
        {
            string css = string.Empty;
            switch (this.GetTopicStatusForUser(forumUser))
            {
                case DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Forbidden:
                    {
                        css = "dcf-topicstatus-no-access";
                        break;
                    }

                case DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.New:
                    {
                        css = "dcf-topicstatus-new";
                        break;
                    }

                case DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Unread:
                    {
                        css = "dcf-topicstatus-unread";
                        break;
                    }

                case DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Read:
                    {
                        css = "dcf-topicstatus-read";
                        break;
                    }

                default:
                    {
                        css = "dcf-topicstatus-unread";
                        break;
                    }
            }

            if (this.Author?.AuthorId == forumUser?.UserId)
            {
                css += " dcf-topicstatus-authored";
            }

            return css;

        }

        internal string GetTopicStatusIconCss(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser)
        {
            switch (this.GetTopicStatusForUser(forumUser))
            {
                case DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Unread:
                    {
                        return "fa fa-file-o fa-2x fa-red";
                    }

                default:
                    {
                        return "fa fa-file-o fa-2x fa-grey";
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

        [IgnoreColumn]
        public DotNetNuke.Services.Tokens.CacheLevel Cacheability
        {
            get
            {
                return DotNetNuke.Services.Tokens.CacheLevel.notCacheable;
            }
        }
        [IgnoreColumn]
        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, DotNetNuke.Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            // replace any embedded tokens in format string
            if (format.Contains("["))
            {
                var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(this.Forum.PortalSettings, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID), this)
                {
                    AccessingUser = accessingUser,
                };
                format = tokenReplacer.ReplaceEmbeddedTokens(format);
            }
            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName)
            {
                case "postid":
                    return PropertyAccess.FormatString(this.PostId.ToString(), format);
                case "topicid":
                    return PropertyAccess.FormatString(this.TopicId.ToString(), format);
                case "contentid":
                    return PropertyAccess.FormatString(this.ContentId.ToString(), format);
                case "forumid":
                    return PropertyAccess.FormatString(this.forumId.ToString(), format);
                case "subject":
                    return PropertyAccess.FormatString(this.Subject, format);
                case "summary":
                    return PropertyAccess.FormatString(this.Summary, format);
                case "body":
                    return PropertyAccess.FormatString(this.Content.Body, format);
                case "lastreplyid":
                    return PropertyAccess.FormatString(this.LastReplyId.ToString(), format);
                case "replycount":
                    return PropertyAccess.FormatString(this.ReplyCount.ToString(), format);
                case "viewcount":
                    return PropertyAccess.FormatString(this.ViewCount.ToString(), format);
                case "subscribercount":
                    return PropertyAccess.FormatString(this.SubscriberCount.ToString(), format);
                case "status":
                    return this.StatusId == -1 ? string.Empty : PropertyAccess.FormatString(this.StatusId.ToString(), format);
                case "statusid":
                    return PropertyAccess.FormatString(this.StatusId.ToString(), format);
                case "previoustopic":
                    return PropertyAccess.FormatString(this.PrevTopic.ToString(), format);
                case "previoustopiclink":
                    if (this.PrevTopic != 0)
                    {
                        var @params = new List<string>()
                {
                    $"{ParamKeys.ViewType}={Views.Topic}",
                    $"{ParamKeys.ForumId}={this.ForumId}",
                    $"{ParamKeys.TopicId}={this.PrevTopic}",
                };
                        if (this.Forum.SocialGroupId > 0)
                        {
                            @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                        }

                        return PropertyAccess.FormatString(Utilities.NavigateURL(this.Forum.TabId, string.Empty, @params.ToArray()), format);
                    }

                    return string.Empty;
                case "nexttopic":
                    return PropertyAccess.FormatString(this.NextTopic.ToString(), format);
                case "nexttopiclink":
                    if (this.NextTopic != 0)
                    {
                        var @params = new List<string>()
                {
                    $"{ParamKeys.ViewType}={Views.Topic}",
                    $"{ParamKeys.ForumId}={this.ForumId}",
                    $"{ParamKeys.TopicId}={this.NextTopic}",
                };
                        if (this.Forum.SocialGroupId > 0)
                        {
                            @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                        }

                        return PropertyAccess.FormatString(Utilities.NavigateURL(this.Forum.TabId, string.Empty, @params.ToArray()), format);
                    }

                    return string.Empty;
                case "rating":
                    return this.Rating < 1 ? string.Empty : PropertyAccess.FormatString(this.Rating.ToString(), format);
                case "topicurl":
                    return PropertyAccess.FormatString(this.TopicUrl.ToString(), format);
                case "forumurl":
                    return PropertyAccess.FormatString(this.ForumURL.ToString(), format);
                case "link":
                    return PropertyAccess.FormatString(this.URL.ToString(), format);
                case "url":
                    return PropertyAccess.FormatString(this.URL.ToString(), format);
                case "lastreplydisplayname":
                    return PropertyAccess.FormatString(this.lastReplyAuthor.DisplayName, format);
                case "datecreated":
                    return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime((DateTime?)this.Content.DateCreated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)), format);
                case "dateupdated":
                    return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime((DateTime?)this.Content.DateUpdated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)), format);
                case "modipaddress":
                    return !this.Forum.GetIsMod(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)) ? string.Empty : PropertyAccess.FormatString(this.Content.IPAddress, format);
                case "modeditdate":
                    if (this.Forum.GetIsMod(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)) &&
                        this.Content.DateUpdated != this.Content.DateCreated)
                    {
                        return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime((DateTime?)this.Content.DateUpdated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)), format);
                    }

                    return string.Empty;
                case "lastpostdate":
                    return this.LastReply?.Content != null ? PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime((DateTime?)this.LastReply.Content.DateCreated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)), format) : string.Empty;
                case "editdate":
                    return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime((DateTime?)this.Content.DateUpdated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)), format);
                case "authorid":
                    return PropertyAccess.FormatString(this.Content.AuthorId.ToString(), format);
                case "authorname":
                    return PropertyAccess.FormatString(this.Content.AuthorName, format);
                case "authordisplaynamelink":
                    return PropertyAccess.FormatString(
                        DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.CanLinkToProfile(
                        this.Forum.PortalSettings,
                        this.Forum.MainSettings,
                        this.ModuleId,
                        new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(
                            accessingUser.PortalID,
                            accessingUser.UserID),
                        this.Author.ForumUser) ?
                            Utilities.NavigateURL(this.Forum.PortalSettings.UserTabId,
                                string.Empty,
                                new[] { $"userId={this.Content.AuthorId}" })
                            : string.Empty,
                        format);
                case "authordisplayname":
                    return PropertyAccess.FormatString(
                        string.IsNullOrEmpty(this.Author?.DisplayName) ? this.Content.AuthorName :
                            DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(
                                this.Forum.PortalSettings,
                                this.Forum.MainSettings,
                                isMod: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).GetIsMod(this.ModuleId),
                                isAdmin: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).IsAdmin || new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).IsSuperUser,
                                this.Author.AuthorId,
                                this.Author.Username,
                                this.Author.FirstName,
                                this.Author.LastName,
                                this.Author.DisplayName).Replace("&amp;#", "&#").Replace("Anonymous", this.Content.AuthorName),
                        format);
                case "authorfirstname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.Author?.DisplayName) ? this.Content.AuthorName : this.Author.FirstName, format);
                case "authorlastname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.Author?.DisplayName) ? this.Content.AuthorName : this.Author.LastName, format);
                case "lastreplyauthorid":
                    return PropertyAccess.FormatString(this.LastReply?.Author?.AuthorId.ToString(), format);
                case "lastreplyauthorname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.LastReply?.Content?.AuthorName) ? string.Empty : this.LastReply?.Content?.AuthorName, format);
                case "lastreplyauthordisplayname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.LastReply?.Author?.DisplayName) ? this.LastReply?.Content?.AuthorName : this.LastReply?.Author?.DisplayName, format);
                case "lastreplyauthorfirstname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.LastReply?.Author?.FirstName) ? this.LastReply?.Content?.AuthorName : this.LastReply?.Author?.FirstName, format);
                case "lastreplyauthorlastname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.LastReply?.Author?.LastName) ? this.LastReply?.Content?.AuthorName : this.LastReply?.Author?.LastName, format);
                case "lastreplyauthoremail":
                    return this.LastReplyId > 0 ? PropertyAccess.FormatString(this.LastReply?.Author?.Email, format) : string.Empty;
                case "lastpostauthordisplaynamelink":
                    return PropertyAccess.FormatString(
                        this.LastReplyId > 0 && this.LastReply.Author.AuthorId > 0 ?
                            DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.CanLinkToProfile(
                                this.Forum.PortalSettings,
                                this.Forum.MainSettings,
                                this.ModuleId,
                                new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(
                                    accessingUser.PortalID,
                                    accessingUser.UserID),
                                this.LastReply.Author.ForumUser) ?
                                Utilities.NavigateURL(this.Forum.PortalSettings.UserTabId,
                                    string.Empty,
                                    new[] { $"userId={this.LastReplyAuthor.AuthorId}" })
                                : string.Empty : string.Empty, format);
                case "lastpostauthordisplayname":
                    return PropertyAccess.FormatString(
                        this.LastReplyId > 0 ? this.LastReply.Author.AuthorId > 0 ? DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(
                                this.Forum.PortalSettings,
                                this.Forum.MainSettings,
                                isMod: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).GetIsMod(this.ModuleId),
                                isAdmin: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).IsAdmin || new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).IsSuperUser,
                                this.LastReply.Author.AuthorId,
                                this.LastReply.Author.Username,
                                this.LastReply.Author.FirstName,
                                this.LastReply.Author.LastName,
                                this.LastReply.Author.DisplayName).Replace("&amp;#", "&#").Replace("Anonymous", this.LastReply.Content.AuthorName)
                            : this.LastReply.Content.AuthorName : string.Empty, format);
                case "statuscssclass":
                    return PropertyAccess.FormatString(this.GetPostStatusCss(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)), format);
                case "posticon":
                    return PropertyAccess.FormatString(this.GetPostStatusCss(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)), format);
                case "posticoncss":
                    return PropertyAccess.FormatString(this.GetPostStatusCss(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)), format);
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
