// Copyright (c) by DNN Community
//
// DNN Community licenses this file to you under the MIT license.
//
// See the LICENSE file in the project root for more information.

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System.Collections.Generic;

    using DotNetNuke.Data;

    internal interface IBadgeController : IRepository<DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo>
    {
        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo> GetActiveBadges(int moduleId);

        DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo GetById(int moduleId, int badgeId);

        void DeleteById<TProperty>(int moduleId, TProperty badgeId);

        DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo Insert(DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo badgeInfo);

        DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo Update(DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo badgeInfo);
    }
}
