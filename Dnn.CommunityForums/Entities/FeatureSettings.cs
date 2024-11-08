﻿namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;
    using System.Collections;

    using DotNetNuke.ComponentModel.DataAnnotations;

    public class FeatureSettings
    {
        private readonly Hashtable featureSettings;

        public FeatureSettings(int moduleId, string settingsKey)
        {
            this.featureSettings = DataCache.GetSettings(moduleId, settingsKey, string.Format(CacheKeys.ForumSettingsByKey, moduleId, settingsKey), true);;
        }

        public FeatureSettings(Hashtable featureSettings)
        {
            this.featureSettings = featureSettings;
        }

        [IgnoreColumn]
        public bool AllowAttach => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AllowAttach]);

        [IgnoreColumn]
        public bool AllowEmoticons => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AllowEmoticons]);

        [IgnoreColumn]
        public bool AllowHTML => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AllowHTML]);

        [IgnoreColumn]
        public bool AllowLikes => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AllowLikes]);

        [IgnoreColumn]
        public bool AllowPostIcon => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AllowPostIcon]);

        [IgnoreColumn]
        public bool AllowRSS => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AllowRSS]);

        [IgnoreColumn]
        public bool AllowScript => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AllowScript]);

        [IgnoreColumn]
        public bool AllowSubscribe => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AllowSubscribe]);

        [IgnoreColumn]
        public int AttachCount => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.AttachCount], 3);

        [IgnoreColumn]
        public int AttachMaxSize => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.AttachMaxSize], 1000);

        [IgnoreColumn]
        public string AttachTypeAllowed => Utilities.SafeConvertString(this.featureSettings[ForumSettingKeys.AttachTypeAllowed], ".jpg,.gif,.png");

        [IgnoreColumn]
        public bool AttachAllowBrowseSite => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AttachAllowBrowseSite]);

        [IgnoreColumn]
        public int MaxAttachWidth => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.MaxAttachWidth], 800);

        [IgnoreColumn]
        public int MaxAttachHeight => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.MaxAttachHeight], 800);

        [IgnoreColumn]
        public bool AttachInsertAllowed => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AttachInsertAllowed]);

        [IgnoreColumn]
        public bool ConvertingToJpegAllowed => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.ConvertingToJpegAllowed]);

        [IgnoreColumn]
        public string EditorHeight => Utilities.SafeConvertString(this.featureSettings[ForumSettingKeys.EditorHeight], "400");

        [IgnoreColumn]
        public EditorTypes EditorMobile
        {
            get
            {
                EditorTypes parseValue;
                return Enum.TryParse(Utilities.SafeConvertString(this.featureSettings[ForumSettingKeys.EditorMobile], EditorTypes.HTMLEDITORPROVIDER.ToString()), true, out parseValue)
                    ? parseValue
                    : EditorTypes.HTMLEDITORPROVIDER;
            }
        }

        [IgnoreColumn]
        public EditorTypes EditorType
        {
            get
            {
                EditorTypes parseValue;
                return Enum.TryParse(Utilities.SafeConvertString(this.featureSettings[ForumSettingKeys.EditorType], EditorTypes.HTMLEDITORPROVIDER.ToString()), true, out parseValue)
                    ? parseValue
                    : EditorTypes.HTMLEDITORPROVIDER;
            }
        }

        [IgnoreColumn]
        public HTMLPermittedUsers EditorPermittedUsers
        {
            get
            {
                HTMLPermittedUsers parseValue;
                return Enum.TryParse(Utilities.SafeConvertString(this.featureSettings[ForumSettingKeys.EditorPermittedUsers], "1"), true, out parseValue)
                    ? parseValue
                    : HTMLPermittedUsers.AuthenticatedUsers;
            }
        }

        [IgnoreColumn]
        public string EditorWidth => Utilities.SafeConvertString(this.featureSettings[ForumSettingKeys.EditorWidth], "100%");

        [IgnoreColumn]
        public string EmailAddress => Utilities.SafeConvertString(this.featureSettings[ForumSettingKeys.EmailAddress], string.Empty);

        [IgnoreColumn]
        public bool IndexContent => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.IndexContent]);

        /// <summary>
        /// Indicates a moderated forum
        /// </summary>
        [IgnoreColumn]
        public bool IsModerated => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.IsModerated]);

        [IgnoreColumn]
        public int TopicsTemplateId => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.TopicsTemplateId]);

        [IgnoreColumn]
        public int TopicTemplateId => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.TopicTemplateId]);

        [IgnoreColumn]
        public int TopicFormId => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.TopicFormId]);

        [IgnoreColumn]
        public int ReplyFormId => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.ReplyFormId]);

        [IgnoreColumn]
        public int QuickReplyFormId => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.QuickReplyFormId]);

        [IgnoreColumn]
        public int ProfileTemplateId => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.ProfileTemplateId]);

        [IgnoreColumn]
        public bool UseFilter => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.UseFilter]);

        [IgnoreColumn]
        public int AutoTrustLevel => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.AutoTrustLevel]);

        [IgnoreColumn]
        public TrustTypes DefaultTrustValue
        {
            get
            {
                TrustTypes parseValue;
                return Enum.TryParse(Utilities.SafeConvertString(this.featureSettings[ForumSettingKeys.DefaultTrustLevel], "0"), true, out parseValue)
                    ? parseValue
                    : TrustTypes.NotTrusted;
            }
        }

        [IgnoreColumn]
        public int ModApproveTemplateId => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.ModApproveTemplateId]);

        [IgnoreColumn]
        public int ModRejectTemplateId => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.ModRejectTemplateId]);

        [IgnoreColumn]
        public int ModMoveTemplateId => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.ModMoveTemplateId]);

        [IgnoreColumn]
        public int ModDeleteTemplateId => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.ModDeleteTemplateId]);

        [IgnoreColumn]
        public int ModNotifyTemplateId => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.ModNotifyTemplateId]);

        [IgnoreColumn]
        public bool AllowTags => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AllowTags]);

        [IgnoreColumn]
        public bool AutoSubscribeEnabled => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AutoSubscribeEnabled]);

        [IgnoreColumn]
        public string AutoSubscribeRoles => Utilities.SafeConvertString(this.featureSettings[ForumSettingKeys.AutoSubscribeRoles], string.Empty);

        [IgnoreColumn]
        public bool AutoSubscribeNewTopicsOnly => Utilities.SafeConvertBool(this.featureSettings[ForumSettingKeys.AutoSubscribeNewTopicsOnly]);

        /// <summary>
        ///  Minimum posts required to create a topic in this forum if the user is not trusted
        /// </summary>
        [IgnoreColumn]
        public int CreatePostCount => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.CreatePostCount]);

        /// <summary>
        /// Minimum posts required to reply to a topic in this forum if the user is not trusted
        /// </summary>
        [IgnoreColumn]
        public int ReplyPostCount => Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.ReplyPostCount]);

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn]
        public int AttachMaxHeight
        {
            get { return Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.AttachMaxHeight], 500); }
        }

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn]
        public int AttachMaxWidth
        {
            get { return Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.AttachMaxWidth], 500); }
        }

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn]
        public int EditorStyle
        {
            get { return Utilities.SafeConvertInt(this.featureSettings[ForumSettingKeys.EditorStyle], 1); }
        }

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn]
        public string EditorToolBar
        {
            get { return Utilities.SafeConvertString(this.featureSettings[ForumSettingKeys.EditorToolbar], "bold,italic,underline"); }
        }

    }
}
