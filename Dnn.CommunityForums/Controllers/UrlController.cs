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
    using System.Data;

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

            for (var u = 0; u <= 200; u++)
            {
                var tid = TopicIdByUrl(portalId, moduleId, urlToCheck);
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

        internal static void ArchiveURL(int portalId, int forumGroupId, int forumId, int topicId, string uRL)
        {
            using var ctx = DotNetNuke.Data.DataContext.Instance();
            ctx.Execute(CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}communityforums_URL_Archive", portalId, forumGroupId, forumId, topicId, uRL);
        }

        internal static string GetUrl(int moduleId, int forumGroupId, int forumId, int topicId, int userId, int contentId)
        {
            try
            {
                using var ctx = DotNetNuke.Data.DataContext.Instance();
                return ctx.ExecuteScalar<string>(CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}communityforums_Util_GetUrl", moduleId, forumGroupId, forumId, topicId, userId, contentId);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool CheckForumURL(int portalId, int moduleId, string vanityName, int forumId, int forumGroupId)
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo fg = DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController.Instance.GetById(forumGroupId, moduleId);
                if (!string.IsNullOrEmpty(fg.PrefixURL))
                {
                    vanityName = fg.PrefixURL + "/" + vanityName;
                }

                int tmpForumId = -1;
                using var ctx = DotNetNuke.Data.DataContext.Instance();
                tmpForumId = ctx.ExecuteScalar<int>(CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}communityforums_URL_CheckForumVanity", portalId, vanityName);
                if (tmpForumId > 0 && forumId == -1)
                {
                    return false;
                }
                else if (tmpForumId == forumId && forumId > 0)
                {
                    return true;
                }
                else if (tmpForumId <= 0)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public static bool CheckGroupURL(int portalId, int moduleId, string vanityName, int forumGroupId)
        {
            try
            {
                int tmpForumGroupId = -1;
                using var ctx = DotNetNuke.Data.DataContext.Instance();
                tmpForumGroupId = ctx.ExecuteScalar<int>(CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}communityforums_URL_CheckGroupVanity", portalId, vanityName);
                if (tmpForumGroupId > 0 && forumGroupId == -1)
                {
                    return false;
                }
                else if (tmpForumGroupId == forumGroupId && forumGroupId > 0)
                {
                    return true;
                }
                else if (tmpForumGroupId <= 0)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
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

        internal static string BuildForumUrl(INavigationManager navigationManager, DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings, ModuleSettings mainSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo)
        {
            // Build the forum Url
            return mainSettings.UseShortUrls ? navigationManager.NavigateURL(forumInfo.GetTabId(), portalSettings, string.Empty, new[] { $"{ParamKeys.ForumId}={forumInfo.ForumID}" })
                : navigationManager.NavigateURL(forumInfo.GetTabId(), portalSettings, string.Empty, new[] { $"{ParamKeys.ForumId}={forumInfo.ForumID}", $"{ParamKeys.ViewType}={Views.Topics}" });
        }

        internal static string BuildModeratorUrl(INavigationManager navigationManager, DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings, ModuleSettings mainSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo)
        {
            return navigationManager.NavigateURL(forumInfo.GetTabId(), portalSettings, string.Empty, new[] { $"{ParamKeys.ViewType}={Views.ModerateTopics}", $"{ParamKeys.ForumId}={forumInfo.ForumID}" });
        }

        private static int TopicIdByUrl(int portalId, int moduleId, string uRL)
        {
            if (uRL.EndsWith("/"))
            {
                uRL = uRL.Substring(0, uRL.Length - 1);
            }

            using var ctx = DotNetNuke.Data.DataContext.Instance();
            return ctx.ExecuteScalar<int>(CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}communityforums_TopicIdByURL", portalId, moduleId, uRL);
        }
    }
}
