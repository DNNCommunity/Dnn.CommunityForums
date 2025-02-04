// Copyright (c) by DNN Community
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
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Modules.ActiveForums.Entities;

    internal partial class ForumGroupController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo>
    {
        internal override string cacheKeyTemplate => CacheKeys.ForumGroupInfo;

        public DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo GetById(int forumGroupId, int moduleId)
        {
            var cachekey = this.GetCacheKey(moduleId, forumGroupId);
            var forumGroup = LoadFromSettingsCache(moduleId, cachekey);
            if (forumGroup == null)
            {
                forumGroup = base.GetById(forumGroupId, moduleId);
                if (forumGroup != null)
                {
                    forumGroup.LoadSecurity();
                    forumGroup.LoadFeatureSettings();
                }

                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheStore(moduleId, cachekey, forumGroup);
            }

            return forumGroup;
        }

        public int Groups_Save(int portalId, DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroupInfo, bool isNew, bool useDefaultFeatures, bool useDefaultSecurity)
        {
            var oldPermissionsId = -1;
            var copyDownDefaultSettings = false;
            var fc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController();
            if (useDefaultSecurity)
            {
                if (isNew)
                {
                    forumGroupInfo.PermissionsId = SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultPermissionId;
                }
                else
                {
                    if (!forumGroupInfo.InheritSecurity)
                    {
                        oldPermissionsId = forumGroupInfo.PermissionsId;
                        forumGroupInfo.PermissionsId = SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultPermissionId;
                        foreach (var forum in fc.GetForums(moduleId: forumGroupInfo.ModuleId).Where(f => f.ForumGroupId == forumGroupInfo.ForumGroupId && f.PermissionsId == oldPermissionsId))
                        {
                            forum.PermissionsId = forumGroupInfo.PermissionsId;
                            fc.Update(forum);
                        }
                    }
                }
            }
            else
            {
                if (isNew || forumGroupInfo.InheritSecurity) /* new forum group or switching from module security to group security */
                {
                    // set forum group permissions to use module default permissions as starting point
                    forumGroupInfo.PermissionsId = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().Insert(new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(permissionId: SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultPermissionId, moduleId: forumGroupInfo.ModuleId)).PermissionsId;

                    if (!isNew)
                    {
                        // reset any forum permissions previously mapped to the module default to map to new permissions id
                        foreach (var forum in fc.GetForums(moduleId: forumGroupInfo.ModuleId).Where(f => f.ForumGroupId == forumGroupInfo.ForumGroupId && f.PermissionsId == SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultPermissionId))
                        {
                            forum.PermissionsId = forumGroupInfo.PermissionsId;
                            fc.Update(forum);
                        }
                    }
                }
            }

            // if not using default features and new group or existing group previously using inherited settings, copy down default settings as a starting point
            if (!useDefaultFeatures && (isNew || forumGroupInfo.InheritSettings))
            {
                copyDownDefaultSettings = true;
            }

            if (useDefaultFeatures)
            {
                if (isNew)
                {
                    forumGroupInfo.GroupSettingsKey = SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultSettingsKey;
                }
                else
                {
                    var oldSettingsKey = forumGroupInfo.GroupSettingsKey;
                    forumGroupInfo.GroupSettingsKey = SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultSettingsKey;
                    foreach (var forum in fc.GetForums(moduleId: forumGroupInfo.ModuleId).Where(f => f.ForumGroupId == forumGroupInfo.ForumGroupId && f.ForumSettingsKey == oldSettingsKey))
                    {
                        forum.ForumSettingsKey = forumGroupInfo.GroupSettingsKey;
                        fc.Update(forum);
                    }
                }
            }
            else
            {
                if (!isNew || forumGroupInfo.InheritSettings)
                {
                    // reset any forum settings keys previously mapped to the module default to map to new settings key
                    forumGroupInfo.GroupSettingsKey = $"G:{forumGroupInfo.ForumGroupId}";
                    foreach (var forum in fc.GetForums(moduleId: forumGroupInfo.ModuleId).Where(f => f.ForumGroupId == forumGroupInfo.ForumGroupId && f.ForumSettingsKey == SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultSettingsKey))
                    {
                        forum.ForumSettingsKey = forumGroupInfo.GroupSettingsKey;
                        fc.Update(forum);
                    }
                }
            }

            // TODO: When these methods are updated to use DAL2 for update, uncomment Cacheable attribute on forumGroupInfo
            forumGroupInfo.ForumGroupId = DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Groups_Save(portalId, forumGroupInfo.ModuleId, forumGroupInfo.ForumGroupId, forumGroupInfo.GroupName, forumGroupInfo.SortOrder, forumGroupInfo.Active, forumGroupInfo.Hidden, forumGroupInfo.PermissionsId, forumGroupInfo.PrefixURL, forumGroupInfo.GroupSettingsKey);
            if (string.IsNullOrEmpty(forumGroupInfo.GroupSettingsKey))
            {
                forumGroupInfo.GroupSettingsKey = $"G:{forumGroupInfo.ForumGroupId}";
                this.Update(forumGroupInfo);
            }

            // if new group and not using default features, copy default features to group features as starting point
            if (copyDownDefaultSettings)
            {
                forumGroupInfo.FeatureSettings = SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).ForumFeatureSettings;
                FeatureSettings.Save(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, forumGroupInfo.FeatureSettings);
                this.Update(forumGroupInfo);
            }

            if (oldPermissionsId != -1)
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().RemoveIfUnused(permissionId: oldPermissionsId, moduleId: forumGroupInfo.ModuleId);
            }

            if (useDefaultFeatures) /* if now using default module settings, remove group settings */
            {
                DataContext.Instance().Execute(System.Data.CommandType.Text, "DELETE FROM {databaseOwner}{objectQualifier}activeforums_Settings WHERE ModuleId = @0 AND GroupKey = @1", forumGroupInfo.ModuleId, $"G:{forumGroupInfo.ForumGroupId}");
            }

            ClearSettingsCache(forumGroupInfo.ModuleId);
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
            ClearSettingsCache(moduleId);
        }
    }
}
