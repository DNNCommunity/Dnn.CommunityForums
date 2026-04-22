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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Policy;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs; 

    using static DotNetNuke.Modules.ActiveForums.Entities.TopicInfo;

    public class ForumsReWriter : IHttpModule
    {
        private readonly IPortalAliasService portalAliasService;

        internal enum ViewUrlType
        {
            Default = 0,
            Views = 1,
            Category = 2,
            Tag = 3,
            Likes = 4,
        }

        internal ModuleSettings MainSettings { get; private set; } = null;

        internal int ModuleId { get; private set; } = DotNetNuke.Common.Utilities.Null.NullInteger;

        internal int PortalId { get; private set; } = DotNetNuke.Common.Utilities.Null.NullInteger;

        internal int ForumId { get; private set; } = DotNetNuke.Common.Utilities.Null.NullInteger;

        internal int ForumGroupId { get; private set; } = DotNetNuke.Common.Utilities.Null.NullInteger;

        internal int TabId { get; private set; } = DotNetNuke.Common.Utilities.Null.NullInteger;

        internal int TopicId { get; private set; } = DotNetNuke.Common.Utilities.Null.NullInteger;

        internal int ContentId { get; private set; } = DotNetNuke.Common.Utilities.Null.NullInteger;

        internal int OtherId { get; private set; } = DotNetNuke.Common.Utilities.Null.NullInteger;

        internal int? CategoryId { get; private set; } = DotNetNuke.Common.Utilities.Null.NullInteger;

        internal int? TagId { get; private set; } = DotNetNuke.Common.Utilities.Null.NullInteger;

        internal ViewUrlType ViewUrlTypeValue {get; private set; } = ViewUrlType.Default;

        internal int Page { get; private set; } = 0;

        internal int Timespan { get; private set; } = 0;

        internal string GroupSegment { get; private set; } = null;

        internal string ForumSegment { get; private set; } = null;

        internal string TopicSegment { get; private set; } = null;

        internal string GridType { get; private set; } = null;

        public ForumsReWriter()
            : this(new DotNetNuke.Entities.Portals.PortalAliasController())
        {
        }

        public ForumsReWriter(IPortalAliasService portalAliasService)
        {
            this.portalAliasService = portalAliasService;
        }

        public void Dispose()
        {
        }

        public void Init(System.Web.HttpApplication context)
        {
            context.BeginRequest += this.OnBeginRequest;
        }

        public void OnBeginRequest(object s, EventArgs e)
        {
            this.ProcessBeginRequest((System.Web.HttpApplication)s);
        }

        internal void ProcessBeginRequest(System.Web.HttpApplication app)
        {
#if DEBUG
            var st = new Stopwatch();
            st.Start();
#endif
            this.ModuleId = DotNetNuke.Common.Utilities.Null.NullInteger;
            this.PortalId = DotNetNuke.Common.Utilities.Null.NullInteger;
            this.ForumId = DotNetNuke.Common.Utilities.Null.NullInteger;
            this.ForumGroupId = DotNetNuke.Common.Utilities.Null.NullInteger;
            this.TabId = DotNetNuke.Common.Utilities.Null.NullInteger;
            this.TopicId = DotNetNuke.Common.Utilities.Null.NullInteger;
            this.ContentId = DotNetNuke.Common.Utilities.Null.NullInteger;
            this.OtherId = DotNetNuke.Common.Utilities.Null.NullInteger;
            this.CategoryId = DotNetNuke.Common.Utilities.Null.NullInteger;
            this.TagId = DotNetNuke.Common.Utilities.Null.NullInteger;
            this.Page = 0;
            this.Timespan = 0;
            this.MainSettings = null;
            this.GroupSegment = null;
            this.ForumSegment = null;
            this.TopicSegment = null;
            var otherGridTypes = new[] { GridTypes.Unanswered, GridTypes.NotRead, GridTypes.MyTopics, GridTypes.ActiveTopics, GridTypes.MySettings, GridTypes.MostLiked, GridTypes.MostReplies, GridTypes.MySubscriptions, GridTypes.Announcements, GridTypes.Unresolved, GridTypes.BadgeUsers, GridTypes.UserBadges, GridTypes.RecycleBin };

            var exclusions = new[]
            {
                "/api/", "/ctl/", "/portals/", "/desktopmodules/",
                ".gif", ".jpg", ".css", ".png", ".svg", ".webp", ".swf", ".cur", ".ico",
                ".axd", ".js", ".aspx", ".htm", ".html", ".ashx", ".asmx",
                ".txt", ".pdf", ".xml", ".csv", ".xls", ".xlsx", ".doc", ".docx", ".ppt", ".pptx", ".zip", ".zipx",
                ".eot", ".ttf", ".otf", ".woff", ".woff2",
            };
            if (exclusions.Any(ex => app.Request.Url.LocalPath.ToLowerInvariant().Contains(ex)))
            {
                return;
            }

            if (app.Request.RawUrl.ToLowerInvariant().Contains(Modes.DnnPrintMode) ||
                (app.Request.RawUrl.ToLowerInvariant().Contains(ParamKeys.ViewType.ToLowerInvariant()) &&
                    new[] { Views.Post, Views.ConfirmAction, Views.SendTo, Views.ModerateReport, Views.Search, Views.ModerateTopics }
                    .Any(x => app.Request.RawUrl.ToLowerInvariant().Contains(x.ToLowerInvariant()))))
            {
                return;
            }

            var uri = app.Request.Url;
            var scheme = uri.Scheme;
            var host = uri.Host;
            var port = uri.Port;
            var path = uri.PathAndQuery;
            var hostAndPath = $"{host}{path}";
            if (!uri.IsDefaultPort)
            {
                hostAndPath = $"{host}:{port}{path}";
            }

            string portalAliasesCacheKey = CacheKeys.PortalAliases;
            IDictionary<string, IPortalAliasInfo> portalAliases = DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheRetrieve(DotNetNuke.Common.Utilities.Null.NullInteger, portalAliasesCacheKey) as IDictionary<string, IPortalAliasInfo>;
            if (portalAliases == null)
            {
                portalAliases = this.portalAliasService.GetPortalAliases();
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(DotNetNuke.Common.Utilities.Null.NullInteger, portalAliasesCacheKey, portalAliases);
            }

            var portalAliasInfo = portalAliases.Values.FirstOrDefault(alias => alias.HttpAlias.StartsWith(hostAndPath));
            if (portalAliasInfo == null)
            {
                portalAliasInfo = this.portalAliasService.GetPortalAlias(host);
                if (portalAliasInfo == null && !uri.IsDefaultPort)
                {
                    portalAliasInfo = this.portalAliasService.GetPortalAlias($"{host}:{port}");
                }

                if (portalAliasInfo == null)
                {
                    return;
                }
            }

            this.PortalId = portalAliasInfo.PortalId;

            string tabPathsCacheKey = string.Format(CacheKeys.TabPaths, this.PortalId);
            List<(int ModuleId, int TabId, string Path)> tabpaths = DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheRetrieve(DotNetNuke.Common.Utilities.Null.NullInteger, portalAliasesCacheKey) as List<(int ModuleId, int TabId, string Path)>;
            if (tabpaths == null)
            {
                var forumModuleInstances = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(this.PortalId)
                .Cast<DotNetNuke.Entities.Modules.ModuleInfo>()
                .Where(module => module.ModuleDefinition.DefinitionName.Equals(Globals.ModuleFriendlyName, StringComparison.OrdinalIgnoreCase) ||
                module.ModuleDefinition.DefinitionName.Equals($"{Globals.ModuleFriendlyName} Viewer", StringComparison.OrdinalIgnoreCase));

                var tabs = new List<ModuleInfo>();
                foreach (var module in forumModuleInstances)
                {
                    tabs.AddRange(DotNetNuke.Entities.Modules.ModuleController.Instance.GetAllTabsModulesByModuleID(module.ModuleID).Cast<DotNetNuke.Entities.Modules.ModuleInfo>());
                }

                // base path for each tab
                tabpaths = new List<(int ModuleId, int TabId, string Path)>();
                tabpaths.AddRange(tabs.Select(module => (module.ModuleID, module.TabID, module.ParentTab.TabPath.Replace("//", "/"))));

                // add any alternate urls for each tab
                foreach (var tabpath in tabpaths.ToList())
                {
                    tabpaths.AddRange(DotNetNuke.Entities.Tabs.TabController.Instance.GetTabUrls(tabpath.TabId, this.PortalId).Select(url => (tabpath.ModuleId, tabpath.TabId, url.Url)));
                }

                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(DotNetNuke.Common.Utilities.Null.NullInteger, tabPathsCacheKey, tabpaths);
            }

            var pathRemaining = hostAndPath.Replace(portalAliasInfo.HttpAlias, string.Empty);
            var theTabPath = tabpaths.FirstOrDefault(tp => pathRemaining.StartsWith(tp.Path, StringComparison.OrdinalIgnoreCase));
            this.TabId = theTabPath.TabId;
            this.ModuleId = theTabPath.ModuleId;
            if (this.ModuleId > 0)
            {
                this.MainSettings = SettingsBase.GetModuleSettings(this.ModuleId);
            }

            if (this.MainSettings == null || !this.MainSettings.URLRewriteEnabled)
            {
                return;
            }

            var searchUrl = pathRemaining.Replace(theTabPath.Path, string.Empty);

            // if there is a query string, we don't need it for the search so remove it
            if (searchUrl.Contains("?"))
            {
                searchUrl = searchUrl.Substring(0, searchUrl.IndexOf("?") - 1);
            }

            // remove leading slash from searchUrl
            if (searchUrl.StartsWith("/"))
            {
                searchUrl = searchUrl.Substring(1);
            }

            var likePrefix = this.MainSettings.PrefixURLLikes ?? "likes";
            var categoryPrefix = this.MainSettings.PrefixURLCategory ?? "category";
            var tagPrefix = this.MainSettings.PrefixURLTag ?? "tag";
            var otherPrefix = this.MainSettings.PrefixURLOther ?? "other";
            var basePrefix = this.MainSettings.PrefixURLBase ?? string.Empty;

            bool canContinue = false;
            this.GetViewTypeFromUrl(searchUrl);
            if (this.GridType != null)
            {
                this.ViewUrlTypeValue = ViewUrlType.Views;
                canContinue = true;
            }
            else if (searchUrl.Contains($"/{likePrefix}/"))
            {
                searchUrl = this.HandleLikesPages(searchUrl);
                if (this.ContentId > DotNetNuke.Common.Utilities.Null.NullInteger)
                {
                    this.ViewUrlTypeValue = ViewUrlType.Likes;
                    canContinue = true;
                }
            }
            else
            {
                string urlSegmentName;
                if (this.ExtractPrefixedSegment(ref searchUrl, categoryPrefix, out urlSegmentName))
                {
                    this.CategoryId = new DotNetNuke.Modules.ActiveForums.Controllers.CategoryController().GetByName(this.ModuleId, urlSegmentName).CategoryId;
                    this.ViewUrlTypeValue = ViewUrlType.Category;
                    canContinue = true;
                }
                else if (this.ExtractPrefixedSegment(ref searchUrl, tagPrefix, out urlSegmentName))
                {
                    this.TagId = new DotNetNuke.Modules.ActiveForums.Controllers.TagController().GetByName(this.ModuleId, urlSegmentName).TagId;
                    this.ViewUrlTypeValue = ViewUrlType.Tag;
                    canContinue = true;
                }
                else
                {
                    this.HandleNonLikesPages(searchUrl);

                    var archivedURL = new DotNetNuke.Modules.ActiveForums.Controllers.ArchivedUrlController().FindByURL(this.PortalId, searchUrl);
                    if (archivedURL != null)
                    {
                        var topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ModuleId).GetById(archivedURL.TopicId);
                        if (topic != null)
                        {
                            Redirect(app.Response, topic.GetLink());
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.GroupSegment) && !string.IsNullOrEmpty(this.ForumSegment) && !string.IsNullOrEmpty(this.TopicSegment))
                        {
                            var forumGroup = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetByUrlPrefix(this.ModuleId, this.GroupSegment);
                            if (forumGroup != null)
                            {
                                var forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetByUrlPrefix(this.ModuleId, this.ForumSegment);
                                if (forum != null && forum.GroupPrefixURL.Equals(this.GroupSegment) && forum.PrefixURL.Equals(this.ForumSegment))
                                {
                                    this.ForumGroupId = forum.ForumGroupId;
                                    this.ForumId = forum.ForumID;
                                    var topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ModuleId).FindByURL(this.ForumId, this.TopicSegment);
                                    if (topic != null)
                                    {
                                        this.TopicId = topic.TopicId;
                                        this.ViewUrlTypeValue = ViewUrlType.Default;
                                        canContinue = true;
                                    }
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(this.GroupSegment) && !string.IsNullOrEmpty(this.ForumSegment))
                        {
                            var forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetByUrlPrefix(this.ModuleId, this.ForumSegment);
                            if (forum != null && forum.PrefixURL.Equals(this.ForumSegment))
                            {
                                this.ForumId = forum.ForumID;
                                this.ForumGroupId = forum.ForumGroupId;
                                this.ViewUrlTypeValue = ViewUrlType.Default;
                                canContinue = true;
                            }
                        }
                        else if (!string.IsNullOrEmpty(this.GroupSegment))
                        {
                            var forumGroup = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetByUrlPrefix(this.ModuleId, this.GroupSegment);
                            if (forumGroup != null && forumGroup.PrefixURL.Equals(this.GroupSegment))
                            {
                                this.ForumGroupId = forumGroup.ForumGroupId;
                                this.ViewUrlTypeValue = ViewUrlType.Default;
                                canContinue = true;
                            }
                        }
                    }
                }
            }

            if (!canContinue)
            {
                if (app.Request.RawUrl.Contains(ParamKeys.TopicId) || app.Request.RawUrl.Contains(ParamKeys.ForumId) || app.Request.RawUrl.Contains(ParamKeys.GroupId))
                {
                    this.HandleOldUrls(app.Request.RawUrl);
                    if (this.TopicId > 0)
                    {
                        var topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ModuleId).GetById(this.TopicId);
                        if (topic != null)
                        {
                            Redirect(app.Response, topic.GetLink());
                        }
                    }

                    Redirect(app.Response, new ControlUtils().BuildUrl(portalId: this.PortalId, tabId: this.TabId, moduleId: this.ModuleId, groupPrefix: string.Empty, forumPrefix: string.Empty, forumGroupId: this.ForumGroupId, forumID: this.ForumId, topicId: this.TopicId, topicURL: string.Empty, tagId: this.TagId.Value, categoryId: this.CategoryId.Value, otherPrefix: string.Empty, pageId: 1, contentId: this.ContentId, socialGroupId: -1));
                }

                string topicUrl = string.Empty;
                if (searchUrl.EndsWith("/"))
                {
                    searchUrl = searchUrl.Substring(0, searchUrl.Length - 1);
                }

                if (searchUrl.Contains("/"))
                {
                    topicUrl = searchUrl.Substring(searchUrl.LastIndexOf("/"));
                }

                topicUrl = topicUrl.Replace("/", string.Empty);
                if (!string.IsNullOrEmpty(topicUrl))
                {
                    var topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ModuleId).FindByURL(this.ForumId, topicUrl);
                    if (topic != null)
                    {
                        var link = topic.GetLink();
                        if (!string.IsNullOrEmpty(link))
                        {
                            Redirect(app.Response, link);
                        }
                    }
                }

                return;
            }

            if (canContinue)
            {
                var qs = string.IsNullOrEmpty(uri.Query) ? string.Empty : $"?{uri.Query}";
                var catQS = this.CategoryId.HasValue && this.CategoryId.Value > 0 ? $"&{ParamKeys.Category}=" + this.CategoryId.ToString() : string.Empty;
                var sPage = this.Page != 0 ? $"&{ParamKeys.PageId}={this.Page}" : string.Empty;
                var sTS = this.Timespan != 0 ? $"&{ParamKeys.TimeSpan}={this.Timespan}" : string.Empty;

                var sendTo = string.Empty;
                if (this.ViewUrlTypeValue.Equals(ViewUrlType.Views))
                {
                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, "~/default.aspx?tabid=" + this.TabId + $"&{ParamKeys.ViewType}={Views.Grid}&{ParamKeys.GridType}=" + this.GridType + sPage + sTS + qs);
                }
                else if (this.ViewUrlTypeValue.Equals(ViewUrlType.Category) && this.CategoryId > 0)
                {
                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, "~/default.aspx?tabid=" + this.TabId + $"&{ParamKeys.Category}=" + this.CategoryId + sPage + qs);
                }
                else if (this.ViewUrlTypeValue.Equals(ViewUrlType.Tag) && this.TagId > 0)
                {
                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, "~/default.aspx?tabid=" + this.TabId + $"&{ParamKeys.ViewType}={Views.Grid}&{ParamKeys.GridType}={Views.Tags}&{ParamKeys.Tags}=" + this.TagId + sPage + qs);
                }
                else if (this.ViewUrlTypeValue.Equals(ViewUrlType.Likes) && this.ContentId > 0)
                {
                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, "~/default.aspx?tabid=" + this.TabId + $"&{ParamKeys.ViewType}={Views.Grid}&{ParamKeys.GridType}={Views.Likes}&{ParamKeys.ContentId}=" + this.ContentId + sPage + qs);
                }
                else if ((this.TopicId > 0) || (this.ForumId > 0) || (this.ForumGroupId > 0))
                {
                    sendTo = ResolveUrl(
                        app.Context.Request.ApplicationPath, $"~/default.aspx?tabid={this.TabId}" +
                                (this.ForumGroupId > 0 ? $"&{ParamKeys.GroupId}={this.ForumGroupId}" : string.Empty) +
                                (this.ForumId > 0 ? $"&{ParamKeys.ForumId}={this.ForumId}" : string.Empty) +
                                (this.TopicId > 0 ? $"&{ParamKeys.TopicId}={this.TopicId}" : string.Empty) +
                                sPage + qs +
                                ((this.ForumGroupId > 0 || this.ForumId > 0) ? catQS : string.Empty));
                }
                else if (this.TabId > 0)
                {
                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, "~/default.aspx?tabid=" + this.TabId + sPage + qs);
                }

