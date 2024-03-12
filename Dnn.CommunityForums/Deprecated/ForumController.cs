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
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.ActiveForums.Data;

namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.")]
    public partial class ForumController
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForums.")]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetForum(int portalId, int moduleId, int forumId, bool ignoreCache)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForum(portalId, moduleId, forumId, ignoreCache);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForums.")]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo GetForum(int portalId, int moduleId, int forumId)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForum(portalId, moduleId, forumId, false);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.CreateGroupForum.")]
        public int CreateGroupForum(int portalId, int moduleId, int socialGroupId, int forumGroupId, string forumName, string forumDescription, bool isPrivate, string forumConfig)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ForumController.CreateGroupForum(portalId, moduleId, socialGroupId, forumGroupId, forumName, forumDescription, isPrivate, forumConfig);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.IterateForumsList().")]
        public string GetForumsHtmlOption(int portalId, int moduleId, User currentUser)
        {
            var sb = new StringBuilder();
            int index = 1;
            var forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(moduleId).Where(f => !f.Hidden && !f.ForumGroup.Hidden && (currentUser.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security.View, currentUser.UserRoles)));
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.IterateForumsList(forums.ToList(), currentUser,
                fi =>
                {
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", "-1", fi.GroupName);
                    index += 1;
                },
                fi =>
                {
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", fi.ForumID.ToString(), "--" + fi.ForumName);
                    index += 1;
                },
                fi =>
                {
                    sb.AppendFormat("<option value=\"{0}\">----{1}</option>", fi.ForumID.ToString(), fi.ForumName);
                    index += 1;
                }
                );
            return sb.ToString();
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Save.")]
        public int Forums_Save(int portalId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi, bool isNew, bool useGroup)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Forums_Save(portalId, fi, isNew, useGroup);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumIdsBySocialGroup.")]
        public string GetForumIdsBySocialGroup(int portalId, int socialGroupId)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumIdsBySocialGroup(portalId, socialGroupId);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser.")]
        public string GetForumsForUser(string userRoles, int portalId, int moduleId, string permissionType = "CanView", bool strict = false)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(userRoles, portalId, moduleId, permissionType, strict);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No Longer Used.")]
        public DataTable GetForumView(int portalId, int moduleId, int currentUserId, bool isSuperUser, string forumIds)
        { 
            return DataProvider.Instance().UI_ForumView(portalId, moduleId, currentUserId, isSuperUser, forumIds).Tables[0];
        }
    }
}
