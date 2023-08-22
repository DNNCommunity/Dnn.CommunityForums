//
// Community Forums
// Copyright (c) 2013-2021
// by DNN Community
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
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using DotNetNuke.Modules.ActiveForums.Entities;
using System.Linq;
using System.Web.Services.Description;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Modules.ActiveForums
{

    #region PermissionCollection
    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
    public class PermissionCollection : CollectionBase, ICollection, IList
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        private PermissionInfo _Item;
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public void CopyTo(Array array, int index)
        {
            List.CopyTo(array, index);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public bool IsSynchronized
        {
            get
            {
                return List.IsSynchronized;
            }
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public object SyncRoot
        {
            get
            {
                return List.SyncRoot;
            }
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int Add(PermissionInfo value)
        {
            return List.Add(value);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public bool Contains(PermissionInfo value)
        {
            return List.Contains(value);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int IndexOf(PermissionInfo value)
        {
            return List.IndexOf(value);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public void Insert(int index, PermissionInfo value)
        {
            List.Insert(index, value);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public bool IsFixedSize
        {
            get
            {
                return List.IsFixedSize;
            }
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public bool IsReadOnly
        {
            get
            {
                return List.IsReadOnly;
            }
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public PermissionInfo this[int index]
        {
            get
            {
                return _Item;
            }
            set
            {
                _Item = value;
            }
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public void Remove(PermissionInfo value)
        {
            List.Remove(value);
        }
    }
    #endregion

    #region Permissions
    public class Permissions
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionsController.HasPerm().")]
        public static bool HasPerm(string AuthorizedRoles, int UserId, int PortalId)
        {
            return Controllers.PermissionsController.HasPerm(AuthorizedRoles, UserId, PortalId);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionsController.HasPerm().")]
        public static bool HasPerm(string AuthorizedRoles, string UserPermSet)
        {
            return Controllers.PermissionsController.HasPerm(AuthorizedRoles, UserPermSet);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionsController.RemovePermFromSet().")]
        public static string RemovePermFromSet(string objectId, int objectType, string PermissionSet)
        {
            return Controllers.PermissionsController.RemovePermFromSet(objectId, objectType, PermissionSet);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionsController.HasPerm().")]
        public static string AddPermToSet(string objectId, int objectType, string PermissionSet)
        {
            return Controllers.PermissionsController.AddPermToSet(objectId, objectType, PermissionSet);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionsController().GetRoleIds().")]
        public static string GetRoleIds(string[] Roles, int PortalId)
        {
            return Controllers.PermissionsController.GetRoleIds(PortalId, Roles);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionsController().HasRequiredPermission().")]
        public static bool HasRequiredPerm(string[] AuthorizedRoles, string[] UserRoles)
        {
            return Controllers.PermissionsController.HasRequiredPerm(AuthorizedRoles, UserRoles);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionsController().HasAccess().")]
        public static bool HasAccess(string AuthorizedRoles, string UserRoles)
        {
            return Controllers.PermissionsController.HasAccess(AuthorizedRoles, UserRoles);
        }
    }
    #endregion
}