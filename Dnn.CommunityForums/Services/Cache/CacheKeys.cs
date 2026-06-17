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

using System;

namespace DotNetNuke.Modules.ActiveForums.Services.Cache
{
    internal static class CacheKeys
    {
        internal const string CachePrefix = "AF-";
        internal const string CacheModulePrefix = "AF-{0}-";
        internal const string UserProfile = "AF-{0}-prof-{1}";
        internal const string ForumUser = "AF-{0}-user-{1}";

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Ranks")]
        internal const string Rewards = "AF-{0}-rwd";

        internal const string RunningInViewer = "AF-{0}-inviewer-{1}";
        internal const string ForumModuleId = "AF-{0}-forummoduleid-{1}";
        internal const string ProfileInfo = "AF-{0}-pi";
        internal const string ForumInfo = "AF-{0}-fi-{1}";
        internal const string ForumInfoWithUser = "AF-{0}-fi-{1}-{2}";
        internal const string HostUrl = "AF-{0}-url";
        internal const string MainSettings = "AF-{0}-ms";
        internal const string TabModuleSettings = "AF-{0}-tms";
        internal const string DefaultSettingsByKey = "AF-{0}-dsk";
        internal const string ForumSettingsByKey = "AF-{0}-fsk-{1}";
        internal const string GroupSettingsByKey = "AF-{0}-gsk-{1}";
        internal const string ForumList = "AF-{0}-fl";
        internal const string SubForumList = "AF-{0}-sfl-{1}";
        internal const string ForumListXml = "AF-{0}-flx";
        internal const string Tokens = "AF-{0}-tk-{1}";
        internal const string ForumViewPrefix = "AF-{0}-FV-";
        internal const string ForumViewForUser = "AF-{0}-FV-{1}-{2}-{3}-{4}";
        internal const string TopicViewPrefix = "AF-{0}-TV-";
        internal const string TopicViewForUser = "AF-{0}-TV-{1}-{2}-{3}-{4}-{5}";
        internal const string TopicsViewPrefix = "AF-{0}-TVS-";
        internal const string TopicsViewForUser = "AF-{0}-TVS-{1}-{2}-{3}-{4}-{5}";
        internal const string ForumViewTemplate = "AF-{0}-fvt-{1}";
        internal const string Toolbar = "AF-{0}-tb-{1}-{2}";
        internal const string ToolbarPrefix = "AF-{0}-tb-";
        internal const string TemplatePrefix = "AF-{0}-tmpl-";
        internal const string Template = "AF-{0}-tmpl-{1}-{2}-{3}";
        internal const string QuickReply = "AF-{0}-qr";
        internal const string CacheEnabled = "AF-{0}-ce";
        internal const string CachingTime = "AF-{0}-ct";
        internal const string CacheUpdate = "AF-{0}-cu";
        internal const string WhatsNew = "AF-{0}-tp";
        internal const string WhatsNewData = "AF-{0}-tprssdata-{1}-{2}-{3}-{4}-{5}";
        internal const string RssTemplate = "AF-{0}-tprss-{1}";
        internal const string ViewRolesForForumList = "AF-{0}-Perm-{1}";
        internal const string Subscriber = "AF-{0}-Subs-{1}-{2}-{3}-{4}";
        internal const string ForumSettings = "AF-{0}-fs-{1}";

        internal const string ForumTopicInfo = "AF-{0}-forumtopicinfo-{1}";
        internal const string ForumTopicInfoPrefix = "AF-{0}-forumtopicinfo";
        internal const string ForumGroupInfo = "AF-{0}-fgi-{1}";
        internal const string ForumGroupSettings = "AF-{0}-fgs-{1}";
        internal const string PermissionsInfo = "AF-{0}-perms-{1}";
        internal const string BadgeInfo = "AF-{0}-badge-{1}";
        internal const string UserBadgeInfo = "AF-{0}-userbadge-{1}";
        internal const string UserBadges = "AF-{0}-userbadges1-{1}";
        internal const string UserBadgesDistinct = "AF-{0}-userbadges2-{1}";
        internal const string BadgeUsers = "AF-{0}-badgeusers-{1}";
        internal const string BadgeUserCount = "AF-{0}-badgeusercount-{1}-{2}";
        internal const string Badges = "AF-{0}-badges";
        internal const string Ranks = "AF-{0}-ranks";
        internal const string RankInfo = "AF-{0}-rank-{1}";
        internal const string AttachmentInfoByContentId = "AF-{0}-aic-{1}";
        internal const string ContentInfo = "AF-{0}-ci-{1}";
        internal const string TopicInfo = "AF-{0}-ti-{1}";
        internal const string TopicInfoByContentId = "AF-{0}-tci-{1}";
        internal const string ReplyInfo = "AF-{0}-ri-{1}";
        internal const string ReplyInfoByContentId = "AF-{0}-rci-{1}";
        internal const string LikeInfo = "AF-{0}-like-{1}";
        internal const string LikeCount = "AF-{0}-lc-{1}";
        internal const string LikedByUser = "AF-{0}-lbu-{1}-{2}";
        internal const string ForumSubscriberPrefix = "AF-{0}-fsub-{1}";
        internal const string ForumSubscriber = "AF-{0}-fsub-{1}-{2}";
        internal const string ForumSubscriberCount = "AF-{0}-fsubcount-{1}";
        internal const string TopicSubscriberPrefix = "AF-{0}-tsub-{1}";
        internal const string TopicSubscriber = "AF-{0}-tsub-{1}-{2}-{3}";
        internal const string TopicSubscriberCount = "AF-{0}-tsub-{1}-{2}";
        internal const string TopicSubscriberCountPrefix = "AF-{0}-tsub-{1}";
        internal const string TopicTrackingInfoPrefix = "AF-{0}-tti-{1}";
        internal const string TopicTrackingInfo = "AF-{0}-tti-{1}-{2}";
        internal const string ForumTrackingInfoPrefix = "AF-{0}-fti-{1}";
        internal const string ForumTrackingInfo = "AF-{0}-fti-{1}-{2}";
        internal const string TopicReadCountPrefix = "AF-{0}-trc-{1}";
        internal const string TopicReadCount = "AF-{0}-trc-{1}-{2}";

