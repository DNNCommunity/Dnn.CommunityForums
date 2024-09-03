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
    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Modules;

    [TableName("activeforums_UserProfiles")]
    [PrimaryKey("ProfileId", AutoIncrement = true)]
    [Scope("PortalId")]
    public class ForumUserInfo
    {
        private DotNetNuke.Entities.Users.UserInfo userInfo;
        private PortalSettings portalSettings;
        private SettingsInfo mainSettings;
        private ModuleInfo moduleInfo;
        private string userRoles = Globals.DefaultAnonRoles + "|-1;||";

        public ForumUserInfo()
        {
            this.userInfo = new DotNetNuke.Entities.Users.UserInfo();
        }        

        public ForumUserInfo(int moduleId)
        {
            this.userInfo = new DotNetNuke.Entities.Users.UserInfo();
            this.ModuleId = moduleId;
        }

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

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

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

        [IgnoreColumn]
        public string[] Roles => this.UserInfo.Roles;

        [IgnoreColumn]
        public string FirstName => this.UserInfo.FirstName;

        [IgnoreColumn]
        public string LastName => this.UserInfo.LastName;

        [IgnoreColumn]
        public string DisplayName => this.UserInfo.DisplayName;

        [IgnoreColumn]
        public string Username => this.UserInfo.Username;

        [IgnoreColumn]
        public string Email => this.UserInfo.Email;

        [IgnoreColumn]
        public bool GetIsMod(int ModuleId)
        {
            return (!(string.IsNullOrEmpty(DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.UserRoles, this.PortalId, ModuleId, "CanApprove"))));
        }

        [IgnoreColumn]
        public bool IsSuperUser => this.UserInfo.IsSuperUser;

        [IgnoreColumn]
        public bool IsAdmin => this.UserInfo.IsAdmin;

        [IgnoreColumn]
        public bool IsAnonymous => this.UserId == -1;

        [IgnoreColumn]
        public bool IsUserOnline { get; set; }

        [IgnoreColumn]
        public CurrentUserTypes CurrentUserType { get; set; }

        [IgnoreColumn]
        public string ForumsAllowed { get; set; }

        [IgnoreColumn]
        public string UserForums { get; set; }

        [IgnoreColumn]
        public int PostCount => this.TopicCount + this.ReplyCount;

        [IgnoreColumn]
        public DotNetNuke.Entities.Profile.ProfilePropertyDefinitionCollection Properties => this.UserInfo.Profile.ProfileProperties;

        [IgnoreColumn]
        TimeSpan TimeZoneOffsetForUser => Utilities.GetTimeZoneOffsetForUser(this.UserInfo);

        public PortalSettings PortalSettings
        {
            get => this.portalSettings ?? (this.portalSettings = Utilities.GetPortalSettings(this.PortalId));
            set => this.portalSettings = value;
        }
        
        [IgnoreColumn]
        public SettingsInfo MainSettings
        {
            get => this.mainSettings ?? (this.mainSettings = SettingsBase.GetModuleSettings(this.ModuleId));
            set => this.mainSettings = value;
        }

        [IgnoreColumn]
        public ModuleInfo ModuleInfo
        {
            get => this.moduleInfo ?? (this.moduleInfo = this.LoadModuleInfo());
            set => this.moduleInfo = value;
        }

        internal ModuleInfo LoadModuleInfo()
        {
            return DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(this.ModuleId, DotNetNuke.Common.Utilities.Null.NullInteger, false);
        }

        [IgnoreColumn]
        public DotNetNuke.Entities.Users.UserInfo UserInfo
        {
            get => this.userInfo ?? (this.userInfo = DotNetNuke.Entities.Users.UserController.Instance.GetUser(this.PortalId, this.UserId));
            set => this.userInfo = value;
        }

        [IgnoreColumn]
        public string UserRoles
        {
            get
            {
                PortalSettings _portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings(this.PortalId);
                var ids = this.GetRoleIds(this.UserInfo, this.PortalId);
                if (string.IsNullOrEmpty(ids))
                {
                    ids = Globals.DefaultAnonRoles + "|-1;||";
                }
                if (this.IsSuperUser)
                {
                    ids += Globals.DefaultAnonRoles + _portalSettings.AdministratorRoleId + ";";
                }
                ids += "|" + this.UserId + "|" + string.Empty + "|";
                this.userRoles = ids;
                return this.userRoles;
            }

            set
            {
                this.userRoles = value;
            }
        }

        [IgnoreColumn]
        private string GetRoleIds(UserInfo u, int PortalId)
        {
            string RoleIds = string.Empty;
            foreach (DotNetNuke.Security.Roles.RoleInfo r in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: PortalId))
            {
                string roleName = r.RoleName;
                foreach (string role in u?.Roles)
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        if (roleName == role)
                        {
                            RoleIds += r.RoleID.ToString() + ";";
                            break;
                        }
                    }
                }
            }

            foreach (DotNetNuke.Security.Roles.RoleInfo r in u?.Social?.Roles)
            {
                RoleIds += r.RoleID.ToString() + ";";
            }

            return RoleIds;
        }
        [IgnoreColumn]
         internal int GetLastReplyRead(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
         {
             var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.UserId, ti.TopicId);
             var forumTrak = new DotNetNuke.Modules.ActiveForums.Controllers.ForumTrackingController().GetByUserIdForumId(this.UserId, ti.ForumId);
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

         [IgnoreColumn]
         internal int GetLastTopicRead(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
         {
             var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.UserId, ti.TopicId);
             var forumTrak = new DotNetNuke.Modules.ActiveForums.Controllers.ForumTrackingController().GetByUserIdForumId(this.UserId, ti.ForumId);
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
        
        [IgnoreColumn]
        internal bool GetIsTopicRead(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.UserId, ti.TopicId);
            if (topicTrak?.LastReplyId >= ti.LastReplyId)
            {
                return true;
            }

            return false;
        }

        [IgnoreColumn]
        internal bool GetIsReplyRead(DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.UserId, ri.TopicId);
            if (topicTrak?.LastReplyId >= ri.ReplyId)
            {
                return true;
            }

            return false;
        }

        [IgnoreColumn]
        internal int GetLastTopicRead(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi)
        {
            var forumTrak = new DotNetNuke.Modules.ActiveForums.Controllers.ForumTrackingController().GetByUserIdForumId(this.UserId, fi.ForumID);
            if (forumTrak != null)
            {
                return forumTrak.MaxTopicRead;
            }

            return 0;
        }

        [IgnoreColumn]
        internal int GetTopicReadCount(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetTopicsReadCountForUserForum(this.UserId, fi.ForumID);
        }

        [IgnoreColumn]
        internal int GetLastTopicReplyRead(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.UserId, ti.TopicId);
            if (topicTrak != null)
            {
                return topicTrak.LastReplyId;
            }

            return 0;
        }

         

    }
}
