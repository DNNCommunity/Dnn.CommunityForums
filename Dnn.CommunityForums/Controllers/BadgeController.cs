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
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Modules.ActiveForums.Services.Cache;

    /// <summary>
    /// Controller for managing badges in the DNN Community Forums module.
    /// </summary>
    internal class BadgeController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo, IBadgeController, BadgeController>, IBadgeController
    {
        protected override Func<IBadgeController> GetFactory()
        {
            return () => new BadgeController();
        }

        /// <summary>
        /// Gets all active badges.
        /// </summary>
        /// <returns>List of active badges.</returns>
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo> GetActiveBadges(int moduleId)
        {
            return this._repositoryControllerBase.Get().Where(b => b.ModuleId.Equals(moduleId));
        }

        public DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo GetById(int moduleId, int badgeId)
        {
            var cachekey = string.Format(CacheKeys.BadgeInfo, moduleId, badgeId);
            var badgeInfo = DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo;
            if (badgeInfo == null)
            {
                badgeInfo = this._repositoryControllerBase.GetById(badgeId, moduleId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Store(moduleId, cachekey, badgeInfo);
            }

            return badgeInfo;
        }

        public new void DeleteById<TProperty>(int moduleId, TProperty badgeId)
        {
            var cachekey = string.Format(CacheKeys.BadgeInfo, moduleId, badgeId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(moduleId, cachekey);
            this._repositoryControllerBase.DeleteById(badgeId);
        }

        public new void Delete(DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo badgeInfo)
        {
            var cachekey = string.Format(CacheKeys.BadgeInfo, badgeInfo.ModuleId, badgeInfo.BadgeId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(badgeInfo.ModuleId, cachekey);
            this._repositoryControllerBase.Delete(badgeInfo);
        }

        public new DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo Insert(DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo badgeInfo)
        {
            this._repositoryControllerBase.Insert(badgeInfo);
            return this.GetById(badgeInfo.ModuleId, badgeInfo.BadgeId);
        }

        public new DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo Update(DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo badgeInfo)
        {
            var cachekey = string.Format(CacheKeys.BadgeInfo, badgeInfo.ModuleId, badgeInfo.BadgeId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(badgeInfo.ModuleId, cachekey);
            this._repositoryControllerBase.Update(badgeInfo);
            return this.GetById(badgeInfo.ModuleId, badgeInfo.BadgeId);
        }
    }
}
