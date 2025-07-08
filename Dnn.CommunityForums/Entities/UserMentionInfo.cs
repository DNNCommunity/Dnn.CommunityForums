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

    /// <summary>
    /// Represents a user content assignment in the DNN Community Forums module.
    /// </summary>
    [TableName("activeforums_UserMentions")]
    [PrimaryKey("UserMentionId", AutoIncrement = true)]
    [Cacheable("activeforums_UserMentions", CacheItemPriority.Normal)]
    [Scope("ModuleId")]
    public class UserMentionInfo
    {
        [IgnoreColumn]
        private string cacheKeyTemplate => CacheKeys.UserMentionInfo;

        private DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUserInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ContentInfo contentInfo;
        private string contentName;
        private string userName;

        /// <summary>
        /// Gets or sets the UserMentionId.
        /// </summary>
        public int UserMentionId { get; set; }

        /// <summary>
        /// Gets or sets the ContentId.
        /// </summary>
        public int ContentId { get; set; }

        /// <summary>
        /// Gets or sets the User ID.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the PortalId.
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// Gets or sets the ModuleId.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time the user was mentioned.
        /// </summary>
        public DateTime DateMentioned { get; set; } = DateTime.UtcNow;

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

        internal DotNetNuke.Modules.ActiveForums.Entities.ContentInfo GetContent() => this.contentInfo = new Controllers.ContentController().GetById(this.ContentId, this.ModuleId);

        [IgnoreColumn]
        public string UserName { get => this.userName ?? (this.userName = this.ForumUser.DisplayName); set => this.userName = value; }

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

        internal string GetCacheKey() => string.Format(this.cacheKeyTemplate, this.ModuleId, this.UserMentionId);

        internal void UpdateCache() => DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.ModuleId, this.GetCacheKey(), this);
    }
}
