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
    using System.Web.Caching;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Tokens;

    [TableName("activeforums_Likes")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [Cacheable("activeforums_Likes", CacheItemPriority.Normal)]
    internal class LikeInfo : DotNetNuke.Services.Tokens.IPropertyAccess
    {
        private DotNetNuke.Modules.ActiveForums.Entities.ContentInfo contentInfo;
        private DotNetNuke.Modules.ActiveForums.SettingsInfo mainSettings;
        private PortalSettings portalSettings;

        public int Id { get; set; }

        public int PostId { get; set; }

        public int UserId { get; set; }

        public bool Checked { get; set; }

        [IgnoreColumn]
        public int ForumId => this.Content.Post.ForumId;

        [IgnoreColumn]
        public bool IsReply => this.Content.Post.IsReply;

        [IgnoreColumn]
        public bool IsTopic => this.Content.Post.IsTopic;

        [IgnoreColumn]
        public int ReplyId => this.Content.Post.ReplyId;

        [IgnoreColumn]
        public Uri RequestUri { get; set; }

        [IgnoreColumn]
        public string RawUrl { get; set; }

        [IgnoreColumn]
        public int PortalId { get => this.Content.Post.PortalId; }

        [IgnoreColumn]
        public int ModuleId { get => this.Content.ModuleId; }

        public int TopicId => this.Content.Post.TopicId;

        [IgnoreColumn]
        public int ContentId => this.PostId;

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo Content
        {
            get => this.contentInfo ?? (this.contentInfo = this.GetContent());
            set => this.contentInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ContentInfo GetContent()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(this.ContentId, this.ModuleId);
        }
        
        [IgnoreColumn]
        public PortalSettings PortalSettings
        {
            get => this.portalSettings ?? (this.portalSettings = this.LoadPortalSettings());
            set => this.portalSettings = value;
        }

        internal PortalSettings LoadPortalSettings()
        {
            return Utilities.GetPortalSettings(this.PortalId);
        }
        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic => this.Content.Post.Topic;

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum => this.Content.Post.Forum;

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo Author => this.Content.Post.Author;

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
            if (!DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.Forum.Security.Read, accessingUser.PortalID, this.Forum.ModuleId, accessingUser.UserID))
            {
                return string.Empty;
            }

            // replace any embedded tokens in format string
            if (format.Contains("["))
            {
                var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(this.PortalSettings, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID), this, this.RequestUri, this.RawUrl)
                {
                    AccessingUser = accessingUser,
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
                case "replyid":
                    return PropertyAccess.FormatString(this.PostId.ToString(), format);
                case "topicid":
                    return PropertyAccess.FormatString(this.TopicId.ToString(), format);
                case "contentid":
                    return PropertyAccess.FormatString(this.ContentId.ToString(), format);
                case "forumid":
                    return PropertyAccess.FormatString(this.ForumId.ToString(), format);
                case "isliked":
                    return !this.Forum.FeatureSettings.AllowLikes ? string.Empty : PropertyAccess.FormatString(this.Content.Post.IsLikedByUser(new Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID)) ? true.ToString() : string.Empty, format);
                case "likecount":
                    return !this.Forum.FeatureSettings.AllowLikes ? string.Empty : PropertyAccess.FormatString(this.Content.Post.LikeCount.ToString(), format);
                case "authorid":
                    return PropertyAccess.FormatString(this.Content.AuthorId.ToString(), format);
                case "authorname":
                    return PropertyAccess.FormatString(this.Content.AuthorName.ToString(), format);
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

            }

            propertyNotFound = true;
            return string.Empty;
        }

        private int GetTabId()
        {
            return this.Forum.PortalSettings.ActiveTab.TabID == -1 || this.Forum.PortalSettings.ActiveTab.TabID == this.Forum.PortalSettings.HomeTabId ? this.Forum.TabId : this.Forum.PortalSettings.ActiveTab.TabID;
        }
    }
}
