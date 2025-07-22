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
    using System.Web.Caching;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Services.Tokens;

    /// <summary>
    /// Represents a user badge assignment in the DNN Community Forums module.
    /// </summary>
    [TableName("activeforums_UserBadges")]
    [PrimaryKey("UserBadgeId", AutoIncrement = true)]
    [Cacheable("activeforums_UserBadges", CacheItemPriority.Normal)]
    [Scope("ModuleId")]
    public class UserBadgeInfo : DotNetNuke.Services.Tokens.IPropertyAccess
    {
        [IgnoreColumn]
        private string cacheKeyTemplate => CacheKeys.UserBadgeInfo;

        private DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUserInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo badgeInfo;
        private string badgeName;
        private string userName;

        public UserBadgeInfo()
        {
        }

        public UserBadgeInfo(int userBadgeId, int badgeId, string badgeName, int userId, string userName, int portalId, int moduleId, bool assigned)
        {
            this.UserBadgeId = userBadgeId;
            this.BadgeId = badgeId;
            this.UserId = userId;
            this.PortalId = portalId;
            this.ModuleId = moduleId;
            this.BadgeName = badgeName;
            this.UserName = userName;
            this.Assigned = assigned;
        }

        /// <summary>
        /// Gets or sets the badge ID.
        /// </summary>
        public int UserBadgeId { get; set; }

        /// <summary>
        /// Gets or sets the badge ID.
        /// </summary>
        public int BadgeId { get; set; }

        /// <summary>
        /// Gets or sets the User ID.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the PortalId.
        /// </summary>
        public int PortalId { get; set; }

        /// Gets or sets the ModuleId.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time the badge was assigned.
        /// </summary>
        public DateTime DateAssigned { get; set; } = DateTime.UtcNow;

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo Badge
        {
            get
            {
                if (this.badgeInfo == null)
                {
                    this.badgeInfo = this.GetBadge();
                    this.UpdateCache();
                }

                return this.badgeInfo;
            }
            set => this.badgeInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo GetBadge() => this.badgeInfo = new Controllers.BadgeController().GetById(this.BadgeId, this.ModuleId);
        
        [IgnoreColumn]
        public string BadgeName { get => this.badgeName ?? (this.badgeName = this.Badge.Name); set => this.badgeName = value; }

        [IgnoreColumn]
        public string UserName { get => this.userName ?? (this.userName = this.ForumUser.DisplayName); set => this.userName = value; }

        [IgnoreColumn]
        public bool Assigned { get; set; }

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo ForumUser
        {
            get
            {
                if (this.forumUserInfo == null)
                {
                    this.forumUserInfo = this.GetForumUser();
                    this.UpdateCache();
                }

                return this.forumUserInfo;
            }
            set => this.forumUserInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetForumUser() => this.forumUserInfo = new Controllers.ForumUserController((int)this.ModuleId).GetByUserId(this.PortalId, this.UserId);

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
                var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(this.ForumUser.PortalSettings, this, this.ForumUser.RequestUri, this.ForumUser.RawUrl)
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
            try
            {
                switch (propertyName)
                {
                    case "id":
                    case "badgeid":
                        return PropertyAccess.FormatString(this.BadgeId.ToString(), format);
                    case "name":
                        return PropertyAccess.FormatString(this.Badge.Name, format);
                    case "description":
                        return PropertyAccess.FormatString(Utilities.EncodeBrackets(length > 0 && this.Badge.Description.Length > length ? string.Concat(Utilities.StripHTMLTag(this.Badge.Description), "...") : Utilities.StripHTMLTag(this.Badge.Description)), format);
                    case "imageurl":
                        return PropertyAccess.FormatString(length > 0 ? this.Badge.GetBadgeImageUrl(this.PortalId, length) : this.Badge.GetBadgeImageUrl(this.PortalId), format);
                    case "dateassigned":
                        return Utilities.GetUserFormattedDateTime((DateTime?)this.DateAssigned, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(new ArgumentException(string.Format(Utilities.GetSharedResource("[RESX:TokenReplacementException]"), "UserBadgeInfo", this.UserBadgeId, propertyName, format)));
                return string.Empty;
            }

            propertyNotFound = true;
            return string.Empty;
        }

        internal string GetCacheKey() => string.Format(this.cacheKeyTemplate, this.ModuleId, this.UserBadgeId);

        internal void UpdateCache() => DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.ModuleId, this.GetCacheKey(), this);
    }
}
