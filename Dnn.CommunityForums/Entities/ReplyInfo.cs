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

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Policy;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Services.Tokens;

    [TableName("activeforums_Replies")]
    [PrimaryKey("ReplyId")]
    public partial class ReplyInfo : DotNetNuke.Modules.ActiveForums.Entities.IPostInfo
    {
        [IgnoreColumn]
        private string cacheKeyTemplate => CacheKeys.ReplyInfo;

        private DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topicInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ContentInfo contentInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author;
        private int? likeCount;

        [IgnoreColumn]
        public int ForumId
        {
            get => this.Topic.ForumId;
            set => this.Topic.ForumId = value;
        }

        [IgnoreColumn]
        public int PostId { get => this.ReplyId; }

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
        public Uri RequestUri { get; set; }

        [IgnoreColumn]
        public string RawUrl { get; set; }

        [IgnoreColumn]
        public int PortalId { get; set; }

        [IgnoreColumn]
        public int ModuleId { get; set; }

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic
        {
            get
            {
                if (this.topicInfo == null)
                {
                    this.topicInfo = this.GetTopic();
                    this.UpdateCache();
                }

                return this.topicInfo;
            }

            set => this.topicInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetTopic()
        {
            return this.topicInfo = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ModuleId).GetById(this.TopicId);
        }

        [IgnoreColumn]
        public string Subject => this.Content.Subject;

        [IgnoreColumn]
        public string Body => this.Content.Body;

        [IgnoreColumn]
        public string Summary => this.Content.Summary;

        [IgnoreColumn]
        public int LikeCount
        {
            get
            {
                if (this.likeCount == null)
                {
                    this.likeCount = new DotNetNuke.Modules.ActiveForums.Controllers.LikeController(this.PortalId, this.ModuleId).Count(this.ContentId);
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
        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo Content
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

        internal DotNetNuke.Modules.ActiveForums.Entities.ContentInfo GetContent()
        {
            return this.contentInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(this.ContentId, this.ModuleId);
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

        internal DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo GetAuthor(int portalId, int moduleId, int authorId)
        {
            return this.author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, moduleId, authorId);
        }

        internal DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus GetReplyStatusForUser(ForumUserInfo forumUser)
        {
            if (!DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.ViewRoleIds, forumUser?.UserRoleIds))
            {
                return DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.Forbidden;
            }

            try
            {
                if (forumUser?.UserId == -1)
                {
                    return Enums.ReplyStatus.Unread;
                }

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
        public bool RunningInViewer
        {
            get
            {
                var portalSettings = Utilities.GetPortalSettings();
                if (portalSettings == null)
                {
                    portalSettings = this.Forum.PortalSettings;
                }

                return portalSettings.ActiveTab != null && portalSettings.ActiveTab.Modules.Cast<DotNetNuke.Entities.Modules.ModuleInfo>().Any(
                    m => m.ModuleDefinition.DefinitionName.Equals(Globals.ModuleFriendlyName + " Viewer", StringComparison.OrdinalIgnoreCase) ||
                    m.ModuleDefinition.DefinitionName.Equals(Globals.ModuleName + " Viewer", StringComparison.OrdinalIgnoreCase));
            }
        }

        [IgnoreColumn]
        public int ForumsOrViewerModuleId
        {
            get
            {
                if (!this.RunningInViewer)
                {
                    return this.ModuleId;
                }

                if (this.Forum.PortalSettings.ActiveTab != null)
                {
                    foreach (DotNetNuke.Entities.Modules.ModuleInfo module in this.Forum.PortalSettings.ActiveTab.Modules.Cast<DotNetNuke.Entities.Modules.ModuleInfo>().Where(m => m.ModuleDefinition.DefinitionName.Equals(Globals.ModuleFriendlyName + " Viewer", StringComparison.OrdinalIgnoreCase) || m.ModuleDefinition.DefinitionName.Equals(Globals.ModuleName + " Viewer", StringComparison.OrdinalIgnoreCase)))
                    {
                        return module.ModuleID;
                    }
                }

                return DotNetNuke.Common.Utilities.Null.NullInteger;
            }
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
            if (!DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.ReadRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser)))
            {
                return string.Empty;
            }

            // replace any embedded tokens in format string
            if (format.Contains("["))
            {
                var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(this.Forum.PortalSettings, new Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID), this, this.RequestUri, this.RawUrl) { AccessingUser = accessingUser, };
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
            try
            {
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
                    case "forumid":
                        return PropertyAccess.FormatString(this.ForumId.ToString(), format);
                    case "subject":
                        {
                            string sPollImage = (this.Topic.TopicType == TopicTypes.Poll ? DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.GetTokenFormatString("[POLLIMAGE]", this.Forum.PortalSettings, accessingUser.Profile.PreferredLocale) : string.Empty);
                            return PropertyAccess.FormatString(Utilities.EncodeBrackets(length > 0 && this.Subject.Length > length ? string.Concat(Utilities.StripHTMLTag(this.Subject).Replace("[", "&#91").Replace("]", "&#93"), "...") : Utilities.StripHTMLTag(this.Subject).Replace("[", "&#91").Replace("]", "&#93") + sPollImage), format);
                        }

                    case "subjectlink":
                        {
                            string sTopicURL = new ControlUtils().BuildUrl(this.Forum.PortalSettings.PortalId, this.GetTabId(), this.Forum.ModuleId, this.Forum.ForumGroup.PrefixURL, this.Forum.PrefixURL, this.Forum.ForumGroupId, this.Forum.ForumID, this.TopicId, this.Topic.TopicUrl, -1, -1, string.Empty, 1, this.ContentId, this.Forum.SocialGroupId);
                            string sPollImage = (this.Topic.TopicType == TopicTypes.Poll ? DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.GetTokenFormatString("[POLLIMAGE]", this.Forum.PortalSettings, accessingUser.Profile.PreferredLocale) : string.Empty);
                            string slink;
                            var @params = new List<string> { $"{ParamKeys.TopicId}={this.TopicId}", $"{ParamKeys.ContentJumpId}={this.Topic.LastReplyId}", };

                            if (this.Forum.SocialGroupId > 0)
                            {
                                @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                            }

                            if (sTopicURL == string.Empty)
                            {
                                @params = new List<string> { $"{ParamKeys.ForumId}={this.ForumId}", $"{ParamKeys.TopicId}={this.TopicId}", $"{ParamKeys.ViewType}={Views.Topic}", };
                                if (this.Forum.MainSettings.UseShortUrls)
                                {
                                    @params.Add($"{ParamKeys.TopicId}={this.TopicId}");
                                }

                                slink = Utilities.NavigateURL(this.GetTabId(), string.Empty, @params.ToArray());
                            }
                            else
                            {
                                slink = sTopicURL;
                            }

                            return PropertyAccess.FormatString(slink, format) + sPollImage;
                        }

                    case "likeslink":
                        {
                            if (this.Forum.FeatureSettings.AllowLikes)
                            {
                                string linkUrl = new ControlUtils().BuildUrl(this.Forum.PortalSettings.PortalId, tabId: this.GetTabId(), moduleId: this.Forum.ModuleId, groupPrefix: this.Forum.ForumGroup.PrefixURL, forumPrefix: this.Forum.PrefixURL, forumGroupId: this.Forum.ForumGroupId, forumID: this.Forum.ForumID, topicId: this.TopicId, topicURL: this.Topic.TopicUrl, tagId: -1, categoryId: -1, otherPrefix: Views.Likes, pageId: 1, contentId: this.ContentId, socialGroupId: this.Forum.SocialGroupId);

                                if (string.IsNullOrEmpty(linkUrl))
                                {
                                    var @params = new List<string> { $"{ParamKeys.ViewType}={Views.Grid}", $"{ParamKeys.GridType}={Views.Likes}", $"{ParamKeys.ContentId}={this.ContentId}", };

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

                    case "summary":
                        return PropertyAccess.FormatString(Utilities.EncodeBrackets(length > 0 && this.Summary.Length > length ? this.Summary.Substring(0, length) : this.Summary), format);
                    case "body":
                        return PropertyAccess.FormatString(Utilities.EncodeBrackets(Utilities.EncodeCodeBlocks(length > 0 && this.Content.Body.Length > length ? this.Content.Body.Substring(0, length) : this.Content.Body)), format);
                    case "bodytitle":
                        return PropertyAccess.FormatString(Utilities.EncodeBrackets(GetTopicTitle(this.Content.Body)), format);
                    case "link":
                        {
                            string subject = Utilities.StripHTMLTag(System.Net.WebUtility.HtmlDecode(this.Subject)).Replace("\"", string.Empty).Replace("#", string.Empty).Replace("%", string.Empty).Replace("+", string.Empty);
                            string sBodyTitle = GetTopicTitle(this.Content.Body);
                            var slink = "<a title=\"" + sBodyTitle + "\" href=\"" + this.GetLink() + "\">" + subject + "</a>";

                            return PropertyAccess.FormatString(slink, format);
                        }

                    case "isliked":
                        return !this.Forum.FeatureSettings.AllowLikes ? string.Empty : PropertyAccess.FormatString(this.IsLikedByUser(new Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)) ? true.ToString() : string.Empty, format);
                    case "likecount":
                        return !this.Forum.FeatureSettings.AllowLikes ? string.Empty : PropertyAccess.FormatString(this.LikeCount.ToString(), format);
                    case "likeonclick":
                        {
                            var bReply = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.ReplyRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                            if (this.Forum.FeatureSettings.AllowLikes)
                            {
                                return PropertyAccess.FormatString(bReply ? $"amaf_likePost({this.Forum.ForumsOrViewerModuleId},{this.Forum.ForumID},{this.ContentId})" : string.Empty, format);
                            }
                        }

                        return string.Empty;
                    case "authorid":
                        return PropertyAccess.FormatString(this.Content.AuthorId.ToString(), format);
                    case "authorname":
                        return PropertyAccess.FormatString(this.Content.AuthorName.ToString(), format);
                    case "authordisplaynamelink":
                        {
                            return PropertyAccess.FormatString(Controllers.ForumUserController.CanLinkToProfile(this.Forum.PortalSettings, this.Forum.MainSettings, this.ModuleId, new Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID), this.Author.ForumUser) ? Utilities.NavigateURL(this.Forum.PortalSettings.UserTabId, string.Empty, $"userId={this.Content.AuthorId}") : string.Empty, format);
                        }

                    case "authordisplayname":
                        return PropertyAccess.FormatString(string.IsNullOrEmpty(this.Author?.DisplayName) ? this.Content.AuthorName : DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(this.Forum.PortalSettings, this.Forum.MainSettings, isMod: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).GetIsMod(this.ModuleId), isAdmin: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).IsAdmin || new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).IsSuperUser, this.Author.AuthorId, this.Author.Username, this.Author.FirstName, this.Author.LastName, this.Author.DisplayName).Replace("&amp;#", "&#").Replace("Anonymous", this.Content.AuthorName), format);
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
                        if (this.Forum.GetIsMod(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)) && this.Content.DateUpdated != this.Content.DateCreated)
                        {
                            return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime((DateTime?)this.Content.DateUpdated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)), format);
                        }

                        return string.Empty;
                    case "modipaddress":
                        return this.Forum.GetIsMod(new Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)) ? PropertyAccess.FormatString(this.Content.IPAddress, format) : string.Empty;
                    case "editdate":
                        return PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime((DateTime?)this.Content.DateUpdated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)), format);
                    case "selectedanswer":
                        if (this.IsReply && this.Topic.StatusId == 3 && this.StatusId == 1)
                        {
                            return PropertyAccess.FormatString(Utilities.GetSharedResource("[RESX:Status:Answer]"), format);
                        }

                        return string.Empty;

                    case "actioneditonclick":
                        {
                            var bEdit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.EditRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                            var bModerate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.ModerateRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                            if (bEdit && (bModerate || ((this.Author.AuthorId == accessingUser.UserID) && (this.Forum.MainSettings.EditInterval == 0 || DateTime.UtcNow.Subtract(this.Content.DateCreated).TotalMinutes > this.Forum.MainSettings.EditInterval))))
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

                                return PropertyAccess.FormatString(Utilities.NavigateURL(this.GetTabId(), string.Empty, editParams.ToArray()), format);
                            }

                            return string.Empty;
                        }

                    case "actionreplyonclick":
                        {
                            var bReply = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.ReplyRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                            var bTrust = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.TrustRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                            var bModerate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.ModerateRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                            if (bReply && (bTrust || bModerate || ((!this.Topic.IsLocked) && (this.Forum.FeatureSettings.ReplyPostCount <= 0 || new Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).PostCount >= this.Forum.FeatureSettings.ReplyPostCount))))
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

                                return PropertyAccess.FormatString(Utilities.NavigateURL(this.GetTabId(), string.Empty, @params.ToArray()), format);
                            }
                        }

                        return string.Empty;
                    case "actionquoteonclick":
                        {
                            var bReply = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.ReplyRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                            var bTrust = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.TrustRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                            var bModerate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.ModerateRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                            if (bReply && (bTrust || bModerate || ((!this.Topic.IsLocked) && (this.Forum.FeatureSettings.ReplyPostCount <= 0 || new Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).PostCount >= this.Forum.FeatureSettings.ReplyPostCount))))
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

                                return PropertyAccess.FormatString(Utilities.NavigateURL(this.GetTabId(), string.Empty, @params.ToArray()), format);
                            }
                        }

                        return string.Empty;
                    case "actiondeleteonclick":
                        {
                            var bDelete = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.DeleteRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                            var bModerate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.ModerateRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                            if (bDelete && (bModerate || (this.Author.AuthorId == accessingUser.UserID && !this.Topic.IsLocked)))
                            {
                                return PropertyAccess.FormatString($"amaf_postDel({this.ForumsOrViewerModuleId},{this.ForumId},{this.TopicId},{this.ReplyId})", format);
                            }
                        }

                        return string.Empty;
                    case "actionalertonclick":
                        {
                            if (accessingUser.UserID > 0)
                            {
                                var editParams = new List<string>()
                                {
                                    $"{ParamKeys.ViewType}={Views.ModerateReport}", $"{ParamKeys.ForumId}={this.ForumId}", $"{ParamKeys.TopicId}={this.TopicId}", $"{ParamKeys.ReplyId}={this.ReplyId}",
                                };
                                if (this.Forum.SocialGroupId > 0)
                                {
                                    editParams.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
                                }

                                return PropertyAccess.FormatString(Utilities.NavigateURL(this.GetTabId(), string.Empty, editParams.ToArray()), format);
                            }
                        }

                        return string.Empty;

                    case "actionbanonclick":
                        {
                            // (Note: can't ban yourself or a superuser/admin)
                            var bBan = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.ManageUsersRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                            if ((bBan || accessingUser.IsAdmin || accessingUser.IsSuperUser) && (this.Author.AuthorId != -1) && (this.Author.AuthorId != accessingUser.UserID) && (this.Author != null) && (!this.Author.ForumUser.IsSuperUser) && (!this.Author.ForumUser.IsAdmin))
                            {
                                var @params = new List<string>()
                                {
                                    $"{ParamKeys.ViewType}={Views.ModerateBan}",
                                    $"{ParamKeys.ForumId}={this.ForumId}",
                                    $"{ParamKeys.TopicId}={this.TopicId}",
                                    $"{ParamKeys.ReplyId}={this.ReplyId}",
                                    $"{ParamKeys.AuthorId}={this.Author.AuthorId}",
                                };

                                return PropertyAccess.FormatString(Utilities.NavigateURL(this.GetTabId(), string.Empty, @params.ToArray()), format);
                            }
                        }

                        return string.Empty;
                    case "actionmarkansweronclick":
                        {
                            if (this.IsReply && this.Topic.StatusId > 0 && this.Topic.StatusId != 3)
                            {
                                var bEdit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.EditRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                                var bModerate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(this.Forum.Security.ModerateRoleIds, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.Forum.PortalSettings, accessingUser));
                                if (bEdit && (bModerate || (accessingUser.UserID == this.Topic.Author.AuthorId && !this.Topic.IsLocked)))
                                {
                                    return PropertyAccess.FormatString($"javascript:amaf_MarkAsAnswer({this.ForumsOrViewerModuleId},{this.ForumId},{this.TopicId},{this.ReplyId});", format);
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
            }
            catch (Exception ex)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(new ArgumentException(string.Format(Utilities.GetSharedResource("[RESX:TokenReplacementException]"), "ReplyInfo", this.ReplyId, propertyName, format)));
                return string.Empty;
            }

            propertyNotFound = true;
            return string.Empty;
        }

        [IgnoreColumn]
        private int GetTabId()
        {
            return this.Forum.GetTabId();
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

        [IgnoreColumn]
        public string GetLink()
        {
            string link = new ControlUtils().BuildUrl(portalId: this.Forum.PortalSettings.PortalId, tabId: this.GetTabId(), moduleId: this.Forum.ModuleId, groupPrefix: this.Forum.ForumGroup.PrefixURL, forumPrefix: this.Forum.PrefixURL, forumGroupId: this.Forum.ForumGroupId, forumID: this.Forum.ForumID, topicId: this.TopicId, topicURL: this.Topic.TopicUrl, tagId: -1, categoryId: -1, otherPrefix: string.Empty, pageId: 1, contentId: this.ContentId, socialGroupId: this.Forum.SocialGroupId);

            if (!string.IsNullOrEmpty(link))
                {
                if (link.EndsWith("/"))
                {
                    link += Utilities.UseFriendlyURLs(this.ModuleId) ? $"#{this.ReplyId}" : $"?{ParamKeys.ContentJumpId}={this.ReplyId}";
                }

                return link;
            }

            var @params = new List<string>
            {
                $"{ParamKeys.ForumId}={this.ForumId}",
                $"{ParamKeys.TopicId}={this.TopicId}",
                $"{ParamKeys.ReplyId}={this.ReplyId}",
                $"{ParamKeys.ViewType}={Views.Post}",
                $"{ParamKeys.ContentJumpId}={this.ReplyId}",
            };

            if (this.Forum.SocialGroupId > 0)
            {
                @params.Add($"{Literals.GroupId}={this.Forum.SocialGroupId}");
            }

            if (this.Forum.MainSettings.UseShortUrls)
            {
                @params.Clear();
                @params.Add($"{ParamKeys.ForumId}={this.ForumId}");
                @params.Add($"{ParamKeys.TopicId}={this.TopicId}");
                @params.Add($"{ParamKeys.ContentJumpId}={this.ReplyId}");
            }

            return Utilities.NavigateURL(this.GetTabId(), string.Empty, @params.ToArray());
        }

        internal string GetCacheKey() => string.Format(this.cacheKeyTemplate, this.ModuleId, this.ReplyId);

        internal void UpdateCache() => DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.ModuleId, this.GetCacheKey(), this);
    }
}
