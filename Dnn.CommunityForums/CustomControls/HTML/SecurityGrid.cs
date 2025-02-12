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

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Text;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;

    [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
    public class SecurityGrid
    {
        private const int permCount = 21;
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public DotNetNuke.Entities.Portals.PortalSettings PortalSettings { get; set; }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public int PortalId
        {
            get
            {
                return this.PortalSettings.PortalId;
            }
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public bool ReadOnly { get; set; }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public string ImagePath { get; set; }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo Security { get; set; }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public int PermissionsId { get; set; }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public SecurityGrid(DotNetNuke.Entities.Portals.PortalSettings ps, bool isReadOnly, string imgPath, DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo sec, int permId)
        {
            this.PortalSettings = ps;
            this.ReadOnly = isReadOnly;
            this.ImagePath = imgPath;
            this.Security = sec;
            this.PermissionsId = permId;
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Not Used.")]
        public string GetNewGrid()
        {
            // Roles
            string[] roles = this.GetSecureObjectList(this.Security, 0).Split(';');
            int[] roleIds = new int[roles.Length - 2 + 1];
            int i = 0;
            for (i = 0; i <= roles.Length - 2; i++)
            {
                if (!string.IsNullOrEmpty(roles[i]))
                {
                    roleIds[i] = Convert.ToInt32(roles[i]);
                }
            }

            Array.Sort(roleIds);
            string tmp = string.Empty;
            foreach (int n in roleIds)
            {
                tmp += n.ToString() + ";";
            }

            List<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo> pl = new List<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>();

            // litNewGrid.Text = "Roles:" & tmp

            // litNewGrid.Text &= "<br />RolesNames:" & Permissions.GetRolesNVC(tmp)
            NameValueCollection nvc = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRolesNVC(this.PortalId, tmp);
            foreach (string key in nvc.AllKeys)
            {
                DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo pi = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo();
                pi.ObjectId = key;
                pi.ObjectName = nvc[key];
                pi.Type = ObjectType.RoleId;
                pl.Add(pi);
            }

			//Users
			string users = GetSecureObjectList(Security, 1);
			string userNames = string.Empty;
			if (! (string.IsNullOrEmpty(users)))
			{ 
				foreach (string uid in users.Split(';'))
				{
					if (! (string.IsNullOrEmpty(uid)))
					{
						DotNetNuke.Entities.Users.UserInfo u = DotNetNuke.Entities.Users.UserController.Instance.GetUser(PortalId, Convert.ToInt32(uid));
						if (u != null)
						{
							DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo pi = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo();
							pi.ObjectId = u.UserID.ToString();
							pi.ObjectName = u.DisplayName;
							pi.Type = ObjectType.UserId;
							pl.Add(pi);
						}

					}
				}
			}
			

            // NOTE: These need to be presented in same order as they are in "SecureActions" enum.
			string[,] grid = new string[pl.Count + 1, permCount+3];
			i = 0;
			foreach (DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo pi in pl)
			{
				grid[i, 0] = pi.ObjectId;
				grid[i, 1] = pi.ObjectName;
				grid[i, 2] = Convert.ToString(pi.Type);
                grid[i, 3] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.View));
                grid[i, 4] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Read));
                grid[i, 5] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Create));
                grid[i, 6] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Reply));
                grid[i, 7] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Edit));
                grid[i, 8] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Delete));
                grid[i, 9] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Move));
                grid[i, 10] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Lock));
                grid[i, 11] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Pin));
                grid[i, 12] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Split));
                grid[i, 13] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Attach));
                grid[i, 14] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Poll));
                grid[i, 15] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Block));
                grid[i, 16] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Trust));
                grid[i, 17] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Subscribe));
                grid[i, 18] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Announce));
                grid[i, 19] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Tag));
                grid[i, 20] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Categorize));
                grid[i, 21] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Prioritize));
                grid[i, 22] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Moderate));
                grid[i, 23] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, this.Security.Ban));
                i += 1;
            }

            System.Type enumType = typeof(SecureActions);
            Array values = Enum.GetValues(enumType);
            StringBuilder sb = new StringBuilder();
            sb.Append("<table cellpadding=\"0\" cellspacing=\"0\"><tr><td valign=\"top\"><div class=\"afsecobjects\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\">");
            sb.Append("<tr><td class=\"afsecobjecthd\" colspan=\"2\">" + Utilities.GetSharedResource("[RESX:SecureObjects]", true) + "</td></tr>");
            string tmpObjectName = string.Empty;
            for (int x = 0; x < pl.Count; x++)
            {
                sb.Append("<tr><td style=\"width:16px;\"></td><td class=\"afsecobject\" style=\"white-space:nowrap;\"><div class=\"afsecobjecttxt\" title=\"" + grid[x, 1] + "\" onmouseover=\"this.firstChild.style.display='';\" onmouseout=\"this.firstChild.style.display='none';\"><span style=\"width:16px;height:16px;float:right;display:none;\">");
                if ((Convert.ToInt32(grid[x, 2]) == 0 && Convert.ToInt32(grid[x, 0]) > 0) | Convert.ToInt32(grid[x, 2]) > 0)
                {
                    if (!this.ReadOnly)
                    {
                        sb.Append("<img src=\"" + this.ImagePath + "mini_del.gif\" alt=\"Remove Object\" style=\"cursor:pointer;z-index:10;\" class=\"afminidel\" onclick=\"securityDelObject(this,'" + grid[x, 0] + "'," + grid[x, 2] + "," + this.PermissionsId + ");\" />");
                    }
                }

                sb.Append("</span>" + grid[x, 1]);
                sb.Append("</div></td></tr>");
            }

            sb.Append("</table></div></td><td valign=\"top\" width=\"94%\"><div class=\"afsecactions\" style=\"overflow-x:auto;overflow-y:hidden;\">");

			sb.Append("<table cellpadding=0 cellspacing=0 border=0 width=\"100%\" id=\"tblSecGrid\">");
			sb.Append("<tr>");
			string keyText = string.Empty;
			for (int td = 3; td <= permCount+2; td++)
			{
				keyText = Convert.ToString(Enum.Parse(enumType, values.GetValue(td - 3).ToString()));
				if (keyText.ToLowerInvariant() == "block")
				{
					sb.Append("<td class=\"afsecactionhd\" style=\"display:none;\">");
				}
				else
				{
					sb.Append("<td class=\"afsecactionhd\">");
				}


                sb.Append(Utilities.LocalizeControl($"[RESX:SecGrid:{keyText}]", isAdmin:true));
				sb.Append("</td>");
			}
			sb.Append("</tr>");
			for (int x = 0; x < pl.Count; x++)
			{
				sb.Append("<tr onmouseover=\"this.className='afgridrowover'\" onmouseout=\"this.className='afgridrow'\">");
				for (int r = 3; r <= permCount+2; r++)
				{
					keyText = Convert.ToString(Enum.Parse(enumType, values.GetValue(r - 3).ToString()));
					bool bState = Convert.ToBoolean(grid[x, r]); //DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPermission(ForumID, Integer.Parse(dr("ObjectId").ToString), key, Integer.Parse(dr("SecureType").ToString), dt)
					string sState = "<img src=\"" + ImagePath + "admin_stop.png\" alt=\"Disabled\" />";
					if (bState)
					{
						sState = "<img src=\"" + ImagePath + "admin_check.png\" alt=\"Enabled\" />";
					}
					if (keyText.ToLowerInvariant() == "block")
					{
						sb.Append("<td class=\"afsecactionelem\" style=\"text-align:center;display:none;\">");
					}
					else
					{
						sb.Append("<td class=\"afsecactionelem\" style=\"text-align:center;\">");
					}

                    sb.Append("<div class=\"afsectoggle\" id=\"" + grid[x, 0] + grid[x, 2] + keyText + "\" ");
                    if (!this.ReadOnly)
                    {
                        sb.Append("onclick=\"securityToggle(this," + this.PermissionsId + ",'" + grid[x, 0] + "','" + grid[x, 1] + "'," + grid[x, 2] + ",'" + keyText + "');\"");
                    }

                    sb.Append(">" + sState + "</div></td>");
                }

                sb.Append("</tr>");
            }

            sb.Append("</table></div></td></tr></table>");
            return sb.ToString();
        }

        private string GetSecureObjectList(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo s, int objectType)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetSecureObjectList(this.PortalSettings, s, (DotNetNuke.Modules.ActiveForums.ObjectType)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.ObjectType), objectType.ToString()));
        }

        private bool PermValue(int objectType, string objectId, string permSet)
        {
            if (string.IsNullOrEmpty(permSet))
            {
                return false;
            }
            else
            {
                return DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(permSet.Split('|')[objectType], objectId + ";");
            }
        }
    }
}
