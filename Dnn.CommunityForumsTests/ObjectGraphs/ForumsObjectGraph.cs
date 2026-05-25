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

namespace DotNetNuke.Modules.ActiveForumsTests.ObjectGraphs
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.Entities;

    /// <summary>
    /// Provides pre-built object graphs for forum groups and forums for use in unit tests.
    /// </summary>
    internal static class ForumsObjectGraph
    {
        // -----------------------------------------------------------------------
        // Module / Portal constants
        // -----------------------------------------------------------------------
        internal const int ModuleId = 1;
        internal const int PortalId = 0;

        // -----------------------------------------------------------------------
        // Forum Group IDs
        // -----------------------------------------------------------------------
        internal const int GeneralGroupId = 1;
        internal const int PrivateGroupId = 2;

        // -----------------------------------------------------------------------
        // Forum IDs
        // -----------------------------------------------------------------------
        internal const int AnnouncementsForumId = 1;
        internal const int GeneralDiscussionForumId = 2;
        internal const int HelpAndSupportForumId = 3;
        internal const int MembersOnlyForumId = 4;
        internal const int AdministratorsOnlyForumId = 5;

        // -----------------------------------------------------------------------
        // Tag IDs
        // -----------------------------------------------------------------------
        internal const int Tag1Id = 1;
        internal const int Tag2Id = 2;
        internal const int Tag3Id = 3;
        internal const int Tag4Id = 4;
        internal const int Tag5Id = 5;
        internal const int Tag6Id = 6;
        internal const int Tag7Id = 7;
        internal const int Tag8Id = 8;
        internal const int Tag9Id = 9;
        internal const int Tag10Id = 10;

        // -----------------------------------------------------------------------
        // Category IDs
        // -----------------------------------------------------------------------
        internal const int Category1Id = 1;
        internal const int Category2Id = 2;
        internal const int Category3Id = 3;
        internal const int Category4Id = 4;
        internal const int Category5Id = 5;
        internal const int Category6Id = 6;
        internal const int Category7Id = 7;
        internal const int Category8Id = 8;
        internal const int Category9Id = 9;
        internal const int Category10Id = 10;

        // -----------------------------------------------------------------------
        // Permission strings
        // -----------------------------------------------------------------------

        /// <summary>Open to all users (anonymous + authenticated).</summary>
        internal static readonly string PublicForumPermission = Globals.DefaultAnonRoles;

        /// <summary>Restricted to registered users only.</summary>
        internal static readonly string RegisteredPermission = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}";

        /// <summary>Restricted to administrators only.</summary>
        internal static readonly string AdministratorsPermission = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}";

        // -----------------------------------------------------------------------
        // Generalized builders
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds a <see cref="ForumGroupInfo"/> with the supplied values.
        /// </summary>
        /// <param name="groupId">Primary key of the group.</param>
        /// <param name="groupName">Display name of the group.</param>
        /// <param name="prefixUrl">URL prefix segment for the group.</param>
        /// <param name="sortOrder">Display sort order.</param>
        /// <param name="viewPermission">
        /// Permission string controlling who can view the group.
        /// Defaults to <see cref="PublicForumPermission"/> when <c>null</c>.
        /// </param>
        /// <param name=readPermission">
        /// Permission string controlling who can read the group.
        /// Defaults to <see cref="PublicForumPermission"/> when <c>null</c>.
        /// </param>
        /// <param name="active">Whether the group is active. Defaults to <c>true</c>.</param>
        /// <param name="hidden">Whether the group is hidden. Defaults to <c>false</c>.</param>
        internal static Modules.ActiveForums.Entities.ForumGroupInfo BuildGroup(
            int groupId,
            string groupName,
            string prefixUrl,
            int sortOrder,
            string viewPermission = null,
            string readPermission = null,
            bool active = true,
            bool hidden = false) =>
            new Modules.ActiveForums.Entities.ForumGroupInfo
            {
                ForumGroupId = groupId,
                ModuleId = ModuleId,
                GroupName = groupName,
                PrefixURL = prefixUrl,
                SortOrder = sortOrder,
                Active = active,
                Hidden = hidden,
                Security = new Modules.ActiveForums.Entities.PermissionInfo
                {
                    View = viewPermission ?? PublicForumPermission,
                    Read = readPermission ?? PublicForumPermission,
                },
            };

        /// <summary>
        /// Builds a <see cref="ForumInfo"/> with the supplied values.
        /// </summary>
        /// <param name="portalSettings">Portal settings used to construct <see cref="ForumInfo"/>.</param>
        /// <param name="forumId">Primary key of the forum.</param>
        /// <param name="group">Parent <see cref="ForumGroupInfo"/>.</param>
        /// <param name="forumName">Display name of the forum.</param>
        /// <param name="prefixUrl">URL prefix segment for the forum.</param>
        /// <param name="description">Forum description.</param>
        /// <param name="sortOrder">Display sort order within the group.</param>
        /// <param name="viewPermission">
        /// Permission string controlling who can view the forum.
        /// Defaults to <see cref="PublicForumPermission"/> when <c>null</c>.
        /// </param>
        /// <param name="readPermission">
        /// Permission string controlling who can view the forum.
        /// Defaults to <see cref="PublicForumPermission"/> when <c>null</c>.
        /// </param>
        /// <param name="totalTopics">Seed value for total topic count.</param>
        /// <param name="totalReplies">Seed value for total reply count.</param>
        /// <param name="active">Whether the forum is active. Defaults to <c>true</c>.</param>
        /// <param name="hidden">Whether the forum is hidden. Defaults to <c>false</c>.</param>
        internal static DotNetNuke.Modules.ActiveForums.Entities.ForumInfo BuildForum(
            PortalSettings portalSettings,
            int forumId,
            Modules.ActiveForums.Entities.ForumGroupInfo group,
            string forumName,
            string prefixUrl,
            string description = "",
            int sortOrder = 1,
            string viewPermission = null,
            string readPermission = null,
            int totalTopics = 0,
            int totalReplies = 0,
            bool active = true,
            bool hidden = false) =>
            new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo(portalSettings)
            {
                ForumID = forumId,
                PortalId = PortalId,
                ModuleId = ModuleId,
                ForumGroupId = group.ForumGroupId,
                ParentForumId = 0,
                ForumName = forumName,
                ForumDesc = description,
                PrefixURL = prefixUrl,
                SortOrder = sortOrder,
                Active = active,
                Hidden = hidden,
                TotalTopics = totalTopics,
                TotalReplies = totalReplies,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo
                {
                    View = viewPermission ?? PublicForumPermission,
                    Read = readPermission ?? PublicForumPermission,
                },
                ForumGroup = group,
            };

        // -----------------------------------------------------------------------
        // Tag builders
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds a <see cref="TagInfo"/> with the supplied values.
        /// </summary>
        internal static TagInfo BuildTag(int tagId, string tagName, int items = 0) =>
            new TagInfo
            {
                TagId = tagId,
                PortalId = PortalId,
                ModuleId = ModuleId,
                TagName = tagName,
                Items = items,
            };

        /// <summary>
        /// Builds 10 tags for tests.
        /// </summary>
        internal static List<TagInfo> BuildTagsGraph() =>
            new List<TagInfo>
            {
                BuildTag(Tag1Id, "announcements", 3),
                BuildTag(Tag2Id, "support", 7),
                BuildTag(Tag3Id, "howto", 5),
                BuildTag(Tag4Id, "tips", 2),
                BuildTag(Tag5Id, "release-notes", 4),
                BuildTag(Tag6Id, "migration", 1),
                BuildTag(Tag7Id, "performance", 6),
                BuildTag(Tag8Id, "glanton", 1),
                BuildTag(Tag9Id, "security", 3),
                BuildTag(Tag10Id, "feedback", 2),
            };

        // -----------------------------------------------------------------------
        // Category builders
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds a <see cref="CategoryInfo"/> with the supplied values.
        /// </summary>
        /// <param name="categoryId">Primary key of the category.</param>
        /// <param name="categoryName">Display name of the category.</param>
        /// <param name="forumId">Forum this category belongs to.</param>
        /// <param name="forumGroupId">Forum group this category belongs to.</param>
        /// <param name="clicks">Seed click count.</param>
        /// <param name="items">Seed item count.</param>
        /// <param name="priority">Sort priority.</param>
        internal static CategoryInfo BuildCategory(
            int categoryId,
            string categoryName,
            int forumId,
            int forumGroupId,
            int clicks = 0,
            int items = 0,
            int priority = 0) =>
            new CategoryInfo
            {
                CategoryId = categoryId,
                PortalId = PortalId,
                ModuleId = ModuleId,
                CategoryName = categoryName,
                ForumId = forumId,
                ForumGroupId = forumGroupId,
                Clicks = clicks,
                Items = items,
                Priority = priority,
            };

        /// <summary>
        /// Builds 10 categories for tests, spread across existing forums and groups.
        /// </summary>
        internal static List<CategoryInfo> BuildCategoriesGraph() =>
            new List<CategoryInfo>
            {
                BuildCategory(Category1Id,  "Bug Reports",        AnnouncementsForumId,     GeneralGroupId,  clicks: 10, items: 3,  priority: 1),
                BuildCategory(Category2Id,  "Feature Requests",   AnnouncementsForumId,     GeneralGroupId,  clicks: 25, items: 7,  priority: 2),
                BuildCategory(Category3Id,  "How-To",             GeneralDiscussionForumId, GeneralGroupId,  clicks: 5,  items: 5,  priority: 1),
                BuildCategory(Category4Id,  "Tips & Tricks",      GeneralDiscussionForumId, GeneralGroupId,  clicks: 8,  items: 2,  priority: 2),
                BuildCategory(Category5Id,  "Release Notes",      HelpAndSupportForumId,    GeneralGroupId,  clicks: 15, items: 4,  priority: 1),
                BuildCategory(Category6Id,  "Installation",       HelpAndSupportForumId,    GeneralGroupId,  clicks: 12, items: 6,  priority: 2),
                BuildCategory(Category7Id,  "Performance",        HelpAndSupportForumId,    GeneralGroupId,  clicks: 3,  items: 1,  priority: 3),
                BuildCategory(Category8Id,  "Members News",       MembersOnlyForumId,       PrivateGroupId,  clicks: 4,  items: 2,  priority: 1),
                BuildCategory(Category9Id,  "Admin Policies",     AdministratorsOnlyForumId, PrivateGroupId, clicks: 2,  items: 1,  priority: 1),
                BuildCategory(Category10Id, "Feedback",           GeneralDiscussionForumId, GeneralGroupId,  clicks: 9,  items: 3,  priority: 3),
            };

        // -----------------------------------------------------------------------
        // Named convenience wrappers
        // -----------------------------------------------------------------------

        /// <summary>Builds the General (public) <see cref="DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo"/>.</summary>
        internal static DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo BuildGeneralGroup() =>
            BuildGroup(GeneralGroupId, "General", "general", sortOrder: 1, PublicForumPermission);

        /// <summary>Builds the Members-Only (restricted) <see cref="DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo"/>.</summary>
        internal static DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo BuildPrivateGroup() =>
            BuildGroup(PrivateGroupId, "Members Only", "members-only", sortOrder: 2, RegisteredPermission);

        /// <summary>Builds the Announcements <see cref="ForumInfo"/>.</summary>
        internal static DotNetNuke.Modules.ActiveForums.Entities.ForumInfo BuildAnnouncementsForum(PortalSettings portalSettings) =>
            BuildForum(portalSettings, AnnouncementsForumId, BuildGeneralGroup(), "Announcements", "announcements", "Site-wide announcements.", sortOrder: 1, totalTopics: 5, totalReplies: 12);

        /// <summary>Builds the General Discussion <see cref="ForumInfo"/>.</summary>
        internal static DotNetNuke.Modules.ActiveForums.Entities.ForumInfo BuildGeneralDiscussionForum(PortalSettings portalSettings) =>
            BuildForum(portalSettings, GeneralDiscussionForumId, BuildGeneralGroup(), "General Discussion", "general-discussion", "Talk about anything.", sortOrder: 2, totalTopics: 42, totalReplies: 137);

        /// <summary>Builds the Help &amp; Support <see cref="ForumInfo"/>.</summary>
        internal static DotNetNuke.Modules.ActiveForums.Entities.ForumInfo BuildHelpAndSupportForum(PortalSettings portalSettings) =>
            BuildForum(portalSettings, HelpAndSupportForumId, BuildGeneralGroup(), "Help & Support", "help-support", "Get help from the community.", sortOrder: 3, totalTopics: 20, totalReplies: 88);

        /// <summary>Builds the Members-Only <see cref="ForumInfo"/>.</summary>
        internal static DotNetNuke.Modules.ActiveForums.Entities.ForumInfo BuildMembersOnlyForum(PortalSettings portalSettings) =>
            BuildForum(portalSettings, MembersOnlyForumId, BuildPrivateGroup(), "Members Only", "members-only-forum", "For registered members.", sortOrder: 1, viewPermission: RegisteredPermission, readPermission: RegisteredPermission, totalTopics: 8, totalReplies: 24);

        /// <summary>Builds the Administrators-Only <see cref="ForumInfo"/>.</summary>
        internal static DotNetNuke.Modules.ActiveForums.Entities.ForumInfo BuildAdministratorsOnlyForum(PortalSettings portalSettings) =>
            BuildForum(portalSettings, AdministratorsOnlyForumId, BuildPrivateGroup(), "Admin", "admin-forum", "Administrators only forum.", sortOrder: 2, viewPermission: AdministratorsPermission, readPermission: AdministratorsPermission, totalTopics: 3, totalReplies: 7);

        /// <summary>
        /// Builds the full forums graph used in tests.
        /// </summary>
        /// <param name="portalSettings">Portal settings used to construct <see cref="ForumInfo"/> instances.</param>
        internal static List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> BuildForumsGraph(PortalSettings portalSettings)
        {
            var generalGroup = BuildGeneralGroup();
            var privateGroup = BuildPrivateGroup();

            return new List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>
            {
                BuildForum(portalSettings, AnnouncementsForumId,     generalGroup, "Announcements",      "announcements",      "Site-wide announcements.",      sortOrder: 1, totalTopics: 5,  totalReplies: 12),
                BuildForum(portalSettings, GeneralDiscussionForumId, generalGroup, "General Discussion", "general-discussion", "Talk about anything.",          sortOrder: 2, totalTopics: 42, totalReplies: 137),
                BuildForum(portalSettings, HelpAndSupportForumId,    generalGroup, "Help & Support",     "help-support",       "Get help from the community.",  sortOrder: 3, totalTopics: 20, totalReplies: 88),
                BuildForum(portalSettings, MembersOnlyForumId, privateGroup, "Members Only", "members-only-forum", "For registered members.",   sortOrder: 1, viewPermission: RegisteredPermission, readPermission: RegisteredPermission, totalTopics: 8, totalReplies: 24),
                BuildForum(portalSettings, AdministratorsOnlyForumId,     privateGroup, "Admin",      "admin-forum",      "Administrators only forum.",  sortOrder: 2, viewPermission: AdministratorsPermission, readPermission: AdministratorsPermission, totalTopics: 3, totalReplies: 7),
            };
        }

    }
}
