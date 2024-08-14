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

namespace DotNetNuke.Modules.ActiveForums.Services.Tokens
{
    using System.Text.RegularExpressions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Tokens;

    internal class TokenReplacer : BaseCustomTokenReplace, IPropertyAccess
    {
        public TokenReplacer(DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo )
        {
            this.TokenContext.CurrentAccessLevel = Scope.DefaultSettings;
            this.PropertySource["resx"] = this;
            this.PropertySource["forum"] = forumInfo;
            this.PropertySource["forumuser"] = forumUser;
            this.PropertySource["user"] = forumUser.UserInfo;
            this.PropertySource["portal"] = portalSettings;
        }

        public TokenReplacer(DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topicInfo)
        {
            this.TokenContext.CurrentAccessLevel = Scope.DefaultSettings;
            this.PropertySource["resx"] = this;
            this.PropertySource["forum"] = topicInfo.Forum;
            this.PropertySource["forumtopic"] = topicInfo;
            this.PropertySource["forumuser"] = forumUser;
            this.PropertySource["user"] = forumUser.UserInfo;
            this.PropertySource["portal"] = portalSettings;
        }
        
        public TokenReplacer(DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.IPostInfo postInfo)
        {
            this.TokenContext.CurrentAccessLevel = Scope.DefaultSettings;
            this.PropertySource["resx"] = this;
            this.PropertySource["forum"] = postInfo.Forum;
            this.PropertySource["forumtopic"] = postInfo.Topic;
            this.PropertySource["forumpost"] = postInfo;
            this.PropertySource["forumuser"] = forumUser;
            this.PropertySource["user"] = forumUser.UserInfo;
            this.PropertySource["portal"] = portalSettings;
        }
        public TokenReplacer(DotNetNuke.Entities.Portals.PortalSettings portalSettings)
        {
            this.TokenContext.CurrentAccessLevel = Scope.DefaultSettings;
            this.PropertySource["resx"] = this;
            this.PropertySource["portal"] = portalSettings;
        }

        public new string ReplaceTokens(string source)
        {
            return base.ReplaceTokens(source);
        }

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
        
        internal static string ReplaceResourceTokens(string tokenizedText)
        {

            const string pattern = @"(\[RESX:.+?\])";
            var matches = new Regex(pattern).Matches(tokenizedText);
            foreach (Match match in matches)
            {
                var sKey = match.Value;
                string sReplace = Utilities.GetSharedResource(match.Value);
                var newValue = match.Value;
                if (!string.IsNullOrEmpty(sReplace))
                {
                    newValue = sReplace;
                }
                tokenizedText = tokenizedText.Replace(sKey, newValue);
            }

            return tokenizedText;
        }
    }
}
