// Copyright (c) 2013-2024 by DNN Community
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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    public partial class admin_manageforums_home : ActiveAdminBase
    {
        private string arrowUp = string.Empty;
        private string arrowDown = string.Empty;
        private string edit = string.Empty;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cbGrid.CallbackEvent += this.cbGrid_Callback;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.arrowUp = "<img src=\"" + this.Page.ResolveUrl(Globals.ModulePath + "images/arrow_up.png") + "\" alt=\"" + this.GetSharedResource("[RESX:MoveUp]") + "\" />";
            this.arrowDown = "<img src=\"" + this.Page.ResolveUrl(Globals.ModulePath + "images/arrow_down.png") + "\" alt=\"" + this.GetSharedResource("[RESX:MoveDown]") + "\" />";
            this.edit = "<img src=\"" + this.Page.ResolveUrl(Globals.ModulePath + "images/forum_edit.png") + "\" alt=\"" + this.GetSharedResource("[RESX:Edit]") + "\" />";
            if (!this.cbGrid.IsCallback)
            {
                this.BindForums();
            }
        }

        private void BindForums()
        {
            IDataReader dr = DataProvider.Instance().Forums_List(this.PortalId, this.ModuleId, -1, -1, false);
            DataTable dt = new DataTable("Forums");
            dt.Load(dr);
            int totalGroups = this.GetGroupCount(dt);
            int totalGroupForum = 0;
            string tmpGroup = string.Empty;
            int groupCount = 0;
            int forumCount = 0;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<table width=\"95%\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");

            sb.Append("<tr class=\"afgroupback\"><td class=\"afgroupback_left\">" + this.RenderSpacer(1, 4) + "</td><td colspan=\"3\" width=\"100%\" onmouseover=\"this.className='agrowedit'\" onmouseout=\"this.className=''\" onclick=\"LoadView('manageforums_forumeditor','" + this.ModuleId + "|M');\">");
            sb.Append(this.GetSharedResource("[RESX:DefaultSettings]"));
            sb.Append("</td><td><div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"LoadView('manageforums_forumeditor','" + this.ModuleId + "|M');\">" + this.edit + "</div></td><td>");
            sb.Append("</td><td>");
            sb.Append("</td><td class=\"afgroupback_right\">" + this.RenderSpacer(1, 4) + "</td></tr>");
            sb.Append("<tr><td colspan=\"8\" width=\"100%\">" + this.RenderSpacer(2, 100) + "</td></tr>");

            foreach (DataRow row in dt.Rows)
            {
                if (tmpGroup != row["ForumGroupId"].ToString())
                {
                    if (groupCount < Globals.GroupCount)
                    {
                        string sGroupName = row["GroupName"].ToString();
                        if (groupCount > 0)
                        {
                            sb.Append("<tr><td colspan=\"8\" width=\"100%\">" + this.RenderSpacer(2, 100) + "</td></tr>");
                        }

                        sb.Append("<tr class=\"afgroupback\"><td class=\"afgroupback_left\">" + this.RenderSpacer(1, 4) + "</td><td colspan=\"3\" width=\"100%\" onmouseover=\"this.className='agrowedit'\" onmouseout=\"this.className=''\" onclick=\"LoadView('manageforums_forumeditor','" + row["ForumGroupId"].ToString() + "|G');\">");
                        sb.Append(sGroupName);
                        sb.Append("</td><td><div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"LoadView('manageforums_forumeditor','" + row["ForumGroupId"].ToString() + "|G');\">" + this.edit + "</div></td><td>");
                        if (groupCount > 0)
                        {
                            sb.Append("<div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"groupMove(" + row["ForumGroupId"].ToString() + ",-1);\">" + this.arrowUp + "</div>");
                        }

                        groupCount += 1;
                        sb.Append("</td><td>");
                        if (groupCount < totalGroups)
                        {
                            sb.Append("<div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"groupMove(" + row["ForumGroupId"].ToString() + ",1);\">" + this.arrowDown + "</div>");
                        }

                        sb.Append("</td><td class=\"afgroupback_right\">" + this.RenderSpacer(1, 4) + "</td></tr>");
                        forumCount = 0;
                        totalGroupForum = this.GetGroupForumCount(dt, Convert.ToInt32(row["ForumGroupId"]));
                        tmpGroup = row["ForumGroupId"].ToString();
                    }
                }

                if (Convert.ToInt32(row["ParentForumId"]) == 0)
                {
                    if (forumCount < Globals.ForumCount)
                    {
                        string sForumName = row["ForumName"].ToString();
                        sb.Append("<tr class=\"afforumback\"><td class=\"afforumback_left\">" + this.RenderSpacer(1, 4) + "</td><td style=\"width:15px;\" width=\"15\">" + this.RenderSpacer(5, 15) + "</td><td colspan=\"2\" width=\"100%\" onmouseover=\"this.className='afrowedit'\" onmouseout=\"this.className=''\" onclick=\"LoadView('manageforums_forumeditor','" + row["ForumId"].ToString() + "|F');\">");
                        sb.Append(sForumName);
                        sb.Append("</td><td><div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"LoadView('manageforums_forumeditor','" + row["ForumId"].ToString() + "|F');\">" + this.edit + "</div></td><td>");
                        if (forumCount > 0)
                        {
                            sb.Append("<div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"forumMove(" + row["ForumId"].ToString() + ",-1);\">" + this.arrowUp + "</div>");
                        }

                        forumCount += 1;
                        sb.Append("</td><td>");
                        if (forumCount < totalGroupForum)
                        {
                            sb.Append("<div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"forumMove(" + row["ForumId"].ToString() + ",1);\">" + this.arrowDown + "</div>");
                        }

                        sb.Append("</td><td class=\"afforumback_right\">" + this.RenderSpacer(1, 4) + "</td></tr>");

                        if (this.HasSubForums(Convert.ToInt32(row["ForumId"]), dt) > 0)
                        {
                            sb.Append(this.AddSubForums(dt, row));
                        }
                    }
                }
            }

            if (!dr.IsClosed)
            {
                dr.Close();
            }

            sb.Append("<tr><td></td><td></td><td></td><td width=\"100%\"></td><td></td><td></td><td></td><td></td></tr><tr><td></td><td></td><td width=\"100%\" colspan=\"2\"></td><td></td><td></td><td></td><td></td></tr></table>");
            this.litForums.Text = sb.ToString();
        }

        private string RenderSpacer(int height, int width)
        {
            return "<img src=\"" + this.Page.ResolveUrl(Globals.ModulePath + "images/spacer.gif") + "\" style=\"height:" + height + "px;width:" + width + "px\" alt=\"-\" />";
        }

        private int GetGroupCount(DataTable dt)
        {
            int i = 0;
            string tmpGroup = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                if (tmpGroup != row["ForumGroupId"].ToString())
                {
                    i += 1;
                    tmpGroup = row["ForumGroupId"].ToString();
                }
            }

            return i;
        }

        private int GetGroupForumCount(DataTable dt, int groupId)
        {
            int i = 0;
            string tmpGroup = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToInt32(row["ForumGroupId"]) == groupId && Convert.ToInt32(row["ParentForumId"]) == 0)
                {
                    i += 1;
                }
            }

            return i;
        }

        private string AddSubForums(DataTable dt, DataRow dr)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int totalSubCount = this.HasSubForums(Convert.ToInt32(dr["ForumId"]), dt);
            int subCount = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToInt32(dr["ForumId"]) == Convert.ToInt32(row["ParentForumId"]))
                {
                    string sForumName = row["ForumName"].ToString();
                    sb.Append("<tr class=\"afforumback\"><td class=\"afforumback_left\">" + this.RenderSpacer(1, 4) + "</td><td style=\"width:15px;\">" + this.RenderSpacer(5, 15) + "</td><td style=\"width:15px;\">" + this.RenderSpacer(5, 15) + "</td><td width=\"100%\" onmouseover=\"this.className='afrowedit'\" onmouseout=\"this.className=''\" onclick=\"LoadView('manageforums_forumeditor','" + row["ForumId"].ToString() + "|F');\">");
                    sb.Append(sForumName);
                    sb.Append("</td><td><div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"LoadView('manageforums_forumeditor','" + row["ForumId"].ToString() + "|F');\">" + this.edit + "</div></td><td>");
                    if (subCount > 0)
                    {
                        sb.Append("<div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"forumMove(" + row["ForumId"].ToString() + ",-1);\">" + this.arrowUp + "</div>");
                    }

                    subCount += 1;
                    sb.Append("</td><td>");
                    if (subCount < totalSubCount)
                    {
                        sb.Append("<div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"forumMove(" + row["ForumId"].ToString() + ",1);\">" + this.arrowDown + "</div>");
                    }

                    sb.Append("</td><td class=\"afforumback_right\">" + this.RenderSpacer(1, 4) + "</td></tr>");
                }
            }

            return sb.ToString();
        }

        private int HasSubForums(int forumId, DataTable dt)
        {
            int subCount = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToInt32(row["ParentForumId"]) == forumId)
                {
                    subCount += 1;
                }
            }

            return subCount;
        }

        private void cbGrid_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            int objectId = Convert.ToInt32(e.Parameters[1]);
            int dir = Convert.ToInt32(e.Parameters[2]);
            switch (e.Parameters[0].ToString().ToLower())
            {
                case "g":
                    DataProvider.Instance().Groups_Move(this.ModuleId, objectId, dir);
                    break;
                case "f":
                    DataProvider.Instance().Forums_Move(this.ModuleId, objectId, dir);
                    break;
            }

            DataCache.ClearAllCache(this.ModuleId);
            DataCache.ClearAllCacheForTabId(this.TabId);
            this.BindForums();
            this.litForums.RenderControl(e.Output);
        }
    }
}
