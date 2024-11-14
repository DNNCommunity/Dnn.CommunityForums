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

using System.Linq;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System.Collections;
    using DotNetNuke.Modules.ActiveForums.Entities;

    internal partial class ForumGroupController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo GetById(int forumGroupId, int moduleId)
        {
            var cachekey = string.Format(CacheKeys.ForumGroupInfo, moduleId, forumGroupId);
            DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroup = DataCache.SettingsCacheRetrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo;
            if (forumGroup == null)
            {
                forumGroup = base.GetById(forumGroupId, moduleId);
                if (forumGroup != null)
                {
                    forumGroup.LoadSecurity();
                    forumGroup.LoadSettings();
                }

                DataCache.SettingsCacheStore(moduleId, cachekey, forumGroup);
            }

            return forumGroup;
        }

        public int Groups_Save(int portalId, DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroupInfo, bool isNew)
        {
            if (forumGroupInfo.PermissionsId == -1)
            {
                forumGroupInfo.PermissionsId = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().CreateAdminPermissions(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetAdministratorsRoleId(portalId).ToString(), forumGroupInfo.ModuleId).PermissionsId;
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CreateDefaultSets(portalId, forumGroupInfo.ModuleId, forumGroupInfo.PermissionsId);
            }

            if (string.IsNullOrEmpty(forumGroupInfo.GroupSettingsKey))
            {
                forumGroupInfo.GroupSettingsKey = $"G:{forumGroupInfo.ForumGroupId}";
            }

            // TODO: When these methods are updated to use DAL2 for update, uncomment Cacheable attribute on forumGroupInfo
            forumGroupInfo.ForumGroupId = DataProvider.Instance().Groups_Save(portalId, forumGroupInfo.ModuleId, forumGroupInfo.ForumGroupId, forumGroupInfo.GroupName, forumGroupInfo.SortOrder, forumGroupInfo.Active, forumGroupInfo.Hidden, forumGroupInfo.PermissionsId, forumGroupInfo.PrefixURL, forumGroupInfo.GroupSettingsKey);

            if (isNew)
            {
                var tc = new TemplateController();
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.TopicsTemplateId, tc.Template_Get(templateName: "TopicsView", portalId: portalId, moduleId: forumGroupInfo.ModuleId).TemplateId.ToString());
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.TopicTemplateId, tc.Template_Get(templateName: "TopicView", portalId: portalId, moduleId: forumGroupInfo.ModuleId).TemplateId.ToString());
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.TopicFormId, tc.Template_Get(templateName: "TopicEditor", portalId: portalId, moduleId: forumGroupInfo.ModuleId).TemplateId.ToString());
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.ReplyFormId, tc.Template_Get(templateName: "ReplyEditor", portalId: portalId, moduleId: forumGroupInfo.ModuleId).TemplateId.ToString());
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.QuickReplyFormId, tc.Template_Get(templateName: "QuickReply", portalId: portalId, moduleId: forumGroupInfo.ModuleId).TemplateId.ToString());
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.ProfileTemplateId, tc.Template_Get(templateName: "ProfileInfo", portalId: portalId, moduleId: forumGroupInfo.ModuleId).TemplateId.ToString());
                Settings.SaveSetting(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, ForumSettingKeys.AllowRSS, "true");
            }

            DataCache.ClearSettingsCache(forumGroupInfo.ModuleId);
            return forumGroupInfo.ForumGroupId;
        }

        public void Groups_Delete(int moduleId, int forumGroupId)
        {
            var fc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController();
            foreach (var forum in fc.GetForums(moduleId: moduleId).Where(f => f.ForumGroupId == forumGroupId))
            {
                fc.Forums_Delete(forum.ForumID, moduleId);
            }
            this.DeleteById(forumGroupId);
            DataCache.ClearSettingsCache(moduleId);
        }
    }
}
