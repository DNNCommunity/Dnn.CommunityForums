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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ForumTrackingController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ForumTrackingInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.ForumTrackingInfo GetByUserIdForumId(int moduleId, int userId, int forumId)
        {
                string cachekey = string.Format(CacheKeys.ForumTrackingInfo, moduleId, userId, forumId);
                DotNetNuke.Modules.ActiveForums.Entities.ForumTrackingInfo forumTrackingInfo = DataCache.ContentCacheRetrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ForumTrackingInfo;
                if (forumTrackingInfo == null)
                {
                    // this accommodates duplicates which may exist since currently no uniqueness applied in database
                    forumTrackingInfo = this.Find("WHERE UserId = @0 AND ForumId = @1", userId, forumId).OrderBy(t => t.LastAccessDateTime).FirstOrDefault();
                    DataCache.ContentCacheStore(moduleId, cachekey, forumTrackingInfo);
                }

                return forumTrackingInfo;
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.ForumTrackingInfo> GetForumsTrackingForUser(int userId)
        {
            return this.Find("WHERE UserId = @0", userId).OrderBy(t => t.ForumId).ThenBy(t => t.LastAccessDateTime);
        }
    }
}
