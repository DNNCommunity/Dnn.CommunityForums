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

        internal static StringBuilder ReplaceForumTokens(StringBuilder template,
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum,
            DotNetNuke.Entities.Portals.PortalSettings portalSettings,
            SettingsInfo mainSettings,
            INavigationManager navigationManager,
            ForumUserInfo forumUser,
            HttpRequest request,
            int tabId, CurrentUserTypes currentUserType)
        {
            string language = forumUser?.UserInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = new Services.URLNavigator().NavigationManager();
            }

            template = RemoveObsoleteTokens(template);
            template = ReplaceTokenSynonyms(template);

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

            template.Replace("[FORUMID]", forum.ForumID.ToString());
            template.Replace("[FORUMGROUPID]", forum.ForumGroupId.ToString());
            template.Replace("[FORUMNAMENOLINK]", forum.ForumName);
            template.Replace("[GROUPNAME]", forum.GroupName);
            template.Replace("[TOTALTOPICS]", forum.TotalTopics.ToString());
            template.Replace("[TOTALREPLIES]", forum.TotalReplies.ToString());
            template.Replace("[PARENTFORUMID]", forum.ParentForumId.ToString());
            if (template.ToString().Contains("[FORUMSUBSCRIBERCOUNT]"))
            {
                template.Replace("[FORUMSUBSCRIBERCOUNT]", forum.SubscriberCount.ToString());
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

            if (template.ToString().Contains("[DCF:FORUM:STATUS:CSSCLASS]"))
            {
                template.Replace("[DCF:FORUM:STATUS:CSSCLASS]", DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumStatusCss(forum, forumUser));
            }

            if (template.ToString().Contains("[FORUMICONCSS]") || template.ToString().Contains("[FORUMICONSM]"))
            {
                string sFolderCSS = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumFolderIconCss(forum, forumUser);
                template.Replace("[FORUMICONCSS]", string.Format(LocalizeTokenString("[FORUMICONCSS]", portalSettings, language), sFolderCSS));
                template.Replace("[FORUMICONSM]", string.Format(LocalizeTokenString("[FORUMICONSM]", portalSettings, language), sFolderCSS));
            }

            if (template.ToString().Contains("[LASTPOSTSUBJECT"))
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

            if (template.ToString().Contains("[DISPLAYNAME]"))
            {
                if (forum.LastPostUserID <= 0)
                {
                    template.Replace("[DISPLAYNAME]", string.Format(LocalizeTokenString("[DISPLAYNAME]", portalSettings, language), forum.LastPostDisplayName));
                }
                else
                {
                    bool isMod = currentUserType == CurrentUserTypes.Admin || currentUserType == CurrentUserTypes.ForumMod || currentUserType == CurrentUserTypes.SuperUser;
                    bool isAdmin = currentUserType == CurrentUserTypes.Admin || currentUserType == CurrentUserTypes.SuperUser;
                    template.Replace("[DISPLAYNAME]", string.Format(LocalizeTokenString("[DISPLAYNAME]", portalSettings, language), DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, forum.ModuleId, true, isMod, isAdmin, forum.LastPostUserID, forum.LastPostUserName, forum.LastPostFirstName, forum.LastPostLastName, forum.LastPostDisplayName)));
                }
            }
            template = ReplaceDateToken(template, "[LASTPOSTDATE]",forum.LastPostDateTime, portalSettings, forumUser);

            return template;
        }

        internal static StringBuilder ReplaceModuleTokens(StringBuilder template, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, ForumUserInfo forumUser, int tabId, int forumModuleId)
        {
            template = RemoveObsoleteTokens(template);
            template = ReplaceTokenSynonyms(template);

            string language = forumUser?.UserInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;
            var urlNavigator = new Services.URLNavigator();

            template.Replace("[PORTALID]", portalSettings?.PortalId.ToString());
            template.Replace("[PORTALNAME]", portalSettings?.PortalName);
            template.Replace("[MODULEID]", forumModuleId.ToString());
            template.Replace("[TABID]", tabId.ToString());
            if (template.ToString().Contains("[FORUMMAINLINK"))
            {
                template.Replace("[FORUMMAINLINK]", string.Format(LocalizeTokenString("[FORUMMAINLINK]", portalSettings, language), urlNavigator.NavigateURL(tabId)));
            }

            template.Replace("[PAGENAME]", HttpUtility.HtmlEncode(string.IsNullOrEmpty(portalSettings.ActiveTab.Title) ? portalSettings.ActiveTab.TabName : portalSettings.ActiveTab.Title));
            return template;
        }

        internal static StringBuilder ReplaceUserTokens(StringBuilder template, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, int tabId, int forumModuleId)
        {
            template = RemoveObsoleteTokens(template);
            template = ReplaceTokenSynonyms(template);

            if (template.ToString().Contains("[DISPLAYNAME]"))
            {
                template.Replace("[DISPLAYNAME]",
                    DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings,
                        forumModuleId,
                        true,
                        forumUser.GetIsMod(forumModuleId),
                        forumUser.IsAdmin || forumUser.IsSuperUser,
                        forumUser.UserId,
                        forumUser?.Username,
                        forumUser?.FirstName,
                        forumUser?.LastName,
                        forumUser?.DisplayName));
            }
            
            template.Replace("[USERID]", forumUser?.UserId.ToString());
            template.Replace("[USERNAME]", forumUser?.Username);
            template.Replace("[FIRSTNAME]", forumUser?.FirstName);
            template.Replace("[LASTNAME]", forumUser?.LastName);
            template.Replace("[FULLNAME]", string.Concat(forumUser?.FirstName, " ", forumUser?.LastName));
            return template;
        }

        internal static StringBuilder ReplaceTopicTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, HttpRequest request, int TabId)
        {
            string language = forumUser?.UserInfo?.Profile.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = (INavigationManager)new Services.URLNavigator();
            }

            var @params = new List<string>();

            template = RemoveObsoleteTokens(template);
            template = ReplaceTokenSynonyms(template);

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

            template.Replace("[POSTID]", topic.TopicId.ToString());
            template.Replace("[AUTHORID]", topic.Content.AuthorId.ToString());
            template.Replace("[REPLYCOUNT]", topic.ReplyCount.ToString());
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

                string sLastReplyURL = navigationManager.NavigateURL(TabId, string.Empty, @params.ToArray());
                string sTopicURL = new ControlUtils().BuildUrl(TabId, topic.Forum.ModuleId, topic.Forum.ForumGroup.PrefixURL, topic.Forum.PrefixURL, topic.Forum.ForumGroupId, topic.Forum.ForumID, topic.TopicId, topic.TopicUrl, -1, -1, string.Empty, 1, -1, topic.Forum.SocialGroupId);
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

                    sLastReadURL = navigationManager.NavigateURL(TabId, string.Empty, @params.ToArray());

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

                        sLastReadURL = navigationManager.NavigateURL(TabId, string.Empty, @params.ToArray());
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

                            slink = "<a title=\"" + sBodyTitle + "\" href=\"" + navigationManager.NavigateURL(TabId, string.Empty, @params.ToArray()) + "\">" + subject + "</a>";
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
                        template = TemplateUtils.ReplaceSubSection(template, Utilities.GetUserFormattedDateTime(topic.LastReply.Content.DateCreated, portalSettings.PortalId, forumUser.UserId), "[LASTPOST]", "[/LASTPOST]");
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

                        sLastReplyTemp = sLastReplyTemp.Replace(LastReplySubjectReplaceTag, Utilities.GetLastPostSubject(topic.LastReply.ReplyId, topic.TopicId, topic.ForumId, TabId, topic.LastReply.Content.Subject, iLength, pageSize: PageSize, replyCount: topic.ReplyCount, canRead: true));
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

            if (template.ToString().Contains("[DCF:TOPIC:STATUS:CSSCLASS]"))
            {
                template.Replace("[DCF:TOPIC:STATUS:CSSCLASS]", DotNetNuke.Modules.ActiveForums.Controllers.TopicController.GetTopicStatusCss(topic, forumUser));
            }

            if (template.ToString().Contains("[DCF:POST:STATUS:CSSCLASS]"))
            {
                template.Replace("[DCF:POST:STATUS:CSSCLASS]", DotNetNuke.Modules.ActiveForums.Controllers.TopicController.GetPostStatusCss(topic, forumUser));
            }

            if (template.ToString().Contains("[POSTICON]") || template.ToString().Contains("[POSTICONCSS]"))
            {
                string css = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.GetTopicStatusCss(topic, forumUser);
                if (template.ToString().Contains("[POSTICONCSS]"))
                {
                    template.Replace("[POSTICONCSS]", css);
                }

                if (template.ToString().Contains("[POSTICON]"))
                {
                    template.Replace("[POSTICON]", string.Format(LocalizeTokenString("[POSTICON]", portalSettings, language), css));
                }
            }

            if (template.ToString().Contains("[EDITDATE]"))
            {
                if (topic.Content.DateUpdated != topic.Content.DateCreated)
                {
                    template = ReplaceDateToken(template, "[EDITDATE]", topic.Content.DateUpdated, portalSettings, forumUser);
                }
                else
                {
                    template.Replace("[EDITDATE]", string.Empty);
                }
            }

            if (template.ToString().Contains("[MODEDITDATE]"))
            {
                if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(topic.Forum.Security.Moderate, forumUser.UserRoles) &&
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(topic.Forum.Security.Edit, forumUser.UserRoles) &&
                    topic.Content.DateUpdated != topic.Content.DateCreated)
                {
                    template = ReplaceDateToken(template, "[MODEDITDATE]", topic.Content.DateUpdated, portalSettings, forumUser);
                }
                else
                {
                    template.Replace("[MODEDITDATE]", string.Empty);
                }
            }
            
            template = ReplaceDateToken(template, "[POSTDATE]", topic.Content.DateCreated, portalSettings, forumUser);
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
                    template.Replace("[POSTRATINGDISPLAY]", topic.Rating == 0 ? string.Empty : string.Format(LocalizeTokenString("[TOPICSTATUS]", portalSettings, language), topic.Rating));
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

                    template.Replace("[NEXTTOPIC]", string.Format(LocalizeTokenString("[NEXTTOPICLINK]", portalSettings, language), Utilities.NavigateURL(TabId, string.Empty, @params.ToArray())));
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

                    template.Replace("[PREVTOPIC]", string.Format(LocalizeTokenString("[PREVTOPICLINK]", portalSettings, language), Utilities.NavigateURL(TabId, string.Empty, @params.ToArray())));
                }
            }

            return template;
        }

        internal static StringBuilder ReplaceReplyTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, HttpRequest request, int TabId)
        {
            string language = forumUser?.UserInfo?.Profile.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = (INavigationManager)new Services.URLNavigator();
            }

            template = RemoveObsoleteTokens(template);
            template = ReplaceTokenSynonyms(template);

            template.Replace("[POSTID]", reply.ReplyId.ToString());
            template.Replace("[AUTHORID]", reply.Content.AuthorId.ToString());

            template = ReplaceDateToken(template, "[POSTDATE]", reply.Content.DateCreated, portalSettings, forumUser);

            template.Replace("[SUBJECT]", reply.Content.Subject.Replace("[", "&#91").Replace("]", "&#93"));

            template = ReplaceBody(template, reply.Content, mainSettings, request.Url);

            if (template.ToString().Contains("[EDITDATE]"))
            {
                if (reply.Content.DateUpdated != reply.Content.DateCreated)
                {
                    template = ReplaceDateToken(template, "[EDITDATE]", reply.Content.DateUpdated, portalSettings, forumUser);
                }
                else
                {
                    template.Replace("[EDITDATE]", string.Empty);
                }
            }

            if (template.ToString().Contains("[MODEDITDATE]"))
            {
                if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(reply.Forum.Security.Moderate, forumUser.UserRoles) &&
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(reply.Forum.Security.Edit, forumUser.UserRoles) &&
                    reply.Content.DateUpdated != reply.Content.DateCreated)
                {
                    template = ReplaceDateToken(template, "[MODEDITDATE]", reply.Content.DateUpdated, portalSettings, forumUser);
                }
                else
                {
                    template.Replace("[MODEDITDATE]", string.Empty);
                }
            }

            if (template.ToString().Contains("[DCF:POST:STATUS:CSSCLASS]"))
            {
                template.Replace("[DCF:POST:STATUS:CSSCLASS]", DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.GetPostStatusCss(reply, forumUser));
            }

            return template;
        }

        private static StringBuilder ReplaceDateToken(StringBuilder template, string token, DateTime datetime, PortalSettings portalSettings, ForumUserInfo forumUser)
        {
            if (template.ToString().Contains(token))
            {
                if (datetime != DateTime.MinValue &&
                    datetime != SqlDateTime.MinValue.Value &&
                    datetime != Utilities.NullDate())
                {
                    return template.Replace(token,
                        Utilities.GetUserFormattedDateTime(datetime, portalSettings.PortalId, forumUser.UserId));
                }

                return template.Replace(token, string.Empty);
            }

            return template;
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

        private static StringBuilder RemoveObsoleteTokens(StringBuilder template)
        {
            // no longer using this
            template = RemoveObsoleteTokenByPrefix(template, "[SPLITBUTTONS2");
            // Add This -- obsolete so just remove
            template = RemoveObsoleteTokenByPrefix(template, "[AF:CONTROL:ADDTHIS");
            return template;
        }
        
        internal static StringBuilder RemoveControlTokensForDnnPrintMode(StringBuilder template)
        {
            template.Replace("[ADDREPLY]", string.Empty);
            template.Replace("[QUICKREPLY]", string.Empty);
            template.Replace("[TOPICSUBSCRIBE]", string.Empty);
            template.Replace("[AF:CONTROL:PRINTER]", string.Empty);
            template.Replace("[AF:CONTROL:EMAIL]", string.Empty);
            template.Replace("[PAGER1]", string.Empty);
            template.Replace("[PAGER2]", string.Empty);
            template.Replace("[SORTDROPDOWN]", string.Empty);
            template.Replace("[POSTRATINGBUTTON]", string.Empty);
            template.Replace("[JUMPTO]", string.Empty);
            template.Replace("[NEXTTOPIC]", string.Empty);
            template.Replace("[PREVTOPIC]", string.Empty);
            template.Replace("[AF:CONTROL:STATUS]", string.Empty);
            template.Replace("[ACTIONS:DELETE]", string.Empty);
            template.Replace("[ACTIONS:EDIT]", string.Empty);
            template.Replace("[ACTIONS:QUOTE]", string.Empty);
            template.Replace("[ACTIONS:REPLY]", string.Empty);
            template.Replace("[ACTIONS:ANSWER]", string.Empty);
            template.Replace("[ACTIONS:ALERT]", string.Empty);
            template.Replace("[ACTIONS:BAN]", string.Empty);
            template.Replace("[ACTIONS:MOVE]", string.Empty);
            template.Replace("[RESX:SortPosts]:", string.Empty);

            return template;
        }
        private static StringBuilder ReplacePortalTokenSynonyms(StringBuilder template)
        {
            // replace synonyms
            template = ReplaceTokenSynonym(template, "[PORTALID]", "[PORTAL:PORTALID]");
            return template;
        }

        private static StringBuilder ReplaceUserTokenSynonyms(StringBuilder template)
        {
            template = ReplaceTokenSynonym(template, "[SENDERUSERNAME]", "[USERNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERFIRSTNAME", "[FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERLASTNAME]", "[LASTNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERDISPLAYNAME]", "[DISPLAYNAME]");
            return template;
        }

        private static StringBuilder ReplaceForumTokenSynonyms(StringBuilder template)
        {
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
            return template;
        }

        private static StringBuilder ReplaceTopicTokenSynonyms(StringBuilder template)
        {
            // replace synonyms
            template = ReplaceTokenSynonym(template, "[DATECREATED]", "[POSTDATE]");
            template = ReplaceTokenSynonym(template, "[AF:LABEL:TopicDateCreated]", "[POSTDATE]");
            template = ReplaceTokenSynonym(template, "[TOPICID]", "[POSTID]");
            template = ReplaceTokenSynonym(template, "[AF:LABEL:ReplyCount]", "[REPLYCOUNT]");
            template = ReplaceTokenSynonym(template, "[VIEWS]", "[VIEWCOUNT]");
            return template;
        }

        private static StringBuilder ReplaceReplyTokenSynonyms(StringBuilder template)
        {
            // replace synonyms
            template = ReplaceTokenSynonym(template, "[DATECREATED]", "[POSTDATE]");
            template = ReplaceTokenSynonym(template, "[AF:LABEL:TopicDateCreated]", "[POSTDATE]");
            template = ReplaceTokenSynonym(template, "[TOPICID]", "[POSTID]");
            template = ReplaceTokenSynonym(template, "[REPLYID]", "[POSTID]");
            template = ReplaceTokenSynonym(template, "[VIEWS]", "[VIEWCOUNT]");
            return template;
        }

        private static StringBuilder ReplaceTokenSynonyms(StringBuilder template)
        {
            // replace synonyms
            template = ReplaceTokenSynonym(template, "[DATECREATED]", "[POSTDATE]");
            template = ReplaceTokenSynonym(template, "[AF:LABEL:TopicDateCreated]", "[POSTDATE]");
            template = ReplaceTokenSynonym(template, "[TOPICID]", "[POSTID]");
            template = ReplaceTokenSynonym(template, "[REPLYID]", "[POSTID]");
            template = ReplaceTokenSynonym(template, "[AF:LABEL:ReplyCount]", "[REPLYCOUNT]");
            template = ReplaceTokenSynonym(template, "[VIEWS]", "[VIEWCOUNT]");
            template = ReplaceTokenSynonym(template, "[AF:CONTROL:FORUMID]", "[FORUMID]");
            template = ReplaceTokenSynonym(template, "[AF:CONTROL:FORUMGROUPID]", "[FORUMGROUPID]");
            template = ReplaceTokenSynonym(template, "[SENDERUSERNAME]", "[USERNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERFIRSTNAME", "[FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERLASTNAME]", "[LASTNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERDISPLAYNAME]", "[DISPLAYNAME]");
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
