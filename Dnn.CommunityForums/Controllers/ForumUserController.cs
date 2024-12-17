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

using System.Runtime.CompilerServices;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.UI.UserControls;
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web;

    internal class ForumUserController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo>
    {
        private readonly int moduleId = -1;

        internal override string cacheKeyTemplate => CacheKeys.ForumUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForumUserController"/> class.
        /// </summary>
        /// <param name="moduleId"></param>
        internal ForumUserController(int moduleId)
        {
            this.moduleId = moduleId;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetById(int portalId, int profileId)
        {
            throw new NotImplementedException("There is no probably need to call this method; if you do, you probably should be using GetByUserId.");
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetById(int profileId)
        {
            throw new NotImplementedException("There is no probably need to call this method; if you do, you probably should be using GetByUserId.");
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetByUserId(int portalId, int userId)
        {
            var cachekey = this.GetCacheKey(portalId: portalId, id: userId);
            var user = DataCache.UserCacheRetrieve(cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo;
            if (user == null)
            {
                if (userId > 0)
                {
                    user = this.Find("WHERE UserId = @0", userId).FirstOrDefault();
                    if (user != null)
                    {
                        user.UserInfo = DotNetNuke.Entities.Users.UserController.GetUserById(portalId: user.PortalId, userId: userId);
                    }
                    else
                    {
                        user = new DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo(this.moduleId)
                        {
                            UserId = userId,
                            PortalId = portalId,
                            TopicCount = 0,
                            ReplyCount = 0,
                            DateCreated = DateTime.UtcNow,
                            DateLastActivity = DateTime.UtcNow,
                            PrefBlockSignatures = false,
                            PrefBlockAvatars = false,
                            PrefTopicSubscribe = false,
                            PrefJumpLastPost = false,
                            PrefDefaultShowReplies = false,
                            PrefDefaultSort = "ASC",
                            PrefPageSize = 20,
                            UserInfo = DotNetNuke.Entities.Users.UserController.GetUserById(portalId: portalId, userId: userId),
                        };
                        if (user.UserInfo != null)
                        {
                            user.DateCreated = user.UserInfo.CreatedOnDate;
                            this.Insert(user);
                        }
                    }
                }
                else
                {
                    user = new DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo(this.moduleId)
                    {
                        UserId = -1,
                        PortalId = portalId,
                        TopicCount = 0,
                        ReplyCount = 0,
                        DateCreated = DateTime.UtcNow,
                        DateLastActivity = DateTime.UtcNow,
                        UserInfo = new DotNetNuke.Entities.Users.UserInfo { UserID = -1, Username = "guest", },
                        PrefBlockSignatures = false,
                        PrefBlockAvatars = false,
                        PrefTopicSubscribe = false,
                        PrefJumpLastPost = false,
                        PrefDefaultShowReplies = false,
                        PrefDefaultSort = "ASC",
                        PrefPageSize = 20,
                    };
                }

                user.ModuleId = this.moduleId;
                DataCache.UserCacheStore(cachekey, user);
            }

            return user;
        }

        internal static int GetUserIdByUserName(int portalId, string userName)
        {
            try
            {
                DotNetNuke.Entities.Users.UserInfo user = DotNetNuke.Entities.Users.UserController.GetUserByName(portalId, userName);
                return user == null ? -1 : user.UserID;
            }
            catch
            {
                return -1;
            }
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use GetUserFromHttpContext() [renamed method]")]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetUser(int portalId, int moduleId)
        {
            return this.GetUserFromHttpContext(portalId, moduleId);
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetUserFromHttpContext(int portalId, int moduleId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo u = null;
            if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                if (HttpContext.Current == null)
                {
                    u = this.DNNGetCurrentUser(portalId, moduleId);
                }
                else if (HttpContext.Current?.Items["DCFForumUserInfo"] != null)
                {
                    u = (DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo)HttpContext.Current?.Items["DCFForumUserInfo"];
                    u = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId).GetByUserId(portalId, u.UserId);
                }
                else
                {
                    u = this.DNNGetCurrentUser(portalId, moduleId);
                }

                u.ModuleId = moduleId;
            }
            else
            {
                u = new DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo(moduleId)
                {
                    UserId = -1,
                    PortalId = portalId,
                    TopicCount = 0,
                    ReplyCount = 0,
                    DateCreated = DateTime.UtcNow,
                    UserInfo = new DotNetNuke.Entities.Users.UserInfo
                    {
                        UserID = -1,
                        Username = "guest",
                    },
                    PrefBlockSignatures = false,
                    PrefBlockAvatars = false,
                    PrefTopicSubscribe = false,
                    PrefJumpLastPost = false,
                    PrefDefaultShowReplies = false,
                    PrefDefaultSort = "ASC",
                    PrefPageSize = 20,
                    ModuleId = moduleId,
                };
            }

            if (HttpContext.Current?.Items["DCFForumUserInfo"] == null)
            {
                HttpContext.Current?.Items.Add("DCFForumUserInfo", u);
            }

            DataCache.UserCacheStore(this.GetCacheKey(portalId: portalId, id: u.UserId), u);
            return u;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo DNNGetCurrentUser(int portalId, int moduleId)
        {
            DotNetNuke.Entities.Users.UserInfo cu = DotNetNuke.Entities.Users.UserController.Instance.GetCurrentUserInfo();
            return new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId).GetByUserId(portalId, cu.UserID);
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Needed.")]
        private DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetDNNUser(int portalId, int userId) => new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(-1).GetByUserId(portalId, userId);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Needed.")]
        internal DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetDNNUser(int portalId, string userName) => new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(-1).GetByUserId(portalId, GetUserIdByUserName(portalId, userName));

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Needed.")]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetUser(int portalId, int moduleId, string userName) => new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(-1).GetByUserId(portalId, GetUserIdByUserName(portalId, userName));

        public static int Save(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user)
        {
            user.DateUpdated = DateTime.UtcNow;
            var x = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(user.ModuleId).Save<int>(user, user.ProfileId);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.ClearCache(user.PortalId, user.UserId);
            return x.UserId;
        }

        public bool GetUserIsAdmin(int portalId, int moduleId, int userId)
        {
            return this.GetByUserId(portalId, userId).IsAdmin;
        }

        public bool GetUserIsSuperUser(int portalId, int moduleId, int userId)
        {
            return this.GetByUserId(portalId, userId).IsSuperUser;
        }

        private struct JournalContentForUser
        {
            internal int ForumId { get; set; }

            internal int TopicId { get; set; }

            internal int ReplyId { get; set; }

            internal string Subject { get; set; }

            internal DateTime DateUpdated { get; set; }
        }

        internal static void BanUser(int portalId, int tabId, int moduleId, string moduleTitle, int forumId, int topicId, int replyId, DotNetNuke.Entities.Users.UserInfo bannedBy, int authorId)
        {
            if (authorId > -1)
            {
                DotNetNuke.Entities.Users.UserInfo bannedUser = DotNetNuke.Entities.Users.UserController.GetUserById(portalId: portalId, userId: authorId);
                string sBody = string.Empty;
                string authorName = string.Empty;
                string sSubject = string.Empty;
                if (replyId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(moduleId).GetById(replyId);
                    sBody = reply.Content.Body;
                    sSubject = reply.Content.Subject;
                    authorName = reply.Author.DisplayName;
                }
                else
                {
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(moduleId).GetById(topicId);
                    sBody = topic.Content.Body;
                    sSubject = topic.Content.Subject;
                    authorName = topic.Author.DisplayName;
                }
                string notificationSubject = Utilities.GetSharedResource("[RESX:BanAlertSubject]");
                notificationSubject = notificationSubject.Replace("[Username]", bannedUser.Username);
                notificationSubject = notificationSubject.Replace("[DisplayName]", bannedUser.DisplayName);
                notificationSubject = notificationSubject.Replace("[BannedBy]", bannedBy.DisplayName);
                string body = Utilities.GetSharedResource("[RESX:BanAlertBody]");
                body = body.Replace("[Subject]", sSubject);

                StringBuilder postsRemoved = new StringBuilder();

                var contentForBannedUser = DataContext.Instance().ExecuteQuery<JournalContentForUser>(System.Data.CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}activeforums_Content_GetJournalKeysForUser", bannedUser.UserID, moduleId).ToList();
                string objectKey;
                contentForBannedUser.ForEach(c =>
                {
                    objectKey = c.ReplyId < 1 ? $"{c.ForumId}:{c.TopicId}" : $"{c.ForumId}:{c.TopicId}:{c.ReplyId}";
                    if (JournalController.Instance.GetJournalItemByKey(portalId, objectKey) != null)
                    {
                        JournalController.Instance.DeleteJournalItemByKey(portalId, objectKey);
                    }
                    postsRemoved.AppendLine($"{Utilities.GetUserFriendlyDateTimeString(c.DateUpdated, moduleId, bannedBy)}\t{c.Subject}");
                    DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.RemoveModerationNotifications(tabId, moduleId, c.ForumId, c.TopicId, c.ReplyId);
                });
                body = body.Replace("[PostsRemoved]", postsRemoved.ToString());

                Notification notification = new Notification();
                notification.NotificationTypeID = NotificationsController.Instance.GetNotificationType(Globals.BanUserNotificationType).NotificationTypeId;
                notification.Subject = notificationSubject;
                notification.Body = body;
                notification.IncludeDismissAction = true;
                notification.SenderUserID = bannedBy.UserID;
                notification.Context = DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.BuildNotificationContextKey(tabId, moduleId, forumId, topicId, replyId);

                var modRoles = DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.GetModeratorRoles(portalId, moduleId, forumId);
                NotificationsController.Instance.SendNotification(notification, portalId, modRoles, null);

                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", moduleTitle));
                string userBannedMsg = String.Format(Utilities.GetSharedResource("[RESX:UserBanned]"), bannedUser.Username);
                log.AddProperty("Message", userBannedMsg);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);

                DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Topics_Delete_For_User(moduleId: moduleId, userId: bannedUser.UserID, delBehavior: SettingsBase.GetModuleSettings(moduleId).DeleteBehavior);
                bannedUser.Membership.Approved = false;
                DotNetNuke.Entities.Users.UserController.UpdateUser(portalId: portalId, user: bannedUser, loggedAction: true);
                DataCache.ClearAllCache(moduleId);
            }
        }

        /// <summary>
        /// Returns the Rank for the user
        /// </summary>
        /// <returns>ReturnType 0 Returns RankDisplay ReturnType 1 Returns RankName</returns>
        public static string GetUserRank(int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, int returnType)
        {
            // ReturnType 0 for RankDisplay
            // ReturnType 1 for RankName
            try
            {
                string sRank = string.Empty;
                var mainSettings = SettingsBase.GetModuleSettings(moduleId);
                if (mainSettings.EnablePoints && user.UserId > 0)
                {
                    var totalPoints = user.PostCount;
                    totalPoints = (user.TopicCount * mainSettings.TopicPointValue) + (user.ReplyCount * mainSettings.ReplyPointValue) + (user.AnswerCount * mainSettings.AnswerPointValue) + user.RewardPoints;

                    var strHost = Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)) + "/";
                    var rc = new RewardController();
                    foreach (var ri in rc.Reward_List(user.PortalId, moduleId, true).Where(ri => ri.MinPosts <= totalPoints && ri.MaxPosts > totalPoints))
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

            var outputName = GetDisplayName(portalSettings, mainSettings, isMod, isAdmin, userId, username, firstName, lastName, displayName);
            outputName = System.Net.WebUtility.HtmlEncode(outputName);

            return string.Format(outputTemplate, outputName);
        }

        internal static bool CanLinkToProfile(DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo accessingUser, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser)
        {
            if (portalSettings == null)
            {
                portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
            }

            if (portalSettings == null)
            {
                return false;
            }

            bool canLinkToProfile = false;
            if (forumUser?.UserId > 0 &&
                portalSettings?.UserTabId != DotNetNuke.Common.Utilities.Null.NullInteger &&
                portalSettings?.UserTabId != -1)
            {
                var profileVisibility = mainSettings.ProfileVisibility;

                switch (profileVisibility)
                {
                    case ProfileVisibilities.Disabled:
                        canLinkToProfile = false;
                        break;

                    case ProfileVisibilities.Everyone:
                        canLinkToProfile = true;
                        break;

                    case ProfileVisibilities.RegisteredUsers:
                        canLinkToProfile = HttpContext.Current.Request.IsAuthenticated;
                        break;

                    case ProfileVisibilities.Moderators:
                        canLinkToProfile = accessingUser != null && (accessingUser.GetIsMod(moduleId) || accessingUser.IsAdmin);
                        break;

                    case ProfileVisibilities.Admins:
                        canLinkToProfile = accessingUser != null && accessingUser.IsAdmin;
                        break;
                }
            }

            return canLinkToProfile;
        }

        internal static string GetDisplayName(DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, bool isMod, bool isAdmin, int userId, string username, string firstName = "", string lastName = "", string displayName = "")
        {
            if (portalSettings == null)
            {
                portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
            }
            if (portalSettings == null)
            {
                return null;
            }

            string outputName = null;
            UserInfo user;

            switch (mainSettings.UserNameDisplay.ToUpperInvariant())
            {
                case "DISPLAYNAME":

                    if (string.IsNullOrWhiteSpace(displayName) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        displayName = user?.DisplayName;
                    }

                    outputName = displayName;
                    break;

                case "USERNAME":

                    if (string.IsNullOrWhiteSpace(username) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        username = user?.Username;
                    }

                    outputName = username;
                    break;

                case "FIRSTNAME":

                    if (string.IsNullOrWhiteSpace(firstName) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        firstName = user?.FirstName;
                    }

                    outputName = firstName;
                    break;

                case "LASTNAME":

                    if (string.IsNullOrWhiteSpace(lastName) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        lastName = user?.LastName;
                    }

                    outputName = lastName;
                    break;

                case "FULLNAME":
                    if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        firstName = Utilities.SafeTrim(user?.FirstName);
                        lastName = Utilities.SafeTrim(user?.LastName);
                    }

                    outputName = string.Concat(firstName, " ", lastName);
                    break;
            }

            outputName = Utilities.SafeTrim(outputName);
            if (string.IsNullOrWhiteSpace(outputName))
            {
                outputName = userId > 0 ? Utilities.GetSharedResource("[RESX:DeletedUser]") : Utilities.GetSharedResource("[RESX:Anonymous]");
            }

            return System.Net.WebUtility.HtmlEncode(outputName);
        }

        internal static string UserStatus(string themePath, bool isUserOnline, int userID, int moduleID)
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

            // GIF files when reduced using DNN class losses its animation, so for gifs send them as is
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

        internal static void UpdateUserTopicCount(int portalId, int userId)
        {
            string sSql = "UPDATE {databaseOwner}{objectQualifier}activeforums_UserProfiles SET TopicCount = ISNULL((Select Count(t.TopicId) FROM ";
            sSql += "{databaseOwner}{objectQualifier}activeforums_Topics as t INNER JOIN ";
            sSql += "{databaseOwner}{objectQualifier}activeforums_Content as c ON t.ContentId = c.ContentId AND c.AuthorId = @1 INNER JOIN ";
            sSql += "{databaseOwner}{objectQualifier}activeforums_ForumTopics as ft ON ft.TopicId = t.TopicId INNER JOIN ";
            sSql += "{databaseOwner}{objectQualifier}activeforums_Forums as f ON ft.ForumId = f.ForumId ";
            sSql += "WHERE c.AuthorId = @1 AND t.IsApproved = 1 AND t.IsDeleted=0 AND f.PortalId=@0),0) ";
            sSql += "WHERE UserId = @1 AND PortalId = @0";
            DataContext.Instance().Execute(System.Data.CommandType.Text, sSql, portalId, userId);
            DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.ClearCache(portalId, userId);

        }

        internal string GetUsersOnline(DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.SettingsInfo mainSettings, int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser)
        {
            bool isAdmin = forumUser.IsAdmin || forumUser.IsSuperUser;
            var sb = new StringBuilder();
            var users = this.Find("WHERE PortalId = @0 AND DateLastActivity >= CAST(DATEADD(mi,@1,GETUTCDATE()) as datetime)", portalSettings.PortalId, -2);
            foreach (var user in users)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                user.ModuleId = moduleId;
                sb.Append(DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, mainSettings, false, isAdmin, user.UserId, user.Username, user.FirstName, user.LastName, user.DisplayName));
            }

            return sb.ToString();
        }

        internal static void ClearCache(int portalId, int userId)
        {
            DataCache.UserCacheClear(string.Format(CacheKeys.ForumUser, portalId, userId));
        }

        internal string GetCacheKey<TProperty>(int portalId, TProperty id)
        {
            return string.Format(this.cacheKeyTemplate, portalId, id);
        }
    }
}
