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

using System.Collections;

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Caching;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Tokens;
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

        [IgnoreColumn]
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

        [IgnoreColumn]
        public int PortalId { get => this.Forum.PortalId; }

        [IgnoreColumn]
        public int ModuleId { get => this.Forum.ModuleId; }

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic
        {
            get => this.topicInfo ?? (this.topicInfo = this.GetTopic());
            set => this.topicInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetTopic()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(this.TopicId);
        }
        
        [IgnoreColumn]
        public string Subject => Content.Subject;

        [IgnoreColumn]
        public string Body => Content.Body;

        [IgnoreColumn]
        public string Summary => Content.Summary;
        
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
            get => this.Topic.Forum;
            set => this.Topic.Forum = value;
        }

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo Author
        {
            get => this.author ?? (this.author = this.GetAuthor(this.PortalId, this.ModuleId, this.Content.AuthorId));
            set => this.author = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo GetAuthor(int portalId, int moduleId, int authorId)
        {
            return new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, moduleId, authorId);
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
                case "replyid":
                    return PropertyAccess.FormatString(this.PostId.ToString(), format);
                case "replytoid":
                    return PropertyAccess.FormatString(this.ReplyToId.ToString(), format);
                case "topicid":
                    return PropertyAccess.FormatString(this.TopicId.ToString(), format);
                case "contentid":
                    return PropertyAccess.FormatString(this.ContentId.ToString(), format);
                case "subject":
                    return PropertyAccess.FormatString(this.Subject, format);
                case "summary":
                    return PropertyAccess.FormatString(this.Summary, format);
                case "body":
                    return PropertyAccess.FormatString(this.Content.Body, format);
                case "authorid":
                    return PropertyAccess.FormatString(this.Content.AuthorId.ToString(), format);
                case "authorname":
                    return PropertyAccess.FormatString(this.Content.AuthorName.ToString(), format);
                case "authordisplayname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.Author?.DisplayName) ? this.Content.AuthorName :
                        DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(
                            this.Forum.PortalSettings,
                            this.Forum.MainSettings,
                            isMod: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).GetIsMod(this.ModuleId),
                            isAdmin: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).IsAdmin || new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).IsSuperUser,
                            this.Author.AuthorId,
                            this.Author.Username,
                            this.Author.FirstName,
                            this.Author.LastName,
                            this.Author.DisplayName).Replace("&amp;#", "&#").Replace("Anonymous", this.Content.AuthorName), format);
                case "authorfirstname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.Author?.DisplayName) ? this.Content.AuthorName : this.Author.FirstName, format);
                case "authorlastname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.Author?.DisplayName) ? this.Content.AuthorName : this.Author.LastName.ToString(), format);
                case "authoremail":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.Author?.DisplayName) ? string.Empty : this.Author.Email.ToString(), format);
                case "statuscssclass":
                    return PropertyAccess.FormatString(this.GetPostStatusCss(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)), format);
                case "posticon":
                    return PropertyAccess.FormatString(this.GetPostStatusCss(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)), format);
                case "posticoncss":
                    return PropertyAccess.FormatString(this.GetPostStatusCss(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)), format);
                case "datecreated":
                    return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime((DateTime?)this.Content.DateCreated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)), format);
                case "dateupdated":
                    return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime((DateTime?)this.Content.DateUpdated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)), format);
                case "modeditdate":
                    if (this.Forum.GetIsMod(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)) &&
                        this.Content.DateUpdated != this.Content.DateCreated)
                    {
                        return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime((DateTime?)this.Content.DateUpdated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)), format);
                    }

                    return string.Empty;
                case "editdate":
                    return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime(
                            (DateTime?)this.Content.DateUpdated,
                            formatProvider,
                            accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)),
                        format);
                case "selectedanswer":
                    if (this.IsReply && this.StatusId == 1)
                    {
                        return PropertyAccess.FormatString(
                            Utilities.GetSharedResource("[RESX:Status:Answer]"),
                            format);
                    }

                    return string.Empty;

                case "actioneditonclick":
                    {
                        var bEdit = Controllers.PermissionController.HasPerm(this.Forum.Security.Edit,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if (bEdit &&
                            (bModerate ||
                             ((this.Author.AuthorId == accessingUser.UserID) && (this.Forum.MainSettings.EditInterval == 0 || DateTime.UtcNow.Subtract(this.Content.DateCreated).TotalMinutes > this.Forum.MainSettings.EditInterval))))
                        {
                            var editParams = new List<string>()
                            {
                                $"{ParamKeys.ViewType}={Views.Post}",
                                $"{ParamKeys.Action}={PostActions.ReplyEdit}",
                                $"{ParamKeys.ForumId}={this.ForumId}",
                                $"{ParamKeys.TopicId}={this.TopicId}",
                                $"{ParamKeys.PostId}={this.PostId}",
                            };
                            if (this.Forum.SocialGroupId > 0)
                            {
                                editParams.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                            }

                            return PropertyAccess.FormatString(
                                Utilities.NavigateURL(this.Forum.PortalSettings.ActiveTab.TabID, string.Empty, editParams.ToArray()),
                                format);
                        }
                        return string.Empty;
                    }
                case "actionreplyonclick":
                    {
                        var bReply = Controllers.PermissionController.HasPerm(this.Forum.Security.Reply,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bTrust = Controllers.PermissionController.HasPerm(this.Forum.Security.Trust,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if (bReply &&
                            (bTrust ||
                             bModerate ||
                             ((!this.Topic.IsLocked) &&
                              (this.Forum.ReplyPostCount <= 0 ||
                               new Controllers.ForumUserController(this.ModuleId).GetByUserId(
                                   accessingUser.PortalID,
                                   accessingUser.UserID).PostCount >= this.Forum.ReplyPostCount))))
                        {
                            var @params = new List<string>()
                            {
                                $"{ParamKeys.ViewType}={Views.Post}",
                                $"{ParamKeys.Action}={PostActions.Reply}",
                                $"{ParamKeys.ForumId}={this.ForumId}",
                                $"{ParamKeys.TopicId}={this.TopicId}",
                                $"{ParamKeys.ReplyId}={this.PostId}",
                            };
                            if (this.Forum.SocialGroupId > 0)
                            {
                                @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                            }

                            return PropertyAccess.FormatString(
                                Utilities.NavigateURL(this.Forum.PortalSettings.ActiveTab.TabID, string.Empty, @params.ToArray()),
                                format);
                        }
                    }

                    return string.Empty;
                case "actionquoteonclick":
                    {
                        var bReply = Controllers.PermissionController.HasPerm(this.Forum.Security.Reply,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bTrust = Controllers.PermissionController.HasPerm(this.Forum.Security.Trust,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if (bReply &&
                            (bTrust ||
                             bModerate ||
                             ((!this.Topic.IsLocked) &&
                              (this.Forum.ReplyPostCount <= 0 ||
                               new Controllers.ForumUserController(this.ModuleId).GetByUserId(
                                   accessingUser.PortalID,
                                   accessingUser.UserID).PostCount >= this.Forum.ReplyPostCount))))
                        {
                            var @params = new List<string>()
                            {
                                $"{ParamKeys.ViewType}={Views.Post}",
                                $"{ParamKeys.Action}={PostActions.Reply}",
                                $"{ParamKeys.ForumId}={this.ForumId}",
                                $"{ParamKeys.TopicId}={this.TopicId}",
                                $"{ParamKeys.QuoteId}={this.PostId}",
                            };
                            if (this.Forum.SocialGroupId > 0)
                            {
                                @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                            }

                            return PropertyAccess.FormatString(
                                Utilities.NavigateURL(this.Forum.PortalSettings.ActiveTab.TabID, string.Empty, @params.ToArray()),
                                format);
                        }
                    }

                    return string.Empty;
                case "actiondeleteonclick":
                    {
                        var bDelete = Controllers.PermissionController.HasPerm(this.Forum.Security.Delete,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if (bDelete && (bModerate ||
                                        (this.Author.AuthorId == accessingUser.UserID && !this.Topic.IsLocked)))
                        {
                            return PropertyAccess.FormatString(
                                $"amaf_postDel({this.ModuleId},{this.ForumId},{this.TopicId},{this.ReplyId})",
                                format);
                        }
                    }

                    return string.Empty;
                case "actionalertonclick":
                    {
                        if (accessingUser.UserID > 0)
                        {
                            var editParams = new List<string>()
                            {
                                $"{ParamKeys.ViewType}={Views.ModerateReport}",
                                $"{ParamKeys.ForumId}={this.ForumId}",
                                $"{ParamKeys.TopicId}={this.TopicId}",
                                $"{ParamKeys.ReplyId}={this.ReplyId}",
                            };
                            if (this.Forum.SocialGroupId > 0)
                            {
                                editParams.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                            }

                            return PropertyAccess.FormatString(
                                Utilities.NavigateURL(this.Forum.PortalSettings.ActiveTab.TabID, string.Empty, editParams.ToArray()),
                                format);
                        }
                    }

                    return string.Empty;

                case "actionbanonclick":
                    {
                        // (Note: can't ban yourself or a superuser/admin)
                        var bBan = Controllers.PermissionController.HasPerm(this.Forum.Security.Ban,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if ((bBan || accessingUser.IsAdmin || accessingUser.IsSuperUser) &&
                            (this.Author.AuthorId != -1) && (this.Author.AuthorId != accessingUser.UserID) && (this.Author != null) && (!this.Author.ForumUser.IsSuperUser) && (!this.Author.ForumUser.IsAdmin))
                        {
                            var @params = new List<string>()
                            {
                                $"{ParamKeys.ViewType}={Views.ModerateBan}",
                                $"{ParamKeys.ForumId}={this.ForumId}",
                                $"{ParamKeys.TopicId}={this.TopicId}",
                                $"{ParamKeys.ReplyId}={this.ReplyId}",
                                $"{ParamKeys.AuthorId}={this.Author.AuthorId}",
                            };

                            return PropertyAccess.FormatString(
                                Utilities.NavigateURL(this.Forum.PortalSettings.ActiveTab.TabID, string.Empty, @params.ToArray()),
                                format);
                        }
                    }

                    return string.Empty;
                case "actionmarkansweronclick":
                    {
                        if (this.IsReply && this.Topic.StatusId > 0 && this.Topic.StatusId != 3 && this.StatusId != 1)
                        {
                            var bEdit = Controllers.PermissionController.HasPerm(this.Forum.Security.Edit,
                                accessingUser.PortalID,
                                this.Forum.ModuleId, accessingUser.UserID);
                            var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                                accessingUser.PortalID,
                                this.Forum.ModuleId, accessingUser.UserID);
                            if (bEdit && (bModerate || (accessingUser.UserID == this.Topic.Author.AuthorId && !this.Topic.IsLocked)))
                            {
                                return PropertyAccess.FormatString(
                                    $"javascript:amaf_MarkAsAnswer({this.ModuleId},{this.ForumId},{this.TopicId},{this.ReplyId});",
                                    format);
                            }
                        }

                        return string.Empty;
                    }

                case "actionmoveonclick": /* only valid for topic not for reply */
                    return string.Empty;
                case "actionlockonclick": /* only valid for topic not for reply */
                    return string.Empty;
                case "actionunlockonclick": /* only valid for topic not for reply */
                    return string.Empty;
                case "actionpinonclick": /* only valid for topic not for reply */
                    return string.Empty;
                case "actionunpinonclick": /* only valid for topic not for reply */
                    return string.Empty;
                case "iconpinned": /* only valid for topic not for reply */
                    return string.Empty;
                case "iconunpinned": /* only valid for topic not for reply */
                    return string.Empty;
                case "iconlocked": /* only valid for topic not for reply */
                    return string.Empty;
                case "iconunlocked": /* only valid for topic not for reply */
                    return string.Empty;
            }

            propertyNotFound = true;
            return string.Empty;
}
    }
}
