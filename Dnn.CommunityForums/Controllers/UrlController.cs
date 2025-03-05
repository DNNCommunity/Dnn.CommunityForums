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
    using DotNetNuke.Abstractions;

    internal static class UrlController
    {
        internal static string BuildTopicUrlSegment(int portalId, int moduleId, int topicId, string subject, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo)
        {
            var cleanSubject = Utilities.CleanName(subject).ToLowerInvariant();
            if (Utilities.IsNumeric(cleanSubject))
            {
                cleanSubject = "Topic-" + cleanSubject;
            }

            var topicUrl = cleanSubject;
            var urlPrefix = "/";

            if (!string.IsNullOrEmpty(forumInfo.ForumGroup.PrefixURL))
            {
                urlPrefix += forumInfo.ForumGroup.PrefixURL + "/";
            }

            if (!string.IsNullOrEmpty(forumInfo.PrefixURL))
            {
                urlPrefix += forumInfo.PrefixURL + "/";
            }

            var urlToCheck = urlPrefix + cleanSubject;

            var topicsDb = new Data.Topics();
            for (var u = 0; u <= 200; u++)
            {
                var tid = topicsDb.TopicIdByUrl(portalId, moduleId, urlToCheck);
                if (tid > 0 && tid == topicId)
                {
                    break;
                }

                if (tid <= 0)
                {
                    break;
                }

                topicUrl = u + 1 + "-" + cleanSubject;
                urlToCheck = urlPrefix + topicUrl;
            }

            if (topicUrl.Length > 150)
            {
                topicUrl = topicUrl.Substring(0, 149);
                topicUrl = topicUrl.Substring(0, topicUrl.LastIndexOf("-", StringComparison.Ordinal));
            }

            return topicUrl;
        }

        internal static string BuildForumUrlSegment(int portalId, int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo)
        {
            string url = "/";

            if (!string.IsNullOrEmpty(forumInfo.ForumGroup.PrefixURL))
            {
                url += forumInfo.ForumGroup.PrefixURL + "/";
            }

            if (!string.IsNullOrEmpty(forumInfo.PrefixURL))
            {
                url += forumInfo.PrefixURL + "/";
            }

            return url;
        }

        internal static string BuildForumUrl(INavigationManager navigationManager, DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings, SettingsInfo mainSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo)
        {
            // Build the forum Url
            return mainSettings.UseShortUrls ? navigationManager.NavigateURL(forumInfo.TabId, portalSettings, string.Empty, new[] { $"{ParamKeys.ForumId}={forumInfo.ForumID}" })
                : navigationManager.NavigateURL(forumInfo.TabId, portalSettings, string.Empty, new[] { $"{ParamKeys.ForumId}={forumInfo.ForumID}", $"{ParamKeys.ViewType}={Views.Topics}" });
        }

        internal static string BuildModeratorUrl(INavigationManager navigationManager, DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings, SettingsInfo mainSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo)
        {
            return navigationManager.NavigateURL(forumInfo.TabId, portalSettings, string.Empty, new[] { $"{ParamKeys.ViewType}={Views.ModerateTopics}", $"{ParamKeys.ForumId}={forumInfo.ForumID}" });
        }
    }
}
