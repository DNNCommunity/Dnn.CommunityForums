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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Web;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;

    public class UserProfiles
    {
        public static string GetAvatar(int userID, int avatarWidth, int avatarHeight)
        {
            PortalSettings portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();

            if (portalSettings == null)
            {
                return string.Empty;
            }

            // GIF files when reduced using DNN class losses its animation, so for gifs send them as is
            var user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userID);
            string imgUrl = string.Empty;

            if (user != null)
            {
                imgUrl = user.Profile.PhotoURL;
            }

            if (!string.IsNullOrWhiteSpace(imgUrl) && imgUrl.ToLower().EndsWith("gif"))
            {
                return string.Format("<img class='af-avatar' alt='' src='{0}' height='{1}px' width='{2}px' />", imgUrl, avatarHeight, avatarWidth);
            }
            else
            {
                return string.Concat("<img class='af-avatar' src='", string.Format(Common.Globals.UserProfilePicFormattedUrl(), userID, avatarWidth, avatarHeight), "' />");
            }
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use method with PortalSettings as a parameter.")]
        public static string GetDisplayName(int moduleId, int userID, string username, string firstName = "", string lastName = "", string displayName = "", string profileNameClass = "af-profile-name") => GetDisplayName(portalSettings: null, moduleId, linkProfile: false, false, false, userID, username, firstName, lastName, displayName, null, profileNameClass);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use method with PortalSettings as a parameter.")]
        public static string GetDisplayName(int moduleId, bool linkProfile, bool isMod, bool isAdmin, int userId, string username, string firstName = "", string lastName = "", string displayName = "", string profileLinkClass = "af-profile-link", string profileNameClass = "af-profile-name") => GetDisplayName(null, moduleId, linkProfile, isMod, isAdmin, userId, username, firstName, lastName, displayName, profileLinkClass, profileNameClass);

        internal static string GetDisplayName(DotNetNuke.Entities.Portals.PortalSettings portalSettings, int moduleId, bool linkProfile, bool isMod, bool isAdmin, int userId, string username, string firstName = "", string lastName = "", string displayName = "", string profileLinkClass = "af-profile-link", string profileNameClass = "af-profile-name")
        {
            if (portalSettings == null)
            {
                portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
            }

            if (portalSettings == null)
            {
                return null;
            }

            var mainSettings = SettingsBase.GetModuleSettings(moduleId);

            var outputTemplate = string.IsNullOrWhiteSpace(profileLinkClass) ? "{0}" : string.Concat("<span class='", profileNameClass, "'>{0}</span>");

            if (linkProfile && userId > 0)
            {
                var profileVisibility = mainSettings.ProfileVisibility;

                switch (profileVisibility)
                {
                    case ProfileVisibilities.Disabled:
                        linkProfile = false;
                        break;

                    case ProfileVisibilities.Everyone: // Nothing to do in this case
                        break;

                    case ProfileVisibilities.RegisteredUsers:
                        linkProfile = HttpContext.Current.Request.IsAuthenticated;
                        break;

                    case ProfileVisibilities.Moderators:
                        linkProfile = isMod || isAdmin;
                        break;

                    case ProfileVisibilities.Admins:
                        linkProfile = isAdmin;
                        break;
                }

                if (linkProfile && portalSettings.UserTabId != null && portalSettings.UserTabId != DotNetNuke.Common.Utilities.Null.NullInteger && portalSettings.UserTabId != -1)
                {
                    outputTemplate = string.Concat("<a href='", Utilities.NavigateURL(portalSettings.UserTabId, string.Empty, new[] { "userid=" + userId }), "' class='", profileLinkClass, "' rel='nofollow'>{0}</a>");
                }
            }

            var displayMode = mainSettings.UserNameDisplay + string.Empty;

            string outputName = null;
            UserInfo user;

            switch (displayMode.ToUpperInvariant())
            {
                case "DISPLAYNAME":

                    if (string.IsNullOrWhiteSpace(username) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        displayName = (user != null) ? user.DisplayName : null;
                    }

                    outputName = displayName;
                    break;

                case "USERNAME":

                    if (string.IsNullOrWhiteSpace(username) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        username = (user != null) ? user.Username : null;
                    }

                    outputName = username;
                    break;

                case "FIRSTNAME":

                    if (string.IsNullOrWhiteSpace(firstName) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        firstName = (user != null) ? user.FirstName : null;
                    }

                    outputName = firstName;
                    break;

                case "LASTNAME":

                    if (string.IsNullOrWhiteSpace(lastName) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        lastName = (user != null) ? user.LastName : null;
                    }

                    outputName = lastName;
                    break;

                case "FULLNAME":
                    if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) && userId > 0)
                    {
                        user = new DotNetNuke.Entities.Users.UserController().GetUser(portalSettings.PortalId, userId);
                        firstName = (user != null) ? Utilities.SafeTrim(user.FirstName) : null;
                        lastName = (user != null) ? Utilities.SafeTrim(user.LastName) : null;
                    }

                    outputName = string.Concat(firstName, " ", lastName);
                    break;
            }

            outputName = Utilities.SafeTrim(outputName);

            if (string.IsNullOrWhiteSpace(outputName))
            {
                outputName = userId > 0 ? Utilities.GetSharedResource("[RESX:DeletedUser]") : Utilities.GetSharedResource("[RESX:Anonymous]");
            }

            outputName = HttpUtility.HtmlEncode(outputName);

            return string.Format(outputTemplate, outputName);
        }

        public static string UserStatus(string themePath, bool isUserOnline, int userID, int moduleID, string altOnlineText = "User is Online", string altOfflineText = "User is Offline")
        {
            if (isUserOnline)
            {
                return "<span class=\"af-user-status\"><i class=\"fa fa-circle fa-blue\"></i></span>";
            }

            return "<span class=\"af-user-status\"><i class=\"fa fa-circle fa-red\"></i></span>";
        }

        /// <summary>
        /// Returns the Rank for the user
        /// </summary>
        /// <returns>ReturnType 0 Returns RankDisplay ReturnType 1 Returns RankName</returns>
        public static string GetUserRank(int portalId, int moduleID, int userID, int posts, int returnType)
        {
            // ReturnType 0 for RankDisplay
            // ReturnType 1 for RankName
            try
            {
                var strHost = Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)) + "/";
                var rc = new RewardController();
                var sRank = string.Empty;
                foreach (var ri in rc.Reward_List(portalId, moduleID, true).Where(ri => ri.MinPosts <= posts && ri.MaxPosts > posts))
                {
                    if (returnType == 0)
                    {
                        sRank = string.Format("<img src='{0}{1}' border='0' alt='{2}' />", strHost, ri.Display.Replace("activeforums/Ranks", "ActiveForums/images/Ranks"), ri.RankName);
                        break;
                    }

                    sRank = ri.RankName;
                    break;
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
