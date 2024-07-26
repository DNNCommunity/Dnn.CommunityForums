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

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;
    using System.Collections;
    using System.Web.Caching;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Services.Log.EventLog;

    [TableName("activeforums_Groups")]
    [PrimaryKey("ForumGroupId", AutoIncrement = true)]
    [Scope("ModuleId")]

    // TODO [Cacheable("activeforums_Groups", CacheItemPriority.Low)] /* TODO: DAL2 caching cannot be used until all CRUD methods use DAL2; must update Save method to use DAL2 rather than stored procedure */
    public partial class ForumGroupInfo
    {
        public int ForumGroupId { get; set; }

        public int ModuleId { get; set; }

        public string GroupName { get; set; }

        public int SortOrder { get; set; }

        public bool Active { get; set; }

        public bool Hidden { get; set; }

        public string GroupSettingsKey { get; set; } = string.Empty;

        public int PermissionsId { get; set; } = -1;

        public string PrefixURL { get; set; } = string.Empty;

        #region Settings & Security

        private PermissionInfo security;
        private Hashtable groupSettings;

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo Security
        {
            get => this.security ?? (this.security = this.LoadSecurity());
            set => this.security = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo LoadSecurity()
        {
            var security = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(this.PermissionsId, this.ModuleId);
            if (security == null)
            {
                security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions();
                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                string message = String.Format(Utilities.GetSharedResource("[RESX:PermissionsMissingForForumGroup]"), this.PermissionsId, this.ForumGroupId);
                log.AddProperty("Message", message);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
            }

            return security;
        }

        [IgnoreColumn()]
        public Hashtable GroupSettings
        {
            get => this.groupSettings ?? (this.groupSettings = this.LoadSettings());
            set => this.groupSettings = value;
        }

        internal Hashtable LoadSettings()
        {
            return (Hashtable)DataCache.GetSettings(this.ModuleId, this.GroupSettingsKey, string.Format(CacheKeys.GroupSettingsByKey, this.ModuleId, this.GroupSettingsKey), true);
        }

        [IgnoreColumn()]
        public bool AllowAttach
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.AllowAttach]); }
        }

        [IgnoreColumn()]
        public bool AllowEmoticons
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.AllowEmoticons]); }
        }

        [IgnoreColumn()]
        public bool AllowHTML
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.AllowHTML]); }
        }

        [IgnoreColumn()]
        public bool AllowLikes
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.AllowLikes]); }
        }

        [IgnoreColumn()]
        public bool AllowPostIcon
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.AllowPostIcon]); }
        }

        [IgnoreColumn()]
        public bool AllowRSS
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.AllowRSS]); }
        }

        [IgnoreColumn()]
        public bool AllowScript
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.AllowScript]); }
        }

        [IgnoreColumn()]
        public int AttachCount
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.AttachCount], 3); }
        }

        [IgnoreColumn()]
        public int AttachMaxSize
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.AttachMaxSize], 1000); }
        }

        [IgnoreColumn()]
        public string AttachTypeAllowed
        {
            get { return Utilities.SafeConvertString(this.GroupSettings[ForumSettingKeys.AttachTypeAllowed], ".jpg,.gif,.png"); }
        }

        [IgnoreColumn()]
        public bool AttachAllowBrowseSite
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.AttachAllowBrowseSite]); }
        }

        [IgnoreColumn()]
        public int MaxAttachWidth
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.MaxAttachWidth], 800); }
        }

        [IgnoreColumn()]
        public int MaxAttachHeight
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.MaxAttachHeight], 800); }
        }

        [IgnoreColumn()]
        public bool AttachInsertAllowed
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.AttachInsertAllowed]); }
        }

        [IgnoreColumn()]
        public bool ConvertingToJpegAllowed
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.ConvertingToJpegAllowed]); }
        }

        [IgnoreColumn()]
        public string EditorHeight
        {
            get { return Utilities.SafeConvertString(this.GroupSettings[ForumSettingKeys.EditorHeight], "400"); }
        }

        [IgnoreColumn()]
        public HTMLPermittedUsers EditorPermittedUsers
        {
            get
            {
                HTMLPermittedUsers parseValue;
                return Enum.TryParse(Utilities.SafeConvertString(this.GroupSettings[ForumSettingKeys.EditorPermittedUsers], "1"), true, out parseValue)
                    ? parseValue
                    : HTMLPermittedUsers.AuthenticatedUsers;
            }
        }

        [IgnoreColumn()]
        public EditorTypes EditorType
        {
            get
            {
                EditorTypes parseValue;
                var val = Enum.TryParse(Utilities.SafeConvertString(this.GroupSettings[ForumSettingKeys.EditorType], EditorTypes.HTMLEDITORPROVIDER.ToString()), true, out parseValue)
                    ? parseValue
                    : EditorTypes.HTMLEDITORPROVIDER;
                return val;
            }
        }

        [IgnoreColumn()]
        public string EditorWidth
        {
            get { return Utilities.SafeConvertString(this.GroupSettings[ForumSettingKeys.EditorWidth], "100%"); }
        }

        [IgnoreColumn()]
        public EditorTypes EditorMobile
        {
            get
            {
                EditorTypes parseValue;
                var val = Enum.TryParse(Utilities.SafeConvertString(this.GroupSettings[ForumSettingKeys.EditorMobile], EditorTypes.HTMLEDITORPROVIDER.ToString()), true, out parseValue)
                    ? parseValue
                    : EditorTypes.HTMLEDITORPROVIDER;
                return val;
            }
        }

        [IgnoreColumn()]
        public string EmailAddress
        {
            get { return Utilities.SafeConvertString(this.GroupSettings[ForumSettingKeys.EmailAddress], string.Empty); }
        }

        [IgnoreColumn()]
        public bool IndexContent
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.IndexContent]); }
        }

        [IgnoreColumn()]
        public bool AutoSubscribeEnabled
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.AutoSubscribeEnabled]); }
        }

        [IgnoreColumn()]
        public string AutoSubscribeRoles
        {
            get { return Utilities.SafeConvertString(this.GroupSettings[ForumSettingKeys.AutoSubscribeRoles], string.Empty); }
        }

        [IgnoreColumn()]
        public bool AutoSubscribeNewTopicsOnly
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.AutoSubscribeNewTopicsOnly]); }
        }

        [IgnoreColumn()]
        public bool IsModerated
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.IsModerated]); }
        }

        [IgnoreColumn()]
        public int TopicsTemplateId
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.TopicsTemplateId]); }
        }

        [IgnoreColumn()]
        public int TopicTemplateId
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.TopicTemplateId]); }
        }

        [IgnoreColumn()]
        public int TopicFormId
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.TopicFormId]); }
        }

        [IgnoreColumn()]
        public int ReplyFormId
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.ReplyFormId]); }
        }

        /// <summary>
        /// TODO:
        /// </summary>
        [IgnoreColumn()]
        public int QuickReplyFormId
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.QuickReplyFormId]); }
        }

        [IgnoreColumn()]
        public int ProfileTemplateId
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.ProfileTemplateId]); }
        }

        [IgnoreColumn()]
        public bool UseFilter
        {
            get { return Utilities.SafeConvertBool(this.GroupSettings[ForumSettingKeys.UseFilter]); }
        }

        [IgnoreColumn()]
        public int AutoTrustLevel
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.AutoTrustLevel]); }
        }

        [IgnoreColumn()]
        public TrustTypes DefaultTrustValue
        {
            get
            {
                TrustTypes parseValue;
                return Enum.TryParse(Utilities.SafeConvertString(this.GroupSettings[ForumSettingKeys.DefaultTrustLevel], "0"), true, out parseValue)
                    ? parseValue
                    : TrustTypes.NotTrusted;
            }
        }

        [IgnoreColumn()]
        public int ModApproveTemplateId
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.ModApproveTemplateId]); }
        }

        [IgnoreColumn()]
        public int ModRejectTemplateId
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.ModRejectTemplateId]); }
        }

        [IgnoreColumn()]
        public int ModMoveTemplateId
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.ModMoveTemplateId]); }
        }

        [IgnoreColumn()]
        public int ModDeleteTemplateId
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.ModDeleteTemplateId]); }
        }

        [IgnoreColumn()]
        public int ModNotifyTemplateId
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.ModNotifyTemplateId]); }
        }

        [IgnoreColumn()]
        public int CreatePostCount // Minimum posts required to create a topic in this forum if the user is not trusted
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.CreatePostCount]); }
        }

        [IgnoreColumn()]
        public int ReplyPostCount // Minimum posts required to reply to a topic in this forum if the user is not trusted
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.ReplyPostCount]); }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int AttachMaxWidth
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.AttachMaxWidth], 500); }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int AttachMaxHeight
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.AttachMaxHeight], 500); }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int EditorStyle
        {
            get { return Utilities.SafeConvertInt(this.GroupSettings[ForumSettingKeys.EditorStyle], 1); }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public string EditorToolBar
        {
            get { return Utilities.SafeConvertString(this.GroupSettings[ForumSettingKeys.EditorToolbar], "bold,italic,underline"); }
        }
        #endregion
    }
}
