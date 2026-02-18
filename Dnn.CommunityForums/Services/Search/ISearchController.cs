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

using DotNetNuke.Modules.ActiveForums.Enums;

namespace DotNetNuke.Modules.ActiveForums.Services.Search
{
    /// <summary>
    /// Defines search-related operations for the forums module.
    /// Implementations should forward to the underlying data provider.
    /// </summary>
    internal interface ISearchController
    {
        /// <summary>
        /// Executes a search against forum content.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="moduleId">The module identifier.</param>
        /// <param name="userId">The current user identifier.</param>
        /// <param name="rowIndex">Zero-based index of the first result row to return.</param>
        /// <param name="pageSize">Number of rows to return.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="searchHours">Limit results to the last N hours.</param>
        /// <param name="authorUserId">Filter by author user id.</param>
        /// <param name="authorUsername">Filter by author username.</param>
        /// <param name="forumsToSearch">Comma-separated forum ids to search.</param>
        /// <param name="tags">Comma-separated tags to filter by.</param>
        /// <param name="resultType">Type of results to return.</param>
        /// <param name="sort">Sort order.</param>
        /// 
        /// 
        /// <returns>A <see cref="DotNetNuke.Modules.ActiveForums.ViewModels.SearchResults"/> containing the search results.</returns>
        DotNetNuke.Modules.ActiveForums.ViewModels.SearchResults Search(
            int portalId,
            int moduleId,
            int userId,
            int rowIndex,
            int pageSize,
            string searchText,
            int searchHours,
            int authorUserId,
            string authorUsername,
            string forumsToSearch,
            string tags,
            SearchResultType resultType,
            SearchSortType sort);
    }
}
