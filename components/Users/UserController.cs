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

using System.Threading;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Modules.ActiveForums
{
    public class UserController
    {
        internal static int GetUserIdByUserName(int PortalId, string UserName)
        {
            try
            {
                DotNetNuke.Entities.Users.UserInfo user = DotNetNuke.Entities.Users.UserController.GetUserByName(PortalId, UserName);
                if (user != null)
                {
                    return user.UserID;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                return -1;

            }

            return -1;
        }
        public User GetUser(int PortalId, int ModuleId)
        {
            User u = null;
            if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                if (HttpContext.Current == null)
                {
                    u = DNNGetCurrentUser(PortalId, ModuleId);
                }
                else if ((HttpContext.Current.Items["AFUserInfo"]) != null)
                {
                    u = (User)(HttpContext.Current.Items["AFUserInfo"]);
                }
                else
                {
                    u = DNNGetCurrentUser(PortalId, ModuleId);
                }
                if (u != null)
                {
                    if (HttpContext.Current.Items["AFUserInfo"] == null)
                    {
                        HttpContext.Current.Items.Add("AFUserInfo", u);
                    }

                }
                return u;
            }
            else
            {
                return new User();
            }
        }
        public User DNNGetCurrentUser(int PortalId, int ModuleId)
        {
            DotNetNuke.Entities.Users.UserInfo cu = DotNetNuke.Entities.Users.UserController.Instance.GetCurrentUserInfo();
            User u = LoadUser(cu);

            u = FillProfile(PortalId, ModuleId, u);
            ForumController fc = new ForumController();
            string fs = fc.GetForumsForUser(u.UserRoles, PortalId, ModuleId, "CanApprove");
            if (!(string.IsNullOrEmpty(fs)))
            {
                u.Profile.IsMod = true;
            }
            else
            {
                u.Profile.IsMod = false;
            }

            return u;
        }
        private User GetDNNUser(int PortalId, int ModuleId, int userId)
        {
            DotNetNuke.Entities.Users.UserController uc = new DotNetNuke.Entities.Users.UserController();
            DotNetNuke.Entities.Users.UserInfo dnnUser = uc.GetUser(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId, userId);
            return LoadUser(dnnUser);
        }
        public User GetDNNUser(int PortalId, int ModuleId, string userName)
        {
            DotNetNuke.Entities.Users.UserInfo dnnUser = DotNetNuke.Entities.Users.UserController.GetUserByName(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId, userName);
            return LoadUser(dnnUser);
        }
        public User GetUser(int PortalId, int ModuleId, int userId)
        {
            User u = GetDNNUser(PortalId, ModuleId, userId);
            if (u != null)
            {
                u = FillProfile(PortalId, ModuleId, u);
                ForumController fc = new ForumController();
                string fs = fc.GetForumsForUser(u.UserRoles, PortalId, ModuleId, "CanApprove");
                if (!(string.IsNullOrEmpty(fs)))
                {
                    u.Profile.IsMod = true;
                }
                else
                {
                    u.Profile.IsMod = false;
                }
            }
            return u;
        }
        public User GetUser(int PortalId, int ModuleId, string userName)
        {
            User u = GetDNNUser(PortalId, ModuleId, userName);
            if (u != null)
            {
                u = FillProfile(PortalId, ModuleId, u);
                ForumController fc = new ForumController();
                string fs = fc.GetForumsForUser(u.UserRoles, PortalId, ModuleId, "CanApprove");
                if (!(string.IsNullOrEmpty(fs)) || u.IsSuperUser || u.IsAdmin)
                {
                    u.Profile.IsMod = true;
                }
                else
                {
                    u.Profile.IsMod = false;
                }
            }
            return u;
        }
        public User FillProfile(int PortalId, int ModuleId, User u)
        {
            if (u != null && u.UserId > 0)
            {
                u.Profile = Profiles_Get(PortalId, ModuleId, u.UserId);
            }
            return u;
        }
        // KR - added caching to profiles to skip the DB hit
        public UserProfileInfo Profiles_Get(int PortalId, int ModuleId, int UserId)
        {

            string cachekey = string.Format("AF-prof-{0}-{1}-{2}", UserId, PortalId, ModuleId);
            DataTable dt = null;
            UserProfileInfo upi = null;
            Data.Profiles db = new Data.Profiles();
            PortalSettings _portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
            if (PortalId == -1)
            {
                PortalId = _portalSettings.PortalId;
            }

            // see if it's in cache already
            object data = DataCache.CacheRetrieve(cachekey);

            if (data != null)
            {
                dt = (DataTable)data;
            }
            else
            {
                dt = DotNetNuke.Common.Globals.ConvertDataReaderToDataTable(db.Profiles_Get(PortalId, -1, UserId));
                DataCache.CacheStore(cachekey, dt);
            }

            foreach (DataRow row in dt.Rows)
            {
                upi = new UserProfileInfo();
                upi.PortalId = PortalId;
                upi.UserID = UserId;
                upi.ModuleId = -1;
                upi.AdminWatch = bool.Parse(row["AdminWatch"].ToString());
                upi.AnswerCount = int.Parse(row["AnswerCount"].ToString());
                upi.AOL = row["AOL"].ToString();
                upi.AttachDisabled = bool.Parse(row["AttachDisabled"].ToString());
                upi.Avatar = row["Avatar"].ToString();
                upi.AvatarDisabled = bool.Parse(row["AvatarDisabled"].ToString());
                upi.AvatarType = (AvatarTypes)(int.Parse(row["AvatarType"].ToString()));
                upi.Badges = row["Badges"].ToString();
                //.Bio = dr("Bio").ToString
                upi.DateLastActivity = DateTime.Parse(row["DateLastActivity"].ToString());
                upi.DateLastPost = DateTime.Parse(row["DateLastPost"].ToString());
                upi.ForumsAllowed = string.Empty;
                upi.ICQ = row["ICQ"].ToString();
                upi.Interests = row["Interests"].ToString();
                upi.IsUserOnline = Convert.ToBoolean(((int.Parse(row["IsUserOnline"].ToString()) == 1) ? 1 : 0));
                upi.Location = row["Location"].ToString();
                upi.MSN = row["MSN"].ToString();
                upi.Occupation = row["Occupation"].ToString();
                upi.PrefBlockAvatars = bool.Parse(row["PrefBlockAvatars"].ToString());
                upi.PrefBlockSignatures = bool.Parse(row["PrefBlockSignatures"].ToString());
                upi.PrefDefaultShowReplies = bool.Parse(row["PrefDefaultShowReplies"].ToString());
                upi.PrefDefaultSort = row["PrefDefaultSort"].ToString();
                upi.PrefJumpLastPost = bool.Parse(row["PrefJumpLastPost"].ToString());
                upi.PrefPageSize = int.Parse(row["PrefPageSize"].ToString());
                upi.PrefSubscriptionType = (SubscriptionTypes)(int.Parse(row["PrefSubscriptionType"].ToString()));
                upi.PrefTopicSubscribe = bool.Parse(row["PrefTopicSubscribe"].ToString());
                upi.PrefUseAjax = bool.Parse(row["PrefUseAjax"].ToString());
                upi.ProfileId = int.Parse(row["ProfileId"].ToString());
                upi.ReplyCount = int.Parse(row["ReplyCount"].ToString());
                upi.RewardPoints = int.Parse(row["RewardPoints"].ToString());
                upi.Signature = row["Signature"].ToString();
                upi.SignatureDisabled = bool.Parse(row["SignatureDisabled"].ToString());
                upi.TopicCount = int.Parse(row["TopicCount"].ToString());
                upi.TrustLevel = int.Parse(row["TrustLevel"].ToString());
                upi.UserCaption = row["UserCaption"].ToString();
                upi.ViewCount = int.Parse(row["ViewCount"].ToString());
                upi.WebSite = row["WebSite"].ToString();
                upi.Yahoo = row["Yahoo"].ToString();
                upi.DateCreated = DateTime.Parse(row["DateCreated"].ToString());

            }

            return upi;
        }
        internal User LoadUser(DotNetNuke.Entities.Users.UserInfo dnnUser)
        {
            PortalSettings _portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
            User u = new User
            {
                UserId = dnnUser.UserID,
                UserName = dnnUser.Username,
                IsSuperUser = dnnUser.IsSuperUser,
                IsAdmin = dnnUser.IsInRole(_portalSettings.AdministratorRoleName),
                DateCreated = dnnUser.Membership.CreatedDate,
                DateUpdated = dnnUser.Membership.LastActivityDate,
                FirstName = dnnUser.FirstName,
                LastName = dnnUser.LastName,
                DisplayName = dnnUser.DisplayName,
                Email = dnnUser.Email,
                UserRoles = GetRoleIds(dnnUser, _portalSettings.PortalId)
            };

            if (dnnUser.IsSuperUser)
            {
                u.UserRoles += Globals.DefaultAnonRoles + _portalSettings.AdministratorRoleId + ";";
            }
            u.UserRoles += "|" + dnnUser.UserID + "|" + string.Empty + "|";

            if (!dnnUser.IsSuperUser)
            {
                u.Properties = GetUserProperties(dnnUser);
            }

            return u;
        }

        private string GetRoleIds(UserInfo u, int PortalId)
        {
            string RoleIds = string.Empty;
            DotNetNuke.Security.Roles.RoleController rc = new DotNetNuke.Security.Roles.RoleController();
            foreach (DotNetNuke.Security.Roles.RoleInfo r in rc.GetPortalRoles(PortalId))
            {
                string roleName = r.RoleName;
                foreach (string role in u.Roles)
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

            foreach (DotNetNuke.Security.Roles.RoleInfo r in u.Social.Roles)
            {
                RoleIds += r.RoleID.ToString() + ";";
            }

            return RoleIds;
        }
        public Hashtable GetUserProperties(DotNetNuke.Entities.Users.UserInfo dnnUser)
        {
            Hashtable ht = new Hashtable();
            foreach (DotNetNuke.Entities.Profile.ProfilePropertyDefinition up in dnnUser.Profile.ProfileProperties)
            {
                ht.Add(up.PropertyName, up.PropertyValue);
            }
            return ht;
        }
    }
}