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
    using System;
    using System.Web.Caching;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.UI.UserControls;

    /// <summary>
    /// Represents a badge in the DNN Community Forums module.
    /// </summary>
    [TableName("activeforums_Badges")]
    [PrimaryKey("BadgeId", AutoIncrement = true)]
    [Cacheable("activeforums_Badges", CacheItemPriority.Normal)]
    [Scope("ModuleId")]
    public class BadgeInfo
    {
        /// <summary>
        /// Gets or sets the badge ID.
        /// </summary>
        public int BadgeId { get; set; }

        /// <summary>
        /// Gets or sets the ModuleId.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets or sets the badge name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the badge description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the file Id for the badge.
        /// </summary>
        public int FileId { get; set; }

        /// <summary>
        /// Gets or sets the SortOrder for the badge.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the BadgeMetric.
        /// </summary>
        public DotNetNuke.Modules.ActiveForums.Enums.BadgeMetric BadgeMetric { get; set; }

        /// <summary>
        /// Gets or sets the Threshold.
        /// </summary>
        public int Threshold { get; set; }

        /// <summary>
        /// Gets or sets the SendAwardNotification.
        /// </summary>
        public bool SendAwardNotification { get; set; }

        /// <summary>
        /// Gets or sets the SuppresssAwardNotificationOnBackfill.
        /// </summary>
        public bool SuppresssAwardNotificationOnBackfill { get; set; }

        /// <summary>
        /// Gets the enum name/description for the badge metric.
        /// </summary>
        [IgnoreColumn]
        public string BadgeMetricEnumName => Enum.GetName(typeof(DotNetNuke.Modules.ActiveForums.Enums.BadgeMetric), this.BadgeMetric);

        /// <summary>
        /// Gets the Url for the badge image.
        /// </summary>
        [IgnoreColumn]
        public string GetBadgeImageUrl(int portalId, int size = 32) => this.FileId <= 0 ? string.Empty : Utilities.ResolveUrl($"https://{Utilities.GetPortalSettings(portalId).DefaultPortalAlias}/DnnImageHandler.ashx?mode=securefile&fileId={this.FileId}&h={size}&w={size}");
    }
}
