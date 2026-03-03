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

    using DotNetNuke.Data;

    internal interface IUserBadgeController : IRepository<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo>
    {
        DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo GetById(int portalId, int moduleId, int badgeId);
        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetForBadge(int moduleId, int badgeId);

        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetDistinctForUser(int portalId, int moduleId, int userId);

        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetForUser(int portalId, int moduleId, int userId);

        DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo GetLatestForUserAndBadge(int portalId, int moduleId, int userId, int badgeId);

        int BadgeCount(int portalId, int moduleId, int userId, int badgeId);

        void AssignUserBadge(int portalId, int moduleId, int userId, int badgeId, string requestUrl);
        void UnassignUserBadge(int portalId, int moduleId, int userId, int badgeId, DateTime dateAssigned);

        bool AssignUserBadgeAfterAction(int portalId, int moduleId, int userId, int badgeId, DateTime dateAssigned, string requestUrl);
    }
}
