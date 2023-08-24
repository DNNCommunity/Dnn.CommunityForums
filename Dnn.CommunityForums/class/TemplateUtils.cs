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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using DotNetNuke.Entities.Portals;
using Microsoft.ApplicationBlocks.Data;

namespace DotNetNuke.Modules.ActiveForums
{
    public class TemplateUtils
    {
        public static List<SubscriptionInfo> lstSubscriptionInfo { get; set; }

        public static string ShowIcon(bool canView, int forumID, int userId, DateTime dateAdded, DateTime lastRead, int lastPostId)
        {
            if (!canView)
                return "folder_forbidden.png";

            if (lastPostId == 0 || userId == -1)
                return "folder_closed.png";

            if (lastRead == DateTime.MinValue)
                lastRead = Utilities.NullDate();

            if (dateAdded != Utilities.NullDate())
                return dateAdded > lastRead ? "folder_new.png" : "folder.png";

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

        private static TemplateInfo GetTemplateByName(string templateName, int moduleId, int portalId)
        {
            var tc = new TemplateController();
            TemplateInfo ti;
            try
            {
                ti = tc.Template_Get(templateName, portalId, moduleId);
            }
            catch (Exception ex)
            {
                ti = new TemplateInfo { TemplateHTML = string.Concat("Error loading ", templateName, " template.") };
                ti.TemplateText = ti.TemplateHTML;
            }

            return ti;
        }

        #region Email
        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, int timeZoneOffset)
        {
            return ParseEmailTemplate(template, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, string.Empty, null, -1, Utilities.GetCultureInfoForUser(portalID, -1), new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0), false);
        }

