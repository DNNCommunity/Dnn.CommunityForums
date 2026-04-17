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
    using System.Drawing;
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
        private const int TopicReplyId = -1;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ForumsSitemapProvider));

        private readonly ForumController forumController;

        public ForumsSitemapProvider()
        {
            this.forumController = new ForumController();
        }

        public override List<SitemapUrl> GetUrls(int portalId, PortalSettings ps, string version)
        {
            var sitemapUrlsByUrl = new Dictionary<string, SitemapUrl>(StringComparer.OrdinalIgnoreCase);
            var portalAlias = new DotNetNuke.Modules.ActiveForums.Helpers.PortalSettingsHelper().GetPortalSettings(portalId).DefaultPortalAlias;
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
            var controlUtils = new ControlUtils();
            var sitemapMetricsByUrl = new Dictionary<string, SitemapUrlMetrics>(StringComparer.OrdinalIgnoreCase);

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

                var forum = this.forumController.GetById(result.ForumId, module.ModuleID);
                if (forum == null || forum.Security == null || !forum.IsPublicForum)
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
                    forumID:  result.ForumId,
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

                link = Utilities.ResolveUrl(link, portalAlias, isSecureTab);
                if (string.IsNullOrWhiteSpace(link))
                {
                    continue;
                }

                if (!sitemapMetricsByUrl.TryGetValue(link, out SitemapUrlMetrics metrics))
                {
                    metrics = new SitemapUrlMetrics();
                    sitemapMetricsByUrl[link] = metrics;
                }

                metrics.Update(result);
            }

            DateTime nowUtc = DateTime.UtcNow;
            foreach (var item in sitemapMetricsByUrl)
            {
                var metrics = item.Value;
                SitemapUrl sitemapUrl;
                if (!sitemapUrlsByUrl.TryGetValue(item.Key, out sitemapUrl))
                {
                    sitemapUrlsByUrl[item.Key] = new SitemapUrl
                    {
                        Url = item.Key,
                        LastModified = metrics.LastModifiedUtc,
                        ChangeFrequency = DetermineChangeFrequency(metrics, nowUtc),
                        Priority = DeterminePriority(metrics, nowUtc),
                    };
                    continue;
                }

                if (metrics.LastModifiedUtc > sitemapUrl.LastModified)
                {
                    sitemapUrl.LastModified = metrics.LastModifiedUtc;
                }
            }
        }

        internal static SitemapChangeFrequency DetermineChangeFrequency(SitemapUrlMetrics metrics, DateTime nowUtc)
        {
            DateTime latestReplyDateUtc = metrics.LatestReplyDateUtc ?? metrics.LastModifiedUtc;
            DateTime firstReplyDateUtc = metrics.FirstReplyDateUtc ?? metrics.TopicCreatedUtc;

            double topicAgeInDays = (nowUtc - metrics.TopicCreatedUtc).TotalDays;
            double replyWindowInDays = (latestReplyDateUtc - firstReplyDateUtc).TotalDays;

            if (topicAgeInDays <= 7 && metrics.ReplyCount >= 5 && replyWindowInDays <= 3)
            {
                return SitemapChangeFrequency.Hourly;
            }

            if (topicAgeInDays <= 30 && (metrics.ReplyCount >= 2 || replyWindowInDays <= 7))
            {
                return SitemapChangeFrequency.Daily;
            }

            if (topicAgeInDays <= 120 || replyWindowInDays <= 30)
            {
                return SitemapChangeFrequency.Weekly;
            }

            if (topicAgeInDays <= 365)
            {
                return SitemapChangeFrequency.Monthly;
            }

            return SitemapChangeFrequency.Yearly;
        }

        internal static float DeterminePriority(SitemapUrlMetrics metrics, DateTime nowUtc)
        {
            float priority = 0.5F;

            priority += Math.Min(metrics.ReplyCount, 20) * 0.015F;

            if (metrics.FirstReplyDateUtc.HasValue && metrics.LatestReplyDateUtc.HasValue)
            {
                double discussionWindowInDays = (metrics.LatestReplyDateUtc.Value - metrics.FirstReplyDateUtc.Value).TotalDays;
                if (discussionWindowInDays >= 30)
                {
                    priority += 0.10F;
                }
                else if (discussionWindowInDays >= 7)
                {
                    priority += 0.05F;
                }
            }

            double updatedRecentlyInDays = (nowUtc - metrics.LastModifiedUtc).TotalDays;
            if (updatedRecentlyInDays <= 7)
            {
                priority += 0.10F;
            }
            else if (updatedRecentlyInDays <= 30)
            {
                priority += 0.05F;
            }

            return Math.Max(0.1F, Math.Min(1F, priority));
        }

        internal class SitemapUrlMetrics
        {
            public DateTime TopicCreatedUtc { get; private set; } = DateTime.MaxValue;

            public DateTime LastModifiedUtc { get; private set; } = DateTime.MinValue;

            public DateTime? FirstReplyDateUtc { get; private set; }

            public DateTime? LatestReplyDateUtc { get; private set; }

            public int ReplyCount { get; private set; }

            public void Update(SearchSitemapResult result)
            {
                if (result.DateCreated < this.TopicCreatedUtc)
                {
                    this.TopicCreatedUtc = result.DateCreated;
                }

                if (result.DateUpdated > this.LastModifiedUtc)
                {
                    this.LastModifiedUtc = result.DateUpdated;
                }

                if (result.ReplyId == TopicReplyId)
                {
                    return;
                }

                this.ReplyCount++;

                if (!this.FirstReplyDateUtc.HasValue || result.DateCreated < this.FirstReplyDateUtc.Value)
                {
                    this.FirstReplyDateUtc = result.DateCreated;
                }

                if (!this.LatestReplyDateUtc.HasValue || result.DateCreated > this.LatestReplyDateUtc.Value)
                {
                    this.LatestReplyDateUtc = result.DateCreated;
                }
            }
        }
    }
}
