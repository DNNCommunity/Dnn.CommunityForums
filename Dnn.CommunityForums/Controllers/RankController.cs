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

    using DotNetNuke.Modules.ActiveForums.Services.Cache;

    /// <summary>
    /// Controller for managing Ranks in the DNN Community Forums module.
    /// </summary>
    internal class RankController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.RankInfo, IRankController, RankController>, IRankController
    {
        protected override Func<IRankController> GetFactory()
        {
            return () => new RankController();
        }

        /// <summary>
        /// Gets all ranks.
        /// </summary>
        /// <returns>List of active badges.</returns>
        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.RankInfo> Get(int moduleId)
        {
            var cachekey = string.Format(CacheKeys.Ranks, moduleId);
            var ranks = DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Retrieve(moduleId, cachekey) as IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.RankInfo>;
            if (ranks == null)
            {
                ranks = this._repositoryControllerBase.Get(moduleId);
                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Store(moduleId, cachekey, ranks);
            }

            return ranks;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.RankInfo GetById(int moduleId, int rankId)
        {
            var cachekey = string.Format(CacheKeys.RankInfo, moduleId, rankId);
            var rankInfo = DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Retrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.RankInfo;
            if (rankInfo == null)
            {
                rankInfo = this._repositoryControllerBase.GetById(id: rankId, scopeValue: moduleId);
                if (rankInfo == null)
                {
                    rankInfo = this._repositoryControllerBase.GetById(id: rankId);
                }

                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Store(moduleId, cachekey, rankInfo);
            }

            return rankInfo;
        }

        public new void DeleteById<TProperty>(int moduleId, TProperty rankId)
        {
            var cachekey = string.Format(CacheKeys.Ranks, moduleId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(moduleId, cachekey);
            cachekey = string.Format(CacheKeys.RankInfo, moduleId, rankId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(moduleId, cachekey);
            this._repositoryControllerBase.DeleteById(rankId);
        }

        public new void Delete(DotNetNuke.Modules.ActiveForums.Entities.RankInfo rankInfo)
        {
            var cachekey = string.Format(CacheKeys.Ranks, rankInfo.ModuleId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(rankInfo.ModuleId, cachekey);
            cachekey = string.Format(CacheKeys.RankInfo, rankInfo.ModuleId, rankInfo.RankId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(rankInfo.ModuleId, cachekey);
            this._repositoryControllerBase.Delete(rankInfo);
        }

        public new DotNetNuke.Modules.ActiveForums.Entities.RankInfo Insert(DotNetNuke.Modules.ActiveForums.Entities.RankInfo rankInfo)
        {
            var cachekey = string.Format(CacheKeys.Ranks, rankInfo.ModuleId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(rankInfo.ModuleId, cachekey);
            cachekey = string.Format(CacheKeys.RankInfo, rankInfo.ModuleId, rankInfo.RankId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(rankInfo.ModuleId, cachekey);
            this._repositoryControllerBase.Insert(rankInfo);
            return this.GetById(rankInfo.ModuleId, rankInfo.RankId);
        }

        public new DotNetNuke.Modules.ActiveForums.Entities.RankInfo Save(DotNetNuke.Modules.ActiveForums.Entities.RankInfo rankInfo)
        {
            var cachekey = string.Format(CacheKeys.Ranks, rankInfo.ModuleId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(rankInfo.ModuleId, cachekey);
            cachekey = string.Format(CacheKeys.RankInfo, rankInfo.ModuleId, rankInfo.RankId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(rankInfo.ModuleId, cachekey);
            this._repositoryControllerBase.Save(rankInfo, rankInfo.RankId);
            return this.GetById(rankInfo.ModuleId, rankInfo.RankId);
        }

        public new DotNetNuke.Modules.ActiveForums.Entities.RankInfo Update(DotNetNuke.Modules.ActiveForums.Entities.RankInfo rankInfo)
        {
            var cachekey = string.Format(CacheKeys.Ranks, rankInfo.ModuleId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(rankInfo.ModuleId, cachekey);
            cachekey = string.Format(CacheKeys.RankInfo, rankInfo.ModuleId, rankInfo.RankId);
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(rankInfo.ModuleId, cachekey);
            this._repositoryControllerBase.Update(rankInfo);
            return this.GetById(rankInfo.ModuleId, rankInfo.RankId);
        }
    }
}
