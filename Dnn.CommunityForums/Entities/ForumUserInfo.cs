//
// Community Forums
// Copyright (c) 2013-2024
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
using DotNetNuke.ComponentModel.DataAnnotations;
using System.Collections;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.UI.UserControls;
using System.Text;

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_UserProfiles")]
    [PrimaryKey("ProfileId", AutoIncrement = true)]
    [Scope("PortalId")]
    public class ForumUserInfo
    {
        private DotNetNuke.Entities.Users.UserInfo _userInfo;
        private string _userRoles = Globals.DefaultAnonRoles + "|-1;||";
        private int moduleId = -1;

        public int ProfileId { get; set; }
        public int UserId { get; set; } = -1;
        public int ModuleId { set => moduleId = -1; get => moduleId; }// => moduleId = -1; } /* this is always -1 so getter is unreliable but required for PetaPoco*/
        public int PortalId { get; set; }
        public int TopicCount { get; set; }
        public int ReplyCount { get; set; }
        public int ViewCount { get; set; }
        public int AnswerCount { get; set; }
        public int RewardPoints { get; set; }
        public string UserCaption { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime DateLastActivity { get; set; }
        public DateTime DateLastPost { get; set; }
        public DateTime DateLastReply { get; set; }
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
        public string[] Roles => UserInfo.Roles;
        [IgnoreColumn]
        public string FirstName => UserInfo.FirstName;
        [IgnoreColumn] 
        public string LastName => UserInfo.LastName;
        [IgnoreColumn]
        public string DisplayName => UserInfo.DisplayName;
        [IgnoreColumn] 
        public string Username => UserInfo.Username;
        [IgnoreColumn] 
        public string Email => UserInfo.Email;
        
        [IgnoreColumn] 
        public bool GetIsMod(int ModuleId)
        {
            return (!(string.IsNullOrEmpty(DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(UserRoles, PortalId, ModuleId, "CanApprove"))));
        }
        [IgnoreColumn]
        public bool IsSuperUser => UserInfo.IsSuperUser;
        [IgnoreColumn]
        public bool IsAdmin => UserInfo.IsAdmin;
        [IgnoreColumn]
        public bool IsUserOnline { get; set; }
        [IgnoreColumn]
        public CurrentUserTypes CurrentUserType { get; set; }
        [IgnoreColumn]
        public string ForumsAllowed { get; set; }
        [IgnoreColumn]
        public string UserForums { get; set; }
        [IgnoreColumn]
        public int PostCount => TopicCount + ReplyCount;
        [IgnoreColumn]
        public DotNetNuke.Entities.Profile.ProfilePropertyDefinitionCollection Properties => UserInfo.Profile.ProfileProperties;
        [IgnoreColumn]
        TimeSpan TimeZoneOffsetForUser => Utilities.GetTimeZoneOffsetForUser(UserInfo);
        [IgnoreColumn]
        public DotNetNuke.Entities.Users.UserInfo UserInfo
        {
            get => _userInfo ?? (_userInfo = DotNetNuke.Entities.Users.UserController.Instance.GetUser(PortalId, UserId));
            set => _userInfo = value;
        }
        [IgnoreColumn]
        public string UserRoles
        {
            get
            {
                PortalSettings _portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings(PortalId);
                var ids = GetRoleIds(UserInfo, PortalId);
                if (string.IsNullOrEmpty(ids))
                {
                    ids = Globals.DefaultAnonRoles + "|-1;||";
                }
                if (IsSuperUser)
                {
                    ids += Globals.DefaultAnonRoles + _portalSettings.AdministratorRoleId + ";";
                }
                ids += "|" + UserId + "|" + string.Empty + "|";
                _userRoles = ids;
                return ids;
            }

            set
            {
                _userRoles = value;
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

    }
}
