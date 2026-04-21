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
    using System.Linq;

    internal class ArchivedUrlController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ArchivedURLInfo>
    {
        internal DotNetNuke.Modules.ActiveForums.Entities.ArchivedURLInfo FindByURL(int portalId, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var normalizedUrl = url.ToLowerInvariant();

            return this.Find("WHERE PortalId = @0 AND URL_Hash = CONVERT(binary(16), HASHBYTES('MD5', @1))", portalId, normalizedUrl)
                .FirstOrDefault(a => IsUrlMatch(a?.Url, normalizedUrl));
        }

        internal static bool IsUrlMatch(string archivedUrl, string normalizedUrl)
        {
            return !string.IsNullOrWhiteSpace(archivedUrl) && archivedUrl.ToLowerInvariant() == normalizedUrl;
        }
    }
}
