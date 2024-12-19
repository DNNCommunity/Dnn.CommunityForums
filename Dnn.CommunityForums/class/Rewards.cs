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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    #region RewardInfo
    public class RewardInfo
    {
        public int RankId { get; set; }

        public int PortalId { get; set; }

        public int ModuleId { get; set; }

        public string RankName { get; set; }

        public int MinPosts { get; set; }

        public int MaxPosts { get; set; }

        public string Display { get; set; }
    }
    #endregion

    #region RewardController
    public class RewardController
    {
        public RewardInfo Reward_Save(RewardInfo reward)
        {
            int rankId = DataProvider.Instance().Ranks_Save(reward.PortalId, reward.ModuleId, reward.RankId, reward.RankName, reward.MinPosts, reward.MaxPosts, reward.Display);
            return this.Reward_Get(reward.PortalId, reward.ModuleId, rankId);
        }

        public void Reward_Delete(int portalId, int moduleId, int rankId)
        {
            DataProvider.Instance().Ranks_Delete(portalId, moduleId, rankId);
        }

        public RewardInfo Reward_Get(int portalId, int moduleID, int rankId)
        {
            var ri = new RewardInfo();
            IDataReader dr = DataProvider.Instance().Ranks_Get(portalId, moduleID, rankId);

            while (dr.Read())
            {
                ri.Display = dr["Display"].ToString();
                ri.MaxPosts = Convert.ToInt32(dr["MaxPosts"]);
                ri.MinPosts = Convert.ToInt32(dr["MinPosts"]);
                ri.ModuleId = moduleID;
                ri.PortalId = portalId;
                ri.RankId = Convert.ToInt32(dr["RankId"]);
                ri.RankName = Convert.ToString(dr["RankName"]);
            }

            dr.Close();

            return ri;
        }

        public List<RewardInfo> Reward_List(int portalId, int moduleId, bool useCache)
        {
            string cacheKey = string.Format(CacheKeys.Rewards, moduleId.ToString());
            List<RewardInfo> rl;

            if (useCache)
            {
                rl = (List<RewardInfo>)DataCache.SettingsCacheRetrieve(moduleId, cacheKey);
                if (rl == null)
                {
                    rl = this.Reward_List(portalId, moduleId);
                    DataCache.SettingsCacheStore(moduleId, cacheKey, rl);
                }
            }
            else
            {
                rl = this.Reward_List(portalId, moduleId);
            }

            return rl;
        }

        public List<RewardInfo> Reward_List(int portalId, int moduleId)
        {
            var rl = new List<RewardInfo>();
            IDataReader dr = DataProvider.Instance().Ranks_List(portalId, moduleId);

            dr.Read();
            dr.NextResult();

            while (dr.Read())
            {
                var ri = new RewardInfo
                {
                    Display = Convert.ToString(dr["Display"]),
                    MaxPosts = Convert.ToInt32(dr["MaxPosts"]),
                    MinPosts = Convert.ToInt32(dr["MinPosts"]),
                    ModuleId = moduleId,
                    PortalId = portalId,
                    RankId = Convert.ToInt32(dr["RankId"]),
                    RankName = Convert.ToString(dr["RankName"]),
                };
                rl.Add(ri);
            }

            dr.Close();

            return rl;
        }
    }
    #endregion
}
