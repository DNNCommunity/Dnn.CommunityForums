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
    using System.Linq;

    using DotNetNuke.Modules.ActiveForums.Services.Cache;

    internal class ArchivedURLController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.ArchivedURLInfo, IArchivedURLController, ArchivedURLController>, IArchivedURLController
    {
        protected override Func<IArchivedURLController> GetFactory()
        {
            return () => new ArchivedURLController();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Readability")]
        public DotNetNuke.Modules.ActiveForums.Entities.ArchivedURLInfo FindByURL(int portalId, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var cacheKey = string.Format(CacheKeys.ArchivedUrl, portalId, url);
            var cached = DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Retrieve(portalId, cacheKey) as DotNetNuke.Modules.ActiveForums.Services.Cache.CacheEntry<DotNetNuke.Modules.ActiveForums.Entities.ArchivedURLInfo>;
            if (cached == null)
            {
                var normalizedUrl = url.Trim().ToLowerInvariant();

                // archived URLs are stored with trailing slash; add for consistent matching
                if (!normalizedUrl.EndsWith("/"))
                {
                    normalizedUrl = normalizedUrl + '/';
                }

                using var ctx = DotNetNuke.Data.DataContext.Instance();
                var archivedURLInfo = ctx.ExecuteQuery<DotNetNuke.Modules.ActiveForums.Entities.ArchivedURLInfo>(
                    System.Data.CommandType.Text,
                    $@"SELECT TOP (1) *
                       FROM {{databaseOwner}}[{{objectQualifier}}communityforums_ArchivedURLs]
                       WHERE PortalId = @0
                         AND URL_Hash = CONVERT(binary(16), HASHBYTES('MD5', CONVERT(varbinary(8000), @1)))
                         AND URL = @1",
                    portalId,
                    normalizedUrl)
                    .FirstOrDefault();

                DotNetNuke.Modules.ActiveForums.Services.Cache.ContentCache.Store(
                    portalId,
                    cacheKey,
                    new CacheEntry<DotNetNuke.Modules.ActiveForums.Entities.ArchivedURLInfo>(archivedURLInfo, archivedURLInfo != null));

                return archivedURLInfo;
            }

            return cached.HasValue ? cached.Value : null;
        }
    }
}
