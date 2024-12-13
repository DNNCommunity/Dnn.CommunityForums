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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    
    using DotNetNuke.Collections;

    internal partial class LikeController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.LikeInfo>
    {
        private readonly int moduleId = -1;
        private readonly int portalId = -1;

        internal override string cacheKeyTemplate => CacheKeys.LikeInfo;

        internal LikeController()
        {
        }

        internal LikeController(int portalId, int moduleId)
        {
            this.portalId = portalId;
            this.moduleId = moduleId;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.LikeInfo GetById(int contentId)
        {
            var cachekey = this.GetCacheKey(moduleId: this.moduleId, id: contentId);
            DotNetNuke.Modules.ActiveForums.Entities.LikeInfo like = DataCache.ContentCacheRetrieve(this.moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.LikeInfo;
            if (like == null)
            {
                like = this.GetById(contentId, this.moduleId);
                if (like != null)
                {
                    like.ModuleId = this.moduleId;
                    like.PortalId = this.portalId;
                    like.GetContent();
                    like.Content?.GetPost();
                }

                UpdateCache(this.moduleId, cachekey, like);
            }

            return like;
        }

        public bool GetForUser(int userId, int postId)
        {
            return userId > 0 && this.Find("WHERE PostId = @0 AND UserId = @1 AND Checked = 1", postId, userId).Any();
        }

        public (int count, bool liked) Get(int userId, int postId)
        {
            return (this.Count(postId), this.GetForUser(userId, postId));
        }

        public List<DotNetNuke.Modules.ActiveForums.Entities.LikeInfo> GetForPost(int postId)
        {
            return this.Find("WHERE PostId = @0 AND Checked = 1", postId).ForEach(l =>
            {
                l.PortalId = this.portalId;
                l.ModuleId = this.moduleId;
                l.GetContent();
                l.Content?.GetPost();
            }).ToList();
        }

        public int Count(int postId)
        {
            return this.Count("WHERE PostId = @0 AND Checked = 1", postId);
        }

        public int Like(int contentId, int userId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.LikeInfo like = this.Find("WHERE PostId = @0 AND UserId = @1", contentId, userId).FirstOrDefault();
            if (like != null)
            {
                if (like.Checked)
                {
                    like.Checked = false;
                }
                else
                {
                    like.Checked = true;
                }

                this.Update(like);
            }
            else
            {
                like = new DotNetNuke.Modules.ActiveForums.Entities.LikeInfo
                {
                    PostId = contentId,
                    UserId = userId,
                    Checked = true
                };
                this.Insert(like);
            }

            like.UpdateCache();
            return this.Count(contentId);
        }
    }
}

#pragma warning disable SA1403 // File may only contain a single namespace
namespace DotNetNuke.Modules.ActiveForums
#pragma warning restore SA1403 // File may only contain a single namespace
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Data;

    [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Replace with DotNetNuke.Modules.ActiveForums.Controllers.LikeController")]
#pragma warning disable SA1400 // Access modifier should be declared
    class LikesController : DotNetNuke.Modules.ActiveForums.Controllers.LikeController
#pragma warning restore SA1400 // Access modifier should be declared
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Replace with DotNetNuke.Modules.ActiveForums.Controllers.LikeController.GetForPost()")]
#pragma warning disable SA1600 // Elements should be documented
        public new List<DotNetNuke.Modules.ActiveForums.Likes> GetForPost(int postId)
#pragma warning restore SA1600 // Elements should be documented
        {
            IDataContext ctx = DataContext.Instance();
            IRepository<DotNetNuke.Modules.ActiveForums.Entities.LikeInfo> repo = ctx.GetRepository<DotNetNuke.Modules.ActiveForums.Entities.LikeInfo>();
            List<DotNetNuke.Modules.ActiveForums.Likes> likes = new List<DotNetNuke.Modules.ActiveForums.Likes>();
            foreach (DotNetNuke.Modules.ActiveForums.Entities.LikeInfo like in base.GetForPost(postId))
            {
                likes.Add((DotNetNuke.Modules.ActiveForums.Likes)like);
            }

            return likes;
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Replace with DotNetNuke.Modules.ActiveForums.Controllers.LikeController.Like()")]
#pragma warning disable SA1600 // Elements should be documented
        public new void Like(int contentId, int userId)
#pragma warning restore SA1600 // Elements should be documented
        {
            base.Like(contentId, userId);
        }
    }
}
