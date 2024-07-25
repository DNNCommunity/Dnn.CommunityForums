//
// Community Forums
// Copyright (c) 2013-2024
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
using System.Linq;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Modules.ActiveForums
{
    public enum SecureActions : int
    {
        View,
        Read,
        Create,
        Reply,
        Edit,
        Delete,
        Lock,
        Pin,
        Attach,
        Poll,
        Block,
        Trust,
        Subscribe,
        Announce,
        Tag,
        Categorize,
        Prioritize,
        ModApprove,
        ModMove,
        ModSplit,
        ModDelete,
        ModUser,
        ModEdit,
        ModLock,
        ModPin
    }

    public enum ObjectType : int
    {
        RoleId,
        UserId,
        GroupId
    }

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
    public enum SecureType : int
    {
        ForumGroup,
        Forum
    }

    [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo.")]
    public partial class PermissionInfo : DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo { }

    [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
    public class PermissionCollection : CollectionBase, ICollection, IList
    {
        private PermissionInfo _Item;

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public void CopyTo(Array array, int index) => List.CopyTo(array, index);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public bool IsSynchronized => List.IsSynchronized;

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public object SyncRoot => List.SyncRoot;

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public int Add(PermissionInfo value) => List.Add(value);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public bool Contains(PermissionInfo value) => List.Contains(value);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public int IndexOf(PermissionInfo value) => List.IndexOf(value);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public void Insert(int index, PermissionInfo value) => List.Insert(index, value);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public bool IsFixedSize => List.IsFixedSize;

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public bool IsReadOnly => List.IsReadOnly;

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public PermissionInfo this[int index] { get => _Item; set => _Item = value; }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public void Remove(PermissionInfo value) => List.Remove(value);
    }

    [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
    public class Permissions
    {
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static bool HasPerm(string AuthorizedRoles, int UserId, int PortalId) => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(AuthorizedRoles, UserId, PortalId);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static bool HasPerm(string AuthorizedRoles, string UserPermSet) => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(AuthorizedRoles, UserPermSet);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static string RemovePermFromSet(string objectId, int objectType, string PermissionSet) => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemovePermFromSet(objectId, objectType, PermissionSet);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static string AddPermToSet(string objectId, int objectType, string PermissionSet) => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(objectId, objectType, PermissionSet);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static string GetRoleIds(string[] Roles, int PortalId) => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(PortalId, Roles);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static bool HasRequiredPerm(string[] AuthorizedRoles, string[] UserRoles) => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(AuthorizedRoles, UserRoles);

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.")]
        public static bool HasAccess(string AuthorizedRoles, string UserRoles)
        { return DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(AuthorizedRoles, UserRoles);
        }

        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo.")]
        public partial class PermissionInfo : DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo { }
    } 
}