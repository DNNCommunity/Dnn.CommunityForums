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
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Web.UI.WebControls;

    public partial class admin_categories : ActiveAdminBase
    {
        public string ImagePath => this.Page.ResolveUrl(string.Concat(this.MainSettings.ThemeLocation, "/images"));

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.agCategories.Callback += this.agCategories_Callback;
            this.agCategories.ItemBound += this.agCategories_ItemBound;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.BindGroups();
        }

        private void agCategories_Callback(object sender, Controls.CallBackEventArgs e)
        {
            try
            {
                if (!(e.Parameters[4] == string.Empty))
                {
                    string sAction = e.Parameters[4].Split(':')[0];

                    switch (sAction.ToUpper())
                    {
                        case "DELETE":
                            {
                                int tagId = Convert.ToInt32(e.Parameters[4].Split(':')[1]);
                                if (Utilities.IsNumeric(tagId))
                                {
                                    new DotNetNuke.Modules.ActiveForums.Controllers.TagController().DeleteById(tagId);
                                }

                                break;
                            }

                        case "SAVE":
                            {
                                string[] sParams = e.Parameters[4].Split(':');
                                string tagName = sParams[1].Trim();
                                int tagId = 0;
                                int forumId = -1;
                                int forumGroupId = -1;
                                if (sParams.Length > 2)
                                {
                                    tagId = Convert.ToInt32(sParams[2]);
                                }

                                if (sParams[3].Contains("FORUM"))
                                {
                                    forumId = Convert.ToInt32(sParams[3].Replace("FORUM", string.Empty));
                                }

                                if (sParams[3].Contains("GROUP"))
                                {
                                    forumGroupId = Convert.ToInt32(sParams[3].Replace("GROUP", string.Empty));
                                }

                                if (!(tagName == string.Empty))
                                {
                                    DataProvider.Instance().Tags_Save(this.PortalId, this.ModuleId, tagId, tagName, 0, 0, 0, -1, true, forumId, forumGroupId);
                                }

                                break;
                            }
                    }
                }

                this.agCategories.DefaultParams = string.Empty;
                int pageIndex = Convert.ToInt32(e.Parameters[0]);
                int pageSize = Convert.ToInt32(e.Parameters[1]);
                string sortColumn = e.Parameters[2].ToString();
                string sort = e.Parameters[3].ToString();
                this.agCategories.Datasource = DataProvider.Instance().Tags_List(this.PortalId, this.ModuleId, true, pageIndex, pageSize, sort, sortColumn, -1, -1);
                this.agCategories.Refresh(e.Output);
            }
            catch (Exception ex)
            {
            }
        }

        private void BindGroups()
        {
            this.drpForums.Items.Add(new ListItem(Utilities.GetSharedResource("DropDownSelect"), "-1"));
            DotNetNuke.Modules.ActiveForums.Entities.ForumCollection allForums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(this.ModuleId);
            var filteredForums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(this.ModuleId).Where(f => f.ForumGroup.Active && f.Active && f.ParentForumId == 0);
            int tmpGroupId = -1;
            foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f in filteredForums)
            {
                if (!(tmpGroupId == f.ForumGroupId))
                {
                    this.drpForums.Items.Add(new ListItem(f.GroupName, "GROUP" + f.ForumGroupId.ToString()));
                    tmpGroupId = f.ForumGroupId;
                }

                this.drpForums.Items.Add(new ListItem(" - " + f.ForumName, "FORUM" + f.ForumID.ToString()));
                if (f.SubForums != null && f.SubForums.Count > 0)
                {
                    foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo ff in f.SubForums)
                    {
                        this.drpForums.Items.Add(new ListItem(" ---- " + ff.ForumName, "FORUM" + ff.ForumID.ToString()));
                    }
                }
            }
        }

        private void agCategories_ItemBound(object sender, Modules.ActiveForums.Controls.ItemBoundEventArgs e)
        {
            // e.Item(1) = Server.HtmlEncode(e.Item(1).ToString)
            // e.Item(2) = Server.HtmlEncode(e.Item(2).ToString)
            e.Item[6] = "<img src=\"" + this.Page.ResolveUrl(Globals.ModulePath + "images/delete16.png") + "\" alt=\"" + this.GetSharedResource("[RESX:Delete]") + "\" height=\"16\" width=\"16\" />";
        }
    }
}
