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

using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Web.UI.WebControls;

namespace DotNetNuke.Modules.ActiveForums.Services.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Linq;
    using System.Text;
    using System;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Services.Log.EventLog;
    using System.Web;
    using DotNetNuke.Services.Tokens;
    using System.Net;
    using Newtonsoft.Json;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Modules.ActiveForums.ViewModels;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Common.Utilities;
    using System.Web.Services.Description;

    internal class TokenReplacer : DotNetNuke.Services.Tokens.BaseCustomTokenReplace
    {
        private const string PropertySource_resx = "resx";
        private const string PropertySource_forumgroup = "forumgroup";
        private const string PropertySource_forum = "forum";
        private const string PropertySource_forumuser = "forumuser";
        private const string PropertySource_user = "user";
        private const string PropertySource_profile = "profile";
        private const string PropertySource_membership = "membership";
        private const string PropertySource_tab = "tab";
        private const string PropertySource_module = "module";
        private const string PropertySource_portal = "portal";
        private const string PropertySource_host = "host";
        private const string PropertySource_forumtopic = "forumtopic";
        private const string PropertySource_forumpost = "forumpost";
        private const string PropertySource_forumtopicaction = "forumtopicaction";
        private const string PropertySource_forumpostaction = "forumpostaction";


        //private static string[] GetObsoleteTokens()
        //{
        //    try
        //    {
        //        string cachekey = CacheKeys.ObsoleteTokens;
        //        var obsoleteTokens = (string[])DataCache.SettingsCacheRetrieve(-1, cachekey);
        //        if (obsoleteTokens == null)
        //        {
        //            obsoleteTokens = new string[] { };
        //            string sPath = DotNetNuke.Modules.ActiveForums.Utilities.MapPath(Globals.ModulePath + "config/LegacyTokenReplacement.json");
        //            using (StreamReader r = new StreamReader(sPath))
        //            {
        //                string json = r.ReadToEnd();
        //                LegacyTokenReplacements config = Newtonsoft.Json.JsonConvert.DeserializeObject<LegacyTokenReplacements>(json);
        //                obsoleteTokens = config.ObsoleteTokens;
        //                r.Close();
        //            }

        //            DataCache.SettingsCacheStore(-1, cachekey, obsoleteTokens);
        //        }

        //        return obsoleteTokens;
        //    }
        //    catch (Exception ex)
        //    {
        //        Exceptions.LogException(ex);
        //        return new string[] { };
        //    }
        //}

        public TokenReplacer(DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo)
        {
            this.PropertySource[key: PropertySource_resx] = new DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer();
            this.PropertySource[key: PropertySource_forum] = forumInfo;
            this.PropertySource[key: PropertySource_forumgroup] = forumInfo.ForumGroup;
            this.PropertySource[key: PropertySource_forumuser] = forumUser;
            this.PropertySource[key: PropertySource_user] = forumUser.UserInfo;
            this.PropertySource[key: PropertySource_profile] = new ProfilePropertyAccess(forumUser.UserInfo);
            this.PropertySource[key: PropertySource_membership] = new MembershipPropertyAccess(forumUser.UserInfo);
            this.PropertySource[key: PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[key: PropertySource_module] = forumInfo.ModuleInfo;
            this.PropertySource[key: PropertySource_portal] = portalSettings;
            this.PropertySource[key: PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public TokenReplacer(DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroupInfo)
        {
            this.PropertySource[key: PropertySource_resx] = new DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer();
            this.PropertySource[key: PropertySource_forumgroup] = forumGroupInfo;
            this.PropertySource[key: PropertySource_forumuser] = forumUser;
            this.PropertySource[key: PropertySource_user] = forumUser.UserInfo;
            this.PropertySource[key: PropertySource_profile] = new ProfilePropertyAccess(forumUser.UserInfo);
            this.PropertySource[key: PropertySource_membership] = new MembershipPropertyAccess(forumUser.UserInfo);
            this.PropertySource[key: PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[key: PropertySource_module] = forumGroupInfo.ModuleInfo;
            this.PropertySource[key: PropertySource_portal] = portalSettings;
            this.PropertySource[key: PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public TokenReplacer(DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topicInfo)
        {
            this.PropertySource[key: PropertySource_resx] = new DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer();
            this.PropertySource[key: PropertySource_forum] = topicInfo.Forum;
            this.PropertySource[key: PropertySource_forumgroup] = topicInfo.Forum.ForumGroup;
            this.PropertySource[key: PropertySource_forumtopic] = topicInfo;
            this.PropertySource[key: PropertySource_forumtopicaction] = topicInfo;
            this.PropertySource[key: PropertySource_forumuser] = topicInfo.Author.ForumUser;
            this.PropertySource[key: PropertySource_user] = topicInfo.Author.ForumUser.UserInfo;
            this.PropertySource[key: PropertySource_profile] = new ProfilePropertyAccess(topicInfo.Author.ForumUser.UserInfo);
            this.PropertySource[key: PropertySource_membership] = new MembershipPropertyAccess(topicInfo.Author.ForumUser.UserInfo);
            this.PropertySource[key: PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[key: PropertySource_module] = topicInfo.Forum.ModuleInfo;
            this.PropertySource[key: PropertySource_portal] = portalSettings;
            this.PropertySource[key: PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public TokenReplacer(DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.IPostInfo postInfo)
        {
            this.PropertySource[key: PropertySource_resx] = new DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer();
            this.PropertySource[key: PropertySource_forum] = postInfo.Forum;
            this.PropertySource[key: PropertySource_forumgroup] = postInfo.Forum.ForumGroup;
            this.PropertySource[key: PropertySource_forumtopic] = postInfo.Topic;
            this.PropertySource[key: PropertySource_forumpost] = postInfo;
            this.PropertySource[key: PropertySource_forumpostaction] = postInfo;
            this.PropertySource[key: PropertySource_forumuser] = postInfo.Author.ForumUser;
            this.PropertySource[key: PropertySource_user] = postInfo.Author.ForumUser.UserInfo;
            this.PropertySource[key: PropertySource_profile] = new ProfilePropertyAccess(postInfo.Author.ForumUser.UserInfo);
            this.PropertySource[key: PropertySource_membership] = new MembershipPropertyAccess(postInfo.Author.ForumUser.UserInfo);
            this.PropertySource[key: PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[key: PropertySource_module] = postInfo.Forum.ModuleInfo;
            this.PropertySource[key: PropertySource_portal] = portalSettings;
            this.PropertySource[key: PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public TokenReplacer(DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser)
        {
            this.PropertySource[key: PropertySource_resx] = new DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer();
            this.PropertySource[key: PropertySource_forumuser] = forumUser;
            this.PropertySource[key: PropertySource_user] = forumUser.UserInfo;
            this.PropertySource[key: PropertySource_profile] = new ProfilePropertyAccess(forumUser.UserInfo);
            this.PropertySource[key: PropertySource_membership] = new MembershipPropertyAccess(forumUser.UserInfo);
            this.PropertySource[key: PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[key: PropertySource_module] = forumUser.ModuleInfo;
            this.PropertySource[key: PropertySource_portal] = portalSettings;
            this.PropertySource[key: PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public TokenReplacer(DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author)
        {
            this.PropertySource[key: PropertySource_resx] = new DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer();
            this.PropertySource[key: PropertySource_forumuser] = forumUser;
            this.PropertySource[key: PropertySource_user] = author.ForumUser.UserInfo;
            this.PropertySource[key: PropertySource_profile] = new ProfilePropertyAccess(author.ForumUser.UserInfo);
            this.PropertySource[key: PropertySource_membership] = new MembershipPropertyAccess(author.ForumUser.UserInfo);
            this.PropertySource[key: PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[key: PropertySource_module] = forumUser.ModuleInfo;
            this.PropertySource[key: PropertySource_portal] = portalSettings;
            this.PropertySource[key: PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public new string ReplaceTokens(string source)
        {
            return base.ReplaceTokens(source);
        }

        internal static StringBuilder ReplaceModuleTokens(StringBuilder template, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, ForumUserInfo forumUser, int tabId, int forumModuleId)
        {
            template = RemoveObsoleteTokens(template);
            template = MapLegacyPortalTokenSynonyms(template);
            template = MapLegacyModuleTokenSynonyms(template);

            string language = forumUser?.UserInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;
            var urlNavigator = new Services.URLNavigator();

            template.Replace("[PAGENAME]", HttpUtility.HtmlEncode(string.IsNullOrEmpty(portalSettings.ActiveTab.Title) ? portalSettings.ActiveTab.TabName : portalSettings.ActiveTab.Title));


            template = DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplace = new DotNetNuke.Services.Tokens.TokenReplace
            {
                AccessingUser = forumUser.UserInfo,
                DebugMessages = false,
                PortalSettings = portalSettings,
                ModuleInfo = forumUser.ModuleInfo,
                User = forumUser.UserInfo,
                ModuleId = forumModuleId,
            };
            template = new StringBuilder(tokenReplace.ReplaceEnvironmentTokens(template.ToString()));
            return template;
        }

        internal static StringBuilder ReplaceTopicActionTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, ForumUserInfo forumUser, HttpRequest request, int tabId, bool useListActions = true)
        {
            string language = forumUser?.UserInfo?.Profile.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = (INavigationManager)new Services.URLNavigator();
            }

            template = RemoveObsoleteTokens(template);
            template = MapLegacyTopicActionTokenSynonyms(template, forumUser, topic, portalSettings, language, useListActions);

            return template;
        }

        internal static StringBuilder ReplacePostActionTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, ForumUserInfo forumUser, HttpRequest request, int tabId, bool canReply, bool useListActions = true)
        {
            string language = forumUser?.UserInfo?.Profile.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = (INavigationManager)new Services.URLNavigator();
            }

            template = RemoveObsoleteTokens(template);
            template = MapLegacyPostActionTokenSynonyms(template, forumUser, post, portalSettings, language, useListActions);

            template = ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplacer = new TokenReplacer(portalSettings, forumUser, post)
            {
                AccessingUser = forumUser.UserInfo
            };
            template = new StringBuilder(tokenReplacer.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplacer.ReplaceTokens(template.ToString()));

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
            template = MapLegacyForumTokenSynonyms(template, forumUser, forum, portalSettings, language);

            /* if no last post or user can't view via security, or subject missing, remove associated last topic tokens */
            if (forum.LastPostID == 0 ||
                !DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum.Security.View, forumUser.UserRoles) ||
                string.IsNullOrEmpty(HttpUtility.HtmlDecode(forum.LastPostSubject)))
            {
                template = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemovePrefixedToken(template, "[FORUM:LASTPOSTSUBJECT");
                template = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemovePrefixedToken(template, "[FORUM:LASTPOSTDISPLAYNAME");
                template = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemovePrefixedToken(template, "[FORUM:LASTPOSTAUTHORDISPLAYNAME");
                template = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemovePrefixedToken(template, "[FORUM:LASTPOSTAUTHORDISPLAYNAMELINK");
                template = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemovePrefixedToken(template, "[FORUM:LASTPOSTDATE");
                template.Replace("[AF:CONTROL:ADDFAVORITE]", string.Empty);
                template.Replace("[RESX:BY]", string.Empty);
            }

            if (template.ToString().Contains("[AF:CONTROL:ADDFAVORITE]"))
            {
                string forumUrl = new ControlUtils().BuildUrl(forum.PortalId, tabId, forum.ModuleId, forum.ForumGroup.PrefixURL, forum.PrefixURL, forum.ForumGroupId, forum.ForumID, -1, -1, string.Empty, 1, -1, forum.SocialGroupId);
                template.Replace("[AF:CONTROL:ADDFAVORITE]", "<a href=\"javascript:afAddBookmark('" + forum.ForumName + "','" + forumUrl + "');\"><img src=\"" + mainSettings.ThemeLocation + "images/favorites16_add.png\" border=\"0\" alt=\"[RESX:AddToFavorites]\" /></a>");
            }

            template = DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(portalSettings, forumUser, forum)
            {
                CurrentAccessLevel = Scope.DefaultSettings,
                AccessingUser = forumUser.UserInfo,
            };
            template = new StringBuilder(tokenReplacer.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplacer.ReplaceTokens(template.ToString()));

            return template;
        }

        internal static StringBuilder ReplaceForumGroupTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroup, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, ForumUserInfo forumUser, HttpRequest request, int tabId, CurrentUserTypes currentUserType)
        {
            string language = forumUser?.UserInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = new Services.URLNavigator().NavigationManager();
            }

            template = RemoveObsoleteTokens(template);
            template = MapLegacyForumGroupTokenSynonyms(template, forumUser, forumGroup, portalSettings, language);

            if (template.ToString().Contains("[GROUPCOLLAPSE]"))
            {
                template.Replace("[GROUPCOLLAPSE]", DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleOpened(target: $"group{forumGroup.ForumGroupId}", title: Utilities.GetSharedResource("[RESX:ToggleGroup]")));
            }

            template = DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(portalSettings, forumUser, forumGroup)
            {
                CurrentAccessLevel = Scope.DefaultSettings,
                AccessingUser = forumUser.UserInfo,
            };
            template = new StringBuilder(tokenReplacer.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplacer.ReplaceTokens(template.ToString()));

            return template;
        }

        internal static StringBuilder ReplaceUserTokens(StringBuilder template, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, ForumUserInfo accessingUser, int forumModuleId)
        {
            string language = accessingUser?.UserInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;
            string dateFormat = Globals.DefaultDateFormat;

            template = RemoveObsoleteTokens(template);
            template = MapLegacyUserTokenSynonyms(template, forumUser, portalSettings, language);

            template = DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(portalSettings, forumUser)
            {
                CurrentAccessLevel = Scope.DefaultSettings,
                AccessingUser = forumUser.UserInfo,
            };
            template = new StringBuilder(tokenReplacer.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplacer.ReplaceTokens(template.ToString()));

            return template;
        }

        internal static StringBuilder ReplaceTopicTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, HttpRequest request)
        {
            string language = forumUser?.UserInfo?.Profile.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = (INavigationManager)new Services.URLNavigator();
            }

            var @params = new List<string>();

            template = RemoveObsoleteTokens(template);
            template = MapLegacyTopicTokenSynonyms(template, forumUser, topic, portalSettings, language);

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



            //// replace tokens unique to topic itself
            //template.Replace("[REPLIES]", topic.ReplyCount.ToString());

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

                string sLastReplyURL = navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, string.Empty, @params.ToArray());
                string sTopicURL = new ControlUtils().BuildUrl(portalSettings.PortalId, portalSettings.ActiveTab.TabID, topic.Forum.ModuleId, topic.Forum.ForumGroup.PrefixURL, topic.Forum.PrefixURL, topic.Forum.ForumGroupId, topic.Forum.ForumID, topic.TopicId, topic.TopicUrl, -1, -1, string.Empty, 1, -1, topic.Forum.SocialGroupId);
                if (!(string.IsNullOrEmpty(sTopicURL)))
                {
                    if (sTopicURL.EndsWith("/"))
                    {
                        sLastReplyURL = sTopicURL + (Utilities.UseFriendlyURLs(topic.Forum.ModuleId)
                            ? string.Concat("#", topic.TopicId)
                            : string.Concat("?", ParamKeys.ContentJumpId, "=", topic.LastReplyId));
                    }
                }

                template.Replace("[AF:ICONLINK:LASTREPLY]", string.Format(GetTokenFormatString("[ICONLINK-LASTREPLY]", portalSettings, language), sLastReplyURL, mainSettings.ThemeLocation));
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

                    sLastReadURL = navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, string.Empty, @params.ToArray());

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

                        sLastReadURL = navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, string.Empty, @params.ToArray());
                    }

                    if (sTopicURL.EndsWith("/"))
                    {
                        sLastReadURL = sTopicURL + (Utilities.UseFriendlyURLs(topic.Forum.ModuleId)
                            ? string.Concat("#", userLastReplyRead)
                            : string.Concat("?", ParamKeys.FirstNewPost, "=", userLastReplyRead));
                    }
                }

                template.Replace("[AF:ICONLINK:LASTREAD]", string.Format(GetTokenFormatString("[ICONLINK-LASTREAD]", portalSettings, language), sLastReadURL, mainSettings.ThemeLocation));
                template.Replace("[AF:URL:LASTREAD]", sLastReadURL);

                if (forumUser.PrefJumpLastPost && sLastReadURL != string.Empty)
                {
                    sTopicURL = sLastReadURL;
                }

                if (template.ToString().Contains("[SUBJECT]") || template.ToString().Contains("[SUBJECTLINK]"))
                {
                    string sPollImage = (topic.TopicType == TopicTypes.Poll ? GetTokenFormatString("[POSTICON]", portalSettings, language) : string.Empty);
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

                            slink = "<a title=\"" + sBodyTitle + "\" href=\"" + navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, string.Empty, @params.ToArray()) + "\">" + subject + "</a>";
                        }
                        else
                        {
                            slink = "<a title=\"" + sBodyTitle + "\" href=\"" + sTopicURL + "\">" + subject + "</a>";
                        }

                        template.Replace("[SUBJECTLINK]", slink + sPollImage);
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

                        sLastReplyTemp = sLastReplyTemp.Replace("[AF:ICONLINK:LASTREPLY]", string.Format(GetTokenFormatString("[ICONLINK-LASTREPLY]", portalSettings, language), sLastReplyURL, mainSettings.ThemeLocation));
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

                        sLastReplyTemp = sLastReplyTemp.Replace(LastReplySubjectReplaceTag, Utilities.GetLastPostSubject(topic.LastReply.ReplyId, topic.TopicId, topic.ForumId, portalSettings.ActiveTab.TabID, topic.LastReply.Content.Subject, iLength, pageSize: PageSize, replyCount: topic.ReplyCount, canRead: true));
                        if (template.ToString().Contains("[LASTPOSTDISPLAYNAME]"))
                        {
                            if (topic.LastReply.Author.AuthorId > 0)
                            {
                                sLastReplyTemp = sLastReplyTemp.Replace("[LASTPOSTDISPLAYNAME]", DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, mainSettings, forumUser.GetIsMod(topic.ModuleId), forumUser.IsAdmin || forumUser.IsSuperUser, topic.LastReply.Author.AuthorId, topic.LastReply.Author.Username, topic.LastReply.Author.FirstName, topic.LastReply.Author.LastName, topic.LastReply.Author.DisplayName).ToString().Replace("&amp;#", "&#"));
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

            template = DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplace = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(portalSettings, forumUser, topic)
            {
                AccessingUser = forumUser.UserInfo
            };
            template = new StringBuilder(tokenReplace.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplace.ReplaceTokens(template.ToString()));

            return template;
        }

        internal static StringBuilder ReplacePostTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post, DotNetNuke.Entities.Portals.PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, HttpRequest request)
        {
            string language = forumUser?.UserInfo?.Profile.PreferredLocale ?? portalSettings?.DefaultLanguage;

            if (navigationManager == null)
            {
                navigationManager = (INavigationManager)new Services.URLNavigator();
            }

            template = RemoveObsoleteTokens(template);
            template = MapLegacyPostTokenSynonyms(template, forumUser, post, portalSettings, language);

            // Perform Profile Related replacements
            var author = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(post.PortalId, post.Forum.ModuleId, post.Author.AuthorId);
            if (template.ToString().Contains("[POSTINFO]"))
            {
                var sPostInfo = TemplateUtils.GetPostInfo(post.ModuleId, author.ForumUser, post.Forum.ThemeLocation, post.Forum.GetIsMod(forumUser), post.Content.IPAddress, author.ForumUser.IsUserOnline, forumUser.CurrentUserType, forumUser.UserId, forumUser.PrefBlockAvatars, forumUser.UserInfo.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                template.Replace("[POSTINFO]", sPostInfo);
            }
            template = ReplaceBody(template, post.Content, mainSettings, request.Url);

            template = DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(portalSettings, forumUser, post)
            {
                AccessingUser = forumUser.UserInfo
            };
            template = new StringBuilder(tokenReplacer.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplacer.ReplaceTokens(template.ToString()));

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

        internal static StringBuilder ReplaceDateToken(StringBuilder template, string token, DateTime datetime, PortalSettings portalSettings, ForumUserInfo accessingUser, string dateFormat = "g")
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

            //var tokens = GetObsoleteTokens();
            if (tokens.Length > 0)
            {
                tokens.ToList().ForEach(t => template = RemoveObsoleteToken(template, t));
            }

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
                "[FORUMTOPIC:PREVIOUSTOPICLINK]",
                "[FORUMTOPIC:NEXTTOPICLINK]",
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
            template = ReplaceTokenSynonym(template, "[PORTALID]", "[PORTAL:PORTALID]");
            template = ReplaceTokenSynonym(template, "[PORTALNAME]", "[PORTAL:PORTALNAME]");
            return template;
        }

        internal static StringBuilder MapLegacyModuleTokenSynonyms(StringBuilder template)
        {
            template = ReplaceTokenSynonym(template, "[MODULEID]", "[MODULE:MODULEID]");
            template = ReplaceTokenSynonym(template, "[TABID]", "[TAB:TABID]");
            return template;
        }

        internal static StringBuilder MapLegacyForumTokenSynonyms(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum, DotNetNuke.Entities.Portals.PortalSettings portalSettings, string language)
        {
            template = ReplaceTokenSynonym(template, "[FORUMID]", "[FORUM:FORUMID]");
            template = ReplaceTokenSynonym(template, "[FORUMGROUPID]", "[FORUMGROUP:FORUMGROUPID]");
            template = ReplaceTokenSynonym(template, "[PARENTFORUMID]", "[FORUM:PARENTFORUMID]");
            template = ReplaceTokenSynonym(template, "[PARENTFORUMNAME]", "[FORUM:PARENTFORUMNAME]");
            template = ReplaceTokenSynonym(template, "[FORUMSUBSCRIBERCOUNT]", "[FORUM:SUBSCRIBERCOUNT]");
            template = ReplaceTokenSynonym(template, "[TOTALREPLIES]", "[FORUM:TOTALREPLIES]");
            template = ReplaceTokenSynonym(template, "[TOTALTOPICS]", "[FORUM:TOTALTOPICS]");
            template = ReplaceTokenSynonym(template, "[FORUMNAMENOLINK]", "[FORUM:FORUMNAME]");
            template = ReplaceTokenSynonym(template, "[AF:CONTROL:FORUMID]", "[FORUM:FORUMID]");
            template = ReplaceTokenSynonym(template, "[AF:CONTROL:FORUMGROUPID]", "[FORUM:FORUMGROUPID]");
            template = ReplaceTokenSynonym(template, "[LASTPOSTDATE]", "[FORUM:LASTPOSTDATE]");

            template = ReplaceTokenSynonymPrefix(template, "[LASTPOSTSUBJECT", "[FORUM:LASTPOSTSUBJECT");
            template = ReplaceTokenSynonymPrefix(template, "[FORUM:GROUPLINK", "[FORUMGROUP:FORUMGROUPLINK");

            var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(portalSettings, forumUser, forum)
            {
                AccessingUser = forumUser.UserInfo
            };
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[RSSLINK]", "[FORUM:RSSLINK", "[RSSLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[PARENTFORUMLINK]", "[FORUM:PARENTFORUMLINK", "[PARENTFORUMLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[FORUMDESCRIPTION]", "[FORUM:FORUMDESCRIPTION", "[FORUMDESCRIPTION]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[FORUMICON]", "[FORUM:FORUMICON", "[FORUMICON]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[MODLINK]", "[FORUM:MODLINK", "[MODLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[GROUPNAME]", "[FORUMGROUP:FORUMGROUPLINK", "[FORUMGROUPLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[FORUMGROUPLINK]", "[FORUMGROUP:FORUMGROUPLINK", "[FORUMGROUPLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[FORUMMAINLINK]", "[FORUM:FORUMMAINLINK", "[FORUMMAINLINK]");

            /* note: this purposely uses same token format string [FORUMLINK] for both [FORUMNAME] and [FORUMLINK] */
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[FORUMNAME]", "[FORUM:FORUMLINK", "[FORUMLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[FORUMLINK]", "[FORUM:FORUMLINK", "[FORUMLINK]");

            /* note: these purposely use same target token but with different format string resource keys */
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[FORUMICONCSS]", "[FORUM:FORUMICONCSS", "[FORUMICONCSS]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[FORUMICONSM]", "[FORUM:FORUMICONCSS", "[FORUMICONSM]");

            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[DISPLAYNAME]", "[FORUM:LASTPOSTAUTHORDISPLAYNAMELINK", "[FORUMLASTPOSTAUTHORDISPLAYNAMELINK]", "[FORUM:LASTPOSTDISPLAYNAME]");

            return template;
        }

        internal static StringBuilder MapLegacyForumGroupTokenSynonyms(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroup, DotNetNuke.Entities.Portals.PortalSettings portalSettings, string language)
        {
            template = ReplaceTokenSynonym(template, "[FORUMGROUPID]", "[FORUMGROUP:FORUMGROUPID]");
            template = ReplaceTokenSynonym(template, "[FORUM:GROUPNAME]", "[FORUMGROUP:FORUMGROUPNAME]");

            template = ReplaceTokenSynonymPrefix(template, "[FORUM:GROUPLINK", "[FORUMGROUP:FORUMGROUPLINK");

            var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(portalSettings, forumUser, forumGroup)
            {
                AccessingUser = forumUser.UserInfo
            };
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[GROUPNAME]", "[FORUMGROUP:FORUMGROUPLINK", "[FORUMGROUPLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[FORUM:GROUPLINK]", "[FORUMGROUP:FORUMGROUPLINK", "[FORUMGROUPLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[FORUMGROUPLINK]", "[FORUMGROUP:FORUMGROUPLINK", "[FORUMGROUPLINK]");
            return template;
        }

        internal static StringBuilder MapLegacyUserTokenSynonyms(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Entities.Portals.PortalSettings portalSettings, string language)
        {
            template = ReplaceTokenSynonym(template, "[SENDERUSERNAME]", "[USER:USERNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERFIRSTNAME", "[USER:FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERLASTNAME]", "[USER:LASTNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERDISPLAYNAME]", "[USER:DISPLAYNAME]");
            
            template = ReplaceTokenSynonym(template, "[USERNAME]", "[USER:USERNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERNAME]", "[USER:USERNAME]");

            template = ReplaceTokenSynonym(template, "[USERID]", "[USER:USERID]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERID]", "[USER:USERID]");

            template = ReplaceTokenSynonym(template, "[EMAIL]", "[USER:EMAIL]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:EMAIL]", "[USER:EMAIL]");

            template = ReplaceTokenSynonym(template, "[FIRSTNAME]", "[USER:FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:FIRSTNAME]", "[USER:FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[LASTNAME]", "[USER:LASTNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:LASTNAME]", "[USER:FIRSTNAME]");

            template = ReplaceTokenSynonym(template, "[SIGNATURE]", "[FORUMUSER:SIGNATURE]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:SIGNATURE]", "[FORUMUSER:SIGNATURE]");

            template = ReplaceTokenSynonym(template, "[AVATAR]", "[FORUMUSER:AVATAR]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:AVATAR]", "[FORUMUSER:AVATAR]");

            template = ReplaceTokenSynonym(template, "[RANKNAME]", "[FORUMUSER:RANKNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:RANKNAME]", "[FORUMUSER:RANKNAME]");

            template = ReplaceTokenSynonym(template, "[RANKDISPLAY]", "[FORUMUSER:RANKDISPLAY]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:RANKDISPLAY]", "[FORUMUSER:RANKDISPLAY]");

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

            var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(portalSettings, forumUser)
            {
                AccessingUser = forumUser.UserInfo
            };
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[DISPLAYNAME]", "[FORUMUSER:USERPROFILELINK", "[DISPLAYNAMELINK]", "[FORUMUSER:DISPLAYNAME]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[AF:PROFILE:DISPLAYNAME]", "[FORUMUSER:USERPROFILELINK", "[DISPLAYNAMELINK]", "[FORUMUSER:DISPLAYNAME]");
            return template;
        }

        internal static StringBuilder MapLegacyTopicTokenSynonyms(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic, DotNetNuke.Entities.Portals.PortalSettings portalSettings, string language)
        {
            // replace synonyms
            template = ReplaceTokenSynonym(template, "[AF:LABEL:LastPostDate]", "[FORUMTOPIC:LASTPOSTDATE]");
            template = ReplaceTokenSynonym(template, "[AF:LABEL:TopicDateCreated]", "[FORUMTOPIC:DATECREATED]");
            template = ReplaceTokenSynonym(template, "[TOPICID]", "[FORUMTOPIC:POSTID]");
            template = ReplaceTokenSynonym(template, "[AF:LABEL:ReplyCount]", "[FORUMTOPIC:REPLYCOUNT]");
            template = ReplaceTokenSynonym(template, "[VIEWS]", "[FORUMTOPIC:VIEWCOUNT]");
            template = ReplaceTokenSynonym(template, "[SUMMARY]", "[FORUMTOPIC:SUMMARY]");
            template = ReplaceTokenSynonym(template, "[TOPICSUBSCRIBERCOUNT]", "[FORUMTOPIC:SUBSCRIBERCOUNT]");
            template = ReplaceTokenSynonym(template, "[TOPICSUBJECT]", "[FORUMTOPIC:SUBJECT]");
            template = ReplaceTokenSynonym(template, "[REPLIES]", "[FORUMTOPIC:REPLYCOUNT]");
            template = ReplaceTokenSynonym(template, "[REPLYCOUNT]", "[FORUMTOPIC:REPLYCOUNT]");
            template = ReplaceTokenSynonym(template, "[VIEWCOUNT]", "[FORUMTOPIC:VIEWCOUNT]");
            template = ReplaceTokenSynonym(template, "[TOPICURL]", "[FORUMTOPIC:URL]");

            var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(portalSettings, forumUser, topic)
            {
                AccessingUser = forumUser.UserInfo
            };

            if (template.ToString().Contains("[ICONLOCK]"))
            {
                template.Replace("[ICONLOCK]", "[ICONLOCKED][ICONUNLOCKED]");
                template = ReplaceLegacyTokenWithFormatString(template,
                    tokenReplacer,
                    portalSettings,
                    language,
                    "[ICONLOCKED]",
                    "[FORUMTOPIC:ICONLOCKED",
                    "[ICONLOCK]-ShowIcon");
                template = ReplaceLegacyTokenWithFormatString(template,
                    tokenReplacer,
                    portalSettings,
                    language,
                    "[ICONUNLOCKED]",
                    "[FORUMTOPIC:ICONUNLOCKED",
                    "[ICONLOCK]-HideIcon");
            }

            if (template.ToString().Contains("[ICONPIN]"))
            {
                template.Replace("[ICONPIN]", "[ICONPINNED][ICONUNPINNED]");
                template = ReplaceLegacyTokenWithFormatString(template,
                    tokenReplacer,
                    portalSettings,
                    language,
                    "[ICONPINNED]",
                    "[FORUMTOPIC:ICONPINNED",
                    "[ICONPIN]-ShowIcon");
                template = ReplaceLegacyTokenWithFormatString(template,
                    tokenReplacer,
                    portalSettings,
                    language,
                    "[ICONUNPINNED]",
                    "[FORUMTOPIC:ICONUNPINNED",
                    "[ICONPIN]-HideIcon");
            }

            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[POSTICON]", "[FORUMTOPIC:POSTICON", "[POSTICON]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[POSTRATINGDISPLAY]", "[FORUMTOPIC:RATING", "[POSTRATING]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[STATUS]", "[FORUMTOPIC:STATUS", "[TOPICSTATUS]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[STATUS]", "[FORUMTOPIC:STATUS", "[TOPICSTATUS]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[NEXTTOPIC]", "[FORUMTOPIC:NEXTTOPICLINK", "[NEXTTOPICLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[PREVTOPIC]", "[FORUMTOPIC:PREVIOUSTOPICLINK", "[PREVTOPICLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[STARTEDBY]", "[FORUMTOPIC:AUTHORDISPLAYNAMELINK", "[TOPICAUTHORDISPLAYNAMELINK]", "[FORUMTOPIC:AUTHORDISPLAYNAME]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[LASTPOSTDISPLAYNAME]", "[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAMELINK", "[TOPICLASTPOSTAUTHORDISPLAYNAMELINK]", "[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAME]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[AF:LABEL:TopicAuthor]", "[FORUMTOPIC:AUTHORDISPLAYNAMELINK", "[TOPICAUTHORDISPLAYNAMELINK]", "[FORUMTOPIC:AUTHORDISPLAYNAME]");
            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[AF:LABEL:LastPostAuthor]", "[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAMELINK", "[TOPICLASTPOSTAUTHORDISPLAYNAMELINK]", "[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAME]");

            return template;
        }

        internal static StringBuilder MapLegacyPostActionTokenSynonyms(StringBuilder template,
            ForumUserInfo forumUser,
            IPostInfo post,
            PortalSettings portalSettings,
            string language,
            bool useListActions)
        {
            var tokenReplacer =
                new TokenReplacer(portalSettings, forumUser, post) { AccessingUser = forumUser.UserInfo };

            if (template.ToString().Contains("[ACTIONS:LOCK]"))
            {
                template.Replace("[ACTIONS:LOCK]", "[ACTIONS:LOCK][ACTIONS:UNLOCK]");
            }
            
            if (template.ToString().Contains("[ACTIONS:PIN]"))
            {
                template.Replace("[ACTIONS:PIN]", "[ACTIONS:PIN][ACTIONS:UNPIN]");
            }
            if (template.ToString().Contains("[ACTIONS:ANSWER]"))
            {
                template.Replace("[ACTIONS:ANSWER]", "[ACTIONS:MARKANSWER][FORUMPOST:SELECTEDANSWER]");
            }

            foreach (var action in new string[] { "EDIT", "MOVE", "DELETE", "LOCK", "BAN", "UNLOCK", "PIN", "UNPIN", "ALERT", "REPLY", "QUOTE", "MARKANSWER" })
            {
                if (template.ToString().Contains($"[ACTIONS:{action}]"))
                {
                    template = ReplaceLegacyTokenWithFormatString(
                        template,
                        tokenReplacer,
                        portalSettings,
                        language,
                        $"[ACTIONS:{action}]",
                        $"[FORUMPOST:ACTION{action}ONCLICK",
                        useListActions ? $"[FORUMPOST:ACTIONS:{action}:LISTITEM]" : $"[FORUMPOST:ACTIONS:{action}:HYPERLINK]");
                }
            }

            return template;
        }

        internal static StringBuilder MapLegacyPostTokenSynonyms(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser, DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post, DotNetNuke.Entities.Portals.PortalSettings portalSettings, string language)
        {
            template = ReplaceTokenSynonym(template, "[POSTID]", "[FORUMPOST:POSTID]");
            template = ReplaceTokenSynonym(template, "[POSTDATE]", "[FORUMPOST:DATECREATED]");
            template = ReplaceTokenSynonym(template, "[MODEDITDATE]", "[FORUMPOST:MODEDITDATE]");
            template = ReplaceTokenSynonym(template, "[MODIPADDRESS]", "[FORUMPOST:MODIPADDRESS]");
            template = ReplaceTokenSynonym(template, "[REPLYID]", "[FORUMPOST:POSTID]");
            template = ReplaceTokenSynonym(template, "[POSTICONCSS]", "[FORUMPOST:POSTICONCSS]");

            var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(portalSettings, forumUser, post)
            {
                AccessingUser = forumUser.UserInfo
            };

            if (template.ToString().Contains("[ICONLOCK]"))
            {
                template.Replace("[ICONLOCK]", "[ICONLOCKED][ICONUNLOCKED]");
                template = ReplaceLegacyTokenWithFormatString(template,
                    tokenReplacer,
                    portalSettings,
                    language,
                    "[ICONLOCKED]",
                    "[FORUMPOST:ICONLOCKED",
                    "[ICONLOCK]-ShowIcon");
                template = ReplaceLegacyTokenWithFormatString(template,
                    tokenReplacer,
                    portalSettings,
                    language,
                    "[ICONUNLOCKED]",
                    "[FORUMPOST:ICONUNLOCKED",
                    "[ICONLOCK]-HideIcon");
            }

            if (template.ToString().Contains("[ICONPIN]"))
            {
                template.Replace("[ICONPIN]", "[ICONPINNED][ICONUNPINNED]");
                template = ReplaceLegacyTokenWithFormatString(template,
                    tokenReplacer,
                    portalSettings,
                    language,
                    "[ICONPINNED]",
                    "[FORUMPOST:ICONPINNED",
                    "[ICONPIN]-ShowIcon");
                template = ReplaceLegacyTokenWithFormatString(template,
                    tokenReplacer,
                    portalSettings,
                    language,
                    "[ICONUNPINNED]",
                    "[FORUMPOST:ICONUNPINNED",
                    "[ICONPIN]-HideIcon");
            }

            template = ReplaceLegacyTokenWithFormatString(template, tokenReplacer, portalSettings, language, "[POSTICON]", "[FORUMPOST:POSTICON", "[POSTICON]");

            return template;
        }

        internal static StringBuilder MapLegacyTopicActionTokenSynonyms(StringBuilder template,
            ForumUserInfo forumUser,
            TopicInfo topic,
            PortalSettings portalSettings,
            string language,
            bool useListActions)
        {
            var tokenReplacer =
                new TokenReplacer(portalSettings, forumUser, topic) { AccessingUser = forumUser.UserInfo };

            if (template.ToString().Contains("[AF:QUICKEDITLINK]"))
            {
                template.Replace("[AF:QUICKEDITLINK]", "[ACTIONS:QUICKEDIT]");
            }

            if (template.ToString().Contains("[ACTIONS:LOCK]"))
            {
                template.Replace("[ACTIONS:LOCK]", "[ACTIONS:LOCK][ACTIONS:UNLOCK]");
            }

            if (template.ToString().Contains("[ACTIONS:PIN]"))
            {
                template.Replace("[ACTIONS:PIN]", "[ACTIONS:PIN][ACTIONS:UNPIN]");
            }

            foreach (var action in new string[] { "EDIT", "QUICKEDIT", "DELETE", "MOVE", "REPLY", "QUOTE", "LOCK", "UNLOCK", "PIN", "UNPIN" })
            {
                if (template.ToString().Contains($"[ACTIONS:{action}]"))
                {
                    template = ReplaceLegacyTokenWithFormatString(
                        template,
                        tokenReplacer,
                        portalSettings,
                        language,
                        $"[ACTIONS:{action}]",
                        $"[FORUMTOPIC:ACTION{action}ONCLICK",
                        useListActions ? $"[FORUMTOPIC:ACTIONS:{action}:LISTITEM]" : $"[FORUMTOPIC:ACTIONS:{action}:HYPERLINK]");
                }
            }
            return template;
        }

        private static StringBuilder ReplaceLegacyTokenWithFormatString(StringBuilder template, TokenReplacer tokenReplacer, DotNetNuke.Entities.Portals.PortalSettings portalSettings, string language, string oldTokenName, string newTokenNamePrefix, string formatStringResourceKey, string replaceWithIfEmpty = "")
        {
            string formatString = GetTokenFormatString(formatStringResourceKey, portalSettings, language);
            formatString = DotNetNuke.Modules.ActiveForums.Services.Tokens.ResourceStringTokenReplacer.ReplaceResourceTokens(new StringBuilder(formatString)).ToString();
            if (tokenReplacer.ContainsTokens(formatString))
            {
                formatString = tokenReplacer.ReplaceTokens(formatString);
            }

            return ReplaceTokenSynonym(template, oldTokenName, $"{newTokenNamePrefix}|{formatString}{(!string.IsNullOrEmpty(replaceWithIfEmpty) ? string.Concat("|", replaceWithIfEmpty) : string.Empty)}]");
        }

        private static StringBuilder ReplaceTokenSynonymPrefix(StringBuilder template, string oldTokenNamePrefix, string newTokenNamePrefix)
        {
            return ReplaceTokenSynonym(template, oldTokenNamePrefix, newTokenNamePrefix);
        }
        
        internal static StringBuilder ReplaceTokenSynonym(StringBuilder template, string fromToken, string toToken)
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
        internal static string ReplaceTokenSynonym(string template, string fromToken, string toToken)
        {
            if (template.Contains(fromToken))
            {
                var log = new LogInfo { LogTypeKey = Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                var message = string.Format(Utilities.GetSharedResource("[RESX:ReplaceObsoleteToken]"), fromToken, toToken);
                log.AddProperty("Message", message);
                LogController.Instance.AddLog(log);
                template = template.Replace(fromToken, toToken);
                return template;
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
            // remove initial [ if passed in with tokenPrefix
            if (tokenPrefix.StartsWith("["))
            {
                tokenPrefix = tokenPrefix.Remove(0, 1);
            }

            // matches [token [embedded token] [embedded token].... ] to remove entire string
            string Pattern = $@"\[{tokenPrefix}(?>\[(?<c>)|[^\[\]]+|\](?<-c>))*(?(c)(?!))\]";
            return new StringBuilder(RegexUtils.GetCachedRegex(Pattern, RegexOptions.Compiled & RegexOptions.IgnorePatternWhitespace, 30).Replace(template.ToString(), string.Empty));

        }

        internal static string GetTokenFormatString(string key, DotNetNuke.Entities.Portals.PortalSettings portalSettings, string language = "en-US")
        {
            string formatString = Utilities.LocalizeString(key, Globals.LegacyTokenResourceFile, (DotNetNuke.Entities.Portals.PortalSettings)portalSettings, language);
            return formatString;
        }

        private static StringBuilder ReplaceBody(StringBuilder template,
            DotNetNuke.Modules.ActiveForums.Entities.ContentInfo content,
            SettingsInfo mainSettings,
            Uri uri)
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

        internal string ReplaceEmbeddedTokens(string source)
        {
            const string Pattern = @"(?<token>(?:(?<text>\[\])|\[(?:(?<object>[^{}\]\[:]+):(?<property>[^\]\[\|]+))(?:\|(?:(?<format>[^\]\[]+)\|(?<ifEmpty>[^\]\[]+))|\|(?:(?<format>[^\|\]\[]+)))?\])|(?<text>\[[^\]\[]+\])|(?<text>[^\]\[]+)\1)";
#if DEBUG
            var sw = new Stopwatch();
#endif
            var matches = RegexUtils.GetCachedRegex(Pattern, RegexOptions.Compiled & RegexOptions.IgnorePatternWhitespace, 30).Matches(source);

            if (matches.Count > 0)
            {
                var sb = new StringBuilder(source);
                foreach (Match match in matches)
                {
                    if (!string.IsNullOrEmpty(match.Groups["token"]?.Value) && match.Groups["object"] != null && this.PropertySource.ContainsKey(match.Groups["object"].Value.ToLowerInvariant()))
                    {
#if DEBUG
                        sw.Restart();
#endif
                        sb.Replace(match.Groups["token"]?.Value, this.ReplaceTokens(match.Groups["token"]?.Value));
#if DEBUG
                        sw.Stop();
                        Debug.WriteLine($"{sw.Elapsed} : TOKEN: {match.Groups["token"]?.Value}");
#endif
                    }
                }

                return sb.ToString();
            }

            return source;
        }
    }
}
