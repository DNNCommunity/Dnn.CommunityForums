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

namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Policy;
    using System.Web.Http;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Modules.ActiveForums.ViewModels;
    using DotNetNuke.Web.Api;

    using Newtonsoft.Json;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// 
    public class SearchController : ControllerBase<SearchController>
    {
        /// <summary>
        /// Simple Search.
        /// </summary>
        ///
        /// <param name="query" type="string">query text.</param>
        /// <returns><see cref="DotNetNuke.Modules.ActiveForums.ViewModels.IPost"/> containing the search results.</returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Search/Search?query=yyy</remarks>
        [HttpGet]
        [DnnAuthorize]
        /* [ForumsAuthorize(SecureActions.View)] no security applied here as it is handled in the search API */
        public HttpResponseMessage Search(string query)
        { 
                return this.ReturnResults(new SearchRequestDto()
                {
                    Query = query,
                    RowIndex = 0,
                    PageSize = 1000,
                    SearchHours = DotNetNuke.Common.Utilities.Null.NullInteger,
                    AuthorUserId = DotNetNuke.Common.Utilities.Null.NullInteger,
                    AuthorUsername = null,
                    ForumIds = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.ForumModuleId, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.PortalSettings.PortalId, this.UserInfo.UserID), DotNetNuke.Modules.ActiveForums.SecureActions.Read).FromHashSetToDelimitedString(","),
                    Tags = string.Empty,
                    ResultType = DotNetNuke.Modules.ActiveForums.Enums.SearchResultType.SearchByTopics,
                    SortResultsBy = DotNetNuke.Modules.ActiveForums.Enums.SearchSortType.SearchSortTypeRelevance,
                });
        }

        /// <summary>
        /// Advanced Search.
        /// </summary>
        ///
        /// <param name="dto" type="SearchRequestDto">Search Request parameters.</param>
        /// <returns><see cref="DotNetNuke.Modules.ActiveForums.ViewModels.IPost"/> containing the search results.</returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Search/Search</remarks>
        [HttpPost]
        [DnnAuthorize]
        /* [ForumsAuthorize(SecureActions.View)] no security applied here as it is handled in the search API */
        public HttpResponseMessage Search([FromBody] SearchRequestDto dto)
        {
            return this.ReturnResults(dto);
        }

        private HttpResponseMessage ReturnResults(SearchRequestDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Query))
                {
                    return this.Request.CreateResponse(HttpStatusCode.NoContent);
                }

                var results = DotNetNuke.Modules.ActiveForums.Services.Search.SearchController.Provider.Search(
                    portalId: this.PortalSettings.PortalId,
                    moduleId: this.ForumModuleId,
                    userId: this.UserInfo.UserID,
                    rowIndex: dto.RowIndex,
                    pageSize: dto.PageSize,
                    searchText: System.Net.WebUtility.UrlEncode(dto.Query),
                    searchHours: dto.SearchHours,
                    authorUserId: dto.AuthorUserId,
                    authorUsername: dto.AuthorUsername,
                    forumsToSearch: !string.IsNullOrEmpty(dto.ForumIds) ? dto.ForumIds : DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.ForumModuleId, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.PortalSettings.PortalId, this.UserInfo.UserID), DotNetNuke.Modules.ActiveForums.SecureActions.Read).FromHashSetToDelimitedString(","),
                    tags: dto.Tags,
                    resultType: dto.ResultType,
                    sort: dto.SortResultsBy);
                var searchResults = new List<DotNetNuke.Modules.ActiveForums.ViewModels.IPost>();
                searchResults.AddRange(results.Results.Select(post => new DotNetNuke.Modules.ActiveForums.ViewModels.IPost(post)));

                if (searchResults != null && searchResults.Count > 0)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, searchResults);
                }

                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        public class SearchRequestDto
        {
            /// <summary>
            /// Keywords to search for.
            /// </summary>
            public string Query { get; set; } = string.Empty;

            /// <summary>
            /// Zero-based index of first row to return; default is 0.
            /// </summary>
            public int RowIndex { get; set; } = 0;

            /// <summary>
            /// Number of rows to return; default is 1000.
            /// </summary>
            public int PageSize { get; set; } = 1000;

            /// <summary>
            /// Limit search to past N number of hours; default is -1 (no limit).
            /// </summary>
            public int SearchHours { get; set; } = -1;

            /// <summary>
            /// DNN User ID for author to filter by.
            /// </summary>
            public int AuthorUserId { get; set; } = -1;

            /// <summary>
            /// Username for author to filter by.
            /// </summary>
            public string AuthorUsername { get; set; } = string.Empty;

            /// <summary>
            /// Comma-separated list of tag to filter by.
            /// </summary>
            public string Tags { get; set; } = string.Empty;

            /// <summary>
            /// Comma-separated list of forum Ids to filter by.
            /// </summary>
            public string ForumIds { get; set; } = string.Empty;

            /// <summary>
            /// Search result type: 0 - Search by Topics; 1 - Search by Posts (Topics &amp; Replies). Default is Search by Topics.
            /// </summary>
            public DotNetNuke.Modules.ActiveForums.Enums.SearchResultType ResultType { get; set; } = Enums.SearchResultType.SearchByTopics;

            /// <summary>
            /// Search result sort type: 0 - Relevance; 1 - Date Modified. Default is Relevance.
            /// </summary>
            public DotNetNuke.Modules.ActiveForums.Enums.SearchSortType SortResultsBy { get; set; } = Enums.SearchSortType.SearchSortTypeRelevance;
        }
    }
}