        public static string ParseEmailTemplate(string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, int timeZoneOffset)
        {
            return ParseEmailTemplate(string.Empty, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, string.Empty, null, -1, Utilities.GetCultureInfoForUser(portalID, -1), new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0), false);
        }

        public static string ParseEmailTemplate(string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, int timeZoneOffset)
        {
            return ParseEmailTemplate(string.Empty, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, comments, null, -1, Utilities.GetCultureInfoForUser(portalID, -1), new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0), false);
        }

        public static string ParseEmailTemplate(string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, DotNetNuke.Entities.Users.UserInfo user, int timeZoneOffset)
        {
            return ParseEmailTemplate(string.Empty, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, string.Empty, user, -1, Utilities.GetCultureInfoForUser(portalID, -1), new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0), false);
        }
        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, int userId, int timeZoneOffset)
        {
            var uc = new DotNetNuke.Entities.Users.UserController();
            var usr = uc.GetUser(portalID, userId);
            return ParseEmailTemplate(template, templateName, portalID, moduleID, tabID, forumID, topicId, replyId, comments, usr, userId, Utilities.GetCultureInfoForUser(portalID, userId), new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0), false);
        }
        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, DotNetNuke.Entities.Users.UserInfo user, int userId, int timeZoneOffset)
        {
            return ParseEmailTemplate(template, templateName, portalID, moduleID, tabID, forumID, topicId: topicId, replyId: replyId, comments: comments, user: user, userId: userId, userCultureInfo: Utilities.GetCultureInfoForUser(portalID, userId), timeZoneOffset: new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0), topicSubscriber: false);
        }
        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, CultureInfo userCulture, TimeSpan timeZoneOffset)
        {
            return ParseEmailTemplate(template, templateName: templateName, portalID: portalID, moduleID: moduleID, tabID: tabID, forumID: forumID, topicId: topicId, replyId: replyId, comments: string.Empty, userId: 0, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false);
        }

        public static string ParseEmailTemplate(string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, CultureInfo userCulture, TimeSpan timeZoneOffset)
        {
            return ParseEmailTemplate(template: string.Empty, templateName: templateName, portalID: portalID, moduleID: moduleID, tabID: tabID, forumID: forumID, topicId: topicId, replyId: replyId, comments: string.Empty, user: null, userId: -1, userCultureInfo: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false);
        }

        public static string ParseEmailTemplate(string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, CultureInfo userCulture, TimeSpan timeZoneOffset)
        {
            return ParseEmailTemplate(template: string.Empty, templateName: templateName, portalID: portalID, moduleID: moduleID, tabID: tabID, forumID: forumID, topicId: topicId, replyId: replyId, comments: comments, user: null, userId: -1, userCultureInfo: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false);
        }

        public static string ParseEmailTemplate(string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, DotNetNuke.Entities.Users.UserInfo user, CultureInfo userCulture, TimeSpan timeZoneOffset)
        {
            return ParseEmailTemplate(string.Empty, templateName, portalID, moduleID, tabID, forumID: forumID, topicId: topicId, replyId: replyId, comments: string.Empty, user: user, userId: -1, userCultureInfo: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false);
        }

        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, int userId, CultureInfo userCulture, TimeSpan timeZoneOffset)
        {
            var uc = new DotNetNuke.Entities.Users.UserController();
            var usr = uc.GetUser(portalID, userId);
            return ParseEmailTemplate(template, templateName, portalID, moduleID, tabID, forumID, topicId: topicId, replyId: replyId, comments: comments, user: usr, userId: userId, userCultureInfo: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false);
        }

        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, int userId, CultureInfo userCulture, TimeSpan timeZoneOffset, bool topicSubscriber)
        {
            var uc = new DotNetNuke.Entities.Users.UserController();
            var usr = uc.GetUser(portalID, userId);
            return ParseEmailTemplate(template, templateName, portalID, moduleID, tabID, forumID, topicId: topicId, replyId: replyId, comments: comments, user: usr, userId: userId, userCultureInfo: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: topicSubscriber);
        }

        public static string ParseEmailTemplate(string template, string templateName, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, string comments, DotNetNuke.Entities.Users.UserInfo user, int userId, CultureInfo userCultureInfo, TimeSpan timeZoneOffset, bool topicSubscriber)
        {
            PortalSettings portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
            var ms = SettingsBase.GetModuleSettings(moduleID);
            var sOut = template;

            // If we have a template name, load the template into sOut
            if (templateName != string.Empty)
            {
                if (templateName.Contains("_Subject_"))
                {
                    templateName = templateName.Replace(string.Concat("_Subject_", moduleID), string.Empty);
                    var objTemplate = GetTemplateByName(templateName, moduleID, portalID);
                    sOut = objTemplate.Subject;
                }
                else
                {
                    var objTemplate = GetTemplateByName(templateName, moduleID, portalID);
                    sOut = objTemplate.TemplateHTML;
                }
            }

            // Load Subject and body from topic or reply
            var subject = string.Empty;
            var body = string.Empty;
            var dateCreated = Utilities.NullDate();
            var authorName = string.Empty;

            if (topicId > 0 && replyId > 0)
            {
                var ri = new ReplyController().Reply_Get(portalID, moduleID, topicId, replyId);
                if (ri != null)
                {
                    subject = ri.Content.Subject;
                    body = ri.Content.Body;
                    dateCreated = ri.Content.DateCreated;
                    authorName = ri.Content.AuthorName;
                }
            }
            else
            {
                var ti = new TopicsController().Topics_Get(portalID, moduleID, topicId);
                if (ti != null)
                {
                    subject = ti.Content.Subject;
                    body = ti.Content.Body;
                    dateCreated = ti.Content.DateCreated;
                    authorName = ti.Content.AuthorName;
                }
            }

            body = Utilities.ManageImagePath(body, Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)));

            // load the forum information
            var fi = new ForumController().Forums_Get(portalId: portalID, moduleId: moduleID, forumId: forumID, useCache: true);

            // Load the user if needed
            if (user == null)
            {
                var objUsers = new DotNetNuke.Entities.Users.UserController();
                var objUser = objUsers.GetUser(portalID, userId);
                user = objUser;
            }

            // Load the user properties
            string sFirstName;
            string sLastName;
            string sDisplayName;
            string sUsername;

            if (user != null)
            {
                sFirstName = user.FirstName;
                sLastName = user.LastName;
                sDisplayName = user.DisplayName;
                sUsername = user.Username;
            }
            else
            {
                sFirstName = string.Empty;
                sLastName = string.Empty;
                sDisplayName = string.Empty;
                sUsername = string.Empty;
            }

            // Build the link
            string link;
            if (string.IsNullOrEmpty(fi.PrefixURL) || !Utilities.UseFriendlyURLs(moduleID))
            {
                if (replyId == 0)
                    link = ms.UseShortUrls ? Common.Globals.NavigateURL(tabID, string.Empty, new[] { string.Concat(ParamKeys.TopicId, "=", topicId) })
                        : Common.Globals.NavigateURL(tabID, string.Empty, new[] { string.Concat(ParamKeys.ForumId, "=", forumID), ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + topicId });
                else
                    link = ms.UseShortUrls ? Common.Globals.NavigateURL(tabID, string.Empty, new[] { string.Concat(ParamKeys.TopicId, "=", topicId), string.Concat(ParamKeys.ContentJumpId, "=", replyId) })
                        : Common.Globals.NavigateURL(tabID, string.Empty, new[] { string.Concat(ParamKeys.ForumId, "=", forumID), string.Concat(ParamKeys.ViewType, "=", Views.Topic), string.Concat(ParamKeys.TopicId, "=", topicId), string.Concat(ParamKeys.ContentJumpId, "=", replyId) });
            }
            else
            {
                var contentId = (replyId > 0) ? replyId : -1;
                link = String.Concat(Common.Globals.NavigateURL(tabID), "/", new Data.Common().GetUrl(moduleID, -1, forumID, topicId, -1, contentId));
            }

            if (!(link.StartsWith("http")))
            {
                if (!link.StartsWith("/"))
                    link = string.Concat("/", link);

                if (link.IndexOf(HttpContext.Current.Request.Url.Host, StringComparison.Ordinal) == -1)
                    link = Common.Globals.AddHTTP(HttpContext.Current.Request.Url.Host) + link;
            }


            // Build the forum Url
            var forumURL = ms.UseShortUrls ? Common.Globals.NavigateURL(tabID, string.Empty, new[] { string.Concat(ParamKeys.ForumId, "=", forumID) })
                : Common.Globals.NavigateURL(tabID, string.Empty, new[] { string.Concat(ParamKeys.ForumId, "=", forumID), string.Concat(ParamKeys.ViewType, "=", Views.Topics) });

            // Build Moderation url
            var modLink = Common.Globals.NavigateURL(tabID, string.Empty, new[] { ParamKeys.ViewType + "=modtopics", string.Concat(ParamKeys.ForumId, "=", forumID) });
            if (modLink.IndexOf(HttpContext.Current.Request.Url.Host, StringComparison.Ordinal) == -1)
                modLink = Common.Globals.AddHTTP(HttpContext.Current.Request.Url.Host) + modLink;

            var result = new StringBuilder(sOut);

            result.Replace("[DISPLAYNAME]", UserProfiles.GetDisplayName(moduleID, userId, authorName, sFirstName, sLastName, sDisplayName));
            result.Replace("[USERNAME]", sUsername);
            result.Replace("[USERID]", userId.ToString());
            result.Replace("[FORUMNAME]", fi.ForumName);
            result.Replace("[PORTALID]", portalID.ToString());
            result.Replace("[FIRSTNAME]", sFirstName);
            result.Replace("[LASTNAME]", sLastName);
            result.Replace("[FULLNAME]", string.Concat(sFirstName, " ", sLastName));
            result.Replace("[GROUPNAME]", fi.GroupName);
            result.Replace("[POSTDATE]", Utilities.GetUserFormattedDateTime(dateCreated, userCultureInfo, timeZoneOffset));
            result.Replace("[COMMENTS]", comments);
            result.Replace("[PORTALNAME]", portalSettings.PortalName);
            result.Replace("[MODLINK]", string.Concat("<a href=\"", modLink, "\">", modLink, "</a>"));
            result.Replace("[LINK]", string.Concat("<a href=\"", link, "\">", link, "</a>"));
            result.Replace("[HYPERLINK]", string.Concat("<a href=\"", link, "\">", link, "</a>"));
            result.Replace("[LINKURL]", link);
            result.Replace("[FORUMURL]", forumURL);
            result.Replace("[FORUMLINK]", string.Concat("<a href=\"", forumURL, "\">", forumURL, "</a>"));

            result.Replace("[POSTEDORREPLIEDTO]", (replyId <= 0 ? Utilities.GetSharedResource("[RESX:posted]") : Utilities.GetSharedResource("[RESX:repliedto]")));
            result.Replace("[POSTEDTO]", (replyId <= 0 ? Utilities.GetSharedResource("[RESX:postedto]") : string.Empty));
            result.Replace("[REPLIEDTO]", (replyId > 0 ? Utilities.GetSharedResource("[RESX:repliedto]") : string.Empty));
            result.Replace("[NEWPOST]", (replyId <= 0 ? Utilities.GetSharedResource("[RESX:NewPost]") : string.Empty));
            result.Replace("[NEWREPLY]", (replyId > 0 ? Utilities.GetSharedResource("[RESX:NewReply]") : string.Empty));
            result.Replace("[SUBSCRIBEDTOPIC]", (topicSubscriber ? Utilities.GetSharedResource("[RESX:SubscribedTopic]") : string.Empty));
            result.Replace("[SUBSCRIBEDTOPICSUBJECT]", (topicSubscriber ? string.Format(Utilities.GetSharedResource("[RESX:SubscribedTopicSubject]"), subject) : string.Empty));
            result.Replace("[SUBSCRIBEDTOPICFORUMNAME]", (topicSubscriber ? string.Format(Utilities.GetSharedResource("[RESX:SubscribedTopicForumName]"), subject, fi.ForumName) : string.Empty));
            result.Replace("[SUBSCRIBEDFORUM]", (topicSubscriber ? string.Empty : "[RESX:SubscribedForum]"));
            result.Replace("[SUBSCRIBEDFORUMNAME]", (topicSubscriber ? string.Empty : string.Format(Utilities.GetSharedResource("[RESX:SubscribedForumName]"), fi.ForumName)));
            result.Replace("[SUBSCRIBEDFORUMORTOPICSUBJECTFORUMNAME]", (topicSubscriber ? string.Format(Utilities.GetSharedResource("[RESX:SubscribedTopicForumName]"), subject, fi.ForumName) : string.Format(Utilities.GetSharedResource("[RESX:SubscribedForumTopicForumName]"), subject, fi.ForumName)));

            // Introduced for Active Forum Email Connector plug-in Starts
            if (result.ToString().Contains("[EMAILCONNECTORITEMID]"))
            {
                // This Try with empty catch is introduced here because this code section is for Email Connector functionality only and this section should not 
                // cause any issue to DNN Community Forums functionality in case it does not run successfully.
                try
                {
                    long itemID = GetEmailInfo(portalID, moduleID, forumID, topicId, HttpContext.Current.Request.UserHostAddress);
                    result.Replace("[EMAILCONNECTORITEMID]", itemID.ToString());
                }
                catch
                { }
            }
            // Introduced for Active Forum Email Connector plug-in Ends

            if (user != null)
            {
                result.Replace("[SENDERUSERNAME]", user.UserID.ToString());
                result.Replace("[SENDERFIRSTNAME]", user.FirstName);
                result.Replace("[SENDERLASTNAME]", user.LastName);
                result.Replace("[SENDERDISPLAYNAME]", user.DisplayName);
            }
            else
            {
                result.Replace("[SENDERUSERNAME]", string.Empty);
                result.Replace("[SENDERFIRSTNAME]", string.Empty);
                result.Replace("[SENDERLASTNAME]", string.Empty);
                result.Replace("[SENDERDISPLAYNAME]", string.Empty);
            }

            result.Replace("[SUBJECT]", subject);
            result.Replace("[BODY]", body);
            result.Replace("[Body]", body);

            return result.ToString();
        }

        private static long GetEmailInfo(int PortalId, int ModuleId, int forumID, int topicID, string ipAddress)
        {
            long ItemID = -1;

            DotNetNuke.Framework.Providers.ProviderConfiguration _providerConfiguration = DotNetNuke.Framework.Providers.ProviderConfiguration.GetProviderConfiguration("data");
            string connectionString;
            string objectQualifier;
            string databaseOwner;
            connectionString = ConfigurationManager.ConnectionStrings["SiteSqlServer"].ConnectionString;
            var objProvider = (DotNetNuke.Framework.Providers.Provider)(_providerConfiguration.Providers[_providerConfiguration.DefaultProvider]);

            objectQualifier = objProvider.Attributes["objectQualifier"];
            if (objectQualifier != "" && objectQualifier.EndsWith("_") == false)
            {
                objectQualifier += "_";
            }

            databaseOwner = objProvider.Attributes["databaseOwner"];
            if (databaseOwner != "" && databaseOwner.EndsWith(".") == false)
            {
                databaseOwner += ".";
            }

            StringBuilder userIds = new StringBuilder();
            userIds.Append("(");

            SubscriptionInfo[] arrSubscriptionInfo = lstSubscriptionInfo.ToArray();
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

            //dbPrefix = databaseOwner + objectQualifier + databaseObjectPrefix;
            IDataReader dataReader = (IDataReader)(SqlHelper.ExecuteReader(connectionString, databaseOwner + objectQualifier + "ActiveForumsEmailConnector_GetEmailInfo", PortalId, ModuleId, forumID, topicID, ipAddress, userIds.ToString()));
            if (dataReader.Read())
            {
                ItemID = Convert.ToInt32(dataReader["RecordID"]);
            }

            return ItemID;
        }
        #endregion

        #region Profile
        public static string GetPostInfo(int portalId, int moduleId, int userId, string username, User up, string imagePath, bool isMod, string ipAddress, bool isUserOnline, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, int timeZoneOffset)
        {
            return GetPostInfo(portalId, moduleId, userId, username, up, imagePath, isMod, ipAddress, isUserOnline, currentUserType, currentUserId, userPrefHideAvatar, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        public static string GetPostInfo(int portalId, int moduleId, int userId, string username, User up, string imagePath, bool isMod, string ipAddress, bool isUserOnline, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, TimeSpan timeZoneOffset)
        {
            var sPostInfo = ParseProfileInfo(portalId, moduleId, userId, username, up, imagePath, isMod, ipAddress, currentUserType, currentUserId, userPrefHideAvatar, timeZoneOffset);
            if (sPostInfo.ToLower().Contains("<br"))
                return sPostInfo;

            var sr = new StringReader(sPostInfo);
            var sTrim = string.Empty;

            while (sr.Peek() != -1)
            {
                var tmp = sr.ReadLine();
                if (tmp != null && tmp.Trim() != string.Empty)
                {
                    sTrim += tmp.Trim() + "<br />";
                }
            }

            return sTrim;
        }

        public static string ParseProfileInfo(int portalId, int moduleId, int userId, string username, User up, string imagePath, bool isMod, string ipAddress, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, int timeZoneOffset)
        {
            return ParseProfileInfo(portalId, moduleId, userId, username, up, imagePath, isMod, ipAddress, currentUserType, currentUserId, userPrefHideAvatar, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        public static string ParseProfileInfo(int portalId, int moduleId, int userId, string username, User up, string imagePath, bool isMod, string ipAddress, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, TimeSpan timeZoneOffset)
        {
            var mainSettings = SettingsBase.GetModuleSettings(moduleId);

            var cacheKey = string.Format(CacheKeys.ProfileInfo, moduleId);
            var myTemplate = Convert.ToString(DataCache.SettingsCacheRetrieve(moduleId, cacheKey));
            if (string.IsNullOrEmpty(myTemplate))
            {
                var objTemplateInfo = GetTemplateByName("ProfileInfo", moduleId, portalId);
                myTemplate = objTemplateInfo.TemplateHTML;
                if (cacheKey != string.Empty)
                    DataCache.SettingsCacheStore(moduleId, cacheKey, myTemplate);
            }

            myTemplate = ParseProfileTemplate(myTemplate, up, portalId, moduleId, imagePath, currentUserType, true, userPrefHideAvatar, false, ipAddress, currentUserId, timeZoneOffset);

            return myTemplate;
        }

        public static string ParseProfileTemplate(string profileTemplate, int userId, int portalId, int moduleId, int currentUserId, int timeZoneOffset)
        {
            return ParseProfileTemplate(profileTemplate, userId, portalId, moduleId, currentUserId, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        public static string ParseProfileTemplate(string profileTemplate, int userId, int portalId, int moduleId, int currentUserId, TimeSpan timeZoneOffset)
        {
            var uc = new UserController();
            var up = uc.GetUser(portalId, moduleId, userId);

            return ParseProfileTemplate(profileTemplate, up, portalId, moduleId, string.Empty, CurrentUserTypes.Anon, false, false, false, string.Empty, currentUserId, timeZoneOffset);
        }

        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, int timeZoneOffset)
        {
            return ParseProfileTemplate(profileTemplate, up, portalId, moduleId, imagePath, currentUserType, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, TimeSpan timeZoneOffset)
        {
            return ParseProfileTemplate(profileTemplate, up, portalId, moduleId, imagePath, currentUserType, false, false, false, string.Empty, -1, timeZoneOffset);
        }

        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, int timeZoneOffset)
        {
            return ParseProfileTemplate(profileTemplate, up, portalId, moduleId, imagePath, currentUserType, false, false, false, string.Empty, -1, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, TimeSpan timeZoneOffset)
        {
            return ParseProfileTemplate(profileTemplate, up, portalId, moduleId, imagePath, currentUserType, false, false, false, string.Empty, -1, timeZoneOffset);
        }

        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int timeZoneOffset)
        {
            return ParseProfileTemplate(profileTemplate, up, portalId, moduleId, imagePath, currentUserType, legacyTemplate, userPrefHideAvatar, userPrefHideSignature, ipAddress, -1, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, TimeSpan timeZoneOffset)
        {
            return ParseProfileTemplate(profileTemplate, up, portalId, moduleId, imagePath, currentUserType, legacyTemplate, userPrefHideAvatar, userPrefHideSignature, ipAddress, -1, timeZoneOffset);
        }

        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, int currentUserId, TimeSpan timeZoneOffset)
        {
            return ParseProfileTemplate(profileTemplate, up, portalId, moduleId, imagePath, currentUserType, false, false, false, string.Empty, currentUserId, timeZoneOffset);
        }

        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, int timeZoneOffset)
        {
            return ParseProfileTemplate(profileTemplate, up, portalId, moduleId, imagePath, currentUserType, legacyTemplate, userPrefHideAvatar, userPrefHideSignature, ipAddress, currentUserId, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }

        public static string ParseProfileTemplate(string profileTemplate, User up, int portalId, int moduleId, string imagePath, CurrentUserTypes currentUserType, bool legacyTemplate, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)
        {
            try
            {
                if (legacyTemplate)
                    profileTemplate = CleanTemplate(profileTemplate);

                if (up.Profile == null)
                    up = new UserController().FillProfile(portalId, -1, up);

                // TODO figure out why/if this recurion is possible.  Seems a bit scary as it could create a loop.
                if (profileTemplate.Contains("[POSTINFO]"))
                {
                    var sPostInfo = GetPostInfo(portalId, moduleId, up.UserId, up.UserName, up, imagePath, false, ipAddress, up.Profile.IsUserOnline, currentUserType, currentUserId, userPrefHideAvatar, timeZoneOffset);
                    profileTemplate = profileTemplate.Replace("[POSTINFO]", sPostInfo);
                }

                var mainSettings = SettingsBase.GetModuleSettings(moduleId);

                // Parse DNN profile fields if needed
                var pt = profileTemplate;
                if (pt.IndexOf("[DNN:PROFILE:", StringComparison.Ordinal) >= 0)
                    pt = ParseProfile(portalId, up.UserId, pt, currentUserType, currentUserId);

                // Parse Roles
                if (pt.Contains("[ROLES:"))
                    pt = ParseRoles(pt, (up.UserId == -1) ? string.Empty : up.Profile.Roles);


                var result = new StringBuilder(pt);

                // Used in a few places to determine if info should be shown or removed.
                var isMod = (currentUserType == CurrentUserTypes.Admin || currentUserType == CurrentUserTypes.ForumMod || currentUserType == CurrentUserTypes.SuperUser);

                // Used in a few places to determine if info should be shown or removed.
                var isAdmin = (currentUserType == CurrentUserTypes.Admin || currentUserType == CurrentUserTypes.SuperUser);

                var isAuthethenticated = currentUserType != CurrentUserTypes.Anon;

                // IP Address
                result.Replace("[MODIPADDRESS]", isMod ? ipAddress : string.Empty);

                // User Edit
                result.Replace("[AF:BUTTON:EDITUSER]", isAdmin && up.UserId > 0 ? string.Format("<button class='af-button af-button-edituser' data-id='{0}' data-name='{1}'>[RESX:Edit]</button>", up.UserId, Utilities.JSON.EscapeJsonString(up.DisplayName)) : string.Empty);

                // Points
                var totalPoints = up.PostCount;
                if (mainSettings.EnablePoints && up.UserId > 0 && up.Profile != null)
                {
                    totalPoints = (up.Profile.TopicCount * mainSettings.TopicPointValue) + (up.Profile.ReplyCount * mainSettings.ReplyPointValue) + (up.Profile.AnswerCount * mainSettings.AnswerPointValue) + up.Profile.RewardPoints;
                    result.Replace("[AF:PROFILE:TOTALPOINTS]", totalPoints.ToString());
                    result.Replace("[AF:POINTS:VIEWCOUNT]", up.Profile.ViewCount.ToString());
                    result.Replace("[AF:POINTS:ANSWERCOUNT]", up.Profile.AnswerCount.ToString());
                    result.Replace("[AF:POINTS:REWARDPOINTS]", up.Profile.RewardPoints.ToString());
                }
                else
                {
                    result.Replace("[AF:PROFILE:TOTALPOINTS]", string.Empty);
                    result.Replace("[AF:POINTS:VIEWCOUNT]", string.Empty);
                    result.Replace("[AF:POINTS:ANSWERCOUNT]", string.Empty);
                    result.Replace("[AF:POINTS:REWARDPOINTS]", string.Empty);
                }

                // User Status
                var sUserStatus = string.Empty;
                if (mainSettings.UsersOnlineEnabled && up.UserId > 0 && up.Profile != null)
                    sUserStatus = UserProfiles.UserStatus(imagePath, up.Profile.IsUserOnline, up.UserId, moduleId, "[RESX:UserOnline]", "[RESX:UserOffline]");

                result.Replace("[AF:PROFILE:USERSTATUS]", sUserStatus);
                result.Replace("[AF:PROFILE:USERSTATUS:CSS]", sUserStatus.Contains("online") ? "af-status-online" : "af-status-offline");

                // Rank
                result.Replace("[AF:PROFILE:RANKDISPLAY]", (up.UserId > 0) ? UserProfiles.GetUserRank(portalId, moduleId, up.UserId, totalPoints, 0) : string.Empty);
                result.Replace("[AF:PROFILE:RANKNAME]", (up.UserId > 0) ? UserProfiles.GetUserRank(portalId, moduleId, up.UserId, totalPoints, 1) : string.Empty);

                // PM Image/link
                var pmUrl = string.Empty;
                var pmLink = string.Empty;
                if (up.UserId > 0 && currentUserId >= 0 && up.UserId != currentUserId)
                {
                    switch (mainSettings.PMType)
                    {
                        case PMTypes.Core:
                            pmLink = string.Concat("<img class='ComposeMessage' data-recipient='{ \"id\": \"user-", up.UserId, "\", \"name\": \"", HttpUtility.JavaScriptStringEncode(up.DisplayName), "\"}' src='", imagePath, "/icon_pm.png' alt=\"[RESX:SendPM]\" title=\"[RESX:SendPM]\" border=\"0\" /></a>");
                            break;

                        case PMTypes.Ventrian:
                            pmUrl = Common.Globals.NavigateURL(mainSettings.PMTabId, string.Empty, new[] { "type=compose", string.Concat("sendto=", up.UserId) });
                            pmLink = string.Concat("<a href=\"", pmUrl, "\"><img src=\"", imagePath, "/icon_pm.png\" alt=\"[RESX:SendPM]\" border=\"0\" /></a>");
                            break;
                    }
                }

                result.Replace("[AF:PROFILE:PMLINK]", pmLink);
                result.Replace("[AF:PROFILE:PMURL]", pmUrl);

                // Signature
                var sSignature = string.Empty;
                if (mainSettings.AllowSignatures != 0 && !userPrefHideSignature && up.Profile != null && !up.Profile.SignatureDisabled)
                {
                    sSignature = up.Profile.Signature;

                    if (sSignature != string.Empty)
                        sSignature = Utilities.ManageImagePath(sSignature);

                    switch (mainSettings.AllowSignatures)
                    {
                        case 1:
                            sSignature = Utilities.HTMLEncode(sSignature);
                            sSignature = sSignature.Replace(System.Environment.NewLine, "<br />");
                            break;
                        case 2:
                            sSignature = Utilities.HTMLDecode(sSignature);
                            break;
                    }
                }

                result.Replace("[AF:PROFILE:SIGNATURE]", sSignature);

                // Avatar
                var sAvatar = string.Empty;
                if (!userPrefHideAvatar && !up.Profile.AvatarDisabled)
                    sAvatar = UserProfiles.GetAvatar(up.UserId, mainSettings.AvatarWidth, mainSettings.AvatarHeight);

                result.Replace("[AF:PROFILE:AVATAR]", sAvatar);

                // Display Name
                result.Replace("[AF:PROFILE:DISPLAYNAME]", UserProfiles.GetDisplayName(moduleId, true, isMod, isAdmin, up.UserId, up.UserName, up.FirstName, up.LastName, up.DisplayName));

                // These fields are no longer used
                result.Replace("[AF:PROFILE:LOCATION]", string.Empty);
                result.Replace("[AF:PROFILE:WEBSITE]", string.Empty);
                result.Replace("[AF:PROFILE:YAHOO]", string.Empty);
                result.Replace("[AF:PROFILE:MSN]", string.Empty);
                result.Replace("[AF:PROFILE:ICQ]", string.Empty);
                result.Replace("[AF:PROFILE:AOL]", string.Empty);
                result.Replace("[AF:PROFILE:OCCUPATION]", string.Empty);
                result.Replace("[AF:PROFILE:INTERESTS]", string.Empty);
                result.Replace("[AF:CONTROL:AVATAREDIT]", string.Empty);
                result.Replace("[AF:BUTTON:PROFILEEDIT]", string.Empty);
                result.Replace("[AF:BUTTON:PROFILESAVE]", string.Empty);
                result.Replace("[AF:BUTTON:PROFILECANCEL]", string.Empty);
                result.Replace("[AF:PROFILE:BIO]", string.Empty);
                result.Replace("[MODUSERSETTINGS]", string.Empty);

                // Date Created
                var sDateCreated = string.Empty;
                var sDateCreatedReplacement = "[AF:PROFILE:DATECREATED]";

                if (up.UserId > 0 && up.Profile != null && up.Profile.DateCreated != null)
                {
                    if (pt.Contains("[AF:PROFILE:DATECREATED:"))
                    {
                        var sFormat = pt.Substring(pt.IndexOf("[AF:PROFILE:DATECREATED:", StringComparison.Ordinal) + (sDateCreatedReplacement.Length), 1);
                        sDateCreated = Utilities.GetUserFormattedDateTime(up.Profile.DateCreated, portalId, currentUserId, sFormat);
                        sDateCreatedReplacement = string.Concat("[AF:PROFILE:DATECREATED:", sFormat, "]");
                    }
                    else
                    {
                        sDateCreated = Utilities.GetUserFormattedDateTime(up.Profile.DateCreated, portalId, currentUserId);
                    }
                }

                result.Replace(sDateCreatedReplacement, sDateCreated);

                // Last Activity
                var sDateLastActivity = string.Empty;
                var sDateLastActivityReplacement = "[AF:PROFILE:DATELASTACTIVITY]";
                if (up.Profile.DateLastActivity != null && up.UserId > 0)
                {
                    if (pt.Contains("[AF:PROFILE:DATELASTACTIVITY:"))
                    {
                        string sFormat = pt.Substring(pt.IndexOf("[AF:PROFILE:DATELASTACTIVITY:", StringComparison.Ordinal) + (sDateLastActivityReplacement.Length), 1);
                        sDateLastActivity = Utilities.GetUserFormattedDateTime(up.Profile.DateLastActivity, portalId, currentUserId, sFormat);
                        sDateLastActivityReplacement = string.Concat("[AF:PROFILE:DATELASTACTIVITY:", sFormat, "]");
                    }
                    else
                    {
                        sDateLastActivity = Utilities.GetUserFormattedDateTime(up.Profile.DateLastActivity, portalId, currentUserId);
                    }
                }
                result.Replace(sDateLastActivityReplacement, sDateLastActivity);

                // Post Count
                result.Replace("[AF:PROFILE:POSTCOUNT]", (up.PostCount == 0) ? string.Empty : up.PostCount.ToString());
                result.Replace("[AF:PROFILE:USERCAPTION]", up.Profile.UserCaption);
                result.Replace("[AF:PROFILE:USERID]", up.UserId.ToString());
                result.Replace("[AF:PROFILE:USERNAME]", Utilities.HTMLEncode(up.UserName).Replace("&amp;#", "&#"));
                result.Replace("[AF:PROFILE:FIRSTNAME]", Utilities.HTMLEncode(up.FirstName).Replace("&amp;#", "&#"));
                result.Replace("[AF:PROFILE:LASTNAME]", Utilities.HTMLEncode(up.LastName).Replace("&amp;#", "&#"));
                result.Replace("[AF:PROFILE:DATELASTPOST]", (up.Profile.DateLastPost == DateTime.MinValue) ? string.Empty : Utilities.GetUserFormattedDateTime(up.Profile.DateLastPost, portalId, currentUserId));
                result.Replace("[AF:PROFILE:TOPICCOUNT]", up.Profile.TopicCount.ToString());
                result.Replace("[AF:PROFILE:REPLYCOUNT]", up.Profile.ReplyCount.ToString());
                result.Replace("[AF:PROFILE:ANSWERCOUNT]", up.Profile.AnswerCount.ToString());
                result.Replace("[AF:PROFILE:REWARDPOINTS]", up.Profile.RewardPoints.ToString());
                result.Replace("[AF:PROFILE:EMAIL]", up.Email);

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
                return template;

            var userRoleArray = string.IsNullOrWhiteSpace(userRoles) ? null : userRoles.Split(';').Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToList();

            const string pattern = @"\[ROLES:(.+?)\]";

            template = Regex.Replace(template, pattern, delegate (Match match)
            {
                if (userRoleArray == null || userRoleArray.Count == 0)
                    return string.Empty;

                var roles = match.Groups[1].Value.Split(';').Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim());

                var replacement = roles.FirstOrDefault(role => role != "-10" && userRoleArray.Contains(role));

                return replacement ?? string.Empty;
            });

            return template;
        }

        private static string CleanTemplate(string template)
        {
            const string pattern = "(\\[.+?\\])";

            var sb = new StringBuilder(template);

            foreach (Match match in Regex.Matches(template, pattern))
            {
                var sReplace = string.Empty;

                switch (match.Value)
                {
                    case "[RANKNAME]":
                        sb.Replace(match.Value, "[AF:PROFILE:RANKNAME]");
                        break;
                    case "[RANKDISPLAY]":
                        sb.Replace(match.Value, "[AF:PROFILE:RANKDISPLAY]");
                        break;
                    case "[AF:PROFILE:LASTACTIVE]":
                        sb.Replace(match.Value, "[AF:PROFILE:DATELASTACTIVITY]");
                        break;
                    case "[MEMBERSINCE]":
                        sb.Replace(match.Value, "[AF:PROFILE:DATECREATED]");
                        break;
                    case "[AF:PROFILE:MEMBERSINCE]":
                        sb.Replace(match.Value, "[AF:PROFILE:DATECREATED]");
                        break;
                    case "[USERSTATUS]":
                        sb.Replace(match.Value, "[AF:PROFILE:USERSTATUS]");
                        break;
                    case "[USERCAPTION]":
                        sb.Replace(match.Value, "[AF:PROFILE:USERCAPTION]");
                        break;
                    case "[USERNAME]":
                        sb.Replace(match.Value, "[AF:PROFILE:USERNAME]");
                        break;
                    case "[USERID]":
                        sb.Replace(match.Value, "[AF:PROFILE:USERID]");
                        break;
                    case "[DISPLAYNAME]":
                        sb.Replace(match.Value, "[AF:PROFILE:DISPLAYNAME]");
                        break;
                    case "[POSTS]":
                        sb.Replace(match.Value, "[AF:PROFILE:POSTCOUNT]");
                        break;
                    case "[AVATAR]":
                        sb.Replace(match.Value, "[AF:PROFILE:AVATAR]");
                        break;
                    case "[LOCATION]":
                        sb.Replace(match.Value, "[AF:PROFILE:LOCATION]");
                        break;
                    case "[WEBSITE]":
                        sb.Replace(match.Value, "[AF:PROFILE:WEBSITE]");
                        break;
                    case "[AF:POINTS:TOPICCOUNT]":
                        sb.Replace(match.Value, "[AF:PROFILE:TOPICCOUNT]");
                        break;
                    case "[AF:POINTS:REPLYCOUNT]":
                        sb.Replace(match.Value, "[AF:PROFILE:REPLYCOUNT]");
                        break;
                    case "[SIGNATURE]":
                        sb.Replace(match.Value, "[AF:PROFILE:SIGNATURE]");
                        break;
                }
            }

            return sb.ToString();
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

        public static string ParseProfile(int portalId, int userId, string template, CurrentUserTypes currentUserType, int currentUserId)
        {
            var objuser = DotNetNuke.Entities.Users.UserController.GetUserById(portalId, userId);
            var s = template ?? string.Empty;
            const string pattern = "(\\[DNN:PROFILE:(.+?)\\])";

            foreach (Match match in Regex.Matches(s, pattern))
            {
                var sReplace = string.Empty;
                var sResource = string.Empty;
                if (objuser != null)
                {
                    var profproperties = objuser.Profile.ProfileProperties;
                    var profprop = profproperties.GetByName(match.Groups[2].Value);
                    sResource = "ProfileProperties_{0}";
                    if (profprop != null)
                    {
                        sResource = string.Format(sResource, match.Groups[2].Value);

                        if (profprop.Visibility == DotNetNuke.Entities.Users.UserVisibilityMode.AdminOnly && (currentUserType != CurrentUserTypes.Anon || currentUserType != CurrentUserTypes.Auth))
                            sReplace = profprop.PropertyValue;
                        else if (profprop.Visibility == DotNetNuke.Entities.Users.UserVisibilityMode.MembersOnly && currentUserType != CurrentUserTypes.Anon)
                            sReplace = profprop.PropertyValue;
                        else if (profprop.Visibility == DotNetNuke.Entities.Users.UserVisibilityMode.AllUsers)
                            sReplace = profprop.PropertyValue;
                        else
                            sReplace = "[RESX:Private]";

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
            var mainSettings = SettingsBase.GetModuleSettings(moduleId);

            return TemplateCache.GetCachedTemplate(moduleId, "TopicView", topicTemplateId);
        }
        public static string PreviewTopic(int topicTemplateID, int portalId, int moduleId, int tabId, Forum forumInfo, int userId, string body, string imagePath, User up, DateTime postDate, CurrentUserTypes currentUserType, int currentUserId, int timeZoneOffset)
        {
            return PreviewTopic(topicTemplateID, portalId, moduleId, tabId, forumInfo, userId, body, imagePath, up, postDate, currentUserType, currentUserId, new TimeSpan(hours: 0, minutes: timeZoneOffset, seconds: 0));
        }
        public static string PreviewTopic(int topicTemplateID, int portalId, int moduleId, int tabId, Forum forumInfo, int userId, string body, string imagePath, User up, DateTime postDate, CurrentUserTypes currentUserType, int currentUserId, TimeSpan timeZoneOffset)
        {
            var sTemplate = GetTopicTemplate(topicTemplateID, moduleId);
            try
            {
                var mainSettings = SettingsBase.GetModuleSettings(moduleId);
                var sTopic = GetTemplateSection(sTemplate, "[TOPIC]", "[/TOPIC]");
                sTopic = sTopic.Replace("[ACTIONS:ALERT]", string.Empty);
                sTopic = sTopic.Replace("[ACTIONS:EDIT]", string.Empty);
                sTopic = sTopic.Replace("[ACTIONS:DELETE]", string.Empty);
                sTopic = sTopic.Replace("[ACTIONS:QUOTE]", string.Empty);
                sTopic = sTopic.Replace("[ACTIONS:REPLY]", string.Empty);
                sTopic = sTopic.Replace("[POSTDATE]", Utilities.GetUserFormattedDateTime(postDate, portalId, userId));
                sTopic = sTopic.Replace("[POSTINFO]", GetPostInfo(portalId, moduleId, userId, up.UserName, up, imagePath, false, HttpContext.Current.Request.UserHostAddress, true, currentUserType, currentUserId, false, timeZoneOffset));
                sTemplate = ParsePreview(portalId, sTopic, body, moduleId);
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
            //TODO: Legacy Attachments Functionality - Probably can remove.
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

            //TODO: Legacy Attachments Functionality - Probably can remove.
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
                        message = message.Replace(m.Value, m.Value.Replace("<br>", System.Environment.NewLine));
                }
                var objCode = new CodeParser();
                template = CodeParser.ParseCode(Utilities.HTMLDecode(template));
            }
            return template;
        }
    }
}