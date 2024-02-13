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
using System.Web;

namespace DotNetNuke.Modules.ActiveForums
{
    public abstract partial class Utilities
    {
        [Obsolete("Deprecated in Community Forums. To be removed in 09.00.00. Use ManageImagePath(string sHTML, Uri hostUri)")]
        public static string ManageImagePath(string sHTML)
        {
            return ManageImagePath(sHTML, HttpContext.Current.Request.Url);
        }
        [Obsolete("Deprecated in Community Forums. To be removed in 09.00.00. Use ManageImagePath(string sHTML, Uri hostUri)")]
        public static string ManageImagePath(string sHTML, string hostWithScheme)
        {
            return ManageImagePath(sHTML, new Uri(hostWithScheme));
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use GetListOfModerators(int portalId, int ModuleId, int forumId).")]
        public static List<DotNetNuke.Entities.Users.UserInfo> GetListOfModerators(int portalId, int forumId)
        {
            return GetListOfModerators(portalId, -1, forumId);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController()")]
        public static string FilterWords(int portalId, int moduleId, string themePath, string strMessage, bool processEmoticons, bool removeHTML = false)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.FilterController.FilterWords(portalId, moduleId, themePath, strMessage, processEmoticons, removeHTML);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController()")]
        public static string RemoveFilterWords(int portalId, int moduleId, string themePath, string strMessage)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.FilterController.RemoveFilterWords(portalId, moduleId, themePath, strMessage);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController()")]
        public static string ImportFilter(int portalID, int moduleID)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.FilterController.ImportFilter(portalID, moduleID);
        }

    }
}
