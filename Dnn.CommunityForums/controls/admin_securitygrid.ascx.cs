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
namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

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
            foreach (DotNetNuke.Security.Roles.RoleInfo ri in DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoles(this.PortalId))
            {
                sb.Append("<option value=\"" + ri.RoleID + "\">" + ri.RoleName + "</option>");
            }

            sb.Append("</select>");
            this.litRoles.Text = sb.ToString();
        }

        private void BuildNewGrid(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo security, int permissionsId)
        {
            // Roles
            string[] roles = this.GetSecureObjectList(security, 0).Split(';');
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

            NameValueCollection nvc = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRolesNVC(this.PortalId, tmp);
            foreach (string key in nvc.AllKeys)
            {
                DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo pi = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo();
                pi.ObjectId = key;
                pi.ObjectName = nvc[key];
                if (string.IsNullOrEmpty(pi.ObjectName))
                {
                    pi.ObjectName = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(portalId: this.PortalId, roleId: Convert.ToInt32(key)).RoleName;
                }

                pi.Type = ObjectType.RoleId;
                pl.Add(pi);
            }

            // Users
            string users = this.GetSecureObjectList(security, 1);
            string userNames = string.Empty;
            if (!string.IsNullOrEmpty(users))
            {
                foreach (string uid in users.Split(';'))
                {
                    if (!string.IsNullOrEmpty(uid))
                    {
                        DotNetNuke.Entities.Users.UserInfo u = DotNetNuke.Entities.Users.UserController.Instance.GetUser(this.PortalId, Convert.ToInt32(uid));
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

            // Groups
            string groups = this.GetSecureObjectList(security, 2);
            if (!string.IsNullOrEmpty(groups))
            {
                foreach (string g in groups.Split(';'))
                {
                    if (!string.IsNullOrEmpty(g))
                    {

                        string gType = g.Split(':')[1];
                        int groupId = Convert.ToInt32(g.Split(':')[0]);
                        RoleInfo role = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(portalId: this.PortalId, roleId: groupId);
                        string groupName = role.RoleName;
                        if (!string.IsNullOrEmpty(groupName))
                        {
                            DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo pi = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo();
                            pi.ObjectId = g;
                            pi.ObjectName = groupName;
                            pi.Type = ObjectType.GroupId;
                            if (Convert.ToInt32(gType) == 0)
                            {
                                pi.ObjectName += " - Owner";
                            }
                            else
                            {
                                pi.ObjectName += " - Member";
                            }

                            pl.Add(pi);
                        }
                    }
                }
            }

            string[,] grid = new string[pl.Count + 1, 28];
            i = 0;
            foreach (DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo pi in pl)
            {
                grid[i, 0] = pi.ObjectId;
                grid[i, 1] = pi.ObjectName;
                grid[i, 2] = Convert.ToInt16(pi.Type).ToString();
                grid[i, 3] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.View));
                grid[i, 4] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Read));
                grid[i, 5] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Create));
                grid[i, 6] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Reply));
                grid[i, 7] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Edit));
                grid[i, 8] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Delete));
                grid[i, 9] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Lock));
                grid[i, 10] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Pin));
                grid[i, 11] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Attach));
                grid[i, 12] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Poll));
                grid[i, 13] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Block));
                grid[i, 14] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Trust));
                grid[i, 15] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Subscribe));
                grid[i, 16] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Announce));
                grid[i, 17] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Tag));
                grid[i, 18] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Categorize));
                grid[i, 19] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.Prioritize));

                grid[i, 20] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.ModApprove));
                grid[i, 21] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.ModMove));
                grid[i, 22] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.ModSplit));
                grid[i, 23] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.ModDelete));
                grid[i, 24] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.ModUser));
                grid[i, 25] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.ModEdit));
                grid[i, 26] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.ModLock));
                grid[i, 27] = Convert.ToString(this.PermValue((int)pi.Type, pi.ObjectId, security.ModPin));

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
                        sb.Append("<img src=\"" + this.Page.ResolveUrl(Globals.ModulePath + "images/mini_del.gif") + "\" alt=\"Remove Object\" style=\"cursor:pointer;z-index:10;\" class=\"afminidel\" onclick=\"securityDelObject(this,'" + grid[x, 0] + "'," + grid[x, 2] + "," + permissionsId + ");\" />");
                    }
                }

                sb.Append("</span>" + grid[x, 1]);
                sb.Append("</div></td></tr>");
            }

            sb.Append("</table></div></td><td valign=\"top\" width=\"94%\"><div class=\"afsecactions\" style=\"overflow-x:auto;overflow-y:hidden;\">");

            // litNewObjects.Text = sb.ToString
            // sb = New StringBuilder
            sb.Append("<table cellpadding=0 cellspacing=0 border=0 width=\"100%\" id=\"tblSecGrid\">");
            sb.Append("<tr>");
            string keyText = string.Empty;
            for (int td = 3; td <= 27; td++)
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

                sb.Append(keyText);
                sb.Append("</td>");
            }

            sb.Append("</tr>");
            for (int x = 0; x < pl.Count; x++)
            {
                sb.Append("<tr onmouseover=\"this.className='afgridrowover'\" onmouseout=\"this.className='afgridrow'\">");
                for (int r = 3; r <= 27; r++)
                {
                    keyText = Convert.ToString(Enum.Parse(enumType, values.GetValue(r - 3).ToString()));
                    bool bState = Convert.ToBoolean(grid[x, r]); // DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPermission(ForumID, Integer.Parse(dr("ObjectId").ToString), key, Integer.Parse(dr("SecureType").ToString), dt)
                    string sState = "<img src=\"" + this.imgOff + "\" alt=\"Disabled\" />";
                    if (bState)
                    {
                        sState = "<img src=\"" + this.imgOn + "\" alt=\"Enabled\" />";
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
                        sb.Append("onclick=\"securityToggle(this," + permissionsId + ",'" + grid[x, 0] + "','" + grid[x, 1] + "'," + grid[x, 2] + ",'" + keyText + "');\"");
                    }

                    sb.Append(">" + sState + "</div></td>");

                }

                sb.Append("</tr>");
            }

            sb.Append("</table></div></td></tr></table>");
            this.litSecGrid.Text = sb.ToString();

            // litNewSecurity.Text = sb.ToString
            // litNewGrid.Text = sb.ToString
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

        private string GetSecureObjectList(DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo s, int objectType)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetSecureObjectList(this.PortalSettings, s, objectType);
        }

        private void cbSecurityToggle_Callback(object sender, CallBackEventArgs e)
        {
            string action = e.Parameters[0].ToString();
            int pId = this.PermissionsId;
            string secId = e.Parameters[2].ToString();
            int secType = Convert.ToInt32(e.Parameters[3].ToString());
            string key = e.Parameters[4].ToString();
            string returnId = e.Parameters[5].ToString();
            string sOut = string.Empty;
            if (action == "delete")
            {
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemoveObjectFromAll(this.ModuleId, secId, secType, pId);

            }
            else if (action == "addobject")
            {
                if (secType == 1)
                {
                    UserController uc = new UserController();
                    User ui = uc.GetUser(this.PortalId, this.ModuleId, secId);
                    if (ui != null)
                    {
                        secId = ui.UserId.ToString();
                    }
                    else
                    {
                        secId = string.Empty;
                    }
                }
                else
                {
                    if (secId.Contains(":"))
                    {
                        secType = 2;
                    }
                }

                if (!string.IsNullOrEmpty(secId))
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(this.ModuleId, pId, "View", secId, secType);
                }

            }
            else
            {
                if (action == "remove")
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemoveObjectFromPermissions(this.ModuleId, pId, key, secId, secType);
                }
                else
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermissions(this.ModuleId, pId, key, secId, secType);
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
