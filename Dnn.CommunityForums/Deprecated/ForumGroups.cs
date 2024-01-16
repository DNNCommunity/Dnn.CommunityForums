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
using System.Data;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Modules.ActiveForums
{

	public partial class ForumGroupInfo
	{
		public ForumGroupInfo()
		{
			Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo();
			PermissionsId = -1;
			PrefixURL = string.Empty;
			GroupSettings = new Hashtable();
		}

		public int ForumGroupId { get; set; }
        public int ModuleId { get; set; }
        public string GroupName { get; set; }
        public int UserID { get; set; }
        public int SortOrder { get; set; }
        public int PortalID { get; set; }
        public bool Hidden { get; set; }
        public bool Active { get; set; }
        public string GroupSettingsKey { get; set; }
        public int PermissionsId { get; set; }
        public string PrefixURL { get; set; }

		#region Settings & Security

		public DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo Security { get; set; }
        public Hashtable GroupSettings { get; set; }

		public bool AllowAttach
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.AllowAttach]); }
		}

		public bool AllowEmoticons
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.AllowEmoticons]); }
		}

		public bool AllowHTML
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.AllowHTML]); }
		}

        public bool AllowLikes
        {
            get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.AllowLikes]); }
        }

        public bool AllowPostIcon
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.AllowPostIcon]); }
		}

		public bool AllowRSS
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.AllowRSS]); }
		}
		public bool AllowScript
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.AllowScript]); }
		}

		public bool AllowSubscribe
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.AllowSubscribe]); }
		}

		public int AttachCount
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.AttachCount], 3); }
		}

		public int AttachMaxHeight
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.AttachMaxHeight], 500); }
		}

		public int AttachMaxSize
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.AttachMaxSize], 1000); }
		}

		public int AttachMaxWidth
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.AttachMaxWidth], 500); }
		}

		public string AttachTypeAllowed
		{
			get { return Utilities.SafeConvertString(GroupSettings[ForumSettingKeys.AttachTypeAllowed], ".jpg,.gif,.png"); }
		}

		public bool AttachAllowBrowseSite
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.AttachAllowBrowseSite]); }
		}

		public int MaxAttachWidth
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.MaxAttachWidth], 800); }
		}

		public int MaxAttachHeight
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.MaxAttachHeight], 800); }
		}

		public bool AttachInsertAllowed
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.AttachInsertAllowed]); }
		}

		public bool ConvertingToJpegAllowed
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.ConvertingToJpegAllowed]); }
		}

		public string EditorHeight
		{
			get { return Utilities.SafeConvertString(GroupSettings[ForumSettingKeys.EditorHeight], "400"); }
		}

		public int EditorStyle
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.EditorStyle], 1); }
		}

		public HTMLPermittedUsers EditorPermittedUsers
		{
			get
			{
				HTMLPermittedUsers parseValue;
				return Enum.TryParse(Utilities.SafeConvertString(GroupSettings[ForumSettingKeys.EditorPermittedUsers], "1"), true, out parseValue)
						   ? parseValue
						   : HTMLPermittedUsers.AuthenticatedUsers;
			}
		}

		public string EditorToolBar
		{
			get { return Utilities.SafeConvertString(GroupSettings[ForumSettingKeys.EditorToolbar], "bold,italic,underline"); }
		}

		public EditorTypes EditorType
		{
			get
			{
				EditorTypes parseValue;
				var val = Enum.TryParse(Utilities.SafeConvertString(GroupSettings[ForumSettingKeys.EditorType], EditorTypes.HTMLEDITORPROVIDER.ToString()), true, out parseValue)
						   ? parseValue
						   : EditorTypes.HTMLEDITORPROVIDER;

				return val;
            }
		}

		public string EditorWidth
		{
			get { return Utilities.SafeConvertString(GroupSettings[ForumSettingKeys.EditorWidth], "100%"); }
		}

        public EditorTypes EditorMobile
        {
            get
            {
                EditorTypes parseValue;
                var val = Enum.TryParse(Utilities.SafeConvertString(GroupSettings[ForumSettingKeys.EditorMobile], EditorTypes.HTMLEDITORPROVIDER.ToString()), true, out parseValue)
                           ? parseValue
                           : EditorTypes.HTMLEDITORPROVIDER;

                return val;
            }
        }

		public string EmailAddress
		{
			get { return Utilities.SafeConvertString(GroupSettings[ForumSettingKeys.EmailAddress], string.Empty); }
		}

		public bool IndexContent
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.IndexContent]); }
		}

		public bool AutoSubscribeEnabled
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.AutoSubscribeEnabled]); }
		}

		public string AutoSubscribeRoles
		{
			get { return Utilities.SafeConvertString(GroupSettings[ForumSettingKeys.AutoSubscribeRoles], string.Empty); }
		}

		public bool AutoSubscribeNewTopicsOnly
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.AutoSubscribeNewTopicsOnly]); }
		}

		public bool IsModerated
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.IsModerated]); }
		}

		public int TopicsTemplateId
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.TopicsTemplateId]); }
		}

		public int TopicTemplateId
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.TopicTemplateId]); }
		}

		public int TopicFormId
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.TopicFormId]); }
		}

		public int ReplyFormId
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.ReplyFormId]); }
		}

		public int QuickReplyFormId
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.QuickReplyFormId]); }
		}

		public int ProfileTemplateId
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.ProfileTemplateId]); }
		}

		public bool UseFilter
		{
			get { return Utilities.SafeConvertBool(GroupSettings[ForumSettingKeys.UseFilter]); }
		}

		public int AutoTrustLevel
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.AutoTrustLevel]); }
		}

		public TrustTypes DefaultTrustValue
		{
			get
			{
				TrustTypes parseValue;
				return Enum.TryParse(Utilities.SafeConvertString(GroupSettings[ForumSettingKeys.DefaultTrustLevel], "0"), true, out parseValue)
						   ? parseValue
						   : TrustTypes.NotTrusted;
			}
		}

		public int ModApproveTemplateId
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.ModApproveTemplateId]); }
		}

		public int ModRejectTemplateId
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.ModRejectTemplateId]); }
		}

		public int ModMoveTemplateId
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.ModMoveTemplateId]); }
		}

		public int ModDeleteTemplateId
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.ModDeleteTemplateId]); }
		}

		public int ModNotifyTemplateId
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.ModNotifyTemplateId]); }
		}

		public int CreatePostCount // Minimum posts required to create a topic in this forum if the user is not trusted
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.CreatePostCount]); }
		}

		public int ReplyPostCount // Minimum posts required to reply to a topic in this forum if the user is not trusted
		{
			get { return Utilities.SafeConvertInt(GroupSettings[ForumSettingKeys.ReplyPostCount]); }
		}
        #endregion
    }

}