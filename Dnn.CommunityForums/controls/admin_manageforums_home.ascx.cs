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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Linq;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.ActiveForums.ViewModels;

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
            var groups = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().Get(this.ModuleId).OrderBy(f => f.SortOrder).ToList();
            var forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(this.ModuleId).OrderBy(f => f.ForumGroup.SortOrder).ThenBy(f => f.SortOrder).ToList();
            var totalGroups = groups.Count;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<table width=\"95%\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");

            sb.Append("<tr class=\"afgroupback\"><td class=\"afgroupback_left\">" + this.RenderSpacer(1, 4) + "</td><td colspan=\"5\" width=\"100%\" onmouseover=\"this.className='agrowedit'\" onmouseout=\"this.className=''\" onclick=\"LoadView('manageforums_forumeditor','" + this.ModuleId + "|M');\">");
            sb.Append(this.GetSharedResource("[RESX:DefaultSettings]"));
            sb.Append("</td><td><div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"LoadView('manageforums_forumeditor','" + this.ModuleId + "|M');\">" + this.edit + "</div></td><td>");
            sb.Append("</td><td>");
            sb.Append("</td><td class=\"afgroupback_right\">" + this.RenderSpacer(1, 4) + "</td></tr>");
            sb.Append("<tr><td colspan=\"8\" width=\"100%\">" + this.RenderSpacer(2, 100) + "</td></tr>");

            int groupCount = 0;
            foreach (var group in groups)
            {
                if (groupCount < Globals.GroupCount)
                {
                    var sGroupName = group.GroupName;
                    if (groupCount > 0)
                    {
                        sb.Append("<tr><td colspan=\"8\" width=\"100%\">" + this.RenderSpacer(2, 100) + "</td></tr>");
                    }

                    sb.Append("<tr class=\"afgroupback\"><td class=\"afgroupback_left\">" + this.RenderSpacer(1, 4) + "</td><td colspan=\"3\" width=\"100%\" onmouseover=\"this.className='agrowedit'\" onmouseout=\"this.className=''\" onclick=\"LoadView('manageforums_forumeditor','" + group.ForumGroupId + "|G');\">");
                    sb.Append(sGroupName);
                    if (group.Hidden)
                    {
                        sb.Append(" <span class=\"amcpnormal\"> (" + this.GetSharedResource("[RESX:Hidden]") + ")</span>");
                    }

                    if (!group.Active)
                    {
                        sb.Append(" <span class=\"amcpnormal\"> (" + this.GetSharedResource("[RESX:NotActive]") + ")</span>");
                    }


                    sb.Append("</td><td>");
                    var inheritance = string.Empty;
                    if (group.InheritSettings)
                    {
                        inheritance = "<img src=\"" + this.Page.ResolveUrl(Globals.ModuleImagesPath + "admin_check.png") + "\" class=\"dcf-controlpanel-inheritance-img\" alt=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSettingsOn]"), group.GroupName, this.GetSharedResource("[RESX:ModuleDefaults]")) + "\" title=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSettingsOn]"), group.GroupName, this.GetSharedResource("[RESX:ModuleDefaults]")) + "\"/>";
                    }
                    else if (group.FeatureSettings.EqualSettings(SettingsBase.GetModuleSettings(this.ModuleId).DefaultFeatureSettings))
                    {
                        inheritance = "<img src=\"" + this.Page.ResolveUrl(Globals.ModuleImagesPath + "info32.png") + "\" class=\"dcf-controlpanel-inheritance-img\" alt=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSettingsRecommended]"), group.GroupName, this.GetSharedResource("[RESX:ModuleDefaults]")) + "\" title=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSettingsRecommended]"), group.GroupName, this.GetSharedResource("[RESX:ModuleDefaults]")) + "\" />";
                    }

                    if (!string.IsNullOrEmpty(inheritance))
                    {
                        sb.Append($"<div class=\"amcpnormal dcf-controlpanel-inheritance-label\">{inheritance}</div>");
                    }

                    sb.Append("</td><td>");
                    inheritance = string.Empty;
                    if (group.InheritSecurity)
                    {
                        inheritance = "<img src=\"" + this.Page.ResolveUrl(Globals.ModuleImagesPath + "admin_check.png") + "\" class=\"dcf-controlpanel-inheritance-img\" alt=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSecurityOn]"), group.GroupName, this.GetSharedResource("[RESX:ModuleDefaults]")) + "\" title=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSecurityOn]"), group.GroupName, this.GetSharedResource("[RESX:ModuleDefaults]")) + "\" />";
                    }
                    else if (group.Security.EqualPermissions(new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(permissionId: SettingsBase.GetModuleSettings(this.ModuleId).DefaultPermissionId, moduleId: this.ModuleId)))
                    {
                        inheritance = "<img src=\"" + this.Page.ResolveUrl(Globals.ModuleImagesPath + "info32.png") + "\" class=\"dcf-controlpanel-inheritance-img\" alt=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSecurityRecommended]"), group.GroupName, this.GetSharedResource("[RESX:ModuleDefaults]")) + "\" title=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSecurityRecommended]"), group.GroupName, this.GetSharedResource("[RESX:ModuleDefaults]")) + "\"/>";
                    }

                    if (!string.IsNullOrEmpty(inheritance))
                    {
                        sb.Append($"<div class=\"amcpnormal dcf-controlpanel-inheritance-label\">{inheritance}</div>");
                    }

                    sb.Append("</td>");
                    sb.Append("<td><div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"LoadView('manageforums_forumeditor','" + group.ForumGroupId + "|G');\">" + this.edit + "</div></td><td>");
                    if (groupCount > 0)
                    {
                        sb.Append("<div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"groupMove(" + group.ForumGroupId + ",-1);\">" + this.arrowUp + "</div>");
                    }

                    groupCount += 1;
                    sb.Append("</td><td>");
                    if (groupCount < totalGroups)
                    {
                        sb.Append("<div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"groupMove(" + group.ForumGroupId + ",1);\">" + this.arrowDown + "</div>");
                    }

                    sb.Append("</td><td class=\"afgroupback_right\">" + this.RenderSpacer(1, 4) + "</td></tr>");
                    int forumCount = 0;
                    int totalGroupForum = forums.Count(f => f.ForumGroupId == group.ForumGroupId && f.ParentForumId == 0);

                    foreach (var forum in forums.Where(f => f.ForumGroupId.Equals(group.ForumGroupId)))
                    {
                        if (forum.ParentForumId == 0)
                        {
                            if (forumCount < Globals.ForumCount)
                            {
                                string sForumName = forum.ForumName;
                                sb.Append("<tr class=\"afforumback\"><td class=\"afforumback_left\">" + this.RenderSpacer(1, 4) + "</td><td style=\"width:15px;\" width=\"15\">" + this.RenderSpacer(5, 15) + "</td><td colspan=\"2\" width=\"100%\" onmouseover=\"this.className='afrowedit'\" onmouseout=\"this.className=''\" onclick=\"LoadView('manageforums_forumeditor','" + forum.ForumID + "|F');\">");
                                sb.Append(sForumName);
                                if (forum.Hidden)
                                {
                                    sb.Append(" <span class=\"amcpnormal\"> (" + this.GetSharedResource("[RESX:Hidden]") + ")</span>");
                                }

                                if (!forum.Active)
                                {
                                    sb.Append(" <span class=\"amcpnormal\"> (" + this.GetSharedResource("[RESX:NotActive]") + ")</span>");
                                }

                                sb.Append("</td><td>");
                                inheritance = string.Empty;
                                if (forum.InheritSettings)
                                {
                                    inheritance = "<img src=\"" + this.Page.ResolveUrl(Globals.ModuleImagesPath + "admin_check.png") + "\" class=\"dcf-controlpanel-inheritance-img\" alt=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSettingsOn]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" title=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSettingsOn]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" />";
                                }
                                else if (forum.FeatureSettings.EqualSettings(forum.ForumGroup.FeatureSettings))
                                {
                                    inheritance = "<img src=\"" + this.Page.ResolveUrl(Globals.ModuleImagesPath + "info32.png") + "\" class=\"dcf-controlpanel-inheritance-img\" alt=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSettingsRecommended]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" title=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSettingsRecommended]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" />";
                                }

                                if (!string.IsNullOrEmpty(inheritance))
                                {
                                    sb.Append($"<div class=\"amcpnormal dcf-controlpanel-inheritance-label\">{inheritance}</div>");
                                }

                                sb.Append("</td><td>");
                                inheritance = string.Empty;
                                if (forum.InheritSecurity)
                                {
                                    inheritance = "<img src=\"" + this.Page.ResolveUrl(Globals.ModuleImagesPath + "admin_check.png") + "\" class=\"dcf-controlpanel-inheritance-img\" alt=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSecurityOn]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" title=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSecurityOn]"), forum.ForumName, forum.ForumGroup.GroupName) + "\"/>";
                                }
                                else if (forum.Security.EqualPermissions(forum.ForumGroup.Security))
                                {
                                    inheritance = "<img src=\"" + this.Page.ResolveUrl(Globals.ModuleImagesPath + "info32.png") + "\" class=\"dcf-controlpanel-inheritance-img\" alt=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSecurityRecommended]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" title=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSecurityRecommended]"), forum.ForumName, forum.ForumGroup.GroupName) + "\"/>";
                                }

                                if (!string.IsNullOrEmpty(inheritance))
                                {
                                    sb.Append($"<div class=\"amcpnormal dcf-controlpanel-inheritance-label\">{inheritance}</div>");
                                }

                                sb.Append("</td>");
                                sb.Append("<td><div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"LoadView('manageforums_forumeditor','" + forum.ForumID + "|F');\">" + this.edit + "</div></td><td>");
                                if (forumCount > 0)
                                {
                                    sb.Append("<div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"forumMove(" + forum.ForumID + ",-1);\">" + this.arrowUp + "</div>");
                                }

                                forumCount += 1;
                                sb.Append("</td><td>");
                                if (forumCount < totalGroupForum)
                                {
                                    sb.Append("<div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"forumMove(" + forum.ForumID + ",1);\">" + this.arrowDown + "</div>");
                                }

                                sb.Append("</td><td class=\"afforumback_right\">" + this.RenderSpacer(1, 4) + "</td></tr>");

                                if (forum.SubForums.Count > 0)
                                {
                                    sb.Append(this.AddSubForums(forum));
                                }
                            }
                        }
                    }
                }
            }

            sb.Append("<tr><td></td><td></td><td></td><td width=\"100%\"></td><td></td><td></td><td></td><td></td></tr><tr><td></td><td></td><td width=\"100%\" colspan=\"2\"></td><td></td><td></td><td></td><td></td></tr></table>");
            this.litForums.Text = sb.ToString();
        }

        private string RenderSpacer(int height, int width)
        {
            return "<img src=\"" + this.Page.ResolveUrl(Globals.ModulePath + "images/spacer.gif") + "\" style=\"height:" + height + "px;width:" + width + "px\" alt=\"-\" />";
        }

        private string AddSubForums(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int totalSubCount = forum.SubForums.Count;
            int subCount = 0;
            foreach (var subforum in forum.SubForums)
            {
                string sForumName = subforum.ForumName;
                sb.Append("<tr class=\"afforumback\"><td class=\"afforumback_left\">" + this.RenderSpacer(1, 4) + "</td><td style=\"width:15px;\">" + this.RenderSpacer(5, 15) + "</td><td style=\"width:15px;\">" + this.RenderSpacer(5, 15) + "</td><td width=\"100%\" onmouseover=\"this.className='afrowedit'\" onmouseout=\"this.className=''\" onclick=\"LoadView('manageforums_forumeditor','" + subforum.ForumID + "|F');\">");
                sb.Append(sForumName);
                if (subforum.Hidden)
                {
                    sb.Append(" <span class=\"amcpnormal\"> (" + this.GetSharedResource("[RESX:Hidden]") + ")</span>");
                }

                if (!subforum.Active)
                {
                    sb.Append(" <span class=\"amcpnormal\"> (" + this.GetSharedResource("[RESX:NotActive]") + ")</span>");
                }

                sb.Append("</td><td>");
                var inheritance = string.Empty;
                if (forum.InheritSettings)
                {
                    inheritance = "<img src=\"" + this.Page.ResolveUrl(Globals.ModuleImagesPath + "admin_check.png") + "\" class=\"dcf-controlpanel-inheritance-img\" alt=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSettingsOn]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" title=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSettingsOn]"), forum.ForumName, forum.ForumGroup.GroupName) + "\"/>";
                }
                else if (forum.FeatureSettings.EqualSettings(forum.ForumGroup.FeatureSettings))
                {
                    inheritance = "<img src=\"" + this.Page.ResolveUrl(Globals.ModuleImagesPath + "info32.png") + "\" class=\"dcf-controlpanel-inheritance-img\" alt=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSettingsRecommended]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" title=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSettingsRecommended]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" />";
                }

                if (!string.IsNullOrEmpty(inheritance))
                {
                    sb.Append($"<div class=\"amcpnormal dcf-controlpanel-inheritance-label\">{inheritance}</div>");
                }

                sb.Append("</td><td>");
                inheritance = string.Empty;
                if (forum.InheritSecurity)
                {
                    inheritance = "<img src=\"" + this.Page.ResolveUrl(Globals.ModuleImagesPath + "admin_check.png") + "\" class=\"dcf-controlpanel-inheritance-img\" alt=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSecurityOn]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" title=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSecurityOn]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" />";
                }
                else if (forum.Security.EqualPermissions(forum.ForumGroup.Security))
                {
                    inheritance = "<img src=\"" + this.Page.ResolveUrl(Globals.ModuleImagesPath + "info32.png") + "\" class=\"dcf-controlpanel-inheritance-img\" alt=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSecurityRecommended]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" title=\"" + string.Format(this.GetSharedResource("[RESX:Tips:InheritSecurityRecommended]"), forum.ForumName, forum.ForumGroup.GroupName) + "\" />";
                }

                if (!string.IsNullOrEmpty(inheritance))
                {
                    sb.Append($"<div class=\"amcpnormal dcf-controlpanel-inheritance-label\">{inheritance}</div>");
                }

                sb.Append("</td>");
                sb.Append("<td><div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"LoadView('manageforums_forumeditor','" + subforum.ForumID + "|F');\">" + this.edit + "</div></td><td>");
                if (subCount > 0)
                {
                    sb.Append("<div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"forumMove(" + subforum.ForumID + ",-1);\">" + this.arrowUp + "</div>");
                }

                subCount += 1;
                sb.Append("</td><td>");
                if (subCount < totalSubCount)
                {
                    sb.Append("<div class=\"af16icon\" onmouseover=\"this.className='af16icon_over';\" onmouseout=\"this.className='af16icon';\" onclick=\"forumMove(" + subforum.ForumID + ",1);\">" + this.arrowDown + "</div>");
                }

                sb.Append("</td><td class=\"afforumback_right\">" + this.RenderSpacer(1, 4) + "</td></tr>");
            }

            return sb.ToString();
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
