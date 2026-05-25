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
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.Entities;

    /// <summary>
    /// Provides pre-built <see cref="ForumUserInfo"/> object graphs for use in unit tests.
    /// </summary>
    internal static class ForumUserObjectGraph
    {
        // -----------------------------------------------------------------------
        // Profile ID constants  (distinct from DNN UserIDs)
        // -----------------------------------------------------------------------
        internal const int AnonymousProfileId = 0;
        internal const int AdminProfileId = 1;
        internal const int RegisteredUser10ProfileId = 2;
        internal const int RegisteredUser12ProfileId = 3;
        internal const int ModeratorProfileId = 4;

        // -----------------------------------------------------------------------
        // Generalized builder
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds a <see cref="ForumUserInfo"/> from the supplied values.
        /// </summary>
        internal static ForumUserInfo BuildForumUser(
            int moduleId,
            PortalSettings portalSettings,
            UserInfo userInfo,
            int profileId,
            int topicCount = 0,
            int replyCount = 0,
            TrustTypes trustLevel = TrustTypes.NotTrusted,
            string userCaption = "",
            string signature = "",
            HashSet<int> userRoleIds = null)
        {
            var forumUser = new ForumUserInfo(moduleId, portalSettings, userInfo)
            {
                ProfileId = profileId,
                UserId = userInfo.UserID,
                PortalId = portalSettings.PortalId,
                TopicCount = topicCount,
                ReplyCount = replyCount,
                TrustLevel = (int)trustLevel,
                UserCaption = userCaption,
                Signature = signature,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                DateLastActivity = DateTime.UtcNow,
                PrefDefaultSort = "ASC",
                PrefDefaultShowReplies = false,
                PrefJumpLastPost = false,
                PrefTopicSubscribe = false,
                PrefBlockAvatars = false,
                PrefBlockSignatures = false,
                PrefPageSize = 20,
                LikeNotificationsEnabled = true,
                PinNotificationsEnabled = true,
                BadgeNotificationsEnabled = true,
                UserMentionNotificationsEnabled = true,
                EnableNotificationsForOwnContent = false,
                UserRoleIds = userRoleIds ?? new HashSet<int>(),
            };

            return forumUser;
        }

        // -----------------------------------------------------------------------
        // Named convenience wrappers
        // -----------------------------------------------------------------------

        /// <summary>Builds an anonymous (guest) <see cref="ForumUserInfo"/> from an existing <see cref="UserInfo"/>.</summary>
        internal static ForumUserInfo BuildAnonymousUser(int moduleId, PortalSettings portalSettings, UserInfo userInfo) =>
            BuildForumUser(
                moduleId,
                portalSettings,
                userInfo,
                profileId: AnonymousProfileId,
                userRoleIds: new HashSet<int>
                {
                    Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser),
                    Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
                });

        /// <summary>Builds a registered <see cref="ForumUserInfo"/> for User10 from an existing <see cref="UserInfo"/>.</summary>
        internal static ForumUserInfo BuildRegisteredUntrustedUser10(int moduleId, PortalSettings portalSettings, UserInfo userInfo) =>
            BuildForumUser(
                moduleId,
                portalSettings,
                userInfo,
                profileId: RegisteredUser10ProfileId,
                topicCount: 1,
                replyCount: 1,
                trustLevel: TrustTypes.NotTrusted,
                userRoleIds: new HashSet<int>
                {
                    DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
                });

        /// <summary>Builds a registered <see cref="ForumUserInfo"/> for User12 from an existing <see cref="UserInfo"/>.</summary>
        internal static ForumUserInfo BuildRegisteredTrustedUser12(int moduleId, PortalSettings portalSettings, UserInfo userInfo) =>
            BuildForumUser(
                moduleId,
                portalSettings,
                userInfo,
                profileId: RegisteredUser12ProfileId,
                topicCount: 20,
                replyCount: 47,
                trustLevel: TrustTypes.Trusted,
                signature: "Forum member since 2020.",
                userRoleIds: new HashSet<int>
                {
                    DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
                });

        /// <summary>Builds an administrator <see cref="ForumUserInfo"/> from an existing <see cref="UserInfo"/>.</summary>
        internal static ForumUserInfo BuildAdminUser(int moduleId, PortalSettings portalSettings, UserInfo userInfo) =>
            BuildForumUser(
                moduleId,
                portalSettings,
                userInfo,
                profileId: AdminProfileId,
                topicCount: 100,
                replyCount: 250,
                trustLevel: TrustTypes.Trusted,
                userCaption: "Administrator",
                userRoleIds: new HashSet<int>
                {
                    DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                    DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
                    Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser),
                });

        /// <summary>Builds a moderator <see cref="ForumUserInfo"/> from an existing <see cref="UserInfo"/>.</summary>
        internal static ForumUserInfo BuildModeratorUser(int moduleId, PortalSettings portalSettings, UserInfo userInfo) =>
            BuildForumUser(
                moduleId,
                portalSettings,
                userInfo,
                profileId: ModeratorProfileId,
                topicCount: 30,
                replyCount: 80,
                trustLevel: TrustTypes.Trusted,
                userCaption: "Moderator",
                userRoleIds: new HashSet<int>
                {
                    DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    DotNetNuke.Tests.Utilities.Constants.RoleID_FirstSocialGroup,
                    Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
                });

        // -----------------------------------------------------------------------
        // Full graph — accepts the shared UserInfo instances from TestBase
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds the complete user graph using <see cref="UserInfo"/> instances that are
        /// already registered with the <c>userController</c> mock in <c>TestBase</c>,
        /// ensuring every <see cref="ForumUserInfo.UserInfo"/> reference is the same object.
        /// </summary>
        internal static List<ForumUserInfo> BuildFullGraph(
            int moduleId,
            PortalSettings portalSettings,
            UserInfo adminUserInfo,
            UserInfo user10Info,
            UserInfo user12Info,
            UserInfo anonUserInfo,
            UserInfo moderatorUserInfo) =>
            new List<ForumUserInfo>
            {
                BuildAnonymousUser(moduleId, portalSettings, anonUserInfo),
                BuildRegisteredUntrustedUser10(moduleId, portalSettings, user10Info),
                BuildRegisteredTrustedUser12(moduleId, portalSettings, user12Info),
                BuildModeratorUser(moduleId, portalSettings, moderatorUserInfo),
                BuildAdminUser(moduleId, portalSettings, adminUserInfo),
            };
    }
}
