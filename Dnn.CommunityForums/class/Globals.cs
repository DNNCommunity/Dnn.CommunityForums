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

    using DotNetNuke.Entities.Modules;

    #region Enumerations

    public enum AttachStores
    {
        FILESYSTEM,
        DATABASE,
    }

    public enum CurrentUserTypes
    {
        Anon,
        Auth,
        ForumMod,
        Admin,
        SuperUser,
    }

    public enum EditorTypes
    {
        TEXTBOX,
        ACTIVEEDITOR,
        HTMLEDITORPROVIDER,
    }

    public enum HTMLPermittedUsers
    {
        AllUsers,
        AuthenticatedUsers,
        TrustedUsers,
        Moderators,
        Administrators,
    }

    public enum AvatarTypes
    {
        LocalFile,
        ExternalLink,
        MultipleLocalFile,
        MultipleExternalLink,
    }

    public enum SubscriptionTypes
    {
        Disabled,
        Instant,
        DailyDigest,
        WeeklyDigest,
    }

    public enum TopicTypes
    {
        Topic,
        Poll,
    }

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
    public enum EmailFormats
    {
        HTML,
        PlainText,
    }

    public enum ProfileVisibilities
    {
        Disabled = 0,
        Everyone = 1,
        RegisteredUsers = 2,
        Moderators = 3,
        Admins = 4,
    }

    public enum PMTypes
    {
        Disabled = 0,
        Core = 1,
        Ventrian = 2,
    }

    public enum TrustTypes
    {
        NotTrusted,
        Trusted,
    }

    public enum ConfirmActions
    {
        TopicSaved,
        TopicDeleted,
        ReplySaved,
        ReplyDeleted,
        MessagePending,
        MessageMoved,
        MessageDeleted,
        SendToComplete,
        SendToFailed,
        AlertSent,
        UserBanned,
    }

    public class FilterTypes
    {
        public const string EMOTICON = "EMOTICON";
        public const string MARKUP = "MARKUP";
        public const string REGEX = "REGEX";
    }
    #endregion

    public class Globals
    {
        public static string DefaultAnonRoles
        {
            get
            {
                return string.Concat(Common.Globals.glbRoleAllUsers, ";", Common.Globals.glbRoleUnauthUser, ";");
            }
        }

        public const string ModuleName = "Active Forums";
        public const string ModuleFriendlyName = "DNN Community Forums";
        public const string ModulePath = "~/DesktopModules/ActiveForums/";
        public const string ModuleConfigPath = Globals.ModulePath + "config/";
        public const string DefaultTemplatePath = Globals.ModulePath + "config/templates/";
        public const string ModuleImagesPath = Globals.ModulePath + "images/";
        public const string TemplatesPath = Globals.ModulePath + "templates/";
        public const string ThemesPath = Globals.ModulePath + "themes/";

        public const string AdminResourceFile = Globals.ModulePath + "App_LocalResources/AdminResources.resx";
        public const string SharedResourceFile = Globals.ModulePath + "App_LocalResources/SharedResources.resx";
        public const string ControlPanelResourceFile = Globals.ModulePath + "App_LocalResources/ControlPanel.ascx.resx";
        public const string LegacyTokenResourceFile = Globals.ModulePath + "App_LocalResources/LegacyTokenResources.resx";
        public const string CacheDependencyFile = Globals.ModulePath + "cache/cachedep.resources";

        public const string ForumsControlsRegisterAMTag = "<%@ Register TagPrefix=\"am\" Namespace=\"DotNetNuke.Modules.ActiveForums.Controls\" Assembly=\"DotNetNuke.Modules.ActiveForums\" %>";
        public const string ForumsControlsRegisterAFTag = "<%@ Register TagPrefix=\"af\" Namespace=\"DotNetNuke.Modules.ActiveForums.Controls\" Assembly=\"DotNetNuke.Modules.ActiveForums\" %>";
        public const string SocialRegisterTag = "<%@ Register TagPrefix=\"social\" Namespace=\"Active.Modules.Social.Controls\" Assembly=\"Active.Modules.Social\" %>";
        public const string DnnControlsRegisterTag = "<%@ Register TagPrefix=\"dnn\" Assembly=\"DotNetNuke\" Namespace=\"DotNetNuke.UI.WebControls\"%>";
        public const string BannerRegisterTag = "<%@ Register TagPrefix=\"dnn\" TagName=\"BANNER\" Src=\"~/Admin/Skins/Banner.ascx\" %>";

        public const int GroupCount = 10000000;
        public const int ForumCount = 10000000;
        public const int SiteCount = -1;

        public const string DefaultDateFormat = "g";

        public const string ModerationNotificationType = "AF-ForumModeration";
        public const string ContentAlertNotificationType = "AF-ContentAlert";
        public const string BanUserNotificationType = "DCF-UserBanned";
        public const string BanUserNotificationTypeDescription = Globals.ModuleFriendlyName + " User Banned";
        public const string LikeNotificationType = "DCF-LikeNotification";
        public const string LikeNotificationTypeDescription = Globals.ModuleFriendlyName + " Like Notification";
        public const string PinNotificationType = "DCF-PinNotification";
        public const string PinNotificationTypeDescription = Globals.ModuleFriendlyName + " Pin Notification";

        public static Version ModuleVersion => new Version(DesktopModuleController.GetDesktopModuleByFriendlyName(Globals.ModuleFriendlyName).Version);
    }

    public class SettingKeys
    {
        public const string GeneralSettingsKey = "GEN";
        public const string Mode = "MODE";
        public const string PageSize = "PAGESIZE";
        public const string AllowSubscribe = "ALLOWSUBSCRIBE";
        public const string UserNameDisplay = "USERNAMEDISPLAY";
        public const string DisableUserProfiles = "DISABLEUSERPROFILES";
        public const string ProfileTabId = "PROFILETABID";
        public const string AllowAvatars = "ALLOWAVATARS";
        public const string AllowAvatarLinks = "ALLOWAVATARLINKS";
        public const string AvatarHeight = "AVATARHEIGHT";
        public const string AvatarWidth = "AVATARWIDTH";
        public const string AvatarDefault = "AVATARDEFAULT";
        public const string AllowSignatures = "ALLOWSIGNATURES";
        public const string StatsEnabled = "STATSENABLED";
        public const string StatsTemplate = "STATSTEMPLATE";
        public const string StatsCache = "STATSCACHE";
        public const string DateFormatString = "DATEFORMATSTRING";
        public const string TimeFormatString = "TIMEFORMATSTRING";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string TimeZoneOffset = "TIMEZONEOFFSET";
        public const string UsersOnlineEnabled = "USERSONLINEENABLED";
        public const string MemberListMode = "MEMBERLISTMODE";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string ForumTemplateId = "FORUMTEMPLATEID";
        public const string DisableAccountTab = "DISABLEACCOUNTTAB";
        public const string Theme = "THEME";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string MailQueue = "MAILQUEUE";
        public const string FullText = "FULLTEXT";
        public const string AllowSubTypes = "ALLOWSUBTYPES";
        public const string FloodInterval = "FLOODINTERVAL";
        public const string EditInterval = "EDITINTERVAL";
        public const string LoggingLevel = "LOGGINGLEVEL";
        public const string DeleteBehavior = "DELETEBEHAVIOR";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string ProdKey = "AMFORUMS";

        public const string EnablePoints = "ENABLEPOINTS";
        public const string TopicPointValue = "TOPICPOINTVALUE";
        public const string ReplyPointValue = "REPLYPOINTVALUE";
        public const string AnswerPointValue = "ANSWERPOINTVALUE";
        public const string ModPointValue = "MODPOINTVALUE";
        public const string MarkAnswerPointValue = "MARKANSWERPOINTVALUE";
        public const string PMType = "PMTYPE";
        public const string PMTabId = "PMTABID";
        public const string InstallDate = "INSTALLDATE";
        public const string IsInstalled = "INSTALLED";
        public const string ProfileVisibility = "PROFILEVISIBILITY";
        public const string UseShortUrls = "SHORTURLS";
        public const string RequireCaptcha = "REQCAPTCHA";
        public const string UseSkinBreadCrumb = "USESKINBC";
        public const string EnableAutoLink = "AUTOLINK";
        public const string EnableURLRewriter = "EURLR";
        public const string PrefixURLBase = "URLBASE";
        public const string PrefixURLTags = "URLTAGS";
        public const string PrefixURLCategories = "URLCATS";
        public const string PrefixURLLikes = "URLLIKES";
        public const string PrefixURLOther = "URLOTHER";

        public const string CacheTemplates = "CACHETEMPLATES";

        public const string DefaultSettingsKey = "DEFAULTSETTINGSKEY";
        public const string DefaultPermissionId = "DEFAULTPERMISSIONID";
    }

    public class ForumSettingKeys
    {
        public const string AllowHTML = "ALLOWHTML";
        public const string AllowScript = "ALLOWSCRIPT";
        public const string AllowSubscribe = "ALLOWSUBSCRIBE";
        public const string AllowEmoticons = "ALLOWEMOTICONS";
        public const string AllowPostIcon = "ALLOWPOSTICON";
        public const string EditorType = "EDITORTYPE";
        public const string EditorWidth = "EDITORWIDTH";
        public const string EditorHeight = "EDITORHEIGHT";
        public const string EditorToolbar = "EDITORTOOLBAR";
        public const string EditorStyle = "EDITORSTYLE";
        public const string EditorPermittedUsers = "EDITORPERMITTEDUSERS";
        public const string EditorMobile = "EDITORMOBILE";
        public const string AttachCount = "ATTACHCOUNT";
        public const string AttachMaxSize = "ATTACHMAXSIZE";
        public const string AttachTypeAllowed = "ATTACHTYPEALLOWED";
        public const string AttachAllowBrowseSite = "ATTACHALLOWBROWSESITE";
        public const string AttachMaxHeight = "ATTACHMAXHEIGHT";
        public const string AttachMaxWidth = "ATTACHMAXWIDTH";
        public const string MaxAttachWidth = "MAXATTACHWIDTH";
        public const string MaxAttachHeight = "MAXATTACHHEIGHT";
        public const string AttachInsertAllowed = "ATTACHINSERTALLOWED";
        public const string ConvertingToJpegAllowed = "CONVERTINGTOJPEGALLOWED";
        public const string IndexContent = "INDEXCONTENT";
        public const string AllowRSS = "ALLOWRSS";
        public const string IsModerated = "ISMODERATED";
        public const string AutoTrustLevel = "AUTOTRUSTLEVEL";
        public const string DefaultTrustLevel = "DEFAULTTRUSTLEVEL";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string ModApproveTemplateId = "MODAPPROVETEMPLATEID";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string ModRejectTemplateId = "MODREJECTTEMPLATEID";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string ModMoveTemplateId = "MODMOVETEMPLATEID";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string ModDeleteTemplateId = "MODDELETETEMPLATEID";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string ModNotifyTemplateId = "MODNOTIFYTEMPLATEID";
        public const string EmailAddress = "EMAILADDRESS";
        public const string UseFilter = "USEFILTER";
        public const string AllowAttach = "ALLOWATTACH";
        public const string AutoSubscribeEnabled = "AUTOSUBSCRIBEENABLED";
        public const string AutoSubscribeRoles = "AUTOSUBSCRIBEROLES";
        public const string AutoSubscribeNewTopicsOnly = "AUTOSUBSCRIBENEWTOPICSONLY";
        public const string AllowTags = "ALLOWTAGS";
        public const string CreatePostCount = "CREATEPOSTCOUNT";
        public const string ReplyPostCount = "REPLYPOSTCOUNT";
        public const string AllowLikes = "ALLOWLIKES";
        public const string TemplateFileNameSuffix = "TEMPLATEFILENAMESUFFIX";

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string TopicsTemplateId = "TOPICSTEMPLATEID";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string TopicTemplateId = "TOPICTEMPLATEID";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string TopicFormId = "TOPICFORMID";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string ReplyFormId = "REPLYFORMID";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string QuickReplyFormId = "QUICKREPLYFORMID";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string ProfileTemplateId = "PROFILETEMPLATEID";
        public const string EmailNotificationSubjectTemplate = "EMAILNOTIFICATIONSUBJECTTEMPLATE";

        public const string ModApproveNotify = "MODAPPROVENOTIFY";
        public const string ModRejectNotify = "MODREJECTNOTIFY";
        public const string ModMoveNotify = "MODMOVENOTIFY";
        public const string ModDeleteNotify = "MODDELETETENOTIFY";
        public const string ModAlertNotify = "MODALERTNOTIFY";

        /*
        public const string MCEnabled = "MCENABLED";
        public const string MCUrl = "MCURL";
        public const string MCAddress = "MCADDRESS";
        public const string MCRestrictByAlias = "MCRESTRICTALIAS";
        public const string MCPop3UserName = "MCPOPUSERNAME";
        public const string MCPop3Password = "MCPOPPASSWORD";
        public const string MCPop3Server = "MCPOPSERVER";
        public const string MCAutoResponseTemplateId = "MCAUTORESPONSE";
        public const string MCAdminNotifyTemplateId = "MCADMINNOTIFY";
        public const string MCSubNotifyTemplateId = "MCSUBNOTIFY";
        public const string MCRejectTemplateId = "MCREJECTNOTIFY";
        public const string MCAutoCreateUsers = "MCAUTOCREATEUSERS";
        public const string MCModType = "MCMODTYPE";
        public const string MCEOMTag = "MCEOMTAG";
        public const string MCEOMTagRequired = "MCEOMTAGREQ";
        public const string MCRemoveHTML = "MCSTRIPHTML";
        */
    }

    public class SearchParamKeys
    {
        public const string Tag = "tg";
        public const string Query = "q";
        public const string SearchType = "k";
        public const string User = "uid";
        public const string Author = "author";
        public const string Search = "sid";
        public const string Sort = "srt";
        public const string ResultType = "rt";
        public const string TimeSpan = "ts";
        public const string Columns = "c";
        public const string Forums = "f";
    }

    public class ParamKeys
    {
        public const string ForumId = "aff";
        public const string GroupId = "afg";
        public const string TopicId = "aft";
        public const string ReplyId = "afr";
        public const string ViewType = "afv";
        public const string QuoteId = "afq";
        public const string PageId = "afpg";
        public const string PostId = "postid";
        public const string ContentId = "contentid";
        public const string UserId = "uid";
        public const string Sort = "afs";
        public const string PageJumpId = "afpgj";
        public const string ContentJumpId = "afc";
        public const string ConfirmActionId = "afca";
        public const string Tags = "aftg";
        public const string FirstNewPost = "afnp";
        public const string AuthorId = "authorid";
        public const string GridType = "afgt";
        public const string Category = "act";
        public const string Action = "action";
        public const string TimeSpan = "ts";
        public const string Mode = "mode";
    }

    public class Modes
    {
        public const string Edit = "edit";
    }

    public class Literals
    {
        public const string Page = "page";
        public const string View = "view";
        public const string ForumId = "ForumId";
        public const string GroupId = "GroupId";
        public const string TopicId = "TopicId";
        public const string ReplyId = "ReplyId";
        public const string UserId = "UserId";
        public const string PostId = "PostId";
        public const string PageId = "PageId";
    }

    public class SortOptions
    {
        public const string Descending = "DESC";
        public const string Ascending = "ASC";
    }

    public class Views
    {
        public const string Topics = "topicsview";
        public const string Topic = "topic";
        public const string ForumView = "forumview";
        public const string TopicNew = "topicnew";
        public const string TopicEdit = "topicedit";
        public const string Grid = "grid";
        public const string Tags = "tags";
        public const string Post = "post";
        public const string Search = "search";
        public const string Profile = "profile";
        public const string MyPreferences = "afprofile";
        public const string MySubscriptions = "afsubscriptions";
        public const string ModerateTopics = "modtopics";
        public const string ModerateBan = "modban";
        public const string ModerateReport = "modreport";
        public const string Likes = "likes";
    }

    internal static class GridTypes
    {
        public const string NotRead = "notread";
        public const string Unanswered = "unanswered";
        public const string ActiveTopics = "activetopics";
        public const string MyTopics = "mytopics";
        public const string MySettings = "afprofile";
        public const string MySubscriptions = "afsubscriptions";
        public const string MostLiked = "mostliked";
        public const string MostReplies = "mostreplies";
        public const string Unresolved = "unresolved";
        public const string Announcements = "announcements";
        public const string Tags = "tags";
    }

    public class PostActions
    {
        public const string TopicEdit = "te";
        public const string ReplyEdit = "re";
        public const string Reply = "reply";
    }

    public class CacheKeys
    {
        public const string CachePrefix = "AF-";
        public const string CacheModulePrefix = "AF-{0}-";
        public const string UserProfile = "AF-{0}-prof-{1}";
        public const string ForumUser = "AF-{0}-user-{1}";
        public const string Rewards = "AF-{0}-rwd";
        public const string ProfileInfo = "AF-{0}-pi";
        public const string ForumInfo = "AF-{0}-fi-{1}";
        public const string ForumInfoWithUser = "AF-{0}-fi-{1}-{2}";
        public const string HostUrl = "AF-{0}-url";
        public const string MainSettings = "AF-{0}-ms";
        public const string DefaultSettingsByKey = "AF-{0}-dsk";
        public const string ForumSettingsByKey = "AF-{0}-fsk-{1}";
        public const string GroupSettingsByKey = "AF-{0}-gsk-{1}";
        public const string ForumList = "AF-{0}-fl";
        public const string SubForumList = "AF-{0}-sfl-{1}";
        public const string ForumListXml = "AF-{0}-flx";
        public const string Tokens = "AF-{0}-tk-{1}";
        public const string ForumViewPrefix = "AF-{0}-FV-";
        public const string ForumViewForUser = "AF-{0}-FV-{1}-{2}-{3}";
        public const string TopicViewPrefix = "AF-{0}-TV-";
        public const string TopicViewForUser = "AF-{0}-TV-{1}-{2}-{3}-{4}-{5}";
        public const string TopicsViewPrefix = "AF-{0}-TVS-";
        public const string TopicsViewForUser = "AF-{0}-TVS-{1}-{2}-{3}-{4}-{5}";
        public const string ForumViewTemplate = "AF-{0}-fvt-{1}";
        public const string Toolbar = "AF-{0}-tb-{1}-{2}";
        public const string TemplatePrefix = "AF-{0}-tmpl-";
        public const string Template = "AF-{0}-tmpl-{1}-{2}";
        public const string QuickReply = "AF-{0}-qr";
        public const string CacheEnabled = "AF-{0}-ce";
        public const string CachingTime = "AF-{0}-ct";
        public const string CacheUpdate = "AF-{0}-cu";
        public const string WhatsNew = "AF-{0}-tp";
        public const string RssTemplate = "AF-{0}-tprss-_{1}";
        public const string ViewRolesForForum = "AF-{0}-CanView-{1}";
        public const string ViewRolesForForumList = "AF-{0}-Perm-{1}";
        public const string Subscriber = "AF-{0}-Subs-{1}-{2}-{3}-{4}";
        public const string ForumSettings = "AF-{0}-fs-{1}";

        public const string ForumTopicInfo = "AF-{0}-forumtopicinfo-{1}";
        public const string ForumTopicInfoPrefix = "AF-{0}-forumtopicinfo";
        public const string ForumGroupInfo = "AF-{0}-fgi-{1}";
        public const string ForumGroupSettings = "AF-{0}-fgs-{1}";
        public const string PermissionsInfo = "AF-{0}-perms-{1}";

        public const string ContentInfo = "AF-{0}-ci-{1}";
        public const string TopicInfo = "AF-{0}-ti-{1}";
        public const string TopicInfoByContentId = "AF-{0}-tci-{1}";
        public const string ReplyInfo = "AF-{0}-ri-{1}";
        public const string ReplyInfoByContentId = "AF-{0}-rci-{1}";
        public const string LikeInfo = "AF-{0}-like-{1}";
        public const string LikeCount = "AF-{0}-lc-{1}";
        public const string LikedByUser = "AF-{0}-lbu-{1}-{2}";
        public const string ForumSubscriberPrefix = "AF-{0}-fsub-{1}";
        public const string ForumSubscriber = "AF-{0}-fsub-{1}-{2}";
        public const string ForumSubscriberCount = "AF-{0}-fsubcount-{1}";
        public const string TopicSubscriberPrefix = "AF-{0}-tsub-{1}";
        public const string TopicSubscriber = "AF-{0}-tsub-{1}-{2}-{3}";
        public const string TopicSubscriberCount = "AF-{0}-tsub-{1}-{2}";
        public const string TopicSubscriberCountPrefix = "AF-{0}-tsub-{1}";
        public const string TopicTrackingInfoPrefix = "AF-{0}-tti-{1}";
        public const string TopicTrackingInfo = "AF-{0}-tti-{1}-{2}";
        public const string ForumTrackingInfoPrefix = "AF-{0}-fti-{1}";
        public const string ForumTrackingInfo = "AF-{0}-fti-{1}-{2}";
        public const string TopicReadCountPrefix = "AF-{0}-trc-{1}";
        public const string TopicReadCount = "AF-{0}-trc-{1}-{2}";

        public const string RoleNames = "AF-rn-{0}";
        public const string RoleIDs = "AF-rids-{0}";
        public const string Roles = "AF-roles-{0}";
        public const string UserRoles = "AF-userroles-{0}";
        public const string CultureInfoForUser = "AF-usercultureinfo-{0}";
        public const string TimeZoneInfoForUser = "AF-usertimezoneinfo-{0}";
    }

    public class SortColumns
    {
        public const string ReplyCreated = "ReplyCreated";
        public const string TopicCreated = "TopicCreated";
    }

    public class ForumViewerSettingsKeys
    {
        public const string AFForumModuleId = "AFForumModuleID";
        public const string AFForumGroupId = "AFForumGroupID";
        public const string AFForumGroup = "AFForumGroup";
        public const string AFViewType = "AFViewType";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string AFTopicsTemplate = "AFTopicsTemplate";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string AFForumViewTemplate = "AFForumViewTemplate";
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public const string AFTopicTemplate = "AFTopicTemplate";
    }

    public class ForumViewerViewType
    {
        public const string GROUP = "AFGROUP";
        public const string TOPICS = "TOPICS";
    }
}
