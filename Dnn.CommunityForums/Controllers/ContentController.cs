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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;

    internal class ContentController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ContentInfo>
    {
        internal override string cacheKeyTemplate => CacheKeys.ContentInfo;

        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo GetById(int contentId, int moduleId)
        {
            var cachekey = this.GetCacheKey(moduleId: moduleId, id: contentId);
            DotNetNuke.Modules.ActiveForums.Entities.ContentInfo content = DataCache.ContentCacheRetrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ContentInfo;
            if (content == null)
            {
                content = base.GetById(contentId);
                if (moduleId.Equals(-1) && !content.Equals(null))
                {
                    content.UpdateCache();
                }
                else
                {
                    DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(moduleId, cachekey, content);
                }
            }

            return content;
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used. Use GetById(int contentId, int moduleId).")]
        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo GetById(int contentId)
        {
            return this.GetById(contentId, -1);
        }
    }
}
