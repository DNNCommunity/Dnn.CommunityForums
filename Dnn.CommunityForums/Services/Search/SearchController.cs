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

namespace DotNetNuke.Modules.ActiveForums.Services.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;

    internal static class SearchController
    {
        /// <summary>
        /// Gets or sets pluggable search provider - defaults to the DataProvider-backed implementation.
        /// Tests can replace this with a mock implementation.
        /// </summary>
        internal static DotNetNuke.Modules.ActiveForums.Services.Search.ISearchController Provider { get; set; } = new DotNetNuke.Modules.ActiveForums.Services.Search.SearchController.LuceneSearchController();

        internal static DotNetNuke.Modules.ActiveForums.ViewModels.SearchResults Search(
            int portalId,
            int moduleId,
            int userId,
            int searchId,
            int rowIndex,
            int pageSize,
            string searchText,
            int searchDays,
            int authorUserId,
            string authorUsername,
            string forumsToSearch,
            string tags,
            SearchSortType sort,
            SearchResultType resultType,
            int maxCacheHours)
        {
            return Provider.Search(
                portalId,
                moduleId,
                userId,
                searchId,
                rowIndex,
                pageSize,
                searchText,
                searchDays,
                authorUserId,
                authorUsername,
                forumsToSearch,
                tags,
                resultType,
                sort,
                maxCacheHours);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        private sealed class DataProviderSearchController : DotNetNuke.Modules.ActiveForums.Services.Search.ISearchController
        {
            public DotNetNuke.Modules.ActiveForums.ViewModels.SearchResults Search(
                int portalId,
                int moduleId,
                int userId,
                int searchId,
                int rowIndex,
                int pageSize,
                string searchText,
                int searchDays,
                int authorUserId,
                string authorUsername,
                string forumsToSearch,
                string tags,
                SearchResultType resultType,
                SearchSortType sort,
                int maxCacheHours) => throw new NotImplementedException();
        }

        private sealed class LuceneSearchController : DotNetNuke.Modules.ActiveForums.Services.Search.ISearchController
        {
            public DotNetNuke.Modules.ActiveForums.ViewModels.SearchResults Search(
                int portalId,
                int moduleId,
                int userId,
                int searchId,
                int rowIndex,
                int pageSize,
                string searchText,
                int searchDays,
                int authorUserId,
                string authorUsername,
                string forumsToSearch,
                string tags,
                SearchResultType resultType,
                SearchSortType sort,
                int maxCacheHours)
            {
                DateTime startQuery = DateTime.UtcNow;

                IList<DotNetNuke.Services.Search.Entities.SearchResult> searchResults = null;
                var query = new DotNetNuke.Services.Search.Entities.SearchQuery();
                query.PortalIds = new[] { portalId };
                query.ModuleId = moduleId;
                query.SearchTypeIds = new[] { SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId };
                query.Tags = tags.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                query.AllowLeadingWildcard = true;
                query.WildCardSearch = true;
                query.SortField = sort.Equals(SearchSortType.SearchSortTypeRelevance) ? SortFields.Relevance : SortFields.LastModified;
                if (searchDays > 0)
                {
                    query.BeginModifiedTimeUtc = DateTime.UtcNow.AddHours(-1 * searchDays); /* Parameter is searchDays but really is maxCacheHours */
                    query.EndModifiedTimeUtc = DateTime.UtcNow;
                }

                query.KeyWords = searchText;
                var numericKeys = new Dictionary<string, int>();
                if (authorUserId > 0)
                {
                    numericKeys.Add("AuthorUserId", authorUserId);
                }

                var forumIds = forumsToSearch.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(f => Utilities.SafeConvertInt(f)).ToList();
                if (forumIds.Count == 1)
                {
                    numericKeys.Add("ForumId", Utilities.SafeConvertInt(forumIds.First(), 0));
                };

                query.NumericKeys = numericKeys;

                try
                {
                    searchResults = DotNetNuke.Services.Search.Controllers.SearchController.Instance.ModuleSearch(query).Results;
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }

                var results = new DotNetNuke.Modules.ActiveForums.ViewModels.SearchResults();
                results.Results = new List<DotNetNuke.Modules.ActiveForums.Entities.IPostInfo>();

                if (searchResults != null)
                {
                    var searchResultsPruned = searchResults;
                    if (forumIds.Count > 1)
                    {
                        // Filter results to only those forums specified
                        searchResultsPruned = searchResultsPruned.Where(doc => doc.NumericKeys.ContainsKey("ForumId") && forumIds.Contains(Utilities.SafeConvertInt(doc.NumericKeys["ForumId"]))).ToList();
                    }

                    if (authorUserId > 0)
                    {
                        // Further filter results to only those by the specified author
                        searchResultsPruned = searchResultsPruned.Where(doc => doc.AuthorUserId.Equals(authorUserId)).ToList();
                    }

                    if (!string.IsNullOrEmpty(authorUsername))
                    {
                        // Further filter results to only those by the specified author
                        searchResultsPruned = searchResultsPruned.Where(doc => doc.AuthorName.Equals(authorUsername)).ToList();
                    }

                    if (resultType.Equals(SearchResultType.SearchByTopics))
                    {
                        searchResultsPruned = searchResultsPruned.Where(doc => !doc.NumericKeys.ContainsKey("ReplyId") || (doc.NumericKeys.ContainsKey("ReplyId") && doc.NumericKeys["ReplyId"] < 1)).ToList();
                    }

                    foreach (var result in searchResultsPruned.Skip(rowIndex * pageSize).Take(pageSize))
                    {
                        DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post = null;
                        var topicId = DotNetNuke.Common.Utilities.Null.NullInteger;
                        Utilities.SafeConvertInt(result.NumericKeys.TryGetValue("TopicId", out topicId));
                        if (resultType.Equals(SearchResultType.SearchByTopics))
                        {
                            post = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(moduleId).GetById(topicId);
                        }
                        else
                        {
                            var contentId = DotNetNuke.Common.Utilities.Null.NullInteger;
                            Utilities.SafeConvertInt(result.NumericKeys.TryGetValue("ContentId", out contentId));
                            var reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(moduleId).GetByContentId(contentId);
                            if (reply != null)
                            {
                                post = (DotNetNuke.Modules.ActiveForums.Entities.IPostInfo)reply;
                            }
                            else
                            {
                                var topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(moduleId).GetById(topicId);
                                if (topic != null)
                                {
                                    post = topic;
                                }
                            }
                        }

                        results.Results.Add(post);
                    }
                }

                results.SearchDuration = (int)(DateTime.UtcNow - startQuery).TotalMilliseconds;
                return results;
            }
        }
    }
}
