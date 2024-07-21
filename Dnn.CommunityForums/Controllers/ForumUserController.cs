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
// 
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.ActiveForums.API;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Services.Journal;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.UI.UserControls;
using Microsoft.ApplicationBlocks.Data;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal class ForumUserController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetById(int UserId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user = null;
            if (UserId > 0)
            {   
                user = base.GetById(UserId);
            };
            if (user != null)
            { 
                user.UserInfo = DotNetNuke.Entities.Users.UserController.GetUserById(portalId: user.PortalId, userId: UserId);
            }
            else
            {
                user = new DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo
                {
                    UserID = -1,
                    TopicCount = 0,
                    ReplyCount = 0,
                    DateCreated = DateTime.UtcNow,
                    UserInfo = new DotNetNuke.Entities.Users.UserInfo
                    {
                        UserID = -1,
                        Username = "guest"
                    }
                };
            }
            return user;
        }
        public static void ClearCache(int UserId)
        {
            DataCache.UserCacheClear(string.Format(CacheKeys.ForumUser,UserId));
        }
        internal static int GetUserIdByUserName(int PortalId, string UserName)
        {
            try
            {
                DotNetNuke.Entities.Users.UserInfo user = DotNetNuke.Entities.Users.UserController.GetUserByName(PortalId, UserName);
                return user != null ? user.UserID : -1;
            }
            catch (Exception ex)
            {
                return -1;

            }
        }
        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetUser(int PortalId, int ModuleId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo u = null;
            if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                if (HttpContext.Current == null)
                {
                    u = DNNGetCurrentUser(PortalId, ModuleId);
                }
                else if ((HttpContext.Current.Items["DCFForumUserInfo"]) != null)
                {
                    u = (DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo)(HttpContext.Current.Items["DCFForumUserInfo"]);
                    u = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetById(u.UserID);
                }
                else
                {
                    u = DNNGetCurrentUser(PortalId, ModuleId);
                }
                if (u != null)
                {
                    if (HttpContext.Current.Items["DCFForumUserInfo"] == null)
                    {
                        HttpContext.Current.Items.Add("DCFForumUserInfo", u);
                    }

                }
                return u;
            }
            else
            {
                return new DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo();
            }
        }
        internal DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo DNNGetCurrentUser(int PortalId, int ModuleId)
        {
            DotNetNuke.Entities.Users.UserInfo cu = DotNetNuke.Entities.Users.UserController.Instance.GetCurrentUserInfo();
            return new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetById(cu.UserID);
        }
        private DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetDNNUser(int portalId, int userId) => new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetById(userId);
        internal DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetDNNUser(int portalId, string userName) => new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetById(GetUserIdByUserName(portalId, userName));
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use GetDNNUser(int portalId, string userName).")]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetDNNUser(string userName)
        {
            DotNetNuke.Entities.Users.UserInfo dnnUser = DotNetNuke.Entities.Users.UserController.GetUserByName(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId, userName);
            return new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetById(dnnUser.UserID);
        }
        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetUser(int PortalId, int ModuleId, int userId)
        {
            return GetDNNUser(PortalId, userId); 
        }
        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetUser(int PortalId, int ModuleId, string userName)
        {
            return GetDNNUser(PortalId, userName);
        }
        public static int Save(int ModuleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user)
        {
            user.DateUpdated = DateTime.UtcNow;
            var x = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().Save<int>(user, user.ProfileId);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.ClearCache(user.UserID);
            return user.UserID;
        }
        private struct JournalContentForUser
        {
            internal int ForumId { get; set; }
            internal int TopicId { get; set; }
            internal int ReplyId { get; set; }
            internal string Subject { get; set; }
            internal DateTime DateUpdated { get; set; }
        }
        internal static void BanUser(int PortalId, int TabId, int ModuleId, string ModuleTitle, int ForumId, int TopicId, int ReplyId, DotNetNuke.Entities.Users.UserInfo BannedBy, int AuthorId)
        {
            if (AuthorId > -1)
            { 
                DotNetNuke.Entities.Users.UserInfo bannedUser = DotNetNuke.Entities.Users.UserController.GetUserById(portalId: PortalId, userId: AuthorId);
                string sBody = string.Empty;
                string authorName = string.Empty;
                string sSubject = string.Empty;
                if (ReplyId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(ReplyId);
                    sBody = reply.Content.Body;
                    sSubject = reply.Content.Subject;
                    authorName = reply.Author.DisplayName;
                }
                else
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
                    sBody = topic.Content.Body;
                    sSubject = topic.Content.Subject;
                    authorName = topic.Author.DisplayName;
                }
                string notificationSubject = Utilities.GetSharedResource("[RESX:BanAlertSubject]");
                notificationSubject = notificationSubject.Replace("[Username]", bannedUser.Username);
                notificationSubject = notificationSubject.Replace("[DisplayName]", bannedUser.DisplayName);
                notificationSubject = notificationSubject.Replace("[BannedBy]", BannedBy.DisplayName);
                string body = Utilities.GetSharedResource("[RESX:BanAlertBody]");
                body = body.Replace("[Subject]", sSubject);

                StringBuilder postsRemoved = new StringBuilder();

                var contentForBannedUser = DataContext.Instance().ExecuteQuery<JournalContentForUser>(System.Data.CommandType.StoredProcedure, "activeforums_Content_GetJournalKeysForUser", bannedUser.UserID, ModuleId).ToList();
                string objectKey;
                contentForBannedUser.ForEach(c =>
                {
                    objectKey = c.ReplyId < 1 ? $"{c.ForumId}:{c.TopicId}" : $"{c.ForumId}:{c.TopicId}:{c.ReplyId}";
                    if (JournalController.Instance.GetJournalItemByKey(PortalId, objectKey) != null)
                    {
                        JournalController.Instance.DeleteJournalItemByKey(PortalId, objectKey);
                    }
                    postsRemoved.AppendLine($"{Utilities.GetUserFriendlyDateTimeString(c.DateUpdated, ModuleId, BannedBy)}\t{c.Subject}");
                    DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.RemoveModerationNotifications(TabId, ModuleId, c.ForumId, c.TopicId, c.ReplyId);
                });
                body = body.Replace("[PostsRemoved]", postsRemoved.ToString());

                Notification notification = new Notification();
                notification.NotificationTypeID = NotificationsController.Instance.GetNotificationType(Globals.BanUserNotificationType).NotificationTypeId;
                notification.Subject = notificationSubject;
                notification.Body = body;
                notification.IncludeDismissAction = true;
                notification.SenderUserID = BannedBy.UserID;
                notification.Context = DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.BuildNotificationContextKey(TabId, ModuleId, ForumId, TopicId, ReplyId);

                var modRoles = DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.GetModeratorRoles(PortalId, ModuleId, ForumId);
                NotificationsController.Instance.SendNotification(notification, PortalId, modRoles, null);

                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", ModuleTitle));
                string userBannedMsg = String.Format(Utilities.GetSharedResource("[RESX:UserBanned]"), bannedUser.Username);
                log.AddProperty("Message", userBannedMsg);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
               
                DataProvider.Instance().Topics_Delete_For_User(ModuleId: ModuleId, UserId: bannedUser.UserID, DelBehavior: SettingsBase.GetModuleSettings(ModuleId).DeleteBehavior);
                bannedUser.Membership.Approved = false;
                DotNetNuke.Entities.Users.UserController.UpdateUser(portalId: PortalId, user: bannedUser, loggedAction: true);
                DataCache.CacheClearPrefix(ModuleId, string.Format("AF-FV-{0}-{1}", PortalId, ModuleId));
            }
        }
        /// <summary>
        /// Returns the Rank for the user
        /// </summary>
        /// <returns>ReturnType 0 Returns RankDisplay ReturnType 1 Returns RankName</returns>
        public static string GetUserRank(int ModuleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, int returnType)
        {
            //ReturnType 0 for RankDisplay
            //ReturnType 1 for RankName
            try
            {
                string sRank = string.Empty;
                var mainSettings = SettingsBase.GetModuleSettings(ModuleId);
                if (mainSettings.EnablePoints && user.UserID > 0)
                {
                    var totalPoints = user.PostCount;
                    totalPoints = (user.TopicCount * mainSettings.TopicPointValue) + (user.ReplyCount * mainSettings.ReplyPointValue) + (user.AnswerCount * mainSettings.AnswerPointValue) + user.RewardPoints;

                    var strHost = Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)) + "/";
                    var rc = new RewardController();
                    foreach (var ri in rc.Reward_List(user.PortalId, ModuleId, true).Where(ri => ri.MinPosts <= totalPoints && ri.MaxPosts > totalPoints))
                    {
                        if (returnType == 0)
                        {
                            sRank = string.Format("<img src='{0}{1}' border='0' alt='{2}' />", strHost, ri.Display.Replace("activeforums/Ranks", "ActiveForums/images/Ranks"), ri.RankName);
                            break;
                        }
                        sRank = ri.RankName;
                        break;
                    }
                }
                return sRank;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        internal static string GetDisplayName(DotNetNuke.Entities.Portals.PortalSettings portalSettings, int moduleId, bool linkProfile, bool isMod, bool isAdmin, int userId, string username, string firstName = "", string lastName = "", string displayName = "", string profileLinkClass = "af-profile-link", string profileNameClass = "af-profile-name")
        {
            if (portalSettings == null)
            {
                portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
            }
            if (portalSettings == null)
            {
                return null;
            }

            var mainSettings = SettingsBase.GetModuleSettings(moduleId);

            var outputTemplate = string.IsNullOrWhiteSpace(profileLinkClass) ? "{0}" : string.Concat("<span class='", profileNameClass, "'>{0}</span>");

            if (linkProfile && userId > 0)
            {
                var profileVisibility = mainSettings.ProfileVisibility;

                switch (profileVisibility)
                {
                    case ProfileVisibilities.Disabled:
                        linkProfile = false;
                        break;

                    case ProfileVisibilities.Everyone: // Nothing to do in this case
                        break;

                    case ProfileVisibilities.RegisteredUsers:
                        linkProfile = HttpContext.Current.Request.IsAuthenticated;
                        break;

                    case ProfileVisibilities.Moderators:
                        linkProfile = isMod || isAdmin;
                        break;

                    case ProfileVisibilities.Admins:
                        linkProfile = isAdmin;
                        break;
                }

                if (linkProfile && portalSettings.UserTabId != null && portalSettings.UserTabId != DotNetNuke.Common.Utilities.Null.NullInteger && portalSettings.UserTabId != -1)
                    outputTemplate = string.Concat("<a href='", Utilities.NavigateURL(portalSettings.UserTabId, string.Empty, new[] { "userid=" + userId }), "' class='", profileLinkClass, "' rel='nofollow'>{0}</a>");
            }

            var displayMode = mainSettings.UserNameDisplay + string.Empty;

            string outputName = null;
            UserInfo user;

            switch (displayMode.ToUpperInvariant())
            {
                case "DISPLAYNAME":

                    if (string.IsNullOrWhiteSpace(username) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        displayName = (user != null) ? user.DisplayName : null;
                    }

                    outputName = displayName;
                    break;

                case "USERNAME":

                    if (string.IsNullOrWhiteSpace(username) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        username = (user != null) ? user.Username : null;
                    }

                    outputName = username;
                    break;

                case "FIRSTNAME":

                    if (string.IsNullOrWhiteSpace(firstName) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        firstName = (user != null) ? user.FirstName : null;
                    }

                    outputName = firstName;
                    break;

                case "LASTNAME":

                    if (string.IsNullOrWhiteSpace(lastName) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        lastName = (user != null) ? user.LastName : null;
                    }

                    outputName = lastName;
                    break;

                case "FULLNAME":
                    if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        firstName = (user != null) ? Utilities.SafeTrim(user.FirstName) : null;
                        lastName = (user != null) ? Utilities.SafeTrim(user.LastName) : null;
                    }

                    outputName = string.Concat(firstName, " ", lastName);
                    break;
            }


            outputName = Utilities.SafeTrim(outputName);

            if (string.IsNullOrWhiteSpace(outputName))
                outputName = userId > 0 ? Utilities.GetSharedResource("[RESX:DeletedUser]") : Utilities.GetSharedResource("[RESX:Anonymous]");

            outputName = HttpUtility.HtmlEncode(outputName);

            return string.Format(outputTemplate, outputName);
        }
        internal static string UserStatus(string themePath, bool isUserOnline, int userID, int moduleID, string altOnlineText = "User is Online", string altOfflineText = "User is Offline")
        {
            if (isUserOnline)
            {
                return "<span class=\"af-user-status\"><i class=\"fa fa-circle fa-blue\"></i></span>";
            }

            return "<span class=\"af-user-status\"><i class=\"fa fa-circle fa-red\"></i></span>";
        }
        internal static string GetAvatar(int userID, int avatarWidth, int avatarHeight)
        {
            PortalSettings portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();

            if (portalSettings == null)
                return string.Empty;

            //GIF files when reduced using DNN class losses its animation, so for gifs send them as is
            var user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userID);
            string imgUrl = string.Empty;

            if (user != null) imgUrl = user.Profile.PhotoURL;

            if (!string.IsNullOrWhiteSpace(imgUrl) && imgUrl.ToLower().EndsWith("gif"))
            {
                return string.Format("<img class='af-avatar' alt='' src='{0}' height='{1}px' width='{2}px' />", imgUrl, avatarHeight, avatarWidth);
            }
            else
            {
                return string.Concat("<img class='af-avatar' src='", string.Format(Common.Globals.UserProfilePicFormattedUrl(), userID, avatarWidth, avatarHeight), "' />");
            }
        }

        internal void UpdateUserTopicCount(int PortalId, int UserId)
        {
            string sSql = "UPDATE databaseOwner}{objectQualifier}activeforums_UserProfiles SET TopicCount = ISNULL((Select Count(t.TopicId) FROM ";
            sSql += "{databaseOwner}{objectQualifier}activeforums_Topics as t INNER JOIN ";
            sSql += "{databaseOwner}{objectQualifier}activeforums_Content as c ON t.ContentId = c.ContentId AND c.AuthorId = @1 INNER JOIN ";
            sSql += "{databaseOwner}{objectQualifier}activeforums_ForumTopics as ft ON ft.TopicId = t.TopicId INNER JOIN ";
            sSql += "{databaseOwner}{objectQualifier}activeforums_Forums as f ON ft.ForumId = f.ForumId ";
            DataContext.Instance().Execute(System.Data.CommandType.Text, sSql, PortalId, UserId);
        }
    }
}
