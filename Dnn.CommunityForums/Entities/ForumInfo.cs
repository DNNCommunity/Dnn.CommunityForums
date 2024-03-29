//
// Community Forums
// Copyright (c) 2013-2021
// by DNN Community
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
//


using System;
using System.Collections;
using System.Collections.Generic;
using DotNetNuke.ComponentModel.DataAnnotations;
using System.Web.Caching;
using DotNetNuke.Modules.ActiveForums.Entities;
using System.Runtime.Remoting.Messaging;
using static DotNetNuke.Modules.ActiveForums.Services.Controllers.TopicController;
using System.Xml;
using DotNetNuke.Common.Controls;
using DotNetNuke.Modules.ActiveForums.Data;
using System.Linq;
using DotNetNuke.Modules.ActiveForums.API;

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_Forums")]
    [PrimaryKey("ForumID", AutoIncrement = true)] /* ForumID because needs to match property name NOT database column name */
    [Scope("ModuleId")]
    [Cacheable("activeforums_Forums", CacheItemPriority.Low)]
    public partial class ForumInfo
    {
        private DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo _forumGroup;
        private DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo _security;
        private List<PropertiesInfo> _properties;
        private List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> _subforums;
        private Hashtable forumSettings;

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
        [ColumnName("LastPostId")]
        public int LastPostID { get; set; }
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
        public DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo ForumGroup
        {
            get => _forumGroup ?? (_forumGroup = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetById(ForumGroupId));
            set => _forumGroup = value;
        }

        [IgnoreColumn()]
        public string GroupName => ForumGroup.GroupName;

        [IgnoreColumn()]
        public string LastTopicUrl { get; set; }

        [IgnoreColumn()]
        public DateTime LastRead { get; set; }

        [IgnoreColumn()]
        public string LastPostFirstName => (PortalId > 0 && LastPostUserID > 0) ? new DotNetNuke.Entities.Users.UserController().GetUser(PortalId, LastPostUserID).FirstName : string.Empty;

        [IgnoreColumn()]
        public string LastPostLastName => (PortalId > 0 && LastPostUserID > 0) ? new DotNetNuke.Entities.Users.UserController().GetUser(PortalId, LastPostUserID).LastName : string.Empty;
        
        [IgnoreColumn()]
        public string LastPostDisplayName => (PortalId > 0 && LastPostUserID > 0) ? new DotNetNuke.Entities.Users.UserController().GetUser(PortalId, LastPostUserID).DisplayName : string.Empty;

        [IgnoreColumn()]
        public bool InheritSecurity => this.PermissionsId == ForumGroup.PermissionsId;

        [IgnoreColumn()]
        public bool InheritSettings => this.ForumSettingsKey == ForumGroup.GroupSettingsKey;

        [IgnoreColumn()]
        public int SubscriberCount => new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Count(portalId: PortalId, moduleId: ModuleId, forumId: ForumID);

        [IgnoreColumn()]
        public string ParentForumName => ParentForumId > 0 ? DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForum(PortalId,ModuleId,ParentForumId).ForumName : string.Empty;
        
        [IgnoreColumn()]
        public int TabId => new DotNetNuke.Entities.Modules.ModuleController().GetModule(ModuleId).TabID;

        [IgnoreColumn()]
        public string ForumURL => URL.ForumLink(TabId, this);

        [IgnoreColumn()]
        public List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> SubForums 
        { 
            get => _subforums ?? (_subforums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(ModuleId).Where(f => f.ForumID == ParentForumId).ToList());
            set => _subforums = value; 
        }

        [IgnoreColumn()]
        public List<PropertiesInfo> Properties
        {
            get => _properties ?? (_properties = (HasProperties ? new PropertiesController().ListProperties(PortalId, 1, ForumID) : new List<PropertiesInfo>()));
            set => _properties = value;
        }

        #region "Settings"

        [IgnoreColumn()]
        public PermissionInfo Security
        {
            get => _security ?? (_security = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(PermissionsId));
            set => _security = value;
        }

        [IgnoreColumn()]
        public Hashtable ForumSettings
        {
            get => forumSettings ?? (forumSettings = (Hashtable)DataCache.GetSettings(ModuleId, ForumSettingsKey, string.Format(CacheKeys.ForumSettingsByKey, ModuleId, ForumSettingsKey), true));
            set => forumSettings = value;
        }

        [IgnoreColumn()]
        public bool AllowAttach => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowAttach]);

        [IgnoreColumn()]
        public bool AllowEmoticons => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowEmoticons]);

        [IgnoreColumn()]
        public bool AllowHTML => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowHTML]);

        [IgnoreColumn()]
        public bool AllowLikes => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowLikes]);

        [IgnoreColumn()]
        public bool AllowPostIcon => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowPostIcon]);

        [IgnoreColumn()]
        public bool AllowRSS => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowRSS]);

        [IgnoreColumn()]
        public bool AllowScript => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowScript]);

        [IgnoreColumn()]
        public bool AllowSubscribe => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowSubscribe]);

        [IgnoreColumn()]
        public int AttachCount => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.AttachCount], 3);

        [IgnoreColumn()]
        public int AttachMaxSize => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.AttachMaxSize], 1000);

        [IgnoreColumn()]
        public string AttachTypeAllowed => Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.AttachTypeAllowed], ".jpg,.gif,.png");

        [IgnoreColumn()]
        public bool AttachAllowBrowseSite => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AttachAllowBrowseSite]);

        [IgnoreColumn()]
        public int MaxAttachWidth => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.MaxAttachWidth], 800);

        [IgnoreColumn()]
        public int MaxAttachHeight => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.MaxAttachHeight], 800);

        [IgnoreColumn()]
        public bool AttachInsertAllowed => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AttachInsertAllowed]);
        
        [IgnoreColumn()]
        public bool ConvertingToJpegAllowed => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.ConvertingToJpegAllowed]);
        
        [IgnoreColumn()]
        public string EditorHeight => Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.EditorHeight], "400");
        
        [IgnoreColumn()]
        public EditorTypes EditorMobile
        {
            get
            {
                EditorTypes parseValue;
                return Enum.TryParse(Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.EditorMobile], EditorTypes.HTMLEDITORPROVIDER.ToString()), true, out parseValue)
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
                return Enum.TryParse(Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.EditorType], EditorTypes.HTMLEDITORPROVIDER.ToString()), true, out parseValue)
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
                return Enum.TryParse(Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.EditorPermittedUsers], "1"), true, out parseValue)
                           ? parseValue
                           : HTMLPermittedUsers.AuthenticatedUsers;
            }
        }
        [IgnoreColumn()]
        public string EditorWidth => Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.EditorWidth], "100%");
        
        [IgnoreColumn()]
        public string EmailAddress => Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.EmailAddress], string.Empty);
        
        [IgnoreColumn()]
        public bool IndexContent => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.IndexContent]);
        
        /// <summary>
        /// Indicates a moderated forum
        /// </summary>
        [IgnoreColumn()]
        public bool IsModerated => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.IsModerated]);
        
        [IgnoreColumn()]
        public int TopicsTemplateId => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.TopicsTemplateId]);
        
        [IgnoreColumn()]
        public int TopicTemplateId => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.TopicTemplateId]);
        
        [IgnoreColumn()]
        public int TopicFormId => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.TopicFormId]);
        
        [IgnoreColumn()]
        public int ReplyFormId => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ReplyFormId]);
        
        [IgnoreColumn()]
        public int QuickReplyFormId => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.QuickReplyFormId]);

        [IgnoreColumn()]        
        public int ProfileTemplateId => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ProfileTemplateId]);
        
        [IgnoreColumn()]
        public bool UseFilter => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.UseFilter]);
        
        [IgnoreColumn()]
        public int AutoTrustLevel => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.AutoTrustLevel]);
        
        [IgnoreColumn()]
        public TrustTypes DefaultTrustValue
        {
            get
            {
                TrustTypes parseValue;
                return Enum.TryParse(Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.DefaultTrustLevel], "0"), true, out parseValue)
                           ? parseValue
                           : TrustTypes.NotTrusted;
            }
        }

        [IgnoreColumn()]
        public int ModApproveTemplateId => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ModApproveTemplateId]);
        
        [IgnoreColumn()]
        public int ModRejectTemplateId => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ModRejectTemplateId]);
        
        [IgnoreColumn()]
        public int ModMoveTemplateId => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ModMoveTemplateId]);
        
        [IgnoreColumn()]
        public int ModDeleteTemplateId => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ModDeleteTemplateId]);
        
        [IgnoreColumn()]
        public int ModNotifyTemplateId => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ModNotifyTemplateId]);
        
        [IgnoreColumn()]
        public bool AllowTags => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowTags]);
        
        [IgnoreColumn()]
        public bool AutoSubscribeEnabled => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AutoSubscribeEnabled]);
        
        [IgnoreColumn()]
        public string AutoSubscribeRoles => Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.AutoSubscribeRoles], string.Empty);
        
        [IgnoreColumn()]
        public bool AutoSubscribeNewTopicsOnly => Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AutoSubscribeNewTopicsOnly]);
        
        /// <summary>
        ///  Minimum posts required to create a topic in this forum if the user is not trusted
        /// </summary>
        [IgnoreColumn()]
        public int CreatePostCount => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.CreatePostCount]);

        [IgnoreColumn()]
        /// <summary>
        /// Minimum posts required to reply to a topic in this forum if the user is not trusted
        /// </summary>
        public int ReplyPostCount => Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ReplyPostCount]);

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
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.AttachMaxHeight], 500); }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int AttachMaxWidth
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.AttachMaxWidth], 500); }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int EditorStyle
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.EditorStyle], 1); }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public string EditorToolBar
        {
            get { return Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.EditorToolbar], "bold,italic,underline"); }
        }
        #endregion
    }
}