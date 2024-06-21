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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Web.UI;
using DotNetNuke.Abstractions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.ActiveForums.API;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Modules.ActiveForums.Entities;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.UserControls;

namespace DotNetNuke.Modules.ActiveForums
{
    internal static class TokenReplacer
    {
        internal static string LocalizeTokenString(string key, DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings, string language = "en-US")
        {
            return Utilities.LocalizeString(key, Globals.TokenResourceFile, (DotNetNuke.Entities.Portals.PortalSettings)portalSettings, language);
        }
        internal static StringBuilder ReplaceUserTokens(StringBuilder template, DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings, SettingsInfo mainSettings, DotNetNuke.Entities.Users.UserInfo userInfo, int TabId, int ForumModuleId)
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
        internal static StringBuilder ReplaceTopicTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topic)
        {
            // no longer using this
            template.Replace("[SPLITBUTTONS2]", string.Empty);
            return template;
        }
        internal static StringBuilder ReplaceModuleTokens(StringBuilder template, DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings, SettingsInfo mainSettings, DotNetNuke.Entities.Users.UserInfo userInfo, int TabId, int ForumModuleId)
        {

            // Add This -- obsolete so just remove
            if (template.ToString().Contains("[AF:CONTROL:ADDTHIS"))
            {
                int inStart = (template.ToString().IndexOf("[AF:CONTROL:ADDTHIS", 0) + 1) + 19;
                int inEnd = (template.ToString().IndexOf("]", inStart - 1) + 1);
                template.Remove(inStart, ((inEnd - inStart) + 1));
            }



            string language = userInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;
            var ctlUtils = new ControlUtils();
            var urlNavigator = new Services.URLNavigator();

            template.Replace("[PORTALID]", portalSettings.PortalId.ToString());
            template.Replace("[PORTALNAME]", portalSettings.PortalName);
            template.Replace("[MODULEID]", ForumModuleId.ToString());
            template.Replace("[TABID]", TabId.ToString());
            template.Replace("[FORUMMAINLINK]", string.Format(LocalizeTokenString("[FORUMMAINLINK]", portalSettings, language), urlNavigator.NavigateURL(TabId)));
            return template;
        }
        internal static StringBuilder ReplaceForumTokens(StringBuilder template, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum, DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings, SettingsInfo mainSettings, DotNetNuke.Entities.Users.UserInfo userInfo, int TabId, int ForumModuleId)
        {
            string language = userInfo?.Profile?.PreferredLocale ?? portalSettings?.DefaultLanguage;
            var ctlUtils = new ControlUtils();
            var urlNavigator = new Services.URLNavigator();
            string groupUrl = ctlUtils.BuildUrl(TabId, ForumModuleId, forum.ForumGroup.PrefixURL, string.Empty, forum.ForumGroupId, -1, -1, -1, string.Empty, 1, -1, forum.SocialGroupId);
            string forumUrl = ctlUtils.BuildUrl(TabId, ForumModuleId, forum.ForumGroup.PrefixURL, forum.PrefixURL, forum.ForumGroupId, forum.ForumID, -1, -1, string.Empty, 1, -1, forum.SocialGroupId);
            string modLink = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildModeratorUrl(urlNavigator.NavigationManager(), portalSettings, mainSettings, forum);

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
                ? urlNavigator.NavigateURL(TabId, new[] { string.Concat(ParamKeys.ForumId, "=", forum.ForumID) })
                : urlNavigator.NavigateURL(TabId, new[] { string.Concat(ParamKeys.ForumId, "=", forum.ForumID), string.Concat(ParamKeys.ViewType, "=", Views.Topics) }));
            template.Replace("[PARENTFORUMNAME]", string.IsNullOrEmpty(forum.ParentForumName) ? string.Empty : forum.ParentForumName);

            string parentForumLink = LocalizeTokenString("[PARENTFORUMLINK]", portalSettings, language);
            if (forum.ParentForumId > 0)
            {
                template.Replace(oldValue: "[PARENTFORUMLINK]", string.Format(parentForumLink,
                                                                              mainSettings.UseShortUrls ?
                                                                              urlNavigator.NavigateURL(TabId, "", new[] { ParamKeys.ForumId + "=" + forum.ParentForumId }) :
                                                                              urlNavigator.NavigateURL(TabId, "", new[] { ParamKeys.ViewType + "=" + Views.Topics, ParamKeys.ForumId + "=" + forum.ParentForumId }),
                                                                              forum.ParentForumName));
            }
            else if (forum.ForumGroupId > 0)
            {
                template.Replace(oldValue: "[PARENTFORUMLINK]", string.Format(parentForumLink, Utilities.NavigateURL(TabId), forum.GroupName));
            }
            template.Replace("[PARENTFORUMLINK]", string.Empty);

            template.Replace("[FORUMDESCRIPTION]", !string.IsNullOrEmpty(forum.ForumDesc) ? string.Format(LocalizeTokenString("[FORUMDESCRIPTION]", portalSettings, language), forum.ForumDesc) : string.Empty);


            bool canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum.Security.View, userInfo.UserID, forum.PortalId);
            string sIcon = TemplateUtils.ShowIcon(canView, forum.ForumID, userInfo.UserID, forum.LastPostDateTime, forum.LastRead, forum.LastPostID);
            string sIconImage = "<img alt=\"" + forum.ForumName + "\" src=\"" + mainSettings.ThemeLocation + "images/" + sIcon + "\" />";

            if (template.ToString().Contains("[FORUMICON]"))
            {
                template.Replace("[FORUMICON]", sIconImage);
            }
            else if (template.ToString().Contains("[FORUMICONCSS]"))
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
                template.Replace("[FORUMICONCSS]", string.Format(LocalizeTokenString("[FORUMICONCSS]", portalSettings, language), sFolderCSS));
            }


            template.Replace("[TOTALTOPICS]", forum.TotalTopics.ToString());
            template.Replace("[TOTALREPLIES]", forum.TotalReplies.ToString());
            template.Replace("[FORUMSUBSCRIBERCOUNT]", forum.SubscriberCount.ToString());
            return template;
        }
    }
}
