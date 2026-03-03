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
    using System.Collections.Generic;

    using DotNetNuke.Data;

    internal interface IForumUserController : IRepository<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo>
    {
        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo> GetActiveUsers(int portalId, int moduleId);

        DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetByUserId(int portalId, int moduleId, int userId);

        DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo DNNGetCurrentUser(int portalId, int moduleId);

        DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo GetUserFromHttpContext(int portalId, int moduleId);

        DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo Save(DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser);

        string GetUsersOnline(DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Modules.ActiveForums.ModuleSettings mainSettings, int moduleId, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser);
    }
}
