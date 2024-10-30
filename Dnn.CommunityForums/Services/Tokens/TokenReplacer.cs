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

namespace DotNetNuke.Modules.ActiveForums.Services.Tokens
{
    using System;
    using System.Data.SqlTypes;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Tokens;

    internal class TokenReplacer : BaseCustomTokenReplace
    {
        private const string PropertySource_resx = "resx";
        private const string PropertySource_dcf = "dcf";
        private const string PropertySource_forumgroup = "forumgroup";
        private const string PropertySource_forum = "forum";
        private const string PropertySource_forumuser = "forumuser";
        private const string PropertySource_user = "user";
        private const string PropertySource_profile = "profile";
        private const string PropertySource_membership = "membership";
        private const string PropertySource_forumauthor = "forumauthor";
        private const string PropertySource_forumauthoruser = "forumauthoruser";
        private const string PropertySource_forumauthorprofile = "forumauthorprofile";
        private const string PropertySource_forumauthormembership = "forumauthormembership";
        private const string PropertySource_tab = "tab";
        private const string PropertySource_module = "module";
        private const string PropertySource_portal = "portal";
        private const string PropertySource_host = "host";
        private const string PropertySource_forumtopic = "forumtopic";
        private const string PropertySource_forumpost = "forumpost";
        private const string PropertySource_forumtopicaction = "forumtopicaction";
        private const string PropertySource_forumpostaction = "forumpostaction";

