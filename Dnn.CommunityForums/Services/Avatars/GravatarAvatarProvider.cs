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

using System.Linq;

namespace DotNetNuke.Modules.ActiveForums.Services.Avatars
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal class GravatarAvatarProvider : IAvatarProvider
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Fetches the avatar image as a byte array.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<(string ContentType, byte[] ImageBytes, DateTime? LastModifiedDateTime)> GetAvatarImageAsync(string email)
        {
            var url = GetAvatarUrl(email);
            if (!string.IsNullOrEmpty(url))
            {
                using (var response = await httpClient.GetAsync(url).ConfigureAwait(false))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                        DateTime? lastModifiedDateTime = null;
                        var lastModifiedHeader = response.Content.Headers.Contains("Last-Modified")
                            ? string.Join(",", response.Content.Headers.GetValues("Last-Modified"))
                            : null;
                        if (!string.IsNullOrEmpty(lastModifiedHeader))
                        {
                            lastModifiedDateTime = DateTime.ParseExact(lastModifiedHeader, "R", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal);
                            if (lastModifiedDateTime > new DateTime(1984, 1, 12)) /* default generic gravatar date */
                            {
                                var contentType = response.Content.Headers.ContentType?.MediaType;
                                var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                                if (!bytes.SequenceEqual(ConvertBase64ToByteArray(GravatarGenericAvatarAsBase64())))
                                {
                                    return (contentType, bytes, lastModifiedDateTime);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                }
            }

            return (null, null, null);
        }

        private static string GetAvatarUrl(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(new ArgumentException("Email must be provided.", nameof(email)));
                return null;
            }

            var hash = Utilities.GetSha256Hash(email.Trim().ToLowerInvariant());
            return $"https://www.gravatar.com/avatar/{hash}";
        }

        private static byte[] ConvertBase64ToByteArray(string base64String)
        {
            // Remove potential Base64 image header if present
            if (base64String.Contains(","))
            {
                base64String = base64String.Split(',')[1];
            }

            return Convert.FromBase64String(base64String);
        }

        private static string GravatarGenericAvatarAsBase64()
        {
            return "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";
        }
    }
}
