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
    using System;
    using System.Linq;

    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;

    internal class ForumGroupController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo, IForumGroupController, ForumGroupController>, IForumGroupController
    {
        protected override Func<IForumGroupController> GetFactory()
        {
            return () => new ForumGroupController();
        }

        public virtual DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo GetById(int forumGroupId, int moduleId)
        {
            var cachekey = string.Format(CacheKeys.ForumGroupInfo, moduleId, forumGroupId);
            var forumGroup = DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo;
            if (forumGroup == null)
            {
                forumGroup = this._repositoryControllerBase.GetById(id: forumGroupId, scopeValue: moduleId);
                if (forumGroup == null)
                {
                    forumGroup = this._repositoryControllerBase.GetById(id: forumGroupId);
                }

                if (forumGroup != null)
                {
                    forumGroup.LoadSecurity();
                    forumGroup.LoadFeatureSettings();
                }

                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Store(moduleId, cachekey, forumGroup);
            }

            return forumGroup;
        }

        internal virtual DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo GetByUrlPrefix(int moduleId, string groupPrefix)
        {
            string cachekey = string.Format(CacheKeys.ForumGroupByUrlPrefix, moduleId, groupPrefix);
            DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroupInfo = DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo;
            if (forumGroupInfo == null)
            {
                // this accommodates duplicates which may exist since currently no uniqueness applied in database
                var forumGroupId = this._repositoryControllerBase.Find("WHERE ModuleId = @0 AND PrefixURL = @1", moduleId, groupPrefix.Trim()).OrderBy(t => t.ForumGroupId).FirstOrDefault()?.ForumGroupId;
                if (forumGroupId.HasValue)
                {
                    forumGroupInfo = this.GetById(forumGroupId.Value, moduleId);
                }

                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Store(moduleId, cachekey, forumGroupInfo);
            }

            return forumGroupInfo;
        }

        public int Groups_Save(int portalId, DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo forumGroupInfo, bool isNew, bool useDefaultFeatures, bool useDefaultSecurity)
        {
            var oldPermissionsId = -1;
            var copyDownDefaultSettings = false;
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
                        foreach (var forum in DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetForums(moduleId: forumGroupInfo.ModuleId).Where(f => f.ForumGroupId == forumGroupInfo.ForumGroupId && f.PermissionsId == oldPermissionsId))
                        {
                            forum.PermissionsId = forumGroupInfo.PermissionsId;
                            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.Update(forum);
                        }
                    }
                }
            }
            else
            {
                if (isNew || forumGroupInfo.InheritSecurity) /* new forum group or switching from module security to group security */
                {
                    // set forum group permissions to use module default permissions as starting point
                    forumGroupInfo.PermissionsId = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.Instance.Insert(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.Instance.GetById(moduleId: forumGroupInfo.ModuleId, permissionsId: SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultPermissionId)).PermissionsId;

                    if (!isNew)
                    {
                        // reset any forum permissions previously mapped to the module default to map to new permissions id
                        foreach (var forum in DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetForums(moduleId: forumGroupInfo.ModuleId).Where(f => f.ForumGroupId == forumGroupInfo.ForumGroupId && f.PermissionsId == SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultPermissionId))
                        {
                            forum.PermissionsId = forumGroupInfo.PermissionsId;
                            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.Update(forum);
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
                    foreach (var forum in DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetForums(moduleId: forumGroupInfo.ModuleId).Where(f => f.ForumGroupId == forumGroupInfo.ForumGroupId && f.ForumSettingsKey == oldSettingsKey))
                    {
                        forum.ForumSettingsKey = forumGroupInfo.GroupSettingsKey;
                        DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.Update(forum);
                    }
                }
            }
            else
            {
                if (!isNew || forumGroupInfo.InheritSettings)
                {
                    // reset any forum settings keys previously mapped to the module default to map to new settings key
                    forumGroupInfo.GroupSettingsKey = $"G{forumGroupInfo.ForumGroupId}";
                    foreach (var forum in DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetForums(moduleId: forumGroupInfo.ModuleId).Where(f => f.ForumGroupId == forumGroupInfo.ForumGroupId && f.ForumSettingsKey == SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultSettingsKey))
                    {
                        forum.ForumSettingsKey = forumGroupInfo.GroupSettingsKey;
                        DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.Update(forum);
                    }
                }
            }

            // TODO: When these methods are updated to use DAL2 for update, uncomment Cacheable attribute on forumGroupInfo
            forumGroupInfo.ForumGroupId = DotNetNuke.Modules.ActiveForums.DataProvider.Instance().Groups_Save(portalId, forumGroupInfo.ModuleId, forumGroupInfo.ForumGroupId, forumGroupInfo.GroupName, forumGroupInfo.SortOrder, forumGroupInfo.Active, forumGroupInfo.Hidden, forumGroupInfo.PermissionsId, forumGroupInfo.PrefixURL, forumGroupInfo.GroupSettingsKey);
            /* refresh to get computed values such as SortOrder */
            forumGroupInfo = this.GetById(forumGroupInfo.ForumGroupId, forumGroupInfo.ModuleId);
            if (string.IsNullOrEmpty(forumGroupInfo.GroupSettingsKey))
            {
                forumGroupInfo.GroupSettingsKey = $"G{forumGroupInfo.ForumGroupId}";
                this._repositoryControllerBase.Update(forumGroupInfo);
            }

            // if new group and not using default features, copy default features to group features as starting point
            if (copyDownDefaultSettings)
            {
                forumGroupInfo.FeatureSettings = SettingsBase.GetModuleSettings(forumGroupInfo.ModuleId).DefaultFeatureSettings;
                FeatureSettings.Save(forumGroupInfo.ModuleId, forumGroupInfo.GroupSettingsKey, forumGroupInfo.FeatureSettings);
                this._repositoryControllerBase.Update(forumGroupInfo);
            }

            if (oldPermissionsId != -1)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.Instance.RemoveIfUnused(permissionsId: oldPermissionsId, moduleId: forumGroupInfo.ModuleId);
            }

            /* if now using default module settings, remove group settings */
            if (useDefaultFeatures)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.Instance.DeleteForModuleIdSettingsKey(forumGroupInfo.ModuleId, $"G{forumGroupInfo.ForumGroupId}");
            }

            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.ClearAll(forumGroupInfo.ModuleId);
            return forumGroupInfo.ForumGroupId;
        }

        public void Groups_Delete(int moduleId, int forumGroupId)
        {
            foreach (var forum in DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetForums(moduleId: moduleId).Where(f => f.ForumGroupId == forumGroupId))
            {
                DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.Forums_Delete(forum.ForumID, moduleId);
            }

            this._repositoryControllerBase.DeleteById(forumGroupId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.ClearAll(moduleId);
        }

        ForumGroupInfo IForumGroupController.GetByUrlPrefix(int moduleId, string groupPrefix)
        {
            return this.GetByUrlPrefix(moduleId, groupPrefix);
        }
    }
}
