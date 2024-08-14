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

using System.Linq;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Log.EventLog;

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
namespace DotNetNuke.Modules.ActiveForums
{
    using System;

    [Obsolete("Deprecated in Community Forums. Remove in 10.00.00. Not Used. Use DotNetNuke.Modules.ActiveForums.Controllers.TokenController()")]

    public class TokensController { TokensController() { throw new NotImplementedException(); } }
}

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;
    using System.Data.SqlTypes;
    using System.Text.RegularExpressions;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Common.Controls;
    using DotNetNuke.Modules.ActiveForums.API;

    internal static partial class TokenController
    {

        internal static List<Token> TokensList(int moduleId, string group)
        {
            try
            {
                List<Token> li =
                    (List<Token>)DataCache.SettingsCacheRetrieve(moduleId,
                        string.Format(CacheKeys.Tokens, moduleId, group));
                if (li == null)
                {
                    li = new List<Token>();
                    Token tk = null;
                    System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
                    string sPath =
                        DotNetNuke.Modules.ActiveForums.Utilities.MapPath(Globals.ModulePath + "config/tokens.config");
                    xDoc.Load(sPath);
                    if (xDoc != null)
                    {
                        System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                        string sQuery = "//tokens/token";
                        if (!(group == string.Empty))
                        {
                            sQuery = string.Concat(sQuery, "[@group='", group, "' or @group='*']");
                        }

                        System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes(sQuery);
                        if (xNodeList.Count > 0)
                        {
                            int i = 0;
                            for (i = 0; i < xNodeList.Count; i++)
                            {
                                tk = new Token();
                                tk.Group = xNodeList[i].Attributes["group"].Value;
                                tk.TokenTag = xNodeList[i].Attributes["name"].Value;
                                if (xNodeList[i].Attributes["value"] != null)
                                {
                                    tk.TokenReplace = HttpUtility.HtmlDecode(xNodeList[i].Attributes["value"].Value);
                                }
                                else
                                {
                                    tk.TokenReplace = HttpUtility.HtmlDecode(xNodeList[i].ChildNodes[0].InnerText);
                                }

                                li.Add(tk);
                            }
                        }
                    }

                    DataCache.SettingsCacheStore(moduleId, string.Format(CacheKeys.Tokens, moduleId, group), li);
                }

                return li;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return null;
            }
        }

        internal static string TokenGet(int moduleId, string group, string tokenName)
        {
            string sOut = string.Empty;
            List<Token> tl = TokensList(moduleId, group);
            foreach (Token t in tl)
            {
                if (t.TokenTag == tokenName)
                {
                    sOut = t.TokenReplace;
                    break;
                }
            }

            return sOut;
        }


        internal static StringBuilder ReplaceModuleTokens(StringBuilder template, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, ForumUserInfo forumUser, int tabId, int forumModuleId)
        {
            template = RemoveObsoleteTokens(template);
            template = MapLegacyPortalTokenSynonyms(template);
            template = MapLegacyModuleTokenSynonyms(template);

            string language = forumUser?.UserInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;
            var urlNavigator = new Services.URLNavigator();

            template.Replace("[PORTAL:PORTALID]", portalSettings?.PortalId.ToString());
            template.Replace("[PORTALNAME]", portalSettings?.PortalName);
            template.Replace("[MODULEID]", forumModuleId.ToString());
            template.Replace("[TABID]", tabId.ToString());
            template.Replace("[PAGENAME]", HttpUtility.HtmlEncode(string.IsNullOrEmpty(portalSettings.ActiveTab.Title) ? portalSettings.ActiveTab.TabName : portalSettings.ActiveTab.Title));
            if (template.ToString().Contains("[FORUMMAINLINK"))
            {
                template.Replace("[FORUMMAINLINK]", string.Format(LocalizeTokenString("[FORUMMAINLINK]", portalSettings, language), urlNavigator.NavigateURL(tabId)));
            }

            return template;
        }

        internal static StringBuilder ReplaceForumTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, ForumUserInfo forumUser, HttpRequest request, int tabId, CurrentUserTypes currentUserType)
        {
            string language = forumUser?.UserInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = new Services.URLNavigator().NavigationManager();
            }

            template = RemoveObsoleteTokens(template);
            template = MapLegacyForumTokenSynonyms(template);

            if (template.ToString().Contains("[GROUPCOLLAPSE]"))
            {
                template.Replace("[GROUPCOLLAPSE]",
                    DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleOpened(
                        target: $"group{forum.ForumGroupId}",
                        title: Utilities.GetSharedResource("[RESX:ToggleGroup]")));
            }

            /* if no last post or user can't view via security, or subject missing, remove associated last topic tokens */
            if (forum.LastPostID == 0 || 
                !DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum.Security.View, forumUser.UserRoles) || 
                string.IsNullOrEmpty(HttpUtility.HtmlDecode(forum.LastPostSubject)))
            {
                template = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.RemovePrefixedToken(template, "[LASTPOSTSUBJECT");
                template.Replace("[RESX:BY]", string.Empty);
                template.Replace("[DISPLAYNAME]", string.Empty);
                template.Replace("[LASTPOSTDATE]", string.Empty);
                template.Replace("[AF:CONTROL:ADDFAVORITE]", string.Empty);
            }

            template.Replace("[FORUM:FORUMID]", forum.ForumID.ToString());
            template.Replace("[FORUM:FORUMGROUPID]", forum.ForumGroupId.ToString());
            template.Replace("[FORUM:FORUMNAMENOLINK]", forum.ForumName);
            template.Replace("[FORUM:GROUPNAME]", forum.GroupName);
            template.Replace("[FORUM:TOTALTOPICS]", forum.TotalTopics.ToString());
            template.Replace("[FORUM:TOTALREPLIES]", forum.TotalReplies.ToString());
            template.Replace("[FORUM:PARENTFORUMID]", forum.ParentForumId.ToString());
            if (template.ToString().Contains("[FORUM:SUBSCRIBERCOUNT]"))
            {
                template.Replace("[FORUM:SUBSCRIBERCOUNT]", forum.SubscriberCount.ToString());
            }

            /* no RSS */
            if (!forum.AllowRSS || !DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum.Security.Read, forumUser.UserRoles))
            {
                template.Replace("[RSSLINK]", string.Empty);
            }
            else if (template.ToString().Contains("[RSSLINK]"))
            {
                string url = DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName(request)) + $"/DesktopModules/ActiveForums/feeds.aspx?portalid={forum.PortalId}&forumid={forum.ForumID}&tabid={tabId}&moduleid={forum.ModuleId}" + (forum.SocialGroupId > 0 ? $"&GroupId={forum.SocialGroupId}" : string.Empty);
                template = template.Replace("[RSSLINK]", "<a href=\"" + url + "\" target=\"_blank\"><img src=\"" + Utilities.ResolveUrl(mainSettings.ThemeLocation + "images/rss.png") + "\" border=\"0\" alt=\"[RESX:RSS]\" /></a>");
            }

            if (template.ToString().Contains("[MODLINK]"))
            {
                string modLink = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildModeratorUrl(navigationManager, portalSettings, mainSettings, forum);
                template.Replace("[MODLINK]", string.Format(LocalizeTokenString("[MODLINK]", portalSettings, language), modLink, modLink));
            }

            if (template.ToString().Contains("[FORUMGROUPLINK]"))
            {
                string groupUrl = new ControlUtils().BuildUrl(tabId, forum.ModuleId, forum.ForumGroup.PrefixURL, string.Empty, forum.ForumGroupId, -1, -1, -1, string.Empty, 1, -1, forum.SocialGroupId);
                template.Replace("[FORUMGROUPLINK]", string.Format(LocalizeTokenString("[FORUMGROUPLINK]", portalSettings, language), groupUrl, forum.GroupName));
            }

            if (template.ToString().Contains("[FORUMNAME]") || template.ToString().Contains("[FORUMLINK]") || template.ToString().Contains("[AF:CONTROL:ADDFAVORITE]"))
            {
                /* note: this purposely uses same token replacement [FORUMLINK] for both [FORUMNAME] and [FORUMLINK] */
                string forumUrl = new ControlUtils().BuildUrl(tabId, forum.ModuleId, forum.ForumGroup.PrefixURL, forum.PrefixURL, forum.ForumGroupId, forum.ForumID, -1, -1, string.Empty, 1, -1, forum.SocialGroupId);
                template.Replace("[FORUMNAME]", string.Format(LocalizeTokenString("[FORUMLINK]", portalSettings, language), forumUrl, forum.ForumName));
                template.Replace("[FORUMLINK]", string.Format(LocalizeTokenString("[FORUMLINK]", portalSettings, language), forumUrl, forum.ForumName));
                template.Replace("[AF:CONTROL:ADDFAVORITE]", "<a href=\"javascript:afAddBookmark('" + forum.ForumName + "','" + forumUrl + "');\"><img src=\"" + mainSettings.ThemeLocation + "images/favorites16_add.png\" border=\"0\" alt=\"[RESX:AddToFavorites]\" /></a>");
            }

            if (template.ToString().Contains("[FORUMURL]"))
            {
                template.Replace("[FORUMURL]",
                mainSettings.UseShortUrls
                ? navigationManager?.NavigateURL(tabId.ToString(), new[] { string.Concat(ParamKeys.ForumId, "=", forum.ForumID) })
                : navigationManager?.NavigateURL(tabId.ToString(), new[] { string.Concat(ParamKeys.ForumId, "=", forum.ForumID), string.Concat(ParamKeys.ViewType, "=", Views.Topics) }));
            }

            template.Replace("[PARENTFORUMNAME]", string.IsNullOrEmpty(forum.ParentForumName) ? string.Empty : forum.ParentForumName);

            if (template.ToString().Contains("[PARENTFORUMLINK]"))
            {
                string parentForumLink = LocalizeTokenString("[PARENTFORUMLINK]", portalSettings, language);
                if (forum.ParentForumId > 0)
                {
                    template.Replace(oldValue: "[PARENTFORUMLINK]", string.Format(parentForumLink,
                                                                                  mainSettings.UseShortUrls ?
                                                                                  navigationManager?.NavigateURL(tabId, string.Empty, new[] { ParamKeys.ForumId + "=" + forum.ParentForumId }) :
                                                                                  navigationManager?.NavigateURL(tabId, string.Empty, new[] { ParamKeys.ViewType + "=" + Views.Topics, ParamKeys.ForumId + "=" + forum.ParentForumId }),
                                                                                  forum.ParentForumName));
                }
                else if (forum.ForumGroupId > 0)
                {
                    template.Replace(oldValue: "[PARENTFORUMLINK]", string.Format(parentForumLink, Utilities.NavigateURL(tabId), forum.GroupName));
                }
            }
            else
            {
                template.Replace("[PARENTFORUMLINK]", string.Empty);
            }

            if (template.ToString().Contains("[FORUMDESCRIPTION]"))
            {
                template.Replace("[FORUMDESCRIPTION]", !string.IsNullOrEmpty(forum.ForumDesc) ? string.Format(LocalizeTokenString("[FORUMDESCRIPTION]", portalSettings, language), forum.ForumDesc) : string.Empty);
            }

            if (template.ToString().Contains("[FORUMICON]"))
            {
                template.Replace("[FORUMICON]", string.Format(LocalizeTokenString("[FORUMICON]", portalSettings, language), forum.ForumName, mainSettings.ThemeLocation, DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumFolderIcon(forum, forumUser)));
            }

            if (template.ToString().Contains("[FORUM:STATUSCSSCLASS]"))
            {
                template.Replace("FORUM:STATUSCSSCLASS]", DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumStatusCss(forum, forumUser));
            }

            if (template.ToString().Contains("[FORUMICONCSS]") || template.ToString().Contains("[FORUMICONSM]"))
            {
                string sFolderCSS = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumFolderIconCss(forum, forumUser);
                template.Replace("[FORUMICONCSS]", string.Format(LocalizeTokenString("[FORUMICONCSS]", portalSettings, language), sFolderCSS));
                template.Replace("[FORUMICONSM]", string.Format(LocalizeTokenString("[FORUMICONSM]", portalSettings, language), sFolderCSS));
            }

            if (template.ToString().Contains("[LASTPOSTSUBJECT"))
            { 
                if (forum.LastPostID < 1)
                {
                    template = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.RemovePrefixedToken(template, "[LASTPOSTSUBJECT");
                }
                else
                {
                    int intLength = 0;
                    if ((template.ToString().IndexOf("[LASTPOSTSUBJECT:", 0) + 1) > 0)
                    {
                        int inStart = (template.ToString().IndexOf("[LASTPOSTSUBJECT:", 0) + 1) + 17;
                        int inEnd = (template.ToString().IndexOf("]", inStart - 1) + 1);
                        string sLength = template.ToString().Substring(inStart - 1, inEnd - inStart);
                        intLength = Convert.ToInt32(sLength);
                    }

                    string lastPostSubjectToken = "[LASTPOSTSUBJECT:" + intLength.ToString() + "]";
                    template.Replace(lastPostSubjectToken, DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetLastPostSubjectLinkTag(forum.LastPostID, forum.LastTopicId, HttpUtility.HtmlDecode(forum.LastPostSubject), intLength, forum));
                }
            }

            if (template.ToString().Contains("[FORUM:LASTPOSTDISPLAYNAME]"))
            {
                if (forum.LastPostID < 1)
                {
                    template.Replace("[FORUM:LASTPOSTDISPLAYNAME]", string.Empty);
                }
                else if (forum.LastPostUserID <= 0)
                {
                    template.Replace("[FORUM:LASTPOSTDISPLAYNAME]", string.Format(LocalizeTokenString("[FORUM:LASTPOSTDISPLAYNAME]", portalSettings, language), forum.LastPostDisplayName));
                }
                else
                {
                    bool isMod = currentUserType == CurrentUserTypes.Admin || currentUserType == CurrentUserTypes.ForumMod || currentUserType == CurrentUserTypes.SuperUser;
                    bool isAdmin = currentUserType == CurrentUserTypes.Admin || currentUserType == CurrentUserTypes.SuperUser;
                    template.Replace("[FORUM:LASTPOSTDISPLAYNAME]", string.Format(LocalizeTokenString("[FORUM:LASTPOSTDISPLAYNAME]", portalSettings, language), DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, forum.ModuleId, true, isMod, isAdmin, forum.LastPostUserID, forum.LastPostUserName, forum.LastPostFirstName, forum.LastPostLastName, forum.LastPostDisplayName)));
                }
            }

            if (forum.LastPostID < 1)
            {
                template = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.RemovePrefixedToken(template,"[LASTPOSTDATE");
            }
            else
            {
                template = ExtractAndReplaceDateTokenWithOptionalFormatParameter(template, "[LASTPOSTDATE", forum.LastPostDateTime, portalSettings, forumUser);
            }

            return template;
        }
        
        internal static StringBuilder ReplaceUserTokens(StringBuilder template, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, ForumUserInfo accessingUser, int forumModuleId)
        {
            string dateFormat = Globals.DefaultDateFormat;

            template = RemoveObsoleteTokens(template);
            template = MapLegacyUserTokenSynonyms(template);

            template.Replace("[FORUMUSER:DISPLAYNAME]", DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, forumModuleId, true, forumUser.GetIsMod(forumModuleId), forumUser.IsAdmin, forumUser.UserId, forumUser.Username, forumUser.FirstName, forumUser.LastName, forumUser.DisplayName));

            template.Replace("[FORUMUSER:USERID]", forumUser?.UserId.ToString());
            template.Replace("[FORUMUSER:USERNAME]", HttpUtility.HtmlEncode(forumUser?.Username).Replace("&amp;#", "&#"));
            template.Replace("[FORUMUSER:FIRSTNAME]", HttpUtility.HtmlEncode(forumUser?.FirstName).Replace("&amp;#", "&#"));
            template.Replace("[FORUMUSER:LASTNAME]", HttpUtility.HtmlEncode(forumUser?.LastName).Replace("&amp;#", "&#"));
            template.Replace("[FORUMUSER:FULLNAME]", string.Concat(forumUser?.FirstName, " ", forumUser?.LastName));
            template.Replace("[FORUMUSER:USERCAPTION]", forumUser.UserCaption);
            template.Replace("[FORUMUSER:EMAIL]", forumUser.Email);

            // Post Count
            template.Replace("[FORUMUSER:POSTCOUNT]", (forumUser.PostCount == 0) ? string.Empty : forumUser.PostCount.ToString());
            template.Replace("[FORUMUSER:DATELASTPOST]", (forumUser.DateLastPost == DateTime.MinValue) ? string.Empty : Utilities.GetUserFormattedDateTime(forumUser.DateLastPost, forumUser.PortalId, accessingUser.UserId));
            template.Replace("[FORUMUSER:TOPICCOUNT]", forumUser.TopicCount.ToString());
            template.Replace("[FORUMUSER:REPLYCOUNT]", forumUser.ReplyCount.ToString());
            template.Replace("[FORUMUSER:VIEWCOUNT]", forumUser?.ViewCount.ToString());

            // Points
            if (mainSettings.EnablePoints && forumUser?.UserId > 0)
            {
                var totalPoints = (forumUser?.TopicCount * mainSettings.TopicPointValue) + (forumUser?.ReplyCount * mainSettings.ReplyPointValue) + (forumUser?.AnswerCount * mainSettings.AnswerPointValue) + forumUser?.RewardPoints;
                template.Replace("[FORUMUSER:TOTALPOINTS]", totalPoints.ToString());
                template.Replace("[FORUMUSER:ANSWERCOUNT]", forumUser?.AnswerCount.ToString());
                template.Replace("[FORUMUSER:REWARDPOINTS]", forumUser?.RewardPoints.ToString());
            }
            else
            {
                template.Replace("[FORUMUSER:TOTALPOINTS]", string.Empty);
                template.Replace("[FORUMUSER:ANSWERCOUNT]", string.Empty);
                template.Replace("[FORUMUSER:REWARDPOINTS]", string.Empty);
            }

            if (template.ToString().Contains("[FORUMUSER:SIGNATURE]"))
            {
                // Signature
                var sSignature = string.Empty;
                if (mainSettings.AllowSignatures != 0 && !forumUser.PrefBlockSignatures && !forumUser.SignatureDisabled)
                {
                    sSignature = forumUser?.Signature;

                    if (sSignature != string.Empty)
                    {
                        sSignature = Utilities.ManageImagePath(sSignature);
                    }

                    switch (mainSettings.AllowSignatures)
                    {
                        case 1:
                            sSignature = HttpUtility.HtmlEncode(sSignature);
                            sSignature = sSignature.Replace(System.Environment.NewLine, "<br />");
                            break;
                        case 2:
                            sSignature = HttpUtility.HtmlDecode(sSignature);
                            break;
                    }
                }

                template.Replace("[FORUMUSER:SIGNATURE]", sSignature);
            }

            // Rank
            template.Replace("[FORUMUSER:RANKDISPLAY]", (forumUser.UserId > 0) ? DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetUserRank(forumModuleId,forumUser, 0) : string.Empty);
            template.Replace("[FORUMUSER:RANKNAME]", (forumUser.UserId > 0) ? DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetUserRank(forumModuleId, forumUser, 1) : string.Empty);

            // Avatar
            if (template.ToString().Contains("[FORUMUSER:AVATAR]"))
            {
                var sAvatar = string.Empty;
                if (!forumUser.PrefBlockAvatars && !forumUser.AvatarDisabled)
                {
                    sAvatar = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetAvatar(
                        forumUser.UserId,
                        mainSettings.AvatarWidth,
                        mainSettings.AvatarHeight);

                }

                template.Replace("[FORUMUSER:AVATAR]", sAvatar);
            }

            // User Status
            var sUserStatus = string.Empty;
            if (mainSettings.UsersOnlineEnabled && forumUser.UserId > 0)
            {
                sUserStatus = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.UserStatus(mainSettings.ThemeLocation, forumUser.IsUserOnline, forumUser.UserId, forumModuleId, "[RESX:UserOnline]", "[RESX:UserOffline]");
            }

            template.Replace("[FORUMUSER:USERSTATUS]", sUserStatus);
            template.Replace("[FORUMUSER:USERSTATUS:CSS]", sUserStatus.Contains("online") ? "af-status-online" : "af-status-offline");
            
            template = ExtractAndReplaceDateTokenWithOptionalFormatParameter(template, "[FORUMUSER:DATELASTACTIVITY", forumUser.DateLastActivity, portalSettings, forumUser);
            template = ExtractAndReplaceDateTokenWithOptionalFormatParameter(template, "[FORUMUSER:DATECREATED", forumUser.DateCreated, portalSettings, forumUser);

            return template;
        }

        internal static StringBuilder ReplaceTopicTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, HttpRequest request, int tabId)
        {
            string language = forumUser?.UserInfo?.Profile.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = (INavigationManager)new Services.URLNavigator();
            }

            var @params = new List<string>();

            template = RemoveObsoleteTokens(template);
            template = MapLegacyTopicTokenSynonyms(template);

            /* if user can't read via security, remove topic tokens */
            if (!DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(topic.Forum.Security.Read, forumUser.UserRoles))
            {
                template = RemovePrefixedToken(template, "[LASTPOSTSUBJECT");
                template.Replace("[LASTPOSTDATE]", string.Empty);
                template.Replace("[BODYTITLE]", string.Empty);
                template.Replace("[BODY]", string.Empty);
                template.Replace("[AF:ICONLINK:LASTREPLY]", string.Empty);
                template.Replace("[AF:URL:LASTREPLY]", string.Empty);
            }

            /* if no replies, remove associated tokens */
            if (topic.LastReplyId < 1)
            {
                template.Replace("[AF:ICONLINK:LASTREPLY]", string.Empty);
                template.Replace("[AF:URL:LASTREPLY]", string.Empty);
                template.Replace("[AF:UI:MINIPAGER]", string.Empty);
            }

            // replace tokens common to topics & replies
            template = ReplacePostTokens(template, topic, portalSettings, mainSettings, navigationManager, forumUser, request, tabId);

            // replace tokens unique to topic itself 
            template.Replace("[REPLYCOUNT]", topic.ReplyCount.ToString());
            template.Replace("[REPLIES]", topic.ReplyCount.ToString());
            template.Replace("[VIEWCOUNT]", topic.ViewCount.ToString());

            string sBodyTitle = GetTopicTitle(topic.Content.Body);

            template.Replace("[BODYTITLE]", sBodyTitle);
            template = ReplaceBody(template, topic.Content, mainSettings, request.Url);

            int BodyLength = -1;
            string BodyTrim = string.Empty;

            if (template.ToString().Contains("[TOPICSUBJECT:"))
            {
                const string pattern = "(\\[TOPICSUBJECT:(.+?)\\])";
                foreach (Match m in Regex.Matches(template.ToString(), pattern))
                {
                    var maxLength = Utilities.SafeConvertInt(m.Groups[2].Value, 255);
                    if (topic.Content.Subject.Length > maxLength)
                    {
                        template.Replace(m.Value, topic.Subject.Substring(0, maxLength) + "...");
                    }
                    else
                    {
                        template.Replace(m.Value, Utilities.StripHTMLTag(topic.Subject));
                    }
                }
            }

            template.Replace("[TOPICSUBJECT]", Utilities.StripHTMLTag(topic.Subject));

            /* from topicsview */
            if (template.ToString().Contains("[BODY:"))
            {
                const string pattern = "(\\[BODY:(.+?)\\])";
                foreach (Match m in Regex.Matches(template.ToString(), pattern))
                {
                    var maxLength = Utilities.SafeConvertInt(m.Groups[2].Value, 512);
                    if (topic?.Content?.Body?.Length > maxLength)
                    {
                        template.Replace(m.Value, topic?.Content?.Body?.Substring(0, maxLength) + "...");
                    }
                    else
                    {
                        template.Replace(m.Value, topic?.Content?.Body);
                    }
                }
            }

            /* from topic view */
            if (template.ToString().Contains("[BODY:"))
            {
                int inStart = (template.ToString().IndexOf("[BODY:", 0) + 1) + 5;
                int inEnd = (template.ToString().IndexOf("]", inStart - 1) + 1) - 1;
                string sLength = template.ToString().Substring(inStart, inEnd - inStart);
                BodyLength = Convert.ToInt32(sLength);
                BodyTrim = "[BODY:" + BodyLength.ToString() + "]";
            }

            if (template.ToString().Contains("[SUMMARY]") && string.IsNullOrEmpty(topic.Content.Summary) &&
                string.IsNullOrEmpty(BodyTrim))
            {
                BodyTrim = "[BODY:250]";
                BodyLength = 250;
            }

            string BodyPlain = string.Empty;
            if (!string.IsNullOrEmpty(BodyTrim))
            {
                BodyPlain = topic?.Content?.Body?.Replace("<br>", System.Environment.NewLine).Replace("<br />", System.Environment.NewLine);
                BodyPlain = Utilities.StripHTMLTag(BodyPlain);
                if (BodyLength > 0 & BodyPlain.Length > BodyLength)
                {
                    BodyPlain = BodyPlain.Substring(0, BodyLength);
                }

                BodyPlain = BodyPlain.Replace(System.Environment.NewLine, "<br />");
                template.Replace(BodyTrim, BodyPlain);
            }

            if (string.IsNullOrEmpty(topic?.Content?.Summary))
            {
                topic.Content.Summary = BodyPlain;
                topic.Content.Summary = topic.Content.Summary.Replace("<br />", "  ");
            }

            template.Replace("[SUMMARY]", topic.Content.Summary);

            if (template.ToString().Contains("[TOPICSUBSCRIBERCOUNT]"))
            {
                template.Replace("[TOPICSUBSCRIBERCOUNT]", topic.SubscriberCount.ToString());
            }

            if (topic.IsLocked)
            {
                template.Replace("fa-lock", "fa-unlock");
                template.Replace("[RESX:Lock]", "[RESX:UnLock]");
                template.Replace("[RESX:LockTopic]", "[RESX:UnLockTopic]");
                template.Replace("[RESX:Confirm:Lock]", "[RESX:Confirm:UnLock]");
            }

            if (template.ToString().Contains("[ICONLOCK]"))
            {
                template.Replace("[ICONLOCK]", string.Format(LocalizeTokenString(topic.IsLocked ? "[ICONLOCK]-ShowIcon" : "[ICONLOCK]-HideIcon", portalSettings, language), topic.TopicId.ToString()));
            }

            if (topic.IsPinned)
            {
                template.Replace("dcf-topic-pin-pin", "dcf-topic-pin-unpin");
                template.Replace("[RESX:Pin]", "[RESX:UnPin]");
                template.Replace("[RESX:PinTopic]", "[RESX:UnPinTopic]");
                template.Replace("[RESX:Confirm:Pin]", "[RESX:Confirm:UnPin]");
            }

            if (template.ToString().Contains("[ICONPIN]"))
            {
                template.Replace("[ICONPIN]", string.Format(LocalizeTokenString( topic.IsPinned ? "[ICONPIN]-ShowIcon" : "[ICONPIN]-HideIcon", portalSettings, language), topic.TopicId.ToString()));
            }

            if (template.ToString().Contains("[TOPICURL]") ||
                template.ToString().Contains("[AF:ICONLINK:LASTREAD]") ||
                template.ToString().Contains("[AF:ICONLINK:LASTREPLY]") ||
                template.ToString().Contains("[AF:URL:LASTREAD]") ||
                template.ToString().Contains("[AF:URL:LASTREPLY]") ||
                template.ToString().Contains("[SUBJECT]") ||
                template.ToString().Contains("[SUBJECTLINK]") ||
                template.ToString().Contains("[LASTPOST") ||
                template.ToString().Contains("[AF:LABEL:Last"))
            {
                @params = new List<string>
                {
                    $"{ParamKeys.TopicId}={topic.TopicId}", $"{ParamKeys.ContentJumpId}={topic.LastReplyId}",
                };

                if (topic.Forum.SocialGroupId > 0)
                {
                    @params.Add($"{Literals.GroupId}={topic.Forum.SocialGroupId}");
                }

                string sLastReplyURL = navigationManager.NavigateURL(tabId, string.Empty, @params.ToArray());
                string sTopicURL = new ControlUtils().BuildUrl(tabId, topic.Forum.ModuleId, topic.Forum.ForumGroup.PrefixURL, topic.Forum.PrefixURL, topic.Forum.ForumGroupId, topic.Forum.ForumID, topic.TopicId, topic.TopicUrl, -1, -1, string.Empty, 1, -1, topic.Forum.SocialGroupId);
                if (!(string.IsNullOrEmpty(sTopicURL)))
                {
                    if (sTopicURL.EndsWith("/"))
                    {
                        sLastReplyURL = sTopicURL + (Utilities.UseFriendlyURLs(topic.Forum.ModuleId)
                            ? string.Concat("#", topic.TopicId)
                            : string.Concat("?", ParamKeys.ContentJumpId, "=", topic.LastReplyId));
                    }
                }

                template.Replace("[AF:ICONLINK:LASTREPLY]", string.Format(LocalizeTokenString("[ICONLINK-LASTREPLY]", portalSettings, language), sLastReplyURL, mainSettings.ThemeLocation));
                template.Replace("[AF:URL:LASTREPLY]", sLastReplyURL);

                string sLastReadURL = string.Empty;
                int? userLastReplyRead = forumUser?.GetLastReplyRead(topic);
                if (userLastReplyRead > 0)
                {
                    @params = new List<string>
                    {
                        $"{ParamKeys.ForumId}={topic.ForumId}",
                        $"{ParamKeys.TopicId}={topic.TopicId}",
                        $"{ParamKeys.ViewType}={Views.Topic}",
                        $"{ParamKeys.FirstNewPost}={userLastReplyRead}",
                    };
                    if (topic.Forum.SocialGroupId > 0)
                    {
                        @params.Add($"{Literals.GroupId}={topic.Forum.SocialGroupId}");
                    }

                    sLastReadURL = navigationManager.NavigateURL(tabId, string.Empty, @params.ToArray());

                    if (mainSettings.UseShortUrls)
                    {
                        @params = new List<string>
                        {
                            $"{ParamKeys.TopicId}={topic.TopicId}",
                            $"{ParamKeys.FirstNewPost}={userLastReplyRead}",
                        };
                        if (topic.Forum.SocialGroupId > 0)
                        {
                            @params.Add($"{Literals.GroupId}={topic.Forum.SocialGroupId}");
                        }

                        sLastReadURL = navigationManager.NavigateURL(tabId, string.Empty, @params.ToArray());
                    }

                    if (sTopicURL.EndsWith("/"))
                    {
                        sLastReadURL = sTopicURL + (Utilities.UseFriendlyURLs(topic.Forum.ModuleId)
                            ? string.Concat("#", userLastReplyRead)
                            : string.Concat("?", ParamKeys.FirstNewPost, "=", userLastReplyRead));
                    }
                }

                template.Replace("[AF:ICONLINK:LASTREAD]", string.Format(LocalizeTokenString("[ICONLINK-LASTREAD]", portalSettings, language), sLastReadURL, mainSettings.ThemeLocation));
                template.Replace("[AF:URL:LASTREAD]", sLastReadURL);

                if (forumUser.PrefJumpLastPost && sLastReadURL != string.Empty)
                {
                    sTopicURL = sLastReadURL;
                }

                template.Replace("[TOPICURL]", sTopicURL);

                if (template.ToString().Contains("[SUBJECT]") || template.ToString().Contains("[SUBJECTLINK]"))
                {
                    string sPollImage = (topic.TopicType == TopicTypes.Poll ? LocalizeTokenString("[POSTICON]", portalSettings, language) : string.Empty);
                    topic.Content.Subject = Utilities.StripHTMLTag(topic.Content.Subject);
                    template.Replace("[SUBJECT]", topic.Content.Subject.Replace("[", "&#91").Replace("]", "&#93") + sPollImage);
                    if (template.ToString().Contains("[SUBJECTLINK]"))
                    {
                        string slink = null;
                        string subject = HttpUtility.HtmlDecode(topic.Subject);
                        subject = Utilities.StripHTMLTag(subject);
                        subject = subject.Replace("\"", string.Empty).Replace("#", string.Empty).Replace("%", string.Empty).Replace("+", string.Empty);

                        if (sTopicURL == string.Empty)
                        {
                            @params = new List<string>
                            {
                                $"{ParamKeys.ForumId}={topic.ForumId}",
                                $"{ParamKeys.TopicId}={topic.TopicId}",
                                $"{ParamKeys.ViewType}={Views.Topic}",
                            };
                            if (mainSettings.UseShortUrls)
                            {
                                @params.Add($"${ParamKeys.TopicId}={topic.TopicId}");
                            }

                            slink = "<a title=\"" + sBodyTitle + "\" href=\"" + navigationManager.NavigateURL(tabId, string.Empty, @params.ToArray()) + "\">" + subject + "</a>";
                        }
                        else
                        {
                            slink = "<a title=\"" + sBodyTitle + "\" href=\"" + sTopicURL + "\">" + subject + "</a>";
                        }

                        template.Replace("[SUBJECTLINK]", slink + sPollImage);
                    }
                }

                if (template.ToString().Contains("[AF:LABEL:LastPostAuthor]"))
                {
                    if (topic.LastReply.Author.AuthorId > 0)
                    {
                        template.Replace("[AF:LABEL:LastPostAuthor]", DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, topic.Forum.ModuleId, true, forumUser.GetIsMod(topic.ModuleId), forumUser.IsAdmin || forumUser.IsSuperUser, topic.LastReply.Author.AuthorId, topic.LastReply.Author.Username, topic.LastReply.Author.FirstName, topic.LastReply.Author.LastName, topic.LastReply.Author.DisplayName).ToString().Replace("&amp;#", "&#"));
                    }
                    else
                    {
                        template.Replace("[AF:LABEL:LastPostAuthor]", topic.LastReply.Content.AuthorName);
                    }
                }

                if (template.ToString().Contains("[LASTPOST]"))
                {
                    if (topic.LastReply?.ReplyId == 0)
                    {
                        template = TemplateUtils.ReplaceSubSection(template, string.Empty, "[LASTPOST]", "[/LASTPOST]");
                    }
                    else
                    {
                        string sLastReply = TemplateUtils.GetTemplateSection(template.ToString(), "[LASTPOST]", "[/LASTPOST]");
                        int iLength = 0;
                        if (sLastReply.Contains("[LASTPOSTSUBJECT:"))
                        {
                            int inStart = (sLastReply.IndexOf("[LASTPOSTSUBJECT:", 0) + 1) + 17;
                            int inEnd = (sLastReply.IndexOf("]", inStart - 1) + 1);
                            string sLength = sLastReply.Substring(inStart - 1, inEnd - inStart);
                            iLength = Convert.ToInt32(sLength);
                        }

                        string LastReplySubjectReplaceTag = "[LASTPOSTSUBJECT:" + iLength.ToString() + "]";
                        string sLastReplyTemp = sLastReply;

                        sLastReplyTemp = sLastReplyTemp.Replace("[AF:ICONLINK:LASTREPLY]", string.Format(LocalizeTokenString("[ICONLINK-LASTREPLY]", portalSettings, language), sLastReplyURL, mainSettings.ThemeLocation));
                        sLastReplyTemp = sLastReplyTemp.Replace("[AF:URL:LASTREPLY]", sLastReplyURL);

                        int PageSize = mainSettings.PageSize;
                        if (forumUser.UserId > 0)
                        {
                            PageSize = forumUser.PrefPageSize;
                        }

                        if (PageSize < 5)
                        {
                            PageSize = 10;
                        }

                        sLastReplyTemp = sLastReplyTemp.Replace(LastReplySubjectReplaceTag, Utilities.GetLastPostSubject(topic.LastReply.ReplyId, topic.TopicId, topic.ForumId, tabId, topic.LastReply.Content.Subject, iLength, pageSize: PageSize, replyCount: topic.ReplyCount, canRead: true));
                        if (template.ToString().Contains("[LASTPOSTDISPLAYNAME]"))
                        {
                            if (topic.LastReply.Author.AuthorId > 0)
                            {
                                sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDISPLAYNAME]", DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, topic.Forum.ModuleId, true, forumUser.GetIsMod(topic.ModuleId), forumUser.IsAdmin || forumUser.IsSuperUser, topic.LastReply.Author.AuthorId, topic.LastReply.Author.Username, topic.LastReply.Author.FirstName, topic.LastReply.Author.LastName, topic.LastReply.Author.DisplayName).ToString().Replace("&amp;#", "&#"));
                            }
                            else 
                            {
                                sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDISPLAYNAME]", topic.LastReply.Content.AuthorName);
                            }
                        }

                        if (template.ToString().Contains("[LASTPOSTDATE]"))
                        {
                            sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDATE]", Utilities.GetUserFormattedDateTime(topic.LastReply.Content.DateCreated, portalSettings.PortalId, forumUser.UserId));
                        }

                        template = TemplateUtils.ReplaceSubSection(template, sLastReplyTemp, "[LASTPOST]", "[/LASTPOST]");
                    }
                }
            }

            if (template.ToString().Contains("[TOPIC:STATUSCSSCLASS]"))
            {
                template.Replace("[TOPIC:STATUSCSSCLASS]", topic.GetTopicStatusCss(forumUser));
            }

            template = ReplaceDateToken(template, "[AF:LABEL:LastPostDate]", topic.LastReply.Content.DateCreated, portalSettings, forumUser);

            if (template.ToString().Contains("[AF:LABEL:TopicAuthor]"))
            {
                if (topic.LastReply.Author.AuthorId > 0)
                {
                    template.Replace("[AF:LABEL:TopicAuthor]", DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, topic.Forum.ModuleId, true, forumUser.GetIsMod(topic.ModuleId), forumUser.IsAdmin || forumUser.IsSuperUser, topic.Author.AuthorId, topic.Author.Username, topic.Author.FirstName, topic.Author.LastName, topic.Author.DisplayName).ToString().Replace("&amp;#", "&#"));
                }
                else
                {
                    template.Replace("[AF:LABEL:TopicAuthor]", topic.Content.AuthorName);
                }
            }

            if (template.ToString().Contains("[STARTEDBY]"))
            {
                var displayName = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, topic.Forum.ModuleId, true, forumUser.GetIsMod(topic.ModuleId), forumUser.IsAdmin || forumUser.IsSuperUser, topic.Author.AuthorId, topic.Content.AuthorName, topic.Author.FirstName, topic.Author.LastName, topic.Author.DisplayName).ToString().Replace("&amp;#", "&#");
                if (Utilities.StripHTMLTag(displayName) == "Anonymous")
                {
                    displayName = displayName.Replace("Anonymous", topic.Content.AuthorName);
                }

                template.Replace("[STARTEDBY]", displayName);
            }

            if (template.ToString().Contains("[POSTRATINGDISPLAY]"))
            {
                    template.Replace("[POSTRATINGDISPLAY]", topic.Rating == 0 ? string.Empty : string.Format(LocalizeTokenString("[POSTRATING]", portalSettings, language), topic.Rating));
            }

            if (template.ToString().Contains("[STATUS]"))
            {
                    template.Replace("[STATUS]", topic.StatusId == -1 ? string.Empty : string.Format(LocalizeTokenString("[TOPICSTATUS]", portalSettings, language), topic.StatusId));
            }

            if (template.ToString().Contains("[NEXTTOPIC]"))
            {
                if (topic.NextTopic == 0)
                {
                    template.Replace("[NEXTTOPIC]", string.Empty);
                }
                else
                {
                    @params = new List<string>()
                    {
                        $"{ParamKeys.ViewType}={Views.Topic}",
                        $"{ParamKeys.ForumId}={topic.ForumId}",
                        $"{ParamKeys.TopicId}={topic.NextTopic}",
                    };
                    if (topic.Forum.SocialGroupId > 0)
                    {
                        @params.Add($"{Literals.GroupId}={topic.Forum.SocialGroupId}");
                    }

                    template.Replace("[NEXTTOPIC]", string.Format(LocalizeTokenString("[NEXTTOPICLINK]", portalSettings, language), Utilities.NavigateURL(tabId, string.Empty, @params.ToArray())));
                }
            }

            if (template.ToString().Contains("[PREVTOPIC]"))
            {
                if (topic.PrevTopic == 0)
                {
                    template.Replace("[PREVTOPIC]", string.Empty);
                }
                else
                {
                    @params = new List<string>()
                    {
                        $"{ParamKeys.ViewType}={Views.Topic}",
                        $"{ParamKeys.ForumId}={topic.ForumId}",
                        $"{ParamKeys.TopicId}={topic.PrevTopic}",
                    };
                    if (topic.Forum.SocialGroupId > 0)
                    {
                        @params.Add($"{Literals.GroupId}={topic.Forum.SocialGroupId}");
                    }

                    template.Replace("[PREVTOPIC]", string.Format(LocalizeTokenString("[PREVTOPICLINK]", portalSettings, language), Utilities.NavigateURL(tabId, string.Empty, @params.ToArray())));
                }
            }

            return template;
        }

        internal static StringBuilder ReplacePostTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, HttpRequest request, int TabId)
        {
            string language = forumUser?.UserInfo?.Profile.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = (INavigationManager)new Services.URLNavigator();
            }

            template = RemoveObsoleteTokens(template);
            template = MapLegacyPostTokenSynonyms(template);

            template.Replace("[POSTID]", post.PostId.ToString());
            template.Replace("[AUTHORID]", post.Content.AuthorId.ToString());

            template = ExtractAndReplaceDateTokenWithOptionalFormatParameter(template, "[POSTDATE", post.Content.DateCreated, portalSettings, forumUser);

            template = ReplaceBody(template, post.Content, mainSettings, request.Url);

            if (template.ToString().Contains("[EDITDATE]"))
            {
                if (post.Content.DateUpdated != post.Content.DateCreated)
                {
                    template = ExtractAndReplaceDateTokenWithOptionalFormatParameter(template, "[EDITDATE", post.Content.DateUpdated, portalSettings, forumUser);
                }
                template.Replace("[EDITDATE]", string.Empty);

            }

            if (template.ToString().Contains("[MODEDITDATE]"))
            {
                if (forumUser != null &&
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(post.Forum.Security.Moderate, forumUser.UserRoles) &&
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(post.Forum.Security.Edit, forumUser.UserRoles) &&
                    post.Content.DateUpdated != post.Content.DateCreated)
                {
                    template = ExtractAndReplaceDateTokenWithOptionalFormatParameter(template, "[MODEDITDATE", post.Content.DateUpdated, portalSettings, forumUser);
                }
                template.Replace("[MODEDITDATE]", string.Empty);
            }

            if (template.ToString().Contains("[POST:STATUSCSSCLASS]"))
            {
                template.Replace("[POST:STATUSCSSCLASS]", post.GetPostStatusCss(forumUser));
            }

            if (template.ToString().Contains("[POSTICON]") || template.ToString().Contains("[POSTICONCSS]"))
            {
                string css = post.GetPostStatusCss(forumUser);
                if (template.ToString().Contains("[POSTICONCSS]"))
                {
                    template.Replace("[POSTICONCSS]", css);
                }

                if (template.ToString().Contains("[POSTICON]"))
                {
                    template.Replace("[POSTICON]", string.Format(LocalizeTokenString("[POSTICON]", portalSettings, language), css));
                }
            }

            return template;
        }

        private static StringBuilder ExtractAndReplaceDateTokenWithOptionalFormatParameter(StringBuilder template, string tokenPrefix, DateTime? datetime, PortalSettings portalSettings, ForumUserInfo forumUser)
        {
            if (datetime == null ||
                datetime == DateTime.MinValue ||
                datetime == SqlDateTime.MinValue.Value ||
                datetime == Utilities.NullDate())
            {
                return RemovePrefixedToken(template, tokenPrefix);
            }

            var replacementToken = $"{tokenPrefix}]";
            var dateFormat = Globals.DefaultDateFormat;
            if (template.ToString().Contains($"{tokenPrefix}:"))
            {
                dateFormat = template.ToString().Substring(template.ToString().IndexOf($"{tokenPrefix}:", StringComparison.Ordinal) + replacementToken.Length, 1);
                if (string.IsNullOrEmpty(dateFormat))
                {
                    dateFormat = Globals.DefaultDateFormat;
                }

                replacementToken = $"{tokenPrefix}:{dateFormat}]";

            }

            return ReplaceDateToken(template, replacementToken, (DateTime)datetime, portalSettings, forumUser, dateFormat);
        }

        private static StringBuilder ReplaceDateToken(StringBuilder template, string token, DateTime datetime, PortalSettings portalSettings, ForumUserInfo accessingUser, string dateFormat = "g")
        {
            if (template.ToString().Contains(token))
            {
                if (datetime != DateTime.MinValue &&
                    datetime != SqlDateTime.MinValue.Value &&
                    datetime != Utilities.NullDate())
                {
                    return template.Replace(token, Utilities.GetUserFormattedDateTime((DateTime?)datetime, portalSettings.PortalId, accessingUser.UserId, dateFormat));
                }

                return template.Replace(token, string.Empty);
            }

            return template;
        }

        internal static StringBuilder RemoveObsoleteTokens(StringBuilder template)
        {
            // no longer using this
            template = RemoveObsoleteTokenByPrefix(template, "[SPLITBUTTONS2");
            // Add This -- obsolete so just remove
            template = RemoveObsoleteTokenByPrefix(template, "[AF:CONTROL:ADDTHIS");

            // no longer using these
            string[] tokens = {
                "[DATECREATED]",
                "[OCCUPATION]",
                "[WEBSITE]",
                "[AF:PROFILE:YAHOO]",
                "[AF:PROFILE:MSN]",
                "[AF:PROFILE:ICQ]",
                "[AF:PROFILE:AOL]",
                "[AF:PROFILE:OCCUPATION]",
                "[AF:PROFILE:INTERESTS]",
                "[AF:PROFILE:LOCATION]",
                "[AF:PROFILE:WEBSITE]",
                "[AF:CONTROL:AVATAREDIT]",
                "[AF:BUTTON:PROFILEEDIT]",
                "[AF:BUTTON:PROFILESAVE]",
                "[AF:BUTTON:PROFILECANCEL]",
                "[AF:PROFILE:BIO]",
                "[MODUSERSETTINGS]",
            };
            tokens.ToList().ForEach(t => template = RemoveObsoleteToken(template, t));

            return template;
            return template;
        }

        internal static StringBuilder RemoveControlTokensForDnnPrintMode(StringBuilder template)
        {
            string[] tokens = {
                "[ADDREPLY]",
                "[QUICKREPLY]",
                "[TOPICSUBSCRIBE]",
                "[AF:CONTROL:PRINTER]",
                "[AF:CONTROL:EMAIL]",
                "[PAGER1]",
                "[PAGER2]",
                "[SORTDROPDOWN]",
                "[POSTRATINGBUTTON]",
                "[JUMPTO]",
                "[NEXTTOPIC]",
                "[PREVTOPIC]",
                "[AF:CONTROL:STATUS]",
                "[ACTIONS:DELETE]",
                "[ACTIONS:EDIT]",
                "[ACTIONS:QUOTE]",
                "[ACTIONS:REPLY]",
                "[ACTIONS:ANSWER]",
                "[ACTIONS:ALERT]",
                "[ACTIONS:BAN]",
                "[ACTIONS:MOVE]",
                "[RESX:SortPosts]",
            };

            tokens.ToList().ForEach(t => template.Replace(t, string.Empty));
            return template;
        }

        internal static StringBuilder MapLegacyPortalTokenSynonyms(StringBuilder template)
        {
            // replace synonyms
            template = ReplaceTokenSynonym(template, "[PORTALID]", "[PORTAL:PORTALID]");
            return template;
        }

        internal static StringBuilder MapLegacyModuleTokenSynonyms(StringBuilder template)
        {
            return template;
        }

        internal static StringBuilder MapLegacyForumTokenSynonyms(StringBuilder template)
        {
            template = ReplaceTokenSynonym(template, "[DISPLAYNAME]", "[FORUM:LASTPOSTDISPLAYNAME]");
            template = ReplaceTokenSynonym(template, "[FORUMID]", "[FORUM:FORUMID]");
            template = ReplaceTokenSynonym(template, "[FORUMGROUPID]", "[FORUM:FORUMGROUPID]");
            template = ReplaceTokenSynonym(template, "[PARENTFORUMID]", "[FORUM:PARENTFORUMID]");
            template = ReplaceTokenSynonym(template, "[PARENTFORUMNAME]", "[FORUM:PARENTFORUMNAME]");
            template = ReplaceTokenSynonym(template, "[GROUPNAME]", "[FORUM:GROUPNAME]");
            template = ReplaceTokenSynonym(template, "[FORUMSUBSCRIBERCOUNT]", "[FORUM:SUBSCRIBERCOUNT]");
            template = ReplaceTokenSynonym(template, "[TOTALREPLIES]", "[FORUM:TOTALREPLIES]");
            template = ReplaceTokenSynonym(template, "[TOTALTOPICS]", "[FORUM:TOTALTOPICS]");
            template = ReplaceTokenSynonym(template, "[FORUMNAMENOLINK]", "[FORUM:FORUMNAME]");
            template = ReplaceTokenSynonym(template, "[AF:CONTROL:FORUMID]", "[FORUM:FORUMID]");
            template = ReplaceTokenSynonym(template, "[AF:CONTROL:FORUMGROUPID]", "[FORUM:FORUMGROUPID]");
            template = ReplaceTokenSynonym(template, "[FORUMDESCRIPTION]", "[FORUM:FORUMDESCRIPTION]");

            return template;
        }

        internal static StringBuilder MapLegacyUserTokenSynonyms(StringBuilder template)
        {
            template = ReplaceTokenSynonym(template, "[SENDERUSERNAME]", "[FORUMUSER:USERNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERFIRSTNAME", "[FORUMUSER:FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERLASTNAME]", "[FORUMUSER:LASTNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERDISPLAYNAME]", "[FORUMUSER:DISPLAYNAME]");

            template = ReplaceTokenSynonym(template, "[SIGNATURE]", "[FORUMUSER:SIGNATURE]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:SIGNATURE]", "[FORUMUSER:SIGNATURE]");

            template = ReplaceTokenSynonym(template, "[USERCAPTION]", "[FORUMUSER:USERCAPTION]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERCAPTION]", "[FORUMUSER:USERCAPTION]");

            template = ReplaceTokenSynonym(template, "[AVATAR]", "[FORUMUSER:AVATAR]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:AVATAR]", "[FORUMUSER:AVATAR]");

            template = ReplaceTokenSynonym(template, "[RANKNAME]", "[FORUMUSER:RANKNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:RANKNAME]", "[FORUMUSER:RANKNAME]");

            template = ReplaceTokenSynonym(template, "[RANKDISPLAY]", "[FORUMUSER:RANKDISPLAY]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:RANKDISPLAY]", "[FORUMUSER:RANKDISPLAY]");

            template = ReplaceTokenSynonym(template, "[USERNAME]", "[FORUMUSER:USERNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERNAME]", "[FORUMUSER:USERNAME]");

            template = ReplaceTokenSynonym(template, "[USERID]", "[FORUMUSER:USERID]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERID]", "[FORUMUSER:USERID]");
            
            template = ReplaceTokenSynonym(template, "[EMAIL]", "[FORUMUSER:EMAIL]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:EMAIL]", "[FORUMUSER:EMAIL]");

            template = ReplaceTokenSynonym(template, "[FIRSTNAME]", "[FORUMUSER:FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:FIRSTNAME]", "[FORUMUSER:FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[LASTNAME]", "[FORUMUSER:LASTNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:LASTNAME]", "[FORUMUSER:FIRSTNAME]");

            template = ReplaceTokenSynonym(template, "[DISPLAYNAME]", "[FORUMUSER:DISPLAYNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:DISPLAYNAME]", "[FORUMUSER:DISPLAYNAME]");

            template = ReplaceTokenSynonym(template, "[MEMBERSINCE]", "[FORUMUSER:DATECREATED]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:DATECREATED]", "[FORUMUSER:DATECREATED]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:MEMBERSINCE]", "[FORUMUSER:DATECREATED]");

            template = ReplaceTokenSynonym(template, "[AF:PROFILE:LASTACTIVE]", "[FORUMUSER:DATELASTACTIVITY]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:DATELASTACTIVITY]", "[FORUMUSER:DATELASTACTIVITY]"); 
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:DATELASTPOST]", "[FORUMUSER:DATELASTPOST]");
            
            template = ReplaceTokenSynonym(template, "[USERCAPTION]", "[FORUMUSER:USERCAPTION]");
            template = ReplaceTokenSynonym(template, "[[AF:PROFILE:USERCAPTION]", "[FORUMUSER:USERCAPTION]");
            
            template = ReplaceTokenSynonym(template, "[USERSTATUS]", "[FORUMUSER:USERSTATUS]"); 
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERSTATUS]", "[FORUMUSER:USERSTATUS]"); 
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERSTATUS:CSS]", "[FORUMUSER:USERSTATUSCSS]");  
            
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:TOTALPOINTS]", "[FORUMUSER:TOTALPOINTS]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:VIEWCOUNT]", "[FORUMUSER:VIEWCOUNT]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:ANSWERCOUNT]", "[FORUMUSER:ANSWERCOUNT]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:REWARDPOINTS]", "[FORUMUSER:REWARDPOINTS]");
            
            template = ReplaceTokenSynonym(template, "[POSTS]", "[FORUMUSER:POSTS]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:POSTS]", "[FORUMUSER:POSTS]");

            template = ReplaceTokenSynonym(template, "[POSTCOUNT]", "[FORUMUSER:POSTCOUNT]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:POSTCOUNT]", "[FORUMUSER:POSTCOUNT]");

            template = ReplaceTokenSynonym(template, "[AF:POINTS:TOPICCOUNT]", "[FORUMUSER:TOPICCOUNT]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:TOPICCOUNT]", "[FORUMUSER:TOPICCOUNT]");

            template = ReplaceTokenSynonym(template, "[AF:POINTS:REPLYCOUNT]", "[FORUMUSER:REPLYCOUNT]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:REPLYCOUNT]", "[FORUMUSER:REPLYCOUNT]");

            return template;
        }
        

        internal static StringBuilder MapLegacyTopicTokenSynonyms(StringBuilder template)
        {
            // replace synonyms
            template = ReplaceTokenSynonym(template, "[AF:LABEL:TopicDateCreated]", "[POSTDATE]");
            template = ReplaceTokenSynonym(template, "[TOPICID]", "[POSTID]");
            template = ReplaceTokenSynonym(template, "[AF:LABEL:ReplyCount]", "[REPLYCOUNT]");
            template = ReplaceTokenSynonym(template, "[VIEWS]", "[VIEWCOUNT]");
            return template;
        }

        internal static StringBuilder MapLegacyPostTokenSynonyms(StringBuilder template)
        {
            template = ReplaceTokenSynonym(template, "[TOPICID]", "[POSTID]");
            template = ReplaceTokenSynonym(template, "[REPLYID]", "[POSTID]");
            return template;
        }

        private static StringBuilder ReplaceTokenSynonym(StringBuilder template, string fromToken, string toToken)
        {
            if (template.ToString().Contains(fromToken))
            {
                var log = new LogInfo { LogTypeKey = Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                var message = string.Format(Utilities.GetSharedResource("[RESX:ReplaceObsoleteToken]"), fromToken, toToken);
                log.AddProperty("Message", message);
                LogController.Instance.AddLog(log);
                return template.Replace(fromToken, toToken);
            }

            return template;
        }

        private static StringBuilder RemoveObsoleteToken(StringBuilder template, string token)
        {
            if (template.ToString().Contains(token))
            {
                var log = new LogInfo { LogTypeKey = Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                var message = string.Format(Utilities.GetSharedResource("[RESX:RemoveObsoleteToken]"), token);
                log.AddProperty("Message", message);
                LogController.Instance.AddLog(log);
                return template.Replace(token, string.Empty); 
            }

            return template;
        }

        private static StringBuilder RemoveObsoleteTokenByPrefix(StringBuilder template, string tokenPrefix)
        {
            if (template.ToString().Contains(tokenPrefix))
            {
                var log = new LogInfo { LogTypeKey = Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                var message = string.Format(Utilities.GetSharedResource("[RESX:RemoveObsoletePrefixedToken]"), tokenPrefix);
                log.AddProperty("Message", message);
                LogController.Instance.AddLog(log);
                return RemovePrefixedToken(template, tokenPrefix);
            }

            return template;
        }

        internal static StringBuilder RemovePrefixedToken(StringBuilder template, string tokenPrefix)
        {
            if (tokenPrefix.Substring(0, 1) != "[")
            {
                tokenPrefix = "[" + tokenPrefix;
            }
            if (template.ToString().Contains(tokenPrefix))
            {
                int inStart = template.ToString().IndexOf(tokenPrefix, 0);
                int inEnd = template.ToString().IndexOf("]", inStart - 1);
                template.Remove(inStart, (inEnd - inStart) + 1);
            }
            return template;
        }

        private static string LocalizeTokenString(string key, DotNetNuke.Entities.Portals.PortalSettings portalSettings, string language = "en-US")
        {
            return Utilities.LocalizeString(key,
                Globals.TokenResourceFile,
                (DotNetNuke.Entities.Portals.PortalSettings)portalSettings,
                language);
        }

        private static StringBuilder ReplaceBody(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.ContentInfo content, SettingsInfo mainSettings, Uri uri)
        {
            if (template.ToString().Contains("[BODY]"))
            {
                string sBody = content?.Body;
                if (string.IsNullOrEmpty(content?.Body))
                {
                    sBody = " <br />";
                }
                else
                {
                    sBody = Utilities.ManageImagePath(HttpUtility.HtmlDecode(content?.Body), uri);

                    sBody = sBody.Replace("[", "&#91;").Replace("]", "&#93;");
                    if (sBody.ToUpper().Contains("&#91;CODE&#93;"))
                    {
                        sBody = Regex.Replace(sBody, "(&#91;CODE&#93;)", "[CODE]", RegexOptions.IgnoreCase);
                        sBody = Regex.Replace(sBody, "(&#91;\\/CODE&#93;)", "[/CODE]", RegexOptions.IgnoreCase);
                    }

                    // sBody = sBody.Replace("&lt;CODE&gt;", "<CODE>")
                    if (Regex.IsMatch(sBody, "\\[CODE([^>]*)\\]", RegexOptions.IgnoreCase))
                    {
                        var objCode = new CodeParser();
                        sBody = CodeParser.ParseCode(HttpUtility.HtmlDecode(sBody));
                    }

                    sBody = Utilities.StripExecCode(sBody);
                    if (mainSettings.AutoLinkEnabled)
                    {
                        sBody = Utilities.AutoLinks(sBody, uri.Host);
                    }

                    if (sBody.Contains("<%@"))
                    {
                        sBody = sBody.Replace("<%@ ", "&lt;&#37;@ ");
                    }

                    if (content.Body.ToLowerInvariant().Contains("runat"))
                    {
                        sBody = Regex.Replace(sBody, "runat", "&#114;&#117;nat", RegexOptions.IgnoreCase);
                    }
                }

                template.Replace("[BODY]", sBody);
            }
            return template;
        }
        
        private static string GetTopicTitle(string body)
        {
            if (!string.IsNullOrEmpty(body))
            {

                body = HttpUtility.HtmlDecode(body);
                body = body.Replace("<br>", System.Environment.NewLine);
                body = Utilities.StripHTMLTag(body);
                body = body.Length > 500 ? body.Substring(0, 500) + "..." : body;
                body = body.Replace("\"", "'");
                body = body.Replace("?", string.Empty);
                body = body.Replace("+", string.Empty);
                body = body.Replace("%", string.Empty);
                body = body.Replace("#", string.Empty);
                body = body.Replace("@", string.Empty);
                return body.Trim();
            }

            return string.Empty;
        }

    }
}
