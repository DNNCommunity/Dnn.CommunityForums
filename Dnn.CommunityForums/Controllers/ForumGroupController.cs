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

using System.Collections;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal partial class ForumGroupController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo GetById(int forumGroupId, int moduleId)
        {
            var cachekey = string.Format(CacheKeys.ForumGroupInfo, moduleId, forumGroupId);
            DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroupInfo = DataCache.SettingsCacheRetrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo;
            if (forumGroupInfo == null)
            {
                forumGroupInfo = base.GetById(forumGroupId, moduleId);
                if (forumGroupInfo != null)
                {
                    forumGroupInfo.Security = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(forumGroupInfo.PermissionsId, forumGroupInfo.ModuleId);
                    forumGroupInfo.GroupSettings = (Hashtable)DataCache.GetSettings(moduleId, forumGroupInfo.GroupSettingsKey, string.Format(CacheKeys.GroupSettingsByKey, moduleId, forumGroupInfo.GroupSettingsKey), true);
                }
                DataCache.SettingsCacheStore(forumGroupInfo.ModuleId, cachekey, forumGroupInfo);
            }
            return forumGroupInfo;
        }
        public int Groups_Save(int portalId, DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroupInfo, bool isNew)
        {
            if (forumGroupInfo.PermissionsId == -1)
            {
                    forumGroupInfo.PermissionsId = (new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().CreateAdminPermissions(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetAdministratorsRoleId(portalId).ToString())).PermissionsId;
            }
            if (forumGroupInfo.ForumGroupId <= 0)
            {
                Insert(forumGroupInfo);
                forumGroupInfo.GroupSettingsKey = $"G:{forumGroupInfo.ForumGroupId}";
            }
            Update(forumGroupInfo);
            if (isNew)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CreateDefaultSets(portalId, forumGroupInfo.PermissionsId);
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.TopicsTemplateId, "0");
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.TopicTemplateId, "0");
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.TopicFormId, "0");
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.ReplyFormId, "0");
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.QuickReplyFormId, "0");
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.AllowRSS, "false");
            }
            DataCache.SettingsCacheClear(forumGroupInfo.ModuleId, string.Format(CacheKeys.ForumList, forumGroupInfo.ModuleId));
            return forumGroupInfo.ForumGroupId;
        }
        public void Group_Delete(int moduleId, int forumGroupId)
        {
            Delete(GetById(id: forumGroupId));
            DataCache.SettingsCacheClear(moduleId, string.Format(CacheKeys.ForumList, moduleId));
        }
    }
}