        internal const string RoleNames = "AF-rn-{0}";
        internal const string RoleIDs = "AF-rids-{0}";
        internal const string Roles = "AF-roles-{0}";
        internal const string UserRoles = "AF-userroles-{0}";
        internal const string CultureInfoForUser = "AF-usercultureinfo-{0}";
        internal const string TimeZoneInfoForUser = "AF-usertimezoneinfo-{0}";
        internal const string UserMentionQuery = "AF-{0}-usermentionquery-{1}-{2}";
        internal const string UserMentionInfo = "AF-{0}-usermentioninfo-{1}";
        internal const string TagMatches = "AF-{0}-tagmatches-{1}";
        internal const string SearchQuery = "AF-{0}-searchquery-{1}";
        internal const string TagByName = "AF-{0}-tagname-{1}";
        internal const string CategoryByName = "AF-{0}-categoryname-{1}";
        internal const string ForumGroupByUrlPrefix = "AF-{0}-fgurl-{1}";
        internal const string ForumByUrlPrefix = "AF-{0}-furl-{1}";
        internal const string TopicByUrl = "AF-{0}-turl-{1}-{2}";
        internal const string PortalAliases = "AF-pa";
        internal const string TabPaths = "AF-tabpaths-{0}";
        internal const string UrlRewrites = "AF-urlrw-{0}";
        internal const string ArchivedUrl = "AF-{0}-archurl-{1}";

        internal const string FilteredTopicsPrefix = "AF-{0}-ft-";
        internal const string TopicAnnouncements = "AF-{0}-ft-ann-{1}-{2}-{3}";
        internal const string TopicAnnouncementsCount = "AF-{0}-ft-ann-count-{1}";
        internal const string MostLikes = "AF-{0}-ft-most-likes-{1}-{2}-{3}-{4}";
        internal const string MostLikesCount = "AF-{0}-ft-most-likes-count-{1}-{2}";
        internal const string TopicMostReplies = "AF-{0}-ft-most-replies-{1}-{2}-{3}-{4}";
        internal const string TopicMostRepliesCount = "AF-{0}-ft-most-replies-count-{1}-{2}";
        internal const string TopicUnresolved = "AF-{0}-ft-unresolved-{1}-{2}-{3}-{4}";
        internal const string TopicUnresolvedCount = "AF-{0}-ft-unresolved-count-{1}-{2}";
        internal const string TopicUnanswered = "AF-{0}-ft-unanswered-{1}-{2}-{3}-{4}";
        internal const string TopicUnansweredCount = "AF-{0}-ft-unanswered-count-{1}-{2}";
        internal const string TaggedTopics = "AF-{0}-ft-tagged-{1}-{2}-{3}-{4}-{5}";
        internal const string TaggedTopicsCount = "AF-{0}-ft-tagged-count-{1}-{2}-{3}";
        internal const string TopicUnread = "AF-{0}-ft-unread-{1}-{2}-{3}-{4}-{5}";
        internal const string TopicUnreadCount = "AF-{0}-ft-unread-count-{1}-{2}-{3}";
        internal const string ActiveTopics = "AF-{0}-ft-active-{1}-{2}-{3}-{4}";
        internal const string ActiveTopicsCount = "AF-{0}-ft-active-count-{1}-{2}";
        internal const string MyTopics = "AF-{0}-ft-mytopics-{1}-{2}-{3}-{4}-{5}";
        internal const string MyTopicsCount = "AF-{0}-ft-mytopics-count-{1}-{2}-{3}";

        internal const string FirstTabIdForModule = "AF-{0}-ftab";
    }
}
