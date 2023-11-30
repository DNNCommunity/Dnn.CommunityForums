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

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_Forums")]
    [PrimaryKey("ForumId", AutoIncrement = true)]
    [Scope("ModuleId")]
    [Cacheable("activeforums_Forums", CacheItemPriority.Low)]
    public partial class ForumInfo
    {
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
        public string ForumSecurityKey { get; set; }
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
        public DateTime LastRead { get; set; }
        [IgnoreColumn()]
        public string LastPostFirstName { get; set; }
        [IgnoreColumn()]
        public string LastPostLastName { get; set; }
        [IgnoreColumn()]
        public string LastPostDisplayName { get; set; }
        [IgnoreColumn()]
        public bool InheritSecurity { get; set; }
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo ForumGroup { get; set; }
        [IgnoreColumn()]
        public int SubscriberCount { get; set; }
        [IgnoreColumn()]
        public string GroupName { get; set; }
        [IgnoreColumn()]
        public string ParentForumName { get; set; }
        [IgnoreColumn()]
        public int TabId { get; set; }

        [IgnoreColumn()]
        public string ForumURL
        {
            get
            {
                return URL.ForumLink(TabId, this);
            }
        }
        [IgnoreColumn()]
        public string TopicUrl { get; set; }
        [IgnoreColumn()]
        public ForumCollection SubForums { get; set; }
        [IgnoreColumn()]
        public List<PropertiesInfo> Properties { get; set; }

        #region Settings & Security
        [IgnoreColumn()]
        public PermissionInfo Security { get; set; }
        [IgnoreColumn()]
        public Hashtable ForumSettings { get; set; }

        #endregion


        // initialization
        public ForumInfo()
        {
            PortalId = -1;
            TabId = -1;
            PermissionsId = -1;
            PrefixURL = string.Empty;
            ForumSettings = new Hashtable();

            Security = new DotNetNuke.Modules.ActiveForums.PermissionInfo();
        }
        [IgnoreColumn()]
        public string TopicSubject { get; set; }
        [IgnoreColumn()]
        public int TopicId { get; set; }
        [IgnoreColumn()]
        public bool AllowAttach
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowAttach]); }
        }
        [IgnoreColumn()]
        public bool AllowEmoticons
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowEmoticons]); }
        }
        [IgnoreColumn()]
        public bool AllowHTML
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowHTML]); }
        }
        [IgnoreColumn()]
        public bool AllowLikes
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowLikes]); }
        }
        [IgnoreColumn()]
        public bool AllowPostIcon
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowPostIcon]); }
        }
        [IgnoreColumn()]
        public bool AllowRSS
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowRSS]); }
        }
        [IgnoreColumn()]
        public bool AllowScript
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowScript]); }
        }
        [IgnoreColumn()]
        public bool AllowSubscribe
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowSubscribe]); }
        }
        [IgnoreColumn()]
        public int AttachCount
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.AttachCount], 3); }
        }
        [IgnoreColumn()]
        public int AttachMaxSize
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.AttachMaxSize], 1000); }
        }
        [IgnoreColumn()]
        public string AttachTypeAllowed
        {
            get { return Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.AttachTypeAllowed], ".jpg,.gif,.png"); }
        }
        [IgnoreColumn()]
        public bool AttachAllowBrowseSite
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AttachAllowBrowseSite]); }
        }
        [IgnoreColumn()]
        public int MaxAttachWidth
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.MaxAttachWidth], 800); }
        }
        [IgnoreColumn()]
        public int MaxAttachHeight
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.MaxAttachHeight], 800); }
        }
        [IgnoreColumn()]
        public bool AttachInsertAllowed
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AttachInsertAllowed]); }
        }
        [IgnoreColumn()]
        public bool ConvertingToJpegAllowed
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.ConvertingToJpegAllowed]); }
        }
        [IgnoreColumn()]
        public string EditorHeight
        {
            get { return Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.EditorHeight], "400"); }
        }
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
        public string EditorWidth
        {
            get { return Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.EditorWidth], "100%"); }
        }
        [IgnoreColumn()]
        public string EmailAddress
        {
            get { return Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.EmailAddress], string.Empty); }
        }
        [IgnoreColumn()]
        public bool IndexContent
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.IndexContent]); }
        }
        [IgnoreColumn()]
        public bool IsModerated
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.IsModerated]); }
        }
        [IgnoreColumn()]
        public int TopicsTemplateId
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.TopicsTemplateId]); }
        }
        [IgnoreColumn()]
        public int TopicTemplateId
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.TopicTemplateId]); }
        }
        [IgnoreColumn()]
        public int TopicFormId
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.TopicFormId]); }
        }
        [IgnoreColumn()]
        public int ReplyFormId
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ReplyFormId]); }
        }
        [IgnoreColumn()]
        /// TODO
        public int QuickReplyFormId
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.QuickReplyFormId]); }
        }
        [IgnoreColumn()]
        public int ProfileTemplateId
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ProfileTemplateId]); }
        }
        [IgnoreColumn()]
        public bool UseFilter
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.UseFilter]); }
        }
        [IgnoreColumn()]
        public int AutoTrustLevel
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.AutoTrustLevel]); }
        }
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
        public int ModApproveTemplateId
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ModApproveTemplateId]); }
        }
        [IgnoreColumn()]
        public int ModRejectTemplateId
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ModRejectTemplateId]); }
        }
        [IgnoreColumn()]
        public int ModMoveTemplateId
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ModMoveTemplateId]); }
        }
        [IgnoreColumn()]
        public int ModDeleteTemplateId
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ModDeleteTemplateId]); }
        }
        [IgnoreColumn()]
        public int ModNotifyTemplateId
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ModNotifyTemplateId]); }
        }
        [IgnoreColumn()]
        public bool AllowTags
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AllowTags]); }
        }
        [IgnoreColumn()]
        public bool AutoSubscribeEnabled
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AutoSubscribeEnabled]); }
        }
        [IgnoreColumn()]
        public string AutoSubscribeRoles
        {
            get { return Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.AutoSubscribeRoles], string.Empty); }
        }
        [IgnoreColumn()]
        public bool AutoSubscribeNewTopicsOnly
        {
            get { return Utilities.SafeConvertBool(ForumSettings[ForumSettingKeys.AutoSubscribeNewTopicsOnly]); }
        }
        [IgnoreColumn()]
        public int CreatePostCount // Minimum posts required to create a topic in this forum if the user is not trusted
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.CreatePostCount]); }
        }
        [IgnoreColumn()]
        public int ReplyPostCount // Minimum posts required to reply to a topic in this forum if the user is not trusted
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.ReplyPostCount]); }
        }

        public static explicit operator ForumInfo(Modules.ActiveForums.Forum v)
        {
            throw new NotImplementedException();
        }
    }
}