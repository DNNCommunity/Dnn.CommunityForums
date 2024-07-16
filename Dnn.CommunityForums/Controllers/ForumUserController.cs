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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using DotNetNuke.Data;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Services.Journal;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Social.Notifications;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal class ForumUserController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetById(int UserId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user = base.GetById(UserId);
            if (user == null)
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
        public static int Save(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user)
        {
            user.DateUpdated = DateTime.UtcNow; 
            var x = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().Save<int>(user, user.UserID);
            UserProfileController.Profiles_ClearCache(user.ModuleId, user.UserID);
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
    }
}
