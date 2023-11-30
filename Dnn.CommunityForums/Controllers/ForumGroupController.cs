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

using System;
using DotNetNuke.Common.Utilities;
using System.Collections;
using System.Data;
using DotNetNuke.Modules.ActiveForums.API;
using System.Reflection;
using DotNetNuke.Modules.ActiveForums.Entities;
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    public partial class ForumGroupController : DotNetNuke.Modules.ActiveForums.Controllers.ControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo Groups_Get(int moduleID, int forumGroupID)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroupInfo = GetForumGroup(moduleID, forumGroupID);
            forumGroupInfo.GroupSettings = (Hashtable)DataCache.GetSettings(moduleID, forumGroupInfo.GroupSettingsKey, string.Format(CacheKeys.ForumGroupSettings, moduleID, forumGroupInfo.ForumGroupId), false);
            return forumGroupInfo;
        }
        public DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo GetForumGroup(int moduleId, int forumGroupId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroupInfo = Repo.GetById(forumGroupId);
            if (forumGroupInfo != null)
            {
                forumGroupInfo.Security = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(forumGroupInfo.PermissionsId);
            }
            return forumGroupInfo;
        }
        public int Groups_Save(int portalId, DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroupInfo, bool isNew)
        {
            var rc = new DotNetNuke.Security.Roles.RoleController();
            var db = new Data.Common();
            var permissionsId = -1;
            if (forumGroupInfo.PermissionsId == -1)
            {
                var ri = rc.GetRoleByName(portalId, "Administrators");
                if (ri != null)
                {
                    forumGroupInfo.PermissionsId = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().CreateAdminPermissions(ri.RoleID.ToString()).PermissionsId;
                    permissionsId = forumGroupInfo.PermissionsId;
                }
            }
            if (forumGroupInfo.ForumGroupId <= 0)
            {
                Repo.Insert(forumGroupInfo);
                forumGroupInfo.GroupSettingsKey = $"G:{forumGroupInfo.ForumGroupId}";
                forumGroupInfo.GroupSecurityKey = $"G:{forumGroupInfo.ForumGroupId}";
            }
            Repo.Update(forumGroupInfo);
            if (isNew)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CreateDefaultSets(portalId, permissionsId);
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.TopicsTemplateId, "0");
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.TopicTemplateId, "0");
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.TopicFormId, "0");
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.ReplyFormId, "0");
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.AllowRSS, "false");
            }
            DataCache.SettingsCacheClear(forumGroupInfo.ModuleId, string.Format(CacheKeys.ForumList, forumGroupInfo.ModuleId));
            return forumGroupInfo.ForumGroupId;
        }
        public void Group_Delete(int moduleId, int forumGroupId)
        {
            Repo.Delete(Repo.GetById(id: forumGroupId));
        }
    }
}
