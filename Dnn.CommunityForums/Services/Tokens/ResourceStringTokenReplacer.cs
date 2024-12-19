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

namespace DotNetNuke.Modules.ActiveForums.Services.Tokens
{
    using System.Text;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Tokens;

    internal class ResourceStringTokenReplacer : DotNetNuke.Services.Tokens.IPropertyAccess
    {
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, DotNetNuke.Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            return Utilities.GetSharedResource($"[RESX:{propertyName}]");
        }

        internal static StringBuilder ReplaceResourceTokens(StringBuilder tokenizedText)
        {
            var matches = RegexUtils.GetCachedRegex(@"(\[RESX:.+?\])", RegexOptions.Compiled).Matches(tokenizedText.ToString());
            foreach (Match match in matches)
            {
                var sKey = match.Value;
                string sReplace = Utilities.GetSharedResource(match.Value);
                var newValue = match.Value;
                if (!string.IsNullOrEmpty(sReplace))
                {
                    newValue = sReplace;
                }

                tokenizedText.Replace(sKey, newValue);
            }

            return tokenizedText;
        }
    }
}
