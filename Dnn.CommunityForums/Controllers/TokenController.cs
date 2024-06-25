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
using System.Collections.Generic;
using System.Text;
using System.Web;
using DotNetNuke.Abstractions;
using DotNetNuke.Modules.ActiveForums.Entities;

namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Remove in 10.00.00. Not Used. Use DotNetNuke.Modules.ActiveForums.Controllers.TokenController()")]
    public class TokensController { TokensController() { throw new NotImplementedException(); } }    
}
    namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal static partial class TokenController
    {
        internal static List<Token> TokensList(int ModuleId, string group)
        {
            try
            {
                List<Token> li = (List<Token>)DataCache.SettingsCacheRetrieve(ModuleId, string.Format(CacheKeys.Tokens, ModuleId, group));
                if (li == null)
                {
                    li = new List<Token>();
                    Token tk = null;
                    System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
                    string sPath = DotNetNuke.Modules.ActiveForums.Utilities.MapPath(Globals.ModulePath + "config/tokens.config");
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
                    DataCache.SettingsCacheStore(ModuleId, string.Format(CacheKeys.Tokens, ModuleId, group), li);
                }
                return li;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return null;
            }
        }
        internal static string TokenGet(int ModuleId, string group, string TokenName)
        {
            string sOut = string.Empty;
            List<Token> tl = TokensList(ModuleId, group);
            foreach (Token t in tl)
            {
                if (t.TokenTag == TokenName)
                {
                    sOut = t.TokenReplace;
                    break;
                }
            }
            return sOut;
        }
        internal static string LocalizeTokenString(string key, DotNetNuke.Entities.Portals.PortalSettings portalSettings, string language = "en-US")
        {
            return Utilities.LocalizeString(key, Globals.TokenResourceFile, (DotNetNuke.Entities.Portals.PortalSettings)portalSettings, language);
        }
        internal static StringBuilder ReplaceUserTokens(StringBuilder template, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, DotNetNuke.Entities.Users.UserInfo userInfo, int TabId, int ForumModuleId)
        {
            template.Replace("[USERID]", userInfo?.UserID.ToString());
            template.Replace("[DISPLAYNAME]", UserProfiles.GetDisplayName(ForumModuleId, (userInfo == null ? -1 : userInfo.UserID), userInfo?.Username, userInfo?.FirstName, userInfo?.LastName, userInfo?.DisplayName));
            template.Replace("[USERNAME]", userInfo?.Username);
            template.Replace("[USERID]", userInfo?.UserID.ToString());
            template.Replace("[FIRSTNAME]", userInfo?.FirstName);
            template.Replace("[LASTNAME]", userInfo?.LastName);
            template.Replace("[FULLNAME]", string.Concat(userInfo?.FirstName, " ", userInfo?.LastName));


            template.Replace("[SENDERUSERNAME]", userInfo?.UserID.ToString());
            template.Replace("[SENDERFIRSTNAME]", userInfo?.FirstName);
            template.Replace("[SENDERLASTNAME]", userInfo?.LastName);
            template.Replace("[SENDERDISPLAYNAME]", userInfo?.DisplayName);

            return template;
        }
        internal static StringBuilder ReplaceTopicTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, DotNetNuke.Entities.Users.UserInfo userInfo, DotNetNuke.Modules.ActiveForums.User user, int UserLastTopicRead, int UserLastReplyRead, int TabId, int ForumModuleId)
        {
            string language = userInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = (INavigationManager)new Services.URLNavigator();
            }
            // no longer using this
            template = RemovePrefixedToken(template, "[SPLITBUTTONS2");


            template.Replace("[TOPICID]", topic.TopicId.ToString());
            template.Replace("[AUTHORID]", topic.Content.AuthorId.ToString());
            template.Replace("[REPLIES]", topic.ReplyCount.ToString());
            template.Replace("[VIEWS]", topic.ViewCount.ToString());



            string sBodyTitle = GetTopicTitle(topic.Content.Body, topic.Content.AuthorId);

            template.Replace("[BODYTITLE]", sBodyTitle);
            template.Replace("[BODY]", HttpUtility.HtmlDecode(topic.Content.Body));
            int BodyLength = -1;
            string BodyTrim = string.Empty;

            if (template.ToString().Contains("[BODY:"))
            {
                int inStart = (template.ToString().IndexOf("[BODY:", 0) + 1) + 5;
                int inEnd = (template.ToString().IndexOf("]", inStart - 1) + 1) - 1;
                string sLength = template.ToString().Substring(inStart, inEnd - inStart);
                BodyLength = Convert.ToInt32(sLength);
                BodyTrim = "[BODY:" + BodyLength.ToString() + "]";
            } if (string.IsNullOrEmpty(topic.Content.Summary) && template.ToString().Contains("[SUMMARY]") && string.IsNullOrEmpty(BodyTrim))
            {
                BodyTrim = "[BODY:250]";
                BodyLength = 250;
            }
            string BodyPlain = string.Empty;
            if (!(BodyTrim == string.Empty))
            {
                BodyPlain = topic.Content.Body.Replace("<br>", System.Environment.NewLine);
                BodyPlain = BodyPlain.Replace("<br />", System.Environment.NewLine);
                BodyPlain = Utilities.StripHTMLTag(BodyPlain);
                if (BodyLength > 0 & BodyPlain.Length > BodyLength)
                {
                    BodyPlain = BodyPlain.Substring(0, BodyLength);
                }
                BodyPlain = BodyPlain.Replace(System.Environment.NewLine, "<br />");
                template.Replace(BodyTrim, BodyPlain);
            }
            if (string.IsNullOrEmpty(topic.Content.Summary))
            {
                topic.Content.Summary = BodyPlain;
                topic.Content.Summary = topic.Content.Summary.Replace("<br />", "  ");
            }


            template.Replace("[SUMMARY]", topic.Content.Summary);
            template.Replace("[TOPICSUBSCRIBERCOUNT]", topic.SubscriberCount.ToString());


            if (topic.IsLocked)
            {
                template.Replace("fa-lock", "fa-unlock");
                template.Replace("[RESX:Lock]", "[RESX:UnLock]");
                template.Replace("[RESX:LockTopic]", "[RESX:UnLockTopic]");
                template.Replace("[RESX:Confirm:Lock]", "[RESX:Confirm:UnLock]");
                template.Replace("[ICONLOCK]", string.Format(LocalizeTokenString("[ICONLOCK]-ShowIcon", portalSettings, language), navigationManager.NavigateURL(TabId), topic.TopicId.ToString()));
            }
            else
            {
                template.Replace("[ICONLOCK]", string.Format(LocalizeTokenString("[ICONLOCK]-HideIcon", portalSettings, language), navigationManager.NavigateURL(TabId), topic.TopicId.ToString()));
            }

            if (topic.IsPinned)
            {
                template.Replace("dcf-topic-pin-pin", "dcf-topic-pin-unpin");
                template.Replace("[RESX:Pin]", "[RESX:UnPin]");
                template.Replace("[RESX:PinTopic]", "[RESX:UnPinTopic]");
                template.Replace("[RESX:Confirm:Pin]", "[RESX:Confirm:UnPin]");
                template.Replace("[ICONPIN]", string.Format(LocalizeTokenString("[ICONPIN]-HideIcon", portalSettings, language), navigationManager.NavigateURL(TabId), topic.TopicId.ToString()));
            }
            else
            {
                template.Replace("[ICONPIN]", string.Format(LocalizeTokenString("[ICONPIN]-HideIcon", portalSettings, language), navigationManager.NavigateURL(TabId), topic.TopicId.ToString()));
            }


            string sTopicURL = string.Empty;
            sTopicURL = new ControlUtils().BuildUrl(TabId, ForumModuleId, topic.Forum.ForumGroup.PrefixURL, topic.Forum.PrefixURL, topic.Forum.ForumGroupId, topic.Forum.ForumID, topic.TopicId, topic.TopicUrl, -1, -1, string.Empty, 1, -1, topic.Forum.SocialGroupId);

            var @params = new List<string> { ParamKeys.TopicId + "=" + topic.TopicId, ParamKeys.ContentJumpId + "=" + topic.LastReplyId };

            if (topic.Forum.SocialGroupId > 0)
                @params.Add($"{Literals.GroupId}={topic.Forum.SocialGroupId}");

            string sLastReplyURL = navigationManager.NavigateURL(TabId, "", @params.ToArray());

            if (!(string.IsNullOrEmpty(sTopicURL)))
            {
                if (sTopicURL.EndsWith("/"))
                {
                    sLastReplyURL = sTopicURL + (Utilities.UseFriendlyURLs(ForumModuleId) ? String.Concat("#", topic.TopicId) : String.Concat("?", ParamKeys.ContentJumpId, "=", topic.LastReplyId));
                }
            }
            string sLastReadURL = string.Empty;
            string sUserJumpUrl = string.Empty;
            if (UserLastReplyRead > 0)
            {
                @params = new List<string> { $"{ParamKeys.ForumId}={topic.ForumId}", $"{ParamKeys.TopicId}={topic.TopicId}", $"{ParamKeys.ViewType}={Views.Topic}", $"{ParamKeys.FirstNewPost}={UserLastReplyRead}" };
                if (topic.Forum.SocialGroupId > 0)
                    @params.Add($"{Literals.GroupId}={topic.Forum.SocialGroupId}");

                sLastReadURL = navigationManager.NavigateURL(TabId, "", @params.ToArray());

                if (mainSettings.UseShortUrls)
                {
                    @params = new List<string> { ParamKeys.TopicId + "=" + topic.TopicId, ParamKeys.FirstNewPost + "=" + UserLastReplyRead };
                    if (topic.Forum.SocialGroupId > 0)
                        @params.Add($"{Literals.GroupId}={topic.Forum.SocialGroupId}");

                    sLastReadURL = navigationManager.NavigateURL(TabId, "", @params.ToArray());

                }
                if (sTopicURL.EndsWith("/"))
                {
                    sLastReadURL = sTopicURL + (Utilities.UseFriendlyURLs(ForumModuleId) ? String.Concat("#", UserLastReplyRead) : String.Concat("?", ParamKeys.FirstNewPost, "=", UserLastReplyRead));
                }
            }

            if (user.Profile.PrefJumpLastPost && sLastReadURL != string.Empty)
            {
                sTopicURL = sLastReadURL;
                sUserJumpUrl = sLastReadURL;
            }

            template.Replace("[AF:ICONLINK:LASTREAD]", "<a href=\"" + sLastReadURL + "\" rel=\"nofollow\"><img src=\"" + mainSettings.ThemeLocation + "/images/miniarrow_down.png\" style=\"vertical-align:middle;\" alt=\"[RESX:JumpToLastRead]\" border=\"0\" class=\"afminiarrow\" /></a>");
            template.Replace("[AF:URL:LASTREAD]", sLastReadURL);



            template.Replace("[AF:ICONLINK:LASTREPLY]", "<a href=\"" + sLastReplyURL + "\" rel=\"nofollow\"><img src=\"" + mainSettings.ThemeLocation + "/images/miniarrow_right.png\" style=\"vertical-align:middle;\" alt=\"[RESX:JumpToLastReply]\" border=\"0\" class=\"afminiarrow\" /></a>");
            template.Replace("[AF:URL:LASTREPLY]", sLastReplyURL);

            template.Replace("[TOPICURL]", sTopicURL);
            topic.Content.Subject = Utilities.StripHTMLTag(topic.Content.Subject);

            string sPollImage = string.Empty;
            if (topic.TopicType == TopicTypes.Poll)
            {
                sPollImage = "&nbsp;<i class=\"fa fa-signal fa-fw fa-red\"></i>";
            }
            template.Replace("[SUBJECT]", topic.Content.Subject + sPollImage);

            if (template.ToString().Contains("[SUBJECTLINK]"))
            {
                string slink = null;
                string subject = HttpUtility.HtmlDecode(topic.Subject);
                subject = Utilities.StripHTMLTag(subject);
                subject = subject.Replace("\"", string.Empty).Replace("#", string.Empty).Replace("%", string.Empty).Replace("+", string.Empty); ;

                if (sTopicURL == string.Empty)
                {
                    string[] Params = { ParamKeys.ForumId + "=" + topic.ForumId, ParamKeys.TopicId + "=" + topic.TopicId, ParamKeys.ViewType + "=" + Views.Topic };
                    if (mainSettings.UseShortUrls)
                    {
                        Params = new string[] { ParamKeys.TopicId + "=" + topic.TopicId };
                    }
                    slink = "<a title=\"" + sBodyTitle + "\" href=\"" + navigationManager.NavigateURL(TabId, "", Params) + "\">" + subject + "</a>";
                }
                else
                {
                    slink = "<a title=\"" + sBodyTitle + "\" href=\"" + sTopicURL + "\">" + subject + "</a>";
                }
                template.Replace("[SUBJECTLINK]", slink + sPollImage);
            }

            template.Replace("[DATECREATED]", Utilities.GetUserFormattedDateTime(topic.Content.DateCreated, portalSettings.PortalId, userInfo.UserID));
            if (template.ToString().Contains("[LASTPOST]"))
            {
                if (topic.LastReply?.ReplyId == 0)
                {
                    template = TemplateUtils.ReplaceSubSection(template, Utilities.GetUserFormattedDateTime(topic.LastReply.Content.DateCreated, portalSettings.PortalId, userInfo.UserID), "[LASTPOST]", "[/LASTPOST]");
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
                    sLastReplyTemp = sLastReplyTemp.Replace("[AF:ICONLINK:LASTREPLY]", "<a href=\"" + sLastReplyURL + "\" rel=\"nofollow\"><img src=\"" + mainSettings.ThemeLocation + "/images/miniarrow_right.png\" style=\"vertical-align:middle;\" alt=\"[RESX:JumpToLastReply]\" border=\"0\" class=\"afminiarrow\" /></a>");
                    sLastReplyTemp = sLastReplyTemp.Replace("[AF:URL:LASTREPLY]", sLastReplyURL);

                    int PageSize = mainSettings.PageSize;
                    if (user.UserId > 0)
                    {
                        PageSize = user.Profile.PrefPageSize;
                    }
                    if (PageSize < 5)
                    {
                        PageSize = 10;
                    }

                    sLastReplyTemp = sLastReplyTemp.Replace(LastReplySubjectReplaceTag, Utilities.GetLastPostSubject(topic.LastReply.ReplyId, topic.TopicId, topic.ForumId, TabId, topic.LastReply.Content.Subject, iLength, pageSize: PageSize, replyCount: topic.ReplyCount, canRead: true));
                    if (topic.LastReply.Author.AuthorId > 0)
                    {
                        sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDISPLAYNAME]", UserProfiles.GetDisplayName(ForumModuleId, true, user.Profile.IsMod, userInfo.IsAdmin || userInfo.IsSuperUser, topic.LastReply.Author.AuthorId, topic.LastReply.Author.Username, topic.LastReply.Author.FirstName, topic.LastReply.Author.LastName, topic.LastReply.Author.DisplayName).ToString().Replace("&amp;#", "&#"));
                    }
                    else
                    {
                        sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDISPLAYNAME]", topic.LastReply.Content.AuthorName);
                    }

                    sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDATE]", Utilities.GetUserFormattedDateTime(topic.LastReply.Content.DateCreated, portalSettings.PortalId, userInfo.UserID));
                    template = TemplateUtils.ReplaceSubSection(template, sLastReplyTemp, "[LASTPOST]", "[/LASTPOST]");
                }



                var displayName = UserProfiles.GetDisplayName(ForumModuleId, true, user.Profile.IsMod, userInfo.IsAdmin || userInfo.IsSuperUser, topic.Author.AuthorId, topic.Content.AuthorName, topic.Author.FirstName, topic.Author.LastName, topic.Author.DisplayName).ToString().Replace("&amp;#", "&#");
                if (Utilities.StripHTMLTag(displayName) == "Anonymous")
                {
                    displayName = displayName.Replace("Anonymous", topic.Content.AuthorName);
                }
                template.Replace("[STARTEDBY]", displayName);





                if (UserLastTopicRead == 0 || (UserLastTopicRead > 0 & UserLastReplyRead < topic.Forum.LastReplyId))
                {
                    template.Replace("[POSTICON]", "<div><i class=\"fa fa-file-o fa-2x fa-red\"></i></div>");
                }
                else
                {
                    template.Replace("[POSTICON]", "<div><i class=\"fa fa-file-o fa-2x fa-grey\"></i></div>");
                }

                if (topic.Rating == 0)
                {
                    template.Replace("[POSTRATINGDISPLAY]", string.Empty);
                }
                else
                {
                    template.Replace("[POSTRATINGDISPLAY]", "<span class=\"fa-rater fa-rate" + topic.Rating.ToString() + "\"><i class=\"fa fa-star1\"></i><i class=\"fa fa-star2\"></i><i class=\"fa fa-star3\"></i><i class=\"fa fa-star4\"></i><i class=\"fa fa-star5\"></i></span>");
                }
                if (template.ToString().Contains("[STATUS]"))
                {
                    if (topic.StatusId == -1)
                    {
                        template.Replace("[STATUS]", string.Empty);
                    }
                    else
                    {
                        template.Replace("[STATUS]", "<div><i class=\"fa fa-status" + topic.StatusId.ToString() + " fa-red fa-2x\"></i></div>");
                    }
                }

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
                int inStart = (template.ToString().IndexOf(tokenPrefix, 0));
                int inEnd = (template.ToString().IndexOf("]", inStart - 1));
                template.Remove(inStart, ((inEnd - inStart) + 1));
            }
            return template;
        }

        internal static StringBuilder ReplaceModuleTokens(StringBuilder template, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, DotNetNuke.Entities.Users.UserInfo userInfo, int TabId, int ForumModuleId)
        {
            // Add This -- obsolete so just remove
            template = RemovePrefixedToken(template, "[AF:CONTROL:ADDTHIS");

            string language = userInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;
            var urlNavigator = new Services.URLNavigator();

            template.Replace("[PORTALID]", portalSettings.PortalId.ToString());
            template.Replace("[PORTALNAME]", portalSettings.PortalName);
            template.Replace("[MODULEID]", ForumModuleId.ToString());
            template.Replace("[TABID]", TabId.ToString());
            template.Replace("[FORUMMAINLINK]", string.Format(LocalizeTokenString("[FORUMMAINLINK]", portalSettings, language), urlNavigator.NavigateURL(TabId)));

            template.Replace("[PAGENAME]", HttpUtility.HtmlEncode(string.IsNullOrEmpty(portalSettings.ActiveTab.Title) ? portalSettings.ActiveTab.TabName : portalSettings.ActiveTab.Title));

            return template;
        }
        internal static StringBuilder ReplaceForumTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, DotNetNuke.Entities.Users.UserInfo userInfo, int TabId, int ForumModuleId, CurrentUserTypes CurrentUserType)
        {
            string language = userInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = (INavigationManager)new Services.URLNavigator();
            }
            var ctlUtils = new ControlUtils();
            string groupUrl = ctlUtils.BuildUrl(TabId, ForumModuleId, forum.ForumGroup.PrefixURL, string.Empty, forum.ForumGroupId, -1, -1, -1, string.Empty, 1, -1, forum.SocialGroupId);
            string forumUrl = ctlUtils.BuildUrl(TabId, ForumModuleId, forum.ForumGroup.PrefixURL, forum.PrefixURL, forum.ForumGroupId, forum.ForumID, -1, -1, string.Empty, 1, -1, forum.SocialGroupId);
            string modLink = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildModeratorUrl(navigationManager, portalSettings, mainSettings, forum);

            template.Replace("[GROUPCOLLAPSE]", DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleOpened(target: $"group{forum.ForumGroupId}", title: Utilities.GetSharedResource("[RESX:ToggleGroup]")));

            template.Replace("[FORUMID]", forum.ForumID.ToString());
            template.Replace("[FORUMGROUPID]", forum.ForumGroupId.ToString());
            template.Replace("[FORUMNAMENOLINK]", forum.ForumName);
            template.Replace("[GROUPNAME]", forum.GroupName);
            template.Replace("[MODLINK]", string.Format(LocalizeTokenString("[MODLINK]", portalSettings, language), modLink, modLink));
            template.Replace("[AF:CONTROL:FORUMID]", forum.ForumID.ToString());
            template.Replace("[AF:CONTROL:FORUMGROUPID]", forum.ForumGroupId.ToString());
            template.Replace("[AF:CONTROL:PARENTFORUMID]", forum.ParentForumId.ToString());
            template.Replace("[FORUMSUBSCRIBERCOUNT]", forum.SubscriberCount.ToString());
            template.Replace("[FORUMGROUPLINK]", string.Format(LocalizeTokenString("[FORUMGROUPLINK]", portalSettings, language), groupUrl, forum.GroupName));
            template.Replace("[FORUMNAME]", string.Format(LocalizeTokenString("[FORUMLINK]", portalSettings, language), forumUrl, forum.ForumName));
            template.Replace("[FORUMLINK]", string.Format(LocalizeTokenString("[FORUMLINK]", portalSettings, language), forumUrl, forum.ForumName));
            template.Replace("[FORUMURL]",
                mainSettings.UseShortUrls
                ? navigationManager?.NavigateURL(TabId.ToString(), new[] { string.Concat(ParamKeys.ForumId, "=", forum.ForumID) })
                : navigationManager?.NavigateURL(TabId.ToString(), new[] { string.Concat(ParamKeys.ForumId, "=", forum.ForumID), string.Concat(ParamKeys.ViewType, "=", Views.Topics) }));
            template.Replace("[PARENTFORUMNAME]", string.IsNullOrEmpty(forum.ParentForumName) ? string.Empty : forum.ParentForumName);

            if (template.ToString().Contains("[PARENTFORUMLINK]"))
            {
                string parentForumLink = LocalizeTokenString("[PARENTFORUMLINK]", portalSettings, language);
                if (forum.ParentForumId > 0)
                {
                    template.Replace(oldValue: "[PARENTFORUMLINK]", string.Format(parentForumLink,
                                                                                  mainSettings.UseShortUrls ?
                                                                                  navigationManager?.NavigateURL(TabId, "", new[] { ParamKeys.ForumId + "=" + forum.ParentForumId }) :
                                                                                  navigationManager?.NavigateURL(TabId, "", new[] { ParamKeys.ViewType + "=" + Views.Topics, ParamKeys.ForumId + "=" + forum.ParentForumId }),
                                                                                  forum.ParentForumName));
                }
                else if (forum.ForumGroupId > 0)
                {
                    template.Replace(oldValue: "[PARENTFORUMLINK]", string.Format(parentForumLink, Utilities.NavigateURL(TabId), forum.GroupName));
                }
            }
            else
            {
                template.Replace("[PARENTFORUMLINK]", string.Empty);
            }
            template.Replace("[FORUMDESCRIPTION]", !string.IsNullOrEmpty(forum.ForumDesc) ? string.Format(LocalizeTokenString("[FORUMDESCRIPTION]", portalSettings, language), forum.ForumDesc) : string.Empty);

            bool canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum.Security.View, userInfo.UserID, forum.PortalId);
            if ((template.ToString().Contains("[FORUMICON]")) || (template.ToString().Contains("[FORUMICONCSS]")) || (template.ToString().Contains("[FORUMICONSM]")))
            {
                string sIcon = TemplateUtils.ShowIcon(canView, forum.ForumID, userInfo.UserID, forum.LastPostDateTime, forum.LastRead, forum.LastPostID);
                if (template.ToString().Contains("[FORUMICON]"))
                {
                    string sIconImage = "<img alt=\"" + forum.ForumName + "\" src=\"" + mainSettings.ThemeLocation + "images/" + sIcon + "\" />";
                    template.Replace("[FORUMICON]", sIconImage);
                }
                else if (template.ToString().Contains("[FORUMICONCSS]") || (template.ToString().Contains("[FORUMICONSM]")))
                {
                    string sFolderCSS = "fa-folder fa-blue";
                    switch (sIcon.ToLower())
                    {
                        case "folder.png":
                            sFolderCSS = "fa-folder fa-blue";
                            break;
                        case "folder_new.png":
                            sFolderCSS = "fa-folder fa-red";
                            break;
                        case "folder_forbidden.png":
                            sFolderCSS = "fa-folder fa-grey";
                            break;
                        case "folder_closed.png":
                            sFolderCSS = "fa-folder-o fa-grey";
                            break;
                    }
                    if (template.ToString().Contains("[FORUMICONCSS]"))
                    {
                        template.Replace("[FORUMICONCSS]", string.Format(LocalizeTokenString("[FORUMICONCSS]", portalSettings, language), sFolderCSS));
                    }
                    if (template.ToString().Contains("[FORUMICONSM]"))
                    {
                        template.Replace("[FORUMICONSM]", string.Format(LocalizeTokenString("[FORUMICONSM]", portalSettings, language), sFolderCSS));
                    }
                }
            }
            var ForumURL = new ControlUtils().BuildUrl(TabId, ForumModuleId, forum.ForumGroup.PrefixURL, forum.PrefixURL, forum.ForumGroupId, forum.ForumID, -1, string.Empty, -1, -1, string.Empty, 1, -1, forum.SocialGroupId);

            template.Replace("[AF:CONTROL:ADDFAVORITE]", "<a href=\"javascript:afAddBookmark('" + forum.ForumName + "','" + ForumURL + "');\"><img src=\"" + mainSettings.ThemeLocation + "images/favorites16_add.png\" border=\"0\" alt=\"[RESX:AddToFavorites]\" /></a>");

            template.Replace("[TOTALTOPICS]", forum.TotalTopics.ToString());
            template.Replace("[TOTALREPLIES]", forum.TotalReplies.ToString());
            template.Replace("[FORUMSUBSCRIBERCOUNT]", forum.SubscriberCount.ToString());

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
                    bool isMod = CurrentUserType == CurrentUserTypes.Admin || CurrentUserType == CurrentUserTypes.ForumMod || CurrentUserType == CurrentUserTypes.SuperUser;
                    bool isAdmin = CurrentUserType == CurrentUserTypes.Admin || CurrentUserType == CurrentUserTypes.SuperUser;
                    template.Replace("[DISPLAYNAME]", string.Format(LocalizeTokenString("[DISPLAYNAME]", portalSettings, language), UserProfiles.GetDisplayName(ForumModuleId, true, isMod, isAdmin, forum.LastPostUserID, forum.LastPostUserName, forum.LastPostFirstName, forum.LastPostLastName, forum.LastPostDisplayName)));
                }
            }
            if (template.ToString().Contains("[LASTPOSTDATE]"))
            {
                template.Replace("[LASTPOSTDATE]", Utilities.GetUserFormattedDateTime(forum.LastPostDateTime, forum.PortalId, userInfo == null ? -1 : userInfo.UserID));
            }
            return template;
        }

        private static string GetTopicTitle(string Body, int AuthorId)
        {
                Body = HttpUtility.HtmlDecode(Body);
                Body = Body.Replace("<br>", System.Environment.NewLine);
                Body = Utilities.StripHTMLTag(Body);
                Body = Body.Length > 500 ? Body.Substring(0, 500) + "..." : Body;
                Body = Body.Replace("\"", "'");
                Body = Body.Replace("?", string.Empty);
                Body = Body.Replace("+", string.Empty);
                Body = Body.Replace("%", string.Empty);
                Body = Body.Replace("#", string.Empty);
                Body = Body.Replace("@", string.Empty);
                return Body.Trim();
           

        }
    }
}

