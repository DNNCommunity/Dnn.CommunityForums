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

namespace DotNetNuke.Modules.ActiveForums.Services.Sitemap
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.ActiveForums.Controllers;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Services.Search;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Sitemap;

    public class ForumsSitemapProvider : SitemapProvider
    {
        private const int FirstPage = 1;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ForumsSitemapProvider));

        private readonly ForumController forumController;

        public ForumsSitemapProvider()
        {
            this.forumController = new ForumController();
        }

        public override List<SitemapUrl> GetUrls(int portalId, PortalSettings ps, string version)
        {
            var sitemapUrlsByUrl = new Dictionary<string, SitemapUrl>(StringComparer.OrdinalIgnoreCase);
            var portalAlias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).FirstOrDefault(a => a.IsPrimary)?.HTTPAlias;
            if (string.IsNullOrWhiteSpace(portalAlias))
            {
                Logger.Warn($"Unable to generate sitemap URLs for portal {portalId} because no primary portal alias could be resolved.");
                return sitemapUrlsByUrl.Values.ToList();
            }

            foreach (ModuleInfo module in ModuleController.Instance.GetModules(portalId))
            {
                if (module.IsDeleted || module.DesktopModule == null || !module.DesktopModule.ModuleName.Trim().Equals(Globals.ModuleName, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                try
                {
                    this.AppendModuleUrls(module, portalAlias, sitemapUrlsByUrl);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unable to generate sitemap URLs for forum module {module.ModuleID}", ex);
                    Exceptions.LogException(ex);
                }
            }

            return sitemapUrlsByUrl.Values.ToList();
        }

        private void AppendModuleUrls(ModuleInfo module, string portalAlias, IDictionary<string, SitemapUrl> sitemapUrlsByUrl)
        {
            var tab = TabController.Instance.GetTab(module.TabID, module.PortalID);
            bool isSecureTab = tab != null && tab.IsSecure;
            var forumsByForumId = new Dictionary<int, ForumInfo>();
            var controlUtils = new ControlUtils();

            // Search_DotNetNuke expects a SQL datetime-compatible minimum date.
            var results = DataContext.Instance().ExecuteQuery<SearchSitemapResult>(
                CommandType.StoredProcedure,
                "{databaseOwner}{objectQualifier}activeforums_Search_GetSearchItemsFromBegDate",
                module.ModuleID,
                SqlDateTime.MinValue.Value);
            foreach (SearchSitemapResult result in results)
            {
                if (!result.IsApproved || result.IsDeleted)
                {
                    continue;
                }

                int forumId = result.ForumId;
                ForumInfo forum;
                if (!forumsByForumId.TryGetValue(forumId, out forum))
                {
                    forum = this.forumController.GetById(forumId, module.ModuleID);
                    forumsByForumId[forumId] = forum;
                }

                if (forum == null || forum.Security == null || !ForumInfo.IsPublicForum(forum.Security.ReadRoleIds))
                {
                    continue;
                }

                string link = controlUtils.BuildUrl(
                    portalId: module.PortalID,
                    tabId: module.TabID,
                    moduleId: module.ModuleID,
                    groupPrefix: result.ForumGroupUrlPrefix,
                    forumPrefix: result.ForumUrlPrefix,
                    forumGroupId: result.ForumGroupId,
                    forumID: forumId,
                    topicId: result.TopicId,
                    topicURL: result.TopicUrl,
                    tagId: Null.NullInteger,
                    categoryId: Null.NullInteger,
                    otherPrefix: string.Empty,
                    pageId: FirstPage,
                    contentId: result.ContentId,
                    socialGroupId: forum.SocialGroupId);

                if (string.IsNullOrWhiteSpace(link))
                {
                    continue;
                }

                // BuildUrl may return a relative segment without a leading slash; normalize for ResolveUrl.
                if (!link.StartsWith("/", StringComparison.Ordinal) && !link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    link = "/" + link;
                }

                link = Utilities.ResolveUrl(link, portalAlias, isSecureTab);
                if (string.IsNullOrWhiteSpace(link))
                {
                    continue;
                }

                DateTime lastModified = result.DateUpdated;

                SitemapUrl sitemapUrl;
                if (!sitemapUrlsByUrl.TryGetValue(link, out sitemapUrl))
                {
                    sitemapUrlsByUrl[link] = new SitemapUrl
                    {
                        Url = link,
                        LastModified = lastModified,
                        ChangeFrequency = SitemapChangeFrequency.Daily,
                        Priority = 0.5F,
                    };
                }
                else if (lastModified > sitemapUrl.LastModified)
                {
                    sitemapUrl.LastModified = lastModified;
                }
            }
        }
    }
}
