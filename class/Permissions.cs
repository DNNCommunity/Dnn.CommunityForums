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
using System.Collections.Specialized;
using System.Linq;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Modules.ActiveForums
{
	public enum SecureActions: int
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

	public enum ObjectType: int
	{
		RoleId,
		UserId,
		GroupId
	}

	public enum SecureType: int
	{
		ForumGroup,
		Forum
	}

#region PermissionInfo
	public class PermissionInfo
	{
#region Private Members

	    #endregion
#region Public Properties

	    public string View { get; set; }
        public string Read { get; set; }
        public string Create { get; set; }
        public string Reply { get; set; }
        public string Edit { get; set; }
        public string Delete { get; set; }
        public string Lock { get; set; }
        public string Pin { get; set; }
        public string Attach { get; set; }
        public string Poll { get; set; }
        public string Block { get; set; }
        public string Trust { get; set; }
        public string Subscribe { get; set; }
        public string Announce { get; set; }
        public string ModApprove { get; set; }
        public string ModMove { get; set; }
        public string ModSplit { get; set; }
        public string ModDelete { get; set; }
        public string ModUser { get; set; }
        public string ModEdit { get; set; }
		public string ModLock { get; set; }
        public string ModPin { get; set; }
        public int UserTrustLevel { get; set; }
        public ObjectType Type { get; set; }
        public string ObjectId { get; set; }
        public string ObjectName { get; set; }
        public string Tag {get; set;}
		public string Categorize {get; set;}
		public string Prioritize {get; set;}
#endregion
	}
#endregion

#region PermissionCollection
	public class PermissionCollection : CollectionBase, ICollection, IList
	{
        private PermissionInfo _Item;

		public void CopyTo(Array array, int index)
		{
			List.CopyTo(array, index);
		}

		public bool IsSynchronized
		{
			get
			{
				return List.IsSynchronized;
			}
		}

		public object SyncRoot
		{
			get
			{
				return List.SyncRoot;
			}
		}
		
		public int Add(PermissionInfo value)
		{
			return List.Add(value);
		}

        public bool Contains(PermissionInfo value)
		{
			return List.Contains(value);
		}

		public int IndexOf(PermissionInfo value)
		{
			return List.IndexOf(value);
		}

		public void Insert(int index, PermissionInfo value)
		{
			List.Insert(index, value);
		}

		public bool IsFixedSize
		{
			get
			{
				return List.IsFixedSize;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return List.IsReadOnly;
			}
		}

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

		public void Remove(PermissionInfo value)
		{
			List.Remove(value);
		}

	}
#endregion

#region Permissions
	public class Permissions
	{
		public static bool HasPerm(string AuthorizedRoles, int UserId, int PortalId)
		{
			string userRoles;
			userRoles = UserRolesDictionary.GetRoles(UserId.ToString() + PortalId.ToString());
			if (string.IsNullOrEmpty(userRoles))
			{
				var uc = new Security.Roles.RoleController();
				string[] roles = DotNetNuke.Entities.Users.UserController.GetUserById(PortalId, UserId).Roles;
				string roleIds = GetRoleIds(roles, PortalId);
				userRoles = string.Concat(roleIds, "|", UserId, "|", string.Empty, "|");
				UserRolesDictionary.AddRoles(UserId.ToString() + PortalId.ToString(), userRoles);
			}

			if (string.IsNullOrEmpty(userRoles))
			{
				return false;
			}

		    return HasPerm(AuthorizedRoles, userRoles);
		}

		public static bool HasPerm(string AuthorizedRoles, string UserPermSet)
		{
			if (string.IsNullOrEmpty(AuthorizedRoles) || string.IsNullOrEmpty(UserPermSet))
			{
				return false;
			}

			string[] permSet = AuthorizedRoles.Split('|');
			string[] userSet = UserPermSet.Split('|');
			//Authorized
			string[] authRoles = permSet[0].Split(';');
			string[] userRoles = userSet[0].Split(';');

		    if (HasRequiredPerm(authRoles, userRoles))
		    {
		        return true;
		    }

		    if (! (string.IsNullOrEmpty(permSet[1])))
			{
				string[] authUsers = permSet[1].Split(';');
				if (! (string.IsNullOrEmpty(userSet[1])))
				{
					string[] userIds = userSet[1].Split(';');
				    if (HasRequiredPerm(authUsers, userIds))
				    {
				        return true;
				    }
				}
			}

			if (! (string.IsNullOrEmpty(permSet[2])))
			{
				string[] authGroups = permSet[2].Split(';');
				if (! (string.IsNullOrEmpty(userSet[2])))
				{
					string[] userGroups = userSet[2].Split(';');
				    if (HasRequiredPerm(authGroups, userGroups))
				    {
				        return true;
				    }
				}
			}

			return false;
		}

		public static string RemovePermFromSet(string objectId, int objectType, string PermissionSet)
		{
			if (string.IsNullOrEmpty(PermissionSet))
			{
				return string.Empty;
			}

			string newSet = PermissionSet;
			string[] permSet = newSet.Split('|');
			string permSection = permSet[objectType];
			string newSection = string.Empty;

			foreach (string s in permSection.Split(';'))
			{
				if (! (string.IsNullOrEmpty(s)))
				{
					if (s != objectId)
					{
						newSection += string.Concat(s, ";");
					}
				}
			}

			permSet[objectType] = newSection;
			if (permSet[0] != null)
			{
				newSet = string.Concat(permSet[0], "|");
			}
			else
			{
				newSet = "|";
			}

			if (permSet[1] != null)
			{
				newSet += string.Concat(permSet[1], "|");
			}
			else
			{
				newSet += "|";
			}

			if (permSet[2] != null)
			{
				newSet += string.Concat(permSet[2], "|");
			}
			else
			{
				newSet += "|";
			}

			return newSet;
		}

		public static string AddPermToSet(string objectId, int objectType, string PermissionSet)
		{
			string newSet = RemovePermFromSet(objectId, objectType, PermissionSet);
			string[] permSet = newSet.Split('|');
			permSet[objectType] += string.Concat(objectId, ";");
			newSet = string.Concat(permSet[0] + "|" + permSet[1] + "|" + permSet[2], "|");

			return newSet;
		}

		internal static bool RemoveObjectFromAll(string objectId, int objectType, int PermissionsId)
		{
			var enumType = typeof(SecureActions);
			Array values = Enum.GetValues(enumType);
			var db = new Data.Common();
			for (int i = 0; i < values.Length; i++)
			{
				string text = Convert.ToString(Enum.Parse(enumType, values.GetValue(i).ToString()));
				string permSet = db.GetPermSet(PermissionsId, text);
				permSet = RemovePermFromSet(objectId, objectType, permSet);
				db.SavePermSet(PermissionsId, text, permSet);
			}

			return true;
		}

		internal static void CreateDefaultSets(int PortalId, int PermissionsId)
		{
			var db = new Data.Common();
			var rc = new Security.Roles.RoleController();
			Security.Roles.RoleInfo ri;
			ri = rc.GetRoleByName(PortalId, "Registered Users");
			string permSet;

			if (ri != null)
			{
				permSet = db.GetPermSet(PermissionsId, "View");
				permSet = AddPermToSet(ri.RoleID.ToString(), 0, permSet);
				db.SavePermSet(PermissionsId, "View", permSet);
			    permSet = db.GetPermSet(PermissionsId, "Read");
				permSet = AddPermToSet(ri.RoleID.ToString(), 0, permSet);
				db.SavePermSet(PermissionsId, "Read", permSet);
			    permSet = db.GetPermSet(PermissionsId, "Create");
				permSet = AddPermToSet(ri.RoleID.ToString(), 0, permSet);
				db.SavePermSet(PermissionsId, "Create", permSet);
			    permSet = db.GetPermSet(PermissionsId, "Reply");
				permSet = AddPermToSet(ri.RoleID.ToString(), 0, permSet);
				db.SavePermSet(PermissionsId, "Reply", permSet);
			}

			permSet = db.GetPermSet(PermissionsId, "View");
			permSet = AddPermToSet("-3", 0, permSet);
			db.SavePermSet(PermissionsId, "View", permSet);
		    permSet = db.GetPermSet(PermissionsId, "Read");
			permSet = AddPermToSet("-3", 0, permSet);
			db.SavePermSet(PermissionsId, "Read", permSet);
		    permSet = db.GetPermSet(PermissionsId, "View");
			permSet = AddPermToSet("-1", 0, permSet);
			db.SavePermSet(PermissionsId, "View", permSet);
		    permSet = db.GetPermSet(PermissionsId, "Read");
			permSet = AddPermToSet("-1", 0, permSet);
			db.SavePermSet(PermissionsId, "Read", permSet);
		}

		public static string GetRoleIds(string[] Roles, int PortalId)
		{
			int i = 0;
			string RoleIds = string.Empty;
			var db = new Data.Common();
			object obj = null; 
			if (obj == null)
			{
				foreach (Security.Roles.RoleInfo ri in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(PortalId))
				{
					string roleName = ri.RoleName;
					foreach (string role in Roles)
					{
						if (!string.IsNullOrEmpty(role))
						{
							if (roleName == role)
							{
								RoleIds += string.Concat(ri.RoleID.ToString(), ";");
								break;
							}
						}
					}
				}
			}

		    return RoleIds;
		}

		internal static NameValueCollection GetRolesNVC(string Roles, int PortalId)
		{
			try
			{
				var nvc = new NameValueCollection();
				string RoleNames = string.Empty;
				int i = 0;
				string roleName;
				foreach (string role in Roles.Split(new[] {';'}))
				{
					if (!string.IsNullOrEmpty(role))
					{
						switch (role)
						{
							case "-1":
								nvc.Add("-1", Common.Globals.glbRoleAllUsersName);
								break;
							case "-3":
								nvc.Add("-3", Common.Globals.glbRoleUnauthUserName);
								break;
							default:
								roleName = GetRole(role, PortalId);
								if (roleName != null)
								{
									nvc.Add(role, roleName);
								}
								break;
						}
					}
				}

				return nvc;
			}
			catch (Exception ex)
			{
				Exceptions.LogException(ex);
				return null;
			}
		}

		internal static string GetRoleNames(string Roles, int PortalId)
		{
			try
			{
				string RoleNames = string.Empty;
				int i = 0;
				string roleName;
				foreach (string role in Roles.Split(new[] {';'}))
				{
					if (!string.IsNullOrEmpty(role))
					{
						switch (role)
						{
							case "-1":
								RoleNames = string.Concat(RoleNames + Common.Globals.glbRoleAllUsersName, ";");
								break;
							case "-3":
								RoleNames = string.Concat(RoleNames + Common.Globals.glbRoleUnauthUserName, ";");
								break;
							default:
								roleName = GetRole(role, PortalId);
								if (roleName != null)
								{
									RoleNames = string.Concat(RoleNames + roleName, ";");
								}
								break;
						}
					}
				}

				return RoleNames;
			}
			catch (Exception ex)
			{
				Exceptions.LogException(ex);
				return string.Empty;
			}
		}

		private static string GetRole(string role, int PortalId)
		{
			return GetRoles(PortalId).Where(r => r.RoleName == role).Select(r=> r.RoleName).FirstOrDefault();
		}

		private static System.Collections.Generic.IList<DotNetNuke.Security.Roles.RoleInfo> GetRoles(int PortalId)
		{
			object obj = DataCache.CacheRetrieve(string.Concat(PortalId, "RoleNames"));
			System.Collections.Generic.IList< DotNetNuke.Security.Roles.RoleInfo> roleNames;
			if (obj == null)
			{
				roleNames = DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: PortalId);
				DataCache.CacheStore(string.Concat(PortalId, "RoleNames"), roleNames);
			}
			else
			{
				roleNames = (System.Collections.Generic.IList<DotNetNuke.Security.Roles.RoleInfo>)obj;
			}

			return roleNames;
		}

		public static bool HasRequiredPerm(string[] AuthorizedRoles, string[] UserRoles)
		{
			bool bolAuth = false;
			if (UserRoles != null)
			{
				foreach (string role in AuthorizedRoles)
				{
					if (! (string.IsNullOrEmpty(role)))
					{
						foreach (string AuthRole in UserRoles)
						{
							if (! (string.IsNullOrEmpty(AuthRole)))
							{
                                //TODO: verify that this logic is correct
								if (role == AuthRole && role != string.Empty && AuthRole != string.Empty)
								{
									bolAuth = true;
									break;
								}
							}
						}
						if (bolAuth)
						{
							break;
						}
					}
				}
				return bolAuth;
			}

		    return false;
		}
		
		public static bool HasAccess(string AuthorizedRoles, string UserRoles)
		{
			bool bolAuth = false;
			if (UserRoles != null)
			{
				foreach (string role in AuthorizedRoles.Split(new[] {';'}))
				{
					foreach (string AuthRole in UserRoles.Split(new[] {';'}))
					{
						if (role == AuthRole && role != "" && AuthRole != "")
						{
							bolAuth = true;
							break;
						}
					}
					if (bolAuth)
					{
						break;
					}
				}

				return bolAuth;
			}

		    return false;
		}
    }
#endregion

}