        public TokenReplacer(PortalSettings portalSettings, ForumUserInfo forumUser, ForumInfo forumInfo, Uri requestUri, string rawUrl)
        {
            forumInfo.RawUrl = rawUrl;
            forumInfo.ForumGroup.RawUrl = rawUrl;
            forumUser.RawUrl = rawUrl;
            forumInfo.RequestUri = requestUri;
            forumInfo.ForumGroup.RequestUri = requestUri;
            forumUser.RequestUri = requestUri;
            this.PropertySource[PropertySource_resx] = new ResourceStringTokenReplacer();
            this.PropertySource[PropertySource_dcf] = new ForumsModuleTokenReplacer(portalSettings, forumInfo.TabId, forumInfo.ModuleId, portalSettings.ActiveTab.TabID == -1 || portalSettings.ActiveTab.TabID == portalSettings.HomeTabId ? forumInfo.TabId : portalSettings.ActiveTab.TabID, portalSettings.ActiveTab.ModuleID == -1 ? forumInfo.ModuleId : portalSettings.ActiveTab.ModuleID, requestUri, rawUrl);
            this.PropertySource[PropertySource_forum] = forumInfo;
            this.PropertySource[PropertySource_forumgroup] = forumInfo.ForumGroup;
            this.PropertySource[PropertySource_forumuser] = forumUser;
            this.PropertySource[PropertySource_user] = forumUser.UserInfo;
            this.PropertySource[PropertySource_profile] = new ProfilePropertyAccess(forumUser.UserInfo);
            this.PropertySource[PropertySource_membership] = new MembershipPropertyAccess(forumUser.UserInfo);
            this.PropertySource[PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[PropertySource_module] = forumInfo.ModuleInfo;
            this.PropertySource[PropertySource_portal] = portalSettings;
            this.PropertySource[PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public TokenReplacer(PortalSettings portalSettings, ForumUserInfo forumUser, ForumGroupInfo forumGroupInfo, Uri requestUri, string rawUrl)
        {
            forumGroupInfo.RawUrl = rawUrl;
            forumUser.RawUrl = rawUrl;
            forumGroupInfo.RequestUri = requestUri;
            forumUser.RequestUri = requestUri;
            this.PropertySource[PropertySource_resx] = new ResourceStringTokenReplacer();
            this.PropertySource[PropertySource_dcf] = new ForumsModuleTokenReplacer(portalSettings, forumGroupInfo.TabId, forumGroupInfo.ModuleId, portalSettings.ActiveTab.TabID == -1 || portalSettings.ActiveTab.TabID == portalSettings.HomeTabId? forumGroupInfo.TabId : portalSettings.ActiveTab.TabID, portalSettings.ActiveTab.ModuleID == -1 ? forumGroupInfo.ModuleId : portalSettings.ActiveTab.ModuleID, requestUri, rawUrl);
            this.PropertySource[PropertySource_forumgroup] = forumGroupInfo;
            this.PropertySource[PropertySource_forumuser] = forumUser;
            this.PropertySource[PropertySource_user] = forumUser.UserInfo;
            this.PropertySource[PropertySource_profile] = new ProfilePropertyAccess(forumUser.UserInfo);
            this.PropertySource[PropertySource_membership] = new MembershipPropertyAccess(forumUser.UserInfo);
            this.PropertySource[PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[PropertySource_module] = forumGroupInfo.ModuleInfo;
            this.PropertySource[PropertySource_portal] = portalSettings;
            this.PropertySource[PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public TokenReplacer(PortalSettings portalSettings, ForumUserInfo forumUser, TopicInfo topicInfo, Uri requestUri, string rawUrl)
        {
            topicInfo.Forum.RawUrl = rawUrl;
            topicInfo.Forum.ForumGroup.RawUrl = rawUrl;
            topicInfo.RawUrl = rawUrl;
            forumUser.RawUrl = rawUrl;
            topicInfo.Forum.RequestUri = requestUri;
            topicInfo.Forum.ForumGroup.RequestUri = requestUri;
            topicInfo.RequestUri = requestUri;
            forumUser.RequestUri = requestUri;
            this.PropertySource[PropertySource_resx] = new ResourceStringTokenReplacer();
            this.PropertySource[PropertySource_dcf] = new ForumsModuleTokenReplacer(portalSettings, topicInfo.Forum.TabId, topicInfo.Forum.ModuleId, portalSettings.ActiveTab.TabID == -1 || portalSettings.ActiveTab.TabID == portalSettings.HomeTabId ? topicInfo.Forum.TabId : portalSettings.ActiveTab.TabID, portalSettings.ActiveTab.ModuleID == -1 ? topicInfo.Forum.ModuleId : portalSettings.ActiveTab.ModuleID, requestUri, rawUrl);
            this.PropertySource[PropertySource_forum] = topicInfo.Forum;
            this.PropertySource[PropertySource_forumgroup] = topicInfo.Forum.ForumGroup;
            this.PropertySource[PropertySource_forumtopic] = topicInfo;
            this.PropertySource[PropertySource_forumtopicaction] = topicInfo;
            this.PropertySource[PropertySource_forumuser] = forumUser;
            this.PropertySource[PropertySource_user] = forumUser.UserInfo;
            this.PropertySource[PropertySource_profile] = new ProfilePropertyAccess(forumUser.UserInfo);
            this.PropertySource[PropertySource_membership] = new MembershipPropertyAccess(forumUser.UserInfo);
            this.PropertySource[PropertySource_forumauthor] = topicInfo.Author.ForumUser;
            this.PropertySource[PropertySource_forumauthoruser] = topicInfo.Author.ForumUser.UserInfo;
            this.PropertySource[PropertySource_forumauthorprofile] = new ProfilePropertyAccess(topicInfo.Author.ForumUser.UserInfo);
            this.PropertySource[PropertySource_forumauthormembership] = new MembershipPropertyAccess(topicInfo.Author.ForumUser.UserInfo);
            this.PropertySource[PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[PropertySource_module] = topicInfo.Forum.ModuleInfo;
            this.PropertySource[PropertySource_portal] = portalSettings;
            this.PropertySource[PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public TokenReplacer(PortalSettings portalSettings, ForumUserInfo forumUser, IPostInfo postInfo, Uri requestUri, string rawUrl)
        {
            postInfo.Forum.RawUrl = rawUrl;
            postInfo.Forum.ForumGroup.RawUrl = rawUrl;
            postInfo.Topic.RawUrl = rawUrl;
            postInfo.RawUrl = rawUrl;
            forumUser.RawUrl = rawUrl;
            postInfo.Forum.RequestUri = requestUri;
            postInfo.Forum.ForumGroup.RequestUri = requestUri;
            postInfo.Topic.RequestUri = requestUri;
            postInfo.RequestUri = requestUri;
            forumUser.RequestUri = requestUri;
            this.PropertySource[PropertySource_resx] = new ResourceStringTokenReplacer();
            this.PropertySource[PropertySource_dcf] = new ForumsModuleTokenReplacer(portalSettings, postInfo.Forum.TabId, postInfo.Forum.ModuleId, portalSettings.ActiveTab.TabID == -1 || portalSettings.ActiveTab.TabID == portalSettings.HomeTabId ? postInfo.Forum.TabId : portalSettings.ActiveTab.TabID, portalSettings.ActiveTab.ModuleID == -1 ? postInfo.Forum.ModuleId : portalSettings.ActiveTab.ModuleID, requestUri, rawUrl);
            this.PropertySource[PropertySource_forum] = postInfo.Forum;
            this.PropertySource[PropertySource_forumgroup] = postInfo.Forum.ForumGroup;
            this.PropertySource[PropertySource_forumtopic] = postInfo.Topic;
            this.PropertySource[PropertySource_forumpost] = postInfo;
            this.PropertySource[PropertySource_forumpostaction] = postInfo;
            this.PropertySource[PropertySource_forumuser] = forumUser;
            this.PropertySource[PropertySource_user] = forumUser.UserInfo;
            this.PropertySource[PropertySource_profile] = new ProfilePropertyAccess(forumUser.UserInfo);
            this.PropertySource[PropertySource_membership] = new MembershipPropertyAccess(forumUser.UserInfo);
            this.PropertySource[PropertySource_forumauthor] = postInfo.Author.ForumUser;
            this.PropertySource[PropertySource_forumauthoruser] = postInfo.Author.ForumUser.UserInfo;
            this.PropertySource[PropertySource_forumauthorprofile] = new ProfilePropertyAccess(postInfo.Author.ForumUser.UserInfo);
            this.PropertySource[PropertySource_forumauthormembership] = new MembershipPropertyAccess(postInfo.Author.ForumUser.UserInfo);
            this.PropertySource[PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[PropertySource_module] = postInfo.Forum.ModuleInfo;
            this.PropertySource[PropertySource_portal] = portalSettings;
            this.PropertySource[PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public TokenReplacer(PortalSettings portalSettings, ForumUserInfo forumUser, Uri requestUri, string rawUrl)
        {
            forumUser.RawUrl = rawUrl;
            forumUser.RequestUri = requestUri;
            this.PropertySource[PropertySource_resx] = new ResourceStringTokenReplacer();
            this.PropertySource[PropertySource_forumuser] = forumUser;
            this.PropertySource[PropertySource_user] = forumUser.UserInfo;
            this.PropertySource[PropertySource_profile] = new ProfilePropertyAccess(forumUser.UserInfo);
            this.PropertySource[PropertySource_membership] = new MembershipPropertyAccess(forumUser.UserInfo);
            this.PropertySource[PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[PropertySource_module] = forumUser.ModuleInfo;
            this.PropertySource[PropertySource_portal] = portalSettings;
            this.PropertySource[PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public TokenReplacer(PortalSettings portalSettings, AuthorInfo author, ForumUserInfo forumUser, Uri requestUri, string rawUrl)
        {
            forumUser.RawUrl = rawUrl;
            forumUser.RequestUri = requestUri;
            this.PropertySource[PropertySource_resx] = new ResourceStringTokenReplacer();
            this.PropertySource[PropertySource_forumuser] = forumUser;
            this.PropertySource[PropertySource_user] = forumUser.UserInfo;
            this.PropertySource[PropertySource_profile] = new ProfilePropertyAccess(forumUser.UserInfo);
            this.PropertySource[PropertySource_membership] = new MembershipPropertyAccess(forumUser.UserInfo);
            this.PropertySource[PropertySource_forumauthor] = author.ForumUser;
            this.PropertySource[PropertySource_forumauthoruser] = author.ForumUser.UserInfo;
            this.PropertySource[PropertySource_forumauthorprofile] = new ProfilePropertyAccess(author.ForumUser.UserInfo);
            this.PropertySource[PropertySource_forumauthormembership] = new MembershipPropertyAccess(author.ForumUser.UserInfo);
            this.PropertySource[PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[PropertySource_module] = forumUser.ModuleInfo;
            this.PropertySource[PropertySource_portal] = portalSettings;
            this.PropertySource[PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public TokenReplacer(PortalSettings portalSettings, int forumTabId, int forumModuleId, int tabId, int moduleId, Uri requestUri, string rawUrl)
        {
            this.PropertySource[PropertySource_resx] = new ResourceStringTokenReplacer();
            this.PropertySource[PropertySource_dcf] = new ForumsModuleTokenReplacer(portalSettings, forumTabId, forumModuleId, tabId, moduleId, requestUri, rawUrl);
            this.PropertySource[PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[PropertySource_portal] = portalSettings;
            this.PropertySource[PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public new string ReplaceTokens(string source)
        {
            return base.ReplaceTokens(source);
        }

        internal static StringBuilder ReplaceForumControlTokens(StringBuilder template, PortalSettings portalSettings, ForumUserInfo forumUser, int forumTabId, int forumModuleId, int tabId, int moduleId, Uri requestUri, string rawUrl)
        {
            template = RemoveObsoleteTokens(template);
            template = ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplacer = new TokenReplacer(portalSettings, forumTabId, forumModuleId, tabId, moduleId, requestUri, rawUrl) { CurrentAccessLevel = Scope.DefaultSettings, AccessingUser = forumUser.UserInfo, };
            template = new StringBuilder(tokenReplacer.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplacer.ReplaceTokens(template.ToString()));
            return template;
        }

        internal static StringBuilder ReplaceForumTokens(StringBuilder template, ForumInfo forum, PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, ForumUserInfo forumUser, int tabId, CurrentUserTypes currentUserType, Uri requestUri, string rawUrl)
        {
            /* if no last post or subject missing, remove associated last topic tokens */
            if (forum.LastPostID == 0 || string.IsNullOrEmpty(HttpUtility.HtmlDecode(forum.LastPostSubject)))
            {
                template = RemovePrefixedToken(template, "[FORUM:LASTPOSTSUBJECT");
                template = RemovePrefixedToken(template, "[FORUM:LASTPOSTDISPLAYNAME");
                template = RemovePrefixedToken(template, "[FORUM:LASTPOSTAUTHORDISPLAYNAME");
                template = RemovePrefixedToken(template, "[FORUM:LASTPOSTAUTHORDISPLAYNAMELINK");
                template = RemovePrefixedToken(template, "[FORUM:LASTPOSTDATE");
                template.Replace("[AF:CONTROL:ADDFAVORITE]", string.Empty);
                template.Replace("[RESX:BY]", string.Empty);
            }

            if (template.ToString().Contains("[AF:CONTROL:ADDFAVORITE]"))
            {
                var forumUrl = new ControlUtils().BuildUrl(forum.PortalId, tabId, forum.ModuleId, forum.ForumGroup.PrefixURL, forum.PrefixURL, forum.ForumGroupId, forum.ForumID, -1, -1, string.Empty, 1, -1, forum.SocialGroupId);
                template.Replace("[AF:CONTROL:ADDFAVORITE]", "<a href=\"javascript:afAddBookmark('" + forum.ForumName + "','" + forumUrl + "');\"><img src=\"" + mainSettings.ThemeLocation + "images/favorites16_add.png\" border=\"0\" alt=\"[RESX:AddToFavorites]\" /></a>");
            }

            template = ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplacer = new TokenReplacer(portalSettings, forumUser, forum, requestUri, rawUrl) { CurrentAccessLevel = Scope.DefaultSettings, AccessingUser = forumUser.UserInfo, };
            template = new StringBuilder(tokenReplacer.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplacer.ReplaceTokens(template.ToString()));
            return template;
        }

        internal static StringBuilder ReplaceForumGroupTokens(StringBuilder template, ForumGroupInfo forumGroup, PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, ForumUserInfo forumUser, int tabId, CurrentUserTypes currentUserType, Uri requestUri, string rawUrl)
        {
            template = ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplacer = new TokenReplacer(portalSettings, forumUser, forumGroup, requestUri, rawUrl) { CurrentAccessLevel = Scope.DefaultSettings, AccessingUser = forumUser.UserInfo, };
            template = new StringBuilder(tokenReplacer.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplacer.ReplaceTokens(template.ToString()));

            return template;
        }

        internal static StringBuilder ReplaceAuthorTokens(StringBuilder template, PortalSettings portalSettings, SettingsInfo mainSettings, AuthorInfo author, ForumUserInfo accessingUser, Uri requestUri, string rawUrl, int forumModuleId)
        {
            template = ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplacer = new TokenReplacer(portalSettings, author, accessingUser, requestUri, rawUrl) { CurrentAccessLevel = Scope.DefaultSettings, AccessingUser = accessingUser?.UserInfo, };
            template = new StringBuilder(tokenReplacer.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplacer.ReplaceTokens(template.ToString()));

            return template;
        }

        internal static StringBuilder ReplaceUserTokens(StringBuilder template, PortalSettings portalSettings, SettingsInfo mainSettings, ForumUserInfo forumUser, ForumUserInfo accessingUser, Uri requestUri, string rawUrl, int forumModuleId)
        {
            var language = accessingUser?.UserInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;

            template = RemoveObsoleteTokens(template);

            template = ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplacer = new TokenReplacer(portalSettings, forumUser, requestUri, rawUrl) { CurrentAccessLevel = Scope.DefaultSettings, AccessingUser = forumUser.UserInfo, };
            template = new StringBuilder(tokenReplacer.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplacer.ReplaceTokens(template.ToString()));

            return template;
        }

        internal static StringBuilder ReplaceTopicTokens(StringBuilder template, TopicInfo topic, PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, ForumUserInfo forumUser, Uri requestUri, string rawUrl)
        {
            topic.Content.Body = ReplaceBody(topic.Content, mainSettings, requestUri).Replace("<br />", "  ");
            topic.Content.Summary = topic.Content.Summary.Replace("<br />", "  ");

            template = ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplacer = new TokenReplacer(portalSettings, forumUser, topic, requestUri, rawUrl) { AccessingUser = forumUser.UserInfo, };
            template = new StringBuilder(tokenReplacer.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplacer.ReplaceTokens(template.ToString()));

            return template;
        }

        private static string ReplaceBody(ContentInfo content, SettingsInfo mainSettings, Uri uri)
        {
            var sBody = content?.Body;
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

            return sBody;
        }

        internal static StringBuilder ReplacePostTokens(StringBuilder template, IPostInfo post, PortalSettings portalSettings, SettingsInfo mainSettings, INavigationManager navigationManager, ForumUserInfo forumUser, Uri requestUri, string rawUrl)
        {
            // Perform Profile Related replacements
            var author = new AuthorInfo(post.PortalId, post.Forum.ModuleId, post.Author.AuthorId);
            if (template.ToString().Contains("[POSTINFO]"))
            {
                var sPostInfo = TemplateUtils.GetPostInfo(post.ModuleId, author.ForumUser, post.Forum.ThemeLocation, post.Forum.GetIsMod(forumUser), post.Content.IPAddress, author.ForumUser.IsUserOnline, forumUser.CurrentUserType, forumUser.UserId, forumUser.PrefBlockAvatars, forumUser.UserInfo.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                template.Replace("[POSTINFO]", sPostInfo);
            }

            post.Content.Body = ReplaceBody(post.Content, mainSettings, requestUri).Replace("<br />", "  ");

            template = ResourceStringTokenReplacer.ReplaceResourceTokens(template);
            var tokenReplacer = new TokenReplacer(portalSettings, forumUser, post, requestUri, rawUrl) { AccessingUser = forumUser.UserInfo, };
            template = new StringBuilder(tokenReplacer.ReplaceEmbeddedTokens(template.ToString()));
            template = new StringBuilder(tokenReplacer.ReplaceTokens(template.ToString()));

            return template;
        }

        private static StringBuilder ExtractAndReplaceDateTokenWithOptionalFormatParameter(StringBuilder template, string tokenPrefix, DateTime? datetime, PortalSettings portalSettings, ForumUserInfo forumUser)
        {
            if (datetime == null || datetime == DateTime.MinValue || datetime == SqlDateTime.MinValue.Value || datetime == Utilities.NullDate())
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
                if (datetime != DateTime.MinValue && datetime != SqlDateTime.MinValue.Value && datetime != Utilities.NullDate())
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
            string[] tokens =
            {
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
                "[AF:TB:MyProfile]",
                "[AF:TB:MemberList]",
            };

            return RemoveObsoleteTokensFromTemplate(template, tokens);
        }

        internal static StringBuilder RemoveObsoleteEmailNotificationTokens(StringBuilder template)
        {
            // no longer using these
            string[] tokens =
            {
                "[COMMENTS]",
                "[REASON]",
            };

            return RemoveObsoleteTokensFromTemplate(template, tokens);
        }

        private static StringBuilder RemoveObsoleteTokensFromTemplate(StringBuilder template, string[] tokens)
        {
            if (tokens.Length > 0)
            {
                tokens.ToList().ForEach(t => template = RemoveObsoleteToken(template, t));
            }

            return template;
        }

        internal static StringBuilder RemoveControlTokensForDnnPrintMode(StringBuilder template)
        {
            string[] tokens =
            {
                "[RESX:SortPosts]",
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
                "[FORUMPOST:NEXTTOPICLINK]",
                "[FORUMTOPIC:NEXTTOPICLINK]",
                "[PREVTOPIC]",
                "[FORUMPOST:PREVIOUSTOPICLINK]",
                "[FORUMTOPIC:PREVIOUSTOPICLINK]",
                "[AF:CONTROL:STATUS]",
                "[ACTIONS:DELETE]",
                "[FORUMPOST:ACTIONDELETEONCLICK]",
                "[FORUMTOPIC:ACTIONDELETEONCLICK]",
                "[ACTIONS:EDIT]",
                "[FORUMPOST:ACTIONEDITONCLICK]",
                "[FORUMTOPIC:ACTIONEDITONCLICK]",
                "[ACTIONS:QUOTE]",
                "[FORUMPOST:ACTIONQUOTEONCLICK]",
                "[FORUMTOPIC:ACTIONQUOTEONCLICK]",
                "[ACTIONS:REPLY]",
                "[FORUMPOST:ACTIONREPLYONCLICK]",
                "[FORUMTOPIC:ACTIONREPLYONCLICK]",
                "[ACTIONS:ANSWER]",
                "[FORUMPOST:ACTIONANSWERONCLICK]",
                "[FORUMTOPIC:ACTIONANSWERONCLICK]",
                "[ACTIONS:ALERT]",
                "[FORUMPOST:ACTIONALERTONCLICK]",
                "[FORUMTOPIC:ACTIONALERTONCLICK]",
                "[ACTIONS:BAN]",
                "[FORUMPOST:ACTIONBANONCLICK]",
                "[FORUMTOPIC:ACTIONBANONCLICK]",
                "[ACTIONS:MOVE]",
                "[FORUMPOST:ACTIONMOVEONCLICK]",
                "[FORUMTOPIC:ACTIONMOVEONCLICK]",
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

        internal static StringBuilder MapLegacyTemplateTokenSynonyms(StringBuilder template)
        {
            template = ReplaceTokenSynonym(template, "[AF:CONTROL:TOPICACTIONS]", "[DCF:TEMPLATE-TOPICACTIONS]");
            template = ReplaceTokenSynonym(template, "[AF:TB:Search]", "[DCF:TEMPLATE-TOOLBARSEARCHPOPUP]");
            return template;
        }

        internal static StringBuilder MapLegacyModuleTokenSynonyms(StringBuilder template)
        {
            template = ReplaceTokenSynonym(template, "[MODULEID]", "[MODULE:MODULEID]");
            template = ReplaceTokenSynonym(template, "[TABID]", "[TAB:TABID]");
            template = ReplaceTokenSynonym(template, "[PAGENAME]", "[TAB:TITLE]");
            return template;
        }

        internal static StringBuilder MapLegacyToolbarTokenSynonyms(StringBuilder template, PortalSettings portalSettings, string language)
        {
            template = RemoveObsoleteTokens(template);
            template = MapLegacyPortalTokenSynonyms(template);
            template = MapLegacyModuleTokenSynonyms(template);
            template = ReplaceTokenSynonym(template, "[AF:TB:Search]", "[DCF:TEMPLATE-TOOLBARSEARCHPOPUP]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:NotRead]", "[DCF:TOOLBAR-NOTREAD-ONCLICK", "[AF:TB:NotRead]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:MyTopics]", "[DCF:TOOLBAR-MYTOPICS-ONCLICK", "[AF:TB:MyTopics]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:MySettings]", "[DCF:TOOLBAR-MYSETTINGS-ONCLICK", "[AF:TB:MySettings]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:MySubscriptions]", "[DCF:TOOLBAR-MYSUBSCRIPTIONS-ONCLICK", "[AF:TB:MySubscriptions]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:ControlPanel]", "[DCF:TOOLBAR-CONTROLPANEL-ONCLICK", "[AF:TB:ControlPanel]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:ModList]", "[DCF:TOOLBAR-MODERATE-ONCLICK", "[AF:TB:ModList]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:Unanswered]", "[DCF:TOOLBAR-UNANSWERED-ONCLICK", "[AF:TB:Unanswered]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:Unresolved]", "[DCF:TOOLBAR-UNRESOLVED-ONCLICK", "[AF:TB:Unresolved]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:Announcements]", "[DCF:TOOLBAR-ANNOUNCEMENTS-ONCLICK", "[AF:TB:Announcements]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:ActiveTopics]", "[DCF:TOOLBAR-ACTIVETOPICS-ONCLICK", "[AF:TB:ActiveTopics]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:MostLiked]", "[DCF:TOOLBAR-MOSTLIKED-ONCLICK", "[AF:TB:MostLiked]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:MostReplies]", "[DCF:TOOLBAR-MOSTREPLIES-ONCLICK", "[AF:TB:MostReplies]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:TB:Forums]", "[DCF:TOOLBAR-FORUMS-ONCLICK", "[AF:TB:Forums]");

            return template;
        }

        internal static StringBuilder MapLegacyForumTokenSynonyms(StringBuilder template, PortalSettings portalSettings, string language)
        {
            template = RemoveObsoleteTokens(template);
            template = MapLegacyPortalTokenSynonyms(template);
            template = MapLegacyModuleTokenSynonyms(template);
            template = MapLegacyForumGroupTokenSynonyms(template, portalSettings, language);

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

            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[RSSLINK]", "[FORUM:RSSLINK", "[RSSLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[PARENTFORUMLINK]", "[FORUM:PARENTFORUMLINK", "[PARENTFORUMLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[FORUMDESCRIPTION]", "[FORUM:FORUMDESCRIPTION", "[FORUMDESCRIPTION]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[FORUMICON]", "[FORUM:FORUMICON", "[FORUMICON]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[MODLINK]", "[FORUM:MODLINK", "[MODLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[GROUPNAME]", "[FORUMGROUP:FORUMGROUPLINK", "[FORUMGROUPLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[FORUMGROUPLINK]", "[FORUMGROUP:FORUMGROUPLINK", "[FORUMGROUPLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[FORUMMAINLINK]", "[FORUM:FORUMMAINLINK", "[FORUMMAINLINK]");

            /* note: this purposely uses same token format string [FORUMLINK] for both [FORUMNAME] and [FORUMLINK] */
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[FORUMNAME]", "[FORUM:FORUMLINK", "[FORUMLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[FORUMLINK]", "[FORUM:FORUMLINK", "[FORUMLINK]");

            /* note: these purposely use same target token but with different format string resource keys */
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[FORUMICONCSS]", "[FORUM:FORUMICONCSS", "[FORUMICONCSS]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[FORUMICONSM]", "[FORUM:FORUMICONCSS", "[FORUMICONSM]");

            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[DISPLAYNAME]", "[FORUM:LASTPOSTAUTHORDISPLAYNAMELINK", "[FORUMLASTPOSTAUTHORDISPLAYNAMELINK]", "[FORUM:LASTPOSTDISPLAYNAME]");

            return template;
        }

        internal static StringBuilder MapLegacyForumGroupTokenSynonyms(StringBuilder template, PortalSettings portalSettings, string language)
        {
            template = ReplaceTokenSynonym(template, "[FORUMGROUPID]", "[FORUMGROUP:FORUMGROUPID]");
            template = ReplaceTokenSynonym(template, "[GROUPCOLLAPSE]", "[FORUMGROUP:GROUPCOLLAPSE]");

            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[GROUPNAME]", "[FORUMGROUP:FORUMGROUPLINK", "[FORUMGROUPLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[FORUMGROUPLINK]", "[FORUMGROUP:FORUMGROUPLINK", "[FORUMGROUPLINK]");
            return template;
        }

        internal static StringBuilder MapLegacyUserTokenSynonyms(StringBuilder template, PortalSettings portalSettings, SettingsInfo mainSettings, string language)
        {
            template = ReplaceTokenSynonym(template, "[SENDERUSERNAME]", "[USER:USERNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERFIRSTNAME]", "[USER:FIRSTNAME]");
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
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERCAPTION]", "[FORUMUSER:USERCAPTION]");

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

            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[DISPLAYNAME]", "[FORUMUSER:USERPROFILELINK", "[DISPLAYNAMELINK]", "[FORUMUSER:DISPLAYNAME]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:PROFILE:DISPLAYNAME]", "[FORUMUSER:USERPROFILELINK", "[DISPLAYNAMELINK]", "[FORUMUSER:DISPLAYNAME]");

            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:BUTTON:EDITUSER]", "[FORUMUSER:EDITLINK", "[EDITUSERLINK]");

            if (mainSettings.PMType == PMTypes.Disabled)
            {
                template.Replace("[AF:PROFILE:PMLINK]", string.Empty);
                template.Replace("[AF:PROFILE:PMURL]", string.Empty);
            }
            else if (mainSettings.PMType == PMTypes.Core)
            {
                template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:PROFILE:PMLINK]", "[FORUMUSER:PMLINK", "[USERPMLINK]");
                template.Replace("[AF:PROFILE:PMURL]", string.Empty);
            }

            return template;
        }

        internal static StringBuilder MapLegacyAuthorTokenSynonyms(StringBuilder template, PortalSettings portalSettings, SettingsInfo mainSettings, string language)
        {
            template = ReplaceTokenSynonym(template, "[SENDERUSERNAME]", "[FORUMAUTHOR:USERNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERFIRSTNAME]", "[FORUMAUTHOR:FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERLASTNAME]", "[FORUMAUTHOR:LASTNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERDISPLAYNAME]", "[FORUMAUTHOR:DISPLAYNAME]");

            template = ReplaceTokenSynonym(template, "[USERNAME]", "[FORUMAUTHOR:USERNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERNAME]", "[FORUMAUTHOR:USERNAME]");

            template = ReplaceTokenSynonym(template, "[USERID]", "[FORUMAUTHOR:USERID]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERID]", "[FORUMAUTHOR:USERID]");

            template = ReplaceTokenSynonym(template, "[EMAIL]", "[FORUMAUTHOR:EMAIL]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:EMAIL]", "[FORUMAUTHOR:EMAIL]");

            template = ReplaceTokenSynonym(template, "[FIRSTNAME]", "[FORUMAUTHOR:FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:FIRSTNAME]", "[FORUMAUTHOR:FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[LASTNAME]", "[FORUMAUTHOR:LASTNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:LASTNAME]", "[FORUMAUTHOR:FIRSTNAME]");

            template = ReplaceTokenSynonym(template, "[SIGNATURE]", "[FORUMAUTHOR:SIGNATURE]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:SIGNATURE]", "[FORUMAUTHOR:SIGNATURE]");

            template = ReplaceTokenSynonym(template, "[AVATAR]", "[FORUMAUTHOR:AVATAR]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:AVATAR]", "[FORUMAUTHOR:AVATAR]");

            template = ReplaceTokenSynonym(template, "[RANKNAME]", "[FORUMAUTHOR:RANKNAME]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:RANKNAME]", "[FORUMAUTHOR:RANKNAME]");

            template = ReplaceTokenSynonym(template, "[RANKDISPLAY]", "[FORUMAUTHOR:RANKDISPLAY]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:RANKDISPLAY]", "[FORUMAUTHOR:RANKDISPLAY]");

            template = ReplaceTokenSynonym(template, "[MEMBERSINCE]", "[FORUMAUTHOR:DATECREATED]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:DATECREATED]", "[FORUMAUTHOR:DATECREATED]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:MEMBERSINCE]", "[FORUMAUTHOR:DATECREATED]");

            template = ReplaceTokenSynonym(template, "[AF:PROFILE:LASTACTIVE]", "[FORUMAUTHOR:DATELASTACTIVITY]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:DATELASTACTIVITY]", "[FORUMAUTHOR:DATELASTACTIVITY]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:DATELASTPOST]", "[FORUMAUTHOR:DATELASTPOST]");

            template = ReplaceTokenSynonym(template, "[USERCAPTION]", "[FORUMAUTHOR:USERCAPTION]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERCAPTION]", "[FORUMAUTHOR:USERCAPTION]");

            template = ReplaceTokenSynonym(template, "[USERSTATUS]", "[FORUMAUTHOR:USERSTATUS]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERSTATUS]", "[FORUMAUTHOR:USERSTATUS]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:USERSTATUS:CSS]", "[FORUMAUTHOR:USERSTATUSCSS]");

            template = ReplaceTokenSynonym(template, "[AF:PROFILE:TOTALPOINTS]", "[FORUMAUTHOR:TOTALPOINTS]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:VIEWCOUNT]", "[FORUMAUTHOR:VIEWCOUNT]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:ANSWERCOUNT]", "[FORUMAUTHOR:ANSWERCOUNT]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:REWARDPOINTS]", "[FORUMAUTHOR:REWARDPOINTS]");

            template = ReplaceTokenSynonym(template, "[POSTS]", "[FORUMAUTHOR:POSTS]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:POSTS]", "[FORUMAUTHOR:POSTS]");

            template = ReplaceTokenSynonym(template, "[POSTCOUNT]", "[FORUMAUTHOR:POSTCOUNT]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:POSTCOUNT]", "[FORUMAUTHOR:POSTCOUNT]");

            template = ReplaceTokenSynonym(template, "[AF:POINTS:TOPICCOUNT]", "[FORUMAUTHOR:TOPICCOUNT]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:TOPICCOUNT]", "[FORUMAUTHOR:TOPICCOUNT]");

            template = ReplaceTokenSynonym(template, "[AF:POINTS:REPLYCOUNT]", "[FORUMAUTHOR:REPLYCOUNT]");
            template = ReplaceTokenSynonym(template, "[AF:PROFILE:REPLYCOUNT]", "[FORUMAUTHOR:REPLYCOUNT]");

            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[DISPLAYNAME]", "[FORUMAUTHOR:USERPROFILELINK", "[DISPLAYNAMELINK]", "[FORUMAUTHOR:DISPLAYNAME]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:PROFILE:DISPLAYNAME]", "[FORUMAUTHOR:USERPROFILELINK", "[DISPLAYNAMELINK]", "[FORUMAUTHOR:DISPLAYNAME]");

            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:BUTTON:EDITUSER]", "[FORUMAUTHOR:EDITLINK", "[EDITAUTHORLINK]");

            if (mainSettings.PMType == PMTypes.Disabled)
            {
                template.Replace("[AF:PROFILE:PMLINK]", string.Empty);
                template.Replace("[AF:PROFILE:PMURL]", string.Empty);
            }
            else if (mainSettings.PMType == PMTypes.Core)
            {
                template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:PROFILE:PMLINK]", "[FORUMAUTHOR:PMLINK", "[AUTHORPMLINK]");
                template.Replace("[AF:PROFILE:PMURL]", string.Empty);
            }

            return template;
        }

        internal static StringBuilder MapLegacyTopicTokenSynonyms(StringBuilder template, PortalSettings portalSettings, string language)
        {
            // replace synonyms
            template = ReplaceTokenSynonym(template, "[AF:LABEL:LastPostDate]", "[FORUMTOPIC:LASTPOSTDATE]");
            template = ReplaceTokenSynonym(template, "[AF:LABEL:TopicDateCreated]", "[FORUMTOPIC:DATECREATED]");
            template = ReplaceTokenSynonym(template, "[TOPICID]", "[FORUMTOPIC:POSTID]");
            template = ReplaceTokenSynonym(template, "[AF:LABEL:ReplyCount]", "[FORUMTOPIC:REPLYCOUNT]");
            template = ReplaceTokenSynonym(template, "[VIEWS]", "[FORUMTOPIC:VIEWCOUNT]");
            template = ReplaceTokenSynonym(template, "[TOPICSUBSCRIBERCOUNT]", "[FORUMTOPIC:SUBSCRIBERCOUNT]");
            template = ReplaceTokenSynonym(template, "[REPLIES]", "[FORUMTOPIC:REPLYCOUNT]");
            template = ReplaceTokenSynonym(template, "[REPLYCOUNT]", "[FORUMTOPIC:REPLYCOUNT]");
            template = ReplaceTokenSynonym(template, "[VIEWCOUNT]", "[FORUMTOPIC:VIEWCOUNT]");
            template = ReplaceTokenSynonym(template, "[TOPICURL]", "[FORUMTOPIC:URL]");
            template = ReplaceTokenSynonym(template, "[BODYTITLE]", "[FORUMTOPIC:BODYTITLE]");

            template = ReplaceTokenSynonym(template, "[AF:URL:LASTREAD]", "[FORUMTOPIC:LASTREADURL]");
            template = ReplaceTokenSynonym(template, "[AF:URL:LASTREPLY]", "[FORUMTOPIC:LASTREPLYURL]");

            template = ReplaceTokenSynonym(template, "[SUBJECT]", "[FORUMTOPIC:SUBJECT]");
            template = ReplaceTokenSynonym(template, "[SUBJECTLINK]", "[FORUMTOPIC:SUBJECTLINK]");

            template = ReplaceTokenSynonymPrefix(template, "[BODY", "[FORUMTOPIC:BODY");
            template = ReplaceTokenSynonymPrefix(template, "[SUMMARY", "[FORUMTOPIC:SUMMARY");
            template = ReplaceTokenSynonymPrefix(template, "[TOPICSUBJECT", "[FORUMTOPIC:SUBJECT");

            if (template.ToString().Contains("[ICONLOCK]"))
            {
                template.Replace("[ICONLOCK]", "[ICONLOCKED][ICONUNLOCKED]");
                template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[ICONLOCKED]", "[FORUMTOPIC:ICONLOCKED", "[ICONLOCK]-ShowIcon");
                template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[ICONUNLOCKED]", "[FORUMTOPIC:ICONUNLOCKED", "[ICONLOCK]-HideIcon");
            }

            if (template.ToString().Contains("[ICONPIN]"))
            {
                template.Replace("[ICONPIN]", "[ICONPINNED][ICONUNPINNED]");
                template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[ICONPINNED]", "[FORUMTOPIC:ICONPINNED", "[ICONPIN]-ShowIcon");
                template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[ICONUNPINNED]", "[FORUMTOPIC:ICONUNPINNED", "[ICONPIN]-HideIcon");
            }

            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[POSTICON]", "[FORUMTOPIC:POSTICON", "[POSTICON]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[POSTRATINGDISPLAY]", "[FORUMTOPIC:RATING", "[POSTRATING]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[NEXTTOPIC]", "[FORUMTOPIC:NEXTTOPICLINK", "[NEXTTOPICLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[PREVTOPIC]", "[FORUMTOPIC:PREVIOUSTOPICLINK", "[PREVTOPICLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[STARTEDBY]", "[FORUMTOPIC:AUTHORDISPLAYNAMELINK", "[TOPICAUTHORDISPLAYNAMELINK]", "[FORUMTOPIC:AUTHORDISPLAYNAME]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[LASTPOSTDISPLAYNAME]", "[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAMELINK", "[TOPICLASTPOSTAUTHORDISPLAYNAMELINK]", "[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAME]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:LABEL:TopicAuthor]", "[FORUMTOPIC:AUTHORDISPLAYNAMELINK", "[TOPICAUTHORDISPLAYNAMELINK]", "[FORUMTOPIC:AUTHORDISPLAYNAME]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:LABEL:LastPostAuthor]", "[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAMELINK", "[TOPICLASTPOSTAUTHORDISPLAYNAMELINK]", "[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAME]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:ICONLINK:LASTREPLY]", "[FORUMTOPIC:LASTREPLYURL", "[ICONLINK-LASTREPLY]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:ICONLINK:LASTREAD]", "[FORUMTOPIC:LASTREADURL", "[ICONLINK-LASTREAD]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[AF:CONTROL:STATUSICON]", "[FORUMTOPIC:STATUSID", "[TOPICSTATUS]");
            return template;
        }

        internal static StringBuilder MapLegacyPostActionTokenSynonyms(StringBuilder template, PortalSettings portalSettings, string language, bool useListActions)
        {
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

            foreach (var action in new[]
            {
                "EDIT",
                "MOVE",
                "DELETE",
                "LOCK",
                "UNLOCK",
                "PIN",
                "UNPIN",
                "BAN",
                "ALERT",
                "REPLY",
                "QUOTE",
                "MARKANSWER",
            })
            {
                if (template.ToString().Contains($"[ACTIONS:{action}]"))
                {
                    template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, $"[ACTIONS:{action}]", $"[FORUMPOST:ACTION{action}ONCLICK", useListActions ? $"[FORUMPOST:ACTIONS:{action}:LISTITEM]" : $"[FORUMPOST:ACTIONS:{action}:HYPERLINK]");
                }
            }

            return template;
        }

        internal static StringBuilder MapLegacyEmailNotificationTokenSynonyms(StringBuilder template, PortalSettings portalSettings, string language)
        {
            template = ReplaceTokenSynonym(template, "[DISPLAYNAME]", "[FORUMAUTHOR:DISPLAYNAME]");
            template = ReplaceTokenSynonym(template, "[USERNAME]", "[FORUMAUTHOR:USERNAME]");
            template = ReplaceTokenSynonym(template, "[USERID]", "[FORUMAUTHOR:USERID]");
            template = ReplaceTokenSynonym(template, "[PORTALID]", "[PORTAL:PORTALID]");
            template = ReplaceTokenSynonym(template, "[FIRSTNAME]", "[FORUMAUTHOR:FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[LASTNAME]", "[FORUMAUTHOR:LASTNAME]");
            template = ReplaceTokenSynonym(template, "[FULLNAME]", "[FORUMAUTHOR:FIRSTNAME] [FORUMAUTHOR:LASTNAME]");
            template = ReplaceTokenSynonym(template, "[GROUPNAME]", "[FORUMGROUP:GROUPNAME]");
            template = ReplaceTokenSynonym(template, "[POSTDATE]", "[FORUMPOST:DATECREATED]");
            template = ReplaceTokenSynonym(template, "[PORTALNAME]", "[PORTAL:PORTALNAME]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[MODLINK]", "[FORUM:MODLINK", "[MODLINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[LINK]", "[FORUMPOST:LINK", "[LINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[HYPERLINK]", "[FORUMPOST:LINK", "[LINK]");
            template = ReplaceTokenSynonym(template, "[LINKURL]", "[FORUMPOST:LINK]");
            template = ReplaceTokenSynonym(template, "[FORUMLINK]", "[FORUM:FORUMLINK]");

            template = ReplaceTokenSynonym(template, "[SENDERUSERNAME]", "[FORUMAUTHOR:USERNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERFIRSTNAME", "[FORUMAUTHOR:FIRSTNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERLASTNAME]", "[FORUMAUTHOR:LASTNAME]");
            template = ReplaceTokenSynonym(template, "[SENDERDISPLAYNAME]", "[FORUMAUTHOR:DISPLAYNAME]");

            return template;
        }

        internal static StringBuilder MapLegacyPostTokenSynonyms(StringBuilder template, PortalSettings portalSettings, string language)
        {
            template = ReplaceTokenSynonym(template, "[POSTID]", "[FORUMPOST:POSTID]");
            template = ReplaceTokenSynonym(template, "[POSTDATE]", "[FORUMPOST:DATECREATED]");
            template = ReplaceTokenSynonym(template, "[MODEDITDATE]", "[FORUMPOST:MODEDITDATE]");
            template = ReplaceTokenSynonym(template, "[MODIPADDRESS]", "[FORUMPOST:MODIPADDRESS]");
            template = ReplaceTokenSynonym(template, "[REPLYID]", "[FORUMPOST:POSTID]");
            template = ReplaceTokenSynonym(template, "[POSTICONCSS]", "[FORUMPOST:POSTICONCSS]");
            template = ReplaceTokenSynonymPrefix(template, "[BODY", "[FORUMPOST:BODY");
            template = ReplaceTokenSynonymPrefix(template, "[SUMMARY", "[FORUMPOST:SUMMARY");

            if (template.ToString().Contains("[ICONLOCK]"))
            {
                template.Replace("[ICONLOCK]", "[ICONLOCKED][ICONUNLOCKED]");
                template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[ICONLOCKED]", "[FORUMPOST:ICONLOCKED", "[ICONLOCK]-ShowIcon");
                template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[ICONUNLOCKED]", "[FORUMPOST:ICONUNLOCKED", "[ICONLOCK]-HideIcon");
            }

            if (template.ToString().Contains("[ICONPIN]"))
            {
                template.Replace("[ICONPIN]", "[ICONPINNED][ICONUNPINNED]");
                template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[ICONPINNED]", "[FORUMPOST:ICONPINNED", "[ICONPIN]-ShowIcon");
                template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[ICONUNPINNED]", "[FORUMPOST:ICONUNPINNED", "[ICONPIN]-HideIcon");
            }

            template = ReplaceTokenSynonym(template, "[LINKURL]", "[FORUMPOST:LINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[LINK]", "[FORUMPOST:LINK", "[LINK]");
            template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, "[HYPERLINK]", "[FORUMPOST:LINK", "[LINK]");

            return template;
        }

        internal static StringBuilder MapLegacyTopicActionTokenSynonyms(StringBuilder template, PortalSettings portalSettings, string language, bool useListActions)
        {
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

            foreach (var action in new[]
            {
                "EDIT",
                "QUICKEDIT",
                "DELETE",
                "MOVE",
                "REPLY",
                "QUOTE",
                "LOCK",
                "UNLOCK",
                "PIN",
                "UNPIN",
            })
            {
                if (template.ToString().Contains($"[ACTIONS:{action}]"))
                {
                    template = ReplaceLegacyTokenWithFormatString(template, portalSettings, language, $"[ACTIONS:{action}]", $"[FORUMTOPIC:ACTION{action}ONCLICK", useListActions ? $"[FORUMTOPIC:ACTIONS:{action}:LISTITEM]" : $"[FORUMTOPIC:ACTIONS:{action}:HYPERLINK]");
                }
            }

            return template;
        }

        private static StringBuilder ReplaceLegacyTokenWithFormatString(StringBuilder template, PortalSettings portalSettings, string language, string oldTokenName, string newTokenNamePrefix, string formatStringResourceKey, string replaceWithIfEmpty = "")
        {
            var formatString = GetTokenFormatString(formatStringResourceKey, portalSettings, language);
            formatString = ResourceStringTokenReplacer.ReplaceResourceTokens(new StringBuilder(formatString)).ToString();
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
            var Pattern = $@"\[{tokenPrefix}(?>\[(?<c>)|[^\[\]]+|\](?<-c>))*(?(c)(?!))\]";
            return new StringBuilder(RegexUtils.GetCachedRegex(Pattern, RegexOptions.Compiled & RegexOptions.IgnorePatternWhitespace, 30).Replace(template.ToString(), string.Empty));
        }

        internal static string GetTokenFormatString(string key, PortalSettings portalSettings, string language = "en-US")
        {
            var formatString = Utilities.LocalizeString(key, Globals.LegacyTokenResourceFile, portalSettings, language);
            return formatString;
        }

        internal string ReplaceEmbeddedTokens(string source)
        {
            const string Pattern = @"(?<token>(?:(?<text>\[\])|\[(?:(?<object>[^{}\]\[:]+):(?<property>[^\]\[\|]+))(?:\|(?:(?<format>[^\]\[]+)\|(?<ifEmpty>[^\]\[]+))|\|(?:(?<format>[^\|\]\[]+)))?\])|(?<text>\[[^\]\[]+\])|(?<text>[^\]\[]+){0}\1)";
            try
            {
                var matches = RegexUtils.GetCachedRegex(Pattern, RegexOptions.Compiled & RegexOptions.IgnoreCase & RegexOptions.IgnorePatternWhitespace, 5).Matches(source);

                if (matches.Count > 0)
                {
                    var sb = new StringBuilder(source);
                    foreach (Match match in matches)
                    {
                        if (!string.IsNullOrEmpty(match.Groups["token"]?.Value) && match.Groups["object"] != null && this.PropertySource.ContainsKey(match.Groups["object"].Value.ToLowerInvariant()))
                        {
                            sb.Replace(match.Groups["token"]?.Value, this.ReplaceTokens(match.Groups["token"]?.Value));
                        }
                    }

                    return sb.ToString();
                }
            }
            catch (RegexMatchTimeoutException ex)
            {
                Exceptions.LogException(ex);
                return source;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw;
            }

            return source;
        }
    }
}
