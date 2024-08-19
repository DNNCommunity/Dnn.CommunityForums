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

using DotNetNuke.Modules.ActiveForums.Controllers;
using DotNetNuke.Modules.ActiveForums.Entities;

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.UI.UserControls;
    using Microsoft.ApplicationBlocks.Data;

    public class TemplateUtils
    {
        public static List<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo> lstSubscriptionInfo { get; set; }
        
        #region "Deprecated Methods"
        [Obsolete("Deprecated in Community Forums. Remove in 10.00.00. Not Used.")]
        public static string ShowIcon(bool canView, int forumID, int userId, DateTime dateAdded, DateTime lastRead, int lastPostId)
        {
            if (!canView)
            {
                return "folder_forbidden.png";
            }

            if (lastPostId == 0 || userId == -1)
            {
                return "folder_closed.png";
            }

            if (lastRead == DateTime.MinValue)
            {
                lastRead = Utilities.NullDate();
            }

            if (dateAdded != Utilities.NullDate())
            {
                return dateAdded > lastRead ? "folder_new.png" : "folder.png";
            }

            return "folder.png";
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public static void LoadTemplateCache(int moduleID)
        {
            var tc = new TemplateController();
            foreach (var ti in tc.Template_List(-1, moduleID))
            {
                DataCache.SettingsCacheStore(moduleID, ti.Title + moduleID, ti.Template);
                DataCache.SettingsCacheStore(moduleID, ti.Subject + "_Subject_" + moduleID, ti.Subject);
            }
        }

        #endregion
        #region Email
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, int timeZoneOffset)
        => ParseEmailTemplate(template, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, string.Empty, null, -1, Utilities.GetCultureInfoForUser(portalID, -1), new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0), false);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string ParseEmailTemplate(string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, int timeZoneOffset)
        => ParseEmailTemplate(string.Empty, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, string.Empty, null, -1, Utilities.GetCultureInfoForUser(portalID, -1), new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0), false);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string ParseEmailTemplate(string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, int timeZoneOffset)
        => ParseEmailTemplate(string.Empty, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, comments, null, -1, Utilities.GetCultureInfoForUser(portalID, -1), new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0), false);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string ParseEmailTemplate(string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, DotNetNuke.Entities.Users.UserInfo user, int timeZoneOffset)
        => ParseEmailTemplate(string.Empty, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, string.Empty, user, -1, Utilities.GetCultureInfoForUser(portalID, -1), new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0), false);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, int userId, int timeZoneOffset)
        {
            var uc = new DotNetNuke.Entities.Users.UserController();
            var usr = uc.GetUser(portalID, userId);
            return ParseEmailTemplate(template, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, comments, usr, userId, Utilities.GetCultureInfoForUser(portalID, userId), new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0), false);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, DotNetNuke.Entities.Users.UserInfo user, int userId, int timeZoneOffset)
        => ParseEmailTemplate(template, templateName, portalID, moduleID, tabID, forumID, topicId: topicId, replyId: replyId, comments: comments, user: user, userId: userId, userCulture: Utilities.GetCultureInfoForUser(portalID, userId), timeZoneOffset: new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0), topicSubscriber: false);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, CultureInfo userCulture, TimeSpan timeZoneOffset)
        => ParseEmailTemplate(template, templateName: templateName, portalID: portalID, moduleID: moduleID, tabID: tabID, forumID: forumID, topicId: topicId, replyId: replyId, comments: string.Empty, userId: 0, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string ParseEmailTemplate(string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, CultureInfo userCulture, TimeSpan timeZoneOffset)
        => ParseEmailTemplate(template: string.Empty, templateName: templateName, portalID: portalID, moduleID: moduleID, tabID: tabID, forumID: forumID, topicId: topicId, replyId: replyId, comments: string.Empty, user: null, userId: -1, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string ParseEmailTemplate(string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, CultureInfo userCulture, TimeSpan timeZoneOffset)
        => ParseEmailTemplate(template: string.Empty, templateName: templateName, portalID: portalID, moduleID: moduleID, tabID: tabID, forumID: forumID, topicId: topicId, replyId: replyId, comments: comments, user: null, userId: -1, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string ParseEmailTemplate(string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, DotNetNuke.Entities.Users.UserInfo user, CultureInfo userCulture, TimeSpan timeZoneOffset)
        => ParseEmailTemplate(string.Empty, templateName, portalID, moduleID, tabID, forumID: forumID, topicId: topicId, replyId: replyId, comments: string.Empty, user: user, userId: -1, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false);

        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, int userId, CultureInfo userCulture, TimeSpan timeZoneOffset)
        {
            var uc = new DotNetNuke.Entities.Users.UserController();
            var usr = uc.GetUser(portalID, userId);
            return ParseEmailTemplate(template, templateName, portalID, moduleID, tabID, forumID, topicId: topicId, replyId: replyId, comments: comments, user: usr, userId: userId, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, int userId, CultureInfo userCulture, TimeSpan timeZoneOffset, bool topicSubscriber)
        {
            var user = new DotNetNuke.Entities.Users.UserController().GetUser(portalID, userId);
            return ParseEmailTemplate(template, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, comments, user, userId, userCulture, timeZoneOffset, topicSubscriber, new Services.URLNavigator().NavigationManager(), HttpContext.Current.Request.Url);
        }

        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, DotNetNuke.Entities.Users.UserInfo user, int userId, CultureInfo userCulture, TimeSpan timeZoneOffset, bool topicSubscriber)
        { return ParseEmailTemplate(template, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, comments, user, userId, userCulture, timeZoneOffset, topicSubscriber, new Services.URLNavigator().NavigationManager(), HttpContext.Current.Request.Url); }
        
        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, DotNetNuke.Entities.Users.UserInfo author, int userId, CultureInfo userCulture, TimeSpan timeZoneOffset, bool topicSubscriber, INavigationManager navigationManager, Uri requestUrl)
        {
            var author2 = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalID, userId);
            return ParseEmailTemplate(template, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, comments, author2, userId, userCulture, timeZoneOffset, topicSubscriber, navigationManager, requestUrl);
        }

        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author, int userId, CultureInfo userCulture, TimeSpan timeZoneOffset, bool topicSubscriber, INavigationManager navigationManager, Uri requestUrl)
        {
            if (navigationManager == null)
            {
                navigationManager = (INavigationManager)new Services.URLNavigator();
            }

            PortalSettings portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings(portalID);
            var moduleSettings = SettingsBase.GetModuleSettings(moduleID);
            var accessingUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(portalID, userId);
            var forumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: forumID, moduleId: moduleID);
            if (author == null)
            {
                author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalID, userId);
            }
            // If we have a template name, load the template into sOut
            if (templateName != string.Empty)
            {
                if (templateName.Contains("_Subject_"))
                {
                    templateName = templateName.Replace(string.Concat("_Subject_", moduleID), string.Empty);
                }
                template = TemplateCache.GetCachedTemplate(moduleID, templateName, -1);
            }

            // Load Subject and body from topic or reply
            string subject = string.Empty;
            string body = string.Empty;
            DateTime dateCreated = Utilities.NullDate();

            if (topicId > 0 && replyId > 0)
            {
                var ri = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController().GetById(replyId);
                if (ri != null)
                {
                    subject = ri.Content.Subject;
                    body = ri.Content.Body;
                    dateCreated = ri.Content.DateCreated;
                }
            }
            else
            {
                var ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);

                if (ti != null)
                {
                    subject = ti.Content.Subject;
                    body = ti.Content.Body;
                    dateCreated = ti.Content.DateCreated;
                }
            }

            body = Utilities.ManageImagePath(body, requestUrl);

            // Build the topic link
            string link;
            if (string.IsNullOrEmpty(forumInfo.PrefixURL) || !Utilities.UseFriendlyURLs(moduleID))
            {
                if (replyId == 0)
                    link = moduleSettings.UseShortUrls ? navigationManager.NavigateURL(tabID, portalSettings, string.Empty, new[] { $"{ParamKeys.TopicId}={topicId}" })
                        : navigationManager.NavigateURL(tabID, portalSettings, string.Empty, new[] { $"{ParamKeys.ForumId}={forumID}", $"{ParamKeys.ViewType}={Views.Topic}", $"{ParamKeys.TopicId}={topicId}" });
                else
                    link = moduleSettings.UseShortUrls ? navigationManager.NavigateURL(tabID, portalSettings, string.Empty, new[] { $"{ParamKeys.TopicId}={topicId}", string.Concat(ParamKeys.ContentJumpId, "=", replyId) })
                        : navigationManager.NavigateURL(tabID, portalSettings, string.Empty, new[] { $"{ParamKeys.ForumId}={forumID}", $"{ParamKeys.ViewType}={Views.Topic}", $"{ParamKeys.TopicId}={topicId}", $"{ParamKeys.ContentJumpId}={replyId}" });
            }
            else
            {
                var contentId = (replyId > 0) ? replyId : -1;
                link = string.Concat(navigationManager.NavigateURL(tabID, portalSettings, string.Empty, (string[])null), "/", new Data.Common().GetUrl(moduleID, -1, forumID, topicId, -1, contentId));
            }

            if (!link.StartsWith("http"))
            {
                if (!link.StartsWith("/"))
                {
                    link = string.Concat("/", link);
                }

                if (HttpContext.Current != null && link.IndexOf(requestUrl.Host, StringComparison.Ordinal) == -1)
                {
                    link = Common.Globals.AddHTTP(requestUrl.Host) + link;
                }
            }

            // Build the forum Url
            string forumURL = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildForumUrl(navigationManager, portalSettings, moduleSettings, forumInfo);

            var templateStringbuilder = new StringBuilder(template);


            templateStringbuilder.Replace("[POSTDATE]", Utilities.GetUserFormattedDateTime(dateCreated, userCulture, timeZoneOffset));
            templateStringbuilder.Replace("[COMMENTS]", comments);
            templateStringbuilder.Replace("[LINK]", string.Concat("<a href=\"", link, "\">", link, "</a>"));
            templateStringbuilder.Replace("[HYPERLINK]", string.Concat("<a href=\"", link, "\">", link, "</a>"));
            templateStringbuilder.Replace("[LINKURL]", link);

            templateStringbuilder.Replace("[POSTEDORREPLIEDTO]", (replyId <= 0 ? Utilities.GetSharedResource("[RESX:posted]") : Utilities.GetSharedResource("[RESX:repliedto]")));
            templateStringbuilder.Replace("[POSTEDTO]", (replyId <= 0 ? Utilities.GetSharedResource("[RESX:postedto]") : string.Empty));
            templateStringbuilder.Replace("[REPLIEDTO]", (replyId > 0 ? Utilities.GetSharedResource("[RESX:repliedto]") : string.Empty));
            templateStringbuilder.Replace("[NEWPOST]", (replyId <= 0 ? Utilities.GetSharedResource("[RESX:NewPost]") : string.Empty));
            templateStringbuilder.Replace("[NEWREPLY]", (replyId > 0 ? Utilities.GetSharedResource("[RESX:NewReply]") : string.Empty));
            templateStringbuilder.Replace("[SUBSCRIBEDTOPIC]", (topicSubscriber ? Utilities.GetSharedResource("[RESX:SubscribedTopic]") : string.Empty));
            templateStringbuilder.Replace("[SUBSCRIBEDTOPICSUBJECT]", (topicSubscriber ? string.Format(Utilities.GetSharedResource("[RESX:SubscribedTopicSubject]"), subject) : string.Empty));
            templateStringbuilder.Replace("[SUBSCRIBEDTOPICFORUMNAME]", (topicSubscriber ? string.Format(Utilities.GetSharedResource("[RESX:SubscribedTopicForumName]"), arg0: subject, forumInfo.ForumName) : string.Empty));
            templateStringbuilder.Replace("[SUBSCRIBEDFORUM]", (topicSubscriber ? string.Empty : "[RESX:SubscribedForum]"));
            templateStringbuilder.Replace("[SUBSCRIBEDFORUMNAME]", (topicSubscriber ? string.Empty : string.Format(Utilities.GetSharedResource("[RESX:SubscribedForumName]"), forumInfo.ForumName)));
            templateStringbuilder.Replace("[SUBSCRIBEDFORUMORTOPICSUBJECTFORUMNAME]", (topicSubscriber ? string.Format(Utilities.GetSharedResource("[RESX:SubscribedTopicForumName]"), subject, forumInfo.ForumName) : string.Format(Utilities.GetSharedResource("[RESX:SubscribedForumTopicForumName]"), subject, forumInfo.ForumName)));

            // Introduced for Active Forum Email Connector plug-in Starts
            if (templateStringbuilder.ToString().Contains("[EMAILCONNECTORITEMID]"))
            {
                // This Try with empty catch is introduced here because this code section is for Email Connector functionality only and this section should not
                // cause any issue to DNN Community Forums functionality in case it does not run successfully.
                try
                {
                    long itemID = GetEmailInfo(portalID, moduleID, forumID, topicId, HttpContext.Current.Request.UserHostAddress);
                    templateStringbuilder.Replace("[EMAILCONNECTORITEMID]", itemID.ToString());
                }
                catch
                { }
            }

            templateStringbuilder.Replace("[SUBJECT]", subject);
            templateStringbuilder.Replace("[BODY]", body);
            templateStringbuilder.Replace("[Body]", body);
            
            //templateStringbuilder = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.ReplaceUserTokens(templateStringbuilder, portalSettings, moduleSettings, author.ForumUser, accessingUser, moduleID);
            //templateStringbuilder = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.ReplaceForumTokens(templateStringbuilder, forumInfo, portalSettings, moduleSettings, navigationManager, author.ForumUser, HttpContext.Current.Request, tabID, CurrentUserTypes.Auth);
            //templateStringbuilder = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.ReplaceModuleTokens(templateStringbuilder, portalSettings, moduleSettings, author.ForumUser, tabID, moduleID);

            return templateStringbuilder.ToString();
        }

        private static long GetEmailInfo(int portalId, int moduleId, int forumID, int topicID, string ipAddress)
        {
            long itemID = -1;

            DotNetNuke.Framework.Providers.ProviderConfiguration _providerConfiguration = DotNetNuke.Framework.Providers.ProviderConfiguration.GetProviderConfiguration("data");
            string connectionString;
            string objectQualifier;
            string databaseOwner;
            connectionString = ConfigurationManager.ConnectionStrings["SiteSqlServer"].ConnectionString;
            var objProvider = (DotNetNuke.Framework.Providers.Provider)_providerConfiguration.Providers[_providerConfiguration.DefaultProvider];

            objectQualifier = objProvider.Attributes["objectQualifier"];
            if (objectQualifier != string.Empty && objectQualifier.EndsWith("_") == false)
            {
                objectQualifier += "_";
            }

            databaseOwner = objProvider.Attributes["databaseOwner"];
            if (databaseOwner != string.Empty && databaseOwner.EndsWith(".") == false)
            {
                databaseOwner += ".";
            }

            StringBuilder userIds = new StringBuilder();
            userIds.Append("(");

            DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo[] arrSubscriptionInfo = lstSubscriptionInfo.ToArray();
            for (int i = 0; i < arrSubscriptionInfo.Length; i++)
            {
                userIds.Append(arrSubscriptionInfo[i].UserId);
                if (i < arrSubscriptionInfo.Length - 1)
                {
                    userIds.Append(",");
                }
                else
                {
                    userIds.Append(")");
                }
            }

            // dbPrefix = databaseOwner + objectQualifier + databaseObjectPrefix;
            IDataReader dataReader = (IDataReader)SqlHelper.ExecuteReader(connectionString, databaseOwner + objectQualifier + "ActiveForumsEmailConnector_GetEmailInfo", portalId, moduleId, forumID, topicID, ipAddress, userIds.ToString());
            if (dataReader.Read())
            {
                itemID = Convert.ToInt32(dataReader["RecordID"]);
            }

            return itemID;
        }
        #endregion

        #region Profile

        #region "Deprecated Methods"

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use GetPostInfo(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, bool isMod, string ipAddress, bool isUserOnline, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, TimeSpan timeZoneOffset)")]
        public static string GetPostInfo(int portalId, int moduleId, int userId, string username, User up, string imagePath, bool isMod, string ipAddress, bool isUserOnline, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, int timeZoneOffset)
        {
            var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(portalId, up.UserId);
            return GetPostInfo(moduleId, user, imagePath, isMod, ipAddress, isUserOnline, currentUserType, currentUserId, userPrefHideAvatar, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        public static string GetPostInfo(int portalId, int moduleId, int userId, string username, User up, string imagePath, bool isMod, string ipAddress, bool isUserOnline, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, TimeSpan timeZoneOffset)
        {
            var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(portalId, up.UserId);
            return GetPostInfo(moduleId, user, imagePath, isMod, ipAddress, isUserOnline, currentUserType, currentUserId, userPrefHideAvatar, timeZoneOffset);
        }
        #endregion "Deprecated Methods"

        internal static string GetPostInfo(int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, bool isMod, string ipAddress, bool isUserOnline, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, TimeSpan timeZoneOffset)
        {
            var sPostInfo = ParseProfileInfo(moduleId, user, imagePath, isMod, ipAddress, currentUserType, currentUserId, userPrefHideAvatar, timeZoneOffset);
           
            return sPostInfo; 
            //if (sPostInfo.ToLower().Contains("<br"))
            //{
            //    return sPostInfo;
            //}

            //var sr = new StringReader(sPostInfo);
            //var sTrim = string.Empty;

            //while (sr.Peek() != -1)
            //{
            //    var tmp = sr.ReadLine();
            //    if (tmp != null && tmp.Trim() != string.Empty)
            //    {
            //        sTrim += tmp.Trim() + "<br />";
            //    }
            //}

            //return sTrim;
        }

        #region "Deprecated Methods"
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileInfo(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, bool isMod, string ipAddress, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, TimeSpan timeZoneOffset)")]
        public static string ParseProfileInfo(int portalId, int moduleId, int userId, string username, User up, string imagePath, bool isMod, string ipAddress, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, int timeZoneOffset)
        {
            var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(portalId, up.UserId);
            return ParseProfileInfo(moduleId, user, imagePath, isMod, ipAddress, currentUserType, currentUserId, userPrefHideAvatar, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileInfo(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, bool isMod, string ipAddress, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, TimeSpan timeZoneOffset)")]
        public static string ParseProfileInfo(int portalId, int moduleId, int userId, string username, User up, string imagePath, bool isMod, string ipAddress, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, TimeSpan timeZoneOffset)
        {
            var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(portalId, up.UserId);
            return ParseProfileInfo(moduleId, user, imagePath, isMod, ipAddress, currentUserType, currentUserId, userPrefHideAvatar, timeZoneOffset);
        }
        #endregion "Deprecated Methods"

        private static string ParseProfileInfo(int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, bool isMod, string ipAddress, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, TimeSpan timeZoneOffset)
        {

            var cacheKey = string.Format(CacheKeys.ProfileInfo, moduleId);
            var myTemplate = Convert.ToString(DataCache.SettingsCacheRetrieve(moduleId, cacheKey));
            if (string.IsNullOrEmpty(myTemplate))
            {
                myTemplate = TemplateCache.GetCachedTemplate(moduleId, "ProfileInfo", -1);
                if (cacheKey != string.Empty)
                {
                    DataCache.SettingsCacheStore(moduleId, cacheKey, myTemplate);
                }
            }
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(user.PortalId, user.UserId);

            myTemplate = ParseProfileTemplate(moduleId, myTemplate, author, imagePath, currentUserType, userPrefHideAvatar, false, ipAddress, currentUserId, timeZoneOffset);
            return myTemplate;
        }

        #region "Deprecated Methods"
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileTemplate(string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string ParseProfileTemplate(string profileTemplate, int userId, int portalId, int moduleId, int currentUserId, int timeZoneOffset)
        {
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, userId);
            return ParseProfileTemplate(moduleId, profileTemplate, author, string.Empty, CurrentUserTypes.Anon, false, false, string.Empty, currentUserId, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileTemplate(string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, int timeZoneOffset)
        {
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, up.UserId);
            return ParseProfileTemplate(moduleId, profileTemplate, author, imagePath, currentUserType, false, false, string.Empty, -1, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileTemplate(string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, TimeSpan timeZoneOffset)
        {
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, up.UserId);
            return ParseProfileTemplate(moduleId, profileTemplate, author, imagePath, currentUserType, false, false, string.Empty, -1, timeZoneOffset);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileTemplate(string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, int timeZoneOffset)
        {
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, up.UserId);
            return ParseProfileTemplate(moduleId, profileTemplate, author, imagePath, currentUserType, false, false, string.Empty, -1, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileTemplate(string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, TimeSpan timeZoneOffset)
        {
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, up.UserId);
            return ParseProfileTemplate(moduleId, profileTemplate, author, imagePath, currentUserType, false, false, string.Empty, -1, timeZoneOffset);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileTemplate(string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int timeZoneOffset)
        {
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, up.UserId);
            return ParseProfileTemplate(moduleId, profileTemplate, author, imagePath, currentUserType, userPrefHideAvatar, userPrefHideSignature, ipAddress, -1, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileTemplate(string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, TimeSpan timeZoneOffset)
        {
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, up.UserId);
            return ParseProfileTemplate(moduleId, profileTemplate, author, imagePath, currentUserType, userPrefHideAvatar, userPrefHideSignature, ipAddress, -1, timeZoneOffset);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileTemplate(string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, int currentUserId, TimeSpan timeZoneOffset)
        {
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, up.UserId);
            return ParseProfileTemplate(moduleId, profileTemplate, author, imagePath, currentUserType, false, false, string.Empty, currentUserId, timeZoneOffset);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileTemplate(string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, int timeZoneOffset)
        {
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, up.UserId);
            return ParseProfileTemplate(moduleId, profileTemplate, author, imagePath, currentUserType, userPrefHideAvatar, userPrefHideSignature, ipAddress, currentUserId, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileTemplate(string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)
        {
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, up.UserId);
            return ParseProfileTemplate(moduleId, profileTemplate, author, imagePath, currentUserType, userPrefHideAvatar, userPrefHideSignature, ipAddress, currentUserId, timeZoneOffset);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfileTemplate(string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string ParseProfileTemplate(string profileTemplate, int userId, int portalId, int moduleId, int currentUserId, TimeSpan timeZoneOffset)
        {
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalId, userId);
            return ParseProfileTemplate(moduleId, profileTemplate, author, string.Empty, CurrentUserTypes.Anon, false, false, string.Empty, currentUserId, timeZoneOffset);
        }
        #endregion "Deprecated Methods"

        internal static string ParseProfileTemplate(int moduleId, string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author, string imagePath, CurrentUserTypes currentUserType, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)
        {
            try
            {
                var portalSettings = Utilities.GetPortalSettings(author.ForumUser.PortalId);
                var mainSettings = SettingsBase.GetModuleSettings(moduleId);
                var accessingUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(author.ForumUser.PortalId, currentUserId);

                var profileTemplateBuilder = new StringBuilder(profileTemplate);


                // Parse DNN profile fields if needed
                var pt = profileTemplate;
                if (pt.IndexOf("[DNN:PROFILE:", StringComparison.Ordinal) >= 0)
                {
                    pt = ParseProfile(author.ForumUser, pt, currentUserType, currentUserId);
                }

                // Parse Roles
                if (pt.Contains("[ROLES:"))
                {
                    pt = ParseRoles(pt, (author.ForumUser.UserId == -1) ? string.Empty : author.ForumUser.UserRoles);
                }

                var result = new StringBuilder(pt);
                
                // IP Address
                result.Replace("[MODIPADDRESS]", accessingUser.IsAdmin || accessingUser.IsSuperUser || accessingUser.GetIsMod(moduleId) ? ipAddress : string.Empty);

                // User Edit
                result.Replace("[AF:BUTTON:EDITUSER]", (accessingUser.IsAdmin || accessingUser.IsSuperUser) && author.ForumUser.UserId > 0 ? string.Format("<button class='af-button af-button-edituser' data-id='{0}' data-name='{1}'>[RESX:Edit]</button>", author.ForumUser.UserId, Utilities.JSON.EscapeJsonString(author.DisplayName)) : string.Empty);


                // PM Image/link
                var pmUrl = string.Empty;
                var pmLink = string.Empty;
                if (author.ForumUser.UserId > 0 && currentUserId >= 0 && author.ForumUser.UserId != currentUserId)
                {
                    switch (mainSettings.PMType)
                    {
                        case PMTypes.Core:
                            pmLink = string.Concat("<img class='ComposeMessage' data-recipient='{ \"id\": \"user-", author.ForumUser.UserId, "\", \"name\": \"", HttpUtility.JavaScriptStringEncode(author.DisplayName), "\"}' src='", imagePath, "/icon_pm.png' alt=\"[RESX:SendPM]\" title=\"[RESX:SendPM]\" border=\"0\" /></a>");
                            break;

                        case PMTypes.Ventrian:
                            pmUrl = Utilities.NavigateURL(mainSettings.PMTabId, string.Empty, new[] { "type=compose", string.Concat("sendto=", author.ForumUser.UserId) });
                            pmLink = string.Concat("<a href=\"", pmUrl, "\"><img src=\"", imagePath, "/icon_pm.png\" alt=\"[RESX:SendPM]\" border=\"0\" /></a>");
                            break;
                    }
                }

                result.Replace("[AF:PROFILE:PMLINK]", pmLink);
                result.Replace("[AF:PROFILE:PMURL]", pmUrl);

                result = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceUserTokens(result, portalSettings, mainSettings, author.ForumUser, accessingUser, moduleId);

                return result.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion

        public static string ParseRoles(string template, string userRoles)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return template;
            }

            var userRoleArray = string.IsNullOrWhiteSpace(userRoles) ? null : userRoles.Split(';').Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToList();

            const string pattern = @"\[ROLES:(.+?)\]";

            template = Regex.Replace(template, pattern, delegate (Match match)
            {
                if (userRoleArray == null || userRoleArray.Count == 0)
                {
                    return string.Empty;
                }

                var roles = match.Groups[1].Value.Split(';').Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim());

                var replacement = roles.FirstOrDefault(role => role != "-10" && userRoleArray.Contains(role));

                return replacement ?? string.Empty;
            });

            return template;
        }


        public static string GetTemplateSection(string template, string startTag, string endTag, bool returnTemplateIfTagNotFound = true)
        {
            var intStartTag = template.IndexOf(startTag, StringComparison.Ordinal);
            var intEndTag = template.IndexOf(endTag, StringComparison.Ordinal);
            if (intStartTag >= 0 && intEndTag > intStartTag)
            {
                var intSubTempStart = intStartTag + startTag.Length;
                var intSubTempEnd = intEndTag;
                var intSubTempLength = intSubTempEnd - intSubTempStart;
                var sSubTemp = template.Substring(intSubTempStart, intSubTempLength);
                return sSubTemp;
            }

            return returnTemplateIfTagNotFound ? template : string.Empty;
        }

        public static string ReplaceSubSection(string template, string subTemplate, string startTag, string endTag)
        {
            var intStartTag = template.IndexOf(startTag, StringComparison.Ordinal);
            var intEndTag = template.IndexOf(endTag, StringComparison.Ordinal);
            if (intStartTag >= 0 && intEndTag > intStartTag)
            {
                var intSubTempStart = intStartTag + startTag.Length;
                var intSubTempEnd = intEndTag - 1;
                var intSubTempLength = intSubTempEnd - intSubTempStart;
                template = template.Substring(0, intStartTag) + subTemplate + template.Substring(intEndTag + endTag.Length);
            }

            return template;
        }

        public static StringBuilder ReplaceSubSection(StringBuilder template, string subTemplate, string startTag, string endTag)
        {
            var intStartTag = template.ToString().IndexOf(startTag, StringComparison.Ordinal);
            var intEndTag = template.ToString().IndexOf(endTag, StringComparison.Ordinal);
            if (intStartTag >= 0 && intEndTag > intStartTag)
            {
                var intSubTempStart = intStartTag + startTag.Length;
                var intSubTempEnd = intEndTag - 1;
                var intSubTempLength = intSubTempEnd - intSubTempStart;
                template = new StringBuilder(template.ToString().Substring(0, intStartTag) + subTemplate + template.ToString().Substring(intEndTag + endTag.Length));
            }
            return template;
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use ParseProfile(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string template, CurrentUserTypes currentUserType, int currentUserId)")]
        public static string ParseProfile(int portalId, int userId, string template, CurrentUserTypes currentUserType, int currentUserId)
            => ParseProfile(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(portalId, userId), template, currentUserType, currentUserId);

        internal static string ParseProfile(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string template, CurrentUserTypes currentUserType, int currentUserId)
        {
            var s = template ?? string.Empty;
            const string pattern = "(\\[DNN:PROFILE:(.+?)\\])";

            foreach (Match match in Regex.Matches(s, pattern))
            {
                var sReplace = string.Empty;
                var sResource = string.Empty;
                if (user != null)
                {
                    var profproperties = user.Properties;
                    var profprop = profproperties.GetByName(match.Groups[2].Value);
                    sResource = "ProfileProperties_{0}";
                    if (profprop != null)
                    {
                        sResource = string.Format(sResource, match.Groups[2].Value);

                        if (profprop.Visibility == DotNetNuke.Entities.Users.UserVisibilityMode.AdminOnly && (currentUserType != CurrentUserTypes.Anon || currentUserType != CurrentUserTypes.Auth))
                        {
                            sReplace = profprop.PropertyValue;
                        }
                        else if (profprop.Visibility == DotNetNuke.Entities.Users.UserVisibilityMode.MembersOnly && currentUserType != CurrentUserTypes.Anon)
                        {
                            sReplace = profprop.PropertyValue;
                        }
                        else if (profprop.Visibility == DotNetNuke.Entities.Users.UserVisibilityMode.AllUsers)
                        {
                            sReplace = profprop.PropertyValue;
                        }
                        else
                        {
                            sReplace = "[RESX:Private]";
                        }

                        sResource = DotNetNuke.Services.Localization.Localization.GetString(sResource, "~/admin/users/app_localresources/profile.ascx.resx");
                    }
                }

                s = s.Replace(match.Value, sReplace);
                s = s.Replace(string.Concat("[RESX:DNNProfile:", match.Groups[2].Value, "]"), sResource);
            }

            return s;
        }

        private static string GetTopicTemplate(int topicTemplateId, int moduleId)
        {
            return TemplateCache.GetCachedTemplate(moduleId, "TopicView", topicTemplateId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use string PreviewTopic(int topicTemplateID, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string body, string imagePath,DateTime postDate, CurrentUserTypes currentUserType, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string PreviewTopic(int topicTemplateID, int portalId, int moduleId, int tabId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo, int userId, string body, string imagePath, User up, DateTime postDate, CurrentUserTypes currentUserType, int currentUserId, int timeZoneOffset)
                => PreviewTopic(topicTemplateID, forumInfo, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(portalId, userId), body, imagePath, postDate, currentUserType, currentUserId, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use string PreviewTopic(int topicTemplateID, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string body, string imagePath,DateTime postDate, CurrentUserTypes currentUserType, int currentUserId, TimeSpan timeZoneOffset)")]
        public static string PreviewTopic(int topicTemplateID, int portalId, int moduleId, int tabId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo, int userId, string body, string imagePath, User up, DateTime postDate, CurrentUserTypes currentUserType, int currentUserId, TimeSpan timeZoneOffset)
                => PreviewTopic(topicTemplateID, forumInfo, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(portalId, userId), body, imagePath, postDate, currentUserType, currentUserId, timeZoneOffset);

        internal static string PreviewTopic(int topicTemplateID, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string body, string imagePath,DateTime postDate, CurrentUserTypes currentUserType, int currentUserId, TimeSpan timeZoneOffset)
        {
            var sTemplate = GetTopicTemplate(topicTemplateID, forumInfo.ModuleId);
            try
            {
                var mainSettings = SettingsBase.GetModuleSettings(forumInfo.ModuleId);
                var sTopic = GetTemplateSection(sTemplate, "[TOPIC]", "[/TOPIC]");
                sTopic = sTopic.Replace("[ACTIONS:ALERT]", string.Empty);
                sTopic = sTopic.Replace("[ACTIONS:EDIT]", string.Empty);
                sTopic = sTopic.Replace("[ACTIONS:DELETE]", string.Empty);
                sTopic = sTopic.Replace("[ACTIONS:QUOTE]", string.Empty);
                sTopic = sTopic.Replace("[ACTIONS:REPLY]", string.Empty);
                sTopic = sTopic.Replace("[POSTDATE]", Utilities.GetUserFormattedDateTime(postDate, forumInfo.PortalId, user.UserId));
                sTopic = sTopic.Replace("[POSTINFO]", GetPostInfo(forumInfo.ModuleId, user, imagePath, false, HttpContext.Current.Request.UserHostAddress, true, currentUserType, currentUserId, false, timeZoneOffset));
                sTemplate = ParsePreview(forumInfo.PortalId, sTopic, body, forumInfo.ModuleId);
                sTemplate = string.Concat("<table class=\"afgrid\" width=\"100%\" cellspacing=\"0\" cellpadding=\"4\" border=\"1\">", sTemplate);
                sTemplate = string.Concat(sTemplate, "</table>");
                sTemplate = Utilities.LocalizeControl(sTemplate);
                sTemplate = Utilities.StripTokens(sTemplate);
            }
            catch (Exception ex)
            {
                sTemplate = ex.ToString();
            }

            return sTemplate;
        }

        private static string ParsePreview(int portalId, string template, string message, int moduleId)
        {
            // TODO: Legacy Attachments Functionality - Probably can remove.
            if (message.Contains("&#91;IMAGE:"))
            {
                var strHost = Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)) + "/";
                const string pattern = "(&#91;IMAGE:(.+?)&#93;)";
                foreach (Match match in Regex.Matches(message, pattern))
                {
                    var sImage = string.Format("<img src=\"{0}DesktopModules/ActiveForums/viewer.aspx?portalid={1}&moduleid={2}&attachid={3}\" border=\"0\" />", strHost, portalId, moduleId, match.Groups[2].Value);
                    message = message.Replace(match.Value, sImage);
                }
            }

            // TODO: Legacy Attachments Functionality - Probably can remove.
            if (message.Contains("&#91;THUMBNAIL:"))
            {
                var strHost = string.Concat(Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)), "/");
                const string pattern = "(&#91;THUMBNAIL:(.+?)&#93;)";
                foreach (Match match in Regex.Matches(message, pattern))
                {
                    var thumbId = match.Groups[2].Value.Split(':')[0];
                    var parentId = match.Groups[2].Value.Split(':')[1];
                    var sImage = string.Format("<a href=\"{0}DesktopModules/ActiveForums/viewer.aspx?portalid={1}&moduleid={2}&attachid={3}\" target=\"_blank\"><img src=\"{4}DesktopModules/ActiveForums/viewer.aspx?portalid={5}&moduleid={6}&attachid={7}\" border=\"0\" /></a>", strHost, portalId, moduleId, parentId, strHost, portalId, moduleId, thumbId);
                    message = message.Replace(match.Value, sImage);
                }
            }

            template = template.Replace("[BODY]", message);
            if (Regex.IsMatch(template, "<CODE([^>]*)>", RegexOptions.IgnoreCase))
            {
                if (Regex.IsMatch(message, "<CODE([^>]*)>", RegexOptions.IgnoreCase))
                {
                    foreach (Match m in Regex.Matches(message, "<CODE([^>]*)>(.*?)</CODE>", RegexOptions.IgnoreCase))
                    {
                        message = message.Replace(m.Value, m.Value.Replace("<br>", System.Environment.NewLine));
                    }
                }

                var objCode = new CodeParser();
                template = CodeParser.ParseCode(HttpUtility.HtmlDecode(template));
            }

            return template;
        }
    }
}
