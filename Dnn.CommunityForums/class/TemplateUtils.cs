// Copyright (c) by DNN Community
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

using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.ActiveForums.Enums;

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;

    public class TemplateUtils
    {
        public static List<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo> lstSubscriptionInfo { get; set; }

        internal static string ParseEmailTemplate(string template, int portalID, int moduleID, int tabID, int forumID, int topicId, int replyId, DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo accessingUser, bool topicSubscriber, INavigationManager navigationManager, Uri requestUrl, string rawUrl)
        {
            if (navigationManager == null)
            {
                navigationManager = (INavigationManager)new Services.URLNavigator();
            }

            PortalSettings portalSettings = new DotNetNuke.Modules.ActiveForums.Helpers.PortalSettingsHelper().GetPortalSettings(portalID);
            var moduleSettings = SettingsBase.GetModuleSettings(moduleID);
            if (author == null)
            {
                author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(portalID, moduleID, accessingUser.UserId);
            }

            var templateStringbuilder = new StringBuilder(template);
            templateStringbuilder = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemoveObsoleteEmailNotificationTokens(templateStringbuilder);
            templateStringbuilder = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyEmailNotificationTokenSynonyms(templateStringbuilder, portalSettings, accessingUser?.UserInfo?.Profile?.PreferredLocale);

            // Load Subject and body from topic or reply
            var postInfo = (topicId > 0 && replyId > 0) ? (IPostInfo)DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Instance.GetById(moduleID, replyId) : DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance.GetById(moduleID, topicId);
            postInfo.Forum.TabId = tabID;
            postInfo.Forum.ForumGroup.TabId = tabID;
            string subject = postInfo.Content.Subject;
            templateStringbuilder.Replace("[POSTEDORREPLIEDTO]", (replyId <= 0 ? Utilities.GetSharedResource("[RESX:posted]") : Utilities.GetSharedResource("[RESX:repliedto]")));
            templateStringbuilder.Replace("[POSTEDTO]", (replyId <= 0 ? Utilities.GetSharedResource("[RESX:postedto]") : string.Empty));
            templateStringbuilder.Replace("[REPLIEDTO]", (replyId > 0 ? Utilities.GetSharedResource("[RESX:repliedto]") : string.Empty));
            templateStringbuilder.Replace("[NEWPOST]", (replyId <= 0 ? Utilities.GetSharedResource("[RESX:NewPost]") : string.Empty));
            templateStringbuilder.Replace("[NEWREPLY]", (replyId > 0 ? Utilities.GetSharedResource("[RESX:NewReply]") : string.Empty));
            templateStringbuilder.Replace("[SUBSCRIBEDTOPIC]", (topicSubscriber ? Utilities.GetSharedResource("[RESX:SubscribedTopic]") : string.Empty));
            templateStringbuilder.Replace("[SUBSCRIBEDTOPICSUBJECT]", (topicSubscriber ? string.Format(Utilities.GetSharedResource("[RESX:SubscribedTopicSubject]"), subject) : string.Empty));
            templateStringbuilder.Replace("[SUBSCRIBEDTOPICFORUMNAME]", (topicSubscriber ? string.Format(Utilities.GetSharedResource("[RESX:SubscribedTopicForumName]"), arg0: subject, postInfo.Forum.ForumName) : string.Empty));
            templateStringbuilder.Replace("[SUBSCRIBEDFORUM]", (topicSubscriber ? string.Empty : "[RESX:SubscribedForum]"));
            templateStringbuilder.Replace("[SUBSCRIBEDFORUMNAME]", (topicSubscriber ? string.Empty : string.Format(Utilities.GetSharedResource("[RESX:SubscribedForumName]"), postInfo.Forum.ForumName)));
            templateStringbuilder.Replace("[SUBSCRIBEDFORUMORTOPICSUBJECTFORUMNAME]", (topicSubscriber ? string.Format(Utilities.GetSharedResource("[RESX:SubscribedTopicForumName]"), subject, postInfo.Forum.ForumName) : string.Format(Utilities.GetSharedResource("[RESX:SubscribedForumTopicForumName]"), subject, postInfo.Forum.ForumName)));

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
                {
                }
            }

            templateStringbuilder = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(templateStringbuilder, postInfo, portalSettings, moduleSettings, navigationManager, accessingUser, requestUrl, rawUrl);

            return templateStringbuilder.ToString();
        }

        private static long GetEmailInfo(int portalId, int moduleId, int forumID, int topicID, string ipAddress)
        {
            long itemID = -1;

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

            // TODO: this stored procedure no longer exists and needs to be addressed as part of the Email Connector plug-in work #1448, but is left here for reference and updated to remove direct dependency on Microsoft.ApplicationBlocks.Data.
            IDataReader dataReader = DotNetNuke.Data.SqlDataProvider.Instance().ExecuteReader("ActiveForumsEmailConnector_GetEmailInfo", portalId, moduleId, forumID, topicID, ipAddress, userIds.ToString());
            if (dataReader.Read())
            {
                itemID = Convert.ToInt32(dataReader["RecordID"]);
            }

            return itemID;
        }

        internal static string GetPostInfo(int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, bool isMod, string ipAddress, bool isUserOnline, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, TimeSpan timeZoneOffset)
        {
            return ParseProfileInfo(moduleId, user, imagePath, isMod, ipAddress, currentUserType, currentUserId, userPrefHideAvatar, timeZoneOffset);
        }

        private static string ParseProfileInfo(int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string imagePath, bool isMod, string ipAddress, CurrentUserTypes currentUserType, int currentUserId, bool userPrefHideAvatar, TimeSpan timeZoneOffset)
        {

            var cacheKey = string.Format(CacheKeys.ProfileInfo, moduleId);
            var myTemplate = Convert.ToString(DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Retrieve(moduleId, cacheKey));
            if (string.IsNullOrEmpty(myTemplate))
            {
                myTemplate = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(moduleId, Enums.TemplateType.ProfileInfo, SettingsBase.GetModuleSettings(moduleId).DefaultFeatureSettings.TemplateFileNameSuffix, user);
                if (cacheKey != string.Empty)
                {
                    DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Store(moduleId, cacheKey, myTemplate);
                }
            }

            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(user.PortalId, moduleId, user.UserId);

            myTemplate = ParseProfileTemplate(moduleId, myTemplate, author, imagePath, currentUserType, userPrefHideAvatar, false, ipAddress, currentUserId, timeZoneOffset);
            return myTemplate;
        }

        internal static string ParseProfileTemplate(int moduleId, string profileTemplate, DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author, string imagePath, CurrentUserTypes currentUserType, bool userPrefHideAvatar, bool userPrefHideSignature, string ipAddress, int currentUserId, TimeSpan timeZoneOffset)
        {
            try
            {
                var portalSettings = new DotNetNuke.Modules.ActiveForums.Helpers.PortalSettingsHelper().GetPortalSettings(author.ForumUser.PortalId);
                var mainSettings = SettingsBase.GetModuleSettings(moduleId);
                var accessingUser = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance.GetByUserId(author.ForumUser.PortalId, moduleId, currentUserId);
                var templateStringbuilder = new StringBuilder(profileTemplate);
                templateStringbuilder = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyAuthorTokenSynonyms(templateStringbuilder, portalSettings, mainSettings, accessingUser?.UserInfo?.Profile?.PreferredLocale);

                // Parse DNN profile fields if needed
                var pt = templateStringbuilder.ToString();
                if (pt.IndexOf("[DNN:PROFILE:", StringComparison.Ordinal) >= 0)
                {
                    pt = ParseProfile(author.ForumUser, pt, currentUserType, currentUserId);
                }

                // Parse Roles
                if (pt.Contains("[ROLES:"))
                {
                    pt = ParseRoles(pt, (author.ForumUser.UserId == -1) ? string.Empty : author.ForumUser.UserPermSet);
                }

                return pt;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string ParseRoles(string template, string userRoles)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return template;
            }

            var userRoleArray = string.IsNullOrWhiteSpace(userRoles) ? null : userRoles.Split(';').Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToList();

            const string pattern = @"\[ROLES:(.+?)\]";

            template = DotNetNuke.Common.Utilities.RegexUtils.GetCachedRegex(pattern).Replace(template, match =>
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

        internal static string ParseProfile(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string template, CurrentUserTypes currentUserType, int currentUserId)
        {
            var s = template ?? string.Empty;
            const string pattern = "(\\[DNN:PROFILE:(.+?)\\])";

            foreach (Match match in DotNetNuke.Common.Utilities.RegexUtils.GetCachedRegex(pattern).Matches(s))
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

                        if (profprop.ProfileVisibility.VisibilityMode == DotNetNuke.Entities.Users.UserVisibilityMode.AdminOnly && (currentUserType == CurrentUserTypes.Admin || currentUserType == CurrentUserTypes.SuperUser))
                        {
                            sReplace = profprop.PropertyValue;
                        }
                        else if (profprop.ProfileVisibility.VisibilityMode == DotNetNuke.Entities.Users.UserVisibilityMode.MembersOnly && currentUserType != CurrentUserTypes.Anon)
                        {
                            sReplace = profprop.PropertyValue;
                        }
                        else if (profprop.ProfileVisibility.VisibilityMode == UserVisibilityMode.AllUsers)
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

        internal static string PreviewTopic(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo user, string body, string imagePath, DateTime postDate, CurrentUserTypes currentUserType, int currentUserId, TimeSpan timeZoneOffset, Uri requestUri, string rawUrl)
        {
            var sTemplate = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(forumInfo.ModuleId, Enums.TemplateType.TopicView, forumInfo.FeatureSettings.TemplateFileNameSuffix, user);
            try
            {
                var sbTopicTemplate = new StringBuilder(GetTemplateSection(sTemplate, "[TOPIC]", "[/TOPIC]"));
                sbTopicTemplate = ReplaceSubSection(sbTopicTemplate, string.Empty, "[AF:CONTROL:TAGS]", "[/AF:CONTROL:TAGS]");

#region "Backward compatibility -- remove in v10.00.00"
                sbTopicTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyUserTokenSynonyms(sbTopicTemplate, forumInfo.PortalSettings, forumInfo.MainSettings, user.UserInfo?.Profile?.PreferredLocale);
                sbTopicTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyAuthorTokenSynonyms(sbTopicTemplate, forumInfo.PortalSettings, forumInfo.MainSettings, user.UserInfo?.Profile?.PreferredLocale);
                sbTopicTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyTopicTokenSynonyms(sbTopicTemplate, forumInfo.PortalSettings, user.UserInfo?.Profile?.PreferredLocale);
                sbTopicTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyPostTokenSynonyms(sbTopicTemplate, forumInfo.PortalSettings, user.UserInfo?.Profile?.PreferredLocale);
#endregion "Backward compatibility -- remove in v10.00.00"

                sbTopicTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemoveControlTokensForDnnPrintMode(sbTopicTemplate);

                var topic = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo
                {
                    Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                    {
                        Body = body,
                        DateCreated = postDate,
                        DateUpdated = postDate,
                    },
                    Author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(user),
                    Forum = forumInfo,
                };
                sbTopicTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(sbTopicTemplate, topic, forumInfo.PortalSettings, forumInfo.MainSettings, new Services.URLNavigator().NavigationManager(), user, requestUri, rawUrl);
                sTemplate = ParsePreview(forumInfo.PortalId, sbTopicTemplate.ToString(), body, forumInfo.ModuleId);
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
                foreach (Match match in DotNetNuke.Common.Utilities.RegexUtils.GetCachedRegex(pattern).Matches(message))
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
                foreach (Match match in DotNetNuke.Common.Utilities.RegexUtils.GetCachedRegex(pattern).Matches(message))
                {
                    var thumbId = match.Groups[2].Value.Split(':')[0];
                    var parentId = match.Groups[2].Value.Split(':')[1];
                    var sImage = string.Format("<a href=\"{0}DesktopModules/ActiveForums/viewer.aspx?portalid={1}&moduleid={2}&attachid={3}\" target=\"_blank\"><img src=\"{4}DesktopModules/ActiveForums/viewer.aspx?portalid={5}&moduleid={6}&attachid={7}\" border=\"0\" /></a>", strHost, portalId, moduleId, parentId, strHost, portalId, moduleId, thumbId);
                    message = message.Replace(match.Value, sImage);
                }
            }

            template = template.Replace("[BODY]", message);
            if (DotNetNuke.Common.Utilities.RegexUtils.GetCachedRegex("<CODE([^>]*)>", RegexOptions.IgnoreCase).IsMatch(template))
            {
                template = CodeParser.ReplaceBreakTagsWithNewLines(template);
                template = CodeParser.ParseCode(System.Net.WebUtility.HtmlDecode(template));
            }

            return template;
        }
    }
}
