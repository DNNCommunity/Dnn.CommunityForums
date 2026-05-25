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
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    public class ForumsReWriter : IHttpModule
    {
        private static readonly string[] AllGridTypes = typeof(GridTypes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && f.FieldType == typeof(string))
        .Select(f => (string)f.GetRawConstantValue())
        .ToArray();

        private static readonly string[] excludedViews = { Views.Post.ToLowerInvariant(), Views.ConfirmAction.ToLowerInvariant(), Views.SendTo.ToLowerInvariant(), Views.ModerateReport.ToLowerInvariant(), Views.Search.ToLowerInvariant(), Views.ModerateTopics.ToLowerInvariant() };

        private static readonly string[] exclusions =
            {
                "/api/", "/ctl/", "/portals/", "/desktopmodules/",
                ".gif", ".jpg", ".css", ".png", ".svg", ".webp", ".swf", ".cur", ".ico",
                ".axd", ".js", ".aspx", ".htm", ".html", ".ashx", ".asmx",
                ".txt", ".pdf", ".xml", ".csv", ".xls", ".xlsx", ".doc", ".docx", ".ppt", ".pptx", ".zip", ".zipx",
                ".eot", ".ttf", ".otf", ".woff", ".woff2",
            };

        private readonly IPortalAliasService portalAliasService;
        private readonly DotNetNuke.Modules.ActiveForums.Controllers.TagController tagController;
        private readonly DotNetNuke.Modules.ActiveForums.Controllers.CategoryController categoryController;
        private readonly DotNetNuke.Modules.ActiveForums.Controllers.ForumController forumController;
        private readonly DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController forumGroupController;

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

        internal string OtherPrefix { get; private set; } = string.Empty;

        internal ViewUrlType ViewUrlTypeValue {get; private set; } = ViewUrlType.Default;

        internal int Page { get; private set; } = 0;

        internal int Timespan { get; private set; } = 0;

        internal string GroupSegment { get; private set; } = null;

        internal string ForumSegment { get; private set; } = null;

        internal string TopicSegment { get; private set; } = null;

        internal string GridType { get; private set; } = null;

        internal ForumsReWriter()
            : this(
                  new DotNetNuke.Entities.Portals.PortalAliasController(),
                  new DotNetNuke.Modules.ActiveForums.Controllers.TagController(),
                  new DotNetNuke.Modules.ActiveForums.Controllers.CategoryController(),
                  new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController(),
                  new DotNetNuke.Modules.ActiveForums.Controllers.ForumController())
        {
        }

        internal ForumsReWriter(
            IPortalAliasService portalAliasService,
            DotNetNuke.Modules.ActiveForums.Controllers.TagController tagController,
            DotNetNuke.Modules.ActiveForums.Controllers.CategoryController categoryController,
            DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController forumGroupController,
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController forumController)
        {
            this.portalAliasService = portalAliasService;
            this.tagController = tagController;
            this.categoryController = categoryController;
            this.forumGroupController = forumGroupController;
            this.forumController = forumController;
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
            var st = new System.Diagnostics.Stopwatch();
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
            this.OtherPrefix = string.Empty;
            this.Page = 0;
            this.Timespan = 0;
            this.MainSettings = null;
            this.GroupSegment = null;
            this.ForumSegment = null;
            this.TopicSegment = null;

            var rawUrlLower = app.Request.RawUrl.ToLowerInvariant();
            if (rawUrlLower.Contains(Modes.DnnPrintMode.ToLowerInvariant()) ||
                (rawUrlLower.Contains(ParamKeys.ViewType.ToLowerInvariant()) &&
                    excludedViews.Any(x => rawUrlLower.Contains(x)))) /* x.ToLowerInvariant() not needed since it's done in declaration of excludedViews */
            {
                return;
            }

            var localPathLower = app.Request.Url.LocalPath.ToLowerInvariant();
            if (exclusions.Any(ex => localPathLower.Contains(ex)))
            {
                return;
            }

            var urlRewriteCacheKey = string.Format(CacheKeys.UrlRewrites, app.Request.Url.ToString().ToLowerInvariant());
            var sendTo = DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheRetrieve(DotNetNuke.Common.Utilities.Null.NullInteger, urlRewriteCacheKey) as string;
            if (!string.IsNullOrEmpty(sendTo))
            {
#if DEBUG
                st.Stop();
                System.Diagnostics.Debug.WriteLine($"url rewriter processing time: {st.ElapsedMilliseconds} ms; from: {rawUrlLower} to: {sendTo}");
#endif
                RewriteUrl(app.Context, sendTo);
                return;
            }

            var uri = app.Request.Url;
            var host = uri.Host;
            var port = uri.Port;
            var path = uri.PathAndQuery;
            var hostAndPath = $"{host}{path}";
            if (!uri.IsDefaultPort)
            {
                hostAndPath = $"{host}:{port}{path}";
            }

            var portalAliasesCacheKey = CacheKeys.PortalAliases;
            var portalAliases = DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheRetrieve(DotNetNuke.Common.Utilities.Null.NullInteger, portalAliasesCacheKey) as IDictionary<string, IPortalAliasInfo>;
            if (portalAliases == null)
            {
                portalAliases = this.portalAliasService.GetPortalAliases();
                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheStore(DotNetNuke.Common.Utilities.Null.NullInteger, portalAliasesCacheKey, portalAliases);
            }

            var portalAliasInfo = portalAliases.Values
                .Where(alias =>
                    !string.IsNullOrEmpty(alias.HttpAlias) &&
                    hostAndPath.StartsWith(alias.HttpAlias, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(alias => alias.HttpAlias.Length)
                .FirstOrDefault();
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

            var tabPathsCacheKey = string.Format(CacheKeys.TabPaths, this.PortalId);
            var tabpaths = DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheRetrieve(DotNetNuke.Common.Utilities.Null.NullInteger, tabPathsCacheKey) as List<(int ModuleId, int TabId, string Path)>;
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

                tabpaths = tabpaths
                .Where(tp => !string.IsNullOrEmpty(tp.Path))
                .OrderByDescending(tp => tp.Path.Length)
                .ToList();

                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheStore(DotNetNuke.Common.Utilities.Null.NullInteger, tabPathsCacheKey, tabpaths);
            }

            if (tabpaths.Count < 1)
            {
                return;
            }

            var pathRemaining = hostAndPath.StartsWith(portalAliasInfo.HttpAlias, StringComparison.OrdinalIgnoreCase)
            ? hostAndPath.Substring(portalAliasInfo.HttpAlias.Length)
            : hostAndPath;

            var theTabPath = tabpaths
                .FirstOrDefault(tp =>
                    !string.IsNullOrEmpty(tp.Path) &&
                    pathRemaining.StartsWith(tp.Path, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(theTabPath.Path) || theTabPath.TabId <= 0 || theTabPath.ModuleId <= 0)
            {
                return;
            }

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

            var searchUrl = pathRemaining.StartsWith(theTabPath.Path, StringComparison.OrdinalIgnoreCase)
                ? pathRemaining.Substring(theTabPath.Path.Length)
                : pathRemaining;

            // if there is a query string, we don't need it for the search so remove it
            if (searchUrl.Contains("?"))
            {
                searchUrl = searchUrl.Substring(0, searchUrl.IndexOf("?"));
            }

            // remove leading slash from searchUrl
            if (searchUrl.StartsWith("/"))
            {
                searchUrl = searchUrl.Substring(1);
            }

            bool canContinue = false;
            this.GetViewTypeFromUrl(searchUrl);
            if (this.GridType != null)
            {
                if (this.CategoryId > 0)
                {
                    this.ViewUrlTypeValue = ViewUrlType.Category;
                }
                else if (this.TagId > 0 || this.GridType.Equals(GridTypes.Tags, StringComparison.OrdinalIgnoreCase))
                {
                    this.ViewUrlTypeValue = ViewUrlType.Tag;
                }
                else
                {
                    this.ViewUrlTypeValue = ViewUrlType.Views;
                }

                canContinue = true;
            }
            else
            {
                var likesPrefix = this.MainSettings.PrefixURLLikes ?? "likes";

                if (searchUrl.StartsWith(likesPrefix + "/", StringComparison.OrdinalIgnoreCase) ||
                    searchUrl.IndexOf("/" + likesPrefix + "/", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    searchUrl = this.HandleLikesPages(searchUrl, likesPrefix);
                    if (this.ContentId > DotNetNuke.Common.Utilities.Null.NullInteger)
                    {
                        this.ViewUrlTypeValue = ViewUrlType.Likes;
                        canContinue = true;
                    }
                }
                else
                {
                    if (this.CategoryId > 0)
                    {
                        this.ViewUrlTypeValue = ViewUrlType.Category;
                        canContinue = true;
                    }
                    else if (this.TagId > 0)
                    {
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
                                Redirect(app, topic.GetLink());
                                return;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(this.GroupSegment) && !string.IsNullOrEmpty(this.ForumSegment) && !string.IsNullOrEmpty(this.TopicSegment))
                            {
                                var forumGroup = this.forumGroupController.GetByUrlPrefix(this.ModuleId, this.GroupSegment);
                                if (forumGroup != null)
                                {
                                    this.ForumGroupId = forumGroup.ForumGroupId;
                                    var forum = this.forumController.GetByUrlPrefix(this.ModuleId, this.ForumSegment);
                                    if (forum != null && forum.GroupPrefixURL.Equals(this.GroupSegment) && forum.PrefixURL.Equals(this.ForumSegment))
                                    {
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
                                var forumGroup = this.forumGroupController.GetByUrlPrefix(this.ModuleId, this.GroupSegment);
                                if (forumGroup != null)
                                {
                                    this.ForumGroupId = forumGroup.ForumGroupId;
                                    var forum = this.forumController.GetByUrlPrefix(this.ModuleId, this.ForumSegment);
                                    if (forum != null && forum.PrefixURL.Equals(this.ForumSegment))
                                    {
                                        this.ForumId = forum.ForumID;
                                        this.ViewUrlTypeValue = ViewUrlType.Default;
                                        canContinue = true;
                                    }
                                    }
                            }
                            else if (!string.IsNullOrEmpty(this.GroupSegment))
                            {
                                var forumGroup = this.forumGroupController.GetByUrlPrefix(this.ModuleId, this.GroupSegment);
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
            }

            if (!canContinue)
            {
                if (rawUrlLower.Contains(ParamKeys.TopicId) || rawUrlLower.Contains(ParamKeys.ForumId) || rawUrlLower.Contains(ParamKeys.GroupId))
                {
                    this.HandleOldUrls(rawUrlLower);
                    if (this.TopicId > 0)
                    {
                        var topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ModuleId).GetById(this.TopicId);
                        if (topic != null)
                        {
                            Redirect(app, topic.GetLink());
                            return;
                        }
                    }

                    Redirect(app, new ControlUtils().BuildUrl(portalId: this.PortalId, tabId: this.TabId, moduleId: this.ModuleId, groupPrefix: string.Empty, forumPrefix: string.Empty, forumGroupId: this.ForumGroupId, forumID: this.ForumId, topicId: this.TopicId, topicURL: string.Empty, tagId: this.TagId.Value, categoryId: this.CategoryId.Value, otherPrefix: this.OtherPrefix, pageId: 1, contentId: this.ContentId, socialGroupId: -1));
                    return;
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
                            Redirect(app, link);
                            return;
                        }
                    }
                }

                return;
            }

            if (canContinue)
            {
                var qs = string.IsNullOrEmpty(uri.Query) ? string.Empty : "&" + uri.Query.TrimStart('?');
                var catQS = this.CategoryId.HasValue && this.CategoryId.Value > 0 ? $"&{ParamKeys.Category}=" + this.CategoryId.ToString() : string.Empty;
                var sPage = this.Page != 0 ? $"&{ParamKeys.PageId}={this.Page}" : string.Empty;
                var sTS = this.Timespan != 0 ? $"&{ParamKeys.TimeSpan}={this.Timespan}" : string.Empty;

                sendTo = string.Empty;
                if (this.ViewUrlTypeValue.Equals(ViewUrlType.Views))
                {
                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, "~/default.aspx?tabid=" + this.TabId + $"&{ParamKeys.ViewType}={Views.Grid}&{ParamKeys.GridType}=" + this.GridType + sPage + sTS + qs);
                }
                else if (this.ViewUrlTypeValue.Equals(ViewUrlType.Category) && this.CategoryId > 0)
                {
                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, "~/default.aspx?tabid=" + this.TabId + $"&{ParamKeys.Category}=" + this.CategoryId + sPage + sTS + qs);
                }
                else if (this.ViewUrlTypeValue.Equals(ViewUrlType.Tag) && this.TagId > 0)
                {
                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, "~/default.aspx?tabid=" + this.TabId + $"&{ParamKeys.ViewType}={Views.Grid}&{ParamKeys.GridType}={Views.Tags}&{ParamKeys.Tags}=" + this.TagId + sPage + sTS + qs);
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
                System.Diagnostics.Debug.WriteLine($"url rewriter processing time: {st.ElapsedMilliseconds} ms; from: {rawUrlLower} to: {sendTo}");
#endif
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(DotNetNuke.Common.Utilities.Null.NullInteger, urlRewriteCacheKey, sendTo);
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

            var parsedUrl = url;
            string segmentName;
            var categoryPrefix = this.MainSettings?.PrefixURLCategory ?? "category";
            var tagPrefix = this.MainSettings?.PrefixURLTag ?? "tag";

            if (this.ExtractPrefixedSegment(ref parsedUrl, categoryPrefix, out segmentName))
            {
                var category = this.categoryController.GetByName(this.ModuleId, segmentName);
                this.CategoryId = category?.CategoryId ?? DotNetNuke.Common.Utilities.Null.NullInteger;
            }
            else if (this.ExtractPrefixedSegment(ref parsedUrl, tagPrefix, out segmentName))
            {
                var tag = this.tagController.GetByName(this.ModuleId, segmentName);
                this.TagId = tag?.TagId ?? DotNetNuke.Common.Utilities.Null.NullInteger;
            }

            var urlLower = parsedUrl.ToLowerInvariant();

            this.GridType = AllGridTypes.FirstOrDefault(g =>
                urlLower.Equals(g, StringComparison.OrdinalIgnoreCase) ||
                urlLower.StartsWith(g + "/", StringComparison.OrdinalIgnoreCase) ||
                urlLower.Contains("/" + g + "/") ||
                urlLower.EndsWith("/" + g, StringComparison.OrdinalIgnoreCase));

            if (this.GridType == null)
            {
                return;
            }

            // /gridtype/ts/{timespan}/{page} or gridtype/ts/{timespan}/{page}
            var tsPattern = $@"(?:^|/){Regex.Escape(this.GridType)}/ts/(?<timespan>\d+)(?:/(?<page>\d+))?(?:/|$)";
            var tsMatch = RegexUtils.GetCachedRegex(tsPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase).Match(parsedUrl);
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

            // /gridtype/{page} or gridtype/{page}
            var pagePattern = $@"(?:^|/){Regex.Escape(this.GridType)}/(?<page>\d+)(?:/|$)";
            var pageMatch = RegexUtils.GetCachedRegex(pagePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase).Match(parsedUrl);
            if (pageMatch.Success && pageMatch.Groups["page"].Success)
            {
                this.Page = Utilities.SafeConvertInt(pageMatch.Groups["page"].Value, 0);
            }
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
            var rx = DotNetNuke.Common.Utilities.RegexUtils.GetCachedRegex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var m = rx.Match(url);
            if (!m.Success)
            {
                return false;
            }

            name = m.Groups["name"].Value;
            this.OtherPrefix = name;

            // remove the matched segment and normalize duplicate slashes
            url = rx.Replace(url, "/");
            url = Regex.Replace(url, "/{2,}", "/");

            return true;
        }

        private static void Redirect(HttpApplication app, string sUrl)
        {
            app.Response.Clear();
            app.Response.StatusCode = 301;
            app.Response.RedirectLocation = sUrl;
            app.Response.Flush();
            app.CompleteRequest();
        }

        internal void HandleNonLikesPages(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            var urlSegmentMatches = DotNetNuke.Common.Utilities.RegexUtils.GetCachedRegex(@"(?<Page>[\d]+)/?\Z|(?<segment>[^\/]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline).Matches(url);
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

        internal string HandleLikesPages(string searchURL, string likesPrefix)
        {
            var newSearchURL = searchURL;
            if (newSearchURL.StartsWith("/"))
            {
                newSearchURL = newSearchURL.Substring(1);
            }

            var escapedLikesPrefix = Regex.Escape(likesPrefix ?? "likes");
            var pattern = $@"(?<likewithoutpage>.*/?{escapedLikesPrefix}/(?<contentId>[\d]+))/(?<page>[\d]+)|(?<likewithoutpage>.*/?{escapedLikesPrefix}/(?<contentId>[\d]+))";
            var likeMatches = RegexUtils.GetCachedRegex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(newSearchURL);
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
                        var pair = parts[i].Split('=');
                        var pairKey = pair[0].ToLowerInvariant();
                        var pairValue = Utilities.SafeConvertInt(pair[1], DotNetNuke.Common.Utilities.Null.NullInteger);
                        if (pairKey == ParamKeys.ForumId.ToLowerInvariant())
                        {
                            this.ForumId = pairValue;
                        }
                        else if (pairKey == ParamKeys.TopicId.ToLowerInvariant())
                        {
                            this.TopicId = pairValue;
                        }
                        else if (pairKey == ParamKeys.PageId.ToLowerInvariant())
                        {
                            this.Page = pairValue;
                        }
                        else if (pairKey == ParamKeys.PageJumpId.ToLowerInvariant())
                        {
                            this.Page = pairValue;
                        }
                        else if (pairKey == ParamKeys.TabId.ToLowerInvariant())
                        {
                            this.TabId = pairValue;
                        }
                        else if (pairKey == ParamKeys.ContentJumpId.ToLowerInvariant())
                        {
                            this.ContentId = pairValue;
                        }
                    }
                    else
                    {
                        if (i + 1 >= parts.Length)
                        {
                            continue;
                        }

                        var pairKey = parts[i].ToLowerInvariant();
                        var pairValue = Utilities.SafeConvertInt(parts[i + 1], DotNetNuke.Common.Utilities.Null.NullInteger);
                        if (pairKey == ParamKeys.ForumId.ToLowerInvariant())
                        {
                            this.ForumId = pairValue;
                        }
                        else if (pairKey == ParamKeys.TopicId.ToLowerInvariant())
                        {
                            this.TopicId = pairValue;
                        }
                        else if (pairKey == ParamKeys.PageId.ToLowerInvariant())
                        {
                            this.Page = pairValue;
                        }
                        else if (pairKey == ParamKeys.PageJumpId.ToLowerInvariant())
                        {
                            this.Page = pairValue;
                        }
                        else if (pairKey == ParamKeys.TabId.ToLowerInvariant())
                        {
                            this.TabId = pairValue;
                        }
                        else if (pairKey == ParamKeys.ContentJumpId.ToLowerInvariant())
                        {
                            this.ContentId = pairValue;
                        }
                    }
                }
            }
        }

    }
}
