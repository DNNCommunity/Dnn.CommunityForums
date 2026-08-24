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

    using DotNetNuke.Modules.ActiveForums.Services.Cache;

    #region RewardInfo
    [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Entities.RankInfo()")]
    public class RewardInfo
    {
        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Entities.RankInfo()")]
        public int RankId { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Entities.RankInfo()")]
        public int PortalId { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Entities.RankInfo()")]
        public int ModuleId { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Entities.RankInfo()")]
        public string RankName { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Entities.RankInfo()")]
        public int MinPosts { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Entities.RankInfo()")]
        public int MaxPosts { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Entities.RankInfo()")]
        public string Display { get; set; }
    }
    #endregion

    #region RewardController
    [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Controllers.RankController()")]
    public class RewardController
    {
        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Controllers.RankController()")]
        public RewardInfo Reward_Save(RewardInfo reward) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Controllers.RankController()")]
        public void Reward_Delete(int portalId, int moduleId, int rankId) => throw new NotImplementedException();

        public RewardInfo Reward_Get(int portalId, int moduleID, int rankId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Controllers.RankController()")]
        public List<RewardInfo> Reward_List(int portalId, int moduleId, bool useCache) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 11.00.00. Moved to Controllers.RankController()")]
        public List<RewardInfo> Reward_List(int portalId, int moduleId) => throw new NotImplementedException();
    }
    #endregion
}
