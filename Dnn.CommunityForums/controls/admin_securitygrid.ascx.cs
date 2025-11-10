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
    using System.Web.UI;

    using DotNetNuke.Security.Roles;

    public partial class admin_securitygrid : ActiveAdminBase
    {
        public string imgOn;
        public string imgOff;

        public bool ReadOnly { get; set; } = false;

        public DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo Perms { get; set; }

        public int PermissionsId { get; set; } = -1;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cbSecurityToggle.CallbackEvent += this.cbSecurityToggle_Callback;
            this.cbSecGrid.CallbackEvent += this.cbSecGrid_Callback;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.imgOn = this.Page.ResolveUrl(Globals.ModulePath + "images/admin_check.png");
            this.imgOff = this.Page.ResolveUrl(Globals.ModulePath + "images/admin_stop.png");
            this.BindRoles();
            if (this.ReadOnly)
            {
                this.gridActions.Visible = false;
            }

            this.BuildNewGrid(this.Perms, this.PermissionsId);
        }

        private void BindRoles()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<select id=\"drpSecRoles\" class=\"amcptxtbx\" style=\"width:150px;\">");
            sb.Append("<option value=\"\">[RESX:DropDownDefault]</option>");
            sb.Append($"<option value=\"{DotNetNuke.Common.Globals.glbRoleAllUsers}\">{DotNetNuke.Common.Globals.glbRoleAllUsersName}</option>");
            sb.Append($"<option value=\"{DotNetNuke.Common.Globals.glbRoleUnauthUser}\">{DotNetNuke.Common.Globals.glbRoleUnauthUserName}</option>");
            foreach (DotNetNuke.Security.Roles.RoleInfo ri in DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoles(this.PortalSettings))
            {
                sb.Append("<option value=\"" + ri.RoleID + "\">" + ri.RoleName + "</option>");
            }

            sb.Append("</select>");
            this.litRoles.Text = sb.ToString();
        }

        private void BuildNewGrid(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo security, int permissionsId)
        {
            // Roles
            string[] roles = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetSecureObjectList(this.PortalSettings, security).Split(';');
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

            NameValueCollection nvc = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRolesNVC(this.PortalSettings, tmp);
            foreach (string key in nvc.AllKeys)
            {
                DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo pi = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo();
                pi.ObjectId = key;
                pi.ObjectName = nvc[key];
                if (string.IsNullOrEmpty(pi.ObjectName))
                {
                    pi.ObjectName = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(portalId: this.PortalId, roleId: Convert.ToInt32(key)).RoleName;
                }

                pl.Add(pi);
            }

            var gridSecurityActions = new string[][,]
                {
                    new string[,]
                    {
                        { Enum.GetName(typeof(SecureActions), SecureActions.View), security.View, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Read), security.Read, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Create), security.Create, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Reply), security.Reply, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Edit), security.Edit, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Delete), security.Delete, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Subscribe), security.Subscribe, },
                    },
                    new string[,]
                    {
                        { Enum.GetName(typeof(SecureActions), SecureActions.Attach), security.Attach, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Poll), security.Poll, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Announce), security.Announce, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Categorize), security.Categorize, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Prioritize), security.Prioritize, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Tag), security.Tag, },
                    },
                    new string[,]
                    {
                        { Enum.GetName(typeof(SecureActions), SecureActions.Moderate), security.Moderate, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.ManageUsers), security.ManageUsers, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Move), security.Move, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Lock), security.Lock, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Pin), security.Pin, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Split), security.Split, },
                        { Enum.GetName(typeof(SecureActions), SecureActions.Trust), security.Trust, },
                    },
                };

            StringBuilder sb = new StringBuilder();

            for (int gridIndex = 0; gridIndex < 3; gridIndex++)
            {
                string gridHeader = string.Empty;
                switch (gridIndex)
                {
                    case 0:
                        gridHeader = Utilities.GetSharedResource("[RESX:SecurityGrid:Basics]", isAdmin: true);
                        break;
                    case 1:
                        gridHeader = Utilities.GetSharedResource("[RESX:SecurityGrid:Advanced]", isAdmin: true);
                        break;
                    case 2:
                        gridHeader = Utilities.GetSharedResource("[RESX:SecurityGrid:SuperUser]", isAdmin: true);
                        break;
                }

                i = 0;
                string[,] grid = new string[pl.Count + 1, 2 + gridSecurityActions[gridIndex].GetUpperBound(0) + 1];

                sb.Append($"<h6>{gridHeader}</h6>");
                sb.Append("<table class=\"dcf-sec-grid\"><tr><td class=\"dcf-sec-grid-roles\"><div class=\"afsecobjects\"><table>");
                sb.Append("<tr><td class=\"afsecobjecthd\" colspan=\"2\">" + Utilities.GetSharedResource("[RESX:SecureObjects]", true) + "</td></tr>");

                foreach (DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo pi in pl)
                {
                    grid[i, 0] = pi.ObjectId;
                    grid[i, 1] = pi.ObjectName;
                    for (int j = gridSecurityActions[gridIndex].GetLowerBound(0); j <= gridSecurityActions[gridIndex].GetUpperBound(0); j++)
                    {
                        grid[i, j + 2] = Convert.ToString(this.PermValue(pi.ObjectId, gridSecurityActions[gridIndex][j, 1]));
                    }

                    sb.Append("<tr><td style=\"width:16px;\"></td><td class=\"afsecobject\" style=\"white-space:nowrap;\"><div class=\"afsecobjecttxt\" title=\"" + grid[i, 1] + "\" onmouseover=\"this.firstChild.style.display='';\" onmouseout=\"this.firstChild.style.display='none';\"><span style=\"width:16px;height:16px;float:right;display:none;\">");
                    if (Convert.ToInt32(grid[i, 0]) > 0)
                    {
                        if (!this.ReadOnly)
                        {
                            sb.Append("<img src=\"" + this.Page.ResolveUrl(Globals.ModulePath + "images/mini_del.gif") + "\" alt=\"Remove Object\" style=\"cursor:pointer;z-index:10;\" class=\"afminidel\" onclick=\"securityDelObject(this,'" + grid[i, 0] + "'," + permissionsId + ");\" />");
                        }
                    }

                    sb.Append("</span>" + grid[i, 1]);
                    sb.Append("</div></td></tr>");
                    i += 1;
                }

                sb.Append("</table></div></td><td class=\"dcf-sec-grid-rights\"><div class=\"afsecactions\">");
                sb.Append("<table  id=\"tblSecGrid" + gridIndex + "\">");
                sb.Append("<tr>");
                string keyText;
                for (int td = gridSecurityActions[gridIndex].GetLowerBound(0); td <= gridSecurityActions[gridIndex].GetUpperBound(0); td++)
                {
                    keyText = gridSecurityActions[gridIndex][td, 0];
                    if (!string.IsNullOrEmpty(keyText))
                    {
                        if (keyText.ToLowerInvariant() == "block")
                        {
                            sb.Append("<td class=\"afsecactionhd\" style=\"display:none;\">");
                        }
                        else
                        {
                            sb.Append("<td class=\"afsecactionhd\" style=\"text-align:center;\">");
                        }

                        sb.Append($"<a href=\"#\" class=\"dcf-controlpanel-tooltip\" data-tooltip=\"[RESX:Tips:SecGrid:{keyText}]\">");
                        sb.Append(Utilities.LocalizeControl($"[RESX:SecGrid:{keyText}]", isAdmin: true));
                        sb.Append("</a>");
                        sb.Append("</td>");
                    }
                }

                sb.Append("</tr>");
                for (int x = 0; x < pl.Count; x++)
                {
                    sb.Append("<tr onmouseover=\"this.className='afgridrowover'\" onmouseout=\"this.className='afgridrow'\">");
                    for (int r = gridSecurityActions[gridIndex].GetLowerBound(0) + 2; r <= gridSecurityActions[gridIndex].GetUpperBound(0) + 2; r++)
                    {
                        keyText = gridSecurityActions[gridIndex][r - 2, 0];
                        sb.Append("<td class=\"afsecactionelem\" style=\"text-align:center;\">");
                        sb.Append($"<div class=\"afsectoggle\" id=\"{grid[x, 0]}{keyText}\"");
                        if (!this.ReadOnly)
                        {
                            sb.Append($"onclick=\"securityToggle(this,{permissionsId},'{grid[x, 0]}','{grid[x, 1]}','{keyText}');\"");
                        }

                        sb.Append(">" + (Convert.ToBoolean(grid[x, r]) ? $"<img src=\"{this.imgOn}\" alt=\"Enabled\" />" : $"<img src=\"{this.imgOff}\" alt=\"Disabled\" />") + "</div></td>");
                    }

                    sb.Append("</tr>");
                }

                sb.Append("</table></div></td></tr></table>");
                this.litSecGrid.Text = sb.ToString();
            }

            this.litSecGrid.Text = sb.ToString();
        }

        private bool PermValue(string objectId, string permSet)
        {
            if (string.IsNullOrEmpty(permSet))
            {
                return false;
            }
            return DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(permSet, objectId + ";");
        }

        private void cbSecurityToggle_Callback(object sender, CallBackEventArgs e)
        {
            string action = e.Parameters[0].ToString();
            int pId = this.PermissionsId;
            string secId = e.Parameters[2].ToString();
            string key = e.Parameters[3].ToString();
            string returnId = e.Parameters[4].ToString();
            string sOut = string.Empty;
            if (action == "delete")
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemoveObjectFromAll(this.ModuleId, secId, pId);
            }
            else if (action == "addobject")
            {
                if (!string.IsNullOrEmpty(secId))
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(this.ModuleId, pId, DotNetNuke.Modules.ActiveForums.SecureActions.View, secId);
                }
            }
            else
            {
                if (action == "remove")
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemoveObjectFromPermissions(this.ModuleId, pId, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), key), secId);
                }
                else
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(this.ModuleId, pId, (DotNetNuke.Modules.ActiveForums.SecureActions)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.SecureActions), key), secId);
                }

                sOut = action + "|" + returnId;
            }

            LiteralControl lit = new LiteralControl(sOut);
            lit.RenderControl(e.Output);
        }

        private void cbSecGrid_Callback(object sender, CallBackEventArgs e)
        {
            this.BuildNewGrid(this.Perms, this.PermissionsId);
            this.litSecGrid.RenderControl(e.Output);
        }
    }
}
