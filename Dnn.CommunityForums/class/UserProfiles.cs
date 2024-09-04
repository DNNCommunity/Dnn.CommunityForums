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

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.")]
    public class UserProfiles
    {
        #region "Deprecated Methods"
        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Not used.")]
        public static string GetAvatar(int userID, int avatarWidth, int avatarHeight)
                => DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetAvatar(userID, avatarWidth, avatarHeight);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use method with PortalSettings as a parameter.")]
        public static string GetDisplayName(int moduleId, int userID, string username, string firstName = "", string lastName = "", string displayName = "", string profileNameClass = "af-profile-name")
            => DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings: null, moduleId, linkProfile: false, false, false, userID, username, firstName, lastName, displayName, null, profileNameClass);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use method with PortalSettings as a parameter.")]
        public static string GetDisplayName(int moduleId, bool linkProfile, bool isMod, bool isAdmin, int userId, string username, string firstName = "", string lastName = "", string displayName = "", string profileLinkClass = "af-profile-link", string profileNameClass = "af-profile-name")
            => DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(null, moduleId, linkProfile, isMod, isAdmin, userId, username, firstName, lastName, displayName, profileLinkClass, profileNameClass);

        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Not used.")]
        internal static string GetDisplayName(DotNetNuke.Entities.Portals.PortalSettings portalSettings, int moduleId, bool linkProfile, bool isMod, bool isAdmin, int userId, string username, string firstName = "", string lastName = "", string displayName = "", string profileLinkClass = "af-profile-link", string profileNameClass = "af-profile-name")
            => DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, moduleId, linkProfile, isMod, isAdmin, userId, username, firstName = "", lastName = "", displayName = "", profileLinkClass = "af-profile-link", profileNameClass = "af-profile-name");

        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Not used.")]
        public static string UserStatus(string themePath, bool isUserOnline, int userID, int moduleID, string altOnlineText = "User is Online", string altOfflineText = "User is Offline")
            => DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.UserStatus(themePath, isUserOnline, userID, moduleID, altOnlineText = "User is Online", altOfflineText = "User is Offline");

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetUserRank()")]
        /// <summary>
        /// Returns the Rank for the user
        /// </summary>
        /// <returns>ReturnType 0 Returns RankDisplay ReturnType 1 Returns RankName</returns>
        public static string GetUserRank(int portalId, int moduleID, int userID, int posts, int returnType)
            => DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetUserRank(moduleID, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleID).GetByUserId(portalId, userID), returnType);
        #endregion "Deprecated Methods"
    }
}
