// Copyright (c) 2013-2024 by DNN Community
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

using System.Web;

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Tokens;

    [TableName("activeforums_Forums")]
    [PrimaryKey("ForumID", AutoIncrement = true)] /* ForumID because needs to match property name NOT database column name */
    [Scope("ModuleId")]

    // TODO [Cacheable("activeforums_Forums", CacheItemPriority.Low)] /* TODO: DAL2 caching cannot be used until all CRUD methods use DAL2; must update Save method to use DAL2 rather than stored procedure */

    public class ForumInfo : DotNetNuke.Services.Tokens.IPropertyAccess
    {
        private ForumGroupInfo forumGroup;
        private List<ForumInfo> subforums;

        [ColumnName("ForumId")]
        public int ForumID { get; set; }

        public int PortalId { get; set; }

        public int ModuleId { get; set; }

        public int ForumGroupId { get; set; }

        public int ParentForumId { get; set; }

        public string ForumName { get; set; }

        public string ForumDesc { get; set; }

        public int SortOrder { get; set; }

        public bool Active { get; set; }

        public bool Hidden { get; set; }

        public int TotalTopics { get; set; }

        public int TotalReplies { get; set; }

        [IgnoreColumn]
        public int LastPostID => this.LastReplyId == 0 ? this.LastTopicId : this.LastReplyId;

        public string ForumSettingsKey { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        public int LastTopicId { get; set; }

        public int LastReplyId { get; set; }

        public string LastPostSubject { get; set; }

        [ColumnName("LastPostAuthorName")]
        public string LastPostUserName { get; set; }

        [ColumnName("LastPostAuthorId")]
        public int LastPostUserID { get; set; }

        [ColumnName("LastPostDate")]
        public DateTime LastPostDateTime { get; set; }

        public int PermissionsId { get; set; }

        public string PrefixURL { get; set; }

        public int SocialGroupId { get; set; }

        public bool HasProperties { get; set; }

        [IgnoreColumn()]
        public ForumGroupInfo ForumGroup
        {
            get => this.forumGroup ?? (this.forumGroup = this.LoadForumGroup());
            set => this.forumGroup = value;
        }

        internal ForumGroupInfo LoadForumGroup()
        {
            var group = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetById(this.ForumGroupId, this.ModuleId);
            if (group == null)
            {
                var log = new LogInfo { LogTypeKey = Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                var message = string.Format(Utilities.GetSharedResource("[RESX:ForumGroupMissingForForum]"), this.ForumGroupId, this.ForumID);
                log.AddProperty("Message", message);
                LogController.Instance.AddLog(log);

                var ex = new NullReferenceException(string.Format(Utilities.GetSharedResource("[RESX:ForumGroupMissingForForum]"), this.ForumGroupId, this.ForumID));
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                throw ex;
            }

            return group;
        }

        [IgnoreColumn()]
        public string GroupName => this.ForumGroup.GroupName;

        [IgnoreColumn()]
        public string LastTopicUrl { get; set; }

        [IgnoreColumn()]
        public DateTime LastRead { get; set; }

        [IgnoreColumn()]
        public string LastPostFirstName
        {
            get
            {
                var name = string.Empty;
                if (this.PortalId >= 0 && this.LastPostUserID > 0)
                {
                    var user = new DotNetNuke.Entities.Users.UserController().GetUser(this.PortalId, this.LastPostUserID);
                    name = user?.FirstName;
                }

                if (string.IsNullOrEmpty(name))
                {
                    name = this.LastPostUserID > 0 ? Utilities.GetSharedResource("[RESX:DeletedUser]") : Utilities.GetSharedResource("[RESX:Anonymous]");
                }

                return name;
            }
        }

        [IgnoreColumn()]
        public string LastPostLastName
        {
            get
            {
                var name = string.Empty;
                if (this.PortalId >= 0 && this.LastPostUserID > 0)
                {
                    var user = new DotNetNuke.Entities.Users.UserController().GetUser(this.PortalId, this.LastPostUserID);
                    name = user?.LastName;
                }

                if (string.IsNullOrEmpty(name))
                {
                    name = this.LastPostUserID > 0 ? Utilities.GetSharedResource("[RESX:DeletedUser]") : Utilities.GetSharedResource("[RESX:Anonymous]");
                }

                return name;
            }
        }

        [IgnoreColumn()]
        public string LastPostDisplayName
        {
            get
            {
                var name = string.Empty;
                if (this.PortalId >= 0 && this.LastPostUserID > 0)
                {
                    var user = new DotNetNuke.Entities.Users.UserController().GetUser(this.PortalId, this.LastPostUserID);
                    name = user?.DisplayName;
                }

                if (string.IsNullOrEmpty(name))
                {
                    name = this.LastPostUserID > 0 ? Utilities.GetSharedResource("[RESX:DeletedUser]") : Utilities.GetSharedResource("[RESX:Anonymous]");
                }

                return name;
            }
        }

        [IgnoreColumn()]
        public bool InheritSecurity => this.PermissionsId == this.ForumGroup.PermissionsId;

        [IgnoreColumn()]
        public bool InheritSettings => this.ForumSettingsKey == this.ForumGroup.GroupSettingsKey;

        [IgnoreColumn()]
        public int SubscriberCount => new Controllers.SubscriptionController().Count(portalId: this.PortalId, moduleId: this.ModuleId, forumId: this.ForumID);

        [IgnoreColumn()]
        public string ParentForumName => this.ParentForumId > 0 ? new Controllers.ForumController().GetById(this.ParentForumId, this.ModuleId).ForumName : string.Empty;

        [IgnoreColumn()]
        public int TabId => new DotNetNuke.Entities.Modules.ModuleController().GetModule(this.ModuleId).TabID;

        [IgnoreColumn()]
        public string ForumURL => URL.ForumLink(this.TabId, this);

        [IgnoreColumn()]
        public List<ForumInfo> SubForums
        {
            get => this.subforums ?? this.LoadSubForums();
            set => this.subforums = value;
        }

        [IgnoreColumn]
        internal List<ForumInfo> LoadSubForums()
        {
            return this.subforums = new Controllers.ForumController().GetSubForums(this.ForumID, this.ModuleId).ToList();
        }

        private List<PropertyInfo> properties;

        [IgnoreColumn()]
        public List<PropertyInfo> Properties
        {
            get => this.properties ?? (this.properties = this.LoadProperties());
            set => this.properties = value;
        }

        [IgnoreColumn]
        internal List<PropertyInfo> LoadProperties()
        {
            return this.HasProperties ? new DotNetNuke.Modules.ActiveForums.Controllers.PropertyController().Get().Where(p => p.PortalId == this.PortalId && p.ObjectType == 1 && p.ObjectOwnerId == this.ForumID).ToList() : new List<PropertyInfo>();
        }
        
        [IgnoreColumn]
        public bool GetIsMod(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser)
        {
            return (!(string.IsNullOrEmpty(DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(forumUser.UserRoles, this.PortalId, this.ModuleId, "CanApprove"))));
        }

        [IgnoreColumn]
        public string ThemeLocation => Utilities.ResolveUrl(SettingsBase.GetModuleSettings(this.ModuleId).ThemeLocation);

        [IgnoreColumn]
        internal DotNetNuke.Modules.ActiveForums.Enums.ForumStatus GetForumStatusForUser(ForumUserInfo forumUser)
        {
            var canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.Security.View, forumUser?.UserRoles);

            if (!canView)
            {
                return DotNetNuke.Modules.ActiveForums.Enums.ForumStatus.Forbidden;
            }
            if (this.LastTopicId == 0)
            {
                return DotNetNuke.Modules.ActiveForums.Enums.ForumStatus.Empty;
            }

            try
            {
                if (this.LastTopicId > forumUser?.GetLastTopicRead(this))
                {
                    return DotNetNuke.Modules.ActiveForums.Enums.ForumStatus.NewTopics;
                }

                if (forumUser?.UserId == -1 || this.TotalTopics > forumUser?.GetTopicReadCount(this))
                {
                    return DotNetNuke.Modules.ActiveForums.Enums.ForumStatus.UnreadTopics;
                }

                return DotNetNuke.Modules.ActiveForums.Enums.ForumStatus.AllTopicsRead;
            }
            catch
            {
                /* this is to handle some limited unit testing without retrieving data */
                return DotNetNuke.Modules.ActiveForums.Enums.ForumStatus.UnreadTopics;
            }
        }

        #region "Settings & Security"

        private PermissionInfo security;
        private Hashtable forumSettings;

        [IgnoreColumn()]
        public PermissionInfo Security
        {
            get => this.security ?? (this.security = this.LoadSecurity());
            set => this.security = value;
        }

        internal PermissionInfo LoadSecurity()
        {
            var security = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(this.PermissionsId, this.ModuleId);
            if (security == null)
            {
                security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(this.ModuleId);
                var log = new LogInfo { LogTypeKey = Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                var message = string.Format(Utilities.GetSharedResource("[RESX:PermissionsMissingForForum]"), this.PermissionsId, this.ForumID);
                log.AddProperty("Message", message);
                LogController.Instance.AddLog(log);
            }

            return security;
        }

        [IgnoreColumn()]
        public Hashtable ForumSettings
        {
            get => this.forumSettings ?? (this.forumSettings = this.LoadSettings());
            set => this.forumSettings = value;
        }

        internal Hashtable LoadSettings()
        {
            return DataCache.GetSettings(this.ModuleId, this.ForumSettingsKey, string.Format(CacheKeys.ForumSettingsByKey, this.ModuleId, this.ForumSettingsKey), true);
        }

        [IgnoreColumn()]
        public bool AllowAttach => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AllowAttach]);

        [IgnoreColumn()]
        public bool AllowEmoticons => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AllowEmoticons]);

        [IgnoreColumn()]
        public bool AllowHTML => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AllowHTML]);

        [IgnoreColumn()]
        public bool AllowLikes => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AllowLikes]);

        [IgnoreColumn()]
        public bool AllowPostIcon => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AllowPostIcon]);

        [IgnoreColumn()]
        public bool AllowRSS => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AllowRSS]);

        [IgnoreColumn()]
        public bool AllowScript => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AllowScript]);

        [IgnoreColumn()]
        public bool AllowSubscribe => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AllowSubscribe]);

        [IgnoreColumn()]
        public int AttachCount => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.AttachCount], 3);

        [IgnoreColumn()]
        public int AttachMaxSize => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.AttachMaxSize], 1000);

        [IgnoreColumn()]
        public string AttachTypeAllowed => Utilities.SafeConvertString(this.ForumSettings[ForumSettingKeys.AttachTypeAllowed], ".jpg,.gif,.png");

        [IgnoreColumn()]
        public bool AttachAllowBrowseSite => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AttachAllowBrowseSite]);

        [IgnoreColumn()]
        public int MaxAttachWidth => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.MaxAttachWidth], 800);

        [IgnoreColumn()]
        public int MaxAttachHeight => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.MaxAttachHeight], 800);

        [IgnoreColumn()]
        public bool AttachInsertAllowed => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AttachInsertAllowed]);

        [IgnoreColumn()]
        public bool ConvertingToJpegAllowed => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.ConvertingToJpegAllowed]);

        [IgnoreColumn()]
        public string EditorHeight => Utilities.SafeConvertString(this.ForumSettings[ForumSettingKeys.EditorHeight], "400");

        [IgnoreColumn()]
        public EditorTypes EditorMobile
        {
            get
            {
                EditorTypes parseValue;
                return Enum.TryParse(Utilities.SafeConvertString(this.ForumSettings[ForumSettingKeys.EditorMobile], EditorTypes.HTMLEDITORPROVIDER.ToString()), true, out parseValue)
                           ? parseValue
                           : EditorTypes.HTMLEDITORPROVIDER;
            }
        }

        [IgnoreColumn()]
        public EditorTypes EditorType
        {
            get
            {
                EditorTypes parseValue;
                return Enum.TryParse(Utilities.SafeConvertString(this.ForumSettings[ForumSettingKeys.EditorType], EditorTypes.HTMLEDITORPROVIDER.ToString()), true, out parseValue)
                           ? parseValue
                           : EditorTypes.HTMLEDITORPROVIDER;
            }
        }

        [IgnoreColumn()]
        public HTMLPermittedUsers EditorPermittedUsers
        {
            get
            {
                HTMLPermittedUsers parseValue;
                return Enum.TryParse(Utilities.SafeConvertString(this.ForumSettings[ForumSettingKeys.EditorPermittedUsers], "1"), true, out parseValue)
                           ? parseValue
                           : HTMLPermittedUsers.AuthenticatedUsers;
            }
        }

        [IgnoreColumn()]
        public string EditorWidth => Utilities.SafeConvertString(this.ForumSettings[ForumSettingKeys.EditorWidth], "100%");

        [IgnoreColumn()]
        public string EmailAddress => Utilities.SafeConvertString(this.ForumSettings[ForumSettingKeys.EmailAddress], string.Empty);

        [IgnoreColumn()]
        public bool IndexContent => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.IndexContent]);

        /// <summary>
        /// Indicates a moderated forum
        /// </summary>
        [IgnoreColumn()]
        public bool IsModerated => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.IsModerated]);

        [IgnoreColumn()]
        public int TopicsTemplateId => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.TopicsTemplateId]);

        [IgnoreColumn()]
        public int TopicTemplateId => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.TopicTemplateId]);

        [IgnoreColumn()]
        public int TopicFormId => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.TopicFormId]);

        [IgnoreColumn()]
        public int ReplyFormId => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.ReplyFormId]);

        [IgnoreColumn()]
        public int QuickReplyFormId => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.QuickReplyFormId]);

        [IgnoreColumn()]
        public int ProfileTemplateId => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.ProfileTemplateId]);

        [IgnoreColumn()]
        public bool UseFilter => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.UseFilter]);

        [IgnoreColumn()]
        public int AutoTrustLevel => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.AutoTrustLevel]);

        [IgnoreColumn()]
        public TrustTypes DefaultTrustValue
        {
            get
            {
                TrustTypes parseValue;
                return Enum.TryParse(Utilities.SafeConvertString(this.ForumSettings[ForumSettingKeys.DefaultTrustLevel], "0"), true, out parseValue)
                       ? parseValue
                       : TrustTypes.NotTrusted;
            }
        }

        [IgnoreColumn()]
        public int ModApproveTemplateId => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.ModApproveTemplateId]);

        [IgnoreColumn()]
        public int ModRejectTemplateId => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.ModRejectTemplateId]);

        [IgnoreColumn()]
        public int ModMoveTemplateId => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.ModMoveTemplateId]);

        [IgnoreColumn()]
        public int ModDeleteTemplateId => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.ModDeleteTemplateId]);

        [IgnoreColumn()]
        public int ModNotifyTemplateId => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.ModNotifyTemplateId]);

        [IgnoreColumn()]
        public bool AllowTags => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AllowTags]);

        [IgnoreColumn()]
        public bool AutoSubscribeEnabled => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AutoSubscribeEnabled]);

        [IgnoreColumn()]
        public string AutoSubscribeRoles => Utilities.SafeConvertString(this.ForumSettings[ForumSettingKeys.AutoSubscribeRoles], string.Empty);

        [IgnoreColumn()]
        public bool AutoSubscribeNewTopicsOnly => Utilities.SafeConvertBool(this.ForumSettings[ForumSettingKeys.AutoSubscribeNewTopicsOnly]);

        /// <summary>
        ///  Minimum posts required to create a topic in this forum if the user is not trusted
        /// </summary>
        [IgnoreColumn()]
        public int CreatePostCount => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.CreatePostCount]);

        /// <summary>
        /// Minimum posts required to reply to a topic in this forum if the user is not trusted
        /// </summary>
        [IgnoreColumn()]
        public int ReplyPostCount => Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.ReplyPostCount]);

        #region "Deprecated Methods"
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int LastPostLastPostID { get; set; }

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int LastPostParentPostID { get; set; }

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int CustomFieldType { get; set; }

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int AttachMaxHeight
        {
            get { return Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.AttachMaxHeight], 500); }
        }

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int AttachMaxWidth
        {
            get { return Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.AttachMaxWidth], 500); }
        }

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int EditorStyle
        {
            get { return Utilities.SafeConvertInt(this.ForumSettings[ForumSettingKeys.EditorStyle], 1); }
        }

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public string EditorToolBar
        {
            get { return Utilities.SafeConvertString(this.ForumSettings[ForumSettingKeys.EditorToolbar], "bold,italic,underline"); }
        }
        #endregion "Deprecated Methods"
        #endregion

        /// <inheritdoc/>
        [IgnoreColumn]
        public DotNetNuke.Services.Tokens.CacheLevel Cacheability
        {
            get
            {
                return DotNetNuke.Services.Tokens.CacheLevel.notCacheable;
            }
        }
        /// <inheritdoc/>
        [IgnoreColumn]
        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, DotNetNuke.Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {                
            var navigationManager = new Services.URLNavigator().NavigationManager();
            var portalSettings = Utilities.GetPortalSettings(this.PortalId);
            var mainSettings = SettingsBase.GetModuleSettings(this.ModuleId);
         
            propertyName = propertyName.ToLowerInvariant();

            if (propertyName.Contains("lastpostsubject:"))
            {
                if (this.LastPostID > 0)
                {
                    int intLength = 0;
                    if ((propertyName.ToString().IndexOf("[lastpostsubject:", 0) + 1) > 0)
                    {
                        int inStart = (propertyName.ToString().IndexOf("[lastpostsubject:", 0) + 1) + 17;
                        int inEnd = (propertyName.ToString().IndexOf("]", inStart - 1) + 1);
                        string sLength = propertyName.ToString().Substring(inStart - 1, inEnd - inStart);
                        intLength = Convert.ToInt32(sLength);
                    }

                    return PropertyAccess.FormatString(DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetLastPostSubjectLinkTag(this.LastPostID, this.LastTopicId, HttpUtility.HtmlDecode(this.LastPostSubject), intLength, this), format);
                }
            }

            switch (propertyName)
            {
                case "forumid":
                    return PropertyAccess.FormatString(this.ForumID.ToString(), format);
                case "themelocation":
                    return PropertyAccess.FormatString(this.ThemeLocation.ToString(), format);
                case "forumdescription":
                    return PropertyAccess.FormatString(this.ForumDesc, format);
                case "forumname":
                    return PropertyAccess.FormatString(this.ForumName, format);
                case "forumnamenolink":
                    return PropertyAccess.FormatString(this.ForumName, format);
                case "forumgroupid":
                    return PropertyAccess.FormatString(this.ForumGroupId.ToString(), format);
                case "forumgrouplink":
                    return PropertyAccess.FormatString(new ControlUtils().BuildUrl(this.TabId, this.ModuleId, this.ForumGroup.PrefixURL, string.Empty, this.ForumGroupId, -1, -1, -1, string.Empty, 1, -1, this.SocialGroupId), format);
                case "forumlink": 
                    return PropertyAccess.FormatString(navigationManager?.NavigateURL(this.TabId.ToString(), new[] { $"{ParamKeys.ForumId}={this.ForumID}", $"{ParamKeys.ViewType}={Views.Topics}" }), format);
                case "forumurl":
                    return PropertyAccess.FormatString(navigationManager?.NavigateURL(this.TabId.ToString(), new[] { $"{ParamKeys.ForumId}={this.ForumID}", $"{ParamKeys.ViewType}={Views.Topics}" }), format);
                case "parentforumlink":
                    return this.ParentForumId < 1 ? string.Empty : PropertyAccess.FormatString(navigationManager?.NavigateURL(this.TabId.ToString(), new[] { $"{ParamKeys.ForumId}={this.ParentForumId}", $"{ParamKeys.ViewType}={Views.Topics}" }), format);
                case "lastpostdate":
                    return this.LastPostID < 1 ? string.Empty : PropertyAccess.FormatString(Utilities.GetUserFormattedDateTime((DateTime?)this.LastPostDateTime, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow)), format);
                case "parentforumname":
                    return this.ParentForumId < 1 ? string.Empty : PropertyAccess.FormatString(this.ParentForumName, format);
                case "parentforumid":
                    return this.ParentForumId < 1 ? string.Empty : PropertyAccess.FormatString(this.ParentForumId.ToString(), format);
                case "groupname":
                    return PropertyAccess.FormatString(this.ForumGroup.GroupName, format);
                case "socialgroupid":
                    return PropertyAccess.FormatString(this.SocialGroupId.ToString(), format);
                case "lastpostid":
                    return PropertyAccess.FormatString(this.LastPostID.ToString(), format);
                case "subscribercount":
                    return PropertyAccess.FormatString(this.SubscriberCount.ToString(), format);
                case "totaltopics":
                    return PropertyAccess.FormatString(this.TotalTopics.ToString(), format);
                case "totalreplies":
                    return PropertyAccess.FormatString(this.TotalReplies.ToString(), format);
                case "lastpostsubject":
                    return this.LastPostID < 1 ? string.Empty : PropertyAccess.FormatString(this.LastPostSubject.ToString(), format);
                case "lastpostuserid":
                    return this.LastPostID < 1 ? string.Empty : PropertyAccess.FormatString(this.LastPostUserID.ToString(), format);
                case "lastpostusername":
                    return this.LastPostID < 1 ? string.Empty : PropertyAccess.FormatString(this.LastPostUserName.ToString(), format);
                case "lastpostfirstname":
                    return this.LastPostID < 1 ? string.Empty : PropertyAccess.FormatString(this.LastPostFirstName.ToString(), format);
                case "lastpostlastname":
                    return this.LastPostID < 1 ? string.Empty : PropertyAccess.FormatString(this.LastPostLastName.ToString(), format);
                case "lastpostdisplayname":
                    if (this.LastPostID < 1)
                    {
                        return string.Empty;
                    }
                    else if (this.LastPostUserID <= 0)
                    {
                        return PropertyAccess.FormatString(this.LastPostDisplayName, format);
                    }
                    else
                    {
                        bool isAdmin = accessingUser.IsAdmin || accessingUser.IsSuperUser;
                        bool isMod = isAdmin || this.GetIsMod(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(accessingUser.PortalID, accessingUser.UserID));
                        return PropertyAccess.FormatString(DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, this.ModuleId, true, isMod, isAdmin, this.LastPostUserID, this.LastPostUserName, this.LastPostFirstName, this.LastPostLastName, this.LastPostDisplayName), format);
                    }
                case "statuscssclass":
                    return PropertyAccess.FormatString(DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumStatusCss(this, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(accessingUser.PortalID, accessingUser.UserID)), format);
                case "forumicon":
                    return PropertyAccess.FormatString(DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumFolderIcon(this, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(accessingUser.PortalID, accessingUser.UserID), mainSettings), format);
                case "forumiconcss":
                    return PropertyAccess.FormatString(DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumFolderIconCss(this, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(accessingUser.PortalID, accessingUser.UserID)), format);

                //case "rsslink":
                //    return DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName((Utilities.GetPortalSettings(this.PortalId).DefaultPortalAlias) + $"/DesktopModules/ActiveForums/feeds.aspx?portalid={this.PortalId}&forumid={this.ForumID}&tabid={this.TabId}&moduleid={this.ModuleId}" + (this.SocialGroupId > 0 ? $"&GroupId={this.SocialGroupId}" : string.Empty);

            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
