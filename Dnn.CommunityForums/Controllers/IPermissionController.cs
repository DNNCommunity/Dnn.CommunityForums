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
    using DotNetNuke.Data;

    internal interface IPermissionController : IRepository<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
    {
        DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo GetById(int permissionsId, int moduleId);

        string SavePermSet(int moduleId, int permissionsId, DotNetNuke.Modules.ActiveForums.SecureActions requestedAccess, string permSet);

        void DeleteById<TProperty>(TProperty permissionsId, int moduleId);

        void Delete(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo);

        DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo Insert(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo);

        DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo Update(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo permissionInfo);

        DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo CreateAdminPermissions(DotNetNuke.Entities.Portals.PortalSettings portalSettings, int moduleId);

        DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo CreateDefaultPermissions(DotNetNuke.Entities.Portals.PortalSettings portalSettings, int moduleId);

        void RemoveIfUnused(int permissionsId, int moduleId);

        void RemoveUnused(int moduleId);

        void UpdateSecurityForSocialGroupForum(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum);
    }
}
