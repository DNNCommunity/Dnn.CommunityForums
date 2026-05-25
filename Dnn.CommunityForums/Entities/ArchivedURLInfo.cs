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

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using DotNetNuke.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents an archived forum URL entry.
    /// </summary>
    [TableName("activeforums_ArchivedURLs")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class ArchivedURLInfo
    {
        /// <summary>
        /// Gets or sets the archived URL identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the archived URL value.
        /// </summary>
        [ColumnName("URL")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the portal identifier.
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// Gets or sets the forum identifier.
        /// </summary>
        public int ForumId { get; set; }

        /// <summary>
        /// Gets or sets the topic identifier.
        /// </summary>
        public int TopicId { get; set; }

        /// <summary>
        /// Gets or sets the forum group identifier.
        /// </summary>
        public int ForumGroupId { get; set; }

        /// <summary>
        /// Gets or sets the MD5 hash for normalized URL lookups.
        /// </summary>
        [ColumnName("URL_Hash")]
        public byte[] UrlHash { get; set; }
    }
}
