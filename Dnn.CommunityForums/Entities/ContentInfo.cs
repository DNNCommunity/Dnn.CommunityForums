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

    using DotNetNuke.ComponentModel.DataAnnotations;

    [TableName("activeforums_Content")]
    [PrimaryKey("ContentId", AutoIncrement = true)]
    public class ContentInfo
    {
        [IgnoreColumn] private string cacheKeyTemplate => CacheKeys.ContentInfo;

        private DotNetNuke.Modules.ActiveForums.Entities.IPostInfo postInfo;

        public int ContentId { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } /* TODO: Once Reply_Save etc. moved from stored procedures to DAL2 for saving, update this to auto-set dates */

        public DateTime DateUpdated { get; set; } = DateTime.UtcNow; /* TODO: Once Reply_Save etc. moved from stored procedures to DAL2 for saving, update this to auto-set dates */

        public int AuthorId { get; set; }

        public string AuthorName { get; set; }

        public bool IsDeleted { get; set; }

        public string IPAddress { get; set; }

        public int ContentItemId { get; set; }

        public int ModuleId { get; set; }

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.IPostInfo Post
        {
            get
            {
                if (this.postInfo == null)
                {
                    this.postInfo = this.GetPost();
                    this.UpdateCache();
                }

                return this.postInfo;
            }
            set => this.postInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.IPostInfo GetPost()
        {
            if (this.postInfo == null)
            {
                this.postInfo = (DotNetNuke.Modules.ActiveForums.Entities.IPostInfo)new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ModuleId).GetByContentId(this.ContentId);
                if (this.postInfo == null)
                {
                    this.postInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ModuleId).GetByContentId(this.ContentId);
                }
            }

            return this.postInfo;
        }

        internal string GetCacheKey() => string.Format(this.cacheKeyTemplate, this.ModuleId, this.ContentId);

        internal void UpdateCache() => DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.ModuleId, this.GetCacheKey(), this);
    }
}
