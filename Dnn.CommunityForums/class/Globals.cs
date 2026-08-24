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

    public enum CurrentUserTypes
    {
        Anon,
        Auth,
        ForumMod,
        Admin,
        SuperUser,
    }

    public enum HTMLPermittedUsers
    {
        AllUsers,
        AuthenticatedUsers,
        TrustedUsers,
        Moderators,
        Administrators,
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
        public const string ModuleOwnerName = "DNN Community";
        public const string ModuleFriendlyName = "DNN Community Forums";
        public const string ModulePath = "~/DesktopModules/ActiveForums/";
        public const string ModuleAbsolutePath = "/DesktopModules/ActiveForums/";
        public const string ModuleConfigPath = Globals.ModulePath + "config/";
        public const string DefaultTemplatePath = Globals.ModulePath + "config/templates/";
        public const string ModuleImagesPath = Globals.ModulePath + "images/";
        public const string TemplatesPath = Globals.ModulePath + "templates/";
        public const string ThemesPath = Globals.ModulePath + "themes/";

        public const string AvatarRefreshGravatar = "GRAVATAR";
        public const string LegacyAvatarsFolderName = "communityforums_Avatars";

        public const string DefaultBadgesFolderName = "DNNCommunityForums/Badges";
        public const string ContentFolderNameBase = "DNNCommunityForums/content/";
        public const string ContentFolderNameFormatString = "DNNCommunityForums/content/{0}/{1}";
        public const string EmbeddedImagesFolderNameFormatString = "DNNCommunityForums/content/{0}/{1}/images/";
        public const string AttachmentsFolderNameFormatString = "DNNCommunityForums/content/{0}/{1}/attachments/";
        public const string LegacyAttachmentFileNameFormatString = "__{0}__{1}__{2}";
        public const string LegacyAttachmentsFolderName = "communityforums_Attach";
        public const string LegacyAttachmentUploadsFolderName = "communityforums_Upload";
        public const string AttachmentUploadsFolderName = "DNNCommunityForums/upload";
        public const string AttachmentFileNameFormatString = "{0}_{1}{2}";

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
        public const string BadgeNotificationType = "DCF-BadgeNotification";
        public const string BadgeNotificationTypeDescription = Globals.ModuleFriendlyName + " Badge Notification";
        public const string UserMentionNotificationType = "DCF-UserMentionNotification";
        public const string UserMentionNotificationTypeDescription = Globals.ModuleFriendlyName + " User Mention Notification";

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
        public const string AvatarRefresh = "AVATARREFRESH";
        public const string AvatarHeight = "AVATARHEIGHT";
        public const string AvatarWidth = "AVATARWIDTH";
        public const string AllowSignatures = "ALLOWSIGNATURES";
        public const string StatsEnabled = "STATSENABLED";
        public const string StatsTemplate = "STATSTEMPLATE";
        public const string StatsCache = "STATSCACHE";
        public const string DateFormatString = "DATEFORMATSTRING";
        public const string TimeFormatString = "TIMEFORMATSTRING";
        public const string UsersOnlineEnabled = "USERSONLINEENABLED";
        public const string MemberListMode = "MEMBERLISTMODE";
        public const string DisableAccountTab = "DISABLEACCOUNTTAB";
        public const string Theme = "THEME";
        public const string AllowSubTypes = "ALLOWSUBTYPES";
        public const string FloodInterval = "FLOODINTERVAL";
        public const string EditInterval = "EDITINTERVAL";
        public const string LoggingLevel = "LOGGINGLEVEL";
        public const string DeleteBehavior = "DELETEBEHAVIOR";
        public const string EnablePoints = "ENABLEPOINTS";
        public const string TopicPointValue = "TOPICPOINTVALUE";
        public const string ReplyPointValue = "REPLYPOINTVALUE";
        public const string AnswerPointValue = "ANSWERPOINTVALUE";
        public const string ModPointValue = "MODPOINTVALUE";
        public const string MarkAnswerPointValue = "MARKANSWERPOINTVALUE";
        public const string PMType = "PMTYPE";
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

        public const string SocialGroupModeForumConfig = "ForumConfig";
        public const string SocialGroupModeForumGroupTemplate= "ForumGroupTemplate";
    }

    public class ForumSettingKeys
    {
        public const string AllowHTML = "ALLOWHTML";
        public const string AllowScript = "ALLOWSCRIPT";
        public const string AllowSubscribe = "ALLOWSUBSCRIBE";
        public const string AllowEmoticons = "ALLOWEMOTICONS";
        public const string AllowPostIcon = "ALLOWPOSTICON";
        public const string EditorType = "EDITORTYPE";
        public const string EditorPermittedUsers = "EDITORPERMITTEDUSERS";
        public const string AttachCount = "ATTACHCOUNT";
        public const string AttachMaxSize = "ATTACHMAXSIZE";
        public const string AttachTypeAllowed = "ATTACHTYPEALLOWED";
        public const string AttachAllowBrowseSite = "ATTACHALLOWBROWSESITE";
        public const string MaxImageWidth = "MAXIMAGEWIDTH";
        public const string MaxImageHeight = "MAXIMAGEHEIGHT";
        public const string IndexContent = "INDEXCONTENT";
        public const string AllowRSS = "ALLOWRSS";
        public const string IsModerated = "ISMODERATED";
        public const string AutoTrustLevel = "AUTOTRUSTLEVEL";
        public const string DefaultTrustLevel = "DEFAULTTRUSTLEVEL";
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

        public const string EmailNotificationSubjectTemplate = "EMAILNOTIFICATIONSUBJECTTEMPLATE";

        public const string UserMentions = "USERMENTIONS";
        public const string UserMentionVisibility = "USERMENTIONVISIBILITY";

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
        public const string User = "uid";
        public const string Author = "author";
        public const string Search = "sid";
        public const string Sort = "srt";
        public const string ResultType = "rt";
        public const string TimeSpan = "ts";
        public const string Forums = "f";
    }

    public static class ParamKeys
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
        public const string BadgeId = "badgeid";
        public const string Message = "afmsg";
        public const string PortalId = "PortalId";
        public const string ModuleId = "ModuleId";
        public const string TabId = "TabId";
    }

    public class ModuleModes
    {
        public const string Standard = "Standard";
        public const string SocialGroup = "SocialGroup";
    }

    public class Modes
    {
        public const string Edit = "edit";
        public const string DnnPrintMode = "dnnprintmode";
    }

    public class MessageTypes
    {
        public const string Move = "Move";
        public const string ModAlert = "ModAlert";
        public const string PendingModeration = "PendingModeration";
        public const string EmailSent = "EmailSent";
        public const string PostSubmit = "PostSubmit";
    }

    public class Literals
    {
        public const string Page = "Page";
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
        public const string RecycleBin = "recyclebin";
        public const string BadgeUsers = "badgeusers";
        public const string UserBadges = "userbadges";
        public const string SendTo = "sendto";
        public const string ConfirmAction = "confirmaction";
        public const string tag = "tag";
        public const string likes = "likes";
        public const string category = "category";
        public const string views = "views";
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
        public const string RecycleBin = "recyclebin";
        public const string BadgeUsers = "badgeusers";
        public const string UserBadges = "userbadges";
    }

    public class PostActions
    {
        public const string TopicEdit = "te";
        public const string ReplyEdit = "re";
        public const string Reply = "reply";
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
        public const string AFTheme = "AFTheme";
    }

    public class ForumViewerViewType
    {
        public const string GROUP = "AFGROUP";
        public const string TOPICS = "TOPICS";
    }
}
