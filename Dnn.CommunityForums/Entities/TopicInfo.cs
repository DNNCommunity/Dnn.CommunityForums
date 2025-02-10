// Copyright (c) by DNN Community
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

#pragma warning disable SA1403 // File may only contain a single namespace
namespace DotNetNuke.Modules.ActiveForums.Entities
#pragma warning restore SA1403 // File may only contain a single namespace
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing.Printing;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;

    using Collections;
    using ComponentModel.DataAnnotations;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Modules.ActiveForums.ViewModels;
    using DotNetNuke.Services.Tokens;

    [TableName("activeforums_Topics")]
    [PrimaryKey("TopicId", AutoIncrement = true)]
#pragma warning disable SA1402 // File may only contain a single type
    public class TopicInfo : DotNetNuke.Modules.ActiveForums.Entities.IPostInfo
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

        [IgnoreColumn] private string cacheKeyTemplate => CacheKeys.TopicInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ContentInfo contentInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo lastReply;
        private DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author;
        private DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo lastReplyAuthor;
        private int forumId = -1;
        private string tags = string.Empty;
        private string selectedcategories;
        private int? subscriberCount;
        private int? likeCount;

        public int TopicId { get; set; }

        [IgnoreColumn]
        public bool IsTopic => true;

        [IgnoreColumn]
        public bool IsReply => false;

        [IgnoreColumn]
        public int ReplyId => 0;

        [IgnoreColumn]
        public int PostId => this.TopicId;

        [IgnoreColumn]
        public Uri RequestUri { get; set; }

        [IgnoreColumn]
        public string RawUrl { get; set; }

        [IgnoreColumn]
        public int ForumId
        {
            get
            {
                // TODO : clean this up to use DAL2
                if (this.forumId < 1 && this.TopicId > 0)
                {
                    this.forumId = Controllers.ForumController.Forum_GetByTopicId(this.ModuleId, this.TopicId);
                    this.Forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.forumId, this.ModuleId);
                    this.UpdateCache();
                }

                return this.forumId;
            }
            set => this.forumId = value;
        }

        [IgnoreColumn]
        public int PortalId { get; set; }

        [IgnoreColumn]
        public int ModuleId { get; set; }

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

        public DateTime? AnnounceStart { get; set; }

        public DateTime? AnnounceEnd { get; set; }

        public TopicTypes TopicType { get; set; }

        public int Priority { get; set; } = 0;

        [ColumnName("URL")]
        public string TopicUrl { get; set; } = string.Empty;

        [IgnoreColumn]
        public string Subject => this.Content.Subject;

        [IgnoreColumn]
        public string Summary => this.Content.Summary;

        [IgnoreColumn]
        public string URL => !string.IsNullOrEmpty(this.TopicUrl) && !string.IsNullOrEmpty(this.ForumURL)
            ? this.ForumURL + this.TopicUrl
            : string.Empty;

        [IgnoreColumn]
        public string ForumURL => !string.IsNullOrEmpty(this.Forum.PrefixURL) && !string.IsNullOrEmpty(this.TopicUrl)
            ? "/" + this.Forum.PrefixURL + "/"
            : string.Empty;

        public int PrevTopic { get; set; }

        public int NextTopic { get; set; }

        public string TopicData { get; set; } = string.Empty;

        [IgnoreColumn]
        public int LastReplyId { get; set; }

        [IgnoreColumn]
        public int Rating { get; set; }

        [IgnoreColumn]
        public TopicInfo Topic => this;

        public TopicInfo GetTopic()
        {
            return this;
        }

        [IgnoreColumn]
        public int SubscriberCount
        {
            get
            {
                if (this.subscriberCount == null)
                {
                    this.subscriberCount =
                        new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(this.PortalId,
                            this.ModuleId,
                            this.ForumId,
                            this.TopicId);
                    this.UpdateCache();
                }

                return (int)this.subscriberCount;
            }
        }

        [IgnoreColumn]
        public int LikeCount
        {
            get
            {
                if (this.likeCount == null)
                {
                    this.likeCount =
                        new DotNetNuke.Modules.ActiveForums.Controllers.LikeController(this.PortalId, this.ModuleId).Count(this.ContentId);
                    this.UpdateCache();
                }

                return (int)this.likeCount;
            }
        }

        public bool IsLikedByUser(ForumUserInfo forumUser)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.LikeController(this.PortalId, this.ModuleId).GetLikedByUser(forumUser.UserId, this.ContentId);
        }

        [IgnoreColumn]
        public ContentInfo Content
        {
            get
            {
                if (this.contentInfo == null)
                {
                    this.contentInfo = this.GetContent();
                    this.UpdateCache();
                }

                return this.contentInfo;
            }

            set => this.contentInfo = value;
        }

        internal ContentInfo GetContent()
        {
            return this.contentInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(this.ContentId, this.ModuleId);
        }

        [IgnoreColumn]
        public ForumInfo Forum
        {
            get
            {
                if (this.forumInfo == null)
                {
                    this.forumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.ForumId, this.ModuleId);
                    this.UpdateCache();
                }

                return this.forumInfo;
            }
            set => this.forumInfo = value;
        }

        [IgnoreColumn]
        public AuthorInfo Author
        {
            get
            {
                if (this.author == null)
                {
                    this.author = this.GetAuthor(this.PortalId, this.ModuleId, this.Content.AuthorId);
                    this.UpdateCache();
                }

                return this.author;
            }
            set => this.author = value;
        }

        [IgnoreColumn]
        public ReplyInfo LastReply
        {
            get
            {
                if (this.lastReply == null)
                {
                    if (this.LastReplyId > 0)
                    {
                        this.lastReply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ModuleId).GetById(this.LastReplyId);
                        this.UpdateCache();
                    }
                }

                return this.lastReply;
            }

            set => this.lastReply = value;
        }

        [IgnoreColumn]
        public AuthorInfo LastReplyAuthor
        {

            get
            {
                if (this.lastReplyAuthor == null)
                {
                    if (this.lastReply == null)
                    {
                        this.lastReplyAuthor = null;
                    }
                    else
                    {
                        this.lastReplyAuthor = this.GetAuthor(this.PortalId, this.ModuleId, this.lastReply.Content.AuthorId);
                    }

                    this.UpdateCache();
                }

                return this.lastReplyAuthor;
            }
            set => this.lastReplyAuthor = value;
        }

        [IgnoreColumn]
        internal AuthorInfo GetAuthor(int portalId, int moduleId, int authorId)
        {
            return new AuthorInfo(portalId, moduleId, authorId);
        }

        [IgnoreColumn]
        public string Tags
        {
            get
            {
                if (string.IsNullOrEmpty(this.tags))
                {
                    this.tags = string.Join(",", new Controllers.TopicTagController().GetForTopic(this.TopicId).Select(t => t.Tag.TagName));
                    if (string.IsNullOrEmpty(this.tags))
                    {
                        this.tags = string.Empty;
                    }
                    this.UpdateCache();
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
                    this.categories = new Controllers.CategoryController()
                        .Find("WHERE ForumId = @0 OR ForumGroupid = @1", this.ForumId, this.Forum.ForumGroupId)
                        .Select(c => new Category(c.TagId, c.TagName, false)).ToList();
                    var topicCategoryIds = new Controllers.TopicCategoryController().GetForTopic(this.TopicId)
                        .Select(t => t.TagId);
                    topicCategoryIds.ForEach(tc =>
                        this.categories.Where(c => c.id == tc).ForEach(c => c.selected = true));
                    this.UpdateCache();
                }

                return this.categories;
            }
        }

        [IgnoreColumn]
        public IEnumerable<Category> SelectedCategories => this.Categories.Where(c => c.selected).ToList();

        [IgnoreColumn]
        public string SelectedCategoriesAsString
        {
            get => this.selectedcategories ?? (this.selectedcategories = string.Join(";", this.SelectedCategories.Select(c => c.id.ToString())));
            set => this.selectedcategories = value;
        }

        [IgnoreColumn]
        public IEnumerable<TopicPropertyInfo> TopicProperties
        {
            get => this.TopicData == string.Empty ? null : Controllers.TopicPropertyController.Deserialize(this.TopicData);
            set => this.TopicData = Controllers.TopicPropertyController.Serialize(this.Forum, value);
        }

        [IgnoreColumn]
        internal Enums.TopicStatus GetTopicStatusForUser(ForumUserInfo forumUser)
        {
            if (!Controllers.PermissionController.HasPerm(this.Forum.Security.View, forumUser?.UserRoles))
            {
                return Enums.TopicStatus.Forbidden;
            }

            try
            {
                if (forumUser?.UserId == -1)
                {
                    return Enums.TopicStatus.Unread;
                }

                if (this.TopicId > forumUser?.GetLastTopicRead(this))
                {
                    return Enums.TopicStatus.New;
                }

                if (forumUser != null && forumUser.GetIsTopicRead(this))
                {
                    return Enums.TopicStatus.Read;
                }

                return Enums.TopicStatus.Unread;
            }
            catch
            {
                /* this is to handle some limited unit testing without retrieving data */
                return Enums.TopicStatus.Unread;
            }
        }

        public Enums.PostStatus GetPostStatusForUser(ForumUserInfo forumUser)
        {
            switch (this.GetTopicStatusForUser(forumUser))
            {
                case Enums.TopicStatus.Forbidden:
                    {
                        return Enums.PostStatus.Forbidden;
                    }

                case Enums.TopicStatus.New:
                    {
                        return Enums.PostStatus.New;
                    }

                case Enums.TopicStatus.Unread:
                    {
                        return Enums.PostStatus.Unread;
                    }

                case Enums.TopicStatus.Read:
                    {
                        return Enums.PostStatus.Read;
                    }

                default:
                    {
                        return Enums.PostStatus.Unread;
                    }
            }
        }

        internal string GetTopicStatusCss(ForumUserInfo forumUser)
        {
            string css;
            switch (this.GetTopicStatusForUser(forumUser))
            {
                case Enums.TopicStatus.Forbidden:
                    {
                        css = "dcf-topicstatus-no-access";
                        break;
                    }

                case Enums.TopicStatus.New:
                    {
                        css = "dcf-topicstatus-new";
                        break;
                    }

                case Enums.TopicStatus.Unread:
                    {
                        css = "dcf-topicstatus-unread";
                        break;
                    }

                case Enums.TopicStatus.Read:
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

        internal string GetTopicStatusIconCss(ForumUserInfo forumUser)
        {
            switch (this.GetTopicStatusForUser(forumUser))
            {
                case Enums.TopicStatus.Unread:
                    {
                        return "fa fa-file-o fa-2x fa-red";
                    }

                default:
                    {
                        return "fa fa-file-o fa-2x fa-grey";
                    }
            }
        }

        public string GetPostStatusCss(ForumUserInfo forumUser)
        {
            string css;
            switch (this.GetPostStatusForUser(forumUser))
            {
                case Enums.PostStatus.Forbidden:
                    {
                        css = "dcf-poststatus-no-access";
                        break;
                    }

                case Enums.PostStatus.New:
                    {
                        css = "dcf-poststatus-new";
                        break;
                    }

                case Enums.PostStatus.Unread:
                    {
                        css = "dcf-poststatus-unread";
                        break;
                    }

                case Enums.PostStatus.Read:
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

        /// <inheritdoc/>
        [IgnoreColumn]
        public CacheLevel Cacheability => CacheLevel.notCacheable;

        /// <inheritdoc/>
        [IgnoreColumn]
        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, DotNetNuke.Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            if (!DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.Forum.Security.Read, accessingUser.PortalID, this.Forum.ModuleId, accessingUser.UserID))
            {
                return string.Empty;
            }

            // replace any embedded tokens in format string
            if (format.Contains("["))
            {
                var tokenReplacer = new Services.Tokens.TokenReplacer(this.Forum.PortalSettings, new Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID), this, this.RequestUri, this.RawUrl)
                {
                    AccessingUser = accessingUser
                };
                format = tokenReplacer.ReplaceEmbeddedTokens(format);
            }

            int length = -1;
            if (propertyName.Contains(":"))
            {
                var splitPropertyName = propertyName.Split(':');
                propertyName = splitPropertyName[0];
                length = Utilities.SafeConvertInt(splitPropertyName[1], -1);
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
                    return PropertyAccess.FormatString(this.ForumId.ToString(), format);
                case "subject":
                    {
                        string sPollImage = (this.Topic.TopicType == TopicTypes.Poll ? DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.GetTokenFormatString("[POLLIMAGE]", this.Forum.PortalSettings, accessingUser.Profile.PreferredLocale) : string.Empty);
                        return PropertyAccess.FormatString(length > 0 && this.Subject.Length > length ? string.Concat(Utilities.StripHTMLTag(this.Subject).Replace("[", "&#91").Replace("]", "&#93"), "...") : Utilities.StripHTMLTag(this.Subject).Replace("[", "&#91").Replace("]", "&#93") + sPollImage, format);
                    }

                case "subjectlink":
                    {
                        string sTopicURL = new ControlUtils().BuildUrl(this.Forum.PortalSettings.PortalId, GetTabId(), this.Forum.ModuleId, this.Forum.ForumGroup.PrefixURL, this.Forum.PrefixURL, this.Forum.ForumGroupId, this.Forum.ForumID, this.TopicId, this.TopicUrl, -1, -1, string.Empty, 1, -1, this.Forum.SocialGroupId);
                        string sPollImage = (this.Topic.TopicType == TopicTypes.Poll ? DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.GetTokenFormatString("[POLLIMAGE]", this.Forum.PortalSettings, accessingUser.Profile.PreferredLocale) : string.Empty);
                        string subject = Utilities.StripHTMLTag(System.Net.WebUtility.HtmlDecode(this.Subject)).Replace("\"", string.Empty).Replace("#", string.Empty).Replace("%", string.Empty).Replace("+", string.Empty); ;
                        string sBodyTitle = GetTopicTitle(this.Content.Body);
                        string slink;
                        var @params = new List<string>
                        {
                            $"{ParamKeys.TopicId}={this.TopicId}", $"{ParamKeys.ContentJumpId}={this.LastReplyId}",
                        };

                        if (this.Forum.SocialGroupId > 0)
                        {
                            @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                        }

                        if (sTopicURL == string.Empty)
                        {
                            @params = new List<string>
                            {
                                $"{ParamKeys.ForumId}={this.ForumId}",
                                $"{ParamKeys.TopicId}={this.TopicId}",
                                $"{ParamKeys.ViewType}={Views.Topic}",
                            };
                            if (this.Forum.MainSettings.UseShortUrls)
                            {
                                @params.Add($"{ParamKeys.TopicId}={this.TopicId}");
                            }

                            slink = "<a title=\"" + sBodyTitle + "\" href=\"" + Utilities.NavigateURL(GetTabId(), string.Empty, @params.ToArray()) + "\">" + subject + "</a>";
                        }
                        else
                        {
                            slink = "<a title=\"" + sBodyTitle + "\" href=\"" + sTopicURL + "\">" + subject + "</a>";
                        }

                        return PropertyAccess.FormatString(slink + sPollImage, format);
                    }

                case "lastreadurl":
                    {
                        var @params = new List<string>
                        {
                            $"{ParamKeys.TopicId}={this.TopicId}", $"{ParamKeys.ContentJumpId}={this.LastReplyId}",
                        };

                        if (this.Forum.SocialGroupId > 0)
                        {
                            @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                        }

                        string sLastReadURL = string.Empty;
                        string sTopicURL = new ControlUtils().BuildUrl(this.Forum.PortalSettings.PortalId, GetTabId(), this.Forum.ModuleId, this.Forum.ForumGroup.PrefixURL, this.Forum.PrefixURL, this.Forum.ForumGroupId, this.Forum.ForumID, this.TopicId, this.TopicUrl, -1, -1, string.Empty, 1, -1, this.Forum.SocialGroupId);
                        int? userLastReplyRead = new Controllers.ForumUserController(this.ModuleId).GetByUserId(
                            accessingUser.PortalID,
                            accessingUser.UserID).GetLastReplyRead(this);
                        if (userLastReplyRead > 0)
                        {
                            @params = new List<string>
                            {
                                $"{ParamKeys.ForumId}={this.ForumId}",
                                $"{ParamKeys.TopicId}={this.TopicId}",
                                $"{ParamKeys.ViewType}={Views.Topic}",
                                $"{ParamKeys.FirstNewPost}={userLastReplyRead}",
                            };
                            if (this.Forum.SocialGroupId > 0)
                            {
                                @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                            }

                            sLastReadURL = Utilities.NavigateURL(GetTabId(),
                                string.Empty,
                                @params.ToArray());
                        }

                        if (this.Forum.MainSettings.UseShortUrls)
                        {
                            @params = new List<string>
                            {
                                $"{ParamKeys.TopicId}={this.TopicId}",
                                $"{ParamKeys.FirstNewPost}={userLastReplyRead}",
                            };
                            if (this.Forum.SocialGroupId > 0)
                            {
                                @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                            }

                            sLastReadURL = Utilities.NavigateURL(GetTabId(), string.Empty, @params.ToArray());
                        }

                        if (sTopicURL.EndsWith("/"))
                        {
                            sLastReadURL = sTopicURL + (Utilities.UseFriendlyURLs(this.Forum.ModuleId)
                                ? string.Concat("#", userLastReplyRead)
                                : string.Concat("?", ParamKeys.FirstNewPost, "=", userLastReplyRead));
                        }

                        return PropertyAccess.FormatString(sLastReadURL, format);
                    }

                case "lastreplyurl":
                    if (this.LastReplyId > 0)
                    {
                        var @params = new List<string>
                        {
                            $"{ParamKeys.TopicId}={this.TopicId}", $"{ParamKeys.ContentJumpId}={this.LastReplyId}",
                        };

                        if (this.Forum.SocialGroupId > 0)
                        {
                            @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                        }

                        string sLastReplyURL = Utilities.NavigateURL(GetTabId(), string.Empty, @params.ToArray());
                        string sTopicURL = new ControlUtils().BuildUrl(this.Forum.PortalSettings.PortalId, GetTabId(), this.Forum.ModuleId, this.Forum.ForumGroup.PrefixURL, this.Forum.PrefixURL, this.Forum.ForumGroupId, this.Forum.ForumID, this.TopicId, this.TopicUrl, -1, -1, string.Empty, 1, -1, this.Forum.SocialGroupId);
                        if (!(string.IsNullOrEmpty(sTopicURL)))
                        {
                            if (sTopicURL.EndsWith("/"))
                            {
                                sLastReplyURL = sTopicURL + (Utilities.UseFriendlyURLs(this.Forum.ModuleId)
                                    ? string.Concat($"#{this.TopicId}")
                                    : string.Concat($"?{ParamKeys.ContentJumpId}={this.LastReplyId}"));
                            }
                        }

                        return PropertyAccess.FormatString(sLastReplyURL, format);
                    }

                    return string.Empty;
                case "bodytitle":
                    return PropertyAccess.FormatString(GetTopicTitle(this.Content.Body), format);
                case "summary":
                    return PropertyAccess.FormatString(
                        !string.IsNullOrEmpty(this.Summary)
                        ? length > 0 && this.Summary.Length > length ? this.Summary.Substring(0, length) : this.Summary
                        : length > 0 && this.Content.Body.Length > length ? this.Content.Body.Substring(0, length) : this.Content.Body, format);
                case "body":
                    return PropertyAccess.FormatString(length > 0 && this.Content.Body.Length > length ? this.Content.Body.Substring(0, length) : this.Content.Body, format);
                case "lastreplyid":
                    return PropertyAccess.FormatString(this.LastReplyId.ToString(), format);
                case "replycount":
                    return PropertyAccess.FormatString(this.ReplyCount.ToString(), format);
                case "viewcount":
                    return PropertyAccess.FormatString(this.ViewCount.ToString(), format);
                case "subscribercount":
                    return PropertyAccess.FormatString(this.SubscriberCount.ToString(), format);
                case "isliked":
                    return !this.Forum.FeatureSettings.AllowLikes ? string.Empty : PropertyAccess.FormatString(this.IsLikedByUser(new Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)) ? true.ToString() : string.Empty, format);
                case "likecount":
                    return !this.Forum.FeatureSettings.AllowLikes ? string.Empty : PropertyAccess.FormatString(this.LikeCount.ToString(), format);
                case "likeonclick":
                    {
                        var bReply = Controllers.PermissionController.HasPerm(this.Forum.Security.Reply,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if (this.Forum.FeatureSettings.AllowLikes)
                        {
                            return PropertyAccess.FormatString(bReply ?
                                $"amaf_likePost({this.Forum.ModuleId},{this.Forum.ForumID},{this.ContentId})" : string.Empty,
                                format);
                        }
                    }

                    return string.Empty;
                case "status":
                    {
                        var bRead = Controllers.PermissionController.HasPerm(this.Forum.Security.Read,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bEdit = Controllers.PermissionController.HasPerm(this.Forum.Security.Edit,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if (((bRead && this.Content.AuthorId == accessingUser.UserID) || (bModerate && bEdit)) & this.StatusId >= 0)
                        {
                            return this.StatusId == -1
                                ? string.Empty
                                : PropertyAccess.FormatString(this.StatusId.ToString(), format);
                        }

                        return string.Empty;
                    }

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

                        return PropertyAccess.FormatString(
                            Utilities.NavigateURL(GetTabId(),
                                string.Empty,
                                @params.ToArray()),
                            format);
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

                        return PropertyAccess.FormatString(
                            Utilities.NavigateURL(GetTabId(),
                                string.Empty,
                                @params.ToArray()),
                            format);
                    }

                    return string.Empty;
                case "rating":
                    return this.Rating < 1 ? string.Empty : PropertyAccess.FormatString(this.Rating.ToString(), format);
                case "url":
                    return PropertyAccess.FormatString(this.URL, format);
                case "topicurl":
                    return PropertyAccess.FormatString(this.TopicUrl, format);
                case "forumurl":
                    return PropertyAccess.FormatString(this.ForumURL, format);
                case "link":
                    {
                        string sTopicURL = new ControlUtils().BuildUrl(this.Forum.PortalSettings.PortalId, this.GetTabId(), this.Forum.ModuleId, this.Forum.ForumGroup.PrefixURL, this.Forum.PrefixURL, this.Forum.ForumGroupId, this.Forum.ForumID, this.TopicId, this.TopicUrl, -1, -1, string.Empty, 1, -1, this.Forum.SocialGroupId);
                        string subject = Utilities.StripHTMLTag(System.Net.WebUtility.HtmlDecode(this.Subject)).Replace("\"", string.Empty).Replace("#", string.Empty).Replace("%", string.Empty).Replace("+", string.Empty);
                        string sBodyTitle = GetTopicTitle(this.Content.Body);
                        string slink;
                        var @params = new List<string>
                        {
                            $"{ParamKeys.TopicId}={this.TopicId}", $"{ParamKeys.ContentJumpId}={this.LastReplyId}",
                        };

                        if (this.Forum.SocialGroupId > 0)
                        {
                            @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                        }

                        if (sTopicURL == string.Empty)
                        {
                            @params = new List<string>
                            {
                                $"{ParamKeys.ForumId}={this.ForumId}",
                                $"{ParamKeys.TopicId}={this.TopicId}",
                                $"{ParamKeys.ViewType}={Views.Topic}",
                            };
                            if (this.Forum.MainSettings.UseShortUrls)
                            {
                                @params.Add($"{ParamKeys.TopicId}={this.TopicId}");
                            }

                            slink = "<a title=\"" + sBodyTitle + "\" href=\"" + Utilities.NavigateURL(this.GetTabId(), string.Empty, @params.ToArray()) + "\">" + subject + "</a>";
                        }
                        else
                        {
                            slink = "<a title=\"" + sBodyTitle + "\" href=\"" + sTopicURL + "\">" + subject + "</a>";
                        }

                        return PropertyAccess.FormatString(slink, format);
                    }

                case "likeslink":
                    {
                        if (this.Forum.FeatureSettings.AllowLikes)
                        {
                            string linkUrl = new ControlUtils().BuildUrl(this.Forum.PortalSettings.PortalId,
                                tabId: this.GetTabId(),
                                moduleId: this.Forum.ModuleId,
                                groupPrefix: this.Forum.ForumGroup.PrefixURL,
                                forumPrefix: this.Forum.PrefixURL,
                                forumGroupId: this.Forum.ForumGroupId,
                                forumID: this.Forum.ForumID,
                                topicId: this.TopicId,
                                topicURL: this.TopicUrl,
                                tagId: -1,
                                categoryId: -1,
                                otherPrefix: Views.Likes,
                                pageId: 1,
                                contentId: this.ContentId,
                                socialGroupId: this.Forum.SocialGroupId);

                            if (string.IsNullOrEmpty(linkUrl))
                            {
                                var @params = new List<string>
                                {
                                    $"{ParamKeys.ViewType}={Views.Grid}", $"{ParamKeys.GridType}={Views.Likes}", $"{ParamKeys.ContentId}={this.ContentId}",
                                };

                                if (this.Forum.SocialGroupId > 0)
                                {
                                    @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                                }

                                linkUrl = Utilities.NavigateURL(this.GetTabId(), string.Empty, @params.ToArray());
                            }

                            return PropertyAccess.FormatString(linkUrl, format);
                        }

                        return string.Empty;
                    }

                case "lastreplydisplayname":
                    return PropertyAccess.FormatString(this.lastReplyAuthor.DisplayName, format);
                case "datecreated":
                    return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime(
                            (DateTime?)this.Content.DateCreated,
                            formatProvider,
                            accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)),
                        format);
                case "dateupdated":
                    return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime(
                            (DateTime?)this.Content.DateUpdated,
                            formatProvider,
                            accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)),
                        format);
                case "modipaddress":
                    {
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        return bModerate
                        ? PropertyAccess.FormatString(this.Content.IPAddress, format)
                        : string.Empty;
                    }

                case "modeditdate":
                    {
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        return bModerate &&
                               this.Content.DateUpdated != this.Content.DateCreated
                            ? PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime(
                                    (DateTime?)this.Content.DateUpdated,
                                    formatProvider,
                                    accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)),
                                format)
                            : string.Empty;
                    }

                case "lastpostdate":
                    return this.LastReply?.Content != null
                        ? PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime(
                                (DateTime?)this.LastReply.Content.DateCreated,
                                formatProvider,
                                accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)),
                            format)
                        : string.Empty;
                case "editdate":
                    return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime(
                            (DateTime?)this.Content.DateUpdated,
                            formatProvider,
                            accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)),
                        format);
                case "authorid":
                    return PropertyAccess.FormatString(this.Content.AuthorId.ToString(), format);
                case "authorname":
                    return PropertyAccess.FormatString(this.Content.AuthorName, format);
                case "authordisplaynamelink":
                    {
                        return PropertyAccess.FormatString(
                            Controllers.ForumUserController.CanLinkToProfile(
                                this.Forum.PortalSettings,
                                this.Forum.MainSettings,
                                this.ModuleId,
                                new Controllers.ForumUserController(this.ModuleId).GetByUserId(
                                    accessingUser.PortalID,
                                    accessingUser.UserID),
                                this.Author.ForumUser)
                                ? Utilities.NavigateURL(this.Forum.PortalSettings.UserTabId,
                                    string.Empty,
                                    $"userId={this.Content.AuthorId}")
                                : string.Empty,
                            format);
                    }

                case "authordisplayname":
                    {
                        var forumUserController = new Controllers.ForumUserController(this.ModuleId);
                        var forumUser = forumUserController.GetByUserId(accessingUser.PortalID, accessingUser.UserID);
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        return PropertyAccess.FormatString(
                        string.IsNullOrEmpty(this.Author?.DisplayName)
                            ? this.Content.AuthorName
                            : Controllers.ForumUserController.GetDisplayName(
                                    this.Forum.PortalSettings,
                                    this.Forum.MainSettings,
                                    bModerate,
                                    forumUser.IsAdmin || forumUser.IsSuperUser,
                                    this.Author.AuthorId,
                                    this.Author.Username,
                                    this.Author.FirstName,
                                    this.Author.LastName,
                                    this.Author.DisplayName).Replace("&amp;#", "&#")
                                .Replace("Anonymous", this.Content.AuthorName),
                        format);
                    }

                case "authorfirstname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.Author?.FirstName)
                            ? this.Content.AuthorName
                            : this.Author.FirstName,
                        format);
                case "authorlastname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.Author?.LastName)
                            ? this.Content.AuthorName
                            : this.Author.LastName,
                        format);
                case "authoremail":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.Author?.Email) ? string.Empty : this.Author.Email.ToString(), format);
                case "lastreplyauthorid":
                    return PropertyAccess.FormatString(this.LastReply?.Author?.AuthorId.ToString(), format);
                case "lastreplyauthorname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.LastReply?.Content?.AuthorName)
                            ? string.Empty
                            : this.LastReply?.Content?.AuthorName,
                        format);
                case "lastreplyauthordisplayname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.LastReply?.Author?.DisplayName)
                            ? this.LastReply?.Content?.AuthorName
                            : this.LastReply?.Author?.DisplayName,
                        format);
                case "lastreplyauthorfirstname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.LastReply?.Author?.FirstName)
                            ? this.LastReply?.Content?.AuthorName
                            : this.LastReply?.Author?.FirstName,
                        format);
                case "lastreplyauthorlastname":
                    return PropertyAccess.FormatString(string.IsNullOrEmpty(this.LastReply?.Author?.LastName)
                            ? this.LastReply?.Content?.AuthorName
                            : this.LastReply?.Author?.LastName,
                        format);
                case "lastreplyauthoremail":
                    return this.LastReplyId > 0
                        ? PropertyAccess.FormatString(this.LastReply?.Author?.Email, format)
                        : string.Empty;
                case "lastpostsubject":
                    {
                        int PageSize = this.Forum.MainSettings.PageSize;
                        var forumUser = new Controllers.ForumUserController(this.ModuleId)
                            .GetByUserId(accessingUser.PortalID, accessingUser.UserID);
                        if (forumUser.UserId > 0)
                        {
                            PageSize = forumUser.PrefPageSize;
                        }

                        if (PageSize < 5)
                        {
                            PageSize = 10;
                        }

                        return PropertyAccess.FormatString(Utilities.GetLastPostSubject(this.LastReply.ReplyId, this.TopicId, this.ForumId, GetTabId(), this.LastReply.Content.Subject, length, pageSize: PageSize, replyCount: this.ReplyCount, canRead: true), format);
                    }

                case "lastpostauthordisplaynamelink":
                    return PropertyAccess.FormatString(
                        this.LastReplyId > 0 && this.LastReply.Author.AuthorId > 0
                            ? Controllers.ForumUserController.CanLinkToProfile(
                                this.Forum.PortalSettings,
                                this.Forum.MainSettings,
                                this.ModuleId,
                                new Controllers.ForumUserController(this.ModuleId).GetByUserId(
                                    accessingUser.PortalID,
                                    accessingUser.UserID),
                                this.LastReply.Author.ForumUser)
                                ? Utilities.NavigateURL(this.Forum.PortalSettings.UserTabId,
                                    string.Empty,
                                    $"userId={this.LastReplyAuthor.AuthorId}")
                                : string.Empty
                            : string.Empty,
                        format);
                case "lastpostauthordisplayname":
                    {
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var forumUserController = new Controllers.ForumUserController(this.ModuleId);
                        var forumUser = forumUserController.GetByUserId(accessingUser.PortalID, accessingUser.UserID);
                        return PropertyAccess.FormatString(
                            this.LastReplyId > 0
                                ? this.LastReply.Author.AuthorId > 0
                                    ? Controllers.ForumUserController.GetDisplayName(
                                            this.Forum.PortalSettings,
                                            this.Forum.MainSettings,
                                            bModerate,
                                            forumUser.IsAdmin || forumUser.IsSuperUser,
                                            this.LastReply.Author.AuthorId,
                                            this.LastReply.Author.Username,
                                            this.LastReply.Author.FirstName,
                                            this.LastReply.Author.LastName,
                                            this.LastReply.Author.DisplayName).Replace("&amp;#", "&#")
                                        .Replace("Anonymous", this.LastReply.Content.AuthorName)
                                    : this.LastReply.Content.AuthorName
                                : string.Empty,
                            format);
                    }

                case "statuscssclass":
                    {
                        var forumUser = new Controllers.ForumUserController(this.ModuleId)
                            .GetByUserId(accessingUser.PortalID, accessingUser.UserID);
                        return PropertyAccess.FormatString(this.GetPostStatusCss(forumUser),
                            format);
                    }

                case "posticoncss":
                    {
                        var forumUser = new Controllers.ForumUserController(this.ModuleId)
                            .GetByUserId(accessingUser.PortalID, accessingUser.UserID);
                        return PropertyAccess.FormatString(this.GetTopicStatusIconCss(forumUser),
                            format);
                    }

                case "posticon":
                    {
                        var forumUser = new Controllers.ForumUserController(this.ModuleId)
                            .GetByUserId(accessingUser.PortalID, accessingUser.UserID);
                        return PropertyAccess.FormatString(this.GetTopicStatusIconCss(forumUser),
                        format);
                    } 

                case "iconpinned":
                    return this.IsPinned
                        ? PropertyAccess.FormatString(this.TopicId.ToString(), format)
                        : string.Empty;
                case "iconunpinned":
                    return !this.IsPinned
                        ? PropertyAccess.FormatString(this.TopicId.ToString(), format)
                        : string.Empty;
                case "iconlocked":
                    return this.IsLocked
                        ? PropertyAccess.FormatString(this.TopicId.ToString(), format)
                        : string.Empty;
                case "iconunlocked":
                    return !this.IsLocked
                        ? PropertyAccess.FormatString(this.TopicId.ToString(), format)
                        : string.Empty;
                case "selectedanswer": /* this applies only to replies not original topic */
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
                            var @params = new List<string>()
                            {
                                $"{ParamKeys.ViewType}={Views.Post}",
                                $"{ParamKeys.Action}={PostActions.TopicEdit}",
                                $"{ParamKeys.ForumId}={this.ForumId}",
                                $"{ParamKeys.TopicId}={this.TopicId}",
                                $"{ParamKeys.PostId}={this.PostId}",
                            };
                            if (this.Forum.SocialGroupId > 0)
                            {
                                @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                            }

                            return PropertyAccess.FormatString(
                                Utilities.NavigateURL(GetTabId(), string.Empty, @params.ToArray()),
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
                              (this.Forum.FeatureSettings.ReplyPostCount <= 0 ||
                                 new Controllers.ForumUserController(this.ModuleId).GetByUserId(
                                     accessingUser.PortalID,
                                     accessingUser.UserID).PostCount >= this.Forum.FeatureSettings.ReplyPostCount))))
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
                                Utilities.NavigateURL(GetTabId(), string.Empty, @params.ToArray()),
                                format);
                        }

                        return string.Empty;
                    }

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
                              (this.Forum.FeatureSettings.ReplyPostCount <= 0 ||
                                 new Controllers.ForumUserController(this.ModuleId).GetByUserId(
                                     accessingUser.PortalID,
                                     accessingUser.UserID).PostCount >= this.Forum.FeatureSettings.ReplyPostCount))))
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
                                Utilities.NavigateURL(GetTabId(), string.Empty, @params.ToArray()),
                                format);
                        }

                        return string.Empty;
                    }

                case "actionquickeditonclick":
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
                            return PropertyAccess.FormatString(
                                $"amaf_quickEdit({this.ModuleId},{this.ForumId},{this.TopicId});",
                                format);
                        }

                        return string.Empty;
                    }

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
                                $"amaf_topicDel({this.ModuleId},{this.ForumId},{this.TopicId});",
                                format);
                        }

                        return string.Empty;
                    }

                case "actionmoveonclick":
                    {
                        var bMove = Controllers.PermissionController.HasPerm(this.Forum.Security.Move,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if (bMove && (bModerate ||
                                      this.Author.AuthorId == accessingUser.UserID))
                        {
                            return PropertyAccess.FormatString(
                                $"javascript:amaf_openMove({this.ModuleId},{this.ForumId},{this.TopicId});",
                                format);
                        }

                        return string.Empty;
                    }

                case "actionlockonclick":
                    {
                        var bLock = Controllers.PermissionController.HasPerm(this.Forum.Security.Lock,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if (!this.IsLocked && bLock && (bModerate || this.Author.AuthorId == accessingUser.UserID))
                        {
                            return PropertyAccess.FormatString(
                                $"javascript:if(confirm('[RESX:Confirm:Lock]')){{amaf_Lock({this.ModuleId},{this.ForumId},{this.TopicId});}};",
                                format);
                        }

                        return string.Empty;
                    }

                case "actionunlockonclick":
                    {
                        var bLock = Controllers.PermissionController.HasPerm(this.Forum.Security.Lock,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if (this.IsLocked && bLock && (bModerate ||
                                                       this.Author.AuthorId == accessingUser.UserID))
                        {
                            return PropertyAccess.FormatString(
                                $"javascript:if(confirm('[RESX:Confirm:UnLock]')){{amaf_Lock({this.ModuleId},{this.ForumId},{this.TopicId});}};",
                                format);
                        }

                        return string.Empty;
                    }

                case "actionpinonclick":
                    {
                        var bPin = Controllers.PermissionController.HasPerm(this.Forum.Security.Pin,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if (!this.IsPinned && bPin && (bModerate ||
                                                       this.Author.AuthorId == accessingUser.UserID))
                        {
                            return PropertyAccess.FormatString(
                                $"javascript:if(confirm('[RESX:Confirm:Pin]')){{amaf_Pin({this.ModuleId},{this.ForumId},{this.TopicId});}};",
                                format);
                        }

                        return string.Empty;
                    }

                case "actionunpinonclick":
                    {
                        var bPin = Controllers.PermissionController.HasPerm(this.Forum.Security.Pin,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        var bModerate = Controllers.PermissionController.HasPerm(this.Forum.Security.Moderate,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if (this.IsPinned && bPin && (bModerate ||
                                                      this.Author.AuthorId == accessingUser.UserID))
                        {
                            return PropertyAccess.FormatString(
                                $"javascript:if(confirm('[RESX:Confirm:UnPin]')){{amaf_Pin({this.ModuleId},{this.ForumId},{this.TopicId});}};",
                                format);
                        }

                        return string.Empty;
                    }

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
                                Utilities.NavigateURL(GetTabId(), string.Empty, editParams.ToArray()),
                                format);
                        }

                        return string.Empty;
                    }

                case "actionbanonclick":
                    {
                        // (Note: can't ban yourself or a superuser/admin)
                        var bBan = Controllers.PermissionController.HasPerm(this.Forum.Security.Ban,
                            accessingUser.PortalID,
                            this.Forum.ModuleId, accessingUser.UserID);
                        if ((bBan || accessingUser.IsAdmin || accessingUser.IsSuperUser) &&
                            (this.Author != null) && (this.Author.AuthorId != -1) && (this.Author.AuthorId != accessingUser.UserID) && (!this.Author.ForumUser.IsSuperUser) && (!this.Author.ForumUser.IsAdmin))
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
                                Utilities.NavigateURL(GetTabId(), string.Empty, @params.ToArray()),
                                format);
                        }
                        return string.Empty;
                    }

                case "actionmarkansweronclick": /* this applies only to replies not original topic */
                    return string.Empty;
            }

            propertyNotFound = true;
            return string.Empty;
        }

        [IgnoreColumn]
        private int GetTabId()
        {
            return this.Forum.PortalSettings.ActiveTab.TabID == -1 || this.Forum.PortalSettings.ActiveTab.TabID == this.Forum.PortalSettings.HomeTabId ? this.Forum.TabId : this.Forum.PortalSettings.ActiveTab.TabID;
        }

        [IgnoreColumn]
        private static string GetTopicTitle(string body)
        {
            if (!string.IsNullOrEmpty(body))
            {

                body = System.Net.WebUtility.HtmlDecode(body);
                body = body.Replace("<br>", System.Environment.NewLine);
                body = Utilities.StripHTMLTag(body);
                body = body.Length > 500 ? body.Substring(0, 500) + "..." : body;
                body = body.Replace("\"", "'");
                body = body.Replace("?", string.Empty);
                body = body.Replace("+", string.Empty);
                body = body.Replace("%", string.Empty);
                body = body.Replace("#", string.Empty);
                body = body.Replace("@", string.Empty);
                return body.Trim();
            }

            return string.Empty;
        }

        internal string GetCacheKey() => string.Format(this.cacheKeyTemplate, this.ModuleId, this.TopicId);

        internal void UpdateCache() => DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.ModuleId, this.GetCacheKey(), this);
    }
}

#pragma warning disable SA1403 // File may only contain a single namespace
namespace DotNetNuke.Modules.ActiveForums
#pragma warning restore SA1403 // File may only contain a single namespace
{
    using System;

    [Obsolete("Deprecated in Community Forums. Scheduled for removal in 09.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.TopicInfo")]
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1402 // File may only contain a single type
    public class TopicInfo : DotNetNuke.Modules.ActiveForums.Entities.TopicInfo
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore SA1402 // File may only contain a single type
    {
    }
}
