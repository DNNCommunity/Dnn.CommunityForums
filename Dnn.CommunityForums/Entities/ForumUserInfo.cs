﻿// Copyright (c) by DNN Community
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
    using System.Linq;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;

    [TableName("activeforums_UserProfiles")]
    [PrimaryKey("ProfileId", AutoIncrement = true)]
    [Scope("PortalId")]
    public class ForumUserInfo : DotNetNuke.Services.Tokens.IPropertyAccess
    {
        [IgnoreColumn]
        private string cacheKeyTemplate => CacheKeys.ForumUser;

        private DotNetNuke.Entities.Users.UserInfo userInfo;
        private PortalSettings portalSettings;
        private SettingsInfo mainSettings;
        private ModuleInfo moduleInfo;
        private string userRoles;

        public ForumUserInfo()
        {
        }

        public ForumUserInfo(int moduleId)
        {
            this.userInfo = new DotNetNuke.Entities.Users.UserInfo();
            this.ModuleId = moduleId;
        }

        public ForumUserInfo(DotNetNuke.Entities.Users.UserInfo userInfo)
        {
            this.userInfo = userInfo;
        }

        public ForumUserInfo(int moduleId, DotNetNuke.Entities.Users.UserInfo userInfo)
        {
            this.userInfo = userInfo;
            this.ModuleId = moduleId;
        }

        [IgnoreColumn]
        public bool IsAuthenticated { get; set; } = false;

        public int ProfileId { get; set; }

        public int UserId { get; set; } = -1;

        public int PortalId { get; set; }

        [IgnoreColumn]
        internal int ModuleId { get; set; }

        public int TopicCount { get; set; }

        public int ReplyCount { get; set; }

        public int ViewCount { get; set; }

        public int AnswerCount { get; set; }

        public int RewardPoints { get; set; }

        public string UserCaption { get; set; }

        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime? DateUpdated { get; set; }

        public DateTime? DateLastActivity { get; set; }

        public DateTime? DateLastPost { get; set; }

        public DateTime? DateLastReply { get; set; }

        public string Signature { get; set; }

        public bool SignatureDisabled { get; set; }

        public int TrustLevel { get; set; }

        public bool AdminWatch { get; set; }

        public bool AttachDisabled { get; set; }

        public string Avatar { get; set; }

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

        [IgnoreColumn] 
        public string RawUrl { get; set; }

        [IgnoreColumn]
        public Uri RequestUri { get; set; }

        [IgnoreColumn]
        public string[] Roles => this.UserInfo?.Roles;

        [IgnoreColumn]
        public string FirstName => this.UserInfo?.FirstName;

        [IgnoreColumn]
        public string LastName => string.IsNullOrEmpty(this.UserInfo?.LastName) ? string.Empty : this.UserInfo?.LastName;

        [IgnoreColumn]
        public string FullName => string.Concat(this.UserInfo?.FirstName, " ", this.UserInfo?.LastName);

        [IgnoreColumn]
        public string DisplayName => string.IsNullOrEmpty(this.UserInfo?.DisplayName) == null ? string.Empty : this.UserInfo?.DisplayName;

        [IgnoreColumn]
        public string Username => this.UserInfo?.Username;

        [IgnoreColumn]
        public string Email => string.IsNullOrEmpty(this.UserInfo?.Email) ? string.Empty : this.UserInfo?.Email;

        [IgnoreColumn]
        public bool IsRegistered => !this.IsAnonymous && this.UserInfo != null && this.UserInfo.Roles.Contains(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredRoleName(this.PortalId));

        [IgnoreColumn]
        public bool GetIsMod(int ModuleId) => !this.IsAnonymous && !(string.IsNullOrEmpty(DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.UserRoles, this.PortalId, ModuleId, "CanApprove")));

        [IgnoreColumn]
        public bool IsSuperUser => this.UserInfo != null && this.UserInfo.IsSuperUser;

        [IgnoreColumn]
        public bool IsAdmin => this.UserInfo != null && this.UserInfo.IsAdmin;

        [IgnoreColumn]
        public bool IsAnonymous => this.UserId == -1;

        [IgnoreColumn]
        public bool IsUserOnline => this.DateLastActivity > DateTime.UtcNow.AddMinutes(-5);

        [IgnoreColumn]
        public CurrentUserTypes CurrentUserType
        {
            get
            {
                if (this.UserInfo.IsInRole(this.PortalSettings.RegisteredRoleName))
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

        [IgnoreColumn]
        TimeSpan TimeZoneOffsetForUser => Utilities.GetTimeZoneOffsetForUser(this.UserInfo);

        [IgnoreColumn]
        public SettingsInfo MainSettings
        {
            get
            {
                if (this.mainSettings == null)
                {
                    this.mainSettings = this.LoadMainSettings();
                    this.UpdateCache();
                }

                return this.mainSettings;
            }

            set
            {
                this.mainSettings = value;
                this.UpdateCache();
            }
        }

        internal SettingsInfo LoadMainSettings()
        {
            return this.mainSettings = SettingsBase.GetModuleSettings(this.ModuleId);
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
        public string UserRoles
        {
            get
            {
                if (string.IsNullOrEmpty(this.userRoles))
                {
                    var ids = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetUsersRoleIds(this.PortalId, this.UserInfo);
                    if (string.IsNullOrEmpty(ids))
                    {
                        ids = Globals.DefaultAnonRoles + "|-1;||";
                    }

                    if (this.IsSuperUser)
                    {
                        ids += Globals.DefaultAnonRoles + this.PortalSettings.AdministratorRoleId + ";";
                    }

                    ids += "|" + this.UserId + "|" + string.Empty + "|";
                    this.userRoles = ids;
                    this.UpdateCache();
                }

                return this.userRoles;
            }

            set
            {
                this.userRoles = value;
            }
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

        internal int GetTopicReadCount(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetTopicsReadCountForUserForum(this.ModuleId, this.UserId, fi.ForumID);
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
        public DotNetNuke.Services.Tokens.CacheLevel Cacheability
        {
            get
            {
                return DotNetNuke.Services.Tokens.CacheLevel.notCacheable;
            }
        }

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

            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName)
            {
                case "avatar":
                    return PropertyAccess.FormatString(!this.PrefBlockAvatars && !this.AvatarDisabled ? DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetAvatar(this.UserId, this.MainSettings.AvatarWidth, this.MainSettings.AvatarHeight) : string.Empty, format);
                case "usercaption":
                    return PropertyAccess.FormatString(this.UserCaption, format);
                case "displayname":
                    return PropertyAccess.FormatString(DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(this.PortalSettings, this.MainSettings, isMod: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(portalId: accessingUser.PortalID, userId: accessingUser.UserID).GetIsMod(this.ModuleId), isAdmin: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(portalId: accessingUser.PortalID, userId: accessingUser.UserID).IsAdmin, this.UserId, this.Username, this.FirstName, this.LastName, this.DisplayName), format);
                case "datecreated":
                    return Utilities.GetUserFormattedDateTime(this.DateCreated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                case "dateupdated":
                    return Utilities.GetUserFormattedDateTime(this.DateUpdated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
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
                    return PropertyAccess.FormatString(this.MainSettings.EnablePoints && this.UserId > 0 ? this.AnswerCount.ToString() : string.Empty, format);
                case "rewardpoints":
                    return PropertyAccess.FormatString(this.MainSettings.EnablePoints && this.UserId > 0 ? this.RewardPoints.ToString() : string.Empty, format);
                case "totalpoints":
                    return PropertyAccess.FormatString(this.MainSettings.EnablePoints && this.UserId > 0 ? ((this.TopicCount * this.MainSettings.TopicPointValue) + (this.ReplyCount * this.MainSettings.ReplyPointValue) + (this.AnswerCount * this.MainSettings.AnswerPointValue) + this.RewardPoints).ToString() : string.Empty, format);
                case "rankdisplay":
                    return PropertyAccess.FormatString(this.UserId > 0 ? DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetUserRank(this.ModuleId, this, 0) : string.Empty, format);
                case "rankname":
                    return PropertyAccess.FormatString(this.UserId > 0 ? DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetUserRank(this.ModuleId, this, 1) : string.Empty, format);
                case "userprofilelink":
                    return PropertyAccess.FormatString(DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.CanLinkToProfile(portalSettings: this.PortalSettings,
                                                                                                                                        mainSettings: this.MainSettings,
                                                                                                                                        moduleId: this.ModuleId,
                                                                                                                                        accessingUser: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID),
                                                                                                                                        forumUser: this) ? Utilities.NavigateURL(this.PortalSettings.UserTabId, string.Empty, new[] { $"userId={this.UserId}" }) : string.Empty, format);
                case "signature":
                    var sSignature = string.Empty;
                    if (this.MainSettings.AllowSignatures != 0 && !this.PrefBlockSignatures && !this.SignatureDisabled)
                    {
                        sSignature = this?.Signature ?? string.Empty;
                        if (!string.IsNullOrEmpty(sSignature))
                        {
                            sSignature = Utilities.ManageImagePath(sSignature);

                            switch (this.MainSettings.AllowSignatures)
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
                        return PropertyAccess.FormatString(this.MainSettings != null && this.MainSettings.UsersOnlineEnabled && this.UserId > 0 ? DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.GetTokenFormatString(this.IsUserOnline ? "[FORUMUSER-USERONLINE]" : "[FORUMUSER-USEROFFLINE]", this.PortalSettings, accessingUser.Profile.PreferredLocale) : string.Empty, format);
                    }

                case "userstatuscss":
                    {
                        return PropertyAccess.FormatString(this.MainSettings != null && this.MainSettings.UsersOnlineEnabled && this.UserId > 0 ? DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.GetTokenFormatString(this.IsUserOnline ? "[FORUMUSER-USERONLINECSS]" : "[FORUMUSER-USEROFFLINECSS]", this.PortalSettings, accessingUser.Profile.PreferredLocale) : string.Empty, format);
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
                                this.MainSettings,
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
                            this.MainSettings,
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
                    return PropertyAccess.FormatString(this.UserId > 0 ? this.UserId.ToString() : string.Empty, format);
                case "useridforeditlink":
                    return PropertyAccess.FormatString((accessingUser.IsAdmin || accessingUser.IsSuperUser) && this.UserId > 0 ? this.UserId.ToString() : string.Empty, format);
                case "displaynameforjson":
                    return PropertyAccess.FormatString(Utilities.JSON.EscapeJsonString(this.DisplayName), format);
            }

            propertyNotFound = true;
            return string.Empty;
        }

        internal string GetCacheKey() => string.Format(this.cacheKeyTemplate, this.PortalId, this.UserId);

        internal void UpdateCache() => DataCache.UserCacheStore(this.GetCacheKey(), this);
    }
}
