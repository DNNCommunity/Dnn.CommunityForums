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
    using System.Security.Cryptography;
    using System.Text;

    internal class ArchivedUrlController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ArchivedURLInfo>
    {
        internal DotNetNuke.Modules.ActiveForums.Entities.ArchivedURLInfo FindByURL(int portalId, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var normalizedUrl = NormalizeUrl(url);
            var normalizedUrlWithoutLeadingSlash = normalizedUrl.TrimStart('/');
            var normalizedUrlHash = ComputeUrlHash(normalizedUrl);
            var normalizedUrlWithoutLeadingSlashHash = ComputeUrlHash(normalizedUrlWithoutLeadingSlash);

            return this.Find("WHERE PortalId = @0 AND (URL_Hash = @1 OR URL_Hash = @2)", portalId, normalizedUrlHash, normalizedUrlWithoutLeadingSlashHash)
                .FirstOrDefault(a => IsUrlMatch(a?.Url, normalizedUrl, normalizedUrlWithoutLeadingSlash));
        }

        internal static byte[] ComputeUrlHash(string normalizedUrl)
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(Encoding.Unicode.GetBytes(normalizedUrl));
            }
        }

        internal static bool IsUrlMatch(string archivedUrl, string normalizedUrl, string normalizedUrlWithoutLeadingSlash)
        {
            var normalizedArchivedUrl = NormalizeUrl(archivedUrl);
            return normalizedArchivedUrl == normalizedUrl || normalizedArchivedUrl == normalizedUrlWithoutLeadingSlash;
        }

        internal static string NormalizeUrl(string url)
        {
            return string.IsNullOrWhiteSpace(url) ? string.Empty : url.Trim().ToLowerInvariant();
        }
    }
}

