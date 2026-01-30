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

using System.Linq;
using System.Text;

using DotNetNuke.Collections;

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.UI.UserControls;

    [TableName("activeforums_UserProfiles")]
    [PrimaryKey("ProfileId", AutoIncrement = true)]
    [Scope("PortalId")]
    public class ForumUserInfo : DotNetNuke.Services.Tokens.IPropertyAccess
    {
        [IgnoreColumn]
        private string cacheKeyTemplate => CacheKeys.ForumUser;

        private DotNetNuke.Entities.Users.UserInfo userInfo;
        private IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> userBadges;
        private PortalSettings portalSettings;
        private DotNetNuke.Modules.ActiveForums.ModuleSettings moduleSettings;
        private ModuleInfo moduleInfo;
        private HashSet<int> userRoleIds;
        private string userPermSet;

        public ForumUserInfo()
        {
        }

        public ForumUserInfo(int moduleId, DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Entities.Users.UserInfo userInfo)
        {
            this.portalSettings = portalSettings;
            this.PortalId = portalSettings.PortalId;
            this.ModuleId = moduleId;
            this.userInfo = userInfo;
        }

        public ForumUserInfo(int moduleId, DotNetNuke.Entities.Portals.PortalSettings portalSettings)
        {
            this.portalSettings = portalSettings;
            this.PortalId = portalSettings.PortalId;
            this.ModuleId = moduleId;
            this.userInfo = new DotNetNuke.Entities.Users.UserInfo();
        }

        public ForumUserInfo(int moduleId)
        {
            this.userInfo = new DotNetNuke.Entities.Users.UserInfo();
            this.ModuleId = moduleId;
        }

        public ForumUserInfo(int moduleId, DotNetNuke.Entities.Users.UserInfo userInfo)
        {
            this.userInfo = userInfo;
            this.ModuleId = moduleId;
        }

        public int ProfileId { get; set; }

        public int UserId { get; set; } = -1;

        public int PortalId { get; set; }

        [IgnoreColumn]
        internal int ModuleId { get; set; } = -1;

        public int TopicCount { get; set; }

        public int ReplyCount { get; set; } 

        public int ViewCount { get; set; }

        public int AnswerCount { get; set; }

        public int RewardPoints { get; set; }

        public string UserCaption { get; set; }

        public DateTime? AvatarLastRefresh { get; set; }

        public DateTime? AvatarSourceLastModified { get; set; }

        public int? AvatarFileId { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? DateUpdated { get; set; }

        public DateTime? DateLastActivity { get; set; }

        public DateTime? DateLastPost { get; set; }

        public DateTime? DateLastReply { get; set; }

        [IgnoreColumn]
        public DateTime? DNNUserDateCreated => this.UserInfo?.CreatedOnDate;

        [IgnoreColumn]
        public DateTime? DNNUserDateUpdated => this.UserInfo?.LastModifiedOnDate;


        public string Signature { get; set; }

        public bool SignatureDisabled { get; set; }

        public int TrustLevel { get; set; }

        public bool AdminWatch { get; set; }

        public bool AttachDisabled { get; set; }

        [IgnoreColumn]
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public string Avatar { get; set; }

        [IgnoreColumn]
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public AvatarTypes AvatarType { get; set; }

        public bool AvatarDisabled { get; set; }

        public string PrefDefaultSort { get; set; } = "ASC";

        public bool PrefDefaultShowReplies { get; set; }

        public bool PrefJumpLastPost { get; set; }

        public bool PrefTopicSubscribe { get; set; }

        public SubscriptionTypes PrefSubscriptionType { get; set; }

        public bool PrefBlockAvatars { get; set; }

        public bool PrefBlockSignatures { get; set; }

        public int PrefPageSize { get; set; } = 20;

        public bool LikeNotificationsEnabled { get; set; } = true;

        public bool PinNotificationsEnabled { get; set; } = true;

        public bool EnableNotificationsForOwnContent { get; set; } = false;

        public bool BadgeNotificationsEnabled { get; set; } = true;

        public bool UserMentionNotificationsEnabled { get; set; } = true;

        [IgnoreColumn]
        public string RawUrl { get; set; }

        [IgnoreColumn]
        public Uri RequestUri { get; set; }

        [IgnoreColumn]
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public string[] Roles => this.UserInfo?.Roles;

        [IgnoreColumn]
        public string FirstName => this.UserInfo?.FirstName;

        [IgnoreColumn]
        public string LastName => string.IsNullOrEmpty(this.UserInfo?.LastName) ? string.Empty : this.UserInfo?.LastName;

        [IgnoreColumn]
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public string FullName => string.Concat(this.UserInfo?.FirstName, " ", this.UserInfo?.LastName);

        [IgnoreColumn]
        public string DisplayName => string.IsNullOrEmpty(this.UserInfo?.DisplayName) == null ? string.Empty : this.UserInfo?.DisplayName;

        [IgnoreColumn]
        public string Username => this.UserInfo?.Username;

        [IgnoreColumn]
        public string Email => string.IsNullOrEmpty(this.UserInfo?.Email) ? string.Empty : this.UserInfo?.Email;

        [IgnoreColumn]
        public bool GetIsMod(int ModuleId)
        {
            return !this.IsAnonymous && !string.IsNullOrEmpty(DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.PortalId, ModuleId, this, DotNetNuke.Modules.ActiveForums.SecureActions.Moderate));
        }

        [IgnoreColumn]
        public bool IsSuperUser => this.UserInfo != null && this.UserInfo.IsSuperUser;

        [IgnoreColumn]
        public bool IsAdmin => this.UserInfo != null && this.UserInfo.IsAdmin;

        [IgnoreColumn]
        public bool IsAuthenticated => !this.IsAnonymous;

        [IgnoreColumn]
        public bool IsAnonymous => this.UserId.Equals(DotNetNuke.Common.Utilities.Null.NullInteger);

        [IgnoreColumn]
        public bool IsUserOnline => this.DateLastActivity > DateTime.UtcNow.AddMinutes(-5);

        [IgnoreColumn] 
        public bool IsRegistered => this.UserInfo.IsInRole(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredUsersRoleName(this.PortalSettings));

        [IgnoreColumn]
        public CurrentUserTypes CurrentUserType
        {
            get
            {
                if (this.UserInfo.IsSuperUser)
                {
                    return CurrentUserTypes.SuperUser;
                }

                if (this.UserInfo.IsAdmin)
                {
                    return CurrentUserTypes.Admin;
                }

                if (this.GetIsMod(this.ModuleId))
                {
                    return CurrentUserTypes.ForumMod;
                }

                if (this.IsAuthenticated)
                {
                    return CurrentUserTypes.Auth;
                }

                return CurrentUserTypes.Anon;
            }
        }

        [IgnoreColumn]
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used")]
        public string ForumsAllowed { get; set; }

        [IgnoreColumn]
        public string UserForums { get; set; }

        [IgnoreColumn]
        public int PostCount => this.TopicCount + this.ReplyCount;

        [IgnoreColumn]
        public DotNetNuke.Entities.Profile.ProfilePropertyDefinitionCollection Properties => this.UserInfo?.Profile?.ProfileProperties;

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        [IgnoreColumn]
        TimeSpan TimeZoneOffsetForUser => Utilities.GetTimeZoneOffsetForUser(this.UserInfo);

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.ModuleSettings ModuleSettings
        {
            get
            {
                if (this.moduleSettings == null)
                {
                    this.moduleSettings = this.LoadModuleSettings();
                    this.UpdateCache();
                }

                return this.moduleSettings;
            }

            set
            {
                this.moduleSettings = value;
                this.UpdateCache();
            }
        }

        internal DotNetNuke.Modules.ActiveForums.ModuleSettings LoadModuleSettings()
        {
            return this.moduleSettings = SettingsBase.GetModuleSettings(this.ModuleId);
        }

        [IgnoreColumn]
        public PortalSettings PortalSettings
        {
            get
            {
                if (this.portalSettings == null)
                {
                    this.portalSettings = this.LoadPortalSettings();
                    this.UpdateCache();
                }

                return this.portalSettings;
            }

            set
            {
                this.portalSettings = value;
                this.UpdateCache();
            }
        }

        internal PortalSettings LoadPortalSettings()
        {
            if (this.PortalId == -1 && this.UserInfo != null && this.UserInfo.PortalID != -1)
            {
                this.PortalId = this.UserInfo.PortalID;
            }

            return this.portalSettings = Utilities.GetPortalSettings(this.PortalId);
        }

        [IgnoreColumn]
        public ModuleInfo ModuleInfo
        {
            get
            {
                if (this.moduleInfo == null)
                {
                    this.moduleInfo = this.LoadModuleInfo();
                    this.UpdateCache();
                }

                return this.moduleInfo;
            }

            set
            {
                this.moduleInfo = value;
                this.UpdateCache();
            }
        }

        internal ModuleInfo LoadModuleInfo()
        {
            return this.moduleInfo = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(this.ModuleId, DotNetNuke.Common.Utilities.Null.NullInteger, false);
        }

        [IgnoreColumn]
        public DotNetNuke.Entities.Users.UserInfo UserInfo
        {
            get
            {
                if (this.userInfo == null)
                {
                    this.userInfo = this.GetUser();
                    this.UpdateCache();
                }

                return this.userInfo;
            }

            set
            {
                this.userInfo = value;
                this.UpdateCache();
            }
        }

        internal DotNetNuke.Entities.Users.UserInfo GetUser()
        {
            return this.userInfo = DotNetNuke.Entities.Users.UserController.Instance.GetUser(this.PortalId, this.UserId);
        }

        [IgnoreColumn]
        public HashSet<int> UserRoleIds
        {
            get
            {
                if (this.userRoleIds == null)
                {
                    this.userRoleIds = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.PortalSettings, this.UserInfo);
                    this.UpdateCache();
                }

                return this.userRoleIds;
            }

            set
            {
                this.userRoleIds = value;
            }
        }

        [IgnoreColumn]
        public string UserPermSet
        {
            get
            {
                if (string.IsNullOrEmpty(this.userPermSet))
                {
                    this.userPermSet = this.UserRoleIds.FromHashSetToDelimitedString<int>(";");
                    this.UpdateCache();
                }

                return this.userPermSet;
            }

            set
            {
                this.userPermSet = value;
            }
        }

        [IgnoreColumn]
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used")]
        public string UserRoles
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        internal int GetLastReplyRead(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.ModuleId, this.UserId, ti.TopicId);
            var forumTrak = new DotNetNuke.Modules.ActiveForums.Controllers.ForumTrackingController().GetByUserIdForumId(this.ModuleId, this.UserId, ti.ForumId);
            if (forumTrak?.MaxReplyRead > topicTrak?.LastReplyId || topicTrak == null)
            {
                if (forumTrak != null)
                {
                    return forumTrak.MaxReplyRead;
                }
            }
            else
            {
                return topicTrak.LastReplyId;
            }

            return 0;

            // from stored procedure
            // CASE WHEN FT.MaxReplyRead > TT.LastReplyId OR TT.LastReplyID IS NULL THEN ISNULL(FT.MaxReplyRead,0) ELSE TT.LastReplyId END AS UserLastReplyRead,
        }

        internal int GetLastTopicRead(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.ModuleId, this.UserId, ti.TopicId);
            var forumTrak = new DotNetNuke.Modules.ActiveForums.Controllers.ForumTrackingController().GetByUserIdForumId(this.ModuleId, this.UserId, ti.ForumId);
            if (forumTrak?.MaxTopicRead > topicTrak?.TopicId || topicTrak == null)
            {
                if (forumTrak != null)
                {
                    return forumTrak.MaxTopicRead;
                }
            }
            else
            {
                return topicTrak.TopicId;
            }

            return 0;

            // from stored procedure
            // CASE WHEN FT.MaxTopicRead > TT.TopicId OR TT.TopicId IS NULL THEN ISNULL(FT.MaxTopicRead,0) ELSE TT.TopicId END AS UserLastTopicRead,
        }

        internal bool GetIsTopicRead(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.ModuleId, this.UserId, ti.TopicId);
            if (topicTrak?.LastReplyId >= ti.LastReplyId)
            {
                return true;
            }

            return false;
        }

        internal bool GetIsReplyRead(DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.ModuleId, this.UserId, ri.TopicId);
            if (topicTrak?.LastReplyId >= ri.ReplyId)
            {
                return true;
            }

            return false;
        }

        internal int GetLastTopicRead(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi)
        {
            var forumTrak = new DotNetNuke.Modules.ActiveForums.Controllers.ForumTrackingController().GetByUserIdForumId(this.ModuleId, this.UserId, fi.ForumID);
            if (forumTrak != null)
            {
                return forumTrak.MaxTopicRead;
            }

            return 0;
        }

        internal int GetLikeCountForUser()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.LikeController().Count("WHERE UserId = @0 AND Checked = 1", this.UserId);
        }

        internal int GetLikeCountForUserSince(DateTime minDateTime)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.LikeController().Count("WHERE UserId = @0 AND Checked = 1 AND DateCreated >= @1", this.UserId, minDateTime);
        }

        internal int GetTopicReadCount(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetTopicsReadCountForUserForum(this.ModuleId, this.UserId, fi.ForumID);
        }

        internal int GetTopicReadCount()
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetTopicsReadCountByUser(this.ModuleId, this.UserId);
        }

        internal int GetTopicReadCountSince(DateTime minDateTimeRead)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetTopicsReadCountByUser(this.ModuleId, this.UserId, minDateTimeRead);
        }

        internal int GetLastTopicReplyRead(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.ModuleId, this.UserId, ti.TopicId);
            if (topicTrak != null)
            {
                return topicTrak.LastReplyId;
            }

            return 0;
        }

        [IgnoreColumn]
        public bool RunningInViewer
        {
            get
            {
                return this.PortalSettings.ActiveTab != null && this.PortalSettings.ActiveTab.Modules.Cast<DotNetNuke.Entities.Modules.ModuleInfo>().Any(
                    m => m.ModuleDefinition.DefinitionName.Equals(Globals.ModuleFriendlyName + " Viewer", StringComparison.OrdinalIgnoreCase) ||
                    m.ModuleDefinition.DefinitionName.Equals(Globals.ModuleName + " Viewer", StringComparison.OrdinalIgnoreCase));
            }
        }

        [IgnoreColumn]
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> Badges
        {
            get
            {
                if (this.userBadges == null)
                {
                    this.GetUserBadges();
                    this.UpdateCache();
                }

                return this.userBadges;
            }
            set => this.userBadges = value;
        }

        internal IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetUserBadges() => this.userBadges = new DotNetNuke.Modules.ActiveForums.Controllers.UserBadgeController(this.PortalId, this.ModuleId).GetForUser(this.UserId);

        [IgnoreColumn]
        public int ForumsOrViewerModuleId
        {
            get
            {
                if (!this.RunningInViewer)
                {
                    return this.ModuleId;
                }

                if (this.PortalSettings.ActiveTab != null)
                {
                    foreach (DotNetNuke.Entities.Modules.ModuleInfo module in this.PortalSettings.ActiveTab.Modules.Cast<DotNetNuke.Entities.Modules.ModuleInfo>().Where(m => m.ModuleDefinition.DefinitionName.Equals(Globals.ModuleFriendlyName + " Viewer", StringComparison.OrdinalIgnoreCase) || m.ModuleDefinition.DefinitionName.Equals(Globals.ModuleName + " Viewer", StringComparison.OrdinalIgnoreCase)))
                    {
                        return module.ModuleID;
                    }
                }

                return DotNetNuke.Common.Utilities.Null.NullInteger;
            }
        }

        public int GetTopicCountSince(DateTime minDateTime)
        {
            string sSql = "SELECT COUNT(*) FROM {databaseOwner}{objectQualifier}activeforums_Content c INNER JOIN {databaseOwner}{objectQualifier}activeforums_Topics t ON t.ContentId = c.ContentId WHERE c.ModuleId = @0 AND c.AuthorId = @1 AND c.IsDeleted = 0 AND c.DateCreated >= @2";
            return DataContext.Instance().ExecuteQuery<int>(System.Data.CommandType.Text, sSql, this.ModuleId, this.UserId, minDateTime).FirstOrDefault();
        }

        public int GetReplyCountSince(DateTime minDateTime)
        {
            string sSql = "SELECT COUNT(*) FROM {databaseOwner}{objectQualifier}activeforums_Content c INNER JOIN {databaseOwner}{objectQualifier}activeforums_Replies r ON r.ContentId = c.ContentId WHERE c.ModuleId = @0 AND c.AuthorId = @1 AND c.IsDeleted = 0 AND c.DateCreated >= @2";
            return DataContext.Instance().ExecuteQuery<int>(System.Data.CommandType.Text, sSql, this.ModuleId, this.UserId, minDateTime).FirstOrDefault();
        }

        [IgnoreColumn]
        public DotNetNuke.Services.Tokens.CacheLevel Cacheability => DotNetNuke.Services.Tokens.CacheLevel.notCacheable;

        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, DotNetNuke.Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {

            // replace any embedded tokens in format string
            if (format.Contains("["))
            {
                var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(this.PortalSettings, this, this.RequestUri, this.RawUrl)
                {
                    AccessingUser = accessingUser,
                };
                format = tokenReplacer.ReplaceEmbeddedTokens(format);
            }

            var length = DotNetNuke.Common.Utilities.Null.NullInteger;
            if (propertyName.Contains(":"))
            {
                var splitPropertyName = propertyName.Split(':');
                propertyName = splitPropertyName[0];
                length = Utilities.SafeConvertInt(splitPropertyName[1], DotNetNuke.Common.Utilities.Null.NullInteger);
            }

            propertyName = propertyName.ToLowerInvariant();
            try
            {
                switch (propertyName)
                {
                    case "avatar":
                        if (this.PrefBlockAvatars || this.AvatarDisabled)
                        {
                            return PropertyAccess.FormatString(string.Empty, format);
                        }

                        var height = this.ModuleSettings.AvatarHeight;
                        var width = this.ModuleSettings.AvatarWidth;
                        if (length > 0)
                        {
                            height = length;
                            width = length;
                        }

                        return PropertyAccess.FormatString(DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetAvatar(this.PortalSettings, this.UserId, width, height), format);
                    case "usercaption":
                        return PropertyAccess.FormatString(this.UserCaption, format);
                    case "displayname":
                        return PropertyAccess.FormatString(DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(this.PortalSettings, this.ModuleSettings, isMod: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(portalId: accessingUser.PortalID, userId: accessingUser.UserID).GetIsMod(this.ModuleId), isAdmin: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(portalId: accessingUser.PortalID, userId: accessingUser.UserID).IsAdmin, this.UserId, this.Username, this.FirstName, this.LastName, this.DisplayName), format);
                    case "datecreated":
                        return Utilities.GetUserFormattedDateTime(this.DateCreated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                    case "dnnuserdatecreated":
                        return Utilities.GetUserFormattedDateTime(this.DNNUserDateCreated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                    case "dateupdated":
                        return Utilities.GetUserFormattedDateTime(this.DateUpdated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                    case "dnnuserdateupdated":
                        return Utilities.GetUserFormattedDateTime(this.DNNUserDateUpdated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                    case "datelastpost":
                        return Utilities.GetUserFormattedDateTime(this.DateLastPost, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                    case "datelastreply":
                        return Utilities.GetUserFormattedDateTime(this.DateLastReply, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                    case "datelastactivity":
                        return Utilities.GetUserFormattedDateTime(this.DateLastActivity, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                    case "postcount":
                        return PropertyAccess.FormatString(this.PostCount.ToString(), format);
                    case "replycount":
                        return PropertyAccess.FormatString(this.ReplyCount.ToString(), format);
                    case "viewcount":
                        return PropertyAccess.FormatString(this.ViewCount.ToString(), format);
                    case "topiccount":
                        return PropertyAccess.FormatString(this.TopicCount.ToString(), format);
                    case "answercount":
                        return PropertyAccess.FormatString(this.ModuleSettings.EnablePoints && this.UserId > 0 ? this.AnswerCount.ToString() : string.Empty, format);
                    case "rewardpoints":
                        return PropertyAccess.FormatString(this.ModuleSettings.EnablePoints && this.UserId > 0 ? this.RewardPoints.ToString() : string.Empty, format);
                    case "totalpoints":
                        return PropertyAccess.FormatString(this.ModuleSettings.EnablePoints && this.UserId > 0 ? ((this.TopicCount * this.ModuleSettings.TopicPointValue) + (this.ReplyCount * this.ModuleSettings.ReplyPointValue) + (this.AnswerCount * this.ModuleSettings.AnswerPointValue) + this.RewardPoints).ToString() : string.Empty, format);
                    case "rankdisplay":
                        return PropertyAccess.FormatString(this.UserId > 0 ? DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetUserRank(this.ModuleId, this, 0) : string.Empty, format);
                    case "rankname":
                        return PropertyAccess.FormatString(this.UserId > 0 ? DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetUserRank(this.ModuleId, this, 1) : string.Empty, format);
                    case "userprofilelink":
                        return PropertyAccess.FormatString(DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.CanLinkToProfile(portalSettings: this.PortalSettings,
                                                                                                                                            moduleSettings: this.ModuleSettings,
                                                                                                                                            moduleId: this.ModuleId,
                                                                                                                                            accessingUser: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID),
                                                                                                                                            forumUser: this) ? Utilities.NavigateURL(this.PortalSettings.UserTabId, string.Empty, new[] { $"userId={this.UserId}" }) : string.Empty, format);
                    case "signature":
                        var sSignature = string.Empty;
                        if (this.ModuleSettings.AllowSignatures != 0 && !this.PrefBlockSignatures && !this.SignatureDisabled)
                        {
                            sSignature = this?.Signature ?? string.Empty;
                            if (!string.IsNullOrEmpty(sSignature))
                            {
                                sSignature = Utilities.ManageImagePath(sSignature, this.RequestUri);

                                switch (this.ModuleSettings.AllowSignatures)
                                {
                                    case 1:
                                        sSignature = System.Net.WebUtility.HtmlEncode(sSignature);
                                        sSignature = sSignature.Replace(System.Environment.NewLine, "<br />");
                                        break;
                                    case 2:
                                        sSignature = System.Net.WebUtility.HtmlDecode(sSignature);
                                        break;
                                }
                            }
                        }

                        return PropertyAccess.FormatString(sSignature, format);
                    case "userstatus":
                        {
                            return PropertyAccess.FormatString(this.ModuleSettings != null && this.ModuleSettings.UsersOnlineEnabled && this.UserId > 0 ? DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.GetTokenFormatString(this.IsUserOnline ? "[FORUMUSER-USERONLINE]" : "[FORUMUSER-USEROFFLINE]", this.PortalSettings, accessingUser.Profile.PreferredLocale) : string.Empty, format);
                        }

                    case "userstatuscss":
                        {
                            return PropertyAccess.FormatString(this.ModuleSettings != null && this.ModuleSettings.UsersOnlineEnabled && this.UserId > 0 ? DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.GetTokenFormatString(this.IsUserOnline ? "[FORUMUSER-USERONLINECSS]" : "[FORUMUSER-USEROFFLINECSS]", this.PortalSettings, accessingUser.Profile.PreferredLocale) : string.Empty, format);
                        }

                    case "userid":
                        return PropertyAccess.FormatString(this.UserId.ToString(), format);
                    case "username":
                        return PropertyAccess.FormatString(this.Username, format);
                    case "userdisplaynamelink":
                        {
                            return PropertyAccess.FormatString(
                                Controllers.ForumUserController.CanLinkToProfile(
                                    this.PortalSettings,
                                    this.ModuleSettings,
                                    this.ModuleId,
                                    new Controllers.ForumUserController(this.ModuleId).GetByUserId(
                                        accessingUser.PortalID,
                                        accessingUser.UserID),
                                    this)
                                    ? Utilities.NavigateURL(this.PortalSettings.UserTabId,
                                        string.Empty,
                                        $"userId={this.UserId}")
                                    : string.Empty,
                                format);
                        }

                    case "userdisplayname":
                        return PropertyAccess.FormatString(string.IsNullOrEmpty(this.DisplayName) ? this.Username :
                            DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(
                                this.PortalSettings,
                                this.ModuleSettings,
                                isMod: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).GetIsMod(this.ModuleId),
                                isAdmin: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).IsAdmin || new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID).IsSuperUser,
                                this.UserId,
                                this.Username,
                                this.FirstName,
                                this.LastName,
                                this.DisplayName).Replace("&amp;#", "&#").Replace("Anonymous", this.Username), format);
                    case "userfirstname":
                        return PropertyAccess.FormatString(string.IsNullOrEmpty(this.FirstName) ? this.Username : this.FirstName, format);
                    case "userlastname":
                        return PropertyAccess.FormatString(string.IsNullOrEmpty(this.LastName) ? this.Username : this.LastName, format);
                    case "useremail":
                        return PropertyAccess.FormatString(string.IsNullOrEmpty(this.Email) ? string.Empty : this.Email, format);
                    case "useridforpmlink":
                        return PropertyAccess.FormatString(accessingUser.UserID > 0 && this.UserId > 0 ? this.UserId.ToString() : string.Empty, format);
                    case "useridforeditlink":
                        return PropertyAccess.FormatString((accessingUser.IsAdmin || accessingUser.IsSuperUser) && this.UserId > 0 ? this.UserId.ToString() : string.Empty, format);
                    case "displaynameforjson":
                        return PropertyAccess.FormatString(Utilities.JSON.EscapeJsonString(this.DisplayName), format);
                    case "badges":
                        if (length < 1)
                        {
                            length = 5; /* if no length specified, default to 5 badges */
                        }

                        var badgeString = string.Empty;
                        var userBadgesToDisplay = this.Badges.GroupBy(b => b.BadgeId).Select(g => g.OrderByDescending(b => b.DateAssigned).First()).ToList().OrderBy(b => b.Badge.SortOrder).Take(length);
                        var badgeTemplate = new StringBuilder(DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(this.ModuleId, Enums.TemplateType.UserBadge, SettingsBase.GetModuleSettings(this.ModuleId).DefaultFeatureSettings.TemplateFileNameSuffix, this));
                        foreach (var userBadge in userBadgesToDisplay)
                        {
                            badgeString += DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceBadgeTokens(badgeTemplate, userBadge, this.PortalSettings, this.ModuleSettings, new Services.URLNavigator().NavigationManager(), new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID), this.RequestUri, this.RawUrl);
                        }

                        return PropertyAccess.FormatString(badgeString, format);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(new ArgumentException(string.Format(Utilities.GetSharedResource("[RESX:TokenReplacementException]"), "ForumUserInfo", this.UserId, propertyName, format)));
                return string.Empty;
            }

            propertyNotFound = true;
            return string.Empty;
        }

        internal string GetCacheKey() => string.Format(this.cacheKeyTemplate, this.PortalId, this.UserId);

        internal void UpdateCache() => DataCache.UserCacheStore(this.GetCacheKey(), this);
    }
}
