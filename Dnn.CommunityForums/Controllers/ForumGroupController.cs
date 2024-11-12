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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System.Collections;
    using System.Runtime;
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
                    forumGroup.LoadFeatureSettings();
                }

                DataCache.SettingsCacheStore(moduleId, cachekey, forumGroup);
            }

            return forumGroup;
        }

        public int Groups_Save(int portalId, DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroupInfo, bool isNew, bool useDefaultFeatures, bool useDefaultSecurity)
        {
            if (useDefaultSecurity)
            {
                forumGroupInfo.PermissionsId = SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultPermissionId;
            }
            else
            {
                if (isNew || forumGroupInfo.InheritSecurity) /* new forum group or switching from module security to group security */
                {
                    // use module default settings
                    forumGroupInfo.PermissionsId = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().Insert(new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(permissionId: SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultPermissionId, moduleId: forumGroupInfo.ModuleId)).PermissionsId;
                }
            }

            if (useDefaultFeatures)
            {
                forumGroupInfo.GroupSettingsKey = SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultSettingsKey;
            }
            else
            {
                if (isNew || forumGroupInfo.InheritSettings)
                {
                    forumGroupInfo.GroupSettingsKey = $"G:{forumGroupInfo.ForumGroupId}";
                    forumGroupInfo.FeatureSettings = SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).ForumFeatureSettings;
                    FeatureSettings.Save(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, forumGroupInfo.FeatureSettings);
                }
            }

            // TODO: When these methods are updated to use DAL2 for update, uncomment Cacheable attribute on forumGroupInfo
            forumGroupInfo.ForumGroupId = DataProvider.Instance().Groups_Save(portalId, forumGroupInfo.ModuleId, forumGroupInfo.ForumGroupId, forumGroupInfo.GroupName, forumGroupInfo.SortOrder, forumGroupInfo.Active, forumGroupInfo.Hidden, forumGroupInfo.PermissionsId, forumGroupInfo.PrefixURL);

            if (useDefaultFeatures)
            {
                DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Forum_ConfigCleanUp(forumGroupInfo.ModuleId, $"G:{forumGroupInfo.ForumGroupId}");
            }

            DataCache.ClearSettingsCache(forumGroupInfo.ModuleId);
            return forumGroupInfo.ForumGroupId;
        }

        public void Groups_Delete(int forumGroupId, int moduleId)
        {
            // TODO: When these methods are updated to use DAL2 for update, uncomment Cacheable attribute on forumGroupInfo
            DataProvider.Instance().Groups_Delete(moduleId, forumGroupId);
        }
    }
}
