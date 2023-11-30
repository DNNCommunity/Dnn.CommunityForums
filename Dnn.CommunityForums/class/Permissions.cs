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
using System.Linq;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Modules.ActiveForums
{
	

#region Permissions
	public class Permissions
	{
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

		public static string GetRoleIds(string[] Roles, int PortalId)
		{
			string RoleIds = (string)DataCache.SettingsCacheRetrieve(-1, string.Format(CacheKeys.RoleIDs, PortalId));
			if (string.IsNullOrEmpty(RoleIds))
			{ 
                foreach (DotNetNuke.Security.Roles.RoleInfo ri in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: PortalId))
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
				DataCache.SettingsCacheStore(-1, string.Format(CacheKeys.RoleIDs, PortalId),RoleIds);
			}

		    return RoleIds;
		}
		
        

    }
#endregion

}