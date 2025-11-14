// Copyright (c) by DNN Community
//
// DNN Community licenses this file to you under the MIT license.
//
// See the LICENSE file in the project root for more information.
//
// Badge is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this Badge notice shall be included in all copies or substantial portions
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
    using DotNetNuke.Modules.ActiveForums.Entities;
    using static DotNetNuke.Entities.Modules.DesktopModuleInfo;

    /// <summary>
    /// Controller for managing badges in the DNN Community Forums module.
    /// </summary>
    internal class BadgeController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo>
    {
        internal override string cacheKeyTemplate => CacheKeys.BadgeInfo;

        /// <summary>
        /// Gets all active badges.
        /// </summary>
        /// <returns>List of active badges.</returns>
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo> GetActiveBadges(int moduleId)
        {
            return this.Get().Where(b => b.ModuleId.Equals(moduleId));
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo GetById(int badgeId, int moduleId)
        {
            var cachekey = this.GetCacheKey(moduleId: moduleId, id: badgeId);
            DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo badgeInfo = DataCache.SettingsCacheRetrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo;
            if (badgeInfo == null)
            {
                badgeInfo = base.GetById(badgeId, moduleId);
                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheStore(moduleId, cachekey, badgeInfo);
            }

            return badgeInfo;
        }

        internal new void DeleteById<TProperty>(TProperty badgeId, int moduleId)
        {
            var cachekey = this.GetCacheKey(moduleId: moduleId, id: badgeId);
            DataCache.SettingsCacheClear(moduleId, cachekey);
            this.DeleteById(badgeId);
        }

        internal new void Delete(DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo badgeInfo)
        {
            var cachekey = this.GetCacheKey(moduleId: badgeInfo.ModuleId, id: badgeInfo.BadgeId);
            DataCache.SettingsCacheClear(badgeInfo.ModuleId, cachekey);
            base.Delete(badgeInfo);
        }

        internal new DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo Insert(DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo badgeInfo)
        {
            base.Insert(badgeInfo);
            return this.GetById(badgeInfo.BadgeId, badgeInfo.ModuleId);
        }

        internal new DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo Update(DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo badgeInfo)
        {
            var cachekey = this.GetCacheKey(moduleId: badgeInfo.ModuleId, id: badgeInfo.BadgeId);
            DataCache.SettingsCacheClear(badgeInfo.ModuleId, cachekey);
            base.Update(badgeInfo);
            return this.GetById(badgeInfo.BadgeId, badgeInfo.ModuleId);
        }
    }
}