#if DEBUG
                st.Stop();
                Debug.WriteLine($"url rewriter processing time: {st.ElapsedMilliseconds} ms; from: {app.Request.RawUrl} to: {sendTo}");
#endif
                RewriteUrl(app.Context, sendTo);
            }
        }

        /// <summary>
        /// Extracts a known grid type from a URL segment, setting the matching <see cref="GridTypes"/> value or <c>null</c>.
        /// Also extracts optional timespan (<c>/ts/{timespan}</c>) and/or page segments.
        /// Supported formats: <c>/gridtype/</c>, <c>/gridtype/{page}</c>, <c>/gridtype/ts/{timespan}</c>, <c>/gridtype/ts/{timespan}/{page}</c>.
        /// </summary>
        /// <param name="url">The URL to inspect.</param>
        internal void GetViewTypeFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                this.GridType = null;
                return;
            }

            var allGridTypes = typeof(GridTypes)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue())
                .ToArray();

            var urlLower = url.ToLowerInvariant();

            this.GridType = allGridTypes.FirstOrDefault(
                g => urlLower.Contains($"/{g.ToLowerInvariant()}/") || urlLower.EndsWith($"/{g.ToLowerInvariant()}"));

            if (this.GridType == null)
            {
                return;
            }

            // /gridtype/ts/{timespan}/{page} or /gridtype/ts/{timespan}
            var tsPattern = $@"/{Regex.Escape(this.GridType)}/ts/(?<timespan>\d+)(?:/(?<page>\d+))?";
            var tsMatch = RegexUtils.GetCachedRegex(tsPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase).Match(url);
            if (tsMatch.Success)
            {
                if (tsMatch.Groups["timespan"].Success)
                {
                    this.Timespan = Utilities.SafeConvertInt(tsMatch.Groups["timespan"].Value, 0);
                }

                if (tsMatch.Groups["page"].Success)
                {
                    this.Page = Utilities.SafeConvertInt(tsMatch.Groups["page"].Value, 0);
                }

                return;
            }

            // /gridtype/{page} — lone number with no ts prefix
            var pagePattern = $@"/{Regex.Escape(this.GridType)}/(?<page>\d+)(?:/|$)";
            var pageMatch = RegexUtils.GetCachedRegex(pagePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase).Match(url);
            if (pageMatch.Success && pageMatch.Groups["page"].Success)
            {
                this.Page = Utilities.SafeConvertInt(pageMatch.Groups["page"].Value, 0);
            }
        }

        private static void Redirect(HttpResponse response, string sUrl)
        {
            response.Clear();
            response.Status = "301 Moved Permanently";
            response.AddHeader("Location", sUrl);
            response.End();
        }


        internal bool ExtractPrefixedSegment(ref string url, string prefix, out string name)
        {
            name = string.Empty;
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(url))
            {
                return false;
            }

            // pattern matches "/prefix/name" with optional trailing slash or end-of-string
            var pattern = $@"{Regex.Escape(prefix)}/(?<name>[^/]+)(?:/|$)";
            var rx = RegexUtils.GetCachedRegex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var m = rx.Match(url);
            if (!m.Success)
            {
                return false;
            }

            name = m.Groups["name"].Value;

            // remove the matched segment and normalize duplicate slashes
            url = rx.Replace(url, "/");
            url = Regex.Replace(url, "/{2,}", "/");

            return true;
        }

        internal void HandleNonLikesPages(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            var urlSegmentMatches = new Regex(@"(?<Page>[\d]+)/?\Z|(?<segment>[^\/]+|[^\/]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline).Matches(url);
            int segmentIndex = 0;
            foreach (Match match in urlSegmentMatches)
            {
                if (match.Groups["Page"].Success)
                {
                    var pageGroup = match.Groups["Page"];
                    if (pageGroup != null && !string.IsNullOrEmpty(pageGroup.Value))
                    {
                        this.Page = Utilities.SafeConvertInt(pageGroup.Value, DotNetNuke.Common.Utilities.Null.NullInteger);
                    }
                }

                if (match.Groups["segment"].Success)
                {
                    var value = match.Groups["segment"].Value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        switch (segmentIndex)
                        {
                            case 0:
                                this.GroupSegment = value;
                                break;
                            case 1:
                                this.ForumSegment = value;
                                break;
                            case 2:
                                this.TopicSegment = value;
                                break;
                        }

                        segmentIndex++;
                    }
                }
            }
        }

        internal string HandleLikesPages(string searchURL)
        {
            var newSearchURL = searchURL; /* if this is a URL for viewing "likes", remove page number but not contentId */
            if (newSearchURL.StartsWith("/"))
            {
                newSearchURL = newSearchURL.Substring(1);
            }

            /* but do remove (optional) page number; it will be restored later */
            var likeMatches = RegexUtils.GetCachedRegex(@"(?<likewithoutpage>.*/?likes/(?<contentId>[\d]+))/(?<page>[\d]+)|(?<likewithoutpage>.*/?likes/(?<contentId>[\d]+))", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(newSearchURL);
            if (likeMatches.Success)
            {
                if (likeMatches.Groups["page"].Success)
                {
                    var pageGroup = likeMatches.Groups["page"];
                    if (pageGroup != null && !string.IsNullOrEmpty(pageGroup.Value))
                    {
                        this.Page = Utilities.SafeConvertInt(pageGroup.Value, DotNetNuke.Common.Utilities.Null.NullInteger);
                    }
                }

                if (likeMatches.Groups["contentId"].Success)
                {
                    var contentIdGroup = likeMatches.Groups["contentId"];
                    if (contentIdGroup != null && !string.IsNullOrEmpty(contentIdGroup.Value))
                    {
                        this.ContentId = Utilities.SafeConvertInt(contentIdGroup.Value, DotNetNuke.Common.Utilities.Null.NullInteger);
                    }
                }

                if (likeMatches.Groups["likewithoutpage"].Success)
                {
                    var likeWithoutPage = likeMatches.Groups["likewithoutpage"];
                    if (likeWithoutPage != null && !string.IsNullOrEmpty(likeWithoutPage.Value))
                    {
                        newSearchURL = likeWithoutPage.Value;
                    }
                }
            }

            return newSearchURL;
        }

        internal static string ResolveUrl(string appPath, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url ?? string.Empty;
            }

            // String does not contain a ~, so just return Url
            if (url[0] != '~')
            {
                return url;
            }

            // There is just the ~ in the Url, return the appPath
            if (url.Length == 1)
            {
                return appPath;
            }

            var rest = (url[1] == '/' || url[1] == '\\')
                ? url.Substring(2) // ~/... or ~\...
                : url.Substring(1); // ~something

            return appPath.Length > 1
                ? appPath + "/" + rest
                : "/" + rest;
        }

        internal static void RewriteUrl(HttpContext context, string sendToUrl)
        {
            // Handle legacy 404 rewrite pattern before querystring splitting
            if (sendToUrl.Contains("/404.aspx?404;"))
            {
                sendToUrl = sendToUrl.Substring(sendToUrl.IndexOf("/404.aspx?404;") + 14);
            }

            // first strip the querystring, if any
            var queryString = string.Empty;
            var sendToUrlLessQString = sendToUrl;

            if (sendToUrl.IndexOf("?") > 0)
            {
                sendToUrlLessQString = sendToUrl.Substring(0, sendToUrl.IndexOf("?"));
                queryString = sendToUrl.Substring(sendToUrl.IndexOf("?") + 1);
            }

            context.RewritePath(sendToUrlLessQString, string.Empty, queryString);
        }

        internal void HandleOldUrls(string rawUrl)
        {
            string currURL = rawUrl;
            if (currURL.Contains("?"))
            {
                currURL = currURL.Substring(currURL.IndexOf("?") + 1);
            }

            string splitter = "/";
            if (currURL.Contains("&"))
            {
                splitter = "&";
            }

            string[] parts = currURL.Split(Convert.ToChar(splitter));
            for (int i = 0; i < parts.Length; i++)
            {
                if (!string.IsNullOrEmpty(parts[i]))
                {
                    if (parts[i].ToLowerInvariant().Contains("="))
                    {
                        string[] pair = parts[i].Split('=');
                        if (pair[0].ToLowerInvariant() == ParamKeys.ForumId.ToLowerInvariant())
                        {
                            this.ForumId = int.Parse(pair[1]);
                        }
                        else if (pair[0].ToLowerInvariant() == ParamKeys.TopicId.ToLowerInvariant())
                        {
                            this.TopicId = int.Parse(pair[1]);
                        }
                        else if (pair[0].ToLowerInvariant() == ParamKeys.PageId.ToLowerInvariant())
                        {
                            this.Page = int.Parse(pair[1]);
                        }
                        else if (pair[0].ToLowerInvariant() == ParamKeys.PageJumpId.ToLowerInvariant())
                        {
                            this.Page = int.Parse(pair[1]);
                        }
                        else if (pair[0].ToLowerInvariant() == ParamKeys.TabId.ToLowerInvariant())
                        {
                            this.TabId = int.Parse(pair[1]);
                        }
                        else if (pair[0].ToLowerInvariant() == ParamKeys.ContentJumpId.ToLowerInvariant())
                        {
                            this.ContentId = int.Parse(pair[1]);
                        }
                    }
                    else
                    {
                        if (parts[i].ToLowerInvariant() == ParamKeys.ForumId.ToLowerInvariant())
                        {
                            this.ForumId = int.Parse(parts[i + 1]);
                        }
                        else if (parts[i].ToLowerInvariant() == ParamKeys.TopicId.ToLowerInvariant())
                        {
                            this.TopicId = int.Parse(parts[i + 1]);
                        }
                        else if (parts[i].ToLowerInvariant() == ParamKeys.PageId.ToLowerInvariant())
                        {
                            this.Page = int.Parse(parts[i + 1]);
                        }
                        else if (parts[i].ToLowerInvariant() == ParamKeys.PageJumpId.ToLowerInvariant())
                        {
                            this.Page = int.Parse(parts[i + 1]);
                        }
                        else if (parts[i].ToLowerInvariant() == ParamKeys.TabId.ToLowerInvariant())
                        {
                            this.TabId = int.Parse(parts[i + 1]);
                        }
                        else if (parts[i].ToLowerInvariant() == ParamKeys.ContentJumpId.ToLowerInvariant())
                        {
                            this.ContentId = int.Parse(parts[i + 1]);
                        }
                    }
                }
            }
        }

    }
}
